namespace Hospital.Web.Infrastructure.AI
{
    public record AiMessage(string Role, string Content);

    public interface IAiService
    {
        // ── Doctor ────────────────────────────────────────────────────────────
        Task<string> GetDiagnosisSuggestionAsync(string symptoms, string patientContext, CancellationToken ct = default);
        Task<string> GetMedicineRecommendationAsync(string diagnosis, string patientContext, CancellationToken ct = default);
        Task<string> GetDrugInteractionCheckAsync(string medicineList, CancellationToken ct = default);
        Task<string> GetEarlySymptomAlertAsync(string symptoms, string patientHistory, CancellationToken ct = default);
        Task<string> GetTreatmentPlanAsync(string diagnosis, string patientContext, CancellationToken ct = default);

        // ── Pharmacist ────────────────────────────────────────────────────────
        Task<string> GetPharmacistDrugInteractionAsync(string medicines, CancellationToken ct = default);
        Task<string> GetDosageGuideAsync(string drugName, string patientInfo, CancellationToken ct = default);
        Task<string> GetDrugSubstituteAsync(string drugName, string reason, CancellationToken ct = default);

        // ── Nurse ─────────────────────────────────────────────────────────────
        Task<string> InterpretVitalsAsync(string vitals, string patientContext, CancellationToken ct = default);
        Task<string> GetNursingCarePlanAsync(string condition, CancellationToken ct = default);

        // ── Lab Tech ──────────────────────────────────────────────────────────
        Task<string> InterpretLabResultAsync(string testName, string value, string unit, string patientContext, CancellationToken ct = default);
        Task<string> SuggestTestPanelAsync(string suspectedCondition, CancellationToken ct = default);

        // ── Receptionist ──────────────────────────────────────────────────────
        Task<string> TriageSymptomsAsync(string symptoms, CancellationToken ct = default);

        // ── Admin ─────────────────────────────────────────────────────────────
        Task<string> AnalyseHospitalStatsAsync(string statsJson, CancellationToken ct = default);

        // ── Patient ───────────────────────────────────────────────────────────
        Task<string> AnalyseDocumentTextAsync(string documentText, CancellationToken ct = default);

        // ── Universal ─────────────────────────────────────────────────────────
        Task<string> ChatAsync(IEnumerable<AiMessage> messages, CancellationToken ct = default);

        bool IsConfigured { get; }
    }
}
