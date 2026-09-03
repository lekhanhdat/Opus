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
using System.Globalization;
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using System.Xml;

namespace AvePoint.ObjectModel.Server13
{
    class AveFeatureDefinition : AvePersistedObject, IAveFeatureDefinition
    {
        private SPFeatureDefinition mFeatureDefinition;
        private AveFeatureDependencyCollection mActivationDependencies;

        public AveFeatureDefinition(SPFeatureDefinition featureDefinition)
            : base(featureDefinition)
        {
            mFeatureDefinition = featureDefinition;
        }

        public AveFeatureDefinition()
            : this(new SPFeatureDefinition())
        { }

        internal SPFeatureDefinition FeatureDefinition
        {
            get
            {
                return mFeatureDefinition;
            }
        }

        #region IAveFeatureDefinition Members

        public IAveFeatureDependencyCollection ActivationDependencies
        {
            get
            {
                if (mActivationDependencies == null)
                {
                    mActivationDependencies = new AveFeatureDependencyCollection(mFeatureDefinition.ActivationDependencies);
                }
                return mActivationDependencies;
            }
        }

        public AveFeatureScope Scope
        {
            get
            {
                return (AveFeatureScope)mFeatureDefinition.Scope;
            }
        }

        public bool Hidden
        {
            get { return mFeatureDefinition.Hidden; }
        }

        public new Guid ID
        {
            get
            {
                return mFeatureDefinition.Id;
            }
            set
            {
                base.ID = value;
            }
        }

        public new string Name
        {
            get
            {
                return mFeatureDefinition.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        public new Version Version
        {
            get
            {
                return mFeatureDefinition.Version;
            }
        }

        public IAveFeatureDefinitionContext SPFeatureDefinitionContext
        {
            get
            {
                 object spFeatureDefinitionContext=AveAssemblyUtility.GetPropertyValue(mFeatureDefinition, "SPFeatureDefinitionContext");
                 if (spFeatureDefinitionContext != null)
                {
                    return new AveFeatureDefinitionContext(spFeatureDefinitionContext);
                }
                return null;
            }
        }

        public string GetDescription(CultureInfo culture)
        {
            return mFeatureDefinition.GetDescription(culture);
        }

        public string GetTitle(CultureInfo culture)
        {
            return mFeatureDefinition.GetTitle(culture);
        }

        public bool AutoActivateInCentralAdmin
        {
            get { return mFeatureDefinition.AutoActivateInCentralAdmin; }
        }

        public bool AlwaysForceInstall
        {
            get { return mFeatureDefinition.AlwaysForceInstall; }
        }

        public string DefaultResourceFile
        {
            get { return mFeatureDefinition.DefaultResourceFile; }
        }

        public Guid SolutionId
        {
            get { return mFeatureDefinition.SolutionId; }
        }

        public string ReceiverAssembly
        {
            get { return mFeatureDefinition.ReceiverAssembly; }
        }

        public Guid FeatureId
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetPropertyValue(mFeatureDefinition, "FeatureId");
            }
        }

        public string RootDirectory
        {
            get { return mFeatureDefinition.RootDirectory; }
        }

        public XmlNode GetXmlDefinition(CultureInfo culture)
        {
            return mFeatureDefinition.GetXmlDefinition(culture);
        }

        public IAveElementDefinitionCollection GetElementDefinitions(CultureInfo ciElements)
        {
            return new AveElementDefinitionCollection(mFeatureDefinition.GetElementDefinitions(ciElements));
        }

        public bool HasElementType(Type typeElementOfInterest)
        {
            string typeMapping = string.Empty;
            typeMapping = XmlConfiguration.GetTypeMapping(typeElementOfInterest.Name);
            Type typeElement = AveAssemblyUtility.GetGenerticType(typeElementOfInterest, typeMapping);

            return (bool)AveAssemblyUtility.InvokeMethod(mFeatureDefinition, "HasElementType", new Type[] { typeof(Type) }, new object[] { typeElement });
        }

        public bool SupportsLanguage(CultureInfo culture)
        {
            return mFeatureDefinition.SupportsLanguage(culture);
        }

        public string ReceiverClass
        {
            get { return mFeatureDefinition.ReceiverClass; }
        }

        #endregion

        #region add for SP2013
        public int CompatibilityLevel
        {
            get { return mFeatureDefinition.CompatibilityLevel; }
        }
        #endregion
    }
}
