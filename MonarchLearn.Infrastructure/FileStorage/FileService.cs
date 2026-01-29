using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MonarchLearn.Application.Interfaces.Services;
using FFMpegCore;

namespace MonarchLearn.Infrastructure.FileStorage
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor; // ✅ Əlavə olundu

        // ✅ Allowed extensions
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".webm" };

        public FileService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        // ✅ Mərkəzi URL metodu
        public string GetFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;

            var request = _httpContextAccessor.HttpContext?.Request;

            // Əgər HTTP context yoxdursa (məsələn Background Job işləyirsə), yolu olduğu kimi qaytar
            if (request == null) return relativePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Linkin əvvəlində / yoxdursa əlavə edirik
            var path = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;

            return $"{baseUrl}{path}";
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            ValidateExtension(fileExtension, folderName);
            ValidateSize(file.Length, folderName);

            string filePath = await SaveFileToDisk(file, folderName);

            // URL qaytarır: /uploads/folder/filename
            return filePath.Replace(_env.WebRootPath, "").Replace("\\", "/");
        }

        public async Task<(string FilePath, int Duration)> UploadVideoWithDurationAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return (null, 0);

            string folderName = "videos";
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            ValidateExtension(fileExtension, folderName);
            ValidateSize(file.Length, folderName);

            // 1. Faylı fiziki olaraq diskə yazırıq
            string fullPath = await SaveFileToDisk(file, folderName);

            try
            {
                // 2. FFProbe vasitəsilə videonu analiz edirik
                var videoInfo = await FFProbe.AnalyseAsync(fullPath);
                int duration = (int)videoInfo.Duration.TotalSeconds;

                // 3. Nisbi yolu (URL) hazırlayırıq
                string relativePath = fullPath.Replace(_env.WebRootPath, "").Replace("\\", "/");
                if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;

                return (relativePath, duration);
            }
            catch (Exception ex)
            {
                // FFmpeg quraşdırılmayıbsa və ya xəta verərsə, müddəti 0 qaytarırıq
                string relativePath = fullPath.Replace(_env.WebRootPath, "").Replace("\\", "/");
                if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;

                return (relativePath, 0);
            }
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            string fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        // --- Köməkçi Metodlar ---

        private void ValidateExtension(string extension, string folderName)
        {
            if (folderName == "videos")
            {
                if (!AllowedVideoExtensions.Contains(extension))
                    throw new ArgumentException($"Invalid video type. Allowed: {string.Join(", ", AllowedVideoExtensions)}");
            }
            else
            {
                if (!AllowedImageExtensions.Contains(extension))
                    throw new ArgumentException($"Invalid image type. Allowed: {string.Join(", ", AllowedImageExtensions)}");
            }
        }

        private void ValidateSize(long fileSize, string folderName)
        {
            long maxSizeBytes = folderName == "videos" ? 500 * 1024 * 1024 : 50 * 1024 * 1024;
            if (fileSize > maxSizeBytes)
                throw new ArgumentException($"File size exceeds {maxSizeBytes / (1024 * 1024)}MB limit");
        }

        private async Task<string> SaveFileToDisk(IFormFile file, string folderName)
        {
            //  wwwroot içində uploads/folder yaradırıq
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return filePath;
        }
    }
}