using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class WatchedJokes
    {
        public WatchedJokes()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int JokesID { get; set; }
        [ForeignKey("JokesID")]
        public virtual JokesList JokeList { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual AdaptLeads AdaptLeads { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
