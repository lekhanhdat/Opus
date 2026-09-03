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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Agent.ExchangeBrowser.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineBrowserContract : BrowserContractBase
    {
        [DataMember]
        public BposInfo BposInfo { get; set; }

        [DataMember]
        public List<ExchangeOnlineTreeNodeDto> ParentNodes { get; set; }

        [DataMember]
        public List<ExchangeOnlineTreeNodeDto> ChildenNodes { get; set; }


        [DataMember]
        public ActionType ActionType { get; set; }

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
        /// 操作的返回值
        /// </summary>
        [DataMember]
        public ExchangeOnlineErrorInfo ErrorInformation { get; set; }

        [DataMember]
        public bool IncludeInPlaceArchiveMailbox { get; set; }

        [DataMember]
        public bool IncludeResourceMailbox { get; set; }

        [DataMember]
        public bool IsO365 { get; set; }
    }
}
