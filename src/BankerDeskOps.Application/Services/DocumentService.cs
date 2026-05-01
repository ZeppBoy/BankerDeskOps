using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.IO;

namespace BankerDeskOps.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly string _storagePath;
        private static readonly List<DocumentDto> _documents = new();
        private static readonly object _lock = new();

        public DocumentService(IConfiguration configuration)
        {
            _storagePath = configuration["DocumentStorage:Path"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documents");
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        public Task<DocumentDto?> GetDocumentAsync(Guid documentId)
        {
            lock (_lock)
            {
                var doc = _documents.FirstOrDefault(d => d.Id == documentId);
                return Task.FromResult(doc);
            }
        }

        public Task<IEnumerable<DocumentDto>> GetDocumentsByApplicationAsync(Guid applicationId)
        {
            lock (_lock)
            {
                var docs = _documents.Where(d => d.ApplicationId == applicationId).ToList();
                return Task.FromResult<IEnumerable<DocumentDto>>(docs);
            }
        }

        public async Task<DocumentDto> UploadDocumentAsync(DocumentUploadRequest request, byte[] fileContent, string fileName)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (fileContent == null) throw new ArgumentNullException(nameof(fileContent));
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));

            var documentId = Guid.NewGuid();
            var safeFileName = Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
            var storageFileName = $"{documentId}_{safeFileName}";
            var filePath = Path.Combine(_storagePath, storageFileName);

            await File.WriteAllBytesAsync(filePath, fileContent);

            var documentDto = new DocumentDto
            {
                Id = documentId,
                ApplicationId = request.ApplicationId,
                FileName = safeFileName,
                ContentType = request.ContentType,
                FileSize = fileContent.Length,
                UploadedAt = DateTime.UtcNow
            };

            lock (_lock)
            {
                _documents.Add(documentDto);
            }

            return documentDto;
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId)
        {
            DocumentDto? doc = null;
            lock (_lock)
            {
                doc = _documents.FirstOrDefault(d => d.Id == documentId);
            }

            if (doc == null) return false;

            var filePath = Path.Combine(_storagePath, doc.FileName);
            if (File.Exists(filePath))
                File.Delete(filePath);

            lock (_lock)
            {
                _documents.Remove(doc);
            }

            return true;
        }
    }
}
