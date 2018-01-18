using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace XMLParser.Models
{
    public class EntityContext : DbContext
    {
        public EntityContext()
        {
            Database.Connection.ConnectionString = @"server=ASSASZ\ARSSERVER;database=entities; trusted_connection=true";
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public DbSet<Entity> Entity { get; set; }
        public DbSet<EntityData> EntityData { get; set; }
    }
}