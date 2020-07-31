using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DurableHello.Monitor
{
    public static class MonitorSequence
    {
        [FunctionName("Monitor_Hello")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            context.SetCustomStatus("0% Completed");
            outputs.Add(await context.CallActivityAsync<string>("Monitor_SayHello", "Tokyo"));
            context.SetCustomStatus("33% Completed");
            outputs.Add(await context.CallActivityAsync<string>("Monitor_SayHello", "Seattle"));
            context.SetCustomStatus("66% Completed");
            outputs.Add(await context.CallActivityAsync<string>("Monitor_SayHello_Direct", "London"));
            context.SetCustomStatus("100% Completed");

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Monitor_SayHello")]
        public static string SayHello([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            var name = context.GetInput<string>();
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("Monitor_SayHello_Direct")]
        public static string SayHelloDirect([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }


        [FunctionName("Monitor_Hello_HttpStart_CustomStatus")]
        public static async Task<HttpResponseMessage> RunCustomStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, methods: "post", Route = "orchestrators/monitor/custom-status")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var eventData = await req.Content.ReadAsAsync<object>();
            var instanceId = await starter.StartNewAsync("Monitor_Hello", eventData);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            var durableOrchestrationStatus = await starter.GetStatusAsync(instanceId);
            while (durableOrchestrationStatus.CustomStatus.ToString() != "100% Completed")
            {
                await Task.Delay(200);
                durableOrchestrationStatus = await starter.GetStatusAsync(instanceId);
            }

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(durableOrchestrationStatus))
            };
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            return httpResponseMessage;
        }

        [FunctionName("Monitor_Hello_HttpStart_RuntimeStatus")]
        public static async Task<HttpResponseMessage> RunRuntimeStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, methods: "post", Route = "orchestrators/monitor/runtime-status")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var eventData = await req.Content.ReadAsAsync<object>();
            var instanceId = await starter.StartNewAsync("Monitor_Hello", eventData);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            var durableOrchestrationStatus = await starter.GetStatusAsync(instanceId);
            while (durableOrchestrationStatus.RuntimeStatus != OrchestrationRuntimeStatus.Completed)
            {
                await Task.Delay(200);
                durableOrchestrationStatus = await starter.GetStatusAsync(instanceId);
            }

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(durableOrchestrationStatus))
            };
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            return httpResponseMessage;
        }
    }
}