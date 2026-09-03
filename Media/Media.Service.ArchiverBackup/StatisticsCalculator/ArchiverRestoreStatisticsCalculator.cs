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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    #endregion

    public class ArchiverRestoreStatisticsCalculator
        : StatisticsCalculatorBase<ArchiverRestoreJob, ArchiverStatisticsCalculateResult>
        , IStatisticsCalculator
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem indexLogicalDevice;
        Dictionary<SPObjectType, RestoreStatistics> resultStatistics;

        public IRestoreServiceTreeHandler TreeHandler { get; set; }
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public override void Open(ArchiverRestoreJob calculateInfo)
        {
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreStatisticsCalculatorOpenBegin);
            this.resultStatistics = InitResultDictionary();
            this.TreeHandler.CutTree(calculateInfo.TreeRoot);
            this.indexLogicalDevice = this.StorageDeviceManager.Open(calculateInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            var openParam = new ArchiverIndexServiceOpenParameter(calculateInfo, indexLogicalDevice);
            this.IndexService.Open(openParam);
        }

        public override ArchiverStatisticsCalculateResult Calculate(ArchiverRestoreJob calculateInfo)
        {
            var result = new ArchiverStatisticsCalculateResult();
            var siteCollectionNode = this.GetSiteCollectionTreeNode(calculateInfo.TreeRoot);
            var restoreTreeHandlerParam = new TreeNodeParameter { CurrentTree = siteCollectionNode, RestoreJob = calculateInfo };
            this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItem);
            this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
            this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItem);
            result.ResultStatistics = this.resultStatistics;
            logger.Info(result.ToString());
            return result;
        }

        void CalculateIndexItem(Object sender, IndexItemProceedEventArgs args)
        {
            this.CalculateStatisticsForeachIndex(args.IndexItem as ArchiverBasicIndex);
        }

        SPTreeNodeDto GetSiteCollectionTreeNode(SPTreeNodeDto treeNode)
        {
            return treeNode.Children[0].Children[0].Children[0];
        }

        public override void ProcessException(Exception e)
        {
            this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreStatisticsCalculatorProcessExceptionError, e.ToString());
        }

        private Dictionary<SPObjectType, RestoreStatistics> InitResultDictionary()
        {
            var resultDic = new Dictionary<SPObjectType, RestoreStatistics>();
            resultDic[SPObjectType.SiteCollection] = new RestoreStatistics();
            resultDic[SPObjectType.Site] = new RestoreStatistics();
            resultDic[SPObjectType.ListOrLibrary] = new RestoreStatistics();
            resultDic[SPObjectType.Folder] = new RestoreStatistics();
            resultDic[SPObjectType.Item] = new RestoreStatistics();
            resultDic[SPObjectType.Document] = new RestoreStatistics();
            resultDic[SPObjectType.ItemVersion] = new RestoreStatistics();
            resultDic[SPObjectType.DocumentVersion] = new RestoreStatistics();
            resultDic[SPObjectType.Attachment] = new RestoreStatistics();
            return resultDic;
        }

        private void CalculateStatisticsForeachIndex(ArchiverBasicIndex index)
        {
            switch (index.Type)
            {
                case "E":
                    CalculateStatisticsByIndex(resultStatistics[SPObjectType.SiteCollection], index);
                    break;
                case "W":
                    CalculateStatisticsByIndex(resultStatistics[SPObjectType.Site], index);
                    break;
                case "L":
                    CalculateStatisticsByIndex(resultStatistics[SPObjectType.ListOrLibrary], index);
                    break;
                case "F":
                    CalculateStatisticsByIndex(resultStatistics[SPObjectType.Folder], index);
                    break;
                case "D":
                case "V":
                    if (index.Name.Contains(":"))
                        CalculateStatisticsByIndex(resultStatistics[SPObjectType.DocumentVersion], index);
                    else
                        CalculateStatisticsByIndex(resultStatistics[SPObjectType.Document], index);
                    break;
                case "I":
                case "U":
                    if (index.Name.Contains(":"))
                        CalculateStatisticsByIndex(resultStatistics[SPObjectType.ItemVersion], index);
                    else
                        CalculateStatisticsByIndex(resultStatistics[SPObjectType.Item], index);
                    break;
                case "A":
                    CalculateStatisticsByIndex(resultStatistics[SPObjectType.Attachment], index);
                    break;
                default://其它类型暂不处理
                    logger.Warn(MediaServiceArchiverBackupResource.ArchiverRestoreStatisticsCalculatorCalculateStatisticsForeachIndexUnknownType, index.Type);
                    break;
            }
        }

        private void CalculateStatisticsByIndex(RestoreStatistics granularRestoreStatistics, ArchiverBasicIndex index)
        {
            granularRestoreStatistics.TotalCount++;
            granularRestoreStatistics.TotalSize += index.DataFileLength;
        }

        public override void Dispose()
        {
            if (IndexService != null)
            {
                IndexService.Close();
            }
            if (indexLogicalDevice != null)
            {
                indexLogicalDevice.Close();
            }
        }
    }
}