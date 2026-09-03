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
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.Records.Core.Utilities.Extensions;
using Google.Apis.Admin.Reports.reports_v1.Data;
using Google.Apis.Drive.v3.Data;
using RAGoogle.Common;
using RAGoogle.RecordsDisposal.Action.ExportOnly;

namespace RAGoogle.Models;

public class GoogleItemData
{
    public string Id { get; set; }
    private Guid uniqueId = Guid.Empty;
    public Guid UniqueId
    {
        get
        {
            if (uniqueId == Guid.Empty)
            {
                uniqueId = $"{DriveId}/{Id}".ToMd5();
            }
            return uniqueId;
        }
    }
    public string Name { get; set; }
    public string RelativePath { get; set; }
    public string Path { get; set; }
    public string FileExtension { get; set; }
    public string MimeType { get; set; }
    public bool? HasAugmentedPermissions { get; set; }
    public bool AllowFileDiscovery { get; set; }
    public string DriveName { get; set; }
    public string ParentId { get; set; }
    public string ParentIds { get; set; }
    public RMNodeLevel Level { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public string CreatedBy { get; set; }
    public string ModifierName { get; set; }
    public string ModifiedBy { get; set; }
    public string TenantId { get; set; }
    public string MemberEmail { get; set; }
    public long? Size { get; set; }
    public List<string> LableIds { get; set; }
    public GoogleItemMetaInfo MetaInfo { get; set; }
    public string DriveId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public List<Revision> Versions { get; set; }
    public string DestinationPath { get; set; }
    public string Description { get; set; }
    public List<Permissions> Permissions { get; set; }
    public string ModifiedByEmail { get; set; }
    public string WebViewLink { get; set; }

}

public class FeedItemInfo
{
    public string ItemId { get; set; }
    public DateTime EventTime { get; set; }
    public Activity Activity { get; set; }
}

public class GoogleItemMetaInfo
{
    public string DocId { get; set; }
    public List<LabelMetaInfo> Labels { get; set; }
    public long FileSize { get; set; }
    //drive info
    public string DriveId { get; set; }
    public string TenantId { get; set; }
    public string DriveName { get; set; }
    public string ModifiedByEmail { get; set; }
}
public class LabelMetaInfo
{
    public string Id { get; set; }
    public string Title { get; set; }
    public long CreatedTime { get; set; }

    public List<FieldMetaInfo> FieldInfos { get; set; }
}

public class FieldMetaInfo
{
    public string Id { get; set; }
    public string Title { get; set; }
    public FieldValueType ValueType { get; set; }
    public List<string> Values { get; set; }
}
public class Permissions
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
}

public enum FieldValueType
{
    dateString,
    integer,
    selection,
    text,
    user
}
