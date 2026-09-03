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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    
    public class AveSPErrorCode
    {
        public const int DefaultValue = -2146232832; 

        public const int DocumentTypeBlocked = -2147221018;

        public const int DocumentSizeExceed = -2147024872;

        public const int WebAppUsageNotEnough = -2130246262;

        public const int ListTemplateNotFound = -2130575237;

        public const int LanguagePackageNotFound = -2130575266;

        public const int SiteUsageNotEnough = -2130575282;

        public const int SiteUsageNotEnoughForDoc = -2147023080;

        public const int TP_E_CHANGE_TOKEN_TOO_EARLY = -2130575172;
        public const int TP_E_OVERQUOTA = -2130575282;
        public const int V_OVER_QUOTA = -2130245277;
        public const int ERROR_NOT_ENOUGH_QUOTA = -2147023080;
        public const int TP_E_USER_DOESNOT_EXIST = -2130575276;

        public const int FILE_NOT_FOUND = -2147024894;
        public const int CHANNEL_TIME_OUT = -2146233083;
        public const int ACCESS_DENIED = -2147024891;


        public const int TP_E_LISTDELETED = -2130575322;
    }
}
