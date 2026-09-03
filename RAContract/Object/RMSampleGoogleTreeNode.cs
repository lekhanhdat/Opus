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
using AvePoint.RA.Contract.Object.Base;
using AvePoint.RA.Contract.Schedule;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.Object;

[DataContract(IsReference = true)]
[JsonObject]
public class RMSampleGoogleTreeNode : RMBaseTreeNode<RMSampleGoogleTreeNode>, IDisposable
{
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public RMSampleGoogleTreeNode Parent { set { base.Parent = value; } get { return base.Parent; } }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public List<RMSampleGoogleTreeNode> Children { set { base.Children = value; } get { return base.Children; } }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string ObjectId { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string ContainerId { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string NodeId { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string SearchKey { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool IsSearch { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string GoogleTenantId { get; set; }
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string DriveId { get; set; }
    public void Dispose()
    {
        try
        {
            foreach (var child in Children)
            {
                using (child as IDisposable)
                { }
            }
            Children = null;
        }
        catch
        { //Noncompliant
        }
    }

    #region Google One
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool IsEnableClassification { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool IsEnableLifeCycleManagement { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string Plan { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public ScheduleInfo ScheduleInfo { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public BrowsePager BrowsePager { get; set; }
    #endregion
}

public class BrowsePager
{
    public int PageSize { get; set; }

    public int PageIndex { get; set; }

    public int TotalCount { get; set; }

    public bool HasNext { get; set; }

    public string SearchText { get; set; }

    public BrowseOrder Order { get; set; }

    public List<BrowseFilter> Filters { get; set; }
}

public class BrowseFilter
{
    public string ColumnName { get; set; }

    public string ColumnValue { get; set; }
}

public class BrowseOrder
{
    public string OrderByColumn { get; set; }

    public bool OrderByDesc { get; set; } = false;
}