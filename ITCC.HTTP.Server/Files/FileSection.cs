namespace ITCC.HTTP.Server.Files
{
    public class FileSection
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public long MaxFileSize { get; set; } = -1;
    }
}
