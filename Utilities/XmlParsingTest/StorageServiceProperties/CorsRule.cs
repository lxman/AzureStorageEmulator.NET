namespace XmlParsingTest.StorageServiceProperties
{
    public class CorsRule
    {
        public string AllowedOrigins { get; set; }
        public string AllowedMethods { get; set; }
        public string AllowedHeaders { get; set; }
        public string ExposedHeaders { get; set; }
        public int MaxAgeInSeconds { get; set; }
    }
}