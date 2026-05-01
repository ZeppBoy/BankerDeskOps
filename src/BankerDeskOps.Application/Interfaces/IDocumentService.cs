using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentDto?> GetDocumentAsync(Guid documentId);
        Task<IEnumerable<DocumentDto>> GetDocumentsByApplicationAsync(Guid applicationId);
        Task<DocumentDto> UploadDocumentAsync(DocumentUploadRequest request, byte[] fileContent, string fileName);
        Task<bool> DeleteDocumentAsync(Guid documentId);
    }

    public class DocumentDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class DocumentUploadRequest
    {
        public Guid ApplicationId { get; set; }
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }
}
