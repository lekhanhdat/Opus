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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CABrokenLinkCheckOperation : CAOperation
    {
        [DataMember]
        public CheckFilter Filter { get; set; }

        [DataMember]
        public int TotalLinkCount { get; set; }
        [DataMember]
        public int InternalLinkCount { get; set; }
        [DataMember]
        public int OuterLinkCount { get; set; }

        [DataMember]
        public int CheckedLinkCount { get; set; }
        [DataMember]
        public int HypCheckedLinkCount { get; set; }
        [DataMember]
        public int ImgCheckedLinkCount { get; set; }
        [DataMember]
        public int DocCheckedLinkCount { get; set; }
        [DataMember]
        public int OthCheckedLinkCount { get; set; }

        [DataMember]
        public int BrokenLinkCount { get; set; }
        [DataMember]
        public int HypBrokenLinkCount { get; set; }
        [DataMember]
        public int ImgBrokenLinkCount { get; set; }
        [DataMember]
        public int DocBrokenLinkCount { get; set; }
        [DataMember]
        public int OthBrokenLinkCount { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LinkInfo:ResultBase
    {
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string ParentUrl { get; set; }
        [DataMember]
        public CheckStatus Status { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public CAStringFormatMessage FormatComment { get; set; }
        [DataMember]
        public string Protocol { get; set; }
        [DataMember]
        public LinkType TypeOfLink { get; set; }
        [DataMember]
        public string ContentType { get; set; }
        [DataMember]
        public string Size { get; set; }
        [DataMember]
        public string CharSet { get; set; }
        [DataMember]
        public string AccessTime { get; set; }
        [DataMember]
        public bool IsInternal { get; set; }
        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public int Depth { get; set; }
        [DataMember]
        public string EmailAddress { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckFilter
    {
        [DataMember]
        public int CheckInternalDepth { get; set; }
        [DataMember]
        public int CheckExternalDepth { get; set; }
        [DataMember]
        public int RecheckTimes { get; set; }
        [DataMember]
        public LinkType AllowedType { get; set; }
        [DataMember]
        public string CheckUrls { get; set; }
        [DataMember]
        public CheckType CheckType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CheckStatus
    {
        [EnumMember]
        NotChecked,
        [EnumMember]
        Checked,
        [EnumMember]
        Skipped,
        [EnumMember]
        Broken,
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LinkType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Href = 1,
        [EnumMember]
        Src = 2,
        [EnumMember]
        Document = 4,
        [EnumMember]
        Email = 8,
        [EnumMember]
        Other = 16,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CheckType
    {
        [EnumMember]
        Skip = 0,
        [EnumMember]
        Match = 1,
        [EnumMember]
        None = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckBrokenOverview
    {
        [DataMember]
        public int TotalUrl { get; set; }
        [DataMember]
        public int CheckedUrl { get; set; }
        [DataMember]
        public int BrokenUrl { get; set; }
        [DataMember]
        public int ImgUrl { get; set; }
        [DataMember]
        public int HrefUrl { get; set; }
        [DataMember]
        public int OtherUrl { get; set; }
        [DataMember]
        public int InternalUrl { get; set; }
        [DataMember]
        public int ExternalUrl { get; set; }
    }

}
