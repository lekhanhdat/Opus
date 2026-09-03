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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.FilterPolicy;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
    using AvePoint.GCommon.Contract.Tree.Object;

    [DataContract]
    public class EDSearchRequest : EDiscoveryRequest
    {
        #region - 可选 -

        [DataMember]
        public string FarmId { get; set; } //需要根据这个属性获取可用Agent.

        [DataMember]
        public bool IsFirstPage { get; set; } //是否是第一页,Agent获得数据时需要.

        [DataMember]
        public List<FilterPolicy> Filters { get; set; } //Filters

        [DataMember]
        public List<QueryInfo> QueryInfoList { get; set; } //保存Agent查询Info.

        [DataMember]
        public List<PageInfo> PageInfoList { get; set; } //当丢失数据向上翻页时使用.

        [DataMember]
        public List<EDFilterPolicy> EDFilters { get; set; }


        #endregion

        #region - Required 必要的属性 -

        [DataMember]
        public string KeyWord { get; set; }          //搜索关键字

        [DataMember]
        public SearchStorage Storage { get; set; }         //与SearchAction组合来确认数据源的来源.

        [DataMember]
        public PageAction PageAction { get; set; }   //页面动作,向上还是向下.

        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }  //完整的Tree

        [DataMember]
        public int CountPerPage { get; set; }        //每篇显示多少行数据.

        [DataMember]
        public int PageCount { get; set; }           //一共要多少篇.

        [DataMember]
        public SearchAction Action { get; set; }     //Search的请求动作

        [DataMember]
        public HoldFileRelevantResults HoldFileRelevant { get; set; } //Search结果的包含内容

        [DataMember]
        public int ResultType { get; set; } //SharePoint-Type,Search搜索的类型.

        [DataMember]
        public int DocAveStartPage { get; set; } //开始页码.

        #endregion

        /// <summary>
        /// 只验证Config的必要属性.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            bool flag = true;
            if (this.KeyWord == null || this.KeyWord.Trim().Equals(""))
            {
                flag = false;
            }
            else if (this.TreeNode == null)
            {
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// 生成发送给Client的信息...
        /// </summary>
        /// <returns></returns>
        public QueryMessage BuildQueryMessage()
        {
            QueryMessage queryMsg = new QueryMessage();
            queryMsg.KeyWord = this.KeyWord;
            queryMsg.ResultType = this.ResultType;
            queryMsg.Filters = this.Filters;
            queryMsg.EDFilters = this.EDFilters;
            queryMsg.HoldFileRelevant = this.HoldFileRelevant;
            queryMsg.QueryInfoList = this.QueryInfoList;
            queryMsg.CountPerPage = this.CountPerPage;
            queryMsg.IsFirstPage = this.IsFirstPage;
            queryMsg.TreeNode = this.TreeNode;
            queryMsg.StartPage = this.DocAveStartPage;
            queryMsg.PageCount = this.PageCount;
            queryMsg.Action = this.PageAction;
            return queryMsg;
        }

        /// <summary>
        /// 提供Clone方法,Clone当前自身状态.
        /// </summary>
        /// <returns></returns>
        public EDSearchRequest Clone()
        {
            EDSearchRequest config = new EDSearchRequest();
            config.FarmId = this.FarmId;
            config.IsFirstPage = this.IsFirstPage;
            config.Filters = this.Filters;
            config.QueryInfoList = this.QueryInfoList;
            config.KeyWord = this.KeyWord;
            config.Storage = this.Storage;
            config.PageAction = this.PageAction;
            config.TreeNode = this.TreeNode;
            config.PageCount = this.PageCount;
            config.Action = this.Action;
            config.HoldFileRelevant = this.HoldFileRelevant;
            config.ResultType = this.ResultType;
            return config;
        }
    }

    /// <summary>
    /// 请求的服务类型.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SearchAction : uint
    {
        [EnumMember]
        OnlineSearch = 1,
        [EnumMember]
        OfflineSearch = 2
    }

    /// <summary>
    /// 数据源获取
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SearchStorage : uint
    {
        [EnumMember]
        SharePoint = 0, //SharePoint需请求Client.
        [EnumMember]
        Archiver = 1,   //Archiver需请求Media.
    }

    /// <summary>
    /// 翻页动作
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PageAction : uint
    {
        [EnumMember]
        Down = 0,       //向下翻页.
        [EnumMember]
        Up = 1          //向上翻页.
    }
}
