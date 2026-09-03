using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon
{
    /// <summary>
    /// Provides string extension methods for logging.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to Base64 format for log output.
        /// Returns the original value when it is null or empty.
        /// </summary>
        /// <param name="value">The source string.</param>
        /// <returns>Base64 wrapped with brackets, or the original value if null or empty.</returns>
        public static string GCommonLogBase64(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            return string.Format("[{0}]", base64);
        }
    }
}
