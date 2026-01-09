using System.ComponentModel.DataAnnotations;

namespace ModelLayer.DTOs
{
    public class CreateLabelRequestDto
    {
        [Required]
        public string Name { get; set; }
    }
}
