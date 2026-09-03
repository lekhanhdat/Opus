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

namespace AvePoint.ObjectModel.Common
{
    class AveServiceApplicationProxyGroup :AveClientObject, IAveServiceApplicationProxyGroup
    {

        public AveServiceApplicationProxyGroup(Dictionary<string,object>sAppProxyGroupProp) 
        {
            base.DataCache.AddPropertyies(sAppProxyGroupProp);
        }

        #region IAveServiceApplicationProxyGroup Members

        public IEnumerable<IAveServiceApplicationProxy> Proxies
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IEnumerable<IAveServiceApplicationProxy> DefaultProxies
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IAveServiceApplicationProxyGroup Default
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsCustom
        {
            get { throw new NotImplementedException(); }
        }

        public void Add(IAveServiceApplicationProxy serviceApplicationProxy)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public IAveServiceApplicationProxyGroup GetOrCreate(IAveFarm farm, string proxyGroupName, bool custom)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAvePersistedUpgradableObject Members

        public Dictionary<Guid, Version> Versions
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IAvePersistedObject Members

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

        public void Update(bool ensure)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAveAutoSerializingObject Members

        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IAveServiceApplicationProxyGroup Members


        public bool ContainsType(Type serviceApplicationProxyType)
        {
            throw new NotImplementedException();
        }

        #endregion

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

        public void Uncache()
        {
            throw new NotImplementedException();
        }

        public IAveLastUpdateInfo LastUpdateInfo
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


        public bool IsDefault(IAveServiceApplicationProxy proxy)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultProxy(IAveServiceApplicationProxy serviceApplicationProxy)
        {
            throw new NotImplementedException();
        }
    }
}
