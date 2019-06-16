using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Models;
using DataAccessLayer.DataAccessServices;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BusinessLogicLayer.Services
{
    public class UserManagementService : AppSettingsService, IUserManagementService
    {
        private UserManagementDataService userManagementDataService = null;

        /// <summary>
        /// Required to pass options of connection string (From Appsettings.json file)
        /// </summary>
        /// <param name="dbOptions"></param>
        public UserManagementService(IOptions<DatabaseSettings> dbOptions) : base(dbOptions)
        {
            userManagementDataService = new UserManagementDataService();
        }

        public UserModel IsValidUser(string userName, string password)
        {
            //DB Call
            //DataSet ds = userManagementDataService.IsValidUser(userName, password);

            var userObj = GetUsers().FirstOrDefault(s => s.Username == userName && s.Password == password);
            return userObj;
        }

        public List<UserModel> GetUsers()
        {
            List<UserModel> lst = new List<UserModel>();
            for (int i = 1; i <= 10; i++)
            {
                lst.Add(new UserModel()
                {
                    UserID = i,
                    Firstname = "Rajnikant-" + i,
                    Lastname = "Serasiya-" + i,
                    Username = "rajni-" + i,
                    Password = "test-" + i,
                    Role = (i % 2 == 0 || i == 1) ? "Admin" : "User"
                });
            }
            return lst;
        }


    }
}
