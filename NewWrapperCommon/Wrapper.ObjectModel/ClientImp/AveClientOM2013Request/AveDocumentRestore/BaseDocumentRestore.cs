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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AveClientRequest.Common;
using AvePoint.GCommon;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using ClientFile = Microsoft.SharePoint.Client.File;
using AvePoint.Wrapper.Resource.Client;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.ObjectModel.WebService;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.ObjectModel.ClientOM.BaseDocumentRestore.#.ctor(AveClientRequest.Common.AveClientContext,AvePoint.ObjectModel.ClientOM.AveClientOM2013Request,System.Object,AvePoint.Wrapper.Common.AveDocumentInfo,System.IO.Stream)", MessageId = "avi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.ObjectModel.ClientOM.BaseDocumentRestore.#.ctor(AveClientRequest.Common.AveClientContext,AvePoint.ObjectModel.ClientOM.AveClientOM2013Request,System.Object,AvePoint.Wrapper.Common.AveDocumentInfo,System.IO.Stream)", MessageId = "mp4")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.ObjectModel.ClientOM.BaseDocumentRestore.#.ctor(AveClientRequest.Common.AveClientContext,AvePoint.ObjectModel.ClientOM.AveClientOM2013Request,System.Object,AvePoint.Wrapper.Common.AveDocumentInfo,System.IO.Stream)", MessageId = "wmv")]
namespace AvePoint.ObjectModel.ClientOM
{
    public abstract class BaseDocumentRestore
    {
        protected static AveLogger Log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected AveClientContext Context;
        //Do not load it for performance.
        protected Web ParentWeb;
        //{System Folder} is null.
        protected List ParentList;
        protected AveClientOM2013Request Request;
        protected AveDocumentInfo DocInfo;
        protected Stream FileStream;
        protected object Authentication;
        protected IAveWeb AveWeb;
        protected AveListMemento ListMemento;
        //If this item is newly created in this job. Used when restoring multi version.
        protected bool IsNewCreated;
        protected string FileServerRelativeUrl;
        //SystemFile: Views, Forms or Files with no ListItem.
        protected bool IsSystemFile;
        protected RestoreResult RestoreResult = RestoreResult.None;
        protected IReport mReport;
        protected List<string> SpecialFileList = new List<string>() { ".master", ".evtx", ".cs" };
        protected bool IsWebWelcomePageChange = false;
        protected LimitedWebPartManager LimitedWebPartManager;
        protected readonly List<string> needProcessVideoExtension = new List<string>() { ".wmv", ".mp4", ".avi" };

        private const string ModernArticlePage = "0x0101009D1CB255DA76424F860D91F20E6C4118";
        private const string ContentTypeId = "ContentType";

        private bool? isModernPage;
        protected bool IsModernPage
        {
            get
            {
                if (!isModernPage.HasValue)
                {
                    isModernPage = DocInfo.DocData.ContainsKey(ContentTypeId) && DocInfo.DocData[ContentTypeId].ToString().StartsWith(ModernArticlePage)
                    && DocInfo.UserData != null && DocInfo.UserData.ContainsKey("ClientSideApplicationId") && DocInfo.UserData["ClientSideApplicationId"] != null;
                }
                return isModernPage.Value;
            }
        }

        public ClientFile File { get; protected set; }

        protected BaseDocumentRestore(AveClientContext context, AveClientOM2013Request request, object authentication, AveDocumentInfo docInfo, Stream fileStream)
        {
            this.Context = context;
            this.DocInfo = docInfo;
            this.Authentication = authentication;
            this.Request = request;
            this.FileStream = fileStream;
            this.AveWeb = docInfo.DocData.ContainsKey("AveWebObject") ? (IAveWeb)docInfo.DocData["AveWebObject"] : null;
        }

        public abstract Dictionary<string, object> Restore();
        public void SetReport(IReport report)
        {
            mReport = report;
        }

        protected virtual void PrepareRestore()
        {
            IsSystemFile = DocInfo.OriginalRowId <= 0;
        }

        protected void UnlockFile(ClientFile file)
        {
            try
            {
                if (CheckFileLockStatus(file))
                {
                    this.Request.DeclareOrUndeclareItem(file.ListItemAllFields.Id, ParentList.Id, AveWeb.Url);
                    //after unlock file , need reload fileInfo.
                    LoadFileInfo(File);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to unlock file. {0}", ex);
            }
        }

        /// <summary>
        /// Before using this function, load File and File.WebPartManager.
        /// Only restore webparts, restore the wiki field after.
        /// </summary>
        /// <returns>Webpart ID mappping from Source to Destination.</returns>
        protected virtual Dictionary<string, string> RestoreWebParts(ClientFile webPartPage)
        {
            if (LimitedWebPartManager == null)
            {
                return null;
            }
            ListItem webPartPageitem = IsSystemFile ? null : webPartPage.ListItemAllFields;
            using (Ave2013WebPartRestore webpartRestore = new Ave2013WebPartRestore(Context, AveWeb, ParentWeb,
                                                                                    ParentList, webPartPage, LimitedWebPartManager,
                                                                                    webPartPageitem, DocInfo.WebPartCache, mReport, Authentication))
            {
                webpartRestore.RestoreWebPartsOnly(webpartRestore.GetNeedRestoreWebParts(DocInfo.WebParts, true));
                return webpartRestore.WebPartIdMapping;
            }
        }

        protected void LoadLimitedWebpartManager(ClientFile webPartPage)
        {
            if (DocInfo.WebParts == null || DocInfo.WebParts.Count <= 0)
            {
                return;
            }
            LimitedWebPartManager = webPartPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
            Context.Load(LimitedWebPartManager, manager => manager.WebParts);
            if (ParentList != null)
            {
                Context.Load(ParentList, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields.SchemaXml));
            }
        }

        /// <summary>
        /// 获取文件信息，对于不同类型的文件如果有不同逻辑需要进行重载
        /// </summary>
        /// <param name="file"></param>
        /// <param name="includeWebParts"></param>
        /// <returns></returns>
        protected virtual bool LoadFileInfo(ClientFile file, bool includeWebParts = true)
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
                        if (includeWebParts && LimitedWebPartManager == null)
                        {
                            LoadLimitedWebpartManager(file);
                        }
                    }
                }
                Context.ExecuteQuery();
                return conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
            }
            catch (ServerException serverException)
            {
                if (serverException.ServerErrorCode == -2130575223 ||
                    serverException.ServerErrorCode == -2147221018 || 
                    serverException.ServerErrorCode == -2147024891) //:-2130575223文件无法被更改, -2147221018文件被block(ADO-154110), -2147024891 没有权限(ADO-171229)
                {
                    throw;
                }
                Log.Debug(AveClientOMRequestResource.LoadFileInfoError, DocInfo.ParentFolderRelativeUrl + "/" + DocInfo.Name, serverException.ToString());
                return false;
            }
            catch (Exception e)
            {
                Log.Debug(AveClientOMRequestResource.LoadFileInfoError, DocInfo.ParentFolderRelativeUrl + "/" + DocInfo.Name, e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 此方法是用来处理冲突的，和文件类型无关的公用的冲突处理逻辑放在此方法中
        /// </summary>
        /// <param name="restoreResult"></param>
        /// <param name="exist"></param>
        /// <returns></returns>
        protected virtual bool ProcessConflictResolution(Dictionary<string, object> restoreResult)
        {
            bool exist = false;
            // 1. 获取文件对象
            File = GetFileByAPI(FileServerRelativeUrl);

            // 2. 加载文件信息，对于不同类型的文件需要走不同的重载
            exist = LoadFileInfo(File);
            if (!exist)
            {
                // ADO-154648 local07和local10在有video ContentType的list下的video document还原到365下结构有改变，要修改url。
                var fileExtension = Path.GetExtension(FileServerRelativeUrl);
                if (needProcessVideoExtension.Contains(fileExtension.ToLower(CultureInfo.InvariantCulture)) &&
                    ContainsVideoContentType())
                {
                    // 和add之后的处理不同这个时候不能修改FileServerRelativeUrl。因为没有video相应的folder，修改后在add的时候会出错。
                    var tempUrl = GetNewFileServerRelativeUrlForVideo(FileServerRelativeUrl);
                    File = GetFileByAPI(tempUrl);
                    exist = LoadFileInfo(File);
                    if (exist)
                    {
                        // video document已经存在，修改FileServerRelativeUrl，避免之后用到错误的url。
                        FileServerRelativeUrl = tempUrl;
                    }
                }
            }
            else
            {
                restoreResult["ConflictWithDocument"] = true;
            }

            #region Check Skip
            if (exist && !IsNewCreated && !DocInfo.SettingInfo.DELETE_ITEM && DocInfo.RestoreOption == AveRestoreMode.Default)
            {
                throw new RestoreResultException(RestoreResult.SkipConflict, "Skip to restore item because of conflict option.");
            }
            #endregion

            #region Check OverWrite By Last Modified.
            if (NeedSkipByLastModifiedTime(File, exist))
            {
                throw new RestoreResultException(RestoreResult.SkippedByLastModifiedTime, "Skip restoring the item because the item is not modified.");
            }
            #endregion

            #region 对目的端文件为Lock的情况进行处理
            if (exist && CheckFileLockStatus(File))
            {
                if (DocInfo.RestoreOption != AveRestoreMode.OverWrite)
                {
                    throw new RestoreResultException(RestoreResult.SkipConflict, "Skip to restore item because it is locked in destination");
                }
                UnlockFile(File);
            }
            #endregion

            if (NeedDeleteByFileType(exist))
            {
                restoreResult["OverwriteAllVersion"] = true;
                exist = !TryDeleteFile();
            }

            return exist;
        }

        protected virtual ClientFile GetFileByAPI(string url)
        {
            return ParentWeb.GetFileByServerRelativeUrl(url);
        }

        /// <summary>
        /// 用来决定当前的File是否应该先删除后还原
        /// </summary>
        /// <param name="exist"></param>
        /// <returns></returns>
        protected virtual bool NeedDeleteByFileType(bool exist)
        {
            return exist && DocInfo.SettingInfo.DELETE_ITEM;
        }

        /// <summary>
        /// 删除文件的公用方法，删除成功返回true
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual bool TryDeleteFile()
        {
            ExceptionHandlingScope ehScope = new ExceptionHandlingScope(Context);
            using (ehScope.StartScope())
            {
                using (ehScope.StartTry())
                {
                    if (File.CheckOutType != CheckOutType.None)
                    {
                        File.UndoCheckOut();
                    }
                    File.DeleteObject();
                }
                using (ehScope.StartCatch())
                {
                }
            }
            return true;
        }

        protected virtual bool CheckFileLockStatus(ClientFile file)
        {
            bool locked = false;
            try
            {
                if (file.ListItemAllFields.FieldValues.ContainsKey("_vti_ItemHoldRecordStatus"))
                {
                    object status = file.ListItemAllFields["_vti_ItemHoldRecordStatus"];
                    int value = 0;
                    if (status != null && int.TryParse(status.ToString(), out value))
                    {
                        locked = IsLocked(value);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(AveLogLevel.WARN, "Failed to check item lock status. Error:{0}", ex);
            }

            return locked;
        }

        protected bool IsLocked(int value)
        {
            //bool isOnHold = ((long)value & 4096L) != 0L;
            bool isRecord = ((long)value & 16L) != 0L;
            return isRecord;
            //return isOnHold || isRecord;
        }

        //Compare modified time if OverwriteByLastModified is checked.
        protected bool NeedSkipByLastModifiedTime(ClientFile file, bool exist)
        {
            if (!exist || !DocInfo.SettingInfo.OverWriteByModifiedTime || !DocInfo.DocData.ContainsKey("BiggestVersionModified"))
            {
                return false;
            }
            DateTime destModified = file.TimeLastModified;
            //和Local保持一致: 当Modified相同时。 如果源端是checkout Version, 并且目的端文件不是checkout时,则继续还原该Version。
            if ((DateTime)DocInfo.DocData["BiggestVersionModified"] == destModified)
            {
                return (DocInfo.OriginalLevel == 255 && file.Level != FileLevel.Checkout) ? false : true;
            }
            return (DateTime)DocInfo.DocData["BiggestVersionModified"] < destModified;
        }

        /// <summary>
        /// 只有一个CheckOut Version 的文件
        /// </summary>
        /// <returns></returns>
        protected bool CheckForceCheckout()
        {
            //Source is checkout and is first version.
            return (DocInfo.OriginalVersion == 512 || DocInfo.OriginalVersion == 1)
                   && (DocInfo.IsOrignialCheckOut || DocInfo.IsCheckOut);
        }

        protected void GenerateItemProperties(ClientFile file, Dictionary<string, object> result)
        {
            if (result == null)
            {
                result = new Dictionary<string, object>();
            }
            result[RestoreResult.ToString()] = true;
            if (file.ListItemAllFields.FieldValues.Count > 0)
            {
                result["RowId"] = file.ListItemAllFields.Id;
            }
            if (RestoreResult == RestoreResult.AddNew)
            {
                result["IsNewCreated"] = true;
            }
            var fileProperties = new Dictionary<string, object>();
            Request.AssembleFileProperties(fileProperties, file, AveWeb.ServerRelativeUrl, file.ListItemAllFields);
            result["File"] = fileProperties;
            if (ListMemento != null && ListMemento.IsListSettingChanged)
            {
                //For keeping list setting in wrapper.
                result["ListVersionSetting"] = ListMemento.ExportCurrentListSetting();
            }
        }

        protected virtual ClientFile AddFileAndKeepVersion(bool overwriteIfExists, ClientFile file)
        {
            if (!DocInfo.HasStream && FileStream.Length <= 0)
            {
                return file;
            }
            bool needCheckinToAddCheckinComment = false;
            needCheckinToAddCheckinComment = PrepareSaveBinary(file);
            if (!needCheckinToAddCheckinComment && file.CheckOutType == CheckOutType.None && !string.IsNullOrEmpty(DocInfo.CheckinComment))
            {
                file.CheckOut();
                needCheckinToAddCheckinComment = true;
            }
            ClientFile newFile = AddFile(overwriteIfExists);
            if (needCheckinToAddCheckinComment)
            {
                file.CheckIn(DocInfo.CheckinComment, CheckinType.OverwriteCheckIn);
            }
            return newFile;
        }

        protected bool PrepareSaveBinary(ClientFile file)
        {
            bool needCheckinToAddCheckinComment = false;
            if (file == null)
            {
                return needCheckinToAddCheckinComment;
            }
            if (file.UIVersion % 512 != 0)//enable minor version
            {
                if (file.CheckOutType == CheckOutType.None)
                {
                    file.CheckOut();
                    needCheckinToAddCheckinComment = true;
                }
                ListMemento.SetListSetting(true, true, false, false);
            }
            else
            {
                if (file.CheckOutType == CheckOutType.None)
                {
                    ListMemento.SetListSetting(false, false, false, false);
                    return needCheckinToAddCheckinComment;
                }
                ListMemento.SetListSetting(true, false, false, false);
            }
            return needCheckinToAddCheckinComment;
        }

        /// <param name="addStream">If update after adding new, do not save stream.</param>
        /// <param name="needIncrease">Increase version only once</param>
        protected virtual void UpdateVersion(ClientFile file, bool addStream, bool needIncrease = true)
        {
            bool needReload = false;
            if (file.UIVersion > DocInfo.OriginalVersion)
            {
                RestoreResult = RestoreResult.VersionConflict;
                Log.Warn("Update file version failed, destination version bigger than source. Source: {0}, Destination: {1}"
                    , DocInfo.OriginalVersion, File.UIVersion);
                return;
            }
            bool isSourceCheckout = DocInfo.IsOrignialCheckOut || DocInfo.IsCheckOut;
            bool isDestinationCheckout = file.CheckOutType != CheckOutType.None;
            if (file.UIVersion == DocInfo.OriginalVersion)
            {
                if (isSourceCheckout && !isDestinationCheckout)
                {
                    RestoreResult = RestoreResult.VersionConflict;
                    Log.Warn("Update file version failed, check out status is mismatch. Source: {0}, Destination: {1}"
                        , isSourceCheckout, isDestinationCheckout);
                    return;
                }
                if (addStream && !IsModernPage && CheckGhostPageNeedAddStream(file))
                {
                    file = AddFileAndKeepVersion(true, file);
                    LoadFileInfo(file);
                    // 改变version之后需要update，record library document的状态会变成有锁状态。需要再unlock一次。
                    UnlockFile(file);
                }
                else if (file != null && !string.IsNullOrEmpty(DocInfo.CheckinComment) && !file.CheckInComment.Equals(DocInfo.CheckinComment) && file.CheckOutType == CheckOutType.None) // ADO-71627 第一个version的file如果不是ghost page，没有地方给version add CheckInComment，在这里加入这个逻辑。
                {
                    file.CheckOut();
                    file.CheckIn(DocInfo.CheckinComment, CheckinType.OverwriteCheckIn);
                    LoadFileInfo(file);
                    UnlockFile(file);
                }
                RestoreWebParts(file);
                SetModernWebPrtFields(file, DocInfo, DocInfo.FieldsInfo.Fields);
                AveListItemRestore.SetFieldValues(file.ListItemAllFields, DocInfo.FieldsInfo.Fields);
                SetUserDataJunctionFieldValues(file.ListItemAllFields);
                SetPropertiesForXSN();
                if (!isSourceCheckout)
                {
                    Update(file, DocInfo.CheckinComment, true);
                    needReload = true;
                }
                else //if (isSourceCheckout && isDestinationCheckout)
                {
                    file.ListItemAllFields.Update();
                    needReload = true;
                }
                if (needReload)
                {
                    LoadFileInfo(file, false);
                }
                UpdateModifiedAndModeration(DocInfo.FieldsInfo.Fields, file);
            }
            else if (needIncrease)
            {
                SPDocVersion sourceVersion = new SPDocVersion(DocInfo.OriginalVersion / 512, DocInfo.OriginalVersion % 512);
                SPDocVersion destinationVersion = new SPDocVersion(file.UIVersion / 512, file.UIVersion % 512);
                var versionHandler = new AveFileVersionHandler(isSourceCheckout, isDestinationCheckout, sourceVersion, destinationVersion, DocInfo.CheckinComment, ListMemento);
                versionHandler.AddNewFileNeedDeleteVersion(RestoreResult);

                var autoDeclare = CheckAutoDeclareSetting();
                versionHandler.IncreaseVersion(file);
                RevertAutoDeclareSetting(autoDeclare);

                LoadFileInfo(file);
                // 改变version之后需要update，record library document的状态会变成有锁状态。需要再unlock一次。
                UnlockFile(file);
                UpdateVersion(file, addStream, false);
            }
            RestoreResult = RestoreResult.AddNew;
        }

        protected virtual void SetModernWebPrtFields(ClientFile file, AveDocumentInfo docInfo, Dictionary<string, object> fields)
        {
        }

        private void RevertAutoDeclareSetting(bool autoDeclare)
        {
            if (autoDeclare)
            {
                if (ParentList != null)
                {
                    ParentList.RootFolder.Properties["ecm_AutoDeclareRecords"] = autoDeclare.ToString();
                    ParentList.RootFolder.Update();
                    Context.ExecuteQuery();
                }
            }
        }

        private bool CheckAutoDeclareSetting()
        {
            var isAutoDeclareRecords = false;
            if (ParentList != null)
            {
                try
                {
                    Context.Load(ParentList, l => l.RootFolder.Properties);
                    Context.ExecuteQuery();
                    if (ParentList.RootFolder.Properties != null
                        && ParentList.RootFolder.Properties.FieldValues.ContainsKey("ecm_AutoDeclareRecords"))
                    {
                        isAutoDeclareRecords = bool.Parse(ParentList.RootFolder.Properties.FieldValues["ecm_AutoDeclareRecords"].ToString());
                    }
                    if (isAutoDeclareRecords)
                    {
                        ParentList.RootFolder.Properties["ecm_AutoDeclareRecords"] = "False";
                        ParentList.RootFolder.Update();
                        Context.ExecuteQuery();
                    }
                }
                catch(Exception e)
                {
                    Log.Warn("An error occurred while getting auto declare setting.Error:{0}", e.ToString());
                }
            }
            return isAutoDeclareRecords;
        }

        /// <summary>
        /// 检查Ghost Page是否需要添加Stream
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected bool CheckGhostPageNeedAddStream(ClientFile file)
        {
            return file.CustomizedPageStatus != CustomizedPageStatus.Uncustomized || DocInfo.OriginalPageStatus != AveCustomizedPageStatus.Uncustomized;
        }

        /// <summary>        
        ///CheckIn file and Update properties.
        ///Keep version.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="checkInComment"></param>
        protected void Update(ClientFile file, string checkInComment, bool keepVersion)
        {
            // ADO-197857 当file为大version且开启小version的时候ValidateUpdateListItem没法keep version，会涨一个小version。
            if (file.UIVersion % 512 == 0 && keepVersion)
            {
                ListMemento.SetListSetting(true, false, null, null);
            }
            MethodInfo updateMethod = typeof(ListItem).GetMethod("ValidateUpdateListItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
            string fileLeafRef = file.ListItemAllFields.FieldValues.ContainsKey("FileLeafRef") ? file.ListItemAllFields["FileLeafRef"] as string : string.Empty;
            IList<ListItemFormUpdateValue> values = new List<ListItemFormUpdateValue>();
            values.Add(new ListItemFormUpdateValue() { FieldName = "FileLeafRef", FieldValue = fileLeafRef });
            if (updateMethod.GetParameters().Length == 3)
            {
                updateMethod.Invoke(file.ListItemAllFields, new object[] { values, keepVersion, checkInComment });
            }
            else
            {
                updateMethod.Invoke(file.ListItemAllFields, new object[] { values, keepVersion });
            }
        }

        protected void SetUserDataJunctionFieldValues(ListItem item)
        {
            if (DocInfo.FieldsInfo.MultiLookupFields == null ||
                DocInfo.FieldsInfo.MultiLookupFields.Count <= 0)
            {
                return;
            }
            foreach (KeyValuePair<string, object> fieldInfo in DocInfo.FieldsInfo.MultiLookupFields)
            {
                item[fieldInfo.Key] = fieldInfo.Value.ToString();
            }
        }

        protected virtual ClientFile AddFile(bool overwriteIfExists)
        {
            string fileType = Path.GetExtension(FileServerRelativeUrl);
            ClientFile newFile = null;
            if (SpecialFileList.Contains(fileType, StringComparer.OrdinalIgnoreCase) || FileStream.Length < WrapperConfiguration.BPOS_S.UploadLimit)
            {
                FileCreationInformation fileCreationInfo = new FileCreationInformation();
                fileCreationInfo.ContentStream = FileStream;
                fileCreationInfo.Url = FileServerRelativeUrl;
                fileCreationInfo.Overwrite = overwriteIfExists;
                newFile = AddFileByAPI(GetFolderByAPI(DocInfo.ParentFolderRelativeUrl).Files, fileCreationInfo);
            }
            else
            {
                if (Context.HasPendingRequest)
                {
                    Context.ExecuteQuery();
                }
                ClientFile.SaveBinaryDirect(Context, FileServerRelativeUrl, FileStream, overwriteIfExists);
                ClientObjectData objData = AveAssemblyUtility.GetPropertyValue(ParentWeb, "ObjectData") as ClientObjectData;
                objData.MethodReturnObjects.Clear();
                newFile = GetFileByAPI(FileServerRelativeUrl);
            }
            RevertWebWelcomePage();
            return newFile;
        }

        protected ClientFile AddTemplateFile(int fileType)
        {
            Folder folder = GetFolderByAPI(DocInfo.ParentFolderRelativeUrl);
            ClientFile newFile = folder.Files.AddTemplateFile(FileServerRelativeUrl, (TemplateFileType)fileType);
            RevertWebWelcomePage();
            LoadFileInfo(newFile);
            return newFile;
        }

        //Before delete this page file, check if it is welcome page.
        protected void CancleWebWelcomePage()
        {
            if (string.IsNullOrEmpty(this.AveWeb.RootFolder.WelcomePage))
            {
                return;
            }
            string webWelcomePageUrl = AveUrlUtility.CombineUrl(this.AveWeb.ServerRelativeUrl, this.AveWeb.RootFolder.WelcomePage);
            if (!string.Equals(webWelcomePageUrl, FileServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            ParentWeb.RootFolder.WelcomePage = string.Empty;
            ParentWeb.RootFolder.Update();
            IsWebWelcomePageChange = true;
        }

        protected void RevertWebWelcomePage()
        {
            if (!IsWebWelcomePageChange)
            {
                return;
            }
            string fileUrl = FileServerRelativeUrl;
            if (FileServerRelativeUrl.StartsWith(AveWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileUrl = FileServerRelativeUrl.Substring(AveWeb.ServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            IsWebWelcomePageChange = false;
            ParentWeb.RootFolder.WelcomePage = fileUrl;
            ParentWeb.RootFolder.Update();
        }

        #region Handle Moderation Status

        protected void UpdateModifiedAndModeration(Dictionary<string, object> userData, ClientFile file)
        {
            if (!WrapperConfiguration.BPOS_S.KeepModeration ||
                ParentList == null ||
                ParentList.BaseType != BaseType.DocumentLibrary ||
                file.ListItemAllFields.FieldValues.Count <= 0)
            {
                return;
            }
            int moderationStatus = DocInfo.DocData.ContainsKey("_ModerationStatus") ? Convert.ToInt32(DocInfo.DocData["_ModerationStatus"]) : -1;
            ResetModerationStatus(ref moderationStatus);
            DateTime originalModified;
            string moderationComments;
            if (NeedWebServiceUpdate(userData, file.ListItemAllFields, moderationStatus, out originalModified, out moderationComments))
            {
                if (!ParentList.EnableModeration)
                {
                    ParentList.EnableModeration = true;
                    ParentList.Update();
                    Context.ExecuteQuery();
                }
                string webAppName = AveUrlUtility.GetServerUrl(Context.Url);
                Dictionary<string, object> needKeepData = new Dictionary<string, object>();
                needKeepData["ModerationStatus"] = moderationStatus;
                needKeepData["Modified"] = originalModified;
                needKeepData["ModerationComments"] = moderationComments;
                UpdateByWebService(file, webAppName, needKeepData);
            }
        }

        protected virtual void UpdateByWebService(ClientFile file, string webAppName, Dictionary<string, object> needKeepData)
        {
            AveWebServiceRequest.UpdateListItems(webAppName, AveWeb.ServerRelativeUrl, ParentList.Title, file.ListItemAllFields.Id, file.ListItemAllFields.FieldValues["FileRef"].ToString(), Authentication, needKeepData);
        }

        protected void ResetModerationStatus(ref int moderationStatus)
        {
            if (ParentList.EnableMinorVersions && moderationStatus == 2)
            {
                moderationStatus = 3;
            }
        }

        protected bool NeedWebServiceUpdate(Dictionary<string, object> userData, ListItem item, int moderationStatus, out DateTime originalModified, out string moderationComments) //还原Document时，checkout，checkin增加version会造成ModerationStatus，Modified，
        {
            originalModified = userData.ContainsKey("Modified") ? (DateTime)userData["Modified"] : DateTime.Now;
            moderationComments = userData.ContainsKey("_ModerationComments") ? userData["_ModerationComments"].ToString() : string.Empty;

            return (item.FieldValues.ContainsKey("_ModerationStatus") && !item.FieldValues["_ModerationStatus"].Equals(moderationStatus)) ||//if ModerationStatus equal.
                   (item.FieldValues.ContainsKey("_ModerationComments") && item.FieldValues["_ModerationComments"] != null && !item.FieldValues["_ModerationComments"].Equals(moderationComments));
        }

        #endregion

        #region for video document
        protected bool ContainsVideoContentType()
        {
            foreach (var ct in DocInfo.AveItem.Folder.ParentList.ContentTypes)
            {
                if (ct.ID.ToString().StartsWith("0x0120D520A808", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        protected string GetNewFileServerRelativeUrlForVideo(string oldUrl)
        {
            var tempUrlIndex = oldUrl.LastIndexOf('/');
            var nameWithExtension = oldUrl.Substring(tempUrlIndex + 1);
            var nameWithoutExtension = nameWithExtension.Substring(0, nameWithExtension.LastIndexOf('.'));
            var tempUrl = oldUrl.TrimEnd('/').Substring(0, tempUrlIndex);
            return string.Format("{0}/{1}/{2}", tempUrl, nameWithoutExtension, nameWithExtension);
        }
        #endregion

        #region for xsn document
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ipfs_streamhash:Document metadata")]
        protected void SetPropertiesForXSN()
        {
            if(DocInfo.Name.EndsWith(".xsn", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(DocInfo.XSNStreamHashValue))
            {
                var properties = new Dictionary<string, object>();
                var values = new Dictionary<string, object>();
                values.Add("ipfs_streamhash", DocInfo.XSNStreamHashValue);
                properties.Add("ChangedMetaInfo", values);
                Request.UpdateFile(DocInfo.ParentWebRelativeUrl, DocInfo.ParentListTitle, DocInfo.ServerRelativeUrl, properties);
            }
        }
        #endregion

        protected virtual Folder GetFolderByAPI(string url)
        {
            return ParentWeb.GetFolderByServerRelativeUrl(url);
        }

        protected virtual ClientFile AddFileByAPI(FileCollection files, FileCreationInformation info)
        {
            return files.Add(info);
        }

    }

   

    public enum PageType
    {
        Invalid = -1,
        StandardPage = 0,
        WikiPage = 1,
        WebPartPage = 2,
        ClientSidePage = 3,
        PublishingPage = 4,
    }


  

    public struct SPDocVersion
    {
        public SPDocVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }
        public int Major;
        public int Minor;
        public void AddVersion(int major, int minor)
        {
            Major += major;
            if (major > 0 && minor <= 0)
            {
                Minor = 0;
                return;
            }
            Minor += minor;
        }

        public bool Equal(SPDocVersion version)
        {
            return this.Major == version.Major &&
                   this.Minor == version.Minor;
        }

        public int ToInt()
        {
            return Major * 512 + Minor;
        }
    }
}
