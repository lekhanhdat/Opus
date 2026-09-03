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

namespace AvePoint.RA.Contract.Explorer
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

        /// <summary>
        /// 存一个Number值 用于Filter的比较
        /// </summary>
        [JsonProperty(PropertyName = "Number", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Number { set; get; }

        [JsonProperty(PropertyName = "YesOrNo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string YesOrNo { set; get; }
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


    /// <summary>
    ///  SingleText = 1,
    ////MultipleText = 2,
    ////DateTime = 3,
    ////SingleChoice = 4,
    ////PeopleOrGroup = 5,
    ////Number = 6,
    ////MultipleChoice = 7,
    ////Taxonomy = 10,
    /// </summary>
    public static class CustomColumnExtension
    {
        public static string GetSingleTextColumnValue(this CustomColumn column)
        {
            if (column == null)
            {
                return null;
            }
            return column.Value;
        }
        public static string GetMultipleTextColumnValue(this CustomColumn column)
        {
            if (column == null)
            {
                return null;
            }
            return column.Value;
        }
        public static string GetNumberColumnValue(this CustomColumn column)
        {
            if (column == null)
            {
                return null;
            }
            return column.Value;
        }
        public static DateTimeColumnValue GetDateTimeColumnValue(this CustomColumn column)
        {
            if (column == null)
            {
                return null;
            }
            DateTimeColumnValue value = new DateTimeColumnValue();
            value.Date = column.Date;
            value.IsSetDayLight = column.IsSetDayLight;
            value.TimeZoneId = column.TimeZoneId;
            return value;
        }
        public static ChoiceColumnValue GetSingleChoiceColumnValue(this CustomColumn column)
        {
            if (column == null)
            {
                return null;
            }
            ChoiceColumnValue value = new ChoiceColumnValue();
            value.Name = column.Name;
            value.Value = column.Value;
            return value;
        }
        public static TaxonomyColumnValue GetTaxonomyColumnValue(this CustomColumn column)
        {
            if (column == null)
            {
                return null;
            }
            TaxonomyColumnValue value = new TaxonomyColumnValue();
            value.Name = column.Name;
            value.Id = column.Id;
            return value;
        }
        public static List<AOSUserDto> GetPeopleOrGroupColumnValue(this CustomColumn column)
        {
            if (column == null)
            {
                return null;
            }
            return column.Users;
        }
        public static List<ChoiceColumnValue> GetMultipleChoiceColumnValue(this CustomColumn column)
        {
            if (column == null)
            {
                return null;
            }
            return column.MultiChoice;
        }
		
		 public static void SetSingleTextColumnValue(this CustomColumn column, string value)
        {
            if (column != null && value != null)
            {
                column.Value = value;
            }
        }
        public static void SetMultipleTextColumnValue(this CustomColumn column, string value)
        {
            if (column != null && value != null)
            {
                column.Value = value;
            }
        }
        public static void SetNumberColumnValue(this CustomColumn column, string value)
        {
            if (column != null && value != null)
            {
                column.Value = value;
            }
        }
        public static void SetDateTimeColumnValue(this CustomColumn column, DateTimeColumnValue value)
        {
            if (column != null && value != null)
            {
                column.Date = value.Date;
                column.IsSetDayLight = value.IsSetDayLight;
                column.TimeZoneId = value.TimeZoneId;
            }
        }
        public static void SetSingleChoiceColumnValue(this CustomColumn column, ChoiceColumnValue value)
        {
            if (column != null && value != null)
            {
                column.Name = value.Name;
                column.Value = value.Value;
            }
        }
        public static void SetTaxonomyColumnValue(this CustomColumn column, TaxonomyColumnValue value)
        {
            if (column != null && value != null)
            { 
                column.Name = value.Name ;
                column.Id = value.Id;
            } 
        }
        public static void SetPeopleOrGroupColumnValue(this CustomColumn column, List<AOSUserDto> users)
        {
            if (column != null && users != null)
            {
                column.Users = users;
            } 
        }
        public static void SetMultipleChoiceColumnValue(this CustomColumn column, List<ChoiceColumnValue> choices)
        {
            if (column != null && choices != null)
            {
                column.MultiChoice = choices;
            } 
        }
    }

}
