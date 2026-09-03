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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Media.Storage.Util;

#region  module

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#FileExists(AvePoint.Media.Storage.StorageInfo)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#FileExists(AvePoint.Media.Storage.StorageInfo)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#DirectoryExists(AvePoint.Media.Storage.StorageInfo)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#DirectoryExists(AvePoint.Media.Storage.StorageInfo)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#CreateNewFolder(AvePoint.Media.Storage.StorageInfo)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#DeleteDirectory(AvePoint.Media.Storage.StorageInfo)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#DeleteDirectory(AvePoint.Media.Storage.StorageInfo)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#DeleteFile(AvePoint.Media.Storage.StorageInfo)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#DeleteFile(AvePoint.Media.Storage.StorageInfo)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#ListFileVersion(AvePoint.Media.Storage.StorageInfo)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#ListFileVersion(AvePoint.Media.Storage.StorageInfo)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#CopyFile(AvePoint.Media.Storage.StorageInfo,AvePoint.Media.Storage.StorageInfo,System.Boolean)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#CopyFile(AvePoint.Media.Storage.StorageInfo,AvePoint.Media.Storage.StorageInfo,System.Boolean)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#ListSubDirectoriesAndFiles(AvePoint.Media.Storage.StorageInfo)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#ListSubDirectoriesAndFiles(AvePoint.Media.Storage.StorageInfo)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#MoveFile(AvePoint.Media.Storage.StorageInfo,AvePoint.Media.Storage.StorageInfo,System.Boolean)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#MoveFile(AvePoint.Media.Storage.StorageInfo,AvePoint.Media.Storage.StorageInfo,System.Boolean)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100013:DoNotMissExceptionHandlingInCatchBlocks", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#Validate()")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#CopyDirectory(AvePoint.Media.Storage.StorageInfo,AvePoint.Media.Storage.StorageInfo,System.Boolean)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#CopyDirectory(AvePoint.Media.Storage.StorageInfo,AvePoint.Media.Storage.StorageInfo,System.Boolean)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#.ctor(System.String,AvePoint.Media.Storage.AbstractXSystem)", MessageId = "Privated")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#OpenDirectory(AvePoint.Media.Storage.StorageInfo,System.IO.FileMode)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#OpenDirectory(AvePoint.Media.Storage.StorageInfo,System.IO.FileMode)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#CreateUser()", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#CreateUser()", MessageId = "jmiller")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#CreateNewFolder(AvePoint.Media.Storage.StorageInfo)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#OpenFile(AvePoint.Media.Storage.StorageInfo)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#OpenFile(AvePoint.Media.Storage.StorageInfo)", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#MoveDirectory(AvePoint.Media.Storage.StorageInfo,AvePoint.Media.Storage.StorageInfo,System.Boolean)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteSystem.#MoveDirectory(AvePoint.Media.Storage.StorageInfo,AvePoint.Media.Storage.StorageInfo,System.Boolean)", MessageId = "pubapi")]
#endregion

namespace AvePoint.Media.Storage.Egnyte
{
    #region CodeReview
    [AveCodeReview(
        "2013/10/16",
        "xiao.zhang@avepoint.com",
        "xiao.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 },
        "ADO-93945",
        true,
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }
        )]
    #endregion

    class EgnyteSystem : AbstractXSystem
    {
        #region Field and property
        internal AveLogger logger = AveLogger.GetInstance(typeof(EgnyteSystem));
        internal EgnyteOpenParameter OpenParameter;
        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                return StorageInterfaceType.Namespace;
            }
        }

        public override String Type
        {
            get
            {
                return "EgnyteSystem";
            }
        }
        #endregion

        static EgnyteSystem()
        {
            ServicePointManager.DefaultConnectionLimit = 1024;
        }

        public EgnyteSystem(String xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.IsSupportAutoChangeDataBlock = true;
            //this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.OpenParameter = new EgnyteOpenParameter();
            //base.Open();
            this.SystemLocation = XriObject.Params[XRIParameterKeys.Egnyte_RootFolderName];
            this.SystemHealth = XSystemHealth.Unknown;
            if (XriObject.Params.ContainsKey(XRIParameterKeys.USE_SHARED))
            {
                this.OpenParameter.UseShared = XriObject.Params[XRIParameterKeys.USE_SHARED];
                if (this.OpenParameter.UseShared.Equals(true))
                {
                    this.SystemLocation = String.Format("Shared/{0}", this.SystemLocation);
                }
                else
                {
                    this.SystemLocation = String.Format("Privated/{0}/{1}", XriObject.Params[XRIParameterKeys.Egnyte_UserName], this.SystemLocation);
                }
            }
            else
            {
                this.SystemLocation = String.Format("Shared/{0}", this.SystemLocation);
            }
            this.Open();
        }

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            base.Open();
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CREATE_IF_NOT_EXISTS))
            {
                createIfNotExist = Boolean.Parse(XriObject.Params[XRIParameterKeys.CREATE_IF_NOT_EXISTS]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Egnyte_Domain))
            {
                this.OpenParameter.Domain = XriObject.Params[XRIParameterKeys.Egnyte_Domain];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Egnyte_Token))
            {
                this.OpenParameter.Token = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.Egnyte_Token]);
            }
            this.SetSystemDescription();
            return new StorageOpenValidResult();
        }

        protected override void SetSystemDescription()
        {
            this.Properties[SystemPropertyKeys.SystemDescriptionKey] = "Egnyte Object Storage Server.";
            var key = new List<String>();
            key.Add(this.OpenParameter.Domain);
            var securityKeys = new List<string>();
            securityKeys.Add(this.OpenParameter.Token);
            this.SystemKey = GenerateSystemKey(key, securityKeys);
        }

        public override StorageOpenValidResult Validate()
        {
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            else
            {
                StorageOpenValidResult validResult = new StorageOpenValidResult();
                StorageInfo rootFolder = new StorageInfo();
                if (!this.DirectoryExists(rootFolder))
                {
                    if (CreateIfNotExists)
                    {
                        this.CreateNewFolder(rootFolder);
                    }
                    else
                    {
                        throw new FileNotFoundException("The root folder doesn't exist.");
                    }
                }
                else
                {
                    if (this.ReadOnly)
                    {
                        validResult.IsReadAble = true;
                        validResult.IsWriteAble = false;
                        validResult.IsDeleteAble = false;
                        validResult.IsHasPermission = true;
                    }
                    else
                    {
                        validResult.IsReadAble = true;
                        validResult.IsHasPermission = true;
                        StorageInfo validateFile = new StorageInfo();
                        validateFile.LowName = System.DateTime.Now.Ticks + "DocAve.txt";
                        try
                        {
                            XStream stream = this.OpenStream(validateFile, FileMode.OpenOrCreate);
                            Byte[] buffer = { 60 };
                            stream.Write(buffer, 0, buffer.Length);
                            stream.Close();
                            validResult.IsWriteAble = true;
                            this.DeleteFile(validateFile);
                            this.logger.Info("Delete temp file successful.");
                            validResult.IsDeleteAble = true;
                        }
                        catch (Exception e)
                        {
                            logger.Warn("User doesn't have delete permission.Domain={1}, message is {2}", this.OpenParameter.Domain, e.Message);
                        }
                    }
                }
                validResult.TotalSpace = this.innerTotalSpace = long.MaxValue - 1;
                validResult.TotalUsedSpace = this.innerTotalUsedSpace = ulong.MinValue;
                validResult.TotalFreeSpace = this.innerTotalFreeSpace = validResult.TotalSpace - validResult.TotalUsedSpace;
                validResult.SystemHealth = XSystemHealth.AvailableAndNotFull;
                return validResult;
            }
        }

        public override Boolean DirectoryExists(StorageInfo info)
        {
            var result = false;
            try
            {
                info.path = PathUtil.CombinePath(this.SystemLocation, info.HighName);
                var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(info.path, info.LowName)));
                EgnyteUtil.Retry<Boolean>(delegate()
                {
                    var request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_GET, url, this.OpenParameter.Token);
                    request.ContentType = "application/json";
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            this.logger.Info(String.Format("Folder exist.HighName={0},LowName={1}", info.HighName, info.LowName));
                            result = true;
                        }
                        else
                        {
                            this.logger.Info(String.Format("Folder doesn't exist.HighName={0},LowName={1}", info.HighName, info.LowName));
                        }
                    }
                    return result;
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (PathNotFoundException e)
            {
                this.logger.Info(String.Format("Folder doesn't exist.HighName={0},LowName={1},Message={2}", info.HighName, info.LowName, e.Message));
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public override Boolean FileExists(StorageInfo info)
        {
            var result = false;
            try
            {
                info.path = PathUtil.CombinePath(this.SystemLocation, info.HighName);
                var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(info.path, info.LowName)));
                EgnyteUtil.Retry<Boolean>(delegate()
                {
                    var request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_GET, url, this.OpenParameter.Token);
                    request.ContentType = "application/json";
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            this.logger.Info(String.Format("File exists.HighName={0},LowName={1}", info.HighName, info.LowName));
                            result = true;
                        }
                        else
                        {
                            this.logger.Info(String.Format("File doesn't exists.HighName={0},LowName={1}", info.HighName, info.LowName));
                        }
                    }
                    return result;
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (PathNotFoundException e)
            {
                this.logger.Info(String.Format("File doesn't exists.HighName={0},LowName={1},Message={2}", info.HighName, info.LowName, e.Message));
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            XDirectoryInfo result;
            try
            {
                if (!DirectoryExists(dirInfo))
                {
                    if (mode == FileMode.Create || mode == FileMode.OpenOrCreate)
                    {
                        result = this.CreateNewFolder(dirInfo);
                    }
                    else
                    {
                        result = null;
                    }
                }
                else
                {
                    var egnyteObject = new EgnyteObject();
                    dirInfo.path = PathUtil.CombinePath(this.SystemLocation, dirInfo.HighName);
                    var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(dirInfo.path, dirInfo.LowName)));
                    result = EgnyteUtil.Retry<EgnyteFolderInfo>(delegate()
                    {
                        var request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_GET, url, this.OpenParameter.Token);
                        request.ContentType = "application/json";
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                this.logger.Info(String.Format("Open directory succeed.highName={0},lowName={1}", dirInfo.HighName, dirInfo.LowName));
                                using (var stream = response.GetResponseStream())
                                {
                                    using (var streamReader = new StreamReader(stream))
                                    {
                                        egnyteObject = EgnyteUtil.ParseJsonString(streamReader.ReadToEnd());
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception(String.Format("Open Directory failed.highName={0},lowName={1}", dirInfo.HighName, dirInfo.LowName));
                            }
                        }
                        return new EgnyteFolderInfo(dirInfo.HighName, dirInfo.LowName, this, egnyteObject);
                    }, this.MaxRetryCount, this.RetryInterval);
                }
            }
            //catch (PathNotFoundException pe)
            //{
            //    this.logger.Warn("Folder is not exist.highName={0},lowName={1},Message={2}", dirInfo.HighName, dirInfo.LowName, pe.Message);
            //    return null;
            //}
            catch (Exception)
            {
                //this.logger.Error(String.Format("Open folder failed.highName={0},lowName={1},Message={2}", dirInfo.HighName, dirInfo.LowName, e.Message));
                throw;
            }
            return result;
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            XFileInfo result;
            try
            {
                fileInfo.path = PathUtil.CombinePath(this.SystemLocation, fileInfo.HighName);
                var egnyteObject = new EgnyteObject();
                var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(fileInfo.path, fileInfo.LowName)));
                result = EgnyteUtil.Retry<EgnyteFileInfo>(delegate()
                {
                    var request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_GET, url, this.OpenParameter.Token);
                    request.ContentType = "application/json";
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            this.logger.Info(String.Format("Open file succeed.highName={0},lowName={1}", fileInfo.HighName, fileInfo.LowName));
                            using (var stream = response.GetResponseStream())
                            {
                                using (var streamReader = new StreamReader(stream))
                                {
                                    egnyteObject = EgnyteUtil.ParseJsonString(streamReader.ReadToEnd());
                                }
                            }
                        }
                        else
                        {
                            throw new Exception(String.Format("Open file failed.highName={0},lowName={1}", fileInfo.HighName, fileInfo.LowName));
                        }
                    }
                    return new EgnyteFileInfo(fileInfo.HighName, fileInfo.LowName, this, egnyteObject);
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (PathNotFoundException pe)
            {
                this.logger.Warn("File is not exist.highName={0},lowName={1}.Message:{2}", fileInfo.HighName, fileInfo.LowName, pe.Message);
                result = null;
            }
            catch (Exception)
            {
                //this.logger.Error(String.Format("Open file failed.highName={0},lowName={1}.Message:{2}", fileInfo.HighName, fileInfo.LowName, e.Message));
                throw;
            }
            return result;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState();
            return new EgnyteStream(this, info, fileMode);
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            var deleteResult = new StorageDeleteResult();
            info.path = PathUtil.CombinePath(this.SystemLocation, info.HighName);
            var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(info.path, info.LowName)));
            try
            {
                deleteResult = EgnyteUtil.Retry<StorageDeleteResult>(delegate()
                {
                    var request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_DELETE, url, this.OpenParameter.Token);
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            this.logger.Info(String.Format("Folder delete succeed. highName:{0},lowName:{1}", info.HighName, info.LowName));
                            deleteResult.IsDeleted = true;
                            if (info.IsDeleteParentFolder)
                                this.DeleteParentFolder(info.HighName);
                        }
                        else
                        {
                            this.logger.Error(String.Format("Folder delete failed. highName:{0},lowName:{1},statusCode:{2}.", info.HighName, info.LowName, response.StatusCode));
                        }
                        return deleteResult;
                    }
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (FileNotFoundException)
            {
                this.logger.Info(String.Format("Folder already not exist. highName:{0},lowName:{1}", info.HighName, info.LowName));
                deleteResult.IsDeleted = true;
            }
            catch (Exception e)
            {
                this.logger.Error(String.Format("Folder delete failed. highName:{0},lowName:{1}.Message:{2}", info.HighName, info.LowName, e.Message));
                deleteResult.IsDeleted = false;
            }
            return deleteResult;
        }

        private void DeleteParentFolder(String highName)
        {
            var directoryNames = highName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var directoryPaths = new List<String>();
            for (int i = 0; i < directoryNames.Length - 1; i++)
            {
                directoryNames[i] = directoryNames[i].TrimEnd('\\').TrimEnd('/') + "\\";
                if (i == 0)
                {
                    directoryPaths.Add(directoryNames[i]);
                }
                else
                {
                    directoryPaths.Add(PathUtil.CombinePath(directoryPaths[i - 1], directoryNames[i]));
                }
            }
            for (int index = directoryPaths.Count - 1; index >= 0; index--)
            {
                var parentFolderInfo = new StorageInfo(directoryPaths[index], "") { IsDeleteParentFolder = false };
                var subObjects = this.ListSubDirectoriesAndFiles(parentFolderInfo);
                if (subObjects.Files.Count == 0 && subObjects.SubDirs.Count == 0)
                {
                    this.DeleteDirectory(parentFolderInfo);
                }
                else
                {
                    break;
                }
            }
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            var deleteResult = new StorageDeleteResult();
            info.path = PathUtil.CombinePath(this.SystemLocation, info.HighName);
            try
            {
                deleteResult = EgnyteUtil.Retry<StorageDeleteResult>(delegate()
                {
                    var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(info.path, info.LowName)));
                    var request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_DELETE, url, this.OpenParameter.Token);
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            this.logger.Info("File delete succeed.highName={0},lowName={1}.", info.HighName, info.LowName);
                            deleteResult.IsDeleted = true;
                        }
                        else
                        {
                            this.logger.Error("File delete succeed.highName={0},lowName={1},statusCode:{2}.", info.HighName, info.LowName, response.StatusCode);
                        }
                        return deleteResult;
                    }
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (FileNotFoundException)
            {
                this.logger.Info(String.Format("File already not exist.highName={0},lowName={1}", info.HighName, info.LowName));
                deleteResult.IsDeleted = true;
            }
            catch (Exception e)
            {
                this.logger.Error(String.Format("File delete failed.highName={0},lowName={1}.Message:{2}", info.HighName, info.LowName, e.Message));
            }
            return deleteResult;
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            try
            {
                var sourceFullPath = PathUtil.CombinePath(PathUtil.CombinePath(this.SystemLocation, sourceDirInfo.HighName), sourceDirInfo.LowName);
                var targetFullPath = PathUtil.CombinePath(PathUtil.CombinePath(this.SystemLocation, targetDirInfo.HighName), targetDirInfo.LowName);
                StorageMoveResult moveResult = new StorageMoveResult();
                if (sourceFullPath.Equals(targetFullPath))
                {
                    throw new Exception(String.Format("Move failed,source file path is equal to target file path.highName={0},lowName={1}", sourceDirInfo.HighName, sourceDirInfo.LowName));
                }
                if (!DirectoryExists(sourceDirInfo))
                {
                    throw new Exception(String.Format("Move failed,source file doesn't exist.highName={0},lowName={1}", sourceDirInfo.HighName, sourceDirInfo.LowName));
                }
                if (!DirectoryExists(targetDirInfo))
                {
                    this.CreateNewFolder(targetDirInfo);
                }
                return EgnyteUtil.Retry<StorageMoveResult>(delegate()
                {
                    sourceDirInfo.path = String.Format("{0}/{1}", this.SystemLocation, sourceDirInfo.HighName);
                    targetDirInfo.path = String.Format("{0}/{1}", this.SystemLocation, targetDirInfo.HighName);
                    var url = String.Format(StorageUrl.EgnyteMove, this.OpenParameter.Domain, EgnyteUtil.Encode(sourceDirInfo.path), EgnyteUtil.Encode(sourceDirInfo.LowName));
                    HttpWebRequest request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_POST, url, this.OpenParameter.Token);
                    request.ContentType = "application/json";
                    if (!targetDirInfo.path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        targetDirInfo.path = "/" + targetDirInfo.path;
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("{\"action\":").Append("\"move\", ").Append("\"destination\":").Append("\"")
                        .Append(EgnyteUtil.Encode(targetDirInfo.path)).Append("/").Append(EgnyteUtil.Encode(targetDirInfo.LowName))
                        .Append("/").Append(EgnyteUtil.Encode(sourceDirInfo.LowName)).Append("\"}");
                    Byte[] mateData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    request.ContentLength = mateData.Length;
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(mateData, 0, mateData.Length);
                    }
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception("Move directory failed, sourcePath=" + PathUtil.CombinePath(sourceDirInfo.path, sourceDirInfo.LowName));
                        }
                        else
                        {
                            this.logger.Info("Move directory succeed, sourcePath=" + PathUtil.CombinePath(sourceDirInfo.path, sourceDirInfo.LowName));
                            moveResult.IsMoved = true;
                            XURIResult uri = new XURIResult();
                            uri.SysId = this.SystemID;
                            uri.SdType = 409;
                            uri.SInfo = new StorageInfo();
                            uri.SInfo.HighName = targetDirInfo.HighName;
                            uri.SInfo.LowName = targetDirInfo.LowName;
                            moveResult.URI = uri;
                        }
                    }
                    return moveResult;
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
                throw;
            }
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            try
            {
                var sourceFullPath = PathUtil.CombinePath(PathUtil.CombinePath(this.SystemLocation, sourceFileInfo.HighName), sourceFileInfo.LowName);
                var targetFullPath = PathUtil.CombinePath(PathUtil.CombinePath(this.SystemLocation, targetFileInfo.HighName), targetFileInfo.LowName);
                if (sourceFullPath.Equals(targetFullPath))
                {
                    throw new Exception("Move file failed,source file path is equal to the target file path.");
                }
                StorageInfo targetParent = new StorageInfo() { HighName = targetFileInfo.HighName };
                if (!DirectoryExists(targetParent))
                {
                    this.CreateNewFolder(targetParent);
                }
                if (!isOverWrite && FileExists(targetFileInfo))
                {
                    throw new Exception("Move file failed,the target file can't be covered.");
                }
                if (isOverWrite && FileExists(targetFileInfo))
                {
                    this.DeleteFile(targetFileInfo);
                }
                StorageMoveResult moveResult = new StorageMoveResult();
                return EgnyteUtil.Retry<StorageMoveResult>(delegate()
                {
                    sourceFileInfo.path = String.Format("{0}/{1}", this.SystemLocation, sourceFileInfo.HighName);
                    targetFileInfo.path = String.Format("{0}/{1}", this.SystemLocation, targetFileInfo.HighName);
                    var url = String.Format(StorageUrl.EgnyteMove, this.OpenParameter.Domain, EgnyteUtil.Encode(sourceFileInfo.path), EgnyteUtil.Encode(sourceFileInfo.LowName));
                    HttpWebRequest request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_POST, url, this.OpenParameter.Token);
                    request.ContentType = "application/json";
                    if (!targetFileInfo.path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        targetFileInfo.path = "/" + targetFileInfo.path;
                    }
                    targetFileInfo.path = targetFileInfo.path.Replace('\\', '/');
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("{\"action\":").Append("\"move\", ").Append("\"destination\":").Append("\"")
                        .Append(targetFileInfo.path).Append("/").Append(targetFileInfo.LowName)
                        .Append("\"}");
                    Byte[] mateData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    request.ContentLength = mateData.Length;
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(mateData, 0, mateData.Length);
                    }
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception("Move file failed, sourcePath=" + PathUtil.CombinePath(sourceFileInfo.path, sourceFileInfo.LowName));
                        }
                        else
                        {
                            this.logger.Info("Move file succeed, sourcePath=" + PathUtil.CombinePath(sourceFileInfo.path, sourceFileInfo.LowName));
                            moveResult.IsMoved = true;
                            XURIResult uri = new XURIResult();
                            uri.SysId = this.SystemID;
                            uri.SdType = 409;
                            uri.SInfo = new StorageInfo();
                            uri.SInfo.HighName = targetFileInfo.HighName;
                            uri.SInfo.LowName = targetFileInfo.LowName;
                            moveResult.URI = uri;
                        }
                    }
                    return moveResult;
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
                throw;
            }
        }

        //public StorageCopyResult CopyDirectory(StorageInfo sourceInfo, StorageInfo targetInfo, bool isOverWrite)
        //{
        //    StorageCopyResult copyResult = new StorageCopyResult();
        //    return EgnyteUtil.Retry<StorageCopyResult>(delegate()
        //    {
        //        sourceInfo.path = String.Format("{0}/{1}", this.SystemLocation, sourceInfo.HighName);
        //        targetInfo.path = String.Format("{0}/{1}", this.SystemLocation, targetInfo.HighName);
        //        var url = String.Format("https://{0}.egnyte.com/pubapi/v1/fs/{1}/{2}", this.OpenParameter.Domain, sourceInfo.path, sourceInfo.LowName);
        //        HttpWebRequest request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_POST, url, this.OpenParameter.Token);
        //        request.ContentType = "application/json";
        //        if (!targetInfo.path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        //        {
        //            targetInfo.path = "/" + targetInfo.path;
        //        }
        //        StringBuilder stringBuilder = new StringBuilder();
        //        stringBuilder.Append("{\"action\":\"").Append("copy\",").Append("\"destination\":\"")
        //            .Append(targetInfo.path).Append("/").Append(targetInfo.LowName)
        //            .Append("/").Append(sourceInfo.LowName).Append("\"}");
        //        Byte[] mateData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
        //        request.ContentLength = mateData.Length;
        //        using (Stream reqStream = request.GetRequestStream())
        //        {
        //            reqStream.Write(mateData, 0, mateData.Length);
        //        }
        //        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //        {
        //            if (response.StatusCode != HttpStatusCode.OK)
        //            {
        //                logger.Error("Copy directory failed.sourcePath=" + PathUtil.CombinePath(sourceInfo.path, sourceInfo.LowName));
        //                throw new Exception();
        //            }
        //            else
        //            {
        //                logger.Info("Copy directory succeed.sourcePath=" + PathUtil.CombinePath(sourceInfo.path, sourceInfo.LowName));
        //                copyResult.IsCopyed = true;
        //                XURIResult uri = new XURIResult();
        //                uri.SysId = this.SystemID;
        //                uri.SdType = 409;
        //                uri.SInfo = new StorageInfo();
        //                uri.SInfo.HighName = targetInfo.HighName;
        //                uri.SInfo.LowName = targetInfo.LowName;
        //                copyResult.URI = uri;
        //                return copyResult;
        //            }
        //        }
        //    });
        //}

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            try
            {
                StorageCopyResult copyResult = new StorageCopyResult();
                var sourceFullPath = PathUtil.CombinePath(PathUtil.CombinePath(this.SystemLocation, sourceFileInfo.HighName), sourceFileInfo.LowName);
                var targetFullPath = PathUtil.CombinePath(PathUtil.CombinePath(this.SystemLocation, targetFileInfo.HighName), targetFileInfo.LowName);
                if (sourceFullPath.Equals(targetFullPath))
                {
                    throw new Exception("Copy file failed,source file path is equal to the target file path.");
                }
                if (!FileExists(sourceFileInfo))
                {
                    throw new Exception("Copy file failed,source file isn't exist");
                }
                if (isOverWrite && FileExists(targetFileInfo))
                {
                    this.DeleteFile(targetFileInfo);
                }
                sourceFileInfo.path = String.Format("{0}/{1}", this.SystemLocation, sourceFileInfo.HighName);
                targetFileInfo.path = String.Format("{0}/{1}", this.SystemLocation, targetFileInfo.HighName);
                var url = String.Format(StorageUrl.EgnyteCopy, this.OpenParameter.Domain, EgnyteUtil.Encode(sourceFileInfo.path), EgnyteUtil.Encode(sourceFileInfo.LowName));
                return EgnyteUtil.Retry<StorageCopyResult>(delegate()
                {
                    HttpWebRequest request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_POST, url, this.OpenParameter.Token);
                    request.ContentType = "application/json";
                    StringBuilder stringBuilder = new StringBuilder();
                    if (!targetFileInfo.path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        targetFileInfo.path = "/" + targetFileInfo.path;
                    }
                    targetFileInfo.path = targetFileInfo.path.Replace('\\', '/');
                    stringBuilder.Append("{\"action\":\"").Append("copy\",").Append("\"destination\":").Append("\"")
                        .Append(targetFileInfo.path).Append("/").Append(targetFileInfo.LowName)
                        .Append("\"}");
                    Byte[] mateData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    request.ContentLength = mateData.Length;
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(mateData, 0, mateData.Length);
                    }
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(String.Format("Copy file failed.sourcePath = {0}, targetPath = {1}.", PathUtil.CombinePath(sourceFileInfo.path, sourceFileInfo.LowName), PathUtil.CombinePath(targetFileInfo.path, targetFileInfo.LowName)));
                        }
                        else
                        {
                            this.logger.Info("Copy file succeed.sourcePath = {0}, targetPath = {1}.", PathUtil.CombinePath(sourceFileInfo.path, sourceFileInfo.LowName), PathUtil.CombinePath(targetFileInfo.path, targetFileInfo.LowName));
                            copyResult.IsCopyed = true;
                            XURIResult uri = new XURIResult();
                            uri.SysId = this.SystemID;
                            uri.SdType = 409;
                            uri.SInfo = new StorageInfo();
                            uri.SInfo.HighName = targetFileInfo.HighName;
                            uri.SInfo.LowName = targetFileInfo.LowName;
                            copyResult.URI = uri;
                            return copyResult;
                        }
                    }
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (Exception e)
            {
                this.logger.Error("Copy file failed.Message = {0}", e.Message);
                throw;
            }
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).Files;
        }

        public XFileInfo ListFileVersion(StorageInfo info)
        {
            EgnyteObject egnyteObject = new EgnyteObject();
            return EgnyteUtil.Retry<XFileInfo>(delegate()
            {
                info.path = PathUtil.CombinePath(this.SystemLocation, info.HighName);
                var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, PathUtil.CombinePath(info.path, info.LowName));
                HttpWebRequest request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_GET, url, this.OpenParameter.Token);
                request.ContentType = "application/json";
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        this.logger.Error(String.Format("List file version failed.highName={0},lowName={1}", info.HighName, info.LowName));
                        throw new Exception();
                    }
                    else
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                this.logger.Info(String.Format("List file version succeed.highName={0},lowName={1}", info.HighName, info.LowName));
                                var result = streamReader.ReadToEnd();
                                egnyteObject = EgnyteUtil.ParseJsonString(result);
                            }
                        }
                    }
                }
                XFileInfo fileInfo = new EgnyteFileInfo(info.HighName, info.LowName, this, egnyteObject);
                return fileInfo;
            }, this.MaxRetryCount, this.RetryInterval);
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            EgnyteObject egnyteObject = new EgnyteObject();
            dirInfo.path = PathUtil.CombinePath(this.SystemLocation, dirInfo.HighName);
            var totalName = String.Format("{0}/{1}", dirInfo.path, dirInfo.LowName);
            return EgnyteUtil.Retry<StorageListResult>(delegate()
            {
                StorageListResult listResult = new StorageListResult();
                var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(dirInfo.path, dirInfo.LowName)));
                HttpWebRequest request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_GET, url, this.OpenParameter.Token);
                request.ContentType = "application/json";
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        this.logger.Error(String.Format("List directory and file failed. highName={0},lowName={1}.", dirInfo.HighName, dirInfo.LowName));
                        throw new Exception();
                    }
                    else
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                this.logger.Info(String.Format("List directory and file succeed. highName={0},lowName={1}.", dirInfo.HighName, dirInfo.LowName));
                                var result = streamReader.ReadToEnd();
                                egnyteObject = EgnyteUtil.ParseJsonString(result);
                            }
                        }
                    }
                }
                if (egnyteObject.Files != null)
                {
                    foreach (var file in egnyteObject.Files)
                    {
                        listResult.Files.Add(new EgnyteFileInfo(PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName), file.Name, this, file));
                    }
                }
                if (egnyteObject.Folders != null)
                {
                    foreach (var folder in egnyteObject.Folders)
                    {
                        listResult.SubDirs.Add(new EgnyteFolderInfo(PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName), folder.Name, this, folder));
                    }
                }
                return listResult;
            }, this.MaxRetryCount, this.RetryInterval);
        }

        private XDirectoryInfo CreateNewFolder(StorageInfo info)
        {
            try
            {
                info.path = PathUtil.CombinePath(this.SystemLocation, info.HighName);
                var url = String.Format(StorageUrl.Egnyte, this.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(info.path, info.LowName)));
                return EgnyteUtil.Retry<XDirectoryInfo>(delegate()
                {
                    HttpWebRequest request = EgnyteUtil.GenerateRequest(EgnyteConstants.HttpMethod_POST, url, this.OpenParameter.Token);
                    StringBuilder stringBuild = new StringBuilder();
                    stringBuild.Append("{\"action\":").Append("\"add_folder\"").Append("}");
                    request.ContentType = "application/json";
                    Byte[] matedata = Encoding.UTF8.GetBytes(stringBuild.ToString());
                    request.ContentLength = matedata.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(matedata, 0, matedata.Length);
                    }
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            this.logger.Info(String.Format("Create folder succeed. highName={0},lowName={1}", info.HighName, info.LowName));
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    request.Abort();
                    return new EgnyteFolderInfo(info.HighName, info.LowName, this, new EgnyteObject());
                }, this.MaxRetryCount, this.RetryInterval);
            }
            catch (Exception e)
            {
                this.logger.Error(String.Format("Create new folder failed.highName={0},lowName={1},Message={2}", info.HighName, info.LowName, e.Message));
                return null;
            }
        }

        //public User CreateUser()
        //{
        //    var url = "https://4test.egnyte.com/pubapi/v1/users";
        //    HttpWebRequest request = EgnyteUtil.GenerateRequest("POST", url, this.OpenParameter.Token);
        //    request.ContentType = " application/json";
        //    StringBuilder stringBuilder = new StringBuilder();
        //    stringBuilder.Append("{\"userName\": \"jmiller\"").Append("\"externalId\": \"S-1-5-21-3623811015-3361044348-30300820-1013\"")
        //        .Append("\"email\": \"jmiller@example.com\"")
        //        .Append("\"name\": {\"familyName\": \"Miller\" \"givenName\": \"John\"}")
        //        .Append("\"active\" true");
        //    Byte[] mateData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
        //    request.ContentLength = mateData.Length;
        //    try
        //    {
        //        using (Stream stream = request.GetRequestStream())
        //        {
        //            stream.Write(mateData, 0, mateData.Length);
        //        }
        //        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //        {
        //            using (Stream st = response.GetResponseStream())
        //            {
        //                using (StreamReader sr = new StreamReader(st))
        //                {
        //                    var stri = sr.ReadToEnd();
        //                }
        //            }
        //        }
        //    }
        //    catch (WebException e)
        //    {

        //    }
        //    return new User();
        //}

        public override void Close()
        {
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
    }
}
