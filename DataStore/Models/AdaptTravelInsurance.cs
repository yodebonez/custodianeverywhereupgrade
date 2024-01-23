using DataStore.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class AdaptTravelInsurance
    {
        public AdaptTravelInsurance()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Zones zone { get; set; }
        [MaxLength(100)]
        public string nationality { get; set; }
        [MaxLength(100)]
        public string destination { get; set; }
        public DateTime departure_date { get; set; }
        public DateTime return_date { get; set; }
        [MaxLength(20)]
        public string phone_number { get; set; }
        [MaxLength(400)]
        public string address { get; set; }
        [MaxLength(30)]
        public string purpose_of_trip { get; set; }
        public decimal premium { get; set; }
        [MaxLength(50)]
        public string transaction_ref { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        public virtual List<Passenger> passengers { get; set; }
    }


    public class Passenger
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(50)]
        public string surname { get; set; }
        [MaxLength(50)]
        public string firstname { get; set; }
        [MaxLength(10)]
        public string title { get; set; }
        public DateTime date_of_birth { get; set; }
        [MaxLength(8)]
        public string gender { get; set; }
        [MaxLength(20)]
        public string passport_number { get; set; }
        [MaxLength(50)]
        public string occupation { get; set; }
        [MaxLength(8)]
        public string extension { get; set; }
        public decimal premium { get; set; }
        [MaxLength(400)]
        public string image_path { get; set; }
    }

}
