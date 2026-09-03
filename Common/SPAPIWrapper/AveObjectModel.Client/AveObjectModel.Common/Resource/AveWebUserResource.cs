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
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Common
{
    class AveWebUserResource : AveUserResource
    {
        private string mResourceName;
        private AveWeb mWeb;
        private IAveRequest request;
        private Dictionary<string, string> mKeyValues;

        public AveWebUserResource(AveWeb web, string resourceName, AveClientObjectData dataCache)
        {
            // TODO: Complete member initialization
            this.mWeb = web;
            this.request = web.SPRequest;
            this.mResourceName = resourceName;

            this.mKeyValues = dataCache.GetProperty<Dictionary<string, string>>(resourceName);
            if (mKeyValues != null)
            {
                base.keyValues = mKeyValues;
            }
        }

        protected override string GetValueForUICultureWithRequest(string cultureName)
        {
            return request.GetWebUserResource(mWeb.ServerRelativeUrl, cultureName,mResourceName);
        }

        protected override void InternalUpdate(Dictionary<string, string> changedTitle)
        {
            request.SetWebUserResource(mWeb.ServerRelativeUrl, mResourceName, changedTitle);
        }
    }
}
