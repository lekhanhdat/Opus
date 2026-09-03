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
using AvePoint.RA.Contract.Services;
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
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.RA.SharePoint.Common;

namespace RAFileSystem.SharePoint.EnforceRuleAction.LeaveStub
{
    public class OnPremSPLeaveStubDocImport : AveSPDoc, IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal AveSPSite parentSite;
        internal AveSPWeb parentWeb;
        internal AveSPList parentList;
        internal AveSPFolder parentFolder;

        internal IAveORecords record;
        internal string fileName;
        internal string desUrl;

        public OnPremSPLeaveStubDocImport(AveSPFolder parentFolder, IAveORecords record, string fileName, string desUrl)
            : base(parentFolder, fileName)
        {
            this.parentFolder = parentFolder;
            this.parentList = parentFolder.ParentList;
            this.parentWeb = parentFolder.ParentList.ParentWeb;
            this.parentSite = parentFolder.ParentSite;
            this.record = record;
            this.fileName = fileName;
            this.desUrl = desUrl;
        }

        public void ImportAveSPDoc(IAveRestoreStream stream)
        {
            using (var performance = new AgentPerformanceScope("SPStubDocImport.ImportAveSPDoc", addToStatistics: true))
            {
                var docInfo = stream.TryReadMetadata(AveMetadataType.ItemMetadataDto).GetMetadata<SPDocumentMetadataDto>();
                var userData = docInfo.UserDataInfo;
                AveRestoreOption mRestoreOption = new AveRestoreOption(0);
                mRestoreOption.ResetRestoreMode((int)AveRestoreMode.OverWrite);
                mRestoreOption.mAveItemRestoreOption.DELETE_ITEM = true;
                this.SetRestoreOption(mRestoreOption);
                try
                {
                    userData["File_x0020_Type"] = "aspx";
                    if (userData.ContainsKey("IconOverlay"))//Declare Item Execute leave stub need remove IconOverlay property.RECO-957
                    {
                        userData.Remove("IconOverlay");
                    }
                    if (docInfo.UserDataInfo == null)
                    {
                        docInfo.UserDataInfo = new Dictionary<string, object>();
                    }
                    if (userData.ContainsKey("MediaServiceMetadata"))
                    {
                        mLog.Info("Current file is leave link file and userData contains MediaServiceMetadata.MediaServiceMetadata:{0}.", userData["MediaServiceMetadata"].ToString().LogBase64());
                        userData.Remove("MediaServiceMetadata");
                    }
                    if (userData.ContainsKey("MediaServiceFastMetadata"))
                    {
                        mLog.Info("Current file is leave link file and userData contains MediaServiceFastMetadata.MediaServiceFastMetadata:{0}.", userData["MediaServiceFastMetadata"].ToString().LogBase64());
                        userData.Remove("MediaServiceFastMetadata");
                    }
                    if (!docInfo.UserDataInfo.ContainsKey("File_x0020_Type"))
                    {
                        mLog.Info("Current file is leave link file and UserDataInfo does not contains File_x0020_Type.File_x0020_Type:{0}.", userData["File_x0020_Type"].ToString().LogBase64());
                        docInfo.UserDataInfo.Add("File_x0020_Type", userData["File_x0020_Type"]);
                    }
                    if (docInfo.DocInfo_Old.ContainsKey("UIVersion"))
                    {
                        mLog.Info("Current file is leave link file and userData contains UIVersion:{0} and reset version to 1.0.", docInfo.DocInfo_Old["UIVersion"].ToString().LogBase64());
                        docInfo.DocInfo_Old["UIVersion"] = 512;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("RA not keep property error {0}", ex.ToString());
                }
                //ItemLevelRestoreItemCTAndFields(userData, this, config);
                //ADO-137478 多个同名文件move到同一目的端需要释放每个文件所属folder对象
                if (parentFolder.RestoringItem != null)
                {
                    parentFolder.RestoringItem.ResetNewItemValues(true, "", "");
                }
                RestoreDocumentMetadataDto(stream, docInfo);                
                using (var performance1 = new AgentPerformanceScope("SPStubDocImport.UpdateRecordRelatedColumn", addToStatistics: true))
                {
                    #region Update Source Move File RecordsRelated Column & Update Source Move File Related File RecordsRelated Column.
                    //SP关联Physical，什么都不需要修改,只需要处理SP关联SP的
                    //RecordsRelated Column type is NoteDataFormat which wrapper has logical to process this type column. we need special logic to keep current column value.
                    //if (userData != null && userData.ContainsKey("RecordsRelated"))
                    //{
                    //    IAveFile desFile = this.parentWeb.SPWeb.GetFile(this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + (this as AveSPDoc).Name);
                    //    string recordsRelatedValue = userData["RecordsRelated"].ToString();
                    //    try
                    //    {
                    //        XmlDocument xmlDoc = new XmlDocument();
                    //        recordsRelatedValue = recordsRelatedValue.Replace("&#58;", ":");
                    //        xmlDoc.LoadXml(recordsRelatedValue);
                    //        foreach (XmlElement ele in xmlDoc.GetElementsByTagName("a"))
                    //        {
                    //            var relatedObjString = ele.GetAttribute("rel");
                    //            relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                    //            JavaScriptSerializer jss = new JavaScriptSerializer();
                    //            RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
                    //            if (relatedObj.SourceFlag == (int)SourceFlag.SharePoint)
                    //            {
                    //                var relatedItemUrl = HttpUtility.UrlDecode(ele.GetAttribute("href"));
                    //                relatedItemUrl = new Uri(relatedObj.SiteUrl).Scheme + @"://" + new Uri(relatedObj.SiteUrl).Authority + relatedItemUrl;
                    //                ele.SetAttribute("href", relatedItemUrl);
                    //                mLog.Info("Replace RecordsRelated success,MoveDesSiteUrl:{0},relatedSiteUrl:{1},replaceRelatedItemUrl:{2}.", parentSite.SiteUrl, relatedObj.SiteUrl, relatedItemUrl);
                    //            }
                    //        }
                    //        recordsRelatedValue = xmlDoc.OuterXml.Replace(":", "&#58;");
                    //        if (desFile.Exists)
                    //        {
                    //            mLog.Info("File Exists,file Name:{0}", desFile.Name);
                    //            if (ScheduleConfiguration.CheckisRecord(desFile.Item))
                    //            {
                    //                mLog.Info("current file is Declare Status and will be Undo declare it.File Name:{0}", desFile.Name);
                    //                record.UndeclareItemAsRecord(desFile.Item);
                    //                desFile.Item["RecordsRelated"] = recordsRelatedValue;
                    //                desFile.Item.SystemUpdate();
                    //                record.DeclareItemAsRecord(desFile.Item);
                    //                mLog.Info("Replace RecordsRelated Declare File Successful.File Name:{0}", desFile.Name);
                    //            }
                    //            else
                    //            {
                    //                desFile.Item["RecordsRelated"] = recordsRelatedValue;
                    //                desFile.Item.SystemUpdate();
                    //                mLog.Info("Replace RecordsRelated File Successful.File Name:{0}", desFile.Name);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //进入这个判断有两种可能，1.当前文件是check out file. 2.当前文件在目的端不存在.
                    //            mLog.Info("File Not Exists when Replace RecordsRelated.file Name:{0}", desFile.Name);
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        mLog.Info("Replace RecordsRelated failed in move to action.Message:{0}.", ex.ToString());
                    //    }

                    //    RelatedRecordsUtility util = new RelatedRecordsUtility(config);
                    //    var sourceProperties = RelatedRecordsUtility.GetRelatedProperties(userData["RecordsRelated"].ToString());
                    //    var destRelatedItemInfo = util.GenerateRMRelatedItemInfo(desFile.Item);
                    //    foreach (RMRelatedItemInfo property in sourceProperties)
                    //    {
                    //        if (property.SourceFlag == (int)SourceFlag.SharePoint)
                    //        {
                    //            util.UpdateSPRelatedSPColumnValue(property, config.moveSourceSiteUrl, config.moveSourceFileUrl, destRelatedItemInfo, "");
                    //        }
                    //        else if (property.SourceFlag == (int)SourceFlag.Physical)
                    //        {
                    //            util.UpdateSPRelatedPhysicalColumnValue(property, config.moveSourceSiteUrl, config.moveSourceFileUrl, destRelatedItemInfo);
                    //        }
                    //    }
                    //}
                    #endregion
                }
            }
        }

        #region Overwrite restore
        /// <summary>
        /// 删除目的端已存在的文件
        /// </summary>
        /// <param name="config"></param>
        /// <param name="desUrl"></param>
        private void DeleteDestinationFile()
        {
            using (var performance = new AgentPerformanceScope("SPStubDocImport.DeleteDestinationFile", addToStatistics: true))
            {
                try
                {
                    var fullUrl = this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + (this as AveSPDoc).Name;
                    IAveFile desFile = this.parentWeb.SPWeb.GetFile(fullUrl);
                    if (desFile.Exists)
                    {
                        mLog.Info("File Exists in destination,file unique id:{0}.", desFile?.UniqueId);
                        try
                        {
                            //移除related 关系
                            //IAveListItem aveListItem = desFile.Item;
                            //var utility = new RelatedRecordsUtility(config);
                            //var relatedInfos = utility.GetRelatedProperties(aveListItem);
                            //foreach (var relatedInfo in relatedInfos)
                            //{
                            //    utility.RemoveRelateColumnValue(relatedInfo, this.parentWeb.SPWeb.Site, fullUrl, aveListItem.UniqueId, "");
                            //}
                            //Local&365:当前user 自动check out/Check Out的文件，可以调用delete方法直接删除，不会抛错。
                            desFile.Delete();
                            mLog.Info("Delete destination File Successful.File unique id:{0}.", desFile?.UniqueId);
                        }
                        catch (Exception e)
                        {
                            mLog.Info("Can not delete current file,id:{0},Message{1}.", desFile?.UniqueId, e.ToString());
                            //if (ScheduleConfiguration.CheckisRecord(desFile.Item))
                            {
                                mLog.Info("current file is Declare Status and will be Undo declare it.File unique Id:{0}.", desFile?.UniqueId);
                                record.UndeclareItemAsRecord(desFile.Item);
                                mLog.Info("Undo declare File Successful.File unique id:{0}.", desFile?.UniqueId);
                                desFile.Delete();
                                mLog.Info("Delete Declare File Successful.File unique id:{0}.", desFile?.UniqueId);
                            }
                        }
                    }
                    else
                    {
                        //当前文件在目的端不存在.
                        mLog.Info("File Not Exists,It may be auto check out file.file unique id:{0}.", desFile.UniqueId);
                    }
                }
                catch (Exception overwriteEx)
                {
                    mLog.Warn("An Exception occur while Before Overwrite restore,Message:{0}.", overwriteEx.ToString());
                }
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemSchemaDependency">false means throw Exception if CT Not Exist  </param>
        /// <param name="skipItemWhenConflict">true means throw Exception if CT conflict</param>
        /// <param name="config"></param>
        public void ItemLevelRestoreItemCTAndFields(Dictionary<string, object> userData, RestoreableObject aveObject)
        {
            using (var performance = new AgentPerformanceScope("SPStubDocImport.ItemLevelRestoreItemCTAndFields", addToStatistics: true))
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
                fieldRestoreOptions.FindOption = new FieldFindOption[] { FieldFindOption.Schema, FieldFindOption.Id, FieldFindOption.InternalName, FieldFindOption.StaticName };

                AveContentTypeRestoreOption ContentTypeRestoreOption = new AveContentTypeRestoreOption();
                ContentTypeRestoreOption.FindOption = new ContentTypeFindOption[] { ContentTypeFindOption.FindBySchema, ContentTypeFindOption.FindById, ContentTypeFindOption.FindByName };
                //FindScope Only FindOption = ContentTypeFindOption.FindByParent can be used 
                ContentTypeRestoreOption.FindScope = new ContentTypeFindScope[] { ContentTypeFindScope.Current, ContentTypeFindScope.Parent, ContentTypeFindScope.Children };
                ContentTypeRestoreOption.CreateOption = new ContentTypeCreateOption[] { ContentTypeCreateOption.UseId, ContentTypeCreateOption.ForceCreate, ContentTypeCreateOption.UseParent };
                ContentTypeRestoreOption.GetParentOption = GetParentContentTypeOption.RestoreFamily;
                ItemDependencyOption itemDependencyOption = ItemDependencyOption.Overwrite;
                switch (itemDependencyOption)
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
                    //TODO:Add new option
                    if (itemDependencyOption == ItemDependencyOption.NotRestore)
                    {
                        aveSPItem.EnsureItemContentTypeDependency(userData, skipItemWhenNotFound, skipItemWhenConflict, ContentTypeRestoreOption);
                    }
                    else
                    {
                        aveSPItem.EnsureItemSchemaDependency(userData, skipItemWhenNotFound, skipItemWhenConflict, ContentTypeRestoreOption, fieldRestoreOptions);
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
            }
        }

        /// <summary>
        /// Restore Document Metadata Dto
        /// </summary>
        /// <param name="fileRestoreOption"></param>
        /// <param name="documentMetadataDto"></param>
        /// <param name="restoreStream"></param>
        /// <returns></returns>
        private void RestoreDocumentMetadataDto(IAveRestoreStream restoreStream, SPDocumentMetadataDto documentMetadataDto)
        {
            using (var performance = new AgentPerformanceScope("SPStubDocImport.RestoreDocumentMetadataDto", addToStatistics: true))
            {
                this.SetStream(restoreStream);
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
                    mLog.Info("AveRestoreResult is : {0}", result.ToString().LogBase64());
                    throw new ConetentSkipException("StorageOptimization_SOARDocImportSkipConflictItem");
                }
            }
        }

        private void RestorePermission(IAveRestoreStream restoreStream)
        {
            using (var performance = new AgentPerformanceScope("SPStubDocImport.RestorePermission", addToStatistics: true))
            {
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
                                security.RestoreRoleAssignments(roleAssignments, new SecurityRestoreOption() { ConflictResolutionForSecurityObject = ConflictResolutionForSecurityObject.OverWrite, ConflictResolutionForPincipal = ConflictResolutionForPincipal.OverWrite });
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private AveRestoreResult RestoreDocument(SPDocumentMetadataDto documentMetadataDto)
        {
            using (var performance = new AgentPerformanceScope("SPStubDocImport.RestoreDocument", addToStatistics: true))
            {
                var restoreResult = this.RestoreSelf(documentMetadataDto.DocInfo_Old, documentMetadataDto.UserDataInfo,
                                                     documentMetadataDto.DocDataJunction, documentMetadataDto.WebParts);
                if (this.AveSPItem != null)
                {
                    this.AveSPItem.RestoreLookupFieldGuidValue(documentMetadataDto.ItemTPGUIDofLookupValue);
                }
                return restoreResult;
            }
        }

        public void Dispose()
        {
            //sp object here do not neet to dispose ,we must use it next document and we dispose it in the end
        }

        private void DisposeObj(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }

    }
}
