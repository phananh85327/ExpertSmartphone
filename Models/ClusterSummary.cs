namespace Backend.Models
{
    public class ClusterSummary
    {
        public ClusterSummary()
        {
            ClusterId = 0;
            Features = null;
        }

        public int ClusterId { get; set; }
        public Dictionary<string, object> Features { get; set; } = new();
    }
}
