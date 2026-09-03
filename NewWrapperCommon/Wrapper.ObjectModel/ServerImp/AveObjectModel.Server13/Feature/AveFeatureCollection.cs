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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System.Collections.Generic;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.I18N;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server13
{
    class AveFeatureCollection : AveAbstractCommonCollection<IAveFeature>, IAveFeatureCollection
    {
        private static IAveResource resource = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, null, AveContextKind.Server13ObjectModel).CreateResource();
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SPFeatureCollection mFeatures;
        private AveSite mSite;
        private AveWeb mWeb;

        public AveFeatureCollection(SPFeatureCollection features)
            : base(features)
        {
            mFeatures = features;
        }

        public AveFeatureCollection(SPFeatureCollection features, AveSite site)
            : base(features)
        {
            mFeatures = features;
            mSite = site;
        }

        public AveFeatureCollection(SPFeatureCollection features, AveWeb web)
            : base(features)
        {
            mFeatures = features;
            mWeb = web;
        }

        #region IAveFeatureCollection Members

        public IAveFeature this[Guid featureId]
        {
            get
            {
                return GetById(featureId);
            }
        }

        public IAveFeature Add(Guid featureId, bool force)
        {
            try
            {
                if (CheckAndAddSpecialFeatureByClient(featureId))
                {
                    return this[featureId];
                }
                return new AveFeature(mFeatures.Add(featureId, force));
            }
            catch (Exception e)
            {
                log.Warn("Add the feature failed, Id: {0}, error message: {1}.", featureId.ToString(), e);
                throw;
            }
        }

        public IAveFeature Add(Guid featureId, bool force, AveFeatureDefinitionScope featdefScope)
        {         
            try
            {
                if (CheckAndAddSpecialFeatureByClient(featureId))
                {
                    return this[featureId];
                }
                return new AveFeature(mFeatures.Add(featureId, force, (SPFeatureDefinitionScope)featdefScope));
            }
            catch (Exception e)
            {
                log.Warn("Add the feature failed, Id: {0}, error message: {1}.", featureId.ToString(), e);              
                throw;
            }
        }

        public IAveFeature GetById(Guid featureId)
        {
            SPFeature feature = mFeatures[featureId];
            if (feature == null)
            {
                return null;
            }
            return new AveFeature(feature);
        }

        public void Remove(Guid featureId, bool force)
        {
            mFeatures.Remove(featureId, force);
        }

        public void Remove(Guid featureId)
        {
            mFeatures.Remove(featureId);
        }

        public IAveFeature Add(Guid featureId)
        {
            try
            {
                if (CheckAndAddSpecialFeatureByClient(featureId))
                {
                    return this[featureId];
                }
                return new AveFeature(mFeatures.Add(featureId));
            }
            catch (Exception e)
            {
                log.Warn("Add the feature failed, Id: {0} ,error message: {1}.", featureId.ToString(), e);
                throw;
            }
           
        }

        //public AveFeatureInfoBox Export(AveFeatureScope scope)
        //{
        //    AveFeatureInfoBox featureBox = new AveFeatureInfoBox();

        //    if (scope == AveFeatureScope.Site)
        //    {
        //        featureBox = mSite.DBService.GetFeatures(mSite.Site, scope);
        //    }
        //    else if (scope == AveFeatureScope.Web)
        //    {
        //        foreach (SPFeature feature in mFeatures)
        //        {
        //            AveFeatureInfo info = new AveFeatureInfo();
        //            info.Id = feature.DefinitionId;
        //            info.Scope = scope;

        //            //if (feature.Definition != null && feature.Definition.ActivationDependencies != null)
        //            //{
        //            //    foreach (SPFeatureDependency depFeature in feature.Definition.ActivationDependencies)
        //            //    {
        //            //        info.Dependencies.Add(depFeature.FeatureId);
        //            //    }
        //            //}

        //            featureBox.FeatureList.Add(info);
        //        }
        //        featureBox.FeatureList.Sort();
        //    }

        //    return featureBox;
        //}

        #endregion

        protected override object CreatElementInstance(object t)
        {
            return new AveFeature(t as SPFeature);
        }

        public override int Count
        {
            get { return mFeatures.Count; }
        }

        public void RestoreFeatures(List<AveFeatureInfo> featureInfoList)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFeatureCollection.RestoreFeatures"))
            {

                if (featureInfoList.Count > 0)
                {
                    foreach (AveFeatureInfo featureInfo in featureInfoList)
                    {
                        foreach (Guid featureId in featureInfo.Dependencies)
                        {
                            try
                            {
                                switch (featureInfo.Scope)
                                {
                                    case AveFeatureScope.WebApplication:
                                    case AveFeatureScope.Site:
                                        Add(featureId, true);
                                        break;
                                    case AveFeatureScope.Web:
                                        try
                                        {
                                            Add(featureId, true);
                                        }
                                        catch (InvalidOperationException)
                                        {
                                            mWeb.Site.Features.Add(featureId, true);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(featureId, e));
                            }
                        }
                        try
                        {
                            Add(featureInfo.Id, true);
                        }
                        catch (Exception e)
                        {
                            log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(featureInfo.Id, e));
                        }
                    }
                }

            }

        }

        public bool CheckAndAddSpecialFeatureByClient(Guid featureId)
        {
            try
            {
                if (!WrapperConfiguration.ActivateFeatureIdsByClient.Contains(featureId))
                {
                    return false;
                }
                var scopeParentUrl = string.Empty;
                var scope = AveFeatureScope.Site;
                if (mWeb != null)
                {
                    scopeParentUrl = mWeb.Url;
                    scope = AveFeatureScope.Web;
                    log.Debug("Activate feature by client API, id: {0}, web url:  {1}.", featureId.ToString(), scopeParentUrl);
                }
                else if (mSite != null)
                {
                    scopeParentUrl = mSite.Url;
                    scope = AveFeatureScope.Site;
                    log.Debug("Activate feature by client API, id: {0}, site url: {1}.", featureId.ToString(), scopeParentUrl);
                }
                else
                {
                    return false;
                }

                if (string.IsNullOrEmpty(scopeParentUrl))
                {
                    log.Warn("The web url of activating feature is empty.");
                    return false;
                }
                using (Microsoft.SharePoint.Client.ClientContext context = new Microsoft.SharePoint.Client.ClientContext(scopeParentUrl))
                {
                    context.AuthenticationMode = Microsoft.SharePoint.Client.ClientAuthenticationMode.Default;

                    //context.Load(context.Web.Features);
                    //context.Load(context.Site.Features);

                    //context.ExecuteQuery();

                    if (scope == AveFeatureScope.Site)
                    {
                        var feature = context.Site.Features.GetById(featureId);
                        context.Load(feature);
                        context.ExecuteQuery();
                        if (feature.ServerObjectIsNull.HasValue && feature.ServerObjectIsNull.Value)
                        {
                            context.Site.Features.Add(featureId, true, Microsoft.SharePoint.Client.FeatureDefinitionScope.None);
                            //context.Load(siteFeature);
                        }

                    }
                    else if (scope == AveFeatureScope.Web)
                    {
                        var feature = context.Web.Features.GetById(featureId);
                        context.Load(feature);
                        context.ExecuteQuery();
                        if (feature.ServerObjectIsNull.HasValue && feature.ServerObjectIsNull.Value)
                        {
                            context.Web.Features.Add(featureId, true, Microsoft.SharePoint.Client.FeatureDefinitionScope.None);
                            //context.Load(feature);
                        }
                    }
                    context.ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                log.Warn("Active feature failed by Client API, Feature Id: {0}, error message: {1}.", featureId, ex);
                return false;
            }
            finally
            {
                if (mWeb != null)
                {
                    mWeb.ReloadWeb();
                    mFeatures = mWeb.Web.Features;
                }
                else if (mSite != null)
                {
                    mSite.ReloadSite();
                    mFeatures = mSite.Site.Features;
                }
            }
            return true;
        }
    }
}
