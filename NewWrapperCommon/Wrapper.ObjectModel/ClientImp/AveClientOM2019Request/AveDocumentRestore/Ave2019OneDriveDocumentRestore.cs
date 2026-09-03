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
using System.IO;
using System.Linq;
using System.Text;
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using AvePoint.Wrapper.Restore;
using Microsoft.SharePoint.Client;
using File = Microsoft.SharePoint.Client.File;
using System.Globalization;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2019OneDriveDocumentRestore : BaseDocumentRestore
    {
        public Ave2019OneDriveDocumentRestore(AveClientContext context, AveClientOM2019Request request, object authentication, AveDocumentInfo docInfo, Stream fileStream)
            : base(context, request, authentication, docInfo, fileStream)
        {
        }

        protected override void PrepareRestore()
        {
            if (DocInfo.ListId == Guid.Empty)
            {
                throw new AveWrapperBaseException("List does not exist.");
            }
            base.PrepareRestore();
            IsNewCreated = DocInfo.RestoringItem.IsNewItem;
            FileServerRelativeUrl = AveUrlUtility.CombineUrl(DocInfo.ParentFolderRelativeUrl, DocInfo.Name);
            ParentWeb = Context.Site.OpenWeb(DocInfo.ParentWebRelativeUrl);
            ParentList = ParentWeb.Lists.GetById(DocInfo.ListId);
            Context.Load(ParentList);
            Context.Load(ParentList, l => l.BaseTemplate);
            ListMemento = new AveListMemento(ParentList);

        }

        public override Dictionary<string, object> Restore()
        {

            var restoreResult = new Dictionary<string, object>();
            PrepareRestore();
            try
            {
                bool exist = ProcessConflictResolution(restoreResult);

                #region 确保EnableModeration是关闭的

                if (EnsureEnableModeration())
                {
                    AddVersionChangeToResult(restoreResult);
                }

                #endregion

                if (!exist)
                {
                    AddNewFile();
                }
                else
                {
                    UpdateVersion();
                }
                LoadFileInfo(File);
            }
            catch (RestoreResultException restoreResultException)
            {
                restoreResult.Add("RestoreMessage", restoreResultException.RestoreErrorMessage);
                RestoreResult = restoreResultException.Result;
            }

            GenerateItemProperties(File, restoreResult);
            return restoreResult;

        }


        private void AddVersionChangeToResult(Dictionary<string, object> listVersionSetting)
        {
            listVersionSetting["EnableModeration"] = this.ParentList.EnableModeration;
            listVersionSetting["ListVersionSetting"] = listVersionSetting;
        }



        private bool EnsureEnableModeration()
        {
            if (!this.ParentList.EnableModeration)
            {
                return false;
            }
            ListMemento.DisableEnableModeration();
            return true;
        }


        protected new bool LoadFileInfo(File file, bool includeWebParts = true)
        {
            try
            {
                ConditionalScope conditionScope = new ConditionalScope(Context, () => file.Exists, true);
                using (conditionScope.StartScope())
                {
                    using (conditionScope.StartIfTrue())
                    {
                        Context.Load(file);
                        if (ParentList != null)
                        {
                            Context.Load(file.ListItemAllFields);
                        }
                    }
                }
                Context.ExecuteQuery();
                return conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
            }
            catch (ServerException serverException)
            {
                Log.Debug(AveClientOMRequestResource.LoadFileInfoError, DocInfo.ParentFolderRelativeUrl + "/" + DocInfo.Name, serverException.ToString());
                return false;
            }
            catch (Exception e)
            {
                Log.Debug(AveClientOMRequestResource.LoadFileInfoError, DocInfo.ParentFolderRelativeUrl + "/" + DocInfo.Name, e.ToString());
                return false;
            }
        }

        protected override bool NeedDeleteByFileType(bool exist)
        {
            return exist && DocInfo.SettingInfo.DELETE_ITEM;
        }

        private void AddNewFile()
        {
            bool forceCheckout = CheckForceCheckout();
            ListMemento.SetListSetting(null, null, null, forceCheckout);
            var folderPath = ResourcePath.FromDecodedUrl(DocInfo.ParentFolderRelativeUrl);
            FileCollectionAddParameters fileAddParameters = new FileCollectionAddParameters();
            fileAddParameters.Overwrite = true;
            var filePath = ResourcePath.FromDecodedUrl(FileServerRelativeUrl);
            File = ParentWeb.GetFolderByServerRelativePath(folderPath).Files.AddUsingPath(filePath, fileAddParameters, FileStream);
            if (!LoadFileInfo(File))
            {
                // ADO-154648 local07和local10在有video ContentType的list下的video document还原到365下结构有改变，要修改url。
                var fileExtension = Path.GetExtension(FileServerRelativeUrl);
                if (needProcessVideoExtension.Contains(fileExtension.ToLower(CultureInfo.InvariantCulture)) &&
                    ContainsVideoContentType())
                {
                    FileServerRelativeUrl = GetNewFileServerRelativeUrlForVideo(FileServerRelativeUrl);
                    var path = ResourcePath.FromDecodedUrl(FileServerRelativeUrl);
                    File = ParentWeb.GetFileByServerRelativePath(path);
                    LoadFileInfo(File);
                }
            }

            var incDriveInreaseVersion = OneDriveUpdaterbase.CreateIncreaseVersion(File, DocInfo.IsOrignialCheckOut, this.ParentList.EnableMinorVersions, this.ParentList.EnableVersioning);

            #region 注册更新属性的事件
            incDriveInreaseVersion.DcoumentUpdateEvent += delegate
            {
                AveListItemRestore.SetFieldValues(File.ListItemAllFields, DocInfo.FieldsInfo.Fields);
                this.Context.Load(File.ListItemAllFields);
                SetUserDataJunctionFieldValues(File.ListItemAllFields);
                File.ListItemAllFields.Update();
            };

            incDriveInreaseVersion.DocumentUpdateForEqualVersionEvent += delegate
            {
                //ADO-156508 给第一个version加checkincomment
                if (File != null && !string.IsNullOrEmpty(DocInfo.CheckinComment) && !File.CheckInComment.Equals(DocInfo.CheckinComment) && File.CheckOutType == CheckOutType.None)
                {
                    File.CheckOut();
                    File.CheckIn(DocInfo.CheckinComment, CheckinType.OverwriteCheckIn);
                    LoadFileInfo(File);
                }
                AveListItemRestore.SetFieldValues(File.ListItemAllFields, DocInfo.FieldsInfo.Fields);
                this.Context.Load(File.ListItemAllFields);
                SetUserDataJunctionFieldValues(File.ListItemAllFields);
                // base.Update会导致document check-in，参考OrdinaryDocument还原逻辑。
                if (!DocInfo.IsOrignialCheckOut)
                {
                    base.Update(File, string.Empty, true);
                }
                else
                {
                    File.ListItemAllFields.Update();
                }
            };

            incDriveInreaseVersion.ResetModifiedByAfterCheckInEvent += delegate
            {
                //由于checkout--checkin 时会修改file的modified by，当时的修改并没有提交，
                //而此次Reset modified by会导致ExecuteQuery时throw Exception，因此在Reset之前会先执行ExecuteQuery
                this.Context.ExecuteQuery();
                //单独修改modified by会导致update time改变，因此此处会Reset所有的数据
                if (AveListItemRestore.SetModifiedBy(File.ListItemAllFields, DocInfo.FieldsInfo.Fields))
                {
                    base.Update(File, string.Empty, true);
                }
            };
            #endregion
            incDriveInreaseVersion.AddNewFileNeedDeleteVersion(this.DocInfo.OriginalVersion, File.UIVersion);
            incDriveInreaseVersion.UpdateFileVersion(this.DocInfo.OriginalVersion, File.UIVersion);

            #region 处理Moderation属性，如果需要
            UpdateModifiedAndModeration(DocInfo.FieldsInfo.Fields, File);
            #endregion

            RestoreResult = RestoreResult.AddNew;
            UnlockFile(File);
        }

        protected virtual void UpdateVersion()
        {
            #region 对不能或者不需要还原的情况进行过滤
            if (File.UIVersion > DocInfo.OriginalVersion)
            {
                RestoreResult = RestoreResult.VersionConflict;
                Log.Warn("Update file version failed, destination version bigger than source. Source: {0}, Destination: {1}"
                    , DocInfo.OriginalVersion, File.UIVersion);
                return;
            }

            bool isSourceCheckout = DocInfo.IsOrignialCheckOut || DocInfo.IsCheckOut;
            bool isDestinationCheckout = File.CheckOutType != CheckOutType.None;

            if (File.UIVersion == DocInfo.OriginalVersion)
            {
                if (isSourceCheckout && !isDestinationCheckout)
                {
                    RestoreResult = RestoreResult.VersionConflict;
                    Log.Warn("Update file version failed, check out status is mismatch. Source: {0}, Destination: {1}"
                        , isSourceCheckout, isDestinationCheckout);
                    return;
                }
            }
            #endregion

            var increaseVersion = OneDriveUpdaterbase.CreateIncreaseVersion(File, DocInfo.IsOrignialCheckOut, this.ParentList.EnableMinorVersions, this.ParentList.EnableVersioning);

            #region 设置CheckIn Comment，在最后一次CheckIn的时候用到
            if (this.DocInfo.DocData.ContainsKey("CheckInComment") && this.DocInfo.DocData["CheckInComment"] != null)
            {
                increaseVersion.SetCheckInComment(this.DocInfo.DocData["CheckInComment"].ToString());
            }
            #endregion

            #region 注册DocumentUpdate事件，该事件里面的内容会在CheckOut之后执行
            increaseVersion.DcoumentUpdateEvent += delegate
            {
                File.SaveBinary(new FileSaveBinaryInformation() { CheckRequiredFields = false, ContentStream = this.FileStream });
                this.Context.Load(File.ListItemAllFields);
                this.Context.ExecuteQuery();
                AveListItemRestore.SetFieldValues(File.ListItemAllFields, DocInfo.FieldsInfo.Fields);
                SetUserDataJunctionFieldValues(File.ListItemAllFields);
                File.ListItemAllFields.Update();
            };

            increaseVersion.DocumentUpdateForEqualVersionEvent += delegate
            {
                File.SaveBinary(new FileSaveBinaryInformation() { CheckRequiredFields = false, ContentStream = this.FileStream });
                this.Context.Load(File.ListItemAllFields);
                this.Context.ExecuteQuery();
                AveListItemRestore.SetFieldValues(File.ListItemAllFields, DocInfo.FieldsInfo.Fields);
                SetUserDataJunctionFieldValues(File.ListItemAllFields);
                // base.Update会导致document check-in，参考OrdinaryDocument还原逻辑。
                if (!DocInfo.IsOrignialCheckOut)
                {
                    base.Update(File, string.Empty, true);
                }
                else
                {
                    File.ListItemAllFields.Update();
                }
            };

            increaseVersion.ResetModifiedByAfterCheckInEvent += delegate
            {
                //由于checkout--checkin 时会修改file的modified by，当时的修改并没有提交，
                //而此次Reset modified by会导致ExecuteQuery时throw Exception，因此在Reset之前会先执行ExecuteQuery
                this.Context.ExecuteQuery();
                //单独修改modified by会导致update time改变，因此此处会Reset所有的数据
                if (AveListItemRestore.SetModifiedBy(File.ListItemAllFields, DocInfo.FieldsInfo.Fields))
                {
                    base.Update(File, string.Empty, true);
                }
            };

            #endregion

            increaseVersion.UpdateFileVersion(DocInfo.OriginalVersion, File.UIVersion);

            #region 更改Moderation状态，如果需要
            UpdateModifiedAndModeration(DocInfo.FieldsInfo.Fields, File);
            #endregion

            RestoreResult = RestoreResult.AddNew;
        }
    }
}
