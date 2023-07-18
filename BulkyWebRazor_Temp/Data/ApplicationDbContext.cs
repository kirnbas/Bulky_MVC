﻿using BulkyWebRazor_Temp.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyWebRazor_Temp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fantasy", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Manga", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Novella", DisplayOrder = 3 }
                );
        }
    }
}
