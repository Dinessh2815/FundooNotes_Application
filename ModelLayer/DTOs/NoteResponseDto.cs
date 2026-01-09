using System;
using System.Collections.Generic;
using System.Text;

namespace ModelLayer.DTOs
{
    public class NoteResponseDto
    {
        public int NoteId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }

        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
