using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trail365.Configuration;

namespace Trail365.Data
{
    public static class DbContextExtension
    {
        public static void SyncSqliteFiles(DbContext[] contextList, string backupDirectory, bool syncOverwriteEnabled, TextWriter logger)
        {
            if (contextList == null) throw new ArgumentNullException(nameof(contextList));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (string.IsNullOrEmpty(backupDirectory)) throw new ArgumentNullException(nameof(backupDirectory));
            List<Task> tasks = new List<Task>();

            logger.WriteLine($"{nameof(SyncSqliteFiles)}: Initialize sync for {contextList.Length} databases from '{backupDirectory}'");

            foreach (var dbcontext in contextList)
            {
                if (ConnectionStrings.TryGetSqliteFileInfo(dbcontext.Database.GetDbConnection().ConnectionString, out var targetFileInfo) == false)
                {
                    throw new InvalidOperationException($"Connectionstring not available for '{dbcontext.GetType().Name}'");
                }

                if (targetFileInfo.Exists)
                {
                    if (!syncOverwriteEnabled)
                    {
                        throw new InvalidOperationException($"Database file '{targetFileInfo.FullName}' does always exists for '{dbcontext.GetType().Name}'");
                    }
                }

                string name = targetFileInfo.Name;

                if (targetFileInfo.Directory.Exists == false)
                {
                    targetFileInfo.Directory.Create();
                }

                var t = Task.Run(() =>
                {
                    if (Utils.TryGetLatest(backupDirectory, name, logger, out var finding))
                    {
                        logger.WriteLine($"{nameof(SyncSqliteFiles)}: FileCopy started for '{name}' from '{finding.FullName}' to '{targetFileInfo.FullName}'");
                        var sw1 = Stopwatch.StartNew();
                        File.Copy(finding.FullName, targetFileInfo.FullName, syncOverwriteEnabled);
                        sw1.Stop();
                        targetFileInfo.Refresh();
                        logger.WriteLine($"{nameof(SyncSqliteFiles)}: FileCopy completed for '{name}' (Duration={sw1.Elapsed.ToString()}, FileSize={Convert.ToInt64(targetFileInfo.Length / 1024)} KBytes)");
                    }
                    else
                    {
                        //this is a restart. Migration can create the entire database
                        logger.WriteLine($"{nameof(SyncSqliteFiles)}: No sync file found for '{name}'.");
                    }
                });
                tasks.Add(t);
            } //foreach!

            logger.WriteLine($"{nameof(SyncSqliteFiles)}: {tasks.Count} Sync tasks created");
            var sw = Stopwatch.StartNew();
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            logger.WriteLine($"{nameof(SyncSqliteFiles)}: Sync tasks completed after {sw.Elapsed.ToString()}");
        }

        public static void LogCacheDependency(this DbContext context, string operationName, Action<DependencyTelemetry> callback)
        {
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            if (string.IsNullOrEmpty(operationName)) throw new ArgumentNullException(nameof(operationName));
            using (var tracker = context.DependencyTrackerForCache(operationName))
            {
                callback(tracker.Telemetry);
            }
        }

        public static void LogDependency(this DbContext context, string operationName, Action<DependencyTelemetry> callback)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            using (var tracker = context.DependencyTracker(operationName))
            {
                callback(tracker.Telemetry);
            }
        }

        public static DependencyTelemetryTracker DependencyTrackerForCache(this DbContext context, string operationName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            IDependencyTracker tr = context as IDependencyTracker;
            if (tr == null) throw new InvalidOperationException($"Dbcontext must implement {nameof(IDependencyTracker)} interface.");
            return context.CreateDependencyTracker(tr.OperationTarget, operationName, tr.OperationType(true));
        }

        public static DependencyTelemetryTracker DependencyTracker(this DbContext context, string operationName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            IDependencyTracker tr = context as IDependencyTracker;
            if (tr == null) throw new InvalidOperationException($"Dbcontext must implement {nameof(IDependencyTracker)} interface.");
            return context.CreateDependencyTracker(tr.OperationTarget, operationName, tr.OperationType(false));
        }

        /// <summary>
        /// destination directory should be created before, if not this code should not be used by multiple Threads!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationPath"></param>
        /// <param name="cancellation"></param>
        /// <param name="logger"></param>
        public static void BackupSqliteDB(this DbContext context, string destinationPath, CancellationToken cancellation, ILogger logger)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            cancellation.ThrowIfCancellationRequested();

            string tempFileName = System.IO.Path.GetTempFileName();
            logger.LogTrace($"Start backup to temporary file: '{tempFileName}'");

            Microsoft.Data.Sqlite.SqliteConnectionStringBuilder destinationConnectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
            {
                Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate,
                DataSource = tempFileName
            };

            Microsoft.Data.Sqlite.SqliteConnection destination = new Microsoft.Data.Sqlite.SqliteConnection(destinationConnectionStringBuilder.ToString());

            Microsoft.Data.Sqlite.SqliteConnection source = (Microsoft.Data.Sqlite.SqliteConnection)context.Database.GetDbConnection();
            source.Open();
            try
            {
                cancellation.ThrowIfCancellationRequested();
                Stopwatch sw1 = Stopwatch.StartNew();

                using (var tracker = context.DependencyTracker(nameof(source.BackupDatabase)))
                {
                    source.BackupDatabase(destination);
                    tracker.Telemetry.Data = destinationPath;
                }

                sw1.Stop();
                logger.LogTrace($"Backup completed to temporary file: File='{tempFileName}', Duration={sw1.ElapsedMilliseconds}ms");
                cancellation.ThrowIfCancellationRequested();
                var dir = Path.GetDirectoryName(destinationPath);
                Stopwatch sw2 = Stopwatch.StartNew();
                System.IO.File.Move(tempFileName, destinationPath);
                sw2.Stop();
                logger.LogTrace($"Backup file moved to final destination from '{tempFileName}' to '{destinationPath}' in {sw2.ElapsedMilliseconds}ms");
                logger.LogInformation($"Backup to '{destination}' completed in {sw1.ElapsedMilliseconds + sw2.ElapsedMilliseconds}ms");
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(tempFileName))
                    {
                        System.IO.File.Delete(tempFileName);
                    }
                }
                catch (IOException ex)
                {
                    logger.LogError(ex, "Exception during delete of temporary backup target");
                }

                source.Close();
            }
        }
    }
}
