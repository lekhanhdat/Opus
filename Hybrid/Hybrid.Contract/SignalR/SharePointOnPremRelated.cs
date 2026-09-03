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
using CommonModel.MethodInfo;

namespace AvePoint.Hybrid.Contract.SignalR
{
    public class SharePointOnPremRelated : RemoteMessage<SharePointOnPremRelatedArgs>
    {
        public override SharePointOnPremRelatedArgs MethodArgs { get; set; }

        public override string MethodName => throw new NotImplementedException();
    }

    public class SharePointOnPremRelatedExecute : RemoteInvoke<SharePointOnPremRelatedArgs, SharePointOnPremRelatedResult>
    {
        public override SharePointOnPremRelatedArgs MethodArgs { get; set; }
        public override SharePointOnPremRelatedResult MethodResult { get; set; }

        public override string MethodName => MethodMapping.MT[typeof(SharePointOnPremRelatedExecute)];
    }

    public class SharePointOnPremRelatedArgs
    {

        public Guid SiteId { get; set; }
        public string SiteUrl { get; set; }
        public Guid WebId { get; set; }
        public string WebUrl { get; set; }
        public Guid ListId { get; set; }
        public Guid ItemId { get; set; }
        public int ItemRowId { get; set; }
        public string Name { get; set; }
        public string RelatedItemInfo { get; set; }
    }

    public class SharePointOnPremRelatedResult
    {
        public string Message { get; set; }
    }

}

