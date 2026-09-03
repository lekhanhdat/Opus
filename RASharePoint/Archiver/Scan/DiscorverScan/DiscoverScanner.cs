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
using AngleSharp.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.Archiver.Scan.DiscorverScan;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.Discover.InsightsEngine;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using AvePoint.Wrapper.Discovery;
using Cloud.Sdk.Data.IE;
using DataOrchestration.Tag.Sdk;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Analyzer;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using RAArchiverCommon.DiscoveryArchiveJob;
using SPDisposeCheck;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class DiscoverScanner : ISharePointScanner
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(DiscoverScanner));
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private Dictionary<string, AveBPOSAccountInfo> _bposCache = new Dictionary<string, AveBPOSAccountInfo>();
        private readonly object locker = new();
        private IScanDataReader mScanDataReader = null;
        private long totalScanCount = 0;
        protected DiscoveryInsiteEngineItemManager itemManager;
        public static IEDataOptimizationService discoverDBService;
        private static object dalDiscoverDBService;
        private static bool useDalDiscoverDBService;
        private const string DalDataOptimizationServiceTypeName = "AvePoint.RA.Service.Services.DalServices.DALDataOptimizationService";
        internal AveDiscoverSite mDiscoverSite = null;
        internal IBackwardDependencyNodeCache<object> mDependencyObjs;
        internal ScanJobSettings jobSettings = null;
        internal ScheduleConfiguration mConfiguration = null;
        internal Guid scopeId = Guid.Empty;
        internal Guid groupId = Guid.Empty;
        internal AveObjectModelFactory mFactory = null;

        private IAveList mSPQueryList = null;
        private int mMaxItemIdInLibrary = 0;
        private SPOFolder SPORootFolder = null;
        public const string SP_ID = "ID";
        public const string SP_UniqueID = "UniqueId";
        private CAMLManager mCAMLManager = null;
        private string mWebAppName = string.Empty;
        private string mSiteUrl = string.Empty;
        public bool siteStorageLimit;
        public AveDiscoverFolder mInitNodeEntityRelatedInfoDiscoverRootFolder = null;
        public Guid mInitNodeEntityRelatedInfoRootFolderId = Guid.Empty;
        private AnalysisOptimizationDiscoverNodeWorker mDiscoverWorker = null;
        private List<string> DesignLists = new List<string>();

        public IDiscoverNodeWorker discoverWorker
        {
            get
            {
                if (mDiscoverWorker == null)
                {
                    mDiscoverWorker = new AnalysisOptimizationDiscoverNodeWorker(jobSettings, mConfiguration, mDependencyObjs);
                }
                return mDiscoverWorker;
            }
            set { }
        }
        public static IEDataOptimizationService Instance
        {
            get
            {
                return discoverDBService;
            }
        }

        public static Task<bool> TagAsArchivedAsync(string itemUniqueId)
        {
            if (useDalDiscoverDBService && dalDiscoverDBService != null)
            {
                var dalResult = InvokeDalTagAsArchivedAsync(itemUniqueId);
                if (dalResult != null)
                {
                    return dalResult;
                }
            }

            if (discoverDBService != null)
            {
                return discoverDBService.TagAsArchivedAsync(itemUniqueId);
            }

            return Task.FromResult(false);
        }

        private static Task<bool> InvokeDalTagAsArchivedAsync(string itemUniqueId)
        {
            try
            {
                var method = dalDiscoverDBService?.GetType().GetMethod("TagAsArchivedAsync", new[] { typeof(string) });
                return method?.Invoke(dalDiscoverDBService, new object[] { itemUniqueId }) as Task<bool>;
            }
            catch (Exception ex)
            {
                mLog.Warn($"InvokeDalTagAsArchivedAsync failed, fallback to IE service. error:{ex}");
                return null;
            }
        }
        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }



        public PCContainer<ArchiveApproveReport> GetPCContainer()
        {
            if (mDiscoverWorker == null)
            {
                return (discoverWorker as AnalysisOptimizationDiscoverNodeWorker).pcContainer;
            }
            else
            {
                return mDiscoverWorker.pcContainer;
            }

        }
        protected IAveSite Site { get; set; }
        internal string WebAppName
        {
            get
            {
                if (string.IsNullOrEmpty(mWebAppName))
                {
                    int indexOfSlash = mSiteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
                    mWebAppName = mSiteUrl;
                    if (indexOfSlash != -1)
                    {
                        mWebAppName = mSiteUrl.Substring(0, mSiteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase));
                    }
                }
                return mWebAppName;
            }
        }

        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private RMDiscoveryOptimizeDataSettingDto mRMDiscoveryOptimizeDataSettingDto = null;
        private RMDiscoveryAOSPOptimizeDataSettingDto mRMDiscoveryAOSPOptimizeDataSettingDto = null;
        private List<DiscoverTagRule> discoverTagRules = new List<DiscoverTagRule>();
        public List<CleanUpItemEntry> CleanUpItemEntrys { get; set; }
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public DiscoverScanner(ScanJobSettings scanJobSettings, RMDiscoveryOptimizeDataSettingDto dataDto, List<CleanUpItemEntry> cleanUpItemEntrys = null)
        {
            mDependencyObjs = new BackwardDependenceNodeCache<object>();
            jobSettings = scanJobSettings;
            mConfiguration = scanJobSettings.Configuration;
            mScanDataReader = new ScanDataReader(mConfiguration);
            mRMDiscoveryOptimizeDataSettingDto = dataDto;
            this.DesignLists = GetDesignLists();
            CleanUpItemEntrys = cleanUpItemEntrys;
        }

        public void RealRun()
        {
            try
            {
                using (new CheckJobStopScope()) { }
                
                InitDiscoverDBService();
                RunDiscoverOptimizationAsync().GetAwaiter().GetResult();
            }
            catch(JobStopException)
            {
                mLog.Error($"Job was stop, stop scan");
            }
            catch (AveExceedStorageLimitException aex)
            {
                siteStorageLimit = true;
                mConfiguration.JobReportDto.AddScanReport(mSiteUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Failed, "RM_JM_SiteStorageLimit_ErrorMessage");
                mLog.Error($"This site has exceeded its maximum file storage limit,set siteStorageLimit = true");
            }
            catch (Exception e)
            {
                mLog.Error($"some thing went wrong when RunDiscoverOptimization ,error :{e.ToString()}");
            }
        }
        private void InitDiscoverDBService()
        {
            SourceFlag sourceFlag = mConfiguration.IsOneDriverSite? SourceFlag.OneDrive: SourceFlag.SharePoint;
            if (mConfiguration.IsTeams)
            {
                sourceFlag = SourceFlag.Teams;
            }
            useDalDiscoverDBService = mRMDiscoveryOptimizeDataSettingDto?.UseDalDataOptimizationService == true;
            discoverDBService = null;
            dalDiscoverDBService = null;
            if (mConfiguration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion)
            {
                if (useDalDiscoverDBService)
                {
                    dalDiscoverDBService = CreateDalDiscoverDBService(mRMDiscoveryOptimizeDataSettingDto, true, sourceFlag, mConfiguration.IsProcessDuplicateDatas);
                    if (dalDiscoverDBService == null)
                    {
                        useDalDiscoverDBService = false;
                        discoverDBService = new IEDataOptimizationService(mRMDiscoveryOptimizeDataSettingDto, true, sourceFlag, mConfiguration.IsProcessDuplicateDatas);
                    }
                }
                else
                {
                    discoverDBService = new IEDataOptimizationService(mRMDiscoveryOptimizeDataSettingDto, true, sourceFlag, mConfiguration.IsProcessDuplicateDatas);
                }
                if (mConfiguration.InactiveDiscoveryRuleInfos != null && mConfiguration.InactiveDiscoveryRuleInfos.Count>0)
                {
                    mConfiguration.InactiveAndRotVerisonRuleInfos.AddRange(mConfiguration.InactiveDiscoveryRuleInfos ?? null);
                }
                if (mConfiguration.ROTDiscoveryRuleInfos != null && mConfiguration.ROTDiscoveryRuleInfos.Count > 0)
                {
                    mConfiguration.InactiveAndRotVerisonRuleInfos.AddRange(mConfiguration.ROTDiscoveryRuleInfos?.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version));
                }
                if (mConfiguration.InactiveAndRotVerisonRuleInfos != null && mConfiguration.InactiveAndRotVerisonRuleInfos.Count > 0)
                {
                    discoverTagRules = mConfiguration.InactiveAndRotVerisonRuleInfos?.ConvertAll(rule =>
                    {
                        var tag = new DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract.TagInfo
                        {
                            IsBuildIn = false,
                        };

                        var criteriaInfoes = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(rule.CriteriaInfoesJson);
                        var ruleInfo = new DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract.RuleInfo
                        {
                            Method = (AnalyseMethod)rule.AnalyseMethod,
                            CriteriaInfoes = criteriaInfoes.ConvertAll(criteriaInfo => new CriteriaInfo
                            {
                                CriteriaType = criteriaInfo.CriteriaType,
                                Order = criteriaInfo.Order,
                                LogicType = (CriteriaLogicType)criteriaInfo.LogicType,
                                ConditionInfo = new ConditionInfo
                                {
                                    Category = (ConditionCategory)criteriaInfo.ConditionInfo.Category,
                                    Logic = criteriaInfo.ConditionInfo.Logic,
                                    Value = criteriaInfo.ConditionInfo.Value,
                                }
                            })
                        };

                        tag.TagDefinition = JsonConvert.SerializeObject(ruleInfo);
                        DiscoverTagRule tagRule = new DiscoverTagRule();
                        tagRule.TagRuleModel = new TagRuleModel
                        {
                            Id = rule.UniqueId,
                            Name = rule.Name,
                            Definition = JsonConvert.SerializeObject(tag),
                            Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                            Type = Cloud.Sdk.Data.IE.DataType.SPDocument,
                            NeedCalculation = true
                        };
                        tagRule.RuleInfo = ruleInfo;
                        return tagRule;
                    });
                }
            }
            else
            {
                if (useDalDiscoverDBService)
                {
                    dalDiscoverDBService = CreateDalDiscoverDBService(mRMDiscoveryOptimizeDataSettingDto, false, sourceFlag, mConfiguration.IsProcessDuplicateDatas);
                    if (dalDiscoverDBService == null)
                    {
                        useDalDiscoverDBService = false;
                        discoverDBService = new IEDataOptimizationService(mRMDiscoveryOptimizeDataSettingDto, false, sourceFlag, mConfiguration.IsProcessDuplicateDatas);
                    }
                }
                else
                {
                    discoverDBService = new IEDataOptimizationService(mRMDiscoveryOptimizeDataSettingDto, false, sourceFlag, mConfiguration.IsProcessDuplicateDatas);
                }
            }
        }

        private static Type ResolveDalDiscoverDBServiceType()
        {
            var fullTypeName = DalDataOptimizationServiceTypeName;
            var candidates = new[]
            {
                $"{fullTypeName}, RevIMService",
                $"{fullTypeName}, RAService",
                fullTypeName,
            };

            foreach (var candidate in candidates)
            {
                var type = Type.GetType(candidate, throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(fullTypeName, throwOnError: false, ignoreCase: false);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore assembly type load failures and keep trying.
                }
            }

            return null;
        }

        private object CreateDalDiscoverDBService(RMDiscoveryOptimizeDataSettingDto setting, bool checkVersionRule, SourceFlag sourceFlag, bool isProcessDuplicateDatas)
        {
            try
            {
                var dalServiceType = ResolveDalDiscoverDBServiceType();
                if (dalServiceType == null)
                {
                    mLog.Warn("DALDataOptimizationService type is not available in current runtime, fallback to IEDataOptimizationService.");
                    return null;
                }

                return Activator.CreateInstance(
                    dalServiceType,
                    setting,
                    checkVersionRule,
                    sourceFlag,
                    isProcessDuplicateDatas);
            }
            catch (Exception ex)
            {
                mLog.Warn($"CreateDalDiscoverDBService failed, fallback to IEDataOptimizationService. error:{ex}");
                return null;
            }
        }

        private IAsyncEnumerable<List<RMDiscoveryFileDataInfo>> GetAllDuplicateFilesAsync(string siteId, string webId, string listId, List<string> objectIds)
        {
            if (useDalDiscoverDBService && dalDiscoverDBService != null)
            {
                var dalResult = InvokeDalGetAllDuplicateFilesAsync(siteId, webId, listId, objectIds);
                if (dalResult != null)
                {
                    return dalResult;
                }
            }

            return discoverDBService.GetAllDuplicateFilesAsync(siteId, webId, listId, objectIds);
        }

        private IAsyncEnumerable<List<RMDiscoveryFileDataInfo>> GetAllFilesAsync(string siteId, string webId, string listId, List<string> tagRuleIds)
        {
            if (useDalDiscoverDBService && dalDiscoverDBService != null)
            {
                var dalResult = InvokeDalGetAllFilesAsync(siteId, webId, listId, tagRuleIds);
                if (dalResult != null)
                {
                    return dalResult;
                }
            }

            return discoverDBService.GetAllFilesAsync(siteId, webId, listId, tagRuleIds);
        }

        private IAsyncEnumerable<List<RMDiscoveryFileDataInfo>> InvokeDalGetAllDuplicateFilesAsync(string siteId, string webId, string listId, List<string> objectIds)
        {
            try
            {
                var method = dalDiscoverDBService?.GetType().GetMethod("GetAllDuplicateFilesAsync", new[] { typeof(string), typeof(string), typeof(string), typeof(List<string>), typeof(int) });
                return method?.Invoke(dalDiscoverDBService, new object[] { siteId, webId, listId, objectIds, 1000 }) as IAsyncEnumerable<List<RMDiscoveryFileDataInfo>>;
            }
            catch (Exception ex)
            {
                mLog.Warn($"InvokeDalGetAllDuplicateFilesAsync failed, fallback to IE service. error:{ex}");
                return null;
            }
        }

        private IAsyncEnumerable<List<RMDiscoveryFileDataInfo>> InvokeDalGetAllFilesAsync(string siteId, string webId, string listId, List<string> tagRuleIds)
        {
            try
            {
                var method = dalDiscoverDBService?.GetType().GetMethod("GetAllFilesAsync", new[] { typeof(string), typeof(string), typeof(string), typeof(List<string>), typeof(int) });
                return method?.Invoke(dalDiscoverDBService, new object[] { siteId, webId, listId, tagRuleIds, 1000 }) as IAsyncEnumerable<List<RMDiscoveryFileDataInfo>>;
            }
            catch (Exception ex)
            {
                mLog.Warn($"InvokeDalGetAllFilesAsync failed, fallback to IE service. error:{ex}");
                return null;
            }
        }
        public async System.Threading.Tasks.Task RunDiscoverOptimizationAsync()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SharePointScanner.Run"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    var node = RMDtoConverter.ConvertRMTree2SPTree(jobSettings.TreeNode);
                    var ruleNode = ConvertTreeNodeToRuleNodeConfig(node, RuleNodeType.Archiver, jobSettings.DiscoverNode);
                    ArchiverNodeItem selectNodeItem = new ArchiverNodeItem(ruleNode);
                    JobExecutionProcessStatisticExecutor.Instance.StartCalculateRuleAndSummary(selectNodeItem.SPNodeLevel.ToString(), selectNodeItem.FullPath);
                    try
                    {
                        selectNodeItem.SiteUrl = jobSettings.DiscoverNode.SiteUrl;
                        selectNodeItem.SiteId = jobSettings.DiscoverNode.SiteId;
                        selectNodeItem.FullPath = jobSettings.DiscoverNode.SiteUrl;
                        selectNodeItem.Name = jobSettings.DiscoverNode.SiteUrl;
                        mSiteUrl = jobSettings.DiscoverNode.SiteUrl;
                        var count = CaculateListCount(selectNodeItem);
                        mLog.Info($"Scan caculate list count is {count}");
                        mConfiguration.ProgressDto.SetBaseCount4Phase(count);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"Scan caculate list count error {e}");
                    }
                    await ProcessSiteCollectionAsync(selectNodeItem);
                    mDependencyObjs.Flush();
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveExceedStorageLimitException aex)
                {
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while scanning error {0}", e.ToString());
                }
                finally
                {
                    discoverWorker.Flush();
                    mDiscoverWorker.pcContainer.EndProduce();
                    JobExecutionProcessStatisticExecutor.Instance.EndCalculateRuleAndScanSummary(totalScanCount, Site);
                }
            }
        }


        public async System.Threading.Tasks.Task RunAsync()
        {

        }

        public virtual List<string> LoadBreakInheritNodeUrls(string scopeUrl, string siteObjectId = "")
        {
            return ArchiverSettingDao.LoadBreakInheritNodeUrls(scopeUrl, siteObjectId, mConfiguration.IsTeams);
        }

        public void SendPhysicalJobDetail(string name, string originPath, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
            {
                ObjectName = name,
                FullPath = originPath,
                ActionType = GetI18NActionType(action),
                DestinationPath = destinationPath,
                ItemType = ItemType,
                Status = status,
                Comment = comment
            });
        }

        private string GetI18NActionType(PhysicalDisposalActionType action)
        {
            string result = string.Empty;
            switch (action)
            {
                case PhysicalDisposalActionType.Pending:
                    result = "RM_JMD_PD_DisposalAction_Pending";
                    break;
                case PhysicalDisposalActionType.Disposal:
                    result = "RM_JMD_PD_DisposalAction_Dispose";
                    break;
                case PhysicalDisposalActionType.Move:
                    result = "RM_JMD_PD_DisposalAction_Move";
                    break;
                default:
                    result = action.ToString();
                    break;
            }
            return result;
        }
        public void SendSPJobDetail(long nodeSize, string originPath, int cacheNodeType, JobDetailsStatus status, string comment = "")
        {
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = originPath;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = mConfiguration.currentRule.Name;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Backup;
            //mArchiverActionJobDetails.Action = "Delete";
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Comment = comment;
            JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(mConfiguration.currentRule, nodeSize, cacheNodeType, status);
            ReportManager.SendJobDetail(mArchiverActionJobDetails);
        }

        public IScanDataReader GetScanDataReader()
        {
            return mScanDataReader;
        }

        public virtual async System.Threading.Tasks.Task ProcessSiteCollectionAsync(ArchiverNodeItem sitecollection)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    try
                    {
                        using (new CheckJobStopScope()) { }
                        //初始化Site对应的一些信息。
                        await InitialSPObjectInfoAsync(discoverWorker, sitecollection);
                        //If the rootWeb has defined a unique rule, we should skip all the site collection.
                        //URL of RootWeb is same as sitecollection's
                        IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;

                        //对这个Site检查Rule.
                        ProcessResult result = (await discoverWorker.ProcessContainerAsync(sitecollection, ProcessType.NeedProcess));
                        if (result == ProcessResult.SkipCurrentNode)
                        {
                            mLog.Info("skip current Node {0}", sitecollection.FullPath);
                            return;
                        }

                        using (AveDiscoverSite discoverySite = sitecollection.DiscoverSPObject as AveDiscoverSite)
                        {
                            using (AveDiscoverWeb rootWeb = discoverySite.GetRootWeb())
                            {
                                if (discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(sitecollection.FullPath)))
                                {
                                    var setting = ArchiverSettingDao.LoadArchiverSetting(rootWeb.WebID, sitecollection.ID);
                                    if (setting != null)
                                    {
                                        mLog.Warn("root web {0} is break inherit from parent", rootWeb.FullUrl);
                                        return;
                                    }
                                }
                                using (ArchiverNodeItem webnode = sitecollection.GenerateSiteNodeItem(rootWeb, mConfiguration, true))
                                {
                                    string rootWebSiteLogoDescription = rootWeb.AveWeb.SiteLogoDescription;//通过调用SiteLogoDescription自动创建出Site Assets List 
                                    await ProcessWebAsync(webnode);
                                }
                            }
                        }

                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (AveExceedStorageLimitException aex)
                    {
                        throw;
                    }
                    catch (AveWrapperI18NException IUPEx)
                    {
                        mLog.Info("Site Collection UserName Or Password Incorrect. Path:{0}. Message:{1}.", sitecollection.FullPath, IUPEx.ToString());
                        throw;
                    }
                    catch (SPObjectReadOnlyException snfe)
                    {
                        mLog.Info("Site Collection is ReadOnly. Path:{0}. Message:{1}.", sitecollection.FullPath, snfe.ToString());

                        throw;
                    }
                    catch (SPObjectLockedException sle)
                    {
                        mLog.Info("Site Collection is Locked. Path:{0}. Message:{1}.", sitecollection.FullPath, sle.ToString());

                        throw;
                    }
                    catch (SPObjectNotFoundException ex)
                    {
                        mLog.Info("Site Collection Not Found. Path:{0}. Message:{1}.", sitecollection.FullPath, ex.ToString());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null && ex.InnerException.Message.Contains("The site do not meet the conditions."))
                        {
                            mLog.Error(string.Format("AveLATMgtApiNotEnabledException in Backup Site Collection :{0}.Site Collection Path:{1}.", ex.ToString(), sitecollection.FullPath));
                        }
                        else
                        {
                            mLog.Error("An unexpected error occurred while processing site collection node.Path:{0}.Message:{1}.", sitecollection.FullPath, ex);
                        }
                        throw;
                    }
                    finally
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.SiteCollection, 0);
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (AveExceedStorageLimitException aex)
            {
                throw;
            }
            catch (Exception e)
            {
                mLog.Error("Process sitecollection error {0}", e.ToString());
                //TO DO Add Detail
                //TO DO I18N
                //base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty,
                //    string.Empty, string.Empty, JobReportDetailStatus.Failed, e.Message);
            }
            finally
            {
                mDiscoverSite = null;
            }
        }

        [SPDisposeCheckIgnoreAttribute(SPDisposeCheckID._120, "Ignore")]
        public async virtual System.Threading.Tasks.Task ProcessWebAsync(ArchiverNodeItem web, bool needInitInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessWeb"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, web);
                    }
                    else
                    {
                        IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                        if (mConfiguration.mInitialTime.AddHours(23) < DateTime.Now)
                        {
                            mLog.Info("The SPSite id Time out, New it again");
                            string mSiteUrl = tmpSite.Url;
                            tmpSite.Dispose();
                            mConfiguration.mInitialTime = DateTime.Now;
                            //tmpSite = new SPSite(mSiteUrl);
                            AveObjectModelFactory factory = mConfiguration.aveObjectModelFactory;

                            tmpSite = factory.CreateSite(mSiteUrl);
                            mDependencyObjs.PutIn(tmpSite, (int)CacheNodeType.SiteCollection, false);
                        }
                        IAveWeb tmpWeb = tmpSite.OpenWeb(web.ID);
                        if (tmpWeb == null)
                        {
                            throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanProcessWebSPObjectNotFoundException, "Site", web.FullPath);
                        }
                        //TODO:Disable language mapping
                        //ScheduleLanguageMapping.ProcessLanguageMapping(tmpWeb);
                        mDependencyObjs.PutIn(tmpWeb, (int)CacheNodeType.Web, false);
                    }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(web, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//web 级别 符合 web rule
                    {
                        return;
                    }
                    await ProcessListCollectionAsync(web);
                    //Process web
                    await ProcessWebCollectionAsync(web);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("Web UserName Or Password Incorrect. Path:{0}. Message:{1}.", web.FullPath, IUPEx.ToString());
                    throw;
                }
                catch (SPObjectReadOnlyException snfe)
                {
                    mLog.Info("Web is ReadOnly. Path:{0}. Message:{1}.", web.FullPath, snfe.ToString());
                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("Web is Locked. Path:{0}. Message:{1}.", web.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("Web Not Found. Path:{0}. Message:{1}.", web.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while processing web node.Path:{0}. Message:{1}.", web.FullPath, e.ToString());
                    throw;
                }
                finally
                {
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.Web, 0);
                }
            }
        }

        /// <summary>
        /// Process all the items under list for initialization
        /// </summary>
        /// <param name="list"></param>
        public virtual async System.Threading.Tasks.Task ProcessListAsync(ArchiverNodeItem list)
        {
            mLog.Info("Begin process list,title is:{0}.", list.Title);
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if ((await discoverWorker.ProcessContainerAsync(list, ProcessType.NeedProcess)) == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }
                    AveDiscoverFolder rootFolder = null;
                    try
                    {
                        mLog.Info("List Begin SPQuery to filter data. Path:{0}.", list.FullPath);
                        InitForSPQueryDiscover(list.SPList);
                        InitArchiverSPQueryRootFolder(list.SPList.RootFolder.ServerRelativeUrl);
                        if (SPORootFolder != null && SPORootFolder.SubFolders != null && SPORootFolder.SubFolders.Count > 0)
                        {
                            InitArchiverSPQueryFolderStructure(list.SPList.RootFolder.ServerRelativeUrl);
                        }
                        rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolderForArchiverSPQuery(SPORootFolder);
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Can not use SPQuery to filter data and change query to Full Scan. Path:{0}. Message:{1}.", list.FullPath, ex.ToString());
                        ReleaseForSPQueryDiscover();
                        rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true);
                        //DB Scan如果SP Query 跪了，则直接抛异常，不走Full Discover
                        throw;
                    }
                    ArchiverNodeItem foldernode = list.GenerateFolderNodeItem(rootFolder, NodeLevel.RootFolder, mDiscoverSite.Site.Url, mConfiguration);
                    await ProcessFolderAsync(foldernode);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("List UserName Or Password Incorrect. Path:{0}. Message:{1}.", list.FullPath, IUPEx.ToString());
                    throw;
                }

                catch (SPObjectReadOnlyException sroe)
                {
                    mLog.Info("List is ReadOnly. Path:{0}. Message:{1}.", list.FullPath, sroe.ToString());

                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("List is Locked. Path:{0}. Message:{1}.", list.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("List Not Found. Path:{0}. Message:{1}.", list.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("[ProcessListAsync]An unexpected error occurred while processing list node.Path:{0}.Message:{1}.", list.FullPath, e.ToString());
                    mConfiguration.JobReportDto.AddScanReport(list.FullPath, 0, (int)CacheNodeType.List, e.Message);
                    mConfiguration.JobReportDto.HasErrorNode = true;
                    throw;
                }
                finally
                {
                    mConfiguration.ProgressDto.UpdateProgress();
                }
            }
        }
        private void ReleaseForSPQueryDiscover()
        {
            try
            {
                mConfiguration.mUseQueryDiscover = false;
                mSPQueryList = null;
                mCAMLManager = null;
                mMaxItemIdInLibrary = 0;
            }
            catch(Exception e)
            {
                mLog.Error($"error occured when ReleaseForSPQueryDiscover,error:{e}");
            }
        }
        private int GetLastItemId(IAveList list, string folderUrl)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetLastItemId(list, folderUrl, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetLastItemId(list, folderUrl, fileQueryXml);
            return Math.Max(lastItemId, maxFileId);
        }
        private void InitForSPQueryDiscover(IAveList list)
        {
            mConfiguration.mUseQueryDiscover = true;
            mSPQueryList = list;
            CamlScan cs = new CamlScan();
            //mCAMLManager = cs.InitCamlQuery(list, list.Fields, mConfiguration.RuleItemCollection, DateTime.UtcNow, true);
            mCAMLManager = new CAMLManager();
            mMaxItemIdInLibrary = GetLastItemId(list, list.RootFolder.ServerRelativeUrl);
            mLog.Info($"Using spquery for list:{list.Title} Max item id:{mMaxItemIdInLibrary}");
        }
        private void InitArchiverSPQueryFolderStructure(string rootFolderServerRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitArchiverSPQueryFolderStructure"))
            {
                int startIndex = 0;
                int endIndex = 0;
                int totaltemsCount = 0;
                int rowLimit = 2000;
                try
                {
                    if (mMaxItemIdInLibrary > 0)
                    {
                        AveCamlQuery query = new AveCamlQuery();
                        mCAMLManager.ScopeType = Types.ScopeTypes.RecursiveAll;
                        mCAMLManager.RowLimit = rowLimit;
                        query.DatesInUtc = true;
                        query.FolderServerRelativeUrl = rootFolderServerRelativeUrl;
                        int executeCount = 0;
                        mLog.Info($"Start to query InitArchiverSPQueryFolderStructure in :{rootFolderServerRelativeUrl}.");
                        List<SPFolderReducedInfo> AllFolderReducedInfos = new List<SPFolderReducedInfo>();
                        AllFolderReducedInfos.Add(new SPFolderReducedInfo() { ServerRelativeUrl = rootFolderServerRelativeUrl, ID = 0 });
                        do
                        {
                            endIndex = startIndex + rowLimit > mMaxItemIdInLibrary ? mMaxItemIdInLibrary : startIndex + rowLimit;
                            mCAMLManager.QueryGroup.Groups.Clear();
                            mCAMLManager.QueryGroup.Conditions.Clear();
                            mCAMLManager.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Gt, startIndex.ToString()));
                            mCAMLManager.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Leq, endIndex.ToString()));
                            mCAMLManager.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, "FSObjType", Types.FieldTypes.Integer, Types.QueryTypes.Eq, ((int)AveFileSystemObjectType.Folder).ToString()));
                            string queryXml = mCAMLManager.GetFullCAML();
                            query.ViewXml = queryXml;
                            mLog.Info("InitArchiverSPQueryFolderStructure xml {0}:{1}.", rootFolderServerRelativeUrl, queryXml);
                            IAveListItemCollection items = mSPQueryList.GetItems(query);
                            executeCount++;
                            totaltemsCount = totaltemsCount + items.Count;
                            mLog.Info("InitArchiverSPQueryFolderStructure {0}, query execute count:{1}. folder items count:{2}.", rootFolderServerRelativeUrl, executeCount, items.Count);
                            var folderItems = items.Where(x => x.FileSystemObjectType == AveFileSystemObjectType.Folder).ToList();
                            var partialReducedInfos = GetFolderReducedInfos(folderItems);
                            //AnalyzeFolderStructureV3(items, SPORootFolder);
                            AllFolderReducedInfos.AddRange(partialReducedInfos);
                            items = null;
                            mLog.Info("InitArchiverSPQueryFolderStructure ProcessDataWithSPQuery finished:{0}.execute count:{1}.", rootFolderServerRelativeUrl, executeCount);
                            if (startIndex + rowLimit < mMaxItemIdInLibrary)
                            {
                                startIndex = startIndex + rowLimit;
                            }
                            else if (startIndex + rowLimit > mMaxItemIdInLibrary && endIndex < mMaxItemIdInLibrary)
                            {
                                startIndex = mMaxItemIdInLibrary - endIndex;
                            }
                            else
                            {
                                break;
                            }
                        }
                        while (true);

                        AnalyzeFolderStructureV3(AllFolderReducedInfos, SPORootFolder);
                        mLog.Info("InitArchiverSPQueryFolderStructure xml {0}:{1}, query execute count:{2} totaltemsCount:{3}.", rootFolderServerRelativeUrl, mCAMLManager.GetFullCAML(), executeCount, totaltemsCount);
                    }
                    else
                    {
                        mLog.Info($"No item in this library, folder url:{rootFolderServerRelativeUrl} max item id:{mMaxItemIdInLibrary}.");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfoldersV2.Path:{0}.Message:{1}.", rootFolderServerRelativeUrl, ex.ToString());
                    throw;
                }
            }
        }
        void AnalyzeFolderStructureV3(List<SPFolderReducedInfo> folderItems, SPOFolder rootFolder)
        {
            var realRootFolder = folderItems.FirstOrDefault(x => string.Equals(x.ServerRelativeUrl.ToString(), rootFolder.Name, StringComparison.OrdinalIgnoreCase));
            if (realRootFolder != null)
            {
                rootFolder.Id = realRootFolder.ID;
            }
            else
            {
                mLog.Error($"Cannot find root folder id by url {rootFolder.Name}");
                throw new Exception($"Cannot find root folder id by url {rootFolder.Name}");
            }
            List<int> results = new List<int>();
            if (rootFolder.SubFolders != null)
            {
                foreach (SPOFolder folder in rootFolder.SubFolders)
                {
                    var failedItemsId = AssignFolderId(folder, rootFolder.Name, folderItems);

                    if (failedItemsId == null)
                    {
                        mLog.Info($"Folder:{folder?.Name} items id is null");
                        continue;
                    }

                    List<int> ids = new List<int>();
                    
                    foreach(int id in failedItemsId)
                    {
                        ids.Add(id);
                        if(ids.Count() > 1000)
                        {
                            RealAnalyzeFolderStructureV3(ids);
                            ids = new List<int>();
                        }
                    }

                    if (ids.Any())
                    {
                        RealAnalyzeFolderStructureV3(ids);
                    }
                }
            }
        }

        public void RealAnalyzeFolderStructureV3(List<int> ids)
        {
            List<RMDiscoveryFileDataInfo> fileInfos = itemManager.SelectValuesFromDBByItemIds(ids.ToArray());
            foreach (var i in ids)
            {
                try
                {
                    var info = fileInfos.Where(f => f.ItemId.Equals(i)).FirstOrDefault();
                    if (info != null)
                    {
                        //mConfiguration.ProgressDto.HasErrorNode = true;
                        string siteIDString = mConfiguration.SiteCollectionID.ToString();
                        string siteUrlString = mConfiguration.SiteCollectionUrl;
                        mLog.Error($"Cannot find ID:{info.ItemId}.Name:{info.FullUrl} when AnalyzeFolderStructureV3.");
                        mConfiguration.JobReportDto.AddScanReport(info.FullUrl, 0, (int)CacheNodeType.Item, "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, "StorageOptimization_SOARRecordManagerFileNotExist");
                        //CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).UpdateStatus(siteIDString, info.ItemId, BackupRestoreStatus.Failed);
                        //mConfiguration.JobReportDto.AddReport(info.url, 0, BackupRestoreStatus.Failed, (int)CacheNodeType.Item, mConfiguration.JobId, "", "", "StorageOptimization_SOARCGDBWrongFilePathError");
                    }
                }
                catch (Exception e)
                {
                    mLog.Error($"Cannot add to report id {i} error {e}");
                }
            }
        }


        private IEnumerable<int> AssignFolderId(SPOFolder folder, string parentFolderServerRelativePath, List<SPFolderReducedInfo> realFolders)
        {
            var currentFolderServerRelativePath = parentFolderServerRelativePath + "/" + folder.Name;
            var realCurrentFolder = realFolders.FirstOrDefault(x => string.Equals(x.ServerRelativeUrl, currentFolderServerRelativePath, StringComparison.OrdinalIgnoreCase));
            if (realCurrentFolder != null)
            {
                folder.Id = realCurrentFolder.ID;
            }
            else
            {
                IEnumerable<int> ids = null;
                //log can't find the folder from SP
                mLog.Error($"Cannot find folder id by url {currentFolderServerRelativePath}");
                try
                {
                    ids = GetItemRowIdUnderFolder(folder);
                }
                catch (Exception e)
                {
                    mLog.Warn($"An error occurred while GetItemRowIdUnderFolder {e.ToString()}");
                }
                if(ids != null)
                {
                    foreach(int id in ids)
                    {
                        yield return id;
                    }
                }
            }

            if (folder.SubFolders != null)
            {
                foreach (var subfolder in folder.SubFolders)
                {
                    var ids = AssignFolderId(subfolder, currentFolderServerRelativePath, realFolders);
                    foreach(int id in ids)
                    {
                        yield return id;
                    }
                }
            }
        }
        private IEnumerable<int> GetItemRowIdUnderFolder(SPOFolder folder)
        {
            if (folder != null)
            {
                if (folder.Items.Count != 0)
                {
                    foreach (var itemId in folder.Items.Select(item => item.Id))
                    {
                        yield return itemId;
                    }
                }
                if (folder.SubFolders != null)
                {
                    foreach (var subFolder in folder.SubFolders)
                    {
                        var idsUnderFolder = GetItemRowIdUnderFolder(subFolder);
                        foreach(int id in idsUnderFolder)
                        {
                            yield return id;
                        }
                    }
                }
            }
        }
        private List<SPFolderReducedInfo> GetFolderReducedInfos(List<IAveListItem> folders)
        {
            List<SPFolderReducedInfo> foldersReducedInfos = new List<SPFolderReducedInfo>();
            foreach (var folder in folders)
            {
                SPFolderReducedInfo info = new SPFolderReducedInfo();
                info.ID = folder.ID;
                info.ServerRelativeUrl = folder.FieldValues["FileRef"].ToString();
                foldersReducedInfos.Add(info);
                mLog.Info($"GetFolderReducedInfos. Folder Id:{info.ID}.Folder ServerRelativeUrl:{info.ServerRelativeUrl}.");
            }
            return foldersReducedInfos;
        }
        private int InnerGetLastItemId(IAveList list, string folderUrl, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folderUrl;
            query.ViewXml = queryXml;
            var itemCollection = list.GetItems(query);
            var item = itemCollection.FirstOrDefault();
            return item != null ? item.ID : -1;
        }
        private void InitArchiverSPQueryRootFolder(string rootFolderServerRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitArchiverSPQueryRootFolder"))
            {
                long fileInfoCount = itemManager.GetCoutOfData();
                SPORootFolder?.Dispose();
                SPORootFolder = SPOFolder.BuildRootFolder(new CacheDBOperator<SPOItem>(), new CacheDBOperator<SPOFolder>(), rootFolderServerRelativeUrl);
                try
                {
                    if (mMaxItemIdInLibrary > 0)
                    {
                        mLog.Info($"Start to query InitArchiverSPQueryRootFolder in :{rootFolderServerRelativeUrl}.");
                        //totaltemsCount = totaltemsCount + items.Count;
                        mLog.Info("InitArchiverSPQueryRootFolder.");
                        AnalyzeListItems(SPORootFolder);
                        mLog.Info("InitArchiverSPQueryRootFolder finished.");
                    }
                    else
                    {
                        mLog.Info($"No item in this library, folder url:{rootFolderServerRelativeUrl} max item id:{mMaxItemIdInLibrary}.");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while InitArchiverSPQueryRootFolder.Path:{0}.Message:{1}.", rootFolderServerRelativeUrl, ex.ToString());
                    throw;
                }
            }
        }
        void AnalyzeListItems(SPOFolder rootFolder)
        {
            int page = 0;
            int size = 1000;
            List<RMDiscoveryFileDataInfo> items;
            do
            {
                items = itemManager.PageSelectValuesFromDB(page++, size);
                foreach (var item in items)
                {
                    var itemUrl = item.FullUrl?.Trim();
                    if (string.IsNullOrEmpty(itemUrl))
                    {
                        mLog.Warn($"Skip discovery item with empty path. ItemId:{item.ItemId}.");
                        continue;
                    }

                    var serverRelativeUrl = itemUrl;
                    if (!useDalDiscoverDBService)
                    {
                        serverRelativeUrl = itemUrl.Substring(WebAppName.Length);
                    }
                    else if (Uri.TryCreate(itemUrl, UriKind.Absolute, out var itemUri))
                    {
                        serverRelativeUrl = itemUri.AbsolutePath;
                    }

                    if (!serverRelativeUrl.StartsWith("/", StringComparison.Ordinal))
                    {
                        serverRelativeUrl = "/" + serverRelativeUrl;
                    }

                    serverRelativeUrl = Uri.UnescapeDataString(serverRelativeUrl);
                    var rootFolderPath = Uri.UnescapeDataString(rootFolder.Name).TrimEnd('/');
                    if (!serverRelativeUrl.StartsWith(rootFolderPath, StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Warn($"Skip discovery item outside current list path. ItemId:{item.ItemId}, Path:{itemUrl}, NormalizedPath:{serverRelativeUrl}, Root:{rootFolder.Name}.");
                        continue;
                    }

                    var relativePath = serverRelativeUrl.Substring(rootFolderPath.Length).Trim('/');
                    if (string.IsNullOrEmpty(relativePath))
                    { continue; }

                    int index = relativePath.LastIndexOf('/');
                    var name = index >= 0 ? relativePath.Substring(index + 1) : relativePath;
                    var relativeFolderPath = index >= 0 ? relativePath.Substring(0, index) : string.Empty;
                    mLog.Info($"AnalyzeListItems. DBFileInfo Id:{item.Id}.itemId:{item.ItemId}.listId:{item.ListId}.webId:{item.WebId}.ItemParentPath:{serverRelativeUrl.Substring(0, serverRelativeUrl.Length - relativePath.Length)}{relativeFolderPath}.");
                    var parentFolder = rootFolder;
                    var parentFoldersName = relativeFolderPath.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < parentFoldersName.Length; i++)
                    {
                        var folderName = parentFoldersName[i];
                        SPOFolder tempFolder = null;
                        tempFolder = parentFolder.SubFolders.GetByName(folderName);

                        if (tempFolder == null)
                        {
                            tempFolder = SPOFolder.BuildUnRootFolder(parentFolder, folderName, -1);
                            parentFolder.SubFolders.Add(tempFolder);
                        }
                        parentFolder = tempFolder;
                    }

                    var id = item.ItemId;
                    if (item.FileSize > 0)
                    {
                        var spoItem = new SPOItem()
                        {
                            Id = id,
                            Name = name
                        };
                        parentFolder.Items.Add(spoItem);
                    }
                    else
                    {
                        mLog.Info($"The Object {item.FullUrl} size less 0 and update CGDBStatus .");
                        string siteIDString = mConfiguration.SiteCollectionID.ToString();
                        string siteUrlString = mConfiguration.SiteCollectionUrl;
                        //CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).UpdateStatus(siteIDString, item.itemId, BackupRestoreStatus.Skipped);
                    }
                }
            } while (items.Count() == size);
        }

        private string GetLastItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit Paged='True'>1</RowLimit>
                </View>";

            return result;
        }
        private string GetLastFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit Paged='True'>1</RowLimit>
                </View>";
            return result;
        }
        /// <summary>
        /// Process folder for initialization
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="needInitInfo"></param>
        public async virtual System.Threading.Tasks.Task ProcessFolderAsync(ArchiverNodeItem folder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessFolder"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(folder, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//add for RevIM RECO-84
                    {
                        return;
                    }
                    await ProcessItemsAndSubfoldersAsync(folder, folder.Cache_NodeType);

                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("Folder UserName Or Password Incorrect. Path:{0}. Message:{1}.", folder.FullPath, IUPEx.ToString());
                    throw;
                }
                catch (SPObjectReadOnlyException sroe)
                {
                    mLog.Info("Folder is ReadOnly. Path:{0}. Message:{1}.", folder.FullPath, sroe.ToString());
                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("Folder is Locked. Path:{0}. Message:{1}.", folder.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("Folder Not Found. Path:{0}. Message:{1}.", folder.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while processing folder node.Path:{0}.Message:{1}.", folder.FullPath, e.ToString());
                    //throw; 非特定异常Folder Scan失败，不应该影响整体Job状态，Folder失败即可。SAAS-38055
                }
            }
        }

        public async virtual System.Threading.Tasks.Task ProcessItemAsync(ArchiverNodeItem nodeItem, IDiscoverNodeWorker discoverWorker, bool needInitInfo = false)
        {
            this.discoverWorker = discoverWorker;
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessItem"))
            {
                ArchiverNodeItem folderItem = null;
                int tempIdx = nodeItem.FullPath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                // if method up can not get IAveDiscoverFolder  ,we can new  an  NodeItem as Folder 
                folderItem = new ArchiverNodeItem()
                {
                    WebApplicationId = nodeItem.WebApplicationId,
                    WebApplicationUrl = nodeItem.WebApplicationUrl,
                    SiteId = nodeItem.SiteId,
                    WebId = nodeItem.WebId,
                    ListId = nodeItem.ListId,
                    SiteUrl = nodeItem.SiteUrl,
                    FullPath = tempIdx != -1 ? nodeItem.FullPath.Substring(0, tempIdx) : nodeItem.FullPath,
                    ID = nodeItem.FolderId,
                    Cache_NodeType = (int)CacheNodeType.Folder,
                    SPNodeLevel = nodeItem.IsRootFolder ? NodeLevel.RootFolder : NodeLevel.Folder
                };
                if (needInitInfo)
                {
                    await InitialSPObjectInfoAsync(discoverWorker, folderItem);
                }
                AveDiscoverItem item = null;
                AveDiscoverFolder folder = (AveDiscoverFolder)folderItem.DiscoverSPObject;
                folderItem.Name = folder.ItemName;
                folderItem.FullPath = folder.FullUrl;
                ProcessResult result = await discoverWorker.ProcessContainerAsync(folderItem, ProcessType.NeedProcess);
                //item = ((IAveDiscoverFolder)folderItem.DiscoverSPObject).GetItemById(nodeItem.ItemId);
                List<AveDiscoverItem> discoverItems = null;
                int retryTime = 0;
                while (retryTime < 10)
                {
                    try
                    {
                        discoverItems = ((AveDiscoverFolder)folderItem.DiscoverSPObject).GetItems();
                        mLog.Info("GetItems success in ProcessItem");
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryTime++;
                        discoverItems = new List<AveDiscoverItem>();
                        mLog.Warn("GetItems Failed in ProcessItem and retry.RetryTime:{0}.Message:{1}.", retryTime, ex.ToString());
                        await System.Threading.Tasks.Task.Delay(5 * 1000);
                    }
                }
                if (discoverItems != null)
                {
                    foreach (AveDiscoverItem aveDiscoverItem in discoverItems)
                    {
                        if (aveDiscoverItem.DocID == nodeItem.ItemId)
                        {
                            item = aveDiscoverItem;
                            await ProcessVersionAndAttachmentsAsync(item, (AveDiscoverFolder)folderItem.DiscoverSPObject, folderItem, discoverWorker);
                            break;
                        }
                    }
                }
                if (item == null)
                {
                    throw new Exception("The Item In This Library Or List Do Not Found");
                }
            }
        }

        public async virtual System.Threading.Tasks.Task ProcessItemsAndSubfoldersAsync(ArchiverNodeItem folderNode, int folderLevel, bool needInitInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RealProcessItemsAndSubfolders"))
            {
                AveDiscoverFolder rootFolder = (folderNode.DiscoverSPObject as AveDiscoverFolder);
                #region process items/documents
                int totalItemCount = rootFolder.GetItemCount();
                try
                {
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseTotalFiles(totalItemCount);
                    }
                    if (mConfiguration.SkipDiscoverItemForFolderLevelRule)
                    {
                        mLog.Info("Current rule is folder rule and skip discover folder sub items.Path:{0}.", folderNode.FullPath);
                    }
                    else
                    {
                        foreach (var items in rootFolder.GetItemsWithStructureForArchiver())
                        {
                            mLog.Info("Current GetItemsWithStructureForArchiver Items Count:{0}.", items.Count);
                            await ProcessDataAsync(items,rootFolder, folderNode, discoverWorker);
                            rootFolder.ClearSubItemsCache();
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.", folderNode.FullPath, ex.ToString());
                }
                finally
                {
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.Folder, 0);
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles(totalItemCount);
                    }
                }
                #endregion

                #region process folders
                try
                {
                    foreach (var folders in rootFolder.GetFoldersWithStructure(true))
                    {
                        mLog.Info("Curent GetFoldersWithStructure folders Count:{0}.", folders.Count);
                        var folderIds = folders.Where(x => x.ID != null).Count() != 0 ? folders.Where(x => x.ID != null).Select(x => x.ID.Value).ToList() : new List<int>();
                        await ProcessDataAsync(folders, folderNode, discoverWorker, needInitInfo: needInitInfo);
                        rootFolder.ClearSubFoldersCache();
                        //Remove IAveFolder Cache.每次Query出的Folder外围处理结束后，清除当次Query缓存的IAveFolder，避免造成内存问题.
                        mLog.Info("Begin remove folder cache GetFoldersWithStructurForArchiver.RemomveCount:{0}.FullPath:{1}.", folderIds.Count, folderNode.FullPath);
                        rootFolder.RemoveFolderCache(folderIds);
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.", folderNode.FullPath, ex.ToString());
                }
                #endregion
                if (rootFolder != null)
                {
                    rootFolder.Dispose();
                }
            }
        }

        public virtual async System.Threading.Tasks.Task InitialSPObjectInfoAsync(IDiscoverNodeWorker discoverWork, ArchiverNodeItem node)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitialSPObjectInfo"))
            {

                mDiscoverSite = InitDiscoverSite(node); //tmpDiscoverSite;
                //初始化Site对象的一些信息。  
                Uri uri = new Uri(node.SiteUrl);
                mConfiguration.mInitialTime = DateTime.Now;
                if (mDiscoverSite.Site == null)
                {
                    throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanProcessSiteSPObjectNotFoundException, "SiteCollection", node.FullPath);
                }
                mDependencyObjs.PutIn(mDiscoverSite.Site, (int)CacheNodeType.SiteCollection, false);
                if (node.SPNodeLevel == NodeLevel.SiteCollection)
                {
                    node.DiscoverSPObject = mDiscoverSite;
                    if (discoverWork != null)
                    {
                        //ProcessResult result = await discoverWork.ProcessDiscoverOptimizationContainerAsync(node.GenerateWebappNodeItem(), ProcessType.NeedProcess);
                    }
                    return;
                }
                JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.SiteCollection, 0);
                switch (node.SPNodeLevel)
                {
                    case NodeLevel.Site:
                        {
                            //ADO-189107 Folder/Site Rule，Backup Discover过程中不需要再赋值默认CacheNodeType。
                            if (mConfiguration.ObjectCache.ContainsKey(node.ID) && mConfiguration.ObjectCache[node.ID] == node.Cache_NodeType)
                            {
                                //node.Cache_NodeType = node.Cache_NodeType;
                            }
                            else if (node.Cache_NodeType <= (int)CacheNodeType.List / 2)
                            {
                                node.Cache_NodeType = (int)CacheNodeType.List / 2;
                            }
                            node = await InitNodeEntityRelatedInfoAsync(discoverWork, node, mConfiguration.AutoApproval, true);
                            //node.Name = (node.DiscoverSPObject as AveDiscoverWeb).Name;//防止取node name时只取site 的name，这里要取site 的相对name。
                            break;
                        }
                    case NodeLevel.Library:
                    case NodeLevel.List:
                        {
                            node.IsSystemObject = false;
                            node = await InitNodeEntityRelatedInfoAsync(discoverWork, node, mConfiguration.AutoApproval, true);
                            break;
                        }
                    case NodeLevel.RootFolder:
                    case NodeLevel.Folder:
                        {
                            IAveWeb spweb = null;
                            IAveList splist = null;
                            try
                            {
                                spweb = mDiscoverSite.Site.OpenWeb(node.WebId);
                            }
                            catch (Exception exc)
                            {
                                mLog.Info("Init Folder Level SPWeb" + exc.ToString());
                                spweb = mDiscoverSite.Site.OpenWeb();
                            }
                            mDependencyObjs.PutIn(spweb, (int)CacheNodeType.Web, false);
                            splist = spweb.GetList(node.FullPath);
                            mDependencyObjs.PutIn(splist, (int)CacheNodeType.List, false);
                            //当Folder Level大于5000时用原本的CacheNodeType，以保证添加到PC Container中 ADO-183775
                            if (node.Cache_NodeType <= (int)CacheNodeType.Item / 2)
                            {
                                node.Cache_NodeType = ((int)CacheNodeType.Item) / 2;
                            }
                            node.IsSystemObject = false;
                            node = await InitNodeEntityRelatedInfoAsync(discoverWork, node, mConfiguration.AutoApproval, true);
                            break;
                        }
                    default: break;
                }

                IAveWeb web = null;
                IAveList list = null;

                if (node.SPNodeLevel > NodeLevel.SiteCollection)
                {
                    try
                    {
                        web = mDiscoverSite.Site.OpenWeb(node.WebId);
                    }
                    catch (Exception exce)
                    {
                        mLog.Info("Get Final SPWeb" + exce.ToString());
                        web = mDiscoverSite.Site.OpenWeb();
                    }
                    mDependencyObjs.PutIn(web, (int)CacheNodeType.Web, false);
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.Web, 0);
                }
                if (node.SPNodeLevel > NodeLevel.Site && web != null)
                {
                    list = web.GetList(node.FullPath);
                    mLog.Info("Current list [{0}] ItemCount [{1}].", node.FullPath, list.ItemCount);
                    mDependencyObjs.PutIn(list, (int)CacheNodeType.List, false);
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.List, 0);
                }
            }
        }

        public static void AssignSPObjectId(SPTreeNodeDto node, ref RuleNodeContract config)
        {
            if (node.Level != NodeLevel.O365GroupSitesGroup
                && node.Level != NodeLevel.PrivateChannelGroup
                && node.Level != NodeLevel.SkyDriveProGroup
                && (node.Level >= NodeLevel.Folder || node.Level == NodeLevel.Sites || node.Level == NodeLevel.Lists))
            {
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.List)
            {
                config.ListId = node.SPObjectId;
                config.ListTitle = node.Name;
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.Site)
            {
                if (string.IsNullOrEmpty(config.WebId))
                {
                    config.WebId = node.SPObjectId;
                }
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.SiteCollection)
            {
                config.SiteId = node.ID;
                config.SiteUrl = node.Url;
                if (node.Parent != null)
                {
                    AssignSPObjectId(node.Parent, ref config);
                }
            }
            if (node.Level == NodeLevel.WebApplication
              || node.Level == NodeLevel.O365GroupSitesGroup
              || node.Level == NodeLevel.SkyDriveProGroup
              || node.Level == NodeLevel.PrivateChannelGroup)
            {
                config.WebAppId = node.SPObjectId;
                config.WebAppUrl = node.FullPath;
            }
        }

        #region private methods

        internal async System.Threading.Tasks.Task ProcessDataAsync(List<AveDiscoverItem> items, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessData"))
            {
                foreach (AveDiscoverItem item in items)
                {
                    string itemFullUrl = item.FullUrl;
                    try
                    {
                        using (new CheckJobStopScope()) { }
                        if (LinkFileCommon.StubFileNameSuffixList.Contains(System.IO.Path.GetExtension(item.LeafName)) && item.CurrentItem != null
                            && item.CurrentItem.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                            && item.CurrentItem.FieldValues[LinkFileCommon.LinkFileFieldName] != null
                            && item.CurrentItem.FieldValues[LinkFileCommon.LinkFileFieldName].ToString().Length > 0)
                        {
                            mLog.Info($"skip stub file:{item.ID}");
                            continue;
                        }
                        await ProcessVersionAndAttachmentsAsync(item, rootFolder, folderNode, discoverWorker);
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception exc)
                    {
                        mLog.Error(string.Format("Error in Backup Single Item :{0}.ItemFullPath:{1}.", exc.ToString(), itemFullUrl));
                    }
                    item.Dispose();
                }
            }
        }

        internal async virtual System.Threading.Tasks.Task ProcessVersionAndAttachmentsAsync(AveDiscoverItem item, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersionAndAttachments"))
            {
                using (ArchiverNodeItem itemNode = folderNode.GenerateItemNodeItem(item, rootFolder, mConfiguration))
                {
                    if (!useDalDiscoverDBService)
                    {
                        var fileRecord = itemManager.SelectValuesFromDBByItemUniqueIds(itemNode.ID.ToString()).FirstOrDefault();
                        if (fileRecord != null && fileRecord.ModifiedTime.Ticks + TimeSpan.FromMinutes(1).Ticks < itemNode.Modified)
                        {
                            mLog.Warn($"this file:{fileRecord.FullUrl} has beed modified,item id:{itemNode.ID},fileRecord:{fileRecord.ModifiedTime.Ticks},itemModified{itemNode.Modified}");
                            return;
                        }
                    }

                    ProcessResult result = await discoverWorker.ProcessItemAsync(itemNode, folderNode);
                    if (result == ProcessResult.CurrentVersionHasApprove || result == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }
                    Stopwatch watch = Stopwatch.StartNew();
                    //Progress attachments 
                    if (item.GetAttachments().Count > 0)
                    {
                        foreach (AveItemObject attachment in item.GetAttachments())
                        {
                            await ProcessAttachmentsAsync(folderNode, itemNode, attachment, discoverWorker);
                        }
                    }
                    //Progress item versions
                    if (item.GetVersions().Count > 1)
                    {
                        foreach (AveVersionObject version in item.GetVersions())
                        {
                            if ((version.Uiversion == item.Uiversion) || (version.Uiversion == 0))
                            {
                                continue;
                            }
                            try
                            {
                                await ProcessVersionsAsync(itemNode, version, folderNode, discoverWorker);
                            }
                            catch (Exception ex)
                            {
                                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanProcessItemVersionsError + ex.ToString());
                            }
                        }
                    }
                    watch.Stop();
                    mLog.Info("ProcessVersionAndAttachments GetAttachments GetVersions costs: {0}.", watch.Elapsed);
                }
            }
        }
        internal async virtual System.Threading.Tasks.Task ProcessVersionsAsync(ArchiverNodeItem item, AveVersionObject version, ArchiverNodeItem folder, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersions"))
            {
                ArchiverNodeItem versionNode = item.GenerateItemVersionNodeItem(version, item, mConfiguration);
                var result = await discoverWorker.ProcessItemAsync(versionNode, item);
            }
        }

        internal async System.Threading.Tasks.Task ProcessDataAsync(List<AveDiscoverFolder> folders, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker, bool needInitInfo = false)
        {
            foreach (AveDiscoverFolder folder in folders)
            {
                using (new CheckJobStopScope()) { }
                if (folderNode.Parent != null && !string.IsNullOrEmpty(folderNode.Parent.RuleId) && folderNode.Parent.DoDelete)
                {
                    mLog.Warn("Folder parent is match rule, so skip BreakInherit check.{0}", folder.FullUrl);
                }
                else if (discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(Site.Url, folder.FullUrl)))
                {
                    mLog.Warn("Folder {0} is break inherit or is null", folder.FullUrl);
                    continue;
                }


                ArchiverNodeItem subFolderNode = folderNode.GenerateFolderNodeItem(folder, NodeLevel.Folder, mDiscoverSite.Site.Url, mConfiguration);
                ProcessResult result = await discoverWorker.ProcessContainerAsync(subFolderNode, ProcessType.NeedProcess);
                if (result == ProcessResult.SkipCurrentNode)
                {
                    continue;
                }
                //add folder attachment
                if (folder.GetAttachments().Count > 0)
                {
                    foreach (AveItemObject attachment in folder.GetAttachments())
                    {
                        await ProcessAttachmentsAsync(folderNode, subFolderNode, attachment, discoverWorker);

                    }
                }
                if (folder.GetVersions().Count > 1)
                {
                    foreach (AveVersionObject version in folder.GetVersions())
                    {
                        if ((version.Uiversion == folder.Uiversion) || (version.Uiversion == 0))
                        {
                            continue;
                        }
                        await ProcessFolderVersionsAsync(version, subFolderNode, folder, discoverWorker);
                    }
                }
                await ProcessItemsAndSubfoldersAsync(subFolderNode, subFolderNode.Cache_NodeType, needInitInfo: needInitInfo);
                folder.Dispose();
            }
        }

        private async System.Threading.Tasks.Task ProcessFolderVersionsAsync(AveVersionObject version, ArchiverNodeItem folder, AveDiscoverFolder disFolder, IDiscoverNodeWorker discoverWorker)
        //for folder's version
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessFolderVersions"))
            {
                using (new CheckJobStopScope()) { }
                int subId = 0;
                if (disFolder.ID != null)
                {
                    subId = (int)disFolder.ID;
                }

                ArchiverNodeItem folderVersionNode = folder.GenerateFolderVersionNodeItem(version, NodeLevel.Folder, disFolder);
                ProcessResult result = await discoverWorker.ProcessContainerAsync(folderVersionNode, ProcessType.NeedProcess);
            }
        }

        internal async virtual System.Threading.Tasks.Task ProcessAttachmentsAsync(ArchiverNodeItem folderNode, ArchiverNodeItem item, AveItemObject attachment, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessAttachments"))
            {
                ProcessResult result = ProcessResult.Default;
                try
                {
                    using (new CheckJobStopScope()) { }
                    ArchiverNodeItem attachmentNode = null;
                    switch (item.ItemType)
                    {
                        case ArchiverCommon.ItemType.ITEM_TYPE:
                            attachmentNode = item.GenerateAttachmentNodeItem(attachment, (AveDiscoverFolder)folderNode.DiscoverSPObject);
                            result = await discoverWorker.ProcessItemAsync(attachmentNode, item);
                            break;
                        default:
                            attachmentNode = item.GenerateAttachmentNodeFolder(attachment, (AveDiscoverFolder)item.DiscoverSPObject);
                            result = await discoverWorker.ProcessItemAsync(attachmentNode, item);
                            break;
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while processing attachments.Path:{0}.Message:{1}.", item.FullPath, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Convert tree node to RuleNodeContract.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RuleNodeContract ConvertTreeNodeToRuleNodeConfig(SPTreeNodeDto node, RuleNodeType type, RMDiscoverOptimizationNode discoverNode )
        {
            if (node == null)
            {
                return null;
            }
            RuleNodeContract result = new RuleNodeContract();
            result.Id = Guid.NewGuid().ToString();
            result.NodeId = discoverNode.SiteId.ToString();
            //result.NodeName = node.;
            result.DisplayName = node.DisplayName;
            result.ManagerTreeId = node.ID;
            result.FullPath = discoverNode.SiteUrl;
            //result.FarmId = node.FarmID;
            //result.SPType = node.SPType;
            //if (node.NodeExtension != null && node.NodeExtension.BposInfo != null)
            //{
            //    result.BposInfo = node.NodeExtension.BposInfo;
            //}
            //if (node.Parent != null)  //Farm 级别没有Parent
            //{
            //    if (node.Parent.Level == NodeLevel.Sites || node.Parent.Level == NodeLevel.Lists || node.Parent.Level == NodeLevel.Folders)
            //    {
            //        result.ParentNodeId = node.Parent.Parent == null ? null : node.Parent.Parent.SPObjectId;
            //        result.ParentNodeName = node.Parent.Parent == null ? null : node.Parent.Parent.Name;
            //    }
            //    else
            //    {
            //        result.ParentNodeId = node.Parent.SPObjectId;
            //        result.ParentNodeName = node.Parent.Name;
            //    }
            //}
            result.NodeLevel = NodeLevel.SiteCollection;
            //result.SPVersion = node.SPVersion;
            result.Type = type;
            AssignSPObjectId(node, ref result);
            //在处理index的时候需要转换children
            result.BreakInheritNodesEncryptBySha1 = new Dictionary<string, RuleNodeContract>();
            //var breakInheritUrl = LoadBreakInheritNodeUrls(node.FullPath);
            //foreach (var b in breakInheritUrl)
            //{
            //    var sh1 = ArchiverCommonStaticMethod.GetBreakInheritSHA1String(b);
            //    result.BreakInheritNodesEncryptBySha1[sh1] = null;
            //}
            return result;
        }

        private AveDiscoverSite InitDiscoverSite(ArchiverNodeItem node)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitDiscoverSite"))
            {
                if (mDiscoverSite != null && string.Compare(mDiscoverSite.Site.Url, node.SiteUrl, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return mDiscoverSite;
                }

                if (mDiscoverSite != null)
                {
                    mDiscoverSite.Dispose();
                }
                var bposInfo = GetBposInfoBySite(node.SiteUrl);
                mFactory = MultiAppUtil.CreateAveObjectModelFactory(node.SiteUrl, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.

                try
                {

                    Site = mFactory.CreateSite(node.SiteUrl);
                    try
                    {
                        long storageMaximumLevel = Site.Quota.StorageMaximumLevel * 1024L * 1024L;
                        mLog.Info($"Current Site:{Site.Url} StorageMaximumLevel is:{Site.Quota.StorageMaximumLevel}.Storage is:{Site.Usage.Storage}.ByteStorageMaximumLevel:{storageMaximumLevel}.");
                        if (Site.Quota.StorageMaximumLevel == 0)
                        {
                            //special env,special site does not permission to get this value, so skip this check when size is 0.
                            mLog.Info($"CheckAveExceedStorageLimit.Current Site:{Site.Url} StorageMaximumLevel is 0, skip check current site storage limit.");
                        }
                        else if (Site.Usage.Storage >= storageMaximumLevel)
                        {
                            mConfiguration.JobReportDto.summaryComments = "RM_JM_SiteStorageLimit_ErrorMessage";
                            throw new AveExceedStorageLimitException("This site has exceeded its maximum file storage limit.");
                        }
                    }
                    catch (AveExceedStorageLimitException e)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"Get Site Storage StorageMaximumLevel Error.error:{e}");
                    }
                }
                catch (AveExceedStorageLimitException aex)
                {
                    throw;
                }
                catch (Exception e)
                {
                    var we = e.InnerException as WebException;
                    if (we != null)
                    {
                        if (we.Status == WebExceptionStatus.ProtocolError)
                        {
                            var httpResp = (we.Response as HttpWebResponse);
                            if (httpResp != null)
                            {
                                if (httpResp.StatusCode == HttpStatusCode.NotFound)
                                {
                                    mLog.Error("[DirtyData] SiteCollection {0} is deleted, error: {1}", node.FullPath, e.ToString());
                                    //base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty, string.Empty, string.Empty, JobReportDetailStatus.Failed, "RM_SS_SiteRemovedFromDAO");
                                    throw;
                                }
                            }
                        }
                    }
                    throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanProcessSiteSPObjectNotFoundException, "SiteCollection", node.FullPath); ;
                }
                #region RevIM job获取自定义属性
                try
                {
                    if (mConfiguration.IsILMode && mConfiguration.RuleCollection != null && mConfiguration.RuleCollection.Values.FirstOrDefault()?.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)
                    {
                        try
                        {
                            mLog.Info("Current rule is ArchiverRuleForRevIM job and need get site collection information, site collection url is :{0}.", node.FullPath);
                            Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            try
                            {
                                node.SiteTitle = Site.RootWeb.Title;
                                columnCollectionOfDisplayName["author"] = Site.Owner.Name;
                                columnCollectionOfDisplayName["editor"] = Site.RootWeb.CurrentUser.Name;
                                node.ItemDisplayColumns = columnCollectionOfDisplayName;
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("Get Version Properties Error{0}", e.ToString());
                            }
                        }
                        catch (Exception exp)
                        {
                            mLog.Warn("Error in Get item columns : " + exp.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("Get RevIM Binding Column Errror, message: {0}", e.ToString());
                }
                #endregion
                AveDiscoverSite tmpDiscoverSite = new AveDiscoverSite(Site, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);

                return tmpDiscoverSite;
            }
        }

        private bool GetEnableRemoveReadOnlyState()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableRemoveReadOnlyState");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        internal async System.Threading.Tasks.Task ProcessListCollectionAsync(ArchiverNodeItem web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessListCollection"))
            {
                Dictionary<Guid, AveDiscoverList> discoveryLists;
                discoveryLists = (web.DiscoverSPObject as AveDiscoverWeb).GetLists();
                foreach (AveDiscoverList list in discoveryLists.Values)
                {
                    using (new CheckJobStopScope()) { }


                    if (list.ItemCount == 0)
                    {
                        mLog.Info($"ProcessListCollectionAsync:discover optimization current list item count is 0.ListURL:{list.RootFolderUrl}.Title:{list.Title}.");
                        continue;
                    }

                    if (mConfiguration?.RMDiscoveryOptimizationSetting?.MS365DataType == (int)MS365DataType.Phl)
                    {
                        if (!CheckIsPHLList(list.Name + list.ListTemplate.ToString()) 
                            && !CheckIsPHLList(CombineListUrlAndTemplate(list)))
                        {
                            mLog.Info($"ProcessListCollectionAsync:Skip the un phl list.ListTitle:{list.Title}.ListName:{list.Name}.ListTemplate:{list.ListTemplate}.ListURL:{list.RootFolderUrl}.");
                            continue;
                        }
                    }
                    else
                    {
                        if (CheckIsDesignList(list.Name + list.ListTemplate.ToString()))
                        {
                            mLog.Info($"ProcessListCollectionAsync:Skip the design list.ListTitle:{list.Title}.ListURL:{list.RootFolderUrl}.");
                            continue;
                        }
                        if (CheckIsDesignList(CombineListUrlAndTemplate(list)))
                        {
                            mLog.Info($"ProcessListCollectionAsync:Skip the design list by URL and Template.ListTitle:{list.Title}.ListURL:{list.RootFolderUrl}.");
                            continue;
                        }
                        if (list.Hidden.HasValue && list.Hidden.Value)
                        {
                            mLog.Info($"ProcessListCollectionAsync:discover optimization current list is Hidden.ListURL:{list.RootFolderUrl}.Title:{list.Title}.");
                            continue;
                        }
                        if (list.Title.Equals("{System Folder}"))
                        {
                            mLog.Info("Current list is System Folder when discover list collection, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);
                            continue;
                        }
                    }
                    try
                    {
                        using (itemManager = DiscoveryInsiteEngineItemManager.GetInstance())
                        {
                            IAveWeb tmpWeb = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as IAveWeb;
                            IAveList tmpList = tmpWeb.GetList(list.RootFolderUrl);
                            mLog.Info("Current list [{0}] ItemCount [{1}].", list.RootFolderUrl, tmpList.ItemCount);
                            if (tmpList != null && tmpList.BaseType == AveBaseType.GenericList)
                            {
                                mLog.Info($"ProcessListCollectionAsync.Current list is general list so skip process.List Title: {tmpList.Title}.");
                                continue;
                            }
                            if (IrmLeaveStubListSkipHelper.TryGetListLevelMatchedRule(mConfiguration, tmpList, out var matchedRule))
                            {
                                mLog.Info(
                                    "Skip list for leave-stub IRM restriction in DSO. ListTitle:{0}, RuleId:{1}, RuleName:{2}, KeepDataOption:{3}, PolicyLevel:{4}, IrmEnabled:{5}, IrmReject:{6}.",
                                    tmpList.Title,
                                    matchedRule?.Id,
                                    matchedRule?.Name,
                                    matchedRule?.KeepDataOption,
                                    matchedRule?.PolicyLevel,
                                    tmpList.IrmEnabled,
                                    tmpList.IrmReject);

                                mConfiguration.JobReportDto.AddScanReport(
                                    list.RootFolderUrl,
                                    0,
                                    (int)CacheNodeType.List,
                                    string.Empty,
                                    JobDetailsStatus.Skipped,
                                    IrmLeaveStubListSkipHelper.SkipReportMessageKey);
                                continue;
                            }
                            string siteIDString = mConfiguration.SiteCollectionID.ToString();
                            string siteUrlString = mConfiguration.SiteCollectionUrl;
                            if (mConfiguration.IsProcessDuplicateDatas)
                            {
                                List<string> itemIdList = new List<string>();
                                if (mConfiguration.currentRule.KeepDataOption == (int)KeepDataOption.ArchiveBackupAndRemove)
                                {
                                    itemIdList = CleanUpItemEntrys.Where(a => a.Action == ArchiveConstants.ArchiveAction).Select(a => a.ItemId).ToList();
                                    mLog.Info($"this ProcessDuplicateDatas rule is ArchiveBackupAndRemove,and the itemIdListCount is:{itemIdList?.Count},CleanUpItemEntrys count is:{CleanUpItemEntrys?.Count}");
                                }
                                else if (mConfiguration.currentRule.KeepDataOption == (int)KeepDataOption.DeleteOnly && WrapperConfiguration.HasDeleteOnlyLicense)
                                {
                                    itemIdList = CleanUpItemEntrys.Where(a => a.Action == ArchiveConstants.DestroyAction).Select(a => a.ItemId).ToList();
                                    mLog.Info($"this ProcessDuplicateDatas rule is DeleteOnly,and the itemIdListCount is:{itemIdList?.Count},CleanUpItemEntrys count is:{CleanUpItemEntrys?.Count}");
                                }
                                else
                                {
                                    mLog.Info($"ProcessListCollectionAsync.KeepDataOption: {mConfiguration.currentRule.KeepDataOption}.HasDeleteOnlyLicense:{WrapperConfiguration.HasDeleteOnlyLicense}");
                                }
                                string webId = web.WebId.ToString();
                                string listId = list.ListId.ToString();
                                await foreach (var tempFileInfo in GetAllDuplicateFilesAsync(siteIDString, webId, listId, itemIdList))
                                {
                                    mLog.Info($"this ProcessDuplicateDatas GetAllDuplicateFilesAsync tempFileInfo count:{tempFileInfo?.Count},siteId:{siteIDString},webid:{web.WebId.ToString()},listid:{list.ListId.ToString()}");
                                    ProcessDiscoveryFileDataInfos(itemManager, tempFileInfo, tmpList);
                                }
                            }
                            else
                            {
                                List<string> tagRuleIds = useDalDiscoverDBService
                                    ? null
                                    : mConfiguration.DiscoveryO365RuleInfoCache.Keys.ToList();
                                await foreach (var tempFileInfo in GetAllFilesAsync(siteIDString, web.WebId.ToString(), list.ListId.ToString(), tagRuleIds))
                                {
                                    ProcessDiscoveryFileDataInfos(itemManager, tempFileInfo, tmpList);
                                }
                            }
                            if (mConfiguration.IsDiscoverOptimizationPreScan)
                            {
                                mLog.Info($"ProcessListCollectionAsync: This is the discover optimization pre-scan process, so skip insert file data infos to database. ListTitle: {tmpList.Title}.");
                                continue;
                            }
                            itemManager.WaitInsertFinish();
                            long count = itemManager.GetCoutOfData();
                            mLog.Info($"discover optimization current list file count is:{count}");
                            if (count > 0)
                            {
                                mLog.Info("Begin discover list, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);
                                ArchiverNodeItem ListNode = web.GenerateListNodeItem(list, tmpList);
                                mDependencyObjs.PutIn(tmpList, (int)CacheNodeType.List, false);
                                using (ListNode)
                                {
                                    await ProcessListAsync(ListNode);
                                }
                            }
                        }
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARScanProcessListCollectionError, list.Title, ex.ToString());
                    }
                    finally
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles(list.ItemCount);
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.List, 0);
                    }
                }
            }
        }


        private string GetOrCacheUserLoginName(Dictionary<int, string> userCache, int? userId, IAveList aveList)
        {
            if (!userId.HasValue)
                return null;
           
            if (userCache.TryGetValue(userId.Value, out string cachedName))
                return cachedName;
           
            try
            {
                IAveUser tmpUser = aveList.ParentWeb.SiteUsers.GetByID(userId.Value);
                if (tmpUser != null)
                {
                    string userName = tmpUser.NoPrefixLoginNameForArchiver;
                    userCache[userId.Value] = userName;
                    return userName;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"GetOrCacheUserEmail: Get User by ID Error,ID:{userId.Value}, Message:{ex}.");
            }
            return string.Empty; 
        }

        private void ProcessDiscoveryFileDataInfos(DiscoveryInsiteEngineItemManager ieItemManager, List<RMDiscoveryFileDataInfo> fileDataInfos, IAveList aveList)
        {
            if (!mConfiguration.IsDiscoverOptimizationPreScan)
            {
                ieItemManager.InsertValue(fileDataInfos);
                return;
            }
            if (fileDataInfos == null || fileDataInfos.Count == 0)
            {
                return;
            }

            var userLoginNameCache = new Dictionary<int, string>();
            bool hasDiscoveryRules = mConfiguration.DiscoveryO365RuleInfoCache.Any();
            bool hasRuleCollection = mConfiguration.RuleCollection.Any();

            if (useDalDiscoverDBService)
            {
                var currentRule = mConfiguration.currentRule;
                var currentPolicyLevel = currentRule?.PolicyLevel ?? PolicyLevel.Document;
                List<RMDiscoveryOffice365RuleInfo> matchedRules = new();

                if (hasDiscoveryRules)
                {
                    if (currentPolicyLevel == PolicyLevel.DocumentVersion)
                    {
                        matchedRules = mConfiguration.DiscoveryO365RuleInfoCache.Values
                            .Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version)
                            .ToList();
                    }
                    else
                    {
                        matchedRules = mConfiguration.DiscoveryO365RuleInfoCache.Values
                            .Where(r => r.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.Version)
                            .ToList();
                    }

                    if (matchedRules.Count == 0)
                    {
                        matchedRules = mConfiguration.DiscoveryO365RuleInfoCache.Values.ToList();
                    }
                }

                string ruleName = matchedRules.Count > 0
                    ? string.Join("; ", matchedRules.Select(r => r.Name))
                    : string.Empty;
                string action = mConfiguration.GetRuleArchiverActionString(currentRule ?? new Rule { KeepDataOption = -1 }, true);

                foreach (var fileData in fileDataInfos)
                {
                    long itemSize = fileData.FileSize;
                    if (currentPolicyLevel == PolicyLevel.DocumentVersion)
                    {
                        if (fileData.Versions != null && fileData.Versions.Count > 0)
                        {
                            itemSize = fileData.Versions.Sum(v => v.VersionSize);
                        }
                        else if (fileData.HistoryVersionsSize > 0)
                        {
                            itemSize = fileData.HistoryVersionsSize;
                        }
                    }
                    else
                    {
                        itemSize += fileData.HistoryVersionsSize;
                    }

                    mConfiguration.JobReportDto.AddScanReportForSimulation(
                        mConfiguration.GetNodeFullPath(fileData.FullUrl),
                        itemSize,
                        (int)CacheNodeType.Item,
                        ruleName,
                        action,
                        fileData.CreatedTime.Ticks,
                        GetOrCacheUserLoginName(userLoginNameCache, (int)fileData.AuthorId, aveList),
                        fileData.ModifiedTime.Ticks,
                        GetOrCacheUserLoginName(userLoginNameCache, (int)fileData.EditorId, aveList)
                    );
                }

                return;
            }

            Rule targetRule = null;

            var versionRules = hasDiscoveryRules
                ? mConfiguration.DiscoveryO365RuleInfoCache.Values
                    .Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version)
                    .ToHashSet()
                : new HashSet<RMDiscoveryOffice365RuleInfo>();

            foreach (var fileData in fileDataInfos)
            {
                List<RMDiscoveryOffice365RuleInfo> matchedRules = new();

                if (!hasDiscoveryRules) //All files in the scope
                {
                    targetRule = mConfiguration.RuleCollection.TryGetValue((int)PolicyLevel.Document, out Rule value) ? value : null;
                    fileData.FileSize += fileData.HistoryVersionsSize;
                }

                if (fileData.Tags.HasValue())
                {
                    foreach (var tag in fileData.Tags)
                    {
                        if (mConfiguration.DiscoveryO365RuleInfoCache.TryGetValue(tag.Key, out var rule))
                            matchedRules.Add(rule);
                    }
                    matchedRules.Sort((a, b) => b.AnalyseMethod.CompareTo(a.AnalyseMethod));
                }

                if (hasRuleCollection && hasDiscoveryRules)
                {
                    bool hasMatchedFileRule = matchedRules.Any(r => !versionRules.Contains(r));
                    bool hasMatchedVersionRule = matchedRules.Any(r => versionRules.Contains(r));
                    var ruleLevel = (hasMatchedVersionRule, hasMatchedFileRule) switch
                    {
                        (false, true) => PolicyLevel.Document,
                        (true, true) => PolicyLevel.Document,
                        (true, false) => PolicyLevel.DocumentVersion,
                        _ => PolicyLevel.None
                    };

                    targetRule = mConfiguration.RuleCollection.TryGetValue((int)ruleLevel, out Rule value) ? value : null;

                    if(ruleLevel == PolicyLevel.DocumentVersion)
                    {
                        long maxTotalSize = 0;
                        var versionTagKeys = matchedRules.Where(versionRules.Contains).Select(r => r.ToTagColumn()).ToHashSet();
                        foreach (var tag in fileData.Tags ?? [])
                        {
                            if (!versionTagKeys.Contains(tag.Key)) continue;
                            if (tag.Value is ExpandoObject tagValue &&
                                tagValue.TryGet("total_size") is long totalVersionSize &&
                                totalVersionSize > maxTotalSize)
                            {
                                maxTotalSize = totalVersionSize;
                            }
                        }
                        fileData.FileSize = maxTotalSize;
                    }
                    else
                    {
                        // If there is any file-level rule matched, include all version sizes.
                        fileData.FileSize += fileData.HistoryVersionsSize;
                    }
                }

                mConfiguration.JobReportDto.AddScanReportForSimulation(
                    mConfiguration.GetNodeFullPath(fileData.FullUrl),
                    fileData.FileSize,
                    (int)CacheNodeType.Item,
                    string.Join("; ", matchedRules.Select(r => r.Name)),
                    mConfiguration.GetRuleArchiverActionString(targetRule ?? new Rule { KeepDataOption = -1 }, true),
                    fileData.CreatedTime.Ticks,
                    GetOrCacheUserLoginName(userLoginNameCache, (int)fileData.AuthorId, aveList),
                    fileData.ModifiedTime.Ticks,
                    GetOrCacheUserLoginName(userLoginNameCache, (int)fileData.EditorId, aveList)
                );
            }
        }


        private bool CheckIsPHLList(string listInfo)
        {
            return "Preservation Hold Library1310".Equals(listInfo, StringComparison.OrdinalIgnoreCase)
                || "PreservationHoldLibrary1310".Equals(listInfo, StringComparison.OrdinalIgnoreCase);
        }

        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (this.DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"An error has occurred when CheckIsDesignList, message:{e.Message}");
            }
            return isDesignList;
        }

        private string CombineListUrlAndTemplate(AveDiscoverList discoverList)
        {
            string combineUrlTemplate = string.Empty;
            string listUrl = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(discoverList.RootFolderUrl))
                {
                    int listUrlIndex = discoverList.RootFolderUrl.LastIndexOf("/");
                    //Root Site RootFolderURL like /SitePages
                    if (listUrlIndex >= 0)
                    {
                        listUrl = discoverList.RootFolderUrl.Substring(listUrlIndex + 1);
                    }
                    else
                    {
                        listUrl = discoverList.Name;
                    }
                    combineUrlTemplate = listUrl + discoverList.ListTemplate.ToString();
                    mLog.Info($"CombineListUrlAndTemplate combineUrlTemplate is {combineUrlTemplate}.discoverList.RootFolderUrl:{discoverList.RootFolderUrl}.");
                }
                else
                {
                    mLog.Info("CombineListUrlAndTemplate discoverList.RootFolderUrl is IsNullOrEmpty.");
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"CombineListUrlAndTemplate error: ({ex})");
                combineUrlTemplate = string.Empty;
            }
            return combineUrlTemplate;
        }

        private List<string> GetDesignLists()
        {
            return WebUtil.GetDesignLists(TenantService.IsCSDTenant());
        }

        internal async System.Threading.Tasks.Task ProcessWebCollectionAsync(ArchiverNodeItem web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessWebCollection"))
            {
                Dictionary<Guid, AveDiscoverWeb> discoverWebs = new Dictionary<Guid, AveDiscoverWeb>();
                if (mConfiguration.Procedure == ScheduleProcedure.Scan)
                {
                    discoverWebs = ((AveDiscoverWeb)web.DiscoverSPObject).GetSubWebs(true);
                }
                else
                {
                    discoverWebs = ((AveDiscoverWeb)web.DiscoverSPObject).GetSubWebs();
                }
                foreach (AveDiscoverWeb tmp in discoverWebs.Values)
                {
                    using (new CheckJobStopScope()) { }
                    try
                    {
                        using (ArchiverNodeItem webnode = web.GenerateSiteNodeItem(tmp, mConfiguration, web.Parent.SPNodeLevel == NodeLevel.SiteCollection))
                        {

                            using (IAveWeb iweb = tmp.AveWeb)
                            {
                                try
                                {
                                    //SAAS-20894 在Get Web Properties中获取，这里不需要判断了。
                                    //else if (string.Equals(iweb.WebTemplate, "CMSPUBLISHING") || string.Equals(iweb.WebTemplate, "BLANKINTERNET") || string.Equals(iweb.WebTemplate, "ENTERWIKI"))
                                    //{
                                    //    //SAAS-11588 添加判断条件判断需要执行此步骤的webtemplate(通过调用SiteLogoUrl自动创建出Site Assets List)
                                    //    string subSiteLogoUrl = iweb.SiteLogoUrl;
                                    //}
                                    string subSiteLogoDescription = iweb.SiteLogoDescription;//通过调用SiteLogoDescription自动创建出Site Assets List
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("Get Web Properties Error{0},WebName is {1}", e.ToString(), web.Name);
                                }

                            }
                            await ProcessWebAsync(webnode);
                        }
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (SPObjectNotFoundException e1)
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARScanProcessWebProcressSubWeb, e1);
                    }
                    finally
                    {
                        using (tmp)
                        { };
                    }
                }
            }
        }


        private async System.Threading.Tasks.Task<ArchiverNodeItem> InitNodeEntityRelatedInfoAsync(IDiscoverNodeWorker discoverWork, ArchiverNodeItem node, bool autoApproval, bool firstCall = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitNodeEntityRelatedInfo"))
            {
                ArchiverNodeItem parent = new ArchiverNodeItem();
                //parent.ID = node.ID;
                parent.WebApplicationUrl = node.WebApplicationUrl;
                parent.SiteId = node.SiteId;
                parent.WebId = node.WebId;
                parent.ListId = node.ListId;
                parent.FullPath = node.FullPath;
                parent.SiteUrl = node.SiteUrl;
                switch (node.SPNodeLevel)
                {
                    case NodeLevel.SiteCollection:
                        {
                            IAveSite site = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                            node = parent.GenerateSiteCollectionNodeItem(site, mConfiguration);
                            await InitialSPObjectInfoAsync(discoverWork, node);
                            break;
                        }
                    case NodeLevel.Site:
                        {
                            IAveSite site = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                            bool isRootWeb = false;
                            bool parentIsRootWeb = false;
                            //Merge Code for CI ADO-94697
                            try
                            {
                                using (IAveWeb web = site.OpenWeb(node.ID))
                                {
                                    if (web.IsRootWeb)
                                    {
                                        parent.SPNodeLevel = NodeLevel.SiteCollection;
                                        isRootWeb = true;
                                    }
                                    else
                                    {
                                        parent.SPNodeLevel = NodeLevel.Site;
                                        parent.ID = web.ParentWebId;
                                        parentIsRootWeb = web.ParentWebId.Equals(site.RootWeb.ID);
                                    }
                                    #region records web property
                                    if (mConfiguration.RuleCollection != null && mConfiguration.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)
                                    {
                                        try
                                        {
                                            mLog.Info("Current rule is ArchiverRuleForRevIM job and need get site information, site collection url is :{0}.", node.FullPath);
                                            Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                                            try
                                            {
                                                if (web.IsRootWeb)
                                                {
                                                    columnCollectionOfDisplayName["author"] = site.Owner.LoginName;
                                                }
                                                else
                                                {
                                                    columnCollectionOfDisplayName["author"] = web.Author.LoginName;
                                                }
                                                columnCollectionOfDisplayName["editor"] = web.CurrentUser.Name;
                                                node.ItemDisplayColumns = columnCollectionOfDisplayName;
                                                node.SiteTitle = web.Title;
                                            }
                                            catch (Exception e)
                                            {
                                                mLog.Warn("Get Version Properties Error{0}", e.ToString());
                                            }
                                        }
                                        catch (Exception exp)
                                        {
                                            mLog.Warn("Error in Get item columns : " + exp.ToString());
                                        }
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("Open web with id error, node id {0},{1}", node.ID, e.ToString());
                                mLog.Info("Will Open web with url again, node url {0}", node.FullPath);
                                using (IAveWeb web = site.OpenWeb(node.FullPath))
                                {
                                    if (web != null && web.Exists)
                                    {
                                        node.ID = web.ID;
                                        if (web.IsRootWeb)
                                        {
                                            parent.SPNodeLevel = NodeLevel.SiteCollection;
                                            isRootWeb = true;
                                        }
                                        else
                                        {
                                            parent.SPNodeLevel = NodeLevel.Site;
                                            parent.ID = web.ParentWebId;
                                            parentIsRootWeb = web.ParentWebId.Equals(site.RootWeb.ID);
                                        }
                                    }
                                    else
                                    {
                                        if (I18NEntity.HasKey(e.Message))
                                        {
                                            throw;
                                        }
                                        throw new SPObjectNotFoundException("StorageOptimization_SOARScanProcessWebSPObjectNotFoundException");
                                    }
                                }
                            }
                            if (isRootWeb)
                            {
                                node.Name = ".";
                            }
                            parent = await InitNodeEntityRelatedInfoAsync(discoverWork, parent, autoApproval);
                            AveDiscoverWeb discoverWeb = null;
                            if (parent.DiscoverSPObject is AveDiscoverSite)
                            {
                                discoverWeb = ((AveDiscoverSite)parent.DiscoverSPObject).GetRootWeb();
                            }
                            else
                            {
                                discoverWeb = ((AveDiscoverWeb)parent.DiscoverSPObject).GetSubWebs()[node.ID];
                            }

                            if (!firstCall)
                            {
                                node = parent.GenerateSiteNodeItem(discoverWeb, mConfiguration, isRootWeb | parentIsRootWeb);
                            }
                            if (node.DiscoverSPObject == null)
                            {
                                node.DiscoverSPObject = discoverWeb;
                            }

                            break;
                        }
                    case NodeLevel.Library:
                    case NodeLevel.List:
                        {
                            parent.SPNodeLevel = NodeLevel.Site;
                            parent.ID = node.WebId;
                            parent = await InitNodeEntityRelatedInfoAsync(discoverWork, parent, autoApproval);
                            using (IAveWeb webs = mDiscoverSite.Site.OpenWeb(node.WebId))
                            {
                                IAveList lists = webs.GetList(node.FullPath);
                                if (null == lists)
                                {
                                    mLog.Warn(string.Format("List {0} Is Null", node.FullPath));
                                    throw new SPObjectNotFoundException("RM_JM_GlobalSearch_CannotFindExchangeItem");
                                }
                                mLog.Info("Current list [{0}] ItemCount [{1}].", node.FullPath, null == lists ? 0 : lists.ItemCount);
                                node.ID = lists.ID;
                                node.Name = lists.Title;
                                node.SPList = lists;
                                if (node.DiscoverSPObject == null)
                                {
                                    AveDiscoverList discoverList = ((AveDiscoverWeb)parent.DiscoverSPObject).GetLists()[node.ID];
                                    if (!firstCall)
                                    {
                                        node = parent.GenerateListNodeItem(discoverList, lists);
                                    }
                                    else
                                    {
                                        node.IsRecord = ArchiverCommonStaticMethod.CheckListRecord(lists);
                                    }
                                    node.DiscoverSPObject = discoverList;
                                    node.ListType = discoverList.Type;
                                }
                                if (mConfiguration.IsILMode && (discoverWorker is RecordsOneDriveScanDiscovrerNodeWorker))
                                {
                                    ((RecordsOneDriveScanDiscovrerNodeWorker)discoverWorker).InitOneDriveItemTermInfoByListId(mConfiguration.SiteCollectionID, lists.ID);
                                }
                            }
                            break;
                        }
                    case NodeLevel.RootFolder:
                        {
                            parent.SPNodeLevel = NodeLevel.List;
                            parent.ID = node.ListId;
                            parent = await InitNodeEntityRelatedInfoAsync(discoverWork, parent, autoApproval);
                            if (node.DiscoverSPObject == null)
                            {
                                AveDiscoverFolder discoverRootFolder = null;
                                //一个List下的Root Folder，只实例化一次即可。对于同一个List下的多个Subfolder符合rule，只需要实例化一次，减少性能浪费.
                                if (mInitNodeEntityRelatedInfoDiscoverRootFolder == null || mInitNodeEntityRelatedInfoRootFolderId != node.ID)
                                {
                                    mLog.Info("Init rootfolder for InitNodeEntityRelatedInfo.Url:{0}.FolderID:{1}.", parent.FullPath, node.ID);
                                    discoverRootFolder = ((AveDiscoverList)parent.DiscoverSPObject).GetRootFolder(true);
                                    mInitNodeEntityRelatedInfoDiscoverRootFolder = discoverRootFolder;
                                    mInitNodeEntityRelatedInfoRootFolderId = node.ID;
                                }
                                else
                                {
                                    mLog.Info("Current list already init rootfolder for InitNodeEntityRelatedInfo.Url:{0}.", parent.FullPath);
                                    discoverRootFolder = mInitNodeEntityRelatedInfoDiscoverRootFolder;
                                }
                                if (!firstCall)
                                {
                                    node = parent.GenerateFolderNodeItem(discoverRootFolder, NodeLevel.RootFolder, mDiscoverSite.Site.Url, mConfiguration);
                                }
                                else
                                {
                                    node.IsRecord = parent.IsRecord;
                                }
                                node.DiscoverSPObject = discoverRootFolder;
                                //rootFolder listType 是 1 需要0 取list的listType
                                node.ListType = ((AveDiscoverList)parent.DiscoverSPObject).Type;
                            }
                            break;
                        }
                    case NodeLevel.Folder:
                        {
                            IAveList list = (IAveList)mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List);
                            string tempFolderPath = "/" + node.FullPath.TrimStart('/').TrimEnd('/');
                            string parentFolderPath = AveUrlUtility.GetParentUrl(tempFolderPath);
                            mLog.Info("InitNodeEntityRelatedInfo parentFolderPath:{0}.", parentFolderPath);
                            parent.ID = list.GetFolder(parentFolderPath).UniqueId;
                            mLog.Info("InitNodeEntityRelatedInfo parentFolderID:{0}.", parent.ID);
                            parent.FullPath = parentFolderPath;
                            if (parent.ID.Equals(list.RootFolder.UniqueId))
                            {
                                parent.SPNodeLevel = NodeLevel.RootFolder;
                                parent.ListId = list.ID;
                            }
                            else
                            {
                                parent.SPNodeLevel = NodeLevel.Folder;
                            }
                            parent = await InitNodeEntityRelatedInfoAsync(discoverWork, parent, autoApproval);
                            if (node.DiscoverSPObject == null)
                            {
                                Guid tempNodeId = node.ID;
                                AveDiscoverFolder discoverFolde = ((AveDiscoverFolder)parent.DiscoverSPObject).GetSubFolders().FirstOrDefault<AveDiscoverFolder>(tmp => tmp.DocID.Equals(tempNodeId));
                                if (discoverFolde == null)
                                {
                                    throw new SPObjectNotFoundException("RM_JM_GlobalSearch_CannotFindExchangeItem");
                                }
                                if (!firstCall)
                                {
                                    node = parent.GenerateFolderNodeItem(discoverFolde, NodeLevel.Folder, mDiscoverSite.Site.Url, mConfiguration);
                                }
                                else
                                {
                                    node.ListType = parent.ListType;
                                    node.IsRecord = parent.IsRecord;
                                    //ADO-165559 folder node export rule,LibRowID需要手动赋值
                                    node.LibRowID = discoverFolde.ID == null ? -1 : discoverFolde.ID.Value;
                                    //node.IsMicroFeedList = parent.IsMicroFeedList;
                                    node.Modified = discoverFolde.TimeLastModified.Ticks;
                                }
                                node.DiscoverSPObject = discoverFolde;
                            }
                            break;
                        }
                    default:
                        break;
                }
                node.Parent = parent;
                if (node.SPNodeLevel != NodeLevel.SiteCollection)//防止冲掉webapp
                {
                    //discoverWork.ProcessContainerLevelNodeWithRule(parent);
                    //递归的时候不checkRule
                    ProcessResult result = await discoverWork.ProcessContainerAsync(parent, ProcessType.NoNeedProcess);
                }
                return node;
            }
        }

        private AveBPOSAccountInfo GetBposInfoBySite(string siteUrl)
        {
            lock (locker)
            {
                if (_bposCache.ContainsKey(siteUrl))
                {
                    return _bposCache[siteUrl];
                }
                else
                {
                    GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                    AveBPOSAccountInfo aveBPOSAccountInfo = PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection).Result;
                    _bposCache.Add(siteUrl, aveBPOSAccountInfo);
                    return aveBPOSAccountInfo;
                }
            }
        }

        public int CaculateListCount(ArchiverNodeItem node)
        {
            int result = 0;
            switch (node.SPNodeLevel)
            {
                case NodeLevel.SiteCollection:
                    result = CaculateSiteListCount(node);
                    break;
                case NodeLevel.Site:
                    result = CaculateWebListCount(node, true, null);
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    result++;
                    break;
                case NodeLevel.RootFolder:
                case NodeLevel.FSFolder:
                case NodeLevel.Folder:
                case NodeLevel.Item:
                    result++;
                    break;
                default:
                    throw new Exception("Unknown Level");
            }
            mLog.Info($"CaculateSiteCollectionItemCount.TotalItemCount:{totalScanCount}.");
            return result;

        }

        private int CaculateSiteListCount(ArchiverNodeItem site)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.CaculateSiteListCount"))
            {
                int result = 0;
                try
                {
                    //初始化Site对应的一些信息。
                    AveObjectModelFactory factory = mConfiguration.aveObjectModelFactory;
                    IAveSite aveSite = factory.CreateSite(site.SiteUrl);
                    if (aveSite == null)
                    {
                        throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanCaculateSiteListCount, "SiteCollection", site.FullPath);
                    }
                    AveDiscoverSite discoverSite = new AveDiscoverSite(factory.CreateSite(), null, AveDiscoveryKind.API, DiscoverModule.Archive);

                    #region//去掉不用的方法
                    //InitialSPObjectInfo(null, ref site);
                    ////If the rootWeb has defined a unique rule, we should skip all the site collection.
                    ////URL of RootWeb is same as sitecollection's
                    //IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                    #endregion

                    using (discoverSite)
                    {
                        using (var web = discoverSite.GetRootWeb())
                        {
                            using (ArchiverNodeItem webnode = site.GenerateSiteNodeItem(web, mConfiguration, true))
                            {
                                result = CaculateWebListCount(webnode, false, web);
                            }
                        }
                    }
                    if (aveSite != null)
                    {
                        aveSite.Dispose();
                        aveSite = null;
                    }
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARSOArchiverCalculateCountSuccess, result);
                }
                catch (SPObjectNotFoundException snfe)
                {
                    mLog.Info("Site Collection Not Found " + snfe.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanCaculateSiteListCount, e);
                }
                return result;
            }
        }

        /// <summary>
        /// Process web content in iteration for initialization
        /// </summary>
        /// <param name="web"></param>
        private int CaculateWebListCount(ArchiverNodeItem web, bool needInitInfo, AveDiscoverWeb discoverWeb)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.CaculateWebListCount"))
            {
                int result = 0;
                try
                {
                    if (needInitInfo)
                    {
                        mConfiguration.mInitialTime = DateTime.Now;
                    }

                    if (discoverWeb != null)
                    {
                        var discoverLists = discoverWeb.GetLists();
                        OutPutListItemCount(discoverLists);
                        var subWebs = discoverWeb.GetSubWebs();
                        result = discoverLists.Count;
                        foreach (var subWeb in subWebs.Values)
                        {
                            try
                            {
                                if (IsWebBreakInherit(web, subWeb))
                                {
                                    continue;
                                }
                                discoverLists = subWeb.GetLists();
                                result += discoverLists.Count;
                                //处理SubSite下面的Subsite
                                CaculateWebListCount(web, needInitInfo, subWeb);
                            }
                            catch (Exception ex)
                            {
                                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARSOArchiverGetSubSiteListCountError, ex.Message);
                            }
                            finally
                            {
                                web.Dispose();
                                subWeb.Dispose();
                            }
                        }
                    }
                    else
                    {
                        AveObjectModelFactory factory = null;
                        factory = mConfiguration.aveObjectModelFactory;
                        using (IAveSite site = factory.CreateSite())
                        {
                            //TODO: Need to take Explicit inclusion into consideration
                            string webUrl = AveUrlUtility.GetServerRelativeUrl(web.FullPath);
                            //string webUrl = web.FullPath.Substring(site.WebApplication.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString().Length);
                            discoverWeb = new AveDiscoverWeb(site, webUrl, DiscoverModule.Archive, factory);
                            var discoverLists = discoverWeb.GetLists();
                            OutPutListItemCount(discoverLists);
                            result = discoverLists.Count;
                            var subWebs = discoverWeb.GetSubWebs();
                            foreach (var subWeb in subWebs.Values)
                            {
                                try
                                {
                                    if (IsWebBreakInherit(web, subWeb))
                                    {
                                        continue;
                                    }
                                    discoverLists = subWeb.GetLists();
                                    result += discoverLists.Count;
                                    CaculateWebListCount(web, needInitInfo, subWeb);
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARSOArchiverGetSubWebListCountError, ex.Message);
                                }
                                finally
                                {
                                    subWeb.Dispose();
                                }
                            }
                            if (discoverWeb != null)
                            {
                                web.Dispose();
                                discoverWeb.Dispose();
                            }
                        }
                    }
                }
                catch (SPObjectNotFoundException snfe)
                {
                    mLog.Info("Site Collection Not Found " + snfe.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanCaculateWebListCount, e);
                }
                finally
                {
                }
                return result;
            }
        }

        protected bool IsWebBreakInherit(ArchiverNodeItem web, AveDiscoverWeb subWeb)
        {
            bool skipCheckBreakInherit = false;
            if (web != null && !string.IsNullOrEmpty(web.RuleId) && web.DoDelete)
            {
                skipCheckBreakInherit = true;
            }
            if (!mConfiguration.UseArchiverImportFile && !skipCheckBreakInherit && !string.IsNullOrEmpty(web.SiteUrl) && (discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(web.SiteUrl, subWeb.FullUrl))))
            {
                return true;
            }
            return false;
        }

        private void OutPutListItemCount(Dictionary<Guid, AveDiscoverList> discoverLists)
        {
            foreach (var list in discoverLists)
            {
                try
                {
                    if (list.Value != null)
                    {
                        totalScanCount += list.Value.ItemCount;
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseTotalFiles(list.Value.ItemCount);
                        mLog.Info($"CaculateWebListCount.ListUrl:{list.Value.RootFolderUrl}.ListTotalCount:{list.Value.ItemCount}.");
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn($"An error occurred while calculate list count. {e}.");
                }
            }
        }
        #endregion
        public void Dispose()
        {
            discoverWorker.Dispose();
            SPORootFolder?.Dispose();
        }
    }
    
}