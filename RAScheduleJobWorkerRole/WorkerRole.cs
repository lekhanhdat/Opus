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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.SharePoint.Common;
using RAGlobalSearch.Common;
using RAScheduleJobWorkerRole;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace AvePoint.RA.ScheduleJobWorkerRole
{
    public class WorkerRole
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IRealTimeQueueService realTimeQueueService;

        protected IRealTimeQueueService RealTimeQueueService
        {
            get
            {
                if (realTimeQueueService == null)
                {
                    realTimeQueueService = (IRealTimeQueueService)PlatformWindsorManager.GetService(typeof(IRealTimeQueueService));
                }
                return realTimeQueueService;
            }
        }

        protected IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        
        public void Run()
        {
            logger.Info("enter worker role running");

            try
            {
                while (true)
                {
                    try
                    {
                        if (RealTimeQueueCounter.CanEnter())
                        {
                            var msg = RealTimeQueueService.GetMessage();
                            ProcessRealTimeQueueMessage(msg);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("get real time queue message error:{0}", e.ToString());
                        Thread.Sleep(5000);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while worker running {e}");
            }
            
            logger.Info("worker role is stopped.");
        }

        private void ProcessRealTimeQueueMessage(RecordsRealTimeMessage msg)
        {
            RealTimeQueueCounter.Increse();

            AveThread t = new AveThread(async () => {
                try
                {
                    //AvePoint.Wrapper.Common.WrapperConfiguration.EnableDownloadLATData = false;
                    TenantLocalValue.LogonGroupId = msg.LogonGroupId;
                    TenantLocalValue.LogonUserEmail = msg.CurrentUserName;
                    ClientRequestLocalValue.ClientIP = msg.ClientIP;
                    logger.Info($"Try to process real time action message. LogonGroupId: {msg.LogonGroupId}, Action: {msg.Action.ToString()}, job id: {msg.JobId}");
                    //RMDBInitializer.UpgradeTenantDBModelOnly();
                    RecordsReturnMessage returnMessage;
                    if (msg.Action == Contract.Object.RealTime.RealTimeAction.ChangeTerm)
                    {
                        returnMessage = await ExplorerService.ChangeTermRealTimeAllSourceAsync(msg.ChangeTermOption, msg.JobId);
                    }
                    else if (msg.Action == Contract.Object.RealTime.RealTimeAction.Declare || msg.Action == Contract.Object.RealTime.RealTimeAction.UnDeclare)
                    {
                        //declare or undeclare in SP 
                        if (msg.Action == Contract.Object.RealTime.RealTimeAction.Declare)
                        {
                            returnMessage = await ExplorerService.DeclareAsRecordRealTimeAsync(msg.RecordIds, msg.JobId, msg.DeclareBy);
                        }
                        else
                        {
                            returnMessage = await ExplorerService.UndeclareAsRecordRealTimeAsync(msg.RecordIds, msg.JobId, msg.DeclareBy);
                        }
                    }
                    else if (msg.Action == Contract.Object.RealTime.RealTimeAction.PhysicalMove)
                    {
                        returnMessage = await ExplorerService.PhysicalExplorerMoveRealTimeAsync(msg.PhysicalMoveOption, msg.JobId);
                    }
                    else if (msg.Action == RealTimeAction.PhysicalMoveRequest)
                    {
                        RecordsReturnMessage messageResult = new RecordsReturnMessage
                        {
                            ResultType = ResultType.Success,
                        };
                        foreach (var moveRequest in msg.PhysicalMoveRequests)
                        {
                            var message = await ExplorerService.PhysicalExplorerMoveRealTimeAsync(moveRequest.PhysicalMoveOption, msg.JobId, moveRequest.GroupRequestId);
                            if (messageResult.ResultType == ResultType.Failed)
                            {
                                messageResult.FailedIds.AddRange(message.FailedIds);
                                messageResult.ResultType = messageResult.ResultType;
                            }
                            returnMessage = messageResult;
                        }
                    }
                    else if (msg.Action == Contract.Object.RealTime.RealTimeAction.GlobalSearchAction)
                    {
                        var globalSearchInfo = msg.GlobalSearchInfo;
                        var action = GlobalSearchActionFactory.GetGlobalSearchAction(globalSearchInfo.Action);
                        List<Contract.Explorer.BaseRecordDto> records = new List<Contract.Explorer.BaseRecordDto>();
                        foreach (var id in globalSearchInfo.RecordIds)
                        {
                            records.Add(new Contract.Explorer.BaseRecordDto()
                            {
                                NodeId = id,
                                Id = id
                            });
                        }
                        await action.DoActionAsync(records, (SourceFlag)globalSearchInfo.SourceFlag, globalSearchInfo.ActionExtension, msg.JobId, false);
                        returnMessage = new RecordsReturnMessage() { JobId = msg.JobId };
                    }
                    else if (msg.Action == Contract.Object.RealTime.RealTimeAction.MLReviewChangeTerm || msg.Action == Contract.Object.RealTime.RealTimeAction.MLReviewApprove)
                    {
                        var changeTermType = msg.Action switch
                        {
                            Contract.Object.RealTime.RealTimeAction.MLReviewChangeTerm => ChangeTermType.AIMAChangeTerm,
                            Contract.Object.RealTime.RealTimeAction.MLReviewApprove => ChangeTermType.AIMADirectlyApprove,
                            _ => ChangeTermType.None,
                        };
                        returnMessage = await ExplorerService.ChangeTermRealTimeForAIAsync(changeTermType, msg.ChangeTermOption, msg.JobId);
                    }
                    else
                    {
                        throw new Exception("this action is illegal");
                    }
                    //var result =  RecordsListener.RealTimeAction(msg);
                }
                catch (Exception e)
                {
                    logger.Error("task error {0}", e);
                }
                finally
                {
                    RealTimeQueueCounter.Decrease();
                }
            });
            t.IsBackground = true;
            try
            {
                t.Start();
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while processing realtime action. {e.Message}");
                RealTimeQueueCounter.Decrease();
                throw;
            }
        }

        public void OnStart()
        {
            logger.Info("ScheduleJobWorkerRole has been started.");

            RMDBContextManager.DisposeTenantMapping();
            GlobalConfig.InitCastle();
            //Database.SetInitializer<RMDbContext>(new MigrateDatabaseToLatestVersion<RMDbContext, AvePoint.RA.DB.TenantMigrations.Configuration>());
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<RMSysDBContext, AvePoint.RA.DB.ControlMigrations.Configuration>());
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            //RMAosClient.Init();
            AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
            PoolUserUtil.Init(true);
            //PoolUserUtil.Init(true);
        }

    }

    class RealTimeQueueCounter
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static long max
        {
            get
            {
                long m;
                if (!long.TryParse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.REALTIME_MAX_JOBS_LIMIT], out m))
                {
                    m = 20; // default value is 20
                }
                return m;
            }
        }
        private static long current = 0;
        private readonly static object lockObj = new object();

        public static bool CanEnter()
        {
            var result = true;
            lock(lockObj)
            {
                result = current < max;
            }
            if (!result)
            {
                logger.Warn($"RealtimeQueueCounter reaches the max count number : {current}, current thread will sleep for 2s");
                Thread.Sleep(2000);
            }
            return result;
        }
        public static void Increse()
        {
            Interlocked.Increment(ref current);
        }
        public static void Decrease()
        {
            Interlocked.Decrement(ref current);
        }
    }
}

