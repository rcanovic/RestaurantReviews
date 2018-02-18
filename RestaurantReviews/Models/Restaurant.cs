using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftWriters.Models
{
    public class Restaurant : BaseModel
    {
        public string Name { get; set; }
        public List<Address> Addresses { get; set; }
        public List<Review> Reviews { get; set; }
       
    }
}
