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
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Model
{

    public class CustomColumn
    {
        ///// <summary>
        ///// 源数据Dictionary中的Key， 也就是Template column的Id
        ///// </summary>
        //[JsonProperty(PropertyName = "Key")]
        //public string Key { set; get; }

        #region 用于存储Single Choice, Taxonamy Value等
        [JsonProperty(PropertyName = "Id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "Value_Array", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Value_Array { get; set; }

        [JsonProperty(PropertyName = "Name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        #endregion

        #region 用于存储DateTime
        /// <summary>
        /// 目前存储的是Local时间，需要换成了UTC以方便查询
        /// </summary>
        [JsonProperty(PropertyName = "Date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "TimeZoneId", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeZoneId { get; set; }

        [JsonProperty(PropertyName = "IsSetDayLight", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsSetDayLight { get; set; }

        #endregion
        /// <summary>
        /// People And Users类型的字段, 值是多项
        /// </summary>
        [JsonProperty(PropertyName = "Users", NullValueHandling = NullValueHandling.Ignore)]
        public List<AOSUserDto> Users { set; get; }

        /// <summary>
        /// 多选字段，值是多项
        /// </summary>
        [JsonProperty(PropertyName = "MultiChoice", NullValueHandling = NullValueHandling.Ignore)]
        public List<ChoiceColumnValue> MultiChoice { set; get; }
    }
}
