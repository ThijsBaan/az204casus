using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Threading.Tasks;

namespace ServiceBusReceiver
{
    class Program
    {
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://johngorterns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=V5I1pFT3e2wZwgDY26YVLnJmhGrN6pW3tCI3KegCeLw=";

        // name of your Service Bus topic
        static string topicName = "thijs";
        static string subName = "sub";

        static async Task Main()
        {

            ManagementClient managementClient = new ManagementClient(connectionString);
            try
            {
                await managementClient.GetTopicAsync(topicName);
            }
            catch (Microsoft.Azure.ServiceBus.MessagingEntityNotFoundException)
            {
                await managementClient.CreateTopicAsync(topicName);
            }
            try
            {
                await managementClient.GetSubscriptionAsync(topicName, subName);
            }
            catch (Microsoft.Azure.ServiceBus.MessagingEntityNotFoundException)
            {
                await managementClient.CreateSubscriptionAsync(topicName, subName);
            }
            

            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Create the clients that we'll use for sending and processing messages.
            ServiceBusClient client = new ServiceBusClient(connectionString);

            // create a processor that we can use to process the messages
            ServiceBusProcessor processor = client.CreateProcessor(topicName, subName, new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body} from subscription: sub");

            // complete the message. messages is deleted from the subscription. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
