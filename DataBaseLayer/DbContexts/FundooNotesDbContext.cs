using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DataBaseLayer.Entities;
using System.Reflection.Metadata;

namespace DataBaseLayer.DbContexts
{
    public class FundooNotesDbContext : DbContext
    {
        public FundooNotesDbContext(DbContextOptions<FundooNotesDbContext> options) : base(options) { }
    
        public DbSet<User> Users {  get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<NoteLabel> NoteLabels { get; set; }
        public DbSet<Collaborator> Collaborators { get; set; }
        public DbSet<NoteHistory> NoteHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Label>()
                .HasIndex(l => new { l.UserId, l.Name })
                .IsUnique();

            modelBuilder.Entity<NoteLabel>().HasKey(n1 => new { n1.NoteId, n1.LabelId });

            modelBuilder.Entity<Collaborator>()
                .HasIndex(c => new { c.NoteId, c.UserId })
                .IsUnique();

            modelBuilder.Entity<Collaborator>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Collaborator>()
                .HasOne(c => c.Note)
                .WithMany(n => n.Collaborators)
                .HasForeignKey(c => c.NoteId)
                .OnDelete(DeleteBehavior.Cascade);



            // 🔹 NoteHistory
            modelBuilder.Entity<NoteHistory>()
                .HasOne(nh => nh.Note)
                .WithMany()
                .HasForeignKey(nh => nh.NoteId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
