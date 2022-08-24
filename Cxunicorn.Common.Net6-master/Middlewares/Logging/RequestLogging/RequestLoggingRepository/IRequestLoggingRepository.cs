using Cxunicorn.Common.Services.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.RequestLogging.RequestLoggingRepository
{
    public interface IRequestLoggingRepository : IBaseRepository<RequestLogEntity>
    {
    }
}
