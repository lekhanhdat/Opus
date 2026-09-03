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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server13
{
    class AveContentTypePageUtil : IAveContentTypePageUtil
    {
        private object mContentTypePageUtil;
        private const string mContentTypePageUtil_Type = "Microsoft.SharePoint.ApplicationPages.ContentTypePageUtil";

        public AveContentTypePageUtil(object contentTypePageUtil)
        {
            mContentTypePageUtil = contentTypePageUtil;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ContentTypePageUtil is SharePoint type name")]
        public AveContentTypePageUtil()
            : this(AveAssemblyUtility.CreateInstance(mContentTypePageUtil_Type))
        { }

        public IAveContentType GetSource(IAveField field, IAveContentType ct, IAveContentTypeCollection availableCts)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }
            if (ct == null)
            {
                throw new ArgumentNullException("ct");
            }
            if (availableCts == null)
            {
                throw new ArgumentNullException("availableCts");
            }
            if ((ct.ID as AveContentTypeId).ContentTypeId != (SPContentTypeId)AveAssemblyUtility.GetStaticPropertyValue(typeof(SPContentTypeId), "Root"))
            {
                IAveContentType type = availableCts[ct.ID.Parent];
                do
                {
                    if ((type == null) || (type.FieldLinks[field.ID] == null))
                    {
                        return ct;
                    }
                    ct = type;
                    type = availableCts[ct.ID.Parent];
                }
                while ((type != null) && (type.ID.Length != 0));
            }
            return ct;

        }
    }
}
