using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogSitesi.Data;
using BlogSitesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Hosting.Server;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BlogSitesi.Controllers
{
    public class AdminController : Controller
    {
        private readonly BlogDbContext _context;

        public AdminController(BlogDbContext context)
        {
            _context = context;
        }

        /* LOGİN İŞLEMLERİ */

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string kullaniciAd,string kullaniciSifre)
        {
            var blogKullanici = _context.Kullanicilar.FirstOrDefault(w => w.kullaniciAd == kullaniciAd && w.kullaniciSifre == kullaniciSifre);
            if (blogKullanici == null)
            {
                return RedirectToAction(nameof(Login));
            }
            HttpContext.Session.SetInt32("id",blogKullanici.kullaniciID);
            return RedirectToAction(nameof(AdminSayfa));
        }

       
        public IActionResult AdminSayfa()
        {
            return View();
        }


        /*PROFİL İŞLEMLERİ*/

        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            // Kullanıcının ID'sini al
            int kullaniciID = HttpContext.Session.GetInt32("id") ?? 0;

            // Kullanıcıyı veritabanından bul
            var kullanici = await _context.Kullanicilar.FindAsync(kullaniciID);

            if (kullanici == null)
            {
                return NotFound();
            }

            return View("Profil", kullanici);
        }

        [HttpPost]
        public async Task<IActionResult> KullaniciGuncelle(Kullanici model, IFormFile yazarFotograf)
        {
            int kullaniciID = HttpContext.Session.GetInt32("id") ?? 0;
            try
            {
                var kullanici = await _context.Kullanicilar.FindAsync(kullaniciID);
                if (kullanici == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı";
                    return RedirectToAction(nameof(Profil));
                }

                kullanici.kullaniciAd = model.kullaniciAd;
                kullanici.yazarAdSoyad = model.yazarAdSoyad;
                kullanici.yazarAciklama = model.yazarAciklama;

                if (yazarFotograf != null && yazarFotograf.Length > 0)
                {
                    var yazarFotografPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", yazarFotograf.FileName);
                    using (var stream = new FileStream(yazarFotografPath, FileMode.Create))
                    {
                        await yazarFotograf.CopyToAsync(stream);
                    }
                    kullanici.yazarFotograf = "/uploads/" + yazarFotograf.FileName;
                }

                if (!string.IsNullOrEmpty(model.kullaniciSifre))
                {
                    kullanici.kullaniciSifre = model.kullaniciSifre;
                }

                _context.Kullanicilar.Update(kullanici);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Güncelleme başarılı";
                return Json(new { success = true, message = "Güncelleme başarılı" }); // Başarı durumunda JSON yanıtı gönderir
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Güncelleme sırasında bir hata oluştu: " + ex.Message;
                return Json(new { success = false, message = "Güncelleme sırasında bir hata oluştu: " + ex.Message }); // Hata durumunda JSON yanıtı gönderir
            }
        }

        [HttpGet]
        public async Task<IActionResult> IcerikDuzenle(int id)
        {
            var yazi = await _context.Yazilar.FindAsync(id);
            if (yazi == null)
            {
                return NotFound();
            }

            return View(yazi);
        }


        [HttpPost]
        public async Task<IActionResult> IcerikDuzenle(int id, Yazi model, IFormFile file)
        {
            // Güncelleme işlemi yapıldıysa ve sayfa yenilendiğinde ModelState.IsValid kontrolüne gerek yok
            if (TempData.ContainsKey("IsUpdated"))
            {
                try
                {
                    var gelenKategori = _context.Kategoriler.SingleOrDefault(x => x.kategoriID == model.kategoriID);
                    model.Kategori = gelenKategori;

                    // Eğer yeni bir dosya seçilmişse, eski dosyayı sil ve yeni dosyayı kaydet
                    if (file != null && file.Length > 0)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", file.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        model.gorselYolu = filePath;
                    }

                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "İçerik başarıyla güncellendi!";
                    return RedirectToAction(nameof(Icerikler));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Güncelleme sırasında bir hata oluştu: " + ex.Message;
                    return RedirectToAction(nameof(Icerikler));
                }
            }

            // ModelState.IsValid kontrolü sadece güncelleme işlemi yapılmadığında yapılır
            if (ModelState.IsValid)
            {
                try
                {
                    var gelenKategori = _context.Kategoriler.SingleOrDefault(x => x.kategoriID == model.kategoriID);
                    model.Kategori = gelenKategori;

                    // Eğer yeni bir dosya seçilmişse, eski dosyayı sil ve yeni dosyayı kaydet
                    if (file != null && file.Length > 0)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", file.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        model.gorselYolu = filePath;
                    }

                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    // Güncelleme işlemi tamamlandığında TempData'deki anahtar değeri kontrol edilecek
                    TempData["IsUpdated"] = true;

                    return RedirectToAction(nameof(IcerikDuzenle), new { id });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Güncelleme sırasında bir hata oluştu: " + ex.Message;
                    return RedirectToAction(nameof(Icerikler));
                }
            }

            return View(model);
        }

        /* KATEGORİ İŞLEMLERİ */


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var kategoriler = _context.Kategoriler.ToList();
            ViewBag.Kategoriler = kategoriler;
            base.OnActionExecuting(context);
        }

        //Kategori Ekleme Fonksiyonu
        public async Task<IActionResult> KategoriEkle(Kategori blogKategori)
        {
            if(blogKategori.kategoriID == 0)
            {
                await _context.AddAsync(blogKategori);
            }
            else
            {
                _context.Update(blogKategori);
            }
            await _context.SaveChangesAsync(); //Yapılan değişiklikleri veritabanına uygula
            return RedirectToAction(nameof(BlogKategori));
        }

        public IActionResult BlogKategori()
        {
            List<Kategori> list = _context.Kategoriler.ToList(); //Tüm kategorileri çekip listeye atıyoruz
            return View(list); //Sonra bu listi sayfaya yolluyoruz
        }

        //Kategori Silme Fonksiyonu
        public async Task<IActionResult> KategoriSil(int? id)
        {
            Kategori blogKategori = await _context.Kategoriler.FindAsync(id);
            _context.Remove(blogKategori);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(BlogKategori));
        }

        //Kategori Düzenleme Fonksiyonu
        public async Task<IActionResult> KategoriDuzenle( int kategoriID)
        {
            var blogKategori = await _context.Kategoriler.FindAsync(kategoriID);
            return Json(blogKategori); 
        }



        /* İÇERİK OLUŞTURMA İŞLEMLERİ */
        [HttpGet]
        public IActionResult Icerik()
        {
            var kategoriler = _context.Kategoriler.ToList();
            ViewBag.Kategoriler = kategoriler;

            return View();
        }

        [HttpPost]
        public IActionResult Icerik(Yazi model, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    // Dosya adını al
                    string fileName = model.icerikBaslik + Path.GetExtension(file.FileName);

                    // Dosyayı wwwroot/uploads dizinine kaydet
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    // Dosya yolu modeldeki gorselYolu alanına atanabilir
                    model.gorselYolu = "/uploads/" + fileName;
                }

                var gelenKategori = _context.Kategoriler.SingleOrDefault(x => x.kategoriID == model.kategoriID);
                model.Kategori = gelenKategori;

                // Modeli veritabanına kaydetme işlemi
                _context.Yazilar.Add(model);
                _context.SaveChanges();

                // Başka bir sayfaya yönlendirme 
                return RedirectToAction("AdminSayfa", "Admin");
            }

            // Eğer ModelState.IsValid false dönerse, hata mesajlarıyla birlikte aynı view'e geri dönülür
            var kategoriler = _context.Kategoriler.ToList();
            ViewBag.Kategoriler = kategoriler;

            return View(model);
        }


        public IActionResult Icerikler()
        {
            List<Yazi> yazilar = _context.Yazilar.ToList();
            return View(yazilar);
        }


        public async Task<IActionResult> IcerikSil(int id)
        {
            Yazi yazi = await _context.Yazilar.FindAsync(id);
            _context.Remove(yazi);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Icerikler));

        }

        public async Task<IActionResult> IcerikYayinDurumu(int id)
        {
            // ID ile yazıyı bul
            Yazi yazi = await _context.Yazilar.FindAsync(id);
            if (yazi == null)
            {
                return NotFound();
            }

            // Durumu tersine çevir
            yazi.yayinDurumu = !yazi.yayinDurumu;

            // Değişiklikleri veritabanına kaydet
            await _context.SaveChangesAsync();

            // İçeriklere yönlendir
            return RedirectToAction(nameof(Icerikler));
        }
    }
}


