using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwtFirstApp.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JwtFirstApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        private string encodeToken;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Login(string username, string pass)
        {
            UserModel login = new UserModel();
            login.UserName = username;
            login.Pass = pass;
            IActionResult response = Unauthorized();

            var user = AuthenticateUser(login);
            if (user != null)
            {
                var tokenSrt = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenSrt });
            }

            return response;
        }

        private UserModel AuthenticateUser(UserModel login)
        {
            UserModel user = null;
            //For demo
            if (login.UserName == "gmero" && login.Pass == "123")
            {
                user = new UserModel
                {
                    UserName = "geovanny",
                    Pass = "123",
                    UserEmail = "gmero@gmail.com"
                };
            }

            return user;
        }

        private string GenerateJSONWebToken(UserModel userinfo)
        {

            
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var Claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userinfo.UserName),
                    new Claim(JwtRegisteredClaimNames.Email, userinfo.UserEmail),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Issuer"],
                    Claims,
                    expires: DateTime.Now.AddMinutes(120),
                    signingCredentials: credentials);

                encodeToken = new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return encodeToken;


        }

        [Authorize]
        [HttpPost("Post")]
        public string Post()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userName = claim[0].Value;
            return "Welcome" + userName;
        }

        [HttpGet("GetValue")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }

}
