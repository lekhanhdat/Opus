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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveFieldIndexCollection : AveAbstractCommonCollection<IAveFieldIndex>, IAveFieldIndexCollection
    {
        private SPFieldIndexCollection mFieldIndexCollection;

        public AveFieldIndexCollection(SPFieldIndexCollection fieldIndexCollection)
            : base(fieldIndexCollection)
        {
            mFieldIndexCollection = fieldIndexCollection;
        }

        #region IAveFieldIndexCollection Members

        public Guid Add(IAveField primaryField, IAveField secondaryField)
        {
            return mFieldIndexCollection.Add((primaryField as AveField).Field, (secondaryField as AveField).Field);
        }

        public void Delete(Guid uniqueId)
        {
            mFieldIndexCollection.Delete(uniqueId);
        }

        public override IAveFieldIndex this[int index]
        {
            get
            {
                SPFieldIndex fieldIndex = mFieldIndexCollection[index];
                if (fieldIndex != null)
                {
                    return new AveFieldIndex(mFieldIndexCollection[index]);
                }
                return null;
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveFieldIndex(t as SPFieldIndex);
        }

        public override int Count
        {
            get { return mFieldIndexCollection.Count; }
        }

        public bool IsDirty
        {
            get
            {
                object result = AveAssemblyUtility.GetPropertyValue(mFieldIndexCollection, "IsDirty");
                return result != null ? (bool)result : false;
            }
        }

        #endregion
    }
}
