namespace Backend.Models
{
    public class PrologInput
    {
        public string Brands { get; set; }
        public string Colors { get; set; }
        public int Memory { get; set; }
        public int Storage { get; set; }
        public decimal Rating { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string OS { get; set; }
        public int SellersAmount { get; set; }
        public decimal ScreenSize { get; set; }
        public int BatterySize { get; set; }
        public int Reviews { get; set; }
    }
}
