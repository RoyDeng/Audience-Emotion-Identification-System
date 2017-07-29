using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AEIS.Models
{
    public class CartItemModel
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }

        private decimal price;
        private int quantity;
        private decimal subTotal;

        public decimal Price
        {
            get { return this.price; }
            set
            {
                this.price = value;
                this.subTotal = this.price * this.quantity;
            }
        }

        public int Quantity
        {
            get { return this.quantity; }
            set
            {
                this.quantity = value;
                this.subTotal = this.price * this.quantity;
            }
        }

        public decimal SubTotal
        {
            get { return this.subTotal; }
        }

        public override int GetHashCode()
        {
            return this.ProductID;
        }

        public override bool Equals(object obj)
        {
            bool ans = false;
            if (obj is CartItemModel)
            {
                CartItemModel item = obj as CartItemModel;
                if (item.ProductID == this.ProductID)
                    ans = true;
            }

            return ans;
        }
    }
}