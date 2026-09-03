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

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveONodeViewSettings : KeyedCollection<Guid, IAveOConfiguredView>, IAveONodeViewSettings
    {
        private object mNodeViewSettings;
        private AveOViewSettingsCollection mParentCollection;
        private const string mNodeViewSettingsType = "Microsoft.Office.DocumentManagement.MetadataNavigation.NodeViewSettings";

        public AveONodeViewSettings(IAveOViewSettingsCollection viewSettingsCollection, string uniqueNodeId, int folderId)
        {
            mNodeViewSettings = AveAssemblyUtility.CreateInstance(mNodeViewSettingsType, new Type[] { ((viewSettingsCollection as AveOViewSettingsCollection).ViewSettingsCollection).GetType(), typeof(string), typeof(int) }, new object[] { (viewSettingsCollection as AveOViewSettingsCollection).ViewSettingsCollection, uniqueNodeId, folderId });
            mParentCollection = (viewSettingsCollection as AveOViewSettingsCollection);
            InsertItemToKeyedCollection();
        }

        public AveONodeViewSettings(object nodeViewSettings)
        {
            mNodeViewSettings = nodeViewSettings;
            InsertItemToKeyedCollection();
        }

        internal void InsertItemToKeyedCollection()
        {
            int index = 0;
            foreach (object configuredView in mNodeViewSettings as IList)
            {
                InsertItem(index, new AveOConfiguredView(configuredView));
                index++;
            }
        }

        internal object NodeViewSettings
        {
            get
            {
                return mNodeViewSettings;
            }
        }

        #region IAveNodeViewSettings Members

        public IAveOViewSettingsCollection ParentCollection
        {
            get
            {
                if (mParentCollection == null)
                {
                    object viewSettingsCollection = AveAssemblyUtility.GetPropertyValue(mNodeViewSettings, "ParentCollection");
                    if (viewSettingsCollection != null)
                    {
                        mParentCollection = new AveOViewSettingsCollection(viewSettingsCollection);
                    }
                }
                return mParentCollection;
            }
        }

        public string UniqueNodeId
        {
            get
            {
                return AveAssemblyUtility.GetPropertyValue(mNodeViewSettings, "UniqueNodeId") as string;
            }
        }

        public Guid DefaultViewId
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetPropertyValue(mNodeViewSettings, "DefaultViewId");
            }
        }

        #endregion

        protected override Guid GetKeyForItem(IAveOConfiguredView item)
        {
            if (item == null)
            {
                return new Guid();
            }
            return (item as AveOConfiguredView).ViewId;
        }

        public void Add(IAveOConfiguredView configuredView)
        {
            AveAssemblyUtility.InvokeMethod(mNodeViewSettings, "Add", new object[] { (configuredView as AveOConfiguredView).ConfigureView });
            base.Add(configuredView);
        }

        public void Clear()
        {
            AveAssemblyUtility.InvokeMethod(mNodeViewSettings, "Clear", new Type[] { }, new object[] { });
            base.Clear();
        }
    }
}
