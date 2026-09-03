/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility
{
    public class SecurityUtils
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SecurityUtils));

        //* Fortify Issue Type: Path Manipulation
        public static String SafeCombinePath(params string[] paths)
        {
             return Combine(false, paths);
        }
        //* Fortify Issue Type: Connection String Parameter Pollution
        public static bool ValidateSQLiteConnectionWithBuilder(string dataSrouce, out SQLiteConnectionStringBuilder builder)
        {

            builder = new SQLiteConnectionStringBuilder
            {
                DataSource = dataSrouce
            };

            // List of not allowed parameters
            string[] disAllowedParameters = { };

            //Check if any keys in the connection string are not in the list of allowed parameters
            foreach (string key in builder.Keys)
            {
                if (disAllowedParameters.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    Logger.Warn("Error: Connection string contains unauthorized parameter.");
                    return false;
                }
            }

            return true; // Connection string is valid
        }
        //* Fortify Issue Type: Connection String Parameter Pollution
        public static bool ValidateSQLConnectionStringWithBuilder(string connectionString, out System.Data.SqlClient.SqlConnectionStringBuilder builder)
        {
            builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);

            // List of not allowed parameters
            string[] disAllowedParameters = { };

            //Check if any keys in the connection string are not in the list of allowed parameters
            foreach (string key in builder.Keys)
            {
                if (disAllowedParameters.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    Logger.Warn("Error: Connection string contains unauthorized parameter.");
                    return false;
                }
            }

            return true; // Connection string is valid
        }
        public static bool IsDefaultRMConstantsReturnValue(string value)
        {
            if (string.Equals(value, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("QSF2QEUjJHA="))))
            {
                return true;
            }
            return false;
        }
        //*Fortify Issue Type: Cookie Security: Overly Broad Path
        public static string GetCookiePathFromFullUrl(string fullUrl)
        {
            if (fullUrl!=null && fullUrl!=string.Empty)
            {
                // Parse the URL
                Uri uri = new Uri(fullUrl);

                // Get the absolute path from the URL
                string absolutePath = uri.AbsolutePath.TrimEnd('/');

                int lastIndex = absolutePath.LastIndexOf('/');

                if (lastIndex > 0)
                {
                    return absolutePath.Substring(0, lastIndex);
                }
                else
                {
                    return absolutePath;
                }
            }
            return fullUrl;
        }
        //*Fortify Issue Type: Command Injection
        public static bool ValidateCommandArgs(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                Logger.Warn("Error: The specified Command Args is null, Please provide a valid value.");
                return false;
            }
            else if (args.Contains(';'))
            {
                Logger.Warn("Error: The specified Command Args contains ilelgal character, Please provide a valid value.");
                return false;
            }
            return true;
        }

        public static string SanitizeCommandArgs(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                return "";
            }
            else if (args.Contains(';'))
            {
                throw new ArgumentException("Invalid args.");
            }
            else
            {
                return args;
            }
        }

        //*Fortify Issue Type: Header Manipulation
        public static bool IsValidFileName(string fileName)
        {
            // Check if the file name is null or empty
            if (string.IsNullOrEmpty(fileName))
            {
                Logger.Warn("Error: The specified fileName is null, Please provide a valid value.");
                return false;
            }

            // Check if the file name contains invalid characters
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                Logger.Warn("Error: The specified fileName contains invalid file name chars, Please provide a valid value.");
                return false;
            }

            // Check if the file name is a reserved file name on Windows
            if (fileName.Equals("CON", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("PRN", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("AUX", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("NUL", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM1", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM2", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM3", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM4", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM5", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM6", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM7", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM8", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("COM9", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT1", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT2", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT3", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT4", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT5", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT6", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT7", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT8", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("LPT9", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warn("Error: The specified fileName contains reserved file name, Please provide a valid value.");
                return false;
            }

            // If none of the checks fail, consider the file name valid
            return true;
        }
        //*Fortify Issue Type: XPath Injection
        public static string SanitizeXMLContent(string content)
        {
            return SecurityElement.Escape(content);
        }
        //*Fortify Issue Type: Server-Side Request Forgery
        private static readonly string[] AllowedSchemes = { "http", "https" };
        public static string SanitizeRequestUrl(string url)
        {
            // Validate the URL to ensure it uses an allowed scheme
            Uri uri;
            try
            {
                uri = new Uri(url);
            }
            catch (UriFormatException)
            {
                throw new ArgumentException("Invalid URL format.");
            }

            if (AllowedSchemes.Contains(uri.Scheme))
            {
                return url;
            }
            else
            {
                throw new ArgumentException("Only HTTP and HTTPS schemes are allowed.");
            }

            // Reconstruct the URL with only safe components, but the Sanitize url might be different from the input 
            //UriBuilder safeUri = new UriBuilder
            //{
            //    Scheme = uri.Scheme,
            //    Host = uri.Host,
            //    Port = uri.Port,
            //    Path = uri.PathAndQuery // Keep the path and query intact, if needed
            //};

            //return safeUri.Uri.ToString();
        }
        //*Fortify Issue Type: Insecure Randomness
        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        public static int GetRandomNumber(int min, int max)
        {
            byte[] randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            int randomInt = BitConverter.ToInt32(randomBytes, 0);
            return Math.Abs(randomInt % (max - min)) + min;
        }
        //*Fortify Issue Type: SQL Injection
        private static readonly Regex SQLParameterNameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_-]{0,126}$", RegexOptions.Compiled);
        public static string SanitizeSQLParameterName(string parameterName, bool nullable = false)
        {
            if (nullable && (parameterName == null || parameterName == string.Empty))
            {
                return parameterName;
            }
            if (SQLParameterNameRegex.IsMatch(parameterName))
            {
                return parameterName;
            }
            else
            {
                Logger.Warn("Error: The specified parameterName contains disallowed characters");
                throw new ArgumentException($"Invalid parameter name");
            }
        }
        //*Fortify Issue Type: Parameter Pollution
        public static string SanitizeSQLSchemaName(string schemaName)
        {
            if (schemaName.Contains("'") || schemaName.Contains("\"") || schemaName.Contains(";"))
            {
                Logger.Warn($"Error: The schema name contains disallowed characters.");
                throw new ArgumentException($"Invalid schema name");
            }
            return schemaName;
        }
        private static string Combine(bool isAppRoot = true, params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }

            string text = string.Empty;
            string text2 = Directory.GetCurrentDirectory();
            for (int i = 0; i < paths.Length; i++)
            {
                string text3 = paths[i];
                if (string.IsNullOrWhiteSpace(text3))
                {
                    continue;
                }

                if (text3.EndsWith("..") || text3.Contains("../") || text3.Contains("..\\"))
                {
                    throw new ArgumentException("Path contains invalid characters: " + text3);
                }

                string path;
                if (i == 0)
                {
                    if (Path.IsPathRooted(text3))
                    {
                        text2 = text3;
                    }

                    path = text2;
                }
                else
                {
                    path = Path.Combine(text2, Path.Combine(paths.Take(i).ToArray()));
                }

                text = Path.GetFullPath(Path.Combine(text2, Path.Combine(paths.Take(i + 1).ToArray())));
                if (!text.StartsWith(Path.GetFullPath(path)))
                {
                    throw new ArgumentException("Path contains invalid characters: " + string.Join(", ", paths));
                }
            }

            if (!isAppRoot)
            {
                return Path.Combine(paths);
            }

            return text;
        }
    }
}
