using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Trail365
{
    public class AzureAppServiceDeploymentStatus
    {
        public bool Complete { get; set; }

        public string Status { get; set; }

        public TimeSpan? Age
        {
            get
            {
                if (this.LastSuccessEndTime.HasValue == false)
                {
                    return null;
                }
                return DateTimeOffset.Now.Subtract(this.LastSuccessEndTime.Value);
            }
        }

        public DateTimeOffset? LastSuccessEndTime { get; set; }

        public static AzureAppServiceDeploymentStatus ReadFromString(string xml)
        {
            if (string.IsNullOrEmpty(xml)) throw new ArgumentNullException(nameof(xml));

            var result = new AzureAppServiceDeploymentStatus();

            XDocument doc = XDocument.Parse(xml);

            foreach (XElement element in doc.Descendants().Where(p => p.HasElements == false))
            {
                string keyName = element.Name.LocalName;
                if (keyName == "status")
                {
                    result.Status = element.Value;
                }
                if (keyName == "complete")
                {
                    result.Complete = bool.Parse(element.Value);
                }

                if (keyName == "lastSuccessEndTime")
                {
                    if (DateTimeOffset.TryParse(element.Value, out var res))
                    {
                        result.LastSuccessEndTime = res;
                    }
                }
            }
            return result;
        }

        public static AzureAppServiceDeploymentStatus ReadFrom(string path)
        {
            string xml = File.ReadAllText(path);
            return ReadFromString(xml);
        }
    }
}
