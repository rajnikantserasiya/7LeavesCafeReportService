using BusinessLogicLayer.Models;
using System.Collections.Generic;

namespace BusinessLogicLayer.Contracts
{
    public interface IUserManagementService
    {
        UserModel IsValidUser(string username, string password);

        List<UserModel> GetUsers();
    }
}
