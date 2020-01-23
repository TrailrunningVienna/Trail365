using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Trail365.Web
{
    public static class Helper
    {
        public static string GetStartTime()
        {
            var t = Process.GetCurrentProcess().StartTime.ToUniversalTime();
            return t.ToString("o");
        }

        public static string GetUptime()
        {
            TimeSpan upTime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            return upTime.ToString();
        }

        public static string GetProductLabel()
        {
            return $"Trail 365 (v{GetProductVersionFromEntryAssembly()})";
        }

        /// <summary>
        /// reading from InformationalVersionAttribute, the only Version that is integrated as Attribute directly on the assembly!
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetProductVersion(Assembly assembly)
        {
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute));
            if (!(attributes.SingleOrDefault() is AssemblyInformationalVersionAttribute attr))
            {
                return "0.0.0.0-undefined";
            }
            return attr.InformationalVersion;
        }

        public static string GetProcessInfo()
        {
            string platform;
            if (System.Environment.Is64BitProcess)
            {
                platform = "64 Bit";
            }
            else
            {
                platform = "32 Bit";
            }
            System.Diagnostics.Process current = System.Diagnostics.Process.GetCurrentProcess();

            string osPF = string.Empty;
            OSPlatform[] pfList = new OSPlatform[] { OSPlatform.Linux, OSPlatform.OSX, OSPlatform.Windows };
            foreach (OSPlatform pf in pfList)
            {
                if (RuntimeInformation.IsOSPlatform(pf))
                {
                    osPF = string.Format("on {0}", pf);
                }
            }
            return string.Format("{0} Cores {1} {2}", System.Environment.ProcessorCount, osPF, platform).Trim();
        }

        public static string GetMinorProductVersionFromEntryAssembly()
        {
            Assembly ass = Assembly.GetEntryAssembly();
            string productVersion = GetProductVersion(ass);
            var splits = productVersion.Split(new char[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length < 2) throw new InvalidOperationException("Version cannot be splited");
            return $"{splits[0]}.{splits[1]}";
        }

        public static string GetMajorProductVersionFromEntryAssembly()
        {
            Assembly ass = Assembly.GetEntryAssembly();
            string productVersion = GetProductVersion(ass);
            var splits = productVersion.Split(new char[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length < 2) throw new InvalidOperationException("Version cannot be splited");
            return $"{splits[0]}";
        }

        public static string GetProductVersionFromEntryAssembly()
        {
            Assembly ass = Assembly.GetEntryAssembly();
            return GetProductVersion(ass);
        }
    }
}
