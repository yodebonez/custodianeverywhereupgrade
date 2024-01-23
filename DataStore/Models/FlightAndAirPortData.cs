using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class FlightAndAirPortData
    {
        public FlightAndAirPortData()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(10)]
        public string AirportCode { get; set; }
        [MaxLength(400)]
        public string AirportName { get; set; }
        [MaxLength(400)]
        public string CityCountry { get; set; }
        [MaxLength(400)]
        public string City { get; set; }
        [MaxLength(400)]
        public string Country { get; set; }
    }
}
