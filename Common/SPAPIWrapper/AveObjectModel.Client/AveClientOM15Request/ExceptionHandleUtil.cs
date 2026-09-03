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
using System.Net;
using System.Text;
using Microsoft.SharePoint.Client;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ClientOM
{
    class ExceptionHandleUtil
    {
        private static List<int> ShouldThrowErrorCodes = new List<int>() { -2147023080 };

        public static void HandleServerException(ServerException e)
        {
            if (ShouldThrowErrorCodes.Contains(e.ServerErrorCode))
            {
                throw e;
            }
        }

        public static void HandleWebException(WebException we)
        {
            //507 is throwed when the site exceed its storage limit
            if (we != null && we.Response != null && (int)(we.Response as HttpWebResponse).StatusCode == 507)
            {
                throw we;
            }            
        }

        public static SPCommonException ConvertServerException(ServerException e)
        {
            return SPCommonException.CreateFromErrorInfo(e.Message, e.ServerStackTrace, e.ServerErrorCode, e.ServerErrorValue, e.ServerErrorTypeName, e.ServerErrorDetails, e.ServerErrorTraceCorrelationId);
        }
        public static bool HandleBatchExecuteException(ServerException se, ref int defaultQueryItemCount, ref int currentQueryItemCount)
        {
            bool needIncreaseIndex = false;
            switch (se.ServerErrorCode)
            {
                case AveSPErrorCode.TP_E_LISTDELETED:
                case AveSPErrorCode.TP_E_FIELDNOTFOUND:
                    throw se;
                case AveSPErrorCode.ERROR_SHARING_BUFFER_EXCEEDED:
                case AveSPErrorCode.V_OWSSVR_CLICK_MENU:
                    //The attempted operation is prohibited because it exceeds the list view threshold enforced by the administrator.
                    if (defaultQueryItemCount > 1)
                    {
                        //attempt to find the most appropriate number
                        defaultQueryItemCount /= 2;
                    }
                    break;
                case AveSPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST:
                    if (currentQueryItemCount == 0)
                    {
                        needIncreaseIndex = true;
                        currentQueryItemCount = defaultQueryItemCount;
                    }
                    break;
                default:
                    break;
            }
            return needIncreaseIndex;
        }
    }
}
