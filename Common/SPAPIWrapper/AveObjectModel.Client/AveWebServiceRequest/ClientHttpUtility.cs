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
/// <summary>
/// this is source code from CSOM sdk.
/// </summary>
namespace Microsoft.SharePoint.Client.Utilities
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using System.Web.UI;
	// Token: 0x020002EC RID: 748
	public class HttpUtility
	{
		// Token: 0x060015AD RID: 5549 RVA: 0x00068A36 File Offset: 0x00066C36
		protected HttpUtility()
		{
		}

		// Token: 0x060015AE RID: 5550 RVA: 0x00068A40 File Offset: 0x00066C40
		public static string HtmlEncode(string valueToEncode)
		{
			if (valueToEncode == null || valueToEncode.Length == 0)
			{
				return valueToEncode;
			}
			StringBuilder stringBuilder = new StringBuilder(255);
			HtmlTextWriter output = new HtmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture));
			HttpUtility.HtmlEncode(valueToEncode, output);
			return stringBuilder.ToString();
		}

		// Token: 0x060015AF RID: 5551 RVA: 0x00068A84 File Offset: 0x00066C84
		public static void HtmlEncode(string valueToEncode, TextWriter output)
		{
			if (valueToEncode == null || valueToEncode.Length == 0 || output == null)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			int length = valueToEncode.Length;
			for (int i = 0; i < length; i++)
			{
				int num3 = (int)valueToEncode[i];
				int num4;
				if (num3 < 63)
				{
					num4 = (int)HttpUtility.HTMLCharMap1[num3];
				}
				else
				{
					num4 = 0;
				}
				if (num4 != 0)
				{
					if (num2 > 0)
					{
						output.Write(valueToEncode.Substring(num, num2));
						num2 = 0;
					}
					num = i + 1;
					output.Write(HttpUtility.HTMLData[num4]);
				}
				else
				{
					num2++;
				}
			}
			if (num < length)
			{
				output.Write(valueToEncode.Substring(num));
			}
		}

		// Token: 0x060015B0 RID: 5552 RVA: 0x00068B18 File Offset: 0x00066D18
		public static string EcmaScriptStringLiteralEncode(string scriptLiteralToEncode)
		{
			if (scriptLiteralToEncode == null || scriptLiteralToEncode.Length == 0)
			{
				return scriptLiteralToEncode;
			}
			StringBuilder stringBuilder = new StringBuilder(255);
			HtmlTextWriter output = new HtmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture));
			HttpUtility.EcmaScriptStringLiteralEncode(scriptLiteralToEncode, output);
			return stringBuilder.ToString();
		}

		// Token: 0x060015B1 RID: 5553 RVA: 0x00068B5C File Offset: 0x00066D5C
		public static void EcmaScriptStringLiteralEncode(string scriptLiteralToEncode, TextWriter output)
		{
			if (scriptLiteralToEncode == null || scriptLiteralToEncode.Length == 0 || output == null)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			int length = scriptLiteralToEncode.Length;
			for (int i = 0; i < length; i++)
			{
				int num3 = (int)scriptLiteralToEncode[i];
				if (num3 > 127)
				{
					if (num2 > 0)
					{
						output.Write(scriptLiteralToEncode.Substring(num, num2));
						num2 = 0;
					}
					num = i + 1;
					output.Write("\\u");
					int num4 = num3 >> 8;
					if (num4 == 0)
					{
						output.Write("00");
					}
					else if (num4 < 16)
					{
						output.Write('0');
						output.Write(num4.ToString("X", CultureInfo.InvariantCulture));
					}
					else
					{
						output.Write(num4.ToString("X", CultureInfo.InvariantCulture));
					}
					num4 = (num3 & 255);
					if (num4 < 16)
					{
						output.Write('0');
						output.Write(num4.ToString("X", CultureInfo.InvariantCulture));
					}
					else
					{
						output.Write(num4.ToString("X", CultureInfo.InvariantCulture));
					}
				}
				else
				{
					ushort num5;
					if (num3 < 95)
					{
						num5 = HttpUtility.ScriptCharMap[num3];
					}
					else
					{
						num5 = 0;
					}
					if (num5 > 0)
					{
						if (num2 > 0)
						{
							output.Write(scriptLiteralToEncode.Substring(num, num2));
							num2 = 0;
						}
						num = i + 1;
						output.Write(HttpUtility.ScriptEncodedChars[(int)num5]);
					}
					else
					{
						num2++;
					}
				}
			}
			if (num < length)
			{
				output.Write(scriptLiteralToEncode.Substring(num));
			}
		}

		


		// Token: 0x060015B4 RID: 5556 RVA: 0x00068D04 File Offset: 0x00066F04
		public static string UrlKeyValueEncode(string keyToEncode, string valueToEncode)
		{
			return HttpUtility.UrlKeyValueEncode(keyToEncode) + "=" + HttpUtility.UrlKeyValueEncode(valueToEncode);
		}

		// Token: 0x060015B5 RID: 5557 RVA: 0x00068D1C File Offset: 0x00066F1C
		public static void UrlKeyValueEncode(string keyToEncode, string valueToEncode, TextWriter output)
		{
			HttpUtility.UrlKeyValueEncode(keyToEncode, output);
			output.Write("=");
			HttpUtility.UrlKeyValueEncode(valueToEncode, output);
		}

		// Token: 0x060015B6 RID: 5558 RVA: 0x00068D38 File Offset: 0x00066F38
		public static string UrlKeyValueEncode(string keyOrValueToEncode)
		{
			if (keyOrValueToEncode == null || keyOrValueToEncode.Length == 0)
			{
				return keyOrValueToEncode;
			}
			StringBuilder stringBuilder = new StringBuilder(255);
			HtmlTextWriter output = new HtmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture));
			HttpUtility.UrlKeyValueEncode(keyOrValueToEncode, output);
			return stringBuilder.ToString();
		}

		// Token: 0x060015B7 RID: 5559 RVA: 0x00068D79 File Offset: 0x00066F79
		public static string UrlKeyValueEncode(Guid guidKeyOrValueToEncode)
		{
			return guidKeyOrValueToEncode.ToString("B").ToUpper(CultureInfo.InvariantCulture);
		}

		// Token: 0x060015B8 RID: 5560 RVA: 0x00068D91 File Offset: 0x00066F91
		public static string UrlKeyValueEncode(int keyOrValueToEncode)
		{
			return keyOrValueToEncode.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x060015B9 RID: 5561 RVA: 0x00068DA0 File Offset: 0x00066FA0
		public static void UrlKeyValueEncode(string keyOrValueToEncode, TextWriter output)
		{
			if (keyOrValueToEncode == null || keyOrValueToEncode.Length == 0 || output == null)
			{
				return;
			}
			bool flag = false;
			int num = 0;
			int num2 = 0;
			int length = keyOrValueToEncode.Length;
			for (int i = 0; i < length; i++)
			{
				char c = keyOrValueToEncode[i];
				if (('0' <= c && c <= '9') || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z'))
				{
					num2++;
				}
				else
				{
					if (num2 > 0)
					{
						output.Write(keyOrValueToEncode.Substring(num, num2));
						num2 = 0;
					}
					HttpUtility.UrlEncodeUnicodeChar(output, keyOrValueToEncode[i], (i < length - 1) ? keyOrValueToEncode[i + 1] : '\0', out flag);
					if (flag)
					{
						i++;
					}
					num = i + 1;
				}
			}
			if (num < length && output != null)
			{
				output.Write(keyOrValueToEncode.Substring(num));
			}
		}

		// Token: 0x060015BA RID: 5562 RVA: 0x00068E6D File Offset: 0x0006706D
		public static string UrlKeyValueDecode(string keyOrValueToDecode)
		{
			if (string.IsNullOrEmpty(keyOrValueToDecode))
			{
				return keyOrValueToDecode;
			}
			return HttpUtility.UrlDecodeHelper(keyOrValueToDecode, keyOrValueToDecode.Length, true);
		}

		// Token: 0x060015BB RID: 5563 RVA: 0x00068E86 File Offset: 0x00067086
		public static string UrlPathEncode(string urlToEncode, bool allowHashParameter)
		{
			return HttpUtility.UrlPathEncode(urlToEncode, allowHashParameter, false);
		}

		// Token: 0x060015BC RID: 5564 RVA: 0x00068E90 File Offset: 0x00067090
		public static string UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters)
		{
			return HttpUtility.UrlPathEncode(urlToEncode, allowHashParameter, encodeUnicodeCharacters, false);
		}

		// Token: 0x060015BD RID: 5565 RVA: 0x00068E9C File Offset: 0x0006709C
		public static string UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters, bool rfcCompliant)
		{
			bool flag = false;
			return HttpUtility.UrlPathEncode(urlToEncode, allowHashParameter, encodeUnicodeCharacters, ref flag, rfcCompliant);
		}

		// Token: 0x060015BE RID: 5566 RVA: 0x00068EB6 File Offset: 0x000670B6
		internal static string UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters, ref bool invalidUnicode)
		{
			return HttpUtility.UrlPathEncode(urlToEncode, allowHashParameter, encodeUnicodeCharacters, ref invalidUnicode, false);
		}

		// Token: 0x060015BF RID: 5567 RVA: 0x00068EC4 File Offset: 0x000670C4
		internal static string UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters, ref bool invalidUnicode, bool rfcCompliant)
		{
			if (urlToEncode == null || urlToEncode.Length == 0)
			{
				return urlToEncode;
			}
			StringBuilder stringBuilder = new StringBuilder(255);
			HtmlTextWriter output = new HtmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture));
			HttpUtility.UrlPathEncode(urlToEncode, allowHashParameter, encodeUnicodeCharacters, output, ref invalidUnicode, rfcCompliant);
			return stringBuilder.ToString();
		}

		// Token: 0x060015C0 RID: 5568 RVA: 0x00068F0A File Offset: 0x0006710A
		public static void UrlPathEncode(string urlToEncode, bool allowHashParameter, TextWriter output)
		{
			HttpUtility.UrlPathEncode(urlToEncode, allowHashParameter, false, output);
		}

		// Token: 0x060015C1 RID: 5569 RVA: 0x00068F18 File Offset: 0x00067118
		public static void UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters, TextWriter output)
		{
			bool flag = false;
			HttpUtility.UrlPathEncode(urlToEncode, allowHashParameter, encodeUnicodeCharacters, output, ref flag, false);
		}

		// Token: 0x060015C2 RID: 5570 RVA: 0x00068F34 File Offset: 0x00067134
		private static void UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters, TextWriter output, ref bool invalidUnicode, bool rfcCompliant)
		{
			if (urlToEncode == null || urlToEncode.Length == 0 || output == null)
			{
				return;
			}
			bool flag = false;
			int i = 0;
			int length = urlToEncode.Length;
			while (i < length && urlToEncode[i] == ' ')
			{
				i++;
			}
			int num = i;
			int num2 = 0;
			while (i < length)
			{
				char c = urlToEncode[i];
				if (c == '?' || (allowHashParameter && c == '#'))
				{
					break;
				}
				if ((c & '￠') == '\0' || c == ' ' || c == '"' || c == '#' || c == '%' || c == '<' || c == '>' || c == '\'' || c == '&' || (rfcCompliant && (c == '|' || c == '\\' || c == '`' || c == '[' || c == ']' || c == '^' || c == '{' || c == '}')))
				{
					if (num2 > 0)
					{
						output.Write(urlToEncode.Substring(num, num2));
						num2 = 0;
					}
					num = i + 1;
					int num3 = (int)(c & 'ÿ');
					if (num3 < 16)
					{
						output.Write("%0");
						output.Write(num3.ToString("X", CultureInfo.InvariantCulture));
					}
					else
					{
						output.Write('%');
						output.Write(num3.ToString("X", CultureInfo.InvariantCulture));
					}
				}
				else if (encodeUnicodeCharacters && c > '\u007f')
				{
					if (num2 > 0)
					{
						output.Write(urlToEncode.Substring(num, num2));
						num2 = 0;
					}
					HttpUtility.UrlEncodeUnicodeChar(output, urlToEncode[i], (i < length - 1) ? urlToEncode[i + 1] : '\0', ref invalidUnicode, out flag);
					if (flag)
					{
						i++;
					}
					num = i + 1;
				}
				else
				{
					num2++;
				}
				i++;
			}
			if (num < length)
			{
				output.Write(urlToEncode.Substring(num));
			}
		}

		// Token: 0x060015C3 RID: 5571 RVA: 0x000690EC File Offset: 0x000672EC
		private static string UrlDecodeHelper(string stringToDecode, int length, bool decodePlus)
		{
			if (stringToDecode == null || stringToDecode.Length == 0)
			{
				return stringToDecode;
			}
			StringBuilder stringBuilder = new StringBuilder(length);
			byte[] array = null;
			int i = 0;
			while (i < length)
			{
				char c = stringToDecode[i];
				if (c < ' ')
				{
					i++;
				}
				else if (decodePlus && c == '+')
				{
					stringBuilder.Append(" ");
					i++;
				}
				else if (HttpUtility.IsHexEscapedChar(stringToDecode, i, length))
				{
					if (array == null)
					{
						array = new byte[(length - i) / 3];
					}
					int count = 0;
					do
					{
						int num = HttpUtility.FromHexNoCheck(stringToDecode[i + 1]) * 16 + HttpUtility.FromHexNoCheck(stringToDecode[i + 2]);
						array[count++] = (byte)num;
						i += 3;
					}
					while (HttpUtility.IsHexEscapedChar(stringToDecode, i, length));
					stringBuilder.Append(Encoding.UTF8.GetChars(array, 0, count));
				}
				else
				{
					stringBuilder.Append(c);
					i++;
				}
			}
			if (length < stringToDecode.Length)
			{
				stringBuilder.Append(stringToDecode.Substring(length));
			}
			return stringBuilder.ToString();
		}

		

		// Token: 0x060015C5 RID: 5573 RVA: 0x00069384 File Offset: 0x00067584
		private static void UrlEncodeUnicodeChar(TextWriter output, char ch, char chNext, out bool fUsedNextChar)
		{
			bool flag = false;
			HttpUtility.UrlEncodeUnicodeChar(output, ch, chNext, ref flag, out fUsedNextChar);
		}

		// Token: 0x060015C6 RID: 5574 RVA: 0x000693A0 File Offset: 0x000675A0
		private static void UrlEncodeUnicodeChar(TextWriter output, char ch, char chNext, ref bool fInvalidUnicode, out bool fUsedNextChar)
		{
			int num = 192;
			int num2 = 224;
			int num3 = 240;
			int num4 = 128;
			int num5 = 55296;
			int num6 = 64512;
			int num7 = 65536;
			fUsedNextChar = false;
			if (ch <= '\u007f')
			{
				output.Write(HttpUtility.s_crgstrUrlHexValue[(int)ch]);
				return;
			}
			if (ch <= '߿')
			{
				int num8 = num | (int)(ch >> 6);
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				num8 = (num4 | (int)(ch & '?'));
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				return;
			}
			if (((int)ch & num6) != num5)
			{
				int num8 = num2 | (int)(ch >> 12);
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				num8 = (num4 | (int)((ch & '࿀') >> 6));
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				num8 = (num4 | (int)(ch & '?'));
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				return;
			}
			if (chNext != '\0')
			{
				int num9 = (int)((int)(ch & 'Ͽ') << 10);
				fUsedNextChar = true;
				num9 |= (int)(chNext & 'Ͽ');
				num9 += num7;
				int num8 = num3 | num9 >> 18;
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				num8 = (num4 | (num9 & 258048) >> 12);
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				num8 = (num4 | (num9 & 4032) >> 6);
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				num8 = (num4 | (num9 & 63));
				output.Write(HttpUtility.s_crgstrUrlHexValue[num8]);
				return;
			}
			fInvalidUnicode = true;
		}

		// Token: 0x060015C7 RID: 5575 RVA: 0x00069518 File Offset: 0x00067718
		private static bool IsHexEscapedChar(string str, int nIndex, int nPathLength)
		{
			return nIndex + 2 < nPathLength && str[nIndex] == '%' && HttpUtility.IsHexDigit(str[nIndex + 1]) && HttpUtility.IsHexDigit(str[nIndex + 2]) && (str[nIndex + 1] != '0' || str[nIndex + 2] != '0');
		}

		// Token: 0x060015C8 RID: 5576 RVA: 0x00069577 File Offset: 0x00067777
		private static int FromHexNoCheck(char digit)
		{
			if (digit <= '9')
			{
				return (int)(digit - '0');
			}
			if (digit <= 'F')
			{
				return (int)(digit - 'A' + '\n');
			}
			return (int)(digit - 'a' + '\n');
		}

		// Token: 0x060015C9 RID: 5577 RVA: 0x00069597 File Offset: 0x00067797
		private static bool IsHexDigit(char digit)
		{
			return ('0' <= digit && digit <= '9') || ('a' <= digit && digit <= 'f') || ('A' <= digit && digit <= 'F');
		}

		// Token: 0x060015CA RID: 5578 RVA: 0x000695C0 File Offset: 0x000677C0
		// Note: this type is marked as 'beforefieldinit'.
		static HttpUtility()
		{
		}


		// Token: 0x04000E0A RID: 3594
		private static readonly ushort[] HTMLCharMap1 = new ushort[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			2,
			3,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			4,
			0,
			5,
			0
		};

		// Token: 0x04000E0B RID: 3595
		

		// Token: 0x04000E0C RID: 3596
		internal static readonly string[] HTMLData = new string[]
		{
			"",
			"&quot;",
			"&amp;",
			"&#39;",
			"&lt;",
			"&gt;",
			" ",
			"<br />",
			"&#160;",
			"<b>",
			"<i>",
			"<u>",
			"</b>",
			"</i>",
			"</u>",
			"<wbr />"
		};

		// Token: 0x04000E0D RID: 3597
		private static readonly ushort[] ScriptCharMap = new ushort[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			3,
			0,
			0,
			4,
			5,
			6,
			7,
			8,
			0,
			9,
			0,
			0,
			0,
			10,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			11,
			0,
			12,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			13,
			0,
			0,
			0
		};

		// Token: 0x04000E0E RID: 3598
		private static readonly string[] ScriptEncodedChars = new string[]
		{
			"",
			"\\n",
			"\\r",
			"\\u0022",
			"\\u0025",
			"\\u0026",
			"\\u0027",
			"\\u0028",
			"\\u0029",
			"\\u002b",
			"\\u002f",
			"\\u003c",
			"\\u003e",
			"\\\\"
		};

		// Token: 0x04000E0F RID: 3599
		private static readonly string[] s_crgstrUrlHexValue = new string[]
		{
			"%00",
			"%01",
			"%02",
			"%03",
			"%04",
			"%05",
			"%06",
			"%07",
			"%08",
			"%09",
			"%0A",
			"%0B",
			"%0C",
			"%0D",
			"%0E",
			"%0F",
			"%10",
			"%11",
			"%12",
			"%13",
			"%14",
			"%15",
			"%16",
			"%17",
			"%18",
			"%19",
			"%1A",
			"%1B",
			"%1C",
			"%1D",
			"%1E",
			"%1F",
			"%20",
			"%21",
			"%22",
			"%23",
			"%24",
			"%25",
			"%26",
			"%27",
			"%28",
			"%29",
			"%2A",
			"%2B",
			"%2C",
			"%2D",
			"%2E",
			"%2F",
			"%30",
			"%31",
			"%32",
			"%33",
			"%34",
			"%35",
			"%36",
			"%37",
			"%38",
			"%39",
			"%3A",
			"%3B",
			"%3C",
			"%3D",
			"%3E",
			"%3F",
			"%40",
			"%41",
			"%42",
			"%43",
			"%44",
			"%45",
			"%46",
			"%47",
			"%48",
			"%49",
			"%4A",
			"%4B",
			"%4C",
			"%4D",
			"%4E",
			"%4F",
			"%50",
			"%51",
			"%52",
			"%53",
			"%54",
			"%55",
			"%56",
			"%57",
			"%58",
			"%59",
			"%5A",
			"%5B",
			"%5C",
			"%5D",
			"%5E",
			"%5F",
			"%60",
			"%61",
			"%62",
			"%63",
			"%64",
			"%65",
			"%66",
			"%67",
			"%68",
			"%69",
			"%6A",
			"%6B",
			"%6C",
			"%6D",
			"%6E",
			"%6F",
			"%70",
			"%71",
			"%72",
			"%73",
			"%74",
			"%75",
			"%76",
			"%77",
			"%78",
			"%79",
			"%7A",
			"%7B",
			"%7C",
			"%7D",
			"%7E",
			"%7F",
			"%80",
			"%81",
			"%82",
			"%83",
			"%84",
			"%85",
			"%86",
			"%87",
			"%88",
			"%89",
			"%8A",
			"%8B",
			"%8C",
			"%8D",
			"%8E",
			"%8F",
			"%90",
			"%91",
			"%92",
			"%93",
			"%94",
			"%95",
			"%96",
			"%97",
			"%98",
			"%99",
			"%9A",
			"%9B",
			"%9C",
			"%9D",
			"%9E",
			"%9F",
			"%A0",
			"%A1",
			"%A2",
			"%A3",
			"%A4",
			"%A5",
			"%A6",
			"%A7",
			"%A8",
			"%A9",
			"%AA",
			"%AB",
			"%AC",
			"%AD",
			"%AE",
			"%AF",
			"%B0",
			"%B1",
			"%B2",
			"%B3",
			"%B4",
			"%B5",
			"%B6",
			"%B7",
			"%B8",
			"%B9",
			"%BA",
			"%BB",
			"%BC",
			"%BD",
			"%BE",
			"%BF",
			"%C0",
			"%C1",
			"%C2",
			"%C3",
			"%C4",
			"%C5",
			"%C6",
			"%C7",
			"%C8",
			"%C9",
			"%CA",
			"%CB",
			"%CC",
			"%CD",
			"%CE",
			"%CF",
			"%D0",
			"%D1",
			"%D2",
			"%D3",
			"%D4",
			"%D5",
			"%D6",
			"%D7",
			"%D8",
			"%D9",
			"%DA",
			"%DB",
			"%DC",
			"%DD",
			"%DE",
			"%DF",
			"%E0",
			"%E1",
			"%E2",
			"%E3",
			"%E4",
			"%E5",
			"%E6",
			"%E7",
			"%E8",
			"%E9",
			"%EA",
			"%EB",
			"%EC",
			"%ED",
			"%EE",
			"%EF",
			"%F0",
			"%F1",
			"%F2",
			"%F3",
			"%F4",
			"%F5",
			"%F6",
			"%F7",
			"%F8",
			"%F9",
			"%FA",
			"%FB",
			"%FC",
			"%FD",
			"%FE",
			"%FF"
		};

		// Token: 0x0200031D RID: 797
		internal static class HtmlStrings
		{
			// Token: 0x04000F2E RID: 3886
			public const string Empty = "";

			// Token: 0x04000F2F RID: 3887
			public const string Quot = "&quot;";

			// Token: 0x04000F30 RID: 3888
			public const string Amp = "&amp;";

			// Token: 0x04000F31 RID: 3889
			public const string Apostrophe = "&#39;";

			// Token: 0x04000F32 RID: 3890
			public const string Lt = "&lt;";

			// Token: 0x04000F33 RID: 3891
			public const string Gt = "&gt;";

			// Token: 0x04000F34 RID: 3892
			public const string Space = " ";

			// Token: 0x04000F35 RID: 3893
			public const string Br = "<br />";

			// Token: 0x04000F36 RID: 3894
			public const string Nbsp = "&#160;";

			// Token: 0x04000F37 RID: 3895
			public const string B = "<b>";

			// Token: 0x04000F38 RID: 3896
			public const string I = "<i>";

			// Token: 0x04000F39 RID: 3897
			public const string U = "<u>";

			// Token: 0x04000F3A RID: 3898
			public const string BClose = "</b>";

			// Token: 0x04000F3B RID: 3899
			public const string IClose = "</i>";

			// Token: 0x04000F3C RID: 3900
			public const string UClose = "</u>";

			// Token: 0x04000F3D RID: 3901
			public const string Wbr = "<wbr />";

			// Token: 0x04000F3E RID: 3902
			public const string Style = "<style>";

			// Token: 0x04000F3F RID: 3903
			public const string StyleClose = "</style>";

			// Token: 0x04000F40 RID: 3904
			public const string Cdata = "<![CDATA[";

			// Token: 0x04000F41 RID: 3905
			public const string CdataClose = "]]>";
		}
	}
}