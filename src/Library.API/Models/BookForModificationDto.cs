using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public abstract class BookForModificationDto

    {
        [Required(ErrorMessage = "You should fill out a title")]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)] public virtual string Description { get; set; }
    }
}