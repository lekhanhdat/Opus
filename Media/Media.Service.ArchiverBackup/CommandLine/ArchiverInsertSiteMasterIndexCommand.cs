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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Archiver;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.Command;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.StorageApi;
    using global::Media.Common.ClassicStorageApi;
    using Storage;


    #endregion directives

    internal class ArchiverInsertSiteMasterIndexCommand
        : CommandBase
        , ICommand
    {
        IXSystem indexLogicalDevice;
        string errorMessage = "Archiver insert site master index failed." + Environment.NewLine;
        IMArchiverSiteMasterIndexService service;

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public IArchiverImportIndexService ImportIndexService { get; set; }

        public override string CommandName
        {
            get { return "ArchiverInsertSiteMasterIndex"; }
        }

        public override string HelpMessage
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("The Syntax of this command is:" + Environment.NewLine);
                builder.Append("-ArchiverInsertSiteMasterIndex ");
                builder.Append("netSharePath ");
                builder.Append(@"domain\username ");
                builder.Append("password ");
                builder.Append("indexVolume" + Environment.NewLine);
                builder.Append("e.g." + Environment.NewLine);
                builder.Append("-ArchiverInsertSiteMasterIndex ");
                builder.Append(@"\\127.0.0.1\C$\someFolder ");
                builder.Append(@".\administrator ");
                builder.Append("admin ");
                builder.Append(@"data_archive\IndexVolume\Farm(DAWEI#SHAREPOINT#SHAREPOINT_CONFIG_FF758B88-0CFA-4EBC-B6D0-7BB625498DD5)\http#7971#dawei\#sites#May");
                return builder.ToString();
            }
        }

        public override string SucceedMessage
        {
            get { return "Archiver insert site master index complete."; }
        }

        public override string ErrorMessage
        {
            get { return errorMessage; }
        }

        protected override bool ExecuteCommand(List<string> args)
        {
            bool excuteResult = true;
            try
            {
                RegisterMediaService();
                OpenLogicalDevice(args[0], args[1], args[2]);
                OpenIndexService(args[3]);
                InsertSiteMasterIndex();
            }
            catch (Exception e)
            {
                excuteResult = false;
                errorMessage += e.ToString();
            }
            finally
            {
                Close();
            }
            return excuteResult;
        }

        private void RegisterMediaService()
        {
            var registerService = MediaServiceLocator.Discover<IStartable>("AvePoint.Media.Service.RegisterService");
            registerService.Start();
        }

        private void InsertSiteMasterIndex()
        {
            var siteMasterIndexes = this.ImportIndexService.GetAllSiteMasterIndex();
            foreach (var siteMasterIndex in siteMasterIndexes)
            {
                InsertControlSiteMasterIndex(siteMasterIndex);
            }
        }

        private void OpenIndexService(string indexVolume)
        {
            ArchiverIndexServiceOpenParameter indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter();
            indexServiceOpenParameter.TreeMode = TreeMode.SiteCollectionMode;
            indexServiceOpenParameter.IndexVolume = indexVolume;
            indexServiceOpenParameter.IndexLogicalDeviceSystem = this.indexLogicalDevice;
            this.IndexService.Open(indexServiceOpenParameter);
        }

        private void Close()
        {
            this.indexLogicalDevice.Close();
            this.IndexService.Close();
        }

        private void InsertControlSiteMasterIndex(ArchiverSiteMasterIndex siteMasterIndex)
        {
            if (service == null)
            {
                //service = MediaProxyBuilder.CreateProxy<IMArchiverSiteMasterIndexService>();
            }

            var archiverIndexSubInfo = new ArchiverIndexSubInfoContract()
            {
                Id = Guid.NewGuid().ToString(),
                JobId = siteMasterIndex.JobId,
                LogicalDeviceId = "f175a8d3-3342-4562-bcea-5f461ba183b1",
                PhysicalDeviceId = "3e696cfa-3390-4318-b619-92cb57923e3e",
                StoragePolicyId = "0224501b-722a-453a-a3e4-d0018a1715c1",
                RetentionTime = siteMasterIndex.BackupTime,
                RetentionTimeSpanSeconds = 0L
            };
            var archiverIndexSubInfoList = new List<ArchiverIndexSubInfoContract>();
            archiverIndexSubInfoList.Add(archiverIndexSubInfo);
            var archiverSiteMasterIndexContract = new ArchiverSiteMasterIndexContract
            {
                JobId = siteMasterIndex.JobId.Substring(0, siteMasterIndex.JobId.LastIndexOf('_')),
                Id = Guid.NewGuid().ToString(),
                ArchiverTime = siteMasterIndex.BackupTime,
                FarmName = siteMasterIndex.FarmName,
                FarmId = "d2dcbdfe-eb8f-4272-a18c-91a5d04af9ac",
                IndexDeviceId = "f175a8d3-3342-4562-bcea-5f461ba183b1",
                WebURL = siteMasterIndex.WebAppName,
                SiteURL = siteMasterIndex.SiteUrl,
                WebId = "23e259f7-6947-425a-981f-dd06106cd7e9",
                SiteId = "09e93c49-70a2-48c9-9503-f22a35fe54e7",
                JobState = 0,
                StoragePolicyId = "0224501b-722a-453a-a3e4-d0018a1715c1",
                SPVersion = siteMasterIndex.SPVersion,
                MergeIndexState = MergeIndexState.Succeed,
                SubInfo = archiverIndexSubInfoList,
            };
            this.service.InsertIntoArchiverSiteMasterIndex(archiverSiteMasterIndexContract);
        }

        protected override bool CheckParameters(List<string> args)
        {
            return args.Count == 4;
        }

        private void OpenLogicalDevice(String UNCPath, String userName, String passWord)
        {
            LogicalDeviceDto indexLogicalDeviceDto = new LogicalDeviceDto();
            var physicalDeviceDto = PhysicalDeviceDto.GenterateFS(UNCPath, userName, SecretUtil.EncryptPassword(passWord));
            indexLogicalDeviceDto.PhysicalDrives.Add(physicalDeviceDto);
            this.indexLogicalDevice = XFactoryCommon.InstanceLibrary(indexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Index));
            this.indexLogicalDevice.Open();
        }
    }
}