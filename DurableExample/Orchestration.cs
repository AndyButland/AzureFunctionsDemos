namespace DurableExample
{
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;

    public static class Orchestration
    {
        [FunctionName("HelloWorld")]
        public static async Task<string> Run([OrchestrationTrigger]DurableOrchestrationContext context)
        {
            var name = context.GetInput<string>();
            var result = await context.CallActivityAsync<string>("SayHello", name);
            return result;
        }

        [FunctionName("SayHello")]
        public static string SayHello([ActivityTrigger]DurableActivityContext context)
        {
            string name = context.GetInput<string>();
            return $"Hello {name}!";
        }
    }
}
