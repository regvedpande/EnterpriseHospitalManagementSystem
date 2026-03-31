using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IAiAssistantService
    {
        AiAssistantPageViewModel Build(AiAssistantRole role, string userId, string? userName, string? prompt);
    }
}
