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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.ContentManager.Object
{
    /// <summary>
    /// 将前台GUI首次进入页面后的Get 请求 要得到的所有数据 封装在这个类里.
    /// </summary>
    [DataContract]
    public class CMResponse
    {
        [DataMember]
        public CMDefaultSettings CMDefaultSettings { set; get; }

        [DataMember]
        public List<NameAndIdDto> LanguageMappings { set; get; }

        [DataMember]
        public List<NameAndIdDto> UserMappings { set; get; }

        [DataMember]
        public List<NameAndIdDto> DomainMappings { set; get; }

        [DataMember]
        public List<NameAndIdDto> TemplateMappings { set; get; }

        [DataMember]
        public List<NameAndIdDto> ColumnMappings { set; get; }

        [DataMember]
        public List<NameAndIdDto> ContentTypeMappings { set; get; }

        [DataMember]
        public List<PlanGroupDtoForOtherModule> PlanGroups { set; get; }

        [DataMember]
        public List<NameAndIdDto> Filterpolicy { set; get; }

        [DataMember]
        public List<StoragePolicyDto> StoragePolicy { get; set; }
    }

    /// <summary>
    /// 每个页面对应一个type
    /// </summary>
    [DataContract]
    public enum CMRequestType 
    {
        [EnumMember]
        CopyDefaultSettings,
        [EnumMember]
        MoveDefaultSettings,
        [EnumMember]
        ConfigMyself,
        [EnumMember]
        PlanBuilder,
        [EnumMember]
        Import,
        [EnumMember]
        CopyQuickSettings,
        [EnumMember] 
        MoveQuickSettings,
        [EnumMember]
        Export
    }
}
