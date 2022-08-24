using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Graph
{
    public class GraphConstants
    {
        public enum GraphPermissionType
        {
            /// <summary>
            /// This represents application permission of Microsoft Graph.
            /// </summary>
            Application,

            /// <summary>
            /// This represents delgeate permission of Microsoft Graph.
            /// </summary>
            Delegate,
        }
    }
}
