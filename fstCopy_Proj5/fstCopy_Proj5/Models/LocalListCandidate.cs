//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class LocalListCandidate
    {
        public int CandidateID { get; set; }
        public string CandidateName { get; set; }
        public string NationalNumber { get; set; }
        public string Gender { get; set; }
        public string Religion { get; set; }
        public string Type_Chair { get; set; }
        public string BirthdateCandidate { get; set; }
        public string Governorate { get; set; }
        public string ElectionArea { get; set; }
        public Nullable<int> NumberOfVotesCandidate { get; set; }
        public Nullable<int> LocalListingID { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
    
        public virtual LocalList LocalList { get; set; }
    }
}
