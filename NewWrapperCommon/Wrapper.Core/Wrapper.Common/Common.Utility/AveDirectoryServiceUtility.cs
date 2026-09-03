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
using System.Text;
using System.DirectoryServices;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Globalization;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    public class AveDirectoryServiceUtility
    {

        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetAccountFromSid(string stringSid, AveObjectModelFactory modelFactory)
        {
            try
            {
                byte[] bSid = ConvertStringSidToBytes(stringSid);
                if (bSid != null)
                {
                    return modelFactory.CreatePeopleEditor().GetAccountFromSid(bSid);
                }
            }
            catch(Exception ex)
            {
                log.Log(AveLogLevel.WARN,WrapperCommonResource.AWCGetAccountFromSTSidToBtsError, stringSid, ex.ToString());
            }
            return string.Empty;
        }

        public static string GetSidFromAccount(string account)
        {
            string sid = string.Empty;
            // Parse the string to check if domain name is present.
            int idx = account.IndexOf('\\');
            if (idx == -1)
            {
                idx = account.IndexOf('@');
            }
            string strDomain = string.Empty;
            string strName = string.Empty;
            if (idx != -1)
            {
                strDomain = account.Substring(0, idx);
                strName = account.Substring(idx + 1);
            }
            else
            {
                strDomain = Environment.MachineName;
                strName = account;
            }
            DirectoryEntry obDirEntry = null;
            try
            {
                Int64 iBigVal = 5;
                Byte[] bigArr = BitConverter.GetBytes(iBigVal);
                obDirEntry = new DirectoryEntry("WinNT://" + strDomain + "/" + strName);
                System.DirectoryServices.PropertyCollection coll = obDirEntry.Properties;
                object obVal = coll["objectSid"].Value;
                if (null != obVal)
                {
                    sid = ConvertBytesToStringSid((Byte[])obVal);
                }
                return sid;
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCGetSTSidToBtsFromAccountError, account, ex.ToString());
            }
            finally
            {
                if (obDirEntry != null)
                {
                    obDirEntry.Dispose();
                }
            }
            return string.Empty;
        }

        public static bool IsStringSid(string value)
        {
            if (!value.StartsWith("s-", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            byte[] temp = ConvertStringSidToBytes(value);
            if (temp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /*
         <Domain/Machine>\Administrator
         ' Pos : 0 | 1 | 2 3 4 5 6 7 | 8 9 10 11 | 12 13 14 15 | 16 17 18 19 | 20 21 22 23 | 24 25 26 27
         ' Value: 01 | 05 | 00 00 00 00 00 05 | 15 00 00 00 | 06 4E 7D 7F | 11 57 56 7A | 04 11 C5 20 | F4 01 00 00
         ' str : S- 1 | | -5 | -21 | -2138918406 | -2052478737 | -549785860 | -500

         ' SID anatomy:
         >> ' Byte Position
         >> ' 0 : SID Structure Revision Level (SRL)
         >> ' 1 : Number of Subauthority/Relative Identifier
         >> ' 2-7 : Identifier Authority Value (IAV) [48 bits]
         >> ' 8-x : Variable number of Subauthority or Relative Identifier (RID)
         >> [32 bits]
         */
        public static byte[] ConvertStringSidToBytes(string sid)
        {
            try
            {
                sid = sid.ToLower(CultureInfo.InvariantCulture).Replace("s", "");

                string[] list = sid.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("{0:X2}", Convert.ToInt32(list[0])));
                sb.Append(String.Format("{0:X2}", Convert.ToInt32(list.Length - 2)));
                sb.Append(String.Format("{0:X12}", Convert.ToInt32(list[1])));
                for (int i = 2; i < list.Length; i++)
                {
                    string tmp = String.Format("{0:X8}", Convert.ToUInt32(list[i]));
                    for (int j = 6; j >= 0; j -= 2)
                    {
                        sb.Append(tmp.Substring(j, 2));
                    }
                }
                string obj = sb.ToString();
                byte[] ret = new byte[obj.Length / 2];
                for (int i = 0; i < obj.Length; i += 2)
                {
                    ret[i / 2] = Convert.ToByte(obj.Substring(i, 2), 16);
                }
                return ret;
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCConverSTSidToBtsError, ex.ToString());
            }
            return null;
        }

        public static string ConvertBytesToStringSid(Byte[] sidBytes)
        {
            StringBuilder strSid = new StringBuilder();
            strSid.Append("S-");
            try
            {
                strSid.Append(sidBytes[0]);
                int subCount = sidBytes[1];
                Int64 iVal = (Int32)(sidBytes[2] << 40) +
                        (Int32)(sidBytes[3] << 32) +
                        (Int32)(sidBytes[4] << 24) +
                        (Int32)(sidBytes[5] << 16) +
                        (Int32)(sidBytes[6] << 8) +
                        (Int32)(sidBytes[7]);
                strSid.Append("-");
                strSid.Append(iVal.ToString());
                for (int i = 0; i < subCount; i++)
                {
                    UInt32 iSubAuth = BitConverter.ToUInt32(sidBytes, 8 + i * 4);
                    strSid.Append("-");
                    strSid.Append(iSubAuth.ToString());
                }
                return strSid.ToString();
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCConverBtToSTSidError, ex.ToString());
            }
            return string.Empty;
        }
    }
}
