using Cxunicorn.Common.Middlewares.Logging.Logger.LoggingRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.Logger
{
    public class AzureTableLogger : ILogger
    {
        private readonly ILoggingRepository _loggerRepository;
        public AzureTableLogger(ILoggingRepository loggingRepository)
        {
            _loggerRepository = loggingRepository;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }
        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var log = new LoggingEntity
            {
                EventId = eventId.ToString(),
                LogLevel = logLevel.ToString(),
                Message = formatter(state, exception),//exception?.ToString(),
                PartitionKey = DateTime.Now.ToString("yyyyMMdd"),
            };

            await _loggerRepository.TableClient.AddEntityAsync(log);
        }
    }
}
