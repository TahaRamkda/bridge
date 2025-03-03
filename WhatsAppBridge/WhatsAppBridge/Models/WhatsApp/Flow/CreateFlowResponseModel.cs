namespace WhatsAppBridge.Models.WhatsApp.Flow
{
    public class CreateFlowResponseModel
    {
        public CreateFlowResponseModel()
        {
            validation_errors = new List<ValidationError>();
        }

        public string id { get; set; }
        public bool success { get; set; }
        public Error error { get; set; }
        public List<ValidationError> validation_errors { get; set; }

        public class Error
        {
            public string message { get; set; }
            public string type { get; set; }
            public string code { get; set; }
            public string error_subcode { get; set; }
            public string error_user_title { get; set; }
            public string error_user_msg { get; set; }
            public string fbtrace_id { get; set; }
        }

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
