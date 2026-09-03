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



using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    internal class AveFeatureImport
    {
        private AveLogger m_Logger = AveLogger.GetInstance(typeof(AveFeatureImport));
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
            dependenciesCache.Add(new Guid("d44a1358-e800-47e8-8180-adf2d0f77543"), new List<AveFeatureLevel> { { new AveFeatureLevel("web.features",new Guid("7ad5272a-2694-4349-953e-ea5ef290e97c")) } });
            dependenciesCache.Add(new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb"), new List<AveFeatureLevel> { { new AveFeatureLevel("site.features",new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa")) } });  //SAAS-336
            dependenciesCache.Add(new Guid("e47705ec-268d-4c41-aa4e-0d8727985ebc"), new List<AveFeatureLevel> { { new AveFeatureLevel("web.features", new Guid("48a243cb-7b16-4b5a-b1b5-07b809b43f47")) } });  //SAAS-1935
        }

        public void Run(List<AveFeatureInfo> featureInfoList)
        {
            if (featureInfoList.Count > 0)
            {
                List<Dictionary<string, object>> featureInfoListProperties = new List<Dictionary<string, object>>();            
                featureInfoList = SortFeaturesByDependecies(featureInfoList);
                foreach (AveFeatureInfo featureInfo in featureInfoList)
                {
                    //此处判断源端是否是O365。
                    if (featureInfo.Dependencies.Count == 0 && dependenciesCache != null && dependenciesCache.ContainsKey(featureInfo.Id))
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
                    //目前在add PWA setting这个feature时会出错，需要过滤掉。
                    if (featureInfo.Id.ToString().Equals("697c64b9-3dff-4981-9394-0a62632120ec", StringComparison.OrdinalIgnoreCase))
                    {
                        m_Logger.Warn("The PWA setting feature can not be added,feature ID:{0}", featureInfo.Id.ToString());
                        continue;
                    }
					
					//SAAS-36938, 此ID为Onedrive feature id,不可加入到非onedrive模板(SPSPERS)的站点中。
                    if (!String.IsNullOrWhiteSpace(m_Web.Template) && !m_Web.Template.Contains("SPSPERS") && featureInfo.Id.ToString().Equals("41baa678-ad62-41ef-87e6-62c8917fc0ad", StringComparison.OrdinalIgnoreCase))
                    {
                        m_Logger.Warn("The Onedrive feature can not be added to current web,current web template:{0}, feature ID:{1}", m_Web.Template, featureInfo.Id.ToString());
                        continue;
                    }

                    var rootWeb = m_Site is null ? m_Web.Site.RootWeb : m_Site.RootWeb;
                    if (AveSPWebTemplate.IsCommunicationSite($"{rootWeb.WebTemplate}#{rootWeb.Configuration}")
                        && (featureInfo.Id == new Guid(AveFeatureConstants.PUBLISHING_FEATURE_ID) || featureInfo.Dependencies.Contains(new Guid(AveFeatureConstants.PUBLISHING_FEATURE_ID))))
                    {
                        m_Logger.Warn($"Skip restoring Publishing feature for Communication site {rootWeb.Url}, feature id: {featureInfo.Id}");
                        continue;
                    }

                    Dictionary<string, object> featureInfoProperties = new Dictionary<string, object>();
                    featureInfoProperties.Add("ID", featureInfo.Id);
                    featureInfoProperties.Add("Dependences", featureInfo.Dependencies);
                    featureInfoProperties.Add("FeatureSource", featureInfo.FeatureSource != null ? featureInfo.FeatureSource : m_FeatureSource);
                    featureInfoListProperties.Add(featureInfoProperties);

                    if (featureInfo.Id.ToString().Equals("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb"))
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

                List<Dictionary<string, object>> featuresProperties = m_Request.RestoreFeatures(m_Web == null ? null : m_Web.ServerRelativeUrl, true, (int)AveFeatureDefinitionScope.Farm, m_FeatureSource, featureInfoListProperties);
                foreach (Dictionary<string, object> featureProperties in featuresProperties)
                {
                    if (featureProperties != null && featureProperties.ContainsKey("DefinitionId") && featureProperties["DefinitionId"] != null)
                    {
                        string id = featureProperties["DefinitionId"].ToString();
                        if (!string.IsNullOrEmpty(id) && new Guid(id) == new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa") && m_Site != null)
                        {
                            m_Site.DataCache.AddProperty("IsPublish",true);
                            //[SAAS-9184]当开启Publishing Feature的时候，SharePoint Designer Settings中的选项会全部置为true，此时更新Cache中的数据。
                            m_Site.DataCache.AddProperty("AllowDesigner", true);
                            m_Site.DataCache.AddProperty("AllowMasterPageEditing", true);
                            m_Site.DataCache.AddProperty("AllowRevertFromTemplate", true);
                            m_Site.DataCache.AddProperty("ShowUrlStructure", true);
                        }
                        else if (!string.IsNullOrEmpty(id) && new Guid(id) == new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb") && m_Web != null)
                        {
                            m_Web.DataCache.AddProperty("IsPublish", true);
                        }
                        AveFeature feature = new AveFeature(m_Request, featureProperties);
                        m_FeatureCollection.ListData.Add(feature);
                    }
                }
            }
        }

        private bool IsFeatureActive(Guid featureId)
        {
            if (m_Site != null && m_Site.Features[featureId] != null)
            {
                return true;
            }
            if (m_Web != null && m_Web.Features[featureId] != null)
            {
                return true;
            }
            if (m_Web != null && m_Web.Site.Features[featureId] != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// sort features by dependecies, the predecessor must be activiated first
        /// </summary>
        /// <param name="featureInfoList"></param>
        /// <returns></returns>
        private List<AveFeatureInfo> SortFeaturesByDependecies(List<AveFeatureInfo> featureInfoList)
        {
            List<AveFeatureInfo> features = new List<AveFeatureInfo>();

            featureInfoList.Sort((feature1, feature2) => feature1.Dependencies.Count.CompareTo(feature2.Dependencies.Count));

            int totalCount = featureInfoList.Count;
            int index = 0;
            do
            {
                if (featureInfoList[index].Dependencies.Count == 0)
                {
                    features.Add(featureInfoList[index]);
                    featureInfoList.RemoveAt(index);
                    if (featureInfoList.Count == 0)
                    {
                        break;
                    }
                    index %= featureInfoList.Count;
                }
                else
                {
                    bool flag = true;
                    foreach (Guid featureId in featureInfoList[index].Dependencies)
                    {
                        if (featureInfoList.Find((featureInfo) => featureInfo.Id == featureId) == null)
                        {
                            continue;
                        }
                        if (features.Find((feature)=>feature.Id== featureId) == null)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        features.Add(featureInfoList[index]);
                        featureInfoList.RemoveAt(index);
                        if (featureInfoList.Count == 0)
                        {
                            break;
                        }
                        index %= featureInfoList.Count;
                    }
                    else
                    {
                        index++;
                        index %= featureInfoList.Count;
                    }
                }                
            }
            while (features.Count < totalCount);

            return features;
        }        


    }

}
