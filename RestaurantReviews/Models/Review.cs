using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftWriters.Models
{
    public class Review : BaseModel 
    { 
        public int EntityId { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; } 
    }
}
