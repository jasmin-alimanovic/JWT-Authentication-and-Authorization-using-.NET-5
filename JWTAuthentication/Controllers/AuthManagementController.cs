using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JWTAuthentication.Configuration;
using JWTAuthentication.Models.DTOs.Requests;
using JWTAuthentication.Models.DTOs.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        private readonly JwtConfig _jwtConfig;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ILogger<SetupController> _logger;


        public AuthManagementController(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, RoleManager<IdentityRole> roleManager, ILogger<SetupController> logger)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _roleManager = roleManager;
            _logger = logger;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>()
                        {
                            "Email already in use"
                        },
                        Success = false
                    });
                }

                var newUser = new IdentityUser()
                {
                    Email = user.Email, UserName = user.Username
                };
                var isCreated = await _userManager.CreateAsync(newUser, user.Password);

                if (isCreated.Succeeded)
                {

                    var resultRoleAddition = await _userManager.AddToRoleAsync(newUser, "AppUser");

                    var jwtToken = await GenerateJwtToken(newUser);

                    return Ok(new RegistrationResponse()
                    {
                        Success = true,
                        Token = jwtToken
                    });
                }
                else
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = isCreated.Errors.Select(x=>x.Description).ToList(),
                        Success = false
                    });
                }
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid payload"
                },
                Success = false
            });
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid login request"
                        },
                        Success = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                if (!isCorrect)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid login request"
                        },
                        Success = false
                    });
                }

                var jwtToken = await GenerateJwtToken(existingUser);
                return Ok(new RegistrationResponse()
                {
                    Success = true,
                    Token = jwtToken
                });
            }
            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid payload"
                },
                Success = false
            });
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var JwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var claims = await GetAllValidClaims(user);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = JwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = JwtTokenHandler.WriteToken(token);

            return jwtToken;
        }



        //get all valid claims for user
        private async Task<List<Claim>> GetAllValidClaims(IdentityUser user)
        {
            var _options = new IdentityOptions();

            var claims = new List<Claim>()
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //get claims from user
            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            //get the user role and add it to the claims
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                

                var role = await _roleManager.FindByNameAsync(userRole);

                if (role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }

            return claims;
        }
    }
}
