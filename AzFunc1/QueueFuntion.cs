using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzFunc1
{
    public class QueueFuntion
    {
        [FunctionName("QueueFuntion")]
        public void Run([QueueTrigger("example-queue", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("C# Queue Trigger function processed a request.");
                Console.WriteLine($"Processing message: {myQueueItem}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error processing message: {myQueueItem}. Error: {ex.Message}");
                throw;
            }
        }
    }
}
