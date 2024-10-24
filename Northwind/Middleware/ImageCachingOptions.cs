namespace Northwind.Middleware
{
    // Configuration options for the middleware
    public class ImageCachingOptions
    {
        public string CacheDirectory { get; set; } = "ImageCache";
        public int MaxCachedImages { get; set; } = 50; // Default max cached images
        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(10); // Default cache expiration time
    }
}
