using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactBook.Models
{
    public class CodeForChangeForgottenPassword
    {
        public int Id { get; set; }
        public long Code { get; set; }
        public DateTime CodeExpires { get; } = DateTime.UtcNow.AddMinutes(15);
        public string UserAplicationId { get; set; }
       
    }
}