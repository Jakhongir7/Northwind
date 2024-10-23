namespace Northwind.Middleware
{
    // Configuration options for the middleware
    public class ImageCachingOptions
    {
        public string CacheDirectory { get; set; } = "ImageCache";
        public int MaxCachedImages { get; set; } = 100;
        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(30);
    }
}
