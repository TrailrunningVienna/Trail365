using System;
using System.Text;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Logic;
using Microsoft.Azure.Management.Logic.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Trail365.IntegrationTests
{
    //public class LogicAppsHelper
    //{
    //    public async Task<bool>       CreateOrUpdateLogicAppAsync(
    //    string azureSubscriptionId,
    //    string tenantId,
    //    string azureClientId,
    //    string azureClientSecret,
    //    string logicAppResourceGroupName,
    //    string logicAppLocation,
    //    string logicAppId,
    //    string workflowDefinition
    //)
    //    {
    //        var success = false;

    //        var logicManagementClient = await
    //            GetLogicManagementClient(azureSubscriptionId, tenantId, azureClientId, azureClientSecret);

    //        var workflow = new Workflow
    //        {
    //            State = WorkflowState.Enabled,
    //            Location = logicAppLocation,
    //            Definition = JToken.Parse(workflowDefinition)
    //        };

    //        workflow.Definition = JToken.Parse(workflowDefinition);
    //        var resultWorkflow = await
    //            logicManagementClient.Workflows.CreateOrUpdateAsync(logicAppResourceGroupName, logicAppId,
    //                workflow);

    //        if (resultWorkflow.Name == logicAppId)
    //            success = true;

    //        return success;
    //    }

    //    public async Task<bool>
    //       DeleteLogicAppAsync(
    //            string azureSubscriptionId,
    //            string tenantId,
    //            string azureClientId,
    //            string azureClientSecret,
    //            string logicAppResourceGroupName,
    //            string logicAppId)
    //    {
    //        var logicManagementClient = await
    //                    GetLogicManagementClient(azureSubscriptionId, tenantId, azureClientId, azureClientSecret);

    //        await logicManagementClient.Workflows.DeleteAsync(logicAppResourceGroupName, logicAppId);
    //        return true;
    //    }

    //    private static async Task<AuthenticationResult> GetAuthenticationResult(AuthenticationContext authContext,
    //        string resource, ClientCredential credentials)
    //    {
    //        return await authContext.AcquireTokenAsync(resource, credentials);
    //    }

    //    private static async Task<LogicManagementClient> GetLogicManagementClient(string azureSubscriptionId,
    //        string tenantId, string azureClientId, string azureClientSecret)
    //    {
    //    //var environment = AzureEnvironment.AzureGlobalCloud;// PublicEnvironments[EnvironmentName.AzureCloud];

    //    //    //var authority = string.Format("{0}{1}", environment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectory],   tenantId);
    //    //var authority = string.Format("{0}{1}", AzureEnvironment.AzureGlobalCloud.AuthenticationEndpoint, tenantId);

    //    //var authContext = new AuthenticationContext(authority);

    //    //    var credential = new ClientCredential(azureClientId, azureClientSecret);

    //    //    var authResult = await GetAuthenticationResult(authContext,
    //    //        environment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId], credential);

    //    //    var tokenCloudCredentials = new TokenCloudCredentials(azureSubscriptionId, authResult.AccessToken);

    //    //    var tokenCreds = new TokenCredentials(tokenCloudCredentials.Token);
    //    throw new ArgumentNullException("");
    //       // return new LogicManagementClient(tokenCreds) { SubscriptionId = azureSubscriptionId };
    //    }
    //}

    public class AutomationTest
    {
        public bool CreateOrUpdateLogicApp(LogicManagementClient logicManagementClient,
        //string azureSubscriptionId,
        //string tenantId,
        //string azureClientId,
        //string azureClientSecret,
        string logicAppResourceGroupName,
        string logicAppLocation,
        string logicAppId,
        string workflowDefinition
    )
        {
            var success = false;

            var workflow = new Workflow
            {
                State = WorkflowState.Enabled,
                Location = logicAppLocation,
                Definition = JToken.Parse(workflowDefinition)
            };

            workflow.Definition = JToken.Parse(workflowDefinition);
            var resultWorkflow = logicManagementClient.Workflows.CreateOrUpdate(logicAppResourceGroupName, logicAppId, workflow);

            if (resultWorkflow.Name == logicAppId)
                success = true;
            return success;
        }

        //private static void CreateLogicApp(IAzure azure, LogicManagementClient client, string resourceGroupName)
        //{
        //    IResourceGroup resGroup = azure.ResourceGroups.GetByName(resourceGroupName);
        //    Region azureRegion = resGroup.Region;
        //    //ServiceClientCredentials creds = azure.
        //    LogicManagementClient cl = new LogicManagementClient(creds);

        //    //azure.

        //    //azure.ManagementClients

        //}

        private static void RunTaskBasedContainer(IAzure azure,
                                         string resourceGroupName,
                                         string containerGroupName,
                                         string containerImage)
        {
            IResourceGroup resGroup = azure.ResourceGroups.GetByName(resourceGroupName);
            Region azureRegion = resGroup.Region;

            //var chartsJson = JsonConvert.SerializeObject(MarketCharts, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            azure.ContainerGroups.Define(containerGroupName)
                .WithRegion(azureRegion)
                .WithExistingResourceGroup(resourceGroupName)
                .WithLinux()
                .WithPublicImageRegistryOnly()
                .WithoutVolume()
                .DefineContainerInstance(containerGroupName + "-1")
                    .WithImage(containerImage)
                    .WithoutPorts()
                    //.WithExternalTcpPort(Port)
                    .WithCpuCoreCount(1.0)
                    .WithMemorySizeInGB(1)
                    //.WithEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING", Environment.GetEnvironmentVariable("AzureWebJobsStorage"))
                    //.WithEnvironmentVariable("charts", chartsJson)
                    //.WithEnvironmentVariable("TIMEOUT", "5000")
                    .Attach()
                //.WithDnsPrefix(containerGroupName)
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .CreateAsync().GetAwaiter().GetResult();
        }

        //https://medium.com/@bogdanbujdea/starting-an-azure-container-from-an-azure-function-using-c-d72b3a411d1d
        //https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md

        private static IAzure GetAzureContext(string secret)
        {
            return GetAzureContext(secret, out _);
        }

        private static IAzure GetAzureContext(string secret, out ServiceClientCredentials credentials)
        {
            //app "mssautomation"
            string applicationClientID = "876b0a5e-2d1e-4618-9786-2d54fd3bc3f3";
            string tenant = "6373d97c-b41e-4e4f-a0c6-1c1794d11803"; //tenant or domain!
            AzureCredentials creds = new AzureCredentialsFactory().FromServicePrincipal(applicationClientID, secret, tenant, AzureEnvironment.AzureGlobalCloud);
            var auth = Azure.Authenticate(creds);
            IAzure azure = auth.WithSubscription("754eef75-37e0-444d-af39-53c6b4a6ca7c");
            credentials = creds;
            return azure;
        }

        public static string getxx()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("{");
            sb.AppendLine("  \"$schema\": \"https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#\",");
            sb.AppendLine("  \"contentVersion\": \"1.0.0.0\",");
            sb.AppendLine("  \"parameters\": {");
            sb.AppendLine("    \"$connections\": {");
            sb.AppendLine("      \"defaultValue\": {},");
            sb.AppendLine("      \"type\": \"Object\"");
            sb.AppendLine("    }");
            sb.AppendLine("  },");
            sb.AppendLine("  \"triggers\": {");
            sb.AppendLine("    \"Recurrence\": {");
            sb.AppendLine("      \"recurrence\": {");
            sb.AppendLine("        \"frequency\": \"Week\",");
            sb.AppendLine("        \"interval\": 3");
            sb.AppendLine("      },");
            sb.AppendLine("      \"type\": \"Recurrence\"");
            sb.AppendLine("    }");
            sb.AppendLine("  },");
            sb.AppendLine("  \"actions\": {");
            sb.AppendLine("    \"Create_container_group\": {");
            sb.AppendLine("      \"runAfter\": {},");
            sb.AppendLine("      \"type\": \"ApiConnection\",");
            sb.AppendLine("      \"inputs\": {");
            sb.AppendLine("        \"body\": {");
            sb.AppendLine("          \"location\": \"West Europe\",");
            sb.AppendLine("          \"properties\": {");
            sb.AppendLine("            \"containers\": [");
            sb.AppendLine("              {");
            sb.AppendLine("                \"name\": \"pbf-splitter-dev\",");
            sb.AppendLine("                \"properties\": {");
            sb.AppendLine("                  \"image\": \"mssxplat/pbf-splitter:latest\",");
            sb.AppendLine("                  \"resources\": {");
            sb.AppendLine("                    \"limits\": {");
            sb.AppendLine("                      \"cpu\": 1,");
            sb.AppendLine("                      \"memoryInGB\": 1");
            sb.AppendLine("                    },");
            sb.AppendLine("                    \"requests\": {");
            sb.AppendLine("                      \"cpu\": 1,");
            sb.AppendLine("                      \"memoryInGB\": 1");
            sb.AppendLine("                    }");
            sb.AppendLine("                  }");
            sb.AppendLine("                }");
            sb.AppendLine("              }");
            sb.AppendLine("            ],");
            sb.AppendLine("            \"osType\": \"Linux\",");
            sb.AppendLine("            \"restartPolicy\": \"Never\"");
            sb.AppendLine("          }");
            sb.AppendLine("        },");
            sb.AppendLine("        \"host\": {");
            sb.AppendLine("          \"connection\": {");
            sb.AppendLine("            \"name\": \"@parameters('$connections')['aci_1']['connectionId']\"");
            sb.AppendLine("          }");
            sb.AppendLine("        },");
            sb.AppendLine("        \"method\": \"put\",");
            sb.AppendLine("        \"path\": \"/subscriptions/@{encodeURIComponent('754eef75-37e0-444d-af39-53c6b4a6ca7c')}/resourceGroups/@{encodeURIComponent('RG_LINUX_BAGET')}/providers/Microsoft.ContainerInstance/containerGroups/@{encodeURIComponent('cg_trail_dev')}\",");
            sb.AppendLine("        \"queries\": {");
            sb.AppendLine("          \"x-ms-api-version\": \"2017-10-01-preview\"");
            sb.AppendLine("        }");
            sb.AppendLine("      }");
            sb.AppendLine("    }");
            sb.AppendLine("  },");
            sb.AppendLine("  \"outputs\": {}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        //[SkippableFact]

        //public void CreateLogicApp()
        //{
        //    Skip.If(true);
        //    string secret = string.Format("{0}", Environment.GetEnvironmentVariable("AZURE_SERVICE_PRINCIPAL_SECRET"));
        //    Skip.If(string.IsNullOrEmpty(secret));

        //    GetAzureContext(secret, out ServiceClientCredentials creds);
        //    LogicManagementClient client = new LogicManagementClient(creds)
        //    {
        //        SubscriptionId = "754eef75-37e0-444d-af39-53c6b4a6ca7c"
        //    };
        //    //string rg = "RG_Development";
        //    string s = AutomationResource.CreateExecuteContainerWorkflow();
        //    Assert.False(string.IsNullOrEmpty(s));
        //    Assert.NotNull(client);
        //}

        [SkippableFact]
        public void ReadAllWorkflowDefinitions()
        {
            Skip.If(true);
            string secret = string.Format("{0}", Environment.GetEnvironmentVariable("AZURE_SERVICE_PRINCIPAL_SECRET"));
            Skip.If(string.IsNullOrEmpty(secret));

            GetAzureContext(secret, out ServiceClientCredentials creds);

            using (var client = new LogicManagementClient(creds)
            {
                SubscriptionId = "754eef75-37e0-444d-af39-53c6b4a6ca7c"
            })
            {
                string rg = "RG_Development";
                //wie geht connections anlegen ?

                foreach (var x in client.Workflows.ListByResourceGroup(rg))
                {
                    System.Diagnostics.Debug.WriteLine(x.Name);

                    foreach (var p in x.Parameters)
                    {
                        System.Diagnostics.Debug.WriteLine(p.Key);
                        Newtonsoft.Json.Linq.JObject entry = p.Value.Value as Newtonsoft.Json.Linq.JObject;
                        System.Diagnostics.Debug.WriteLine(entry.ToString());
                    }

                    System.Diagnostics.Debug.WriteLine(x.Definition.GetType().ToString());
                    Newtonsoft.Json.Linq.JObject def = x.Definition as Newtonsoft.Json.Linq.JObject;
                    System.Diagnostics.Debug.WriteLine(def.ToString());
                }

                //System.Diagnostics.Debug.WriteLine(azure.ToString());
                //foreach (var rg in azure.ResourceGroups.List())
                //{
                //    System.Diagnostics.Debug.WriteLine(rg.Name);
                //}

                //foreach (var cg in azure.ContainerGroups.List())
                //{
                //    System.Diagnostics.Debug.WriteLine(cg.Name);
                //}

                //var rgLinux = azure.ResourceGroups.GetByName("RG_Production_Linux");

                //// var containerGroup = azure.ContainerGroups.GetByResourceGroup(rgLinux.Name, "pbf-splitter");

                //RunTaskBasedContainer(azure, rgLinux.Name, "pbf-splitter", "mssxplat/pbf-splitter:latest");

                ////foreach (var cnt in containerGroup.Containers.Values)
                ////{
                ////    cnt.Command.
                ////}
            }
        }

        [SkippableFact]
        public void xxx()
        {
            Skip.If(true);
            string secret = string.Format("{0}", Environment.GetEnvironmentVariable("AZURE_SERVICE_PRINCIPAL_SECRET"));
            Skip.If(string.IsNullOrEmpty(secret));
            var azure = GetAzureContext(secret);

            System.Diagnostics.Debug.WriteLine(azure.ToString());
            foreach (var rg in azure.ResourceGroups.List())
            {
                System.Diagnostics.Debug.WriteLine(rg.Name);
            }

            foreach (var cg in azure.ContainerGroups.List())
            {
                System.Diagnostics.Debug.WriteLine(cg.Name);
            }

            var rgLinux = azure.ResourceGroups.GetByName("RG_Production_Linux");

            // var containerGroup = azure.ContainerGroups.GetByResourceGroup(rgLinux.Name, "pbf-splitter");

            RunTaskBasedContainer(azure, rgLinux.Name, "pbf-splitter", "mssxplat/pbf-splitter:latest");

            //foreach (var cnt in containerGroup.Containers.Values)
            //{
            //    cnt.Command.
            //}
        }
    }
}
