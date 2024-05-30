using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogSitesi.Models
{
	public class Kullanici
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int kullaniciID { get; set; }

        [Required]
        [StringLength(50)]
        public string kullaniciAd { get; set; }

        [Required]
        [StringLength(15)]
        public string kullaniciSifre { get; set; }

        [Required]
        [StringLength(25)]
        public string yazarAdSoyad { get; set; }

        public string yazarAciklama { get; set; }

        public string? yazarFotograf { get; set; }

    }
}

