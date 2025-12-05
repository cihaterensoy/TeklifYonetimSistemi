using System;
namespace TeklifYonetimSistemi.Models.ViewModels
{
	public class DashboardViewModel
	{
		//genel istatistik
		public int ToplamMusteri { get; set; }
		public int ToplamProje { get; set; }
		public int ToplamUrun { get; set; }

		//admin
		public decimal ToplamCiro { get; set; }
		public int YoneticiOnayiBekleyen { get; set; }
		public int KritikStokSayisi { get; set; }

		//satiş
		public int RevizeGerekenler { get; set; }
		public int MusteriOnayiBekleyen { get; set; }
		public int KapananSatislarim { get; set; }

		//müşteri
		public int OnayimiBekleyenler { get; set; }
		public int AktifProjelerim { get; set; }


		public decimal ToplamNetKar { get; set; }
		public decimal TeklifBsariOrani { get; set; }//yüzde oranıyla vereceğim 
		public int PasifMusteriSayisi { get; set; }//6 aydan eski işlem görmeyen kullanıcı sayisi


		//Kategori Analizi(Kategori Adı -> Satış Adedi/Tutarı)

		public Dictionary<string, int> KategoriDagilimi { get; set; }

	}
}

