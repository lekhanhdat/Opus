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
using System.Web;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Restore
{
    class URLDataFormat:BaseDataFormat
    {
        private string description;
        private int originalVersion;

        public URLDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem, string description,int originalVersion) :
            base(xmlField, destField, mItem)
        {
            this.description = description;
            this.originalVersion = originalVersion;
        }
        public override object CheckFieldValue(object value)
        {
            return GetUrlValue(value, mItem.RowId);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "/_catalogs/masterpage")]
        private IAveFieldUrlValue GetUrlValue(object value, int docRowId)
        {
            bool needSiteCollectionLevel = mItem.ParentList.SPList != null
                && (mItem.ParentList.SPList.BaseTemplate == AveListTemplateType.DesignCatalog
                     && (base.destField.InternalName.Equals("ThemeUrl", StringComparison.Ordinal)
                         || base.destField.InternalName.Equals("FontSchemeUrl", StringComparison.Ordinal)
                        )
                    );
            var urlValue = mItem.ParentSite.ObjectModelFactory.CreateFieldUrlValue();
            string url = value.ToString();
            urlValue.Url = url;
            urlValue.Description = description;
            return urlValue;
        }
    }
}
