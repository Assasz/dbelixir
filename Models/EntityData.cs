using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace XMLParser.Models
{
    public class EntityData
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Entity")]
        public int EntityID { get; set; }

        public int PropertyID { get; set; }

        public string Data { get; set; }

        public virtual Entity Entity { get; set; }
    }
}