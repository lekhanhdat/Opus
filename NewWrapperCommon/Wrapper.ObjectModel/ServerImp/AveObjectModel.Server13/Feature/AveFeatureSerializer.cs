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

namespace AvePoint.ObjectModel.Server13
{
    internal class AveFeatureSerializer : IAveFeatureSerializer
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private AveFeatureScope m_scope;
        private AveFeatureImport m_importManager = null;

        public AveFeatureSerializer(AveSite site)
        {
            mSite = site;
            m_scope = AveFeatureScope.Site;
            m_importManager = new AveFeatureImport(site);
        }

        public AveFeatureSerializer(AveWeb web)
        {
            mWeb = web;
            m_scope = AveFeatureScope.Web;
            m_importManager = new AveFeatureImport(web);
        }

        [WrapperOptimization(true)]
        private AveFeatureInfoBox GetSiteFeatures()
        {
            //we should backup feature by api to backup feature dependencies
            return GetSiteFeaturesProxy();
        }
        private AveFeatureInfoBox GetSiteFeaturesProxy()
        {
            return GetFeaturesInternal(mSite.Features as AveFeatureCollection, AveFeatureScope.Site);
        }

        private AveFeatureInfoBox GetFeaturesInternal(AveFeatureCollection features, AveFeatureScope scope)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureSerializer.GetFeaturesInternal"))
            {

                AveFeatureInfoBox featureBox = new AveFeatureInfoBox();

                foreach (AveFeature feature in features)
                {
                    AveFeatureInfo info = new AveFeatureInfo(feature.DefinitionId, scope);
                    info.FeatureDefinitionId = feature.DefinitionId;
                    info.CompatibilityLevel = feature.Definition == null ? 15 : feature.Definition.CompatibilityLevel;
                    if (feature.Definition != null && feature.Definition.ActivationDependencies != null)
                    {
                        foreach (AveFeatureDependency depFeature in feature.Definition.ActivationDependencies)
                        {
                            info.Dependencies.Add(depFeature.FeatureId);
                        }
                    }
                    featureBox.FeatureList.Add(info);
                }
                featureBox.FeatureList.Sort();

                return featureBox;

            }

        }

        [WrapperOptimization(true)]
        private AveFeatureInfoBox GetWebFeatures()
        {
            //we should backup feature by api to backup feature dependencies
            return GetWebFeaturesProxy();
        }
        private AveFeatureInfoBox GetWebFeaturesProxy()
        {
            return GetFeaturesInternal(mWeb.Features as AveFeatureCollection, AveFeatureScope.Web);
        }

        #region IAveSerializationSurrogate<AveFeatureInfoBox,object,List<AveFeatureInfo>> Members

        public AveFeatureInfoBox GetObjectData()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureSerializer.GetObjectData"))
            {

                AveFeatureInfoBox featureBox = new AveFeatureInfoBox();

                if (m_scope == AveFeatureScope.Site)
                {
                    featureBox = GetSiteFeatures();
                    featureBox.Scope = AveFeatureScope.Site;
                }
                else if (m_scope == AveFeatureScope.Web)
                {
                    featureBox = GetWebFeatures();
                    featureBox.Scope = AveFeatureScope.Web;
                }

                return featureBox;

            }

        }

        public object SetObjectData(List<AveFeatureInfo> featureInfoList)
        {
            if (featureInfoList == null)
            {
                return null;
            }

            m_importManager.Run(featureInfoList);

            return null;
        }

        #endregion
    }
}
