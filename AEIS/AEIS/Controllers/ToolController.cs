using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace AEIS.Controllers
{
    public class ToolController : Controller
    {
        public string CheckUsernameDuplicate(string username)
        {
            int cnt = 0;
            string constr = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = string.Format("SELECT COUNT(*) FROM user WHERE username=@username");

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = username;
                    con.Open();
                    cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Dispose();
                    con.Close();
                }
            }
            if (cnt > 0) return "y";
            else return "n";
        }
    }
}
