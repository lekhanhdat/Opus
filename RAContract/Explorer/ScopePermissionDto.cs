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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract]
    public class ScopePermissionSimpleDto
    {
        [DataMember]
        public List<string> ScopeIds { get; set; }
        [DataMember]
        public List<AOSUserDto> Accounts { get; set; }
        [DataMember]
        public RMScopePermissionEnum Permission { get; set; }
        [DataMember]
        public bool IsInherit { get; set; }
    }

    public class ScopePermissionDto
    {
        public List<ScopeInfoDto> ScopeInfos { get; set; }
        /// <summary>
        /// 有权限的userId集合
        /// </summary>
        public List<int> AccountIds { get; set; }
        /// <summary>
        /// 目前只有All权限
        /// </summary>
        public RMScopePermissionEnum Permission { get; set; }
        public bool IsInheritSave { get; set; }
        public PermissionUserConflictOption UserConflictOption { get; set; }
    }
    [DataContract]
    public class PhysicalObjectPermissionDto
    {
        [DataMember]
        public List<AOSUserDto> Accounts { get; set; }
        [DataMember]
        public RMScopePermissionEnum Permission { get; set; }
        [DataMember]
        public bool IsInheritSave { get; set; }
    }

    public class ScopeInfoDto
    {
        [DataMember(EmitDefaultValue = false)]
        public string ScopeId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ParentScopeId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        /// <summary>
        /// Id Path 用/分隔,以/结尾
        /// </summary>
        public string ScopeFullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ScopeNameFullPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int NodeType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ActionAuditInfo { get; set; }
    }

    //save the job context
    public class ScopePermissionJobContextDto
    {
        [DataMember(EmitDefaultValue = false)]
        public List<ScopeInfoDto> Scopes { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public GSPermissionJobContextDto GSJobContextDto { get; set; }
    }

    //global search set permission
    public class GSPermissionJobContextDto
    {
        //public JobType JobType { get; set; }
        /// <summary>
        /// 页面执行Search的User
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public ExplorerQueryDto QueryDto { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ExplorerQueryV3Dto QueryV3Dto { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> NodeIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<int> AccountIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsInheritSave { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public RMScopePermissionEnum PermissionType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public PermissionUserConflictOption UserConflictOption { get; set; }
    }
    [DataContract]
    public class GSPermissionSimpleDto
    {
        [DataMember]
        public List<AOSUserDto> Accounts { get; set; }
        [DataMember]
        public List<Guid> NodeIds { get; set; }
        [DataMember]
        public ExplorerQueryDto QueryDto { get; set; }
        [DataMember]
        public ExplorerQueryV3Dto QueryV3Dto { get; set; }
        [DataMember]
        public PermissionUserConflictOption UserConflictOption { get; set; }
    }

    public class UsersAndBreakInheritStatus
    {
        public List<AOSUserDto> Accounts { get; set; }
        public bool BreakInheritStatus { get; set; }
    }
    [DataContract]
    public enum PermissionUserConflictOption
    {
        [EnumMember]
        Overwrite = 0,
        [EnumMember]
        Append = 1,
    }
}
