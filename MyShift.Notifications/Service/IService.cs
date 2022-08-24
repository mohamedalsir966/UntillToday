using MyShift.Notifications.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Notifications.Service
{
    public interface IService
    {
        Task<string> GetDataToNotifiy();
       // bool SendDataToQueue(string massge);
    }
}
