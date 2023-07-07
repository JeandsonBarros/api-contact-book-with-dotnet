using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactBook.Models
{
    public class Pagination
    {

        public Pagination()
        {
            Page = 1;
            Size = 10;
        }
        public Pagination(int page, int size)
        {
            Page = page < 1 ? 1 : page;
            Size = size < 1 ? 10 : size;
        }

        public int Page { get; set; }
        public int Size { get; set; }

    }
}