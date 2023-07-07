using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ContactBook.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Telephone { get; set; }
        
        [JsonIgnore]
        public string UserAplicationId { get; set; }
        [JsonIgnore]
        public UserAplication User { get; set; }
    }
}