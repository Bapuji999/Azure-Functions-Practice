using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzFunc1
{
    public class TimerTriggeredFunction
    {
        [FunctionName("TimerTriggeredFunction")]
        public async Task Run([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"C# Timer trigger function executed at: {DateTime.Now}");

            string apiUrl = "https://localhost:7036/WeatherForecast";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(300);
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        Console.WriteLine($"Response from the GetWeatherForecast: {result}");

                        Console.ForegroundColor = ConsoleColor.White;
                        var storageAccountConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                        QueueClient queueClient = new QueueClient(storageAccountConnectionString, "example-queue");

                        byte[] resultBytes = Encoding.UTF8.GetBytes(result);
                        await InsertMessage(queueClient, Convert.ToBase64String(resultBytes));

                        //var messages = await ReadMessage(queueClient);
                        //foreach (var message in messages)
                        //{
                        //    byte[] decodedBytes = Convert.FromBase64String(message.MessageText);
                        //    string originalResult = Encoding.UTF8.GetString(decodedBytes);
                        //    Console.ForegroundColor = ConsoleColor.White;
                        //    Console.WriteLine($"Message in Queue: {originalResult}");
                        //}
                        //await DeleteQueue(queueClient);

                    }
                    else
                    {
                        log.LogInformation($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (HttpRequestException e)
                {
                    log.LogInformation($"Request error: {e.Message}");
                }
            }
        }
        public async static Task InsertMessage(QueueClient queueClient, string result)
        {
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(result, TimeSpan.FromSeconds(0), TimeSpan.FromDays(3));
        }
        public async static Task<QueueMessage[]> ReadMessage(QueueClient queueClient)
        {
            QueueMessage[] retrivedMessage = await queueClient.ReceiveMessagesAsync(1);
            return retrivedMessage;
        }
        public async static Task DeleteQueue(QueueClient queueClient)
        {
            await queueClient.DeleteIfExistsAsync();
        }
    }
}
