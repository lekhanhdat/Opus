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
using AvePoint.Common;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.SharePoint.Common
{
    public class ArchiveConfigFileInfo
    {
        private static readonly RALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public XmlElement ArchiveDel { get; private set; }

        public List<int> ListTemplateTable = new List<int>();

        public bool IsDeleteRecord;

        public bool IsDeleteLinkFile;

        public bool IsOpenPerformanceLog;

        public ExportSetting ExportConfig;

        public CAUrlSetting CAUrlSetting;

        public int DeletionCacheSize;

        public int ConflictOption;

        public bool ForcedServerAPIDiscover;

        public bool IsCheckModifyTime;

        public int LifeCycleCacheSize;

        public List<string> SkipExtentionName = new List<string>();

        public int MultiProcessCount;

        public bool SingleJobInPool = true;

        public bool IsBackupUserProfile;

        public string VEOType = string.Empty;

        public bool EndUserAdvanceMode = false;
        //public string CAUrl = string.Empty;

        //默认情况备份NewsFeed
        public bool IsBackupNewsfeed = true;

        //高级别Rule Keep 操作是否包含底层Item, Default = false;
        public bool IncludeItemInContainerRule = false;

        public List<string> DisplayColumns;

        public List<string> RADisplayColumns;

        //默认备份Connector Library中 LinkFile的真实Content
        public bool BackupLinkFileRealContent = true;

        //默认备份Metadata Service
        public bool BackupManagedMetadata = true;

        //Record Manager默认不备份ManagedMetadata
        public bool RecordManagerBackupManagedMetadata = false;

        public bool KeepModeration = true;

        public bool IsSyncItemPermission;

        //ADO-193383 添加FSA Stub 后缀名读取配置文件逻辑
        public string FSAStubNameFormat = string.Empty;

        public string MovetoLinkFileInfo = string.Empty;

        public bool UseDocumentIDAsLandingPageURL = false;

        public string UserAgentTag = string.Empty;

        public bool IsDeleteLabelItem = false;

        public int RecordsRelatedJobTimeOut;

        public bool SPToSPMoveAllVersion = true;

        public ArchiveConfigFileInfo()
        {
            XmlDocument envDoc = new XmlDocument();
            envDoc.Load(SOCommonObjects.SOConfigurationFilePath);
            mLog.Info("SOConfigurationFilePath is: {0}.", SOCommonObjects.SOConfigurationFilePath);
            ArchiveDel = (XmlElement)envDoc.DocumentElement.SelectSingleNode("Archive");
            IsDeleteRecord = GetIsDeleteRecord();
            IsDeleteLinkFile = GetIsDeleteLinkFile();
            GetListTemplate(ref ListTemplateTable);
            //IsOpenPerformanceLog = GetIsOpenPerformanceLog(envDoc);
            ExportConfig = GetExportSetting();
            CAUrlSetting = GetCAUrlSetting();
            DeletionCacheSize = GetDeletionCacheSize();
            ConflictOption = GetConflictOption();
            ForcedServerAPIDiscover = GetForcedServerAPIDiscover();
            IsCheckModifyTime = GetIsCheckModifyTime();
            LifeCycleCacheSize = GetLifeCycleItemsCacheSize();
            SkipExtentionName = GetSkipExtentionName();
            MultiProcessCount = GetMultiThreadItemCount();
            //SingleJobInPool = IsUseMultipleProcess(envDoc);
            IsBackupUserProfile = GetIsBackupUserProfile();
            VEOType = GetVEOType();
            EndUserAdvanceMode = GetEndUserAdvanceMode();
            //CAUrl = GetCAUrl();
            IsBackupNewsfeed = GetIsBackupNewsfeed();
            IncludeItemInContainerRule = GetIncludeItemInContainerRule();
            DisplayColumns = GetDisplayColumnNames();
            RADisplayColumns = GetRADisplayColumnNames();
            BackupLinkFileRealContent = GetBackupLinkFileRealContent();
            BackupManagedMetadata = GetBackupManagedMetadata();
            RecordManagerBackupManagedMetadata = GetRecordManagerBackupManagedMetadata();
            KeepModeration = GetKeepModeration();
            IsSyncItemPermission = GetIsSyncItemPermission();
            FSAStubNameFormat = GetFSAStubNameFormat();
            MovetoLinkFileInfo = GetMoveToActionStubFileContent();
            UseDocumentIDAsLandingPageURL = GetUseDocumentIDAsLandingPageURL();
            UserAgentTag = GetUserAgentTag();
            IsDeleteLabelItem = GetIsDeleteLabelItem();
            RecordsRelatedJobTimeOut = GetRecordsRelatedJobTimeOut();
            SPToSPMoveAllVersion = GetSPToSPMoveAllVersion();
        }
        private string GetConfigFile(string key)
        {
            return ArchiveDel.GetAttribute(key);
        }
        //if reture true :  delete the ApproveDB
        public bool KeepApproveDB()
        {
            if (GetConfigFile("keepApproveDatabase") != string.Empty && GetConfigFile("keepApproveDatabase").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //if reture true : Do not delete the item 
        public bool KeepDocument(string itemName)
        {
            foreach (string extention in SkipExtentionName)
            {
                if (itemName.EndsWith(extention, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        //the skip file's extention name ToLower
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        private List<string> GetSkipExtentionName()
        {
            StringBuilder sb = new StringBuilder();
            List<string> skipExtentionName = new List<string>();
            string[] skip = GetConfigFile("skip").Split(' ');
            for (int i = 0; i < skip.Count(); i++)
            {
                if (skip[i] != string.Empty)
                {
                    skipExtentionName.Add(skip[i].ToLower());
                    sb.Append(skip[i].ToLower());
                }
            }
            mLog.Info("SkipExtensionName value is :{0}", sb.ToString());
            return skipExtentionName;
        }

        //if reture true : Do not delete the container structure 
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "config file")]
        public bool KeepContainerStructure()
        {
            if (GetConfigFile("keepSharepointStructure") != string.Empty && GetConfigFile("keepSharepointStructure").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void GetListTemplate(ref List<int> listTemplate)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < GetConfigFile("listTemplate").Split(' ').Count(); i++)
            {
                listTemplate.Add(int.Parse(GetConfigFile("listTemplate").Split(' ')[i]));
                sb.Append(int.Parse(GetConfigFile("listTemplate").Split(' ')[i]) + ";");
            }
            mLog.Info("ListTemplate value is :{0}", sb.ToString());
        }

        private bool GetIsDeleteRecord()
        {
            bool IsDeleteRecord = false;
            try
            {
                if (GetConfigFile("IsDeleteRecord") != string.Empty && GetConfigFile("IsDeleteRecord").Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    IsDeleteRecord = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: IsDeleteRecord" + ex.ToString());
                IsDeleteRecord = false;
            }
            mLog.Info("IsDeleteRecord value is :{0}", IsDeleteRecord.ToString());
            return IsDeleteRecord;
        }
        private bool GetIsDeleteLinkFile()
        {
            bool IsDeleteLinkFile = false;
            try
            {
                if (GetConfigFile("IsDeleteLinkFile") != string.Empty && GetConfigFile("IsDeleteLinkFile").Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    IsDeleteLinkFile = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: IsDeleteLinkFile" + ex.ToString());
                IsDeleteLinkFile = false;
            }
            mLog.Info("IsDeleteLinkFile value is :{0}", IsDeleteLinkFile.ToString());
            return IsDeleteLinkFile;
        }
        //private bool GetIsOpenPerformanceLog(XmlDocument xd)
        //{
        //    try
        //    {
        //        IsOpenPerformanceLog = bool.Parse(xd.DocumentElement.GetAttribute(SOCommonObjects.ConfigurationOption.OpenPerformanceTimer));
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Warn("Error in Get Config Attribute: IsOpenPerformanceLog" + ex.ToString());
        //        IsOpenPerformanceLog = false;
        //    }
        //    mLog.Info("IsOpenPerformanceLog value is :{0}", IsOpenPerformanceLog.ToString());
        //    return IsOpenPerformanceLog;
        //}
        private CAUrlSetting GetCAUrlSetting()
        {
            CAUrlSetting caUrlSetting = new CAUrlSetting();
            Dictionary<string, string> caUrls = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();
            try
            {
                XmlElement caUrlElement = (XmlElement)ArchiveDel.SelectSingleNode(CAUrlSetting.CONFIG_CAURLSETTING);
                if (caUrlElement != null)
                {
                    if (caUrlElement.HasAttribute(CAUrlSetting.CONFIG_CAURL))
                    {
                        caUrls.Add(CAUrlSetting.CONFIG_CAURL, caUrlElement.GetAttribute(CAUrlSetting.CONFIG_CAURL));
                        sb.Append(caUrlElement.GetAttribute(CAUrlSetting.CONFIG_CAURL) + ";");
                    }
                    foreach (var node in caUrlElement.ChildNodes)
                    {
                        XmlElement xe = node as XmlElement;
                        if (xe.HasAttribute(CAUrlSetting.CONFIG_CAURL) && xe.HasAttribute(CAUrlSetting.CONFIG_URL))
                        {
                            if (!caUrls.ContainsKey(xe.GetAttribute(CAUrlSetting.CONFIG_URL)))
                            {
                                caUrls.Add(xe.GetAttribute(CAUrlSetting.CONFIG_URL), xe.GetAttribute(CAUrlSetting.CONFIG_CAURL));
                                sb.Append(xe.GetAttribute(CAUrlSetting.CONFIG_URL) + ";");
                            }
                        }
                    }
                }
                else
                {
                    mLog.Info("Archiver config file CentralAdminUrl node is empty.");
                }

            }
            catch (Exception ex)
            {
                mLog.Warn("Init local ca Url error :{0}", ex.ToString());
            }
            caUrlSetting.CaUrls = caUrls;
            mLog.Info("CentralAdminUrl value is :{0}", sb.ToString());
            return caUrlSetting;
        }

        private ExportSetting GetExportSetting()
        {
            ExportSetting exportSetting = new ExportSetting();
            XmlElement ExportSettingElement = (XmlElement)ArchiveDel.SelectSingleNode(ExportSetting.CONFIG_EXPORTSETTING);

            if (ExportSettingElement != null)
            {
                //读edrm节点
                XmlElement folderNameLengthSetting = (XmlElement)ExportSettingElement.SelectSingleNode(ExportSetting.CONFIG_EDRM);
                exportSetting.ManifestXmlSize = int.Parse(folderNameLengthSetting.GetAttribute(ExportSetting.CONFIG_MANIFESTXMLSETTING));
            }
            mLog.Info("ExportSetting ManifestXmlSize value is :{0}", exportSetting.ManifestXmlSize);
            return exportSetting;
        }

        private int GetDeletionCacheSize()
        {
            int size = 100000;
            string cacheSize = GetConfigFile("deleteCacheSize");
            if (cacheSize != string.Empty)
            {
                size = Convert.ToInt32(cacheSize);
            }
            mLog.Info("DeletionCacheSize value is :{0}", size);
            return size;
        }

        private int GetConflictOption()
        {
            int option = 0;
            try
            {
                if (GetConfigFile("RecordManagerConflictOption") != string.Empty)
                {
                    option = Convert.ToInt32(GetConfigFile("RecordManagerConflictOption"));
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: RecordManagerConflictOption" + ex.ToString());
                option = 0;
            }
            mLog.Info("RecordManagerConflictOption value is :{0}", option);
            return option;
        }

        private bool GetForcedServerAPIDiscover()
        {
            bool ForcedServerAPIDiscover = false;
            try
            {
                if (GetConfigFile("ForcedServerAPIDiscover").Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    ForcedServerAPIDiscover = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: ForcedServerAPIDiscover" + ex.ToString());
                ForcedServerAPIDiscover = false;
            }
            mLog.Info("ForcedServerAPIDiscover value is :{0}", ForcedServerAPIDiscover.ToString());
            return ForcedServerAPIDiscover;
        }

        private bool GetIsCheckModifyTime()
        {
            bool isCheckModifyTime = true;
            try
            {
                string value = GetConfigFile("isCheckModifyTime");
                if (value != string.Empty && value.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    isCheckModifyTime = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: isCheckModifyTime" + ex.ToString());
                isCheckModifyTime = true;
            }
            mLog.Info("IsCheckModifyTime value is :{0}", isCheckModifyTime.ToString());
            return isCheckModifyTime;
        }

        private int GetLifeCycleItemsCacheSize()
        {
            int size = 100000;
            try
            {
                string cacheSize = GetConfigFile("LifeCycleItemsCacheSize");
                if (cacheSize != string.Empty)
                {
                    size = Convert.ToInt32(cacheSize);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: LifeCycleItemsCacheSize" + ex.ToString());
            }
            mLog.Info("LifeCycleItemsCacheSize value is :{0}", size);
            return size;
        }

        private int GetMultiThreadItemCount()
        {
            int multiThreadItemCount = 1;
            try
            {
                string multiItemCount = GetConfigFile("MultiThreadItemCount");
                if (multiItemCount != string.Empty)
                {
                    multiThreadItemCount = Convert.ToInt32(multiItemCount);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: MultiThreadItemCount" + ex.ToString());
            }
            mLog.Info("MultiThreadItemCount value is :{0}", multiThreadItemCount);
            return multiThreadItemCount;
        }
        /// <summary>
        /// read the config file attributte 'UseMultipleProcess'
        /// </summary>
        /// <param name="xDoc"></param>
        /// <returns></returns>
        //private bool IsUseMultipleProcess(XmlDocument xDoc)
        //{
        //    try
        //    {
        //        if (xDoc.DocumentElement != null && !string.IsNullOrEmpty(xDoc.DocumentElement.GetAttribute(SOCommonObjects.ConfigurationOption.UseMultipleProcess)))
        //        {
        //            SingleJobInPool = bool.Parse(xDoc.DocumentElement.GetAttribute(SOCommonObjects.ConfigurationOption.UseMultipleProcess));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        mLog.Warn(e.ToString());
        //        return true;
        //    }
        //    mLog.Info("SingleJobInPool value is :{0}", SingleJobInPool);
        //    return SingleJobInPool;
        //}

        /// <summary>
        /// 配置文件控制备份user profile service
        /// backupUserprofile字段不允许修改，此字段修改会导致客户升级配置文件属性出错 ADO-160119
        /// </summary>
        /// <returns>true means backup</returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public bool GetIsBackupUserProfile()
        {
            bool backUpUserProfile = true;
            try
            {
                if (GetConfigFile("backupUserprofile") != string.Empty && GetConfigFile("backupUserprofile").Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    backUpUserProfile = false;
                }
                else
                {
                    backUpUserProfile = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Is Backup User Profile,Message: {0}." + ex.ToString());
                backUpUserProfile = true;
            }
            mLog.Info("backUpUserProfile value is :{0}", backUpUserProfile);
            return backUpUserProfile;
        }

        /// <summary>
        /// 获取VEO类型
        /// </summary>
        /// <returns></returns>
        public string GetVEOType()
        {
            string VEOType = string.Empty;
            try
            {
                VEOType = GetConfigFile("VEOType");
            }
            catch (Exception ex)
            {
                mLog.Warn("Can not Get Get VEO Type,Message: {0}." + ex.ToString());
            }
            mLog.Info("VEOType value is :{0}", VEOType);
            return VEOType;
        }
        public string GetCAUrl()
        {
            string caUrl = string.Empty;
            try
            {
                caUrl = GetConfigFile("centralAdminUrl");
            }
            catch (Exception ex)
            {
                mLog.Warn("Can not Get GetCAUrl,Message: {0}." + ex.ToString());
            }
            mLog.Info("centralAdminUrl value is :{0}", caUrl);
            return caUrl;
        }

        /// <summary>
        /// Get EndUser AdvanceMode setting & Default value is false
        /// </summary>
        /// <returns></returns>
        private bool GetEndUserAdvanceMode()
        {
            bool endUserAdvanceMode = false;
            try
            {
                string value = GetConfigFile("EndUserArchiverAdvancedMode");
                if (value != string.Empty && value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    endUserAdvanceMode = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: EndUserArchiverAdvancedMode" + ex.ToString());
                endUserAdvanceMode = false;
            }
            mLog.Info("EndUserArchiverAdvancedMode value is :{0}", endUserAdvanceMode.ToString());
            return endUserAdvanceMode;
        }

        private bool GetIsBackupNewsfeed()
        {
            bool isBackupNewsfeed = true;
            try
            {
                string value = GetConfigFile("IsBackupNewsfeed");
                if (value != string.Empty && value.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    isBackupNewsfeed = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: IsBackupNewsfeed" + ex.ToString());
                isBackupNewsfeed = true;
            }
            mLog.Info("IsBackupNewsfeed value is :{0}", isBackupNewsfeed.ToString());
            return isBackupNewsfeed;
        }

        /// <summary>
        /// 6.8.2版本隐藏配置文件属性错误，原来为IncluedItemInContainerRule
        /// </summary>
        private bool GetIncludeItemInContainerRule()
        {
            bool incluedItemInContainerRule = false;
            try
            {
                string value = GetConfigFile("IncludeItemInContainerRule");
                if (value != string.Empty && value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    incluedItemInContainerRule = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: IncludeItemInContainerRule" + ex.ToString());
                incluedItemInContainerRule = false;
            }
            mLog.Info("IncludeItemInContainerRule value is :{0}", incluedItemInContainerRule.ToString());
            return incluedItemInContainerRule;
        }

        private List<string> GetDisplayColumnNames()
        {

            List<string> columns = new List<string>();
            try
            {
                StringBuilder sb = new StringBuilder();
                XmlElement columnsEle = (XmlElement)ArchiveDel.SelectSingleNode("DisplayColumns");
                if (columnsEle != null)
                {
                    foreach (var node in columnsEle.GetElementsByTagName("Column"))
                    {
                        XmlElement xe = (XmlElement)node;
                        string columnTitle = xe.GetAttribute("name");
                        sb.Append(columnTitle + ";");
                        columns.Add(columnTitle);
                    }
                }
                mLog.Info("DisplayColumnName value is :{0}", sb.ToString());
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: DisplayColumns" + ex.ToString());
            }
            return columns;
        }

        private List<string> GetRADisplayColumnNames()
        {
            List<string> columns = new List<string>();
            try
            {
                StringBuilder sb = new StringBuilder();
                XmlElement columnsEle = (XmlElement)ArchiveDel.SelectSingleNode("RecordsDisplayColumns");
                if (columnsEle != null)
                {
                    foreach (var node in columnsEle.GetElementsByTagName("Column"))
                    {
                        XmlElement xe = (XmlElement)node;
                        string columnTitle = xe.GetAttribute("name");
                        sb.Append(columnTitle + ";");
                        columns.Add(columnTitle);
                    }
                }
                mLog.Info("RecordsDisplayColumns value is :{0}", sb.ToString());
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: RecordsDisplayColumns" + ex.ToString());
            }
            return columns;
        }

        private bool GetBackupLinkFileRealContent()
        {
            bool isBackupLinkFileRealContent = true;
            try
            {
                string value = GetConfigFile("BackupLinkFileRealContent");
                if (value != string.Empty && value.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    isBackupLinkFileRealContent = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: isBackupLinkFileRealContent" + ex.ToString());
                isBackupLinkFileRealContent = true;
            }
            mLog.Info("isBackupLinkFileRealContent value is :{0}", isBackupLinkFileRealContent.ToString());
            return isBackupLinkFileRealContent;
        }

        private bool GetBackupManagedMetadata()
        {
            bool isBackupManagedMetadata = true;
            try
            {
                string value = GetConfigFile("BackupManagedMetadata");
                if (value != string.Empty && value.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    isBackupManagedMetadata = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: BackupManagedMetadata" + ex.ToString());
                isBackupManagedMetadata = true;
            }
            mLog.Info("BackupManagedMetadata value is :{0}", isBackupManagedMetadata.ToString());
            return isBackupManagedMetadata;
        }

        private bool GetRecordManagerBackupManagedMetadata()
        {
            bool isRecordManagerBackupManagedMetadata = false;
            try
            {
                string value = GetConfigFile("RecordManagerBackupManagedMetadata");
                if (value != string.Empty && value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    isRecordManagerBackupManagedMetadata = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: RecordManagerBackupManagedMetadata" + ex.ToString());
                isRecordManagerBackupManagedMetadata = false;
            }
            mLog.Info("RecordManagerBackupManagedMetadata value is :{0}", isRecordManagerBackupManagedMetadata.ToString());
            return isRecordManagerBackupManagedMetadata;
        }

        private bool GetKeepModeration()
        {
            bool isKeepModeration = true;
            try
            {
                string value = GetConfigFile("KeepModerationValue");
                if (value != string.Empty && value.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    isKeepModeration = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: KeepModerationValue" + ex.ToString());
                isKeepModeration = true;
            }
            mLog.Info("KeepModerationValue value is :{0}", isKeepModeration.ToString());
            return isKeepModeration;
        }

        private bool GetIsSyncItemPermission()
        {
            bool isSyncItemPermission = false;
            try
            {
                string value = GetConfigFile("IsSyncItemPermission");
                if (value != string.Empty && value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    isSyncItemPermission = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: IsSyncItemPermission" + ex.ToString());
                isSyncItemPermission = false;
            }
            mLog.Info("IsSyncItemPermission value is :{0}", isSyncItemPermission.ToString());
            return isSyncItemPermission;
        }

        private string GetFSAStubNameFormat()
        {
            string fSAStubNameFormat = "stub.html";
            try
            {
                string value = GetConfigFile("FSAStubNameFormat").TrimEnd(' ');
                if (!string.IsNullOrEmpty(value))
                {
                    fSAStubNameFormat = value;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Can not Get FSAStubNameFormat, use the default stub.html. Message: {0}." + ex.ToString());
            }
            mLog.Info("fSAStubNameFormat value is :{0}", fSAStubNameFormat.ToString());
            return fSAStubNameFormat;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private string GetMoveToActionStubFileContent()//string contentTypeId, string urlPath, string siteUrl)
        {
            string contentInfo = string.Empty;
            try
            {
                string aspxPath = SOCommonObjects.AgentCommonArchiveMoveToStubPath;// AveEnv.AgentDataFolder.TrimEnd('\\') + @"\SP2013\Arch\" + "AgentCommonArchiveMoveToStub.aspx";
                using (StreamReader sr = new StreamReader(aspxPath, Encoding.UTF8))
                {
                    contentInfo = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in get record stub file, reason : " + ex.ToString());
            }
            return contentInfo;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "IDAs")]
        private bool GetUseDocumentIDAsLandingPageURL()
        {
            bool useDocumentID = false;
            try
            {
                string value = GetConfigFile("UseDocumentIDAsLandingPageURL");
                if (value != string.Empty && value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    useDocumentID = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: UseDocumentIDAsLandingPageURL" + ex.ToString());
                useDocumentID = false;
            }
            mLog.Debug("UseDocumentIDAsLandingPageURL value is :{0}", useDocumentID.ToString());
            return useDocumentID;
        }

        private string GetUserAgentTag()
        {
            string userAgentTag = string.Empty;
            try
            {
                string value = GetConfigFile("UserAgentTag").TrimEnd(' ');
                if (!string.IsNullOrEmpty(value))
                {
                    userAgentTag = value;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Can not Get UserAgentTag. Message: {0}." + ex.ToString());
            }
            mLog.Info("UserAgentTag value is :{0}", userAgentTag);
            return userAgentTag;
        }

        private bool GetIsDeleteLabelItem()
        {
            bool IsDeleteLabelItem = false;
            try
            {
                if (GetConfigFile("IsDeleteLabelItem") != string.Empty && GetConfigFile("IsDeleteLabelItem").Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    IsDeleteLabelItem = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: IsDeleteLabelItem" + ex.ToString());
                IsDeleteLabelItem = false;
            }
            mLog.Info("IsDeleteLabelItem value is :{0}", IsDeleteLabelItem.ToString());
            return IsDeleteLabelItem;
        }

        /// <summary>
        /// Default 30 minutes
        /// </summary>
        /// <returns></returns>
        private int GetRecordsRelatedJobTimeOut()
        {
            int jobTimeOutTime = 30;
            try
            {
                if (GetConfigFile("RecordsRelatedJobTimeOut") != string.Empty)
                {
                    jobTimeOutTime = Convert.ToInt32(GetConfigFile("RecordsRelatedJobTimeOut"));
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: RecordsRelatedJobTimeOut" + ex.ToString());
                jobTimeOutTime = 30;
            }
            mLog.Info("RecordsRelatedJobTimeOut value is :{0}", jobTimeOutTime.ToString());
            return jobTimeOutTime * 6;
        }

        private bool GetSPToSPMoveAllVersion()
        {
            bool IsSPToSPMoveAllVersion = true;
            try
            {
                if (GetConfigFile("SPToSPMoveAllVersion") != string.Empty && GetConfigFile("SPToSPMoveAllVersion").Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    IsSPToSPMoveAllVersion = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Config Attribute: SPToSPMoveAllVersion" + ex.ToString());
                IsSPToSPMoveAllVersion = true;
            }
            mLog.Info("SPToSPMoveAllVersion value is :{0}", IsSPToSPMoveAllVersion.ToString());
            return IsSPToSPMoveAllVersion;
        }
    }

    public class ExportSetting
    {
        public static String CONFIG_EXPORTSETTING = "ExportSettings";
        public static String CONFIG_EDRM = "EDRM";
        public static String CONFIG_MANIFESTXMLSETTING = "ManifestXmlSizeSetting";

        public int ManifestXmlSize { get; set; }
    }
    public class CAUrlSetting
    {
        public static String CONFIG_CAURLSETTING = "CentralAdminUrl";
        public static String CONFIG_CAURL = "centralAdminUrl";
        public static String CONFIG_URL = "url";
        public Dictionary<string, string> CaUrls { get; set; }
    }

    public partial class SOCommonObjects
    {

        public static readonly string SOConfigurationFileName = "AgentCommonStorageENV.cfg";

        public static readonly string AgentCommonArchiveMoveToStub = "AgentCommonArchiveMoveToStub.aspx";

        public static readonly string AgentCommonStubLandingPage = "AgentCommonStubLandingPage.aspx";

        public static readonly string SOStopJobFlag = "JobStop.cmd";

        public static string SOConfigurationFilePath
        {
            get { return AveEnv.AgentDataFolder.TrimEnd('\\') + @"\SP2013\Arch\" + SOConfigurationFileName; }
        }

        public static string AgentCommonArchiveMoveToStubPath
        {
            get { return AveEnv.AgentDataFolder.TrimEnd('\\') + @"\SP2013\Arch\" + AgentCommonArchiveMoveToStub; }
        }

        public static string AgentCommonStubLandingPagePath
        {
            get { return AveEnv.AgentDataFolder.TrimEnd('\\') + @"\SP2013\Arch\" + AgentCommonStubLandingPage; }
        }

        public static string mCachePath
        {
            get { return "\\SP2013\\Arch\\Cache\\"; }
        }

        public static string mExtendRuleXmlPath
        {
            get { return AveEnv.AgentDataFolder + "\\SP2013\\Arch\\ExtenderRules.xml"; }
        }

        public static string SOConnectorIntegrationAssemblyName = "SP2013ConnectorBusinessLogic";
        public static string SOConnectorCleanUpIntegrationClassName = "StorageOptimization.Connector.BusinessLogic.CleanUpOrphanBlob.ConnectorCleanUp";
    }
}
