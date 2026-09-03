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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Vault.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VaultJobDto : BaseJobDto
    {
        /// <summary>
        /// 主job对应的子job
        /// </summary>
        [DataMember]
        public IList<SubJobDto> SubJobs { get; set; }

        /// <summary>
        /// 保留ScanFile文件是否生成的状态，默认0为生成，1为未生成
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public int ScanFileIsExist { get; set; }

        /// <summary>
        /// 用于标识Job是否要被stop，默认0为正常跑，1为此job要被stop
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public int JobIsStopping { get; set; }

        /// <summary>
        /// Scheduled extender和Archive跑job的时候将ProcessingPoolId存到job记录里
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string ProcessingPoolId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string FarmName { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string FarmID { get; set; } 

        /// <summary>
        /// 如果发现当前Job中的Site Collection有其它Job正在运行，将这些SIte Collection记录下来，等待重新运行时做处理。
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string SiteCollectionList { get; set; }

        /// <summary>
        /// 存放scheduled 和 archiver run job节点的scope即fullpath，由于fullpath可能比较大，所以映射String_5字段，有255大小
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string Scope { get; set; }

        [DataMember]
        public string SourceFarm { get; set; }

        [DataMember]
        public string DestinationFarm { get; set; }

        [DataMember]
        public RestoreType RestoreType { get; set; }

    }

    /// <summary>
    /// in place(default), out place for restore
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreType
    {
        [EnumMember]
        InPlace,
        [EnumMember]
        OutPlace,
        [EnumMember]
        ToFileSystem
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScanFileExist
    {
        [EnumMember]
        Exist = 0,
        [EnumMember]
        Not_Exist = 1
    }
}
