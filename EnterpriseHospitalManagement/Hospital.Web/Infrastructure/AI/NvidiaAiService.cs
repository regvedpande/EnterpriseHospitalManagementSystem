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

        private static readonly string DoctorSystemPrompt =
            "You are MedCoreAI, a clinical decision support assistant embedded in a hospital management system. " +
            "You help licensed doctors by analysing symptoms, suggesting differential diagnoses, and outlining " +
            "evidence-based treatment options. Always remind the doctor that final clinical decisions rest with them. " +
            "Be concise, structured, and medically accurate. Use markdown headings and bullet points.";

        private static readonly string PatientSystemPrompt =
            "You are MedCoreAI, a patient health assistant in a hospital management system. " +
            "You help patients understand their health documents, identify potential conditions from described symptoms, " +
            "and suggest when to seek medical care. Always advise the patient to consult a qualified doctor. " +
            "Be empathetic, clear, and avoid overly technical jargon. Use bullet points and simple headings.";

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly HttpClient _http;
        private readonly ILogger<NvidiaAiService> _log;

        public bool IsConfigured { get; }

        public NvidiaAiService(IHttpClientFactory factory, IConfiguration config, ILogger<NvidiaAiService> log)
        {
            _log  = log;
            var key = config["Nvidia:ApiKey"];
            IsConfigured = !string.IsNullOrWhiteSpace(key);

            _http = factory.CreateClient("nvidia");
            _http.BaseAddress = new Uri(BaseUrl);
            if (IsConfigured)
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            if (IsConfigured)
                _log.LogInformation("[AI] NVIDIA NIM configured — model: {Model}", Model);
            else
                _log.LogWarning("[AI] Nvidia:ApiKey not set — AI features will be disabled");
        }

        public async Task<string> GetDiagnosisSuggestionAsync(string symptoms, string patientContext, CancellationToken ct = default)
        {
            var userContent = string.IsNullOrWhiteSpace(patientContext)
                ? $"Patient symptoms: {symptoms}"
                : $"Patient context: {patientContext}\n\nSymptoms: {symptoms}";

            return await ChatAsync(
            [
                new AiMessage("system", DoctorSystemPrompt),
                new AiMessage("user", userContent)
            ], ct);
        }

        public async Task<string> AnalyseDocumentTextAsync(string documentText, CancellationToken ct = default)
        {
            var prompt = $"Please analyse the following medical document text and identify:\n" +
                         $"1. Key health indicators\n2. Potential conditions or concerns\n" +
                         $"3. Recommended follow-up actions\n\nDocument content:\n{documentText}";

            return await ChatAsync(
            [
                new AiMessage("system", PatientSystemPrompt),
                new AiMessage("user", prompt)
            ], ct);
        }

        public async Task<string> ChatAsync(IEnumerable<AiMessage> messages, CancellationToken ct = default)
        {
            if (!IsConfigured)
                return "AI service is not configured. Please contact your system administrator.";

            var payload = new
            {
                model    = Model,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }),
                temperature = 0.5,
                max_tokens  = 1024,
            };

            var json    = JsonSerializer.Serialize(payload, JsonOpts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var resp = await _http.PostAsync("/v1/chat/completions", content, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                {
                    _log.LogError("[AI] NVIDIA API error {Status}: {Body}", resp.StatusCode, body);
                    return $"AI service returned an error ({resp.StatusCode}). Please try again later.";
                }

                using var doc    = JsonDocument.Parse(body);
                var message = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return message ?? "No response received from AI.";
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[AI] Request failed");
                return "AI service is temporarily unavailable. Please try again later.";
            }
        }
    }
}
