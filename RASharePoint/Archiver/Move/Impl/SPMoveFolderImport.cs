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
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AvePoint.RA.SharePoint.ArchiverCommon;

namespace AvePoint.RA.SharePoint.Archiver.Move
{
    public class SPMoveFolderImport : AveSPFolder, IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal AveSPSite parentSite;
        internal AveSPWeb parentWeb;
        internal AveSPList parentList;
        internal AveSPFolder parentFolder;
        ScheduleConfiguration mConfig;
        IAveRestoreStream importStream;

        public SPMoveFolderImport(AveSPFolder parentFolder, ScheduleConfiguration mConfig, IAveRestoreStream stream, string folderName)
            : base(parentFolder, folderName)
        {
            this.parentFolder = parentFolder;
            this.parentList = parentFolder.ParentList;
            this.parentWeb = parentFolder.ParentList.ParentWeb;
            this.parentSite = parentFolder.ParentSite;
            this.mConfig = mConfig;
            importStream = stream;
        }

        public void ImportAveSPFolder()
        {
            var folderInfo = importStream.ReadMetadata().GetMetadata<AveSPFolderMetadataDto>();
            //var folderInfo = importStream.TryReadMetadata(AveMetadataType.ItemMetadataDto).GetMetadata<AveSPFolderMetadataDto>();
            var userData = folderInfo.UserDataInfo;
            //for keep document set version.
            if (folderInfo.DocInfo_Old != null && folderInfo.DocInfo_Old.ContainsKey("docset_LastRefresh"))
            {
                folderInfo.DocInfo_Old.Remove("docset_LastRefresh");
            }
            //Container confilct use default.
            AveRestoreOption mRestoreOption = new AveRestoreOption(0);
            mRestoreOption.ResetRestoreMode((int)AveRestoreMode.Default);
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
            ItemLevelRestoreItemCTAndFields(userData, this);
            #region Match Exist CT Job
            //ADO-130694 Match Exist CT Job do not restore metadata
            if (mConfig.itemDependencyOption == ItemDependencyOption.NotRestore)
            {
                folderInfo.UserDataInfo = new Dictionary<string, object>();
                //#tp_ContentTypeId为ContentType 对应的Key，把这个属性还原，即可还原Content Type这个Column
                if (userData.ContainsKey("#tp_ContentTypeId"))
                {
                    folderInfo.UserDataInfo.Add("#tp_ContentTypeId", userData["#tp_ContentTypeId"]);
                }
                //File_x0020_Type为文件类型对应的key，还原时需要用它来还原图标  ADO-153817
                if (userData.ContainsKey("File_x0020_Type"))
                {
                    folderInfo.UserDataInfo.Add("File_x0020_Type", userData["File_x0020_Type"]);
                }
                #region necessary column
                //SAAS-14477 添加更新file的时候必要的column 
                if (userData.ContainsKey("Author"))
                {
                    folderInfo.UserDataInfo.Add("Author", userData["Author"]);
                }
                if (userData.ContainsKey("Editor"))
                {
                    folderInfo.UserDataInfo.Add("Editor", userData["Editor"]);
                }
                if (userData.ContainsKey("Created"))
                {
                    folderInfo.UserDataInfo.Add("Created", userData["Created"]);
                }
                if (userData.ContainsKey("Modified"))
                {
                    folderInfo.UserDataInfo.Add("Modified", userData["Modified"]);
                }
                #endregion
            }
            #endregion

            RestoreFolderMetadataDto(folderInfo);
        }

        private void RestoreFolderMetadataDto(AveSPFolderMetadataDto folderMetadataDto)
        {
            this.parentSite.SPMembers.RestoreUsers(folderMetadataDto.UserCache.Users, false, false, false);
            this.parentSite.SPMembers.RestoreGroups(folderMetadataDto.GroupCache.Groups);
            if (folderMetadataDto.MetadataInfo != null)
            {
                this.parentSite.MetadataService.Restore(folderMetadataDto.MetadataInfo);
            }
            this.RestoreSelf(folderMetadataDto.DocInfo_Old, folderMetadataDto.UserDataInfo, folderMetadataDto.DocDataJunction);
            if (this.AveSPItem != null)
            {
                this.AveSPItem.RestoreLookupFieldGuidValue(folderMetadataDto.ItemTPGUIDofLookupValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemSchemaDependency">false means throw Exception if CT Not Exist  </param>
        /// <param name="skipItemWhenConflict">true means throw Exception if CT conflict</param>
        /// <param name="config"></param>
        public void ItemLevelRestoreItemCTAndFields(Dictionary<string, object> userData, RestoreableObject aveObject)
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

            switch (mConfig.itemDependencyOption)
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
                if (mConfig.itemDependencyOption == ItemDependencyOption.NotRestore)
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
        }

        public void Dispose()
        {
            //sp object here do not neet to dispose ,we must use it next document and we dispose it in the end
            //DisposeObj(parentFolder);
            //DisposeObj(parentList);
            //DisposeObj(parentWeb);
            //DisposeObj(parentSite);
        }

    }
}
