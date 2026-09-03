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



using Microsoft.Office.DocumentManagement.MetadataNavigation;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOMetadataNavigationKeyFilter : AveOMetadataNavigationItem, IAveOMetadataNavigationKeyFilter
    {
        private readonly string mMetadataNavigationKeyFilter_SupportedFields_Member = "SupportedFields";
        private MetadataNavigationKeyFilter mMetadataNavigationKeyFilter;
        private AveOSupportedFieldsLookup mSupportedFields;

        public AveOMetadataNavigationKeyFilter(MetadataNavigationKeyFilter metadataNavigationKeyFilter)
            : base(metadataNavigationKeyFilter)
        {
            mMetadataNavigationKeyFilter = metadataNavigationKeyFilter;
        }

        public AveOMetadataNavigationKeyFilter()
            : this(new MetadataNavigationKeyFilter())
        { }

        public AveOMetadataNavigationKeyFilter(IAveField field)
            : this(new MetadataNavigationKeyFilter((field as AveField).Field))
        { }

        internal MetadataNavigationKeyFilter MetadataNavigationKeyFilter
        {
            get
            {
                return mMetadataNavigationKeyFilter;
            }
        }

        public IAveOSupportedFieldsLookup SupportedFields
        {
            get
            {
                if (mSupportedFields == null)
                {
                    mSupportedFields = new AveOSupportedFieldsLookup(AveAssemblyUtility.GetPropertyValue(mMetadataNavigationKeyFilter, mMetadataNavigationKeyFilter_SupportedFields_Member));
                }
                return mSupportedFields;
            }
        }
    }
}
