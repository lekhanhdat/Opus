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

namespace AvePoint.Wrapper.Restore
{
    class NoteDataFormat : BaseDataFormat
    {
        private int rowId;
        public NoteDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem, int rowId) :
            base(xmlField, destField, mItem)
        {
            this.rowId = rowId;
        }

        public override object CheckFieldValue(object value)
        {
            bool needReplaceLast = false;
            string result = string.Empty;
            if (mItem.ParentList.SPList.BaseTemplate == AveListTemplateType.MicroFeed &&
                (destField.InternalName.Equals("RefRoot", System.StringComparison.OrdinalIgnoreCase) || (destField.InternalName.Equals("RefReply", System.StringComparison.OrdinalIgnoreCase))))
            {
                result = AveReplaceProcessor.ReplaceNewsFeedLinks(value.ToString(),
                    mItem.ParentList.SPList,
                    mItem.ParentSite.MappingManager,
                    mItem.ParentSite.SourceSiteInfo,
                    mItem.ParentSite.ServerRelativeUrl, ref needReplaceLast);
            }
            else
            {
                result = AveReplaceProcessor.ReplaceXmlLinks(value.ToString(), mItem.ParentSite.MappingManager, mItem.ParentSite.SourceSiteInfo, mItem.ParentSite.ServerRelativeUrl, this.mItem.ParentList.SPList, ref needReplaceLast);
            }
            if (needReplaceLast)
            {
                mItem.ParentList.ParentWeb.ParentSite.AddUnReplaceUrlIDCache(mItem.ParentList.ParentWeb.SPWeb.ID, mItem.ParentList.SPList.ID, rowId, destField.InternalName);
            }
            return result;
        }
    }
}
