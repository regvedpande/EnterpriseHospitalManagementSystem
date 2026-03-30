using Hospital.Utilities;
using Hospital.Web.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class AiAssistantController : Controller
    {
        private readonly IAiService _ai;
        private readonly ILogger<AiAssistantController> _log;

        // Max file size: 5 MB
        private const long MaxFileBytes = 5 * 1024 * 1024;

        public AiAssistantController(IAiService ai, ILogger<AiAssistantController> log)
        {
            _ai  = ai;
            _log = log;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.IsConfigured = _ai.IsConfigured;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyseSymptoms(
            [FromForm] string symptoms,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symptoms))
                return BadRequest("Please describe your symptoms.");

            var result = await _ai.ChatAsync(
            [
                new AiMessage("system",
                    "You are MedCoreAI, a patient health assistant. Analyse the described symptoms and provide: " +
                    "1) Possible conditions (not a diagnosis), 2) Immediate self-care tips, " +
                    "3) Whether the patient should seek urgent or routine care. " +
                    "Remind the patient to consult a doctor. Use clear, simple language with bullet points."),
                new AiMessage("user", $"My symptoms: {symptoms}")
            ], ct);

            return Json(new { result });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyseDocument(IFormFile? document, CancellationToken ct)
        {
            if (document == null || document.Length == 0)
                return BadRequest("Please upload a file.");

            if (document.Length > MaxFileBytes)
                return BadRequest("File is too large. Maximum size is 5 MB.");

            var ext = Path.GetExtension(document.FileName).ToLowerInvariant();
            if (ext != ".txt" && ext != ".pdf" && ext != ".csv" && ext != ".md")
                return BadRequest("Supported file types: .txt, .pdf, .csv, .md");

            string text;
            try
            {
                using var stream = document.OpenReadStream();
                if (ext == ".pdf")
                {
                    // Extract text from PDF pages as raw bytes — use simple text extraction
                    text = await ExtractTextFromPdfAsync(stream, ct);
                }
                else
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                    text = await reader.ReadToEndAsync(ct);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[AI] Document read failed for {FileName}", document.FileName);
                return BadRequest("Could not read the file. Please ensure it is not corrupted.");
            }

            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("No readable text found in the document.");

            // Truncate to 8 000 chars to stay within model context
            if (text.Length > 8000)
                text = text[..8000] + "\n\n[Document truncated for analysis]";

            var result = await _ai.AnalyseDocumentTextAsync(text, ct);
            return Json(new { result, fileName = document.FileName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequest req,
            CancellationToken ct)
        {
            if (req?.Messages == null || req.Messages.Count == 0)
                return BadRequest("Messages are required.");

            var aiMessages = req.Messages.Select(m => new AiMessage(m.Role, m.Content));
            var result = await _ai.ChatAsync(aiMessages, ct);
            return Json(new { result });
        }

        // Minimal PDF text extraction — reads ASCII/UTF8 text streams embedded in PDF
        private static async Task<string> ExtractTextFromPdfAsync(Stream stream, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();

            // Extract readable strings from raw PDF bytes (simple heuristic)
            var sb = new StringBuilder();
            var raw = Encoding.Latin1.GetString(bytes);
            int pos = 0;
            while (pos < raw.Length)
            {
                int start = raw.IndexOf("BT", pos, StringComparison.Ordinal);
                if (start < 0) break;
                int end = raw.IndexOf("ET", start, StringComparison.Ordinal);
                if (end < 0) break;
                var block = raw[start..end];
                // Extract text between parentheses (Tj/TJ operators)
                int i = 0;
                while (i < block.Length)
                {
                    int lp = block.IndexOf('(', i);
                    if (lp < 0) break;
                    int rp = block.IndexOf(')', lp);
                    if (rp < 0) break;
                    var word = block[(lp + 1)..rp];
                    // Filter non-printable
                    if (word.All(c => c >= 32 && c < 127))
                        sb.Append(word).Append(' ');
                    i = rp + 1;
                }
                pos = end + 2;
            }

            var result = sb.ToString().Trim();
            // Fall back: return raw printable ASCII if BT/ET blocks not found
            if (string.IsNullOrWhiteSpace(result))
            {
                result = new string(raw.Where(c => c >= 32 && c < 127 || c == '\n' || c == '\r').ToArray());
                result = System.Text.RegularExpressions.Regex.Replace(result, @"\s{3,}", "\n").Trim();
            }
            return result;
        }

        public record ChatMessageDto(string Role, string Content);
        public record ChatRequest(List<ChatMessageDto> Messages);
    }
}
