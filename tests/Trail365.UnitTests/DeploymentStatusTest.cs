using System.Text;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class DeploymentStatusTest
    {
        public static string GetStatusXmlSample()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<deployment>");
            sb.Append("    <id>e62359528137867f0228b0ee2612455afaad1b2d</id>");
            sb.Append("    <author>Werner Mairl</author>");
            sb.Append("    <deployer>Bitbucket</deployer>");
            sb.Append("    <authorEmail>werner.mairl@gmail.co</authorEmail>");
            sb.Append("    <message>new sample track");
            sb.Append("</message>");
            sb.Append("    <progress></progress>");
            sb.Append("    <status>Success</status>");
            sb.Append("    <statusText></statusText>");
            sb.Append("    <lastSuccessEndTime>2019-08-08T03:49:19.5238305Z</lastSuccessEndTime>");
            sb.Append("    <receivedTime>2019-08-08T03:48:41.508919Z</receivedTime>");
            sb.Append("    <startTime>2019-08-08T03:48:41.7432475Z</startTime>");
            sb.Append("    <endTime>2019-08-08T03:49:19.5238305Z</endTime>");
            sb.Append("    <complete>True</complete>");
            sb.Append("    <is_temp>False</is_temp>");
            sb.Append("    <is_readonly>False</is_readonly>");
            sb.Append("</deployment>");
            return sb.ToString();
        }
    }
}
