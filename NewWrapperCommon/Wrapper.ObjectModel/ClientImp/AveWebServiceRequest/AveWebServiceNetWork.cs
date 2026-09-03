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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web.Services.Protocols;
using System.Xml;
using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CodeReview;

namespace AvePoint.ObjectModel.WebService
{
    using DicType = Dictionary<string, object>;
    using AvePoint.ObjectModel.WebService.WebPartPages;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Resource.Client;
    using System.Diagnostics.CodeAnalysis;
    using AvePoint.ObjectModel.WebService.SiteData;
    using AveClientRequest.Common;
    using Office365.Api;

    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_11, CodeReviewConstants.CHECK_LIST_ID_CS_1 }, null, true)]
    public class AveWebServiceNetWork : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWebServiceNetWork));
        private const string mSiteDataUrlPostfix = "/_vti_bin/SiteData.asmx";
        private const string mSiteUrlPostfix = "/_vti_bin/Sites.asmx";
        private const string mWebUrlPostfix = "/_vti_bin/Webs.asmx";
        private const string mListUrlPostfix = "/_vti_bin/Lists.asmx";
        private const string mFileUrlPostfix = "/_vti_bin/Copy.asmx";
        private const string mVersionsUrlPostfix = "/_vti_bin/versions.asmx";
        private const string mUserGroupUrlPostfix = "/_vti_bin/UserGroup.asmx";
        private const string mWebPartPagesPostfix = "/_vti_bin/WebPartPages.asmx";
        private const string mUserProfileUrlPostfix = "/_vti_bin/UserProfileService.asmx";
        private const string mFormsServicesPostfix = "/_vti_bin/FormsServices.asmx";
        private const string mSocialDataServiceUrlPostfix = "/_vti_bin/socialdataservice.asmx";
        private const string mAdminServiceUrlPostfix = "/_vti_adm/admin.asmx";
        //
        private SoapHttpClientProtocol mNetWorker;
        private ReconnectableHttpWebRequest mHttpWebRequest;
        private object mResultFromAuthentic;
        private ITokenProvider mTokenProvider;
        //
        private AveWebServiceType mType;
        private string mMessage;
        private AveBPOSAccountInfo mUser;
        private string mSiteUrl;
        private string mServerUrl;
        private bool mExistNetWokerEntity;
        private bool mExistHttpWebRequestEntity;

        public AveWebServiceNetWork(AveBPOSAccountInfo user, string siteUrl, object obj)
        {
            mUser = user;
            mSiteUrl = siteUrl;
            mServerUrl = AveUrlUtility.GetServerUrl(siteUrl);
            mMessage = string.Empty;
            mType = AveWebServiceType.Invalied;
            mNetWorker = null;
            mHttpWebRequest = null;
            mExistNetWokerEntity = false;
            mExistHttpWebRequestEntity = false;
            mResultFromAuthentic = obj;
        }
        public AveWebServiceNetWork(AveBPOSAccountInfo user, string siteUrl, object obj, ITokenProvider tokenProvider)
        {
            mUser = user;
            mSiteUrl = siteUrl;
            mServerUrl = AveUrlUtility.GetServerUrl(siteUrl);
            mMessage = string.Empty;
            mType = AveWebServiceType.Invalied;
            mNetWorker = null;
            mHttpWebRequest = null;
            mExistNetWokerEntity = false;
            mExistHttpWebRequestEntity = false;
            mResultFromAuthentic = obj;
            mTokenProvider = tokenProvider;
        }

        public AveBPOSAccountInfo User
        {
            get
            {
                return this.mUser;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "socialdataservice is a kind of webservice")]
        public bool InitialNetWorker(AveWebServiceType type, string netWorkUrl)
        {
            try
            {
                mType = type;
                string urlPostfix = string.Empty;
                switch (type)
                {
                    case AveWebServiceType.Sites:
                        mNetWorker = new Sites.Sites();
                        urlPostfix = mSiteUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.SiteData:
                        mNetWorker = new SiteData.SiteData();
                        urlPostfix = mSiteDataUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.Lists:
                        mNetWorker = new Lists.Lists();
                        urlPostfix = mListUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.Webs:
                        mNetWorker = new Webs.Webs();
                        urlPostfix = mWebUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.Copy:
                        mNetWorker = new Copy.Copy();
                        urlPostfix = mFileUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.Versions:
                        mNetWorker = new Versions.Versions();
                        urlPostfix = mVersionsUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.UserGroup:
                        mNetWorker = new UserGroup.UserGroup();
                        urlPostfix = mUserGroupUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.HttpWebRequest:
                        mHttpWebRequest = ReconnectableHttpWebRequest.CreateRequest(new Uri(netWorkUrl));
                        //mHttpWebRequest.Timeout = WrapperConfiguration.BPOS_S.HttpWebRequestTimeout;
                        //mHttpWebRequest.ReadWriteTimeout = WrapperConfiguration.BPOS_S.HttpWebRequestReadWriteTimeout;
                        //this type is just used to get file stream at present.
                        mHttpWebRequest.Timeout = WrapperConfiguration.UpLoadFileStreamTimeout * 1000;//30 mins
                        mHttpWebRequest.ReadWriteTimeout = WrapperConfiguration.UpLoadFileStreamTimeout * 1000;
                        mExistHttpWebRequestEntity = true;
                        break;
                    case AveWebServiceType.WebPartPages:
                        mNetWorker = new WebPartPages.WebPartPagesWebService();
                        urlPostfix = mWebPartPagesPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.UserProfile:
                        mNetWorker = new UserProfileService.UserProfileService();
                        urlPostfix = mUserProfileUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.FormsServices:
                        mNetWorker = new FormsServices.FormsServicesWebService();
                        urlPostfix = mFormsServicesPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.SocialDataService:
                        mNetWorker = new SocialDataService.SocialDataService();
                        urlPostfix = mSocialDataServiceUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                    case AveWebServiceType.Admin:
                        mNetWorker = new Admin.Admin();
                        urlPostfix = mAdminServiceUrlPostfix;
                        mExistNetWokerEntity = true;
                        break;
                }
                //ICertificatePolicy oldPolicy = System.Net.ServicePointManager.CertificatePolicy;
                //System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

                //Credential
                SetHttpWebRequestsCredential();
                //Protocol Url
                if (mExistNetWokerEntity)
                {
                    mNetWorker.Timeout = WrapperConfiguration.BPOS_S.HttpWebRequestTimeout;
                    mNetWorker.Url = netWorkUrl.TrimEnd('/') + urlPostfix;
                }
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                mMessage = e.Message;
                this.Dispose();
                return false;
            }
            return true;
        }
        public void SetHttpWebRequestsCredential()
        {
            if (mResultFromAuthentic is CookieContainer)
            {
                if (mExistNetWokerEntity)
                {
                    mNetWorker.CookieContainer = mResultFromAuthentic as CookieContainer;
                }
                else if (mExistHttpWebRequestEntity)
                {
                    mHttpWebRequest.CookieContainer = mResultFromAuthentic as CookieContainer;
                }
            }
            else if (mResultFromAuthentic is NetworkCredential)
            {
                if (mExistNetWokerEntity)
                {
                    mNetWorker.Credentials = mResultFromAuthentic as NetworkCredential;
                }
                else if (mExistHttpWebRequestEntity)
                {
                    mHttpWebRequest.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
                    mHttpWebRequest.Credentials = mResultFromAuthentic as NetworkCredential;
                }
            }
            else if(mTokenProvider != null)
            {

                if (mExistNetWokerEntity)
                {
                    var protocol = mNetWorker as AveSoapHttpClientProtocol;
                    if (protocol != null)
                    {
                        protocol.TokenProvider = mTokenProvider;
                    }
                    else
                    {
                        throw new Exception("The protocol need implemente from AveSoapHttpClientProtocol.");
                    }
                }
                else if (mExistHttpWebRequestEntity)
                {
                    mHttpWebRequest.SetTokenProvider(mSiteUrl, mTokenProvider, false);
                }
            }
            else
            {
                throw new Exception("No available credentials.");
            }
        }

        #region  WebService methods
        public string SiteGetSite(string url)
        {
            return (mNetWorker as Sites.Sites).GetSite(url);
        }
        public void SiteGetSite2(out SiteData._sSiteMetadata sSiteMetadata, out SiteData._sWebWithTime[] vWebs, out string strUsers, out string strGroups, out string[] vGroups)
        {
            (mNetWorker as SiteData.SiteData).GetSite(out sSiteMetadata, out vWebs, out strUsers, out strGroups, out vGroups);
        }
        public uint GetWebLanguage()
        {
            _sWebMetadata sWebMetadata;
            _sWebWithTime[] vWebs;
            _sListWithTime[] vLists;
            _sFPUrl[] vFPUrls;
            string strRoles;
            string[] vRolesUsers;
            string[] vRolesGroups;
            (mNetWorker as SiteData.SiteData).GetWeb(out sWebMetadata, out vWebs, out vLists, out vFPUrls, out strRoles, out vRolesUsers, out vRolesGroups);
            return sWebMetadata.Language;
        }
        public void GetSiteTemplates(uint lcid, out Sites.Template[] templateList)
        {
            (mNetWorker as Sites.Sites).GetSiteTemplates(lcid, out templateList);
        }
        public XmlNode WebGetContentTypes()
        {
            return (mNetWorker as Webs.Webs).GetContentTypes();
        }
        public XmlNode WebGetContentType(string contentTypeId)
        {
            return (mNetWorker as Webs.Webs).GetContentType(contentTypeId);
        }
        public XmlNode WebGetWebProperties(string url)
        {
            return (mNetWorker as Webs.Webs).GetWeb(url);
        }
        public void SiteOpenWeb(out SiteData._sWebMetadata sWebMetadata,
            out SiteData._sWebWithTime[] vWebs,
            out SiteData._sListWithTime[] vLists,
            out SiteData._sFPUrl[] vFPUrls,
            out string strRoles,
            out string[] vRolesUsers,
            out string[] vRolesGroups)
        {
            (mNetWorker as SiteData.SiteData).GetWeb(out sWebMetadata,
                out vWebs,
                out vLists,
                out vFPUrls,
                out strRoles,
                out vRolesUsers,
                out vRolesGroups);
        }
        public XmlNode WebGetLists()
        {
            return (mNetWorker as Lists.Lists).GetListCollection();
        }
        public XmlNode WebGetWebCollection()
        {
            return (mNetWorker as Webs.Webs).GetWebCollection();
        }
        public void WebRevertAllDocumentContentStreams()
        {
            (mNetWorker as Webs.Webs).RevertAllFileContentStreams();
        }
        public void WebRevertContentStream(string fileUrl)
        {
            (mNetWorker as Webs.Webs).RevertFileContentStream(fileUrl);
        }
        public byte[] GetFileAllBytes(string fileFullUrl)
        {
            Copy.FieldInformation[] fileInfo;
            byte[] allBytes;
            (mNetWorker as Copy.Copy).GetItem(fileFullUrl, out fileInfo, out allBytes);
            return allBytes;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ja-JP:Language-Japanese.")]
        public Stream GetVersionDataStream()
        {
            if (mExistHttpWebRequestEntity == true)
            {
                WebHeaderCollection headerCollection = mHttpWebRequest.Headers;
                mHttpWebRequest.Accept = "*/*";
                mHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET4.0C; .NET4.0E)";
                headerCollection["Accept-Language"] = "en-US,ja-JP;q=0.5";
                return mHttpWebRequest.GetResponse().GetResponseStream();
            }
            else
            {
                throw new Exception("No contact exist.");
            }
        }
        public XmlNode FileGetFile(string serverRelativeUrl, string listName, string[] camlQueryNode)
        {
            XmlDocument queryDoc = new XmlDocument();
            XmlDocument viewDoc = new XmlDocument();
            XmlDocument queryOpertion = new XmlDocument();
            queryDoc.LoadXml(string.Format(
                "<Query><Where><Eq>" +
                "<FieldRef Name='FileRef'/>" +
                "<Value Type='Lookup'>{0}</Value>" +
                "</Eq></Where></Query>",
                serverRelativeUrl.TrimStart('/').TrimEnd('/')));
            viewDoc.LoadXml(camlQueryNode[0]);
            queryOpertion.LoadXml(camlQueryNode[2]);
            XmlNode informReturn = (mNetWorker as Lists.Lists).GetListItems(listName, null, queryDoc, viewDoc, null, queryOpertion, null);
            return informReturn;
        }
        public void FileDeleteVersion(string fileServerRelativeUrl, string version)
        {
            (mNetWorker as Versions.Versions).DeleteVersion(fileServerRelativeUrl, version);
        }
        public XmlNode ListGetItems(string listName, string[] camlQueryNode)
        {
            XmlDocument queryDoc = new XmlDocument();
            XmlDocument viewFieldsDoc = new XmlDocument();
            XmlDocument queryOpertion = new XmlDocument();
            queryDoc.LoadXml(camlQueryNode[1]);
            viewFieldsDoc.LoadXml(camlQueryNode[0]);
            queryOpertion.LoadXml(camlQueryNode[2]);
            XmlNode inforReturn = (mNetWorker as Lists.Lists).GetListItems(listName, null, queryDoc, viewFieldsDoc, null, queryOpertion, null);
            return inforReturn;
        }
        public XmlNode FolderGetItems(string listName, string parentFolderServerRelatvieUrl, string[] camlQueryNode)
        {
            XmlDocument queryDoc = new XmlDocument();
            XmlDocument viewDoc = new XmlDocument();
            XmlDocument queryOpertion = new XmlDocument();
            viewDoc.LoadXml(camlQueryNode[0]);
            queryDoc.LoadXml(string.Format(
                            "<Query><Where><Eq>" +
                            "<FieldRef Name='FileDirRef' />" +
                            "<Value Type='Text'>{0}</Value>" +
                            "</Eq></Where></Query>",
                            parentFolderServerRelatvieUrl.TrimEnd('/').TrimStart('/')));
            queryOpertion.LoadXml(camlQueryNode[2]);
            XmlNode inforReturn = (mNetWorker as Lists.Lists).GetListItems(listName, null, queryDoc, viewDoc, null, queryOpertion, null);
            return inforReturn;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        public XmlNode FolderListGetSubFoldersOrFiles(string listName, string parentFolderServerRelatvieUrl, AveFileSystemObjectType type, string[] camlQueryNode)
        {
            XmlDocument queryDoc = new XmlDocument();
            XmlDocument viewDoc = new XmlDocument();
            XmlDocument queryOpertion = new XmlDocument();
            viewDoc.LoadXml(camlQueryNode[0]);
            queryDoc.LoadXml(string.Format(
                            "<Query><Where><And>" +
                            "<Eq><FieldRef Name='FileDirRef' /><Value Type='Text'>{0}</Value></Eq>" +
                            "<Eq><FieldRef Name='FSObjType' /><Value Type='Text'>{1}</Value></Eq>" +
                            "</And></Where></Query>",
                            parentFolderServerRelatvieUrl.TrimEnd('/').TrimStart('/'), (int)type));
            queryOpertion.LoadXml(camlQueryNode[2]);
            XmlNode inforReturn = (mNetWorker as Lists.Lists).GetListItems(listName, null, queryDoc, viewDoc, null, queryOpertion, null);
            return inforReturn;
        }
        public void FolderEnumItems(string folderUrl, out SiteData._sFPUrl[] urls)
        {
            (mNetWorker as SiteData.SiteData).EnumerateFolder(folderUrl, out urls);
        }
        public XmlNode ListGetListContentTypes(string listName)
        {
            return (mNetWorker as Lists.Lists).GetListContentTypes(listName, string.Empty);
        }
        public XmlNode ListOrListItemGetListContentType(string listName, string contentTypeId)
        {
            return (mNetWorker as Lists.Lists).GetListContentType(listName, contentTypeId);
        }
        public XmlNode ListGetListAttribute(string listName)
        {
            return (mNetWorker as Lists.Lists).GetList(listName);
        }
        public string ListAddAttachment(string listName, string itemId, string fileName, byte[] attachment)
        {
            return (mNetWorker as Lists.Lists).AddAttachment(listName, itemId, fileName, attachment);
        }
        public XmlNode ListAddList(string listName, string description, int templateID)
        {
            return (mNetWorker as Lists.Lists).AddList(listName, description, templateID);
        }

        public XmlNode ListAddListFromFeature(string listName, string description, Guid featureID, int templateID)
        {
            return (mNetWorker as Lists.Lists).AddListFromFeature(listName, description, featureID, templateID);
        }

        public XmlNode ListGetAttachmentCollection(string listTitle, int itemId)
        {
            return (mNetWorker as Lists.Lists).GetAttachmentCollection(listTitle, itemId.ToString());
        }
        public void ListDeleteAttachment(string listName, string listItemID, string url)
        {
            (mNetWorker as Lists.Lists).DeleteAttachment(listName, listItemID, url);
        }
        public XmlNode ListGetList(string listTile)
        {
            return (mNetWorker as Lists.Lists).GetList(listTile);
        }
        public XmlNode ListGetVersionCollection(string strlistID, string strlistItemID, string strFieldName)
        {
            return (mNetWorker as Lists.Lists).GetVersionCollection(strlistID, strlistItemID, strFieldName);
        }
        public void ListGetVersionCollectionAsync(string strlistID, string strlistItemID, string strFieldName, object userState)
        {
            (mNetWorker as Lists.Lists).GetVersionCollectionAsync(strlistID, strlistItemID, strFieldName, userState);
        }

        public void ListGetVersionCollectionCompletedRegister(Lists.GetVersionCollectionCompletedEventHandler getVersionCollection_Success)
        {
            (mNetWorker as Lists.Lists).GetVersionCollectionCompleted += getVersionCollection_Success;
        }

        public XmlNode FileGetVersions(string fileSiteRelativeUrl)
        {
            return (mNetWorker as Versions.Versions).GetVersions(fileSiteRelativeUrl);
        }
        public XmlNode UserGroupGetUserInfo(string loginName)
        {
            return (mNetWorker as UserGroup.UserGroup).GetUserInfo(loginName);
        }
        public XmlNode UserGroupGetUserCollectionFromWeb()
        {
            return (mNetWorker as UserGroup.UserGroup).GetUserCollectionFromWeb();
        }
        public XmlNode UserGroupGetAllUserCollectionFromWeb()
        {
            return (mNetWorker as UserGroup.UserGroup).GetAllUserCollectionFromWeb();
        }
        public XmlNode UserGroupGetUserCollectionFromSite()
        {
            return (mNetWorker as UserGroup.UserGroup).GetUserCollectionFromSite();
        }
        public XmlNode UserGroupGetUserCollectionFromGroup(string groupName)
        {
            return (mNetWorker as UserGroup.UserGroup).GetUserCollectionFromGroup(groupName);
        }
        public void UserGroupAddGroup(string groupName, string ownerName, string ownerType, string defaultUserName, string description)
        {
            (mNetWorker as UserGroup.UserGroup).AddGroup(groupName, ownerName, ownerType, defaultUserName, description);
        }
        public XmlNode UserGroupGetGroupInfo(string groupName)
        {
            return (mNetWorker as UserGroup.UserGroup).GetGroupInfo(groupName);
        }
        public XmlNode UserGroupGetGroupCollectionFromWeb()
        {
            return (mNetWorker as UserGroup.UserGroup).GetGroupCollectionFromWeb();
        }
        public XmlNode UserGroupGetGroupCollectionFromUser(string loginName)
        {
            return (mNetWorker as UserGroup.UserGroup).GetGroupCollectionFromUser(loginName);
        }
        public XmlNode FileRestoreVersion(string fileServerRelativeUrl, string version)
        {
            return (mNetWorker as Versions.Versions).RestoreVersion(fileServerRelativeUrl, version);
        }
        public UserProfileService.PropertyData[] UserProfileGetUserProfile(string accountName)
        {
            return (mNetWorker as UserProfileService.UserProfileService).GetUserProfileByName(accountName);
        }
        public UserProfileService.PropertyData[] UserProfileCreateUserProfile(string loginName)
        {
            return (mNetWorker as UserProfileService.UserProfileService).CreateUserProfileByAccountName(loginName);
        }
        public void UserProfileModifyUserPropertyByAccountName(string loginName, UserProfileService.PropertyData[] changeProperties)
        {
            (mNetWorker as UserProfileService.UserProfileService).ModifyUserPropertyByAccountName(loginName, changeProperties);
        }

        /// <summary>
        /// Get User Profile Propertys.
        /// User Profile Propertys is similar to list fields.
        /// </summary>
        public UserProfileService.PropertyInfo[] UserProfileGetUserProfileSchema()
        {
            return (mNetWorker as UserProfileService.UserProfileService).GetUserProfileSchema();
        }

        public UserProfileService.QuickLinkData[] UserProfileGetUserLinks(string accountName)
        {
            return (mNetWorker as UserProfileService.UserProfileService).GetUserLinks(accountName);
        }
        public UserProfileService.QuickLinkData UserProfileAddLink(string accountName, string name, string url, string group, int policyLevel)
        {
            return (mNetWorker as UserProfileService.UserProfileService).AddLink(accountName, name, url, group, (UserProfileService.Privacy)policyLevel);
        }
        public void UserProfileUpdateLink(string accountName, UserProfileService.QuickLinkData data)
        {
            (mNetWorker as UserProfileService.UserProfileService).UpdateLink(accountName, data);
        }
        public void UserProfileRemoveLink(string accountName, int id)
        {
            (mNetWorker as UserProfileService.UserProfileService).RemoveLink(accountName, id);
        }

        public UserProfileService.MembershipData[] UserProfileGetUserMemberShips(string accountName)
        {
            return (mNetWorker as UserProfileService.UserProfileService).GetUserMemberships(accountName);
        }
        public UserProfileService.MembershipData UserProfileAddMemberShip(string accountName, UserProfileService.MembershipData membershipInfo, string group, int privacyLevel)
        {
            return (mNetWorker as UserProfileService.UserProfileService).AddMembership(accountName, membershipInfo, group, (UserProfileService.Privacy)privacyLevel);
        }
        public void UserProfileUpdateMembershipPrivacy(string accountName, Guid sourceInternal, string sourceReference, int privacyLevel)
        {
            (mNetWorker as UserProfileService.UserProfileService).UpdateMembershipPrivacy(accountName, sourceInternal, sourceReference, (UserProfileService.Privacy)privacyLevel);
        }
        public void UserProfileCreateMemberGroup(UserProfileService.MembershipData memberShipData)
        {
            (mNetWorker as UserProfileService.UserProfileService).CreateMemberGroup(memberShipData);
        }

        public UserProfileService.ContactData[] UserProfileGetUserColleagues(string accountName)
        {
            return (mNetWorker as UserProfileService.UserProfileService).GetUserColleagues(accountName);
        }
        public UserProfileService.ContactData UserProfileAddColleague(string accountName, string colleagueAccountName, string group, int privacyLevel, bool isInWorkGroup)
        {
            return (mNetWorker as UserProfileService.UserProfileService).AddColleague(accountName, colleagueAccountName, group, (UserProfileService.Privacy)privacyLevel, isInWorkGroup);
        }
        public void UserProfileUpdateColleaguePrivacy(string accountName, string colleagueAccountName, int privacyLevel)
        {
            (mNetWorker as UserProfileService.UserProfileService).UpdateColleaguePrivacy(accountName, colleagueAccountName, (UserProfileService.Privacy)privacyLevel);
        }
        public void UserProfileRemoveColleague(string accountName, string colleagueAccountName)
        {
            (mNetWorker as UserProfileService.UserProfileService).RemoveColleague(accountName, colleagueAccountName);
        }

        public string WebPagePagesGetOrignalHtmlOnPage(string documentUrl)
        {
            return (mNetWorker as WebPartPages.WebPartPagesWebService).GetWebPartPage(documentUrl, SPWebServiceBehavior.Version3);
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "wpz is a value of dictionary")]
        public string WebPagePagesGetWebPartOnPage(string pageUrl)
        {
            //"ID", "__WebPartId", "ExportMode", "IsIncluded", "IsClosed", "AssemblyFullName", "TypeFullName", "SolutionId", "FormLocation", "ContentTypeId"
            // "ZoneID", "PartOrder", "IsIncluded", "WebPartIdProperty"
            XmlNode definationXmls = GetWebPartProperties(pageUrl);
            string htmlPage = null;
            bool initHtmlPage = false;
            string editModeHtmlPage = null;
            bool initEditMode = false;
            if (definationXmls != null && definationXmls.ChildNodes.Count > 0)
            {
                XmlElement resultXml = definationXmls.OwnerDocument.CreateElement(definationXmls.Name, definationXmls.NamespaceURI);
                foreach (XmlNode child in definationXmls.ChildNodes)
                {
                    XmlElement definationXml = child as XmlElement;
                    if (definationXml == null)
                    {
                        mLogger.Debug("a child node of definition XMLs is not an element,outerXml:{0},PageUrl {1}", child.OuterXml, pageUrl);
                        continue;
                    }

                    try
                    {
                        //这个Dictionary用于缓存需要额外在HtmlPage中或者EditModeContent中获取的WebPart属性。在下面的逻辑中如果找到属性对应的节点，并且属性值不是Null或者Empty时，
                        //才会将属性的值赋给Value。
                        var propertiesNeedToBackup = new Dictionary<string, string>() 
                        { 
                           { "ID", null }, { "ExportMode", null }, { "IsClosed", null }, { "IsIncluded", null }, { "FormLocation", null }, 
                           { "ContentTypeId", null }, { "ZoneID", null }, { "PartOrder", null } 
                        };
                        //,{ "SolutionId", null }
                        //像SolutionId一类的属性，只有特殊Webpart才会有
                        AddSpecialWebPartPropertyToDictionary(propertiesNeedToBackup, definationXml);
                        LookupPropertiesFromDefinationXml(propertiesNeedToBackup, definationXml);

                        var propertiesNotFoundInXml = propertiesNeedToBackup.Where(entry => string.IsNullOrEmpty(entry.Value)).ToDictionary(pair => pair.Key, pair => pair.Value);
                        if (propertiesNotFoundInXml.Count > 0)
                        {
                            if (!initHtmlPage)
                            {
                                htmlPage = InitHtmlPage(pageUrl, definationXml);
                                initHtmlPage = true;
                            }

                            if (!string.IsNullOrEmpty(htmlPage))
                            {
                                HtmlNode node = GetWebPartHtmlNode(htmlPage, definationXml);
                                if (node != null)
                                {
                                    LookupPropertiesFromHtml(propertiesNotFoundInXml, node);
                                }
                            }

                            if (propertiesNotFoundInXml.ContainsKey("ZoneID") && string.IsNullOrEmpty(propertiesNotFoundInXml["ZoneID"]))
                            {
                                if (!initEditMode)
                                {
                                    editModeHtmlPage = GetPageConentInEditMode(pageUrl);
                                }
                                if (!string.IsNullOrEmpty(editModeHtmlPage))
                                {
                                    string webPartId = definationXml.Attributes["ID"].Value.Trim('{', '}').ToLowerInvariant();
                                    string value = GetZoneIdFromEditMode(editModeHtmlPage, webPartId);
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        propertiesNotFoundInXml["ZoneID"] = value;
                                    }
                                }
                            }

                            foreach (var kv in propertiesNotFoundInXml)
                            {
                                if (!string.IsNullOrEmpty(kv.Value))
                                {
                                    propertiesNeedToBackup[kv.Key] = kv.Value;
                                }
                            }
                        }
                        FormatProperties(propertiesNeedToBackup, definationXml);
                        TransferToResultXml(resultXml, definationXml, propertiesNeedToBackup, propertiesNotFoundInXml);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("An error occurred while backing up a web part base info by client API XML:{0},PageUrl:{1},Error{2}", definationXmls.OuterXml, pageUrl, ex);
                    }
                }
                return resultXml.OuterXml.ToString().Replace(definationXmls.NamespaceURI, string.Empty).Replace("xmlns=\"\"", string.Empty);
            }
            return null;
        }

        /// <summary>
        /// 添加特殊WebPart属性
        /// </summary>
        /// <param name="propertiesNeedToBackup"></param>
        /// <param name="definationXml"></param>
        private void AddSpecialWebPartPropertyToDictionary(Dictionary<string, string> propertiesNeedToBackup, XmlElement definationXml)
        {
            if (!definationXml.NamespaceURI.Equals("http://schemas.microsoft.com/WebPart/v2", StringComparison.OrdinalIgnoreCase))
            {
                string typeName = GetV3AssemblyInfo(definationXml);
                if (!string.IsNullOrEmpty(typeName) && typeName.StartsWith("Microsoft.SharePoint.WebPartPages.SPUserCodeWebPart", StringComparison.OrdinalIgnoreCase))
                {
                    propertiesNeedToBackup.Add("SolutionId", null);
                    propertiesNeedToBackup.Add("AssemblyFullName", null);
                    propertiesNeedToBackup.Add("TypeFullName", null);
                }
            }
        }

        /// <summary>
        /// 获取V3WebPart的type属性
        /// </summary>
        /// <param name="definationXml"></param>
        /// <returns></returns>
        private string GetV3AssemblyInfo(XmlElement definationXml)
        {
            try
            {
                var v3NameSpaceManager = new XmlNamespaceManager(definationXml.OwnerDocument.NameTable);
                v3NameSpaceManager.AddNamespace("d", "http://schemas.microsoft.com/WebPart/v3");
                var typeNameNode = definationXml.SelectSingleNode("//d:metaData/d:type/@name", v3NameSpaceManager);
                if (typeNameNode != null && !string.IsNullOrEmpty(typeNameNode.Value))
                {
                    return typeNameNode.Value;
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("An error occurred while getting assembly info of a web part,definition Xml:{0},error:{1} ", definationXml.OuterXml, ex);
            }
            return null;
        }

        private void AttachBaseInfoToXml(Dictionary<string, string> propertiesNeedToBackup, XmlElement definationXml)
        {
            List<string> needProperties = new List<string> { "ZoneID", "PartOrder", "IsIncluded", "WebPartIdProperty" };
            foreach (string property in needProperties)
            {
                if (!propertiesNeedToBackup.ContainsKey(property) || string.IsNullOrEmpty(propertiesNeedToBackup[property]))
                {
                    mLogger.Log(AveLogLevel.WARN, "Invalid value of WebPart property. Property Name:{0}", property);
                    continue;
                }

                XmlElement tempElement = definationXml.OwnerDocument.CreateElement(property);
                tempElement.InnerText = propertiesNeedToBackup[property];
                definationXml.AppendChild(tempElement);
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "wpz"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "'wpz' is a webpart zone")]
        private void FormatProperties(Dictionary<string, string> propertiesNeedToBackup, XmlElement definationXml)
        {
            if (string.IsNullOrEmpty(propertiesNeedToBackup["IsIncluded"]) && !string.IsNullOrEmpty(propertiesNeedToBackup["IsClosed"]))
            {
                propertiesNeedToBackup["IsIncluded"] = propertiesNeedToBackup["IsClosed"].Equals(Boolean.FalseString, StringComparison.OrdinalIgnoreCase).ToString().ToLowerInvariant();
            }

            if (string.IsNullOrEmpty(propertiesNeedToBackup["ZoneID"]))
            {
                propertiesNeedToBackup["ZoneID"] = "wpz";
            }

            if (string.IsNullOrEmpty(propertiesNeedToBackup["PartOrder"]))
            {
                propertiesNeedToBackup["PartOrder"] = "0";
            }

            string strWebpart = null;
            if (!string.IsNullOrEmpty(propertiesNeedToBackup["ID"]))
            {
                if (propertiesNeedToBackup["ID"].StartsWith("g_", StringComparison.OrdinalIgnoreCase))
                {
                    strWebpart = propertiesNeedToBackup["ID"].Substring(2).Replace("_", "-");
                }
                else
                {
                    strWebpart = propertiesNeedToBackup["ID"].Trim(new char[] { '{', '}' });
                }
            }

            if (AveTypeHelper.IsGuid(strWebpart))
            {
                propertiesNeedToBackup["WebPartIdProperty"] = "g_" + strWebpart.ToLowerInvariant().Replace("-", "_");
            }
            else
            {
                XmlNode Id = definationXml.SelectSingleNode(".//*[name()='ID']");
                if (Id == null || !AveTypeHelper.IsGuid(Id.InnerText.TrimStart('g').Replace("_", "-")))
                {
                    Id = FindPropertyNodeByAttribute(definationXml, "ID");
                }
                if (Id != null)
                {
                    propertiesNeedToBackup["WebPartIdProperty"] = Id.InnerText.Trim(new char[] { '{', '}' }).ToLowerInvariant();
                }
                else
                {
                    propertiesNeedToBackup["WebPartIdProperty"] = string.Empty;
                }
            }
        }

        private void AddPropertiesToDefinationXml(Dictionary<string, string> propertiesNotFoundInXml, XmlElement definationXml, Dictionary<string, string> propertiesAll)
        {
            propertiesNotFoundInXml.Add("WebPartIdProperty", propertiesAll["WebPartIdProperty"]);
            foreach (var entry in propertiesNotFoundInXml)
            {
                string value = null;
                if (!string.IsNullOrEmpty(entry.Value))
                {
                    value = entry.Value;
                }
                else if (!string.IsNullOrEmpty(propertiesAll[entry.Key]))
                {
                    value = propertiesAll[entry.Key];
                }
                else
                {
                    continue;
                }

                var node = FindPropertyNodeByAttribute(definationXml, entry.Key);
                if (node != null)
                {
                    node.InnerText = value;
                }
                else
                {
                    XmlNode tempNode = definationXml.SelectSingleNode(".//*[name() = 'properties']");
                    if (tempNode != null)
                    {
                        XmlElement propertyNode = tempNode.OwnerDocument.CreateElement("property", tempNode.NamespaceURI) as XmlElement;
                        propertyNode.SetAttribute("name", entry.Key);
                        propertyNode.InnerText = value;
                        tempNode.AppendChild(propertyNode);
                    }
                    else
                    {
                        XmlElement propertyNode = definationXml.OwnerDocument.CreateElement(entry.Key, definationXml.NamespaceURI);
                        propertyNode.InnerText = value;
                        definationXml.AppendChild(propertyNode);
                    }
                }
            }
        }


        /// <summary>
        /// 从html中获取WebPart属性
        /// </summary>
        /// <param name="htmlNode"></param>
        /// <returns></returns>
        private void LookupPropertiesFromHtml(Dictionary<string, string> propertiesNotFound, HtmlNode htmlNode)
        {
            for (int i = 0; i < propertiesNotFound.Count; i++)
            {
                var keyValuePair = propertiesNotFound.ElementAt(i);
                switch (keyValuePair.Key)
                {
                    case "ZoneID":
                        string zoneId = GetZoneIdFromHtml(htmlNode);
                        if (!string.IsNullOrEmpty(zoneId))
                        {
                            propertiesNotFound[keyValuePair.Key] = zoneId;
                        }
                        break;
                    case "PartOrder":
                        string partOrder = GetPartOrderFromHtml(htmlNode);
                        if (!string.IsNullOrEmpty(partOrder))
                        {
                            propertiesNotFound[keyValuePair.Key] = partOrder;
                        }
                        break;
                    case "IsClosed":
                        var close = htmlNode.GetAttributeValue("__designer:IsClosed", string.Empty);
                        if (!string.IsNullOrEmpty(close))
                        {
                            propertiesNotFound[keyValuePair.Key] = close;
                        }
                        else
                        {
                            if ((close = htmlNode.GetAttributeValue(keyValuePair.Key, string.Empty)) != string.Empty)
                            {
                                propertiesNotFound[keyValuePair.Key] = close;
                            }
                        }
                        break;
                    default:
                        if (htmlNode.GetAttributeValue(keyValuePair.Key, string.Empty) != string.Empty)
                        {
                            propertiesNotFound[keyValuePair.Key] = htmlNode.GetAttributeValue(keyValuePair.Key, string.Empty);
                        }
                        break;
                }
            }
        }

        private string InitHtmlPage(string pageUrl, XmlElement definationXml)
        {
            string content = null;
            try
            {
                content = (mNetWorker as WebPartPages.WebPartPagesWebService).GetWebPartPage(pageUrl, SPWebServiceBehavior.Version3);
            }
            catch (Exception ex)
            {
                if ((ex is SoapException) && (ex.Message.Contains("0x80070005") || ex.Message.Contains("Access is denied")))
                {
                    throw new AveSecurityTrimingException("The request failed with HTTP status 401: Unauthorized.", ex);
                }
                mLogger.Warn("Get web part page html failed.Url:{0}.Error Message:{1}.XML:{2}", pageUrl, ex.ToString(), definationXml.OuterXml);
            }
            return content;
        }

        /// <summary>
        /// Xml中存在节点并且值不为Null或者Empty ，才会初始化Dictionary
        /// </summary>
        /// <param name="propertiesNeedToBackup"></param>
        /// <param name="definationXml"></param>
        private void LookupPropertiesFromDefinationXml(Dictionary<string, string> propertiesNeedToBackup, XmlElement definationXml)
        {
            for (int i = 0; i < propertiesNeedToBackup.Count; i++)
            {
                var keyValuePair = propertiesNeedToBackup.ElementAt(i);
                string value = GetValueByPropertyName(definationXml, keyValuePair.Key);
                if (!string.IsNullOrEmpty(value))
                {
                    propertiesNeedToBackup[keyValuePair.Key] = value;
                }
            }
        }

        /// <summary>
        /// 将DefinationXml转化为ResultXml
        /// </summary>
        /// <param name="resultXml"></param>
        /// <param name="definationXml"></param>
        /// <param name="propertiesAll"></param>
        /// <param name="propertiesNotFoundInXml"></param>
        private void TransferToResultXml(XmlElement resultXml, XmlElement definationXml, Dictionary<string, string> propertiesAll, Dictionary<string, string> propertiesNotFoundInXml)
        {
            try
            {
                XmlElement newWebPartElement = null;
                //前面的逻辑，如果是SandBoxSolution才会向Dictionary中添加SolutionId entry
                if (propertiesAll.ContainsKey("SolutionId"))
                {
                    newWebPartElement = GetSandBoxWebPart(definationXml, propertiesAll);
                }
                else
                {
                    //此处为了还原时格式的正确，v3的节点必须要有webParts节点，v2则不需要
                    if (definationXml.NamespaceURI.Equals("http://schemas.microsoft.com/WebPart/v2"))
                    {
                        newWebPartElement = definationXml.OwnerDocument.CreateElement(definationXml.Name, definationXml.NamespaceURI);
                    }
                    else
                    {
                        newWebPartElement = definationXml.OwnerDocument.CreateElement("webParts", definationXml.NamespaceURI);
                    }
                    newWebPartElement.SetAttribute("ID", definationXml.Attributes["ID"].Value);
                    //innerXML会有namespace，故改用append，规范xmlnamespace，此处的namespace不可用string.replace，可能会有namespace前半段和该被替换的namespace重合的情况出现，造成还原抛异常
                    for (int i = 0; i < definationXml.ChildNodes.Count; i++)
                    {
                        XmlNode tempNode = definationXml.ChildNodes[i].CloneNode(true);
                        newWebPartElement.AppendChild(tempNode);
                    }
                }
                AddPropertiesToDefinationXml(propertiesNotFoundInXml, newWebPartElement, propertiesAll);
                if (!definationXml.NamespaceURI.Equals("http://schemas.microsoft.com/WebPart/v2"))
                {
                    AttachBaseInfoToXml(propertiesAll, newWebPartElement);
                }
                resultXml.AppendChild(newWebPartElement);
            }
            catch (Exception ex)
            {
                mLogger.Debug("An error occurred while convert definition Xml to result xml,web part Xml:{0},error:{1}", definationXml.OuterXml, ex);
            }
        }

        private string GetZoneIdFromEditMode(string pageContentInEditMode, string webPartId)
        {

            if (!string.IsNullOrEmpty(pageContentInEditMode))
            {
                int webpartIdIndex = pageContentInEditMode.IndexOf(string.Format("WebPartID=\"{0}\"", webPartId), StringComparison.OrdinalIgnoreCase);
                if (webpartIdIndex > 0)
                {
                    int zoneIdStartIndex = pageContentInEditMode.LastIndexOf("zoneID=\"", webpartIdIndex, StringComparison.OrdinalIgnoreCase);
                    if (zoneIdStartIndex > 0)
                    {
                        int zoneIdStrLen = "zoneID=\"".Length;
                        return pageContentInEditMode.Substring(zoneIdStartIndex + zoneIdStrLen, pageContentInEditMode.IndexOf("\"", zoneIdStartIndex + zoneIdStrLen, StringComparison.OrdinalIgnoreCase) - zoneIdStartIndex - zoneIdStrLen);
                    }
                }
            }
            return null;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "abcdef"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "webpartid"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "abcdefg"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "XML Attribute")]
        private HtmlNode GetWebPartHtmlNode(string metaDataHtml, XmlNode webPartDefinationXml)
        {
            string id = webPartDefinationXml.Attributes["ID"].Value;
            Guid webPartGuid = new Guid(id);
            string formattedID = webPartGuid.ToString("B").ToUpperInvariant();
            HtmlDocument doc = new HtmlDocument();
            doc.OptionOutputOriginalCase = true;
            doc.LoadHtml(metaDataHtml);
            return doc.DocumentNode.SelectSingleNode("//node()[translate(@__webpartid,'abcdef','ABCDEF')='" + formattedID + "']");
        }

        private HtmlDocument InitDesignWebPartDoc(HtmlNode webPartHtmlNode)
        {
            string designValue = webPartHtmlNode.GetAttributeValue("__designer:Values", string.Empty);
            HtmlDocument designDoc = null;
            if (!string.IsNullOrEmpty(designValue))
            {
                designDoc = new HtmlDocument();
                designDoc.LoadHtml(System.Web.HttpUtility.HtmlDecode(designValue));
            }
            return designDoc;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "zoneid"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "zoneid Node Name")]
        private string GetZoneIdFromHtml(HtmlNode htmlNode)
        {
            string zoneId = htmlNode.GetAttributeValue("ZoneID", string.Empty);
            if (string.IsNullOrEmpty(zoneId))
            {
                var designDoc = InitDesignWebPartDoc(htmlNode);
                if (designDoc != null)
                {
                    HtmlNode zoneIdNode = designDoc.DocumentNode.SelectSingleNode("p[@n='ZoneID']");
                    if (zoneIdNode != null)
                    {
                        zoneId = zoneIdNode.GetAttributeValue("T", string.Empty);
                    }
                }
                if (string.IsNullOrEmpty(zoneId))
                {
                    //SAAS-6479 zoneid is a node, not a attribute of a node
                    var zoneIdNode = htmlNode.SelectSingleNode(".//zoneid");
                    if (zoneIdNode != null)
                    {
                        zoneId = zoneIdNode.InnerText;
                    }
                }
                if (string.IsNullOrEmpty(zoneId))
                {
                    zoneId = GetZoneIdFromParentNode(htmlNode);
                }
            }
            return zoneId;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "zonetemplate"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "webpartpages"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "webpartzone")]
        private string GetZoneIdFromParentNode(HtmlNode htmlNode)
        {
            string zoneid=null;
            //ADO-165143 MediaWebpart上没有zoneid，通过其webpartzone节点找其zoneid
            if (htmlNode.ParentNode != null && htmlNode.ParentNode.Name.Equals("zonetemplate", StringComparison.OrdinalIgnoreCase))
            {
                if (htmlNode.ParentNode.ParentNode != null && htmlNode.ParentNode.ParentNode.Name.Equals("webpartpages:webpartzone", StringComparison.OrdinalIgnoreCase))
                {
                    zoneid = htmlNode.ParentNode.ParentNode.GetAttributeValue("id", string.Empty);
                }
            }
            return zoneid;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "partorder"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "partorder node name")]
        private string GetPartOrderFromHtml(HtmlNode htmlNode)
        {
            string partOrder = htmlNode.GetAttributeValue("PartOrder", string.Empty);
            if (string.IsNullOrEmpty(partOrder))
            {
                HtmlNode partOrderNode = null;
                var designDoc = InitDesignWebPartDoc(htmlNode);
                if (designDoc != null)
                {
                    partOrderNode = designDoc.DocumentNode.SelectSingleNode("p[@n='PartOrder']");
                    if (partOrderNode == null)
                    {
                        partOrderNode = designDoc.DocumentNode.SelectSingleNode("p[@n='ZoneIndex']");
                    }
                }
                if (partOrderNode == null)
                {
                    partOrderNode = htmlNode.SelectSingleNode(".//partorder");
                    if (partOrderNode != null && !string.IsNullOrEmpty(partOrderNode.InnerText))
                    {
                        partOrder = partOrderNode.InnerText;
                    }
                    else
                    {
                        partOrder = null;
                    }
                }
                else
                {
                    partOrder = partOrderNode.GetAttributeValue("T", "-1");
                    if (partOrder.Equals("-1", StringComparison.OrdinalIgnoreCase))
                    {
                        partOrder = null;
                    }
                }
            }
            return partOrder;
        }

        private string GetValueByPropertyName(XmlNode definationXml, string property)
        {
            string value = null;
            var propertyNode = FindPropertyNodeByAttribute(definationXml, property);
            if (propertyNode != null)
            {
                value = propertyNode.InnerText;
            }
            return value;
        }

        private XmlNode FindPropertyNodeByAttribute(XmlNode definationXml, string property)
        {
            XmlNode node = null;
            XmlNode properties = definationXml.SelectSingleNode(".//*[name() = 'properties']");
            if (properties != null)
            {
                foreach (XmlNode propertyNode in properties.ChildNodes)
                {
                    if (propertyNode.Attributes == null || propertyNode.Attributes["name"] == null)
                    { continue; }
                    if (property.Equals(propertyNode.Attributes["name"].Value))
                    {
                        node = propertyNode;
                        break;
                    }
                }
            }
            else
            {
                XmlNode propertyNode = definationXml.SelectSingleNode(".//*[name() = '" + property + "']");
                if (propertyNode != null)
                {
                    node = propertyNode;
                }
            }
            return node;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "webpartpages"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "webpartpages is a XML namespace")]
        private XmlElement GetSandBoxWebPart(XmlElement definationXml, Dictionary<string, string> propertiesAll)
        {
            XmlElement elment = definationXml.OwnerDocument.CreateElement("webParts");

            if (definationXml.Attributes["ID"] != null)
            {
                elment.SetAttribute("ID", definationXml.Attributes["ID"].Value);
            }


            string assemblyInfo = null;
            if (!string.IsNullOrEmpty(propertiesAll["TypeFullName"]) && !string.IsNullOrEmpty(propertiesAll["AssemblyFullName"]))
            {
                assemblyInfo = propertiesAll["TypeFullName"] + "," + propertiesAll["AssemblyFullName"];
            }
            else
            {
                assemblyInfo = GetV3AssemblyInfo(definationXml);
            }

            if (string.IsNullOrEmpty(assemblyInfo))
            {
                mLogger.Debug("Can't get assembly info of web part ,definition Xml:{0}", definationXml.OuterXml);
            }

            string sandboxWebpart = "<webPart xmlns=\"http://schemas.microsoft.com/WebPart/v3\"><metaData><type name=\"{0}\" /><importErrorMessage>$Resources:core,ImportErrorMessage;</importErrorMessage><Solution SolutionId=\"{1}\" xmlns=\"http://schemas.microsoft.com/sharepoint/\" /></metaData><data><properties></properties></data></webPart>";
            elment.InnerXml = string.Format(sandboxWebpart, assemblyInfo, propertiesAll["SolutionId"]);

            XmlElement originalPropertiesNode = definationXml.SelectSingleNode(".//*[name() = 'properties']") as XmlElement;
            if (originalPropertiesNode != null)
            {
                XmlElement propertiesNode = elment.SelectSingleNode(".//*[name() = 'properties']") as XmlElement;
                propertiesNode.InnerXml = originalPropertiesNode.InnerXml;
            }
            return elment;
        }

        private string GetPageConentInEditMode(string pageUrl)
        {
            string htmlPage = AveHttpWebRequestUtility.HttpGet(pageUrl, mResultFromAuthentic);
            if (!string.IsNullOrEmpty(htmlPage))
            {
                Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(htmlPage);
                formValues["__EVENTARGUMENT"] = "edit";
                formValues["__EVENTTARGET"] = "ctl05";
                formValues["MSOLayout_InDesignMode"] = 1;
                formValues["MSOSPWebPartManager_DisplayModeName"] = "Design";
                formValues.Remove(string.Empty);
                //formValues["MSOAuthoringConsole_FormContext"] = 1;
                byte[] body = AveHttpWebRequestUtility.GetByte(formValues, string.Empty);
                return AveHttpWebRequestUtility.HttpReturn(pageUrl, mResultFromAuthentic, "application/x-www-form-urlencoded", body, null);
            }
            return null;
        }

        public Guid AddWebPartToZone(string pageUrl, string webPartXml, string zoneId, int zoneIndex)
        {
            try
            {
                return (mNetWorker as WebPartPages.WebPartPagesWebService).AddWebPartToZone(pageUrl, webPartXml, Storage.Shared, zoneId, zoneIndex);
            }
            catch (Exception ex)
            {
                try
                {
                    mLogger.Warn("Add WebPart to zone:{0} error,Error Message:{1}.", zoneId, ex.ToString());
                    return (mNetWorker as WebPartPages.WebPartPagesWebService).AddWebPart(pageUrl, webPartXml, Storage.Shared);
                }
                catch (SoapException exception)
                {
                    string soapError = exception.ToString();
                    throw new Exception("Soap exception: " + soapError, exception);
                }
                catch (Exception exception2)
                {
                    throw new Exception("Could not add web part to page: (" + exception2.Message + ").", exception2);
                }
            }
        }


        //public void AddWebPart(string sWebPartPageUrl, string webPartStr, string zoneId, int zoneIndex)
        //{
        //    XmlNode webPartNode = StringToXmlNode(webPartStr);
        //    if (((webPartNode == null) || string.IsNullOrEmpty(sWebPartPageUrl)) || ((mNetWorker as WebPartPages.WebPartPagesWebService) == null))
        //    {
        //        throw new ArgumentException("One of the necessary parameters passed for adding a web part is null.");
        //    }
        //    string webPartXml = "<webParts>" + webPartNode.InnerXml + "</webParts>";
        //    string pageUrl = this.JoinUrl(mServerUrl, sWebPartPageUrl);
        //    AddWebPartToZone(pageUrl, webPartXml, zoneId, zoneIndex);
        //}

        //public void DeleteAllWebPart(string pageServerRelativeUrl)
        //{
        //    string pageUrl = AveUrlUtility.CombineUrl(mServerUrl, pageServerRelativeUrl);
        //    XmlNode webPartProperties = GetWebPartProperties(pageUrl);
        //    foreach (XmlNode node7 in webPartProperties)
        //    {
        //        string str4 = node7.Attributes["ID"].Value;
        //        (mNetWorker as WebPartPages.WebPartPagesWebService).DeleteWebPart(pageServerRelativeUrl, new Guid(str4), Storage.Shared);
        //    }
        //}

        public void DeleteSite(string siteUrl)
        {
            (mNetWorker as Admin.Admin).DeleteSite(siteUrl);
        }

        public string AddSite(uint lcid, string owner, string template, string title, string url)
        {
            return (mNetWorker as Admin.Admin).CreateSite(url, title, string.Empty, (int)lcid, template, owner, null, null, null, null);
        }

        private XmlNode GetWebPartProperties(string pageUrl)
        {
            XmlNode webPartProperties = null;
            try
            {
                webPartProperties = (mNetWorker as WebPartPages.WebPartPagesWebService).GetWebPartProperties2(pageUrl, Storage.Shared, SPWebServiceBehavior.Version3);
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    HttpWebResponse exceptionReponse = (e as WebException).Response as HttpWebResponse;
                    if (exceptionReponse != null
                        && (exceptionReponse.StatusCode == HttpStatusCode.Forbidden || exceptionReponse.StatusCode == HttpStatusCode.Unauthorized))
                    {
                        throw new AveSecurityTrimingException("The request failed with HTTP status 401: Unauthorized.", e);
                    }
                }
                try
                {
                    mLogger.Warn("Get WebPart:{0} failed.Error Message:{1}.", pageUrl, e.ToString());
                    webPartProperties = (mNetWorker as WebPartPages.WebPartPagesWebService).GetWebPartProperties(pageUrl, Storage.Shared);
                }
                catch (Exception ex)
                {
                    if (webPartProperties != null && webPartProperties.LastChild is XmlComment)
                    {
                        throw new Exception("Fetching web parts for site " + pageUrl + " failed. " + ((XmlComment)webPartProperties.LastChild).Value + ".Error Message:" + ex.ToString());
                    }
                }
            }
            if (webPartProperties == null)
            {
                mLogger.Log(AveLogLevel.WARN, "No web parts on this page or failed to get web part property. Page Url:{0}", pageUrl);
            }
            return webPartProperties;
        }

        //public void UpdateWebPart(Guid webPartGuid, string sWebPartXml, string sPageUrlContainingWebPart)
        //{
        //    try
        //    {
        //        (mNetWorker as WebPartPages.WebPartPagesWebService).SaveWebPart(sPageUrlContainingWebPart, webPartGuid, sWebPartXml, Storage.Shared);
        //    }
        //    catch (SoapException exception)
        //    {
        //        string soapError = exception.ToString();
        //        throw new Exception("Soap exception while updating web part on page '" + sPageUrlContainingWebPart + "', error: " + soapError, exception);
        //    }
        //    catch (Exception exception2)
        //    {
        //        throw new Exception("A problem was encountered updating a web part on page '" + sPageUrlContainingWebPart + "'. Error: " + exception2.Message, exception2);
        //    }
        //}

        //public void DeleteWebPart(Guid webPartGuid, string pageUrl)
        //{
        //    (mNetWorker as WebPartPages.WebPartPagesWebService).DeleteWebPart(pageUrl, webPartGuid, Storage.Shared);
        //}

        public static XmlNode StringToXmlNode(string sXml)
        {
            XmlNode firstChild;
            if (string.IsNullOrEmpty(sXml))
            {
                return null;
            }
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(sXml);
                firstChild = document.FirstChild;
            }
            catch (Exception exception)
            {
                throw new Exception("Converting an XML string to node failed. The input string is:\n '" + sXml + "'.", exception.InnerException);
            }
            return firstChild;
        }

        public XmlNode UserGroupGetGroupCollectionFromSite()
        {
            return (mNetWorker as UserGroup.UserGroup).GetGroupCollectionFromSite();
        }

        public void ListUpdateList(string listGuid, XmlNode listProperties, XmlNode newFields, XmlNode updateFields, XmlNode deleteFields, string listVersion)
        {
            (mNetWorker as Lists.Lists).UpdateList(listGuid, listProperties, newFields, updateFields, deleteFields, listVersion);
        }
        public int SocialDataGetRatings(string itemUrl)
        {
            SocialDataService.SocialRatingDetail ratingDetail = (mNetWorker as SocialDataService.SocialDataService).GetRatingOnUrl(itemUrl);
            return ratingDetail.Rating;
        }
        public void SocialDataSetRatings(string itemUrl, int ratings, string title, SocialDataService.FeedbackData dataEntry)
        {
            DateTime time = (mNetWorker as SocialDataService.SocialDataService).SetRating(itemUrl, ratings, title, dataEntry);
        }
        public void UserGroupAddUserToGroup(string groupName, string userName, string userLoginName, string userEmail, string userNotes)
        {
            (mNetWorker as UserGroup.UserGroup).AddUserToGroup(groupName, userName, userLoginName, userEmail, userNotes);
        }
        public void UserGroupRemoveUserFromGroup(string groupName, string userLoginName)
        {
            (mNetWorker as UserGroup.UserGroup).RemoveUserFromGroup(groupName, userLoginName);
        }
        public void UserGroupUpdateUser(string userLoginName, string userName, string userEmail, string userNotes)
        {
            (mNetWorker as UserGroup.UserGroup).UpdateUserInfo(userLoginName, userName, userEmail, userNotes);
        }
        public void UserGroupRemoveUserFromSite(string loginName)
        {
            (mNetWorker as UserGroup.UserGroup).RemoveUserFromSite(loginName);
        }

        public string AssociateWorkflowMarkup(string configUrl, string configVersion)
        {
            return (mNetWorker as WebPartPages.WebPartPagesWebService).AssociateWorkflowMarkup(configUrl, configVersion);
        }

        public AvePoint.ObjectModel.WebService.FormsServices.MessagesResponse BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            return (mNetWorker as FormsServices.FormsServicesWebService).BrowserEnableUserFormTemplate(formTemplateUrl);
        }

        public AvePoint.ObjectModel.WebService.FormsServices.DesignCheckerInformation SetFormsForListItem(int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId)
        {
            return (mNetWorker as FormsServices.FormsServicesWebService).SetFormsForListItem(lcid, base64FormTemplate, applicationId, listGuid, contentTypeId);
        }

        public XmlNode UpdateContentTypeXmlDocuments(string listName, string ctId, XmlNode node)
        {
            if (string.IsNullOrEmpty(listName))
            {
                return (mNetWorker as Webs.Webs).UpdateContentTypeXmlDocument(ctId, node);
            }
            else
            {
                return (mNetWorker as Lists.Lists).UpdateContentTypeXmlDocument(listName, ctId, node);
            }
        }

        public XmlNode UpdateContentType(string listName, string contentTypeId, XmlNode contentTypeProperties, XmlNode newFields, XmlNode updateFields, XmlNode deleteFields, string addToView)
        {
            if (string.IsNullOrEmpty(listName))
            {
                return (mNetWorker as Webs.Webs).UpdateContentType(contentTypeId, contentTypeProperties, newFields, updateFields, deleteFields);
            }
            else
            {
                return (mNetWorker as Lists.Lists).UpdateContentType(listName, contentTypeId, contentTypeProperties, newFields, updateFields, deleteFields, addToView);
            }
        }

        public void AddTag(string url, Guid termId, string title, bool? isPrivate)
        {
            (mNetWorker as SocialDataService.SocialDataService).AddTag(url, termId, title, isPrivate);
        }

        public void AddComment(string url, string comment, bool? isHighPriority, string title)
        {
            (mNetWorker as SocialDataService.SocialDataService).AddComment(url, comment, isHighPriority, title);
        }

        public void DeleteTag(string url, Guid termId)
        {
            (mNetWorker as SocialDataService.SocialDataService).DeleteTag(url, termId);
        }

        #endregion

        #region Private Method

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "WpNs:Node of xml.")]
        //private void GetIsIncludedForV3WebParts(string sWppHtml, Dictionary<string, bool> isIncludedDict, Dictionary<string, string> idToWebpartIdProperty)
        //{
        //    if (!string.IsNullOrEmpty(sWppHtml))
        //    {
        //        string webpartId = "WebPartId";
        //        string isInclude = "Included";
        //        string webpartIdProperty = "webpartIdProperty";
        //        string reg = "((IsIncluded\\s*=\\s*\"(?<{0}>[\\w]*)\")|(ViewGuid\\s*=\\s*\"(?<{2}>[{{\\w-}}]*)\")|(ID\\s*=\\s*\"g_(?<{1}>[\\w_]*)\"))";
        //        string wpnsReg = "<WpNs.*" + reg + ".*" + reg;
        //        string xslListViewReg = "<WebPartPages:XsltListViewWebPart.*" + reg + ".*" + reg + ".*" + reg;
        //        Regex regex = new Regex(string.Format("(" + wpnsReg + "|" + xslListViewReg + ")", isInclude, webpartIdProperty, webpartId), RegexOptions.IgnoreCase);
        //        foreach (Match match in regex.Matches(sWppHtml))
        //        {
        //            bool result = false;
        //            if (bool.TryParse(match.Groups[isInclude].Value, out result))
        //            {
        //                isIncludedDict.Add(match.Groups[webpartIdProperty].Value.Replace("_", "-").ToUpperInvariant(), result);
        //            }
        //            if (match.Groups[webpartId].Success && match.Groups[webpartIdProperty].Success)
        //            {
        //                idToWebpartIdProperty.Add(match.Groups[webpartId].Value.Replace("{", "").Replace("}", "").ToUpperInvariant(), match.Groups[webpartIdProperty].Value.Replace("_", "-").ToUpperInvariant());
        //            }
        //        }
        //    }
        //}
        private string JoinUrl(string sStart, string sEnd)
        {
            StringBuilder builder = new StringBuilder(0x101);
            builder.Append(sStart.TrimEnd(new char[] { '/' }));
            builder.Append('/');
            builder.Append(sEnd = sEnd.TrimStart(new char[] { '/' }));
            return builder.ToString();
        }
        #endregion

        #region IDisposable Method
        public void Dispose()
        {
            if (mExistNetWokerEntity == true)
            {
                mNetWorker.Dispose();
                mNetWorker = null;
                mExistNetWokerEntity = false;
                mType = AveWebServiceType.Invalied;
                mMessage = string.Empty;
            }
            if (mExistHttpWebRequestEntity == true)
            {
                mHttpWebRequest = null;
                mExistHttpWebRequestEntity = false;
                mType = AveWebServiceType.Invalied;
                mMessage = string.Empty;
            }
        }
        #endregion

        //[Obsolete("No use now, will remove it later")]
        //private Dictionary<string, string> SelectWebPartProperty(List<Dictionary<string, string>> webPartPropertiesList, string id)
        //{
        //    foreach (Dictionary<string, string> webpartProps in webPartPropertiesList)
        //    {
        //        if (webpartProps.Count <= 0)
        //        {
        //            continue;
        //        }
        //        string webpartid = string.Empty;
        //        if (webpartProps.ContainsKey("__WebPartId"))
        //        {
        //            webpartid = webpartProps["__WebPartId"].Trim('{', '}');
        //        }
        //        else if (webpartProps.ContainsKey("ID"))
        //        {
        //            if (webpartProps["ID"].StartsWith("g_", StringComparison.OrdinalIgnoreCase))
        //            {
        //                webpartid = webpartProps["ID"].Substring(2).Replace("_", "-");
        //            }
        //            else
        //            {
        //                webpartid = webpartProps["ID"].Trim(new char[] { '{', '}' });
        //            }
        //            if (!AveTypeHelper.IsGuid(webpartid))
        //            {
        //                continue;
        //            }
        //        }
        //        if (id.Equals(webpartid, StringComparison.OrdinalIgnoreCase))
        //        {
        //            return webpartProps;
        //        }
        //    }
        //    return null;
        //}
        /// <summary>
        /// Add webpart properties to webpart information xml 
        /// </summary>
        /// <param name="node">webpart information xml's root node</param>
        /// <param name="webpartProps">properties get from file's content</param>
        /// <param name="properties">properties need to be add to xml</param>
        //[Obsolete("No use now, will remove it later")]
        //private void HandleWebPartProperties(XmlNode node, Dictionary<string, string> webpartProps)
        //{
        //    try
        //    {
        //        if (node.NamespaceURI.Equals("http://schemas.microsoft.com/WebPart/v2", StringComparison.OrdinalIgnoreCase))
        //        {
        //            HandleV2WebPartProperties(node, webpartProps);
        //            return;
        //        }
        //        HandleV3WebPartProperties(node, webpartProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        mLogger.Debug(AveWebServiceRequestResource.HandleWebPartPropertiesError, "Title" + " Description" + " ExportMode", node.OuterXml, ex.ToString());
        //    }
        //}

        //[Obsolete("No use now, will remove it later")]
        //private void HandleV2WebPartProperties(XmlNode node, Dictionary<string, string> webpartProps)
        //{
        //    foreach (XmlNode childNode in node.ChildNodes)
        //    {
        //        if (childNode.NodeType.Equals(XmlNodeType.Element) &&
        //            webpartProps.ContainsKey(childNode.Name) &&
        //            !string.IsNullOrEmpty(webpartProps[childNode.Name]))
        //        {
        //            bool boolValue = false;
        //            if (bool.TryParse(webpartProps[childNode.Name], out boolValue))
        //            {
        //                childNode.InnerText = webpartProps[childNode.Name].ToLower(System.Globalization.CultureInfo.CurrentCulture);
        //                continue;
        //            }
        //            childNode.InnerText = webpartProps[childNode.Name];
        //        }
        //    }
        //}

        //[Obsolete("No use now, will remove it later")]
        //private void HandleV3WebPartProperties(XmlNode node, Dictionary<string, string> webpartProps)
        //{
        //    XmlNode propertiesNode = node.SelectSingleNode(".//*[name() = 'properties']");
        //    if (propertiesNode == null)
        //    {
        //        return;
        //    }
        //    foreach (XmlNode childNode in propertiesNode.ChildNodes)
        //    {
        //        if (childNode.NodeType.Equals(XmlNodeType.Element) &&
        //            childNode.Attributes["name"] != null &&
        //            webpartProps.ContainsKey(childNode.Attributes["name"].Value))
        //        {
        //            childNode.InnerText = webpartProps[childNode.Attributes["name"].Value];
        //        }
        //    }
        //}

        //[Obsolete("No use now, will remove it later")]
        //private void FormatWebpartPropertyInCache(XmlNode root, Dictionary<string, string> webpartProps)
        //{
        //    if (!webpartProps.ContainsKey("IsIncluded") && webpartProps.ContainsKey("__designer:IsClosed"))
        //    {
        //        webpartProps["IsIncluded"] = webpartProps["__designer:IsClosed"].Equals(Boolean.FalseString, StringComparison.OrdinalIgnoreCase) ? "true" : "false";
        //    }
        //    if (webpartProps.ContainsKey("ID"))
        //    {
        //        string strWebpart = string.Empty;
        //        if (webpartProps["ID"].StartsWith("g_", StringComparison.OrdinalIgnoreCase))
        //        {
        //            strWebpart = webpartProps["ID"].Substring(2).Replace("_", "-");
        //        }
        //        else
        //        {
        //            strWebpart = webpartProps["ID"].Trim(new char[] { '{', '}' });
        //        }
        //        if (AveTypeHelper.IsGuid(strWebpart))
        //        {
        //            webpartProps["WebPartIdProperty"] = strWebpart;
        //        }
        //    }
        //    else
        //    {
        //        XmlNode Id = root.SelectSingleNode(".//*[name()='ID']");
        //        if (Id == null)
        //        {
        //            Id = root.SelectSingleNode(".//*[@name='ID']");
        //        }
        //        if (Id != null)
        //        {
        //            webpartProps["WebPartIdProperty"] = new Guid(Id.InnerText.TrimStart('g').Replace("_", "")).ToString();
        //        }
        //    }
        //}

        //[Obsolete("No use now, will remove it later")]
        //private void AddPropertiesToWebpartDefinationXml(XmlNode root, string[] needProperties, Dictionary<string, string> webpartProps)
        //{
        //    foreach (string property in needProperties)
        //    {
        //        if (!webpartProps.ContainsKey(property))
        //        {
        //            continue;
        //        }
        //        XmlElement tempElement = root.OwnerDocument.CreateElement(property);
        //        tempElement.InnerText = webpartProps[property];
        //        root.AppendChild(tempElement);
        //    }
        //}
    }

    public enum AveWebServiceType
    {
        Invalied = 0,
        HttpWebRequest,
        Sites,
        Webs,
        SiteData,
        Lists,
        Copy,
        Versions,
        UserGroup,
        WebPartPages,
        UserProfile,
        FormsServices,
        SocialDataService,
        Admin
    }

    public class TrustAllCertificatePolicy : System.Net.ICertificatePolicy
    {
        public TrustAllCertificatePolicy()
        { }
        public bool CheckValidationResult(ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            return true;
        }
    }

}
