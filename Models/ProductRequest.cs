namespace Back.Models
{
    public class ProductRequest
    {
        public ProductRequest()
        {
            Brands = string.Empty;
            Models = string.Empty;
            Colors = string.Empty;
            Memory = 0;
            Storage = 0;
            Camera = true;
            OriginalPrice = 0;
            Mobile = string.Empty;
            DiscountPercentage = 0;
            OS = string.Empty;
            SellersAmount = 0;
            ScreenSize = 0;
            BatterySize = 0;
        }

        public string Brands { get; set; }
        public string Models { get; set; }
        public string Colors { get; set; }
        public int Memory { get; set; }
        public int Storage { get; set; }
        public bool Camera { get; set; }
        public decimal OriginalPrice { get; set; }
        public string Mobile { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string OS { get; set; }
        public int SellersAmount { get; set; }
        public decimal ScreenSize { get; set; }
        public int BatterySize { get; set; }
    }
}
