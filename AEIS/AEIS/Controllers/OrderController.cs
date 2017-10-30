using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AEIS.Controllers
{
    public class OrderController : Controller
    {
        public ActionResult CheckOut()
        {
            if (Session["username"] != null)
            {
                HashSet<Models.CartItemModel> cart = Session["Cart"] as HashSet<Models.CartItemModel>;
                double total = 0;
                if (cart != null) foreach (var c in cart) total += Convert.ToDouble(c.SubTotal);
                int shipping = 0;
                if (total != 0) if (total < 1000) shipping = 150; else if (total < 2000) shipping = 80;
                ViewData["Shipping"] = shipping;
                ViewData["Amount"] = (int)Math.Round(total, 0);
                ViewData["Total"] = (int)Math.Round(total, 0) + shipping;
                Models.UserModel user = null;
                Dictionary<string, object> rs = null;
                string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = string.Format("SELECT firstname, lastname, email FROM user WHERE username=@username");
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Session["username"].ToString().ToLower();
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
                        user = new Models.UserModel();
                        user.FirstName = rs["firstname"].ToString();
                        user.LastName = rs["lastname"].ToString();
                        user.Email = rs["email"].ToString();
                    }
                }
                return View(Tuple.Create(cart, user));
            }
            else return Redirect("~/login");
        }

        public ActionResult CheckOutProcess()
        {
            HashSet<Models.CartItemModel> cart = Session["Cart"] as HashSet<Models.CartItemModel>;
            double total = 0;
            if (cart != null) foreach (var c in cart) total += Convert.ToDouble(c.SubTotal);
            if (total != 0) if (total < 1000) total += 150; else if (total < 2000) total += 80;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            string OrderID = PublishOrderID();
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("INSERT INTO `order`(order_id, user_id, total, date_added) VALUES(@order_id, @user_id, @total, @date_added)");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@order_id", MySqlDbType.VarChar).Value = OrderID;
                    cmd.Parameters.Add("@user_id", MySqlDbType.Int32).Value = GetUserID();
                    cmd.Parameters.Add("@total", MySqlDbType.Decimal).Value = total;
                    cmd.Parameters.Add("@date_added", MySqlDbType.DateTime).Value = DateTime.Now;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    con.Close();
                }
            }
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("INSERT INTO order_detail(order_id, product_id, unitprice, quantity) VALUES(@order_id, @product_id, @unitprice, @quantity)");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    foreach (var c in cart)
                    {
                        cmd.Parameters.Add("@order_id", MySqlDbType.VarChar).Value = OrderID;
                        cmd.Parameters.Add("@product_id", MySqlDbType.UInt32).Value = c.ProductID;
                        cmd.Parameters.Add("@unitprice", MySqlDbType.Decimal).Value = c.Price;
                        cmd.Parameters.Add("@quantity", MySqlDbType.Int32).Value = c.Quantity;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                    con.Close();
                }
            }
            return Redirect("~/FinishCheckOut");
        }

        public ActionResult FinishCheckOut()
        {
            Session["Cart"] = null;
            Session.Remove("Cart");
            return View();
        }

        public ActionResult OrderDetail(string OrderID)
        {
            if (Session["username"] != null)
            {
                Models.OrderModel order = null;
                Models.UserModel user = null;
                List<Models.OrderDetailModel> od = new List<Models.OrderDetailModel>();
                Dictionary<string, object> rs = null;
                string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = string.Format("SELECT order_id, total, o.date_added, u.firstname, u.lastname, email FROM `order` AS o INNER JOIN user AS u ON o.user_id=u.user_id WHERE order_id=@order_id");
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@order_id", MySqlDbType.VarChar).Value = OrderID;
                        con.Open();
                        using (MySqlDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.HasRows)
                            {
                                rs = new Dictionary<string, object>();
                                sdr.Read();
                                for (int i = 0; i < sdr.FieldCount; i++)
                                    rs.Add(sdr.GetName(i), sdr[i]);
                                sdr.Close();
                            }
                            order = new Models.OrderModel();
                            order.OrderID = rs["order_id"].ToString();
                            order.Total = Convert.ToDecimal(rs["total"]);
                            order.DateAdded = rs["date_added"].ToString();
                            user = new Models.UserModel();
                            user.FirstName = rs["firstname"].ToString();
                            user.LastName = rs["lastname"].ToString();
                            user.Email = rs["email"].ToString();
                        }
                    }
                }
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = "SELECT od.product_id, unitprice, od.quantity, name FROM order_detail as od INNER JOIN product as p ON od.product_id=p.product_id WHERE order_id=@order_id";
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@order_id", MySqlDbType.VarChar).Value = OrderID;
                        con.Open();
                        using (MySqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                od.Add(new Models.OrderDetailModel
                                {
                                    ProdcutID = Convert.ToInt32(sdr["product_id"]),
                                    UnitePrice = Convert.ToDecimal(sdr["unitprice"]),
                                    Quantity = Convert.ToInt32(sdr["quantity"]),
                                    Name = sdr["name"].ToString()
                                });
                            }
                        }
                        cmd.Dispose();
                        con.Close();
                    }
                }
                return View(Tuple.Create(order, user, od));
            }
            else return Redirect("~/login");
        }

        public int GetUserID()
        {
            Dictionary<string, object> rs = null;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT * FROM user WHERE username=@username");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Session["username"].ToString();
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
            return Convert.ToInt32(rs["user_id"]);
        }

        public static string PublishOrderID()
        {
            Random rnd = new Random();
            string id = "";
            for (int i = 0; i < 14; ++i)
                switch (rnd.Next(0, 3))
                {
                    case 0: id += rnd.Next(0, 10); break;
                    case 1: id += (char)rnd.Next(65, 91); break;
                    case 2: id += (char)rnd.Next(97, 122); break;
                }
            return id;
        }
    }
}