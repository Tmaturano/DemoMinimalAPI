using System.ComponentModel.DataAnnotations;

namespace DemoMinimalAPI.DTOs
{
    public class SupplierInputDto
    {
        [Required(ErrorMessage = $"{nameof(Name)} is required"), MaxLength(200)]
        public string? Name { get; set; }

        [Required(ErrorMessage = $"{nameof(Document)} is required"), MaxLength(14)]
        public string? Document { get; set; }
        public bool Active { get; set; }
    }
}
