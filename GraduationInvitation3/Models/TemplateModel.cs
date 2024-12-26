using System.ComponentModel.DataAnnotations;

namespace GraduationInvitation3.Models
{
    public class TemplateModel
    {
        [Required]
        [Display(Name = "Template Image")]
        public IFormFile TemplateFile { get; set; }
    }
}
