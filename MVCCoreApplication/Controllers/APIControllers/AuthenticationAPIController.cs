using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Models;
using LoggerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MVCCoreApplication.TokenHandler;

namespace MVCCoreApplication.Controllers.APIControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationAPIController : ControllerBase
    {

        private readonly IUserManagementService _userService;
        private ILoggerManager _logger;
        private readonly TokenManagementModel _tokenManagement;
        private JWTTokenHandler objTokenHandler = null;

        public AuthenticationAPIController(IUserManagementService userService, IOptions<TokenManagementModel> tokenManagement, ILoggerManager logger)
        {
            _userService = userService;
            _logger = logger;
            _tokenManagement = tokenManagement.Value;
            objTokenHandler = new JWTTokenHandler(_tokenManagement);
        }

        [AllowAnonymous]
        [HttpPost, Route("authenticate")]
        public IActionResult Authenticate([FromBody] TokenRequestModel request)
        {
            _logger.LogDebug(string.Format("authenticate api called with parameters. Username:{0} ", request.Username));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            UserModel objUser = _userService.IsValidUser(request.Username, request.Password);
            if (objUser == null)
            {
                _logger.LogDebug(string.Format("User is not valid. Username:{0} ", request.Username));
                return BadRequest(ResponseMessageModel.CreateResponseMessage("Invalid User", "Username or password is not correct."));
            }

            objUser.AccessToken = objTokenHandler.GenerateJWTToken(objUser.UserID);
            objUser.RefreshToken = objTokenHandler.GenerateRefreshToken();

            _logger.LogDebug(string.Format("Generated access token and refresh token for UserID:{0}", objUser.UserID));

            return Ok(objUser);
        }

        [AllowAnonymous]
        [HttpPost, Route("refreshtoken")]
        public IActionResult Refresh(RefreshTokenModel objRefreshTokenModel)
        {
            _logger.LogDebug(string.Format("refreshtoken method is called."));

            if (string.IsNullOrEmpty(objRefreshTokenModel.AccessToken) || string.IsNullOrWhiteSpace(objRefreshTokenModel.AccessToken)
                || string.IsNullOrEmpty(objRefreshTokenModel.RefreshToken) || string.IsNullOrWhiteSpace(objRefreshTokenModel.RefreshToken)
                || objRefreshTokenModel.LoginUserID == 0)
                return BadRequest(ResponseMessageModel.CreateResponseMessage("Token invalid.", "Access token and Refresh Token should not be empty."));

            var principal = objTokenHandler.GetPrincipalFromExpiredToken(objRefreshTokenModel.AccessToken);            

            string identityName = principal.Identity.Name;
            if (string.IsNullOrEmpty(identityName) || string.IsNullOrWhiteSpace(identityName))
            {
                _logger.LogDebug(string.Format("Not able to identify user from JWT authentication mechanisam.Provided token is not valid."));
                return BadRequest(ResponseMessageModel.CreateResponseMessage("Token invalid.", "Provided token is not valid."));
            }

            long.TryParse(identityName, out long userID);
            if (Convert.ToInt64(userID) != 0 && (Convert.ToInt64(userID) != objRefreshTokenModel.LoginUserID))
            {
                _logger.LogDebug(string.Format("Provided token is not matched with logged in userID. LoginUserID:{0}", objRefreshTokenModel.LoginUserID, " and UserID generated from token is ", userID));
                return BadRequest(ResponseMessageModel.CreateResponseMessage("Token invalid.", "Provided token is not matched with logged in userID."));
            }

            _logger.LogDebug(string.Format("Generating new access token and refresh token for UserID:{0}", userID));
            // Get refresh token from db and check input refresh token is valid or not
            //var savedRefreshToken = GenerateRefreshToken(username); //retrieve the refresh token from a data store
            //if (savedRefreshToken != refreshToken)
            //    throw new SecurityTokenException("Invalid refresh token");

            var newJwtToken = objTokenHandler.GenerateJWTToken(Convert.ToInt32(userID));
            var newRefreshToken = objTokenHandler.GenerateRefreshToken();

            //Save new refresh token into db
            //DeleteRefreshToken(username, refreshToken);
            //SaveRefreshToken(username, newRefreshToken);

            var res = new
            {
                accessToken = newJwtToken,
                refreshToken = newRefreshToken
            };
            _logger.LogDebug(string.Format("Generated new access token and refresh token for UserID:{0}", userID));

            return Ok(ResponseMessageModel.CreateResponseMessage(res, "Generated new access token and refresh token."));
        }

        //[Authorize(Roles = Role.Admin)]
        [HttpGet, Route("getalluser")]
        public IActionResult GetAll()
        {
            List<UserModel> lstUser = _userService.GetUsers();
            return Ok(ResponseMessageModel.CreateResponseMessage(lstUser, "All User list"));
        }
    }

    public static class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}