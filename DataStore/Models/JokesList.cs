using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class JokesList
    {
        public JokesList()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(300)]
        public string youtube_link { get; set; }
        [MaxLength(100)]
        public string credit { get; set; }
        public DateTime? created { get; set; }
        [MaxLength(100)]
        public string uploadedby { get; set; }
        [MaxLength(300)]
        public string title { get; set; }
    }
}
