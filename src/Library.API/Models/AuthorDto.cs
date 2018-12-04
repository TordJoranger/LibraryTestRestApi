using System;
using Library.API.Models;

namespace Library.API
    {
    public class AuthorDto :LinkedResourceBaseDto
        {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public int Age {get; set;}

        public string Genre { get; set; }

        }
    }
