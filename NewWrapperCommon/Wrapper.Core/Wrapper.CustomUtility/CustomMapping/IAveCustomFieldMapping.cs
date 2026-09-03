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

namespace AvePoint.Wrapper.Common
{
    public interface IAveCustomFieldMapping : IDisposable
    {
        /// <summary>
        /// Get the mapping field info according to the source field info.
        /// </summary>
        /// <param name="sourceFieldInfo">Source field info</param>
        /// <returns>A custom field object. Return null if no mapping needed</returns>
        AveCustomFieldInfo GetMappingFieldBeforeAdd(AveSourceFieldInfo sourceFieldInfo);

        /// <summary>
        /// Get all the new fields which need to be created in the destination.
        /// PS: Add to the default content type by default.
        /// </summary>
        /// <returns>The collection of the field info which you want to create in the destination. Return null if no field needed to be created</returns>
        List<AveCustomFieldInfo> GetNewFieldsBeforeAdd();

        /// <summary>
        /// Get the value mapping according to the source information
        /// </summary>
        /// <param name="sourceFieldValueInfo">The source value information</param>
        /// <returns>The value after mapping</returns>
        string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo);

        List<string> GetMultiMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo);

        /// <summary>
        /// No use for custom field mapping for now
        /// </summary>
        /// <param name="fieldInternalName"></param>
        /// <returns></returns>
        object GetMappingNullValue(string fieldInternalName);

        void GetValuesFromExcel(string excelPath);

        string GetValueFromGuiMapping(AveSourceFieldValueInfo source);
    }
}
