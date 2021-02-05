using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PepperHouse.Models
{
    public class OrderDetails
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int OrderID { get; set; }
        [ForeignKey("OrderID")]
        public virtual OrderHeader OrderHeader { get; set; }

        [Required]
        public int MenuItemID { get; set; }
        [ForeignKey("MenuItemID")]
        public virtual MenuItem MenuItem { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }
    }
}
