using System.Collections.Concurrent;

namespace Northwind.Middleware
{
    public class ImageCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ImageCachingOptions _options;
        private readonly ILogger<ImageCachingMiddleware> _logger;

        // Thread-safe dictionary to manage cached images
        private readonly ConcurrentDictionary<string, CachedImageInfo> _cache = new();

        public ImageCachingMiddleware(RequestDelegate next, ImageCachingOptions options, ILogger<ImageCachingMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;

            // Ensure the cache directory exists
            if (!Directory.Exists(_options.CacheDirectory))
            {
                Directory.CreateDirectory(_options.CacheDirectory);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is for an image that might already be cached
            if (TryGetCachedImage(context, out var cachedFilePath))
            {
                // Return the cached image
                _logger.LogInformation($"Serving cached image: {cachedFilePath}");
                await ServeCachedImage(context, cachedFilePath);
                return;
            }

            // Call the next middleware in the pipeline
            var originalResponseStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            // Restore original stream
            memoryStream.Position = 0;
            context.Response.Body = originalResponseStream;

            // Check if the response is an image
            var contentType = context.Response.ContentType;
            if (IsValidImageContentType(contentType))
            {
                var imageBytes = memoryStream.ToArray();

                // Cache the image if valid
                var filePath = CacheImage(context, imageBytes);
                if (filePath != null)
                {
                    _logger.LogInformation($"Image cached: {filePath}");
                }
            }

            // Write the response back to the client
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(context.Response.Body);
        }

        // Check if the content type is an image format we care about
        private bool IsValidImageContentType(string contentType)
        {
            return contentType != null && (contentType.StartsWith("image/jpeg") || contentType.StartsWith("image/png") || contentType.StartsWith("image/bmp"));
        }

        // Cache the image to the file system
        private string CacheImage(HttpContext context, byte[] imageBytes)
        {
            // Generate a unique file name based on the request path
            var fileName = Path.GetFileName(context.Request.Path) + ".cache";
            var filePath = Path.Combine(_options.CacheDirectory, fileName);

            try
            {
                // Ensure we don't exceed the max count
                if (_cache.Count >= _options.MaxCachedImages)
                {
                    CleanupCache();
                }

                // Write the file to the cache directory
                File.WriteAllBytes(filePath, imageBytes);

                // Add the image to the cache dictionary
                _cache[fileName] = new CachedImageInfo
                {
                    FilePath = filePath,
                    LastAccessTime = DateTime.Now
                };

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching image.");
                return null;
            }
        }

        // Serve a cached image
        private async Task ServeCachedImage(HttpContext context, string filePath)
        {
            context.Response.ContentType = GetContentType(filePath);
            var imageBytes = await File.ReadAllBytesAsync(filePath);
            await context.Response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);

            // Update the last access time
            var fileName = Path.GetFileName(filePath);
            if (_cache.TryGetValue(fileName, out var cachedImage))
            {
                cachedImage.LastAccessTime = DateTime.Now;
            }
        }

        // Check if the requested image is cached
        private bool TryGetCachedImage(HttpContext context, out string cachedFilePath)
        {
            var fileName = Path.GetFileName(context.Request.Path) + ".cache";
            cachedFilePath = Path.Combine(_options.CacheDirectory, fileName);

            if (_cache.ContainsKey(fileName) && File.Exists(cachedFilePath))
            {
                return true;
            }

            cachedFilePath = null;
            return false;
        }

        // Clean up old images from the cache if they haven't been accessed within the expiration time
        private void CleanupCache()
        {
            var expiredItems = _cache
                .Where(kvp => (DateTime.Now - kvp.Value.LastAccessTime) > _options.CacheExpiration)
                .ToList();

            foreach (var expiredItem in expiredItems)
            {
                if (File.Exists(expiredItem.Value.FilePath))
                {
                    File.Delete(expiredItem.Value.FilePath);
                }

                _cache.TryRemove(expiredItem.Key, out _);
            }
        }

        // Get content type based on file extension
        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}
