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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    public class EXOMoveItemImport : AveSPDoc, IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal AveSPSite parentSite;
        internal AveSPWeb parentWeb;
        internal AveSPList parentList;
        internal AveSPFolder parentFolder;
        internal IAveORecords record;
        internal string fileName;
        internal string desUrl;
        private readonly Guid BCSColumnID = new Guid("20f84bba906045b4af568ee102a52dcb");
        public string ErrorMessage;

        public EXOMoveItemImport(AveSPFolder parentFolder, IAveORecords record, string name, string desUrl = null)
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
        public string ImportAveEXOItem(string filePath, EXOConfiguration config, Dictionary<string, string> ItemProperties)
        {
            string realName = string.Empty;
            using (var performance = new PerformanceScope("EXOMoveItemImport.ImportAveEXOItem", "", true))            
            {
                //var docInfo = stream.TryReadMetadata(AveMetadataType.ItemMetadataDto).GetMetadata<AveSPDocumentMetadataDto>();

                //AveRestoreOption mRestoreOption = new AveRestoreOption(0);
                ContentConflictResolution contentConflictResolution = config.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution;
                AveSPDoc doc = this as AveSPDoc;
                switch (contentConflictResolution)
                {
                    case ContentConflictResolution.Overwrite:
                    case ContentConflictResolution.Skip:
                        realName = doc.Name;
                        break;
                    case ContentConflictResolution.Append:
                        realName = ResetDocNameIfNeedAppend(config, doc, doc.Name);
                        break;
                }
                this.fileName = realName;
                // DELETE_ITEM = true means Delete Des Document
                //mRestoreOption.mAveItemRestoreOption.DELETE_ITEM = true;
                //this.SetRestoreOption(mRestoreOption);

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
                //ItemLevelRestoreItemCTAndFields(userData, this, config);


                if (contentConflictResolution == ContentConflictResolution.Overwrite)
                {
                    DeleteDestinationFile(config);
                }
                //ADO-137478 多个同名文件move到同一目的端需要释放每个文件所属folder对象
                if (parentFolder.RestoringItem != null)
                {
                    parentFolder.RestoringItem.ResetNewItemValues(true, "", "");
                }
                //RestoreDocumentMetadataDto(stream, docInfo);
                RestoreDocument(contentConflictResolution, filePath, realName, config, ItemProperties);
                CheckInDestinationFile(realName);
            }
            return realName;
        }

        private void CheckInDestinationFile(string fileName)
        {
            try
            {
                var fullUrl = this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName;
                IAveFile file = this.parentWeb.SPWeb.GetFile(fullUrl);
                if (file.CheckOutType != AveCheckOutType.None)
                {
                    mLog.Info("Destination file is in checkout state (Type: {0}). Enforcing Check-In. File:{1}", file.CheckOutType, file.UniqueId);

                    file.CheckIn("");

                    mLog.Info("Check in file successful. File:{0}", file.UniqueId);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while checking in destination file. FileName:{0} Error:{1}", fileName, ex.ToString());
            }
        }

        public Guid GetDesExistFileRecordID()
        {
            using (var performance = new PerformanceScope("EXOMoveItemImport.GetDesExistFileRecordID", "", true))         
            {
                Guid fileRecordID = Guid.Empty;
                try
                {
                    IAveFile desFile = this.parentWeb.SPWeb.GetFile(this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + this.fileName);
                    if (desFile.Exists)
                    {
                        //string fileFullPath = new Uri(parentWeb.SPWeb.Site.Url).Scheme + @"://" + new Uri(parentWeb.SPWeb.Site.Url).Authority + desFile.ServerRelativeUrl.Replace("\\", "/");
                        //filePathMD5 = new Guid(HashCodeHelper.ToMD5HashCode(fileFullPath.ToLowerInvariant()));
                        fileRecordID = AvePoint.RA.RACommonUtility.IDGenerator.GetRecordId(parentWeb.SPWeb.Site.ID, desFile.UniqueId);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info("GetDesExistFileUniqueID Failed.Message:{0}.", ex.ToString());
                }
                return fileRecordID;
            }
        }

        private string ResetDocNameIfNeedAppend(EXOConfiguration config, AveSPDoc doc, string realName)
        {
            return ResetItemNameIfNeedAppend(config, doc, realName, doc.ResetAvailableName);
        }

        private string ResetItemNameIfNeedAppend(EXOConfiguration config, RestoreableObject item, string realName, Func<DateTime, string> ResetAvailableName)
        {
            string newName = ResetAvailableName(DateTime.MinValue);
            //if (!config.appendItemMapping.ContainsKeyAppendName(realName))
            //{
            //    config.appendItemMapping.AddToMappingAppendName(realName, newName);
            //}
            return newName;
        }

        private void RestoreDocument(ContentConflictResolution contentConflictResolution, string contentPath, string realName, EXOConfiguration config, Dictionary<string, string> ItemProperties)
        {
            using (var performance = new PerformanceScope("EXOMoveItemImport.RestoreDocument", "", true))
            {
                var fullUrl = this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName;
                IAveFile desFile = this.parentWeb.SPWeb.GetFile(fullUrl);
                switch (contentConflictResolution)
                {
                    case ContentConflictResolution.Skip:
                        if (desFile.Exists)
                        {
                            throw new ConetentSkipException("StorageOptimization_SOARDocImportSkipConflictItem");
                        }
                        else
                        {
                            CreateFile(contentPath, fullUrl, false);
                        }
                        break;
                    case ContentConflictResolution.Overwrite:
                        CreateFile(contentPath, fullUrl, true);
                        break;
                    case ContentConflictResolution.Append:
                        CreateFile(contentPath, fullUrl, false);
                        break;
                    default:
                        throw new Exception("invalid ContentConflictResolution");
                }
                UpdateFileItemFields(fullUrl, config, ItemProperties);
            }
        }
        private void UpdateFileItemFields(string fullUrl,EXOConfiguration config, Dictionary<string, string> ItemProperties)
        {
            try
            {
                List<MoveMetadataInfo> dataList = config.CurrentRule.EXORule.spMoveOption.MoveToSPDataList;
                bool IsCheckedMoveMetedata = config.CurrentRule.EXORule.spMoveOption.IsMoveToSP;
                if (IsCheckedMoveMetedata && dataList != null && dataList.Count > 0)
                {
                    IAveFile tempFile = this.parentWeb.SPWeb.GetFile(fullUrl);
                    foreach (var tempData in dataList)
                    {
                        var itemPropValue = ItemProperties.GetValue(tempData.ExoColumn);
                        tempFile.Item[tempData.SPColumn]= itemPropValue;
                        try
                        {
                            tempFile.Item.SystemUpdate();
                        }
                        catch (Exception e)
                        {
                            tempFile = this.parentWeb.SPWeb.GetFile(fullUrl);
                            this.ErrorMessage = "RM_ExoMoveToSP_ExoCol_ErrorMessage";
                            mLog.Error($"update file item fields failed,spcolumn:{tempData.SPColumn},exo column:{tempData.ExoColumn},item property value:{itemPropValue},error:{e.ToString()}");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                mLog.Error($"update file item fields failed,item url:{fullUrl},error:{e.ToString()}");
            }
        }
        public string UpdateBCSColumn(EXOConfiguration config, Guid termId)
        {
            using (var performance = new PerformanceScope("EXOMoveItemImport.UpdateBCSColumn", "", true))
            {
                string errorMessage = string.Empty;
                var colSetting = config.GetDestinationColumnSetting(parentSite.SiteUrl);
                if (colSetting.Exist)
                {
                    var bscField = GetBCSField(colSetting.UseExisting, colSetting.ColumnName);
                    if (bscField == null)
                    {
                        return "StorageOptimization_SOARRecordManagerEXOListBCSNotExist";
                    }
                    var term = parentSite.SPSite.AveSPTaxonomySession.GetTerm(termId);
                    if (term == null)
                    {
                        return "StorageOptimization_SOARRecordManagerEXOSourceTermNotExist";
                    }
                    if (!InSameTermScope(termId, bscField))
                    {
                        return "StorageOptimization_SOARRecordManagerEXONotInSameTermScope";
                    }
                    var fullUrl = this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + this.fileName;
                    IAveFile desFile = this.parentWeb.SPWeb.GetFile(fullUrl);
                    mLog.Info("File Exists in destination,file:{0}.", desFile.UniqueId);
                    try
                    {
                        var item = desFile.Item;
                        item[bscField.ID] = termId;
                        item[bscField.TextField] = term.Name;
                        item.SystemUpdate();
                        mLog.Info("Update destination file property successful.File:{0}.", desFile.UniqueId);
                    }
                    catch (Exception e)
                    {
                        mLog.Info("Failed to update property for current file,Name:{0},Message{1}.", desFile.Name, e.ToString());
                        errorMessage = "StorageOptimization_SOARRecordManagerEXOKeepClassificationFailed";
                    }
                }
                else
                {
                    return "StorageOptimization_SOARRecordManagerEXOTermSettingNotFound";
                }
                return errorMessage;
            }
        }

        private bool InSameTermScope(Guid termId, IAveTaxonomyField field)
        {
            try
            {
                if (field.AnchorId == Guid.Empty)
                {
                    //term scope is termset
                    var sourceTermSet = parentSite.SPSite.AveSPTaxonomySession.GetTerm(termId).TermSet;
                    return sourceTermSet.ID.Equals(field.TermSetId) ? true : false;
                }
                else
                {
                    //term scope is term
                    var destinationTerm = parentSite.SPSite.AveSPTaxonomySession.GetTerm(field.AnchorId);
                    if (destinationTerm == null)
                    {
                        return false;
                    }
                    //check if in the same termset
                    var sourceTerm = parentSite.SPSite.AveSPTaxonomySession.GetTerm(termId);
                    if (!destinationTerm.TermSet.ID.Equals(sourceTerm.TermSet.ID))
                    {
                        return false;
                    }

                    //check path of term
                    return sourceTerm.PathOfTerm.StartsWith(destinationTerm.PathOfTerm + ";") ? true : false;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"An error occurred while checking same term group. Error{e.ToString()}");
            }
            return false;
        }

        private IAveTaxonomyField GetBCSField(bool useExisting, string columnName)
        {
            IAveTaxonomyField taxField = null;
            if (!useExisting)
            {
                var bcsColumn = parentList.SPList.Fields.GetFieldById(BCSColumnID, false);
                if (bcsColumn == null)
                {
                    var tempField = parentList.SPList.Fields.Where(f => f.Title == columnName).FirstOrDefault();
                    if (tempField != null)
                    {
                        taxField = tempField as IAveTaxonomyField;
                    }
                }
                else
                {
                    taxField = bcsColumn as IAveTaxonomyField;
                }
            }
            else
            {
                var tempField = parentList.SPList.Fields.Where(f => f.Title == columnName).FirstOrDefault();
                if (tempField != null)
                {
                    taxField = tempField as IAveTaxonomyField;
                }
            }
            return taxField;
        }
        private void CreateFile(string contentPath, string fileUrl, bool overWrite)
        {
            using (FileStream stream = new FileStream(contentPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                this.parentFolder.SPFolder.Files.Add(fileUrl, stream, overWrite);
                mLog.Info("Add File Successful.");
            }
        }

        private void DeleteDestinationFile(EXOConfiguration config)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("EXOMoveItemImport.DeleteDestinationFile"))
            {
                try
                {
                    var fullUrl = this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + this.fileName;
                    IAveFile desFile = this.parentWeb.SPWeb.GetFile(fullUrl);
                    if (desFile.Exists)
                    {
                        mLog.Info("File Exists in destination,file:{0}.", desFile.UniqueId);
                        try
                        {
                            //移除related 关系
                            IAveListItem aveListItem = desFile.Item;
                            var utility = new RelatedRecordsUtility(config);
                            var relatedInfos = utility.GetRelatedProperties(aveListItem);
                            foreach (var relatedInfo in relatedInfos)
                            {
                                utility.RemoveRelateColumnValue(relatedInfo, this.parentWeb.SPWeb.Site, fullUrl, aveListItem.UniqueId, "");
                            }
                            //Local&365:当前user 自动check out/Check Out的文件，可以调用delete方法直接删除，不会抛错。
                            desFile.Delete();
                            mLog.Info("Delete destination File Successful.File:{0}.", desFile.UniqueId);
                        }
                        catch (Exception e)
                        {
                            mLog.Info("Can not delete current file,ID:{0},Message{1}.", desFile.UniqueId, e.ToString());
                            if (ArchiverCommonStaticMethod.CheckisRecord(desFile.Item))
                            {
                                mLog.Info("current file is Declare Status and will be Undo declare it.File:{0}.", desFile.UniqueId);
                                record.UndeclareItemAsRecord(desFile.Item);
                                mLog.Info("Undo declare File Successful.File:{0}.", desFile.UniqueId);
                                desFile.Delete();
                                mLog.Info("Delete Declare File Successful.File:{0}.", desFile.UniqueId);
                            }
                        }
                    }
                    else
                    {
                        //当前文件在目的端不存在.
                        mLog.Info("File Not Exists,It may be auto check out file.file Name:{0}.", desFile.UniqueId);
                    }
                }
                catch (Exception overwriteEx)
                {
                    mLog.Warn("An Exception occur while Before Overwrite restore,Message:{0}.", overwriteEx.ToString());
                }
            }
        }

        public void UpdateFields(Dictionary<string, string> propertyDic)
        {
            Guid fileId = Guid.Empty;
            using (AvePerformanceScope pc = new AvePerformanceScope("EXOMoveItemImport.UpdateFields"))
            {
                try
                {
                    var fullUrl = this.parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + this.fileName;
                    IAveFile desFile = this.parentWeb.SPWeb.GetFile(fullUrl);
                    if (desFile.Exists)
                    {
                        mLog.Info("File Exists in destination,file id:{0}.", desFile.UniqueId);
                        try
                        {
                            var item = desFile.Item;
                            foreach (var pro in propertyDic)
                            {
                                item[pro.Key] = pro.Value;
                            }
                            item.Update();
                            mLog.Info("Update destination file property successful.File id:{0}.", desFile.UniqueId);
                        }
                        catch (Exception e)
                        {
                            mLog.Info("Failed to update property for current file,Name:{0},Message{1}.", desFile.Name, e.ToString());
                        }
                        fileId = desFile.UniqueId;
                    }
                    else
                    {
                        //当前文件在目的端不存在.
                        mLog.Info("File Not Exists,It may be auto check out file.file id:{0}.", desFile.UniqueId);
                    }
                }
                catch (Exception overwriteEx)
                {
                    mLog.Warn($"An Exception occur while Before Overwrite restore,file name :{0} Message:{1}.", fileId, overwriteEx.ToString());
                }
            }
        }

        public void Dispose()
        {
            //sp object here do not neet to dispose ,we must use it next document and we dispose it in the end
        }
    }
}
