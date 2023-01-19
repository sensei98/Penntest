using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using VRefSolutions.Domain.Models;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VRefSolutions.DAL
{
    public class VRefSolutionsContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<TrainingState> TrainingStates { get; set; }
        public DbSet<Altitude> Altitudes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<EcamMessage> EcamMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Auto generated Id values on creation
            modelBuilder.Entity<Organization>().Property(o => o.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Training>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Altitude>().Property(a => a.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Event>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EcamMessage>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EventType>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<TrainingState>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<TrainingState>().Property(e => e.EcamMessages).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));


            modelBuilder.Entity<Training>()
                .HasMany(t => t.Participants)
                .WithMany(u => u.Trainings);

            var splitStringConverter = new ValueConverter<List<string>, string>(v => string.Join(";", v), v => v.Split(new[] { ';' }).ToList());
            modelBuilder.Entity<Training>()
                .Property(t => t.Videos)
                .HasConversion(splitStringConverter);
            // modelBuilder.Entity<Training>()
            //     .Property(t => t.VideoAccesURLs)
            //     .HasConversion(splitStringConverter);
            modelBuilder.Entity<Training>()
                .Property(t => t.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (Status)Enum.Parse(typeof(Status), v)
                );
            modelBuilder.Entity<User>()
                .Property(u => u.UserType)
                .HasConversion(
                    v => v.ToString(),
                    v => (Role)Enum.Parse(typeof(Role), v)
                );
            // Convert timestamp to string in database, and parse back to TimeStamp when filling object
            var timeStampConverter = new ValueConverter<TimeStamp, string>(v => v.ToString(), v => TimeStamp.Parse(v, ':'));
            modelBuilder.Entity<Event>()
                .Property(e => e.TimeStamp)
                .HasConversion(timeStampConverter);
            modelBuilder.Entity<Altitude>()
                .Property(a => a.TimeStamp)
                .HasConversion(timeStampConverter);

        }

        public VRefSolutionsContext(DbContextOptions<VRefSolutionsContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}