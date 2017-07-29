using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace AEIS.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Login(string msg)
        {
            if (msg == "fail")  ViewData["msg"] = "使用者名稱或密碼有誤!";
            return View();
        }

        [HttpPost]
        public ActionResult LoginProcess(string Username, string Password, string Url)
        {
            int cnt = 0;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] orignalPassword = System.Text.Encoding.UTF8.GetBytes(Password);
                byte[] hashedPassword = sha.ComputeHash(orignalPassword);
                string query = string.Format("SELECT COUNT(*) FROM user WHERE username = @username AND password = @password");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Username;
                    cmd.Parameters.Add("@password", MySqlDbType.Blob).Value = hashedPassword;
                    con.Open();
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Dispose();
                    con.Close();
                }
            }
            if (cnt > 0)
            {
                Session.Add("UserName", Username);
                return RedirectToAction("Index", "Home");
            }
            else return RedirectToAction("Login", new { msg = "fail" });
        }

        public ActionResult Logout()
        {
            Session["UserName"] = "";
            Session.Remove("UserName");
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult RegisterProcess(Models.UserModel u)
        {
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] orignalPassword = System.Text.Encoding.UTF8.GetBytes(u.Password);
                byte[] hashedPassword = sha.ComputeHash(orignalPassword);
                string query = string.Format("INSERT INTO user(username, password, firstname, lastname, email, date_added) VALUES(@username, @password, @firstname, @lastname, @email, @date_added)");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = u.Username;
                    cmd.Parameters.Add("@password", MySqlDbType.Blob).Value = hashedPassword;
                    cmd.Parameters.Add("@firstname", MySqlDbType.VarChar).Value = u.FirstName;
                    cmd.Parameters.Add("@lastname", MySqlDbType.VarChar).Value = u.LastName;
                    cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = u.Email;
                    cmd.Parameters.Add("@date_added", MySqlDbType.DateTime).Value = DateTime.Now;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    con.Close();
                    Session.Add("UserName", u.Username);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Profile(string msg)
        {
            if (Session["username"] != null)
            {
                Models.UserModel user = new Models.UserModel();
                Models.ShopModel shop = new Models.ShopModel();
                Dictionary<string, object> rs = null;
                string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = string.Format("SELECT firstname, lastname, email, name, description FROM user AS u LEFT JOIN shop AS s ON u.user_id=s.user_id WHERE u.user_id=@user_id");
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@user_id", MySqlDbType.VarChar).Value = GetUserID();
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
                        user.FirstName = rs["firstname"].ToString();
                        user.LastName = rs["lastname"].ToString();
                        user.Email = rs["email"].ToString();
                        shop.Name = rs["name"].ToString();
                        shop.Description = rs["description"].ToString();
                        if (msg == "fail") ViewData["msg"] = "舊密碼有誤!";
                    }
                }

                ViewData["type"] = CheckShopDuplicate(GetUserID());
                return View(Tuple.Create(user, shop));
            }
            else return RedirectToAction("Login", "User");
        }

        public ActionResult MyProduct(int? page)
        {
            if (Session["username"] != null)
            {
                if (CheckShopExist(GetUserID()))
                {
                    if (page == null) page = 1;
                    int total = TotalMyProductCount();
                    total = (int)Math.Ceiling(total / 6.0);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 1; i <= total; i++)
                    {
                        if (total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='MyProduct?page={0}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", i - 1));
                        if (i == page.Value)
                            sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='MyProduct?page={0}'>{0}</a></li>", i));
                        else
                            sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='MyProduct?page={0}'>{0}</a></li>", i));
                        if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='MyProduct?page={0}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", i + 1));
                    }
                    ViewData["pages"] = sb.ToString();
                    List<Models.ProductModel> products = new List<Models.ProductModel>();
                    string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                    using (MySqlConnection con = new MySqlConnection(constr))
                    {
                        string query = "SELECT * FROM product WHERE shop_id=@shop_id LIMIT @start, @number";
                        using (MySqlCommand cmd = new MySqlCommand(query))
                        {
                            cmd.Connection = con;
                            cmd.Parameters.Add("@shop_id", MySqlDbType.Int32).Value = GetShopID();
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
                    return View(products);
                }
                else
                {
                    ViewData["msg"] = "尚未建立商店!";

                    return View();
                }
            }
            else return RedirectToAction("Login", "User");
        }

        public int TotalMyProductCount()
        {
            int cnt = 0;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM shop WHERE shop_id=@shop_id", con))
                {
                    cmd.Parameters.Add("@shop_id", MySqlDbType.Int32).Value = GetShopID();
                    con.Open();
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                }
            }
            return cnt;
        }

        public ActionResult MyProductDetail(int ProductID)
        {
            if (Session["username"] != null)
            {
                Models.VideoModel video = new Models.VideoModel();
                Models.ProductModel product = new Models.ProductModel();
                List<Models.EmotionModel> emotions = new List<Models.EmotionModel>();
                Dictionary<string, object> rs = null;
                string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = string.Format("SELECT p.product_id, category_id, name, description, quantity, price, title, number, date_start, date_end, viewed FROM video AS v, product AS p WHERE p.product_id=@product_id");
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
                        video.Title = rs["title"].ToString();
                        video.Number = rs["number"].ToString();
                        video.DateStart = rs["date_start"].ToString();
                        video.DateEnd = rs["date_end"].ToString();
                        video.Viewed = Convert.ToInt32(rs["viewed"]);
                        product.ProductID = Convert.ToInt32(rs["product_id"]);
                        product.Name = rs["name"].ToString();
                        product.Description = rs["description"].ToString();
                        product.Quantity = Convert.ToInt32(rs["quantity"]);
                        product.Price = Convert.ToDecimal(rs["price"]);
                        ViewData["categery"] = GetProductCategoryName(Convert.ToInt32(rs["category_id"]));
                    }
                }
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = string.Format("SELECT SUM(unitprice*od.quantity) AS price, SUM(od.quantity) AS quantity FROM order_detail AS od INNER JOIN product AS p ON od.product_id=p.product_id WHERE od.product_id=@product_id");
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
                        ViewData["price"] = rs["price"] != DBNull.Value ? Convert.ToDecimal(rs["price"]) : 0;
                        ViewData["quantity"] = rs["quantity"] != DBNull.Value ? Convert.ToDecimal(rs["quantity"]) : 0;
                    }
                }
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = "SELECT AVG(happiness) AS happiness, date_added FROM emotion AS e INNER JOIN video AS v ON e.video_id=v.video_id WHERE e.date_added BETWEEN v.date_start AND v.date_end AND v.product_id=@product_id GROUP BY UNIX_TIMESTAMP(e.date_added) DIV 10";
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@product_id", MySqlDbType.Int32).Value = ProductID;
                        con.Open();
                        using (MySqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                emotions.Add(new Models.EmotionModel
                                {
                                    Happiness = Convert.ToDouble(sdr["happiness"]),
                                    DateAdded = Convert.ToDateTime(sdr["date_added"])
                                });
                            }
                        }
                        con.Close();
                    }
                }
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = string.Format("SELECT AVG(anger) AS anger, AVG(contempt) AS contempt, AVG(disgust) AS disgust, AVG(fear) AS fear, AVG(happiness) AS happiness, AVG(neutral) AS neutral, AVG(sadness) AS sadness, AVG(surprise) AS surprise FROM emotion WHERE video_id=@video_id");
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@video_id", MySqlDbType.Int32).Value = GetVideoID(ProductID);
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
                        ViewData["anger"] = rs["anger"] != DBNull.Value ? Convert.ToDouble(rs["anger"]) : 0;
                        ViewData["contempt"] = rs["contempt"] != DBNull.Value ? Convert.ToDouble(rs["contempt"]) : 0;
                        ViewData["disgust"] = rs["disgust"] != DBNull.Value ? Convert.ToDouble(rs["disgust"]) : 0;
                        ViewData["fear"] = rs["fear"] != DBNull.Value ? Convert.ToDouble(rs["fear"]) : 0;
                        ViewData["happiness"] = rs["happiness"] != DBNull.Value ? Convert.ToDouble(rs["happiness"]) : 0;
                        ViewData["neutral"] = rs["neutral"] != DBNull.Value ? Convert.ToDouble(rs["neutral"]) : 0;
                        ViewData["sadness"] = rs["sadness"] != DBNull.Value ? Convert.ToDouble(rs["sadness"]) : 0;
                        ViewData["surprise"] = rs["surprise"] != DBNull.Value ? Convert.ToDouble(rs["surprise"]) : 0;
                        con.Close();
                    }
                }
                return View(Tuple.Create(video, product, emotions));
            }
            else return RedirectToAction("Login", "User");
        }

        public ActionResult MyOrder(int? page)
        {
            if (Session["username"] != null)
            {
                if (page == null) page = 1;
                int total = TotalMyOrderCount();
                total = (int)Math.Ceiling(total / 10.0);
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i <= total; i++)
                {
                    if (total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='MyOrder?page={0}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", i - 1));
                    if (i == page.Value)
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='MyOrder?page={0}'>{0}</a></li>", i));
                    else
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='MyOrder?page={0}'>{0}</a></li>", i));
                    if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='MyOrder?page={0}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", i + 1));
                }
                ViewData["pages"] = sb.ToString();
                List<Models.OrderModel> orders = new List<Models.OrderModel>();
                string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = "SELECT * FROM `order` WHERE user_id=@user_id LIMIT @start, @number";
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@user_id", MySqlDbType.VarChar).Value = GetUserID();
                        cmd.Parameters.Add("@start", MySqlDbType.Int32).Value = (page - 1) * 10;
                        cmd.Parameters.Add("@number", MySqlDbType.Int32).Value = 10;
                        con.Open();
                        using (MySqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                orders.Add(new Models.OrderModel
                                {
                                    OrderID = sdr["order_id"].ToString(),
                                    Total = Convert.ToDecimal(sdr["total"]),
                                    DateAdded = sdr["date_added"].ToString()
                                });
                            }
                        }
                        con.Close();
                    }
                }
                return View(orders);
            }
            else return RedirectToAction("Login", "User");
        }

        public int TotalMyOrderCount()
        {
            int cnt = 0;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM `order` WHERE user_id=@user_id", con))
                {
                    cmd.Parameters.Add("@user_id", MySqlDbType.Int32).Value = GetUserID();
                    con.Open();
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                }
            }
            return cnt;
        }

        public ActionResult UpdateUserProcess(Models.UserModel u)
        {
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("UPDATE user SET firstname=@firstname, lastname=@lastname, email=@email WHERE username=@username");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Session["username"].ToString().ToLower();
                    cmd.Parameters.Add("@firstname", MySqlDbType.VarChar).Value = u.FirstName;
                    cmd.Parameters.Add("@lastname", MySqlDbType.VarChar).Value = u.LastName;
                    cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = u.Email;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    con.Close();
                }
            }
            return RedirectToAction("Profile", "User");
        }

        public ActionResult ChangePasswordProcess(string old_password, string new_password)
        {
            if (CheckPassword(old_password))
            {
                string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    SHA256Managed sha = new SHA256Managed();
                    byte[] orignalPassword = System.Text.Encoding.UTF8.GetBytes(new_password);
                    byte[] hashedPassword = sha.ComputeHash(orignalPassword);
                    string query = string.Format("UPDATE user SET password=@password WHERE username=@username");
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Session["username"].ToString().ToLower();
                        cmd.Parameters.Add("@password", MySqlDbType.Blob).Value = hashedPassword;
                        con.Open();
                        int cnt = cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        con.Close();
                    }
                    return RedirectToAction("Profile", "User");
                }
            }
            else return RedirectToAction("Profile", "User", new { msg = "fail" });
        }

        public ActionResult ForgetPassword(string msg)
        {
            if (msg == "success") ViewData["msg"] = "success";
            else if (msg == "fail") ViewData["msg"] = "fail";
            return View();
        }

        public ActionResult ForgetPasswordProcess(Models.UserModel u)
        {
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT COUNT(*) FROM user WHERE username=@username AND email=@email");
                using (MySqlCommand cmd1 = new MySqlCommand(query))
                {
                    cmd1.Connection = con;
                    cmd1.Parameters.Add("@username", MySqlDbType.VarChar).Value = u.Username;
                    cmd1.Parameters.Add("@email", MySqlDbType.VarChar).Value = u.Email;
                    con.Open();
                    int cnt = Convert.ToInt32(cmd1.ExecuteScalar());
                    cmd1.Dispose();
                    con.Close();
                    if (cnt > 0)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        Random rnd = new Random();
                        for (int i = 1; i <= 8; i++)
                        {
                            char c = Convert.ToChar(rnd.Next(33, 127));
                            sb.Append(c);
                        }
                        string password = sb.ToString();
                        SHA256Managed sha = new SHA256Managed();
                        byte[] orignalPassword = System.Text.Encoding.UTF8.GetBytes(password);
                        byte[] hashedPassword = sha.ComputeHash(orignalPassword);
                        query = string.Format("UPDATE user SET password=@password WHERE username=@username");
                        using (MySqlCommand cmd2 = new MySqlCommand(query))
                        {
                            cmd2.Connection = con;
                            cmd2.Parameters.Add("@username", MySqlDbType.VarChar).Value = u.Username;
                            cmd2.Parameters.Add("@password", MySqlDbType.Blob).Value = hashedPassword;
                            con.Open();
                            cmd2.ExecuteNonQuery();
                            cmd2.Dispose();
                            con.Close();
                        }
                        System.Net.Mail.SmtpClient mailServer = new System.Net.Mail.SmtpClient("msa.hinet.net");
                        System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                        mail.From = new System.Net.Mail.MailAddress("xoxo4795946@yahoo.com.tw", "AEIS");
                        mail.To.Add(new System.Net.Mail.MailAddress(u.Email));
                        mail.Subject = "AEIS密碼變更通知";
                        mail.SubjectEncoding = System.Text.Encoding.UTF8;
                        mail.IsBodyHtml = true;
                        mail.BodyEncoding = System.Text.Encoding.UTF8;
                        sb = new System.Text.StringBuilder();
                        sb.Append(string.Format("<h3>親愛的{0}您好：</h3>", u.Username));
                        sb.Append(string.Format("<h3>您的新登入密碼為：<span style='color:red;'>{0}</span></h3>", password));
                        sb.Append("<h3>請利用新密碼登入網站後，透過會員專區來變更您的密碼！</h3>");
                        sb.Append("<h3>感謝您對本站的支持，謝謝！</h3>");
                        mail.Body = sb.ToString();
                        mailServer.Send(mail);

                        return RedirectToAction("ForgetPassword", "User", new { msg = "success" });
                    }
                    else return RedirectToAction("ForgetPassword", "User", new { msg = "fail" });
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

        public int GetVideoID(int ProductID)
        {
            Dictionary<string, object> rs = null;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT * FROM video WHERE product_id=@product_id");
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
            return Convert.ToInt32(rs["video_id"]);
        }

        public bool CheckPassword(string password)
        {
            int cnt = 0;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] orignalPassword = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hashedPassword = sha.ComputeHash(orignalPassword);
                string query = string.Format("SELECT COUNT(*) FROM user WHERE username=@username AND password=@password");

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Session["username"].ToString().ToLower();
                    cmd.Parameters.Add("@password", MySqlDbType.VarChar).Value = hashedPassword;
                    con.Open();
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Dispose();
                    con.Close();
                }
            }
            if (cnt > 0) return true;
            else return false;
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

        public bool CheckShopDuplicate(int user_id)
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
