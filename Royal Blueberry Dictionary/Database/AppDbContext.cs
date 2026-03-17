using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Royal_Blueberry_Dictionary.Model;
using System.Windows.Automation.Peers;

namespace Royal_Blueberry_Dictionary.Database
{
    // Auto config database
    public class AppDbContext : DbContext
    {
        public DbSet<WordEntry> WordEntries { get; set; }   
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CachedWord> CachedWords { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
         
         
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
         
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tạo khóa chính cho WordEntry 
            modelBuilder.Entity<WordEntry>().HasKey(w => w.Id);
            // Tạo các thuộc tính
            modelBuilder.Entity<WordEntry>()
                .HasIndex(w => new { w.UserId, w.Word , w.MeaningIndex})
                .IsUnique();

            // Tag
            modelBuilder.Entity<Tag>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.UserId, t.Name })
                .IsUnique();

            // CachedWord
            modelBuilder.Entity<CachedWord>()
                .HasKey(c => c.Word);
        }   
    }
}
