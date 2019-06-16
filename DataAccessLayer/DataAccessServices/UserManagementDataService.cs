using Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DataAccessLayer.DataAccessServices
{
    public class UserManagementDataService
    {
        public DataSet IsValidUser(string userName, string password)
        {
            return DBHelper.ExecuteProcedure(Constants.LoginUser_SP, SetValidUserParameters(userName, password));
        }

        private List<SqlParameter> SetValidUserParameters(string userName, string password)
        {
            List<SqlParameter> filters = new List<SqlParameter>();
            filters.Add(new SqlParameter("@username", userName));
            filters.Add(new SqlParameter("@password", password));

            return filters;
        }
    }

}
