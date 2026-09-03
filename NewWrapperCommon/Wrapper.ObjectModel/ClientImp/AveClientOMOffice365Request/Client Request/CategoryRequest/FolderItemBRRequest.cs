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
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClientFile = Microsoft.SharePoint.Client.File;
using System.Collections.Specialized;
using System.Globalization;
using System.Collections;
using Microsoft.SharePoint.Client.RecordsRepository;
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request
    {
        [KeepOriginalWithAPI]
        public override Dictionary<string, Dictionary<string, int>> GetListItemGuidAndRowIdMappingsInLargeList(string webServerRelativeUrl, string rootFolderServerRelativeUrl, Guid listId, List<string> fieldNameList)
        {
            return base.GetListItemGuidAndRowIdMappingsInLargeList( webServerRelativeUrl,  rootFolderServerRelativeUrl,  listId,  fieldNameList);
        }

        [NoAPI]
        public override void RevertAllDocumentContentStreams(string webServerRelativeUrl)
        {
            base.RevertAllDocumentContentStreams( webServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override void DeleteViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string fieldName)
        {
            base.DeleteViewField(webServerRelativeUrl, listTitle, listId, viewId, fieldName);
        }

        [NoAPI]
        public override int GetListItemRatings(string listItemUrl)
        {
            return base.GetListItemRatings( listItemUrl);
        }

        [ReplaceByAPI("No file.Properties")]
        protected override void LoadFileSpecialProperty(ClientContext context, ClientFile file)
        {
            context.Load(file, f => f.CheckedOutByUser, f => f.Author, f => f.ModifiedBy,f=>f.Properties);
        }

        [ReplaceByAPI("No file.Properties")]
        public override void AssembleFileProperties(Dictionary<string, object> fileProperties, ClientFile file, string webServerRelativeUrl, ListItem item)
        {
            base.AssembleFileProperties(fileProperties, file, webServerRelativeUrl, item);

            if (file.IsObjectPropertyInstantiated("Properties"))
            {
                fileProperties["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = file.Properties.FieldValues;
            }
        }

        [ReplaceByAPI("No file.Properties")]
        protected override void LoadFiles(AveClientContext context, Folder folder, string listName)
        {
            if (string.IsNullOrEmpty(listName))
            {
                context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.CheckedOutByUser, file => file.Author, file => file.ModifiedBy, file => file.Properties));
            }
            else
            {
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    //if (folder.Files.Count > 0)
                    //{
                    using (excepScope.StartTry())
                    {
                        context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.ListItemAllFields,
                                                                                       file => file.CheckedOutByUser,
                                                                                       file => file.Author,
                                                                                       file => file.ModifiedBy,
                                                                                       file => file.Properties));
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.CheckedOutByUser,
                                                                                       file => file.Author,
                                                                                       file => file.ModifiedBy,
                                                                                       file => file.Properties));
                    }
                }
            }
        }

        [ReplaceByAPIAttribute]
        public override Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile newFile = null;
                string fileType = Path.GetExtension(urlOfFile);
                if (mSpecialFileList.Contains(fileType, StringComparer.OrdinalIgnoreCase))
                {
                    Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                    FileCreationInformation fci = new FileCreationInformation();
                    fci.Url = urlOfFile;
                    fci.Content = file;
                    fci.Overwrite = overwrite;
                    newFile = AddFileByAPI(folder.Files, fci);
                }
                else
                {
                    context.ExecuteQuery();
                    MemoryStream stream = new MemoryStream(file);
                    if (urlOfFile.StartsWith("http", StringComparison.OrdinalIgnoreCase) || urlOfFile.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        //urlOfFile = urlOfFile.Substring(WebAppName.Length);
                        Uri fileUri = new Uri(urlOfFile);
                        urlOfFile = fileUri.AbsolutePath;
                    }
                    else if (!string.IsNullOrEmpty(webServerRelativeUrl) && (string.IsNullOrEmpty(folderServerRelativeUrl) || !urlOfFile.Trim('/').StartsWith(folderServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase)) && !urlOfFile.Trim('/').StartsWith(webServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        if (urlOfFile.Trim('/').IndexOf("/", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            if (string.IsNullOrEmpty(folderServerRelativeUrl))
                            {
                                urlOfFile = webServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                            }
                            else
                            {
                                urlOfFile = folderServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                            }
                        }
                        else
                        {
                            urlOfFile = string.Format("{0}/{1}", webServerRelativeUrl.TrimEnd('/'), urlOfFile.TrimStart('/'));
                        }
                    }
                    newFile = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(urlOfFile));
                    newFile.SaveBinary(new FileSaveBinaryInformation { ContentStream = stream });
                }

                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        context.Load(newFile);
                        context.Load(newFile.ListItemAllFields);
                        context.Load(newFile.CheckedOutByUser);
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(newFile);
                    }
                }
                context.ExecuteQuery();
                if (excepScope.HasException)
                {
                    mLogger.Warn("Get AddFile CheckedOutByUser Error, newFileUrl:{0} , Error Message:{1}", urlOfFile, excepScope.ErrorMessage);
                }
                fileProperties["Exists"] = true;
                AssembleFileProperties(fileProperties, newFile, webServerRelativeUrl, newFile.ListItemAllFields);
                return fileProperties;
            }
        }

        [ReplaceByAPIAttribute]
        public override Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, string listName, Stream file, bool overwrite, string checkInComment, bool checkRequiredFields, bool? listEnableMinorVersion)
        {
            try
            {
                string serverRelativeUrl = string.Empty;
                if (urlOfFile.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    urlOfFile.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = urlOfFile.Substring(WebAppName.Length);
                }
                else if (urlOfFile.StartsWith(folderServerRelativeUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = urlOfFile;
                }
                else
                {
                    serverRelativeUrl = folderServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                }
                using (AveClientContext context = CreateContext())
                {
                    Web parentWeb = context.Site.OpenWeb(webServerRelativeUrl);
                    var filepath = ResourcePath.FromDecodedUrl(serverRelativeUrl);
                    ClientFile targetFile = parentWeb.GetFileByServerRelativePath(filepath);
                    ConditionalScope conditionScope = new ConditionalScope(context, () => targetFile.Exists, true);
                    using (conditionScope.StartScope())
                    {
                        using (conditionScope.StartIfTrue())
                        {
                            context.Load(targetFile);
                        }
                    }
                    context.ExecuteQuery();
                    bool exist = conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
                    bool needCheckin = false;
                    if (exist && listName != null && targetFile.CheckOutType == CheckOutType.None)
                    {
                        targetFile.CheckOut();
                        needCheckin = true;
                    }
                    //".master", ".evtx", ".cs"
                    string fileType = Path.GetExtension(serverRelativeUrl);
                    if (string.Equals(fileType, ".master", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(fileType, ".evtx", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(fileType, ".cs", StringComparison.OrdinalIgnoreCase)
                        || file.Length < WrapperConfiguration.BPOS_S.UploadLimit)
                    {
                        FileSaveBinaryInformation saveInfo = new FileSaveBinaryInformation();
                        saveInfo.ContentStream = file;
                        targetFile.SaveBinary(saveInfo);//if file is not exist, this function will create new, else update the file.
                    }
                    else
                    {
                        mLogger.Info("Upload file by slice");
                        ClientResult<long> bytesUploaded = null;

                        Guid uploadId = Guid.NewGuid();
                        using (BinaryReader br = new BinaryReader(file))
                        {
                            byte[] buffer = new byte[2 * 1024 * 1024];
                            Byte[] lastBuffer = null;
                            long fileoffset = 0;
                            long totalBytesRead = 0;
                            int bytesRead;
                            bool first = true;
                            bool last = false;

                            // Read data from filesystem in blocks 
                            while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalBytesRead = totalBytesRead + bytesRead;

                                // We've reached the end of the file
                                if (totalBytesRead == file.Length)
                                {
                                    last = true;
                                    // Copy to a new buffer that has the correct size
                                    lastBuffer = new byte[bytesRead];
                                    Array.Copy(buffer, 0, lastBuffer, 0, bytesRead);
                                }

                                if (first)
                                {
                                    using (MemoryStream contentStream = new MemoryStream())
                                    {
                                        // Add an empty file.
                                        FileCollectionAddParameters fileAddParameters = new FileCollectionAddParameters();
                                        fileAddParameters.Overwrite = true;
                                        var filePath = ResourcePath.FromDecodedUrl(serverRelativeUrl);
                                        var folderPath = ResourcePath.FromDecodedUrl(folderServerRelativeUrl);
                                        targetFile = parentWeb.GetFolderByServerRelativePath(folderPath).Files.AddUsingPath(filePath, fileAddParameters, contentStream);

                                        // Start upload by uploading the first slice. 
                                        using (MemoryStream s = new MemoryStream(buffer))
                                        {
                                            // Call the start upload method on the first slice
                                            bytesUploaded = targetFile.StartUpload(uploadId, s);
                                            context.ExecuteQuery();
                                            // fileoffset is the pointer where the next slice will be added
                                            fileoffset = bytesUploaded.Value;
                                        }

                                        // we can only start the upload once
                                        first = false;
                                    }
                                }
                                else
                                {
                                    // Get a reference to our file
                                    var fileUrlPath = ResourcePath.FromDecodedUrl(serverRelativeUrl);
                                    targetFile = parentWeb.GetFileByServerRelativePath(fileUrlPath);

                                    if (last)
                                    {
                                        // Is this the last slice of data?
                                        using (MemoryStream s = new MemoryStream(lastBuffer))
                                        {
                                            // End sliced upload by calling FinishUpload
                                            targetFile.FinishUpload(uploadId, fileoffset, s);
                                            context.ExecuteQuery();

                                            // return the file object for the uploaded file
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        using (MemoryStream s = new MemoryStream(buffer))
                                        {
                                            // Continue sliced upload
                                            bytesUploaded = targetFile.ContinueUpload(uploadId, fileoffset, s);
                                            context.ExecuteQuery();
                                            // update fileoffset for the next slice
                                            fileoffset = bytesUploaded.Value;
                                        }
                                    }
                                }

                            } // while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                        }
                    }

                    if (needCheckin)//restore checkInComment as local.
                    {
                        targetFile.CheckIn(checkInComment, listEnableMinorVersion.HasValue && listEnableMinorVersion.Value ? CheckinType.MinorCheckIn : CheckinType.MajorCheckIn);
                    }
                    if (!string.IsNullOrEmpty(listName))
                    {
                        SafeLoadFile(context, targetFile);
                    }
                    else
                    {
                        context.Load(targetFile);
                    }
                    context.ExecuteQuery();
                    Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                    fileProperties["Exists"] = true;
                    fileProperties["ListName"] = listName;
                    AssembleFileProperties(fileProperties, targetFile, webServerRelativeUrl, targetFile.ListItemAllFields);
                    return fileProperties;
                }
            }
            catch (WebException webEx)
            {
                HttpWebResponse response = webEx.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.RequestUriTooLong &&
                    response.Headers != null && response.Headers["X-MSDAVEXT_Error"] != null)//Block Type File.eg. '***.ashx'
                {
                    string message = System.Web.HttpUtility.HtmlDecode(response.Headers["X-MSDAVEXT_Error"]);
                    if (message.StartsWith("589924", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(message.Substring(message.IndexOf(" ", StringComparison.OrdinalIgnoreCase) + 1).Trim());
                    }
                }
                throw;
            }
        }

        [KeepOriginalWithAPIAttribute]
        public override Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, int templateFileType)
        {
            return base.AddFile(webServerRelativeUrl, folderServerRelativeUrl, urlOfFile, templateFileType);
        }

        [KeepOriginalWithAPIAttribute]
        public override Dictionary<string, object> AddFolder(string webServerRelativeUrl, Guid listId, string folderServerRelativeUrl, string strUrl)
        {
            return base.AddFolder(webServerRelativeUrl, listId, folderServerRelativeUrl, strUrl);
        }

        [KeepOriginalWithAPIAttribute]
        public override Dictionary<string, object> AddView(string webServerRelativeUrl, string listTitle, Guid listId, string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, int type, bool bPersonalView)
        {
            return base.AddView(webServerRelativeUrl, listTitle, listId, strViewName, strCollViewFields, strQuery, iRowLimit, bPaged, bMakeViewDefault, type, bPersonalView);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetUserSolutions()
        {
            return base.GetUserSolutions();
        }
        [KeepOriginalWithAPI]
        public override IList<Dictionary<string, object>> GetManagedThemes()
        {
            return base.GetManagedThemes();
        }
        [KeepOriginalWithAPI]
        public override Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source)
        {
            return base.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, source);
        }
        [KeepOriginalWithAPI]
        public override Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId)
        {
            try
            {
                return mWebServiceRequest.GetFileVersionStream(webServerRelativeUrl, fileServerRelativeUrl, fileVerionServerRelativeUrl, versionId);
            }
            catch (Exception e)
            {
                try
                {
                    mLogger.Warn("get file version stream by WebService failed. error message:{0}", e.ToString());
                    return GetFileVersionStreamByRestApi(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl), fileServerRelativeUrl, versionId);
                }
                catch (Exception e1)
                {
                    mLogger.Warn("get file version stream by rest api failed. error message:{0}", e1.ToString());
                    using (AveClientContext context = CreateContext())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        var path = ResourcePath.FromDecodedUrl(fileServerRelativeUrl);
                        ClientFile file = web.GetFileByServerRelativePath(path);
                        FileVersion version = file.Versions.GetById(versionId);
                        ClientResult<Stream> content = version.OpenBinaryStream();
                        context.ExecuteQuery();
                        //binary copy is required, cause ClientResult<Stream> can't be used after context is disposed
                        //MemoryStream binary = new MemoryStream((int)content.Value.Length);
                        AveCoordinatedStream binary = new AveCoordinatedStream();
                        AveIOHelper.Copy(content.Value, binary);
                        binary.Position = 0;
                        return binary;
                    }
                }
            }
        }
        private Stream GetFileVersionStreamByRestApi(string webUrl, string fileServerRelativeUrl, int uiVersion)
        {
            string methodCmd = string.Format("getfilebyserverrelativeurl('{0}')", fileServerRelativeUrl);
            string versionCmd = string.Format("versions({0})", uiVersion);
            string request = string.Format("{0}/_api/Web/{1}/{2}/$value", webUrl, methodCmd, versionCmd);
            mLogger.Info("Large file include version request: {0}", request);

            Stream stream = null;
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                               new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                               new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                               new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
            retryHelper.ExecuteWithRetryMechanism(() =>
            {
                stream = GetContentStream(request, "FileVersionContentFS");
            });
            return stream;
        }
        private Stream GetContentStream(string cmd, string internalName)
        {
            ReconnectableHttpWebRequest webRequest = ReconnectableHttpWebRequest.CreateRequest(cmd);
            //调用了reset api，provideRequestDigest赋值为true。
            ITokenProvider IDCLRToken = this.tokenProviders.GetProviderByType(TokenType.IDCLR);
            webRequest.SetTokenProvider(mWebUrl, IDCLRToken != null ? IDCLRToken : tokenProviders.MainTokenProvider, true);
            var result = webRequest.GetResponse() as HttpWebResponse;
            AveCoordinatedStream content = new AveCoordinatedStream();
            using (Stream stream = result.GetResponseStream())
            {
                AveIOHelper.Copy(stream, content);
                content.Position = 0;
            }
            return content;
        }

        [KeepOriginalWithAPI]
        public override byte[] GetFileBinary(string webServerRelativeUrl, string fileServerRelativeUrl, int options)
        {
            return base.GetFileBinary(webServerRelativeUrl, fileServerRelativeUrl, options);
        }

        [NoAPI]
        public override List<Dictionary<string, object>> GetListCheckOutFiles(string webServerRelativeUrl, Guid listId)
        {
            return base.GetListCheckOutFiles(webServerRelativeUrl, listId);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields)
        {
            // 由于已经马上6.10 hard了，为了防止客户问题发现问题不好修改，提供配置文件控制是否使用以前web service的方式来获取column value。
            // 如果之后客户已经测试都没有问题，请删除这个控制，直接使用API方式。
            if (WrapperConfiguration.BPOS_S.BackupItemVersionByAPI)
            {
                return GetItemVersions(webRelativeUrl, listId, itemId, "", needLoadFields);
            }
            else
            {
                return mWebServiceRequest.GetItemVersions(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, cultureInfo, needLoadFields);
            }
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listId, int itemId, string itemUrl, Dictionary<string, string> needLoadFields)
        {
            var fileVersions = new Dictionary<int, FileVersion>();
            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
            using (ClientContext context = CreateContext())
            {
                var web = context.Site.OpenWeb(webRelativeUrl);
                var list = web.Lists.GetById(new Guid(listId));
                var item = list.GetItemById(itemId);
                context.Load(item, i => i.Versions, i => i.File.Exists);
                context.ExecuteQuery();
                var isFile = item.File.IsPropertyAvailable("Exists") && item.File.Exists;
                if (isFile)
                {
                    context.Load(item.File.Versions, fv => fv.Include(f => f.CheckInComment,
                        f => f.ID));
                    context.Load(item.File, f => f.UIVersionLabel, f => f.CheckInComment);
                    context.ExecuteQuery();
                    fileVersions = item.File.Versions.ToDictionary(fv => fv.ID, fv => fv);
                }
                if (item.Versions.Count <= 0)
                {
                    listItemVersionsProperties["HasVersion"] = false;
                }
                foreach (var version in item.Versions)
                {
                    Dictionary<string, object> fieldValues = new Dictionary<string, object>();
                    var listItemVersionProperties = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> fieldValue in version.FieldValues)
                    {
                        var value = fieldValue.Value;
                        // 
                        if (string.Equals(fieldValue.Key, "Created_x0020_Date", StringComparison.Ordinal))
                        {
                            value = DateTime.Parse(value.ToString(), null, DateTimeStyles.AdjustToUniversal);
                        }
                        AssembleItemProperties(fieldValues, value, fieldValue.Key);
                    }
                    #region set check in comment for documents
                    if (isFile)
                    {
                        if (version.VersionLabel.Equals(item.File.UIVersionLabel))
                        {
                            fieldValues["_CheckinComment"] = item.File.CheckInComment;
                        }
                        else if (fileVersions.ContainsKey(version.VersionId))
                        {
                            var fileVersion = fileVersions[version.VersionId];
                            fieldValues["_CheckinComment"] = fileVersion.CheckInComment;
                        }
                    }
                    #endregion
                    itemVersionPropertiesList.Add(GetNeedLoadFields(fieldValues, needLoadFields));
                }
                listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
                return listItemVersionsProperties;
            }
        }


        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            return base.GetFileVersions(webServerRelativeUrl, fileServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetAttachments(string webRelativeUrl, string listTitle, int itemId)
        {
            return base.GetAttachments(webRelativeUrl, listTitle, itemId);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFile(string webServerRelativeUrl, string serverRelativeUrl, string listName)
        {
            return base.GetFile(webServerRelativeUrl, serverRelativeUrl, listName);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFiles(string webServerRelativeUrl, string listName, string folderServerRelativeUrl)
        {
            return base.GetFiles(webServerRelativeUrl, listName, folderServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        {
            return base.GetFolders(webServerRelativeUrl, listName, listId, folderServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFolder(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        {
            return base.GetFolder(webServerRelativeUrl, listName, listId, folderServerRelativeUrl);

        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetItems(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode)
        {
            return base.GetItems(webServerRelativeUrl, listName, listId, camlQueryNode);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetItemByGuid(Guid webId, Guid listId, Guid tp_Guid)
        {
            return base.GetItemByGuid(webId, listId, tp_Guid);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Guid uniqueId)
        {
            return base.GetItem(webServerRelativeUrl, listName, listId, itemId, uniqueId);
        }

        public override Dictionary<string, object> GetItemByUniqueId(Guid webId, Guid listId, Guid itemId)
        {
            return base.GetItemByUniqueId(webId, listId, itemId);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetItemByUrl(Guid webId, string itemUrl, out Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                listId = Guid.Empty;
                Dictionary<string, object> itemProp = null;
                Web web = context.Site.OpenWebById(webId);
                var path = ResourcePath.FromDecodedUrl(itemUrl);
                ListItem item = web.GetListItemUsingPath(path);
                var list = item.ParentList;
                if (item != null)
                {
                    context.Load(list, tempList => tempList.BaseType, tempList => tempList.BaseTemplate, tempList => tempList.Id);
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments);
                    context.Load(item, tempItem => tempItem.DisplayName);
                    context.ExecuteQuery();

                    listId = list.Id;
                    itemProp = new Dictionary<string, object>();
                    GetItemDic(itemProp, item);
                    if (!ItemHasVersion(list, itemProp) || !WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                    {
                        itemProp["HasVersion"] = false;
                    }
                }
                return itemProp;
            }
        }

        [ReplaceByAPI]
        public override void GetItemDic(Dictionary<string, object> itemProperties, ListItem item)
        {
            var properties = new Hashtable();
            try
            {
                if (item.IsObjectPropertyInstantiated("Properties"))
                {
                    foreach (var p in item.Properties.FieldValues)
                    {
                        properties[p.Key] = p.Value;
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Error while loading item properties.Error:{0}", e);
            }
            itemProperties["Properties"] = properties;
            base.GetItemDic(itemProperties, item);
        }

        [ReplaceByAPI]
        protected override void LoadItemProperty(AveClientContext context, ListItem item)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                //ADO-157190 365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName时会出异常
                using (scope.StartTry())
                {
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments, tempItem => tempItem.DisplayName, tempItem => tempItem.Properties);
                }
                using (scope.StartCatch())
                {
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments);
                }
            }
        }

        [KeepOriginalWithAPI]
        public override void DeleteFolder(string webServerRelativeUrl, string folderServerRelativeUrl)
        {
            base.DeleteFolder(webServerRelativeUrl, folderServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override void DeleteItem(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId)
        {
            base.DeleteItem(webServerRelativeUrl, listUrl, listTitle, listId, itemId);
        }

        [ReplaceByAPI]
        public override void DeleteItemVersion(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, int versionId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listTitle);
                }
                context.Load(list);
                ListItem item = list.GetItemById(itemId);
                var version = item.Versions.GetById(versionId);
                version.DeleteObject();
                context.ExecuteQuery();
            }
        }

        [KeepOriginalWithAPI]
        public override void DeleteView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId)
        {
            base.DeleteView(webServerRelativeUrl, listName, listId, viewId);
        }

        [KeepOriginalWithAPI]
        public override void DeleteRecycleItem(Guid id, string webServerRelativeUrl = null)
        {
            base.DeleteRecycleItem(id, webServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override void DeleteFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            base.DeleteFileVersions(webServerRelativeUrl, fileServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override void DeleteFileVersion(string webServerRelativeUrl, string fileServerRelativeUrl, int id)
        {
            base.DeleteFileVersion(webServerRelativeUrl, fileServerRelativeUrl, id);
        }

        [KeepOriginalWithAPI]
        public override void DeleteFileVersion(string fileServerRelativeUrl, string webServerRelativeUrl, string versionLabel)
        {
            base.DeleteFileVersion(webServerRelativeUrl, fileServerRelativeUrl, versionLabel);
        }

        [KeepOriginalWithAPI]
        public override void DeleteAttachment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid webId, Guid listId, int rowId, string attachmentName)
        {
            base.DeleteAttachment(webServerRelativeUrl, listServerRelativeUrl, listTitle, webId, listId, rowId, attachmentName);
        }

        [KeepOriginalWithAPI]
        public override void DeleteFile(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            base.DeleteFile(webServerRelativeUrl, fileServerRelativeUrl);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> AddAttachmentNow(string webRelativeUrl, string listName, Guid listId, int itemId, string leafName, byte[] attachment)
        {
            Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
            Dictionary<string, object> itemProperties = base.GetItem(webRelativeUrl, listName, listId, itemId, default(Guid));
            using (var context = CreateContext())
            {
                using (var stream = new MemoryStream(attachment))
                {
                    var web = context.Site.OpenWeb(webRelativeUrl);
                    var list = listId == Guid.Empty ? web.Lists.GetByTitle(listName) : web.Lists.GetById(listId);
                    var item = list.GetItemById(itemId);
                    var attachmentCreationInfo = new AttachmentCreationInformation
                    {
                        FileName = leafName,
                        ContentStream = stream
                    };
                    var att = item.AttachmentFiles.Add(attachmentCreationInfo);
                    context.Load(att);
                    context.ExecuteQuery();
                    attachmentProperties.Add("FileName", leafName);
                    attachmentProperties.Add("ServerRelativeUrl", att.ServerRelativeUrl);
                }
                KeepItemProperties(webRelativeUrl, listName, listId, itemId, itemProperties);
            }
            return attachmentProperties;
        }
        private void KeepItemProperties(string webUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> itemProperties)
        {
            Dictionary<string, object> keeps = new Dictionary<string, object>();
            Dictionary<string, object> itemPros = new Dictionary<string, object>();
            #region Reset Modified time to keep modified time property
            itemPros.Add("Modified", itemProperties["TimeLastModified"]);
            itemPros.Add("_ModerationStatus", itemProperties["_ModerationStatus"]);
            #endregion
            keeps[AveObjectModelConstant.UpdateMethodName] = "Update";
            keeps["ChangedFieldValues"] = itemPros;
            base.UpdateItem(webUrl, listTitle, listId, itemId, keeps);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateFolder(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl, Dictionary<string, object> folderProperties)
        {
            return base.UpdateFolder(webServerRelativeUrl, listName, listId, folderServerRelativeUrl, folderProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId, Dictionary<string, object> viewProperties)
        {
            return base.UpdateView(webServerRelativeUrl, listName, listId, viewId, viewProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties)
        {
            return base.UpdateItem(webServerRelativeUrl, listName, listId, itemId, itemProperties);
        }

        [KeepOriginalWithAPI]
        public override Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId)
        {
            return base.GetListItemGuid(webId, listId, tp_Guid, rowId);
        }
        [KeepOriginalWithAPI]
        public override Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            return base.GetDocIdByTp_Guid(siteId, webId, listId, parentId, tp_Guid, rowId);
        }
        [KeepOriginalWithAPI]
        public override bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName)
        {
            return base.IsHaveSameName(webId, listId, dirName, leafName);
        }
        [KeepOriginalWithAPI]
        public override bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            return base.IsListItemHaveSameName(siteId, webId, tpGuid, listId, rowId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> AddItem(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, int parentId, int underlyingObjectType, string leafName, Dictionary<string, object> itemProperties, bool isDiscussion)
        {
            return base.AddItem(webServerRelativeUrl, listName, listId, folderUrl, parentId, underlyingObjectType, leafName, itemProperties, isDiscussion);
        }
        [NoAPI]
        public override void CustomizeReport(Dictionary<string, object> parameters, Guid reportId)
        {
            base.CustomizeReport(parameters, reportId);
        }
        [NoAPI("真实365已经走不到这个方法。")]
        public override Dictionary<string, string> GetMetaInfo(string webServerRelativeUrl, string docServerRelativeUrl)
        {
            return base.GetMetaInfo(webServerRelativeUrl, docServerRelativeUrl);
        }
        [ReplaceByAPI]
        public override void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            using (var context = CreateContext())
            {
                var webServerRelativeUrl = webUrl.Substring(WebAppName.TrimEnd('/').Length);
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(listId);
                var item = list.GetItemById(itemId);
                var isRecod = Records.IsRecord(context, item);
                context.ExecuteQuery();
                if (isRecod.Value)
                {
                    Records.UndeclareItemAsRecord(context, item);
                }
                else
                {
                    Records.DeclareItemAsRecord(context, item);
                }
                context.ExecuteQuery();
            }
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem)
        {
            return base.GetItemExist(SiteId, webId, listId, id, dirName, leafName, isListItem);
        }
        [KeepOriginalWithAPI]
        public override DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId)
        {
            return base.GetItemLastModifiedTime(siteId, webId, listId, id, hasDocLibRowId);
        }
        [KeepOriginalWithAPI]
        public override DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId)
        {
            return base.GetItemLastModifiedTime(siteId, webId, listId, tp_Guid, ref docId);
        }
        [KeepOriginalWithAPI]
        public override DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            return base.GetItemLastModifiedTime(siteId, webId, listId, dirName, leafName, ref docId);
        }
        [KeepOriginalWithAPI]
        public override Guid RecycleItem(string webRelativeUrl, string listRelativeUrl, string listTitle, Guid listId, int itemId)
        {
            return base.RecycleItem(webRelativeUrl, listRelativeUrl, listTitle, listId, itemId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> RestoreAttachment(Dictionary<string, object> data, Dictionary<string, object> userData, Stream fileStream)
        {
            using (AveClientContext context = base.CreateContext())
            {
                using (AveO365AttachmentRestore attachmentRestore = new AveO365AttachmentRestore(this, context, tokenProviders.MainTokenProvider))
                {
                    return attachmentRestore.RestoreAttachment(data, fileStream);
                }
            }
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream, IReport report)
        {
            string oldWebUrl = string.Empty;
            if (!string.IsNullOrEmpty(info.ParentWebRelativeUrl) && !string.IsNullOrEmpty(this.mWebUrl) && this.mWebUrl.Contains("/sites"))
            {
                oldWebUrl = this.mWebUrl;
                this.mWebUrl = string.Format("{0}{1}", this.mWebUrl.Substring(0, this.mWebUrl.IndexOf("/sites", StringComparison.OrdinalIgnoreCase)), info.ParentWebRelativeUrl);
            }
            try
            {
                using (AveClientContext context = base.CreateContext())
                {
                    Site site = context.Site;
                    using (var documentRestore = new AveO365DocumentRestore(this, site, tokenProviders, context, mServerVersion, report))
                    {
                        return documentRestore.RestoreDocument(info, fileStream);
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(oldWebUrl))
                {
                    this.mWebUrl = oldWebUrl;
                }
            }
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (AveO365FolderRestore folderRestore = new AveO365FolderRestore(this, site, context, tokenProviders))
                {
                    return folderRestore.RestoreFolder(data, userData);
                }
            }
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (var listItemRestore = new AveO365ListItemRestore(this, site, context, tokenProviders.GetProviderByType(TokenType.IDCLR)))
                {
                    return listItemRestore.RestoreListItem(data, userData, AddItemMapping);
                }
            }
        }
        [KeepOriginalWithAPI]
        public override void RestoreFileVersion(string versionLabel, string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            base.RestoreFileVersion(versionLabel, webServerRelativeUrl, fileServerRelativeUrl);
        }
        [KeepOriginalWithAPI]
        public override void RestoreRecycleItem(Guid id, string webServerRelativeUrl = null)
        {
            base.RestoreRecycleItem(id, webServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateReadOnlyField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProperties)
        {
            return base.UpdateReadOnlyField(webServerRelativeUrl, listName, listId, internalName, fieldSource, contentTypeProp, fieldProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProperties)
        {
            return base.UpdateField(webServerRelativeUrl, listName, listId, internalName, fieldSource, contentTypeProp, fieldProperties);
        }

        [NoAPI("Cannot update file properties")]
        public override Dictionary<string, object> UpdateFile(string webServerRelativeUrl, string listName, string fileServerRelativeUrl, Dictionary<string, object> prop)
        {
            return base.UpdateFile(webServerRelativeUrl, listName, fileServerRelativeUrl, prop);
        }

        [NoAPI]
        public override void RevertContentStream(string webServerRelativeUrl, string fileUrl)
        {
            base.RevertContentStream(webServerRelativeUrl, fileUrl);
        }

        [KeepOriginalWithAPI]
        public override void Publish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            base.Publish(webServerRelativeUrl, fileServerRelativeUrl, comment);
        }

        [KeepOriginalWithAPI]
        public override void UnPublish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            base.UnPublish(webServerRelativeUrl, fileServerRelativeUrl, comment);
        }

        [KeepOriginalWithAPI]
        public override void UndoCheckOut(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            base.UndoCheckOut(webServerRelativeUrl, fileServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, byte[] file)
        {
            base.SaveBinary(webServerRelativeUrl, fileServerRelativeUrl, file);
        }

        [KeepOriginalWithAPI]
        public override void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, Stream file)
        {
            base.SaveBinary(webServerRelativeUrl, fileServerRelativeUrl, file);
        }

        [KeepOriginalWithAPI]
        public override void MoveTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, int flags)
        {
            base.MoveTo(webServerRelativeUrl, fileServerRelativeUrl, strNewUrl, flags);
        }

        [ReplaceByAPI]
        public override void CopyTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, bool bOverWrite)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                var path = ResourcePath.FromDecodedUrl(strNewUrl);
                file.CopyToUsingPath(path, bOverWrite);
                context.Load(file);
                context.ExecuteQuery();
            }
        }


        [KeepOriginalWithAPI]
        public override Dictionary<string, object> CheckOut(string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            return base.CheckOut(webServerRelativeUrl, fileServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> CheckIn(string webServerRelativeUrl, string fileServerRelativeUrl, string comment, int checkinType)
        {
            return base.CheckIn(webServerRelativeUrl, fileServerRelativeUrl, comment, checkinType);
        }

        [KeepOriginalWithAPI]
        public override void Deny(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            base.Deny(webServerRelativeUrl, fileServerRelativeUrl, comment);
        }

        [KeepOriginalWithAPI]
        public override void Approve(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        {
            base.Approve(webServerRelativeUrl, fileServerRelativeUrl, comment);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> SetComplianceTag(Guid webID, Guid listID, int rowID, AveItemComplianceTagInfo complianceSettingInfo)
        {
            try
            {
                using (var context = CreateContext())
                {
                    var web = context.Site.OpenWebById(webID);
                    var list = web.Lists.GetById(listID);
                    var item = list.GetItemById(rowID);
                    //item.SetComplianceTag(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag);

                    bool blockDel = (complianceSettingInfo.ComplianceSettingFlag & 1) != 0;
                    bool blockEdit = (complianceSettingInfo.ComplianceSettingFlag & 4) != 0;
                    bool changed = (complianceSettingInfo.ComplianceSettingFlag & 2) != 0;
                    item.SetComplianceTagWithMetaInfo(complianceSettingInfo.ComplianceTag, blockDel, blockEdit, complianceSettingInfo.ComplianceWrittenDate, complianceSettingInfo.ComplianceUserLoginName, false, false);
                    //item.SetComplianceTagWithExplicitMetasUpdate(complianceTag, complianceSettingFlags, complianceWrittenDate, string.Empty);
                    context.Load(item);
                    context.Load(item, i => i.ComplianceInfo);
                    context.ExecuteQuery();
                    return AssembleComplianceTagInfo(item);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to set the compliance info from list item, webID: {0}, listID: {1}, rowID:{2}. Exception: {3}", webID, listID, rowID, ex));
            }
            return new Dictionary<string, object>();
        }

        [ReplaceByAPI]
        public override void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            using (var context = CreateContext())
            {
                var web = context.Site.OpenWebById(webID);
                var list = web.Lists.GetById(listID);
                var item = list.GetItemById(rowID);
                item.SetComplianceTag(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock, false);

                //bool blockDel = (complianceSettingInfo.ComplianceSettingFlag & 1) != 0;
                //bool blockEdit = (complianceSettingInfo.ComplianceSettingFlag & 4) != 0;
                //bool changed = (complianceSettingInfo.ComplianceSettingFlag & 2) != 0;
                //item.SetComplianceTagWithMetaInfo(complianceSettingInfo.ComplianceTag, blockDel, blockEdit, complianceSettingInfo.ComplianceWrittenDate, complianceSettingInfo.ComplianceUserLoginName, false);
                //item.SetComplianceTagWithExplicitMetasUpdate(complianceTag, complianceSettingFlags, complianceWrittenDate, string.Empty);
                context.Load(item);
                context.Load(item, i => i.ComplianceInfo);
                context.ExecuteQuery();
                //return AssembleComplianceTagInfo(item);
            }
        }

        [KeepOriginalWithAPI]
        public override void AddDocumentsetVersion(string webRelativeUrl, string listTitle, int itemId, bool isMajor, string comment)
        {
            base.AddDocumentsetVersion(webRelativeUrl, listTitle, itemId, isMajor, comment);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> AddDocumentSet(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string name, IAveContentTypeId contentTypeId)
        {
            return base.AddDocumentSet(webServerRelativeUrl, listName, listId, folderUrl, name, contentTypeId);
        }

        [ReplaceByAPI]
        public override void PostRestoreModernWebpart(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo sourceSiteInfo, Func<string, string> GetUserFromMapping)
        {
            new SharePointDocumentDataProcessor(site, mapping, sourceSiteInfo, GetUserFromMapping).PostActionImpl();
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetFolderById(string webServerRelativeUrl, Guid folderId)
        {
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            using (var context = CreateContext(mWebUrl))
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder fodler = web.GetFolderById(folderId);
                LoadFolderProperties(folderProperties, context, fodler, webServerRelativeUrl, folderId);
                return folderProperties;
            }
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetFileById(string webServerRelativeUrl, Guid fileId)
        {
            Dictionary<string, object> fileProperties = new Dictionary<string, object>();
            using (var context = CreateContext(mWebUrl))
            {
                bool fileExists = false;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var file = web.GetFileById(fileId);
                ConditionalScope fileExistScope = new ConditionalScope(context, () => file.Exists);
                using (fileExistScope.StartScope())
                {
                    using (fileExistScope.StartIfTrue())
                    {
                        SafeLoadFile(context, file);
                    }
                }
                try
                {
                    context.ExecuteQuery();
                    fileProperties["Exists"] = fileExistScope.TestResult.HasValue && fileExistScope.TestResult.Value;
                    fileExists = Convert.ToBoolean(fileProperties["Exists"]);
                }
                catch (Exception ex)
                {
                    mLogger.Debug("An error occurred while getting file.Message:{0}.", ex);
                    fileProperties["Exists"] = false;
                    fileExists = false;
                }
                if (fileExists)
                {
                    AssembleFileProperties(fileProperties, file, webServerRelativeUrl, file.ListItemAllFields);
                }
            }
            return fileProperties;
        }

        [ReplaceByAPI]
        public override void MoveTo(string parentWebUrl, string parentWebServerRelativeUrl, string folderServerRelativeUrl, string newUrl)
        {
            using (var context = CreateContext(parentWebUrl))
            {
                var folderPath = ResourcePath.FromDecodedUrl(folderServerRelativeUrl);
                var folder = context.Web.GetFolderByServerRelativePath(folderPath);
                folder.MoveToUsingPath(ResourcePath.FromDecodedUrl(newUrl));
                context.ExecuteQuery();
            }
        }


    }
}
