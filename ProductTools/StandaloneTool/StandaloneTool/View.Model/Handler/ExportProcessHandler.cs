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
using Amazon.Runtime.Internal.Transform;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using DataExportCore;
using StandaloneTool.Common;
using StandaloneTool.Model.Common;
using StandaloneTool.View.Model.Binding;
using StandaloneTool.View.Model.Command;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace StandaloneTool.View.Model.Handler
{
    internal class ExportProcessHandler : BackgroundWorkerBase
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(ExportProcessHandler));
        private ProcessViewModel processViewModel = ProcessViewModel.Instance;
        private BaseDataContext dataContext = BaseDataContext.Instance;
        private ExchangeDataInfo exchangeDataInfo = ExchangeDataInfo.GetInstance();
        private ExportLocationViewModel exportLocationViewModel = ExportLocationViewModel.Instance;
        private GlobalConfiguration globalConfig = GlobalConfiguration.Instance;
        private readonly DatabaseHelper dbHelper = DatabaseHelper.Instance;
        private ProgressManager progressManager;
        public override void Execute()
        {
            InitializeBackgroundWorker(this);
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            exportLocationViewModel.InitErrorMessage();
            Application.Current.Dispatcher.Invoke(() =>
            {
                dataContext.ModelCommonInfo.IsCover = false;
                dataContext.NavigationOperator.AutoSwitchPageNext();
            }, DispatcherPriority.Normal);

            try
            {
                ExportProcessHandler instance = (ExportProcessHandler)e.Argument;
                instance.ExecuteExportArchivedSites();
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while execute export process: {ex}.");
            }
        }

        protected override async void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                GlobalInfo.TotalExportedSize = progressManager.TotalExportSize;
                GlobalInfo.FinalJobStatus = progressManager.GetFinalJobStatus();
                //Avoid error occurred while exporting process, the progress is not enough 100%.
                await RefreshMainProgressBar(processViewModel.Process, 100);
            }, DispatcherPriority.Normal);

            await Task.Delay(500);
            dataContext.NavigationOperator.AutoSwitchPageNext();
        }

        private void ExecuteExportArchivedSites()
        {
            try
            {
                if (GlobalInfo.Module is Module.OneDrive or Module.SharePointOnline)
                {
                    logger.Info($"Start export archived sites for module [{GlobalInfo.Module}], count site [{exchangeDataInfo.SelectionList.Count}], location [{exportLocationViewModel.LocationType}]. IsSkipAPData: {GlobalInfo.IsSkipAPData}");
                    CreateExportDestination(exportLocationViewModel.ExportLocation);
                    var archiverSites = exchangeDataInfo.SelectionList.Select(i => (ArchiverSiteBase)i).ToList();
                    progressManager = new ProgressManager(archiverSites.Count);
                    if (GlobalInfo.EncryptionInfoCache.TryGetValue(GlobalInfo.ExportDBFilePath, out string encryptExportPwd) && !string.IsNullOrEmpty(encryptExportPwd))
                    {
                        var exportOption = new ExportOption()
                        {
                            EncryptedDBPath = GlobalInfo.ExportDBFilePath,
                            ExportLocation = exportLocationViewModel.ExportLocation,
                            EncryptedPassword = encryptExportPwd.ToSecureString(),
                            DataType = (DataType)GlobalInfo.Module,
                            MaxThreadCount = globalConfig.GetSetting<int>(AppSettingKey.MAX_THREAD_COUNT),
                            IsSkipAPData = GlobalInfo.IsSkipAPData,
                            AvepointMappingStorage = GlobalInfo.AvepointMappingStorage,
                            TargetStorage = GlobalInfo.TargetStorage,
                            TargetStorageType = GlobalInfo.TargetStorageType
                        };
                        ExportController exportController = new ExportController(archiverSites, exportOption, progressManager);
                        progressManager.OverallProgressChanged += ((progress, processDetail) =>
                        {
                            processViewModel.CurrentProcessingDetail = processDetail;
                            var cappedProgress = Math.Min((int)progress, 99);
                            processViewModel.Process = cappedProgress;
                            processViewModel.ProgressText = cappedProgress;
                        });
                        exportController.Execute().GetAwaiter().GetResult();
                        logger.Info($"Finished export archived sites for module [{GlobalInfo.Module}], count [{exchangeDataInfo.SelectionList.Count}], total size [{progressManager.TotalExportSize}], job status [{progressManager.GetFinalJobStatus()}].");
                        return;
                    }
                    logger.Info($"Could not get encrypt export password with export db path [{GlobalInfo.ExportDBFilePath}].");
                }
                else if (GlobalInfo.Module is Module.Teams)
                {
                    logger.Info($"Start export archived sites for module [{GlobalInfo.Module}], count site [{exchangeDataInfo.SelectionList.Count}], location [{exportLocationViewModel.LocationType}].");
                    CreateExportDestination(exportLocationViewModel.ExportLocation);
                    Dictionary<ArchiverSiteBase, List<ArchiverSiteBase>> dicArchiverTeams = GetArchiverTeamsInfos(out var teamsCount, out var commonSiteIndexes);
                    progressManager = new ProgressManager(teamsCount);
                    if (GlobalInfo.EncryptionInfoCache.TryGetValue(GlobalInfo.ExportDBFilePath, out string encryptExportPwd) && !string.IsNullOrEmpty(encryptExportPwd))
                    {
                        var exportOption = new ExportOption()
                        {
                            EncryptedDBPath = GlobalInfo.ExportDBFilePath,
                            ExportLocation = exportLocationViewModel.ExportLocation,
                            EncryptedPassword = encryptExportPwd.ToSecureString(),
                            DataType = (DataType)GlobalInfo.Module,
                            MaxThreadCount = globalConfig.GetSetting<int>(AppSettingKey.MAX_THREAD_COUNT),
                            IsSkipAPData = GlobalInfo.IsSkipAPData,
                            AvepointMappingStorage = GlobalInfo.AvepointMappingStorage,
                            TargetStorage = GlobalInfo.TargetStorage,
                            TargetStorageType = GlobalInfo.TargetStorageType
                        };
                        ExportTeamsController exportController = new ExportTeamsController(dicArchiverTeams, exportOption, progressManager, commonSiteIndexes);
                        progressManager.OverallProgressChanged += ((progress, processDetail) =>
                        {
                            processViewModel.CurrentProcessingDetail = processDetail;
                            var cappedProgress = Math.Min((int)progress, 99);
                            processViewModel.Process = cappedProgress;
                            processViewModel.ProgressText = cappedProgress;
                        });
                        exportController.Execute().GetAwaiter().GetResult();
                        logger.Info($"Finished export archived sites for module [{GlobalInfo.Module}], count [{exchangeDataInfo.SelectionList.Count}], total size [{progressManager.TotalExportSize}], job status [{progressManager.GetFinalJobStatus()}].");
                        return;
                    }
                    logger.Info($"Could not get encrypt export password with export db path [{GlobalInfo.ExportDBFilePath}].");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while execute export process. Exception: {ex}.");
            }
        }

        private Dictionary<ArchiverSiteBase, List<ArchiverSiteBase>> GetArchiverTeamsInfos(out int teamsCount, out List<CommonSiteMasterIndexExportDto> commonSiteIndexes)
        {
            var archiverTeams = exchangeDataInfo.SelectionList.Select(i => (ArchiverSiteBase)i).ToList();
            teamsCount = archiverTeams.Count;
            commonSiteIndexes = dbHelper.GetCommonArchiverSitesBySiteURLs(Module.Teams, archiverTeams.Select(_ => _.SiteUrl).ToList());
            var teamsIndex = commonSiteIndexes;
            var teamsRestoreDoNotArchiverTeams = archiverTeams.Where(_ => !teamsIndex.Any(c => c.SiteURL.Equals(_.GroupAddress))).Select(
                _ => new CommonSiteMasterIndexExportDto
                {
                    SiteId = _.SiteId,
                    SiteURL = _.SiteUrl,
                }).ToList();
            var dicArchiverTeams = new Dictionary<ArchiverSiteBase, List<ArchiverSiteBase>>();
            var archiverSiteObjects = dbHelper.GetArchiverSiteMasterIndexesByGroupAddressAndJobId(archiverTeams.Select(_ => _.GroupAddress).ToList(), commonSiteIndexes.Where(_ => !string.IsNullOrEmpty(_.JobId)).Select(_ => _.JobId).ToList());
            List<ArchiverSiteBase> archiverSites;
            var teamsRestore = commonSiteIndexes;
            teamsRestore.AddRange(teamsRestoreDoNotArchiverTeams);
            foreach (var teams in teamsRestore)
            {
                archiverSites = archiverSiteObjects.Where(_ => (!string.IsNullOrEmpty(_.GroupMailboxAddress) && _.GroupMailboxAddress.Equals(teams.SiteURL)) 
                || (!string.IsNullOrEmpty(_.JobId) && !string.IsNullOrEmpty(teams.JobId) && _.JobId.StartsWith(teams.JobId))).Select(item =>
                    new ArchiverSiteBase
                    {
                        SiteId = item.SiteId,
                        SiteUrl = item.SiteURL,
                        GroupAddress = teams.SiteURL,
                    }).ToList();
                dicArchiverTeams.Add(new ArchiverSiteBase
                {
                    SiteId = teams.SiteId,
                    SiteUrl = teams.SiteURL,
                    GroupAddress = teams.SiteURL,
                }, archiverSites);
            };
            return dicArchiverTeams;
        }

        private void CreateExportDestination(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while creating export folder, destination [{path}]. Exception: {ex}.");
            }
        }

        public async Task RefreshMainProgressBar(int s, int e)
        {
            if (GlobalInfo.TotalExportedSize.Equals(0))
            {
                processViewModel.Process = e;
                processViewModel.ProgressText = e;
                return;
            }

            for (var i = s; i <= e || i >= e; i+=i)
            {
                await Task.Delay(50);
                if (i >= e)
                {
                    processViewModel.Process = e;
                    processViewModel.ProgressText = e;
                    break;
                }
                processViewModel.Process = i;
                processViewModel.ProgressText = i;
            }

        }
    }
}
