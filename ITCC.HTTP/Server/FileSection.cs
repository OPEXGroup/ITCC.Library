namespace ITCC.HTTP.Server
{
    public class FileSection
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public long MaxFileSize { get; set; } = -1;
    }
}
