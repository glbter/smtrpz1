﻿using System.Data.Common;
using System.Data.Entity;
using TennisClub.Data.model;

namespace TennisClub.Data.context
{
    public class PostgresDbContext : DbContext
    {
        public DbSet<ChildInDb> ChildDbSet { get; set; }
        public DbSet<GroupInDb> GroupDbSet { get; set; }

        public PostgresDbContext(DbConnection connection) : base(connection, true)
        {; 
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //----------------child-------------------
            modelBuilder.Entity<ChildInDb>()
                .ToTable("Child")
                .HasKey(t => t.Id);
            
            // modelBuilder.Entity<ChildInDb>()    
            //     .Property(it => it.PreferableDay)
            //     .HasConversion<string>();
            //     
            // modelBuilder.Entity<ChildInDb>()    
            //     .Property(it => it.GameLevel)
            //     .HasConversion<string>();
            
            //---------------group--------------------------------
            modelBuilder.Entity<GroupInDb>()
                .ToTable("Group")
                .HasKey(t => t.Id);
            
            // modelBuilder.Entity<GroupInDb>()    
            //     .Property(it => it.LessonsDay)
            //     .HasConversion<string>();
            //
            // modelBuilder.Entity<GroupInDb>()
            //     .Property(it => it.GameLevel)
            //     .HasConversion<string>();
        }
    }
}