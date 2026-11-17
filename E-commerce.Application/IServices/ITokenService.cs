using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.IServices
{
    public interface ITokenService
    {
        string CreateToken(string userId, string email, string userName, IEnumerable<string> roles);
    }
}
