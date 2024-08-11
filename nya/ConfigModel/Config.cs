namespace nya.ConfigModel
{
    internal class Config
    {
        public string Bucket { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public string DestinationFolder { get; set; } = string.Empty;
    }
}
