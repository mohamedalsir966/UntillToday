using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Queue
{
    public abstract class BaseQueue<T> : IBaseQueue<T>
    {
        private readonly ServiceBusSender sender;
        public static readonly int MaxNumberOfMessagesInBatchRequest = 100;
        public BaseQueue(ServiceBusClient client, string queueName)
        {
            this.sender = client.CreateSender(queueName);
        }

        public async Task SendAsync(T queueMessageContent)
        {
            if (queueMessageContent == null)
            {
                throw new ArgumentNullException(nameof(queueMessageContent));
            }

            var messageBody = JsonConvert.SerializeObject(queueMessageContent);
            var serviceBusMessage = new ServiceBusMessage(messageBody);
            await this.sender.SendMessageAsync(serviceBusMessage);
        }

        public async Task SendAsync(IEnumerable<T> queueMessageContentBatch)
        {
            if (queueMessageContentBatch == null)
            {
                throw new ArgumentNullException(nameof(queueMessageContentBatch));
            }

            var queueMessageContentBatchAsList = queueMessageContentBatch.ToList();

            // Check that the number of messages to add to the queue in the batch request is not
            // more than the maximum allowed.
            if (queueMessageContentBatchAsList.Count > BaseQueue<T>.MaxNumberOfMessagesInBatchRequest)
            {
                throw new InvalidOperationException("Exceeded maximum Azure service bus message batch size.");
            }

            // Create batch list of messages to add to the queue.
            var serviceBusMessages = queueMessageContentBatchAsList
                .Select(queueMessageContent =>
                {
                    var messageBody = JsonConvert.SerializeObject(queueMessageContent);
                    return new ServiceBusMessage(messageBody);
                })
                .ToList();

            await this.sender.SendMessagesAsync(serviceBusMessages);
        }

        public async Task SendDelayedAsync(T queueMessageContent, double delayNumberOfSeconds)
        {
            if (queueMessageContent == null)
            {
                throw new ArgumentNullException(nameof(queueMessageContent));
            }

            var messageBody = JsonConvert.SerializeObject(queueMessageContent);
            var scheduledEnqueueTimeUtc = DateTime.UtcNow + TimeSpan.FromSeconds(delayNumberOfSeconds);
            var serviceBusMessage = new ServiceBusMessage(messageBody);
            await this.sender.ScheduleMessageAsync(serviceBusMessage, scheduledEnqueueTimeUtc);
        }
    }
}
