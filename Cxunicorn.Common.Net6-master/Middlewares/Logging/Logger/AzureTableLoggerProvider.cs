using Cxunicorn.Common.Middlewares.Logging.Logger.LoggingRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.Logger
{
    public class AzureTableLoggerProvider : ILoggerProvider
    {
        private readonly ILoggingRepository _loggingRepository;
        public AzureTableLoggerProvider(ILoggingRepository loggingRepository)
        {
            _loggingRepository = loggingRepository;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new AzureTableLogger(_loggingRepository);
        }

        public void Dispose()
        {
        }
    }
}
