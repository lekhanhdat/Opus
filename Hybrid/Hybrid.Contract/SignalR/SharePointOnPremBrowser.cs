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
using CommonModel.MethodInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.Contract.SignalR
{
    public class SharePointOnPremBrowser : RemoteMessage<SharePointOnPremBrowserArgs>
    {
        public override SharePointOnPremBrowserArgs MethodArgs { get; set; }

        public override string MethodName => throw new NotImplementedException();
    }

    public class SharePointOnPremBrowserExecute : RemoteInvoke<SharePointOnPremBrowserArgs, SharePointOnPremBrowserResult>
    {
        public override SharePointOnPremBrowserArgs MethodArgs { get; set; }
        public override SharePointOnPremBrowserResult MethodResult { get; set; }

        public override string MethodName => MethodMapping.MT[typeof(SharePointOnPremBrowserExecute)];
    }

    public class SharePointOnPremBrowserArgs
    {
        public string BatchId { get; set; }

        public string Message { get; set; }
    }

    public class SharePointOnPremBrowserResult
    {
        public SharePointOnPremBrowserResultEnum Result { get; set; }
        public string Message { get; set; }
    }

    public enum SharePointOnPremBrowserResultEnum
    {
        Successed,
        Failed,
    }
}
