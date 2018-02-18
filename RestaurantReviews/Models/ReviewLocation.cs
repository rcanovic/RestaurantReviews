using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoftWriters.Models
{
    public class ReviewLocation : BaseModel
    {
        public int ReviewId { get; set; }
        public int AddressId { get; set; }
    }
}