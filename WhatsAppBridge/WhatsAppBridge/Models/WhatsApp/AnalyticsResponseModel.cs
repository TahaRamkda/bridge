using System;
using System.Collections.Generic;
using WhatsAppBridge.Helpers;

namespace WhatsAppBridge.Models.WhatsApp
{
    public class AnalyticsResponseModel
    {
        public Analytics analytics { get; set; }
    }

    public class Analytics
    {
        public List<string> phone_numbers { get; set; }
        public List<string> country_codes { get; set; }
        public string granularity { get; set; }
        public List<DataPoint> data_points { get; set; }

        public Analytics()
        {
            phone_numbers = new List<string>();
            country_codes = new List<string>();
            data_points = new List<DataPoint>();
        }
    }

    public class DataPoint
    {
        public long start { get; set; }
        public long end { get; set; }
        public long sent { get; set; }
        public long delivered { get; set; }
        public DateTime StartDateUtc => CommonHelper.ConvertFromEpoch(start);
        public DateTime EndDateUtc => CommonHelper.ConvertFromEpoch(end);
    }
}