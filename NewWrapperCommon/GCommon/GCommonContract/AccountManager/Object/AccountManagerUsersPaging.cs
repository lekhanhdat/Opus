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
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract]
    public class AccountManagerUsersPaging
    {
        //总记录条数
        [DataMember]
        public int TotalCount { get; set; }

        //总页数
        [DataMember]
        public int TotalPage { get; set; }

        //当前页数
        [DataMember]
        public int CurrentPage { get; set; }

        //每一页显示条数
        [DataMember]
        public int EveryPageCount { get; set; }

        //对哪一列排序
        [DataMember]
        public OrderColumn OrderColumn { get; set; }

        //排序类型：升序 0、降序 1
        [DataMember]
        public OrderType OrderType { get; set; }

        //Search 关键字
        [DataMember]
        public string SearchKeyWord { get; set; }

        //当前页记录
        [DataMember]
        public List<AccountMappingDto> Results { get; set; }
    }
}