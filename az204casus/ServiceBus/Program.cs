using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Threading.Tasks;

namespace ServiceBusMessager
{
    class Program
    {
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://johngorterns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=V5I1pFT3e2wZwgDY26YVLnJmhGrN6pW3tCI3KegCeLw=";

        // name of your Service Bus topic
        static string topicName = "thijs";

        // number of messages to be sent to the topic
        private const int numOfMessages = 3;

        static async Task Main()
        {

            ManagementClient managementClient = new ManagementClient(connectionString);
            TopicDescription topic = null;
            try
            {
                topic = await managementClient.GetTopicAsync(topicName);
            } catch (Microsoft.Azure.ServiceBus.MessagingEntityNotFoundException)
            {
                topic = await managementClient.CreateTopicAsync(topicName);
            }


            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Create the clients that we'll use for sending and processing messages.
            ServiceBusClient client = new ServiceBusClient(connectionString);

            ServiceBusSender sender = client.CreateSender(topicName);

            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= numOfMessages; i++)
            {
                // try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    // if it is too large for the batch
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus topic
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages} messages has been published to the topic.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }
    }
}
