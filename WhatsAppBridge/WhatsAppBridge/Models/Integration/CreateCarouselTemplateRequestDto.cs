

using static WhatsAppBridge.Models.Integration.CreateMessageTemplateRequestDto;

namespace WhatsAppBridge.Models.Integration
{
    public class CreateCarouselTemplateRequestDto
    {
        public CreateCarouselTemplateRequestDto()
        {
            Cards = new List<CardDto>();
        }

        public string ClientId { get; set; }
        public string SenderNameId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string LanguageCode { get; set; }
        public BodyDto Body { get; set; }
        public List<CardDto> Cards { get; set; }

        public class CardDto
        {
            public CardDto()
            {
                Buttons = new List<ButtonDto>();
            }

            public HeaderDto Header { get; set; }
            public BodyDto Body { get; set; }
            public List<ButtonDto> Buttons { get; set; }
        } 
    }
}
