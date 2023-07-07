using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ContactBook.DTOs
{
    public class ContactDto
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "IsActive is required!")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Telephone is required!")]
        public string Telephone { get; set; }
    }

    [ValidateNever]
    public class ContactDtoViewModel
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Telephone { get; set; }
    }
}