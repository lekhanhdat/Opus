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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPAttachment : RestoreableObject, AvePoint.Wrapper.Restore.IAveSPAttachment, ISPAttachmentImport
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveSPAttachment));
        protected AveAttachmentInfo mAttachmentInfo = new AveAttachmentInfo();
        private AveSPFolder mAveRootItemParentFolder;
        private IAveBackupRestoreQueryService mQueryService;
        private IAveRestoreStream mReceiver;
        private AveSPItem mAveSPItem;
        protected IAveAttachment mAttachment;
        private AveSPSite mParentSite;
        private IAveListItem mRootItem;

        private AveStorageInfo mStorageInfo;
        private AveSPWeb mAveWeb;
        private AveSPListItem mAveSPListItem;

        private static object lockObj = new object();
        public AveSPFolder ParentFolder
        {
            get { return mAveRootItemParentFolder; }
        }

        public IAveListItem ListItem
        {
            get { return mRootItem; }
        }

        public string Name
        {
            get { return mAttachmentInfo.RealName; }
        }
        private Guid ParentId
        {
            get
            {
                if (mAttachmentInfo.ParentId == Guid.Empty)
                {
                    mAttachmentInfo.ParentId = mAttachment.GetParentId();
                }
                return mAttachmentInfo.ParentId;
            }
        }

        public string SrcUrl
        {
            get
            {
                return mAttachmentInfo.SrcUrl;
            }
        }

        public string Url
        {
            get
            {
                return mAttachmentInfo.Url;
            }
        }

        public long Size
        {
            get
            {
                return mAttachmentInfo.Size;
            }
        }

        public AveSPAttachment(AveSPWeb aveWeb, AveSPListItem aveSPItem, IAveRestoreStream aveRestoreStream)
        {
            mAveWeb = aveWeb;
            mAveSPListItem = aveSPItem;
            mReceiver = aveRestoreStream;
            mParentSite = mAveWeb.ParentSite;
            mAveRootItemParentFolder = aveSPItem.ParentFolder;
        }

        public AveSPAttachment(AveSPFolder parent, string name)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAttachment.Constructor"))
            {

            mAveRootItemParentFolder = parent;
            mQueryService = parent.QueryService;
            mAttachmentInfo.FullName = name;
            mAttachmentInfo.RealName = mAttachmentInfo.FullName.Substring(mAttachmentInfo.FullName.LastIndexOf(':') + 1);
            int rowId = Convert.ToInt32(mAttachmentInfo.FullName.Substring(0, mAttachmentInfo.FullName.IndexOf("_.", StringComparison.OrdinalIgnoreCase)));
            int tempId = parent.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(parent.ParentList.SPList.ID, rowId);
            if (tempId != -1)
            {
                rowId = tempId;
            }
            mRootItem = mAveRootItemParentFolder.ParentList.SPList.GetItemById(rowId);
            InitializeAttachmentInfo(parent);
            mParentSite = parent.ParentSite;


            }

        }
        //replicator 使用
        public AveSPAttachment(AveSPFolder parent, AveSPListItem listItem, string name)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAttachment.Constructor"))
            {

            if (parent.ParentList.ParentWeb.ReloadWebAndParentInternalForSPRequestTimeout(false))
            {
                parent.ParentList.ReloadList();
            }
            mAveRootItemParentFolder = parent;
            mQueryService = parent.QueryService;
            mAttachmentInfo.RealName = name.Substring(name.LastIndexOf(':') + 1);
            mRootItem = listItem.SPListItem;
            InitializeAttachmentInfo(parent);
            mParentSite = parent.ParentSite;

            }

        }
        public void InitializeAttachmentInfo(AveSPFolder parent)
        {
            mAttachmentInfo.ListId = parent.ParentList.SPList.ID;
            mAttachmentInfo.SiteId = parent.ParentSite.SPSite.ID;
            mAttachment = parent.ParentSite.ObjectModelFactory.CreateAttachment(mAttachmentInfo, mRootItem);
            mAttachmentInfo.Attachment = mAttachment;
            mAttachmentInfo.MappingManager = parent.ParentSite.MappingManager;
        }
        //used for replicator discussionBoard
        public AveSPAttachment(AveSPFolder parent, AveSPFolder folder, string name)
        {
            mAveRootItemParentFolder = parent;
            mQueryService = parent.QueryService;
            mAttachmentInfo.RealName = name.Substring(name.LastIndexOf(':') + 1);
            mRootItem = folder.SPFolder.Item;
            if (mRootItem != null)
            {
                //TODO: It's related with SharePoiont language. Need to be confirmed.
                mRootItem = folder.SPFolder.ParentList.GetItemById(mRootItem.ID);
            }
            InitializeAttachmentInfo(parent);
            mParentSite = parent.ParentSite;
        }

        public void SetStream(IAveRestoreStream stream)
        {
            mReceiver = stream;
        }
        /// <summary>
        /// Add attachment
        /// </summary>
        /// <exception>
        /// ArgumentNullException, content size of attachment is 0.
        /// </exception>
        public void AddAttachment()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAttachment.AddAttachment"))
            {

            mAttachmentInfo.DocumentSize = mReceiver.ContentLength;
            mAttachmentInfo.WebappBlockTypes = ParentFolder.ParentList.ParentWeb.ParentSite.webappBlockTypes;
            IAveWebApplication Webapp = ParentFolder.ParentList.ParentWeb.ParentSite.SPSite.WebApplication;
            if (mAttachmentInfo.WebappBlockTypes == null && Webapp != null)
            {
                mAttachmentInfo.WebappBlockTypes = Webapp.BlockedFileExtensions;
            }
            if (mAttachmentInfo.WebappBlockTypes != null && !string.IsNullOrEmpty(mAttachmentInfo.RealName))
            {
                string[] names = mAttachmentInfo.RealName.Split('.');
                if (mAttachmentInfo.WebappBlockTypes.Contains(names[names.Length - 1]))
                {
                    throw new Exception(string.Format("{0} has been blocked from this Web site by the server administrators.", mAttachmentInfo.RealName));
                }
            }
            //Need set value of EnableAttachments property to true if parent list template is Posts.
            //Although it value still show false but after reset value to true the Posts list can add attachment. 
            if (mRootItem.ParentList.BaseTemplate == AveListTemplateType.Posts)
            {
                mRootItem.ParentList.EnableAttachments = true;
                try
                {
                    mRootItem.ParentList.Update();
                }
                catch (Exception e)
                {
                    log.Warn("Update List after set EnableAttachments to true with exception :" + e.ToString());
                }
            }
            else
            {
                if (!mRootItem.ParentList.EnableAttachments)
                {
                    ParentFolder.RestoringItem.NeedSkipped = true;
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_NotEnableAttachment, mRootItem.ParentList.Title);
                }
            }
            mAveSPItem = new AveSPItem(AveItemType.Attachement, mAveRootItemParentFolder, "");
            mAttachmentInfo.AveItem = mParentSite.ObjectModelFactory.CreateAveItem(mAttachmentInfo, mAveRootItemParentFolder.SPFolder, mAveRootItemParentFolder.ParentWeb.SPWeb, mAveRootItemParentFolder.ParentList.SPList);
            mRootItem.Attachments.RestoreAttachment(mAttachmentInfo, mReceiver);

            }

        }
        //修改Alldocs中对应的TimeCreated和TimeLastModified字段。
        public void UpdateAllDocsPropertyByNative(DateTime timeCreated, DateTime timeLastModified)
        {
            try
            {
                mQueryService.UpdateAllDocsPropertyByNative(timeCreated, timeLastModified, ParentId, mAveRootItemParentFolder.ParentList.ParentWeb.ParentSite.SPSite.ID, mAttachmentInfo.RealName);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateAttachmentFailed, e);
            }
        }
        public void Delete()
        {
            if (this.ParentFolder.ParentList.ParentWeb.ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
            {
                this.ParentFolder.ParentList.BackupListSetting();
                if (this.ParentFolder.ParentList.SPList.EnableVersioning)
                {
                    this.ParentFolder.ParentList.SPList.EnableVersioning = false;
                    lock (lockObj)
                    {
                        this.ParentFolder.ParentList.SPList.Update(); 
                    }
                    this.ParentFolder.ParentList.SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
                }
            }
            if (mAttachment == null)
            {
                foreach (IAveAttachment att in mRootItem.Attachments)
                {
                    if (att.FileName == mAttachmentInfo.RealName)
                    {
                        mAttachment = att;
                        break;
                    }
                }
            }
            mAttachment.Delete();
        }

        protected bool TryGetAttachment()
        {
            if (this.mAttachment != null)
            {
                return true;
            }
            foreach (IAveAttachment att in mRootItem.Attachments)
            {
                if (att.FileName == mAttachmentInfo.RealName)
                {
                    this.mAttachment = att;
                    break;
                }
            }
            return this.mAttachment != null;
        }

        public bool IsAttachmentExists()
        {
            return mRootItem.Attachments.Exists(mAttachmentInfo);
        }

        //public void RestoreAttachment(AveAttachmentInfo attachmentInfo)
        //{
        //    mIsRestored = true;
        //    IAveListItem listItem = mAveSPItem.SPListItem;
        //    string relativeUrl = listItem.Attachments.AddNow(attachmentInfo.LeafName, new byte[1]);
        //    IAveFile aveFile = mAveWeb.SPWeb.GetFile(mAveWeb.SPWeb.ServerRelativeUrl + AveProtocolHeaderConstants.URL_SEPERATOR + relativeUrl);
        //    AveSPFileStream fileStream = new AveSPFileStream(mReceiver);
        //    aveFile.SaveBinary(fileStream);
        //    mAttachmentInfo.Url = aveFile.ServerRelativeUrl;
        //    mAttachmentInfo.SrcUrl = attachmentInfo.SrcUrl;
        //    mAttachmentInfo.Size = attachmentInfo.Size;
        //}
        public string ResetAvailableName()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetAvailableName"))
            {

                try
                {
                    if (!IsAttachmentExists())
                    {
                        return mAttachmentInfo.RealName;
                    }
                    string extension = string.Empty;
                    string prevName = mAttachmentInfo.RealName;
                    int pos = mAttachmentInfo.RealName.LastIndexOf('.');
                    if (pos > 0)
                    {
                        extension = mAttachmentInfo.RealName.Substring(pos, mAttachmentInfo.RealName.Length - pos);
                        prevName = mAttachmentInfo.RealName.Substring(0, pos);
                    }
                    for (int i = 1; i <= 1000; ++i)
                    {
                        StringBuilder temp = new StringBuilder(prevName);
                        temp.Append("_");
                        temp.Append(i.ToString());
                        temp.Append(extension);

                        mAttachmentInfo.RealName = temp.ToString();
                        if (!IsAttachmentExists())
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("ResetAvailableName error. \n error message:{0}", e));
                    //mLog.Warn("ResetAvailableName Error: " + e.ToString());
                }
                return mAttachmentInfo.RealName;

            }

        }
        public void ResetName(string newName)
        {
            mAttachmentInfo.RealName = newName;
        }
        /// <summary>
        /// Check if the attachment is too large
        /// </summary>
        /// <returns></returns>
        public bool IsAttachmentsSizeAllowed()
        {
            bool isAllowed = true;
            int sourceSize = (int)Math.Ceiling((double)mReceiver.ContentLength / (double)(1024 * 1024));
            if (ParentFolder.ParentList.ParentWeb.ParentSite.SPContextKind != AveContextKind.ClientObjectModel)
            {
                if (sourceSize <= ParentFolder.ParentList.ParentWeb.ParentSite.SPSite.WebApplication.MaximumFileSize)
                {
                    isAllowed = true;
                }
                else
                {
                    isAllowed = false;
                }
            }
            return isAllowed;
        }

        #region IAveSPAttachment Members


        public void InitializeAttachmentInfo(IAveSPFolder parent)
        {
            this.InitializeAttachmentInfo(parent as AveSPFolder);
        }

        IAveSPFolder IAveSPAttachment.ParentFolder
        {
            get
            {
                return mAveRootItemParentFolder;
            }
        }

        #endregion
        public void Dispose()
        {
            mAveSPItem.Dispose();
        }

        public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPAttachmentRestoreOption spAttachmentRestoreOption)
        {
            if (restoreStream == null)
            {
                throw new ArgumentNullException("restoreStream");
            }

            if (spAttachmentRestoreOption == null)
            {
                throw new ArgumentNullException("spAttachmentRestoreOption");
            }

            var attachmentRestoreDto = new AveAttachmentRestoreHelper.AveSPAttachmentRestoreDto()
                {
                    SPAttachment = this,
                    SPAttachmentRestoreOption = spAttachmentRestoreOption,
                };

            return AveAttachmentRestoreHelper.RestoreAttachment(restoreStream, attachmentRestoreDto);
        }
    }

    /// <summary>
    /// Attachment Restore Healper
    /// </summary>
    static class AveAttachmentRestoreHelper
    {
        /// <summary>
        /// 临时使用
        /// </summary>
        internal sealed class AveSPAttachmentRestoreDto
        {
            public AveSPAttachment SPAttachment;
            public SPAttachmentRestoreOption SPAttachmentRestoreOption;
            public AveMetadata Metadata;
        }

        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(AveAttachmentRestoreHelper));

        private static readonly Dictionary<AveMetadataType, RestoreAction<AveSPAttachmentRestoreDto, MetadataRestoreReport>> restoreActions = new Dictionary<AveMetadataType, RestoreAction<AveSPAttachmentRestoreDto, MetadataRestoreReport>>
                {
                    {AveMetadataType.DocProperty, RestoreDocProperty},
                    {AveMetadataType.DocStorageInfo, RestoreDocStorageInfo}
                };

        private static MetadataRestoreReport RestoreDocProperty(AveSPAttachmentRestoreDto restoreDto)
        {
            return RestoreActionExecutor.ExecuteAction(AveMetadataType.DocProperty, restoreDto.SPAttachmentRestoreOption.IncludePerformanceDetails,
                () =>
                    {
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        data = restoreDto.Metadata.GetMetadata<Dictionary<string, object>>();
                        restoreDto.SPAttachment.UpdateAllDocsPropertyByNative((DateTime)data["Created"], (DateTime)data["Modified"]);
                        return new MetadataRestoreDetails();
                    });
        }

        private static MetadataRestoreReport RestoreDocStorageInfo(AveSPAttachmentRestoreDto restoreDto)
        {
            // 什么也不用做，AveMetadataType.DocStorageInfo在之前由AddAttachment处理。
            return new MetadataRestoreReport(AveMetadataType.DocStorageInfo)
                {
                    Details = new MetadataRestoreDetails()
                        {
                            Message = "Do nothing,AveMetadataType.DocStorageInfo has been handled in AddAttachment method.",
                            Status = WrapperRestoreStatus.Skipped
                        }
                };
        }

        /// <summary>
        /// Handle Metadata
        /// </summary>
        /// <param name="restoreDto"></param>
        /// <returns></returns>
        private static MetadataRestoreReport HandleMetadata(AveSPAttachmentRestoreDto restoreDto)
        {
            RestoreAction<AveSPAttachmentRestoreDto, MetadataRestoreReport> restoreAction = null;

            if (restoreActions.TryGetValue(restoreDto.Metadata.MetadataType, out restoreAction))
            {
                return restoreAction(restoreDto);
            }
            else
            {
                logger.Error("Cannot handle this type:{0}", restoreDto.Metadata.MetadataType);
                //TODO 以后需要处理这个
            }

            return null;
        }

        /// <summary>
        /// restore attachment
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="restoreDto"></param>
        /// <returns></returns>
        internal static SPFileRestoreReport RestoreAttachment(IAveRestoreStream restoreStream, AveSPAttachmentRestoreDto restoreDto)
        {
            if (restoreStream == null)
            {
                throw new ArgumentNullException("restoreStream");
            }

            if (restoreDto == null)
            {
                throw new ArgumentNullException("AttachmentRestoreDto");
            }

            var restoreReport = new SPFileRestoreReport();

            using (WrapperStopwatch.CreateInstance(restoreDto.SPAttachmentRestoreOption.IncludePerformanceDetails,
                                                restoreReport.UpdateTimeUsage))
            {
                if (restoreDto.SPAttachment.IsAttachmentExists())
                {
                    if (restoreDto.SPAttachment.CheckRestoreOption(AveRestoreMode.OverWrite))
                    {
                        restoreDto.SPAttachment.Delete();
                    }
                    else
                    {
                        return restoreReport;
                    }
                }
                restoreDto.SPAttachment.SetStream(restoreStream);
                restoreDto.SPAttachment.AddAttachment();

                AveMetadata metadata = null;

                while ((metadata = restoreStream.ReadMetadata()) != null)
                {
                    restoreDto.Metadata = metadata;
                    restoreReport.Add(metadata.MetadataType, AveAttachmentRestoreHelper.HandleMetadata(restoreDto));
                }

                return restoreReport;
            }
        }
    }

    //internal class AveSPAttachmentV1 : AveSPAttachment, ISPAttachmentImport
    //{
    //    private readonly AveSPWebV1 parentWeb;
    //    private readonly AveSPListV1 parentList;
    //    private readonly AveSPFolderV1 parentFolder;
    //    private readonly AveSPListItem parentListItem;

    //    public AveSPAttachmentV1(AveSPWebV1 parentWeb, AveSPListItem parentListItem, IAveRestoreStream aveRestoreStream)
    //        : base(parentWeb, parentListItem, aveRestoreStream)
    //    {
    //        this.parentWeb = parentWeb;
    //        this.parentListItem = parentListItem;
    //    }

    //    public AveSPAttachmentV1(AveSPFolderV1 parentFolder, string name)
    //        : base(parentFolder, name)
    //    {
    //        this.parentFolder = parentFolder;
    //    }

    //    public AveSPAttachmentV1(AveSPFolderV1 parentFolder, AveSPListItem parentListItem, string name)
    //        : base(parentFolder, parentListItem, name)
    //    {
    //        this.parentFolder = parentFolder;
    //        this.parentListItem = parentListItem;
    //    }

    //    /// <summary>
    //    /// Restore attachment
    //    /// 
    //    /// 这个是新加的接口,外围请暂时不要调用
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="spAttachmentRestoreOption"></param>
    //    /// <returns></returns>
    //    /// 
    //    public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPAttachmentRestoreOption spAttachmentRestoreOption)
    //    {
    //        if (restoreStream == null)
    //        {
    //            throw new ArgumentNullException("restoreStream");
    //        }

    //        if (spAttachmentRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("spFolderRestoreOption");
    //        }

    //        var restoreReport = new SPFileRestoreReport();

    //        using (WrapperStopwatch.CreateInstance(spAttachmentRestoreOption.IncludePerformanceDetails, restoreReport.UpdateTimeUsage))
    //        {
    //            if (this.TryGetAttachment())
    //            {
    //                HandlAttachmentConflict(spAttachmentRestoreOption);
    //            }
    //            ProcessSourceMetadata(spAttachmentRestoreOption);

    //            this.AddAttachment();

    //            AveMetadata metadata = null;
    //            while ((metadata = restoreStream.ReadMetadata()) != null)
    //            {
    //                var action = GetAction(metadata.MetadataType);

    //                if (action != null)
    //                {
    //                    var metadataRestoreReport = new MetadataRestoreReport(metadata.MetadataType);
    //                    using (WrapperStopwatch.CreateInstance(spAttachmentRestoreOption.IncludePerformanceDetails, metadataRestoreReport.AddTimeUsage))
    //                    {
    //                        action(restoreStream, spAttachmentRestoreOption, metadata, metadataRestoreReport);
    //                    }

    //                    restoreReport.Add(metadata.MetadataType, metadataRestoreReport);
    //                }
    //                else
    //                {
    //                    //log.Error("There is no action for {0}, please submit a request for this type.", metadata.MetadataType);
    //                }
    //            }
    //        }

    //        return restoreReport;
    //    }

    //    private void ProcessSourceMetadata(SPAttachmentRestoreOption option)
    //    {
    //        if (option.ProcessAttachmentFunc != null)
    //        {
    //            SPAttachmentMetadataDto sourceData = new SPAttachmentMetadataDto() { Name = this.mAttachmentInfo.RealName };
    //            option.ProcessAttachmentFunc(sourceData);
    //            this.mAttachmentInfo.RealName = sourceData.Name;
    //            sourceData = null;
    //        }
    //    }

    //    private void HandlAttachmentConflict(SPAttachmentRestoreOption option)
    //    {
    //        SPItemRestoreAction action = SPItemRestoreAction.Default;
    //        if (option.AttachmentConflictHandlFunc != null)
    //        {
    //            action = option.AttachmentConflictHandlFunc(this.mAttachment);
    //        }
    //        if (action == SPItemRestoreAction.Skip)
    //        {
    //            //log
    //            //OmitException
    //            throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, "Skip");
    //        }
    //        if (this.CheckRestoreOption(AveRestoreMode.OverWrite) || action == SPItemRestoreAction.Overwrite)
    //        {
    //            this.Delete();
    //        }
    //    }

    //    private Action<IAveRestoreStream, SPAttachmentRestoreOption, AveMetadata, MetadataRestoreReport> GetAction(AveMetadataType metadataType)
    //    {
    //        Action<IAveRestoreStream, SPAttachmentRestoreOption, AveMetadata, MetadataRestoreReport> action = null;

    //        switch (metadataType)
    //        {
    //            case AveMetadataType.DocProperty:
    //                action = RestoreAttachmentDocProperty;
    //                break;
    //            case AveMetadataType.DocStorageInfo:
    //                action = RestoreAttachmentDocStoreageInfo;
    //                break;
    //            default:
    //                break;
    //        }
    //        return action;
    //    }

    //    private void RestoreAttachmentDocProperty(IAveRestoreStream restoreStream, SPAttachmentRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        var docData = metadata.GetMetadata<Dictionary<string, object>>();
    //        this.UpdateAllDocsPropertyByNative((DateTime)docData["Created"], (DateTime)docData["Modified"]);
    //    }

    //    private void RestoreAttachmentDocStoreageInfo(IAveRestoreStream restoreStream, SPAttachmentRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
          
    //            var docData = metadata.GetMetadata<Dictionary<string, object>>();
    //            if (docData != null && docData.ContainsKey("Size"))
    //            {
    //                //mDataSize = Convert.ToInt64(data["Size"]);
    //                //report.Details.AnalyzeReport(Size = Convert.ToInt64(docData["Size"]);
    //            }
    //    }
    //}
}
