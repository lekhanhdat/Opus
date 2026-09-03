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
    using System.Reflection;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.IO;
    using AvePoint.Media.Core.IO.Input;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.Command;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using AvePoint.Media.StorageApi;
    using global::Media.Common.ClassicStorageApi;

    #endregion directives

    internal class ArchiverRebuildIndexCommand
        : CommandBase
        , ICommand
    {
        IXSystem indexLogicalDevice;
        IXSystem dataLogicalDevice;
        List<ArchiverBasicIndex> indexes;
        IRebuildIndexInputStream inputStream;
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IArchiverBackupIndexService BackupIndexService { get; set; }

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public override string CommandName
        {
            get { return "ArchiverRebuildIndex"; }
        }

        public override string HelpMessage
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("The Syntax of this command is:" + Environment.NewLine);
                builder.Append("ArchiverRebuildIndex ");
                builder.Append("data net share path ");
                builder.Append(@"domain\username ");
                builder.Append("password");
                builder.Append("index net share path ");
                builder.Append(@"domain\username ");
                builder.Append("password" + Environment.NewLine);
                builder.Append("e.g." + Environment.NewLine);
                builder.Append("ArchiverRebuildIndex ");
                builder.Append(@"\\127.0.0.1\C$\data folder ");
                builder.Append(@"farm name\webApplication\site URL ");
                builder.Append(@".\administrator ");
                builder.Append("admin ");
                builder.Append(@"\\127.0.0.1\C$\index folder ");
                builder.Append(@"farm name\webApplication\site URL ");
                builder.Append(@".\administrator ");
                builder.Append("admin");
                return builder.ToString();
            }
        }

        public override string SucceedMessage
        {
            get { return "Archiver rebuild index successfully."; }
        }

        public override string ErrorMessage
        {
            get { return "Archiver rebuild index failed."; }
        }

        protected override bool ExecuteCommand(List<string> args)
        {
            bool result = default(bool);
            try
            {
                Open(args);
                RebuildIndexes();
                result = true;
            }
            catch (System.Exception ex)
            {
                logger.Error(MediaServiceArchiverBackupResource.ArchiverRebuildIndexCommandExecuteCommandError, ex.ToString());
            }
            finally
            {
                Close();
            }
            return result;
        }

        private void Close()
        {
            this.dataLogicalDevice.Close();
            this.inputStream.Close();
            this.IndexService.Close();
        }

        private void Open(List<string> args)
        {
            indexes = new List<ArchiverBasicIndex>();
            OpenDataLogicalDevice(args[0], args[1], args[2]);
            OpenIndexLogicalDevice(args[4], args[5], args[6]);
            string dataVolume = args[3];
            OpenIndexService(args[7]);
            OpenRebuildIndexInputStreamParameter openParam = new OpenRebuildIndexInputStreamParameter()
            {
                DataLogicalDevice = this.dataLogicalDevice,
                DataVolume = dataVolume
            };
            inputStream = new RebuildIndexInputStream(openParam);
            inputStream.Open();
        }

        private void RebuildIndexes()
        {
            while (inputStream.CheckHasMoreIndex())
            {
                RebuildIndexInfo rebuildIndex = inputStream.GetNextIndexInfo();
                if (rebuildIndex.JobId != null && rebuildIndex.EncryptionInfo != null)
                    this.BackupIndexService.UpdateJobInfoIndex(rebuildIndex.JobId, ServiceConstants.EncryptionInfoKey, rebuildIndex.EncryptionInfo);
                indexes.Add(GenerateArchiverIndex(rebuildIndex));
            }
            this.BackupIndexService.InsertArchiveIndexes(indexes);
        }

        private ArchiverBasicIndex GenerateArchiverIndex(RebuildIndexInfo rebuildIndex)
        {
            try
            {
                var tempHeaderIndex = SerializerHelper.DeserializeFromBase64String<ArchiverHeadIndex>(rebuildIndex.IndexSerializerString);
                if (tempHeaderIndex == null)
                {
                    return SerializerHelper.DeserializeFromBase64String<ArchiverBodyIndex>(rebuildIndex.IndexSerializerString);
                }
                return tempHeaderIndex;
            }
            catch
            {
                var tempBodyIndex = SerializerHelper.DeserializeFromBase64String<ArchiverBodyIndex>(rebuildIndex.IndexSerializerString);
                return tempBodyIndex;
            }
        }

        private void OpenIndexService(string indexVolume)
        {
            var openParam = new ArchiverIndexServiceOpenParameter();
            openParam.TreeMode = TreeMode.SiteCollectionMode;
            openParam.IndexVolume = indexVolume;
            openParam.IndexLogicalDeviceSystem = indexLogicalDevice;
            openParam.IsNeedCreateNewIndex = true;
            this.IndexService.Open(openParam);
        }

        protected override bool CheckParameters(List<string> args)
        {
            return args.Count == 8;
        }

        private void OpenIndexLogicalDevice(String UNCPath, String userName, String passWord)
        {
            LogicalDeviceDto indexLogicalDeviceDto = new LogicalDeviceDto();
            var physicalDeviceDto = PhysicalDeviceDto.GenterateFS(UNCPath, userName, SecretUtil.EncryptPassword(passWord));
            indexLogicalDeviceDto.PhysicalDrives.Add(physicalDeviceDto);
            this.indexLogicalDevice = XFactoryCommon.InstanceLibrary(indexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Index));
            this.indexLogicalDevice.Open();
        }

        private void OpenDataLogicalDevice(String UNCPath, String userName, String passWord)
        {
            LogicalDeviceDto indexLogicalDeviceDto = new LogicalDeviceDto();
            var physicalDeviceDto = PhysicalDeviceDto.GenterateFS(UNCPath, userName, SecretUtil.EncryptPassword(passWord));
            indexLogicalDeviceDto.PhysicalDrives.Add(physicalDeviceDto);
            this.dataLogicalDevice = XFactoryCommon.InstanceLibrary(indexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Data));
            this.dataLogicalDevice.Open();
        }
    }
}