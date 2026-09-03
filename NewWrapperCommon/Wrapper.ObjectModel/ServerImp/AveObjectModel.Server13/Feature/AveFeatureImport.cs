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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.GCommon.Utility.I18N;
//using Microsoft.SharePoint;
//using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.I18N;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server13
{
    internal class AveFeatureImport
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveFeatureImport));
        private AveSite mSite;
        private AveWeb mWeb;
        private IAveFeatureDefinitionCollection mFeatureDefinitions;

        public AveFeatureImport(AveSite site)
        {
            mSite = site;
        }
        public AveFeatureImport(AveWeb web)
        {
            mWeb = web;
            mSite = mWeb.Site as AveSite;
        }

        public void Run(List<AveFeatureInfo> data)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureImport.Run"))
            {

                Dictionary<Guid, AveFeatureInfo> siteFeatures = new Dictionary<Guid, AveFeatureInfo>();
                Dictionary<Guid, AveFeatureInfo> webFeatures = new Dictionary<Guid, AveFeatureInfo>();
                foreach (AveFeatureInfo featureInfo in data)
                {
                    if (featureInfo.Scope == AveFeatureScope.Site)
                    {
                        siteFeatures.Add(featureInfo.Id, featureInfo);
                    }
                    else if (featureInfo.Scope == AveFeatureScope.Web)
                    {
                        webFeatures.Add(featureInfo.Id, featureInfo);
                    }
                }
                foreach (AveFeatureInfo featureInfo in data)
                {
                    if (featureInfo.Scope == AveFeatureScope.Site)
                    {
                        ActivateSiteFeature(featureInfo, siteFeatures);
                    }
                    else if (featureInfo.Scope == AveFeatureScope.Web)
                    {
                        ActivateWebFeature(featureInfo, webFeatures);
                    }
                }

            }

        }

        private bool HaveDependencies(Guid featureId, Dictionary<Guid, AveFeatureInfo> features, out AveFeatureInfo feature)
        {
            features.TryGetValue(featureId, out feature);
            return feature != null && feature.Dependencies.Count > 0;
        }

        /// <summary>
        /// 激活依赖的Feature
        /// </summary>
        /// <param name="featureId">依赖的Feature Id</param>
        /// <param name="features">所有备份数据里的Feature Info</param>
        /// <param name="isNeedReload">是否需要Reload</param>
        private void ActivateDependcies(Guid featureId, Dictionary<Guid, AveFeatureInfo> features, ref bool isNeedReload)
        {
            AveFeatureInfo feature;
            //如果这个依赖是备份数据中的一员，递归还原这个依赖
            if (HaveDependencies(featureId, features, out feature))
            {
                if (feature.Scope == AveFeatureScope.Site)
                {
                    ActivateSiteFeature(feature, features);
                }
                else if (feature.Scope == AveFeatureScope.Web)
                {
                    ActivateWebFeature(feature, features);
                }
            }
            else
            {
                //直接激活这个依赖
                isNeedReload = ActivateDependenceFeature(featureId);
            }
        }

        /// <summary>
        /// 还原Site Feature
        /// </summary>
        /// <param name="info">要还原的Feature Info</param>
        /// <param name="siteFeatures">所有备份的Site Feature Info</param>
        private void ActivateSiteFeature(AveFeatureInfo info, Dictionary<Guid, AveFeatureInfo> siteFeatures)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureImport.ActivateSiteFeature"))
            {

                //先还原Feature的所有依赖
                foreach (Guid featureId in info.Dependencies)
                {
                    try
                    {
                        bool isNeedReload = false;
                        ActivateDependcies(featureId, siteFeatures, ref isNeedReload);
                    }
                    catch (Exception e)
                    {
                        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(featureId, e));
                    }
                }
                //--------------这里是否需要添加Reload------------------
                //如果Feature没有激活，就激活Feature
                if (mSite.Features[info.Id] == null)
                {
                    var isSiteFeature = this.IsSiteFeature(info.Id);
                    ActivateFeature(mSite.Features as AveFeatureCollection, info.Id, isSiteFeature);
                }

            }

        }

        /// <summary>
        /// 还原Web Feature
        /// </summary>
        /// <param name="info">要还原的Feature Info</param>
        /// <param name="webFeatures">所有备份的Web Feature Info</param>
        private void ActivateWebFeature(AveFeatureInfo info, Dictionary<Guid, AveFeatureInfo> webFeatures)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureImport.ActivateWebFeature"))
            {

                foreach (Guid featureId in info.Dependencies)
                {
                    try
                    {
                        bool isNeedReload = false;
                        ActivateDependcies(featureId, webFeatures, ref isNeedReload);
                        //现在要还原的是Publishing Web，发现Publishing Site没有激活，激活Publishing Site以后要Reload Site&Web
                        if (isNeedReload && featureId.Equals(AveSP2013FeatureDefinitions.PublishingSite))
                        {
                            //We need to update and reload site/web each time after active the dependency feature. ADO-59063
                            mSite.Update();
                            mSite.ReloadSite();
                            mWeb.Update();
                            mWeb.ReloadWeb();
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(featureId, e));
                    }
                }
                //--------------这里是否需要添加Reload------------------
                if (mWeb.Features[info.Id] == null)
                {
                    /*
                     *For office365 to local subsit level job，can't active publishing feature in destination. 
                     */
                    if (info.Id.Equals(AveSP2013FeatureDefinitions.PublishingWeb) && mSite.Features[AveSP2013FeatureDefinitions.PublishingSite] == null)
                    {
                        ActivateDependenceFeature(AveSP2013FeatureDefinitions.PublishingSite);
                    }
                    var isSiteFeature = this.IsSiteFeature(info.Id);
                    ActivateFeature(mWeb.Features as AveFeatureCollection, info.Id, isSiteFeature);
                }

            }

        }

        //激活依赖之前检查Feature Definition是否存在
        private bool ActivateDependenceFeature(Guid featureId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureImport.ActivateDependenceFeature"))
            {

                if (this.mFeatureDefinitions == null)
                {
                    this.mFeatureDefinitions = mSite.WebApplication.Farm.Local.FeatureDefinitions;
                }
                var featureDefinition = this.mFeatureDefinitions[featureId];
                if (featureDefinition == null)
                {
                    //The Farm Didn't install the Feature
                    throw new FeatureNotFoundException(featureId, AveFeatureDefinitionScope.Farm.ToString(), string.Empty);
                }
                return ActivateFeature(GetFeatureCollection(featureDefinition.Scope), featureId);

            }

        }

        /// <summary>
        /// 获取某一级别的Feature Collection
        /// </summary>
        /// <param name="featureScope"></param>
        /// <returns></returns>
        private AveFeatureCollection GetFeatureCollection(AveFeatureScope featureScope)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureImport.GetFeatureCollection"))
            {

                switch (featureScope)
                {
                    case AveFeatureScope.WebApplication:
                        return mSite.WebApplication.Features as AveFeatureCollection;
                    case AveFeatureScope.Site:
                        return mSite.Features as AveFeatureCollection;
                    case AveFeatureScope.Web:
                        return mWeb.Features as AveFeatureCollection;
                    case AveFeatureScope.Farm://Unknown, need more research.
                    case AveFeatureScope.ScopeInvalid:
                    default:
                        throw new Exception(string.Format(ServerAPIResource.FeatureScopeNotExist, featureScope.ToString()));
                }

            }

        }

        /// <summary>
        /// 真正的激活一个Feature
        /// </summary>
        /// <param name="features"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        private bool ActivateFeature(AveFeatureCollection features, Guid featureId, bool isSiteFeature = false)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureImport.ActivateFeature"))
            {

                try
                {
                    if (!isSiteFeature)
                    {
                        features.Add(featureId, true);
                    }
                    else
                    {
                        features.Add(featureId, true, AveFeatureDefinitionScope.Site);
                    }
                    //to-do 这个属性是由 publishing feature控制的，可以去掉
                    // ToString("D"), D: 统一id ToString的格式。
                    // 这个不能去掉，O365到13subsite的publishing feature这个property必须手动set。
                    if (featureId.ToString("D").Equals(AveSP2013FeatureDefinitions.PublishingWeb.ToString("D")))
                    {
                        mWeb.AllProperties["__PublishingFeatureActivated"] = "True";
                        mWeb.Update();
                    }
                    return true;
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(featureId, e));
                    return false;
                }

            }

        }

        private bool IsSiteFeature(Guid featureId)
        {
            IAveFeatureDefinition definition = null;
            try
            {
                definition = mSite.FeatureDefinitions[featureId];
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while getting the feature definition in site collection.Site Url: {0}, Feature Id: {1}, Exception:{2}", mSite.Url, featureId, e);
            }
            return definition != null ? true : false;
        }
    }
}
