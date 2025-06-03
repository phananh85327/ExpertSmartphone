namespace Backend.Models
{
    public class Product
    {
        public Product()
        {
            ProductID = 0;
            Brands = string.Empty;
            Models = string.Empty;
            Colors = string.Empty;
            Memory = 0;
            Storage = 0;
            Camera = true;
            Rating = 0;
            SellingPrice = 0;
            OriginalPrice = 0;
            Mobile = string.Empty;
            Discount = 0;
            DiscountPercentage = 0;
            OS = string.Empty;
            SellersAmount = 0;
            ScreenSize = 0;
            BatterySize = 0;
            Reviews = 0;
        }

        public int ProductID { get; set; }
        public string Brands { get; set; }
        public string Models { get; set; }
        public string Colors { get; set; }
        public int Memory { get; set; }
        public int Storage { get; set; }
        public bool Camera { get; set; }
        public decimal Rating { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public string Mobile { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string OS { get; set; }
        public int SellersAmount { get; set; }
        public decimal ScreenSize { get; set; }
        public int BatterySize { get; set; }
        public int Reviews { get; set; }
    }
}
