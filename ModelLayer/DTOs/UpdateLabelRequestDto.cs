using System.ComponentModel.DataAnnotations;

namespace ModelLayer.DTOs
{
    public class UpdateLabelRequestDto
    {
        [Required]
        public string Name { get; set; }
    }
}