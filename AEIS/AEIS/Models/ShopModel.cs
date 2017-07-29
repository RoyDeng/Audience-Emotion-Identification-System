using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AEIS.Models
{
    public class ShopModel
    {
        public int? ShopID { get; set; }
        public int? UserID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Username { get; set; }
    }
}
