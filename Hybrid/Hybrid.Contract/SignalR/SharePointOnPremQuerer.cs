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
    public class SharePointOnPremQuerer : RemoteMessage<SharePointOnPremQuererArgs>
    {
        public override SharePointOnPremQuererArgs MethodArgs { get; set; }

        public override string MethodName => throw new NotImplementedException();
    }

    public class SharePointOnPremQuererExecute : RemoteInvoke<SharePointOnPremQuererArgs, SharePointOnPremQuererResult>
    {
        public override SharePointOnPremQuererArgs MethodArgs { get; set; }
        public override SharePointOnPremQuererResult MethodResult { get; set; }

        public override string MethodName => MethodMapping.MT[typeof(SharePointOnPremQuererExecute)];
    }

    public class SharePointOnPremQuererArgs
    {
        public string SiteUrl { get; set; }
        public Guid SiteId { get; set; }
        public Guid WebId { get; set; }
        public Guid ListId { get; set; }
        public Guid ItemId { get; set; }

        public bool IsUsingExistColumnName { get; set; }

        public string ExistColumnName { get; set; }
    }

    public class SharePointOnPremQuererResult
    {
        public int Id { get; set; }
        public Guid FolderId { get; set; }
        public bool ParentFolderIsRootFolder { get; set; }
        public string FolderUrl { get; set; }
        public Guid UniqueId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Guid WebId { get; set; }
        public string WebUrl { get; set; }
        public Guid SiteId { get; set; }
        public string SiteUrl { get; set; }
        public int Level { get; set; }
        public Guid ListId { get; set; }
        public string WebServerRelativeUrl { get; set; }
        public string ListUrl { get; set; }
        public string ItemUrl { get; set; }
        public string RelatedRecordsInfo { get; set; }
        public string FullPath { get; set; }
        public string RecordId { get; set; }
        public bool DeclareAsRecord { get; set; }
        public string TermId { get; set; }
        public bool IsRecord { get; set; }
    }
}

