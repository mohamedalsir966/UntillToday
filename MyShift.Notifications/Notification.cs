using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MyShift.Notifications.Entitys;
using MyShift.Notifications.Service;
using Newtonsoft.Json;

namespace MyShift.Notifications
{
    public class Notification
    {
        
        private readonly IService _serviceEngine;
        
        public Notification( IService serviceEngin)
        {
            _serviceEngine = serviceEngin;
        }
        
        [FunctionName("Notification")]
        public async Task NotificationFunction([QueueTrigger("%QueueTriggerName%", Connection = "AzureWebJobsStorage")] string myQueueItem, Microsoft.Extensions.Logging.ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
             PaginatedList<ShiftEntity> QueueMessage = null;

            QueueMessage = JsonConvert.DeserializeObject<PaginatedList<ShiftEntity>>(myQueueItem); 
            var Respose = await _serviceEngine.GetDataToNotifiy();
               
        }
    }
}
