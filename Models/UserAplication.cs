using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ContactBook.Models
{
    public class UserAplication : IdentityUser
    {
        public string Name { get; set;}
        public IList<Contact> Contacts { get; } = new List<Contact>();
    }
}