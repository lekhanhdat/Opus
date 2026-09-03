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

namespace AvePoint.Wrapper.Restore
{
    interface IValueConvertObject
    {
        object ConvertSingleValue(string value);
        object ConvertMultiValue(List<string> values);
    }
    class BaseValueConvertObject : IValueConvertObject
    {
        protected AveSPItem mItem;
        protected IAveField destField;
        protected int originalRowId;

        public BaseValueConvertObject(IAveField destField, AveSPItem mItem, int originalRowId)
        {
            this.destField = destField;
            this.mItem = mItem;
            this.originalRowId = originalRowId;
        }

        public virtual object ConvertSingleValue(string value)
        {
            return value;
        }

        public virtual object ConvertMultiValue(List<string> values)
        {
            return null;
        }

        protected object SerializeMultiValue(List<string> values, string splitChar)
        {
            StringBuilder str = new StringBuilder();
            bool hasValue = false;
            foreach (var v in values)
            {
                str.Append(v);
                str.Append(splitChar);
                hasValue = true;
            }
            if (hasValue)
            {
                str.Length -= splitChar.Length;
                return str.ToString();
            }
            else
            {
                return null;
            }
        }

        protected string ReplaceLinks(string value, IAveField destField)
        {
            using (new AvePerformanceScope("Restore.SetFieldValueBase.ReplaceLinks"))
            {
                bool needReplaceLast = false;
                string xmlLinks = AveReplaceProcessor.ReplaceXmlLinks(value, mItem.ParentSite.MappingManager, mItem.ParentSite.SourceSiteInfo, mItem.ParentSite.ServerRelativeUrl, this.mItem.ParentList.SPList, ref needReplaceLast);
                if (needReplaceLast)
                {
                    mItem.ParentList.ParentWeb.ParentSite.AddUnReplaceUrlIDCache(mItem.ParentList.ParentWeb.SPWeb.ID, mItem.ParentList.SPList.ID, originalRowId, destField.InternalName);
                }
                return xmlLinks;
            }
        }
    }
}
