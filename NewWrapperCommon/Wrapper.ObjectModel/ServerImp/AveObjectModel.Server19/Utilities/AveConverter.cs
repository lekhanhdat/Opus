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
using System.Xml;

namespace AvePoint.ObjectModel.Server19
{
    /// <summary>
    /// Summary description for AveConverter.
    /// </summary>
    public class AveConverter
    {
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

        public static void WriteBack(int id, ref byte[] buf, int offset)
        {
            for (int i = 0; i < 4; i++)
            {
                buf[offset + i] = (byte)(id & 255);
                id >>= 8;
            }
        }

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

        public static string textToXml(string text)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement e = doc.CreateElement("A");
            e.InnerText = text;
            string inner = e.InnerXml;
            return inner.Replace("\"", "&quot;");
        }

        public static string EncodeSpecialChar(string name)
        {
            //			string t=name.Replace("%","%%");
            //			return t.Replace("\\","%1");
            // %-%1;\-%2
            try
            {
                if (name != null)
                {
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
                }
            }
            catch (ArgumentException)
            { }
            return name;
        }

        public static string DecodeSpecialChar(string name)
        {
            //			string t=name.Replace("%1","\\");
            //			return t.Replace("%%","%");

            // %-%1;\-%2
            try
            {
                if (name != null)
                {
                    int mIndex = name.IndexOf('%', 0);
                    while (mIndex >= 0)
                    {
                        if (name.Substring(mIndex, 2).Equals("%1", StringComparison.OrdinalIgnoreCase))
                            name = name.Substring(0, mIndex) + "%" + name.Substring(mIndex + 2);
                        else if (name.Substring(mIndex, 2).Equals("%2", StringComparison.OrdinalIgnoreCase))
                            name = name.Substring(0, mIndex) + "\\" + name.Substring(mIndex + 2);
                        mIndex = name.IndexOf('%', mIndex + 1);
                    }
                }
            }
            catch (ArgumentException) { }
            return name;
        }

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
