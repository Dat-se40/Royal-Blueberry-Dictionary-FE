using Microsoft.EntityFrameworkCore;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;

namespace Royal_Blueberry_Dictionary.Database
{
    // Auto config database
    public class AppDbContext : DbContext
    {
        public DbSet<WordEntry> WordEntries { get; set; }   
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CachedWord> CachedWords { get; set; }
        public DbSet<WordTagRelation> WordTagRelations { get; set; }
        public DbSet<Package> packages { get; set; }
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
                        .Property(e => e.TagIdsJson)
                         .HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
            v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<string>()
        );

            // Tag
            modelBuilder.Entity<Tag>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.UserId, t.Name })
                .IsUnique();

            // CachedWord
            modelBuilder.Entity<CachedWord>()
                .HasKey(c => c.Word);

            // Package 
            modelBuilder.Entity<Package>()
                .HasKey(p => p.Id);
            // WordTagRelation
            modelBuilder.Entity<WordTagRelation>()
                .HasKey(wt => wt.Id);
        }
    }
}
