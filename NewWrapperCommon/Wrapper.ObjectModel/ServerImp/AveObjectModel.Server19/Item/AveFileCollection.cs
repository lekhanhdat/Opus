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
using System.IO;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.ObjectModel.Server19.NonPublicAPI;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using AvePoint.GCommon.Contract.CodeReview;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace AvePoint.ObjectModel.Server19
{
    class AveFileCollection : AveAbstractCommonCollection<IAveFile>, IAveFileCollection, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveFileCollection));

        private const string mFiles_AddGhosted_Method = "AddGhosted";
        private SPFileCollection mFiles;
        private AveWeb mWeb;
        private AveSite mSite;
        private AveFolder mFolder;
        private AveDocumentSerializer mDocumentSerializer;

        public AveFileCollection(AveFolder folder, SPFileCollection files)
            : base(files)
        {
            mFolder = folder;
            mWeb = folder.ParentWeb as AveWeb;
            mSite = mWeb.Site as AveSite;
            mFiles = files;
        }

        #region IAveFileCollection Members

        public IAveFile Add(AveFileCreationInformation parameters)
        {
            return new AveFile(mWeb, mFiles.Add(SPResourcePath.FromDecodedUrl(parameters.Url), parameters.Content, new SPFileCollectionAddParameters { Overwrite = parameters.Overwrite }));
        }

        public IAveFile Add(string urlOfFile, AveTemplateFileType templateFileType)
        {
            return new AveFile(mWeb, mFiles.Add(urlOfFile, (SPTemplateFileType)templateFileType));
        }

        public IAveFile Add(string urlOrFile, Stream file, bool overwrite)
        {
            return new AveFile(mWeb, mFiles.Add(SPResourcePath.FromDecodedUrl(urlOrFile), file, new SPFileCollectionAddParameters {Overwrite= overwrite }));
        }

        public IAveFile this[string urlOrFile]
        {
            get
            {
                return new AveFile(mWeb, mFiles[urlOrFile]);
            }
        }

        public IAveFile Add(string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            return new AveFile(mWeb, mFiles.Add(SPResourcePath.FromDecodedUrl(urlOfFile), file, new SPFileCollectionAddParameters { Overwrite = overwrite, CheckInComment = checkInComment, CheckRequiredFields = checkRequiredFields }));
        }

        public IAveFile Add(string urlOfFile, System.IO.Stream file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            return new AveFile(mWeb, mFiles.Add(SPResourcePath.FromDecodedUrl(urlOfFile), file, new SPFileCollectionAddParameters { Overwrite = overwrite, CheckInComment = checkInComment, CheckRequiredFields = checkRequiredFields }));
        }

        public IAveFile Add(string url, byte[] file, bool overwrite)
        {
            return new AveFile(mWeb, mFiles.Add(SPResourcePath.FromDecodedUrl(url), file, new SPFileCollectionAddParameters { Overwrite = overwrite }));
        }

        public IAveFile Add(string urlOfFile, byte[] file)
        {
            return new AveFile(mWeb, mFiles.Add(SPResourcePath.FromDecodedUrl(urlOfFile), file, new SPFileCollectionAddParameters()));
        }

        public IAveFile AddGhosted(string sourceFilePath, string targetFilePath, bool bIsPublishing)
        {
            object[] paramObjs = new object[] { sourceFilePath, targetFilePath, bIsPublishing };
            return new AveFile(mWeb, (SPFile)AveAssemblyUtility.InvokeMethod(mFiles, mFiles_AddGhosted_Method, paramObjs));
        }

        public override IAveFile this[int index]
        {
            get
            {
                return new AveFile(mWeb, mFiles[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveFile(mWeb, t as SPFile);
        }

        public override int Count
        {
            get { return mFiles.Count; }
        }

        public IAveFolder Folder
        {
            get
            {
                return mFolder;
            }
        }

        public IAveWeb Web
        {
            get { return mWeb; }
        }

        public IAveDocumentSerializer DocumentSerializer
        {
            get
            {
                if (mDocumentSerializer == null)
                {
                    mDocumentSerializer = new AveDocumentSerializerV1(this);
                }
                return mDocumentSerializer;
            }
        }

        #endregion

        public bool IsDirty
        {
            get
            {
                object result = AveAssemblyUtility.GetPropertyValue(mFiles, "IsDirty");
                return result != null ? (bool)result : false;
            }
        }

        private void RestoreHoldsStatus()
        {

        }

        /// <summary>
        /// Nothing happened in this function.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="file"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool ChangeContent(IAveFile file, AveDocumentInfo info)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFileCollection.ChangeContent"))
            {
                if (info.Version == file.UIVersion)
                {
                    bool changed = false;
                    AveFile spFile = file as AveFile;
                    AveSPDocContentReplacer replacer = new AveSPDocContentReplacer(mSite, spFile.File, info);
                    Stream stream = replacer.ReplaceWebPartContent(out changed);
                    if (changed && stream != null)
                    {
                        try
                        {
                            if (mSite.NativeApiPermission != WrapperNativeApiPermission.FullControl &&
                                (!spFile.InDocumentLibrary || spFile.File.Item == null))
                            {
                                spFile.File.SaveBinaryExtension(stream);
                                return true;
                            }
                            SPUser author = null;
                            SPUser editor = null;
                            if (!spFile.InDocumentLibrary || spFile.File.Item == null)
                            {
                                author = spFile.File.Author;
                                editor = spFile.File.ModifiedBy;
                            }
                            else
                            {
                                //SPFile.Author和SPFile.ModifiedBy是从SPFile.Property中获取value，而界面上显示的是对应的Column Value，出现过两个Value不一致的情况
                                //因此从相关Column中find对应的user信息
                                var columnValue = new SPFieldUserValue(spFile.File.ParentFolder.ParentWeb, spFile.File.Item[SPBuiltInFieldId.Author].ToString());
                                author = columnValue.User;
                                columnValue = new SPFieldUserValue(spFile.File.ParentFolder.ParentWeb, spFile.File.Item[SPBuiltInFieldId.Editor].ToString());
                                editor = columnValue.User;
                            }
                            info.DTimeLastModified = spFile.TimeLastModified;
                            info.DTimeCreated = spFile.TimeCreated;
                            info.Level = (int)spFile.Level;

                            spFile.SaveBinaryWithoutIncreasingVersion(stream);

                            if (mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                            {
                                mSite.QueryService.UpdateAllDocsPropertyByNative(info, info.DTimeCreated, info.DTimeLastModified, spFile.UIVersion);
                                mSite.QueryService.UpdateSpecialPropertyByNative(editor.ID.ToString(), author.ID.ToString(), info.DTimeLastModified, info.DTimeCreated, info);
                            }
                            else
                            {
                                //当Agent Account没有DBO权限的时候，只有真正在Document Library里面的非系统文件才需要更新4个相关的Column
                                //上面的判断已经处理了系统文件替换content的情况，这个地方就不需要再次判断
                                //if (spFile.InDocumentLibrary && spFile.File.Item != null)
                                //{
                                //SPFile.TimeLastModified is UTC Time
                                //Info中的DateTime都是UTC时间，使用API更新的时候，需要转换成对应的Local Time
                                SPTimeZone zone = spFile.File.ParentFolder.ParentWeb.RegionalSettings.TimeZone;
                                spFile.File.Item[SPBuiltInFieldId.Author] = author;
                                spFile.File.Item[SPBuiltInFieldId.Editor] = editor;
                                spFile.File.Item[SPBuiltInFieldId.Modified] = zone.UTCToLocalTime(info.DTimeLastModified);
                                spFile.File.Item[SPBuiltInFieldId.Created] = zone.UTCToLocalTime(info.DTimeCreated);
                                if (!AveItem.AveItemSystemUpdate(spFile.File.Item, false, true, info.Level == 1, true))
                                {
                                    logger.Log(AveLogLevel.WARN, "Failed to internal update file basic info while replacing file content. File url:{0}", spFile.ServerRelativeUrl);
                                }
                            }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("An error occurred while replacing WebPart in content in post action, source version: {0}, current file version: {1}, file url:{2}, error: {3}", file.UIVersion, info.Version, spFile.ServerRelativeUrl, ex);
                            return false;
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Dispose();
                            }
                        }
                    }
                }
                return false;
            }
        }

        public IAveFile AddStreamInternal(string urlOfFile, Stream stream, bool bIsMigrate, bool bIsPublish, bool bcheckRequiredProps, bool bAutoCheckoutOnInvalidData, bool bForceCreateVersion, string lockIdMatch, IAveUser createdBy, IAveUser modifiedBy, DateTime timeCreated, DateTime timeLastModified, object varProperties, string checkinComment, bool bOverwrite, Stream formatMetadata, string etagToMatch, bool bSyncUpdate, out AveVirusCheckStatus virusCheckStatus, out string virusCheckMessage, out string etagNew)
        {
            AveVirusCheckStatus _virusCheckStatus = AveVirusCheckStatus.Clean;
            string _virusCheckMessage = string.Empty;
            string _etagNew = string.Empty;
            Type tSPFileStreamManager = AveAssemblyUtility.GetType("Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.SPFileStreamManager");
            Type[] types = new Type[] { typeof(string), typeof(Stream), tSPFileStreamManager, typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(string), typeof(SPUser), typeof(SPUser), typeof(DateTime), typeof(DateTime), typeof(object), typeof(string), typeof(bool), typeof(Stream), typeof(string), typeof(bool), typeof(SPVirusCheckStatus).MakeByRefType(), typeof(string).MakeByRefType(), typeof(string).MakeByRefType() };
            object[] objs = new object[] { urlOfFile, stream, null, bIsMigrate, bIsPublish, bcheckRequiredProps, bAutoCheckoutOnInvalidData, bForceCreateVersion, lockIdMatch, (createdBy as AveUser).User, (modifiedBy as AveUser).User, timeCreated, timeLastModified, varProperties, checkinComment, bOverwrite, formatMetadata, etagToMatch, bSyncUpdate, (SPVirusCheckStatus)_virusCheckStatus, _virusCheckMessage, _etagNew };
            SPFile file = (SPFile)AveAssemblyUtility.InvokeMethod(mFiles, "AddStreamInternal", types, objs);
            virusCheckStatus = AveTypeHelper.CastEnumValue<AveVirusCheckStatus>(objs[objs.Length - 3]);
            virusCheckMessage = (string)objs[objs.Length - 2];
            etagNew = (string)objs[objs.Length - 1];
            return new AveFile(mWeb, file);
        }


        public IAveFile Add(string urlOfFile, byte[] file, Hashtable properties, IAveUser createdBy, IAveUser modifiedBy, DateTime timeCreated, DateTime timeLastModified, bool overwrite)
        {
            return new AveFile(mWeb, mFiles.Add(SPResourcePath.FromDecodedUrl( urlOfFile),new MemoryStream(file), properties, ((AveUser)createdBy).User, ((AveUser)modifiedBy).User, timeCreated, timeLastModified,string.Empty, overwrite,true));
        }

        public void Dispose()
        {
            if (mDocumentSerializer != null)
            {
                mDocumentSerializer.Dispose();
            }
        }
    }
}