using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class Quote : CoreModels
    {
        public Quote()
        {

        }

        [Required]
        public TravelCategory Region { get; set; }
        [Required]
        public DateTime DepartureDate { get; set; }
        [Required]
        public DateTime ReturnDate { get; set; }
        [Required]
        public List<DateTime> DateOfBirth { get; set; }
        public double? LoadingRate { get; set; }
        public bool IsFlatLoading { get; set; } = false;
    }

    public class RateCategory
    {
        public string type { get; set; }
        public double included_rate { get; set; }
        public double? excluded_rate { get; set; }
    }

    public class TravelRate
    {
        public string _class { get; set; }
        public string days { get; set; }
        public List<RateCategory> category { get; set; }
    }

    public class Package
    {
        public string type { get; set; }
        public List<string> values { get; set; }
        public string name { get; set; }
    }

    public class _Category
    {
        public int region { get; set; }
        public List<Package> package { get; set; }
    }

    public class PackageList
    {
        public List<string> benefits { get; set; }
        public List<_Category> category { get; set; }
    }

    public class plans
    {
        public double premium { get; set; }
        public double loadedpremium { get; set; }
        public double exchangeRate { get; set; }
        public Package package { get; set; }
        public int travellers { get; set; }
        public List<_breakDown> breakDown { get; set; }

    }

    public class _breakDown
    {
        public int Id { get; set; }
        public double premium { get; set; }
        public DateTime dateOfBirth { get; set; }
        public double loadedpremium { get; set; }
    }

    public class TravelBroker
    {
        public long Id { get; set; }
        [MaxLength(200)]
        public string Fullname { get; set; }
        public string Surname { get; set; }
        public string Othername { get; set; }
        public DateTime DateOfBirth { get; set; }
        [MaxLength(6)]
        public string Gender { get; set; }
        [MaxLength(300)]
        public string Nationality { get; set; }
        [MaxLength(300)]
        public string CountryOfOrigin { get; set; }
        [MaxLength(50)]
        public string PassPortNumber { get; set; }
        [MaxLength(300)]
        public string Occupation { get; set; }
        [MaxLength(15)]
        public string MobileNumber { get; set; }
        [MaxLength(300)]
        public string EmailAddress { get; set; }
        public byte[] PassportImage { get; set; }
        [MaxLength(50)]
        public string GeographicalZone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [MaxLength(400)]
        public string Destination { get; set; }
        [MaxLength(40)]
        public string PurposeOfTrip { get; set; }
        public decimal TotalCost { get; set; }
        public long CompanyProfileID { get; set; }
        public string UserID { get; set; }
        public decimal Premium { get; set; }
        public decimal SwitchFee { get; set; }
        [MaxLength(300)]
        public string HomeAddress { get; set; }
        public decimal Commision { get; set; }
        [MaxLength(40)]
        public string PolicyNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public long BranchID { get; set; }
        public double CommisionRate { get; set; }
        [MaxLength(20)]
        public string CertificateNo { get; set; }
        public long? BusinessTypeID { get; set; }
        [MaxLength(30)]
        public string Extension { get; set; }
        public string Source { get; set; }
        [MaxLength(400)]
        public string Others { get; set; }
        [MaxLength(400)]
        public string ImagePath { get; set; }
        public bool IsGroup { get; set; }
        public bool IsGroupLeader { get; set; }
        public string GroupReference { get; set; }
        public int GroupCount { get; set; }

        public string TransactionRef { get; set; }
    }

    public class ReferralModel
    {
        public ReferralModel()
        {

        }
        public int AgntRefID { get; set; }
        public string Agnt_Name { get; set; }
        public string Agnt_Status { get; set; }
        public string Agnt_Num { get; set; }
        public string Data_Source { get; set; }
    }
}
