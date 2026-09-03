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
    internal class AveContentTypeUserResource : AveUserResource
    {
        AveContentType mContentType;
        string mContentTypeResourceName;

        public AveContentTypeUserResource(AveContentType contentType, string resourceName, string contentTypeResourceName, AveClientObjectData dataCache)
            : base(contentType.ParentWeb as AveWeb, resourceName, dataCache)
        {
            mContentType = contentType;
            mContentTypeResourceName = contentTypeResourceName;
        }

        protected override void RetrieveValuesForAllLanguage()
        {
            if (mWeb.SupportedUICultures.Count() == 1)
            {
                switch (mResourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        cultureAndValueMappings[mWeb.LanguageCulture.Name] = mContentType.Name;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        cultureAndValueMappings[mWeb.LanguageCulture.Name] = mContentType.Description;
                        break;
                    default:
                        logger.Error("Unknow user resource. Name: {0}", mResourceName);
                        break;
                }
            }
            else
            {
                var listId = mContentType.ParentList == null ? Guid.Empty : mContentType.ParentList.ID;
                var mappings = mRequest.GetContentTypeUserResource(mWeb.ServerRelativeUrl, listId, mResourceName, mContentTypeResourceName, mContentType.ID.ToString(),
                    mWeb.SupportedUICultures.Select(c => c.Name).ToList());

                foreach (var para in mappings)
                {
                    cultureAndValueMappings.Add(para.Key, para.Value);
                }
            }
        }
    }
}
