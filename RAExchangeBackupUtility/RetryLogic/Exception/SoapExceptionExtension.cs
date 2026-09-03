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
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Services.Protocols
{
    static class SoapExceptionExtension
    {
        //SoapException.Detail
        //<?xml version="1.0" encoding="utf-8"?><s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><s:Fault><faultcode xmlns:a="http://schemas.microsoft.com/exchange/services/2006/types">a:ErrorServerBusy</faultcode><faultstring xml:lang="en-US">The server cannot service this request right now. Try again later.</faultstring><detail><e:ResponseCode xmlns:e="http://schemas.microsoft.com/exchange/services/2006/errors">ErrorServerBusy</e:ResponseCode><e:Message xmlns:e="http://schemas.microsoft.com/exchange/services/2006/errors">The server cannot service this request right now. Try again later.</e:Message><t:MessageXml xmlns:t="http://schemas.microsoft.com/exchange/services/2006/types"><t:Value Name="BackOffMilliseconds">299984</t:Value></t:MessageXml></detail></s:Fault></s:Body></s:Envelope>
        //可以通过解析exception.Detail获取BackOffMilliseconds
        public static bool IsServerBusyException(this SoapException exception)
        {
            return exception.GetErrorCode() == ServiceError.ErrorServerBusy;
        }

        /// <summary>
        /// 将SoapException中的Code转换成ExchangeService对应的ErrorCode
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>-1: SoapException为空或者无法转换成ServiceError</returns>
        public static ServiceError GetErrorCode(this SoapException exception)
        {
            var errorCode = (ServiceError)(-1);
            if (exception.Code != null && !string.IsNullOrEmpty(exception.Code.Name))
            {
                Enum.TryParse(exception.Code.Name, out errorCode);
            }
            return errorCode;
        }
    }
}
