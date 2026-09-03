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
using Microsoft.Office.DocumentManagement.MetadataNavigation;
using AvePoint.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOMetadataNavigationHierarchy : AveOMetadataNavigationItem, IAveOMetadataNavigationHierarchy
    {
        private readonly string mMetadataNavigationHierarchy_SupportedFields_Member = "SupportedFields";
        private MetadataNavigationHierarchy mMetadataNavigationHierarchy;
        private AveOSupportedFieldsLookup mSupportedFields;

        public AveOMetadataNavigationHierarchy(MetadataNavigationHierarchy metadataNavigationHierarchy)
            : base(metadataNavigationHierarchy)
        {
            mMetadataNavigationHierarchy = metadataNavigationHierarchy;
        }

        public AveOMetadataNavigationHierarchy()
            : this(new MetadataNavigationHierarchy())
        { }

        public AveOMetadataNavigationHierarchy(IAveField aveField)
            : base(aveField)
        {
            mMetadataNavigationHierarchy = new MetadataNavigationHierarchy((aveField as AveField).Field);
            mMetadataNavigationItem = mMetadataNavigationHierarchy;
        }

        internal MetadataNavigationHierarchy MetadataNavigationHierarchy
        {
            get
            {
                return mMetadataNavigationHierarchy;
            }
        }

        #region IAveMetadataNavigationHierarchy Members

        public IAveOMetadataNavigationHierarchy CreateContentTypeHierarchy()
        {
            return new AveOMetadataNavigationHierarchy(MetadataNavigationHierarchy.CreateContentTypeHierarchy());
        }

        public bool IsFolderHierarchy
        {
            get
            {
                return mMetadataNavigationHierarchy.IsFolderHierarchy;
            }
        }

        public IAveOSupportedFieldsLookup SupportedFields
        {
            get
            {
                if (mSupportedFields == null)
                {
                    mSupportedFields = new AveOSupportedFieldsLookup(AveAssemblyUtility.GetPropertyValue(mMetadataNavigationHierarchy, mMetadataNavigationHierarchy_SupportedFields_Member));
                }
                return mSupportedFields;
            }
        }

        public IAveOMetadataNavigationHierarchy CreateFolderHierarchy()
        {
            return new AveOMetadataNavigationHierarchy(MetadataNavigationHierarchy.CreateFolderHierarchy());
        }

        #endregion
    }
}
