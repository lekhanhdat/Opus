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




namespace AvePoint.Media.Service.DomainModel
{
    #region directives

    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.RA.Common.Cache;
    using System.Collections.Generic;

    #endregion directives

    public class ExchangeRestoreJob
        : RestoreJobBase
    {
        public string CycleId { get; set; }

        public string ModulePath => RestoreRequest.GetModulePath();

        public ExchangeOnlineTreeNodeDto ExchangeTreeRoot { get; set; }

        public Dictionary<string, string> IndexStorageInfoMap { get; set; }

        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        public long? FromSentDate { get; set; }

        public long? ToSentDate { get; set; }

        public ExchangeRestoreRequest RestoreRequest { get; set; }

        public LogicalDeviceDto IndexDBLogicalDevice { get; set; }

        public override string ToString()
        {
            return string.Format("ExchangeRestoreJob : FarmName: {0}, PlanId: {1}, CycleId: {2}, JobId: {3}, DataVolume: {4}, IndexVolume: {5}, BackupJobId: {6}, OnlyOneJob: {7}, FromSentDate: {8}, ToSentDate: {9}.",
                FarmName,
                PlanId,
                CycleId,
                JobId,
                DataVolume,
                IndexVolume,
                BackupJobId,
                OnlyOneJob,
                FromSentDate,
                ToSentDate
                );
        }

        //public ExchangeRestoreJob()
        //{ }

        public ExchangeRestoreJob(ExchangeRestoreRequest request)
        {
            RestoreRequest = request;
           

            //var generator = new ExchangeVolumeGenerator();

            //var volumeParam = new VolumeParameter()
            //{
            //    PlanId = request.BackupPlanId,
            //    CycleId = request.BackupCycleId,
            //    JobId = request.BackupJobId,
            //};

            //DataVolume = generator.GenerateDataVolume(volumeParam);
            //IndexVolume = generator.GenerateIndexVolume(volumeParam);

            JobId = request.JobId;
            PlanId = request.BackupPlanId;
            CycleId = request.BackupCycleId;
            BackupJobId = request.BackupJobId;
            BackupTime = request.BackupTime;
            OnlyOneJob = request.OnlyOneJob;
            CacheSetting = request.CacheLocation;
            LogicalDevice = request.LogicalDevice;
            IndexDBLogicalDevice = request.IndexDBLogicalDevice;
            ExchangeTreeRoot = request.TreeRoot;
            IndexStorageInfoMap = request.IndexStorageInfoMap;
            RestoreSecurityInfos = request.RestoreSecurityInfos;
            IndexEncryptionInfoWrapper = request.IndexEncryptionInfoWrapper;
            //SourceDataType = request.SourceDataType;
            //IsIncludeDeletedContents = request.IsIncludeDeletedContents;
            FromSentDate = request.FromSentDate;
            ToSentDate = request.ToSentDate;
        }

        public ExchangeRestoreJob()
        {
        }
    }
}