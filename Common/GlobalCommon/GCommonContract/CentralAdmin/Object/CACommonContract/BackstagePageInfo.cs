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

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackstagePageInfo
    {
        //Paging
        [DataMember]
        public int PageIndex { get; set; }

        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int ItemsCount { get; set; }

        
        
        //FIlter

        [DataMember]
        public Dictionary<string,List<string>> ColumnFilters {get;set;}

        [DataMember]
        public List<string> ColumnSortMemberPaths { get; set; }

        /// <summary>
        /// Sort 属性名和是否升序
        /// </summary>
        [DataMember]
        public Dictionary<string, bool> SortAttributeNameAndIsAsc { get; set; }
        //Search

        [DataMember]
        public string SearchText { get; set; }

        [DataMember]
        public BackstageReturnValue ReturnValue { get; set; }
        
    }

    [KnownType(typeof(CASearchResultNodeInfo))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackstageReturnValue
    {
        [DataMember]
        public List<int> AllKeys { get; set; }

        [DataMember]
        public Dictionary<int, BackstageResultBase> OnePageData { get; set; }

        [DataMember]
        public List<int> KeysToInit { get; set; }

        [DataMember]
        public BackstageReturnValueType ReturnValueType { get; set; }

        
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum BackstageReturnValueType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        AllKeys = 1,

        [EnumMember]
        OnePageData = 2,

       
    }


    [KnownType(typeof(CASearchResultNodeInfo))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackstageResultBase
    {
        [DataMember]
        public int key { get; set; }
    }

}
