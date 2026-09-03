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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using AvePoint.Wrapper.Common;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Microsoft.Azure.Cosmos;
using System.Diagnostics;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    internal class ExchangeMoveToController : IBackupController, IDisposable
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExchangeExportController));
        private IExplorerDao _explorerDao;
        protected IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao(true);
                }
                return _explorerDao;
            }
        }

        private IRecordsHistoryService mRecordsHistoryService = null;
        public IRecordsHistoryService RecordsHistoryService
        {
            get
            {
                if (mRecordsHistoryService == null)
                {
                    mRecordsHistoryService = (IRecordsHistoryService)PlatformWindsorManager.GetService(typeof(IRecordsHistoryService));
                }
                return mRecordsHistoryService;
            }
        }
        public static readonly string Separator = "|I18NSplit|";
        protected EXOConfiguration config = null;
        protected Guid WellKnowTermColumnGuid = new Guid("AA44DC13-6491-40C8-8C4C-5FE81370EFF3");
        protected int WellKnowTermColumnId = 0xF666;
        protected EXOMoveItemRestore restore = new EXOMoveItemRestore();
        public ExchangeMoveToController(EXOConfiguration configuration)
        {
            config = configuration;
        }
        public virtual void Process(EXOArchiveData node)
        {
            Stopwatch stopwatchForMove = new Stopwatch();
            stopwatchForMove.Start();
            logger.Info($"start move email:{node.ItemId}");
            string comment = string.Empty;
            string errorMessage = string.Empty;
            string destUrl = string.Empty;
            bool deleteRecord = false;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            bool skip = false;
            int contentSize = 0;

            if (config != null && config.CurrentRule != null && config.CurrentRule.EXORule != null && config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting != null)
            {
                string exportPath = string.Empty;
                string itemName = string.Empty;
                string msgFileName = string.Empty;
                Item EXOItem = null;
                try
                {
                    bool keepClassification = config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.KeepSourceClassification;
                    bool deleteSourceItem = config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.DeleteSourceItem;
                    EXOItem = Item.Bind(config.service, new ItemId(node.ItemId)).GetAwaiter().GetResult();
                    itemName = EXOItem?.Subject;
                    if (EXOItem != null)
                    {
                        contentSize = EXOItem.Size;
                    }
                    Guid desOldRecordID = Guid.Empty;
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    switch (EXOItem.ItemClass)
                    {
                        case "IPM.Schedule.Meeting.Canceled":
                        case "IPM.Schedule.Meeting.Request":
                        case "IPM.Schedule.Meeting.Resp.Neg":
                        case "IPM.Schedule.Meeting.Resp.Pos":
                        case "IPM.Schedule.Meeting.Resp.Tent":
                            logger.Warn($"Exchange Export skip.Name{itemName}.Path:{node.FullPath}.ItemClass:{EXOItem.ItemClass}.");
                            errorMessage = "StorageOptimization_EXOMoveAndExportSkip";
                            status = JobDetailsStatus.Skipped;
                            return;
                        default:
                            break;
                    }
                    logger.Info($"start export email:{EXOItem?.Id?.ToString()}");
                    using (EXOMoveItemExport exporter = new EXOMoveItemExport())
                    {
                        exportPath = exporter.ExportEXOItem(config.SubJobId, EXOItem, config.service);
                    }
                    logger.Info($"finish export email:{EXOItem?.Id.ToString()},cost:{stopwatch.ElapsedMilliseconds}");
                    stopwatch.Stop();
                    EXOMoveItemImport importor = null;
                    if (!string.IsNullOrWhiteSpace(exportPath))
                    {
                        using (var performance = new PerformanceScope("ExchangeMoveToController.MoveItem", "", true))
                        {
                            try
                            {
                                Stopwatch stopwatchForRestoreParent = new Stopwatch();
                                stopwatchForRestoreParent.Start();
                                logger.Info($"start restore parent email:{EXOItem.Id.ToString()}");
                                restore.Init(config, false);
                                restore.RestoreParentInfo(config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url, node.ItemProperties);
                                logger.Info($"finish restore parent,cost:{stopwatchForRestoreParent.ElapsedMilliseconds}");
                                stopwatchForRestoreParent.Stop();

                                string fileName = ArchiverCommonStaticMethod.EscapeName(config.EXOInvalidCharacterMapping, EXOItem?.Subject + ".msg");
                                importor = new EXOMoveItemImport(restore.aveSPFolder, restore.Record, fileName);
                                desOldRecordID = config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution != ContentConflictResolution.Append ? importor.GetDesExistFileRecordID() : Guid.Empty;
                                Stopwatch stopwatchForRestore = new Stopwatch();
                                stopwatchForRestore.Start();
                                logger.Info($"start restore email:{EXOItem.Id.ToString()}");
                                msgFileName = importor.ImportAveEXOItem(exportPath, config, node.ItemProperties);
                                logger.Info($"finish restore email:{EXOItem.Id.ToString()},cost:{stopwatchForRestore.ElapsedMilliseconds}");
                                stopwatchForRestore.Stop();
                                errorMessage = importor.ErrorMessage;
                            }
                            #region version exception
                            catch (ConetentSkipException contentExp)
                            {
                                skip = true;
                                status = JobDetailsStatus.Skipped;
                                errorMessage = contentExp.Message;
                                logger.Info("Content Skip: FileName: {0}", EXOItem?.Id?.ToString());
                            }
                            //File length exceed 128 catch exception
                            catch (PathTooLongException e)
                            {
                                logger.Warn(string.Format("Filename or list URL too long. Reason: {0}.", e.ToString()));
                                throw;
                            }
                            catch (SkipException)
                            {
                                logger.Warn("Content Type Or Column Conflict,Skip Current Node: {0}", EXOItem?.Id?.ToString());
                                throw;
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error in Move to Destination Library," + ex.ToString());
                                throw;
                            }
                            #endregion
                            #region Declare & link xml.
                            Stopwatch deletWatch = new Stopwatch();
                            deletWatch.Start();
                            logger.Info($"start delete email:{EXOItem.Id.ToString()}");
                            Record desRecord = new Record();
                            if (status != JobDetailsStatus.Skipped)
                            {
                                desRecord = restore.GetDesFileRecord(msgFileName);
                                destUrl = desRecord.FullPath;
                            }
                            #endregion

                            string termId = string.Empty;
                            if (status != JobDetailsStatus.Skipped && keepClassification)
                            {
                                termId = GetExoTermId(EXOItem);
                            }
                            #region Delete
                            //If content not skip ,delete source file, control in GUI?
                            if (!skip && deleteSourceItem)
                            {
                                //Delete Source File
                                DeleteSourceEXOItem(EXOItem);
                            }
                            logger.Info($"finish delete email,cost:{deletWatch.ElapsedMilliseconds}");
                            deletWatch.Stop();
                            //keep source, delete records in archiver table
                            if (!skip && !deleteSourceItem)
                            {
                                deleteRecord = true;
                            }

                            #endregion

                            if (status != JobDetailsStatus.Skipped)
                            {
                                bool useDestinationTerm = true;
                                Guid recordID = AvePoint.RA.RAExchange.Common.IDGenerator.GetRecordId(config.ExchangeNodeName, node.ItemId);
                                if (keepClassification && desRecord.SourceFlag == (int)SourceFlag.SharePoint)
                                {
                                    if (!string.IsNullOrWhiteSpace(termId))
                                    {
                                        errorMessage = importor.UpdateBCSColumn(config, new Guid(termId));
                                        if (!string.IsNullOrWhiteSpace(errorMessage))
                                        {
                                            status = JobDetailsStatus.Failed;
                                        }
                                        else
                                        {
                                            desRecord.TermId = new Guid(termId);
                                            useDestinationTerm = false;
                                        }
                                    }
                                    else
                                    {
                                        logger.Info("Source file doesn't have term id.");
                                    }
                                }
                                UpdateMoveActionExploreDB(recordID, desOldRecordID, desRecord, deleteSourceItem, keepClassification, useDestinationTerm, node.FullPath);
                                if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage!= "RM_ExoMoveToSP_ExoCol_ErrorMessage")
                                {
                                    throw new Exception(errorMessage);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("StorageOptimization_SOARRecordManagerEXOCreateMSGFailed");
                    }
                    if (importor != null)
                    {
                        try
                        {
                            importor.Dispose();
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.ToString());
                        }
                    }
                }
                catch (Exception exception)
                {
                    //config.JobReportDtoV2.AddSummaryComments(ReportAction.Move, "StorageOptimization13_SOARSORecordManagerErrorComment");
                    errorMessage = exception.Message;
                    logger.Error($"Error occurred while moving mail. FullPath:{node.FullPath} Error:{exception.ToString()}. ItemClass:{EXOItem.ItemClass}.");

                    if (errorMessage.Contains("StorageOptimization_SOARRecordManagerEXONotInSameTermScope"))
                    {
                        logger.Error("the email was moved successfully and only failed to retain the source classification, mark as Exception");
                        status = JobDetailsStatus.Exception;
                    }
                    else if (exception is SkipException)
                    {
                        status = JobDetailsStatus.Failed;
                    }
                    else if (exception is PathTooLongException)
                    {
                        status = JobDetailsStatus.Failed;
                        errorMessage = "StorageOptimization_SOARRecordManagerFileNameTooLong";
                        logger.Error("Error in Record Manager Job,Item Name : {0},Reason: {1}", itemName, exception.ToString());
                    }
                    else
                    {
                        logger.Error("Error in Record Manager Job,Item Name : {0},Reason: {1}", itemName, exception.ToString());
                        status = JobDetailsStatus.Failed;
                    }
                    throw;
                }
                finally
                {
                    if (!string.IsNullOrWhiteSpace(exportPath))
                    {
                        DeleteTempFile(exportPath);
                    }
                    EXOCommonUtil.AddDetail(EXOItem, node.FullPath, config.RuleName, destUrl, status, "RM_EXODisposal_Action_Move", errorMessage);
                    logger.Info($"finish move email:{node.ItemId},cost:{stopwatchForMove.ElapsedMilliseconds}");
                    stopwatchForMove.Stop();
                }
            }
        }
        public void Finish()
        {
            logger.Info("MoveTo action finished");
        }

        private string GetExoTermId(Item EXOItem)
        {
            ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(WellKnowTermColumnGuid, WellKnowTermColumnId, MapiPropertyType.String);
            return LoadExtendProperties(EXOItem, extendedPropertyDefinition).FirstOrDefault().Value;
        }

        private Dictionary<PropertyDefinitionBase, string> LoadExtendProperties(Item EXOItem, params PropertyDefinitionBase[] definitions)
        {
            Dictionary<PropertyDefinitionBase, string> dictionary = new Dictionary<PropertyDefinitionBase, string>();
            try
            {
                PropertySet propertySet = new PropertySet(BasePropertySet.FirstClassProperties, definitions);
                Item item = Item.Bind(config.service, EXOItem.Id, propertySet).GetAwaiter().GetResult();
                for (int i = 0; i < definitions.Length; i++)
                {
                    PropertyDefinitionBase propertyDefinitionBase = definitions[i];
                    string value;
                    item.TryGetProperty<string>(propertyDefinitionBase, out value);
                    dictionary[propertyDefinitionBase] = value;
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error in load item properties, reason : {0}", ex.ToString()), Array.Empty<object>());
            }
            return dictionary;
        }
        protected void DeleteTempFile(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while deleting msg file. Path:{path} Error:{e.ToString()}");
            }
        }


        protected void UpdateMoveActionExploreDB(Guid sourceRecordID, Guid desOldRecordID, Record desRecord, bool isSourceDeleted, bool keepClassification, bool useDestinationTerm, string nodeFullPath)
        {
            using (var performance = new PerformanceScope("ExchangeMoveToController.UpdateMoveActionExploreDB", "", true))
            {
                //SP Move 源端文件不一定在ExplorerDB存在，需要先判断源端在不在ExplorerDB中.
                //目的端是SP，NodeID需要记录Move之前的File ID.
                Guid sourceSiteID = Guid.Empty;
                Record sourceRecord = ExplorerDao.ReadById(new Guid(config.MailboxRealGuid), sourceRecordID);
                if (sourceRecord == null)
                {
                    //sourceRecord = ExplorerDao.ReadById(new Guid(config.DAOMailBoxTreeNodeID), sourceRecordID);
                    //if (sourceRecord != null)
                    //{
                    //    sourceSiteID = new Guid(config.DAOMailBoxTreeNodeID);
                    //}
                }
                else
                {
                    sourceSiteID = new Guid(config.MailboxRealGuid);
                }


                if (sourceRecord != null)
                {
                    desRecord = CopySourceNonSPRecordPropertyToDesRecord(desRecord, sourceRecord, keepClassification, useDestinationTerm, nodeFullPath);
                    //desRecord.SourceFlag = (int)PhysicalCore.Contract.SourceFlag.SharePoint;
                    desRecord.ContainerId = config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.ContainerId;
                    desRecord.AppendMetaInfoForMovedData();
                    if (ExplorerDao.ReadById(desRecord.ScopeId, desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID) != null)
                    {
                        //源端和目的端都存在，则添加新的记录到ExploreDB中，删除源端和目的端原有记录，由于源端和目的端RecordID已经变了，不能再次使用。
                        ExplorerDao.Add(desRecord);
                        //RECO-3552 对于Move 操作，Report 要求将原端数据更新成 4 = Moved 状态，不进行删除Explorer 数据操作
                        if (isSourceDeleted)
                        {
                            var rec = ExplorerDao.QueryAll(s => s.ScopeId == sourceSiteID && s.Id == sourceRecordID).FirstOrDefault();
                            if (rec != null)
                            {
                                ExplorerDao.UpdateAll(s => s.ScopeId == sourceSiteID && s.Id == sourceRecordID && s.RecordStatus == 1, r => { r.RecordStatus = 4; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                            }
                        }
                        //for  exchange move to sp ,change records status to 1 for dashboard calculate.
                        ExplorerDao.UpdateAll(s => s.ScopeId == desRecord.ScopeId && s.Id == (desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID), r => { r.RecordStatus = 5; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                        if (desRecord.HoldStatus)
                        {
                            //if (isSourceDeleted)
                            //{
                            //    RecordsDBOperation.UpdateRMRecordAlliancesTableRecordsId(sourceRecordID, desRecord.Id);
                            //}
                            //else
                            //{
                            //    RecordsDBOperation.ApplySourceHoldInfoForDestination(sourceRecordID, desRecord.Id);
                            //}
                        }
                    }
                    else
                    {
                        try
                        {
                            ExplorerDao.Add(desRecord);
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null && ex.InnerException is CosmosException && ((CosmosException)ex.InnerException).StatusCode == System.Net.HttpStatusCode.Conflict)
                            {
                                logger.Info($"Item already exists in cosmos db. Id:{desRecord.Id}");
                            }
                            else
                            {
                                throw;
                            }
                        }
                        //RECO-3552 对于Move 操作，Report 要求将原端数据更新成 4 = Moved 状态，不进行删除Explorer 数据操作
                        if (isSourceDeleted)
                        {
                            var rec = ExplorerDao.QueryAll(s => s.ScopeId == sourceSiteID && s.Id == sourceRecordID).FirstOrDefault();
                            if (rec != null)
                            {
                                ExplorerDao.UpdateAll(s => s.ScopeId == sourceSiteID && s.Id == sourceRecordID && s.RecordStatus == 1, r => { r.RecordStatus = 4; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                            }

                        }
                        if (desRecord.HoldStatus)
                        {
                            //if (isSourceDeleted)
                            //{
                            //    RecordsDBOperation.UpdateRMRecordAlliancesTableRecordsId(sourceRecordID, desRecord.Id);
                            //}
                            //else
                            //{
                            //    RecordsDBOperation.ApplySourceHoldInfoForDestination(sourceRecordID, desRecord.Id);
                            //}
                        }
                    }
                }
                else
                {
                    if (ExplorerDao.ReadById(desRecord.ScopeId, desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID) != null)
                    {
                        //Configuration.explorerDao.Delete(desRecord.ScopeId, desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID);
                        //for  exchange move to sp ,change records status to 1 for dashboard calculate.
                        ExplorerDao.UpdateAll(s => s.ScopeId == desRecord.ScopeId && s.Id == (desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID), r => { r.RecordStatus = 5; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                    }
                    else
                    {
                        // don't do anything.
                    }
                }
            }
        }
        private Record CopySourceNonSPRecordPropertyToDesRecord(Record desRecord, Record sourceRecord, bool keepClassification, bool useDestinationTerm, string nodeFullPath)
        {
            #region Copy Source non SP Record property to Des Record
            desRecord.CollectTime = DateTime.UtcNow.Ticks;
            desRecord.CreateDate = sourceRecord.CreateDate;
            desRecord.DeclaredBy = sourceRecord.DeclaredBy;
            desRecord.DestroyedTime = sourceRecord.DestroyedTime;
            desRecord.DisposalDueDate = 0;
            desRecord.ExtensionForFile = sourceRecord.ExtensionForFile;
            desRecord.Extsion1 = sourceRecord.Extsion1;
            desRecord.HoldBy = sourceRecord.HoldBy;
            desRecord.HoldId = sourceRecord.HoldId;
            desRecord.HoldReleaseTime = sourceRecord.HoldReleaseTime;
            desRecord.HoldStatus = sourceRecord.HoldStatus;
            desRecord.HoldUntilTimes = sourceRecord.HoldUntilTimes;
            desRecord.HoldByUsers = sourceRecord.HoldByUsers;
            desRecord.AppendHolds_Array = sourceRecord.AppendHolds_Array;
            desRecord.MetaInfo = sourceRecord.MetaInfo;
            //RECO - 3615, RECO-3616 当前版本，Move行为仍然不去管所有属性，依赖后期data sync行为。所以create by modified by 还从sourceRecord 获取。
            desRecord.ModifiedBy = sourceRecord.ModifiedBy;
            desRecord.CreatedBy = sourceRecord.CreatedBy;
            //desRecord.NodeType = sourceRecord.NodeType;

            AddRecordHistory(desRecord, sourceRecord, nodeFullPath);
            desRecord.RecordOwner = sourceRecord.RecordOwner;
            desRecord.RecordsId = sourceRecord.RecordsId;
            desRecord.RecordStatus = sourceRecord.RecordStatus;
            desRecord.RelatedRecords = sourceRecord.RelatedRecords;
            desRecord.RelatedRecordsCount = sourceRecord.RelatedRecordsCount;
            desRecord.RuleId = Guid.Empty;
            desRecord.RuleLevel = 0;
            //desRecord.SourceFlag = sourceRecord.SourceFlag;
            if (desRecord.SourceFlag == (int)SourceFlag.SharePoint && !useDestinationTerm)
            {
                desRecord.TermName = sourceRecord.TermName;
            }
            if (desRecord.SourceFlag == (int)SourceFlag.OneDrive)
            {
                if (keepClassification)
                {
                    desRecord.TermId = sourceRecord.TermId;
                }
                else
                {
                    desRecord.TermId = Guid.Empty;
                    desRecord.TermName = string.Empty;
                }
            }
            else
            {
                if (desRecord.TermId == Guid.Empty)
                {
                    desRecord.TermName = string.Empty;
                }
            }
            #endregion
            return desRecord;
        }
        private string AddRecordHistory(Record desRecord, Record sourceRecord, string nodeFullPath)
        {
            string recordHistory = string.Empty;
            try
            {
                RecordsHistoryService.AddRecordsHistory(new List<Guid> { sourceRecord.Id }, $"RM_Explorer_RecordHistorySuccessfulInformation{Separator}{nodeFullPath}{Separator}{desRecord.FullPath}");
                RecordsHistoryService.CloneMoveHistoryRecords(sourceRecord.Id, desRecord.Id);
            }
            catch (Exception ex)
            {
                recordHistory = string.Empty;
                logger.Info(string.Format("GetRecordHistory Error:{0}.", ex.ToString()));
            }
            return recordHistory;
        }
        private void DeleteSourceEXOItem(Item item)
        {
            try
            {
                item.Delete(DeleteMode.HardDelete);
            }
            catch (Exception ex)
            {
                logger.Error("Cannot delete the item, item Subject : {1}, reason : {0}.", ex.ToString(), item?.Id?.ToString() ?? string.Empty);
                throw;
            }
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
        }
    }
}
