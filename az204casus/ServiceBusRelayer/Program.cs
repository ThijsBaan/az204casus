using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Threading.Tasks;

namespace ServiceBusRelayer
{
    class Program
    {
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://johngorterns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=V5I1pFT3e2wZwgDY26YVLnJmhGrN6pW3tCI3KegCeLw=";

        // name of your Service Bus topic
        static string topicName = "thijs";
        static string subName = "sub";

        static string relayName = "youri";

        static async Task Main()
        {
            await Initialize(topicName);
            await Initialize(relayName);
            await Receiver();
        }

        private static async Task Receiver()
        {
            ServiceBusClient client = new ServiceBusClient(connectionString);

            ServiceBusProcessor processor = client.CreateProcessor(topicName, subName, new ServiceBusProcessorOptions());

            try
            {
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body} from subscription: sub");

            if (String.IsNullOrEmpty(body))
            {
                body = "";
            }

            await Messager(body);

            await args.CompleteMessageAsync(args.Message);
        }

        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        static async Task Messager(string message)
        {
            ServiceBusClient client = new ServiceBusClient(connectionString);

            ServiceBusSender sender = client.CreateSender(relayName);

            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            message += " https://www.youtube.com/watch?v=dQw4w9WgXcQ ";

            // try adding a message to the batch
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(message)))
            {
                // if it is too large for the batch
                throw new Exception($"The message is too large to fit in the batch.");
            }

            try
            {
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A message has been published to the topic.");
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }

        private static async Task Initialize(string topic)
        {
            ManagementClient managementClient = new ManagementClient(connectionString);
            try
            {
                await managementClient.GetTopicAsync(topic);
            }
            catch (Microsoft.Azure.ServiceBus.MessagingEntityNotFoundException)
            {
                Console.WriteLine($"Topic {topic} not found, creating...");
                await managementClient.CreateTopicAsync(topic);
            }
            try
            {
                await managementClient.GetSubscriptionAsync(topic, subName);
            }
            catch (Microsoft.Azure.ServiceBus.MessagingEntityNotFoundException)
            {
                Console.WriteLine($"Subscription {subName} not found, creating...");
                await managementClient.CreateSubscriptionAsync(topic, subName);
            }
        }
    }
}
