using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hospital.Web.Infrastructure.AI
{
    public class NvidiaAiService : IAiService
    {
        private const string BaseUrl = "https://integrate.api.nvidia.com/v1";
        private const string Model   = "meta/llama-3.1-70b-instruct";

        // ── System prompts per role ────────────────────────────────────────────
        private const string SysDoctor =
            "You are MedCoreAI, a clinical decision support assistant for licensed doctors. " +
            "Provide evidence-based differential diagnoses, treatment protocols, medicine recommendations with dosages, " +
            "and early warning indicators. Structure output clearly with markdown headings and bullet points. " +
            "Always note that the final clinical decision rests with the attending physician.";

        private const string SysPharmacist =
            "You are MedCoreAI, a clinical pharmacy assistant for hospital pharmacists. " +
            "Provide precise drug interaction analysis, dosage guidance, contraindications, and therapeutic substitutions. " +
            "Cite relevant drug classes and mechanism of action where helpful. Use bullet points and clear headings.";

        private const string SysNurse =
            "You are MedCoreAI, a nursing support assistant. Help nurses interpret patient vitals, " +
            "build nursing care plans, understand clinical procedures, and identify deteriorating patients. " +
            "Be clear, action-oriented, and concise. Flag any critical values prominently.";

        private const string SysLabTech =
            "You are MedCoreAI, a laboratory medicine assistant for lab technicians. " +
            "Interpret test results with normal reference ranges, flag critical values, " +
            "explain clinical significance, and suggest appropriate follow-up tests. Use tables for ranges where useful.";

        private const string SysReceptionist =
            "You are MedCoreAI, a patient intake triage assistant for hospital receptionists. " +
            "Assess reported symptoms for urgency (Emergency / Urgent / Routine), recommend the appropriate medical department, " +
            "and provide guidance on what information to collect during check-in. Be concise and use bullet points.";

        private const string SysAdmin =
            "You are MedCoreAI, a hospital analytics and management assistant. " +
            "Analyse operational statistics, identify trends and anomalies, and provide strategic recommendations " +
            "to improve hospital performance, patient outcomes, and resource allocation. Use clear headings and bullet points.";

        private const string SysPatient =
            "You are MedCoreAI, a patient health assistant. Help patients understand symptoms and health documents. " +
            "Be empathetic, use simple language, and always advise consulting a doctor for diagnosis or treatment.";

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy   = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly HttpClient _http;
        private readonly ILogger<NvidiaAiService> _log;

        public bool IsConfigured { get; }

        public NvidiaAiService(IHttpClientFactory factory, IConfiguration config, ILogger<NvidiaAiService> log)
        {
            _log = log;
            var key = config["Nvidia:ApiKey"];
            IsConfigured = !string.IsNullOrWhiteSpace(key);
            _http = factory.CreateClient("nvidia");
            _http.BaseAddress = new Uri(BaseUrl);
            if (IsConfigured)
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            if (IsConfigured)
                _log.LogInformation("[AI] NVIDIA NIM configured — model: {Model}", Model);
            else
                _log.LogWarning("[AI] Nvidia:ApiKey not set — AI features disabled");
        }

        // ── Doctor ─────────────────────────────────────────────────────────────
        public Task<string> GetDiagnosisSuggestionAsync(string symptoms, string patientContext, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysDoctor),
                new("user", $"Patient context: {patientContext}\n\nPresenting symptoms: {symptoms}\n\nProvide: differential diagnoses (top 3), recommended investigations, and initial management.")
            ], ct);

        public Task<string> GetMedicineRecommendationAsync(string diagnosis, string patientContext, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysDoctor),
                new("user", $"Patient context: {patientContext}\n\nConfirmed/working diagnosis: {diagnosis}\n\nRecommend first-line and alternative medicines with dosage, frequency, duration, and key precautions.")
            ], ct);

        public Task<string> GetDrugInteractionCheckAsync(string medicineList, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysDoctor),
                new("user", $"Check for clinically significant drug interactions between the following medicines:\n{medicineList}\n\nFor each interaction found: severity (Major/Moderate/Minor), mechanism, clinical effect, and recommended action.")
            ], ct);

        public Task<string> GetEarlySymptomAlertAsync(string symptoms, string patientHistory, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysDoctor),
                new("user", $"Patient history: {patientHistory}\n\nCurrent symptoms: {symptoms}\n\nAnalyse for early warning signs of serious conditions. Identify red flags, time-sensitive diagnoses, and recommended immediate actions.")
            ], ct);

        public Task<string> GetTreatmentPlanAsync(string diagnosis, string patientContext, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysDoctor),
                new("user", $"Patient context: {patientContext}\n\nDiagnosis: {diagnosis}\n\nCreate a comprehensive treatment plan including: immediate management, pharmacotherapy, monitoring parameters, follow-up schedule, and patient education points.")
            ], ct);

        // ── Pharmacist ─────────────────────────────────────────────────────────
        public Task<string> GetPharmacistDrugInteractionAsync(string medicines, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysPharmacist),
                new("user", $"Perform a comprehensive drug interaction check for:\n{medicines}\n\nList all interactions by severity (Major/Moderate/Minor), mechanism of interaction, clinical consequences, and pharmacist recommendations.")
            ], ct);

        public Task<string> GetDosageGuideAsync(string drugName, string patientInfo, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysPharmacist),
                new("user", $"Drug: {drugName}\nPatient info: {patientInfo}\n\nProvide: standard adult dosing, renal/hepatic dose adjustments if applicable, paediatric dosing, maximum dose, administration instructions, and key counselling points.")
            ], ct);

        public Task<string> GetDrugSubstituteAsync(string drugName, string reason, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysPharmacist),
                new("user", $"Drug to substitute: {drugName}\nReason for substitution: {reason}\n\nSuggest suitable therapeutic alternatives with dosage equivalence, cost considerations, and any clinical differences to note.")
            ], ct);

        // ── Nurse ──────────────────────────────────────────────────────────────
        public Task<string> InterpretVitalsAsync(string vitals, string patientContext, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysNurse),
                new("user", $"Patient context: {patientContext}\n\nVitals: {vitals}\n\nInterpret each vital sign, identify abnormal values, assess overall clinical picture, flag any critical values requiring immediate escalation, and recommend nursing interventions.")
            ], ct);

        public Task<string> GetNursingCarePlanAsync(string condition, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysNurse),
                new("user", $"Patient condition: {condition}\n\nCreate a nursing care plan with: nursing diagnoses (NANDA format), patient goals/outcomes, nursing interventions, and evaluation criteria.")
            ], ct);

        // ── Lab Tech ───────────────────────────────────────────────────────────
        public Task<string> InterpretLabResultAsync(string testName, string value, string unit, string patientContext, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysLabTech),
                new("user", $"Test: {testName}\nResult: {value} {unit}\nPatient context: {patientContext}\n\nProvide: normal reference range, interpretation (normal/abnormal/critical), clinical significance, possible causes if abnormal, and recommended follow-up actions.")
            ], ct);

        public Task<string> SuggestTestPanelAsync(string suspectedCondition, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysLabTech),
                new("user", $"Suspected condition / clinical presentation: {suspectedCondition}\n\nRecommend an appropriate test panel. For each test: name, test code, clinical rationale, and urgency (STAT / Routine).")
            ], ct);

        // ── Receptionist ───────────────────────────────────────────────────────
        public Task<string> TriageSymptomsAsync(string symptoms, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysReceptionist),
                new("user", $"Patient reported symptoms: {symptoms}\n\nAssess: (1) Urgency level — Emergency / Urgent / Routine with reasoning, (2) Recommended department/specialist, (3) Immediate actions for reception staff, (4) Key questions to ask the patient.")
            ], ct);

        // ── Admin ──────────────────────────────────────────────────────────────
        public Task<string> AnalyseHospitalStatsAsync(string statsJson, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysAdmin),
                new("user", $"Hospital statistics summary:\n{statsJson}\n\nAnalyse: (1) Key performance insights, (2) Areas of concern or anomalies, (3) Trends worth monitoring, (4) Actionable recommendations for management.")
            ], ct);

        // ── Patient ────────────────────────────────────────────────────────────
        public Task<string> AnalyseDocumentTextAsync(string documentText, CancellationToken ct = default) =>
            ChatAsync([
                new("system", SysPatient),
                new("user", $"Analyse this medical document:\n\n{documentText}\n\nProvide: key health indicators, potential conditions or concerns, and recommended follow-up actions in simple language.")
            ], ct);

        // ── Core chat completion ────────────────────────────────────────────────
        public async Task<string> ChatAsync(IEnumerable<AiMessage> messages, CancellationToken ct = default)
        {
            if (!IsConfigured)
                return "AI service is not configured. Please contact your system administrator.";

            var payload = new
            {
                model       = Model,
                messages    = messages.Select(m => new { role = m.Role, content = m.Content }),
                temperature = 0.4,
                max_tokens  = 1200,
            };

            var content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json");

            try
            {
                using var resp = await _http.PostAsync("/v1/chat/completions", content, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                {
                    _log.LogError("[AI] NVIDIA API error {Status}: {Body}", resp.StatusCode, body);
                    return $"AI service returned an error ({resp.StatusCode}). Please try again.";
                }

                using var doc = JsonDocument.Parse(body);
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No response received.";
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[AI] Request failed");
                return "AI service is temporarily unavailable. Please try again later.";
            }
        }
    }
}
