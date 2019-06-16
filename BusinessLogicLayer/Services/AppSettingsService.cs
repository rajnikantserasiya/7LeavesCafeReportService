using BusinessLogicLayer.Models;
using Helpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Services
{
    public class AppSettingsService
    {        
        public AppSettingsService(IOptions<DatabaseSettings> dbOptions)
        {            
            DBHelper.SetDBConnectionString(dbOptions.Value.ConnectionString);
        }
    }
}
