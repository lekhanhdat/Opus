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

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOMetadataNavigationCollection<TItemType> : KeyedCollection<Guid, TItemType>, IAveOMetadataNavigationCollection<TItemType> where TItemType : IAveOMetadataNavigationItem
    {
        private readonly string mMetadataNavigationCollection_Type = "Microsoft.Office.DocumentManagement.MetadataNavigation.MetadataNavigationCollection<TItemType>";
        private object mMetadataNavigationCollection;

        public AveOMetadataNavigationCollection()
        {
            mMetadataNavigationCollection = AveAssemblyUtility.CreateInstance(mMetadataNavigationCollection_Type);
            InsertItemToKeyedCollection();
        }

        public AveOMetadataNavigationCollection(object metadataNavigationCollection)
        {
            mMetadataNavigationCollection = metadataNavigationCollection;
            InsertItemToKeyedCollection();
        }

        internal void InsertItemToKeyedCollection()
        {
            int index = 0;
            foreach (object configuredView in mMetadataNavigationCollection as IList)
            {
                object metadataNaviagtionItem = AveServerAssemblyInit.CreateElement(typeof(IAveOMetadataNavigationItem), configuredView);
                InsertItem(index, ((TItemType)metadataNaviagtionItem));
                index++;
            }
        }

        protected override Guid GetKeyForItem(TItemType item)
        {
            return item.FieldId;
        }

        public new bool Remove(Guid key)
        {
            bool retValue = (bool)AveAssemblyUtility.InvokeMethod(mMetadataNavigationCollection, "Remove", new Type[] { typeof(Guid) }, new object[] { key });
            base.Remove(key);
            return retValue;
        }
    }
}
