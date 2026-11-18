using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Settings
{
    public class EmailSettings
    {
        public string Host { get; set; } = default!;

        public int Port { get; set; }

        public string UserName { get; set; } = default!;

        public string Password { get; set; } = default!;

        public string FromEmail { get; set; } = default!;

        public string FromName { get; set; } = default!;
    }
}
