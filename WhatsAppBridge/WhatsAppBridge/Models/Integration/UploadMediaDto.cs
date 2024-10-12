namespace WhatsAppBridge.Models.Integration
{
    public class UploadMediaDto
    {
        public UploadMediaDto()
        {
            Medias = new List<MediaDto>();
        }

        public string PhoneId { get; set; }
        public List<MediaDto> Medias { get; set; }

        public class MediaDto
        {
            public string Id { get; set; }
            public string Url { get; set; }  
        }
    }
}
