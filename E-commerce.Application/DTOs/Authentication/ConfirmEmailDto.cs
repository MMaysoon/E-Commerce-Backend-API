using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.DTOs.Authentication
{
    public class ConfirmEmailDto
    {
        public string UserId { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}
