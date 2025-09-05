using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace StudyCycleCLI.Model
{
    internal class AppDbContext : DbContext
    {
        public DbSet<StudyCycle> StudyCycles { get; set; }
        public DbSet<StudyCycleSubject> StudyCycleSubjects { get; set; }
        public string DbPath { get; }

        public AppDbContext()
        {
            Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
            string path = Environment.GetFolderPath(folder);
            this.DbPath = System.IO.Path.Join(path, "studycycleclidb.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source={this.DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudyCycle>()
                .HasMany(sc => sc.Subjects)
                .WithOne(scs => scs.StudyCycle)
                .HasForeignKey(scs => scs.StudyCycleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
