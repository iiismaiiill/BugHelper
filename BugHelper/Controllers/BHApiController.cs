using BugHelper.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace BugHelper.Controllers
{
    public class BHApiController : ApiController
    {
        private SorularContext sc = new SorularContext();
        public List<SorularModel> GetSorular()
        {
            var model = sc.Sorular.ToList();
            return model;
        }
        public void PostSoruEkle(SorularModel model)
        {
            sc.Sorular.Add(model);
            model.SorulmaTarihi = System.DateTime.Now;
            sc.SaveChanges();
        }
        public void PutSoru(SorularModel model)
        {
            var soru = sc.Sorular.Where(i => i.Id == model.Id || i.SoruBaslik == model.SoruBaslik ||
            i.SoruIcerik == model.SoruIcerik
            ).FirstOrDefault();
            if (soru != null) { 
            soru.KodlamaDili = model.KodlamaDili;
            soru.Onay = model.Onay;
            soru.SoruBaslik = model.SoruBaslik;
            soru.SoruIcerik = model.SoruIcerik;
            soru.SoruSahibi = model.SoruSahibi;
            sc.SaveChanges();
                }
        }
        public void DeleteSoru(SorularModel model)
        {
            var soru = sc.Sorular.Where(i => i.Id == model.Id || i.SoruBaslik == model.SoruBaslik ||
            i.SoruIcerik == model.SoruIcerik
            ).FirstOrDefault();
            if (soru != null) { 
            sc.ArtiOy.RemoveRange(sc.ArtiOy.Where(i => i.Soru.Id == soru.Id));
            sc.EksiOy.RemoveRange(sc.EksiOy.Where(i => i.Soru.Id == soru.Id));
            sc.ArtiOyCevaplar.RemoveRange(sc.Cevaplar.Where(i => i.Soru.Id == soru.Id).SelectMany(a => a.ArtiOyCevaplar));
            sc.EksiOyCevaplar.RemoveRange(sc.Cevaplar.Where(i => i.Soru.Id == soru.Id).SelectMany(a => a.EksiOyCevaplar));
            sc.Cevaplar.RemoveRange(sc.Cevaplar.Where(i => i.Soru.Id == soru.Id));
            sc.Sorular.Remove(sc.Sorular.Where(i => i.Id == soru.Id).FirstOrDefault());
            sc.SaveChanges();
            }
        }
    }
}
