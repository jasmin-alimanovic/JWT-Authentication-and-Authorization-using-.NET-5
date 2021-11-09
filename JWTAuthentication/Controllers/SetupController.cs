using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JWTAuthentication.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JWTAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {

        private readonly AppDbContext _context;
        
        private readonly UserManager<IdentityUser> _userManager;
        
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ILogger<SetupController> _logger;


        public SetupController(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<SetupController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        [HttpGet]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string name)
        {
            //check if role exists
            var roleExists = await _roleManager.RoleExistsAsync(name);

            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(name));

                if (roleResult.Succeeded)
                {
                    _logger.LogInformation($"The Role {name} has been added successfully");
                    return Ok(new
                    {
                        result=$"The role {name} has been added successfully"
                    });
                }
                else
                {
                    _logger.LogInformation($"The Role {name} has not been added.");
                    return BadRequest(new
                    {
                        result = $"The role {name} has not been added."
                    });
                }

                
            }

            return BadRequest(new {error = "Role already exists"});
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);

        }

        [HttpPost]
        [Route("AddUserToRole")]
        public async Task<IActionResult> AddUserToRole(string email, string roleName)
        {
            //check if user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"The user with {email} does not exists.");
                return BadRequest(new
                {
                    result = $"The user with {email} does not exists."
                });
            }


            //check if role exists
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                return BadRequest(new
                {
                    result = $"The role {roleName} does not exists."
                });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            //check if usere is assigned to role

            if (result.Succeeded)
            {
                return Ok(new
                {
                    result = $"{user} has been added to {roleName}"
                });
            }
            return BadRequest(new
            {
                result = $"The user {email} has not been added to {roleName}."
            });

        }


        [HttpGet]
        [Route("GetAllUserRoles")]
        public async Task<IActionResult> GetAllUserRoles(string email)
        {
            //check if user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"The user with {email} does not exists.");
                return BadRequest(new
                {
                    result = $"The user with {email} does not exists."
                });
            }


            //return roles
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        [HttpPost]
        [Route("RemoveUserFromRole")]
        public async Task<IActionResult> RemoveUserFromRole(string email, string roleName)
        {
            //user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"The user with {email} does not exists.");
                return BadRequest(new
                {
                    result = $"The user with {email} does not exists."
                });
            }

            //role exists
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                return BadRequest(new
                {
                    result = $"The role {roleName} does not exists."
                });
            }


            //remove user from role

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    result = $"User {email} has been removed from role {roleName}"
                });
            }
            else
            {
                return BadRequest(new
                {
                    result = $"Failed to remove user {email} from role {roleName}."
                });
            }
        }

    }
}
