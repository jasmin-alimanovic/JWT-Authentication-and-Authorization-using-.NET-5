using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JWTAuthentication.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JWTAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsSetupController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ILogger<ClaimsSetupController> _logger;


        public ClaimsSetupController(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<ClaimsSetupController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllClaims(string email)
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

            var userClaims = await _userManager.GetClaimsAsync(user);
            return Ok(userClaims);
        }


        [HttpPost]
        [Route("AddClaimsToUser")]
        public async Task<IActionResult> AddClaimsToUser(string email, string claimName, string claimValue)
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

            var userClaim = new Claim(claimName, claimValue);

            var result = await _userManager.AddClaimAsync(user, userClaim);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    result = $"User {user.Email} has a claim {claimName} added to them"
                });
            }

            return BadRequest(new
            {
                error = $"Failed to add claim {claimName} to the user {user.Email}"
            });

        }


    }
}
