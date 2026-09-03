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
using System.Text;
using System.Xml;
using System.Linq;

using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using LS.SPWorkflowProcessor.Common;
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Restore.NintexForm;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.SPWorkflowProcessor
{
    internal enum SPWorkflowFileContentProcType
    {
        Invalid,
        Config,
        Xoml,
        Rules,
        Aspx,
        Xaml
    }

    public class SPWorkflowFileContentCustomProc
    {
        private SPWorkflowFileContentProc mConfigFileProc;
        private SPWorkflowFileContentProc mXomlFileProc;
        private SPWorkflowFileContentProc mRulesFileProc;
        private SPWorkflowFileContentProc mAspxFileProc;
        private SPWorkflowFileContentProc mXamlFileProc;

        public SPWorkflowFileContentProc ConfigFileProcessor
        {
            get { return mConfigFileProc; }
            set { mConfigFileProc = value; }
        }

        public SPWorkflowFileContentProc XomlFileProcessor
        {
            get { return mXomlFileProc; }
            set { mXomlFileProc = value; }
        }

        public SPWorkflowFileContentProc RulesFileProcessor
        {
            get { return mRulesFileProc; }
            set { mRulesFileProc = value; }
        }

        public SPWorkflowFileContentProc AspxFileProcessor
        {
            get { return mAspxFileProc; }
            set { mAspxFileProc = value; }
        }

        public SPWorkflowFileContentProc XamlFileProcessor
        {
            get { return mXamlFileProc; }
            set { mXamlFileProc = value; }
        }
    }

    /// <summary>
    /// todo:wbhu,结构需要重构，基类承担了部分10Mode workflow template file处理的职责,重构后此类只负责数据替换
    /// </summary>
    public class SPWorkflowFileContentProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected IAveFile mFile = null;
        public IAveFile SPFile
        {
            get { return mFile; }
        }
        protected byte[] mOriginalContent = null;
        public byte[] OriginalContent
        {
            get
            {
                return mOriginalContent;
            }
        }

        public WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// 外围设置的，不会改。safe
        /// </summary>
        private static Dictionary<Guid, SPWorkflowFileContentCustomProc> mCustomProcs;
        public static Dictionary<Guid, SPWorkflowFileContentCustomProc> CustomContentProcessors
        {
            get
            {
                if (mCustomProcs == null)
                    mCustomProcs = new Dictionary<Guid, SPWorkflowFileContentCustomProc>();
                return mCustomProcs;
            }
            set
            {
                mCustomProcs = value;
            }
        }

        private static SPWorkflowFileContentProc GetInstance(SPWFAssociationUnit assoUnit, SPWorkflowFileContentProcType procType)
        {
            var workflowType = assoUnit.WorkflowType;
            var baseId = assoUnit.SerializableData.mBaseId;
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWorkflowFileContentProc.GetInstance");
            SPWorkflowFileContentProc instance = null;
            SPWorkflowFileContentCustomProc customProc = null;
            if (CustomContentProcessors.ContainsKey(baseId))
                customProc = CustomContentProcessors[baseId];
            else if (CustomContentProcessors.ContainsKey(Guid.Empty))
                customProc = CustomContentProcessors[Guid.Empty];

            if (customProc != null)
            {
                switch (procType)
                {
                    case SPWorkflowFileContentProcType.Config:
                        if (customProc.ConfigFileProcessor != null)
                        {
                            instance = customProc.ConfigFileProcessor;
                            var configFileProcessor = instance as ConfigFileProc;
                            if(configFileProcessor!= null)
                            {
                                configFileProcessor.AssociationUnit = assoUnit;
                            }
                        }
                        else
                            instance = new ConfigFileProc(assoUnit);
                        break;
                    case SPWorkflowFileContentProcType.Xoml:
                        if (customProc.XomlFileProcessor != null)
                            instance = customProc.XomlFileProcessor;
                        else
                            instance = new XomlFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Rules:
                        if (customProc.RulesFileProcessor != null)
                            instance = customProc.RulesFileProcessor;
                        else
                            instance = new RulesFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Aspx:
                        if (customProc.AspxFileProcessor != null)
                            instance = customProc.AspxFileProcessor;
                        else
                            instance = new AspxFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Xaml:
                        if (customProc.XamlFileProcessor != null)
                            instance = customProc.XamlFileProcessor;
                        else
                            instance = new XamlFileProc();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (procType)
                {
                    case SPWorkflowFileContentProcType.Config:
                        instance = new ConfigFileProc(assoUnit);
                        break;
                    case SPWorkflowFileContentProcType.Xoml:
                        instance = new XomlFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Rules:
                        instance = new RulesFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Aspx:
                        instance = new AspxFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Xaml:
                        instance = new XamlFileProc();
                        break;
                    default:
                        break;
                }
            }
            if (instance != null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.FileContentProc_CustomProcAssemblyName, procType.ToString(), instance.GetType().Assembly.FullName);
                instance.WorkflowType = workflowType;
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SPWorkflowFileContentProc.GetInstance");
            return instance;
        }

        public static SPWorkflowFileContentProc CreateInstance(SPWFAssociationUnit assoUnit, IAveFile spFile, byte[] content)
        {
            SPWorkflowFileContentProc instance = null;
            string extension = spFile == null ? "xaml" : spFile.GetExtension().ToLower(CultureInfo.CurrentCulture);
            switch (extension)
            {
                case "xml":
                    instance = GetInstance(assoUnit, SPWorkflowFileContentProcType.Config);
                    break;
                case "xoml":
                    instance = GetInstance(assoUnit, SPWorkflowFileContentProcType.Xoml);
                    break;
                case "rules":
                    instance = GetInstance(assoUnit, SPWorkflowFileContentProcType.Rules);
                    break;
                case "aspx":
                    instance = GetInstance(assoUnit, SPWorkflowFileContentProcType.Aspx);
                    break;
                case "xaml":
                    instance = GetInstance(assoUnit, SPWorkflowFileContentProcType.Xaml);
                    instance.mOriginalContent = content;
                    break;
                case "":
                    instance = GetInstance(assoUnit, SPWorkflowFileContentProcType.Xoml);
                    break;
                default:
                    //throw new Exception("Not supported.");
                    break;
            }
            if (instance != null)
                instance.mFile = spFile;
            return instance;
        }

        public static SPWorkflowFileContentProc CreateInstance(SPWFAssociationUnit assoUnit, IAveFile spFile)
        {
            return CreateInstance(assoUnit, spFile, null);
        }

        public virtual string ReplaceContent(Dictionary<string, object> dic)
        {
            string strContent = string.Empty;
            if (mFile != null)
            {
                using (StreamReader objReader = new StreamReader(mFile.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions)))
                {
                    strContent = objReader.ReadToEnd();
                }

                foreach (KeyValuePair<string, object> pair in dic)
                {
                    int replacedCount = 0;
                    strContent = LSUtilityOfBytes.LSReplaceStringIgnoreCase(strContent, pair.Key, pair.Value.ToString(), int.MaxValue, out replacedCount);
                }
            }
            return strContent;
        }

        public static string ConvertBytesToString(byte[] contentBytes)
        {
            string strContent = string.Empty;
            try
            {
                using (MemoryStream stream = new MemoryStream(contentBytes))
                {
                    using (StreamReader objReader = new StreamReader(stream))
                    {
                        strContent = objReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.LoadXmlContentError, ex);
            }
            return strContent;
        }

        public string ReplaceNintexContentTypeID(string content)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);
            XmlNodeList nodes = xmlDoc.SelectNodes(".//*[@TaskContentTypeId != '']");
            foreach (XmlNode node in nodes)
            {
                if (node.Name.Equals("ns1:ApprovalActivityInternal2"))
                {

                    try
                    {
                        XmlElement xe = (XmlElement)node;
                        string ContentTypeID = xe.GetAttribute("TaskContentTypeId");
                        xe.SetAttribute("TaskContentTypeId", SPWorkflowProcessorRuntime.MappingManager.WebMappingManager.WebLevelCTIdMapping[ContentTypeID].ToString());
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, "{0}", e);
                    }
                }
            }
            #region Replace content type id for create item action.
            nodes = xmlDoc.SelectNodes(".//*[@ContentType != '']");
            foreach (XmlNode node in nodes)
            {
                if (node.Name.Equals("ns1:CreateItemWithContentTypesActivity", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        XmlElement xe = (XmlElement)node;
                        string contentTypeId = xe.GetAttribute("ContentType");
                        string listIdString = xe.GetAttribute("ListId");
                        if (string.IsNullOrEmpty(contentTypeId) || string.IsNullOrEmpty(listIdString))
                        {
                            continue;
                        }
                        listIdString = listIdString.Trim(new char[] { '{', '}' });
                        if (!AveTypeHelper.IsGuid(listIdString))
                        {
                            continue;
                        }
                        Guid listId = new Guid(listIdString);
                        logger.Debug("Destination ListId : {0}", listId);
                        IAveContentTypeId destinationContentTypeId;
                        if (SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.TryGetValueFromListLevelContentTypeIdMapping(listId, contentTypeId, out destinationContentTypeId))
                        {
                            logger.Debug("Replace content type id in create item action. SourceId : {0}, DestinationID: {1}", contentTypeId, destinationContentTypeId);
                            xe.SetAttribute("ContentType", destinationContentTypeId.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Replace content type id in xoml file failed. Error: {0}", e);
                    }
                }
            }
            #endregion
            content = xmlDoc.OuterXml;
            return content;
        }

        /// <summary>
        /// 有一些特殊的action不需要替换user，在此处过滤
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public virtual bool NeedReplaceUser(XmlElement node, string attributeName)
        {
            bool needReplace = true;
            if (node != null && node.Name != null)
            {
                if (string.Equals(attributeName, "UsernameCredentials", StringComparison.OrdinalIgnoreCase))
                {
                    if (node.Name.IndexOf("UpdateUserProfileActivity", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        needReplace = false;
                    }
                }
                else if (string.Equals(attributeName, "Username", StringComparison.OrdinalIgnoreCase))
                {
                    if (node.Name.IndexOf("CreateAudienceActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("CompileAudienceActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("QueryUserProfileStoreActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("DeleteAudienceActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("GetMeetingSuggestionsActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("QueryExcelWebServiceActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("RunSqlActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("ExchangeCreateTaskActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("BDCQueryActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("CopyToFolderActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("CreateCRMEntityRecordActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("DeleteCRMEntitiesActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("QueryCRMEntitiesActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("UpdateCRMRecordActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("WebRequestActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("QueryLdapActivity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        node.Name.IndexOf("SendReceiveMessageActivity", StringComparison.OrdinalIgnoreCase) >= 0)
                    //ExchangeCreateTaskActivity
                    //BDCQueryActivity
                    //CopyToFolderActivity
                    {
                        needReplace = false;
                    }
                }
            }
            else
            {
                needReplace = false;
            }
            return needReplace;
        }

        public virtual string ReplaceUserInNintexWorkflow(string content)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);
            ReplaceNintexUserInXmlElement(xmlDoc);
            //XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            //nsmgr.AddNamespace("ns3", "clr-namespace:Nintex.Workflow.HumanApproval;Assembly=Nintex.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=913f6bae0ca5ae12");
            //XmlNodeList nodes = xmlDoc.SelectNodes(".//*[@User!='' or @UserID!='']", nsmgr);

            string attributeName = "Permissions";
            XmlNodeList nodes = xmlDoc.SelectNodes(".//*[@Permissions!='']");
            foreach (XmlNode node in nodes)
            {
                try
                {
                    XmlElement xe = (XmlElement)node;
                    if (xe.HasAttribute(attributeName))
                    {
                        string value = xe.GetAttribute(attributeName);
                        var userCollections = Regex.Split(value, "##", RegexOptions.IgnoreCase);
                        foreach (var userValue in userCollections)
                        {
                            string[] names = Regex.Split(userValue, ";#", RegexOptions.IgnoreCase);
                            for (int i = 0; i < names.Length; i++)
                            {
                                if (names[i].Contains("@"))
                                {
                                    string newName = SPWorkflowCommon.OnModifyEmailAddress(null, names[i]);
                                    if (!newName.Equals(names[i]))
                                    {
                                        value = value.Replace(names[i], newName);
                                        continue;
                                    }
                                }
                                if (names[i].Contains("\\") && NeedReplaceUser(xe, attributeName))
                                {
                                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(names[i]);
                                    if (user != null)
                                    {
                                        value = value.Replace(names[i], user.LoginName);
                                    }
                                }
                            }
                        }
                        xe.SetAttribute(attributeName, value);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.NintexReplaceUserError, e);
                }
            }

            content = xmlDoc.OuterXml;
            return content;
        }

        /// <summary>
        /// 需要替换url的Nintex workflow node name,不区分大小写
        /// </summary>
        private static Dictionary<string, string> NintexNeedReplaceUrlNodeNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"ns1:CreateWeb2Activity","ns1:CreateWeb2Activity"},
            {"ns1:CreateList2Activity","ns1:CreateList2Activity"},
            {"ns1:UpdateMultipleItemActivity","ns1:UpdateMultipleItemActivity"},
            {"ns1:DeleteMultipleItemActivity","ns1:DeleteMultipleItemActivity"},
            {"ns1:CreateSiteSpecificItemActivity","ns1:CreateSiteSpecificItemActivity"},
            {"ns1:DeleteSiteCollectionActivity","ns1:DeleteSiteCollectionActivity"},
            {"ns1:CreateSiteCollectionActivity","ns1:CreateSiteCollectionActivity"},
            {"ns1:DeleteWeb2Activity","ns1:DeleteWeb2Activity"},
            {"ns1:CopyToSharepointSite2Activity","ns1:CopyToSharepointSite2Activity"},
            {"ns1:ContextDataActivity","ns1:ContextDataActivity"},
            {"ns1:ConvertDocumentActivity","ns1:ConvertDocumentActivity"},
            {"ns1:ProvisionExchangeUserEmailActivity","ns1:ProvisionExchangeUserEmailActivity"},
            {"ns2:Message","ns2:Message"},
            {"ns1:ReadDocumentActivity","ns1:ReadDocumentActivity"},
            {"ns1:ExchangeCreateTaskActivity","ns1:ExchangeCreateTaskActivity"},
            {"ns1:UpdateDocumentActivity","ns1:UpdateDocumentActivity"},
            {"ns1:WebRequestActivity","ns1:WebRequestActivity"},
        };

        /// <summary>
        /// 需要替换url的Nintex workflow attribute name，需要区分大小写
        /// </summary>
        private static Dictionary<string, string> NintexNeedReplaceUrlAttributeNames = new Dictionary<string, string>()
        {
           {"ParentWebUrl","ParentWebUrl"},
           {"Url","Url"},
           {"HiddenUrl","HiddenUrl"},
           {"SiteUrl","SiteUrl"},
           {"WebApplicationUrl","WebApplicationUrl"},
           {"OutputUrl","OutputUrl"},
           {"InputUrl","InputUrl"},
           {"Input","Input"},
           { "UrlPath","UrlPath"},
           { "ExchangeServiceUrl","ExchangeServiceUrl"}
        };

        /// <summary>
        /// 需要替换url的Nintex workflow rich text attribute name，需要区分大小写
        /// </summary>
        private static Dictionary<string, string> NintexNeedReplaceUrlRichTextAttributeNames = new Dictionary<string, string>()
        {
           {"Body","Body"}
        };

        public virtual string ReplaceUrlInNintexWorkflow(string content)
        {
            if (SPWorkflowProcessorRuntime.MappingManager == null || SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager == null ||
                SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.DestSiteInfo == null)
            {
                return content;
            }
            try
            {
                AveSiteMappingManager siteMappingManager = SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager;
                string destSiteUrl = siteMappingManager.DestSiteInfo.ServerRelativeUrl;
                ReplaceOption option = new ReplaceOption(true, true, true);
                AveWorkflowReplaceProcessor processor = new AveWorkflowReplaceProcessor(siteMappingManager.SiteManagedMappings, option, siteMappingManager.SourceSiteInfo, destSiteUrl);

                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.InnerXml = content;
                foreach (XmlNode node in xDoc.GetElementsByTagName("*"))
                {
                    if (NintexNeedReplaceUrlNodeNames.ContainsKey(node.Name))
                    {
                        ReplaceUrlInNode(node, processor);
                    }
                }
                return xDoc.InnerXml;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while replace url in nintex workflow template files", e);
                return content;
            }
        }

        //这两个Action只在07中特殊，所以默认替换即可，不需要考虑目的端SP Version。
        private static Dictionary<string, string> NintexNeedReplaceActivityNames = new Dictionary<string, string>
        {
            { ":CreateWebActivity" ,":CreateWeb2Activity"},
            { "'CreateWebActivity","\"createWeb2Activity"},
            { ":CreateListActivity",":CreateList2Activity"},
            { "'CreateListActivity","\"createList2Activity"}
        };

        private string ReplaceActivityNameInNintexWorkflow(string content)
        {
            try
            {
                foreach (var para in NintexNeedReplaceActivityNames)
                {
                    int replacedCount;
                    content = LSUtilityOfBytes.LSReplaceStringIgnoreCase(content, para.Key, para.Value, int.MaxValue, out replacedCount);
                }
                return content;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while replace activity rule in nintex workflow template files", e);
                return content;
            }
        }
        private string ReplaceAudienceRuleContent(string content)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.InnerXml = content;
                foreach (XmlNode node in xDoc.GetElementsByTagName("*"))
                {
                    if (node.Name.Contains("CreateAudienceActivity", StringComparison.OrdinalIgnoreCase))
                    {
                        var audienceRules = node.Attributes["AudienceRules"];
                        if (audienceRules != null)
                        {
                            int replacedCount;
                            audienceRules.Value = LSUtilityOfBytes.LSReplaceStringIgnoreCase(audienceRules.Value, "#string#", "#string (Single Value)#", int.MaxValue, out replacedCount);
                        }
                    }
                }
                return xDoc.InnerXml;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while replace audience rule in nintex workflow template files", e);
                return content;
            }
        }
        public virtual string ReplaceSpecial07Content(string content)
        {
            content = ReplaceActivityNameInNintexWorkflow(content);
            content = ReplaceAudienceRuleContent(content);
            return content;
        }

        private static void ReplaceUrlInNode(XmlNode node, AveWorkflowReplaceProcessor replaceProcessor)
        {

            XmlElement xe = (XmlElement)node;
            if (xe.Attributes != null)
            {
                foreach (XmlAttribute attribute in xe.Attributes)
                {
                    if (attribute != null && !string.IsNullOrEmpty(attribute.Name) && !string.IsNullOrEmpty(attribute.Value))
                        if (NintexNeedReplaceUrlAttributeNames.ContainsKey(attribute.Name))
                        {
                            string value = attribute.Value;
                            //Input只处理http开头的value
                            if (string.Equals(attribute.Name, "Input", StringComparison.Ordinal))
                            {
                                value = value.TrimStart('\t');
                                if (!value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }
                            attribute.Value = replaceProcessor.UrlReplace(attribute.Value);
                        }
                    if (NintexNeedReplaceUrlRichTextAttributeNames.ContainsKey(attribute.Name))
                    {
                        attribute.Value = replaceProcessor.ReplaceUrlContent(attribute.Value);
                    }
                }
            }
        }

        #region replace user in workflow template files

        /// <summary>
        /// 处理sharepoint designer workflow中user替换的逻辑,需要进一步完善
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual string ReplaceUserInSPDkflow(string content)
        {
            string contentReplaced = content;

            #region attribute values
            contentReplaced = ReplaceAssignedToUserInSPDkflow(contentReplaced);
            contentReplaced = ReplaceEmailToUserInSPDkflow(contentReplaced);
            contentReplaced = ReplaceAssigneesColumnUserInSPDkflow(contentReplaced);
            contentReplaced = ReplaceCCUserInSPDWorkflow(contentReplaced);
            contentReplaced = ReplaceUserValueInSPDWorkflow(contentReplaced);
            contentReplaced = ReplaceModifiedByUserInSPDkflow(contentReplaced);
            //AssigneesColumn="dev11\domainuser001"

            contentReplaced = ReplaceImpersonationUserInSPDkflow(contentReplaced);   //CI-42477
            #endregion

            contentReplaced = ReplaceUserForFindValueActivityInSPDkflow(contentReplaced);

            return contentReplaced;
        }

        private string ReplaceUsersForBuildAssignmentsXml(string encodeAssignments)
        {
            if (string.IsNullOrEmpty(encodeAssignments))
            {
                return string.Empty;
            }
            try
            {
                var decodeAssignments = System.Web.HttpUtility.HtmlDecode(encodeAssignments);
                var doc = new XmlDocument();
                doc.LoadXml(decodeAssignments);
                var xnsm = new XmlNamespaceManager(doc.NameTable);
                xnsm.AddNamespace("my", doc.DocumentElement.GetNamespaceOfPrefix("my"));
                xnsm.AddNamespace("pc", doc.DocumentElement.GetNamespaceOfPrefix("pc"));
                var persons = doc.SelectNodes(".//pc:Person", xnsm);
                if (persons != null)
                {
                    foreach (XmlNode person in persons)
                    {
                        try
                        {
                            var personElement = person as XmlElement;
                            if (personElement != null && personElement.HasChildNodes)
                            {

                                var displayNameNode = personElement.SelectSingleNode("pc:DisplayName", xnsm);
                                var accountIdNode = personElement.SelectSingleNode("pc:AccountId", xnsm);
                                if (accountIdNode != null)
                                {
                                    var accountId = accountIdNode.InnerText;
                                    var user = SPPermissionProcessor.GetOrCreateUser(accountId);
                                    if (user != null)
                                    {
                                        var newAccountId = user.LoginName;
                                        if (!newAccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase))
                                        {
                                            accountIdNode.InnerText = newAccountId;
                                            if (displayNameNode != null)
                                            {
                                                displayNameNode.InnerText = newAccountId;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        logger.Debug("Cannot find user {0} in destination.", accountId);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("An error occurred while replace single user in BuildAssignmentsXml.NodeInfo:{0},Error:{1}", person.OuterXml, ex);
                        }
                    }
                }
                return doc.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while ReplaceUsersForBuildAssignmentsXml. /r/nXmlInfo:{0}, /r/nError:{1}", encodeAssignments, ex);
                return encodeAssignments;
            }
        }


        /// <summary>
        /// for spd checkout item action
        /// Check Out Item(FindValueActivity)
        /// Copy List Item(FindValueActivity)
        /// Delete Item(FindValueActivity)
        /// Discard Check Out Item (FindValueActivity)
        /// Update List Item (FindValueActivity)
        /// Wait for Field Change in Current Item(WaitForActivity)
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReplaceUserForFindValueActivityInSPDkflow(string content)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(content);
                //(FieldName||ExternalFieldName)  &&  (Editor||Author||CheckoutUser)
                XmlNodeList findValueActivities = doc.SelectNodes(".//*[@ExternalFieldName='Editor']|.//*[@ExternalFieldName='Author']|.//*[@ExternalFieldName='CheckoutUser']|.//*[@FieldName='Author']|.//*[@FieldName='Editor']|.//*[@FieldName='CheckoutUser']");
                foreach (XmlNode node in findValueActivities)
                {
                    if (node is XmlElement)
                    {
                        XmlElement topNode = node as XmlElement;
                        try
                        {
                            XmlElement secondNode = topNode.ChildElements().FirstOrDefault<XmlElement>();
                            if (secondNode != null)
                            {
                                XmlElement userInfoNode = secondNode.ChildElements().FirstOrDefault<XmlElement>();
                                if (userInfoNode != null && !string.IsNullOrEmpty(userInfoNode.InnerText))
                                {
                                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(userInfoNode.InnerText);
                                    if (user != null)
                                    {
                                        userInfoNode.InnerText = user.LoginName;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Debug("An error occurred while replace user in workflow definition or template./r/nNodeInfo:{0},/r/nError:{1}", node == null ? "" : node.OuterXml, e);
                        }
                    }
                }
                content = doc.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, ex);
            }
            finally
            {
                doc.RemoveAll();
            }
            return content;
        }

        public virtual string ReplaceUrlForFindValueActivity(string content)
        {
            if (SPWorkflowProcessorRuntime.MappingManager == null || SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager == null ||
                SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.DestSiteInfo == null)
            {
                return content;
            }
            AveSiteMappingManager siteMappingManager = SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager;
            string destSiteUrl = siteMappingManager.DestSiteInfo.ServerRelativeUrl;
            ReplaceOption option = new ReplaceOption(true, true, true);
            AveWorkflowReplaceProcessor processor = new AveWorkflowReplaceProcessor(siteMappingManager.SiteManagedMappings, option, siteMappingManager.SourceSiteInfo, destSiteUrl);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(content);
                XmlNodeList findValueActivities = doc.SelectNodes(".//*[@ExternalFieldName='FileRef']");
                foreach (XmlNode node in findValueActivities)
                {
                    if (node is XmlElement)
                    {
                        XmlElement topNode = node as XmlElement;
                        try
                        {
                            XmlElement secondNode = topNode.ChildElements().FirstOrDefault<XmlElement>();
                            if (secondNode != null)
                            {
                                XmlElement userInfoNode = secondNode.ChildElements().FirstOrDefault<XmlElement>();
                                if (userInfoNode != null && !string.IsNullOrEmpty(userInfoNode.InnerText))
                                {
                                    userInfoNode.InnerText = processor.UrlReplace(userInfoNode.InnerText);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Debug("An error occurred while replace user in workflow definition or template./r/nNodeInfo:{0},/r/nError:{1}", node == null ? "" : node.OuterXml, e);
                        }
                    }
                }
                content = doc.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, ex);
            }
            finally
            {
                doc.RemoveAll();
            }
            return content;
        }

        /// <summary>
        /// 根据attribute name找出对应的xmlNode，然后进行user替换
        /// </summary>
        /// <param name="content"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        private string ReplaceUserByAttributeName(string content, string attributeName)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(content);
                //需要时再添加
                //string x = doc.DocumentElement.GetNamespaceOfPrefix("x");
                //XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                //nsmgr.AddNamespace("X", x);
                XmlNodeList nodes = doc.SelectNodes(".//*[@" + attributeName + "!='']");
                foreach (XmlNode node in nodes)
                {
                    if (node is XmlElement)
                    {
                        XmlElement xe = node as XmlElement;
                        string attributeValue = xe.GetAttribute(attributeName);

                        if (attributeValue.StartsWith("{ActivityBind", StringComparison.OrdinalIgnoreCase))
                        {
                            //1.考虑解析binding，因为有的是通过X:Name=bindging的id来关联的，有的是直接通过return value来关联的
                            //需要选择多个node，因为可能多值assign
                            //先判断是否有用return value的，如果没有，再用x:Name找
                            if (!ReplaceUserAttributesRelatedByReturnValue(doc, attributeValue))
                            {
                                ReplaceUserAttributesRelatedByID(doc, attributeValue);
                            }
                        }
                        else if (NeedReplaceUser(xe, attributeName))
                        {
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(attributeValue);
                            if (user != null)
                            {
                                xe.SetAttribute(attributeName, user.LoginName);
                            }
                        }
                    }
                }
                content = doc.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, ex);
            }
            finally
            {
                doc.RemoveAll();
            }

            return content;
        }

        /// <summary>
        /// 统一在ReplaceUserInSPDkflow中调用,替换AssignTo的user（包括AssignTo关联的user）
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReplaceAssignedToUserInSPDkflow(string content)
        {
            return ReplaceUserByAttributeName(content, "AssignedTo");
        }

        /// <summary>
        /// 统一在ReplaceUserInSPDkflow中调用,替换Email To的user
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReplaceEmailToUserInSPDkflow(string content)
        {
            return ReplaceUserByAttributeName(content, "To");
        }

        /// <summary>
        /// 统一在ReplaceUserInSPDkflow中调用,替换AssigneesColumn的user
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReplaceAssigneesColumnUserInSPDkflow(string content)
        {
            return ReplaceUserByAttributeName(content, "AssigneesColumn");
        }

        /// <summary>
        /// 统一在ReplaceUserInSPDkflow中调用,替换CC的user（包括CC关联的user）
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReplaceCCUserInSPDWorkflow(string content)
        {
            return ReplaceUserByAttributeName(content, "CC");
        }

        /// <summary>
        /// 统一在ReplaceUserInSPDkflow中调用,替换使用UserValue这个Attribute的user(目前用到的是Lookup Manager of a User )
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReplaceUserValueInSPDWorkflow(string content)
        {
            return ReplaceUserByAttributeName(content, "UserValue");
        }

        /// <summary>
        /// 替换assign to attribute中通过return value binding的activity的user
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="assignedTo"></param>
        private bool ReplaceUserAttributesRelatedByReturnValue(XmlDocument doc, string assignedTo)
        {
            #region 通过returnvalue 关联的activity
            bool findValueSuccessful = false;
            XmlNodeList bindingNodes = doc.SelectNodes(".//*[@Value][@ReturnValue='" + assignedTo + "']");
            if (bindingNodes.Count > 0)
            {
                findValueSuccessful = true;
                foreach (XmlNode singleNode in bindingNodes)
                {
                    if (singleNode is XmlElement)
                    {
                        XmlElement singleElement = singleNode as XmlElement;
                        //目前在这层binding的都是item的field，不需要替换，如果有发现需要替换的再添加逻辑处理
                        try
                        {
                            if (singleElement != null)
                            {
                                string valueAttribute = singleElement.GetAttribute("Value");
                                if (!string.IsNullOrEmpty(valueAttribute))
                                {
                                    if (valueAttribute.StartsWith("{ActivityBind", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ReplaceUserAttributesRelatedByReturnValue(doc, valueAttribute);
                                    }
                                    else
                                    {
                                        if (NeedReplaceUser(singleElement, "Value"))
                                        {
                                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(valueAttribute);
                                            if (user != null)
                                            {
                                                singleElement.SetAttribute("Value", user.LoginName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug("An error occurred while replace single user in workflow template file./r/nNodeInfo:{0}./r/nError:{1}", singleNode == null ? "" : singleNode.OuterXml, ex);
                        }
                    }
                }
            }

            return findValueSuccessful;

            #endregion
        }

        /// <summary>
        /// 替换assign to attribute中通过X:Name这个attribute的activity的user
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="assignedTo"></param>
        private void ReplaceUserAttributesRelatedByID(XmlDocument doc, string attributeValue)
        {
            #region 通过X:Name 关联的activity
            //目前只发现BuildAssignmentsXmlActivity一种情况
            int start = 13;//{ActivityBind
            int end = attributeValue.IndexOf(',');
            string id = attributeValue.Substring(start, end - start).Trim();

            try
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("x", doc.DocumentElement.GetNamespaceOfPrefix("x"));

                XmlNodeList nodes = doc.SelectNodes(".//*[@x:Name='" + id + "'][@Value!='']", nsmgr);
                foreach (XmlNode node in nodes)
                {
                    if (node is XmlElement)
                    {
                        XmlElement xe = node as XmlElement;
                        if (xe != null)
                        {
                            string assignmentsValue = xe.GetAttribute("Value");

                            assignmentsValue = ReplaceUsersForBuildAssignmentsXml(assignmentsValue);

                            xe.SetAttribute("Value", assignmentsValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, ex);
            }

            #endregion
        }

        /// <summary>
        /// 统一在ReplaceUserInSPDkflow中调用，替换ModifiedBy User
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string ReplaceModifiedByUserInSPDkflow(string content)
        {
            return ReplaceUserByAttributeName(content, "UserName");
        }

        private string ReplaceImpersonationUserInSPDkflow(string content)
        {
            return ReplaceUserByAttributeName(content, "Users");
        }

        #endregion

        public virtual string ReplaceOtherContentInSPDWorkflow(string content)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(content);
                XmlElement xe = (XmlElement)doc["ns0:RootWorkflowActivityWithData"];
                string ns0Root = xe.GetAttribute("xmlns:ns0");
                string ns0RootNewValue = ns0Root;
                if (ns0Root.ToLower(CultureInfo.InvariantCulture).Contains("Microsoft.SharePoint.WorkflowActions".ToLower(CultureInfo.InvariantCulture)))
                {
                    ns0RootNewValue = ns0Root.Replace("Version=12.0.0.0", "Version=14.0.0.0");
                }

                content = content.Replace(ns0Root, ns0RootNewValue);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, ex);
            }
            finally
            {
                doc.RemoveAll();
            }
            return content;
        }

        protected string ReplaceSearchResultType(string content)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(content);
                XmlNodeList nodes = xmlDoc.SelectNodes(string.Format(".//*[@{0}!='']", "FileExtension"));
                foreach (XmlNode node in nodes)
                {

                    XmlElement xe = (XmlElement)node;
                    if (xe.HasAttribute("FileExtension") && xe.GetAttribute("FileExtension") != null && xe.Name.IndexOf("MOSSSearchQueryActivity") > 0)
                    {
                        string value = xe.GetAttribute("FileExtension");
                        value = value.Replace("IsDocument=1", "isdocument:1");
                        value = value.Replace("fileextension='doc' or fileextension='docx' or fileextension='dot'", "filetype:doc or filetype:docx or filetype:dot");
                        value = value.Replace("fileextension='xls' or fileextension='xlsx' or fileextension='xlt'", "filetype:xls or filetype:xlsx or filetype:xlt");
                        value = value.Replace("fileextension='ppt' or fileextension='pptx'", "filetype:ppt or filetype:pptx");
                        xe.SetAttribute("FileExtension", value);

                    }

                }
                return xmlDoc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.NintexReplaceUserError, e);
            }
            return content;
        }



        private void ReplaceNintexUserInXmlElement(XmlDocument xmlDoc)
        {
            string[] xmlAttributes = { "User", "UserID", "Username", "QueryUsername", "UsernameCredentials", "UserToAdd",
                "SecurityGroupName" , "RequestXml" , "SecondarySiteOwner", "SiteOwner", "WebAdministrator", "Attendees" };
            foreach (string element in xmlAttributes)
            {
                XmlNodeList nodes = xmlDoc.SelectNodes(string.Format(".//*[@{0}!='']", element));
                foreach (XmlNode node in nodes)
                {
                    try
                    {
                        XmlElement xe = (XmlElement)node;
                        if (xe.HasAttribute("IsUser") && !Boolean.Parse(xe.GetAttribute("IsUser")))
                        {
                            continue;
                        }
                        string name = xe.GetAttribute(element);
                        if (name.Equals("{x:Null}", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        if (name.Contains("@"))
                        {
                            string newName = SPWorkflowCommon.OnModifyEmailAddress(null, name);
                            if (!newName.Equals(name))
                            {
                                xe.SetAttribute(element, newName);
                                continue;
                            }
                        }
                        if (element.Equals("RequestXml", StringComparison.OrdinalIgnoreCase) && (xe.Name.IndexOf("ExchangeCreateAppointmentActivity") >= 0 || xe.Name.IndexOf("ExchangeCreateTaskActivity") >= 0))
                        {
                            xe.SetAttribute(element, ReplaceRequestXMLUser(name));
                        }
                        if (NeedReplaceUser(xe, element))
                        {
                            var names = name.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            StringBuilder value = new StringBuilder();
                            foreach (var username in names)
                            {
                                IAveUser user = SPPermissionProcessor.GetOrCreateUser(username);
                                if (user != null)
                                {
                                    value.Append(user.LoginName);
                                    value.Append(';');
                                }
                            }
                            if (value.Length > 0)
                            {
                                value.Length = value.Length - 1;
                                xe.SetAttribute(element, value.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.NintexReplaceUserError, e);
                    }
                }
            }
        }

        private string ReplaceRequestXMLUser(string innerText)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(innerText);
            XmlNodeList nodes = xmlDoc.GetElementsByTagName("EmailAddress");
            foreach (XmlNode node in nodes)
            {
                try
                {
                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(node.InnerText);
                    node.InnerText = user.LoginName;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.NintexReplaceUserError, e);
                }
            }
            return xmlDoc.OuterXml;
        }
    }

    internal sealed class ConfigFileProc : SPWorkflowFileContentProc
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SPWFAssociationUnit assoUnit;

        public ConfigFileProc()
        {
        }

        public ConfigFileProc(SPWFAssociationUnit assoUnit)
        {
            this.assoUnit = assoUnit;
        }
        public SPWFAssociationUnit AssociationUnit
        {
            get { return this.assoUnit; }
            set { this.assoUnit = value; }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Hist:History as key.")]
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default Config File Processor Replace");
            string strContent = string.Empty;

            try
            {
                if (mFile != null)
                {
                    using (StreamReader objReader = new StreamReader(mFile.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions)))
                    {
                        strContent = objReader.ReadToEnd();
                    }

                    XmlDocument xmlConfig = null;
                    try
                    {
                        xmlConfig = new XmlDocument();

                        strContent = UpdateLookupListId(strContent, dic);

                        xmlConfig.LoadXml(strContent);
                        if (NintexWorkflowUtility.IsNintexWorkflow(assoUnit) && assoUnit.ParentAveSPWeb != null)
                        {
                            UpdateNintexFormDataNodeValue(xmlConfig, "/WorkflowConfig/FormData/Data", assoUnit.ParentAveSPWeb, assoUnit.ParentList);
                        }
                        if(NintexWorkflowUtility.IsNintexWorkflow(assoUnit))
                        {
                            InsertFakeMetaDataNode(xmlConfig);
                        }
                        UpdateNodeValue(xmlConfig, dic, "/WorkflowConfig/Template/@DocLibID", "TemplateListId");
                        UpdateNodeValue(xmlConfig, dic, "/WorkflowConfig/Template/@XomlVersion", "XomlFileVersion");
                        UpdateNodeValue(xmlConfig, dic, "/WorkflowConfig/Template/@RulesVersion", "RulesFileVersion");
                        UpdateNodeValue(xmlConfig, dic, "/WorkflowConfig/Template/@BaseID", "BaseID");

                        UpdateContentTypeIdNode(dic, xmlConfig, mFile);
                        UpdateCategoryNode(dic, xmlConfig, mFile);

                        UpdateNodeValue(xmlConfig, dic, "/WorkflowConfig/Association/@ListID", "ParentId");
                        UpdateNodeValue(xmlConfig, dic, "/WorkflowConfig/Association/@TaskListID", "TaskListId");

                        AddOrUpdateHistoryListId(dic, xmlConfig);

                        UpdateNodeValue(xmlConfig, dic, "/WorkflowConfig/Template/@XomlHref", "XomlHref");  //for ADO-174953
                        UpdateNodeValue(xmlConfig, dic, "/WorkflowConfig/Template/@RulesHref", "RulesHref");  //for ADO-174953

                        UpdateContentTypesNode(dic, xmlConfig, mFile);

                        //-<Initiation URL="_layouts/NintexWorkflow/StartWorkflow.aspx">
                        if (SPWorkflowProcessorRuntime.ObjectModelFactory == null && mFile.ParentFolder.ParentWeb.Site.APIType == AveAPIType.Server)
                        {
                            SPWorkflowProcessorRuntime.ObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(mFile.ParentFolder.ParentWeb.Site.Url, null);
                        }
                        if (SPWorkflowProcessorRuntime.ObjectModelFactory != null && SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind.IsServerMode13Upper())
                        {
                            //this is nintex workflow's bug, we add special logic to handle it ,do some replacement
                            UpDateAttributeValue(xmlConfig, "/WorkflowConfig/Initiation/@URL", "_layouts/NintexWorkflow/StartWorkflow.aspx", "_layouts/15/NintexWorkflow/StartWorkflow.aspx");
                        }
                        strContent = xmlConfig.OuterXml;
                        try
                        {
                            if (mFile.Level != AveFileLevel.Checkout)
                            {
                                mFile.CheckOut(false, string.Empty);
                            }
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
                            logger.Warn("An exception occurred while checkout file. exception:{0}", e.ToString());
                        }
                        mFile.SaveBinary(Encoding.UTF8.GetBytes(strContent));
                    }
                    finally
                    {
                        if (xmlConfig != null)
                            xmlConfig.RemoveAll();
                    }
                }
                else
                {
                    throw new ApplicationException("Config file cannot be found");
                }
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default Config File Processor Replace");
            }
            return strContent;
        }

        private string UpdateLookupListId(string strContent, Dictionary<string, object> dic)
        {
            try
            {
                List<string> prefixList = new List<string> { "LookupList>" };
                //strContent = System.Web.HttpUtility.HtmlDecode(strContent);
                foreach (var prefix in prefixList)
                {
                    strContent = UpdateLookupListId(prefix, AveRegexCommon.GUIDREG, strContent, dic);
                    strContent = UpdateLookupListId(prefix, AveRegexCommon.GUIDREG_WITH_HTML_ENCODE, strContent, dic);
                }
            }
            catch (SPWFProcessorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Debug("Failed to replace lookup list id in wconfig file, exception: {0}", ex);
            }
            return strContent;
        }

        private string UpdateLookupListId(string prefix, string regKey, string strContent, Dictionary<string, object> dic)
        {
            Regex reg = new Regex(prefix + regKey, RegexOptions.IgnoreCase);
            int startPos = 0;
            while (true)
            {
                var match = reg.Match(strContent, startPos);
                if (match.Success)
                {
                    startPos = match.Index + 1;
                    var guidStr = strContent.Substring(match.Index + prefix.Length, match.Length - prefix.Length);
                    if (!string.IsNullOrEmpty(guidStr))
                    {
                        // if guidStr contains html encode "%2d", change to '-'
                        guidStr = guidStr.Replace("%2d", "-").Replace("%2D", "-");
                        var guid = new Guid(guidStr);
                        strContent = ReplaceListId(strContent, guid, mFile.ParentFolder.ParentWeb, dic);
                    }
                }
                else
                {
                    break;
                }
            }
            return strContent;
        }

        private static string ReplaceListId(string strContent, Guid listID, IAveWeb web, Dictionary<string, object> mapping)
        {
            if (SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.ListIdMappingContainsKey(listID))
            {
                Regex replaceReg = new Regex(listID.ToString(), RegexOptions.IgnoreCase);
                Guid mappingID;
                SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.GetValueFromListIdMapping(listID, out mappingID);
                logger.Debug("Replace wfconfig lookup list id: {0} -> {1}", listID, mappingID);
                strContent = replaceReg.Replace(strContent, mappingID.ToString());
            }
            else if (mapping.ContainsKey(listID.ToString().ToUpper(CultureInfo.InvariantCulture)))
            {
                var objectName = mapping[listID.ToString().ToUpper(CultureInfo.InvariantCulture)].ToString();
                if (objectName.StartsWith("[ListID]", StringComparison.OrdinalIgnoreCase))
                {
                    string webUrl = string.Empty;
                    string listName = objectName.Substring(8);
                    var index = listName.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    if (index > 0)
                    {
                        AveSiteMappingManager siteMappingManager = SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager;
                        string destSiteUrl = siteMappingManager.DestSiteInfo.ServerRelativeUrl;
                        ReplaceOption option = new ReplaceOption(true, true, true);
                        AveWorkflowReplaceProcessor processor = new AveWorkflowReplaceProcessor(siteMappingManager.SiteManagedMappings, option, siteMappingManager.SourceSiteInfo, destSiteUrl);
                        webUrl = processor.UrlReplace(listName.Substring(0, index));
                        listName = listName.Substring(index + 1);
                    }
                    string mappingTitle = string.Empty;
                    IAveList listObj = null;
                    if (SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.GetValueFromListTitleMappnig(web.ID, listName, out mappingTitle))
                    {
                        listName = mappingTitle;
                    }
                    if (!string.IsNullOrEmpty(webUrl))
                    {
                        using (var tempWeb = web.Site.OpenWeb(webUrl))
                        {
                            listObj = tempWeb.GetListByName(listName, false);
                        }
                    }
                    else
                    {
                        listObj = web.GetListByName(listName, false);
                    }
                    if (listObj != null)
                    {
                        logger.Debug("Replace wfconfig lookup list id: {0} -> {1}", listID, listObj.ID);
                        Regex replaceReg = new Regex(listID.ToString(), RegexOptions.IgnoreCase);
                        strContent = replaceReg.Replace(strContent, listObj.ID.ToString());
                    }
                    else
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction);
                    }
                }
                else
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction);
                }
            }
            else
            {
                if (web.GetList(listID) == null)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction);
                }
            }
            return strContent;
        }

        private static void UpdateContentTypesNode(Dictionary<string, object> dic, XmlDocument xmlConfig, IAveFile file)
        {
            XmlNodeList ctList = xmlConfig.SelectNodes("/WorkflowConfig/ContentTypes/ContentType");
            if (ctList != null)
            {
                foreach (XmlElement ctNode in ctList.OfType<XmlElement>())
                {
                    string oldCTId = ctNode.GetAttribute("ContentTypeID");
                    if (!string.IsNullOrEmpty(oldCTId))
                    {
                        string oldCTIdKey = oldCTId.ToUpperEx(2, oldCTId.Length - 2);
                        if (dic.ContainsKey(oldCTIdKey))
                        {
                            ctNode.SetAttribute("ContentTypeID", (string)dic[oldCTIdKey]);
                        }
                        else
                        {
                            string newId = EnsureContentTypeId(file, oldCTId, "");
                            ctNode.SetAttribute("ContentTypeID", newId);
                        }
                    }
                }
            }
        }

        private static void UpdateContentTypeIdNode(Dictionary<string, object> dic, XmlDocument xmlConfig, IAveFile file)
        {
            //todo:wbhu,现在只有还原reusable workflow template时,新逻辑会生效,暂时不对老逻辑处理,下个版本再去掉老逻辑
            var contentTypeNode = xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@ContentTypeID");
            if (contentTypeNode != null)
            {
                string contentTypeId = null;
                object value;
                if (dic.TryGetValue("ContentTypeId", out value))
                {
                    contentTypeId = Convert.ToString(value);
                }
                else
                {
                    if (dic.TryGetValue(AveWorkflowConstants.ReplaceDictionary_ContentTypeID, out value))
                    {
                        string[] contentTypeInfo = GetSplitString(value.ToString(), 2, new[] { ";" }, true);
                        string id = contentTypeInfo[0];
                        string name = contentTypeInfo[1];
                        contentTypeId = EnsureContentTypeId(file, id, name);
                    }
                    else
                    {
                        string id = contentTypeNode.Value;
                        string name = "";
                        contentTypeId = EnsureContentTypeId(file, id, name);
                    }
                }
                if (!string.IsNullOrEmpty(contentTypeId) && !string.IsNullOrEmpty(contentTypeNode.Value))
                {
                    contentTypeNode.Value = contentTypeId;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="maxValueCount"></param>
        /// <param name="seprator"></param>
        /// <param name="fixToMaxValueCount"></param>
        /// <returns></returns>
        private static string[] GetSplitString(string content, int maxValueCount, string[] seprator, bool fixToMaxValueCount)
        {
            string[] result = content.Split(seprator, maxValueCount, StringSplitOptions.None);
            if (fixToMaxValueCount && result.Length < maxValueCount)
            {
                string[] newResult = new string[maxValueCount];
                result.CopyTo(newResult, 0);
                int i = result.Length;
                while (i < maxValueCount)
                {
                    newResult[i] = "";
                    i++;
                }
                return newResult;
            }
            return result;
        }

        private static void UpdateCategoryNode(Dictionary<string, object> dic, XmlDocument xmlConfig, IAveFile file)
        {
            //todo:wbhu,现在只有还原reusable workflow template时,新逻辑会生效,暂时不对老逻辑处理,下个版本再去掉老逻辑
            XmlNode categoryNode = xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@Category");
            if (categoryNode != null)
            {
                //old logic
                if (dic.ContainsKey("ContentTypeId"))
                {
                    if (categoryNode.Value.Contains(AveWorkflowConstants.Workflow_Category_ContentType_Prefix))
                    {
                        categoryNode.Value = AveWorkflowConstants.Workflow_Category_ContentType_Prefix + (string)dic["ContentTypeId"];
                    }
                }
                else //new one
                {
                    object category;
                    if (!dic.TryGetValue(AveWorkflowConstants.ReplaceDictionary_Category, out category))
                    {
                        category = categoryNode.Value;
                    }
                    string value = Convert.ToString(category);
                    if (string.IsNullOrEmpty(value))
                    {
                        value = AveWorkflowConstants.Workflow_Category_Default;
                    }
                    categoryNode.Value = HandleCategoryValue(value, file);
                }
            }
        }

        public static string AnalyzeContentTypeInfoInCategory(string categoryString, IAveFile file)
        {
            StringBuilder categoryInfoBuilder = new StringBuilder();
            List<string> categories = categoryString.Split(new string[] { AveWorkflowConstants.Workflow_Category_Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var category in categories)
            {
                if (category.StartsWith(AveWorkflowConstants.Workflow_Category_ContentType_Prefix, StringComparison.OrdinalIgnoreCase))
                {
                    string contentTypeId = category.Substring(12);
                    IAveContentType contentType = file.Web.AvailableContentTypes.GetById(contentTypeId);
                    if (contentType != null)
                    {
                        categoryInfoBuilder.AppendFormat("{0}{1}{2};{3}", AveWorkflowConstants.Workflow_Category_Separator, AveWorkflowConstants.Workflow_Category_ContentType_Prefix, contentTypeId, contentType.Name);
                    }
                    else
                    {
                        categoryInfoBuilder.AppendFormat("{0}{1}", AveWorkflowConstants.Workflow_Category_Separator, category);
                    }
                }
                else
                {
                    categoryInfoBuilder.AppendFormat("{0}{1}", AveWorkflowConstants.Workflow_Category_Separator, category);
                }
            }
            string result = categoryInfoBuilder.ToString();
            if (result.Length > 2)
            {
                result = result.Substring(2);
            }
            return result;
        }

        private static string HandleCategoryValue(string categoryValue, IAveFile file)
        {
            string result;
            try
            {
                StringBuilder categoryInfoBuilder = new StringBuilder();
                List<string> categories = categoryValue.Split(new string[] { AveWorkflowConstants.Workflow_Category_Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var category in categories)
                {
                    if (category.StartsWith(AveWorkflowConstants.Workflow_Category_ContentType_Prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        //0  "ContentType;" 1 ContentTypeId 2 ContentTypeName
                        string[] contentTypeinfos = GetSplitString(category, 3, new[] { ";" }, true);
                        string id = contentTypeinfos[1];
                        string contentTypeName = contentTypeinfos[2];

                        var newId = EnsureContentTypeId(file, id, contentTypeName);
                        categoryInfoBuilder.AppendFormat("{0}{1}{2}", AveWorkflowConstants.Workflow_Category_Separator, AveWorkflowConstants.Workflow_Category_ContentType_Prefix, newId);
                    }
                    else
                    {
                        categoryInfoBuilder.AppendFormat("{0}{1}", AveWorkflowConstants.Workflow_Category_Separator, category);
                    }
                }
                result = categoryInfoBuilder.ToString();
                if (result.Length > 2)
                {
                    result = result.Substring(2);
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while handle category info in workflow configuration template file.FileName:{0},Value:{1},Error:{2}", file.Name, categoryValue, e);
                result = categoryValue;
            }
            return result;
        }

        private static string EnsureContentTypeId(IAveFile file, string id, string contentTypeName)
        {
            //优先找mapping,mapping找不到用Name找,name也找不到就用原端Id
            var newId = GetMappedSiteContentTypeId(id, SPWorkflowProcessorRuntime.MappingManager);
            if (string.IsNullOrEmpty(newId))
            {
                IAveContentType contentType = file.Web.AvailableContentTypes[contentTypeName];
                if (contentType != null)
                {
                    newId = contentType.ID.ToString();
                }
            }
            if (string.IsNullOrEmpty(newId))
            {
                newId = id;
            }
            return newId;
        }

        private static void AddOrUpdateHistoryListId(IDictionary<string, object> dic, XmlDocument xmlConfig)
        {
            object historyListId;
            if (dic.TryGetValue("HistListId", out historyListId))
            {
                var historyListIdNode = xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@HistoryListID");
                if (historyListIdNode != null)
                {
                    historyListIdNode.Value = (string)historyListId;
                }
                else
                {
                    try
                    {
                        XmlNode assoNode = xmlConfig.SelectSingleNode("/WorkflowConfig/Association");
                        XmlElement assoHis = (XmlElement)assoNode;
                        if (assoHis != null)
                        {
                            assoHis.SetAttribute("HistoryListID", (string)historyListId);
                        }
                    }
                    catch (Exception ex)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.Common_XmlFileHandleException, ex.Message);
                        logger.Warn("An exception occurred while handle xml file. exception:{0}", ex);
                    }
                }
            }
            else
            {
                logger.Warn("Replace dictionary does not has the value of key HistListId");
            }
        }

        private static string GetMappedSiteContentTypeId(string oldContentTypeId, AveMappingManager mappingManager)
        {
            string contentTypeId = null;
            if (mappingManager == null)
            {
                logger.Warn("Cannot get mapped Site ContentType Id as Mapping Manager is null.OldId:{0}", oldContentTypeId);
                return null;
            }
            if (string.IsNullOrEmpty(oldContentTypeId))
            {
                logger.Warn("Cannot get mapped Site ContentType Id as oldContentTypeId is null.OldId:{0}", oldContentTypeId);
                return null;
            }
            bool mapped = false;
            IAveContentTypeId newId;
            var keyId = oldContentTypeId.ToUpperEx(2, oldContentTypeId.Length - 2);
            if (SPWorkflowProcessorRuntime.MappingManager.WebMappingManager.WebLevelCTIdMapping.TryGetValue(keyId, out newId))
            {
                if (newId != null)
                {
                    contentTypeId = newId.ToString();
                }
            }
            return contentTypeId;
        }

        private static void UpdateNintexFormDataNodeValue(XmlDocument xmlConfig, string nodePath, IAveSPWeb aveSPWeb, IAveList parentList)
        {
            var formDataNodes = xmlConfig.SelectNodes(nodePath);
            if (formDataNodes != null)
            {
                var nintexFormServie = new NintexFormContentProcessorServer(aveSPWeb, parentList, true);
                foreach (XmlNode formDataNode in formDataNodes)
                {
                    try
                    {
                        formDataNode.InnerText = nintexFormServie.ReplaceFormContent(formDataNode.InnerText, string.Empty, true);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while updte nintex form in workflow,form content:{0}, error:{1}", formDataNode.InnerText, e);
                    }
                }
            }

        }
        /// <summary>
        /// 07 nintex workflow config file里没有metadata节点，转移到高版本之后删除不掉workflow template.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static void InsertFakeMetaDataNode(XmlDocument xmlConfig)
        {
            try
            {
                var metaDataNode = xmlConfig.DocumentElement.SelectSingleNode("//MetaData");
                if (metaDataNode == null)
                {
                    metaDataNode = CreateNode(xmlConfig, "MetaData", string.Empty);
                    metaDataNode.AppendChild(CreateNode(xmlConfig, "EcbId", Guid.Empty.ToString()));
                    xmlConfig.DocumentElement.AppendChild(metaDataNode);
                }
                var ecbIdNode = metaDataNode.SelectSingleNode("//EcbId");
                if (ecbIdNode == null)
                {
                    metaDataNode.AppendChild(CreateNode(xmlConfig, "EcbId", Guid.Empty.ToString()));
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while insert fake metadata node in nintex workflow template files", e);
            }
        }
        private static XmlElement CreateNode(XmlDocument xd, string name, string value)
        {
            var node = xd.CreateElement(name);
            if (!string.IsNullOrEmpty(value))
            {
                node.InnerText = value;
            }
            return node;
        }
        private static void UpdateNodeValue(XmlDocument xmlConfig, Dictionary<string, object> replaceDictionary, string nodePath, string key)
        {
            var configNode = xmlConfig.SelectSingleNode(nodePath);
            if (configNode != null)
            {
                object value;
                if (replaceDictionary.TryGetValue(key, out value))
                {
                    configNode.Value = (string)value;
                }
                else
                {
                    logger.Warn("The key {0} does not exist in replaceDictionary.", key);
                }
            }
        }

        private static void UpDateAttributeValue(XmlDocument xmlConfig, string nodePath, string oldValue, string newValue)
        {
            try
            {
                var configNode = xmlConfig.SelectSingleNode(nodePath);
                if (configNode != null)
                {
                    var attributeValue = configNode.Value;
                    if (string.Equals(attributeValue, oldValue, StringComparison.OrdinalIgnoreCase))
                    {
                        configNode.Value = newValue;
                    }
                    else
                    {
                        logger.Warn("The url {0} is not the default start form url.", attributeValue);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occourred while UpDateAttributeValue,NodePath is:{0},oldValue:{1},newValue:{2} Error:{3}", nodePath, oldValue, newValue, e);
            }
        }

        public static Dictionary<string, string> GetTemplateProperties(byte[] content)
        {
            string strContent = ConvertBytesToString(content);
            XmlDocument xmlConfig = XmlHelper.LoadXmlDocument(strContent);
            if (xmlConfig == null)
            {
                return new Dictionary<string, string>();
            }
            Dictionary<string, string> properties = XmlHelper.GetElementAttributes(xmlConfig, "/WorkflowConfig/Template");
            return properties;
        }
    }

    internal sealed class XomlFileProc : SPWorkflowFileContentProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private string ResetXmlNameSpaceURI(string xomlFileContent)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xomlFileContent);
            Dictionary<string, string> cach = new Dictionary<string, string>();
            foreach (XmlAttribute attribute in document.DocumentElement.Attributes)
            {
                if (attribute != null && attribute.Name != null && attribute.Name.StartsWith("xmlns:ns") && attribute.Value.EndsWith("PublicKeyToken=null"))
                {
                    cach[attribute.Value] = attribute.Value.Replace("PublicKeyToken=null", "PublicKeyToken=71e9bce111e9429c");
                    attribute.Value = cach[attribute.Value];
                }
            }
            var tempContent = document.OuterXml;
            //更新 child 的namespace uri
            foreach (var item in cach)
            {
                tempContent = tempContent.Replace(item.Key, item.Value);
            }
            return tempContent;
        }
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default Xoml File Processor Replace");
            string strContent = string.Empty;
            if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
            {
                if (mFile != null)
                {
                    using (StreamReader objReader = new StreamReader(mFile.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions)))
                    {
                        strContent = objReader.ReadToEnd();
                    }

                    CheckUsedUserDefinedActionByContent(strContent);
                    //moved from SPWorkflowSubFileUnit.RestoreWorkflowTemplateFiles List ContenTypeId,优先替换
                    if (SPWorkflowProcessorRuntime.MappingManager != null)
                    {
                        strContent = AveReplaceProcessor.ReplaceTaskContentTypeIdInXoml(strContent, SPWorkflowProcessorRuntime.MappingManager.WebMappingManager);
                        strContent = AveReplaceProcessor.ReplaceActionContentTypeIDInXoml(strContent, SPWorkflowProcessorRuntime.MappingManager.ListMappingManager.ListLevelCTIdMapping);
                        strContent = ReplaceUrlForInfoPath(strContent, this.SPFile.Web.Site.Url);
                        strContent = ReplaceUrlForEmail(strContent);
                    }

                    switch (WorkflowType)
                    {
                        case WorkflowType.NintexWorkflowLocal:
                            strContent = NintexWorkflowUtility.ReplaceIdsInUserDefinedAction(strContent, mFile.ParentFolder.ParentWeb.Site.ID, mFile.ParentFolder.ParentWeb.ID, Guid.Empty);
                            strContent = base.ReplaceUserInNintexWorkflow(strContent);
                            strContent = base.ReplaceNintexContentTypeID(strContent);
                            strContent = base.ReplaceUrlInNintexWorkflow(strContent);
                            strContent = base.ReplaceSpecial07Content(strContent);
                            if (SPWorkflowProcessorRuntime.ObjectModelFactory == null && mFile.ParentFolder.ParentWeb.Site.APIType == AveAPIType.Server)
                            {
                                SPWorkflowProcessorRuntime.ObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(mFile.ParentFolder.ParentWeb.Site.Url, null);
                            }
                            if (SPWorkflowProcessorRuntime.ObjectModelFactory != null && SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind.IsServerMode13Upper())
                            {
                                //this is nintex workflow's bug, we add special logic to handle it ,do some replacement
                                strContent = base.ReplaceSearchResultType(strContent);
                            }
                            if (SPWorkflowProcessorRuntime.ObjectModelFactory != null && SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind.IsServerMode16Upper())
                            {
                                //Nintex 10 to 16 xoml需要特殊处理，namespaceURI PublicKeyToken不能为null
                                strContent = ResetXmlNameSpaceURI(strContent);
                            }
                            break;
                    }

                    foreach (KeyValuePair<string, object> pair in dic)
                    {
                        int replacedCount = 0;
                        strContent = LSUtilityOfBytes.LSReplaceStringIgnoreCase(strContent, pair.Key, pair.Value.ToString(), int.MaxValue, out replacedCount);
                    }

                    strContent = base.ReplaceUserInSPDkflow(strContent);
                    strContent = base.ReplaceUrlForFindValueActivity(strContent);
                    //donot need it any more, version replace has been added into replace dictionary
                    ////strContent = base.ReplaceOtherContentInSPDWorkflow(strContent);

                }


                string charSet = mFile.CharSetName;
                if (string.IsNullOrEmpty(charSet))
                    charSet = "utf-8";
                try
                {
                    if (mFile.Level != AveFileLevel.Checkout)
                    {
                        mFile.CheckOut(false, string.Empty);
                    }
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
                    logger.Warn("An exception occurred while checkout file. exception:{0}", e.ToString());
                }

                mFile.SaveBinary(Encoding.GetEncoding(charSet).GetBytes(strContent));
                SPWorkflowProcessorRuntime.Log(Logs.FileContentProc_FileCharsetName, mFile.Name, charSet);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default Xoml File Processor Replace");
            }
            return strContent;
        }

        private static string ReplaceUrlForInfoPath(string tempContentStr, string siteUrl)
        {
            try
            {
                logger.Debug("Begin ReplaceUrlForInfoPath.");
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(tempContentStr);
                string ns0Prefixdfs = xmlDoc.DocumentElement.GetNamespaceOfPrefix("ns0");
                string ns1Prefixdfs = xmlDoc.DocumentElement.GetNamespaceOfPrefix("ns1");
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("ns0", ns0Prefixdfs);
                nsmgr.AddNamespace("ns1", ns1Prefixdfs);
                XmlNodeList infopathNodes = xmlDoc.SelectNodes("/ns0:RootWorkflowActivityWithData/ns1:MultiOutcomeActivity/ns1:MultiOutcomeInternal", nsmgr);
                foreach (XmlNode node in infopathNodes)
                {
                    var element = node as XmlElement;
                    if (element != null && element.HasAttribute("FormUrl"))
                    {
                        var formurl = element.Attributes["FormUrl"].Value;
                        var xmlDoc2 = new XmlDocument();
                        xmlDoc2.LoadXml(formurl);
                        if (xmlDoc2.DocumentElement.HasAttribute("PublishFolderServerRelativeUrl"))
                        {
                            var urlValue = xmlDoc2.DocumentElement.Attributes["PublishFolderServerRelativeUrl"].Value;
                            var replaceValue = AveReplaceProcessor.UrlReplace(urlValue, SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.SourceSiteInfo, siteUrl);
                            xmlDoc2.DocumentElement.Attributes["PublishFolderServerRelativeUrl"].Value = replaceValue;
                        }
                        element.Attributes["FormUrl"].Value = xmlDoc2.OuterXml;
                    }
                }
                return xmlDoc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Warn("An error occourred while Replace UrlForInfoPath.xml is:{0} Error:{1}", tempContentStr, e);
                return tempContentStr;
            }
        }
        private static string ReplaceUrlForEmail(string content)
        {
            if (SPWorkflowProcessorRuntime.MappingManager == null || SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager == null ||
    SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.DestSiteInfo == null)
            {
                return content;
            }
            try
            {
                AveSiteMappingManager siteMappingManager = SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager;
                string destSiteUrl = siteMappingManager.DestSiteInfo.ServerRelativeUrl;
                ReplaceOption option = new ReplaceOption(true, true, true);
                AveWorkflowReplaceProcessor processor = new AveWorkflowReplaceProcessor(siteMappingManager.SiteManagedMappings, option, siteMappingManager.SourceSiteInfo, destSiteUrl);

                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.InnerXml = content;

                //support more than one ns0:EmailActivity node.
                foreach (XmlNode node in xDoc.GetElementsByTagName("*"))
                {
                    if (node.Name.Equals("ns0:EmailActivity", StringComparison.OrdinalIgnoreCase))
                    {
                        XmlElement xe = (XmlElement)node;
                        if (xe.Attributes != null)
                        {
                            foreach (XmlAttribute attribute in xe.Attributes)
                            {
                                if (string.Equals(attribute.Name, "Body", StringComparison.OrdinalIgnoreCase))
                                {
                                    attribute.Value = processor.ReplaceEmailContent(attribute.Value);
                                }
                            }
                        }
                        //break;
                    }
                }
                return xDoc.InnerXml;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while replace url in workflow template files. Error: {0}", e);
                return content;
            }
        }

        private static void CheckUsedUserDefinedActionByContent(string wfConfigFileContent)
        {
            logger.Debug("Check used user define action in common content replace.");
            string GUIDREG = "[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}";
            string GUIDREG_WITH_HTML_ENCODE = "[A-F0-9]{8}(%2D[A-F0-9]{4}){3}%2D[A-F0-9]{12}";
            CheckUsedUserDefinedActionByContent("StaticId=\"", GUIDREG, wfConfigFileContent);
            CheckUsedUserDefinedActionByContent("StaticId=\"", GUIDREG_WITH_HTML_ENCODE, wfConfigFileContent);
        }

        private static void CheckUsedUserDefinedActionByContent(string prefix, string regKey, string strContent)
        {
            Regex reg = new Regex(prefix + regKey, RegexOptions.IgnoreCase);
            int startPos = 0;
            while (true)
            {
                var match = reg.Match(strContent, startPos);
                if (match.Success)
                {
                    startPos = match.Index + 1;
                    var guidStr = strContent.Substring(match.Index + prefix.Length, match.Length - prefix.Length);
                    if (!string.IsNullOrEmpty(guidStr))
                    {
                        // if guidStr contains html encode "%2d", change to '-'
                        guidStr = guidStr.Replace("%2d", "-").Replace("%2D", "-");
                        var guid = new Guid(guidStr);
                        if (!SPWorkflowProcessorRuntime.NeedRestoreUserDefiniedActionId.Contains(guid))
                        {
                            logger.Debug("Need restore user defined action with static id by content, id: {0}", guid.ToString());
                            SPWorkflowProcessorRuntime.NeedRestoreUserDefiniedActionId.Add(guid);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    internal sealed class RulesFileProc : SPWorkflowFileContentProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default Rules File Processor Replace");
            string strContent = base.ReplaceContent(dic);
            if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
            {
                strContent = ReplaceUserInSPDkflowRules(strContent);
            }
            string charSet = mFile.CharSetName;
            if (string.IsNullOrEmpty(charSet))
                charSet = "utf-8";
            try
            {
                if (mFile.Level != AveFileLevel.Checkout)
                {
                    mFile.CheckOut(false, string.Empty);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
                logger.Warn("An exception occurred while checkout file. exception:{0}", e.ToString());
            }

            mFile.SaveBinary(Encoding.GetEncoding(charSet).GetBytes(strContent));
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default Rules File Processor Replace");
            return strContent;
        }

        private string ReplaceUserInSPDkflowRules(string content)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(content);
                XmlNodeList nodes = doc.DocumentElement.GetElementsByTagName("ns0:CodePrimitiveExpression.Value");
                string originalStr = string.Empty;
                string pattern = string.Empty;
                int lenGroup = -1;
                int lenUser = -1;
                Regex regex = new Regex(string.Empty);
                foreach (XmlNode node in nodes)
                {
                    originalStr = node.InnerText;
                    if (originalStr.Contains("\\"))
                    {
                        lenGroup = originalStr.IndexOf('\\');
                        lenUser = originalStr.Substring(lenGroup + 1).Length;
                        pattern = @"([^\/\[\];=,\+\*\?<>@]){" + lenGroup + @"}\\{1}([^\/\[\]:;\|=,\+\*\?<>@]){" + lenUser + "}";
                        regex = new Regex(pattern);
                        if (regex.IsMatch(originalStr))
                        {
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(originalStr);
                            if (user != null)
                            {
                                node.InnerText = user.LoginName;
                            }
                        }
                    }
                }
                content = doc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, e);
            }
            finally
            {
                doc.RemoveAll();
            }
            return content;
        }
    }

    internal sealed class AspxFileProc : SPWorkflowFileContentProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default ASPX File Processor Replace");
            string strContent = string.Empty;
            //WebPartPropertiesProc wpProc = new WebPartPropertiesProc();
            //wpProc.ReplaceAllWebPartProperties(spFile, dic);
            strContent = base.ReplaceContent(dic);
            string charSet = mFile.CharSetName;
            if (string.IsNullOrEmpty(charSet))
                charSet = "utf-8";
            try
            {
                if (mFile.Level != AveFileLevel.Checkout)
                {
                    mFile.CheckOut(false, string.Empty);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
                logger.Log(AveLogLevel.DEBUG, "An error occurred while replacing content, error message: {0}", e);
            }

            mFile.SaveBinary(Encoding.GetEncoding(charSet).GetBytes(strContent));
            SPWorkflowProcessorRuntime.Log(Logs.FileContentProc_FileCharsetName, mFile.Name, charSet);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default ASPX File Processor Replace");
            return strContent;
        }
    }

    internal sealed class XamlFileProc : SPWorkflowFileContentProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default Xaml File Processor Replace");
            string strContent = Encoding.UTF8.GetString(OriginalContent);


            foreach (KeyValuePair<string, object> pair in dic)
            {
                int replacedCount = 0;
                strContent = LSUtilityOfBytes.LSReplaceStringIgnoreCase(strContent, pair.Key, pair.Value.ToString(), int.MaxValue, out replacedCount);
            }

            if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
            {
                strContent = base.ReplaceUserInSPDkflow(strContent);
            }
            strContent = ReplaceUsersFor13ModeWorkflow(strContent);

            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default Xaml File Processor Replace");
            return strContent;
        }

        private string ReplaceUsersFor13ModeWorkflow(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                XmlNamespaceManager xnsm = new XmlNamespaceManager(doc.NameTable);
                xnsm.AddNamespace("local", doc.DocumentElement.GetNamespaceOfPrefix("local"));
                xnsm.AddNamespace("local1", doc.DocumentElement.GetNamespaceOfPrefix("local1"));
                xnsm.AddNamespace("p", doc.DocumentElement.GetNamespaceOfPrefix("p"));
                xnsm.AddNamespace("p1", doc.DocumentElement.GetNamespaceOfPrefix("p1"));
                xnsm.AddNamespace("p2", doc.DocumentElement.GetNamespaceOfPrefix("p2"));
                ReplaceEqualUsers(doc, xnsm);
                ReplaceExpandInitFormUsers(doc, xnsm, new string[] { "local", "p" });
                ReplaceExpandInitFormUsers(doc, xnsm, new string[] { "p1", "p2" });
                return doc.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while ReplaceUsersFor13ModeWorkflow. /r/nXmlInfo:{0}, /r/nError:{1}", content, ex);
                return content;
            }
        }

        private void ReplaceEqualUsers(XmlDocument doc, XmlNamespaceManager xnsm)
        {

            try
            {
                var persons = doc.SelectNodes(".//local1:IsEqualUser", xnsm);
                if (persons != null)
                {
                    foreach (XmlNode person in persons)
                    {
                        try
                        {
                            var personElement = person as XmlElement;
                            if (personElement != null)
                            {

                                var userLogin = personElement.GetAttribute("Right");
                                if (!string.IsNullOrEmpty(userLogin))
                                {
                                    var user = SPPermissionProcessor.GetOrCreateUser(userLogin);
                                    if (user != null)
                                    {
                                        personElement.SetAttribute("Right", user.LoginName);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("An error occurred while replace single user in ReplaceEqualsUsersFor13ModeWorkflow.NodeInfo:{0},Error:{1}", person.OuterXml, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while ReplaceEqualsUsersFor13ModeWorkflow./r/nError:{0}", ex);
            }
        }

        private void ReplaceExpandInitFormUsers(XmlDocument doc, XmlNamespaceManager xnsm, string[] args)
        {

            try
            {
                XmlNodeList users = doc.SelectNodes(string.Format(".//{0}:ExpandInitFormUsers", args[0]), xnsm);
                foreach (XmlNode user in users)
                {
                    try
                    {
                        XmlNode userCollection = user.SelectSingleNode(string.Format(".//{0}:BuildCollection.Values", args[1]), xnsm);
                        if (userCollection != null && userCollection.ChildNodes.Count > 0)
                        {
                            foreach (XmlElement person in userCollection.ChildElements())
                            {
                                string oldLogin = person.InnerText.Trim();
                                IAveUser loginUser = SPPermissionProcessor.GetOrCreateUser(oldLogin);
                                if (loginUser != null)
                                {
                                    person.InnerText = loginUser.LoginName;
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An error occurred while replace single user in BuildCollection.NodeInfo:{0},Error:{1}", user.OuterXml, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while ReplaceExpandInitFormUsers./r/nError:{0}", ex);
            }
        }


    }

    internal class WebPartPropertiesProc
    {
        public void ReplaceAllWebPartProperties(IAveFile spFile, Dictionary<string, object> dic)
        {

            IAveLimitedWebPartManager wpManager = spFile.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared);
            foreach (IAveWebPart wp in wpManager.WebParts)
            {
                WebPartPropertiesProc instance = null;
                string typeId = wp.WebPartTypeID;// (Guid)LSInvoker.GetProperty(wp, "WebPartTypeID");
                switch (typeId.ToString().ToUpper(CultureInfo.InvariantCulture))
                {
                    case "":
                        break;
                    default:
                        break;
                }

                if (instance != null)
                    instance.ReplaceWebPartProperties(wp, dic);
            }
            wpManager.Dispose();
            wpManager.Web.Dispose();

        }

        public virtual void ReplaceWebPartProperties(IAveWebPart wp, Dictionary<string, object> dic)
        {

        }
    }

    internal sealed class DataFormWebPartProc : WebPartPropertiesProc
    {
        public override void ReplaceWebPartProperties(IAveWebPart wp, Dictionary<string, object> dic)
        {
            base.ReplaceWebPartProperties(wp, dic);
        }

    }
}
