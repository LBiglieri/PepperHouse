using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PepperHouse.Models
{
    public class Coupon
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string CouponType { get; set; }
        public enum ECouponType { Percent=0, Dollar=1 }

        [Required]
        public double Discount { get; set; }

        [Required]
        public double MinumumAmount { get; set; }

        public byte[] Picture { get; set; }

        public bool IsActive { get; set; }
    }
}
