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
using System.Xml;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOSearchServiceApplicationProxy : AveIisWebServiceApplicationProxy, IAveOSearchServiceApplicationProxy
    {
        private IAveSite m_Site;

        public AveOSearchServiceApplicationProxy(IAveSite site)
        {
            m_Site = site;
        }

        public int AddScope(IAveOScopeInfo scopeInfo, out int statusCode)
        {
            throw new NotImplementedException();
        }

        public int AddConsumer(string consumerName)
        {
            throw new NotImplementedException();
        }

        public int AddDisplayGroup(IAveODisplayGroupInfo displayGroupInfo, out int statusCode)
        {
            throw new NotImplementedException();
        }

        public int AddRule(IAveORuleInfo ruleInfo, int scopeId)
        {
            throw new NotImplementedException();
        }

        public List<string> GetConsumers()
        {
            throw new NotImplementedException();
        }

        public List<int> GetDisplayGroupListInfo(int displayGroupId)
        {
            throw new NotImplementedException();
        }

        public IAveODisplayGroupInfo GetDisplayGroupInfo(int displayGroupId)
        {
            throw new NotImplementedException();
        }

        public int GetDisplayGroupIDFromName(string consumerName, string name)
        {
            throw new NotImplementedException();
        }

        public List<IAveODisplayGroupInfo> GetDisplayGroupsInfo()
        {
            throw new NotImplementedException();
        }

        public List<IAveORuleInfo> GetRulesInfo(int scopeId, out int statusCode)
        {
            throw new NotImplementedException();
        }

        public IAveOSearchServiceApplication GetProxy(Wrapper.Common.IAveServiceContext ServiceContext)
        {
            throw new NotImplementedException();
        }

        public IAveOScopeInfo GetScopeInfo(int scopeId)
        {
            throw new NotImplementedException();
        }

        public int GetScopeIDFromName(string consumerName, string name)
        {
            throw new NotImplementedException();
        }

        public List<IAveOScopeInfo> GetScopesInfo()
        {
            throw new NotImplementedException();
        }

        public void SetScopeInfo(IAveOScopeInfo scopeInfo)
        {
            throw new NotImplementedException();
        }

        public void SetDisplayGroupInfo(IAveODisplayGroupInfo displayGroupInfo)
        {
            throw new NotImplementedException();
        }

        public void SetDisplayGroupListInfo(int displayGroupId, List<int> scopeIds)
        {
            throw new NotImplementedException();
        }

        public IAveOFASTAdminProxy FASTAdminProxy
        {
            get { throw new NotImplementedException(); }
        }

        public bool CheckAssemblyQualifiedName(string name)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, Version> Versions
        {
            get { throw new NotImplementedException(); }
        }

        public bool NeedsUpgrade
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool NeedsUpgradeIncludeChildren
        {
            get { throw new NotImplementedException(); }
        }

        public AveTriState IsBackwardsCompatible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveConfigurationDatabase ConfigurationDatabase
        {
            get { throw new NotImplementedException(); }
        }

        public string DisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public IAveFarm Farm
        {
            get { throw new NotImplementedException(); }
        }

        public Guid ID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAvePersistedObject Parent
        {
            get { throw new NotImplementedException(); }
        }

        public AveObjectStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TypeName
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Hashtable Properties
        {
            get { throw new NotImplementedException(); }
        }

        public bool WasCreated
        {
            get { throw new NotImplementedException(); }
        }

        public long Version
        {
            get { throw new NotImplementedException(); }
        }

        public void Provision()
        {
            throw new NotImplementedException();
        }

        public void Unprovision()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Update(bool ensure)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }

        public IAveSite Site
        {
            get
            {
                return m_Site;
            }
        }

        public IAveOLocationConfiguration[] GetLocationConfigurations(out long lastUpdate, out bool useCrawlProxy)
        {
            throw new NotImplementedException();
        }


        public IAveOSearchServiceApplicationInfo GetSearchServiceApplicationInfo()
        {
            throw new NotImplementedException();
        }
    }
}
