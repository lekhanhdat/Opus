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
    new string[] { },
    null,
    true)]

    #endregion CodeReview

    [SQLiteFunction(Name = "DOCUMENTNAME", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class DocumentNameFunction : SQLiteFunction
    {
        /// <summary>
        /// Intercept name from name column
        /// </summary>
        /// <param name="args">Name from index database</param>
        /// <returns>Document name</returns>
        public override Object Invoke(Object[] args)
        {
            var documentName = default(String);
            var name = Convert.ToString(args[0]);
            documentName = name.Contains(":") ? name.Substring(0, name.IndexOf(":", StringComparison.OrdinalIgnoreCase)) : name;
            return documentName;
        }
    }
}