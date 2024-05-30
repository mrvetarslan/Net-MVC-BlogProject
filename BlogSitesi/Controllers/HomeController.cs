using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlogSitesi.Models;
using Microsoft.EntityFrameworkCore;
using BlogSitesi.Data;

namespace BlogSitesi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BlogDbContext _context;

    public HomeController(ILogger<HomeController> logger,BlogDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var kategoriler = await _context.Kategoriler.ToListAsync();
        ViewBag.Kategoriler = kategoriler;

        var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(x => x.kullaniciID == 1003);
        ViewBag.Kullanici = kullanici;
        // Son 3 yazıyı tarihlerine göre sıralayarak çekiyoruz
        var sonYazılar = _context.Yazilar.OrderByDescending(y => y.yayinTarihi).Take(3).ToList();

        return View(sonYazılar);

    }

    public async Task<IActionResult> Hakkimda()
    {
        var kategoriler = await _context.Kategoriler.ToListAsync();
        ViewBag.Kategoriler = kategoriler;

        return View();
    }



    [HttpGet]
    public async Task<IActionResult> IceriklerList(int kategoriId)
    {
        var kategoriler = await _context.Kategoriler.ToListAsync();
        ViewBag.Kategoriler = kategoriler;

        // Belirli kategoriye göre yazıları getir, en son eklenen en üstte olacak şekilde sırala
        var yazilar = await _context.Yazilar
                                    .Where(y => y.kategoriID == kategoriId && y.yayinDurumu == true)
                                    .OrderByDescending(y => y.yayinTarihi) // En son eklenen en üstte olacak şekilde sırala
                                    .Select(y => new Yazi
                                    {
                                        icerikID = y.icerikID,
                                        icerikBaslik = y.icerikBaslik,
                                        icerikAciklama = y.icerikAciklama,
                                        gorselYolu = y.gorselYolu,
                                        yayinTarihi = y.yayinTarihi, // Tarih alanını da ekle
                                        Kategori = y.Kategori
                                    })
                                    .ToListAsync();

        // Kategori adını al
        var kategoriAd = await _context.Kategoriler
                                       .Where(k => k.kategoriID == kategoriId)
                                       .Select(k => k.kategoriAd)
                                       .FirstOrDefaultAsync();

        // ViewBag ile kategori adını view'a aktar
        ViewBag.kategoriAd = kategoriAd;

        return View(yazilar);
    }



    public async Task<IActionResult> IcerikDetay(int icerikId)
    {
        var kategoriler = await _context.Kategoriler.ToListAsync();
        ViewBag.Kategoriler = kategoriler;

        // Belirli kategoriye göre yazıları getir
        var yazi = _context.Yazilar.FirstOrDefault(x => x.icerikID == icerikId);
        ViewBag.Yazilar = yazi;


        if (yazi == null)
        {
            return NotFound(); // İçerik bulunamadı durumunda 404 Not Found döndürülür.
        }
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


}

