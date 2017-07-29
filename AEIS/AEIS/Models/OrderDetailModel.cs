using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AEIS.Models
{
    public class OrderDetailModel
    {
        public int OrderDetailID { get; set; }
        public string OrderID { get; set; }
        public int ProdcutID { get; set; }
        public Decimal UnitePrice { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
    }
}
