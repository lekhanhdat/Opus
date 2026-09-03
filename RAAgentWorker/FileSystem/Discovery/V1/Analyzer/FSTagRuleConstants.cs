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

namespace RAFileSystem.FileSystem.Discovery.V1.Analyzer
{
    internal static class FSTagRuleConstants
    {
        public static readonly Guid R_CATEGORY_RULE_UNIQUE_ID = new Guid("90FC78C6-B174-F603-0C94-36FC2EBB4C63");

        public static readonly Guid O_CATEGORY_RULE_UNIQUE_ID = new Guid("3FA5DF93-77B2-ADDB-248A-910AAE3BEBFB");

        public static readonly Guid T_CATEGORY_RULE_UNIQUE_ID = new Guid("D95B73E5-5EDE-2C50-A9B4-1C7D75413001");

        public static readonly Guid ROT_RULE_UNIQUE_ID = new Guid("F77DC78E-75A7-25D9-D8A7-73FB4DB23B87");

        public static readonly Guid SIZE_RANGE_UNIQUE_ID = new Guid("46E074F9-3D24-93D0-1619-FC0C81149884");

        public static readonly Guid DATE_RANGE_UNIQUE_ID = new Guid("0FFC0501-D919-0FE2-4891-A6D15D30D310");

        public static readonly String SIZE_RANGE_NAME = $"tags_{SIZE_RANGE_UNIQUE_ID.ToString().ToLower().Replace("-", "")}";

        public static readonly String DATE_RANGE_COLUMN_NAME = $"tags_{DATE_RANGE_UNIQUE_ID.ToString().ToLower().Replace("-", "")}";
    }
}
