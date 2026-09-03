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
//using AvePoint.Adonis.Records.Object.ActionOnly;
//using AvePoint.GCommon.Contract.Tree.Object;
//using AvePoint.RA.SharePoint.Common;
//using AvePoint.Wrapper.Common;
//using AvePoint.Wrapper.Discovery;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AvePoint.RA.SharePoint.ActionOnly.SPActionOnly
//{
//    public class SPActionProcessorNormal : BaseSPActionProcessor
//    {
//        public SPActionProcessorNormal(RecordsActionOnlyJobMessage message) : base(message)
//        {

//        }
//        public SPActionProcessorNormal(RecordsActionOnlyJobMessage message, SPTreeNodeDto current) : base(message, current)
//        {

//        }
//        public override void ProcessSiteCollection(SPTreeNodeDto site)
//        {
//            ProgressService.IncreaseBase(1);
//            IAveSite aveSite = null;
//            IAveDiscoverSite discoverSite = null;
//            using (new RA.Common.PerformanceScope(string.Format("Process Site Collection")))
//            {
//                aveSite = ObjectModelFactory.CreateSite(site.FullPath);
//                discoverSite = discoverFactory.CreateDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
//                foreach (var discoverWeb in discoverSite.GetAllWebs())
//                {
                   
//                    try
//                    {
//                        ProcessSite(discoverWeb.Value);
//                    }
//                    catch (Exception e)
//                    {
//                        JobHasErrorNode = true;
//                        AddFailedDetail(discoverWeb.Value, e.Message);
//                        logger.Warn($"Process site collection failed {e.ToString()}");
//                    }
//                }
//            }
//            DisposeSPObj(aveSite);
//            base.ProcessSiteCollection(site);
//        }
//        public override void ProcessSite(IAveDiscoverWeb site)
//        {
//            ProgressService.IncreaseBase(1);
//            logger.Info($"Process site {site.FullUrl}");
//            if (IsInExcludeNodeList(site.AveWeb.Url))
//            {
//                return;
//            }
//            lock (LockObj)
//            {
//                try
//                {
//                    TimeZones.Add(site.AveWeb.ID, site.AveWeb.RegionalSettings.TimeZone);
//                }
//                catch (Exception e)
//                {
//                    logger.Info($"Init time zone failed {e.ToString()}");
//                }
//            }
//            foreach (var list in site.GetLists())
//            {
//                try
//                {
//                    if (IsInExcludeNodeList(list.Value.RootFolderUrl))
//                    {
//                        continue;
//                    }
//                    ProcessList(list.Value);
//                }
//                catch (Exception e)
//                {
//                    JobHasErrorNode = true;
//                    AddFailedDetail(list.Value, e.Message);
//                    logger.Warn($"Process list failed {e.ToString()}");
//                }
//            }
//            DisposeSPObj(site);
//            base.ProcessSite(site);
//        }
//        public override void ProcessList(IAveDiscoverList discoverList)
//        {
//            ProgressService.IncreaseBase(1);
//            if (IsSystemList(discoverList))
//            {
//                return;
//            }
//            IAveList list;
//            try
//            {
//                list = discoverList.GetListObject();
//            }
//            catch (Exception e)
//            {
//                logger.Info($"Skip list type {discoverList.ServerTemplate} :{discoverList.Title} Can't get listobj");
//                return;
//            }
//            if (list.BaseType != AveBaseType.DocumentLibrary)
//            {
//                logger.Info($"Skip all other list type {list.BaseType} :{list.Title}");
//                return;
//            }
//            IAveTaxonomyField mmsField = GetTaxonomyField(list.Fields, BCSColumnName);
//            var BCSColumnInternalName = mmsField.InternalName;
//            var rootFolder = discoverList.GetRootFolder();
//            ProcessFolder(rootFolder);
//            base.ProcessList(discoverList);
//        }
//        public override void ProcessList(IAveList list)
//        {
//            if (list.BaseType != AveBaseType.DocumentLibrary)
//            {
//                logger.Info($"Skip all other list type {list.BaseType} :{list.Title}");
//                return;
//            }
//            IAveTaxonomyField mmsField = GetTaxonomyField(list.Fields, BCSColumnName);
//            var BCSColumnInternalName = mmsField.InternalName;
//            var discoverFolders = CommonUtility.GetAllFolders(list);
//            foreach (var folder in discoverFolders)
//            {
//                ProcessFolder(folder);
//            }
//            base.ProcessList(list);
//        }
//        public override void ProcessFolder(IAveDiscoverFolder folder)
//        {
//            IAveTaxonomyField mmsField = GetTaxonomyField(folder.AveFolder.ParentList.Fields, BCSColumnName);
//            var BCSColumnInternalName = mmsField.InternalName;
//            foreach (var item in folder.GetItems())
//            {
//                if (item.CurrentItem != null)
//                {
//                    ProcessItem(item.CurrentItem, BCSColumnInternalName);
//                }
//            }
//            foreach (var subfolder in folder.GetSubFolders())
//            {
//                ProcessFolder(subfolder);
//            }
//            base.ProcessFolder(folder);
//        }
//        public override void ProcessFolder(IAveFolder folder)
//        {
//            IAveTaxonomyField mmsField = GetTaxonomyField(folder.ParentList.Fields, BCSColumnName);
//            var BCSColumnInternalName = mmsField.InternalName;
            
//            foreach (var subfolder in folder.SubFolders)
//            {
//                ProcessFolder(subfolder);
//            }

//            base.ProcessFolder(folder);
//        }
//        public override void ProcessItem(IAveListItem item, string BCSColumnInternalName)
//        {
//            base.ProcessItem(item, BCSColumnInternalName);
//        }
//        public override bool Run()
//        {
//            bool result = false;
//            try
//            {
//                //Multithread is need in site collection level???
//                logger.Info($"Current Node Level {CurrentNode.Level.ToString()} : URL {CurrentNode.Url}");
//                switch (CurrentNode.Level)
//                {
//                    case NodeLevel.SiteCollection:
//                        ProcessSiteCollection(CurrentSiteColTreeNode);
//                        break;
//                    case NodeLevel.Site:
//                        var aveSite = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath);
//                        var discoverSite = discoverFactory.CreateDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
//                        IAveDiscoverWeb site;
//                        using (new RA.Common.PerformanceScope(string.Format("Get Discover site obj")))
//                        {
//                            site = discoverSite.GetAllWebs()[new Guid(CurrentNode.SPObjectId)];//TO DO Performance..
//                        }

//                        ProcessSite(site);
//                        break;
//                    case NodeLevel.List://No Need discover obj.
//                        var aveSite1 = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath);
//                        var discoverSite1 = discoverFactory.CreateDiscoverSite(aveSite1, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
//                        IAveDiscoverWeb site1;
//                        using (new RA.Common.PerformanceScope(string.Format("Get Discover site obj")))
//                        {
//                            site1 = discoverSite1.GetAllWebs()[new Guid(CurrentNode.Parent.Parent.SPObjectId)];//TO DO Performance..
//                        }
//                        IAveDiscoverList discoverList;
//                        IAveWeb aveWeb = aveSite1.OpenWeb(CurrentNode.Parent.Parent.SPObjectId);
                        
//                        using (new RA.Common.PerformanceScope(string.Format("Get Discover List obj")))
//                        {
//                            discoverList = discoverSite1.GetDiscoverList(aveSite1, aveWeb, CurrentNode.FullPath);
//                        }
//                        ProcessList(discoverList);
//                        break;
//                }
//            }
//            finally
//            {
//                result = base.Run();
//            }
//            return result;
//        }
//    }
//}
