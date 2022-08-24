using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.BotNotification
{
    public class SendMessageResponse
    {
        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the result type.
        /// </summary>
        public SendMessageResult ResultType { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list representing all of the status code responses received when trying
        /// to send the notification to the recipient. These results can include success, failure, and throttle
        /// status codes.
        /// </summary>
        public string? AllSendStatusCodes { get; set; }

        /// <summary>
        /// Gets or sets the number of throttle responses.
        /// </summary>
        public int TotalNumberOfSendThrottles { get; set; }

        /// <summary>
        /// Gets or sets the error message when trying to send the notification.
        /// </summary>
        public string? ErrorMessage { get; set; }
        public string? ActivityId { get; set; }
    }
    public enum SendMessageResult
    {
        /// <summary>
        /// Type indicating sending the notification succeeded.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Type indicating sending the notification was throttled.
        /// </summary>
        Throttled,

        /// <summary>
        /// Type indicating sending the notification failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Type indicating that the recipient can't be found.
        /// When sending a notification to a removed recipient, the send function gets 404 error.
        /// The recipient should be excluded from the list.
        /// </summary>
        RecipientNotFound,
    }
}
