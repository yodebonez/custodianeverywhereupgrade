using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpSellingAndCrossSelling.Config
{
    public class RequestModel
    {
        public RequestModel()
        {

        }
        public string Gender { get; set; }
        public DateTime Age { get; set; }
        public string Occupation { get; set; }
        public string Premium { get; set; }
    }


    public class DbModels
    {
        public DbModels()
        {

        }

        public string Occupation { get; set; }
        public DateTime Date_of_Birth { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string currentProds { get; set; }
        public decimal Premium { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class RecommendationList
    {
        public RecommendationList()
        {

        }

        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> ProductList { get; set; }
    }

    public class DateMode {
        public DateMode()
        {

        }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
