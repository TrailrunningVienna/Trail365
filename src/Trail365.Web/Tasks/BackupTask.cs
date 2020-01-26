using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Tasks;

namespace Trail365.Web.Tasks
{
    /// <summary>
    /// FULL Backup
    /// </summary>
    public class BackupTask : BackgroundTask
    {
        protected override Task Execute(CancellationToken cancellationToken)
        {
            //idea: check if it was not modified and skip!
            var trailDB = this.Context.ServiceProvider.GetRequiredService<TrailContext>();
            var taskDB = this.Context.ServiceProvider.GetRequiredService<TaskContext>();
            var identityDB = this.Context.ServiceProvider.GetRequiredService<IdentityContext>();
            var _settings = this.Context.ServiceProvider.GetRequiredService<IOptionsMonitor<AppSettings>>().CurrentValue;
            var items = new DbContext[] { trailDB, taskDB, identityDB };
            var dictionary = new Dictionary<DbContext, string>();
            var tasks = new List<Task>();

            foreach (var dbcontext in items)
            {
                if (ConnectionStrings.TryGetSqliteFileInfo(dbcontext.Database.GetDbConnection().ConnectionString, out var dbFileInfo) == false)
                {
                    throw new InvalidOperationException($"Connectionstring not available for '{dbcontext.GetType().Name}'");
                }

                if (dbFileInfo.Exists == false)
                {
                    throw new InvalidOperationException($"Database file '{dbFileInfo.FullName}' does not exists for '{dbcontext.GetType().Name}'");
                }

                string cn = dbcontext.GetType().Name.ToLowerInvariant().Replace("context", string.Empty);

                string targetFileName = Utils.GetTargetFileName(_settings.BackupDirectory, cn, dbFileInfo.LastWriteTimeUtc, dbFileInfo.Name);

                if (File.Exists(targetFileName))
                {
                    continue; //backup is up to date!
                }

                var dir = Path.GetDirectoryName(targetFileName);

                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }
                dictionary.Add(dbcontext, targetFileName);
            }

            foreach (var dbcontext in dictionary.Keys)
            {
                var targetFileName = dictionary[dbcontext];
                var t = Task.Run(() =>
                {
                    dbcontext.BackupSqliteDB(targetFileName, cancellationToken, this.Context.DefaultLogger);
                }, cancellationToken);
                tasks.Add(t);
            }
            return Task.WhenAll(tasks.ToArray());
        }
    }
}
