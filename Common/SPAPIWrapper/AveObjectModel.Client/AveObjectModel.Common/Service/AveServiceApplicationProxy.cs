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
    class AveServiceApplicationProxy : AveClientObject, IAveServiceApplicationProxy
    {
        public AveServiceApplicationProxy()
        { }

        public AveServiceApplicationProxy( Dictionary<string, object> prop ) 
        {
            base.DataCache.AddChangedProperties(prop);
        }
        public bool CheckAssemblyQualifiedName(string name)
        {
            return AssemblyQualifiedName == null ? false : AssemblyQualifiedName.Equals(name, StringComparison.CurrentCultureIgnoreCase);
        }

        internal string AssemblyQualifiedName 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("AssemblyQualifiedName");
            }
        }

        //
        public Dictionary<Guid, Version> Versions
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<Guid, Version>>("Versions");
            }
        }

        public bool NeedsUpgrade
        {
            get
            {
                return base.DataCache.GetProperty<bool>("NeedsUpgrade");
            }
            set
            {
                base.DataCache.AddChangedProperty("NeedsUpgrade", value);
            }
        }

        public IAveConfigurationDatabase ConfigurationDatabase
        {
             get
            {
                return base.DataCache.GetProperty<IAveConfigurationDatabase>("ConfigurationDatabase");
            }
        }

        public string DisplayName
        {
             get
            {
                return base.DataCache.GetProperty<string>("DisplayName");
            }
        }

        public IAveFarm Farm
        {
             get
            {
                return base.DataCache.GetProperty<IAveFarm>("Farm");
            }
        }

        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("ID");
            }
            set
            {
                base.DataCache.AddChangedProperty("ID", value);
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }

        public IAvePersistedObject Parent
        {
             get
            {
                return base.DataCache.GetProperty<IAvePersistedObject>("Parent");
            }
        }

        public AveObjectStatus Status
        {
            get
            {
                return base.DataCache.GetProperty<AveObjectStatus>("Status");
            }
            set
            {
                base.DataCache.AddChangedProperty("Status", value);
            }
        }

        public string TypeName
        {
             get
            {
                return base.DataCache.GetProperty<string>("TypeName");
            }
        }

        public System.Collections.Hashtable Properties
        {
             get
            {
                return base.DataCache.GetProperty<System.Collections.Hashtable>("Properties");
            }
        }

        public bool WasCreated
        {
            get
            {
                return base.DataCache.GetProperty<bool>("WasCreated");
            }
        }

        public long Version
        {
            get
            {
                return base.DataCache.GetProperty<long>("Version");
            }
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

        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
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
    }
}
