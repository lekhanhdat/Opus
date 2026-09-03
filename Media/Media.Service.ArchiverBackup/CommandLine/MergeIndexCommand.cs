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
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.Command;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.StorageApi;
    using global::Media.Common.ClassicStorageApi;
    using Storage;

    #endregion directives

    internal class MergeIndexCommand
        : CommandBase
        , ICommand
    {
        IXSystem indexLogicalDevice;
        String errorMessage;
        String succeedMessage;
        static readonly int indexLimit = 32775;
        static readonly string SelectTableNameArchiverHead = "SELECT * FROM " + IndexConstants.TableNameArchiveHead + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectTableNameArchiverBody = "SELECT * FROM " + IndexConstants.TableNameArchiveBody + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectTableNameArchiverJobInfo = "SELECT * FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID IS NOT NULL";
        static readonly string SelectTableNameArchiverSiteMaster = "SELECT * FROM " + IndexConstants.TableNameArchiveSiteMaster;
        static readonly string SelectTableNameArchiverHeadCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveHead;
        static readonly string SelectTableNameArchiverBodyCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody;

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMainProcessor { get; set; }

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMapProcessor { get; set; }

        public override string CommandName
        {
            get { return "mergeIndex"; }
        }

        public override string HelpMessage
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("The Syntax of this command is:" + Environment.NewLine);
                builder.Append("-mergeIndex ");
                builder.Append("netSharePath ");
                builder.Append(@"domain\username ");
                builder.Append("password" + Environment.NewLine);
                builder.Append("e.g." + Environment.NewLine);
                builder.Append("-mergeIndex ");
                builder.Append(@"\\127.0.0.1\C$\someFolder ");
                builder.Append(@".\administrator ");
                builder.Append("admin");
                return builder.ToString();
            }
        }

        public override string SucceedMessage
        {
            get { return succeedMessage; }
        }

        public override string ErrorMessage
        {
            get { return errorMessage; }
        }

        protected override bool ExecuteCommand(List<string> args)
        {
            string[] sArray = Regex.Split(args[0], "data_archive", RegexOptions.IgnoreCase);
            var UNCPath = args[0].Substring(0, args[0].Length - sArray[sArray.Length - 1].Length - "data_archive".Length - "\\".Length);
            var highName = "data_archive" + sArray[sArray.Length - 1];
            var userName = args[1];
            var passWord = args[2];
            var isSuccessful = true;
            try
            {
                this.OpenLogicalDevice(UNCPath, userName, passWord);
                var storageInfo = XConvert.FromNames(highName, null);
                var fileList = new List<XFileInfo>();
                var tempFileList = this.indexLogicalDevice.ListFiles(storageInfo);
                tempFileList.ForEach(item =>
                {
                    if (!item.Name.Equals(ServiceConstants.IndexDBName))
                    {
                        fileList.Add(item);
                    }
                });

                this.OpenMainIndex(highName);
                if(fileList.Count > 0)
                {
                    fileList.ForEach(item =>
                    {
                        this.OpenMapIndex(item, highName);
                        this.InsertIntoMainIndex();
                        this.IndexMapProcessor.Close();
                    });
                }
                ReleaseResource();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                this.errorMessage = String.Format("Merge index failed, the detail is {0}.", ex.ToString());
            }
            if (isSuccessful == true)
                this.succeedMessage = "Merge index complete.";
            return isSuccessful;
        }

        private void ReleaseResource()
        {
            if (this.indexLogicalDevice != null)
            {
                this.indexLogicalDevice.Close();
            }
            if (this.IndexMainProcessor != null)
            {
                this.IndexMainProcessor.Close();
            }
        }

        protected override bool CheckParameters(List<string> args)
        {
            var result = true;
            if (args.Count != 3)
            {
                errorMessage = this.HelpMessage;
                result = false;
            }

            return result;
        }

        private void OpenMainIndex(String path)
        {
            var openParam = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = ServiceConstants.IndexDBName,
                IndexVolume = path,
                IndexLogicalDeviceSystem = this.indexLogicalDevice,
            };
            this.InitIndexProcessor(openParam, path);
        }

        private void OpenMapIndex(XFileInfo fileInfo, String path)
        {
            var openParam = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = fileInfo.Name,
                IndexVolume = path,
                IndexLogicalDeviceSystem = this.indexLogicalDevice,
            };

            this.InitIndexProcessor(openParam, path);
        }

        private void InsertIntoMainIndex()
        {
            this.InsertIntoHeadOrBodyIndex(IndexConstants.TableNameArchiveHead, SelectTableNameArchiverHead);
            this.InsertIntoHeadOrBodyIndex(IndexConstants.TableNameArchiveBody, SelectTableNameArchiverBody);
            this.InsertIntoJobInfoTable();
            this.InsertIntoSiteMasterIndex();
        }

        private void InsertIntoHeadOrBodyIndex(string tableName, string sql)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            int count = tableName.Equals(IndexConstants.TableNameArchiveHead) ?
                Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(SelectTableNameArchiverHeadCount, null))
                : Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(SelectTableNameArchiverBodyCount, null));
            long number = count / indexLimit;
            long size = count % indexLimit;
            int offset = 0;

            if (number >= 1)
            {
                for (int i = 0; i < number; i++)
                {
                    param["@OFFSET"] = offset;
                    param["@LENGTH"] = indexLimit;
                    if (tableName.Equals(IndexConstants.TableNameArchiveHead))
                    {
                        this.IndexMainProcessor.Execute(IndexConstants.TableNameArchiveHead, this.IndexMapProcessor.ExecuteQuery(sql, param));
                    }
                    else
                    {
                        this.IndexMainProcessor.Execute(IndexConstants.TableNameArchiveBody, this.IndexMapProcessor.ExecuteQuery(sql, param));
                    }
                }
            }
            if (size > 0)
            {
                param["@OFFSET"] = offset;
                param["@LENGTH"] = size;
                if (tableName.Equals(IndexConstants.TableNameArchiveHead))
                {
                    this.IndexMainProcessor.Execute(IndexConstants.TableNameArchiveHead, this.IndexMapProcessor.ExecuteQuery(sql, param));
                }
                else
                {
                    this.IndexMainProcessor.Execute(IndexConstants.TableNameArchiveBody, this.IndexMapProcessor.ExecuteQuery(sql, param));
                }
            }
        }

        private void InitIndexProcessor(ArchiverIndexServiceOpenParameter openParam, String path)
        {
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            StorageInfo storageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(storageInfo))
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(openParam.IndexLogicalDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(openParam.IndexLogicalDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            openParam.IndexLogicalDeviceSystem.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent);
            param.DownLoadResult = indexDownLoadInfo;
            param.IndexWorkingSystem = openParam.IndexLogicalDeviceSystem;
            if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
                this.IndexMainProcessor.Open(param);
            else
                this.IndexMapProcessor.Open(param);
        }

        private void OpenLogicalDevice(String UNCPath, String userName, String passWord)
        {
            LogicalDeviceDto indexLogicalDeviceDto = new LogicalDeviceDto();
            var physicalDeviceDto = PhysicalDeviceDto.GenterateFS(UNCPath, userName, SecretUtil.EncryptPassword(passWord));

            indexLogicalDeviceDto.PhysicalDrives.Add(physicalDeviceDto);
            this.indexLogicalDevice = XFactoryCommon.InstanceLibrary(indexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Index));
            this.indexLogicalDevice.Open();
        }

        private void InsertIntoSiteMasterIndex()
        {
            this.IndexMainProcessor.Execute(IndexConstants.TableNameArchiveSiteMaster, this.IndexMapProcessor.ExecuteQuery(SelectTableNameArchiverSiteMaster, null));
        }

        private void InsertIntoJobInfoTable()
        {
            this.IndexMainProcessor.Execute(IndexConstants.TableNameArchiveJobInfo, this.IndexMapProcessor.ExecuteQuery(SelectTableNameArchiverJobInfo, null));
        }
    }
}