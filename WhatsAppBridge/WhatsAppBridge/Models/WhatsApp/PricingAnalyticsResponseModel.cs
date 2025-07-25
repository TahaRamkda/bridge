using WhatsAppBridge.Helpers;

namespace WhatsAppBridge.Models.WhatsApp
{
    public class PricingAnalyticsResponseModel
    {
        public PricingAnalytics pricing_analytics { get; set; }

        public class PricingAnalytics
        {
            public PricingAnalytics()
            {
                data = new List<Datum>();
            }

            public List<Datum> data { get; set; }
        }

        public class Datum
        {
            public Datum()
            {
                data_points = new List<DataPoint>();
            }

            public List<DataPoint> data_points { get; set; }
        }

        public class DataPoint
        {
            public long start { get; set; }
            public long end { get; set; }
            public int conversation { get; set; }
            public string phone_number { get; set; }
            public string pricing_type { get; set; }
            public string pricing_category { get; set; }
            public decimal cost { get; set; }
            public DateTime StartDateUtc => CommonHelper.ConvertFromEpoch(start);
            public DateTime EndDateUtc => CommonHelper.ConvertFromEpoch(end);
        }
    }
}
