namespace Hospital.ViewModels
{
    public class PortalBootstrapViewModel
    {
        public PortalUserViewModel User { get; set; } = new();
        public PortalDashboardViewModel Dashboard { get; set; } = new();
        public AiAssistantPageViewModel? AiAssistant { get; set; }
        public List<PortalNavigationItemViewModel> Navigation { get; set; } = new();
    }

    public class PortalUserViewModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string RoleDisplay { get; set; } = "";
        public string DefaultRoute { get; set; } = "/dashboard";
        public string Initials { get; set; } = "U";
    }

    public class PortalDashboardViewModel
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public List<PortalMetricViewModel> Metrics { get; set; } = new();
        public List<PortalChartViewModel> Charts { get; set; } = new();
        public List<PortalRecentItemViewModel> RecentItems { get; set; } = new();
    }

    public class PortalMetricViewModel
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Icon { get; set; } = "fa-chart-simple";
        public string Tone { get; set; } = "blue";
        public string? Description { get; set; }
    }

    public class PortalChartViewModel
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();
    }

    public class PortalRecentItemViewModel
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Meta { get; set; } = "";
        public string Status { get; set; } = "";
        public string Icon { get; set; } = "fa-circle";
        public string Tone { get; set; } = "blue";
    }

    public class PortalNavigationItemViewModel
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string Route { get; set; } = "";
        public string Icon { get; set; } = "fa-circle";
    }

    public class PortalAiPromptRequest
    {
        public string Prompt { get; set; } = "";
    }
}
