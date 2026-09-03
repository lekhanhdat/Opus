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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPFeature : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveObjectFeature mObj;

        private IReport report = new AveWrapperReport();

        public AveSPFeature(object obj)
        {
            mObj = AveObjectFeature.CreateInstance(obj);
        }

        public void Restore(AveFeatureInfoBox featureInfoBox)
        {
            if (featureInfoBox != null)
            {
                try
                {
                    if (featureInfoBox.FeatureList != null && featureInfoBox.FeatureList.Count > 0)
                    {
                        mObj.RestoreFeatures(featureInfoBox.FeatureList);
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    if (featureInfoBox.Scope == AveFeatureScope.Site)
                    {
                        mLog.Warn("An error occurred while restore site feature. ", ex);
                        report.AddDetail(new AveWrapperReportDto("SiteFeature", "SiteFeature", AveReportObjectType.SiteFeature, AveStatus.Skipped, "You don't have permission to restore site feature. " + ex.Message));
                    }
                    if (featureInfoBox.Scope == AveFeatureScope.Web)
                    {
                        mLog.Warn("An error occurred while restore web feature. ", ex);
                        report.AddDetail(new AveWrapperReportDto("WebFeature", "WebFeature", AveReportObjectType.WebFeature, AveStatus.Skipped, "You don't have permission to restore web feature. " + ex.Message));
                    }
                }
                catch (Exception e)
                {
                    if (featureInfoBox.Scope == AveFeatureScope.Site)
                    {
                        mLog.Warn("An error occurred while restore site feature. ", e);
                        report.AddDetail(new AveWrapperReportDto("SiteFeature", "SiteFeature", AveReportObjectType.SiteFeature, AveStatus.Failed, "You don't have permission to restore site feature. " + e.Message));
                        throw e;
                    }
                    if (featureInfoBox.Scope == AveFeatureScope.Web)
                    {
                        mLog.Warn("An error occurred while restore web feature. ", e);
                        report.AddDetail(new AveWrapperReportDto("WebFeature", "WebFeature", AveReportObjectType.WebFeature, AveStatus.Failed, "You don't have permission to restore web feature. " + e.Message));
                        throw e;
                    }
                }
            }
        }

        public IReport GetReport()
        {
            return report;
        }

        public void Dispose()
        {
        }
    }

    public abstract class AveObjectFeature
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static AveObjectFeature CreateInstance(object obj)
        {
            AveObjectFeature instance = null;

            string type = obj.GetType().Name;
            switch (type)
            {
                case "AveSPSite":
                    instance = new AveSiteFeature((AveSPSite)obj);
                    break;
                case "AveSPWeb":
                    instance = new AveWebFeature((AveSPWeb)obj);
                    break;
                    //case "AveSPWebApp":
                    //instance = new AveWebAppFeature((AveSPWebApp)obj);
                    break;
                default:
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType().ToString());
            }

            return instance;
        }

        //public abstract void ActivateFeature(AveFeatureInfo featureInfo);

        public abstract void RestoreFeatures(List<AveFeatureInfo> featureInfoList);

        //protected bool ActivateFeature(IAveFeatureCollection featureCollection, Guid featureId)
        //{
        //    try
        //    {
        //        featureCollection.Add(featureId, true);
        //        return true;
        //    }
        //    catch (InvalidOperationException e)
        //    {
        //        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while add the feature. feature id:{0}\n error message:{1}", featureId, e));
        //        return false;
        //    }
        //}
    }

    public class AveSiteFeature : AveObjectFeature
    {
        private AveSPSite mAveSPSite;
        public AveSiteFeature(AveSPSite aveSite)
        {
            mAveSPSite = aveSite;
        }

        //public override void ActivateFeature(AveFeatureInfo featureInfo)
        //{
        //    foreach (Guid featureId in featureInfo.Dependencies)
        //    {
        //        try
        //        {
        //            if (mAveSPSite.SPSite.Features[featureId] == null)
        //                ActivateFeature(mAveSPSite.SPSite.Features, featureId);
        //        }
        //        catch (Exception e)
        //        {
        //            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while activate the feature. feature id:{0}\n error message:{1}", featureId, e));
        //        }
        //    }
        //    if (mAveSPSite.SPSite.Features[featureInfo.Id] == null)
        //    {
        //        ActivateFeature(mAveSPSite.SPSite.Features, featureInfo.Id);
        //    }
        //}

        public override void RestoreFeatures(List<AveFeatureInfo> featureInfoList)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSiteFeature.RestoreSiteFeatures"))
            {
#endif
                List<AveFeatureInfo> needRestoreFeatureInfoList = new List<AveFeatureInfo>();
                foreach (AveFeatureInfo featureInfo in featureInfoList)
                {
                    if (mAveSPSite.SPSite.Features[featureInfo.Id] == null)
                    {
                        List<Guid> dependence = new List<Guid>();
                        foreach (Guid guid in featureInfo.Dependencies)
                        {
                            if (mAveSPSite.SPSite.Features[guid] == null)
                            {
                                dependence.Add(guid);
                            }
                        }
                        featureInfo.Dependencies = dependence;
                        needRestoreFeatureInfoList.Add(featureInfo);
                    }
                }
                mAveSPSite.SPSite.FeatureSerializer.SetObjectData(needRestoreFeatureInfoList); 
                //mAveSPSite.SPSite.Features.RestoreFeatures(needRestoreFeatureInfoList);
#if PerformanceLog
            }
#endif
        }
    }

    public class AveWebFeature : AveObjectFeature
    {
        private AveSPWeb mAveSPWeb;
        public AveWebFeature(AveSPWeb aveWeb)
        {
            mAveSPWeb = aveWeb;
        }

        //public override void ActivateFeature(AveFeatureInfo featureInfo)
        //{
        //    foreach (Guid featureId in featureInfo.Dependencies)
        //    {
        //        try
        //        {
        //            if (mAveSPWeb.SPWeb.Features[featureId] == null && mAveSPWeb.ParentSite.SPSite.Features[featureId] == null)
        //            {
        //                bool value = ActivateFeature(mAveSPWeb.SPWeb.Features, featureId);
        //                if (value == false)
        //                {
        //                    ActivateFeature(mAveSPWeb.ParentSite.SPSite.Features, featureId);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while activate the feature. feature id:{0}\n error message:{1}", featureId, e));
        //        }
        //    }
        //    if (mAveSPWeb.SPWeb.Features[featureInfo.Id] == null)
        //    {
        //        ActivateFeature(mAveSPWeb.SPWeb.Features, featureInfo.Id);
        //    }
        //}

        public override void RestoreFeatures(List<AveFeatureInfo> featureInfoList)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSiteFeature.RestoreWebFeatures"))
            {
#endif
                //CI  SAAS-7111
                DeactivateMinimalDownloadStrategy(featureInfoList);

                List<AveFeatureInfo> needRestoreFeatureInfoList = new List<AveFeatureInfo>();
                foreach (AveFeatureInfo featureInfo in featureInfoList)
                {
                    if (mAveSPWeb.SPWeb.Features[featureInfo.Id] == null)
                    {
                        List<Guid> dependence = new List<Guid>();
                        foreach (Guid guid in featureInfo.Dependencies)
                        {
                            if (mAveSPWeb.SPWeb.Features[guid] == null && mAveSPWeb.SPWeb.Site.Features[guid] == null)
                            {
                                dependence.Add(guid);
                            }
                        }
                        featureInfo.Dependencies = dependence;
                        needRestoreFeatureInfoList.Add(featureInfo);
                    }
                }
                mAveSPWeb.SPWeb.FeatureSerializer.SetObjectData(needRestoreFeatureInfoList);    
                mAveSPWeb.SPWeb.Update();
                mAveSPWeb.SPWeb.ReloadWeb();
                //mAveSPWeb.SPWeb.Features.RestoreFeatures(needRestoreFeatureInfoList);
#if PerformanceLog
            }
#endif
        }
        private void DeactivateMinimalDownloadStrategy(List<AveFeatureInfo> featureInfoList)
        {
            try
            {
                Guid minimalDownloadStrategy = new Guid("87294c72-f260-42f3-a41b-981a2ffce37a");

                if (featureInfoList != null)
                {
                    bool featureExisted = featureInfoList.Exists((featureInfo) => featureInfo.Id == minimalDownloadStrategy);

                    if (!featureExisted && mAveSPWeb.SPWeb.Features[minimalDownloadStrategy] != null)
                    {
                        mAveSPWeb.SPWeb.Features.Remove(minimalDownloadStrategy, true);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("failed dto deactivate minimal download strategy due to: {0}", e.ToString());
            }
        }
    }

    //public class AveWebAppFeature : AveObjectFeature
    //{
    //    private AveSPWebApp mAveSPWebApp;

    //    public AveWebAppFeature(AveSPWebApp aveWebApp)
    //    {
    //        mAveSPWebApp = aveWebApp;
    //    }

    //    public override void ActivateFeature(AveFeatureInfo featureInfo)
    //    {
    //        foreach (Guid featureId in featureInfo.Dependencies)
    //        {
    //            if (mAveSPWebApp.WebApp.Features[featureId] == null)
    //            {
    //                ActivateFeature(mAveSPWebApp.WebApp.Features, featureId);
    //            }
    //        }
    //        if (mAveSPWebApp.WebApp.Features[featureInfo.Id] == null)
    //        {
    //            ActivateFeature(mAveSPWebApp.WebApp.Features, featureInfo.Id);
    //        }
    //    }

    //    public override void RestoreFeatures(List<AveFeatureInfo> featureInfoList)
    //    {
    //        List<AveFeatureInfo> needRestoreFeatureInfoList = new List<AveFeatureInfo>();
    //        foreach (AveFeatureInfo featureInfo in featureInfoList)
    //        {
    //            if (mAveSPWebApp.WebApp.Features[featureInfo.Id] == null)
    //            {
    //                List<Guid> dependence = new List<Guid>();
    //                foreach (Guid guid in featureInfo.Dependencies)
    //                {
    //                    if (mAveSPWebApp.WebApp.Features[guid] == null)
    //                    {
    //                        dependence.Add(guid);
    //                    }
    //                }
    //                featureInfo.Dependencies = dependence;
    //                needRestoreFeatureInfoList.Add(featureInfo);
    //            }
    //        }
    //        mAveSPWebApp.WebApp.Features.RestoreFeatures(needRestoreFeatureInfoList);
    //    }
    //}

}
