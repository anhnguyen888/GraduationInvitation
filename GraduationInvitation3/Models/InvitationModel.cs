using System.ComponentModel.DataAnnotations;

namespace GraduationInvitation3.Models
{
    public class InvitationModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Tên người mời")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
        [Display(Name = "Lời nhắn gửi")]
        public string Message { get; set; }

        public string SelectedTemplate { get; set; }
    }
}
