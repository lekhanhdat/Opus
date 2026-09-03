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
using System.Xml;
using System.Globalization;

namespace AvePoint.ObjectModel.Common
{
    class AveFeatureDefinition : AveClientObject, IAveFeatureDefinition
    {
        private IAveRequest mRequest;
        private AveFeature mFeature;

        public AveFeatureDefinition(AveFeature feature, IAveRequest request, Dictionary<string, object> featureDefProperties)
        {
            mFeature = feature;
            mRequest = request;
            base.DataCache.AddPropertyies(featureDefProperties);
        }

        #region IAveFeatureDefinition Members

        public IAveFeatureDependencyCollection ActivationDependencies
        {
            get 
            {
                if (base.DataCache.IsPropertyNotLoaded("ActivationDependencies"))
                {
                    Dictionary<string, object> featureDependecyColProperties = base.DataCache.GetProperty<Dictionary<string, object>>("ActivationDependencies" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveFeatureDependencyCollection featureDependencyCol = new AveFeatureDependencyCollection(this, mRequest, featureDependecyColProperties);
                    base.DataCache.PropertiesCache["ActivationDependencies"] = featureDependencyCol;
                    return featureDependencyCol;
                }
                return base.DataCache.GetProperty<IAveFeatureDependencyCollection>("ActivationDependencies");
            }
        }

        public bool AutoActivateInCentralAdmin
        {
            get { throw new NotImplementedException(); }
        }

        public AveFeatureScope Scope
        {
            get 
            {
                string scope = base.DataCache.GetProperty<string>("Scope");
                if (scope == "Farm")
                {
                    return AveFeatureScope.Farm;
                }
                if ((scope == "WebApplication") || (scope == "WssWebApplication"))
                {
                    return AveFeatureScope.WebApplication;
                }
                if (scope == "Site")
                {
                    return AveFeatureScope.Site;
                }
                if (scope != "Web")
                {
                    throw new ArgumentException();
                }
                return AveFeatureScope.Web;
            }
        }

        public bool Hidden
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Hidden");          
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
                base.DataCache.AddChangedProperty("Name",value);
            }
        }

        public string ReceiverClass
        {
            //在server上实现，Client没有实现,返回默认值
            get { return default(string); }
        }

        public Guid FeatureId
        {
            get { throw new NotImplementedException(); }
        }

        public string GetDescription(System.Globalization.CultureInfo culture)
        {
            return base.DataCache.GetProperty<string>("Description");
        }

        public string GetTitle(System.Globalization.CultureInfo culture)
        {
            return this.Name;
        }

        #endregion

        #region IAvePersistedObject Members

        public IAveConfigurationDatabase ConfigurationDatabase
        {
            get { throw new NotImplementedException(); }
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
            get { throw new NotImplementedException(); }
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
            get 
            {
                return base.DataCache.GetProperty<string>("TypeName");    
            }
        }

        public System.Collections.Hashtable Properties
        {
            get { throw new NotImplementedException(); }
        }

        public bool WasCreated
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

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public IAveFeatureDefinitionContext SPFeatureDefinitionContext
        {
            get { throw new NotImplementedException(); }
        }

        #endregion


        public long Version
        {
            get { throw new NotImplementedException(); }
        }

        public void Update(bool ensure)
        {
            throw new NotImplementedException();
        }

        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }

        public bool AlwaysForceInstall
        {
            get { throw new NotImplementedException(); }
        }

        public string DefaultResourceFile
        {
            get { throw new NotImplementedException(); }
        }

        public Guid SolutionId
        {
            get { throw new NotImplementedException(); }
        }

        public string ReceiverAssembly
        {
            get { throw new NotImplementedException(); }
        }

        public string RootDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public bool SupportsLanguage(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public XmlNode GetXmlDefinition(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public IAveElementDefinitionCollection GetElementDefinitions(CultureInfo ciElements)
        {
            throw new NotImplementedException();
        }

        Version IAveFeatureDefinition.Version
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasElementType(Type typeElementOfInterest)
        {
            throw new NotImplementedException();
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

        #region add for SP2013
        public int CompatibilityLevel
        {
            get { return base.DataCache.GetProperty<int>("CompatibilityLevel"); }
        }
        #endregion
    }
}
