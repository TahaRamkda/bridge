namespace WhatsAppBridge.Models.WhatsApp.Flow
{
    public class FlowByIdResponse
    {
        public FlowByIdResponse()
        {
            categories = new List<string>();
            validation_errors = new List<ValidationError>();
        }

        public string id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public List<string> categories { get; set; }
        public List<ValidationError> validation_errors { get; set; }

        public class ValidationError
        {
            public string error { get; set; }
            public string error_type { get; set; }
            public string message { get; set; }
            public List<Pointer> pointers { get; set; }
            public class Pointer
            {
                public int line_start { get; set; }
                public int line_end { get; set; }
                public int column_start { get; set; }
                public int column_end { get; set; }
                public string path { get; set; }
            }
        }
    }
}
