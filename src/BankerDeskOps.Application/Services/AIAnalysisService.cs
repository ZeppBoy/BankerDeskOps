using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly ILoanApplicationRepository _applicationRepository;
        private readonly IDocumentService _documentService;

        public AIAnalysisService(
            ILoanApplicationRepository applicationRepository,
            IDocumentService documentService)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        public async Task<AIAnalysisResultDto> AnalyzeApplicationAsync(Guid applicationId)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
                throw new ArgumentException($"Loan application with ID {applicationId} not found.");

            var riskScore = CalculateRiskScore(application);
            var findings = GenerateFindings(application, riskScore);

            return new AIAnalysisResultDto
            {
                ApplicationId = applicationId,
                RiskScore = riskScore,
                RiskLevel = GetRiskLevel(riskScore),
                Findings = findings,
                Recommendation = riskScore >= 0.6m,
                AnalyzedAt = DateTime.UtcNow
            };
        }

        public async Task<DocumentAnalysisDto> AnalyzeDocumentAsync(Guid documentId)
        {
            var document = await _documentService.GetDocumentAsync(documentId);
            if (document == null)
                throw new ArgumentException($"Document with ID {documentId} not found.");

            return new DocumentAnalysisDto
            {
                DocumentId = documentId,
                DocumentType = InferDocumentType(document.FileName),
                IsValid = true,
                ExtractedData = new List<string> { "Document processed successfully" },
                AnalyzedAt = DateTime.UtcNow
            };
        }

        public async Task<RiskAssessmentDto> AssessRiskAsync(RiskAssessmentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var creditScoreFactor = CalculateCreditScoreFactor(request.CreditScore);
            var dtiRatio = request.MonthlyIncome > 0 ? request.MonthlyDebtPayments / request.MonthlyIncome : 1m;
            var employmentFactor = CalculateEmploymentFactor(request.EmploymentStatus);

            var overallRiskScore = (creditScoreFactor * 0.5m) + ((1m - dtiRatio) * 0.3m) + (employmentFactor * 0.2m);

            return new RiskAssessmentDto
            {
                CreditScore = request.CreditScore,
                DebtToIncomeRatio = dtiRatio,
                EmploymentStatus = request.EmploymentStatus,
                OverallRiskScore = overallRiskScore,
                RiskCategory = GetRiskCategory(overallRiskScore)
            };
        }

        private decimal CalculateRiskScore(LoanApplication application)
        {
            decimal amountFactor = 1m;
            if (application.TotalAmount > 500000m)
                amountFactor = 0.7m;
            else if (application.TotalAmount > 200000m)
                amountFactor = 0.85m;

            return Math.Round(amountFactor * ((decimal)(new Random().NextDouble()) * 0.4m + 0.6m), 2);
        }

        private List<string> GenerateFindings(LoanApplication application, decimal riskScore)
        {
            var findings = new List<string>();

            if (application.TotalAmount > 500000m)
                findings.Add("High loan amount detected. Additional verification recommended.");

            if (riskScore < 0.5m)
                findings.Add("Low risk score indicates potential credit issues.");

            if (!string.IsNullOrEmpty(application.RepaymentPlan))
                findings.Add("Repayment plan is defined and structured.");

            return findings;
        }

        private string GetRiskLevel(decimal riskScore)
        {
            return riskScore switch
            {
                >= 0.8m => "Low",
                >= 0.6m => "Medium",
                _ => "High"
            };
        }

        private decimal CalculateCreditScoreFactor(decimal creditScore)
        {
            if (creditScore >= 750) return 1m;
            if (creditScore >= 700) return 0.9m;
            if (creditScore >= 650) return 0.8m;
            if (creditScore >= 600) return 0.7m;
            return 0.5m;
        }

        private decimal CalculateEmploymentFactor(string employmentStatus)
        {
            return employmentStatus.ToLower() switch
            {
                "permanent" or "full-time" => 1m,
                "contract" or "part-time" => 0.8m,
                "self-employed" => 0.7m,
                _ => 0.5m
            };
        }

        private string GetRiskCategory(decimal riskScore)
        {
            return riskScore switch
            {
                >= 0.8m => "Low Risk",
                >= 0.6m => "Medium Risk",
                _ => "High Risk"
            };
        }

        private static string InferDocumentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".pdf" => "PDF Document",
                ".jpg" or ".jpeg" or ".png" => "Image Document",
                ".doc" or ".docx" => "Word Document",
                _ => "Unknown Document Type"
            };
        }
    }
}
