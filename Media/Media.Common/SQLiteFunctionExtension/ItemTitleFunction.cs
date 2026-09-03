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




namespace AvePoint.Media.Common
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.CodeReview;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/6/20",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
    "ADO-34389",
    true)]

    #endregion CodeReview

    [SQLiteFunction(Name = "ITEMTITLE", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class ItemTitleFunction : SQLiteFunction
    {
        /// <summary>
        /// Generate item title by attributes column
        /// </summary>
        /// <param name="args">Attributes from index database</param>
        /// <returns>Item title</returns>
        public override Object Invoke(Object[] args)
        {
            var itemName = default(String);
            var attribute = Convert.ToString(args[0]);
            if (attribute != null && attribute.Contains(ServiceConstants.ExtraChar))
            {
                itemName = attribute;
                if (attribute.Contains("Title:"))
                    itemName = attribute.Substring(attribute.IndexOf("Title:", StringComparison.Ordinal) + 6);
                else if(attribute.Contains("Title" + ServiceConstants.Delimiter))
                    itemName = attribute.Substring(attribute.IndexOf("Title" + ServiceConstants.Delimiter, StringComparison.Ordinal) + 6);
                itemName = itemName.Remove(itemName.IndexOf(ServiceConstants.ExtraChar));
            }
            return itemName;
        }
    }
}