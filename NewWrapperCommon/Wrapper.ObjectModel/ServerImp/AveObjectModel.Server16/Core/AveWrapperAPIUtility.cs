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

using AvePoint.ObjectModel.Server16;
using AvePoint.ObjectModel.Server16.Office;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Wrapper.Core.SPAPI;
using Microsoft.SharePoint;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Core.WrapperAPI
{
    class AveWrapperAPIUtility : ISPAPIUtility
    {
        public IAveWebApplication GetWebApplication(string url)
        {
            var spWebApp = new AveWebApplication(url);

            if (spWebApp.WebApplication == null)
            {
                return null;
            }

            return spWebApp;
        }

        public IAveSite GetSiteCollection(string url)
        {
            AveSite spSite;
            try
            {
                spSite = new AveSite(url);
                if (spSite.Site == null)
                {
                    return null;
                }

            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            return spSite;
        }

        public IAveSite GetSiteCollection(string url, Common.O365AccountInfo o365AccountInfo)
        {
            throw new NotImplementedException();
        }

        public IAveSite GetSiteCollection(string url, IAveUserToken token)
        {
            AveSite spSite;
            try
            {
                spSite = new AveSite(url, token);
                if (spSite.Site == null)
                {
                    return null;
                }

            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            return spSite;
        }

        /// <summary>
        /// Wrapper Site collection
        /// </summary>
        /// <param name="siteObj"></param>
        /// <returns></returns>
        public IAveSite WrapperSiteCollection(object siteObj)
        {
            if (siteObj != null && siteObj is Microsoft.SharePoint.SPSite)
            {
                return new AveSite(siteObj as Microsoft.SharePoint.SPSite);
            }

            return null;
        }

        public IAveQuery CreateQuery()
        {
            return new AveQuery();
        }


        public IAveServiceContext GetServiceContext()
        {
            return new AveServiceContext();
        }

        public IAveSiteSubscriptionIdentifier GetSiteSubscriptionIdentifier()
        {
            return new AveSiteSubscriptionIdentifier();
        }

        public IAveOUserProfileManager GetUserProfileManager(IAveServiceContext context, IAveSite site)
        {
            return new AveOUserProfileManager(context);
        }

        public IAveRegionalSettings CreateRegionalSetting(IAveWeb web, bool bIsUserRegionalSetting)
        {
            return new AveRegionalSettings(web, bIsUserRegionalSetting);
        }


        public IAveOSocialCommentManager GetSocialCommentManager(IAveServiceContext context)
        {
            return new AveOSocialCommentManager(context);
        }


        public IAveTaxonomySession GetTaxonomySession(IAveServiceContext context)
        {
            return new AveTaxonomySession(context);
        }


        public bool Support(Common.WrapperSPMode spMode, Version version)
        {
            return spMode == Common.WrapperSPMode.Server && version != null && version.Major == 15;
        }


        public IAveTenant GetAdminSite(string adminUrl, Common.O365AccountInfo o365AccountInfo)
        {
            throw new NotImplementedException();
        }

        public IAveFarm GetFarm()
        {
            return new AveFarm().Local;
        }


        public IAveWebService GetContentService()
        {
            return new AveWebService(Microsoft.SharePoint.Administration.SPWebService.ContentService);
        }

        public IAveSecurity GetSecurity()
        {
            return new AveSecurity();
        }

        public IAveFieldUserValue CreateFieldUserValue(IAveWeb web, string fieldValue)
        {
            return new AveFieldUserValue(web, fieldValue);
        }

        public IAveFieldUrlValue CreateFieldUrlValue(string fieldValue)
        {
            return new AveFieldUrlValue(fieldValue);
        }


        public IAveOScopeInfo CreateSearchScopeInfo()
        {
            return new AveOScopeInfo();
        }

        public IAveORuleInfo CreateRuleInfo()
        {
            return new AveORuleInfo();
        }

        public IAveODisplayGroupInfo CreateDisplayGroupInfo()
        {
            return new AveODisplayGroupInfo();
        }

        /// <summary>
        /// 进行一些初始化工作，如Server端初始化AveAssebmlyUtility的Assembly Cache。
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", Justification = "This method won't run in a mutlithread environment")]
        public void Initialize()
        {
            AveServerAssemblyInit.LoadAssembly();

            //防止多线程调用该函数时，由于Dictionary Insert方法所导致的死循环。
            SPBuiltInFieldId.Contains(Guid.Empty);
        }

        public IAveFieldLink CreateFieldLink(IAveField field)
        {
            return new AveFieldLink(field as AveField);
        }

        public IAveWorkflowAssociation CreateWorkflowAssociation()
        {
            return new AveWorkflowAssociation();
        }

        public IAveContentType CreateContentType(IAveContentTypeId contentTypId)
        {
            return new AveContentType(contentTypId as AveContentTypeId);
        }

    }
}
