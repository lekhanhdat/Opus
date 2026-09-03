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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// The encoding helper.
	/// </summary>
	/// <summary>
	/// The encoding helper.
	/// </summary>
	internal static class EncodingHelper
	{
		public static void AddKeyValueStringsWithUrlEncoding(StringBuilder messageBuilder, Dictionary<string, string> keyValuePairs)
		{
			foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
			{
				AddKeyValueString(messageBuilder, UrlEncode(keyValuePair.Key), UrlEncode(keyValuePair.Value));
			}
		}

		public static void AddStringWithUrlEncoding(StringBuilder messageBuilder, string key, char[] value)
		{
			char[] array = null;
			try
			{
				array = UrlEncode(value);
				AddKeyValueString(messageBuilder, UrlEncode(key), array);
			}
			finally
			{
				array.SecureClear();
			}
		}

		public static void AddKeyValueString(StringBuilder messageBuilder, string key, string value)
		{
			AddKeyValueString(messageBuilder, key, value.ToCharArray());
		}

		public static Dictionary<string, string> ParseKeyValueList(string input, char delimiter, bool urlDecode, CallState callState)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			List<string> list = SplitWithQuotes(input, delimiter);
			foreach (string item in list)
			{
				List<string> list2 = SplitWithQuotes(item, '=');
				if (list2.Count == 2 && !string.IsNullOrWhiteSpace(list2[0]) && !string.IsNullOrWhiteSpace(list2[1]))
				{
					string text = list2[0];
					string text2 = list2[1];
					if (urlDecode)
					{
						text = UrlDecode(text);
						text2 = UrlDecode(text2);
					}
					text = text.Trim().PlatformSpecificToLower();
					text2 = text2.Trim().Trim('"').Trim();
					if (dictionary.ContainsKey(text))
					{
						ADALLogger.Warning(callState, "Key/value pair list contains redundant key '{0}'.", text);
					}
					dictionary[text] = text2;
				}
			}
			return dictionary;
		}

		public static byte[] ToByteArray(this StringBuilder stringBuilder)
		{
			if (stringBuilder != null)
			{
				UTF8Encoding uTF8Encoding = new UTF8Encoding();
				char[] array = new char[stringBuilder.Length];
				try
				{
					stringBuilder.CopyTo(0, array, 0, stringBuilder.Length);
					return uTF8Encoding.GetBytes(array);
				}
				finally
				{
					array.SecureClear();
				}
			}
			return null;
		}

		public static void SecureClear(this StringBuilder stringBuilder)
		{
			if (stringBuilder != null)
			{
				for (int i = 0; i < stringBuilder.Length; i++)
				{
					stringBuilder[i] = '\0';
				}
				stringBuilder.Length = 0;
			}
		}

		public static void SecureClear(this byte[] bytes)
		{
			if (bytes != null)
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					bytes[i] = 0;
				}
			}
		}

		public static void SecureClear(this char[] chars)
		{
			if (chars != null)
			{
				for (int i = 0; i < chars.Length; i++)
				{
					chars[i] = '\0';
				}
			}
		}

		internal static string Base64Encode(string input)
		{
			string result = string.Empty;
			if (!string.IsNullOrEmpty(input))
			{
				result = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
			}
			return result;
		}

		internal static string Base64Decode(string encodedString)
		{
			string result = null;
			if (!string.IsNullOrEmpty(encodedString))
			{
				byte[] array = Convert.FromBase64String(encodedString);
				result = Encoding.UTF8.GetString(array, 0, array.Length);
			}
			return result;
		}

		internal static char[] UrlEncode(char[] message)
		{
			if (message == null)
			{
				return null;
			}
			char[] array = new char[message.Length * 2];
			int num = 0;
			char[] array2 = new char[1];
			for (int i = 0; i < message.Length; i++)
			{
				char c = array2[0] = message[i];
				string message2 = new string(array2);
				string text = UrlEncode(message2);
				char[] array3 = text.ToCharArray();
				if (num + array3.Length > array.Length)
				{
					Array.Resize(ref array, array.Length + message.Length * 2);
				}
				array3.CopyTo(array, num);
				num += array3.Length;
			}
			Array.Resize(ref array, num);
			return array;
		}

		internal static List<string> SplitWithQuotes(string input, char delimiter)
		{
			List<string> list = new List<string>();
			if (string.IsNullOrWhiteSpace(input))
			{
				return list;
			}
			int num = 0;
			bool flag = false;
			string text;
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] == delimiter && !flag)
				{
					text = input.Substring(num, i - num);
					if (!string.IsNullOrWhiteSpace(text.Trim()))
					{
						list.Add(text);
					}
					num = i + 1;
				}
				else if (input[i] == '"')
				{
					flag = !flag;
				}
			}
			text = input.Substring(num);
			if (!string.IsNullOrWhiteSpace(text.Trim()))
			{
				list.Add(text);
			}
			return list;
		}

		private static void AddKeyValueString(StringBuilder messageBuilder, string key, char[] value)
		{
			string text = (messageBuilder.Length == 0) ? string.Empty : "&";
			messageBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}=", new object[2]
			{
				text,
				key
			});
			messageBuilder.Append(value);
		}

		public static char[] ToCharArray(this SecureString secureString)
		{
			char[] array = new char[secureString.Length];
			IntPtr intPtr = Marshal.SecureStringToCoTaskMemUnicode(secureString);
			for (int i = 0; i < secureString.Length; i++)
			{
				array[i] = (char)Marshal.ReadInt16(intPtr, i * 2);
			}
			Marshal.ZeroFreeCoTaskMemUnicode(intPtr);
			return array;
		}

		public static string UrlEncode(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return message;
			}
			message = Uri.EscapeDataString(message);
			message = message.Replace("%20", "+");
			return message;
		}

		public static string UrlDecode(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return message;
			}
			message = message.Replace("+", "%20");
			message = Uri.UnescapeDataString(message);
			return message;
		}
	}
}