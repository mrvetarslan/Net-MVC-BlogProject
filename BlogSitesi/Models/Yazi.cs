using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogSitesi.Models
{
	public class Yazi
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int icerikID { get; set; } //Burda bir sıkıntı var

        public string? icerikBaslik { get; set; }

        public string? icerikAciklama { get; set; }

        public string? icerik { get; set; }

        public string? gorselYolu { get; set; }

        public bool? yayinDurumu { get; set; } = false;

        public DateTime? yayinTarihi { get; set; }

        public int? kategoriID { get; set; }

        [ForeignKey("kategoriID")]
        public Kategori? Kategori { get; set; }
    }
}

