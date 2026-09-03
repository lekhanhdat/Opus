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
using System.Collections;
using System.Collections.ObjectModel;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOFieldIndexDictionary : KeyedCollection<string, IAveOFieldIndexRef>, IAveOFieldIndexDictionary
    {
        private const string mFieldIndexDictionary_Type = "Microsoft.Office.Server.Utilities.FieldIndexDictionary";
        private const string mFieldIndexDictory_GetAvailableIndicesForList_Method = "GetAvailableIndicesForList";
        private object mFieldIndexDictionary;

        public AveOFieldIndexDictionary()
        {
            mFieldIndexDictionary = AveAssemblyUtility.CreateInstance(mFieldIndexDictionary_Type);
            InsertItemToKeyedCollection();
        }

        public AveOFieldIndexDictionary(object fieldIndexDictionary)
        {
            mFieldIndexDictionary = fieldIndexDictionary;
            InsertItemToKeyedCollection();
        }

        internal void InsertItemToKeyedCollection()
        {
            int index = 0;
            foreach (object fieldIndexRef in mFieldIndexDictionary as IList)
            {
                InsertItem(index, new AveOFieldIndexRef(fieldIndexRef));
                index++;
            }
        }

        internal object FieldIndexDictionary
        {
            get
            {
                return mFieldIndexDictionary;
            }
        }

        protected override string GetKeyForItem(IAveOFieldIndexRef item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            return (item as AveOFieldIndexRef).FieldIndexKey;
        }

        #region IAveFieldIndexDictionary Members

        public IAveOFieldIndexDictionary GetAvailableIndicesForList(IAveList sourceList)
        {
            return GetAvailableIndicesForList(sourceList, true);
        }

        public IAveOFieldIndexDictionary GetAvailableIndicesForList(IAveList sourceList, bool includeMultiValueLookup)
        {
            return new AveOFieldIndexDictionary(AveAssemblyUtility.InvokeStaticMethod(mFieldIndexDictionary_Type, mFieldIndexDictory_GetAvailableIndicesForList_Method, new Type[] { typeof(SPList), includeMultiValueLookup.GetType() }, new object[] { (sourceList as AveList).List, includeMultiValueLookup }));
        }

        #endregion
    }
}
