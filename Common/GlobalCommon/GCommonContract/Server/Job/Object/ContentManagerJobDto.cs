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




using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ContentManager.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Job.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentManagerJobDto : BaseJobDto
    {
        /// <summary>
        /// 该属性用来保存move的时候是手动删除还是自动删除.
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public DeleteType DeleteType { get; set; }

        /// <summary>
        /// 该属性用来保存删除的状态，用来控制ribbon.
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_2)]
        public DeleteState DeleteState { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_10)]
        public string GradeResult { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string Extension { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string BackUpInfo { get; set; } 

        /// <summary>
        /// move类型的job需要存储当时plan中的delete checkout file 属性，在做delete content时发给agent
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public DeleteCheckOutFileType DeleteCheckOutFileType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CMJobDtoExtension 
    {
        [DataMember]
        public string AgentID { set; get; } //GUID
        [DataMember]
        public string AgentHost { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobOperationType 
    {
        [EnumMember]
        None,
        [EnumMember]
        Pause,
        [EnumMember]
        Retry,
        [EnumMember]
        Stop,
        [EnumMember]
        Resume,
        [EnumMember]
        Start,
        [EnumMember]
        ReStart, 
        [EnumMember]
        RollBack
    }
}
