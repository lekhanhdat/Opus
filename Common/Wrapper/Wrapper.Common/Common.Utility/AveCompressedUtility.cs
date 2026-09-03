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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Text;
using System.Threading;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Common
{
    public class AveCompressedUtility
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveCompressedUtility));

        public static string GetTCompressedString(byte[] buffer)
        {
            string str = string.Empty;
            if (!IsTCompressedBytes(buffer))
            {
                //当从07升级到10的情况下，可能出现一些compress字段的值，是Unicode编码的。经过分析，如果是Unicode编码的，buffer字节数组的偶数位基本上是0x00的。
                //在此以前四个字节的偶数位为0x00情况下，判定为Unicode编码。
                if (buffer != null && buffer.Length > 4 && buffer[1] == 0x00 && buffer[3] == 0x00)
                {
                    return Encoding.Unicode.GetString(buffer);
                }
                return Encoding.UTF8.GetString(buffer);
            }
            int len = 0;
            for (int i = 3; i >= 0; --i)
            {
                len <<= 8;
                len += buffer[i + 8];
            }
            byte[] temp = new byte[len];
            using (MemoryStream ms = new MemoryStream(buffer, 12, buffer.Length - 12))
            {
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    ms.ReadByte();
                    ms.ReadByte();
                    ds.ReadEx(temp, 0, len);
                    str = Encoding.UTF8.GetString(temp);
                }
            }
            return str;
        }

        public static bool IsTCompressedBytes(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 12)
            {
                return false;
            }
            if (buffer[0] == 0xA8 && buffer[1] == 0xA9 && buffer[2] == 0x30 && buffer[3] == 0x31
                && buffer[4] == 0x0C && buffer[5] == 0x00 && buffer[6] == 0x00 && buffer[7] == 0x00)
            {
                return true;
            }
            return false;
        }

        public static Hashtable GetMetaInfoHashtable(String metaInfoString)
        {
            Hashtable hash = new Hashtable();
            try
            {
                MetaInfoHandler metaInfoHandler = new MetaInfoHandler(metaInfoString);
                hash = metaInfoHandler.ToDictionary();
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.ERROR, "Convert metaInfo string {0} to dictionary failed:{1}", metaInfoString, e);
            }
            return hash;
        }

        /// <summary>
        /// 
        /// https://msdn.microsoft.com/en-us/library/jj593676(v=office.12).aspx
        /// 
        ///  METADICT-VALUE = "T" METADICT-CONSTRAINT-CHAR "|" TIME
        ///  / "V" METADICT-CONSTRAINT-CHAR "|" METADICT-STRING-VECTOR
        ///  / "B" METADICT-CONSTRAINT-CHAR "|" BOOLEAN
        ///  / "I" METADICT-CONSTRAINT-CHAR "|" INT
        ///  / "U" METADICT-CONSTRAINT-CHAR "|" METADICT-INT-VECTOR
        ///  / "D" METADICT-CONSTRAINT-CHAR "|" DOUBLE
        ///  / "S" METADICT-CONSTRAINT-CHAR "|" STRING
        /// 
        ///  X: The client MUST ignore the value.
        ///  R: The client MAY read the value but MUST NOT write the value.
        ///  W: The client MAY read or write the value.
        /// </summary>
        /// <param name="metaInfoString"></param>
        /// <returns></returns>
        public static Hashtable GetMetaInfoWithType(string metaInfoString)
        {
            Hashtable properties = new Hashtable();
            string[] mSplitedString = metaInfoString.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string mStr in mSplitedString)
            {
                try
                {
                    int index1 = mStr.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                    int index2 = mStr.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    if (index1 < 0 || index2 < 0)
                    {
                        continue;
                    }
                    string key = mStr.Substring(0, index1);
                    string type = mStr.Substring(index1 + 1, index2 - index1 -1 );
                    string value = mStr.Substring(index2 + 1);
                    properties[key] = ConvertString(type, value);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "convert meta info string:{0} to key and value failed:{1}", mStr, e);
                    continue;
                }
            }
            return properties;
        }

        private static object ConvertString(string type, string value)
        {
            if((!string.IsNullOrEmpty(type)) && type.Length > 0)
            {
                switch(type[0])
                {
                    case 'B':
                        {
                            bool boolValue;
                            if (bool.TryParse(value, out boolValue))
                            {
                                return boolValue;
                            }
                        }
                        break;
                    case 'I':
                        {
                            int intValue;
                            if (int.TryParse(value, out intValue))
                            {
                                return intValue;
                            }
                        }
                        break;
                    case 'D':
                        {
                            double doubleValue;
                            if (double.TryParse(value, out doubleValue))
                            {
                                return doubleValue;
                            }
                        }
                        break;
                }
            }

            return value;
        }

        public static Dictionary<string, string> GetMetaInfoDictionary(string metaInfoString)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] mSplitedString = metaInfoString.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string mStr in mSplitedString)
            {
                try
                {
                    int index1 = mStr.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                    int index2 = mStr.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    if (index1 < 0 && index2 < 0)
                    {
                        continue;
                    }
                    string key = index1 > 0 ? mStr.Substring(0, index1) : mStr.Substring(0, index2);
                    string value = index2 > 0 ? mStr.Substring(index2 + 1) : String.Empty;
                    dic[key] = value;
                }
                catch(Exception e) 
                {
                    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetMetadataInfoDicError, e.ToString());
                    continue;
                }
            }
            return dic;
        }
        public static Dictionary<Dictionary<string, string>, string> GetMetaInfoDictionaryWithSeparator(string metaInfoString)
        {
            Dictionary<Dictionary<string, string>, string> dic = new Dictionary<Dictionary<string, string>, string>();
            string[] mSplitedString = metaInfoString.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string mStr in mSplitedString)
            {
                try
                {
                    Dictionary<string, string> tempdic = new Dictionary<string, string>();
                    int index1 = mStr.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                    int index2 = mStr.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    if (index1 < 0 && index2 < 0)
                    {
                        continue;
                    }
                    string key = index1 > 0 ? mStr.Substring(0, index1) : mStr.Substring(0, index2);
                    string value = index2 > 0 ? mStr.Substring(index2 + 1) : String.Empty;
                    string separator = index2 > index1 ? mStr.Substring(index1, index2 - index1 + 1) : string.Empty;
                    tempdic.Add(key, value);
                    dic.Add(tempdic, separator);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetMetadataInfoDicError, e.ToString());
                    continue;
                }
            }
            return dic;
        }
        public static byte[] GetTCompressedBytes(string str)
        {
            byte[] temp = Encoding.UTF8.GetBytes(str);
            byte[] head = new byte[12];
            head[0] = (byte)168;
            head[1] = (byte)169;
            head[2] = (byte)48;
            head[3] = (byte)49;
            head[4] = (byte)12;
            AveConvert.ToBytes(temp.Length, head, 8);
            using (MemoryStream ms = new MemoryStream())
            {
                ZLibStream zs = new ZLibStream(ms, CompressionMode.Compress);
                zs.Write(temp, 0, temp.Length);
                zs.Close();
                byte[] buffer = (byte[])ms.ToArray();

                byte[] compressedData = new byte[12 + buffer.Length];
                Array.Copy(head, 0, compressedData, 0, 12);
                Array.Copy(buffer, 0, compressedData, 12, buffer.Length);
                return compressedData;
            }
        }

        public static string GetStringFromBase64String(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }

        public static Dictionary<string, string> ModifyMetaInfoString(Hashtable metaInfo, string metaInfoString)
        {
            var fileHoldValue = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var splitedStrings = metaInfoString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string splitedStr in splitedStrings)
            {
                try
                {
                    int index1 = splitedStr.IndexOf(":", StringComparison.Ordinal);
                    int index2 = splitedStr.IndexOf("|", StringComparison.Ordinal);
                    if (index1 < 0 && index2 < 0)
                    {
                        continue;
                    }
                    string key = index1 > 0 ? splitedStr.Substring(0, index1) : splitedStr.Substring(0, index2);
                    string value = index2 > 0 ? splitedStr.Substring(index2 + 1) : String.Empty;
                    switch (key)
                    {
                        case "_vti_ItemHoldRecordStatus":
                            fileHoldValue.Add(key, value);
                            metaInfo[key] = "0";
                            break;
                        case "ecm_ItemLockHolders":
                        case "ecm_ItemDeleteBlockHolders":
                        case "_dlc_Holds_Property":
                        case "IconOverlay":
                        case "ecm_RecordRestrictions":
                        case "_vti_ItemDeclaredRecord":
                            fileHoldValue.Add(key, value);
                            metaInfo[key] = string.Empty;
                            break;
                        default:
                            metaInfo[key] = value;
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Warn("modify file hold lock error: " + e.ToString());
                    continue;
                }
            }

            if (!fileHoldValue.ContainsKey("_vti_ItemHoldRecordStatus"))
            {
                fileHoldValue.Add("_vti_ItemHoldRecordStatus", null);
            }
            if (!fileHoldValue.ContainsKey("ecm_ItemLockHolders"))
            {
                fileHoldValue.Add("ecm_ItemLockHolders", null);
            }
            if (!fileHoldValue.ContainsKey("ecm_ItemDeleteBlockHolders"))
            {
                fileHoldValue.Add("ecm_ItemDeleteBlockHolders", null);
            }
            if (!fileHoldValue.ContainsKey("_dlc_Holds_Property"))
            {
                fileHoldValue.Add("_dlc_Holds_Property", null);
            }
            if (!fileHoldValue.ContainsKey("IconOverlay"))
            {
                fileHoldValue.Add("IconOverlay", null);
            }
            if (!fileHoldValue.ContainsKey("_vti_ItemDeclaredRecord"))
            {
                fileHoldValue.Add("_vti_ItemDeclaredRecord", null);
            }
            if (!fileHoldValue.ContainsKey("ecm_RecordRestrictions"))
            {
                fileHoldValue.Add("ecm_RecordRestrictions", null);
            }
            return fileHoldValue;
        }

        public static void ModifyMetaInfoForLock(string key, ref string value, Dictionary<string, string> fileHoldValue)
        {
            switch (key)
            {
                case "_vti_ItemHoldRecordStatus":
                    fileHoldValue.Add(key, value);
                    value = "0";
                    break;
                case "ecm_ItemLockHolders":
                case "ecm_ItemDeleteBlockHolders":
                case "_dlc_Holds_Property":
                case "IconOverlay":
                case "ecm_RecordRestrictions":
                case "_vti_ItemDeclaredRecord":
                    fileHoldValue.Add(key, value);
                    value = string.Empty;
                    break;
                default:
                    break;
            }
        }
    }

}
