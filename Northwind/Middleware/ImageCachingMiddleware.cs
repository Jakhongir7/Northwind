using System.Collections.Concurrent;

namespace Northwind.Middleware
{
    public class ImageCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ImageCachingMiddleware> _logger;
        private readonly ImageCachingOptions _options;
        private readonly Dictionary<string, DateTime> _cacheTracker = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public ImageCachingMiddleware(RequestDelegate next, ILogger<ImageCachingMiddleware> logger, ImageCachingOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;

            // Ensure the cache directory exists
            if (!Directory.Exists(_options.CacheDirectory))
            {
                Directory.CreateDirectory(_options.CacheDirectory);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Before the response is sent, capture it to inspect and potentially cache images
            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Proceed with the request pipeline
            await _next(context);

            // Check if the content type is an image format
            if (context.Response.ContentType != null && IsImageContentType(context.Response.ContentType))
            {
                var cacheKey = GetCacheKey(context.Request.Path);

                // Save the image to the cache if it doesn't already exist
                if (!IsImageCached(cacheKey))
                {
                    await SaveImageToCache(cacheKey, memoryStream.ToArray());
                    CleanExpiredCache();
                }

                // Write the response back to the original body
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            else
            {
                // Write the response back to the original body if it's not an image
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }

        private bool IsImageContentType(string contentType)
        {
            // Check for common image content types
            return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        private string GetCacheKey(string path)
        {
            // Generate a cache key based on the request path
            return Path.GetFileName(path);
        }

        private bool IsImageCached(string cacheKey)
        {
            var cachePath = Path.Combine(_options.CacheDirectory, cacheKey);
            return File.Exists(cachePath);
        }

        private async Task SaveImageToCache(string cacheKey, byte[] imageBytes)
        {
            await _semaphore.WaitAsync();
            try
            {
                // Enforce max cached images count
                if (Directory.GetFiles(_options.CacheDirectory).Length >= _options.MaxCachedImages)
                {
                    CleanOldestCachedImage();
                }

                var cachePath = Path.Combine(_options.CacheDirectory, cacheKey);
                await File.WriteAllBytesAsync(cachePath, imageBytes);
                _cacheTracker[cacheKey] = DateTime.UtcNow;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void CleanExpiredCache()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _cacheTracker
                .Where(kvp => (now - kvp.Value) > _options.CacheExpiration)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                DeleteCachedImage(key);
            }
        }

        private void CleanOldestCachedImage()
        {
            var oldestKey = _cacheTracker.OrderBy(kvp => kvp.Value).FirstOrDefault().Key;
            if (oldestKey != null)
            {
                DeleteCachedImage(oldestKey);
            }
        }

        private void DeleteCachedImage(string cacheKey)
        {
            var cachePath = Path.Combine(_options.CacheDirectory, cacheKey);
            if (File.Exists(cachePath))
            {
                File.Delete(cachePath);
                _cacheTracker.Remove(cacheKey);
            }
        }
    }
}
