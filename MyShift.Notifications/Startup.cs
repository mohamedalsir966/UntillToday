using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MyShift.Notifications;
using MyShift.Notifications.Service;
using Persistence.Repositories;
using Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Cxunicorn.Common.Services.BotNotification;
using Cxunicorn.Common.Middlewares.TeamsActivityHandler.Users;
using Cxunicorn.Common.Middlewares.Logging.Logger.LoggingRepository;
using Cxunicorn.Common.Services.Tables;
using Cxunicorn.Common.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using MyShift.Notifications.Bot;
using MyShift.Notifications.Helpers.Notification;

[assembly: FunctionsStartup(typeof(Startup))]

namespace MyShift.Notifications
{
  public  class Startup : FunctionsStartup
    {

       
       
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();
            //builder.Services.Configure<RepositoryOptions>(config.GetSection("repositoryOptions"));
            //builder.Services.AddBlobClient();INotificationDataHelper

            //builder.Services.AddTableClient();

            builder.Services.AddScoped<IService, ServiceEng>();
            builder.Services.AddScoped<ILogsRepository, LogsRepository>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
           // builder.Services.AddScoped<IUsersDataRepository, UsersDataRepository>();
            builder.Services.AddScoped<INotificationDataHelper, NotificationDataHelper>();
           //builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
           // builder.Services.AddScoped<ILoggingRepository, LoggingRepository>();
            builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            builder.Services.AddTransient<IBot, ProactiveBot>();
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer("Data Source=.;Initial Catalog=NotificationDBV2;Integrated Security=True"));

        }
    }
}
