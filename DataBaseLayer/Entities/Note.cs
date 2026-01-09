using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseLayer.Entities
{
    public class Note
    {
        [Key]
        public int NoteId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }

        public string Color { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Collaborator> Collaborators { get; set; } = new List<Collaborator>();


    }
}