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
using Cloud.Sdk.Data.CloudInsights;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    internal class CloudInsightsDataReader : IEnumerator<AuditDownloadDataInfo>, IEnumerable<AuditDownloadDataInfo>
    {
        private readonly static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Queue<AuditDownloadDataInfo> mAuditQueue;

        private StorageSettingModel mStorageSettingModel;

        private AuditDownloadDataInfo mCurrent;
        private bool mSPAuditPacketStorage = false;
        private DateTime mStartTime;
        private DateTime mEndTime;
        private string mJobId;
        private string googleBucket;
        CloudInsightsConnectorBase cloudInsightsConnector;

        public delegate void SetRportCount(Object sender, SetCalculatedCountEventArgs args);
        public delegate void IncreaseProgress(Object sender, IncreaseProgressEventArgs args);
        public Exception Error { get; private set; }
        public bool Finished { get; set; }
        public AuditDownloadDataInfo Current => mCurrent;

        //public Dictionary<string, string> UserMappings = null;

        object IEnumerator.Current => mCurrent;

        public CloudInsightsDataReader(StorageSettingModel storageSettingModel, string jobid, DateTime startTime, DateTime endTime)
        {
            mLog.Info("Storage setting type is " + storageSettingModel.StorageType);
            //if (storageSettingModel.StorageType == TenantStorageType.GoogleBucket)
            //{
            //    try
            //    {
            //        var storageClient = XFactory.InstanceSystem(storageSettingModel.StorageXri);
            //        storageClient.Open();
            //        storageClient.Validate();
            //    }
            //    catch (Exception e)
            //    {
            //        mLog.Warn($"List google storage error:{e}");
            //    }
            //}

            mStorageSettingModel = storageSettingModel;
            mJobId = jobid;

            mStartTime = startTime;
            mEndTime = endTime;
            switch (mStorageSettingModel.StorageType)
            {
                case TenantStorageType.Default:
                case TenantStorageType.AzureStorage:
                    {
                        cloudInsightsConnector = new CloudInsightsAzureConnector(mJobId, mStorageSettingModel.AzureStorageSas, mStartTime, mEndTime);
                        break;
                    }
                case TenantStorageType.AmazonS3:
                    {
                        cloudInsightsConnector = new CloudInsightsAmazonConnector(mJobId, mStorageSettingModel.AmazonS3Model, mStartTime, mEndTime);
                        break;
                    }
                case TenantStorageType.GoogleBucket:
                    {
                        cloudInsightsConnector = new CloudInsightsGoogleConnector(mJobId, mStorageSettingModel.StorageXri, mStartTime, mEndTime);
                        break;
                    }
                default:
                    {
                        throw new NotImplementedException(mStorageSettingModel.StorageType.ToString());
                    }
            }
            mAuditQueue = cloudInsightsConnector.AuditFileQueue;
        }

        public void SetAuditPacketStore(bool packetStore, List<string> siteUrls)
        {
            mSPAuditPacketStorage = packetStore;
            cloudInsightsConnector.SetAuditPacketStore(packetStore, siteUrls);
        }

        public IEnumerator<AuditDownloadDataInfo> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            lock (mAuditQueue)
            {
                while (mAuditQueue.Count == 0)
                {
                    if (Error != null)
                    {
                        throw Error;
                    }
                    if (Finished)
                    {
                        mCurrent = null;
                        return false;
                    }
                    Monitor.Wait(mAuditQueue);
                }
                mCurrent = mAuditQueue.Dequeue();
                Monitor.Pulse(mAuditQueue);
            }
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Build()
        {
            //UserMappings = cloudInsightsConnector.GetUserMappings();
            var mProcessor = new Thread(Process) { IsBackground = true };
            mProcessor.Start();
        }

        private void Finish()
        {
            lock (mAuditQueue)
            {
                Finished = true;
                Monitor.Pulse(mAuditQueue);
            }
        }

        public void SetEvent(SetRportCount setRportCount, IncreaseProgress increaseProgress)
        {
            cloudInsightsConnector.SetRportCount += new EventHandler<SetCalculatedCountEventArgs>(setRportCount);
            cloudInsightsConnector.IncreaseProgress += new EventHandler<IncreaseProgressEventArgs>(increaseProgress);
        }

        public void Dispose()
        {
            if (Error != null)
            {
                mLog.Warn("CloudInsightsDataReader dispose " + Error.Message + Error.StackTrace);
            }
        }

        private void Process()
        {
            try
            {
                cloudInsightsConnector.Run();
            }
            catch (Exception e)
            {
                mLog.Error($"CloudInsightsConnector run failed. error is {e.ToString()}");
                Error = e;
            }
            finally
            {
                Finish();
            }
        }
    }
}
