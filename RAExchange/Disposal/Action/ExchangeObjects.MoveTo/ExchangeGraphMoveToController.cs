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
using System.Diagnostics;
using System.IO;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using ExchangeBackupUtility.Graph;
using ExchangeUtility.Graph;

namespace AvePoint.RA.RAExchange.Disposal.Action;

internal class ExchangeGraphMoveToController(EXOConfiguration configuration) : ExchangeMoveToController(configuration), IBackupController
{
    private readonly IRALogger _logger = RALogger.GetInstance(typeof(ExchangeGraphMoveToController));
    
    public override void Process(EXOArchiveData node)
    {
        Stopwatch stopwatchForMove = new();
        stopwatchForMove.Start();
        _logger.Info($"start move email by graph:{node.ItemId}");
        ExchangeGraphMoveItemUtil util = new();
        if (config is { CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting: not null })
        {
            util.KeepClassification =
                config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.KeepSourceClassification;
            util.DeleteSourceItem = config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.DeleteSourceItem;
            IExchangeItem item = null;
            var itemName = util.ItemName;
            var exportPath = util.ExportPath;
            try
            {
        
                BindToEXOItem(ref item, node, util);

                if (util.Status == JobDetailsStatus.Skipped) return;

                if (!string.IsNullOrWhiteSpace(util.ExportPath))
                {
                    using var performance = new PerformanceScope("ExchangeGraphMoveToController.MoveItem", "", true);
                    RestoreDocument(item, node, util);

                    DeleteFile(item, util);

                    UpdateCosmosRecord(node, util);
                }
                else
                {
                    throw new Exception("StorageOptimization_SOARRecordManagerEXOCreateMSGFailed");
                }

                if (util.Importer != null)
                {
                    try
                    {
                        util.Importer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                //config.JobReportDtoV2.AddSummaryComments(ReportAction.Move, "StorageOptimization13_SOARSORecordManagerErrorComment");
                util.ErrorMessage = exception.Message;
                _logger.Error(
                    $"Error occurred while moving mail. Id:{node.ItemId} Error:{exception.ToString()}. ItemClass:{item.ItemType}.");
                if (util.ErrorMessage.Contains("StorageOptimization_SOARRecordManagerEXONotInSameTermScope"))
                {
                    _logger.Error(
                        "the email was moved successfully and only failed to retain the source classification, mark as Exception");
                    util.Status = JobDetailsStatus.Exception;
                }
                else if (exception is SkipException)
                {
                    util.Status = JobDetailsStatus.Failed;
                }
                else if (exception is PathTooLongException)
                {
                    util.Status = JobDetailsStatus.Failed;
                    util.ErrorMessage = "StorageOptimization_SOARRecordManagerFileNameTooLong";
                    _logger.Error("Error in Record Manager Job,Item Name : {0},Reason: {1}", itemName,
                        exception.ToString());
                }
                else
                {
                    _logger.Error("Error in Record Manager Job,Item Name : {0},Reason: {1}", itemName,
                        exception.ToString());
                    util.Status = JobDetailsStatus.Failed;
                }

                throw;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(exportPath))
                {
                    DeleteTempFile(exportPath);
                }

                EXOCommonUtil.AddDetail(item, node.FullPath, config.RuleName, util.DestUrl, util.Status, "RM_EXODisposal_Action_Move", util.ErrorMessage);
                _logger.Info($"finish move email by graph:{node.ItemId},cost:{stopwatchForMove.ElapsedMilliseconds}");
                stopwatchForMove.Stop();
            }
        }
    }

    private void BindToEXOItem(ref IExchangeItem item, EXOArchiveData node, ExchangeGraphMoveItemUtil util)
    {
        var authObject = AuthorizationManager.Instance.GetAuthObjectForGraph(config.ExchangeNodeName);
        item = ExchangeFactoryProvider.Create(true).CreateItem(config.MailboxId, node.ItemId, node.ParentFolderId, authObject);
        util.ItemName = item?.ItemName;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        switch (item?.ItemType)
        {
            case "IPM.Schedule.Meeting.Canceled":
            case "IPM.Schedule.Meeting.Request":
            case "IPM.Schedule.Meeting.Resp.Neg":
            case "IPM.Schedule.Meeting.Resp.Pos":
            case "IPM.Schedule.Meeting.Resp.Tent":
                _logger.Warn(
                    $"Exchange Export skip.Name{util.ItemName}.Path:{node.FullPath}.ItemClass:{item.ItemType}.");
                util.ErrorMessage = "StorageOptimization_EXOMoveAndExportSkip";
                util.Status = JobDetailsStatus.Skipped;
                return;
            default:
                break;
        }

        _logger.Info($"start export email:{item?.ItemId}");
        using (EXOMoveItemExport exporter = new())
        {
            util.ExportPath = exporter.ExportEXOItem(config.SubJobId, item).GetAwaiter().GetResult();
        }

        _logger.Info($"finish export email:{item?.ItemId},cost:{stopwatch.ElapsedMilliseconds}");
        stopwatch.Stop();
    }

    private void UpdateCosmosRecord(EXOArchiveData node, ExchangeGraphMoveItemUtil util)
    {
        var desRecord = util.DesRecord;
        var errorMessage = util.ErrorMessage;
        if (util.Status != JobDetailsStatus.Skipped)
        {
            bool useDestinationTerm = true;
            Guid recordId =
                AvePoint.RA.RAExchange.Common.IDGenerator.GetRecordId(config.ExchangeNodeName, node.ItemId);
            if (util.KeepClassification && desRecord.SourceFlag == (int)SourceFlag.SharePoint)
            {
                if (!string.IsNullOrWhiteSpace(util.TermId))
                {
                    errorMessage = util.Importer.UpdateBCSColumn(config, new Guid(util.TermId));
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        util.Status = JobDetailsStatus.Failed;
                    }
                    else
                    {
                        desRecord.TermId = new Guid(util.TermId);
                        useDestinationTerm = false;
                    }
                }
                else
                {
                    _logger.Info("Source file doesn't have term id.");
                }
            }

            UpdateMoveActionExploreDB(recordId, util.DesOldRecordId, desRecord, util.DeleteSourceItem,
                util.KeepClassification, useDestinationTerm, node.FullPath);
            if (!string.IsNullOrWhiteSpace(errorMessage) &&
                errorMessage != "RM_ExoMoveToSP_ExoCol_ErrorMessage")
            {
                throw new Exception(errorMessage);
            }
            util.ErrorMessage = errorMessage;
        }
    }

    private void DeleteFile(IExchangeItem item, ExchangeGraphMoveItemUtil util)
    {
        Stopwatch deleteWatch = new();
        deleteWatch.Start();
        _logger.Info($"start delete email:{item.ItemId.ToString()}");
        
        util.DesRecord = new Record();
        if (util.Status != JobDetailsStatus.Skipped)
        {
            util.DesRecord = restore.GetDesFileRecord(util.MsgFileName);
            util.DestUrl = util.DesRecord.FullPath;
        }
        
        if (util.Status != JobDetailsStatus.Skipped && util.KeepClassification)
        {
            item.TryGetExtendProperty(ExtendProperty.Term, out var term);
            util.TermId = term;
        }

        //If content not skip ,delete source file, control in GUI?
        if (!util.Skip && util.DeleteSourceItem)
        {
            //Delete Source File
            DeleteSourceEXOItem(item);
        }

        _logger.Info($"finish delete email,cost:{deleteWatch.ElapsedMilliseconds}");
        deleteWatch.Stop();
    }

    private void RestoreDocument(IExchangeItem item, EXOArchiveData node, ExchangeGraphMoveItemUtil util)
    {
        try
        {
            Stopwatch stopwatchForRestoreParent = new Stopwatch();
            stopwatchForRestoreParent.Start();
            _logger.Info($"start restore parent email:{item?.ItemId}");
            restore.Init(config, false);
            restore.RestoreParentInfo(
                config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url,
                node.ItemProperties);
            _logger.Info($"finish restore parent,cost:{stopwatchForRestoreParent.ElapsedMilliseconds}");
            stopwatchForRestoreParent.Stop();

            string fileName = ArchiverCommonStaticMethod.EscapeName(config.EXOInvalidCharacterMapping,
                item?.ItemName + ".msg");
            util.Importer = new EXOMoveItemImport(restore.aveSPFolder, restore.Record, fileName);
            util.DesOldRecordId =
                config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution !=
                ContentConflictResolution.Append
                    ? util.Importer .GetDesExistFileRecordID()
                    : Guid.Empty;
            Stopwatch stopwatchForRestore = new();
            stopwatchForRestore.Start();
            _logger.Info($"start restore email:{item?.ItemId}");
            util.MsgFileName = util.Importer .ImportAveEXOItem(util.ExportPath, config, node.ItemProperties);
            _logger.Info(
                $"finish restore email:{item?.ItemId},cost:{stopwatchForRestore.ElapsedMilliseconds}");
            stopwatchForRestore.Stop();
            util.ErrorMessage = util.Importer.ErrorMessage;
        }

        #region version exception

        catch (ConetentSkipException contentExp)
        {
            util.Skip = true;
            util.Status = JobDetailsStatus.Skipped;
            util.ErrorMessage = contentExp.Message;
            _logger.Info("Content Skip: FileName: {0}", item?.ItemId);
        }
        //File length exceed 128 catch exception
        catch (PathTooLongException e)
        {
            _logger.Warn($"Filename or list URL too long. Reason: {e.ToString()}.");
            throw;
        }
        catch (SkipException)
        {
            _logger.Warn("Content Type Or Column Conflict,Skip Current Node: {0}", item?.ItemId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error("Error in Move to Destination Library," + ex.ToString());
            throw;
        }

        #endregion
    }

    private void DeleteSourceEXOItem(IExchangeItem item)
    {
        try
        {
            item.DeleteAsync(true).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.Error("Cannot delete the item, item Subject : {1}, reason : {0}.", ex.ToString(), item?.ItemId ?? string.Empty);
            throw;
        }
    }

    public void Finish()
    {
        _logger.Info("MoveTo action finished");
    }
}