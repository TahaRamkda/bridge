namespace WhatsAppBridge.Models.Integration.Flow
{
    public class FlowResponse
    {
        public FlowResponse()
        {
            Responses = new List<Response>();
        }

        public string FlowToken { get; set; }
        public List<Response> Responses { get; set; }

        public class Response
        {
            public string QuestionKey { get; set; } // The key of the question (e.g., "Screen_One_C1_Q")
            public string AnswerKey { get; set; } // The key of the answer (e.g., "Screen_One_C1")
            public string Question { get; set; } // The actual question text
            public string Type { get; set; } // "TextResponse" or "CheckboxResponse"
            public string TextResponse { get; set; } // Only for text answers
            public List<string> CheckboxResponse { get; set; } // Only for checkbox/multiple choice answers
        }
    }
}
