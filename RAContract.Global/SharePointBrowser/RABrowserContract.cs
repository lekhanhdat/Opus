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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.SharePointBrowser
{
    [DataContract]
    public class RABrowserContract
    {
        [DataMember]
        public string LogonUserEmail { get; set; }
        [DataMember]
        public string LogonGroupId { get; set; }
        [DataMember]
        public string LogonUserId { get; set; }
        [DataMember]
        public BrowserType Type { get; set; }
        [DataMember]
        public string Message { get; set; }

        public RABrowserContract(string message, BrowserType type, string logonUserEmail, string logonGroupId, string logonUserId)
        {
            LogonUserEmail = logonUserEmail;
            LogonGroupId = logonGroupId;
            LogonUserId = logonUserId;
            Type = type;
            Message = message;
        }
    }
    [DataContract]
    public enum BrowserType
    {
        [EnumMember]
        SharePointOnline = 1,
        [EnumMember]
        ExchangeOnline = 2,
        [EnumMember]
        OneDrive = 3,
        [EnumMember]
        CheckEndUserPermission =4,
        [EnumMember]
        Teams = 5
    }
}
