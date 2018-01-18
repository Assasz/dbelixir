using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XMLParser.Models
{
    public class Entity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy, hh:mm tt}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime LastUpdateDate { get; set; }

        public int Items { get; set; }

        public string Schema { get; set; }

        public Entity()
        {
            this.Items = 0;
            this.LastUpdateDate = DateTime.Now;
        }
    }
}