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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
