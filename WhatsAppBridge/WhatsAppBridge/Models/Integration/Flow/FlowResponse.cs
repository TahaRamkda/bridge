namespace WhatsAppBridge.Models.Integration.Flow
{
    public class FlowResponse
    {
        public FlowResponse()
        {
            responses = new List<Response>();
        }

        public string flowToken { get; set; }
        public List<Response> responses { get; set; }

        public class Response
        {
            public Response()
            {
                multiSelect = new List<string>();
            }

            public string questionKey { get; set; } // The key of the question (e.g., "Screen_One_C1_Q")
            public string answerKey { get; set; } // The key of the answer (e.g., "Screen_One_C1")
            public string question { get; set; } // The actual question text
            public string type { get; set; } // "TextResponse" or "CheckboxResponse"
            public string text { get; set; } // Only for text answers
            public List<string> multiSelect { get; set; } // Only for checkbox/multiple choice answers
        }
    }
}
