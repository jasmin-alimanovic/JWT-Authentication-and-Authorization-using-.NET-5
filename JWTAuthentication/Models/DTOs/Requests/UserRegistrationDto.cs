using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace JWTAuthentication.Models.DTOs.Requests
{
    public class UserRegistrationDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email
        {
            get; set;

        }
        public string Password { get; set; }
    }
}
