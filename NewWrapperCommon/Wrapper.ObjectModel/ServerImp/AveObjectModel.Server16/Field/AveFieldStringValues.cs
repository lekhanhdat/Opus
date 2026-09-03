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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server16
{
    class AveFieldStringValues : IAveFieldStringValues
    {
        private const string mFieldStringValues_Type = "Microsoft.SharePoint.SPFieldStringValues";
        private object mFieldStringValues;
        private SPListItem m_Item;

        public AveFieldStringValues(object fieldStringValues)
        {
            mFieldStringValues = fieldStringValues;
        }

        public AveFieldStringValues(SPListItem item, AveFieldValuesType type)
        {
            m_Item = item;
            mFieldStringValues = AveAssemblyUtility.CreateInstance(mFieldStringValues_Type, new Type[] { item.GetType(), typeof(int) }, new object[] { item, (int)type });
        }

        #region IAveFieldStringValues Members

        public string[] FieldNames
        {
            get
            {
                return (string[])AveAssemblyUtility.GetPropertyValue(m_Item, "FieldNames");
            }
        }

        public string GetFieldValue(string fieldName)
        {
            return AveAssemblyUtility.InvokeMethod(mFieldStringValues, "GetFieldValue", new Type[] { typeof(string) }, new object[] { fieldName }) as string;
        }

        #endregion
    }
}
