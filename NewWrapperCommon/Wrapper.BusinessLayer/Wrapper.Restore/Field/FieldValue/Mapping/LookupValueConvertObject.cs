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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using System.Reflection;

namespace AvePoint.Wrapper.Restore
{
    class LookupValueConvertObject : BaseValueConvertObject
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected IAveFieldLookup destLookupField;
        private int originalVersion;
        private object sourceValue;
        private string sourceFieldInternalName;

        public LookupValueConvertObject(IAveFieldLookup destField, AveSPItem mItem, int originalRowId, object sourceValue, int originalVersion, string sourceFieldName)
            : base(destField, mItem, originalRowId)
        {
            this.destLookupField = destField;
            this.originalVersion = originalVersion;
            this.sourceValue = sourceValue;
            this.sourceFieldInternalName = sourceFieldName;
        }

        public override object ConvertSingleValue(string value)
        {
            int itemId = GetLookupIdByMappingValue(value);
            if (itemId <= 0)
            {
                CacheLookupValueInfo();
                return null;
            }
            return itemId;
        }

        public override object ConvertMultiValue(List<string> values)
        {
            bool needCache = false;
            IAveFieldLookupValueCollection lookupValues = mItem.ParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
            foreach (var displayValue in values)
            {
                int itemId = GetLookupIdByMappingValue(displayValue);
                if (itemId > 0)
                {
                    lookupValues.Add(mItem.ParentSite.ObjectModelFactory.CreateFieldLookupValue(itemId, displayValue));
                }
                else
                {
                    needCache = true;
                }
            }
            if(needCache)
            {
                CacheLookupValueInfo();
            }
            return lookupValues;
        }

        private int GetLookupIdByMappingValue(string value)
        {
            int itemId=0;
            if (!string.IsNullOrEmpty(destLookupField.LookupList))
            {
                itemId = mItem.ParentSite.GetLookupItemIdByDisplayValue(destLookupField.LookupWebId, new Guid(destLookupField.LookupList), destLookupField.LookupField, value);
            }
            return itemId;
        }

        protected void CacheLookupValueInfo()
        {
            AveXmlField xmlField = mItem.ParentList.AveFields.XmlFields[sourceFieldInternalName];
            mItem.ParentSite.xmlFieldCache[destLookupField.ID] = xmlField;
            AveLookupObject obj;
            mItem.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromLookupFieldMapping(mItem.ParentList.SPList.ID, destLookupField.ID, out obj);
            Guid lookupListID = Guid.Empty;
            if (obj != null)
            {
                lookupListID = new Guid(obj.SourceListId);
            }
            else if (!string.IsNullOrEmpty(destLookupField.LookupList))
            {
                lookupListID = new Guid(destLookupField.LookupList);
            }
            else
            {
                //此种情况只有column mapping创建的lookup并且关联的lookuplist没有还原的情况下，此时的lookupListID只做key值，不做真实的lookupListID使用。并且在post action中没有使用该值
                lookupListID = Guid.NewGuid();
            }
            var lookupFieldIdValue = new AveLookupFieldInfo
            {
                LookupListID = lookupListID,
                LookupFieldID = destLookupField.ID,
                Version = originalVersion,
                LookupFieldValue = sourceValue
            };
            mItem.ParentList.AveFields.ResetListLookupFieldIdValues(lookupFieldIdValue);
        }
    }
}
