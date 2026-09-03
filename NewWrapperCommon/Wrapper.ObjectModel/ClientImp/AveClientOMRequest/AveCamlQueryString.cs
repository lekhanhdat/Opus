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

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveCamlQueryString
    {
        /// <summary>
        /// 当viewFields == null时，IncludeWithDefaultProperties方法会获取到所有column value。当viewFields.Count == 0 时，只获取基本属性，比如UIVersion,Level等。
        /// 有些column如果不确定是否存在时，可以放在viewFields里，Query不会出错。  如果放在IncludeWithDefaultProperties里，会出现Field or property not exists的Error.
        /// </summary>
        /// <param name="viewFields"></param>
        /// <param name="rowLimit"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string GetAllItemsString(List<string> viewFields, int rowLimit, QueryFindOption option)
        {
            StringBuilder builder = new StringBuilder();
            if (option == QueryFindOption.None)
            {
                builder.Append("<View>");
            }
            else
            {
                builder.Append(string.Format("<View Scope='{0}'>", option.ToString()));
            }
            if (viewFields != null)
            {
                builder.Append("<ViewFields>");
                foreach (string field in viewFields)
                {
                    builder.Append(string.Format("<FieldRef Name='{0}'/>", field));
                }
                builder.Append("</ViewFields>");
            }
            if (rowLimit > 0)
            {
                builder.Append(string.Format("<RowLimit>{0}</RowLimit>", rowLimit));
            }
            builder.Append("</View>");
            return builder.ToString();
        }
    }

    public enum QueryFindOption 
    {
        None,
        RecursiveAll
    }
}