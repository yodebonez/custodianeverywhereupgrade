using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class NonLifeClaimsDocument
    {
        public NonLifeClaimsDocument()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(30)]
        public string extension { get; set; }
        [MaxLength(40)]
        public string name { get; set; }
        [MaxLength(300)]
        public string path { get; set; }
    }
}
