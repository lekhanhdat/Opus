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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Authorization;
using AvePoint.RA.RAExchange.Common;
using ExchangeBackupUtility;
using ExchangeUtility;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Discover
{
    //Discover  EXO node 的基类。
    //1.提供最底层的Init 方法，用来连接EXO ，实例化Tree 上的当前节点，用于向下discover。(目前支持user 类型和group 类型的mailbox)
    //2.提供虚方法GetFolders， 目前所有功能GetFolders 的行为相同，所以使用虚方法，后期有变化可以放到接口中让各个discover方式自己维护
    //3.提供EXO 对象的基本属性，比如MailboxAddress，TreeNodeDTO, e.g.
    public class RMEXODiscoverBase
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEXODiscoverBase));
        protected ExchangeOnlineTreeNodeDto TreeNodeDto = null;

        //表示当前Tree节点对应的EXO Folder
        protected ExchangeFolder CurrentFolder { get; private set; }
        protected string MailboxAddress { get;  set; }
        /// <summary>
        /// 旧的ID，可能是DAOTreeNodeID，也可能是GUID的AOS MailboxID(经过特殊处理满足Records GUID格式需求的ID)
        /// </summary>
        protected string MailboxGuid { get; private set; }
        /// <summary>
        /// AOS AOS真正的Mailbox Object ID，类型为String
        /// </summary>
        protected string AOSObjectId { get; private set; }


        protected virtual IEnumerable<ExchangeFolder> GetFolders(ExchangeFolder folder)
        {
            using (var performance = new PerformanceScope("EXO.RMEXODataSync.GetSubFolders", "", true))
            {
                foreach (var f in folder.GetAllSubFolders().Where(f => f.FolderType == "IPF.Note"))
                {
                    //在返回Folder 的时候需要计算一下当前Folder 的SyncState，来保证Folder 下次的Inc job 能根据Sync state 进行inc
                    f.GenerateCurrentSyncState();
                    yield return f;
                }
            }
        }

        protected virtual IEnumerable<ExchangeFolder> GetFoldersDeep(ExchangeFolder folder)
        {
            using (var performance = new PerformanceScope("EXO.RMEXODataSync.GetSubFolders", "", true))
            {
                foreach (var f in folder.GetAllSubFoldersDeep().Where(f => f.FolderType == "IPF.Note"))
                {
                    //在返回Folder 的时候需要计算一下当前Folder 的SyncState，来保证Folder 下次的Inc job 能根据Sync state 进行inc
                    f.GenerateCurrentSyncState();
                    yield return f;
                }
            }
        }


        #region Get from config file later
        protected int MaxBackupItemsThreads { get; private set; } = 25;
        protected int MinBackupItemsThreads { get; private set; } = 10;
        protected bool EnableBulkGenerateItems { get; private set; } = true;
        protected int MaxBulkItemsCount { get; private set; } = 50;
        protected int MaxBulkItemSize { get; private set; } = 20;//in MB
        #endregion

        //目前只提供structure injection 方式，如果需要逻辑变化，可以考虑setter injection
        public RMEXODiscoverBase(ExchangeOnlineTreeNodeDto tree)
        {
            TreeNodeDto = tree;
        }

        public virtual void Init()
        {
            MailboxAddress  = TreeManagement.GetMailboxNode(TreeNodeDto)?.Name;
            TreeManagement tm = new TreeManagement();
            CurrentFolder = tm.GetExchangeFolderFromTreeNode(TreeNodeDto);
            InitFromConfig();
            //ExchangeMailbox mailbox = new ExchangeMailbox(MailboxAddress, AuthorizationManager.Instance.GetAuthObject(MailboxAddress));
            //MailboxGuid = mailbox.GetRealMailboxGuid();
            MailboxGuid = tm.GetRealMailboxGuid(TreeNodeDto);
            AOSObjectId = tm.GetAOSObjectId(TreeNodeDto);
        }

        private void InitFromConfig()
        {
            try
            {
                this.EnableBulkGenerateItems = bool.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_ENABLE_BULK_GENERATE_ITEMS]);
                this.MaxBackupItemsThreads = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_DISCOVER_THREADS_LIMIT]);
                this.MaxBulkItemsCount = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_COUNT_LIMIT]);
                this.MaxBulkItemSize = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_SIZE_LIMIT]);
            }
            catch (Exception ex)
            {
                logger.Error($"An exception occurred while trying to get the configuration, reason:{ex.ToString()}. Set the value to default.");
                //this.MaxRestoreItemsThreads = 2;
                //this.MinRestoreItemsThreads = 1;
                //this.MaxTotalSizeOnDownload = 20;
                this.EnableBulkGenerateItems = true;
                this.MaxBulkItemsCount = 50;
                this.MaxBulkItemSize = 20;
                //this.SetApplicationImpersonation = true;
                //this.EWSMonitorMode = 3;
                //this.EWSMonitorInterval = 300;
            }
        }
    }
}
