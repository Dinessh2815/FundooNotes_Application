using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseLayer.Entities
{
    public class NoteHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [ForeignKey("Note")]
        public int NoteId { get; set; }
        public Note Note { get; set; }

        public string Action { get; set; }   // Created / Updated / Deleted

        public DateTime ActionAt { get; set; } = DateTime.UtcNow;
    }
}
