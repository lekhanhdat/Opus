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
using AvePoint.Wrapper.Common.Office;
using System.Collections;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveORemoteScopes : AveOScopesManager, IAveORemoteScopes
    {
        private AveServiceContext m_ServiceContext;
        private IAveRequest m_Request;

        public AveORemoteScopes(AveServiceContext serviceContext)
        {
            m_ServiceContext = serviceContext;
            m_Request = serviceContext.Request;
        }

        public override IAveOScopeDisplayGroupCollection AllDisplayGroups
        {
            get
            {
                if (base.m_DisplayGroups == null)
                {
                    GetDisplayGroupsForSite();
                }
                return base.m_DisplayGroups;
            }
        }

        public override IAveOScopeCollection AllScopes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllScopes"))
                {
                    GetDisplayGroupsForSite();
                }
                return base.DataCache.GetProperty<IAveOScopeCollection>("AllScopes");
            }
        }

        private IEnumerable GetDisplayGroupsForSite()
        {
            if (base.m_DisplayGroups != null)
            {
                return base.m_DisplayGroups;
            }
            List<Dictionary<string, object>> DisplayGroupsProp = m_Request.GetDisplayGroupsForSite();
            base.m_DisplayGroups = new AveOScopeDisplayGroupCollection(DisplayGroupsProp, m_Request);
            base.DataCache.PropertiesCache["AllScopes"] = base.m_DisplayGroups.GetAllScopes();
            return base.m_DisplayGroups;
        }

        public IAveOScope GetScope(Uri siteUrl, string name)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable GetDisplayGroupsForSite(Uri siteUrl)
        {
            return GetDisplayGroupsForSite();
            //if (base.m_DisplayGroups != null)
            //{
            //    return base.m_DisplayGroups;
            //}
            //List<Dictionary<string, object>> DisplayGroupsProp = m_Request.GetDisplayGroupsForSite();
            //base.m_DisplayGroups = new AveOScopeDisplayGroupCollection(DisplayGroupsProp, m_Request);
            //base.DataCache.PropertiesCache["AllScopes"] = base.m_DisplayGroups.GetAllScopes();
            //return base.m_DisplayGroups;
        }

        public override IEnumerable GetUnusedScopesForSite(Uri siteUrl)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable GetScopesForSite(Uri siteUrl)
        {
            return this.AllScopes;
        }

        public override IEnumerable GetSharedScopes()
        {
            throw new NotImplementedException();
        }
    }
}
