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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProfileReturnMessage
    {
        [DataMember]
        public string UserWhoChanged { get; set; }
        [DataMember]
        public ProfileContextSource ContextSource { get; set; }
        [DataMember]
        public ProfileStatus Status { get; set; }
        [DataMember]
        public DateTime OccurredTime { get; set; }
        [DataMember]
        public List<CAStringFormatMessage> ContextDetailMsg { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public AdminProfileContextData ContextData { get; set; }

        /// <summary>
        /// 此属性给Server整合数据之用, 用于存储触发Rule的节点下的Detail信息, 应用于Auditor类型的数据
        /// </summary>
        [DataMember]
        public List<ProfileObjectDetail> MultiContextDetails { get; set; }
        [DataMember]
        public string ResultId { get; set; }
    }

    /// <summary>
    /// 此类给Server整合数据之用, 用于存储触发Rule的节点下的Detail信息, 应用于Auditor类型的数据
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProfileObjectDetail : IComparable<ProfileObjectDetail>
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime OccurredTime { get; set; }
        [DataMember]
        public List<CAStringFormatMessage> ContextDetailMsg { get; set; }
        [DataMember]
        public AdminProfileContextData ContextData { get; set; }
        [DataMember]
        public ProfileStatus Status { get; set; }

        /// <summary>
        /// 标识触发每条违规记录的EventType
        /// </summary>
        [DataMember]
        public AdminEventType TriggeredEventType { get; set; }

        /// <summary>
        /// Used By Server(Job Monitor),agent also use this properties
        /// </summary>
        [DataMember]
        public string ApplyNodeURL { get; set; }
        /// <summary>
        /// 自定义排序时间最新的排在前面
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ProfileObjectDetail other)
        {
            if (other == null)
            {
                return -1;
            }
            else
            {
                return -this.OccurredTime.CompareTo(other.OccurredTime);
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminProfileContextData
    {
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public string DataXML { get; set; }
        /// <summary>
        /// 目前只有ScanMode用来保存违规信息，用于Fix
        /// </summary>
        [DataMember]
        public List<string> ViolationValues { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProfileContextSource
    {
        [DataMember]
        public Guid FarmId { get; set; }
        [DataMember]
        public Guid WebAppId { get; set; }
        [DataMember]
        public Guid SiteId { get; set; }
        [DataMember]
        public Guid WebId { get; set; }
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public Guid ItemId { get; set; }

        //6.3老数据使用,6.4已废弃。
        [DataMember]
        [Obsolete]
        public SPTreeNodeDto CurrentNode { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }
        [DataMember]
        public string FullPath { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string SPObjectId { get; set; }
        /// <summary>
        /// Used By Server(Job Monitor),agent also use this properties
        /// </summary>
        [DataMember]
        public string ApplyNodeURL { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProfileStatus : int
    {
        [EnumMember]
        Passed = 0,
        [EnumMember]
        Fixed = 1,
        [EnumMember]
        WaitFixed = 2,
        [EnumMember]
        CanNotFixed = 3,
        /// <summary>
        /// 与页面上的Status状态相对应
        /// </summary>
        [EnumMember]
        Error = 4,
        [EnumMember]
        OutOfPolicy = 5,
        [EnumMember]
        ValidateError = 6,
        [EnumMember]
        FixError = 7,
        [EnumMember]
        DontFix = 8,
    }

    public class PolicyEnforcerResult : ResultBase
    {
        [DataMember]
        public string ProfileId { get; set; }
        [DataMember]
        public List<ReceiverInfo> ReceiverInfos { get; set; }
        [DataMember]
        public bool SendImmediately { get; set; }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string ApplyNodeId { get; set; }
        /// <summary>
        /// 给Server Job Monitor View Detail用
        /// 当Profile从节点上取消Apply之后, 无法获取Scope信息
        /// </summary>
        [DataMember]
        public string ApplyNodeURL { get; set; }
        [DataMember]
        public string RuleId { get; set; }
        [DataMember]
        public string ViolationNodeId { get; set; }
        [DataMember]
        public Guid FarmId { get; set; }
        [DataMember]
        public ProfileContextSource ContextSource { get; set; }

        [DataMember]
        public AdminProfileContextData ContextData { get; set; }
        [DataMember]
        public List<CAStringFormatMessage> ContextDetailMsg { get; set; }
        [DataMember]
        public ProfileStatus Status { get; set; }
        [DataMember]
        public DateTime OccurredTime { get; set; }
        /// <summary>
        /// 标识触发每条违规记录的EventType
        /// </summary>
        [DataMember]
        public AdminEventType TriggeredEventType { get; set; }
        /// <summary>
        /// Server端使用,老数据升级序列化需要使用该属性
        /// </summary>
        [DataMember]
        public string JobId { get; set; }

        public long HiddenDate { get; set; }

        public long ExpiredDate { get; set; }

        public long JobStartTime { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReceiverInfo
    {
        [DataMember]
        public string Receiver { get; set; }
        [DataMember]
        public ReceiverType ReceiverType { get; set; }
        [DataMember]
        public string PreferredLanguage { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReceiverType : int
    {
        [EnumMember]
        InputUser = 0,
        [EnumMember]
        ViolateRuleUser = 1,
        [EnumMember]
        SiteCollectionAdministrator = 2,
    }
}
