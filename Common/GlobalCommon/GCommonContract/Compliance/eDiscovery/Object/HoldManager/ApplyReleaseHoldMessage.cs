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



using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object.HoldManager
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Storage.Entity;
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ApplyReleaseHoldMessage : HoldBaseMessage
    {

        #region apply hold 传的参数

        /// <summary>
        /// 要做hold的file列表
        /// </summary>
        [DataMember]
        public List<ReleaseHoldFile> HoldFiles { get; set; }

        /// <summary>
        /// 被哪些Hold Item进行hold
        /// </summary>
        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }

        #endregion

        #region release 传的参数

        /// <summary>
        /// 要Release的files
        /// </summary>
        [DataMember]
        public List<HoldItemDto> ReleaseHoldItems { get; set; }

        #endregion

        #region Real Time 传的参数

        /// <summary>
        /// real time中，需要hold的文件集合
        /// </summary>
        [DataMember]
        public List<RealTimeFile> RealTimeHoldFiles { get; set; }

        /// <summary>
        /// real time 中需要release的文件集合
        /// </summary>
        [DataMember]
        public List<RealTimeFile> RealTimeReleaseFiles { get; set; }

        /// <summary>
        /// 做Release的类型
        /// </summary>
        [DataMember]
        public RealTimeType RealTimeType { get; set; }

        /// <summary>
        /// 产生的sub job的id列表
        /// </summary>
        [DataMember]
        public List<string> SubJobIds { get; set; }

        #endregion

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OffLineHoldMessage : HoldBaseMessage
    {
        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }
        [DataMember]
        public string HoldJobId { get; set; }
        [DataMember]
        public string SearchJobId { get; set; }
        [DataMember]
        public PhysicalDeviceDto SearchResultLocation { get; set; }
    }

    /// <summary>
    /// 改名！
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReleaseHoldFile
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string SiteCollectionId { get; set; }

        [DataMember]
        public string WebId { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public string DataGuid { get; set; }

        [DataMember]
        public string ListId { get; set; }

        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }

        [DataMember]
        public int DataSource { get; set; }


        public ReleaseHoldFile()
        {
        }

        public ReleaseHoldFile(HeldFileDto heldFile)
        {
            this.SiteCollectionId = heldFile.SiteCollectionId;
            this.WebId = heldFile.WebId;
            this.Location = heldFile.Location;
            this.DataGuid = heldFile.DataGuid;
            this.ListId = heldFile.ListId;
            this.Id = heldFile.Id;
            this.DataSource = (int)heldFile.DataSourceType;
        }

        public ReleaseHoldFile(SearchResult searchResult)
        {
            this.SiteCollectionId = searchResult.SiteId.ToString();
            this.WebId = searchResult.WebId.ToString();
            this.Location = searchResult.Location;
            this.DataGuid = searchResult.ItemId.ToString();
            this.ListId = searchResult.ListId.ToString();
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RealTimeFile : ReleaseHoldFile
    {
        /// <summary>
        /// 表示这个file是要做hold还是release
        /// </summary>
        [DataMember]
        public RealTimeFileType RealTimeFileType { get; set; }

        /// <summary>
        /// 相应的holditem
        /// </summary>
        [DataMember]
        public HoldItemDto HoldItem { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RealTimeFileType
    {
        [EnumMember]
        Hold = 0,
        [EnumMember]
        Release = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RealTimeType
    {
        [EnumMember]
        OnlyHold = 0,
        [EnumMember]
        OnlyRelease = 1,
        [EnumMember]
        Both = 2
    }


}
