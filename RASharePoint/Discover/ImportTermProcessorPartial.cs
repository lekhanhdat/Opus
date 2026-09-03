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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Discover
{
    /// <summary>
    /// ITP的分部类
    /// </summary>
    public partial class ImportTermProcessor
    {

        #region Rule 验证自定义列
        public async Task ValidateCustomColumnAsync(string columnType, string columnValue, string columnName, string columnTimeZone, RMTagContentInfo tagCustomColumn)
        {
            //自定义列数据类型
            TagContentInfoType tagInfoType = GetTagType(columnType);
            tagCustomColumn.Type = tagInfoType;
            if (string.IsNullOrEmpty(columnName))
            {
                throw new Exception("RM_JS_TM_TermImport_NoCustomColumnName");
            }
            tagCustomColumn.ColumnName = columnName;
            if (string.IsNullOrEmpty(columnValue))
            {
                throw new Exception("RM_JS_TM_TermImport_NoCustomColumnValue");
            }
            tagCustomColumn.Value = columnValue;

            #region check Type&Value
            bool isSucc = true;
            float reVal = 0.0f;
            DateTime reDt = DateTime.Now;
            string[] arr = new string[] { "yes", "no" };
            switch (tagInfoType)
            {
                case TagContentInfoType.Number:
                    {
                        isSucc = float.TryParse(columnValue, out reVal);
                        break;
                    }
                case TagContentInfoType.Text:
                    {
                        isSucc = !string.IsNullOrEmpty(columnValue);
                        break;
                    }
                case TagContentInfoType.DateTime:
                    {
                        isSucc = DateTime.TryParse(columnValue, out reDt);
                        break;
                    }
                case TagContentInfoType.Boolean:
                    {
                        isSucc = arr.Contains(columnValue.ToLowerInvariant().Trim());
                        break;
                    }
                default:
                    break;
            }

            if(!isSucc)
            {
                throw new Exception("The entered value is invalid. Please check and enter again.");  //找到国际化字符串
            }
            #endregion

            #region check DateTime
            if (tagCustomColumn.Type == TagContentInfoType.DateTime)
            {
                var dateTimeStr = columnValue;
                try
                {
                    dateTimeStr = GetDateTimeStr(dateTimeStr);
                }
                catch
                {
                    throw new Exception("RM_JS_TM_TermImport_CustomColumnValueDateTimeError");
                }
                var timeZoneId = GetTimeZoneId(columnTimeZone);
                //在创建rule时,tagCustomColumn.DateTime会自动赋值，这里不用赋值
                //DateTime dt = Convert.ToDateTime(dateTimeStr);
                //dt = DateTimeUtil.ConvertTimeToUtcDate(dt, timeZoneId, true);
                //tagCustomColumn.DateTime = dt;
                tagCustomColumn.Value = dateTimeStr;
                tagCustomColumn.TimeZoneId = timeZoneId ?? (await GeneralSetting).TimeZoneId;
            }
            else
            {
                tagCustomColumn.DateTime = DateTime.MinValue;
            }
            #endregion
        }

        #endregion

        #region Rule 验证MoveUrl
        /// <summary>
        /// 验证MoveUrl
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public bool ValidateMoveUrl(MoveToDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LocationPath))
            {
                mLog.Info($"ValidateMoveUrl:isNull");
                throw new Exception("RM_JS_RDM_CreateRule_Validation_NoInputLocaltion");
            }
            /*
            var rst = ExplorerService.CheckSPUrl4Rule(dto.LocationPath, dto.SPAccount);
            if (rst == null)
            {
                mLog.Info($"ValidateMoveUrl:MoveUrl {dto.LocationPath} is not valid,");
                throw new Exception("RM_JS_Rule_SPDestUrlError");
            }
            */
            return true;
        }
        #endregion


    }
}
