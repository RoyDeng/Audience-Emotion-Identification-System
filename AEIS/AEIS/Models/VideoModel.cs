using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AEIS.Models
{
    public class VideoModel
    {
        public int? VideoID { get; set; }
        public int? ProductID { get; set; }
        public string Title { get; set; }
        public string Number { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }
        public int Online { get; set; }
        public int Viewed { get; set; }
    }
}
