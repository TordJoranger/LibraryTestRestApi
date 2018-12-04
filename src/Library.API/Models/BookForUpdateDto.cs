using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
    {
    public class BookForUpdateDto :BookForModificationDto
        {
            [Required]
            public override string Description { get; set; }
        }
    }