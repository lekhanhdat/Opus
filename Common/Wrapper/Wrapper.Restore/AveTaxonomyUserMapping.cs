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
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public class AveTaxonomyUserMapping
    {
        /// <summary>
        /// User Mapping列表.
        /// </summary>
        public Dictionary<string, string> UserMappings { get; set; }
        /// <summary>
        /// Domain Mapping 列表.
        /// </summary>
        public Dictionary<string, string> DomainMappings { get; set; }
        /// <summary>
        /// Source Default User
        /// </summary>
        public string SourceDefaultUser { get; set; }
        /// <summary>
        /// Target Default User
        /// </summary>
        public string TargetDefaultUser { get; set; }
    }

    public class AveTaxonomyUserMappingUtility
    {
        private AveLogger sLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IAveSite mSite;
        private IAveUtility mUtility;

        public AveTaxonomyUserMappingUtility(IAveSite site, AveObjectModelFactory objectModelFactory)
        {
            mSite = site;
            mUtility = objectModelFactory.Utility;
            //初始化webapp认证模式的provider
            AveAuthenticationUtility.InitAuthenticationProvider(site.WebApplication);
        }

        public string GetMappingUserLogin(string login, Dictionary<string, string> userMapping, Dictionary<string, string> domainMapping, out bool isSharepointGroup)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyUserMappingUtility.GetMappingUserLogin"))
            {
#endif
                var mappingManager = new AveSPUserMappingManager(userMapping, domainMapping);
                string newLogin = mappingManager.GetMappingUserLogin(login, false, true);
                newLogin = ResolvePrincipal(newLogin, out isSharepointGroup);
                return newLogin;

#if PerformanceLog
            }
#endif
        }
        private string ResolvePrincipal(string login, out bool isSharepointGroup)
        {
            isSharepointGroup = false;
            IAvePrincipalInfo info = mUtility.ResolvePrincipal(mSite.RootWeb, login, AvePrincipalType.SecurityGroup | AvePrincipalType.User | AvePrincipalType.SharePointGroup, AvePrincipalSource.All, null, false);
            if (info != null)
            {
                login = info.LoginName;
                //isSharepointGroup = info.isSharepointGroup;等待wrapper提供这个属性
            }
            return login;
        }
    }
}
