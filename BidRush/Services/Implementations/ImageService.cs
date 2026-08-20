using BidRush.DTOs.Response;
using BidRush.Services.Interfaces;
using BidRush.Settings;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;

namespace BidRush.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ImageStorageSettings _imageStorageSettings;
        public ImageService(IWebHostEnvironment environment, IOptions<ImageStorageSettings> imageStorageSettings)
        {
            _environment = environment;
            _imageStorageSettings = imageStorageSettings.Value;
        }

        #region Save Image
        public async Task<ImagePathResponseDto> SaveAuctionImageAsync(IFormFile image)
        {
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (extension != ".jpg" &&
                extension != ".jpeg" &&
                extension != ".png")
            {
                return new ImagePathResponseDto
                {
                    ResponseCode = 400,
                    ResponseMessage = "Invalid image format"
                };
            }

            if (image.Length > _imageStorageSettings.MaxFileSizeBytes)
            {
                return new ImagePathResponseDto
                {
                    ResponseCode = 400,
                    ResponseMessage = "Image size exceeds limit"
                };
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(
               _environment.WebRootPath,
               _imageStorageSettings.AuctionsFolder);

            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var relativePath = Path.Combine(_imageStorageSettings.AuctionsFolder, fileName);

            return new ImagePathResponseDto
            {
                ResponseCode = 200,
                ResponseMessage = "Image saved successfully",
                FilePath = relativePath.Replace("\\", "/")
            };
        }
        #endregion

        #region Get Image
        public Task<ImagePathResponseDto> GetImageAsync(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.FromResult(
                    new ImagePathResponseDto
                    {
                        ResponseCode = 404,
                        ResponseMessage = "Image path not found."
                    });
            }

            var fullPath = Path.Combine(_environment.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(fullPath))
            {
                return Task.FromResult(
                    new ImagePathResponseDto
                    {
                        ResponseCode = 404,
                        ResponseMessage = "Image file not found."
                    });
            }

            return Task.FromResult(
                new ImagePathResponseDto
                {
                    ResponseCode = 200,
                    ResponseMessage = "Image found successfully.",
                    FilePath = relativePath
                });
        }

        #endregion
    }
}
