namespace Back.Models
{
    public class RatingRequest
    {
        public RatingRequest()
        {
            Rating = 0;
            Reviews = 0;
        }

        public decimal Rating { get; set; }
        public int Reviews { get; set; }
    }
}
