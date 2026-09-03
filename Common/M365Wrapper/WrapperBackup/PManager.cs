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

//using AvePoint.Application.Redis;
//using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
//using System;
//using AvePoint.RA.CommonUtil;
//using Microsoft365Backup.CommonUtil.Misc;
//using AvePoint.GCommon.JobManagement.LongRunning;



//namespace ExchangeUtility.Graph
//{
//    public class PManager
//    {
//        private static RALogger logger = RALogger.GetInstance(typeof(PManager));

//        private static PManager thisInstance = new PManager();

//        public static PManager Instance()
//        {
//            return thisInstance;
//        }

//        private CData currentData = new CData();
//        private Boolean needSetData = false;

//        private (string Key, DateTime Time) lastHandleData;

//        public void InitMailboxData(ExchangeMailboxType mailboxType, string mailboxName, long startTime)
//        {
//            currentData.CMailboxName = mailboxName;
//            currentData.CMailboxStarttime = startTime;
//            currentData.CompetedItemCount = 0;
//            RefreshLastHandleData();
//            if (mailboxType != ExchangeMailboxType.User)
//            {
//                return;
//            }
//            needSetData = true;
//        }

//        public void GetReportData(TransferData data)
//        {
//            try
//            {
//                if (data.Type == OType.Folder)
//                {
//                    currentData.CFolderPath = data.Path;
//                    currentData.CFolderStarttime = data.StartTime;
//                    currentData.CompetedItemCount = 0;
//                }
//                if (data.Type == OType.Item)
//                {
//                    currentData.CItemName = data.Title;
//                    currentData.CompetedItemCount++;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Error("Get Report Data Exception:{0}", ex);
//            }
//        }


//        public void SetReportData(string jobId)
//        {
//            RefreshLastHandleData();
//            if (!needSetData)
//            {
//                return;
//            }
//            try
//            {
//                var subJobContext = new SubJobRuntimeContext()
//                {
//                    SiteCollection = currentData.CMailboxName,
//                    SiteCollectionStartTime = currentData.CMailboxStarttime,
//                    Object = currentData.CFolderPath,
//                    ObjectStartTime = currentData.CFolderStarttime,
//                    Item = currentData.CItemName,
//                    ItemCount = currentData.CompetedItemCount,
//                    ObjectType = SubJobRTContextObjType.Folder,
//                };
//                Redis_SubjobService.SetSubJobRuntime(IdentityManager.IdentityContent, jobId, subJobContext);
//            }
//            catch (Exception ex)
//            {
//                logger.Error("Set Report Data Exception:{0}", ex);
//            }
//        }

//        public void RemoveSubJobRuntimeInfo(string jobId)
//        {
//            Redis_SubjobService.RemoveSubJobRuntimeInfo(IdentityManager.IdentityContent, jobId);
//        }

//        public void RefreshLastHandleData()
//        {
//            if (lastHandleData.Key != currentData.Key)
//            {
//                lastHandleData = (currentData.Key, DateTime.Now);
//            }
//        }

//        public bool JobIsHang() => lastHandleData.Key == currentData.Key && lastHandleData.Time.AddDays(7) < DateTime.Now;
//    }

//    class CData
//    {
//        public string CMailboxName { get; set; }
//        public long CMailboxStarttime { get; set; }
//        public string CFolderPath { get; set; }
//        public long CFolderStarttime { get; set; }
//        public int CompetedItemCount { get; set; }
//        public string CItemName { get; set; }
//        public string Key => $"{CMailboxName}{CFolderPath}{CItemName}";
//    }

//    public class TransferData
//    {
//        public string Title { get; set; }
//        public string Path { get; set; }
//        public long StartTime { get; set; }
//        public long FinishTime { get; set; }
//        public long Size { get; set; }
//        public OStatus Status { get; set; }
//        public OType Type { get; set; }
//    }

//    public enum OType
//    {
//        Mailbox = 0,
//        Folder = 1,
//        Item = 2,
//    }

//    public enum OStatus
//    {
//        Success = 0,
//        Failed,
//        Skipped
//    }
//}