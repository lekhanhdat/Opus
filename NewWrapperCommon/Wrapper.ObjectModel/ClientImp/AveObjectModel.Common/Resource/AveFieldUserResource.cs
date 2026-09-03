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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AvePoint.ObjectModel.Common
{
    internal class AveFieldUserResource : AveUserResource
    {
        private AveField mField;
        private string mFieldResourceName;
        private Dictionary<string, object> mContentTypeProp;
        private Dictionary<string, object> mFieldProp = new Dictionary<string, object>();
        public AveFieldUserResource(AveField field, string resourceName, string fieldResourceName,  Dictionary<string, object> contentProp, AveClientObjectData dataCache)
            : base(field.ParentWeb, resourceName, dataCache)
        {
            mField = field;
            mFieldResourceName = fieldResourceName;
            mContentTypeProp = contentProp;
            this.mFieldProp["ObjectPath"] = mDataCache.GetProperty<object>("ObjectPath");
            this.mFieldProp["FieldType"] = mDataCache.GetProperty<object>("FieldType");
        }

        protected override void RetrieveValuesForAllLanguage()
        {
            if (mWeb.SupportedUICultures.Count() == 1)
            {
                switch (mResourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        cultureAndValueMappings[mWeb.LanguageCulture.Name] = mField.Title;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        cultureAndValueMappings[mWeb.LanguageCulture.Name] = mField.Description;
                        break;
                    default:
                        logger.Error("Unknow user resource. Name: {0}", mResourceName);
                        break;
                }
            }
            else
            {
                var listId = mField.ParentList == null ? Guid.Empty : mField.ParentList.ID;
                var mappings = mRequest.GetFieldUserResource(mWeb.ServerRelativeUrl, listId, mResourceName, mFieldResourceName, mContentTypeProp, mFieldProp,
                    mWeb.SupportedUICultures.Select(c => c.Name).ToList());

                foreach (var para in mappings)
                {
                    cultureAndValueMappings.Add(para.Key, para.Value);
                }
            }
        }
    }
}
