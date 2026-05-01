using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IAIAnalysisService
    {
        Task<AIAnalysisResultDto> AnalyzeApplicationAsync(Guid applicationId);
        Task<DocumentAnalysisDto> AnalyzeDocumentAsync(Guid documentId);
        Task<RiskAssessmentDto> AssessRiskAsync(RiskAssessmentRequest request);
    }

    public class AIAnalysisResultDto
    {
        public Guid ApplicationId { get; set; }
        public decimal RiskScore { get; set; }
        public string RiskLevel { get; set; } = null!;
        public List<string> Findings { get; set; } = new();
        public bool Recommendation { get; set; }
        public DateTime AnalyzedAt { get; set; }
    }

    public class DocumentAnalysisDto
    {
        public Guid DocumentId { get; set; }
        public string DocumentType { get; set; } = null!;
        public bool IsValid { get; set; }
        public List<string> ExtractedData { get; set; } = new();
        public DateTime AnalyzedAt { get; set; }
    }

    public class RiskAssessmentDto
    {
        public decimal CreditScore { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public string EmploymentStatus { get; set; } = null!;
        public decimal OverallRiskScore { get; set; }
        public string RiskCategory { get; set; } = null!;
    }

    public class RiskAssessmentRequest
    {
        public Guid ApplicationId { get; set; }
        public decimal CreditScore { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyDebtPayments { get; set; }
        public string EmploymentStatus { get; set; } = null!;
    }
}
