using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Queue
{
    public interface IBaseQueue<T>
    {
        public Task SendAsync(T queueMessageContent);
        public Task SendAsync(IEnumerable<T> queueMessageContentBatch);
        public Task SendDelayedAsync(T queueMessageContent, double delayNumberOfSeconds);
    }
}
