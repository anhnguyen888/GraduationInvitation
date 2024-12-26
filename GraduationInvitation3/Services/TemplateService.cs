namespace GraduationInvitation3.Services
{
    // Services/TemplateService.cs
    public class TemplateService : ITemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private string _defaultTemplate;
        private const string TemplateFolder = "templates";

        public TemplateService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _defaultTemplate = GetAvailableTemplates().FirstOrDefault() ?? string.Empty;
        }

        public List<string> GetAvailableTemplates()
        {
            var templatePath = Path.Combine(_environment.WebRootPath, TemplateFolder);
            if (!Directory.Exists(templatePath))
            {
                Directory.CreateDirectory(templatePath);
            }

            return Directory.GetFiles(templatePath, "*.jpg")
                           .Union(Directory.GetFiles(templatePath, "*.png"))
                           .Select(Path.GetFileName)
                           .ToList();
        }

        public string GetDefaultTemplate() => _defaultTemplate;

        public void SetDefaultTemplate(string templateName)
        {
            if (GetAvailableTemplates().Contains(templateName))
            {
                _defaultTemplate = templateName;
            }
        }

        public async Task SaveTemplateAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return;

            var templatePath = Path.Combine(_environment.WebRootPath, TemplateFolder);
            if (!Directory.Exists(templatePath))
            {
                Directory.CreateDirectory(templatePath);
            }

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(templatePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (string.IsNullOrEmpty(_defaultTemplate))
            {
                _defaultTemplate = fileName;
            }
        }
    }
}
