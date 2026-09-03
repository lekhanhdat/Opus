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

using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.Internal.Restore;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    /// <summary>
    /// Ave SP File
    /// </summary>
    class SPFileImport :SPItemImport, ISPFileImport
    {
        private IFileImport fileImport;
        private ISPFolderImport parentFolder;
        private IAveFile file;
        private string serverRelativeUrl;

        public SPFileImport(SPListImport parentList, string folderUrl, string fileName)
            : base(parentList)
        {
            #region Verify Params
            if (parentList == null)
            {
                throw new ArgumentNullException("parentList");
            }

            if (string.IsNullOrEmpty(folderUrl))
            {
                throw new ArgumentNullException("folderUrl");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            #endregion
            serverRelativeUrl = folderUrl.Trim('/') + "/" + fileName;

            this.fileImport = this.parentList.ParentSite.DeploymentAPI.CreateFileImport(this.parentList.ListImport, folderUrl, fileName);
        }

        public SPFileRestoreReport Restore(Common.IAveRestoreStream restoreStream, SPFileRestoreOption spFileRestoreOption)
        {
            var profiler = new DefaultRestoreFileProfiler();

            Restore(restoreStream, spFileRestoreOption, profiler);

            return profiler.GenerateReport();
        }

        //需要提前处理一下User Group MMS信息
        public void Restore(Common.IAveRestoreStream restoreStream, SPFileRestoreOption spFileRestoreOption,
                            ISPFileRestoreProfiler restoreFileProfiler)
        {
            #region Verify Params
            if (restoreStream == null)
            {
                throw new ArgumentNullException("restoreStream");
            }

            if (spFileRestoreOption == null)
            {
                throw new ArgumentNullException("spFileRestoreOption");
            }
            if (restoreStream == null)
            {
                throw new ArgumentNullException("restoreStream");
            }
            if (spFileRestoreOption == null)
            {
                throw new ArgumentNullException("spFileRestoreOption");
            }
            #endregion

        }

        private Action<IAveRestoreStream, SPFileRestoreOption, AveMetadata, MetadataRestoreReport> GetAction(AveMetadataType metadataType)
        {
            Action<IAveRestoreStream, SPFileRestoreOption, AveMetadata, MetadataRestoreReport> action = null;
            switch (metadataType)
            {
                case AveMetadataType.DocProperty:
                case AveMetadataType.ItemMetadataDto:
                    action = RestoreItemMetadata;
                    break;
                case AveMetadataType.RoleAssignment:
                    //action = RestoreRoleAssignments;
                    break;
                //case AveMetadataType.RoleAssignmentsDto:
                //    break;
                //case AveMetadataType.RoleAssignmentInheritStatus:
                //    break;
                case AveMetadataType.DocImmedSubscriptions:
                case AveMetadataType.DocSchedSubscriptions:
                    //action = RestoreItemAlert;
                    break;
                case AveMetadataType.SocialTag:
                    //action = RestoreSocialTag;
                    break;
                case AveMetadataType.SocialComment:
                    //action = RestoreSocialComment;
                    break;
                case AveMetadataType.DocumentTagging:
                    //action = RestoreDocumentTag;
                    break;
                case AveMetadataType.WorkflowInstance:
                    //action = RestoreWorkflowInstance;
                    break;
                case AveMetadataType.WorkflowSchedule:
                    //action = RestoreWorkflowSchedule;
                    break;
            }
            return action;
        }

        private void RestoreItemMetadata(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
        {
            var metadataDto = GetSPDocumentMetadataDto(restoreStream, metadata);
            if (TryGetItem(restoreOption, metadataDto))
            {
                HandleConflict(restoreOption);
            }

            ProcessMetadataInfo(metadataDto, restoreOption);
            var importDto = ConvertSPImportDto(metadataDto, restoreStream);

            if (ShouldImportSPFile(importDto))
            {
                ImportSPFile(importDto, restoreOption);
                PostImportSPFile();
            }



            //FileImport to real restore document.

        }
        /// <summary>
        /// 执行外围特殊处理
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="metadata"></param>
        /// <param name="option"></param>
        /// <returns> </returns>
        private bool ProcessMetadataInfo(SPDocumentMetadataDto metaDto, SPFileRestoreOption option)
        {
            if (metaDto == null)
            {
                throw new ArgumentNullException("metaDto");
            }
            if (option == null)
            {
                throw new ArgumentNullException("option");
            }

            return AveDelegateExecutor.SafeExecuteFunc(option.ProcessFileMetadataDto, metaDto);
        }

        /// <summary>
        /// 真正restore document之前对Info进行处理
        /// </summary>
        /// <param name="importDto"></param>
        /// <returns></returns>
        private bool ShouldImportSPFile(SPDocumentMetadataImportDto importDto)
        {
            return CheckDependency(importDto);
        }
        
        /// <summary>
        /// Check Dependency, Lookup column list etc.
        /// PostAction
        /// </summary>
        /// <param name="importDto"></param>
        /// <returns>True is dependency has found, otherwise, return false</returns>
        private bool CheckDependency(SPDocumentMetadataImportDto importDto)
        {
            return true;
        }

        /// <summary>
        /// real restore document
        /// </summary>
        /// <param name="importDto"></param>
        private void ImportSPFile(SPDocumentMetadataImportDto importDto, SPFileRestoreOption option)
        {
            if (this.File.UIVersion > importDto.DocInfo.UIVersion)
            {
                throw new Exception("Non-Supported Insert Version");
            }
            else if (this.File.UIVersion == importDto.DocInfo.UIVersion)
            {
                UpdateCurrentVersion(importDto);
            }
            else
            {
                CreateNewVersion(importDto, option);
            }

        }

        /// <summary>
        /// Restore document 的后期处理
        /// </summary>
        private void PostImportSPFile()
        {
 
        }

        private void UpdateCurrentVersion(SPDocumentMetadataImportDto importDto)
        {
            //TODO
            //fileImport.EnsureListSetting(importDto.DocInfo.UIVersion, 0, false);
        }

        private void CreateNewVersion(SPDocumentMetadataImportDto importDto, SPFileRestoreOption option)
        {

        }

        /// <summary>
        /// 处理备份数据
        /// </summary>
        /// <param name="sourceDto"></param>
        /// <returns></returns>
        private SPDocumentMetadataImportDto ConvertSPImportDto(SPDocumentMetadataDto sourceDto, IAveRestoreStream stream)
        {
            var docImportDto = new SPDocumentMetadataImportDto();
            //docImportDto.DocInfo = sourceDto.DocInfo;
            docImportDto.WebParts = sourceDto.WebParts;
            docImportDto.StorageInfo = sourceDto.StorageInfo;
            docImportDto.StorageInfo13 = sourceDto.StorageInfo13;
            docImportDto.IsView = sourceDto.IsView;
            docImportDto.ContentStream = stream;
            docImportDto.ColumnValueInfos = this.FixupColumnValues(sourceDto.UserDataInfo, sourceDto.DocDataJunction, sourceDto.ItemTPGUIDofLookupValue);
            if (sourceDto.IsView)
            {
                docImportDto.ViewInfos = FixupViewInfo(sourceDto);
            }

            return docImportDto;
        }

        /// <summary>
        /// Doc使用LeafName Check Conflict，LeafName在构造对象的时候就传入
        /// 最好是在获取源端数据之前就进行Check，这样可以减少Skip等的处理, 需要还原的在获取数据，并且升级等处理
        /// 需要有源端Version信息，要Check 到Version级别的冲突
        /// need to init this.IAveFile object
        /// </summary>
        /// <param name="option"></param>
        /// <returns>True is need handle conflict, otherwise return false</returns>
        private bool TryGetItem(SPFileRestoreOption option, SPItemMetadataDto sourceData)
        {
            //if (option.ConflictCheckOption == SPItemConflictCheckOption.None)
            //{
            //    //reasonMsg = "No need to check conflict.";
            //    return false;
            //}
            //else if (option.ConflictCheckOption == SPItemConflictCheckOption.CheckExist)
            //{
            //    //获取IAveFile 对象，如果不存在，则抛出异常
            //}

            //和Folder下最大的LeafName进行比较，确定是否冲突
            //正常的CheckConflict 逻辑

            return true;
        }

        /// <summary>
        /// 获取到的冲突对象为IAveFile
        /// </summary>
        /// <param name="option"></param>
        /// <param name="sourceData"></param>
        private void HandleConflict(SPFileRestoreOption option)
        {
            SPItemRestoreAction action = GetItemRestoreAction(option);

            switch (action)
            {
                case SPItemRestoreAction.Skip:
                    //TODO Log
                    throw new Exception("Skip");
                case SPItemRestoreAction.DiscardCheckOut:
                    //TODO Log
                    this.File.UndoCheckOut();
                    throw new Exception("Omit");
                case SPItemRestoreAction.Default:
                    break;
            }


        }

        /// <summary>
        /// SPItemRestoreOption是根据ConflictHandleOption和具体的Handle结果共同作用得到的
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private SPItemRestoreAction GetItemRestoreAction(SPFileRestoreOption option)
        {
            SPItemRestoreAction action = SPItemRestoreAction.Skip;

            //switch (option.ConflictHandleOption)
            //{
            //    case SPItemConflictHandleOption.Skip:
            //        action = SPItemRestoreAction.Skip;
            //        break;
            //    case SPItemConflictHandleOption.Custom:
            //        if (option.ConflictHandleFunc == null)
            //        {
            //            //TODO Log;
            //            action = SPItemRestoreAction.Skip;
            //        }
            //        else
            //        {
            //            action = option.ConflictHandleFunc(this.File);
            //        }
            //        break;
            //    case SPItemConflictHandleOption.Overwrite:
            //        //TODO 删除的逻辑需要处理一下

            //        action = SPItemRestoreAction.Overwrite;
            //        break;
            //    default:
            //        action = SPItemRestoreAction.Default;
            //        break;
            //}

            return action;
        }

        private List<AveViewInfo> FixupViewInfo(SPDocumentMetadataDto sourceDto)
        {
            List<AveViewInfo> views = new List<AveViewInfo>();
            return views;
        }

        private SPDocumentMetadataDto GetSPDocumentMetadataDto(IAveRestoreStream stream, AveMetadata metadata)
        {
            SPDocumentMetadataDto docDto = null;
            switch (metadata.MetadataType)
            {
                case AveMetadataType.DocProperty:
                    docDto = new SPDocumentMetadataDto
                    {
                        DocInfo_Old = metadata.GetMetadata<Dictionary<string, object>>(),
                        WebParts = stream.GetMetadataObj<List<AveWebPartBaseInfo>>(AveMetadataType.DocWebPart),
                        MetadataInfo = stream.GetMetadataObj<List<AveTermStoreInfo>>(AveMetadataType.MetadataService),
                        UserDataInfo = stream.GetMetadataObj<Dictionary<string, object>>(AveMetadataType.DocData),
                        DocDataJunction = stream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocDataJunction),
                        ItemTPGUIDofLookupValue = stream.GetMetadataObj<Dictionary<string, string>>(AveMetadataType.LookupFieldGuidValue),
                        ItemUIVersionNums = stream.GetMetadataObj<List<int>>(AveMetadataType.DocVersions)
                    };
                    break;
                case AveMetadataType.ItemMetadataDto:
                    docDto = metadata.GetMetadata<SPDocumentMetadataDto>();
                    break;
                default:
                    WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Error, "Invalid MetadataType to get SPDocumentMetadataDto. MetadataType:{0}", metadata.MetadataType.ToString());
                    break;
            }
            return docDto;
        }

        public void Dispose()
        {
            if (fileImport != null)
            {
                fileImport.Dispose();
            }
        }

        public Common.IAveFile File
        {
            get { return this.file; }
        }

        public Common.IAveListItem Item
        {
            get { throw new NotImplementedException(); }
        }
    }
}
