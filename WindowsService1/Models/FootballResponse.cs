using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1.Models
{
    public class League
    {
        public int league_id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public int season { get; set; }
        public DateTime? season_start { get; set; }
        public DateTime? season_end { get; set; }
        public string logo { get; set; }
        public string flag { get; set; }
        public bool standings { get; set; }
        public bool is_current { get; set; }
    }

    public class Leagues
    {
        public int results { get; set; }
        public List<League> leagues { get; set; }
    }

    public class _api
    {
        public Leagues api { get; set; }
    }
}
