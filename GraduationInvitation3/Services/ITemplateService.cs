namespace GraduationInvitation3.Services
{
    public interface ITemplateService
    {
        List<string> GetAvailableTemplates();
        string GetDefaultTemplate();
        void SetDefaultTemplate(string templateName);
        Task SaveTemplateAsync(IFormFile file);
    }
}
