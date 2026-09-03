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
    #region using directive

    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.CodeReview;

    #endregion using directive

    #region CodeReview

    [AveCodeReview(
    "2012/6/20",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion CodeReview

    [SQLiteFunction(Name = "CONTAINERNAME", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class ContainerNameFunction : SQLiteFunction
    {
        /// <summary>
        /// Intercept container name from name column
        /// </summary>
        /// <param name="args">Name from index database</param>
        /// <returns>Container name</returns>
        public override Object Invoke(Object[] args)
        {
            var containerName = default(String);
            var name = Convert.ToString(args[0]);
            if (name.Contains("\\"))
                containerName = name.Substring(name.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
            else if (name.Contains("/"))
                containerName = name.Substring(name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
            return containerName;
        }
    }
}