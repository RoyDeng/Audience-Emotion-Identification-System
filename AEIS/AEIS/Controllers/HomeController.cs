using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace AEIS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<Models.ProductModel> products = new List<Models.ProductModel>();
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT p.product_id, name, price FROM order_detail AS od INNER JOIN product AS p ON od.product_id=p.product_id ORDER BY od.quantity LIMIT 10";
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
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
            return View(products);
        }

        public ActionResult Terms()
        {
            return View();
        }
    }
}