using BugHelper.Identity;
using PagedList;
using System.Collections.Generic;

namespace BugHelper.Models
{
    public class SoruCevapModel
    {
        public SorularContext SoruCevapContext { get; set; }
        public SorularModel[] SorularModelListe{ get; set; }
        public CevaplarModel[] CevaplarModelListe { get; set; }
        public List<CevaplarModel> CevaplarModelForSoru { get; set; }
        public SorularModel Soru { get; set; }
        public string SoruSahibi { get; set; }
        public bool FavorideMi = false;
        public int SoruId { get; set; }
        public int CevapSayisi { get; set; }
        public ApplicationUser SoruIzleyıcı { get; set; }
        public int CevapId { get; set; }
        public IPagedList PagedList { get; set; }
    }
}