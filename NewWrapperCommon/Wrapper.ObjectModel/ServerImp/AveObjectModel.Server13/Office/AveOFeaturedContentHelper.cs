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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOFeaturedContentHelper : AveOKeywordHelper, IAveOFeaturedContentHelper   
    {
        private const string mFeaturedContentHelper_Type = "Microsoft.Office.Server.Search.Extended.Administration.Facade.FeaturedContentHelper";
        private object mFeaturedContentHelper;

        public AveOFeaturedContentHelper(string siteID, IAveServiceContext serviceContext)
            :base(siteID,serviceContext)
        {
            mFeaturedContentHelper = AveAssemblyUtility.CreateInstance(mFeaturedContentHelper_Type, new Type[] { typeof(string), typeof(SPServiceContext) }, new object[] { siteID, (serviceContext as AveServiceContext).ServiceContext });
        }

        #region IAveOFeaturedContentHelper Members
        
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint method name")]
        public bool AddFeaturedConent(string keyWordTerm, string title, string url, ArrayList userContexts, DateTime? startDate, DateTime? endDate)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mFeaturedContentHelper, "AddFeaturedConent", new Type[] { typeof(string), typeof(string), typeof(string), typeof(ArrayList), typeof(DateTime), typeof(DateTime) }, new object[] {keyWordTerm, title, url, userContexts, startDate, endDate });
        }

        public bool DeleteFeaturedContent(string featuredContentName)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mFeaturedContentHelper, "DeleteFeaturedContent", new Type[] { typeof(string) }, new object[] { featuredContentName });
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint method name")]
        public bool ModifyFeaturedConent(string keyWordTerm, string featuredContentName, string newName, string url, ArrayList contexts, DateTime? startDate, DateTime? endDate)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mFeaturedContentHelper, "ModifyFeaturedConent", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(ArrayList), typeof(DateTime), typeof(DateTime) }, new object[] { keyWordTerm, featuredContentName, newName, url, contexts, startDate, endDate });
        }

        #endregion
    }
}
