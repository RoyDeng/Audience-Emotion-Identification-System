using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AEIS.Controllers
{
    public class CartController : Controller
    {
        public ActionResult Index()
        {
            HashSet<Models.CartItemModel> cart = Session["Cart"] as HashSet<Models.CartItemModel>;
            double total = 0;
            if (cart != null) foreach (var c in cart) total += Convert.ToDouble(c.SubTotal);
            int shipping = 0;
            if (total != 0) if (total < 1000) shipping = 150; else if (total < 2000) shipping = 80;
            ViewData["Shipping"] = shipping;
            ViewData["Amount"] = (int)Math.Round(total, 0);
            ViewData["Total"] = (int)Math.Round(total, 0) + shipping;
            return View(cart);
        }

        public ActionResult WantToBuy(int? ProductID)
        {
            HashSet<Models.CartItemModel> cart;
            if (Session["Cart"] == null) cart = new HashSet<Models.CartItemModel>();
            else cart = Session["Cart"] as HashSet<Models.CartItemModel>;
            Models.CartItemModel item = new Models.CartItemModel();
            item.ProductID = ProductID.Value;
            if (!cart.Contains(item))
            {
                Dictionary<string, object> result = GetCartProductInfo(ProductID.Value);

                item.Name = result["name"].ToString();
                item.Price = Convert.ToDecimal(result["price"]);
                item.Quantity = 1;
                item.Stock = Convert.ToInt32(result["quantity"]);

                cart.Add(item);
                Session["Cart"] = cart;
            }
            double total = 0;
            foreach (var c in cart) total += Convert.ToDouble(c.SubTotal);
            return RedirectToAction("Index", "Cart");
        }

        public string AddToCart(int? ProductID)
        {
            HashSet<Models.CartItemModel> cart;
            if (Session["Cart"] == null) cart = new HashSet<Models.CartItemModel>();
            else cart = Session["Cart"] as HashSet<Models.CartItemModel>;
            Models.CartItemModel item = new Models.CartItemModel();
            item.ProductID = ProductID.Value;
            if (!cart.Contains(item))
            {
                Dictionary<string, object> result = GetCartProductInfo(ProductID.Value);

                item.Name = result["name"].ToString();
                item.Price = Convert.ToDecimal(result["price"]);
                item.Quantity = 1;
                item.Stock = Convert.ToInt32(result["quantity"]);

                cart.Add(item);
                Session["Cart"] = cart;
            }
            double total = 0;
            foreach (var c in cart) total += Convert.ToDouble(c.SubTotal);
            string msg = "已加入購物車!";
            return msg;
        }

        public ActionResult RemoveCartItem(int ProductID)
        {
            return RedirectToAction("Cart", "Cart");
        }

        public ActionResult UpdateCart()
        {
            HashSet<Models.CartItemModel> cart = Session["Cart"] as HashSet<Models.CartItemModel>;
            foreach (var ci in cart)
            {
                int num = int.Parse(Request.Form[string.Format("q({0})", ci.ProductID)]);
                ci.Quantity = num;
            }
            Session["Cart"] = cart;
            return RedirectToAction("Index", "Cart");
        }

        public ActionResult CleanCart()
        {
            Session["Cart"] = null;
            Session.Remove("Cart");
            return RedirectToAction("Index", "Cart");
        }

        public Dictionary<string, object> GetCartProductInfo(int ProductID)
        {
            Dictionary<string, object> rs = null;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT * FROM product WHERE product_id=@product_id");

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@product_id", MySqlDbType.Int32).Value = ProductID;
                    con.Open();
                    MySqlDataReader sdr = cmd.ExecuteReader();

                    if (sdr.HasRows)
                    {
                        rs = new Dictionary<string, object>();
                        sdr.Read();
                        for (int i = 0; i < sdr.FieldCount; i++)
                            rs.Add(sdr.GetName(i), sdr[i]);
                        sdr.Close();
                    }
                    cmd.Dispose();
                    con.Close();
                }
            }
            return rs;
        }
    }
}