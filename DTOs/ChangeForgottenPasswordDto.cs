using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ContactBook.DTOs
{
    public class ChangeForgottenPasswordDto
    {
        [Required(ErrorMessage = "Code is required!")]
        public long Code { get; set; }
        
        [Required(ErrorMessage = "NewPassword is required!")]
        public string NewPassword { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; }
    }
}