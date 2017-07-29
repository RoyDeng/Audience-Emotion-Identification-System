using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AEIS.Models
{
    public class ProductModel
    {
        public int ProductID { get; set; }
        public int ShopID { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
        public Decimal Price { get; set; }
        public string DateAdded { get; set; }
    }
}
