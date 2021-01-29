using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PepperHouse.Models.ViewModels
{
    public class MenuViewModel
    {
        public IEnumerable<MenuItem> MenuItem { get; set; }
        public IEnumerable<Category> Category { get; set; }
        public IEnumerable<Coupon> Coupon { get; set; }

    }
}
