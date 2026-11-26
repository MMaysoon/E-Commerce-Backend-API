using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.DTOs.Authentication
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = default!;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = default!;
    }
}
