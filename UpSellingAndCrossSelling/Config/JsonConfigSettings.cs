using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpSellingAndCrossSelling.Config
{
    public class JsonConfigSettings
    {
        public JsonConfigSettings()
        {

        }

        public string Key { get; set; }
        public string EndPoint { get; set; }
        public string ProductName { get; set; }
        public bool IsActive { get; set; }
        public string ProductDescription { get; set; }
        public string HyperLink { get; set; }
    }
}
