﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MeleeMeDatabase
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MeleeMeEntities : DbContext
    {
        public MeleeMeEntities()
            : base("name=MeleeMeEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<m_Credentials> m_Credentials { get; set; }
        public DbSet<m_User> m_User { get; set; }
        public DbSet<m_Melee> m_Melee { get; set; }
        public DbSet<m_MeleeStats> m_MeleeStats { get; set; }
        public DbSet<m_Connections> m_Connections { get; set; }
        public DbSet<m_UserConnections> m_UserConnections { get; set; }
    }
}
