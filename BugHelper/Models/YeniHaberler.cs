using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugHelper.Models
{
    public class YeniHaberler
    {
        public int Id { get; set; }
        public string HaberBaslik { get; set; }
        public string HaberIcerik { get; set; }
        public string HaberLinki { get; set; }
    }
}