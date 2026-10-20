using System.ComponentModel.DataAnnotations;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.DTOs.Auth
{
    public class GoogleAuthRequestDto
    {
        [Required(ErrorMessage = "Google ID Token là bắt buộc")]
        public string IdToken { get; set; } = string.Empty;

        public UserRoleEnum Role { get; set; } = UserRoleEnum.Student;
    }
}
