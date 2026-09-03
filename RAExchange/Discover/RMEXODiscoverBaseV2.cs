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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.RAExchange.Common;
using ExchangeBackupUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using ExchangeFolder = ExchangeBackupUtility.ExchangeFolder;

namespace AvePoint.RA.RAExchange.Discover;

public class RMEXODiscoverBaseV2
{
    private readonly Contract.Services.IRALogger _logger = RALogger.GetInstance(typeof(RMEXODiscoverBaseV2));
    
    private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

    private const string SUPPORT_GRAPH_API = "EXOJOB_USING_GRAPH_API";

    protected bool IsNullClassification;

    protected ExchangeOnlineTreeNodeDto TreeNodeDto;
    
    protected IExchangeFolder CurrentFolder;
    
    protected ExchangeService Service;
    
    protected string MailboxAddress { get;  set; }
    
    protected bool IsSupportGraphApi { get; set; }
    
    #region Get from config file later
    protected int MaxBackupItemsThreads { get; private set; } = 25;
    protected int MinBackupItemsThreads { get; private set; } = 10;
    protected bool EnableBulkGenerateItems { get; private set; } = true;
    protected int MaxBulkItemsCount { get; private set; } = 50;
    protected int MaxBulkItemSize { get; private set; } = 20;//in MB
    #endregion
    
    /// <summary>
    /// 旧的ID，可能是DAOTreeNodeID，也可能是GUID的AOS MailboxID(经过特殊处理满足Records GUID格式需求的ID)
    /// </summary>
    protected string MailboxGuid { get; private set; }
    /// <summary>
    /// AOS AOS真正的Mailbox Object ID，类型为String
    /// </summary>
    protected string AOSObjectId { get; private set; }
    
    protected RMEXODiscoverBaseV2(ExchangeOnlineTreeNodeDto tree, bool isNullClassification) : this(tree)
    {
        IsNullClassification = isNullClassification;
    }

    protected RMEXODiscoverBaseV2(ExchangeOnlineTreeNodeDto tree)
    {
        TreeNodeDto = tree;
    }
    
    public virtual void Init()
    {
        var supportGraphApi = _keyValueDao.GetValueByKeyAsync(SUPPORT_GRAPH_API).Result;
        TreeNodeDto.UsingModernApp = bool.TryParse(supportGraphApi, out var flag) && flag;
        MailboxAddress  = TreeManagement.GetMailboxNode(TreeNodeDto)?.Name;
        TreeManagement tm = new();
        MailboxGuid = tm.GetRealMailboxGuid(TreeNodeDto);
        IsSupportGraphApi = EXOGraphApiResolver.ShouldUseGraph(_keyValueDao, MailboxAddress, tm.GetRealMailboxStringId(TreeNodeDto), TreeNodeDto);
        CurrentFolder = tm.GetExchangeFolderFromTreeNodeV2(TreeNodeDto, MailboxGuid, IsSupportGraphApi);
        //TODO: REMOVE When using Graph API for Archive Action
        Service = CreateExchangeService(tm);
        InitFromConfig();
        AOSObjectId = tm.GetAOSObjectId(TreeNodeDto);

        if (IsSupportGraphApi)
        {
            this.MaxBackupItemsThreads = _keyValueDao.GetExoGraphDiscoverThreadsLimit();
            _logger.Info($"Graph API is enabled for mailbox {TreeNodeDto.ID}, set MaxBackupItemsThreads to {MaxBackupItemsThreads} based on configuration.");
        }
    }

    private ExchangeService CreateExchangeService(TreeManagement tm)
    {
        // If not support Graph API, we should use EWS Service from CurrentFolder directly
        if (!IsSupportGraphApi && CurrentFolder is ExchangeFolder folder)
        {
            return folder.GetService();
        }

        // If support Graph API, we should get EWS Service from new ExchangeFolder instance
        var ewsFolder = tm.GetExchangeFolderFromTreeNodeV2(TreeNodeDto, MailboxGuid, false) as ExchangeFolder;
        
        return ewsFolder.GetService();
    }
    
    protected virtual IEnumerable<IExchangeFolder> GetFolders(IExchangeFolder folder)
    {
        using var performance = new PerformanceScope("EXO.RMEXODataSync.GetSubFolders", "", true);
        foreach (var f in folder.GetAllSubFolders().Where(f => f.FolderType == "IPF.Note"))
        {
            f.GenerateCurrentSyncState();
            yield return f;
        }
    }

    private void InitFromConfig()
    {
        try
        {
            EnableBulkGenerateItems = bool.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_ENABLE_BULK_GENERATE_ITEMS]);
            MaxBackupItemsThreads = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_DISCOVER_THREADS_LIMIT]);
            MaxBulkItemsCount = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_COUNT_LIMIT]);
            MaxBulkItemSize = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_SIZE_LIMIT]);
        }
        catch (Exception ex)
        {
            _logger.Error($"An exception occurred while trying to get the configuration, reason:{ex.ToString()}. Set the value to default.");
            EnableBulkGenerateItems = true;
            MaxBulkItemsCount = 50;
            MaxBulkItemSize = 20;
        }
    }
}