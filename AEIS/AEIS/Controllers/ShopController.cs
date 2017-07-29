using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AEIS.Controllers
{
    public class ShopController : Controller
    {
        public ActionResult Index(int? page, string name)
        {
            if (page == null) page = 1;
            if (name == "") name = null;
            int total = TotalShopCount(name);
            total = (int)Math.Ceiling(total / 12.0);
            StringBuilder sb = new StringBuilder();
            if (name != null)
                for (int i = 1; i <= total; i++)
                {
                    if (total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Shops?page={0}&name={1}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", i - 1, name));
                    if (i == page.Value)
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='Shops?page={0}&name={1}'>{0}</a></li>", i, name));
                    else
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='Shops?page={0}&name={1}'>{0}</a></li>", i, name));
                    if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Shops?page={0}&name={1}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", i + 1, name));
                }
            else
                for (int i = 1; i <= total; i++)
                {
                    if (total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Shops?page={0}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", i - 1));
                    if (i == page.Value)
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='Shops?page={0}'>{0}</a></li>", i));
                    else
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='Shops?page={0}'>{0}</a></li>", i));
                    if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Shops?page={0}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", i + 1));
                }
            ViewData["pages"] = sb.ToString();
            string query = "";
            List<Models.ShopModel> shops = new List<Models.ShopModel>();
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                if (name != null) query = "SELECT shop_id, name, description, username FROM shop as s INNER JOIN user AS u ON s.user_id=u.user_id WHERE s.name LIKE @name LIMIT @start, @number";
                else query = "SELECT shop_id, name, description, username FROM shop as s INNER JOIN user AS u ON s.user_id=u.user_id LIMIT @start, @number";
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    if (name != "") cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = "%" + name + "%";
                    cmd.Parameters.Add("@start", MySqlDbType.Int32).Value = (page - 1) * 12;
                    cmd.Parameters.Add("@number", MySqlDbType.Int32).Value = 12;
                    con.Open();
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            shops.Add(new Models.ShopModel
                            {
                                ShopID = Convert.ToInt32(sdr["shop_id"]),
                                Name = sdr["name"].ToString(),
                                Description = sdr["description"].ToString(),
                                Username = sdr["username"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return View(shops);
        }

        public int TotalShopCount(string name)
        {
            int cnt = 0;
            string query = "";
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                if (name != null) query = "SELECT COUNT(*) FROM shop WHERE name LIKE @name";
                else query = "SELECT COUNT(*) FROM shop";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    con.Open();
                    if (name != null) cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = "%" + name + "%";
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                }

            }
            return cnt;
        }

        public ActionResult ShopDetail(int ShopID, int? page)
        {
            if (page == null) page = 1;
            int total = TotalShopDetailCount();
            total = (int)Math.Ceiling(total / 6.0);
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= total; i++)
            {
                if (total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='ShopDetail?ShopID={0}&page={1}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", ShopID, i - 1));
                if (i == page.Value)
                    sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='ShopDetail?ShopID={0}&page={1}'>{1}</a></li>", ShopID, i));
                else
                    sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='ShopDetail?ShopID={0}&page={1}'>{1}</a></li>", ShopID, i));
                if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='ShopDetail?ShopID={0}&page={1}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", ShopID, i + 1));
            }
            ViewData["pages"] = sb.ToString();
            Dictionary<string, object> rs = null;
            Models.ShopModel shop = new Models.ShopModel();
            Models.UserModel user = new Models.UserModel();
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT name, description, username FROM shop AS s INNER JOIN user AS u ON s.user_id=u.user_id WHERE s.shop_id=@shop_id");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@shop_id", MySqlDbType.Int32).Value = ShopID;
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
                    shop.Name = rs["name"].ToString();
                    shop.Description = rs["description"].ToString();
                    user.Username = rs["username"].ToString();
                }
            }
            List<Models.ProductModel> products = new List<Models.ProductModel>();
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT * FROM product WHERE shop_id=@shop_id LIMIT @start, @number";
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@shop_id", MySqlDbType.Int32).Value = ShopID;
                    cmd.Parameters.Add("@start", MySqlDbType.Int32).Value = (page - 1) * 6;
                    cmd.Parameters.Add("@number", MySqlDbType.Int32).Value = 6;
                    con.Open();
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            products.Add(new Models.ProductModel
                            {
                                ProductID = Convert.ToInt32(sdr["product_id"]),
                                Name = sdr["name"].ToString(),
                                Description = sdr["description"].ToString(),
                                Price = Convert.ToDecimal(sdr["price"])
                            });
                        }
                    }
                    con.Close();
                }
            }

            return View(Tuple.Create(shop, user, products));
        }

        public int TotalShopDetailCount()
        {
            int cnt = 0;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM shop", con))
                {
                    con.Open();
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                }

            }
            return cnt;
        }

        public ActionResult SearchShop(string Name)
        {
            List<Models.ShopModel> shops = new List<Models.ShopModel>();
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT * FROM shop WHERE name LIKE ('%'+@name+'%')";
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = Name;
                    con.Open();
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            shops.Add(new Models.ShopModel
                            {
                                Name = sdr["name"].ToString(),
                                Description = sdr["description"].ToString(),
                            });
                        }
                    }
                    con.Close();
                }
            }
            return View(shops);
        }

        public ActionResult ModifyShopProcess(Models.ShopModel s, string type)
        {
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                if (type == "Create")
                {
                    string query = string.Format("INSERT INTO shop(user_id, name, description) VALUES(@user_id, @name, @description)");

                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@user_id", MySqlDbType.VarChar).Value = GetUserID();
                        cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = s.Name;
                        cmd.Parameters.Add("@description", MySqlDbType.VarChar).Value = s.Description;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        con.Close();
                    }
                    return RedirectToAction("Profile", "User");
                }
                else
                {
                    string query = string.Format("UPDATE shop SET name=@name, description=@description WHERE shop_id=@shop_id");

                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@shop_id", MySqlDbType.Int32).Value = GetShopID();
                        cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = s.Name;
                        cmd.Parameters.Add("@description", MySqlDbType.VarChar).Value = s.Description;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        con.Close();
                    }
                    return RedirectToAction("Profile", "User");
                }
            }
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

        public int GetShopID()
        {
            Dictionary<string, object> rs = null;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT * FROM shop WHERE user_id=@user_id");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@user_id", MySqlDbType.Int32).Value = GetUserID();
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
            return Convert.ToInt32(rs["shop_id"]);
        }
    }
}
