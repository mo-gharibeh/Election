﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace fstCopy_Proj5.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ElectionEntities : DbContext
    {
        public ElectionEntities()
            : base("name=ElectionEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Ad> Ads { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<DATE> DATES { get; set; }
        public virtual DbSet<Debate> Debates { get; set; }
        public virtual DbSet<GeneralListCandidate> GeneralListCandidates { get; set; }
        public virtual DbSet<GeneralListing> GeneralListings { get; set; }
        public virtual DbSet<LocalList> LocalLists { get; set; }
        public virtual DbSet<LocalListCandidate> LocalListCandidates { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
