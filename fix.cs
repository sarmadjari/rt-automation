using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;



namespace MySpace
{
    public static class fix
    {

        public class Properties2
        {
            public string addressPrefix { get; set; }
            public string nextHopType { get; set; }
            public string nextHopIpAddress { get; set; }
            public string hasBgpOverride { get; set; }
        }
        public class RoutesItem
        {
            public string name { get; set; }
            public string id { get; set; }
            public Properties2 properties { get; set; }
            public string type { get; set; }
        }

        public class Properties
        {
            public bool disableBgpRoutePropagation { get; set; }
            public List <RoutesItem> routes { get; set; }
        }

        public class routeTables
        {
            public string name { get; set; }
            public string id { get; set; }
            public string location { get; set; }
            public Properties properties { get; set; }
        }

        [FunctionName("fix")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Status: Function Triggered");

            //-------------------------------------------------------------------------------------------------------------------
            var subscriptionId = "";
            var resourceGroupName = "";
            var routeTableName = "";

            // Authorization
            var endpointURL ="https://management.azure.com/subscriptions/"+subscriptionId+"/resourceGroups/"+resourceGroupName+"/providers/Microsoft.Network/routeTables/"+routeTableName+"?api-version=2022-11-01";
            var credential = new DefaultAzureCredential();
            var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://management.azure.com/.default" }));
            var accessToken = token.Token;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Get current Route Tables settings
            var responseGet = await client.GetAsync(endpointURL);
            var jsonResponse = await responseGet.Content.ReadAsStringAsync();
            log.LogInformation("Status: current settings: " + jsonResponse);
            dynamic js = JsonConvert.DeserializeObject(jsonResponse);

            routeTables rt = new routeTables();
            rt.properties = new Properties();
            rt = js.ToObject<routeTables>();

            // Disable BGP Route Propagation 
            rt.properties.disableBgpRoutePropagation = true;

            // Save thew new Route Tables settings
            string jsonString = JsonConvert.SerializeObject(rt);
            log.LogInformation("Status: new settings: " + jsonString);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var responsePut = await client.PutAsync(endpointURL,httpContent);
            jsonResponse = await responsePut.Content.ReadAsStringAsync();
            log.LogInformation("Status: " + responsePut.StatusCode);

            //-------------------------------------------------------------------------------------------------------------------

            return new OkObjectResult(jsonResponse);
        }
    }
}
