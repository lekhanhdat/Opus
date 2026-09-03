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

namespace AvePoint.Metadata;
using System;
using System.IO;
using System.Text;
using System.Xml;

public class AveConvert
{
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

    private static char[] s_mphex2ch;

    static AveConvert()
    {
        s_mphex2ch = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
    }

    public static string ConvertAveObjToAveXml(string name, object value)
    {
        int XML_SIZE = 64 * 1024;
        MemoryStream ms = new MemoryStream(XML_SIZE);
        XmlTextWriter xmlWriter = new XmlTextWriter(ms, new UTF8Encoding(false));
        ms.Position = 0;
        AveXmlSerializer.Serialize(xmlWriter, name, value);
        xmlWriter.Flush();
        byte[] buffer = new byte[ms.Position];
        buffer = ms.GetBuffer();
        ms.Close();
        return Encoding.UTF8.GetString(buffer);
    }

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
    /// looks like it is BitConvert.GetBytes of a(int), and then put it into buf according to offset
    /// </summary>
    /// <param name="a"></param>
    /// <param name="buf"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
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

    public static string ConvertByteToContentTypeId(byte[] contentTypeBin)
    {
        StringBuilder tempStr = new StringBuilder("0x");
        int len = 0;
        byte[] tempAry = contentTypeBin;
        len = tempAry.Length;
        for (int i = 0; i < len; i++)
            tempStr.Append(tempAry[i].ToString("X2"));
        return tempStr.ToString();
    }

    public static object ChangeType(object value, Type type)
    {
        return Activator.CreateInstance(type, value);
    }

    public static string HexStringFromBytes(byte[] rgb)
    {
        StringBuilder sb = new StringBuilder("0x", 2 + ((rgb != null) ? (rgb.Length * 2) : 0));
        if (rgb != null)
        {
            foreach (byte num in rgb)
            {
                CharsOfByte(num, sb);
            }
        }
        return sb.ToString();
    }

    public static void CharsOfByte(byte b, StringBuilder sb)
    {
        sb.Append(s_mphex2ch[b >> 4]);
        sb.Append(s_mphex2ch[b & 15]);
    }

    public static byte Hex(char ch)
    {
        switch (ch)
        {
            case '0':
                return 0;

            case '1':
                return 1;

            case '2':
                return 2;

            case '3':
                return 3;

            case '4':
                return 4;

            case '5':
                return 5;

            case '6':
                return 6;

            case '7':
                return 7;

            case '8':
                return 8;

            case '9':
                return 9;

            case 'A':
            case 'a':
                return 10;

            case 'B':
            case 'b':
                return 11;

            case 'C':
            case 'c':
                return 12;

            case 'D':
            case 'd':
                return 13;

            case 'E':
            case 'e':
                return 14;

            case 'F':
            case 'f':
                return 15;
        }
        throw new ArgumentException();
    }

}