namespace Northwind.Middleware
{
    // Simple class to manage cached image metadata
    public class CachedImageInfo
    {
        public string FilePath { get; set; }
        public DateTime LastAccessTime { get; set; }
    }
}
