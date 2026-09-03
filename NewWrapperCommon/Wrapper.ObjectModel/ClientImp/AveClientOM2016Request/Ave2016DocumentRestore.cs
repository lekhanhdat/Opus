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
using AveClientRequest.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Application;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2016DocumentRestore : Ave2013DocumentRestore, IDisposable
    {
        protected AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 为unittest添加构造函数
        /// </summary>
        public Ave2016DocumentRestore() { }

        public Ave2016DocumentRestore(AveClientOM2016Request request, Site site, object obj, AveClientContext conText, string serverVersion, IReport report)
            : base(request, site, obj, conText, serverVersion,report)
        {
        }

        public void Dispose()
        {
            base.Dispose();
            //TODO
        }
    }
}