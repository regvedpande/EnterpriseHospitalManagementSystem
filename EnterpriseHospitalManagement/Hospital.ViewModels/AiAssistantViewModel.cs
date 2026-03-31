using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public enum AiAssistantRole
    {
        Doctor,
        Pharmacist,
        Nurse,
        LabTech,
        Receptionist,
        Admin,
        Patient
    }

    public class AiAssistantPageViewModel
    {
        public AiAssistantRole Role { get; set; }
        public string AreaName { get; set; } = "";
        public string PageTitle { get; set; } = "";
        public string SidebarTitle { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string PromptLabel { get; set; } = "Ask the assistant";
        public string PromptPlaceholder { get; set; } = "";
        public string SubmitLabel { get; set; } = "Generate guidance";
        public string Prompt { get; set; } = "";
        public string ResponseTitle { get; set; } = "";
        public string ResponseSummary { get; set; } = "";
        public string ResponseStatusLabel { get; set; } = "Ready";
        public string ResponseStatusTone { get; set; } = "info";
        public string Disclaimer { get; set; } = "";
        public List<string> Capabilities { get; set; } = new();
        public List<string> SuggestedPrompts { get; set; } = new();
        public List<string> LiveInsights { get; set; } = new();
        public List<string> DetectedSignals { get; set; } = new();
        public List<AiAssistantMetricViewModel> Metrics { get; set; } = new();
        public List<AiAssistantFactViewModel> ResponseFacts { get; set; } = new();
        public List<AiAssistantSectionViewModel> Sections { get; set; } = new();
    }

    public class AiAssistantMetricViewModel
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Icon { get; set; } = "fa-sparkles";
        public string Tone { get; set; } = "blue";
        public string? Description { get; set; }
    }

    public class AiAssistantSectionViewModel
    {
        public string Title { get; set; } = "";
        public List<string> Items { get; set; } = new();
    }

    public class AiAssistantFactViewModel
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class AiAssistantPromptInputModel
    {
        [Display(Name = "Prompt")]
        public string? Prompt { get; set; }
    }
}
