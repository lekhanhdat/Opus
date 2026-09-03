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
using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.StorageUrl.#.cctor()", MessageId = "fileops")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.StorageUrl.#.cctor()", MessageId = "vo")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.StorageUrl.#.cctor()", MessageId = "msecnd")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.StorageUrl.#.cctor()", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.StorageUrl.#.cctor()", MessageId = "frankfurt")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.StorageUrl.#.cctor()", MessageId = "avepointonlineservices")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.StorageUrl.#.cctor()", MessageId = "cloudtoken")]
namespace AvePoint.Media.Storage
{
    #region using directives
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using GCommon;
    using Util;
    #endregion

    public static class StorageUrl
    {
        private static readonly Boolean loaded;
        private static readonly Object sync = new object();
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(StorageUrl));

        #region Amazon
        private static String amazonHostName = @"s3.amazonaws.com";
        private static String amazonCaliforniaHostName = "s3-us-west-1.amazonaws.com";
        private static String amazonIrelandHostName = @"s3-eu-west-1.amazonaws.com";
        private static String amazonSingaporeHostName = @"s3-ap-southeast-1.amazonaws.com";
        private static String amazonSydneyHostName = @"s3-ap-southeast-2.amazonaws.com";
        private static String amazonTokyoHostName = @"s3-ap-northeast-1.amazonaws.com";
        private static String amazonSaopauloHostName = @"s3-sa-east-1.amazonaws.com";
        private static String amazonFrankfurtHostName = @"s3-eu-central-1.amazonaws.com";
        private static String amazonOregonHostName = @"s3-us-west-2.amazonaws.com";
        private static String amazonBucket_US = @"bucket.us";
        private static String amazonBucket_US_West = @"bucket.us.west";
        private static String amazonBucket_EU = @"bucket.eu";
        private static String amazonBucket_APAC = @"bucket.apac";
        private static String amazonBucket_Tokyo = @"bucket.tokyo";
        private static String amazonBucket_Frankfurt = @"bucket.frankfurt";
        #endregion

        #region Atmos
        private static String atmos = @"http://accessPoint.emccis.com/rest/namespace";
        #endregion

        #region AT_T
        private static String at_t = @"https://storage.synaptic.att.com/rest/namespace";
        #endregion

        #region Azure
        private static String azure = @"http://blob.core.windows.net";
        private static String azureFormat = @"{0}://{1}.blob.core.windows.net/{2}";
        private static String azureCnd = @"http://{0}.vo.msecnd.net";
        private static String azureCndFormat = @"{0}://{1}.blob.core.windows.net";
        #endregion

        #region box
        private static String box = @"https://api.box.com/2.0";
        private static String boxUserInfo = @"/users/me";
        private static String boxFindId = @"/folders/{0}/items?limit={1}&offset={2}";
        private static String boxFolderInfo = @"/folders/{0}";
        private static String boxCreateFolder = @"/folders";
        private static String boxDeleteFolder = @"/folders/{0}?recursive=true";
        private static String boxCopyFile = @"/files/{0}/copy";
        private static String boxFileInfo = @"/files/{0}";
        private static String boxList = @"/folders/{0}/items?limit={1}&offset={2}";
        private static String boxCopyFolder = @"/folders/{0}/copy";
        private static String boxDownload = @"/files/{0}/content";
        private static String boxDownloadWithVersion = @"/files/{0}/content?version={1}";
        private static String boxDeleteFile = @"/files/{0}/versions/{1}";
        private static String boxAsUser = @"/folders/0?fields=item_collection,name ";
        private static String boxUsers = @"/users";
        private static String boxListFileVersion = @"/files/{0}/versions";
        private static String boxFileVersion = @"/files/{0}/versions/{1}";
        private static String boxAuthToken = @"https://www.box.com/api/oauth2/token";
        private static String boxUpload = @"https://upload.box.com/api/2.0/files/content";
        private static String boxUploadWithVersion = @"https://upload.box.com/api/2.0/files/{0}/content";
        private static String boxFileTags = @"/files/{0}?fields=tags";
        private static String boxFolderTags = @"/folders/{0}?fields=tags";
        private static String boxGetAuthTokenWithDocAveOnline = "https://api.avepointonlineservices.com/api/cloudtoken/GetRefreshToken?refreshToken={0}&deviceType=box";
        private static String boxLockFile = @"/files/{0}?fields=lock";
        #endregion

        #region dropbox
        private static String dropboxApi = @"https://api.dropboxapi.com/2";
        private static String dropboxContentApi = @"https://content.dropboxapi.com/2";
        private static String dropboxDelta = @"/files/list_folder";
        private static String dropboxDelete = @"/files/delete";
        private static String dropboxCreateFolder = @"/files/create_folder";
        private static String dropboxDownload = @"/files/download";
        private static String dropboxNormalUpload = @"/files/upload";
        private static String dropboxUploadSessionStart = @"/files/upload_session/start";
        private static String dropboxUploadSessionAppend_v2 = @"/files/upload_session/append_v2";
        private static String dropboxUploadSessionFinish = @"/files/upload_session/finish";
        private static String dropboxSpaceUsage = @"/users/get_space_usage";
        private static String dropboxList = @"/files/list_folder";
        private static String dropboxListContinue = @"/files/list_folder/continue";
        private static String dropboxMeta = @"/files/get_metadata";
        private static String dropboxCopy = @"/files/copy";
        private static String dropboxMove = @"/files/move";
        #endregion

        #region egnyte
        private static String egnyte = @"https://{0}.egnyte.com/pubapi/v1";
        private static String egnyteOneParam = @"/fs/{1}";
        private static String egnyteTwoParam = @"/fs/{1}/{2}";
        private static String egnyteStream = @"/fs-content/{1}";
        #endregion

        #region googleDrive
        private static String googleDrive = @"https://www.googleapis.com";
        private static String googleDriveNormal = @"/drive/v2/files";
        private static String googleDriveAbout = @"/drive/v2/about?access_token={0}";
        private static String googleDriveSetToken = @"https://accounts.google.com/o/oauth2/token";
        private static String googleDriveSignIn = @"https://accounts.google.com/o/oauth2/auth?scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fdrive&redirect_uri=urn:ietf:wg:oauth:2.0:oob&response_type=code&client_id={0}";
        private static String googleDriveUpload = @"https://www.googleapis.com/upload/drive/v2/files?uploadType=multipart";
        private static String googleDriveQueryFolder = @"/drive/v2/files?access_token={0}&q={1}";
        private static String googleDriveFile = @"/drive/v2/files/{0}?access_token={1}";
        private static String googleDriveProperty = @"/drive/v2/files/{0}";
        #endregion

        #region objectAtmos
        private static String objectAtmos = @"{0}/rest/objects/{1}";
        private static String objectAtmosWithHttp = @"http://{0}/rest/objects/{1}";
        private static String objectAtmosUpload = @"{0}/rest/objects";
        private static String objectAtmosUploadWithHttp = @"http://{0}/rest/objects";
        #endregion

        #region oneDrive
        private static String oneDrive = @"https://apis.live.net/v5.0";
        private static String oneDriveSignIn = @"https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type={2}&redirect_uri={3}";
        private static String oneDriveToken = @"https://login.live.com/oauth20_token.srf";
        private static String oneDriveQuota = @"/me/skydrive/quota?access_token={0}";
        private static String oneDriveGetFolderId = @"/me/skydrive/files?access_token={0}";
        private static String oneDriveCreateFolder = @"/me/skydrive";
        private static String oneDriveUpdate = @"/{0}";
        private static String oneDriveObject = @"/{0}?access_token={1}";
        private static String oneDriveDownload = @"/{0}/content?access_token={1}";
        private static String oneDriveUpload = @"/{0}/files/{1}?access_token={2}";
        private static String oneDriveRegex = @"/([^\\/]+)/";
        #endregion

        #region rackspace
        private static String rackspace = "https://api.mosso.com/auth";
        #endregion

        public static String AmazonHostName { get { return amazonHostName; } }
        public static String AmazonCaliforniaHostName { get { return amazonCaliforniaHostName; } }
        public static String AmazonIrelandHostName { get { return amazonIrelandHostName; } }
        public static String AmazonSingaporeHostName { get { return amazonSingaporeHostName; } }
        public static String AmazonSydneyHostName { get { return amazonSydneyHostName; } }
        public static String AmazonTokyoHostName { get { return amazonTokyoHostName; } }
        public static String AmazonSaopauloHostName { get { return amazonSaopauloHostName; } }
        public static String AmazonFrankfurtHostName { get { return amazonFrankfurtHostName; } }
        public static String AmazonOregonHostName { get { return amazonOregonHostName; } }
        public static String AmazonBucket_US { get { return amazonBucket_US; } }
        public static String AmazonBucket_US_West { get { return amazonBucket_US_West; } }
        public static String AmazonBucket_EU { get { return amazonBucket_EU; } }
        public static String AmazonBucket_APAC { get { return amazonBucket_APAC; } }
        public static String AmazonBucket_Tokyo { get { return amazonBucket_Tokyo; } }
        public static String AmazonBucket_Frankfurt { get { return amazonBucket_Frankfurt; } }

        public static String Atmos { get { return atmos; } }

        public static String AT_T { get { return at_t; } }

        public static String Azure { get { return azure; } }
        public static String AzureCnd { get { return azureCnd; } }
        public static String AzureFormat { get { return azureFormat; } }
        public static String AzureCndFormat { get { return azureCndFormat; } }

        public static String Box { get { return box; } }
        public static String BoxUserInfo { get { return box + boxUserInfo; } }
        public static String BoxFindId { get { return box + boxFindId; } }
        public static String BoxFolderInfo { get { return box + boxFolderInfo; } }
        public static String BoxCreateFolder { get { return box + boxCreateFolder; } }
        public static String BoxDeleteFolder { get { return box + boxDeleteFolder; } }
        public static String BoxList { get { return box + boxList; } }
        public static String BoxCopyFile { get { return box + boxCopyFile; } }
        public static String BoxFileInfo { get { return box + boxFileInfo; } }
        public static String BoxCopyFolder { get { return box + boxCopyFolder; } }
        public static String BoxDownload { get { return box + boxDownload; } }
        public static String BoxDownloadWithVersion { get { return box + boxDownloadWithVersion; } }
        public static String BoxDeleteFile { get { return box + boxDeleteFile; } }
        public static String BoxAsUser { get { return box + boxAsUser; } }
        public static String BoxUsers { get { return box + boxUsers; } }
        public static String BoxListFileVersion { get { return box + boxListFileVersion; } }
        public static String BoxFileVersion { get { return box + boxFileVersion; } }
        public static String BoxAuthToken { get { return boxAuthToken; } }
        public static String BoxUpload { get { return boxUpload; } }
        public static String BoxUploadWithVersion { get { return boxUploadWithVersion; } }
        public static String BoxFileTags { get { return box + boxFileTags; } }
        public static String BoxFolderTags { get { return box + boxFolderTags; } }
        public static String BoxGetAuthTokenWithDocAveOnline { get { return boxGetAuthTokenWithDocAveOnline; } }
        public static String BoxLockFile { get { return box + boxLockFile; } }

        public static String DropboxDelta { get { return dropboxApi + dropboxDelta; } }
        public static String DropboxDelete { get { return dropboxApi + dropboxDelete; } }
        public static String DropboxCreateFolder { get { return dropboxApi + dropboxCreateFolder; } }
        public static String DropboxDownload { get { return dropboxContentApi + dropboxDownload; } }
        public static String DropboxNormalUpload { get { return dropboxContentApi + dropboxNormalUpload; } }
        public static String DropboxUploadSessionStart { get { return dropboxContentApi + dropboxUploadSessionStart; } }
        public static String DropboxUploadSessionAppend_v2 { get { return dropboxContentApi + dropboxUploadSessionAppend_v2; } }
        public static String DropboxUploadSessionFinish { get { return dropboxContentApi + dropboxUploadSessionFinish; } }
        public static String DropboxSpaceUsage { get { return dropboxApi + dropboxSpaceUsage; } }
        public static String DropboxList { get { return dropboxApi + dropboxList; } }
        public static String DropboxListContinue { get { return dropboxApi + dropboxListContinue; } }
        public static String DropboxMeta { get { return dropboxApi + dropboxMeta; } }
        public static String DropboxCopy { get { return dropboxApi + dropboxCopy; } }
        public static String DropboxMove { get { return dropboxApi + dropboxMove; } }

        public static String Egnyte { get { return egnyte + egnyteOneParam; } }
        public static String EgnyteCopy { get { return egnyte + egnyteTwoParam; } }
        public static String EgnyteMove { get { return egnyte + egnyteTwoParam; } }
        public static String EgnyteStream { get { return egnyte + egnyteStream; } }

        public static String GoogleDriveNormal { get { return googleDrive + googleDriveNormal; } }
        public static String GoogleDriveAbout { get { return googleDrive + googleDriveAbout; } }
        public static String GoogleDriveSetToken { get { return googleDriveSetToken; } }
        public static String GoogleDriveSignIn { get { return googleDriveSignIn; } }
        public static String GoogleDriveUpload { get { return googleDriveUpload; } }
        public static String GoogleDriveQueryFolder { get { return googleDrive + googleDriveQueryFolder; } }
        public static String GoogleDriveFile { get { return googleDrive + googleDriveFile; } }
        public static String GoogleDriveProperty { get { return googleDrive + googleDriveProperty; } }
        public static String GoogleDriveExist { get { return atmos; } }

        public static String ObjectAtmos { get { return objectAtmos; } }
        public static String ObjectAtmosWithHttp { get { return objectAtmosWithHttp; } }
        public static String ObjectAtmosUpload { get { return objectAtmosUpload; } }
        public static String ObjectAtmosUploadWithHttp { get { return objectAtmosUploadWithHttp; } }

        public static String OneDriveSignIn { get { return oneDriveSignIn; } }
        public static String OneDriveToken { get { return oneDriveToken; } }
        public static String OneDriveQuota { get { return oneDrive + oneDriveQuota; } }
        public static String OneDriveGetFolderId { get { return oneDrive + oneDriveGetFolderId; } }
        public static String OneDriveCreateFolder { get { return oneDrive + oneDriveCreateFolder; } }
        public static String OneDriveUpdate { get { return oneDrive + oneDriveUpdate; } }
        public static String OneDriveObject { get { return oneDrive + oneDriveObject; } }
        public static String OneDriveDownload { get { return oneDrive + oneDriveDownload; } }
        public static String OneDriveUpload { get { return oneDrive + oneDriveUpload; } }
        public static String OneDriveRegex { get { return oneDrive + oneDriveRegex; } }

        public static String Rackspace { get { return rackspace; } }

        static StorageUrl()
        {
            if (!loaded)
            {
                lock (sync)
                {
                    if (!loaded)
                    {
                        loaded = true;
                        Load();
                    }
                }
            }
        }

        private static void Load()
        {
            var fileFullPath = Path.Combine(ExecutorContext.BinDirectory, @"StorageUrl.config");
            try
            {
                if (File.Exists(fileFullPath))
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(fileFullPath);
                    var nodes = xmlDocument.GetElementsByTagName("URL");
                    logger.Info("There are {0} authentication url configuration node", nodes.Count);
                    foreach (XmlNode node in nodes)
                    {
                        var nodeName = node.Attributes["name"].Value;
                        var field = typeof(StorageUrl).GetField(nodeName, BindingFlags.NonPublic | BindingFlags.Static);
                        if (field != null)
                        {
                            var nodeValue = node.Attributes["value"].Value;
                            field.SetValue(null, nodeValue);
                            logger.Info("Configure succeed, name:{0}, value:{1}.", nodeName, nodeValue);
                        }
                        else
                            logger.Warn("Configure failed because the target which called {0} does not found.", nodeName);
                    }
                }
                else
                {
                    logger.Info("Storage authentication url configuration file does not exist, path:{0}", fileFullPath);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Load authentication url configuration file failed, it will use the default value, path:{0}, detail:{1}.", fileFullPath, e);
            }
        }
    }
}
