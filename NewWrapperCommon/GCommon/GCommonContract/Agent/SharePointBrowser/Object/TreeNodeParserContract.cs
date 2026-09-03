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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;

namespace AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object
{
    /// <summary>
    /// Import Excel Plan的Contract
    /// </summary>
    [DataContract]
    public class TreeNodeParserContract:BrowserContractBase
    {
        /// <summary>
        /// sever传来ParseUnit集合
        /// </summary>
        [DataMember]
        public List<TreeNodeParserUnit> Units { get; set; }
    }
    /// <summary>
    /// 一个ParserUnit中包含一条ParseString信息，供生成TreeNode
    /// </summary>
    [DataContract]
    public class TreeNodeParserUnit
    {
        /// <summary>
        /// 需要解析成node的string信息
        /// </summary>
        [DataMember]
        public string ParseString { get; set; }
        /// <summary>
        /// manual input,site collection 级别job中，需要sever传给agent webapp url
        /// </summary>
        [DataMember]
        public string WebAppUrl { get; set; }
        /// <summary>
        /// job的level
        /// </summary>
        [DataMember]
        public NodeLevel Level { get; set; }
        /// <summary>
        /// 解析成的TreeNode结果
        /// </summary>
        [DataMember]
        public SPTreeNodeDto Tree { get; set; }
        /// <summary>
        /// 转换的类型，目前有两种：Url与FullPath
        /// </summary>
        [DataMember]
        public TreeNodeParserType Type { get; set; }
        /// <summary>
        /// 是否是Bpos环境
        /// </summary>
        [DataMember]
        public bool IsBpos { get; set; }
        /// <summary>
        /// Bpos information
        /// </summary>
        [DataMember]
        public BposInfo BposInfo { get; set; }
        /// <summary>
        /// Remote站点的SPVersion
        /// </summary>
        [DataMember]
        public string SPVersion { get; set; }
        /// <summary>
        /// 主要给Remote farm使用
        /// </summary>
        [DataMember]
        public string FarmId { get; set; }
        /// <summary>
        /// 转换过程中是否出错
        /// </summary>
        [DataMember]
        public bool HasError { get; set; }
        /// <summary>
        /// 出错的类型
        /// </summary>
        [DataMember]
        public TreeNodeParserErrorType Error { get; set; }
    }
    /// <summary>
    /// 转换类型
    /// </summary>
    [DataContract]
    public enum TreeNodeParserType : int
    { 
        /// <summary>
        /// 默认值，未知类型
        /// </summary>
        [EnumMember]
        Unknown = 0,
        /// <summary>
        /// 转换FullPath
        /// </summary>
        [EnumMember]
        FullPathParser = 1,
        /// <summary>
        /// 转换Url
        /// </summary>
        [EnumMember]
        UrlParser = 2,
    }

    /// <summary>
    /// Parse过程中生成TreeNode出错的错误类型
    /// </summary>
    [DataContract]
    public enum TreeNodeParserErrorType : int
    {
        /// <summary>
        /// 默认未出错
        /// </summary>
        [EnumMember]
        Unknown = 0,
        /// <summary>
        /// Bpos信息有误
        /// </summary>
        [EnumMember]
        BposInfoIncorrect = 1,
        /// <summary>
        /// ParseString有误
        /// </summary>
        [EnumMember]
        TreeNodeNotFound = 2,
        /// <summary>
        /// job level值有误
        /// </summary>
        [EnumMember]
        TreeLevelMismatched = 3,
        /// <summary>
        /// 转换类型有误
        /// </summary>
        [EnumMember]
        ParserTypeErrorMessage = 4,
    }
}
