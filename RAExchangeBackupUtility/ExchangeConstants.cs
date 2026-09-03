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




namespace ExchangeUtility
{
    using AvePoint.GCommon.Utility.Cryptography;
    using System.Text;

    public class ExchangeConstants
    {
        public const char PathParser = (char)0x12;
        public static readonly string PathStarts = ((char)0x12).ToString() + ((char)0x12).ToString();
        public const string PathCombine = "\\";
        public const char PathCombineChar = '\\';
        public const long FolderSize = 51200;
        public const string DeleteStatus = "Delete";
        //public const string DefaultServiceUrl = "https://outlook.office365.com/EWS/Exchange.asmx";
        //public const string CNConnectionUrl = "https://partner.outlook.cn/PowerShell/";
        //public const string CommonConnectionUrl = "https://ps.outlook.com/powershell/";
        //public const string InitialDomainNameSuffixCN = "partner.onmschina.cn";
        public const string ShellUrl = "http://schemas.microsoft.com/powershell/Microsoft.Exchange";
        public const string InPlaceArchiveMailbox = "In-Place Archive Mailbox";
        public const string ResourceMailbox = "Resource Mailbox";
        //public const string EwsResourceUrl = "https://outlook.office365.com";
        //public const string GraphResourceUrl = "https://graph.windows.net";
        //public const string MicrosoftGraphResourceUrl = "https://graph.microsoft.com";
        public const string IMPERSONATION_HEADER_NAME = "X-AnchorMailbox";
        
        public const string CALENDAR_LOGGING = "\ufffeRecoverable Items\ufffeCalendar Logging";
        public const string ERRORMESSAGE_AUDITS_FOLDER = "Access is denied. Check credentials and try again., Non-system logon cannot access Audits folder.";

        #region Exception Constants
        public const string ERRORMESSAGE_GROUP_NONEUSER = "This group does not have any owners or members,please add it and try again";
        #endregion

        public static string ConvertItemId(string itemId)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
            byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(itemId));
            string idValue = string.Empty;
            char[] HEXChar = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            for (int i = 0; i < 4; i++)
            {
                byte t = result[i];
                idValue += HEXChar[(int)((t >> 4) & 0x0f)];
                idValue += HEXChar[(int)(t & 0x0f)];
            }
            return idValue;
        }
    }
}
