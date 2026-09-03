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
    internal class AveFeatureImport
    {
        private AveSite m_Site;
        private AveWeb m_Web;
        private AveFeatureCollection m_FeatureCollection;
        private IAveRequest m_Request;
        private string m_FeatureSource;
        private Dictionary<Guid, List<AveFeatureLevel>> dependenciesCache = null;//因为源端o365取不出依赖关系，所以记录一些feature的依赖关系

        public AveFeatureImport(AveSite site, IAveRequest request)
        {
            m_Site = site;
            m_Web = site.RootWeb as AveWeb;
            m_FeatureCollection = site.Features as AveFeatureCollection;
            m_Request = request;
            m_FeatureSource = "site.features";
        }
        public AveFeatureImport(AveWeb web, IAveRequest request)
        {
            m_Web = web;
            m_FeatureCollection = web.Features as AveFeatureCollection;
            m_Request = request;
            m_FeatureSource = "web.features";
            dependenciesCache = new Dictionary<Guid, List<AveFeatureLevel>>();
            dependenciesCache.Add(new Guid("d44a1358-e800-47e8-8180-adf2d0f77543"), new List<AveFeatureLevel> { { new AveFeatureLevel("web.features", new Guid("7ad5272a-2694-4349-953e-ea5ef290e97c")) } });
            dependenciesCache.Add(AveSP2010FeatureDefinitions.PublishingWeb, new List<AveFeatureLevel> { { new AveFeatureLevel("site.features",  AveSP2010FeatureDefinitions.PublishingSite) } });  //SAAS-336
            dependenciesCache.Add(new Guid("e47705ec-268d-4c41-aa4e-0d8727985ebc"), new List<AveFeatureLevel> { { new AveFeatureLevel("web.features", new Guid("48a243cb-7b16-4b5a-b1b5-07b809b43f47")) } });  //SAAS-1935
        }

        public void Run(List<AveFeatureInfo> featureInfoList)
        {
            List<Dictionary<string, object>> featureInfoListProperties = new List<Dictionary<string, object>>();
            foreach (AveFeatureInfo featureInfo in featureInfoList)
            {
                //此处判断源端是否是O365。
                if (dependenciesCache != null && dependenciesCache.ContainsKey(featureInfo.Id))
                {
                    foreach (AveFeatureLevel featureLevel in dependenciesCache[featureInfo.Id])
                    {
                        Guid id = featureLevel.FeatureGuid;
                        string featureSource = featureLevel.FeatureSource;
                        if (!IsFeatureActive(id))
                        {
                            featureInfo.Dependencies.Add(id);
                            featureInfo.FeatureSource = featureSource;
                        }
                    }
                }
                Dictionary<string, object> featureInfoProperties = new Dictionary<string, object>();
                featureInfoProperties.Add("ID", featureInfo.Id);
                featureInfoProperties.Add("Dependences", featureInfo.Dependencies);
                featureInfoProperties.Add("FeatureSource", featureInfo.FeatureSource != null ? featureInfo.FeatureSource : m_FeatureSource);
                featureInfoListProperties.Add(featureInfoProperties);

                if (Guid.Equals(featureInfo.Id,AveSP2010FeatureDefinitions.PublishingWeb))
                {
                    //若目的端需开启publish Feature，且Minimal Download Strategy feature已经开启,则active publish feature后会deactive Minimal Download Strategy Feature
                    Guid miniFeatureId = new Guid("87294c72-f260-42f3-a41b-981a2ffce37a");
                    if (m_Web.Features[miniFeatureId] != null)
                    {
                        Dictionary<string, object> miniFeatureInfoProperties = new Dictionary<string, object>();
                        miniFeatureInfoProperties.Add("ID", miniFeatureId);
                        miniFeatureInfoProperties.Add("Dependences", new List<Guid>());
                        featureInfoListProperties.Add(miniFeatureInfoProperties);
                    }
                    //开启web publish feature会自动勾选Navigation的Show pages,在这里手动赋值，可以减少不必要的获取web信息的通信次数
                    if (!m_Web.AllProperties.ContainsKey("__GlobalNavigationIncludeTypes") && !m_Web.AllProperties.ContainsKey("__CurrentNavigationIncludeTypes"))
                    {
                        m_Web.AllProperties["__GlobalNavigationIncludeTypes"] = 2;
                        m_Web.AllProperties["__CurrentNavigationIncludeTypes"] = 2;
                    }
                }
            }
            //featureInfoListProperties.Sort(SortByDependecies);

            List<Dictionary<string, object>> featuresProperties = m_Request.RestoreFeatures(m_Web.ServerRelativeUrl, true, (int)AveFeatureDefinitionScope.Farm, m_FeatureSource, featureInfoListProperties);
            foreach (Dictionary<string, object> featureProperties in featuresProperties)
            {
                if (featureProperties != null && featureProperties.ContainsKey("DefinitionId") && featureProperties["DefinitionId"] != null)
                {
                    string id = featureProperties["DefinitionId"].ToString();
                    if (!string.IsNullOrEmpty(id) && new Guid(id) ==  AveSP2010FeatureDefinitions.PublishingSite && m_Site != null)
                    {
                        m_Site.DataCache.PropertiesCache["IsPublish"] = true;
                    }
                    else if (!string.IsNullOrEmpty(id) && new Guid(id) == AveSP2010FeatureDefinitions.PublishingWeb && m_Web != null)
                    {
                        m_Web.DataCache.PropertiesCache["IsPublish"] = true;
                    }
                    AveFeature feature = new AveFeature(m_Request, featureProperties);
                    m_FeatureCollection.ListData.Add(feature);
                }
            }
            //开启一些Feature会创建List, 这里需要清空List缓存
            // m_Web.DataCache.RemoveProperty("Lists"); reload web时会更新lists缓存，所以不需要在此处清除
        }

        private bool IsFeatureActive(Guid featureId)
        {
            if (m_Site != null && m_Site.Features[featureId] != null)
            {
                return true;
            }
            else if (m_Web.Features[featureId] != null)
            {
                return true;
            }
            else if (m_Web.Site.Features[featureId] != null)
            {
                return true;
            }
            return false;
        }

        private static int SortByDependecies(object objFront, object objForward)
        {
            Dictionary<string, object> dicFront = objFront as Dictionary<string, object>;
            Dictionary<string, object> dicForward = objForward as Dictionary<string, object>;
            return (dicFront["Dependences"] as List<Guid>).Count.CompareTo((dicForward["Dependences"] as List<Guid>).Count);
        }
    }
}
