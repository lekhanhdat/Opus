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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using RAGoogle.Archive.ArchiverJob;
using RAGoogle.Archive.Scan.Implement;
using RAGoogle.Archive.Scan.Interface;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover.Impl;
using RAGoogle.Helper;
using RAGoogle.Models;
using RAGoogle.Report;

namespace RAGoogle.Archive.Scan.Base;

public abstract class GoogleScannerBase : IGoogleScanner
{
    protected readonly IRALogger logger = RALogger.GetInstance(typeof(GoogleScannerBase));

    internal ScanJobSettings jobSettings { get; set; }
    internal GoogleConfiguration mConfiguration { get; set; }
    private IScanDataReader mScanDataReader { get; set; }
    internal GoogleDriveTreeNodeDto node { get; set; }
    private IDiscoverNodeWorker discoverWorker { get; set; }
    protected const int MaxDegreeOfParallelism = 10;
    protected const int MaxDegreeOfParallelismForFolder = 1;
    protected StopJobCts Cts;
    private RMGoogleArchiveFullDiscover _disocvery { get; set; }
    protected ReportCenter _reportCenter { get; set; }

    public GoogleScannerBase(ScanJobSettings scanJobSettings, StopJobCts cts)
    {
        jobSettings = scanJobSettings;
        node = scanJobSettings.Configuration.SelectedNode;
        mConfiguration = scanJobSettings.Configuration;
        mScanDataReader = new ScanDataReader(mConfiguration);
        discoverWorker = new DiscoverNodeWorkerBase(mConfiguration);
        this._reportCenter = scanJobSettings.Configuration.ReportCenter;
        this._disocvery = InitArchiveFullDiscoverAsync(node).GetAwaiter().GetResult();
        this.Cts = cts;
    }
    public async Task RunAsync()
    {
        try
        {
            using CheckJobStopScope subScope = new();
            switch (node.Level)
            {
                case NodeLevel.GoogleMyDrive:
                case NodeLevel.GoogleSharedDrive:
                    {
                        await ProcessDriveAsync(node, mConfiguration.GoogleSetting);
                        break;
                    }
                case NodeLevel.GoogleFolder:
                    {
                        var archiveNode = new ArchiverNodeItem()
                        {
                            ID = node.ObjectId,
                            Title = node.Name,
                            NodeLevel = node.Level,
                            Discovery = _disocvery
                        };
                        await ProcessFolderAsync(archiveNode);
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException($"Node level {node.Level} is not supported.");
                    }
            };
        }
        catch (JobStopException)
        {
            logger.Warn("The job has stopped.");
            _reportCenter.JobHasStopped = true;
            throw new JobStopException("The job has stopped.");
        }
        catch (Exception ex)
        {
            logger.Info($"Failed to processing node:{node.ID},exception:{ex}");
            throw;
        }
        finally
        {
            discoverWorker.Flush();
            if (_reportCenter.GetMainJobState() == JobStatus.Stopping)
            {
                logger.Warn("The main job is stopping, need to stop all sub job.");
                throw new JobStopException("The job has stopped.");
            }
        }
    }
    private async Task ProcessDriveAsync(GoogleDriveTreeNodeDto node, RMGoogleSetting setting)
    {
        using (var performance = new PerformanceScope("GoogleScannerBase.ProcessDriveAsync"))
        using (CheckJobStopScope jScope = new CheckJobStopScope())
        {
            try
            {
                var container = new ArchiverNodeItem()
                {
                    ID = node.Parent.ID,
                    Title = node.Parent.Name,
                    Discovery = _disocvery,
                    NodeLevel = node.Parent.Level
                };
                var driveNode = container.GenerateDriveNodeItem(node);
                var result = await discoverWorker.ProcessContainerAsync(driveNode, ProcessType.NeedProcess);
                if (result == ProcessResult.SkipCurrentNode)
                {
                    logger.Warn("Drive skip.");
                    return;
                }
                await ProcessItemsAndSubfoldersAsync(driveNode);
            }
            catch (JobStopException)
            {
                logger.Warn("The records disposal job has been stopped.");
                throw new JobStopException("The job has stopped."); ;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to process records disposal job, Message: {ex}");
                throw;
            }
        }
    }

    private async Task ProcessItemsAndSubfoldersAsync(ArchiverNodeItem node)
    {
        var message = string.Empty;
        JobDetailsStatus status = JobDetailsStatus.Successful;
        await node.GetSubFiles().ParallelExecute(async item =>
        {
            var ruleName = string.Empty;
            try
            {
                using (CheckJobStopScope subJScope = new CheckJobStopScope())
                {
                    using (new PerformanceScope("GoogleScannerBase:ProcessDataItemAsync"))
                    {
                        if (item == null || item.Level != RMNodeLevel.GoogleFile) return;
                        logger.Info($"Process item name:{item.Id}");

                        var parentIds = (node.NodeLevel == NodeLevel.GoogleFolder) ? $"{item.ParentIds}/{node.ID}" : $"{item.ParentIds}";

                        using (var itemNode = node.GenerateItemNodeItem(item, parentIds))
                        {
                            await discoverWorker.ProcessItemAsync(itemNode, node);
                            ruleName = itemNode.RuleName;
                            //discover file version
                            if (item.Versions != null && item.Versions.Count > 1)
                            {
                                var allHistoryVersions = item.Versions.OrderByDescending(v => v.ModifiedTimeDateTimeOffset).Skip(1);
                                int index = allHistoryVersions.Count();
                                foreach (var version in allHistoryVersions)
                                {
                                    var versionItem = new GoogleItemData
                                    {
                                        Id = item.Id,
                                        Name = item.Name,
                                        RelativePath = item.RelativePath,
                                        Path = item.Path,
                                        FileExtension = item.FileExtension,
                                        HasAugmentedPermissions = item.HasAugmentedPermissions,
                                        AllowFileDiscovery = item.AllowFileDiscovery,
                                        DriveName = item.DriveName,
                                        ParentId = item.ParentId,
                                        Level = item.Level,
                                        CreatedTime = version.ModifiedTimeDateTimeOffset?.UtcDateTime ?? item.CreatedTime,
                                        ModifiedTime = version.ModifiedTimeDateTimeOffset?.UtcDateTime ?? item.ModifiedTime,
                                        CreatedBy = item.CreatedBy,
                                        ModifierName = version.LastModifyingUser.DisplayName,
                                        ModifiedBy = version.LastModifyingUser.DisplayName,
                                        TenantId = item.TenantId,
                                        MemberEmail = item.MemberEmail,
                                        Size = version.Size ?? item.Size, //version size of file that type is google style is null
                                        LableIds = item.LableIds,
                                        MetaInfo = item.MetaInfo,
                                        DriveId = item.DriveId,
                                        IsDeleted = item.IsDeleted,
                                        Versions = [version],
                                        ParentIds = parentIds
                                    };

                                    using (var versionItemNode = itemNode.GenerateItemVersionNodeItem(versionItem, index--))
                                    {
                                        await discoverWorker.ProcessItemAsync(versionItemNode, itemNode);
                                        ruleName = versionItemNode.RuleName;
                                    }
                                }
                            }
                        }
                        logger.Info($"Process item name:{item.Id}.");
                    }
                }
            }
            catch (JobStopException)
            {
                logger.Warn("The job has been stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = JobDetailsStatus.Failed;
                logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
            }
        }, MaxDegreeOfParallelismForFolder, Cts.Token);

        await ProcessFolderAsync(node);
    }

    private async Task ProcessFolderAsync(ArchiverNodeItem node)
    {
        var message = string.Empty;
        JobDetailsStatus status = JobDetailsStatus.Successful;
        using (CheckJobStopScope jScope = new())
        {
            await node.GetSubFolders().ParallelExecute(async item =>
            {
                try
                {
                    using (CheckJobStopScope subJScope = new CheckJobStopScope())
                    {
                        using (new PerformanceScope("GoogleScannerBase:ProcessDataItemAsync"))
                        {
                            if (item == null || item.Level != RMNodeLevel.GoogleFolder) return;
                            logger.Info($"Process item name:{item.Name}");
                            var parentIds = (node.NodeLevel == NodeLevel.GoogleFolder) ? $"{item.ParentIds}/{node.ID}" : $"{item.ParentIds}";
                            using (var itemNode = node.GenerateFolderNodeItem(item, parentIds))
                            {
                                await discoverWorker.ProcessContainerAsync(itemNode, ProcessType.NeedProcess);
                                await ProcessItemsAndSubfoldersAsync(itemNode);
                            }
                        }
                    }
                }
                catch (JobStopException)
                {
                    logger.Warn("The job has been stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    status = JobDetailsStatus.Failed;
                    logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
                }
            }, MaxDegreeOfParallelismForFolder, Cts.Token);
        }
    }
    public IScanDataReader GetScanDataReader()
    {
        return mScanDataReader;
    }
    #region Archive
    public async Task<RMGoogleArchiveFullDiscover> InitArchiveFullDiscoverAsync(GoogleDriveTreeNodeDto node)
    {
        GoogleDriveData driveData = ConvertHelper.ConvertDtoNodeTreeToData(node, mConfiguration.AppProfile.TenantId);
        RMGoogleArchiveFullDiscover fullDiscover = new(null, driveData);
        fullDiscover.Init(_reportCenter, mConfiguration.AppProfile, true, true);
        await fullDiscover.InitDiscoverAsync();
        return await Task.FromResult(fullDiscover);
    }
    #endregion
    public void Dispose()
    {
        //discoverWorker.Dispose();
    }

}