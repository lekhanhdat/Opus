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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Exceptions;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Content;
using DataExportCore.Discover;
using DataExportCore.Discover.Node;
using DataExportCore.Export;
using DataExportCore.Utils;
using Merged18NResources.MediaServiceArchiverBackup;
using Storage;

namespace DataExportCore
{
    public class ConversationExportProcessor
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(ExchangeExportProcessor));
        private IndexDatabaseHelper _dbHelper;
        private ArchiverSiteBase _archiverMailBox;
        public ArchiverIndexService? _IndexService { get; set; }
        private string _groupAddress;
        private Reporter _reporter;
        private ExportOption _exportOption;

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor
        {
            get
            {
                if (_IndexService == null)
                {
                    _IndexService = new ArchiverIndexService()
                    {
                        IndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>(),
                        IndexSynchronizer = new IndexDatabaseSynchronizer()
                    };
                    return _IndexService.IndexProcessor;
                }
                else
                {
                    return _IndexService.IndexProcessor;
                }
            }
            set { }
        }

        public ConversationExportProcessor(ArchiverSiteBase archiverSite, IndexDatabaseHelper dbHelper, ExportOption exportOption, Reporter reporter, string groupAddress)
        {
            _archiverMailBox = archiverSite;
            _dbHelper = dbHelper;
            _exportOption = exportOption;
            _reporter = reporter;
            _groupAddress = groupAddress;
        }

        public void Execute()
        {

            try
            {
                logger.Info($"Starting execution process for conversation [{_archiverMailBox.GroupAddress}]");

                var browseInfo = BuildRestoreRequest(_archiverMailBox.GroupAddress, _archiverMailBox.SiteUrl);
                Open(browseInfo);

                //var headCount = (long)IndexProcessor.ExecuteScalar("select COUNT(DISTINCT COL_PATH_MD5) from " + IndexConstants.TableNameArchiveHead + " where COL_TYPE in ('E','W','L','F')", []);
                //var bodyCount = (long)IndexProcessor.ExecuteScalar("select COUNT(DISTINCT COL_PATH_MD5) from " + IndexConstants.TableNameArchiveBody + " where COL_TYPE in ('D','I','A','U','V')", []);
                //logger.Info($"site [{_archiverMailBox.GroupAddress}] has data count: {headCount + bodyCount}");

                //_reporter.StartProgress(headCount + bodyCount);

                ExportQueue<TeamsDiscoveryNode> exportQueue = new ExportQueue<TeamsDiscoveryNode>();
                DiscoverContent(exportQueue);

                var siteExportPath = ExportInternal(_reporter, exportQueue);

                logger.Info($"Execution process completed for conversation [{_archiverMailBox.GroupAddress}]");

                //ExportUtility.AddUploadedTeamsToReport(_archiverMailBox.GroupAddress, I18NEntity.GetString("SATool_ExportPath_ChannelConversations") , siteExportPath, GlobalCache.TargetStorageType, NodeType.Conversation);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred when executing export process for conversation [{_archiverMailBox.GroupAddress}]. Error: {e}");
                throw;
            }
            finally
            {
                IndexProcessor.Close();
            }
        }

        private string ExportInternal(Reporter reporter, ExportQueue<TeamsDiscoveryNode> exportQueue)
        {
            logger.Info($"Starting internal export for [{_archiverMailBox.SiteUrl}] to {(GlobalCache.TargetStorageType == StorageDeviceType.None ? "File System/NetShare" : GlobalCache.TargetStorageType.ToString())}");
            ChannelConversationExportWorkerBase worker;
            switch (GlobalCache.TargetStorageType)
            {
                case StorageDeviceType.CloudAzure:
                case StorageDeviceType.SFTP:
                    worker = new ChannelConversationCloudExportWorker(reporter, exportQueue, _groupAddress);
                    break;
                case StorageDeviceType.None:
                    worker = new ChannelConversationLocalExportWorker(reporter, exportQueue, _groupAddress);
                    break;
                default:
                    logger.Error($"Target storage type is invalid. TargetStorageType: {GlobalCache.TargetStorageType}");
                    throw new Exception("Target storage type is invalid. Please check again");
            }

            try
            {
                var containerPath = worker.Process();
                logger.Info($"Internal export completed successfully for [{_archiverMailBox.SiteUrl}]");
                return containerPath;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred during internal export for site [{_archiverMailBox.SiteUrl}]. Error: {e}");
                throw;
            }
            finally
            {
                worker.Dispose();
            }
        }

        private void DiscoverContent(ExportQueue<TeamsDiscoveryNode> exportQueue)
        {
            try
            {
                logger.Info($"Starting content discovery mailbox for [{_groupAddress}]");

                var discover = new TeamsDiscover(_groupAddress, _dbHelper, IndexProcessor, _archiverMailBox.SiteUrl);
                discover.SetExportQueue(exportQueue);
                var mProcessor = new Thread(new ThreadStart(delegate ()
                {
                    try
                    {
                        discover.Process();
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred during content discovery for mailbox [{_groupAddress}]. Error: {ex}");
                    }
                }))
                {
                    IsBackground = true
                };

                mProcessor.Start();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while starting content discovery for mailbox [{_groupAddress}]. Error: {e}");
                throw;
            }
        }

        private void Open(ArchiverBrowseInfo browseInfo)
        {
            var openParam = new ArchiverIndexServiceOpenParameter(browseInfo, GlobalDeviceCache.CacheManager.CacheSystem, GlobalDeviceCache.IndexDevice)
            {
                WaitIndexLockerTimeOutInMs = 3000,
                IndexDatabaseName = ServiceConstants.IndexDBName
            };

            ArchiverIndexMutex archiverIndexMutex = new ArchiverIndexMutex(openParam.IndexVolume + openParam.IndexDatabaseName);
            var gotLock = archiverIndexMutex.WaitAsync(openParam.WaitIndexLockerTimeOutInMs).GetAwaiter().GetResult();
            if (!gotLock)
            {
                logger.Error($"Failed to get lock to opening archiver index db: {openParam.IndexVolume}");
                throw new OpenIndexDbTimeoutException($"Failed to get lock to open archiver index db: {openParam.IndexVolume}");
            }

            try
            {
                logger.Info("Opening IndexCacheDeviceSystem");
                openParam.IndexCacheDeviceSystem.Open();
                var param = GetIndexProcessorParameter(openParam);
                logger.Info("Opening IndexProcessor");
                IndexProcessor.Open(param);
                logger.Info("IndexProcessor opened successfully.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occured when opening index processor for [{openParam.IndexVolume}]. Error: {e}");
                throw;
            }
            finally
            {
                archiverIndexMutex.Release();
            }
        }

        private ArchiverIndexProcessorParameter GetIndexProcessorParameter(ArchiverIndexServiceOpenParameter openParam)
        {
            try
            {

                logger.Info("Setting up Index Processor parameters");
                if (MediaConfigInfo.CommonConfigInfo == null)
                {
                    MediaConfigInfo.CommonConfigInfo = PlatformWindsorManager.GetService<CommonConfigInfo>();
                }
                IndexDatabaseDownLoadResult indexDownLoadInfo;
                var realIndexDeviceSystem = (openParam.IndexCacheDeviceSystem != null && MediaConfigInfo.CommonConfigInfo.ForceUseCache) ? openParam.IndexCacheDeviceSystem : openParam.IndexLogicalDeviceSystem;
                var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
                if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
                {
                    if (MediaConfigInfo.CommonConfigInfo.ForceUseCache && openParam.IndexCacheDeviceSystem != null)
                    {
                        logger.Info("Downloading index database");
                        var dbInfo = new IndexDatabaseInfo(openParam);
                        indexDownLoadInfo = IndexDownloader.Download(dbInfo, openParam);
                    }
                    else
                        indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
                }
                else
                {
                    if (openParam.IsNeedCreateNewIndex)
                        indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(realIndexDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
                    else throw new IndexCanNotFoundException(MediaServiceArchiverBackupResource.ArchiverIndexServiceOpenIndexCanNotFoundException);
                }
                realIndexDeviceSystem.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
                IdentityManager.IdentityMode = IdentityMode.Process;
                var indexProcessorParameter = new ArchiverIndexProcessorParameter
                {
                    DownLoadResult = indexDownLoadInfo,
                    IndexWorkingSystem = realIndexDeviceSystem,
                    IsNeedCheckIntegrity = openParam.IsNeedCheckIntegrity
                };

                try
                {
                    indexProcessorParameter.DBPassWord = ExportUtility.CustomAesEncryptorWrapper.Decrypt(GlobalCache.MasterKey);
                    logger.Info("Decrypted DB password successfully.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to decrypt DB password. Error: {ex}");
                    throw;
                }

                logger.Info("IndexProcessor parameters setup completed.");
                return indexProcessorParameter;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while setting up IndexProcessor parameters for [{openParam.IndexVolume}]. Error: {e}");
                throw;
            }
        }

        private ArchiverBrowseInfo BuildRestoreRequest(string emailAddress, string siteUrl)
        {
            var volumeParam = new VolumeParameter()
            {
                FarmName = string.Empty,
                EmailAddress = emailAddress,
                SiteCollectionUrl = emailAddress
            };

            ArchiverBrowseInfo browseInfo = new ArchiverBrowseInfo()
            {
                IndexVolume = new TeamsArchiverVolumeGenerator().GenerateIndexVolume(volumeParam),
                Path = siteUrl,
                EndTime = DateTime.MaxValue.Ticks,
                SiteUrl = siteUrl,
            };

            return browseInfo;
        }
    }
}
