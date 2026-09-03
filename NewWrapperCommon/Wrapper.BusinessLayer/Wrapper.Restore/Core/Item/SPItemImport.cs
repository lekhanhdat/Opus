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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    class SPItemImport
    {
        protected SPListImport parentList;

        protected SPItemImport(SPListImport parentList)
        {
            this.parentList = parentList;
        }

        /// <summary>
        /// 1.转换对应的Column Value
        /// 2.合并AllUserData & DataJunc等相关Column Value
        /// 3.需要调用ListFieldCollection的方法，List级别最好缓存特定类型的Column
        /// </summary>
        /// <param name="allUserData"></param>
        /// <param name="dataJunc"></param>
        /// <param name="TPGUIDLookupValue"></param>
        /// <returns></returns>
        protected Dictionary<string, AveFieldValueInfo> FixupColumnValues(Dictionary<string, object> allUserData, List<Dictionary<string, object>> dataJunc, Dictionary<string, string> TPGUIDLookupValue)
        {
            Dictionary<string, AveFieldValueInfo> columnValues = new Dictionary<string, AveFieldValueInfo>();

            return columnValues;
        }
    }
}
