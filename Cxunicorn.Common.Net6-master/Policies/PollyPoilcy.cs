using Microsoft.Graph;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Policies
{
    public class PollyPolicy
    {
        /// <summary>
        /// Get the graph retry policy.
        /// </summary>
        /// <param name="maxAttempts">the number of max attempts.</param>
        /// <returns>A retry policy that can be applied to async delegates.</returns>
        public static AsyncRetryPolicy GetGraphRetryPolicy(int maxAttempts)
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: maxAttempts);

            // Only Handling 502 Bad Gateway Exception
            // Other exception such as 429, 503, 504 is handled by default by Graph SDK.
            return Policy
                .Handle<ServiceException>(e =>
                e.StatusCode == HttpStatusCode.BadGateway)
                .WaitAndRetryAsync(delay);
        }
        public static AsyncRetryPolicy GetRetryPolicy(int maxAttempts, TimeSpan RetryDelay)
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: RetryDelay, retryCount: maxAttempts);
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(delay);
        }
    }
}
