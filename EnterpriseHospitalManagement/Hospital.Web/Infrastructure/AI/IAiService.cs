namespace Hospital.Web.Infrastructure.AI
{
    public record AiMessage(string Role, string Content);

    public interface IAiService
    {
        /// <summary>Doctor-oriented: diagnose symptoms and suggest treatment plan.</summary>
        Task<string> GetDiagnosisSuggestionAsync(string symptoms, string patientContext, CancellationToken ct = default);

        /// <summary>General chat completion — send full conversation history.</summary>
        Task<string> ChatAsync(IEnumerable<AiMessage> messages, CancellationToken ct = default);

        /// <summary>Analyse extracted text from a patient document for disease indicators.</summary>
        Task<string> AnalyseDocumentTextAsync(string documentText, CancellationToken ct = default);

        bool IsConfigured { get; }
    }
}
