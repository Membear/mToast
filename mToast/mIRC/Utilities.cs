using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MircSharp
{
    public class Utilities
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.ASCII.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
