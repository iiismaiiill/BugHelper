	/* Kullanıcının diğer kullanıcıları takibi için yazılan metod */
[HttpPost]
        public PartialViewResult TakipEt(string kullaniciAdi)
        {
            dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().TakipEttikleri.Add(new TakipEttikleriModel //Burada Foreign Key'in kullanımı vardır. Görüldüğü üzere Users üzerinden TakipEttikleri ekleniyor ve bu ikisi Entity Framework tarafından foreign key ile ilişkilendiriliyor. Daha sonra bunlara ilişkin değerlere birbirleri üzerinden erişebiliyoruz 
            {                                       //alttaki kod satırları da benzer şekildeki linq sorgularından ibarettir
                TakipEttikleri = kullaniciAdi
            });
            dc.Users.Where(i => i.UserName == kullaniciAdi).FirstOrDefault().Takipci.Add(new TakipciModel
            {
                Takipci = User.Identity.Name
            });
            dc.SaveChanges();
            string mesaj = "<div class=\"alert alert-success\" role=\"alert\">" + kullaniciAdi + " adlı kullanıcıyı takip ediyorsunuz</div>";
            return PartialView("_Mesaj", mesaj);
        }
		
    /* Kullanıcının diğer kullanıcıları takipten çıkabilmesi için yazılan metod */
        public PartialViewResult TakiptenCik(string kullaniciAdi)
        {
            dc.TakipEttikleri.Remove(dc.TakipEttikleri.Where(i => i.ApplicationUser.UserName == User.Identity.Name && i.TakipEttikleri == kullaniciAdi).FirstOrDefault());
            dc.Takipci.Remove(dc.Takipci.Where(i => i.ApplicationUser.UserName == kullaniciAdi && i.Takipci == User.Identity.Name).FirstOrDefault());
            dc.SaveChanges();
            string mesaj = "<div class=\"alert alert-success\" role=\"alert\">" + kullaniciAdi + " adlı kullanıcıyı takipten çıktınız</div>";
            return PartialView("_Mesaj", mesaj);
        }
		
		/* *******************************************/
		/* **Takip etme durumu ile ilgili metodlar** */
		/* *******************************************/
		
		/* Kullanıcıların profillerinin görüntülenmesine ilişkin metodda kullanıcının, profiline girdiği kullanıcıyı o anda takip edip etmediğine ilişkin
		kontroller yapıldı */
		
		[AllowAnonymous]
        [Route("kullanici/{takmaAd}/{KullaniciAdi}")]
        public ActionResult ProfilGoruntule()
        {
            string kullaniciAdi = (RouteData.Values["KullaniciAdi"].ToString());//urlye gelen KullaniciAdi değerini çekip oluşturduğumuz stringe daha sonra kullanmak üzere atıyoruz
            string mesaj = "<div class=\"alert alert-warning\" role=\"alert\">Kullanıcı bulunamadı</div>";
            model.SoruSayisi = sc.Sorular.Count(i => i.SoruSahibi == kullaniciAdi);
            model.CevapSayisi = sc.Cevaplar.Where(i => i.CevapSahibi == kullaniciAdi).Count();
            model.Kullanici = dc.Users.Where(i => i.UserName == kullaniciAdi).FirstOrDefault();
            if (model.Kullanici == null) return View("Hata", "_Layout", mesaj);
            model.KullaniciSorulari = sc.Sorular.Where(i => i.SoruSahibi == kullaniciAdi).OrderByDescending(i => i.Id).Take(5).ToList();//sorular database'inden, profiline girdiğimiz kullanıcı adı ile ilişkili soruları Id değerine göre azalan şekilde sırala(burada amaç listeyi ters çevirip son 5 soruyu almak) ve 5'ini al
            model.KullaniciCevaplari = sc.Cevaplar.Where(i => i.CevapSahibi == kullaniciAdi).OrderByDescending(i => i.Id).Take(5).ToList();
            model.KullaniciCevapSorulari = new List<SorularModel>(); //(NullReferenceException)
            if (dc.Takipci.Where(i => i.ApplicationUser.UserName == kullaniciAdi).FirstOrDefault() != null)//(NullReferenceException)
            {
                model.Takipciler = dc.Takipci.Where(i => i.ApplicationUser.UserName == kullaniciAdi).ToList();
            }
            if (User.Identity.IsAuthenticated)
            {
                if (dc.TakipEttikleri.Where(i => i.ApplicationUser.UserName == kullaniciAdi).FirstOrDefault() != null)//(NullReferenceException)
                {
                    model.TakipEdilenler = dc.TakipEttikleri.Where(i => i.ApplicationUser.UserName == kullaniciAdi).ToList();//linq sorgularının mantığı aynı olduğu için bunlara açıklama yapmıyorum
                }
            }
            for (int x = 0; x < model.KullaniciCevaplari.Count; x++)
            {
                var temp = model.KullaniciCevaplari.ElementAt(x).Id;
                model.KullaniciCevapSorulari.Add(sc.Cevaplar.Where(i => i.Id == temp).Select(a => a.Soru).FirstOrDefault());
            }
            if (User.Identity.IsAuthenticated && dc.TakipEttikleri.Any(i => i.ApplicationUser.UserName == User.Identity.Name && i.TakipEttikleri == kullaniciAdi))//(NullReferenceException) için farklı bir kullanım.Sorgu yaparken Any kullanırsak sorgu yaptığımız yerde parantez içine aldığımız nesneden olup olmadığını boolean değer olarak söyler, bu şekilde de NullReferenceException handling yapabiliriz
            {                                   //bu kontroldeki amaç giriş yapan kullanıcının profiline girdiği kullanıcıyı takıp edip etmediğine dair bir boolean değer kullanmak ve bu değer ile view'de takip butonunu gerektiği şekilde göstermek(takip et ya da takipten çık)
                model.TakipteMi = true;
            }
            return View(model);
        }
		
		