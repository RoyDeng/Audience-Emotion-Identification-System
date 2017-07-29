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
    public class ProductController : Controller
    {
        public ActionResult Index(int? page, string name, int? category)
        {
            if (page == null) page = 1;
            if (name == "") name = null;
            if (category == null) category = 0;
            int total = TotalProductCount(name);
            total = (int)Math.Ceiling(total / 6.0);
            StringBuilder sb = new StringBuilder();
            if (name != null)
                for (int i = 1; i <= total; i++)
                {
                    if (total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Products?page={0}&name={1}&category={2}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", i - 1, name, category));
                    if (i == page.Value)
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='Products?page={0}&name={1}&category={2}'>{0}</a></li>", i, name, category));
                    else
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='Products?page={0}&name={1}&category={2}'>{0}</a></li>", i, name, category));
                    if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Products?page={0}&name={1}&category={2}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", i + 1, name, category));
                }
            else
                for (int i = 1; i <= total; i++)
                {
                    if (total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Products?page={0}&category={1}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", i - 1, category));
                    if (i == page.Value)
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='Products?page={0}&category={1}'>{0}</a></li>", i, category));
                    else
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='Products?page={0}&category={1}'>{0}</a></li>", i, category));
                    if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Products?page={0}&category={1}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", i + 1, category));
                }
            ViewData["pages"] = sb.ToString();
            ViewData["name"] = name;
            ViewData["cat"] = category;
            string query = "";
            List<Models.ProductModel> products = new List<Models.ProductModel>();
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                if (name != null)
                    if (category != 0)
                        query = "SELECT * FROM product WHERE quantity>0 AND name LIKE @name AND category_id=@category_id LIMIT @start, @number";
                    else
                        query = "SELECT * FROM product WHERE quantity>0 AND name LIKE @name LIMIT @start, @number";
                else
                    if (category != 0)
                        query = "SELECT * FROM product WHERE quantity>0 AND category_id=@category_id LIMIT @start, @number";
                    else
                        query = "SELECT * FROM product WHERE quantity>0 LIMIT @start, @number";
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    if (name != null) cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = "%" + name + "%";
                    if (category != 0) cmd.Parameters.Add("@category_id", MySqlDbType.VarChar).Value = category;
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
                                Price = Convert.ToDecimal(sdr["price"])
                            });
                        }
                    }
                    con.Close();
                }
            }
            ViewData["category"] = AllProductCategory();
            return View(products);
        }

        public int TotalProductCount(string name)
        {
            int cnt = 0;
            string query = "";
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                if (name != null) query = "SELECT COUNT(*) FROM product WHERE name LIKE @name";
                else query = "SELECT COUNT(*) FROM product";
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

        public ActionResult ProductDetail(int ProductID)
        {
            Dictionary<string, object> rs = null;
            Models.ProductModel product = new Models.ProductModel();
            Models.ShopModel shop = new Models.ShopModel();
            Models.UserModel user = new Models.UserModel();
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT p.product_id, category_id, p.shop_id, p.name, p.description, quantity, price, s.name AS shop_name, username, number FROM ((product AS p INNER JOIN shop AS s ON p.shop_id=s.shop_id) INNER JOIN user AS u ON s.user_id=u.user_id) INNER JOIN video AS v ON p.product_id=v.product_id WHERE p.product_id=@product_id");

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
                    ViewData["categery"] = GetProductCategoryName(Convert.ToInt32(rs["category_id"]));
                    product.ProductID = Convert.ToInt32(rs["product_id"]);
                    product.ShopID = Convert.ToInt32(rs["shop_id"]);
                    product.Name = rs["name"].ToString();
                    product.Description = rs["description"].ToString();
                    product.Quantity = Convert.ToInt32(rs["quantity"]);
                    product.Price = Convert.ToDecimal(rs["price"]);
                    shop.Name = rs["shop_name"].ToString();
                    user.Username = rs["username"].ToString();
                    ViewData["number"] = rs["number"].ToString();
                }
            }

            return View(Tuple.Create(product, shop, user));
        }

        public ActionResult CreateProduct()
        {
            if (Session["username"] != null)
            {
                if (CheckShopExist(GetUserID()))
                {
                    ViewData["category"] = AllProductCategory();
                    return View();
                }
                else
                {
                    ViewData["msg"] = "尚未建立商店!";
                    return RedirectToAction("MyProduct", "User");
                }
            }
            else return RedirectToAction("Login", "User");
        }

        public ActionResult ModifyProductProcess(Models.ProductModel p, HttpPostedFileBase Image)
        {
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("INSERT INTO product(shop_id, category_id, name, description, quantity, image, price, date_added) VALUES(@shop_id, @category_id, @name, @description, @quantity, @image, @price, @date_added)");

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    byte[] image = null;
                    if (Image.InputStream != null)
                    {
                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        Image.InputStream.CopyTo(ms, Image.ContentLength);
                        image = ms.ToArray();
                    }
                    cmd.Connection = con;
                    cmd.Parameters.Add("@shop_id", MySqlDbType.Int32).Value = GetShopID();
                    cmd.Parameters.Add("@category_id", MySqlDbType.Int32).Value = p.CategoryID;
                    cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = p.Name;
                    cmd.Parameters.Add("@description", MySqlDbType.VarChar).Value = p.Description;
                    cmd.Parameters.Add("@quantity", MySqlDbType.Int32).Value = p.Quantity;
                    cmd.Parameters.Add("@image", MySqlDbType.LongBlob).Value = image;
                    cmd.Parameters.Add("@price", MySqlDbType.Decimal).Value = p.Price;
                    cmd.Parameters.Add("@date_added", MySqlDbType.DateTime).Value = DateTime.Now;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    con.Close();
                }
            }
            return RedirectToAction("CreateVideo", "Video"); ;
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

        public ActionResult GetProductImage(int? ProductID)
        {
            byte[] image = null;
            if (ProductID != null)
            {
                string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = string.Format("SELECT image FROM product WHERE product_id=@product_id");
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@product_id", MySqlDbType.Int32).Value = ProductID;
                        con.Open();
                        image = cmd.ExecuteScalar() as byte[];
                        cmd.Dispose();
                        con.Close();
                    }
                }
            }
            if (image != null) return File(image, "image/gif");
            else return new EmptyResult();
        }

        public Dictionary<int, string> AllProductCategory()
        {
            Dictionary<int, string> rs = null;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT category_id, name FROM category");

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    MySqlDataReader sdr = cmd.ExecuteReader();
                    rs = new Dictionary<int, string>();
                    while (sdr.Read())
                        rs.Add(sdr.GetInt32(0), sdr.GetString(1));
                    sdr.Close();
                    cmd.Dispose();
                    con.Close();
                    Console.Write(rs);
                }
            }
            return rs;
        }

        public string GetProductCategoryName(int CategoryID)
        {
            Dictionary<string, object> rs = null;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT name FROM category WHERE category_id=@category_id");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@category_id", MySqlDbType.Int32).Value = CategoryID;
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
            return rs["name"].ToString();
        }

        public bool CheckShopExist(int user_id)
        {
            int cnt = 0;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT COUNT(*) FROM shop WHERE user_id=@user_id");

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@user_id", MySqlDbType.Int32).Value = user_id;
                    con.Open();
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Dispose();
                    con.Close();
                }
            }
            if (cnt > 0) return true;
            else return false;
        }
    }
}
