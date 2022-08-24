using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Helpers
{
    public class PasswordHelper
    {
        public static string GeneratePassword(bool RequireNonAlphanumeric = true, bool RequireDigit = true, bool RequireLowercase = true, bool RequireUppercase = true, int RequiredLength = 10)
        {
            StringBuilder password = new();
            Random random = new();

            while (password.Length < RequiredLength)
            {
                char c = (char)random.Next(32, 126);

                password.Append(c);

                if (char.IsDigit(c))
                    RequireDigit = false;
                else if (char.IsLower(c))
                    RequireLowercase = false;
                else if (char.IsUpper(c))
                    RequireUppercase = false;
                else if (!char.IsLetterOrDigit(c))
                    RequireNonAlphanumeric = false;
            }

            if (RequireNonAlphanumeric)
                password.Append((char)random.Next(33, 48));
            if (RequireDigit)
                password.Append((char)random.Next(48, 58));
            if (RequireLowercase)
                password.Append((char)random.Next(97, 123));
            if (RequireUppercase)
                password.Append((char)random.Next(65, 91));

            return password.ToString();
        }
    }
}
