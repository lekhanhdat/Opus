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


using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Common;
using System;
using System.Reflection;
using AvePoint.GCommon;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Wrapper.Common.Office;
using System.Xml;
using System.Web;
using AvePoint.GCommon.Utility;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.RACommonUtility;
using System.Linq;


namespace AvePoint.RA.SharePoint.Archiver.Move
{
    public class SPMoveDocImport : AveSPDoc, IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal AveSPSite parentSite;
        internal AveSPWeb parentWeb;
        internal AveSPList parentList;
        internal AveSPFolder parentFolder;
        internal IAveORecords record;
        internal string fileName;
        internal string desUrl;
        internal bool isOneDriveSite;

        public SPMoveDocImport(AveSPFolder parentFolder, IAveORecords record, string name, string desUrl)
            : base(parentFolder, name)
        {
            this.parentFolder = parentFolder;
            this.parentList = parentFolder.ParentList;
            this.parentWeb = parentFolder.ParentList.ParentWeb;
            this.parentSite = parentFolder.ParentSite;
            this.record = record;
            this.fileName = name;
            this.desUrl = desUrl;
        }

        public void ImportAveSPDoc(IAveRestoreStream stream, ScheduleConfiguration config, bool isFirstVersion, Guid UniqueId, bool restoreRetentionLabImmediate, ref string retentionLabelForRAMode)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SPMoveDocImport.ImportAveSPDoc"))
            {
                var docInfo = stream.TryReadMetadata(AveMetadataType.ItemMetadataDto).GetMetadata<AveSPDocumentMetadataDto>();
                if (!restoreRetentionLabImmediate && docInfo.DocInfo_Old != null && docInfo.DocInfo_Old.ContainsKey("ComplianceTag"))
                {
                    retentionLabelForRAMode = docInfo.DocInfo_Old["ComplianceTag"].ToString();
                    docInfo.DocInfo_Old.Remove("ComplianceTag");
                }

                var userData = docInfo.UserDataInfo;
                AveRestoreOption mRestoreOption = new AveRestoreOption(0);
                ContentConflictResolution contentConflictResolution = config.currentRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution;
                switch (contentConflictResolution)
                {
                    case ContentConflictResolution.Overwrite:
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.OverWrite);
                        break;
                    case ContentConflictResolution.Skip:
                        //AveRestoreMode.Default  means skip 
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.Default);
                        break;
                    case ContentConflictResolution.Append:
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.Append);
                        bool? isItemExistInDestination = false;
                        AveSPDoc doc = this as AveSPDoc;
                        mRestoreOption.mAveRestoreMode = ResetDocNameIfNeedAppend(config, doc, doc.Name, ref isItemExistInDestination);
                        break;
                }
                // DELETE_ITEM = true means Delete Des Document
                mRestoreOption.mAveItemRestoreOption.DELETE_ITEM = true;
                this.SetRestoreOption(mRestoreOption);

                #region reload web,list
                try
                {
                    //ADO-160387 此方法每次reload大约15~30ms，为了保证对象统一，需要每次都要重新reload，保证content type 和field还原不冲突。
                    this.parentWeb.ReloadWeb();
                    this.parentList.ReloadList();
                }
                catch (Exception ReloadEx)
                {
                    mLog.Warn("Can not Reload Web or List in Record Manager Job,but not affect Current job,Reason:{0}.", ReloadEx.ToString());
                }
                #endregion
                ItemLevelRestoreItemCTAndFields(userData, this, config);
                #region Match Exist CT Job
                //ADO-130694 Match Exist CT Job do not restore metadata
                if (config.itemDependencyOption == ItemDependencyOption.NotRestore)
                {
                    docInfo.UserDataInfo = new Dictionary<string, object>();
                    //#tp_ContentTypeId为ContentType 对应的Key，把这个属性还原，即可还原Content Type这个Column
                    if (userData.ContainsKey("#tp_ContentTypeId"))
                    {
                        docInfo.UserDataInfo.Add("#tp_ContentTypeId", userData["#tp_ContentTypeId"]);
                    }
                    //File_x0020_Type为文件类型对应的key，还原时需要用它来还原图标  ADO-153817
                    if (userData.ContainsKey("File_x0020_Type"))
                    {
                        docInfo.UserDataInfo.Add("File_x0020_Type", userData["File_x0020_Type"]);
                    }
                    #region necessary column
                    //SAAS-14477 添加更新file的时候必要的column 
                    if (userData.ContainsKey("Author"))
                    {
                        docInfo.UserDataInfo.Add("Author", userData["Author"]);
                    }
                    if (userData.ContainsKey("Editor"))
                    {
                        docInfo.UserDataInfo.Add("Editor", userData["Editor"]);
                    }
                    if (userData.ContainsKey("Created"))
                    {
                        docInfo.UserDataInfo.Add("Created", userData["Created"]);
                    }
                    if (userData.ContainsKey("Modified"))
                    {
                        docInfo.UserDataInfo.Add("Modified", userData["Modified"]);
                    }
                    #endregion
                }
                #endregion

                if (isFirstVersion && contentConflictResolution == ContentConflictResolution.Overwrite)
                {
                    DeleteDestinationFile(config);
                }
                //ADO-137478 多个同名文件move到同一目的端需要释放每个文件所属folder对象
                if (isFirstVersion && parentFolder.RestoringItem != null)
                {
                    parentFolder.RestoringItem.ResetNewItemValues(true, "", "");
                }
                RestoreDocumentMetadataDto(stream, docInfo);

                this.AveSPItem.SPListItem.SystemUpdateForProps(new Dictionary<string, object> { [ArchiverCommonStaticMethod.LastMovedDateProp] = DateTime.UtcNow });
                mLog.Info($"Updated last moved date for moved file. File:{this.AveSPItem.SPListItem.UniqueId}.");

                if (config.IsILMode)
                {
                    using (AvePerformanceScope mRecordRelatedColumn = new AvePerformanceScope("SP2013ArchiveBackUp.ItemRecordManager.UpdateRecordRelatedColumn"))
                    {
                        #region Update Source Move File RecordsRelated Column.
                        //SP关联Physical，什么都不需要修改,只需要处理SP关联SP的
                        IAveFile desFile = this.parentWeb.SPWeb.GetFile(this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + (this as AveSPDoc).Name);
                        //RecordsRelated Column type is NoteDataFormat which wrapper has logical to process this type column. we need special logic to keep current column value.
                        if (userData != null && userData.ContainsKey("RecordsRelated"))
                        {
                            string recordsRelatedValue = userData["RecordsRelated"].ToString();
                            try
                            {
                                XmlDocument xmlDoc = new XmlDocument();
                                recordsRelatedValue = recordsRelatedValue.Replace("&#58;", ":");
                                xmlDoc.LoadXml(recordsRelatedValue);
                                foreach (XmlElement ele in xmlDoc.GetElementsByTagName("a"))
                                {
                                    var relatedObjString = ele.GetAttribute("rel");
                                    relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                                    RMRelatedItemInfo relatedObj =  SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                                    if (relatedObj.SourceFlag == (int)SourceFlag.SharePoint)
                                    {
                                        var relatedItemUrl = HttpUtility.UrlDecode(ele.GetAttribute("href"));
                                        relatedItemUrl = new Uri(relatedObj.SiteUrl).Scheme + @"://" + new Uri(relatedObj.SiteUrl).Authority + relatedItemUrl;
                                        ele.SetAttribute("href", relatedItemUrl);
                                        mLog.Info("Replace RecordsRelated success,MoveDesSiteUrl:{0},relatedSiteUrl:{1},replaceRelatedItemUrl:{2}.", parentSite.SiteUrl, relatedObj.SiteUrl, relatedItemUrl);
                                    }
                                }
                                recordsRelatedValue = xmlDoc.OuterXml.Replace(":", "&#58;");
                                if (desFile.Exists)
                                {
                                    mLog.Info("File Exists,file:{0}", desFile.UniqueId);
                                    if (ArchiverCommonStaticMethod.CheckisRecord(desFile.Item))
                                    {
                                        mLog.Info("current file is Declare Status and will be Undo declare it.File:{0}", desFile.UniqueId);
                                        record.UndeclareItemAsRecord(desFile.Item);
                                        desFile.Item["RecordsRelated"] = recordsRelatedValue;
                                        desFile.Item.SystemUpdate();
                                        record.DeclareItemAsRecord(desFile.Item);
                                        mLog.Info("Replace RecordsRelated Declare File Successful.File:{0}", desFile.UniqueId);
                                    }
                                    else
                                    {
                                        desFile.Item["RecordsRelated"] = recordsRelatedValue;
                                        desFile.Item.SystemUpdate();
                                        mLog.Info("Replace RecordsRelated File Successful.File:{0}", desFile.UniqueId);
                                    }
                                }
                                else
                                {
                                    //进入这个判断有两种可能，1.当前文件是check out file. 2.当前文件在目的端不存在.
                                    mLog.Info("File Not Exists when Replace RecordsRelated.file:{0}", desFile.UniqueId);
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Info("Replace RecordsRelated failed in move {0} to action.Message:{1}.", desFile.ServerRelativeUrl, ex.ToString());
                            }
                        }
                        #endregion

                        #region Update Source Move File Related File RecordsRelated Column.
                        if (userData != null && userData.ContainsKey("RecordsRelated"))
                        {
                            RelatedRecordsUtility util = new RelatedRecordsUtility();
                            var sourceProperties = RelatedRecordsUtility.GetRelatedProperties(userData["RecordsRelated"].ToString());
                            var destRelatedItemInfo = util.GenerateRMRelatedItemInfo(desFile.Item);
                            var dbUtil = new RMExplorer.RMExplorerMoveDBUtil();
                            var ids = sourceProperties.Select(d =>
                            {
                                Guid selectResult;
                                if (d.SourceFlag == (int)SourceFlag.All || d.SourceFlag == (int)SourceFlag.SharePoint)
                                {
                                    selectResult = IDGenerator.GetRecordId(d.SiteId, d.id);
                                }
                                else
                                {
                                    selectResult = d.id;
                                }
                                return selectResult;
                            }).ToArray();
                            var allRecords = dbUtil.GetRecords(ids);
                            foreach (RMRelatedItemInfo property in sourceProperties)
                            {
                                if (property.SourceFlag == (int)SourceFlag.SharePoint)
                                {

                                    // need a guid
                                    util.UpdateRelateColumnValue(property, config.moveSourceSiteUrl, config.moveSourceFileUrl, UniqueId, destRelatedItemInfo);
                                }
                                else if (property.SourceFlag == (int)SourceFlag.Physical)
                                {
                                    ///need a guid and all  List<Record> allRecords
                                    util.UpdateRelateColumnValuePhysical(property, config.moveSourceSiteUrl, config.moveSourceFileUrl, UniqueId, destRelatedItemInfo, allRecords);
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
        }
        public void ImportAveSPDoc(IAveRestoreStream stream, ScheduleConfiguration config, bool isFirstVersion)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SPMoveDocImport.ImportAveSPDoc"))
            {
                var docInfo = stream.TryReadMetadata(AveMetadataType.ItemMetadataDto).GetMetadata<AveSPDocumentMetadataDto>();
                var userData = docInfo.UserDataInfo;
                AveRestoreOption mRestoreOption = new AveRestoreOption(0);
                ContentConflictResolution contentConflictResolution = config.currentRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution;
                switch (contentConflictResolution)
                {
                    case ContentConflictResolution.Overwrite:
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.OverWrite);
                        break;
                    case ContentConflictResolution.Skip:
                        //AveRestoreMode.Default  means skip 
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.Default);
                        break;
                    case ContentConflictResolution.Append:
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.Append);
                        bool? isItemExistInDestination = false;
                        AveSPDoc doc = this as AveSPDoc;
                        mRestoreOption.mAveRestoreMode = ResetDocNameIfNeedAppend(config, doc, doc.Name, ref isItemExistInDestination);
                        break;
                }
                // DELETE_ITEM = true means Delete Des Document
                mRestoreOption.mAveItemRestoreOption.DELETE_ITEM = true;
                this.SetRestoreOption(mRestoreOption);

                #region reload web,list
                try
                {
                    //ADO-160387 此方法每次reload大约15~30ms，为了保证对象统一，需要每次都要重新reload，保证content type 和field还原不冲突。
                    this.parentWeb.ReloadWeb();
                    this.parentList.ReloadList();
                }
                catch (Exception ReloadEx)
                {
                    mLog.Warn("Can not Reload Web or List in Record Manager Job,but not affect Current job,Reason:{0}.", ReloadEx.ToString());
                }
                #endregion
                ItemLevelRestoreItemCTAndFields(userData, this, config);
                #region Match Exist CT Job
                //ADO-130694 Match Exist CT Job do not restore metadata
                if (config.itemDependencyOption == ItemDependencyOption.NotRestore)
                {
                    docInfo.UserDataInfo = new Dictionary<string, object>();
                    //#tp_ContentTypeId为ContentType 对应的Key，把这个属性还原，即可还原Content Type这个Column
                    if (userData.ContainsKey("#tp_ContentTypeId"))
                    {
                        docInfo.UserDataInfo.Add("#tp_ContentTypeId", userData["#tp_ContentTypeId"]);
                    }
                    //File_x0020_Type为文件类型对应的key，还原时需要用它来还原图标  ADO-153817
                    if (userData.ContainsKey("File_x0020_Type"))
                    {
                        docInfo.UserDataInfo.Add("File_x0020_Type", userData["File_x0020_Type"]);
                    }
                    #region necessary column
                    //SAAS-14477 添加更新file的时候必要的column 
                    if (userData.ContainsKey("Author"))
                    {
                        docInfo.UserDataInfo.Add("Author", userData["Author"]);
                    }
                    if (userData.ContainsKey("Editor"))
                    {
                        docInfo.UserDataInfo.Add("Editor", userData["Editor"]);
                    }
                    if (userData.ContainsKey("Created"))
                    {
                        docInfo.UserDataInfo.Add("Created", userData["Created"]);
                    }
                    if (userData.ContainsKey("Modified"))
                    {
                        docInfo.UserDataInfo.Add("Modified", userData["Modified"]);
                    }
                    #endregion
                }
                #endregion

                if (isFirstVersion && contentConflictResolution == ContentConflictResolution.Overwrite)
                {
                    DeleteDestinationFile(config);
                }
                //ADO-137478 多个同名文件move到同一目的端需要释放每个文件所属folder对象
                if (isFirstVersion && parentFolder.RestoringItem != null)
                {
                    parentFolder.RestoringItem.ResetNewItemValues(true, "", "");
                }
                RestoreDocumentMetadataDto(stream, docInfo);

                this.AveSPItem.SPListItem.SystemUpdateForProps(new Dictionary<string, object> { [ArchiverCommonStaticMethod.LastMovedDateProp] = DateTime.UtcNow });
                mLog.Info($"Updated last moved date for moved file. File:{this.AveSPItem.SPListItem.UniqueId}.");

                if (config.IsILMode)
                {
                    using (AvePerformanceScope mRecordRelatedColumn = new AvePerformanceScope("SP2013ArchiveBackUp.ItemRecordManager.UpdateRecordRelatedColumn"))
                    {
                        #region Update Source Move File RecordsRelated Column.
                        //SP关联Physical，什么都不需要修改,只需要处理SP关联SP的
                        IAveFile desFile = this.parentWeb.SPWeb.GetFile(this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + (this as AveSPDoc).Name);
                        //RecordsRelated Column type is NoteDataFormat which wrapper has logical to process this type column. we need special logic to keep current column value.
                        if (userData != null && userData.ContainsKey("RecordsRelated"))
                        {
                            string recordsRelatedValue = userData["RecordsRelated"].ToString();
                            try
                            {
                                XmlDocument xmlDoc = new XmlDocument();
                                recordsRelatedValue = recordsRelatedValue.Replace("&#58;", ":");
                                xmlDoc.LoadXml(recordsRelatedValue);
                                foreach (XmlElement ele in xmlDoc.GetElementsByTagName("a"))
                                {
                                    var relatedObjString = ele.GetAttribute("rel");
                                    relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                                    RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                                    if (relatedObj.SourceFlag == (int)SourceFlag.SharePoint)
                                    {
                                        var relatedItemUrl = HttpUtility.UrlDecode(ele.GetAttribute("href"));
                                        relatedItemUrl = new Uri(relatedObj.SiteUrl).Scheme + @"://" + new Uri(relatedObj.SiteUrl).Authority + relatedItemUrl;
                                        ele.SetAttribute("href", relatedItemUrl);
                                        mLog.Info("Replace RecordsRelated success,MoveDesSiteUrl:{0},relatedSiteUrl:{1},replaceRelatedItemUrl:{2}.", parentSite.SiteUrl, relatedObj.SiteUrl, relatedItemUrl);
                                    }
                                }
                                recordsRelatedValue = xmlDoc.OuterXml.Replace(":", "&#58;");
                                if (desFile.Exists)
                                {
                                    mLog.Info("File Exists,file:{0}", desFile.UniqueId);
                                    if (ScheduleConfiguration.CheckisRecord(desFile.Item))
                                    {
                                        mLog.Info("current file is Declare Status and will be Undo declare it.File:{0}", desFile.UniqueId);
                                        record.UndeclareItemAsRecord(desFile.Item);
                                        desFile.Item["RecordsRelated"] = recordsRelatedValue;
                                        desFile.Item.SystemUpdate();
                                        record.DeclareItemAsRecord(desFile.Item);
                                        mLog.Info("Replace RecordsRelated Declare File Successful.File:{0}", desFile.UniqueId);
                                    }
                                    else
                                    {
                                        desFile.Item["RecordsRelated"] = recordsRelatedValue;
                                        desFile.Item.SystemUpdate();
                                        mLog.Info("Replace RecordsRelated File Successful.File:{0}", desFile.UniqueId);
                                    }
                                }
                                else
                                {
                                    //进入这个判断有两种可能，1.当前文件是check out file. 2.当前文件在目的端不存在.
                                    mLog.Info("File Not Exists when Replace RecordsRelated.file:{0}", desFile.UniqueId);
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Info("Replace RecordsRelated failed in move {0} to action.Message:{1}.", desFile.ServerRelativeUrl, ex.ToString());
                            }
                        }
                        #endregion

                        #region Update Source Move File Related File RecordsRelated Column.
                        if (userData != null && userData.ContainsKey("RecordsRelated"))
                        {
                            RelatedRecordsUtility util = new RelatedRecordsUtility();
                            var sourceProperties = RelatedRecordsUtility.GetRelatedProperties(userData["RecordsRelated"].ToString());
                            var destRelatedItemInfo = util.GenerateRMRelatedItemInfo(desFile.Item);
                            foreach (RMRelatedItemInfo property in sourceProperties)
                            {
                                if (property.SourceFlag == (int)SourceFlag.SharePoint)
                                {
                                    util.UpdateSPRelatedSPColumnValue(property, config.moveSourceSiteUrl, config.moveSourceFileUrl, destRelatedItemInfo, "");
                                }
                                else if (property.SourceFlag == (int)SourceFlag.Physical)
                                {
                                    util.UpdateSPRelatedPhysicalColumnValue(property, config.moveSourceSiteUrl, config.moveSourceFileUrl, destRelatedItemInfo);
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
        }
        #region Overwrite restore
        /// <summary>
        /// 删除目的端已存在的文件
        /// </summary>
        /// <param name="config"></param>
        /// <param name="desUrl"></param>
        private void DeleteDestinationFile(ScheduleConfiguration config)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SPMoveDocImport.DeleteDestinationFile"))
            {
                try
                {
                    var fullUrl = this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + (this as AveSPDoc).Name;
                    IAveFile desFile = this.parentWeb.SPWeb.GetFile(fullUrl);
                    if (desFile.Exists)
                    {
                        mLog.Info("File Exists in destination,file:{0}.", desFile.UniqueId);
                        try
                        {
                            //移除related 关系
                            IAveListItem aveListItem = desFile.Item;

                            ValidateBeforeOverWrite(config, aveListItem);

                            var utility = new RelatedRecordsUtility();
                            //dai ti
                            utility.RemoveRelatedPropertyForListItem(aveListItem);
                            //Here is archiver's logic
                            //var relatedInfos = utility.GetRelatedProperties(aveListItem);
                            //foreach (var relatedInfo in relatedInfos)
                            //{
                            //    utility.RemoveRelateColumnValue(relatedInfo, this.parentWeb.SPWeb.Site, fullUrl, aveListItem.UniqueId, "");
                            //}
                            //Local&365:当前user 自动check out/Check Out的文件，可以调用delete方法直接删除，不会抛错。
                            desFile.Delete();
                            mLog.Info("Delete destination File Successful.File:{0}.", desFile.UniqueId);
                        }
                        catch (ConetentSkipException)
                        {
                            mLog.Info("This file was already moved and overwritten in current job so skip it.File:{0}.", desFile.UniqueId);
                            throw;
                        }
                        catch (Exception e)
                        {
                            mLog.Info("Can not delete current file,Name:{0},Message{1}.", desFile.Name, e.ToString());
                            var isDeclaredRecord = ArchiverCommonStaticMethod.CheckisRecord(desFile.Item);
                            var isHaveRecordLabel = ArchiverCommonStaticMethod.IsHaveRecordLabel(desFile.Item);
                            if (isDeclaredRecord || isHaveRecordLabel)
                            {
                                if (isDeclaredRecord)
                                {
                                    mLog.Info("current file is Declare Status and will be Undo declare it.File:{0}.", desFile.UniqueId);
                                    record.UndeclareItemAsRecord(desFile.Item);
                                    mLog.Info("Undo declare File Successful.File:{0}.", desFile.UniqueId);
                                }
                                if (isHaveRecordLabel)
                                {
                                    mLog.Info("current file is locked by record label and will remove record label of it.File:{0}.", desFile.UniqueId);
                                    desFile.Item.SetComplianceTagOnBulkItems("");
                                    mLog.Info("Remove record label Successful .File:{0}.", desFile.UniqueId);
                                }
                                desFile.Delete();
                                mLog.Info("Delete locked File Successful.File:{0}.", desFile.UniqueId);
                            }
                        }
                    }
                    else
                    {
                        //当前文件在目的端不存在.
                        mLog.Info("File Not Exists,It may be auto check out file.file:{0}.", desFile.UniqueId);
                    }
                }
                catch (ConetentSkipException)
                {
                    throw;
                }
                catch (Exception overwriteEx)
                {
                    mLog.Warn("An Exception occur while Before Overwrite restore,Message:{0}.", overwriteEx.ToString());
                }
            }
        }

        /// <summary>
        /// validate the moved date property of the destination file before overwrite it.
        /// if the file with the same name was moved while this job is running, skip it.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private void ValidateBeforeOverWrite(ScheduleConfiguration config, IAveListItem aveListItem)
        {
            using (AvePerformanceScope pc = new("SPMoveDocImport.ValidateBeforeOverWrite"))
            {
                try
                {
                    var lastMovedDateGuid = ArchiverCommonStaticMethod.LastMovedDateProp;
                    if (!aveListItem.Properties.ContainsKey(lastMovedDateGuid)
                        || !DateTime.TryParse(aveListItem.Properties[lastMovedDateGuid]?.ToString(), out var movedDate))
                    {
                        var props = new Dictionary<string, object> { [lastMovedDateGuid] = DateTime.UtcNow };
                        aveListItem.SystemUpdateForProps(props);
                        mLog.Info($"This file has not been moved. File:{aveListItem.UniqueId}.");
                        return;
                    }

                    var movedDateUTC = movedDate.ToUniversalTime();
                    if (movedDateUTC > config.ArchiverUNCTime)
                    {
                        mLog.Info($"This file is moved while this job is running so skip. File:{aveListItem.UniqueId}, movedDateUTC: {movedDateUTC}.");
                        throw new ConetentSkipException("StorageOptimization_SOARDocImportSkipConflictItem");
                    }

                    mLog.Info($"This file is moved before this job. File:{aveListItem.UniqueId}, movedDateUTC: {movedDateUTC}.");
                }
                catch (Exception e)
                {
                    mLog.Info("Error occurred while validating the des file moved date,Name:{0},Message{1}.", aveListItem.Name, e.ToString());
                    throw;
                }
            }

            return;
        }

        #endregion

        #region Append
        private AveRestoreMode ResetDocNameIfNeedAppend(ScheduleConfiguration config, AveSPDoc doc, string realName, ref bool? isItemExistInDestination)
        {
            return ResetItemNameIfNeedAppend(config, doc, realName, doc.ResetAvailableName, doc.ResetName, ref isItemExistInDestination);
        }

        private AveRestoreMode ResetItemNameIfNeedAppend(ScheduleConfiguration config, RestoreableObject item, string realName, Func<DateTime, string> ResetAvailableName, Action<string> ResetName, ref bool? isItemExistInDestination)
        {
            bool isThumbnailData = false;
            string newName = string.Empty;
            if (NeedAppend(item, out isThumbnailData))
            {
                if (isThumbnailData)
                {
                    string picName = ChangeThumbnailNameToPicName(realName);
                    if (config.appendItemMapping.ContainsKeyAppendName(picName))
                    {
                        newName = AppendThumbnailName(config.appendItemMapping.GetValueAppendName(picName));
                        ResetName(newName);
                    }
                    else//name 保持不变
                    {
                    }
                    item.RestoreOption.ResetRestoreMode((int)AveRestoreMode.OverWrite);
                    isItemExistInDestination = isItemExistInDestination ?? false;
                    return AveRestoreMode.OverWrite;
                }
                else if (!config.appendItemMapping.ContainsKeyAppendName(realName))
                {
                    newName = ResetAvailableName(DateTime.MinValue);
                    if (realName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                    {
                        config.appendItemMapping.AddToMappingAppendName(realName, newName);
                        return AveRestoreMode.OverWrite;
                    }
                    else
                    {
                        config.appendItemMapping.AddToMappingAppendName(realName, newName);
                        ResetName(config.appendItemMapping.GetValueAppendName(realName));
                    }
                }
                else
                {
                    ResetName(config.appendItemMapping.GetValueAppendName(realName));
                }
                if (!string.Equals(realName, config.appendItemMapping.GetValueAppendName(realName), StringComparison.Ordinal))
                {
                    isItemExistInDestination = isItemExistInDestination ?? false;//Append
                    return AveRestoreMode.Append;
                }
                else
                {
                    return AveRestoreMode.Default;
                }
            }
            else if (item.CheckRestoreOption(AveRestoreMode.Append))
            {
                return AveRestoreMode.Default;
            }
            return item.RestoreOption.mAveRestoreMode;
        }

        private string AppendThumbnailName(string realName)
        {
            try
            {
                string name = realName.Substring(0, realName.LastIndexOf('.'));
                string extension = realName.Substring(realName.LastIndexOf('.') + 1);
                return name + '_' + extension + ".jpg";
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while append thumbnail name. Name: {0}. Error: {1}", realName, e.ToString());
                return realName;
            }
        }

        private string ChangeThumbnailNameToPicName(string realName)
        {
            try
            {
                string tempName = realName.Substring(0, realName.LastIndexOf('.'));
                string name = tempName.Substring(0, tempName.IndexOf('_'));
                string extension = tempName.Substring(tempName.LastIndexOf('_') + 1);
                return name + '.' + extension;
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while change thumbnail name. Name: {0}. Error: {1}", realName, e.ToString());
                return realName;
            }
        }
        private bool IsThumbnail(AveSPDoc doc)
        {
            try
            {
                List<int> targetListTemplates = new List<int> { 109, 851, 2100 };//109 refer to Picture Library,851 refer to images Library,2100 refer to slide Library

                List<string> targetFolders = new List<string> { "_w", "_t" };//Hidden folder where thumbnails file placed
                if (targetFolders.Contains(doc.ParentFolder.Name)) //here we shouldn't use IgnoreCase
                {
                    if (targetListTemplates.Contains(Convert.ToInt32(doc.ParentFolder.ParentList.ListInfo.BaseTemplate)))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while deciding if the document is thumbnail: {0}", e.ToString());
                return false;
            }
        }

        private bool NeedAppend(RestoreableObject itemObject, out bool isThumbnailData)
        {
            isThumbnailData = false;
            if (itemObject is AveSPDoc)
            {
                AveSPDoc doc = itemObject as AveSPDoc;
                if (doc.ParentFolder == null || doc.ParentFolder.ParentList == null)
                {
                    return false;
                }
                if (IsThumbnail(doc))
                {
                    isThumbnailData = true;
                    return true;
                }
                //dont need to append file if itemObject is system file or in system list
                //return !(AppendUtility.CheckIsSystemFile(doc));
                //We Only have Document RM Rule,We can pass system file at scan
                return true;
            }
            return false;
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemSchemaDependency">false means throw Exception if CT Not Exist  </param>
        /// <param name="skipItemWhenConflict">true means throw Exception if CT conflict</param>
        /// <param name="config"></param>
        public void ItemLevelRestoreItemCTAndFields(Dictionary<string, object> userData, RestoreableObject aveObject, ScheduleConfiguration config)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SPMoveDocImport.ItemLevelRestoreItemCTAndFields"))

            {
                AveSPItem aveSPItem;
                if (aveObject is AveSPDoc)
                {
                    aveSPItem = (aveObject as AveSPDoc).AveSPItem;
                }
                else if (aveObject is AveSPListItem)
                {
                    aveSPItem = (aveObject as AveSPListItem).AveSPItem;
                }
                else if (aveObject is AveSPFolder)
                {
                    aveSPItem = (aveObject as AveSPFolder).EnsureCTFieldItem;
                }
                else
                {
                    throw new ArgumentNullException("Wrong Item Type");
                }

                AveFieldRestoreOption fieldRestoreOptions = new AveFieldRestoreOption();
                bool itemSchemaDependency = false;
                bool skipItemWhenNotFound = false;
                bool skipItemWhenConflict = false;
                fieldRestoreOptions.FindOption = new FieldFindOption[] { FieldFindOption.FindBySchema, FieldFindOption.FindById,
                                                                FieldFindOption.FindByInternalName, FieldFindOption.FindByStaticName };

                AveContentTypeRestoreOption ContentTypeRestoreOption = new AveContentTypeRestoreOption();
                ContentTypeRestoreOption.FindOption = new ContentTypeFindOption[] { ContentTypeFindOption.FindBySchema, ContentTypeFindOption.FindById, ContentTypeFindOption.FindByName };
                //FindScope Only FindOption = ContentTypeFindOption.FindByParent can be used 
                ContentTypeRestoreOption.FindScope = new ContentTypeFindScope[] { ContentTypeFindScope.Current, ContentTypeFindScope.Parent, ContentTypeFindScope.Children };
                ContentTypeRestoreOption.CreateOption = new ContentTypeCreateOption[] { ContentTypeCreateOption.UseId, ContentTypeCreateOption.ForceCreate, ContentTypeCreateOption.UseParent };
                ContentTypeRestoreOption.GetParentOption = GetParentContentTypeOption.RestoreFamily;
                //配置文件默认值是Overwrite
                switch (config.itemDependencyOption)
                {
                    case ItemDependencyOption.NotRestore:
                        itemSchemaDependency = false;
                        skipItemWhenNotFound = true;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.None;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.AppendDestinationWin;
                        break;
                    case ItemDependencyOption.Overwrite:
                        itemSchemaDependency = true;
                        skipItemWhenNotFound = false;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Overwrite;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.Overwrite;
                        break;
                    case ItemDependencyOption.Append:
                        itemSchemaDependency = true;
                        skipItemWhenNotFound = false;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Append;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.AppendSourceWin;
                        break;
                    case ItemDependencyOption.SkipConfilctItem:
                        itemSchemaDependency = true;
                        skipItemWhenNotFound = false;
                        skipItemWhenConflict = true;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Skip;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.Skip;
                        break;
                }
                try
                {

                    if (config.IsILMode)
                    {
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Skip;
                    }
                    //TODO:Add new option
                    if (config.itemDependencyOption == ItemDependencyOption.NotRestore)
                    {
                        aveSPItem.EnsureItemContentTypeDependency(userData, itemSchemaDependency, skipItemWhenNotFound, skipItemWhenConflict, ContentTypeRestoreOption);
                    }
                    else
                    {
                        aveSPItem.EnsureItemSchemaDependency(userData, null, itemSchemaDependency, skipItemWhenNotFound, skipItemWhenConflict, ContentTypeRestoreOption, fieldRestoreOptions);
                    }
                }
                catch (AveSchemaDependencyConflictException ce)
                {
                    throw new SkipException(ce.Message, ce);
                }
                catch (AveSchemaDependencyNotFoundException ne)
                {
                    throw new SkipException(ne.Message, ne);
                }
                catch (Exception ex)
                {
                    if (config.DestinationIsOneDriveSite && ex.InnerException != null
                            && (ex.InnerException.Message.Contains("Invalid field type: TaxonomyFieldType")))
                    {
                        mLog.Warn("Current DestinationIsOneDriveSite and skip TaxonomyFieldType restore");
                        return;
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Restore Document Metadata Dto
        /// </summary>
        /// <param name="fileRestoreOption"></param>
        /// <param name="documentMetadataDto"></param>
        /// <param name="restoreStream"></param>
        /// <returns></returns>
        private void RestoreDocumentMetadataDto(IAveRestoreStream restoreStream, AveSPDocumentMetadataDto documentMetadataDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SPMoveDocImport.RestoreDocumentMetadataDto"))
            {
                this.SetStream(restoreStream);
                this.parentSite.SPMembers.RestoreUsers(documentMetadataDto.UserCache.Users, false, false, false);
                this.parentSite.SPMembers.RestoreGroups(documentMetadataDto.GroupCache.Groups);
                if (documentMetadataDto.MetadataInfo != null)
                {
                    this.parentSite.MetadataService.Restore(documentMetadataDto.MetadataInfo);
                }
                AveRestoreResult result = AveRestoreResult.Normal;
                try
                {
                    result = this.RestoreDocument(documentMetadataDto);
                    this.RestorePermission(restoreStream);
                }
                catch (AveWrapperSkipException e)
                {
                    mLog.Info("This is AveWrapperSkipException,Need Out Skip. Exception:{0}", e.Message);
                    throw new ConetentSkipException("StorageOptimization_SOARDocImportSkipConflictItem");
                }
                //当源端version在目的端能find到或源端文件modify time和目的端modify time相同，skip
                if (result == AveRestoreResult.Omit || result == AveRestoreResult.SkipTheSameItem)
                {
                    mLog.Info("AveRestoreResult is : {0}", result.ToString());
                    throw new ConetentSkipException("StorageOptimization_SOARDocImportSkipConflictItem");
                }
            }
        }

        private void RestorePermission(IAveRestoreStream restoreStream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SPMoveDocImport.RestorePermission"))
            {
                try
                {
                    mLog.Info("Begin to restoring archiver document Users and Groups.");
                    List<AveMetadata> archiveUserCacheMetadata = restoreStream.TryReadMetadataList(AveMetadataType.UserCache);
                    if (archiveUserCacheMetadata != null)
                    {
                        foreach (var user in archiveUserCacheMetadata)
                        {
                            var userList = user.GetMetadata<AveUserList>();
                            if (userList != null)
                            {
                                foreach (AveUserInfo userInfo in userList.Users)
                                {
                                    //this.AveSPItem.ParentSite.SPMembers.RestoreUser(userInfo, new MembersRestoreOption() { IsSiteLevel = false, OverWrite = true, SkipWithoutPermissions = false });
                                    this.AveSPItem.ParentSite.SPMembers.RestoreUser(userInfo);
                                }
                            }
                        }
                    }
                    List<AveMetadata> groupCacheMetadata = restoreStream.TryReadMetadataList(AveMetadataType.GroupCache);
                    if (groupCacheMetadata != null)
                    {
                        foreach (var groups in groupCacheMetadata)
                        {
                            var groupList = groups.GetMetadata<AveGroupList>();
                            if (groupList != null)
                            {
                                foreach (AveGroupInfo group in groupList.Groups)
                                {
                                    //this.AveSPItem.ParentSite.SPMembers.RestoreGroup(group, new MembersRestoreOption() { IsSiteLevel = false, OverWrite = true, SkipWithoutPermissions = false });
                                    this.AveSPItem.ParentSite.SPMembers.RestoreGroup(group);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("An error occurred while restoring archiver document Groups. Exception: {0}", ex.ToString());
                }
                finally
                {
                    mLog.Info("End to restoring archiver document Users and Groups.");
                }
                AveMetadata metadata;
                while ((metadata = restoreStream.ReadMetadata()) != null)
                {
                    switch (metadata.MetadataType)
                    {
                        case AveMetadataType.RoleAssignment:
                            {
                                var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                                //由于DocImport没有继承AveSPItem，所以在创建Instance时传入AveSPItem而不是this对象
                                AveObjectSecurity security = AveObjectSecurity.CreateInstance(this.AveSPItem);
                                security.SourceHasUniqueRoleAssignment = this.AveSPItem.HasUniqueRoleAssignments;
                                security.RestoreRoleAssignments(roleAssignments, new SecurityRestoreOption() { ConflictResolutionForSecurityObject = ConflictResolutionForSecurityObject.OverWrite, ConflictResolutionForPincipal = ConflictResolutionForPincipal.OverWrite, IsIncludeShareLink = true });
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        public Guid GetDesExistFileRecordID()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SPMoveDocImport.GetDesExistFileRecordID"))
            {
                Guid fileRecordID = Guid.Empty;
                try
                {
                    IAveFile desFile = this.parentWeb.SPWeb.GetFile(this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + (this as AveSPDoc).Name);
                    if (desFile.Exists)
                    {
                        //string fileFullPath = new Uri(parentWeb.SPWeb.Site.Url).Scheme + @"://" + new Uri(parentWeb.SPWeb.Site.Url).Authority + desFile.ServerRelativeUrl.Replace("\\", "/");
                        //filePathMD5 = new Guid(HashCodeHelper.ToMD5HashCode(fileFullPath.ToLowerInvariant()));
                        fileRecordID = ArchiverCommonStaticMethod.GetRecordId(parentWeb.SPWeb.Site.ID, desFile.UniqueId);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info("GetDesExistFileUniqueID Failed.Message:{0}.", ex.ToString());
                }
                return fileRecordID;
            }
        }
        private AveRestoreResult RestoreDocument(AveSPDocumentMetadataDto documentMetadataDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SPMoveDocImport.RestoreDocument"))
            {
                /*
                 * 2100是slide library，这个library必须关闭才好用
                 */
                using (new AveEventReceiverUtility(parentList != null && parentList.SPList != null && (int)parentList.SPList.BaseTemplate == 2100))
                {
                    var restoreResult = this.RestoreSelf(documentMetadataDto.DocInfo_Old, documentMetadataDto.UserDataInfo,
                                                         documentMetadataDto.DocDataJunction, documentMetadataDto.WebParts);

                    #region Declare File restore lookup column need undeclare first.
                    //Office 365 Declare File status is Declare when restore,so we need undeclare first then restore look up column.
                    if (documentMetadataDto.ItemTPGUIDofLookupValue != null && documentMetadataDto.ItemTPGUIDofLookupValue.Count != 0)
                    {
                        try
                        {
                            IAveFile file = parentWeb.SPWeb.GetFile(desUrl.TrimEnd('/') + "/" + fileName);
                            IAveListItem newItem = file.Item;
                            //REC-2432 Host Header Site Collection通过IAveFile GetFile(string serverRelativeUrl);方式获取不到IAveListItem对象.
                            if (newItem == null)
                            {
                                mLog.Info("Current IAveListItem is null and will reget IAveListItem by List GetItemByUniqueId.");
                                newItem = parentList.SPList.GetItemByUniqueId(file.UniqueId);
                                mLog.Info("Reget IAveListItem successful by List GetItemByUniqueId. IAveListItem is null:{0}.", newItem == null);
                            }
                            if (ArchiverCommonStaticMethod.CheckisRecord(file.Item))
                            {
                                mLog.Info("Current file is declare file,file UniqueId:{0}.", file.UniqueId);
                                record.UndeclareItemAsRecord(file.Item);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Info("Can not UndeclareItemAsRecord when RestoreLookupFieldGuidValue,Message:{0}.", ex.Message);
                        }
                    }
                    #endregion

                    if (this.AveSPItem != null)
                    {
                        this.AveSPItem.RestoreLookupFieldGuidValue(documentMetadataDto.ItemTPGUIDofLookupValue);
                    }

                    return restoreResult;
                }
            }
        }

        public void Dispose()
        {
            //sp object here do not neet to dispose ,we must use it next document and we dispose it in the end
        }


    }
}

