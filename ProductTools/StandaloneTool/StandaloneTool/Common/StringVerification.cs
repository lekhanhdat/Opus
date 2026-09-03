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
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace StandaloneTool.Common
{
    public class StringVerification
    {

        private ArrayList GetDiskList()
        {
            ArrayList aList = new ArrayList();

            foreach (DriveInfo item in DriveInfo.GetDrives())
            {
                if (item.DriveType == DriveType.Fixed)
                    aList.Add(item.Name.Replace('\\', ' ').Trim());
            }

            return aList;
        }

        private bool VerifyDisk(string path)
        {
            ArrayList aList = GetDiskList();
            string a, b, c, d;
            a = aList[0].ToString().ToLower().Substring(0, 1);
            b = aList[aList.Count - 1].ToString().ToLower().Substring(0, 1);
            c = aList[0].ToString().Substring(0, 1);
            d = aList[aList.Count - 1].ToString().Substring(0, 1);
            //string regexpath = @"^([c-zC-Z]\:|\\)\\([^\\]+\\)*[^\/:*?<>|]";
            string regexpath = @"^[c-zC-Z]:(([c-zC-Z]*)||([c-zC-Z]*\\))*";
            string replacestr = a + "-" + b + c + "-" + d;
            regexpath = regexpath.Replace("c-zC-Z", replacestr);
            bool result = Regex.IsMatch(path, regexpath);
            return result;
        }


        public bool VerifyDirectory(string item)
      {
            bool result = true;

            if (item.StartsWith(@"\\")) //Check shared folder for netshare
            {
                if (item.Length == 2 || item.Substring(2).Contains(@"\\")) return false;
                return result;
            }

            if (item.ToLower().Trim().Split(':').Length > 2 || item.ToLower().Trim().Length < 3)
            {
                result = false;
            }
            if (item.ToLower().Trim().Split("/*?\"<>|".ToCharArray()).Length > 1)
            {
                result = false;
            }
            if (!VerifyDisk(item.ToLower().Trim()))
            {
                result = false;
            }

            return result;
        }

        public bool ValidatePort(string port)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(port))
            {
                port = port.Trim();
                if (ValidateNumber(port))
                {
                    int portNumber = Convert.ToInt32(port);

                    result = ValidateIntegerPort(portNumber);
                }
            }
            return result;
        }

        private bool ValidateIntegerPort(int portNumber)
        {
            bool result = false;

            if (portNumber >= 1 && portNumber <= 65535)
            {
                result = true;
            }
            return result;
        }

        public bool ValidateNumber(string item)
        {
            if (String.IsNullOrEmpty(item))
            {
                return false;
            }
            item = item.Trim();
            if (item.Length != 0)
            {
                if ((item[0] == (char)48 || item[0] == (char)65296) && item.Length != 1)
                {
                    return false;
                }
                for (int i = 0; i < item.Length; i++)
                {
                    if (!Char.IsNumber(item, i))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

    }
}
