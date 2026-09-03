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

namespace AvePoint.ObjectModel.Server13
{
    class AveMetadataListFieldSettings : IAveMetadataListFieldSettings
    {
        private const string mMetadataListFieldSettings_Type = "Microsoft.SharePoint.Taxonomy.MetadataListFieldSettings";
        private const string mMetadataListFieldSettings_EnableKeywordsField_Member = "EnableKeywordsField";
        private const string mMetadataListFieldSettings_EnableMetadataPromotion_Member = "EnableMetadataPromotion";
        private const string mMetadataListFieldSettings_ListHasKeywordsField_Member = "ListHasKeywordsField";
        private const string mMetadatalistFieldSettings_KeywordsFieldExistsInContentTypes_Method = "KeywordsFieldExistsInContentTypes";
        private const string mMetadatalistFieldSettings_Update_Method = "Update";
        private object mMetadataListFieldSettings;

        public AveMetadataListFieldSettings(IAveList list)
        {
            mMetadataListFieldSettings = AveAssemblyUtility.CreateInstance(mMetadataListFieldSettings_Type, new Type[] { ((list as AveList).List).GetType() }, new object[] { ((list as AveList).List) });
        }

        #region IAveMetadataListFieldSettings Members

        public bool EnableKeywordsField
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mMetadataListFieldSettings, mMetadataListFieldSettings_EnableKeywordsField_Member);
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mMetadataListFieldSettings, mMetadataListFieldSettings_EnableKeywordsField_Member, value);
            }
        }

        public bool EnableMetadataPromotion
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mMetadataListFieldSettings, mMetadataListFieldSettings_EnableMetadataPromotion_Member);
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mMetadataListFieldSettings, mMetadataListFieldSettings_EnableMetadataPromotion_Member, value);
            }
        }

        public bool ListHasKeywordsField
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mMetadataListFieldSettings, mMetadataListFieldSettings_ListHasKeywordsField_Member);
            }
        }

        public bool KeywordsFieldExistsInContentTypes(bool bAdd)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mMetadataListFieldSettings, mMetadatalistFieldSettings_KeywordsFieldExistsInContentTypes_Method, new object[] { bAdd });
        }

        public void Update()
        {
            AveAssemblyUtility.InvokeMethod(mMetadataListFieldSettings, mMetadatalistFieldSettings_Update_Method, new object[] { });
        }

        #endregion
    }
}
