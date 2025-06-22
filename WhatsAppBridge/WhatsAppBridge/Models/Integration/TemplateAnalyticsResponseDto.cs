using Newtonsoft.Json;
using System.Collections.Generic;

namespace WhatsAppBridge.Models.Integration
{
    public class TemplateAnalyticsResponseDto
    {
        [JsonProperty("data")]
        public List<DataItem> Data { get; set; }

        [JsonProperty("paging")]
        public Paging Paging { get; set; }
    }

    public class DataItem
    {
        [JsonProperty("granularity")]
        public string Granularity { get; set; }

        [JsonProperty("product_type")]
        public string ProductType { get; set; }

        [JsonProperty("data_points")]
        public List<DataPoint> DataPoints { get; set; }
    }

    public class DataPoint
    {
        [JsonProperty("template_id")]
        public string TemplateId { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }

        [JsonProperty("sent")]
        public int Sent { get; set; }

        [JsonProperty("delivered")]
        public int Delivered { get; set; }

        [JsonProperty("read")]
        public int Read { get; set; }

        [JsonProperty("cost")]
        public List<Cost> Cost { get; set; }

        // Optional: include if "clicked" might appear in future responses
        [JsonProperty("clicked")]
        public List<Clicked> Clicked { get; set; }
    }

    public class Cost
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public double? Value { get; set; }
    }

    public class Clicked
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("button_content")]
        public string ButtonContent { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class Paging
    {
        [JsonProperty("cursors")]
        public Cursors Cursors { get; set; }
    }

    public class Cursors
    {
        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }
    }
}
