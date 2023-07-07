using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ContactBook.DTOs
{
    public class UserDto
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; }
    }

    [ValidateNever]
    public class UserDtoViewModel
    {

        public string Name { get; set; }
        
        public string Email { get; set; }

        public string Password { get; set; }
    }
}