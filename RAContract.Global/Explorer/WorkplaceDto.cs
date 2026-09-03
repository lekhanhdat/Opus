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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract]
    public class WorkplaceDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public int SourceType { get; set; }
    }

    [DataContract]
    public class GetWorkspaceRequestDto
    {
        [DataMember]
        public int SourceType { get; set; }
    }
    
    [DataContract]
    public class WorkspaceRequestDto 
    {
        [DataMember]
        public string WorkplaceId { get; set; }
        [DataMember]
        public string HoldId { get; set; }
        [DataMember]
        public int SourceType { get; set; }
        [DataMember]
        public WorkspaceHoldSettingDto WorkspaceHoldSettingDto {  get; set; }
    }
    
    [DataContract]
    public class WorkspaceHoldSettingDto
    {
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public int Unit { get; set; }
        [DataMember]
        public string CalenderTime { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public bool IsDayLightSaving { get; set; }
    }
    [DataContract]
    public class WorkspaceHoldUpdateDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string HoldId { get; set; }

        [DataMember]
        public string HoldBy { get; set; }

        [DataMember]
        public WorkspaceHoldSettingDto WorkspaceHoldSettingDto { get; set; }

    }

    [DataContract]
    public class WorkspaceHoldPageRequestDto
    {
        [DataMember]
        public int PageIndex { get; set; }

        [DataMember]
        public int PageSize { get; set; }
    }

    [DataContract]
    public class WorkspaceHoldItemDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string WorkplaceId { get; set; }

        [DataMember]
        public string HoldId { get; set; }

        [DataMember]
        public string HoldBy { get; set; }

        [DataMember]
        public string HoldTitle { get; set; }

        [DataMember]
        public string WorkplaceUrl { get; set; }

        [DataMember]
        public bool IsChecked { get; set; }

        [DataMember]
        public bool IsHold { get; set; }

        [DataMember]
        public string WorkplaceName { get; set; }

        [DataMember]
        public int SourceType { get; set; }

        [DataMember]
        public string ReleaseTime { get; set; }
    }

}
