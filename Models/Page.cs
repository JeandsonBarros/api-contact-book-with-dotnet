using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactBook.Models
{
    public class PageResponse<T>
    {
        public PageResponse(T data, int page, int size, int totalRecords, string uri)
        {
            Page = page;
            Size = size;
            TotalRecords = totalRecords;
            Data = data;
            TotalPages = totalRecords / size == 0 ? 1 : totalRecords / size;
            NextPage = page + 1 <= TotalPages ? new Uri($"{uri}?page={page + 1}&size={size}") : null;
            PreviousPage = page - 1 >= 1 ? new Uri($"{uri}?page={page - 1}&size={size}") : null;
            FirstPage = new Uri($"{uri}?page={1}&size={size}");
            LastPage = new Uri($"{uri}?page={TotalPages}&size={size}");
        }
        public PageResponse(T data, int page, int size, int totalRecords)
        {
            Page = page;
            Size = size;
            TotalRecords = totalRecords;
            Data = data;
            TotalPages = totalRecords / size == 0 ? 1 : totalRecords / size;
        }

        public T Data { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public Uri NextPage { get; set; }
        public Uri PreviousPage { get; set; }
        public Uri FirstPage { get; set; }
        public Uri LastPage { get; set; }

    }
}