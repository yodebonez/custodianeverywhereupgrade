using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class PinnedNews
    {
        public PinnedNews()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string JsonNewsObject { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual AdaptLeads AdaptLeads { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
