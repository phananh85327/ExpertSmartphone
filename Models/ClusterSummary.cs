namespace Backend.Models
{
    public class ClusterSummary
    {
        public ClusterSummary()
        {
            ClusterId = 0;
            Features = new Dictionary<string, object>();
        }

        public int ClusterId { get; set; }
        public Dictionary<string, object> Features { get; set; } = new();
    }
}
