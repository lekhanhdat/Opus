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
using System.Collections;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using System.Xml;
using System.Data.SqlClient;
using System.IO;


namespace AvePoint.Wrapper.Restore
{

    public class AveSPWebPart
    {
        protected static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected AveSPWebPartManager mAveWebPartManager;

        protected AveSPDoc mAveDoc;
        //protected AveSqlConnection mSqlCon;

        protected AveWebPartBaseInfo mWebPartInfo;

        public AveWebPartBaseInfo WebPartInfo
        {
            get { return mWebPartInfo; }
            set { mWebPartInfo = value; }
        }

        protected string AssemblyName;
        protected string WebPartType;
        private AveSPSite mParentSite;
        protected bool IsShared = true;
        protected Dictionary<string, object> Properties = new Dictionary<string, object>();

        public AveSPWebPart(AveSPDoc spDoc, AveSPWebPartManager manager)
        {
            mAveDoc = spDoc;
            mParentSite = mAveDoc.ParentSite;
            //mSqlCon = mAveDoc.AveSite.SqlConn;
            mAveWebPartManager = manager;
        }

        public AveSPWebPart(AveSPSite parentSite)
        {            
            mParentSite = parentSite;         
        }

        public void Restore(IList webPartInfos)
        {
            foreach (Object webPartInfoOrId in webPartInfos)
            {
                try
                {
                    if (webPartInfoOrId is AveWebPartBaseInfo)
                    {
                        this.mWebPartInfo = webPartInfoOrId as AveWebPartBaseInfo;

                        RealRestore();
                    }
                    else if (webPartInfoOrId is string)
                    {
                        System.Web.UI.WebControls.WebParts.WebPart webPart = mAveWebPartManager.ReloadWebPart(webPartInfoOrId as string, true);
                        UpdateWebPartByType(webPart, false);
                        mAveWebPartManager.UpdateWebPart(webPart, true);
                    }
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, "WP10RTWebPart096 {0} {1}", webPartInfoOrId, e.ToString());
                }
            }
        }

        public void RestoreWebPartV2(List<AveWebPartBaseInfo> webPartList)
        {
            if (mAveDoc.SPFile != null)
            {
                List<AveWebPartBaseInfo> postActionRestoredWebParts = new List<AveWebPartBaseInfo>();
                List<AveWebPartBaseInfo> restoreWebParts = new List<AveWebPartBaseInfo>();
                XmlDocument webpartDoc = new XmlDocument();
                foreach (AveWebPartBaseInfo webpartInfo in webPartList)
                {
                    webpartDoc.LoadXml(webpartInfo.DefinitionXml);
                    int result = this.ReplaceOldIdsInWebPartXml(webpartInfo, webpartDoc);
                    if (result == 1)
                    {
                        webpartInfo.DefinitionXml = webpartDoc.OuterXml;
                        restoreWebParts.Add(webpartInfo);
                    }
                    else if (mAveDoc.IsCurrentVersion)
                    {
                        postActionRestoredWebParts.Add(webpartInfo);
                    }
                }
                IAveLimitedWebPartManager webpartManager = mParentSite.ObjectModelFactory.CreateLimitedWebPartManager(mParentSite.SPSite, mAveDoc.SPFile.Web, mAveDoc.SPFile);
                if (postActionRestoredWebParts.Count > 0)
                {
                    mParentSite.WebPartPageMapping.Add(webpartManager, postActionRestoredWebParts);
                }
                if (restoreWebParts.Count > 0)
                {
                    try
                    {
                        webpartManager.RestoreWebParts(restoreWebParts);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("error occured when restore webpart, message: " + e.Message + "  stacktrace: " + e.StackTrace);
                    }
                }
            }
        }

        public void PostRestoreWebParts()
        {            
            List<IAveLimitedWebPartManager> shouldRemoved = new List<IAveLimitedWebPartManager>();
            foreach (KeyValuePair<IAveLimitedWebPartManager, List<AveWebPartBaseInfo>> webpartManager in mParentSite.WebPartPageMapping)
            {
                List<AveWebPartBaseInfo> canRestoreWebParts = new List<AveWebPartBaseInfo>();
                XmlDocument webpartDoc = new XmlDocument();
                foreach (AveWebPartBaseInfo webpartBaseInfo in webpartManager.Value)
                {
                    webpartDoc.LoadXml(webpartBaseInfo.DefinitionXml);
                    int result = this.ReplaceOldIdsInWebPartXml(webpartBaseInfo, webpartDoc);
                    if (result == 1)
                    {
                        webpartBaseInfo.DefinitionXml = webpartDoc.OuterXml;
                        canRestoreWebParts.Add(webpartBaseInfo);
                        shouldRemoved.Add(webpartManager.Key);
                    }                    
                }
                if (canRestoreWebParts.Count > 0)
                {
                    webpartManager.Key.RestoreWebParts(canRestoreWebParts);
                }
            }
            foreach (IAveLimitedWebPartManager lwp in shouldRemoved)
            {
                mParentSite.WebPartPageMapping.Remove(lwp);
            }
        }

        //not support yet
        protected bool LoadWebPartProperties(Dictionary<string, object> properties, byte[] allUser, byte[] perUser)
        {
            properties.Clear();
            if (allUser == null && perUser == null)
            {
                return true;
            }
            //Assembly assem = Assembly.GetAssembly(typeof(IAveWebPart));

            //switch (WebPartType)
            //{
            //    case "Microsoft.SharePoint.WebPartPages.XsltListViewWebPart":
            //    case "Microsoft.SharePoint.WebPartPages.XsltListFormWebPart":
            //    case "Microsoft.SharePoint.WebPartPages.SilverlightWebPart":
            //    case "Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart":
            //    //ContentEditorWebPart load出来的属性值没有和本来的属性值对应上
            //    case "Microsoft.SharePoint.WebPartPages.ContentEditorWebPart":
            //        return false;
            //    default:
            //        break;
            //}
            try
            {
                int resultCode = 0;
                Dictionary<string, object> tmpDic = AveWebPartUtility.GetProperties(allUser, perUser, out resultCode);
                foreach (string key in tmpDic.Keys)
                {
                    if (key.Equals("ViewGuid"))
                    {//当ViewGuid对应的view不存在时，webPart不能add成功。该属性暂时需要跳过
                        continue;
                    }
                    properties[key] = tmpDic[key];
                }
                //Type XmlSchema = assem.GetType("Microsoft.SharePoint.WebPartPages.XmlSchema");
                //Type WebPartNameTable = assem.GetType("Microsoft.SharePoint.WebPartPages.WebPartNameTable");
                //Type CompressedXmlReader = assem.GetType("Microsoft.SharePoint.WebPartPages.CompressedXmlReader");

                //ConstructorInfo cons = WebPartNameTable.GetConstructors()[0];
                //object o = cons.Invoke(null);

                //ConstructorInfo readerCon = CompressedXmlReader.GetConstructors()[0];

                //XmlReader reader = (XmlReader)readerCon.Invoke(new object[] { new XmlNamespaceManager((XmlNameTable)o), perUser, allUser });

                //string propertyName = String.Empty;
                //string value = String.Empty;

                //while (reader.Read())
                //{
                //    switch (reader.NodeType)
                //    {
                //        case XmlNodeType.Element:
                //            propertyName = reader.LocalName;
                //            break;
                //        //case XmlNodeType.Attribute:
                //        //    string value1 = reader11.Value;
                //        //    string name = reader11.Name;
                //        //    break;
                //        case XmlNodeType.CDATA:
                //        case XmlNodeType.Text:
                //            value = reader.Value;
                //            break;
                //        case XmlNodeType.EndElement:
                //            if (!String.IsNullOrEmpty(propertyName) && propertyName != "WebPart")
                //            {
                //                properties[propertyName] = value;
                //            }
                //            propertyName = String.Empty;
                //            value = String.Empty;
                //            break;
                //        default:
                //            break;
                //    }
                //}

                ////[DOC-54131]Migration07to10中的ThisWeekInPicturesWebPart的不同
                //switch (WebPartType)
                //{
                //    case "Microsoft.SharePoint.Portal.WebControls.ThisWeekInPicturesWebPart":
                //        if (!properties.ContainsKey("ImageLibrary"))
                //        {
                //            properties["ImageLibrary"] = "This Week in Pictures Library";
                //        }
                //        break;
                //    default:
                //        break;
                //}

            }
            catch (Exception ex)
            {
                //mLog.Log(AveLogLevel.INFO, string.Format("Can't analyze webpart info, is not moss webPart."));
                mLog.Info("Can't analyze webpart info, is not moss webPart.", ex);
                return false;
            }
            return true;
        }

        //DOC-DOC-53408 DateFilterWebpart add时会抛异常 一个异常是在Microsoft.SharePoint.Portal.WebControls.DateFilterWebPart.InitializeDatePicker(DateTimeControl datePicker)方法里
        //SPContext.Current.Web为null，另外一个是System.Web.UI.Control.ResolveAdapter()方法里System.Web.HttpContext.Current.Request.Browser 为null  by linxin
        //public  void  FakeSPContext(IAveWeb web)
        //{
        //    try
        //    {
        //        if (System.Web.HttpContext.Current == null)
        //        {
        //            System.Web.HttpRequest request = new System.Web.HttpRequest("", web.Url, "");
        //            System.Web.HttpContext.Current = new System.Web.HttpContext(request,
        //              new System.Web.HttpResponse(new StringWriter()));
        //        }

        //        // SPContext is based on SPControl.GetContextWeb(), which looks here
        //        if (System.Web.HttpContext.Current.Items["HttpHandlerSPWeb"] == null)
        //            System.Web.HttpContext.Current.Items["HttpHandlerSPWeb"] = web;
        //        if (System.Web.HttpContext.Current.Request.Browser == null)
        //        {
        //            System.Web.HttpBrowserCapabilities browser = new System.Web.HttpBrowserCapabilities();
        //            var field = browser.GetType().BaseType.GetField("_browser", BindingFlags.Instance | BindingFlags.NonPublic);
        //            field.SetValue(browser, System.Web.HttpContext.Current.Request.UserAgent);
        //            field = browser.GetType().BaseType.GetField("_havebrowser", BindingFlags.Instance | BindingFlags.NonPublic);
        //            field.SetValue(browser, true);
        //            System.Web.HttpContext.Current.Request.Browser = browser;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //mLog.Warn("An error occured while FakeSPContext. error:{0}", ex.ToString());
        //    }

        //}

        public virtual void RealRestore()
        {
            try
            {
                #region replace webpart type id first
                Guid tempWebPartId;
                if (mAveDoc.ParentSite.NeedWebPartIDMapping.TryGetValue(mWebPartInfo.WebPartTypeId, out tempWebPartId))
                {
                    mWebPartInfo.WebPartTypeId = tempWebPartId;
                }
                #endregion
                if (!mAveDoc.ParentSite.WebPartTypeIDMapping.ContainsKey(mWebPartInfo.WebPartTypeId) && (mWebPartInfo.Assembly == null || mWebPartInfo.Class == null))
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("Can't find the webpart assembly info."));
                    mLog.Warn("Can't find the webpart assembly info.");
                    return;
                }

                System.Web.UI.WebControls.WebParts.WebPart webPart = null;

                #region Get WebPart
                bool isMossWebPart = false;
                if (mWebPartInfo.Assembly != null && mWebPartInfo.Class != null)
                {
                    AssemblyName = mWebPartInfo.Assembly;
                    WebPartType = mWebPartInfo.Class;
                }
                else
                {
                    string[] assembly = mAveDoc.ParentSite.WebPartTypeIDMapping[mWebPartInfo.WebPartTypeId].ToString().Split('|');
                    AssemblyName = assembly[0];
                    WebPartType = assembly[1];
                }
                if (!CheckListId())
                {
                    mAveDoc.ParentFolder.ParentList.ParentWeb.AddUnRestoreWebPartInfo(mWebPartInfo.ListTitle, mAveDoc.SPFile.UniqueId, mWebPartInfo);
                    return;
                }

                #region notview

                if (!string.IsNullOrEmpty(mWebPartInfo.WebPartIdProperty))
                {
                    webPart = mAveWebPartManager.GetWebPart(mWebPartInfo.WebPartIdProperty, IsShared);
                }
                if (webPart == null)
                {
                    webPart = mAveWebPartManager.CreateWebPartInstance(AssemblyName, WebPartType);

                    isMossWebPart = RestoreWebPartProperties(webPart);
                    UpdateWebPartByType(webPart, true);
                    try
                    {
                        mAveWebPartManager.AddWebPart(webPart, mWebPartInfo.ZoneID, mWebPartInfo.PartOrder);
                    }
                    catch (Exception ex)
                    {
                        mAveDoc.Web.FakeSPContext();//DOC-53408
                        mAveWebPartManager.AddWebPart(webPart, mWebPartInfo.ZoneID, mWebPartInfo.PartOrder);
                    }
                }
                #endregion
                RestoreCommonProperties(webPart);
                #region not support
                //if (!isMossWebPart)
                //{
                //    if (mWebPartInfo.AllUsersProperties != null || mWebPartInfo.PerUserProperties != null)
                //    {
                //        UpdatePropertiesInDatabase(webPart.ID);
                //        if (mWebPartInfo.BaseViewID >= 0)
                //        {
                //            UpdateView(webPart.ID, mWebPartInfo.BaseViewID, mWebPartInfo.View, mWebPartInfo.ContentTypeId);
                //        }
                //        webPart = mAveWebPartManager.ReloadWebPart(webPart.ID, IsShared);
                //        //AllUsersProperties中存储了webpartID, 
                //        if (webPart == null && !string.IsNullOrEmpty(mWebPartInfo.WebPartIdProperty))
                //        {
                //            webPart = mAveWebPartManager.ReloadWebPart(mWebPartInfo.WebPartIdProperty, IsShared);
                //        }
                //        if (webPart == null)
                //        {
                //            mLog.Log(AveLogLevel.WARN, string.Format("Reload WebPart failed."));
                //            //mLog.Warn("Reload WebPart failed.");
                //            return;
                //        }
                //    }
                //    if (webPart is IAveWebPart)
                //    {
                //        IAveWebPart tempWebpart = webPart as IAveWebPart;
                //        string authorizationFilter = tempWebpart.AuthorizationFilter;
                //        if (!string.IsNullOrEmpty(authorizationFilter))
                //        {
                //            authorizationFilter = AveAudienceManager.ReplaceAudienceId(mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.AudienceIDMapping, authorizationFilter);
                //            tempWebpart.AuthorizationFilter = authorizationFilter;
                //        }
                //    }
                //}
                #endregion
                if (webPart == null)
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("Can't get the webpart object."));
                    mLog.Warn("Can't get the webpart object.");
                    return;
                }

                #endregion
                if (mWebPartInfo.Personalization != null)
                {
                    AvePersonalizationInfo currentUserPersonalizationInfo = null;
                    System.Web.UI.WebControls.WebParts.WebPart personalWebPart = mAveWebPartManager.ReloadWebPart(webPart.ID, false);
                    foreach (AvePersonalizationInfo personInfo in mWebPartInfo.Personalization)
                    {
                        try
                        {
                            int userId = mAveDoc.ParentSite.SPMembers.FindMember(personInfo.UserID, true).ID;
                            if (userId == mAveDoc.Web.CurrentUser.ID)
                            {
                                currentUserPersonalizationInfo = personInfo;
                                continue;
                            }
                            RestorePersonalization(personalWebPart, personInfo, userId);
                            personalWebPart = mAveWebPartManager.ReloadWebPart(webPart.ID, false);
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "WP10RTWebPart285 {0} {1} {2}", mWebPartInfo.TitleUrl, mWebPartInfo.ID, e.ToString());
                        }
                    }
                    RestorePersonalization(personalWebPart, currentUserPersonalizationInfo, mAveDoc.Web.CurrentUser.ID);
                }

                if (mWebPartInfo.WebPartIdProperty != null)
                {
                    webPart.ID = mWebPartInfo.WebPartIdProperty;
                }
                else
                {
                    string id = "g_" + mWebPartInfo.ID.ToString();
                    id = id.Replace("-", "_");
                    webPart.ID = id;
                }

                UpdateWebPartByType(webPart, false);

                mAveWebPartManager.UpdateWebPart(webPart, IsShared);

                //如果源端的Properties都为null，但是目的端的不一定为null，清空一下目的端的，不然多余的数据可能导致显示不一致。
                if (mWebPartInfo.AllUsersProperties == null && mWebPartInfo.PerUserProperties == null)
                {
                    UpdatePropertiesInDatabase(webPart.ID);
                }

                if (mWebPartInfo.BaseViewID != null)
                {
                    UpdateView(webPart.ID, (int)mWebPartInfo.BaseViewID, mWebPartInfo.View, mWebPartInfo.ContentTypeId);
                }
                if (mWebPartInfo.UserID > 0)
                {
                    int userId = mAveDoc.ParentSite.SPMembers.FindMemberId(mWebPartInfo.UserID);
                    UpdateUserID(webPart.ID, userId, false);
                }
                if (mWebPartInfo.PageVersion != 0 && mWebPartInfo.PageVersion < mAveDoc.SPFile.UIVersion)
                {
                    UpdateWebPartInfo(webPart.ID);
                }
            }
            catch (Exception ex)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while resotre web part {0}.\n error message:{1}", AssemblyName + "|" + WebPartType, ex));
                mLog.Warn("An error occurred while resotre web part {0}, error:{1}", AssemblyName + "|" + WebPartType, ex.ToString());
            }
        }

        protected bool CheckViewContentType(ref byte[] idbytes)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in idbytes)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
                string sourceId = "0x" + sb.ToString();
                if (sourceId.Equals("0x") || sourceId.Equals("0x012001"))//0x: in all folders 0x012001:in top-level folder
                {
                    return true;
                }
                else if (mAveDoc.ParentFolder.ParentList.ListLevelCTMapping.ContainsKey(sourceId))//in other folder
                {
                    string destId = mAveDoc.ParentFolder.ParentList.ListLevelCTMapping[sourceId].ID.ToString().TrimStart('0').TrimStart('x');
                    if ((destId.Length % 2) != 0)
                        destId += " ";
                    byte[] returnBytes = new byte[destId.Length / 2];
                    for (int i = 0; i < returnBytes.Length; i++)
                    {
                        returnBytes[i] = Convert.ToByte(destId.Substring(i * 2, 2), 16);
                    }
                    idbytes = returnBytes;
                    return true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("WP10RTWebPart480", ex);
            }
            return false;
        }

        protected void RestoreCommonProperties(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            if (!mWebPartInfo.IsIncluded)
            {
                mAveWebPartManager.CloseWebPart(webPart, true);
            }
        }

        protected bool UpdateWebPartByType(System.Web.UI.WebControls.WebParts.WebPart webPart, bool beforeAdd)
        {
            SpecialWebPartUpdater updater = SpecialWebPartUpdater.GetWebPartUpdater(webPart, mAveDoc);
            if (updater == null)
            {
                return true;
            }
            try
            {
                if (beforeAdd)
                {
                    return updater.DoUpateBeforeAdd(mWebPartInfo);
                }
                else
                {
                    return updater.DoUpateAfterAdd(mWebPartInfo);
                }
            }
            catch (Exception ex)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("Can't update webpart.\n error message:{0}", ex));
                mLog.Warn("Can't update webpart.", ex);
                return true;
            }
        }

        public static bool UpdateWebPartByType(System.Web.UI.WebControls.WebParts.WebPart webPart, bool beforeAdd, AveSPDoc aveDoc, AveWebPartBaseInfo webPartInfo)
        {
            SpecialWebPartUpdater updater = SpecialWebPartUpdater.GetWebPartUpdater(webPart, aveDoc);
            if (updater == null)
            {
                return true;
            }
            try
            {
                if (beforeAdd)
                {
                    return updater.DoUpateBeforeAdd(webPartInfo);
                }
                else
                {
                    return updater.DoUpateAfterAdd(webPartInfo);
                }
            }
            catch (Exception ex)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("Can't update webpart.\n error message:{0}", ex));
                mLog.Warn("Can't update webpart.", ex);
                return true;
            }
        }

        protected bool RestoreWebPartProperties(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            bool isMossWebPart = LoadWebPartProperties(properties, mWebPartInfo.AllUsersProperties, mWebPartInfo.PerUserProperties);
            if (isMossWebPart)
            {
                RestoreWebPartProperties(webPart, properties);
            }
            return isMossWebPart;
        }

        protected bool RestoreWebPartProperties(System.Web.UI.WebControls.WebParts.WebPart webPart, Dictionary<string, object> properties)
        {
            IAveWebPart mossPart = webPart as IAveWebPart;

            foreach (KeyValuePair<string, object> pair in properties)
            {
                try
                {
                    if (mossPart != null)
                    {
                        if (pair.Key == "Height")
                        {
                            mossPart.Height = pair.Value.ToString();
                            continue;
                        }
                        else if (pair.Key == "Width")
                        {
                            mossPart.Width = pair.Value.ToString();
                            continue;
                        }
                        else if (pair.Key == "TitleUrl")
                        {
                            mossPart.TitleUrl = AveReplaceProcessor.UrlReplace(pair.Value.ToString(), mAveDoc.ParentSite.SiteManagedMappings, new ReplaceOption(true));
                            continue;
                        }
                    }

                    if (pair.Key == "ListId")
                    {
                        if (mWebPartInfo.ListId != Guid.Empty)
                        {
                            AveSPWebPartManager.SetWebPartProperty(webPart, pair.Key, mWebPartInfo.ListId.ToString());
                        }
                    }
                    else if (pair.Key == "ListName")
                    {
                        if (mWebPartInfo.ListId != Guid.Empty)
                        {
                            AveSPWebPartManager.SetWebPartProperty(webPart, pair.Key, mWebPartInfo.ListId.ToString("B").ToUpper());
                        }
                    }
                    else if (pair.Key == "MembershipGroupId" && webPart is IAveMembersWebPart)
                    {
                        int originGroupId = Convert.ToInt32(pair.Value);
                        int newGroupId = -1;
                        if (mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SPMembers.SiteUserIDMapping.ContainsKey(originGroupId))
                        {
                            object obj = mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SPMembers.SiteUserIDMapping[originGroupId];
                            if (obj is AveSPMemberInfo)
                            {
                                AveSPMemberInfo member = obj as AveSPMemberInfo;
                                newGroupId = member.NewId;
                            }
                        }
                        if (newGroupId < 0)
                        {
                            mLog.Info("Can't find mapping Group Id while restore MembersWebPart property.");
                            AveSPWebPartManager.SetWebPartProperty(webPart, pair.Key, originGroupId);
                        }
                        else
                        {
                            AveSPWebPartManager.SetWebPartProperty(webPart, pair.Key, newGroupId);
                        }
                    }
                    else if (pair.Key == "IsIncludedFilter")
                    {
                        string audience = pair.Value.ToString();
                        if (mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.AudienceIDMapping != null)
                        {
                            audience = AveAudienceManager.ReplaceAudienceId(mAveDoc.ParentSite.AudienceIDMapping, audience);
                        }
                        AveSPWebPartManager.SetWebPartProperty(webPart, pair.Key, audience);
                    }
                    else
                    {
                        string propertyName = pair.Key;
                        object value = pair.Value;
                        if (propertyName.Equals("XML"))
                        {
                            propertyName = "Xml";
                        }
                        if (propertyName.Equals("XSLLink"))
                        {
                            propertyName = "XslLink";
                        }
                        if (propertyName.EndsWith("Link", StringComparison.OrdinalIgnoreCase) || propertyName.EndsWith("URL", StringComparison.OrdinalIgnoreCase))
                        {
                            if (value.ToString().Contains('/'))
                            {
                                value = AveReplaceProcessor.UrlReplace(value.ToString(), mAveDoc.ParentSite.SiteManagedMappings, new ReplaceOption(true));
                            }
                        }
                        AveSPWebPartManager.SetWebPartProperty(webPart, propertyName, value);
                    }

                }
                catch (Exception aE)
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("Error happened when Setting the property of WebPart, property: {0}, WebPart Type: {1}\n error message:{2}", pair.Key, webPart.GetType(), aE));
                    mLog.Warn("Error happened when Setting the property of WebPart, property: {0}, WebPart Type: {1}. Reason: {2}", pair.Key, webPart.GetType().ToString(), aE.ToString());
                }
            }

            return false;
        }

        protected void RestorePersonalization(System.Web.UI.WebControls.WebParts.WebPart webPart, AvePersonalizationInfo personalInfo, int userId)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            bool isMossWebPart = LoadWebPartProperties(properties, null, personalInfo.PerUserProperties);

            if (isMossWebPart)
            {
                RestoreWebPartProperties(webPart, properties);
                mAveWebPartManager.UpdateWebPart(webPart, false);
            }

            if (!personalInfo.IsInCluded)
            {
                mAveWebPartManager.CloseWebPart(webPart, false);
            }

            if (!isMossWebPart && personalInfo.PerUserProperties != null)
            {
                UpdatePersonalPropertiesInDatabase(webPart.ID, personalInfo.PerUserProperties);
            }

            if (userId != mAveDoc.Web.CurrentUser.ID)
            {
                UpdateUserID(webPart.ID, userId, true);
            }
        }

        protected bool CheckListId()
        {
            Type webPartType = AveAssemblyUtility.GetType(AssemblyName, WebPartType);
            if (webPartType.GetInterface("IListWebPart") != null)
            {
                Guid webId = Guid.Empty;
                if (mWebPartInfo.WebPartList != null && mWebPartInfo.WebPartList.Count > 0)
                {
                    if (mAveDoc.ParentSite.WebIDMapping.ContainsKey(mWebPartInfo.WebPartList[0].WebId))
                    {
                        webId = mAveDoc.ParentSite.WebIDMapping[mWebPartInfo.WebPartList[0].WebId];
                    }
                    else
                    {
                        webId = mAveDoc.ParentSite.GetMappingWeb(mWebPartInfo.WebPartList[0].FullUrl, true);
                    }
                }
                else
                {
                    webId = mAveDoc.Web.ID;
                }
                if (mWebPartInfo.ListId.Equals(Guid.Empty) && string.IsNullOrEmpty(mWebPartInfo.ListTitle))
                {
                    return true;
                }
                #region --for language mapping
                string realListTitle = mWebPartInfo.ListTitle;
                if (mAveDoc.ParentSite.AveLanguageProcesser == null)
                {
                    //do nothing
                }
                else if (mAveDoc.ParentSite.AveLanguageProcesser.ListMapping.ContainsKey(realListTitle))
                {
                    mWebPartInfo.ListTitle = mAveDoc.ParentSite.AveLanguageProcesser.ListMapping[realListTitle].ToString();
                }
                #endregion
                Guid listId = mAveDoc.ParentSite.GetMappingList(webId, mWebPartInfo.ListTitle, mWebPartInfo.ListId);
                if (!listId.Equals(Guid.Empty))
                {
                    mWebPartInfo.ListId = listId;
                    if (!webId.Equals(Guid.Empty))
                    {
                        if (mWebPartInfo.WebPartList == null)
                        {
                            mWebPartInfo.WebPartList = new List<AveWebPartListInfo>();
                            mWebPartInfo.WebPartList.Add(new AveWebPartListInfo());
                        }
                        mWebPartInfo.WebPartList[0].WebId = webId;
                    }
                    return true;
                }
                return false;
            }
            return true;
        }

        protected void UpdatePropertiesInDatabase(string webPartId)
        {
            if (!this.mParentSite.ObjectModelFactory.IsSPInstalled)
            {
                return;
            }
            mAveWebPartManager.UpdatePropertiesInDatabase(webPartId, mAveDoc.ParentSite.SPSite.ID, mAveDoc.SPFile.UniqueId, mWebPartInfo.AllUsersProperties, mWebPartInfo.PerUserProperties);
        }

        protected void UpdatePersonalPropertiesInDatabase(string webPartId, byte[] perUserBytes)
        {
            if (!this.mParentSite.ObjectModelFactory.IsSPInstalled)
            {
                return;
            }
            mAveWebPartManager.UpdatePersonalPropertiesInDatabase(webPartId, mAveDoc.ParentSite.SPSite.ID, mAveDoc.SPFile.Web.CurrentUser.ID, perUserBytes);
        }

        protected void UpdateUserID(string webPartId, int userId, bool isPersonal)
        {
            if (!this.mParentSite.ObjectModelFactory.IsSPInstalled)
            {
                return;
            }
            mAveWebPartManager.UpdateUserID(webPartId, mAveDoc.ParentSite.SPSite.ID, mAveDoc.SPFile.UniqueId, mAveDoc.Web.CurrentUser.ID, userId, isPersonal);
            mAveWebPartManager.Dispose();
        }

        protected void UpdateView(string webPartId, int baseViewId, byte[] view, byte[] contentTypeId)
        {
            if (!this.mParentSite.ObjectModelFactory.IsSPInstalled)
            {
                return;
            }
            bool needUpdateContentType = CheckViewContentType(ref contentTypeId);

            if (needUpdateContentType)
            {
                mAveWebPartManager.UpdateView(webPartId, mAveDoc.ParentSite.SPSite.ID, mAveDoc.SPFile.UniqueId, baseViewId, view, contentTypeId);
            }
            else
            {
                mAveWebPartManager.UpdateView(webPartId, mAveDoc.ParentSite.SPSite.ID, mAveDoc.SPFile.UniqueId, baseViewId, view, null);
            }
        }

        protected void UpdateWebPartInfo(string webPartId)
        {
            if (!this.mParentSite.ObjectModelFactory.IsSPInstalled)
            {
                return;
            }
            mAveWebPartManager.UpdateWebPartInfo(webPartId, mAveDoc.ParentSite.SPSite.ID, mAveDoc.SPFile.UniqueId, mWebPartInfo.PageVersion, (byte)mAveDoc.SPFile.Level, mWebPartInfo.Level, mWebPartInfo.IsCurrentVersion, mAveDoc.SPFile.UIVersion);
        }

        // 1 means can restore   0, 2 means shoud restore in post action 
        protected int ReplaceOldIdsInWebPartXml(AveWebPartBaseInfo webpartInfo, XmlDocument webpartDoc)
        {
            XmlNode tempNode = webpartDoc.FirstChild;
            if (string.IsNullOrEmpty(tempNode.NamespaceURI))
            {
                tempNode = webpartDoc.FirstChild.FirstChild;
            }
            if (tempNode.NamespaceURI.Equals("http://schemas.microsoft.com/WebPart/v2"))
            {
                ReplaceLinkUrlV2(tempNode);
            }
            else
            {
                ReplaceLinkUrlV3(tempNode);
            }
            XmlNode libNode = webpartDoc.SelectSingleNode(".//*[@name = 'LibraryGuid']");
            if (libNode != null)
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (mParentSite.ListIdMapping.ContainsKey(oldLibId))
                {
                    libNode.InnerText = mParentSite.ListIdMapping[oldLibId].ToString();
                    XmlNode viewIdNode = webpartDoc.SelectSingleNode(".//*[@name = 'ViewGuid']");
                    if (mParentSite.ViewGuidMapping.ContainsKey(new Guid(viewIdNode.InnerText)))
                    {
                        viewIdNode.InnerText = mParentSite.ViewGuidMapping[new Guid(viewIdNode.InnerText)].ToString();
                    }
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            XmlNode webNode = webpartDoc.SelectSingleNode(".//*[@name = 'WebId']");
            if (webNode != null)
            {
                return ReplaceXsltListViewWebPart(webpartInfo, webpartDoc);
            }
            else
            {
                return ReplaceListViewWebPart(webpartInfo, webpartDoc);
            }            
        }

        private void ReplaceLinkUrlV2(XmlNode rootNode)
        {
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                if (node.Name.EndsWith("Link", StringComparison.OrdinalIgnoreCase) || node.Name.EndsWith("URL", StringComparison.OrdinalIgnoreCase) || node.Name.Equals("MediaSource"))
                {
                    if (node.InnerText.ToString().Contains('/'))
                    {
                        node.InnerText = AveReplaceProcessor.UrlReplace(node.InnerText.ToString(), mParentSite.SiteManagedMappings, new ReplaceOption(true, true));
                    }
                }
            }
        }

        private void ReplaceLinkUrlV3(XmlNode rootNode)
        {
            foreach (XmlNode pNode in rootNode.ChildNodes)
            {
                if (pNode.Name.Equals("data"))
                {
                    XmlNode tempNode = pNode.FirstChild;
                    if (tempNode == null)
                    {
                        return;
                    }
                    foreach (XmlNode node in tempNode.ChildNodes)
                    {
                        if (node.Attributes.Count > 0 && (node.Attributes[0].Value.EndsWith("Link", StringComparison.OrdinalIgnoreCase) || node.Attributes[0].Value.EndsWith("URL", StringComparison.OrdinalIgnoreCase) || node.Attributes[0].Value.Equals("MediaSource")))
                        {
                            if (node.InnerText.ToString().Contains('/'))
                            {
                                node.InnerText = AveReplaceProcessor.UrlReplace(node.InnerText.ToString(), mParentSite.SiteManagedMappings, new ReplaceOption(true, true));
                            }
                        }
                    }
                }
            }
        }

        protected int ReplaceListViewWebPart(AveWebPartBaseInfo webPartInfo, XmlDocument webpartDoc)
        {
            XmlNode webNode = null;            
            XmlNode listIdNode = null;
            XmlNode listNameNode = null;
            XmlNode defNode = null;            
            Guid webId = Guid.Empty;
            Guid listId = Guid.Empty;            
            webNode = webpartDoc.SelectSingleNode("//*[name() = 'WebId']");
            if (webNode != null)
            {
                webId = new Guid(webNode.InnerText);
                if (webId != Guid.Empty && mParentSite.WebIDMapping.ContainsKey(webId))
                {                     
                    webNode.InnerText = mParentSite.WebIDMapping[webId].ToString();                                            
                }
            }
            listIdNode = webpartDoc.SelectSingleNode("//*[name() = 'ListId']");
            listNameNode = webpartDoc.SelectSingleNode("//*[name() = 'ListName']");
            if (listIdNode != null)
            {
                listId = new Guid(listIdNode.InnerText);
                if (mParentSite.ListIdMapping.ContainsKey(listId))
                {                            
                    defNode = webpartDoc.SelectSingleNode("//*[name() = 'ListViewXml']");
                    Guid currentViewId = Guid.Empty;
                    if (defNode != null)
                    {
                        XmlDocument viewNode = new XmlDocument();
                        viewNode.LoadXml(defNode.InnerText);
                        string viewId = viewNode.DocumentElement.GetAttribute("Name");
                        Guid viewGuid = new Guid(viewId);
                        if (mParentSite.ViewGuidMapping.ContainsKey(viewGuid))
                        {
                            currentViewId = mParentSite.ViewGuidMapping[viewGuid];
                            viewNode.DocumentElement.SetAttribute("Name", "{" + currentViewId.ToString() + "}");
                            if (mAveDoc.SPView != null && currentViewId == mAveDoc.SPView.ID)
                            {
                                webPartInfo.IsViewBuildInWebPart = true;
                            }
                        }
                        defNode.InnerText = viewNode.OuterXml;
                    }                                                                   
                    if (listIdNode != null)
                    {
                        listIdNode.InnerText = mParentSite.ListIdMapping[listId].ToString(); ;
                    }
                    if (listNameNode != null)
                    {
                        listNameNode.InnerText = "{" + listIdNode.InnerText + "}";
                    }
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {               
                return 1;
            }                                                  
        }        

        protected int ReplaceXsltListViewWebPart(AveWebPartBaseInfo webPartInfo, XmlDocument webpartDoc)
        {
            XmlNode webNode = null;
            XmlNode listNode = null;
            XmlNode listIdNode = null;
            XmlNode defNode = null;            
            Guid webId = Guid.Empty;
            Guid listId = Guid.Empty;            
            webNode = webpartDoc.SelectSingleNode(".//*[@name = 'WebId']");
            if (webNode != null)
            {
                webId = new Guid(webNode.InnerText);
                if (webId == Guid.Empty || mParentSite.WebIDMapping.ContainsKey(webId))
                {
                    if (webId != Guid.Empty)
                    {
                        webNode.InnerText = mParentSite.WebIDMapping[webId].ToString();
                    }
                }
            }

            listNode = webpartDoc.SelectSingleNode(".//*[@name = 'ListName']");
            if (listNode != null)
            {
                listId = new Guid(listNode.InnerText);
                if (mParentSite.ListIdMapping.ContainsKey(listId))
                {
                    listIdNode = webpartDoc.SelectSingleNode(".//*[@name = 'ListId']");
                    defNode = webpartDoc.SelectSingleNode(".//*[@name = 'XmlDefinition']");
                    Guid currentViewId = Guid.Empty;
                    if (defNode != null)
                    {
                        XmlDocument viewNode = new XmlDocument();
                        viewNode.LoadXml(defNode.InnerText);
                        string viewId = viewNode.DocumentElement.GetAttribute("Name");
                        Guid viewGuid = new Guid(viewId);
                        if (mParentSite.ViewGuidMapping.ContainsKey(viewGuid))
                        {
                            currentViewId = mParentSite.ViewGuidMapping[viewGuid];
                            viewNode.DocumentElement.SetAttribute("Name", "{" + currentViewId.ToString() + "}");
                            if (mAveDoc.SPView != null && currentViewId == mAveDoc.SPView.ID)
                            {
                                webPartInfo.IsViewBuildInWebPart = true;
                            }
                        }
                        defNode.InnerText = viewNode.OuterXml;
                    }                                             
                    if (listNode != null)
                    {
                        listNode.InnerText = "{" + mParentSite.ListIdMapping[listId].ToString() + "}";
                    }
                    if (listIdNode != null)
                    {
                        listIdNode.InnerText = mParentSite.ListIdMapping[listId].ToString(); ;
                    }
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {                        
                return 1;
            }                         
        }
    }   

    public class AveSPViewWebPart : AveSPWebPart
    {
        public AveSPViewWebPart(AveSPDoc spDoc, AveSPWebPartManager manager)
            : base(spDoc, manager)
        {
            //do nothing
        }

        public override void RealRestore()
        {
            try
            {
                #region replace webpart type id first
                Guid tempWebPartId;
                if (mAveDoc.ParentSite.NeedWebPartIDMapping.TryGetValue(mWebPartInfo.WebPartTypeId, out tempWebPartId))
                {
                    mWebPartInfo.WebPartTypeId = tempWebPartId;
                }
                #endregion
                if (!mAveDoc.ParentSite.WebPartTypeIDMapping.ContainsKey(mWebPartInfo.WebPartTypeId) && (mWebPartInfo.Assembly == null || mWebPartInfo.Class == null))
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("Can't find the webpart assembly info."));
                    mLog.Warn("Can't find the webpart assembly info.");
                    return;
                }

                System.Web.UI.WebControls.WebParts.WebPart webPart = null;

                #region Get WebPart
                //bool isView = mAveDoc.AveView != null && mAveDoc.AveView.Views.ContainsKey(mWebPartInfo.ID);
                if (mWebPartInfo.Assembly != null && mWebPartInfo.Class != null)
                {
                    AssemblyName = mWebPartInfo.Assembly;
                    WebPartType = mWebPartInfo.Class;
                }
                else
                {
                    string[] assembly = mAveDoc.ParentSite.WebPartTypeIDMapping[mWebPartInfo.WebPartTypeId].ToString().Split('|');
                    AssemblyName = assembly[0];
                    WebPartType = assembly[1];
                }
                if (!CheckListId())
                {
                    mAveDoc.ParentFolder.ParentList.ParentWeb.AddUnRestoreWebPartInfo(mWebPartInfo.ListTitle, mAveDoc.SPFile.UniqueId, mWebPartInfo);
                    return;
                }

                #region isView
                IAveView view = null;
                bool isPersional = (mWebPartInfo.UserID > 0); //如果存在UserID，判断其属于 persional view.
                try
                {
                    if (isPersional)
                    {
                        IsShared = false;
                        webPart = mAveWebPartManager.GetPersonalViewWebPart(ref view, mWebPartInfo.ID, IsShared, mWebPartInfo.UserID);
                    }
                }
                catch { }

                //这里可能有两种情况　１．PersonalView第一次还原的时候　２．不是PersonalView的时候
                if (view == null)
                {
                    view = mAveDoc.ParentFolder.ParentList.SPList.Views[mAveDoc.AveView.Views[mWebPartInfo.ID]];
                }
                if (view != null)
                {
                    //mAveDoc.AveView.RestoreViewProperties(mWebPartInfo, view);
                    string baseViewID = mWebPartInfo.BaseViewID.ToString();
                    string strValidBaseViewID = mAveWebPartManager.GetValidBaseViewIdStr(mAveDoc.ParentFolder.ParentList.SPList);
                    if (!strValidBaseViewID.Contains("|" + baseViewID + "|"))
                    {
                        mLog.Log(AveLogLevel.WARN, "source baseViewID is not equal with view baseViewID. view title:{0}", view.Title);
                        //mLog.Warn("source baseViewID is not equal with view baseViewID. view title:{0}", view.Title);
                        return;
                    }
                    webPart = mAveWebPartManager.GetWebPart(view.ID, IsShared);
                    if (mWebPartInfo.View != null)
                    {
                        try
                        {
                            string viewString = AveCompressedUtility.GetTCompressedString(mWebPartInfo.View);
                            if (!string.IsNullOrEmpty(viewString))
                            {
                                XmlDocument xDoc = new XmlDocument();
                                viewString = "<root>" + viewString + "</root>";
                                xDoc.LoadXml(viewString);
                                XmlNodeList nodes = xDoc.GetElementsByTagName("FieldRef");
                                for (int i = nodes.Count - 1; i >= 0; i--)
                                {
                                    if (nodes[i].Attributes["Name"] != null)
                                    {
                                        string fieldName = nodes[i].Attributes["Name"].Value;
                                        if (mAveDoc.ParentFolder.ParentList.AveFields.FieldInternalNameMapping.ContainsKey(fieldName))
                                        {
                                            fieldName = mAveDoc.ParentFolder.ParentList.AveFields.FieldInternalNameMapping[fieldName];
                                            nodes[i].Attributes["Name"].Value = fieldName;
                                        }
                                        else if (mAveDoc.ParentFolder.ParentList.AveFields.GetField(fieldName) == null)
                                        {
                                            nodes[i].ParentNode.RemoveChild(nodes[i]);
                                        }
                                    }
                                }
                                if (xDoc.GetElementsByTagName("CalendarSettings").Count > 0)
                                {
                                    Guid webId = mAveDoc.ParentFolder.ParentList.ParentWeb.SPWeb.ID;
                                    Guid listId = mAveDoc.ParentFolder.ParentList.SPList.ID;
                                    Guid viewId = view.ID;
                                    mAveDoc.ParentSite.AddToNeedResetCalendarSettingsViews(webId, listId, viewId);
                                }
                                if (xDoc.GetElementsByTagName("GroupBy").Count > 0)
                                {
                                    XmlNode groupNode = xDoc.GetElementsByTagName("GroupBy")[0];
                                    if (groupNode.ChildNodes.Count > 1)
                                    {
                                        string firGp = groupNode.ChildNodes[0].Attributes["Name"].Value;
                                        string secGp = groupNode.ChildNodes[1].Attributes["Name"].Value;
                                        if (!string.IsNullOrEmpty(firGp) && firGp.Equals(secGp, StringComparison.OrdinalIgnoreCase))
                                        {
                                            groupNode.RemoveChild(groupNode.ChildNodes[1]);
                                        }
                                    }
                                }
                                viewString = xDoc.FirstChild.InnerXml;
                                mWebPartInfo.View = AveCompressedUtility.GetTCompressedBytes(viewString);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("An error occured while check view webpart field mapping. error:{0}", e.ToString());
                        }
                    }
                }
                else
                {
                    mLog.Log(AveLogLevel.WARN, "can't get this view, view ID:{0}, view Title:{1}", mWebPartInfo.ID, mWebPartInfo.DisplayName);
                    //mLog.Warn("can't get this view, view ID:{0}, view Title:{1}", mWebPartInfo.ID, mWebPartInfo.DisplayName);
                    return;
                }
                #endregion

                RestoreCommonProperties(webPart);

                #region not support
                ////if (!isMossWebPart)
                ////{
                //if (mWebPartInfo.AllUsersProperties != null || mWebPartInfo.PerUserProperties != null)
                //{
                //    UpdatePropertiesInDatabase(webPart.ID);
                //    if (mWebPartInfo.BaseViewID >= 0)
                //    {
                //        UpdateView(webPart.ID, mWebPartInfo.BaseViewID, mWebPartInfo.View, mWebPartInfo.ContentTypeId);
                //    }
                //    webPart = mAveWebPartManager.ReloadWebPart(webPart.ID, IsShared);
                //    //AllUsersProperties中存储了webpartID, 
                //    if (webPart == null && !string.IsNullOrEmpty(mWebPartInfo.WebPartIdProperty))
                //    {
                //        webPart = mAveWebPartManager.ReloadWebPart(mWebPartInfo.WebPartIdProperty, IsShared);
                //    }
                //    if (webPart == null)
                //    {
                //        mLog.Log(AveLogLevel.WARN, string.Format("Reload WebPart failed."));
                //        //mLog.Warn("Reload WebPart failed.");
                //        return;
                //    }
                //}
                //if (webPart is IAveWebPart)
                //{
                //    IAveWebPart tempWebpart = webPart as IAveWebPart;
                //    string authorizationFilter = tempWebpart.AuthorizationFilter;
                //    if (!string.IsNullOrEmpty(authorizationFilter))
                //    {
                //        authorizationFilter = AveAudienceManager.ReplaceAudienceId(mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.AudienceIDMapping, authorizationFilter);
                //        tempWebpart.AuthorizationFilter = authorizationFilter;
                //    }
                //}
                ////}
                #endregion

                if (webPart == null)
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("Can't get the webpart object."));
                    mLog.Warn("Can't get the webpart object.");
                    return;
                }

                #endregion

                if (mWebPartInfo.Personalization != null)
                {
                    AvePersonalizationInfo currentUserPersonalizationInfo = null;
                    System.Web.UI.WebControls.WebParts.WebPart personalWebPart = mAveWebPartManager.ReloadWebPart(webPart.ID, false);
                    foreach (AvePersonalizationInfo personInfo in mWebPartInfo.Personalization)
                    {
                        try
                        {
                            int userId = mAveDoc.ParentSite.SPMembers.FindMember(personInfo.UserID, true).ID;
                            if (userId == mAveDoc.Web.CurrentUser.ID)
                            {
                                currentUserPersonalizationInfo = personInfo;
                                continue;
                            }
                            RestorePersonalization(personalWebPart, personInfo, userId);
                            personalWebPart = mAveWebPartManager.ReloadWebPart(webPart.ID, false);
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "WP10RTWebPart285 {0} {1} {2}", mWebPartInfo.TitleUrl, mWebPartInfo.ID, e.ToString());
                        }
                    }
                    RestorePersonalization(personalWebPart, currentUserPersonalizationInfo, mAveDoc.Web.CurrentUser.ID);
                }

                UpdateWebPartByType(webPart, false);

                mAveWebPartManager.UpdateWebPart(webPart, IsShared);

                //如果源端的Properties都为null，但是目的端的不一定为null，清空一下目的端的，不然多余的数据可能导致显示不一致。
                if (mWebPartInfo.AllUsersProperties == null && mWebPartInfo.PerUserProperties == null)
                {
                    UpdatePropertiesInDatabase(webPart.ID);
                }

                if (mWebPartInfo.BaseViewID >= 0)
                {
                    UpdateView(webPart.ID, (int)mWebPartInfo.BaseViewID, mWebPartInfo.View, mWebPartInfo.ContentTypeId);
                }
                if (mWebPartInfo.UserID > 0)
                {
                    int userId = mAveDoc.ParentSite.SPMembers.FindMemberId(mWebPartInfo.UserID);
                    UpdateUserID(webPart.ID, userId, false);
                }
                if (mWebPartInfo.PageVersion != 0 && mWebPartInfo.PageVersion < mAveDoc.SPFile.UIVersion)
                {
                    UpdateWebPartInfo(webPart.ID);
                }
            }
            catch (Exception ex)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while resotre web part {0}.\n error message:{1}", AssemblyName + "|" + WebPartType, ex));
                mLog.Warn("An error occurred while resotre web part {0}, error:{1}", AssemblyName + "|" + WebPartType, ex.ToString());
            }
        }
    }

    internal class SpecialWebPartUpdater
    {
        protected System.Web.UI.WebControls.WebParts.WebPart mWebPart;
        protected AveSPDoc mAveDoc;
        protected SpecialWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
        {
            mWebPart = webPart;
            mAveDoc = aveDoc;
        }

        public static SpecialWebPartUpdater GetWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
        {
            if (webPart == null)
            {
                return null;
            }
            Type subType = Type.GetType(string.Format("AvePoint.Wrapper.Restore.{0}Updater", webPart.GetType().Name), false, true);
            if (subType == null)
            {
                return null;
            }
            return subType.GetConstructor(new Type[] { typeof(System.Web.UI.WebControls.WebParts.WebPart), typeof(AveSPDoc) }).Invoke(new object[] { webPart, aveDoc }) as SpecialWebPartUpdater;
        }

        public virtual bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo) { return true; }

        public virtual bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo) { return true; }
    }

    #region IListWebParts
    class IListWebPartUpdater : SpecialWebPartUpdater
    {
        public IListWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
            : base(webPart, aveDoc) { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            if (webPartInfo.Flags > 0)
            {
                ((IAveListWebPart)mWebPart).ViewFlags = GetViewFlags(webPartInfo.Flags);
            }
            if (webPartInfo.Type > 0)
            {
                ((IAveListWebPart)mWebPart).PageType = (AvePAGETYPE)(webPartInfo.Type);
            }
            if (webPartInfo.ListId != Guid.Empty)
            {
                ((IAveListWebPart)mWebPart).ListId = webPartInfo.ListId;
            }
            return true;
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            ((IAveListWebPart)mWebPart).ViewFlags = GetViewFlags(webPartInfo.Flags);
            if (webPartInfo.Type.HasValue)
            {
                ((IAveListWebPart)mWebPart).PageType = (AvePAGETYPE)(webPartInfo.Type);
            }
            if (!string.IsNullOrEmpty(mWebPart.TitleUrl))
            {
                string url = AveReplaceProcessor.UrlReplace(mWebPart.TitleUrl, mAveDoc.ParentSite.SiteManagedMappings, new ReplaceOption(true));
                if (!mWebPart.TitleUrl.Equals(url))
                {
                    mWebPart.TitleUrl = url;
                }
            }
            if (!string.IsNullOrEmpty(mWebPart.TitleIconImageUrl))
            {
                string iconUrl = AveReplaceProcessor.UrlReplace(mWebPart.TitleIconImageUrl, mAveDoc.ParentSite.SiteManagedMappings, new ReplaceOption(true));
                if (!mWebPart.TitleIconImageUrl.Equals(iconUrl))
                {
                    mWebPart.TitleIconImageUrl = iconUrl;
                }
            }
            if (!string.IsNullOrEmpty(mWebPart.CatalogIconImageUrl))
            {
                string catalogIconUrl = AveReplaceProcessor.UrlReplace(mWebPart.CatalogIconImageUrl, mAveDoc.ParentSite.SiteManagedMappings, new ReplaceOption(true));
                if (!mWebPart.CatalogIconImageUrl.Equals(catalogIconUrl))
                {
                    mWebPart.CatalogIconImageUrl = catalogIconUrl;
                }
            }
            return true;
        }

        public AveViewFlags GetViewFlags(int flags)
        {
            AveViewFlags flag = AveViewFlags.None;
            foreach (AveViewFlags viewFlag in Enum.GetValues(typeof(AveViewFlags)))
            {
                if ((flags & (int)viewFlag) != 0)
                {
                    flag = flag | viewFlag;
                }
            }
            return flag;
        }
    }

    class AveListViewWebPartUpdater : IListWebPartUpdater
    {
        public AveListViewWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
            : base(webPart, aveDoc) { }
        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {

            Guid listId = webPartInfo.ListId;
            if (mAveDoc.ParentSite.ListIdMapping.ContainsKey(listId))
            {
                listId = mAveDoc.ParentSite.ListIdMapping[listId];
            }
            IAveListViewWebPart part = mWebPart as IAveListViewWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
            {
                part.WebId = webPartInfo.WebPartList[0].WebId;
            }
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            Guid listId = webPartInfo.ListId;
            if (mAveDoc.ParentSite.ListIdMapping.ContainsKey(listId))
            {
                listId = mAveDoc.ParentSite.ListIdMapping[listId];
            }
            IAveListViewWebPart part = mWebPart as IAveListViewWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
            {
                part.WebId = webPartInfo.WebPartList[0].WebId;
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveXsltListViewWebPartUpdater : IListWebPartUpdater
    {
        public AveXsltListViewWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
            : base(webPart, aveDoc) { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {

            Guid listId = webPartInfo.ListId;
            IAveXsltListViewWebPart part = mWebPart as IAveXsltListViewWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
            {
                part.WebId = webPartInfo.WebPartList[0].WebId;
            }
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            Guid listId = webPartInfo.ListId;
            IAveXsltListViewWebPart part = mWebPart as IAveXsltListViewWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
            {
                part.WebId = webPartInfo.WebPartList[0].WebId;
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    // add the method for DataFormWebPart update
    class AveDataFormWebPartUpdater : IListWebPartUpdater
    {
        public AveDataFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
            : base(webPart, aveDoc) { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {

            Guid listId = webPartInfo.ListId;
            IAveDataFormWebPart part = mWebPart as IAveDataFormWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            part.ListId = listId;
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            UpdateDataFormWebPartBinding();
            if (webPartInfo != null)
            {
                //Guid listId = webPartInfo.ListId;
                //DataFormWebPart part = mWebPart as DataFormWebPart;
                //part.ListName = listId.ToString("B").ToUpper();
                //part.ListId = listId;

                //XmlDocument xmlDoc = new XmlDocument();
                //xmlDoc.InnerXml = "<t>" + part.ParameterBindings + "</t>";
                //XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("//ParameterBinding[@Name='ListID']");
                //foreach (XmlNode node in nodes)
                //{
                //    (node as XmlElement).SetAttribute("DefaultValue", listId.ToString("B").ToUpper());
                //}
                //part.ParameterBindings = xmlDoc.FirstChild.InnerXml;

                return base.DoUpateAfterAdd(webPartInfo);
            }
            return true;
        }
        //DOC-64411 DOC-62257
        private void UpdateDataFormWebPartBinding()
        {
            try
            {

                IAveDataFormWebPart mDataFormWP = this.mWebPart as IAveDataFormWebPart;
                string dataSourceString = mDataFormWP.DataSourcesString;
                if (String.IsNullOrEmpty(dataSourceString)) //There is exception when the DataSourceString is string.Empty
                {
                    return;
                }
                XmlDocument docDataSource = new XmlDocument();
                Dictionary<string, string> dataSourceDict = new Dictionary<string, string>();
                dataSourceString = dataSourceString.Substring(dataSourceString.LastIndexOf("%>") + 2);
                dataSourceString = dataSourceString.Replace(':', '_');
                docDataSource.LoadXml("<root>" + dataSourceString + "</root>");
                foreach (XmlElement node in docDataSource.GetElementsByTagName("webpartpages_DataFormParameter"))
                {
                    string strName = node.GetAttribute("ParameterKey");
                    if (!String.IsNullOrEmpty(strName) && !dataSourceDict.ContainsKey(strName))
                    {
                        dataSourceDict.Add(strName, node.GetAttribute("DefaultValue"));
                    }
                }
                if (dataSourceDict.ContainsKey("ListID"))
                {
                    #region link to subsite list
                    Guid oldDataSourceListID = new Guid(dataSourceDict["ListID"]);
                    string oldDataSourceWebURL = dataSourceDict.ContainsKey("WebURL") ? dataSourceDict["WebURL"].TrimEnd('/', '\\') : null;
                    if (oldDataSourceListID.Equals(Guid.Empty))
                    {
                        return;
                    }
                    if (!mAveDoc.ParentSite.ListIdMapping.ContainsKey(oldDataSourceListID))
                    {
                        mAveDoc.ParentSite.AddUnRestoreWebPartInfo(mAveDoc.Web.ID, oldDataSourceListID, mAveDoc.SPFile.UniqueId, this.mWebPart.ID);
                        return;
                    }

                    string bindingString = mDataFormWP.ParameterBindings;
                    if (string.IsNullOrEmpty(bindingString))
                    {
                        return;
                    }
                    XmlDocument docBinding = new XmlDocument();
                    Dictionary<string, Dictionary<string, string>> bindingDict = new Dictionary<string, Dictionary<string, string>>();
                    docBinding.LoadXml("<root>" + bindingString + "</root>");
                    foreach (XmlElement node in docBinding.GetElementsByTagName("ParameterBinding"))
                    {
                        string strName = node.GetAttribute("Name");
                        if (!String.IsNullOrEmpty(strName))
                        {
                            Dictionary<string, string> tempDict = new Dictionary<string, string>();
                            tempDict.Add("DefaultValue", node.GetAttribute("DefaultValue"));
                            tempDict.Add("Location", node.GetAttribute("Location"));
                            if (!bindingDict.ContainsKey(strName))
                            {
                                bindingDict[strName] = tempDict;
                            }
                        }
                    }


                    string oldBindingWebURL = null;

                    if (bindingDict.ContainsKey("WebURL"))//DOC-64411
                    {
                        string curWebUrl = oldDataSourceWebURL;
                        oldBindingWebURL = bindingDict["WebURL"]["DefaultValue"].TrimEnd('/', '\\');
                        if (oldBindingWebURL != null && mAveDoc.ParentSite.WebUrlMapping.ContainsKey(oldDataSourceWebURL))
                        {
                            curWebUrl = mAveDoc.ParentSite.WebUrlMapping[oldDataSourceWebURL];
                            mDataFormWP.ParameterBindings = mDataFormWP.ParameterBindings.Replace(oldBindingWebURL, curWebUrl);
                            mDataFormWP.DataSourcesString = mDataFormWP.DataSourcesString.Replace(oldDataSourceWebURL, curWebUrl);
                        }
                    }
                    if (bindingDict.ContainsKey("ListID"))
                    {
                        Guid curListID = oldDataSourceListID;
                        Guid oldBindingListID = new Guid(bindingDict["ListID"]["DefaultValue"]);
                        Guid curWebID = oldDataSourceWebURL != null ? mAveDoc.ParentSite.GetMappingWeb(oldDataSourceWebURL, true) : mAveDoc.ParentSite.GetMappingList(mAveDoc.Web.ID, string.Empty, oldDataSourceListID);
                        curListID = mAveDoc.ParentSite.GetMappingList(curWebID, string.Empty, oldDataSourceListID);
                        mDataFormWP.ParameterBindings = mDataFormWP.ParameterBindings.Replace(oldBindingListID.ToString().ToUpper(), curListID.ToString().ToUpper());
                        mDataFormWP.DataSourcesString = mDataFormWP.DataSourcesString.Replace(oldDataSourceListID.ToString().ToUpper(), curListID.ToString().ToUpper());
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    class AveXsltListFormWebPartUpdater : IListWebPartUpdater
    {
        public AveXsltListFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
            : base(webPart, aveDoc) { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            Guid listId = webPartInfo.ListId;
            IAveXsltListFormWebPart part = mWebPart as IAveXsltListFormWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
            {
                part.WebId = webPartInfo.WebPartList[0].WebId;
            }
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            Guid listId = webPartInfo.ListId;
            IAveXsltListFormWebPart part = mWebPart as IAveXsltListFormWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
            {
                part.WebId = webPartInfo.WebPartList[0].WebId;
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveListFormWebPartUpdater : IListWebPartUpdater
    {
        public AveListFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
            : base(webPart, aveDoc) { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            Guid listId = webPartInfo.ListId;
            IAveListFormWebPart part = mWebPart as IAveListFormWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            Guid listId = webPartInfo.ListId;
            IAveListFormWebPart part = mWebPart as IAveListFormWebPart;
            part.ListName = listId.ToString("B").ToUpper();
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveBrowserFormWebPartUpdater : IListWebPartUpdater
    {
        public AveBrowserFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
            : base(webPart, aveDoc) { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }
    #endregion

    //class ChartWebPartUpdater : SpecialWebPartUpdater
    //{
    //    private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

    //    public ChartWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }

    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        Type webpartType = mWebPart.GetType();
    //        IList dataBindings = AveAssemblyUtility.GetPropertyValue(mWebPart, "DataBindings") as IList;
    //        foreach (object dataBinding in dataBindings)
    //        {
    //            try
    //            {
    //                object dataSource = AveAssemblyUtility.GetPropertyValue(dataBinding, "DataSource");
    //                if (dataSource != null)
    //                {
    //                    switch (dataSource.GetType().Name)
    //                    {
    //                        case "DataSourceWebList":
    //                            string webName = AveAssemblyUtility.GetPropertyValue(dataSource, "SiteName") as string;
    //                            string listTitle = AveAssemblyUtility.GetPropertyValue(dataSource, "ListTitle") as string;
    //                            string listUrl = AveAssemblyUtility.GetPropertyValue(dataSource, "ListUrl") as string;
    //                            string dataProviderPageUrl = AveAssemblyUtility.GetPropertyValue(dataSource, "DataProviderPageUrl") as string;
    //                            listUrl = AveReplaceProcessor.UrlReplace(listUrl, mAveDoc.AveSite.SiteManagedMappings, new ReplaceOption(true));
    //                            AveAssemblyUtility.SetPropertyValue(dataSource, "SiteName", AveReplaceProcessor.UrlReplace(webName, mAveDoc.AveSite.WebUrlMapping, new ReplaceOption(true)));
    //                            AveAssemblyUtility.SetPropertyValue(dataSource, "DataProviderPageUrl", AveReplaceProcessor.UrlReplace(dataProviderPageUrl, mAveDoc.AveSite.SiteManagedMappings, new ReplaceOption(true, true)));
    //                            AveAssemblyUtility.SetPropertyValue(dataSource, "ListTitle", AveReplaceProcessor.UrlReplace(listTitle, mAveDoc.AveSite.ListUrlMapping, new ReplaceOption(true)));
    //                            AveAssemblyUtility.SetPropertyValue(dataSource, "ListUrl", listUrl);
    //                            AveAssemblyUtility.SetPropertyValue(mWebPart, "ListUrl", listUrl);
    //                            break;
    //                        default:
    //                            break;
    //                    }
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                mLog.Log(AveLogSeverity.Warn, string.Format("An error occurred while do update after add. webpart titleUrl:{0}, webpart ID:{1}\n error message:{2}", webPartInfo.TitleUrl, webPartInfo.ID, e));
    //            }
    //        }
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //}

    //class ClientApplicationWebPartBaseUpdater : SpecialWebPartUpdater
    //{
    //    public ClientApplicationWebPartBaseUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }

    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        ClientApplicationWebPartBase webPart = mWebPart as ClientApplicationWebPartBase;
    //        string url = webPart.Url;
    //        List<Dictionary<string,string>> mappings = new List<Dictionary<string,string>>();
    //        mappings.Add(mAveDoc.AveSite.SiteUrlMapping);
    //        mappings.Add(mAveDoc.AveSite.WebUrlMapping);
    //        mappings.Add(mAveDoc.AveSite.ListUrlMapping);
    //        webPart.Url = AveReplaceProcessor.UrlReplace(url, mappings, new ReplaceOption(true));
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //}

    //class SilverlightWebPartUpdater : ClientApplicationWebPartBaseUpdater
    //{
    //    public SilverlightWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }

    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //}

    //class PictureLibrarySlideshowWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public PictureLibrarySlideshowWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }

    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        PictureLibrarySlideshowWebPart webPart = mWebPart as PictureLibrarySlideshowWebPart;
    //        Guid listId = webPart.LibraryGuid;
    //        if (!mAveDoc.AveSite.ListIdMapping.ContainsKey(listId))
    //        {
    //            mAveDoc.AveSite.AddUnRestoreWebPartInfo(mAveDoc.Web.ID, listId, mAveDoc.SPFile.UniqueId, mWebPart.ID);
    //            return false;
    //        }
    //        SPList list = mAveDoc.Web.Lists[mAveDoc.AveSite.ListIdMapping[listId]];
    //        webPart.LibraryGuid = list.ID;
    //        webPart.ViewGuid = list.DefaultView.ID;
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //}

    //class MediaWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public MediaWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }

    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        MediaWebPart webPart = mWebPart as MediaWebPart;
    //        if (webPart.MediaSource != null)
    //        {
    //            string oldUrl = webPart.MediaSource;
    //            webPart.MediaSource = AveReplaceProcessor.UrlReplace(oldUrl, mAveDoc.AveSite.SiteManagedMappings, new ReplaceOption(true));
    //        }
    //        if (webPart.PreviewImageSource != null)
    //        {
    //            string oldUrl = webPart.PreviewImageSource;
    //            webPart.PreviewImageSource = AveReplaceProcessor.UrlReplace(oldUrl, mAveDoc.AveSite.SiteManagedMappings, new ReplaceOption(true));
    //        }
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //}

    //class ContentEditorWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public ContentEditorWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }

    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        ContentEditorWebPart webPart = mWebPart as ContentEditorWebPart;
    //        if (webPart.Content != null)
    //        {
    //            XmlElement xe = webPart.Content;
    //            webPart.Content = ReplaceContentLinks(xe);
    //        }
    //        if (webPart.ContentLink != null) // add this for replace the ContentLink  url
    //        {
    //            webPart.ContentLink = AveReplaceProcessor.UrlReplace(webPart.ContentLink, mAveDoc.AveSite.SiteManagedMappings, new ReplaceOption(true));
    //        }
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //    private XmlElement ReplaceContentLinks(XmlElement xe)
    //    {
    //        try
    //        {
    //            foreach (XmlNode node in xe.GetElementsByTagName("a"))
    //            {
    //                node.Attributes["href"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["href"].Value,mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
    //            }
    //            foreach (XmlNode node in xe.GetElementsByTagName("img"))
    //            {
    //                node.Attributes["src"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["src"].Value, mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
    //            }
    //            foreach (XmlNode node in xe.ChildNodes)
    //            {
    //                if (node.NodeType == XmlNodeType.CDATA)
    //                {
    //                    string innerText = AveReplaceProcessor.ReplaceStringLinks(node.InnerText, mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true, true));
    //                    node.InnerText = innerText;
    //                }
    //            }
    //            return xe;
    //        }
    //        catch (Exception e)
    //        {
    //            return xe;
    //        }
    //    }
    //}

    //class ContentByQueryWebPartUpdater : DataFormWebPartUpdater //DOC-65944     Replace The TitleIconImageUrl of ContentByQueryWebPart
    //{
    //    public ContentByQueryWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }

    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        ContentByQueryWebPart webPart = mWebPart as ContentByQueryWebPart;
    //        if (!string.IsNullOrEmpty(webPart.ListGuid))//先判断list是否存在，如果不存在，会在postaction中处理。如果先处理weburl，会导致替换两次，root site会出错
    //        {
    //            Guid listId = new Guid(webPart.ListGuid.Trim());
    //            if (!mAveDoc.AveSite.ListIdMapping.ContainsKey(listId))
    //            {
    //                mAveDoc.AveSite.AddUnRestoreWebPartInfo(mAveDoc.Web.ID, listId, mAveDoc.SPFile.UniqueId, mWebPart.ID);
    //                return false;
    //            }
    //            webPart.ListGuid = mAveDoc.AveSite.ListIdMapping[listId].ToString();
    //        }
    //        if (!string.IsNullOrEmpty(webPart.WebUrl))
    //        {
    //            if (!webPart.WebUrl.StartsWith("~sitecollection"))
    //            {
    //                webPart.WebUrl = AveReplaceProcessor.UrlReplace(webPart.WebUrl, mAveDoc.AveSite.SiteManagedMappings, new ReplaceOption(true));
    //            }
    //        }
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //}

    //class SummaryLinkWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public SummaryLinkWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }
    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        SummaryLinkWebPart webPart = mWebPart as SummaryLinkWebPart;
    //        if (!string.IsNullOrEmpty(webPart.SummaryLinkStore))
    //        {
    //            string value = AveReplaceProcessor.ReplaceUrlInXml(webPart.SummaryLinkStore, mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
    //            webPart.SummaryLinkValue = new Microsoft.SharePoint.Publishing.Fields.SummaryLinkFieldValue(value);
    //            webPart.SummaryLinkStore = value;
    //        }
    //        if (webPart.ManagedLinks != null)
    //        {
    //            for (int i = 0; i < webPart.ManagedLinks.Count; i++)
    //            {
    //                string link = webPart.ManagedLinks[i] as string;
    //                if (!string.IsNullOrEmpty(link))
    //                {
    //                    webPart.ManagedLinks[i] = AveReplaceProcessor.UrlReplace(link, mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
    //                }
    //            }
    //        }
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //}

    //class WhereaboutsWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public WhereaboutsWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }
    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        WhereaboutsWebPart webPart = mWebPart as WhereaboutsWebPart;
    //        bool result = true;
    //        if (webPart.EventListId != Guid.Empty)
    //        {
    //            if (!mAveDoc.AveSite.ListIdMapping.ContainsKey(webPart.EventListId))
    //            {
    //                mAveDoc.AveSite.AddUnRestoreWebPartInfo(mAveDoc.Web.ID, webPart.EventListId, mAveDoc.SPFile.UniqueId, mWebPart.ID);
    //                result = false;
    //            }
    //            else
    //            {
    //                webPart.EventListId = mAveDoc.AveSite.ListIdMapping[webPart.EventListId];
    //            }
    //        }
    //        if (webPart.CallTrackingListId != Guid.Empty)
    //        {
    //            if (!mAveDoc.AveSite.ListIdMapping.ContainsKey(webPart.CallTrackingListId))
    //            {
    //                mAveDoc.AveSite.AddUnRestoreWebPartInfo(mAveDoc.Web.ID, webPart.CallTrackingListId, mAveDoc.SPFile.UniqueId, mWebPart.ID);
    //                result = false;
    //            }
    //            else
    //            {
    //                webPart.CallTrackingListId = mAveDoc.AveSite.ListIdMapping[webPart.CallTrackingListId];
    //            }
    //        }
    //        return result;
    //    }

    //}

    //class TableOfContentsWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public TableOfContentsWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc) : base(webPart, aveDoc) { }
    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        try
    //        {
    //            Type objType = mWebPart.GetType();
    //            PropertyInfo property = objType.GetProperty("AnchorLocation", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //            string oldAnchorLocation = Convert.ToString(property.GetValue(mWebPart, null));
    //            string newAnchorLocation = AveReplaceProcessor.UrlReplace(oldAnchorLocation, mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
    //            property.SetValue(mWebPart, newAnchorLocation, null);
    //            return base.DoUpateAfterAdd(webPartInfo);
    //        }
    //        catch
    //        {
    //            //
    //            return false;
    //        }
    //    }
    //}

    //class ContactFieldControlUpdater : SpecialWebPartUpdater
    //{
    //    public ContactFieldControlUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc) : base(webPart, aveDoc) { }
    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        try
    //        {
    //            Type objType = mWebPart.GetType();
    //            PropertyInfo property = objType.GetProperty("Contact", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //            int oldMemberID = Convert.ToInt32(property.GetValue(mWebPart, null));
    //            int newMemberID = -1;
    //            if (oldMemberID > 0)
    //            {
    //                if (mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SPMembers.SiteUserIDMapping.ContainsKey(oldMemberID))
    //                {
    //                    object obj = mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SPMembers.SiteUserIDMapping[oldMemberID];
    //                    if (obj is AveSPMemberInfo)
    //                    {
    //                        AveSPMemberInfo member = obj as AveSPMemberInfo;
    //                        newMemberID = member.NewId;
    //                    }
    //                }
    //                if (newMemberID > 0)
    //                {
    //                    property.SetValue(mWebPart, newMemberID, null);
    //                }
    //            }
    //            return base.DoUpateAfterAdd(webPartInfo);
    //        }
    //        catch
    //        {
    //            //
    //            return false;
    //        }
    //    }
    //}

    //class TasksAndToolsWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public TasksAndToolsWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }
    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        try
    //        {
    //            Type objType = mWebPart.GetType();
    //            PropertyInfo property = objType.GetProperty("TasksAndToolsWebUrl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //            string tasksAndToolsWebUrl = Convert.ToString(property.GetValue(mWebPart, null));
    //            if (!String.IsNullOrEmpty(tasksAndToolsWebUrl))
    //            {
    //                tasksAndToolsWebUrl = AveReplaceProcessor.UrlReplace(tasksAndToolsWebUrl, mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
    //                property.SetValue(mWebPart, tasksAndToolsWebUrl, null);
    //            }
    //            return base.DoUpateAfterAdd(webPartInfo);
    //        }
    //        catch
    //        {
    //            //
    //            return false;
    //        }
    //    }
    //}

    //class KPIListWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public KPIListWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveSPDoc aveDoc)
    //        : base(webPart, aveDoc) { }
    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        try
    //        {
    //            Type objType = mWebPart.GetType();
    //            PropertyInfo property = objType.GetProperty("ListURL", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //            string listURL = Convert.ToString(property.GetValue(mWebPart, null));
    //            if (!string.IsNullOrEmpty(listURL))
    //            {
    //                listURL = AveReplaceProcessor.UrlReplace(listURL, mAveDoc.ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
    //                property.SetValue(mWebPart, listURL, null);
    //                //TitleUrl和listURL关联,如果不在此给TitleUrl设置成empty,在update的时候会抛出异常。
    //                if (listURL.Equals(mWebPart.TitleUrl))
    //                {
    //                    mWebPart.TitleUrl = string.Empty;
    //                }
    //            }
    //            return base.DoUpateAfterAdd(webPartInfo);
    //        }
    //        catch
    //        {
    //            //
    //            return false;
    //        }
    //    }
    //}
}