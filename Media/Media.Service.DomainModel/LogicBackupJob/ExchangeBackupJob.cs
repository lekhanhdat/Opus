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
    #region using directives

    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using System.Collections.Generic;
    using GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Context;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.Media.Service.DomainModel.DocAve6x;


    #endregion using directives

    [StorageInfoMetaDataBuilder(Key = "AvePoint.Media.Service.DomainModel.ExchangeStorageInfoMetaDataBuilder")]
    public class ExchangeBackupJob
        : BackupJobBase
    {
        //public string CycleId { get; set; }

        public string ParentJobId { get; set; }

        public int Order { get; set; }

        //public string PreviousFBJobId { get; set; }

        public int PlanType { get; set; }

        public long BackupTime { get; set; }

        public string StorageInfo { get; set; }

        public string GroupName { get; set; }

        //public bool IsBackuped { get; set; }

        //public List<string> UserAddressList { get; set; }

        //public string StoragePolicyId { get; set; }

        public LogicalDeviceDto IndexLogicalDevice { get; set; }

        public List<string> NeedUpdateIndexVolumns { get; set; }

        public DataEncryptionInfoWrapper DataEncryptionInfoWrapper { get; set; }
       
        
        public override string ToString()
        {
            return string.Format("Exchange backup job info, FarmName: {0},PlanId: {1},JobId: {2}, DataVolume: {3}, IndexVolume: {4}, Order: {5}.",
                FarmName,
                PlanId,
                JobId,
                DataVolume,
                IndexVolume,
                Order);
        }

        public ExchangeBackupJob()
        { }

        public ExchangeBackupJob(ExchangeBackupRequest request)
        {
            ParentJobId = request.JobId.Contains("_")
                ? request.JobId.Substring(0, request.JobId.IndexOf("_"))
                : request.JobId;
            var volumeParam = new VolumeParameter()
            {
                PlanId = request.PlanId,
                CycleId = request.CycleId,
                JobId = ParentJobId,
                EmailAddress = request.MailBoxAddress,
            };
            //studo
            IVolumeGenerator generator = new ExchangeVolumeGenerator();
            DataVolume = generator.GenerateDataVolume(volumeParam);
            IndexVolume = generator.GenerateIndexVolume(volumeParam);
            //NeedUpdateIndexVolumns = GenerateIndexVolume(request, generator);
            //DataMode = request.DataSecurity;
            //if (request.DataSecurity.IsCompressed())
            //{
            //    CompressionType = (int)(request.CompressionType);
            //}
            //else
            //{
            //    CompressionType = -1;
            //}
            PlanType = request.PlanType;
            //PlanName = request.PlanName;
            PlanId = request.PlanId;
            //CycleId = request.CycleId;
            JobId = request.JobId;
            //UserAddressList = request.UserAddressList;
            BackupTime = request.BackupTime;
            CacheSetting = request.CacheLocation;
            LogicalDevice = request.LogicalDevice;
            IndexLogicalDevice = request.IndexLogicalDevice;
            Order = request.Order;
            StoragePolicyName = request.StoragePolicyId;
            GroupName = request.GroupName;
            StorageInfo = request.StorageInfo;
            DataEncryptionInfoWrapper = request.DataEncryptionInfoWrapper;
            IndexEncryptionInfoWrapper = request.IndexEncryptionInfoWrapper;
            //DefaultBlobStorage4Index = request.DefaultBlobStorage4Index;
            //NeedValidateAllDevices = request.NeedValidateAllDevices;
        }

        //private List<string> GenerateIndexVolume(ExchangeBackupRequest request, IVolumeGenerator generator)
        //{
        //    List<string> result = new List<string>();
        //    if (request.NeedUpdateJobIds != null)
        //    {
        //        foreach (var jobId in request.NeedUpdateJobIds)
        //        {
        //            var volumeParam = new VolumeParameter()
        //            {
        //                PlanId = request.PlanId,
        //                CycleId = jobId,
        //                JobId = jobId,
        //            };
        //            result.Add(generator.GenerateIndexVolume(volumeParam));
        //        }
        //    }
        //    return result;
        //}
    }
}