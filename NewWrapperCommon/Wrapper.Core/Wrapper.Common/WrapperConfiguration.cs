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



namespace AvePoint.Wrapper.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Configuration;
    using System.IO;
    using AvePoint.Common;
    using System.Xml;
    using AvePoint.GCommon;
    using System.Text.RegularExpressions;
    using AvePoint.Common.FilterEngine;
    #endregion

    public class ProxyInfo
    {
        public ProxyInfo(string username, string password, string address, bool bypassProxyOnLocal = true, string[] bypassList = null)
        {
            if (!string.IsNullOrEmpty(username))
            {
                this.Username = username;
            }
            if (!string.IsNullOrEmpty(password))
            {
                this.Password = System.Text.Encoding.Unicode.GetString(AvePoint.GCommon.Utility.Cryptography.ConfigurationProtectionUtil.UnProtectWithBase64(password));
            }
            this.Address = address;
            BypassProxyOnLocal = bypassProxyOnLocal;
            BypassList = bypassList;
        }
        public bool BypassProxyOnLocal { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Address { get; private set; }
        public string[] BypassList { get; private set; }
        private string BypassString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (BypassList != null)
                {
                    foreach(var bypass in BypassList)
                    {
                        sb.Append(bypass + "      ");
                    }
                }
                return sb.ToString();
            }
        }
        public override string ToString()
        {
            return string.Format("bypass local address: {0} , UserName {1} , address {2}, Bypass: {3}", BypassProxyOnLocal, Username, Address, BypassString);
        }
    }

    public partial class WrapperConfiguration
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(WrapperConfiguration));

        private const string configurationFileName = "AgentCommonWrapperConfig.config";
        public static WrapperConfigurationForBPOSS BPOS_S = null;

        private static FileSystemWatcher configurationFileWatcher = null;

        #region Query Timeout
        public static int QueryServiceCommandTimeout = 5 * 60;
        public static int QueryServiceConnectTimeout = 3 * 60;
        #endregion

        #region AppToken
        public static string TenantId = "";
        public static string ClientId = "";
        public static bool UseAppToken = false;
        public static string CertificateFilepath = "";
        public static string CertificateFilePassword = "";
        #endregion

        public static string UserAgentTag = string.Empty;

        public static bool IsMonitorEnable = false;
        public static int MonitorLogFileSize = 5;
        public static int MonitorLogFileCount = 5;
        public static int CheckInterval = 10;
        public static bool DeleteDestinationUserProfileLink = false;// merge from ci ADO-204795, 目前只给SPM用，SPM自行赋值，不走wrapper的配置文件。
        public static bool ReplaceUserPrefix = true;
        public static bool InfoPathReplaceRelativeUrl = false;
        public static bool BackupWebPartPropertiesAsDic = false;
        public static bool BackupOnlineUserResource = true;//Modern site默认勾选所有alternate language，严重影响效率，加此option控制。
        
        public static bool UseStubAccessTimeRule = false;

        public static bool RestoredAllWebProperties = false;
        public static string SpecialWebPropertyNames = string.Empty;
        public static bool RestorePortalConnection = true;

        public static bool ForceFoundationModel = false;

        public static bool KeepDocumentIdValue = true;
        public static bool UpdateLookupColumnValueBeforePost = false;
        public static bool OverwriteDocIdPrefix = false;
        public static bool GenerateTokenDirectly = false;
        public static bool QueryVersionByNative = true;
        public static bool FindContentTypeByResourceFolder = true;

        public static bool NeedToUploadIndex { get; set; }
        public static Dictionary<string, string> workflowIneternalNameMapping = new Dictionary<string, string>();

        public static bool DebugNintexWorkflowMigration { get; private set; }

        public static bool PublishNintexWorkflowWithAPI { get; private set; }

        public static RecordFilterPolicyLog RecordFilterPolicyLog = RecordFilterPolicyLog.None;

        public static UniqueFieldSolution UniqueFieldSolution = UniqueFieldSolution.Skip;

        //Added to release the byte[] object of attachment in SP2013
        public static int ReleaseAttachmentSizeThreshold = 3 * 1024 * 1024;

        public static bool RestoreDefaultContentTypeRequiredProperty = false;

        public static int ZipLevel = 0;
        public static int AutoZipTriggerSize = 1 << 8;

        public static bool IsProxyEnabled = false;

        public static ProxyInfo ProxyInfo = null;
        public static int CheckAppInstanceInstalledTime = 60;

        public static bool KeepVersionSettingDuringRestore;

        public static int MaxConnectionCount;
        public static bool EnableStackInfo = false;

        //BPOS information  httpwebrequest timeout for upload file
        public static int UpLoadFileStreamTimeout = 30 * 60;//use second as unit and set default as 100 the same as httpwebrequest default timeout value

        public static bool IgnoreDiscoverModifiedBySystem = false;

        public static XmlNode FeatureMapping = null;

        /// <summary>
        /// 在AveXmlSerializer序列化过程中是否替换无法encoding的字符
        /// </summary>
        public static bool ReplaceSurrogateChar { get; set; }

        /// <summary>
        /// 是否使用新的URLReplaceProcessor
        /// true: use
        /// false: not use
        /// default: false
        /// </summary>
        public static bool UseNewUrlReplaceProcessor { get; set; }

        /// <summary>
        /// 记录使用新的UrlReplaceProcessor时 一些输出信息
        /// </summary>
        public static bool RecordNewUrlReplaceProcessorMessage { get; set; }

        /// <summary>
        /// For SP2016 file upload size of slice.
        /// </summary>
        public static int EachUploadSliceSize { get; set; }

        /// <summary>
        /// 控制是否添加[], 默认为True.ADO-187109
        /// </summary>
        public static bool AddBracketsForFormula { get; set; }

        public static bool IsProcessApprovalDatasOnly { get; set; }

        //目前只是过滤一些会导致目的端站点不可用的WebPart，WebPart功能不可用的不在此范围内
        public static List<string> SkipInWssWebPartLists = new List<string>()
                    {
                        "Microsoft.SharePoint.Portal.WebControls.IndicatorWebpart",
                        "Microsoft.SharePoint.Portal.WebControls.KPIListWebPart",
                        "Microsoft.SharePoint.Portal.WebControls.RSSAggregatorWebPart",
                        "Microsoft.SharePoint.Portal.WebControls.SiteDocuments",
                        "Microsoft.SharePoint.Publishing.WebControls.SummaryLinkWebPart",
                        "Microsoft.SharePoint.Publishing.WebControls.TableOfContentsWebPart",
                        "Microsoft.SharePoint.Taxonomy.TermProperty",
                        "Microsoft.SharePoint.Publishing.WebControls.MediaWebPart",
                        "Microsoft.SharePoint.Portal.WebControls.ProfileBrowser",
                        "Microsoft.SharePoint.Portal.WebControls.SocialCommentWebPart",
                        "Microsoft.SharePoint.Portal.WebControls.TagCloudWebPart",
                        "Microsoft.SharePoint.Portal.WebControls.SiteFeedWebPart"
                    };
        /// <summary>
        /// 控制online到local的webpart是否还原。 目前只支持目的端是Local 13的case。ADO-203910
        /// </summary>
        public static bool RestoreWebPartFromOnlineToLocal = false;
        /// <summary>
        /// Only for Replicator.
        /// </summary>
        public static bool GetLookupItemByLeafNameDuringRestore = false;

        public static List<Guid> ActivateFeatureIdsByClient = new List<Guid>() 
        { 
            AveSP2010FeatureDefinitions.NintexWorkflow, 
            AveSP2010FeatureDefinitions.NintexWorkflowInfoPath, 
            AveSP2010FeatureDefinitions.NintexWorkflowContentTypeUpgrade,
            AveSP2010FeatureDefinitions.NintexWorkflowWebParts,
            new Guid("53164b55-e60f-4bed-b582-a87da32b92f1"),
            new Guid("54668547-c03f-4bb5-aaab-d9568ebaf9c9"),
            AveSP2010FeatureDefinitions.NintexWorkflowWeb,
            new Guid("2fb9d5df-2fb5-403d-b155-535c256be1dc")
        };

        #region -- Wrapper Common --
        public static AveOpenBinaryOptions OpenBinaryOptions = AveOpenBinaryOptions.Unprotected;
        public static WrapperNativeApiPermission DefaultNativePermissionLevel = WrapperNativeApiPermission.FullControl;
        public static bool VerifyNativePermissionAutomatically = true;
        #endregion

        static WrapperConfiguration()
        {
            Init();
        }

        private static void Init()
        {

            LoadConfig();
            RegistWatchEvent();
        }

        private static void RegistWatchEvent()
        {
            try
            {
                string congfilePath = string.IsNullOrEmpty(AveEnv.AgentBinFolder) ?
                configurationFileName : Path.Combine(AveEnv.AgentBinFolder, configurationFileName);
                FileInfo configurationInfo = new FileInfo(congfilePath);
                if (configurationFileWatcher != null)
                {
                    configurationFileWatcher.EnableRaisingEvents = false;
                }
                else
                {
                    configurationFileWatcher = new FileSystemWatcher();
                    configurationFileWatcher.Changed += new FileSystemEventHandler(ConfigRationChange);
                    configurationFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                }
                configurationFileWatcher.Path = configurationInfo.DirectoryName;
                configurationFileWatcher.Filter = configurationInfo.Name;
                configurationFileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                mLog.Error("Initializing file {0} watcher failed. Error: {1}.", AgentConstants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig, ex.ToString());
            }
        }

        private static void ConfigRationChange(object sender, FileSystemEventArgs e)
        {
            configurationFileWatcher.WaitForChanged(WatcherChangeTypes.Changed, 2000);
            LoadConfig();
        }

        private static void LoadConfig()
        {
            try
            {
                string congfilePath = string.IsNullOrEmpty(AveEnv.AgentBinFolder) ?
                configurationFileName : Path.Combine(AveEnv.AgentBinFolder, configurationFileName);
                bool changed = false;
                XmlDocument xmlDoc = new XmlDocument();

                if (!File.Exists(congfilePath))
                {
                    xmlDoc.LoadXml("<configuration />");
                    changed = true;
                }
                else
                {
                    xmlDoc.Load(congfilePath);
                }

                XmlNode queryNode = EnsureXmlNode(xmlDoc.DocumentElement, "WrapperQueryService", ref changed);
                QueryServiceCommandTimeout = GetConfigrationFromNode(queryNode, "CommandTimeout", QueryServiceCommandTimeout, ref changed);
                QueryServiceConnectTimeout = GetConfigrationFromNode(queryNode, "ConnectTimeout", QueryServiceConnectTimeout, ref changed);

                XmlNode commNode = EnsureXmlNode(xmlDoc.DocumentElement, "WrapperCommon", ref changed);
                InitWFInternalMapping(commNode, GetConfigrationFromNode(commNode, "WorkflowInternalNameMap", "", ref changed));
                OpenBinaryOptions = (AveOpenBinaryOptions)GetConfigrationFromNode(commNode, "OpenBinaryOptions", (int)OpenBinaryOptions, ref changed);
                ForceFoundationModel = GetConfigrationFromNode(commNode, "ForceFoundationModel", false, ref changed);
                RestoredAllWebProperties = GetConfigrationFromNode(commNode, "RestoredAllWebProperties", false, ref changed);
                SpecialWebPropertyNames = GetConfigrationFromNode(commNode, "SpecialWebPropertyNames", string.Empty, ref changed);
                UseStubAccessTimeRule = GetConfigrationFromNode(commNode, "UseStubAccessTimeRule", false, ref changed);
                ReplaceUserPrefix = GetConfigrationFromNode(commNode, "ReplaceUserPrefix", true, ref changed);
                InfoPathReplaceRelativeUrl = GetConfigrationFromNode(commNode, "InfoPathReplaceRelativeUrl", false, ref changed);
                BackupWebPartPropertiesAsDic = GetConfigrationFromNode(commNode, "BackupWebPartPropertiesAsDic", false, ref changed);
                BackupOnlineUserResource = GetConfigrationFromNode(commNode, "BackupOnlineUserResource", true, ref changed);

                KeepVersionSettingDuringRestore = GetConfigrationFromNode(commNode, "KeepVersionSettingDuringRestore", false, ref changed);
                UseNewUrlReplaceProcessor = GetConfigrationFromNode(commNode, "UseNewUrlReplaceProcessor", true, ref changed);
                RecordNewUrlReplaceProcessorMessage = GetConfigrationFromNode(commNode, "RecordNewUrlReplaceProcessorMessage", true, ref changed);
                EachUploadSliceSize = GetConfigrationFromNode(commNode, "EachUploadSliceSize", 8, ref changed);
                QueryVersionByNative = GetConfigrationFromNode(commNode, "QueryVersionByNative", true, ref changed);
                UserAgentTag = GetConfigrationFromNode(commNode, "UserAgentTag", "ISV|AvePoint|DocAve/{0}", ref changed);
                // 兼容老数据，将老数据中的value替换成新的value。
                if(string.Equals(UserAgentTag, "AvePoint", StringComparison.OrdinalIgnoreCase))
                {
                    UserAgentTag = "ISV|AvePoint|DocAve/{0}";
                    changed = SetConfigrationToNode(commNode, "UserAgentTag", UserAgentTag);
                }
                if (UserAgentTag.IndexOf("{0}", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(WrapperConfiguration).Assembly.Location);
                    UserAgentTag = string.Format(UserAgentTag, string.Concat(fileVersion.ProductMajorPart, ".", fileVersion.ProductMinorPart));
                }
                AddBracketsForFormula = GetConfigrationFromNode(commNode, "AddBracketsForFormula", true, ref changed);
                if (EachUploadSliceSize <= 0 || EachUploadSliceSize > 2047)
                {
                    EachUploadSliceSize = 8;
                }
                #region  Generate Apptoken
                TenantId = GetConfigrationFromNode(commNode, "TenantId", "", ref changed);
                ClientId = GetConfigrationFromNode(commNode, "ClientId", "", ref changed);
                UseAppToken = GetConfigrationFromNode(commNode, "UseAppToken", false, ref changed);
                CertificateFilepath = GetConfigrationFromNode(commNode, "CertificateFilepath", "", ref changed);
                CertificateFilePassword = GetConfigrationFromNode(commNode, "CertificateFilePassword", "", ref changed);
                
                #endregion
                try
                {
                    RecordFilterPolicyLog = (RecordFilterPolicyLog)Enum.Parse(typeof(RecordFilterPolicyLog),
                        GetConfigrationFromNode(commNode, "RecordFilterPolicyLog", "None", ref changed));
                    DefaultNativePermissionLevel =
                        (WrapperNativeApiPermission)Enum.Parse(typeof(WrapperNativeApiPermission),
                        GetConfigrationFromNode(commNode, "DefaultNativePermissionLevel", DefaultNativePermissionLevel.ToString(),
                        ref changed));
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, "Wrapper Configuration file settings wrong at {0}", e.ToString());
                }
                VerifyNativePermissionAutomatically =
                    GetConfigrationFromNode(commNode, "VerifyNativePermissionAutomatically", VerifyNativePermissionAutomatically,
                    ref changed);
                ReplaceSurrogateChar = GetConfigrationFromNode(commNode, "ReplaceSurrogateChar", false, ref changed);

                RestorePortalConnection = GetConfigrationFromNode(commNode, "RestorePortalConnection", true, ref changed);
                KeepDocumentIdValue = GetConfigrationFromNode(commNode, "KeepDocumentIdValue", true, ref changed);
                UpdateLookupColumnValueBeforePost = GetConfigrationFromNode(commNode, "UpdateLookupColumnValueBeforePost", false, ref changed);
                OverwriteDocIdPrefix = GetConfigrationFromNode(commNode, "OverwriteDocIdPrefix", false, ref changed);
                DebugNintexWorkflowMigration = GetConfigrationFromNode(commNode, "DebugNintexWorkflowMigration", false, ref changed);
                PublishNintexWorkflowWithAPI = GetConfigrationFromNode(commNode, "PublishNintexWorkflowWithAPI", false, ref changed);
                UniqueFieldSolution = (UniqueFieldSolution)GetConfigrationFromNode(commNode, "UniqueFieldResolution", 1, ref changed);
                SkipInWssWebPartLists = GetConfigrationFromNode(commNode, "SkipInWss2013WebPartLists", SkipInWssWebPartLists, "WebPartType", ref changed);
                //<ActivateFeatureIdsByClient>
                //    <ID>53164b55-e60f-4bed-b582-a87da32b92f1</ID>
                //    <ID>54668547-c03f-4bb5-aaab-d9568ebaf9c9</ID>
                //</ActivateFeatureIdsByClient>
                ActivateFeatureIdsByClient = GetConfigrationFromNode(commNode, "ActivateFeatureIdsByClient", ActivateFeatureIdsByClient, "ID", ref changed);

                CheckAppInstanceInstalledTime = GetConfigrationFromNode(commNode, "CheckAppInstanceInstalledTime", 60, ref changed);

                MaxConnectionCount = GetConfigrationFromNode(commNode, "MaxConnectionCount", 100, ref changed);
                EnableStackInfo = GetConfigrationFromNode(commNode, "EnableStackInfo", false, ref changed);

                InitBPOSS(xmlDoc.DocumentElement, true, ref changed);
                RestoreDefaultContentTypeRequiredProperty = GetConfigrationFromNode(commNode, "RestoreDefaultContentTypeRequiredProperty", false, ref changed);
                //BPOS relative information this is add for upload big binary data file ADO-30260
                XmlNode bposNode = EnsureXmlNode(xmlDoc.DocumentElement, "WrapperBPOS", ref changed);
                UpLoadFileStreamTimeout = GetConfigrationFromNode(bposNode, "UpLoadFileStreamTimeout", UpLoadFileStreamTimeout, ref changed);

                XmlNode proxyNode = EnsureXmlNode(xmlDoc.DocumentElement, "ProxySettings", ref changed);
                IsProxyEnabled = GetConfigrationFromNode(proxyNode, "EnableProxy", false, ref changed);
                string server = GetConfigrationFromNode(proxyNode, "Server", "", ref changed);
                string username = GetConfigrationFromNode(proxyNode, "Username", "", ref changed);
                string password = GetConfigrationFromNode(proxyNode, "Password", "", ref changed);
                bool bypassProxyOnLocal = GetConfigrationFromNode(proxyNode, "BypassProxyOnLocal", true, ref changed);
                string[] bypassList = GetConfigrationFromNode(proxyNode, "BypassList", new List<string>(), "address", ref changed).ToArray();
                GenerateTokenDirectly = GetConfigrationFromNode(commNode, "GenerateTokenDirectly", false, ref changed);
                ZipLevel = GetConfigrationFromNode(commNode, "ZipLevel", 0, ref changed);

                if (IsProxyEnabled)
                {
                    ProxyInfo = new ProxyInfo(username, password, server, bypassProxyOnLocal, bypassList);
                }

                FeatureMapping = EnsureXmlNode(xmlDoc.DocumentElement, "FeatureMapping", ref changed);

                if (changed)
                {
                    xmlDoc.Save(congfilePath);
                }
            }
            catch (Exception ex)
            {
                mLog.Debug("An error occurred when init Wrapper Configuration; Message: {0}", ex.ToString());
            }
        }

        private static void InitWFInternalMapping(XmlNode commNode, string mappingsStr)
        {
            if (!string.IsNullOrEmpty(mappingsStr))
            {
                try
                {
                    var mappings = mappingsStr.Split(';');
                    foreach (var map in mappings)
                    {
                        var pair = map.Split(',');
                        workflowIneternalNameMapping[pair[0]] = pair[1];
                    }
                }
                catch (Exception ex)
                {
                    mLog.Debug("An error occurred when InitWFInternalMapping; Message: {0}", ex.ToString());
                }
            }
        }
        
        public static void InitBPOSS(XmlElement config, bool includeVersion, ref bool changed)
        {
            if (BPOS_S == null)
            {
                BPOS_S = new WrapperConfigurationForBPOSS();
            }
            BPOS_S.Init(config, includeVersion, ref changed);
        }

        /// <summary>
        /// Used to reload proxy settings
        /// </summary>
        public static void InitProxyParameters()
        {
            string congfilePath = string.IsNullOrEmpty(AveEnv.AgentBinFolder) ?
            configurationFileName : Path.Combine(AveEnv.AgentBinFolder, configurationFileName);
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                if (!File.Exists(congfilePath))
                {
                    return;
                }
                xmlDoc.Load(congfilePath);

                XmlNode proxyNode = GetXmlNode(xmlDoc.DocumentElement, "ProxySettings");
                IsProxyEnabled = bool.Parse(GetXmlNode(proxyNode, "EnableProxy").InnerText);
                if (IsProxyEnabled)
                {
                    string server = GetXmlNode(proxyNode, "Server").InnerText;
                    string username = GetXmlNode(proxyNode, "Username").InnerText;
                    string password = GetXmlNode(proxyNode, "Password").InnerText;
                    bool changed = false;
                    bool bypassProxyOnLocal = GetConfigrationFromNode(proxyNode, "BypassProxyOnLocal", true, ref changed);
                    string[] bypassList = GetConfigrationFromNode(proxyNode, "BypassList", new List<string>(), "address", ref changed).ToArray();
                    ProxyInfo = new ProxyInfo(username, password, server, bypassProxyOnLocal, bypassList);
                }
                else
                {
                    ProxyInfo = null;
                }
            }
            catch (Exception ex)
            {
                mLog.Debug("An error occurred when init proxy settings; Message: {0}", ex.ToString());
            }
        }

        private static XmlNode GetXmlNode(XmlNode xmlElement, string nodeName)
        {
            string nodePath = ".//*[name()='" + nodeName + "']";
            return xmlElement.SelectSingleNode(nodePath);
        }

        public static XmlNode EnsureXmlNode(XmlElement node, string name, ref bool changed)
        {
            XmlNode subNode = null;

            foreach (XmlNode tempNode in node.ChildNodes)
            {
                if (tempNode.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    subNode = tempNode;
                    break;
                }
            }

            if (subNode == null)
            {
                subNode = node.OwnerDocument.CreateElement(name);
                node.AppendChild(subNode);
                changed = true;
            }

            return subNode;
        }

        public static string GetConfigrationFromNode(XmlNode node, string subNodeName, string defaultValue, ref bool changed)
        {
            string result = defaultValue;

            XmlNode subNode = null;

            foreach (XmlNode tempNode in node.ChildNodes)
            {
                if (tempNode.Name.Equals(subNodeName, StringComparison.OrdinalIgnoreCase))
                {
                    subNode = tempNode;
                    break;
                }
            }

            if (subNode == null)
            {
                subNode = node.OwnerDocument.CreateElement(subNodeName);
                subNode.InnerText = defaultValue;
                node.AppendChild(subNode);
                changed = true;
            }
            else
            {
                result = subNode.InnerText.Trim();
            }

            return result;
        }

        public static bool SetConfigrationToNode(XmlNode node, string subNodeName, string value)
        {
            XmlNode subNode = null;

            foreach (XmlNode tempNode in node.ChildNodes)
            {
                if (tempNode.Name.Equals(subNodeName, StringComparison.OrdinalIgnoreCase))
                {
                    subNode = tempNode;
                    break;
                }
            }

            if (subNode == null)
            {
                subNode = node.OwnerDocument.CreateElement(subNodeName);
                subNode.InnerText = value;
                node.AppendChild(subNode);
            }
            else
            {
                subNode.InnerText = value;
            }
            return true;
        }

        public static bool GetConfigrationFromNode(XmlNode node, string subNodeName, bool defaultValue, ref bool changed)
        {
            return bool.Parse(GetConfigrationFromNode(node, subNodeName, defaultValue.ToString(), ref changed));
        }

        public static int GetConfigrationFromNode(XmlNode node, string subNodeName, int defaultValue, ref bool changed)
        {
            return int.Parse(GetConfigrationFromNode(node, subNodeName, defaultValue.ToString(), ref changed));
        }

        public static List<string> GetConfigrationFromNode(XmlNode node, string subNodeName, List<string> defaultValue, string subNodeChildName, ref bool changed)
        {
            XmlNode subNode = null;
            List<string> result = defaultValue;

            foreach (XmlNode tempNode in node.ChildNodes)
            {
                if (tempNode.Name.Equals(subNodeName, StringComparison.OrdinalIgnoreCase))
                {
                    subNode = tempNode;
                    break;
                }
            }
            if (subNode == null)
            {
                subNode = node.OwnerDocument.CreateElement(subNodeName);
                foreach (string value in defaultValue)
                {
                    XmlNode tmp = node.OwnerDocument.CreateElement(subNodeChildName);
                    tmp.InnerText = value;
                    subNode.AppendChild(tmp);
                }
                node.AppendChild(subNode);
                changed = true;
            }
            else
            {
                foreach (XmlNode type in subNode)
                {
                    if (!result.Contains(type.InnerText))
                    {
                        result.Add(type.InnerText);
                    }
                }
            }

            return result;
        }

        public static List<Guid> GetConfigrationFromNode(XmlNode node, string subNodeName, List<Guid> defaultValue, string subNodeChildName, ref bool changed)
        {
            XmlNode subNode = null;
            var result = new List<Guid>();

            foreach (XmlNode tempNode in node.ChildNodes)
            {
                if (tempNode.Name.Equals(subNodeName, StringComparison.OrdinalIgnoreCase))
                {
                    subNode = tempNode;
                    break;
                }
            }
            if (subNode == null)
            {
                result.AddRange(defaultValue);
                subNode = node.OwnerDocument.CreateElement(subNodeName);
                foreach (var value in defaultValue)
                {
                    XmlNode tmp = node.OwnerDocument.CreateElement(subNodeChildName);
                    tmp.InnerText = value.ToString();
                    subNode.AppendChild(tmp);
                }
                node.AppendChild(subNode);
                changed = true;
            }
            else
            {
                foreach (XmlNode type in subNode)
                {
                    if (!string.IsNullOrEmpty(type.InnerText))
                    {
                        var id = new Guid(type.InnerText);
                        if (!result.Contains(id))
                        {
                            result.Add(id);
                        }
                    }
                }
            }

            return result;
        }


        public static string GetAttributeFromNode(XmlNode node, string name, string defaultValue, ref bool changed)
        {
            string result = defaultValue;
            XmlAttribute tempattribute = null;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    tempattribute = attribute;
                    break;
                }
            }

            if (tempattribute == null)
            {
                XmlAttribute attribute = node.OwnerDocument.CreateAttribute(name);
                attribute.Value = defaultValue;
                node.Attributes.Append(attribute);
                changed = true;
            }
            else
            {
                result = tempattribute.Value.Trim();
            } 

            return result;
        }

        public static bool GetAttributeFromNode(XmlNode node, string name, bool defaultValue, ref bool changed)
        {
            bool result;
            if (bool.TryParse(GetAttributeFromNode(node, name, defaultValue.ToString(), ref changed), out result))
            {
                return result;
            }
            return defaultValue;
        }

        public static int GetAttributeFromNode(XmlNode node, string name, int defaultValue, ref bool changed)
        {
            int result;
            if (int.TryParse(GetAttributeFromNode(node, name, defaultValue.ToString(), ref changed), out result))
            {
                return result;
            }
            return defaultValue;
        }

        public static void AddInterActiveTag()
        {
            lock (UserAgentTag)
            {
                var index = UserAgentTag.IndexOf("|Interactive");
                if (index < 0)
                {
                    UserAgentTag = UserAgentTag + "|Interactive";
                }
            }
        }

        public static void RemoveInterActiveTag()
        {
            lock (UserAgentTag)
            {
                var index = UserAgentTag.IndexOf("|Interactive");
                if (index > 0)
                {
                    UserAgentTag = UserAgentTag.Substring(0, index);
                }
            }
        }

    }
}

public enum UniqueFieldSolution
{
    Skip = 1,
    Overwrite = 2,
    Continue = 3
}