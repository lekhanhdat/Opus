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
using Microsoft.Office.DocumentManagement.MetadataNavigation;
using AvePoint.Common;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server13.Office
{
    abstract class AveOMetadataNavigationItem : IAveOMetadataNavigationItem
    {
        private readonly string mMetadataNavigationItem_IsSupportedType_Member = "IsSupportedType";
        private readonly string mMetadataNavigationItem_SupportedFields_Member = "SupportedFields";
        private readonly string mMetadataNavigationItem_UpdateToMatchSPField_Mothed = "UpdateToMatchSPField";
        private readonly string mMetadataNavigationItem_SpecialFieldIdContentType_Member = "SpecialFieldIdContentType";
        private readonly string mMetadataNavigationItem_SpecialFieldIdFolder_Member = "SpecialFieldIdFolder";
        private readonly string mMetadataNavigationItem_InitializeForContentType_Mothed = "InitializeForContentType";
        private readonly string mMetadataNavigationItem_InitializeForFolder_Mothed = "InitializeForFolder";
        private readonly string mMetadataNavigationItem_InitializeFromSPField_Mothed = "InitializeFromSPField";
        private readonly string mMetadataNavigationItem_TryGetFieldObject_Mothed = "TryGetFieldObject";
        protected MetadataNavigationItem mMetadataNavigationItem;
        private AveOSupportedFieldsLookup mSuppertedFields;

        public AveOMetadataNavigationItem(MetadataNavigationItem metadataNavigationItem)
        {
            mMetadataNavigationItem = metadataNavigationItem;
        }

        public AveOMetadataNavigationItem()
        {
            mMetadataNavigationItem = new MetadataNavigationHierarchy();
        }

        public AveOMetadataNavigationItem(IAveField metaDataField)
        {
            mMetadataNavigationItem = new MetadataNavigationHierarchy((metaDataField as AveField).Field);
        }

        internal MetadataNavigationItem MetadataNavigationItem
        {
            get
            {
                return mMetadataNavigationItem;
            }
        }

        #region IAveMetadataNavigationItem Members

        public Guid FieldId
        {
            get { return mMetadataNavigationItem.FieldId; }
        }

        public string FieldDisplayName
        {
            get
            {
                return mMetadataNavigationItem.FieldDisplayName;
            }
        }

        public bool IsSupportedType
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mMetadataNavigationItem, mMetadataNavigationItem_IsSupportedType_Member);
            }
        }

        public IAveOSupportedFieldsLookup SupportedFields
        {
            get
            {
                if (mSuppertedFields == null)
                {
                    mSuppertedFields = new AveOSupportedFieldsLookup(AveAssemblyUtility.GetPropertyValue(mMetadataNavigationItem, mMetadataNavigationItem_SupportedFields_Member));
                }
                return mSuppertedFields;
            }
        }

        public Guid SpecialFieldIdContentType
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetFieldValue(mMetadataNavigationItem, typeof(MetadataNavigationItem), mMetadataNavigationItem_SpecialFieldIdContentType_Member);
            }
        }

        public Guid SpecialFieldIdFolder
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetFieldValue(mMetadataNavigationItem, typeof(MetadataNavigationItem), mMetadataNavigationItem_SpecialFieldIdFolder_Member);
            }
        }

        public void UpdateToMatchSPField(IAveField matchingField)
        {
            AveAssemblyUtility.InvokeMethod(mMetadataNavigationItem, mMetadataNavigationItem_UpdateToMatchSPField_Mothed, new Type[] { typeof(SPField) }, new object[] { (matchingField as AveField).Field });
        }

        public void InitializeForContentType(string displayName)
        {
            AveAssemblyUtility.InvokeMethod(mMetadataNavigationItem, mMetadataNavigationItem_InitializeForContentType_Mothed, new Type[] { typeof(string) }, new object[] { displayName });
        }

        public void InitializeForFolder()
        {
            AveAssemblyUtility.InvokeMethod(mMetadataNavigationItem, mMetadataNavigationItem_InitializeForFolder_Mothed, new Type[] { }, new object[] { });
        }

        public void InitializeFromSPField(IAveField metaDataField)
        {
            AveAssemblyUtility.InvokeMethod(mMetadataNavigationItem, mMetadataNavigationItem_InitializeFromSPField_Mothed, new Type[] { typeof(SPField) }, new object[] { (metaDataField as AveField).Field });
        }

        public IAveField TryGetFieldObject(IAveFieldCollection sourceFieldCollection)
        {
            SPField field = (SPField)AveAssemblyUtility.InvokeMethod(mMetadataNavigationItem, mMetadataNavigationItem_TryGetFieldObject_Mothed, new Type[] { typeof(SPFieldCollection) }, new object[] { (sourceFieldCollection as AveFieldCollection).FieldCollection });
            if (field == null)
            {
                return null;
            }
            return new AveField(field);
        }

        public IAveField TryGetFieldObject(IAveWeb web, Guid listId)
        {
            SPField field = (SPField)AveAssemblyUtility.InvokeMethod(mMetadataNavigationItem, mMetadataNavigationItem_TryGetFieldObject_Mothed, new Type[] { typeof(SPWeb), listId.GetType() }, new object[] { (web as AveWeb).Web, listId });
            if (field == null)
            {
                return null;
            }
            return new AveField(field);
        }

        public bool IsContentTypeField
        {
            get
            {
                return mMetadataNavigationItem.IsContentTypeField;
            }
        }

        public string FieldTypeAsString
        {
            get
            {
                return mMetadataNavigationItem.FieldTypeAsString;
            }
        }

        public string FieldTitle
        {
            get
            {
                return mMetadataNavigationItem.FieldTitle;
            }
        }

        public AveFieldType FieldType
        {
            get { return (AveFieldType)mMetadataNavigationItem.FieldType; }
        }

        public bool IsTaxonomyField
        {
            get { return mMetadataNavigationItem.IsTaxonomyField; }
        }

        #endregion
    }
}
