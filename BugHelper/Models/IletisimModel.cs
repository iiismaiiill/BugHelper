using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugHelper.Models
{
    public class IletisimModel
    {
        public int Id { get; set; }
        [Required]
        public string AdSoyad { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Mesaj { get; set; }
    }
    public class IletisimPagedModel
    {
        public IPagedList PagedList { get; set; }
        public List<IletisimModel> IletisimModelList { get; set; }
    }
}