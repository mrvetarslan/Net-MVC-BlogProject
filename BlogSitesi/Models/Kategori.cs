using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogSitesi.Models
{
	public class Kategori
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int kategoriID { get; set; }

        [Required]
        [StringLength(40)]
        public string kategoriAd { get; set; }

        public ICollection<Yazi> Yazilar { get; set; }

    }
}

