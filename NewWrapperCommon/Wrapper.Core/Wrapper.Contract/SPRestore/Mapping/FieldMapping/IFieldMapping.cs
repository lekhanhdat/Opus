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

namespace AvePoint.Wrapper.Core.SPRestore.Mapping
{
    using System.Collections.Generic;

    /// <summary>
    /// Basic interface for column mapping and column value mapping
    /// </summary>
    public interface IFieldMapping
    {
        /// <summary>
        /// Get the field info after mapping
        /// </summary>
        /// <param name="sourceFieldInfo">
        /// Source Field basic info.
        /// Use the derived class if you want to use more field properties
        /// [IMPORTANT]For callers, you must define which properties can be used and make sure these properties are signed properly.
        /// </param>
        /// <returns>
        /// Destiation field info after mapping, null info not mapped(todo:oliver返回空还是本身). 
        /// Return the derived class if you want to get more field properties
        /// [IMPORTANT]For implementor, you must define which properties can be used and make sure these properties are signed properly.
        /// </returns>
        SPFieldInfo GetMappingFieldInfo(SPConditionableFieldInfo sourceFieldInfo);
        /// <summary>
        /// Get the field value after mapping
        /// </summary>
        /// <param name="sourceFieldValueInfo">
        /// Source field value info, including todo:Oliver
        /// Use the derived class if you want to use more field properties
        /// [IMPORTANT]For callers, you must define which properties can be used and make sure these properties are signed properly.
        /// </param>
        /// <returns>
        /// string format of the field value after mapping, (todo:oliver返回空还是本身).
        /// </returns>
        string GetMappingFieldValue(SPFieldValueInfo sourceFieldValueInfo);
        /// <summary>
        /// Get a list of field info which is new added by mapping.
        /// 目前Excel Mapping和Dynamic Mapping会用到
        /// </summary>
        /// <returns></returns>
        List<SPFieldInfo> GetNewAddedFields();
        /// <summary>
        /// Get a list of field name and value pairs which is new added by mapping.
        /// 原端column值为空, Mapping成非空的值, 返回mapping后的field internal name和value的集合
        /// </summary>
        /// <returns>
        /// (field internal name after mapping, field value after mapping), 如果没有返回null。
        /// </returns>
        Dictionary<string, string> GetNewAddedFieldValues();
    }
}
