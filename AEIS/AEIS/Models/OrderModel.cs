using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AEIS.Models
{
    public class OrderModel
    {
        public string OrderID { get; set; }
        public int UserID { get; set; }
        public Decimal Total { get; set; }
        public string DateAdded { get; set; }
    }
}
