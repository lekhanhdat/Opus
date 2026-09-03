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
using AvePoint.Wrapper.Core.Internal;
using AvePoint.Wrapper.Common.Office;
using AvePoint.ObjectModel.Common;
using AvePoint.ObjectModel.Common.Office;
using AvePoint.Wrapper.Core.SPAPI;
using System.Net;

namespace AvePoint.Wrapper.Core.WrapperAPI
{
    class AveWrapperAPIUtility : ISPAPIUtility
    {
        public IAveWebApplication GetWebApplication(string url)
        {
            throw new NotImplementedException();
        }

        public IAveSite GetSiteCollection(string url)
        {
            throw new NotImplementedException();
        }

        public IAveTenant GetAdminSite(string adminUrl, Common.O365AccountInfo o365AccountInfo)
        {
            var account = new AveBPOSAccountInfo()
            {
                Domain = o365AccountInfo.Domain,
                Password = o365AccountInfo.Password,
                UserName = o365AccountInfo.UserName
            };
            //IAveTenant tenant = null;
            //if (isOnline)
            //{ tenant = new AveTenant(adminUrl, account); }
            //else
            //{ tenant = new AveLocalTenant(adminUrl, account); }
            return new AveTenant(adminUrl, account); ;
        }

        public IAveSite GetSiteCollection(string url, Common.O365AccountInfo o365AccountInfo)
        {
            AveSite spSite;
            try
            {
                spSite = new AveSite(url, new AveBPOSAccountInfo() { Domain = o365AccountInfo.Domain, Password = o365AccountInfo.Password, UserName = o365AccountInfo.UserName });
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            catch (WebException)
            {
                return null;
            }
            return spSite;
        }

        public IAveSite GetSiteCollection(string url, IAveUserToken token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Wrapper Site collection
        /// </summary>
        /// <param name="siteObj"></param>
        /// <returns></returns>
        public IAveSite WrapperSiteCollection(object siteObj)
        {
            return null;
        }

        public IAveQuery CreateQuery()
        {
            throw new NotSupportedException("This method is not supported in BPOS mode.");
        }


        public IAveServiceContext GetServiceContext()
        {
            return new AveServiceContext();
        }

        public IAveSiteSubscriptionIdentifier GetSiteSubscriptionIdentifier()
        {
            throw new NotImplementedException();
        }

        public IAveRegionalSettings CreateRegionalSetting(IAveWeb web, bool bIsUserRegionalSetting)
        {
            throw new NotImplementedException();
        }


        public IAveOSocialCommentManager GetSocialCommentManager(IAveServiceContext context)
        {
            return new AveOSocialCommentManager(context);
        }


        public IAveTaxonomySession GetTaxonomySession(IAveServiceContext context)
        {
            throw new NotImplementedException();
        }


        public bool Support(Common.WrapperSPMode spMode, Version version)
        {
            return spMode == Common.WrapperSPMode.O365 && version != null && version.Major == 0;
        }

        public IAveFarm GetFarm()
        {
            throw new NotImplementedException();
        }


        public IAveWebService GetContentService()
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// 不支持Search
        /// </summary>
        /// <returns></returns>
        public IAveOScopeInfo CreateSearchScopeInfo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 不支持Search
        /// </summary>
        /// <returns></returns>
        public IAveORuleInfo CreateRuleInfo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 不支持Search
        /// </summary>
        /// <returns></returns>
        public IAveODisplayGroupInfo CreateDisplayGroupInfo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 进行一些初始化工作，如Server端初始化AveAssebmlyUtility的Assembly Cache。
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {

        }


        public IAveOUserProfileManager GetUserProfileManager(IAveServiceContext context, IAveSite site)
        {

            return new AveOUserProfileManager(context as AveServiceContext, site as AveSite);
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
            throw new NotSupportedException();
        }
    }
}