using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Demos.Durable.Chaining
{
    public static class ChainSequence
    {
        [FunctionName("Chaining_Hello")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Chaining_SayHello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("Chaining_SayHello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("Chaining_SayHello_Direct", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Chaining_SayHello")]
        public static string SayHello([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            string name = context.GetInput<string>();
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        } 

        [FunctionName("Chaining_SayHello_Direct")]
        public static string SayHelloDirect([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!"; 
        }

        [FunctionName("Chaining_Hello_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/chaining/start")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Chaining_Hello", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}