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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.SharePoint.ScanLocalNode.Browser
{
    public class TreeBrowser
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(TreeBrowser));

        private static readonly AveObjectModelFactory ObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto);

        private static readonly IReportService<JMJobDetails> JobDetailManager = JobContext.Current.JobDetailManager.Create();

        private static readonly object Locker = new object();

        private static string _FarmId = null;

        public static string FarmId
        {
            get
            {
                if (_FarmId == null)
                {
                    lock (Locker)
                    {
                        if (_FarmId == null)
                        {
                            _FarmId = GetFarmId();
                        }
                    }
                }
                return _FarmId;
            }
        }

        private static string GetFarmId()
        {
            try
            {
                var farmId = ObjectModelFactory.CreateFarm()?.Local?.ID;
                Logger.Info($"Successful get farmId: [{farmId}].");
                return farmId == Guid.Empty ? null : farmId.ToString();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get farm id. Error: [{e}]");
                AddJobFailedDetail("", "", "RM_JS_Rule_ObjectLevel_Farm", e.Message);
            }
            return null;
        }

        public static HashSet<OnPremiseSPLocalNode> GetWebApplications(ref bool hasFailed)
        {
            var result = new HashSet<OnPremiseSPLocalNode>();

            try
            {
                Logger.Info("Start get web applications.");
                var webService = ObjectModelFactory.CreateWebService();
                if (webService.ContentService == null)
                {
                    throw new Exception("Can't get the content service, maybe the agent account does not have sufficient permission.");
                }

                foreach (var webApp in webService.ContentService.WebApplications)
                {
                    try
                    {
                        var node = ConvertWebApplicationToLocalNode(webApp);
                        Logger.Info($"Browse web application id: [{node.ObjectId}], url: [{node.Url.LogBase64()}].");
                        result.Add(node);
                    }
                    catch (Exception e)
                    {
                        hasFailed = true;
                        Logger.Error($"An error occurred while browsing web application: [{webApp?.ID}] name: [{webApp?.Name.LogBase64()}]. Error: {e}");
                        try
                        {
                            AddJobFailedDetail(webApp.Name, webApp.AlternateUrls?.GetResponseUrl(AveUrlZone.Default).Uri.ToString(), "RM_JS_Rule_ObjectLevel_WebApplication", e.Message);
                        }
                        catch (Exception innerEx)
                        {
                            Logger.Error($"An error occurred while add webapp job failed detail. Error: {innerEx}");
                        }

                    }
                }
                Logger.Info($"Finish get web applications. Count: [{result.Count}].");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get web applications. Error: {e}");
                AddJobFailedDetail("", "", "RM_JS_Rule_ObjectLevel_Farm", e.Message);
                throw;
            }

            return result;
        }

        public static HashSet<OnPremiseSPLocalNode> GetSiteCollections(OnPremiseSPLocalNode webAppNode, ref bool hasFailed)
        {
            var result = new HashSet<OnPremiseSPLocalNode>();

            try
            {
                Logger.Info($"Start get site collections under web applicaton, Url: [{webAppNode.Url.LogBase64()}], Id: [{webAppNode.ObjectId}].");

                var webApp = ObjectModelFactory.CreateWebApplication(webAppNode.Url);
                if (webApp == null)
                {
                    throw new Exception($"Can't find web application: {webAppNode.Url}.");
                }

                foreach (var site in webApp.Sites)
                {
                    //由于manage path被删除，导致load出的应用了该path的site 是null，需要过滤 && FasterCreation Site need be filtered out
                    if (site == null || site.IsSiteMaster)
                    {
                        continue;
                    }

                    var siteTitle = string.Empty;
                    try
                    {
                        siteTitle = site.RootWeb.Title;
                        var node = ConvertSiteCollectionToLocalNode(site, siteTitle, webAppNode.Id);
                        Logger.Info($"Browse site collection: [{site.Url.LogBase64()}]");
                        result.Add(node);
                        site.Dispose();
                    }
                    catch (Exception e)
                    {
                        hasFailed = true;
                        Logger.Info($"An error occurred while browsing site colleciton: [{site.Url.LogBase64()}]. Error: {e}");
                        try
                        {
                            AddJobFailedDetail(siteTitle, site.Url, "RM_JS_Rule_ObjectLevel_SiteCollection", e.Message);
                        }
                        catch (Exception innerEx)
                        {
                            Logger.Error($"An error occurred while add site collection job failed detail. Error: {innerEx}");
                        }
                    }
                }

                Logger.Info($"Successful get site collections under web applicaton, Url: [{webAppNode.Url.LogBase64()}], Id: [{webAppNode.ObjectId}]. Count: [{result.Count}].");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get site collection under web applicaton, Url: [{webAppNode.Url.LogBase64()}], Id: [{webAppNode.ObjectId}]. Error: {e}");
                AddJobFailedDetail(webAppNode.Name, webAppNode.Url, "RM_JS_Rule_ObjectLevel_WebApplication", e.Message);
                throw;
            }

            return result;
        }

        private static OnPremiseSPLocalNode ConvertWebApplicationToLocalNode(IAveWebApplication webApp)
        {
            return new OnPremiseSPLocalNode
            {
                Id = Guid.NewGuid().ToString(),
                ObjectId = webApp.ID.ToString(),
                ParentId = FarmId,
                FarmId = FarmId,
                Url = webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString(),
                Name = webApp.Name,
                Description = null,
                NodeLevel = (int)NodeLevel.WebApplication,
                SiteCollectionType = 0,
                SPVersion = "0",
                CreateTime = DateTime.UtcNow.Ticks,
                ModifiedDate = DateTime.UtcNow.Ticks,
            };
        }

        private static OnPremiseSPLocalNode ConvertSiteCollectionToLocalNode(IAveSite site, string title, string parentId)
        {
            return new OnPremiseSPLocalNode
            {
                Id = Guid.NewGuid().ToString(),
                ObjectId = site.ID.ToString(),
                ParentId = parentId,
                FarmId = FarmId,
                Url = site.Url,
                Name = title,
                Description = null,
                NodeLevel = (int)NodeLevel.SiteCollection,
                SiteCollectionType = 0,
                SPVersion = "0",
                CreateTime = DateTime.UtcNow.Ticks,
                ModifiedDate = DateTime.UtcNow.Ticks,
            };
        }

        private static void AddJobFailedDetail(string name, string url, string itemType, string exceptionComment)
        {
            JobDetailManager.Commit(new JMScanLocalNodesJobDetails
            {
                ObjectName = name,
                FullPath = url,
                ItemType = itemType,
                Action = string.Empty,
                Status = JobDetailsStatus.Failed,
                Comment = exceptionComment,
                AgentName = OSInformation.HostName
            });
        }
    }
}
