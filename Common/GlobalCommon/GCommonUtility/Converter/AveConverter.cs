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




namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Xml;
    #endregion
    /// <summary>
    /// Summary description for AveConverter.
    /// </summary>
    public class AveConverter
    {
        /// <summary>
        /// 替换字符串中的特殊字符。
        /// <p>将字符串中ASCII码为前20的字符替换为空格。</p>
        /// </summary>
        /// <param name="property">原字符串。</param>
        /// <returns>转化后的字符串。</returns>
        public static string ReplaceSpecialChar(string property)
        {
            char[] temp = property.ToCharArray();
            int i;
            bool flag = false;
            for (i = 0; i < temp.Length; i++)
            {
                if (temp[i] > 0 && temp[i] < 0x20)
                {
                    temp[i] = ' ';
                    flag = true;
                }
            }
            if (flag)
                return new string(temp);
            else
                return property;
        }

        /// <summary>
        /// Convert integer to big bytes.
        /// </summary>
        /// <param name="a">Integer.</param>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Const 4.</returns>
        public static int ToBigBytes(int a, byte[] buf, int offset)
        {
            buf[offset + 3] = (byte)a;
            a >>= 8;
            buf[offset + 2] = (byte)a;
            a >>= 8;
            buf[offset + 1] = (byte)a;
            a >>= 8;
            buf[offset + 0] = (byte)a;
            a >>= 8;
            return 4;
        }
        /// <summary>
        /// Convert integer64 to big bytes.
        /// </summary>
        /// <param name="a">Integer64.</param>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Const 8.</returns>
        public static int ToBigBytes(long a, byte[] buf, int offset)
        {
            int i = 8;
            do
            {
                buf[offset + --i] = (byte)a;
                a >>= 8;
            } while (i > 0);
            return 8;
        }

        /// <summary>
        /// Convert unsigned integer to big bytes.
        /// </summary>
        /// <param name="a">Unsigned integer.</param>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Const 4.</returns>
        public static int ToBigBytes(uint a, byte[] buf, int offset)
        {
            buf[offset + 3] = (byte)a;
            a >>= 8;
            buf[offset + 2] = (byte)a;
            a >>= 8;
            buf[offset + 1] = (byte)a;
            a >>= 8;
            buf[offset + 0] = (byte)a;
            a >>= 8;
            return 4;
        }

        /// <summary>
        /// Convert integer to big bytes.
        /// </summary>
        /// <param name="a">Integer.</param>
        /// <returns>Big bytes.</returns>
        public static byte[] ToBigBytes(int a)
        {
            byte[] buffer = new byte[4];

            ToBigBytes(a, buffer, 0);

            return buffer;
        }

        /// <summary>
        /// Convert bytes to integer.
        /// </summary>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Converted integer.</returns>
        public static int ToBigInt(byte[] buf, int offset)
        {
            int i;
            int a = 0;
            for (i = 0; i < 4; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;

        }

        /// <summary>
        /// Convert bytes to unsigned integer.
        /// </summary>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Unsigned integer.</returns>
        public static uint ToBigUint(byte[] buf, int offset)
        {
            int i;
            uint a = 0;
            for (i = 0; i < 4; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;

        }

        /// <summary>
        /// Convert bytes to short integer.
        /// </summary>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Short integer.</returns>
        public static short ToBigShort(byte[] buf, int offset)
        {
            int i;
            short a = 0;
            for (i = 0; i < 2; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;

        }

        /// <summary>
        /// Convert bytes to unsigned short integer.
        /// </summary>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Unsigned short integer.</returns>
        public static ushort ToBigUShort(byte[] buf, int offset)
        {
            int i;
            ushort a = 0;
            for (i = 0; i < 2; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;

        }

        /// <summary>
        /// Convert integer to bytes.
        /// </summary>
        /// <param name="a">Integer.</param>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Const 4.</returns>
        public static int ToBytes(int a, byte[] buf, int offset)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                buf[offset++] = (byte)a;
                a >>= 8;
            }
            return 4;
        }

        /// <summary>
        /// Convert bytes to integer.
        /// </summary>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Converted integer.</returns>
        public static int ToInt(byte[] buf, int offset)
        {
            int i;
            int a = 0;
            offset += 3;
            for (i = 0; i < 4; i++)
            {
                a <<= 8;
                a += buf[offset--];
            }
            return a;
        }

        public static long ToBigLong(byte[] buf, int offset)
        {
            int i;
            long a = 0;
            for (i = 0; i < 8; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;
        }

        /// <summary>
        /// Convert unsigned integer to bytes.
        /// </summary>
        /// <param name="a">Integer.</param>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Const 4.</returns>
        public static int ToBytes(uint a, byte[] buf, int offset)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                buf[offset++] = (byte)a;
                a >>= 8;
            }
            return 4;
        }

        /// <summary>
        /// Convert bytes to unsigned integer.
        /// </summary>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Unsigned integer.</returns>
        public static uint ToUint(byte[] buf, int offset)
        {
            int i;
            uint a = 0;
            offset += 3;
            for (i = 0; i < 4; i++)
            {
                a <<= 8;
                a += buf[offset--];
            }
            return a;
        }

        /// <summary>
        /// To integer reversed.
        /// </summary>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Converted integer.</returns>
        public static int ToIntReversed(byte[] buf, int offset)
        {
            int i;
            int a = 0;
            for (i = 3; i >= 0; i--)
            {
                a <<= 8;
                a += buf[offset + i];
            }
            return a;

        }

        /// <summary>
        /// Write back.
        /// </summary>
        /// <param name="id">Integer.</param>
        /// <param name="buf">Data buffer.</param>
        /// <param name="offset">Offset.</param>
        public static void WriteBack(int id, ref byte[] buf, int offset)
        {
            for (int i = 0; i < 4; i++)
            {
                buf[offset + i] = (byte)(id & 255);
                id >>= 8;
            }
        }

        /// <summary>
        /// Convert content type bytes to string.
        /// </summary>
        /// <param name="buf">Data buffer.</param>
        /// <returns>Converted string.</returns>
        public static string ContentTypeBytesToString(byte[] buf)
        {
            string str = "0x";

            foreach (int bt in buf)
            {
                string temStr = System.String.Format("{0:X}", bt);
                if (temStr.Length == 1)
                {
                    temStr = "0" + temStr;
                }
                str += temStr;
            }
            return str;
        }

        /// <summary>
        /// 将字符串转换为xml文本内容。
        /// 处理xml文本中的敏感字符。
        /// </summary>
        /// <param name="text">带有xml文本敏感字符的字符串。</param>
        /// <returns>转换后的字符串。可放入xml标签中。</returns>
        public static string textToXml(string text)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement e = doc.CreateElement("A");
            e.InnerText = text;
            string inner = e.InnerXml;
            string result = inner.Replace("\"", "&quot;");
            return result;
        }

        /// <summary>
        /// Encode special chars.
        /// </summary>
        /// <param name="name">String contains special chars.</param>
        /// <returns>Converted string.</returns>
        public static string EncodeSpecialChar(string name)
        {
            //			string t=name.Replace("%","%%");
            //			return t.Replace("\\","%1");
            // %-%1;\-%2

            int minIndex = -1;
            int i, j;
            i = name.IndexOf('%');
            j = name.IndexOf('\\');
            if ((i < j || j == -1) && i != -1)
                minIndex = i;
            else
                minIndex = j;
            while (minIndex >= 0)
            {
                if (name.Substring(minIndex, 1) == "%")
                    name = name.Substring(0, minIndex) + "%1" + name.Substring(minIndex + 1);
                else if (name.Substring(minIndex, 1).Equals("\\", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(0, minIndex) + "%2" + name.Substring(minIndex + 1);
                i = name.IndexOf('%', minIndex + 2);
                j = name.IndexOf('\\', minIndex + 2);
                if ((i < j || j == -1) && i != -1)
                    minIndex = i;
                else
                    minIndex = j;
            }

            return name;
        }

        /// <summary>
        /// Decode special chars.
        /// </summary>
        /// <param name="name">String contains special chars.</param>
        /// <returns>Converted string.</returns>
        public static string DecodeSpecialChar(string name)
        {
            //			string t=name.Replace("%1","\\");
            //			return t.Replace("%%","%");

            // %-%1;\-%2

            int mIndex = name.IndexOf('%', 0);
            while (mIndex >= 0 && mIndex < name.Length - 1)
            {
                if (name.Substring(mIndex, 2).Equals("%1", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(0, mIndex) + "%" + name.Substring(mIndex + 2);
                else if (name.Substring(mIndex, 2).Equals("%2", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(0, mIndex) + "\\" + name.Substring(mIndex + 2);
                mIndex = name.IndexOf('%', mIndex + 1);
            }

            return name;
        }

        /// <summary>
        /// Convert long integer to bytes.
        /// </summary>
        /// <param name="l">Long integer.</param>
        /// <returns>Bytes array.</returns>
        public static byte[] LongToBytes(long l)
        {
            byte[] buf = new byte[8];
            buf[7] = (byte)l;
            l >>= 8;
            buf[6] = (byte)l;
            l >>= 8;
            buf[5] = (byte)l;
            l >>= 8;
            buf[4] = (byte)l;
            l >>= 8;
            buf[3] = (byte)l;
            l >>= 8;
            buf[2] = (byte)l;
            l >>= 8;
            buf[1] = (byte)l;
            l >>= 8;
            buf[0] = (byte)l;
            l >>= 8;
            return buf;
        }

        /// <summary>
        /// Convert bytes to long integer.
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static long BytesToLong(byte[] buf)
        {
            long a = 0;
            for (int i = 0; i < 8; i++)
            {
                a <<= 8;
                a += buf[i];
            }
            return a;
        }
    }
}
