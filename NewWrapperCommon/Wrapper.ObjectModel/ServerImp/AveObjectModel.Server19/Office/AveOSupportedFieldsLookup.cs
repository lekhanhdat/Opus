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
using Microsoft.Office.DocumentManagement.MetadataNavigation;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSupportedFieldsLookup : IAveOSupportedFieldsLookup
    {
        private readonly string mSupportedFieldsLookup_Type = "Microsoft.Office.DocumentManagement.MetadataNavigation.SupportedFieldsLookup";
        private readonly string mSupportedFieldsLookup_SupportsKeywordFields_Member = "SupportsKeywordFields";
        private readonly string mSupportedFieldsLookup_IsSupported_Mothed = "IsSupported";
        private object mSupportedFieldsLookup;

        public AveOSupportedFieldsLookup()
        {
            mSupportedFieldsLookup = AveAssemblyUtility.CreateInstance(mSupportedFieldsLookup_Type);
        }

        public AveOSupportedFieldsLookup(object supportedFieldsLookup)
        {
            mSupportedFieldsLookup = supportedFieldsLookup;
        }

        #region IAveSupportedFieldsLookup Members

        public bool SupportsKeywordFields
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSupportedFieldsLookup, mSupportedFieldsLookup_SupportsKeywordFields_Member);
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSupportedFieldsLookup, mSupportedFieldsLookup_SupportsKeywordFields_Member, value);
            }
        }

        public bool IsSupported(IAveOMetadataNavigationItem metadataNavigationItem)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mSupportedFieldsLookup, mSupportedFieldsLookup_IsSupported_Mothed, new Type[] { typeof(MetadataNavigationItem) }, new object[] { (metadataNavigationItem as AveOMetadataNavigationItem).MetadataNavigationItem });
        }

        public bool IsSupported(IAveField field)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mSupportedFieldsLookup, mSupportedFieldsLookup_IsSupported_Mothed, new Type[] { typeof(SPField) }, new object[] { (field as AveField).Field });
        }

        #endregion
    }
}
