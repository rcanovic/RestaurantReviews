using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoftWriters.Models
{
    public class BaseModel
    {
        public int Id { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string ModifiedByUserId { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public bool IsDeleted { get; set; } = false;

        public BaseModel()
        {
            CreatedByUserId = "API";
            CreatedDateTime = DateTime.Now;
            IsDeleted = false;
        }
    }
}