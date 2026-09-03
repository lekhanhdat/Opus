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


namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EORestoreCacheTreeContract
    {
        [DataMember]
        public List<EORestoreCacheTreeDto> CacheTrees { get; set; }

        [DataMember]
        public string BackupJobId { get; set; }

        [DataMember]
        public long BackupTime { get; set; }

        /// <summary>Data import时,media发送的FarmId是media根据Name计算来的,代表不了唯一性,
        /// Control根据FarmName找真正FarmId.</summary>
        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public MediaAction Action { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EORestoreCacheTreeDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string FullPath { get; set; }

        /// <summary>
        /// 节点的FarmId,在search时,区分节点属于哪棵Farm(注释:根据FarmName名字MD5计算出来的,并不代表真正的FarmId).
        /// </summary>
        [DataMember]
        public string FarmId { get; set; }

        /// <summary>
        /// 根据FullPath的一个Hash Code的计算算法,计算出的NodeID.
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string WebApplicationId { get; set; }

        /// <summary>
        /// farm level时,ParentId is null.
        /// </summary>
        [DataMember]
        public string ParentId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public NodeType Type { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOBackupJobsMappingDto
    {
        /// <summary>
        /// 和RestoreCacheTree表Id关联
        /// </summary>
        [DataMember]
        public string TreeId { get; set; }

        /// <summary>
        /// 和Job表Id关联
        /// </summary>
        [DataMember]
        public string BackupJobId { get; set; }

        [DataMember]
        public long BackupTime { get; set; }
    }

    public class RestoreCacheTreeSavedInfo
    {
        public EORestoreCacheTreeDto CacheTreeDto { get; set; }

        public bool StoragedInCacheTree { get; set; }

        public bool StoragedInMapping { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MediaAction
    {
        [EnumMember]
        Backup,
        [EnumMember]
        DataImport,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EORestoreCacheTreeSearchParams
    {
        [DataMember]
        public string SearchNodeFullPath { get; set; }

        [DataMember]
        public NodeLevel TargetLevel { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string WebApplicationId { get; set; }

        [DataMember]
        public long StartTime { get; set; }

        [DataMember]
        public long EndTime { get; set; }
    }
}
