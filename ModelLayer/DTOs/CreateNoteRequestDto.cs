using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModelLayer.DTOs
{
    public class CreateNoteRequestDto
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public string Color { get; set; }

        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
    }

}
