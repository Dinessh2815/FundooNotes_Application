using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseLayer.Entities
{
    public class Collaborator
    {
        [Key]
        public int CollaboratorId { get; set; }

        [ForeignKey("Note")]
        public int NoteId { get; set; }
        public Note Note { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public bool CanEdit { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
