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
using System.Text;
using AvePoint.GCommon.Contract.Tree.Object;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;

namespace AvePoint.GCommon.Contract.SharePointBrowser
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointBrowserContract : BrowserContractBase
    {
        [DataMember]
        public ApiObjectModelType ObjectModelType { get; set; }

        [DataMember]
        public BposInfo BposInfo { get; set; }

        /// <summary>
        /// 从GUI获取的父节点。如果为null或者count==0，说明需要load Web Application。如果里面只有Web Application节点，则说明需要load Site collection，以此类推
        /// </summary>
        [DataMember]
        public List<SPTreeNodeDto> ParentNodes { get; set; }
        /// <summary>
        /// Agent端返回的获取到的子节点集合
        /// </summary>
        [DataMember]
        public List<SPTreeNodeDto> ChildenNodes { get; set; }       
        /// <summary>
        /// 用来区分不同的模块的browser，比如Extender的Browser和Item的不一样
        /// </summary>
        [DataMember]
        public string AgentType { get; set; }
        /// <summary>
        /// 分页获取Item用，从Agent获得，获取下一页时再传回去
        /// </summary>
        [DataMember]
        public string PageInfo { get; set; }

        /// <summary>
        /// 分页获取Item用，表示想获取几个item
        /// </summary>
        [DataMember]
        public uint PerPage { get; set; }

        /// <summary>
        /// 表示该节点的子节点是否有下一页
        /// </summary>
        [DataMember]
        public bool HasNextPage { get; set; }

        /// <summary>
        /// 从第几个节点开始Borwse
        /// </summary>
        [DataMember]
        public int StartIndex { get; set; }

        /// <summary>
        /// 用于记录有多少个节点，分页使用
        /// </summary>
        [DataMember]
        public int ChildrenCount { get; set; }

        /// <summary>
        /// 从GUI获取的父节点。如果为null或者count==0，说明需要load Web Application。如果里面只有Web Application节点，则说明需要load Site collection，以此类推
        /// </summary>
        [Obsolete]
        [DataMember]
        public List<IAveTreeNodeDto> PRParentNodes { get; set; }
        /// <summary>
        /// Agent端返回的获取到的子节点集合
        /// </summary>
        [Obsolete]
        [DataMember]
        public List<IAveTreeNodeDto> PRChildenNodes { get; set; }
        [DataMember]
        public FilterPolicyInfo FilterPolicy { get; set; } 
        [DataMember]
        public bool IsAdvancedSearchEnable { get; set; }
        [DataMember]
        public bool IsBPOS { get; set; }

        //用于Security Trimming，发送所有AD用户列表
        [DataMember]
        public List<string> UserNameList { get; set; }

        //用于Security Trimming，在Browse或者Schedule时获取用户权限信息
        [DataMember]
        public List<SPTreePermissionMappingDto> PermissionList { get; set; }

        [DataMember]
        public SharePointBrowserError Error { get; set; }

        public bool HasError
        {
            get
            {
                if (Error == null || Error.Type == SharePointBrowserErrorType.None)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public SharePointBrowserContract()
        {
            ParentNodes = new List<SPTreeNodeDto>();
            ChildenNodes = new List<SPTreeNodeDto>();
            PRParentNodes = new List<IAveTreeNodeDto>();
            PRChildenNodes = new List<IAveTreeNodeDto>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointBrowserError
    {
        [DataMember]
        public SharePointBrowserErrorType Type { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SharePointBrowserErrorType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        NoResponse = 1,

        [EnumMember]
        NoAvailibleAgent = 2,

        [EnumMember]
        UnKnown = 3,

        [EnumMember]
        TooManyChildren = 4,
    }
}
