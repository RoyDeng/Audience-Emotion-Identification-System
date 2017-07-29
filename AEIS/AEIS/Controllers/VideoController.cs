using System;
using Microsoft.ProjectOxford.Emotion.Contract;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Text;

namespace AEIS.Controllers
{
    public class VideoController : Controller
    {
        public ActionResult Index(int? page, string title)
        {
            if (page == null) page = 1;
            if (title == "") title = null;
            int total = TotalVideoCount(title);
            total = (int)Math.Ceiling(total / 12.0);
            StringBuilder sb = new StringBuilder();
            if (title != null)
                for (int i = 1; i <= total; i++)
                {
                    if (total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Videos?page={0}&title={1}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", i - 1, title));
                    if (i == page.Value)
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='Videos?page={0}&title={1}'>{0}</a></li>", i, title));
                    else
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='Videos?page={0}&title={1}'>{0}</a></li>", i, title));
                    if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Videos?page={0}&title={1}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", i + 1, title));
                }
            else
                for (int i = 1; i <= total; i++)
                {
                    if(total > 1 && (i != 1 || i > 1)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Videos?page={0}'><span aria-hidden='true'><i class='fa fa-angle-left'></i></span></a></li>", i - 1));
                    if (i == page.Value)
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 u-pagination-v1-5--active rounded g-pa-4-11' href='Videos?page={0}'>{0}</a></li>", i));
                    else
                        sb.Append(String.Format("<li class='list-inline-item hidden-sm-down'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-11' href='Videos?page={0}'>{0}</a></li>", i));
                    if (total > 1 && (i < total || i != total)) sb.Append(String.Format("<li class='list-inline-item'><a class='u-pagination-v1__item u-pagination-v1-5 rounded g-pa-4-13' href='Videos?page={0}'><span aria-hidden='true'><i class='fa fa-angle-right'></i></span></a></li>", i + 1));
                }
            ViewData["pages"] = sb.ToString();
            string query = "";
            List<Models.VideoModel> videos = new List<Models.VideoModel>();
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                if (title != null) query = "SELECT p.product_id, title, number, v.date_start FROM video as v INNER JOIN product AS p ON v.product_id=p.product_id WHERE v.title LIKE @title AND v.online=1 LIMIT @start, @number";
                else query = "SELECT p.product_id, title, number, v.date_start FROM video as v INNER JOIN product AS p ON v.product_id=p.product_id WHERE v.online=1 LIMIT @start, @number";
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    if (title != null) cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = "%" + title + "%";
                    cmd.Parameters.Add("@start", MySqlDbType.Int32).Value = (page - 1) * 12;
                    cmd.Parameters.Add("@number", MySqlDbType.Int32).Value = 12;
                    con.Open();
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            videos.Add(new Models.VideoModel
                            {
                                ProductID = Convert.ToInt32(sdr["product_id"]),
                                Title = sdr["title"].ToString(),
                                Number = sdr["number"].ToString(),
                                DateStart = sdr["date_start"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return View(videos);
        }

        public int TotalVideoCount(string title)
        {
            int cnt = 0;
            string query = "";
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                if (title != null) query = "SELECT COUNT(*) FROM video WHERE title LIKE @title AND online=1";
                else query = "SELECT COUNT(*) FROM video WHERE online=1";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    if (title != null) cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = "%" + title + "%";
                    con.Open();
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                }
            }
            return cnt;
        }

        public ActionResult CreateVideo()
        {
            if (Session["username"] != null) return View();
            else return RedirectToAction("Login", "User");
        }

        public ActionResult StreamVideoProcess(Models.VideoModel v)
        {
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("INSERT INTO video(product_id, title, number, date_start, online) VALUES(@product_id, @title, @number, @date_start, @online)");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    v.Number = GetRandomString(6) + DateTime.Now.ToString("yyyy-MM-dd");
                    cmd.Connection = con;
                    cmd.Parameters.Add("@product_id", MySqlDbType.Int32).Value = GetProductID();
                    cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = v.Title;
                    cmd.Parameters.Add("@number", MySqlDbType.VarChar).Value = v.Number;
                    cmd.Parameters.Add("@date_start", MySqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@online", MySqlDbType.Int32).Value = 1;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    con.Close();
                }
                return RedirectToAction("StreamVideo", "Video");
            }
        }

        public ActionResult StreamVideo()
        {
            if (Session["username"] != null)
            {
                Models.VideoModel video = new Models.VideoModel();
                Models.ProductModel product = new Models.ProductModel();
                Dictionary<string, object> rs = null;
                string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = string.Format("SELECT p.product_id, category_id, name, description, quantity, price, title, number FROM video AS v, product AS p WHERE p.product_id=@product_id");
                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.Add("@product_id", MySqlDbType.Int32).Value = GetProductID();
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
                        product.ProductID = Convert.ToInt32(rs["product_id"]);
                        product.Name = rs["name"].ToString();
                        product.Description = rs["description"].ToString();
                        product.Quantity = Convert.ToInt32(rs["quantity"]);
                        product.Price = Convert.ToDecimal(rs["price"]);
                        ViewData["categery"] = GetProductCategoryName(Convert.ToInt32(rs["category_id"]));
                    }
                }
                return View(Tuple.Create(video, product));
            }
            else return RedirectToAction("Login", "User");
        }

        public ActionResult WatchVideo(string Number)
        {
            Models.VideoModel video = new Models.VideoModel();
            Models.ProductModel product = new Models.ProductModel();
            Dictionary<string, object> rs = null;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT p.product_id, category_id, name, description, quantity, price, video_id, title, number FROM video AS v INNER JOIN product AS p ON v.product_id=p.product_id WHERE v.number=@number");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@number", MySqlDbType.VarChar).Value = Number;
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
                    video.VideoID = Convert.ToInt32(rs["video_id"]);
                    video.Title = rs["title"].ToString();
                    video.Number = rs["number"].ToString();
                    product.ProductID = Convert.ToInt32(rs["product_id"]);
                    product.Name = rs["name"].ToString();
                    product.Description = rs["description"].ToString();
                    product.Quantity = Convert.ToInt32(rs["quantity"]);
                    product.Price = Convert.ToDecimal(rs["price"]);
                    ViewData["categery"] = GetProductCategoryName(Convert.ToInt32(rs["category_id"]));
                }
            }
            return View(Tuple.Create(video, product));
        }

        public void UploadVideoProcess(string number, string viewed)
        {
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("UPDATE video SET online=@online, date_end=@date_end, viewed=@viewed WHERE number=@number");

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@number", MySqlDbType.VarChar).Value = number;
                    cmd.Parameters.Add("@online", MySqlDbType.Int32).Value = 0;
                    cmd.Parameters.Add("@date_end", MySqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@viewed", MySqlDbType.Int32).Value = Convert.ToInt32(viewed);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    con.Close();
                }
            }
        }

        [HttpPost]
        public ActionResult UploadVideo()
        {
            foreach (string upload in Request.Files)
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
                var file = Request.Files[upload];
                if (file == null) continue;
                file.SaveAs(Path.Combine(path, Request.Form[0]));
            }
            return Json(Request.Form[0]);
        }

        public void UploadEmotionProcess(int VideoID, Emotion[] emotionResult)
        {
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("INSERT INTO emotion(video_id, anger, contempt, disgust, fear, happiness, neutral, sadness, surprise, date_added) VALUES(@video_id, @anger, @contempt, @disgust, @fear, @happiness, @neutral, @sadness, @surprise, @date_added)");

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    if (emotionResult != null && emotionResult.Length > 0)
                    {
                        foreach (Emotion emotion in emotionResult)
                        {
                            cmd.Parameters.Add("@video_id", MySqlDbType.Int32).Value = VideoID;
                            cmd.Parameters.Add("@anger", MySqlDbType.Double).Value = Convert.ToDouble(emotion.Scores.Anger);
                            cmd.Parameters.Add("@contempt", MySqlDbType.Double).Value = Convert.ToDouble(emotion.Scores.Contempt);
                            cmd.Parameters.Add("@disgust", MySqlDbType.Double).Value = Convert.ToDouble(emotion.Scores.Disgust);
                            cmd.Parameters.Add("@fear", MySqlDbType.Double).Value = Convert.ToDouble(emotion.Scores.Fear);
                            cmd.Parameters.Add("@happiness", MySqlDbType.Double).Value = Convert.ToDouble(emotion.Scores.Happiness);
                            cmd.Parameters.Add("@neutral", MySqlDbType.Double).Value = Convert.ToDouble(emotion.Scores.Neutral);
                            cmd.Parameters.Add("@sadness", MySqlDbType.Double).Value = Convert.ToDouble(emotion.Scores.Sadness);
                            cmd.Parameters.Add("@surprise", MySqlDbType.Double).Value = Convert.ToDouble(emotion.Scores.Surprise);
                            cmd.Parameters.Add("@date_added", MySqlDbType.DateTime).Value = DateTime.Now;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                        }
                    }
                    cmd.Dispose();
                    con.Close();
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

        public int GetProductID()
        {
            Dictionary<string, object> rs = null;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT * FROM product WHERE shop_id=@shop_id ORDER BY product_id DESC LIMIT 1");
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@shop_id", MySqlDbType.Int32).Value = GetShopID();
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
            return Convert.ToInt32(rs["product_id"]);
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

        public static string GetRandomString(int length)
        {
            Random rnd = new Random();
            string code = "";
            for (int i = 0; i < length; ++i)
                switch (rnd.Next(0, 3))
                {
                    case 0: code += rnd.Next(0, 10); break;
                    case 1: code += (char)rnd.Next(65, 91); break;
                    case 2: code += (char)rnd.Next(97, 122); break;
                }
            return code;
        }
    }
}
