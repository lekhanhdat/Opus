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
using Aos.Sdk.Models.Tenant;
using AvePoint.Common.Portal;
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.ReportCenter;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services.AccountManager;
using Cloud.Sdk.CloudInsights;
using Cloud.Sdk.Data.CloudInsights;
using RAReportCenter.ClientAuditReport.Utility;
using RATeams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static RAReportCenter.ClientAuditReport.Utility.ClientAuditException;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    /// <summary>
    /// If you want to debug this
    /// 1. Register SPO in Test environment AOS, and run CloudInsights job in test environment. 
    /// 2. Edit region:CreateCloudInsightsClient, Set tenantId(TenantGroupId) to the value in the test environment.
    /// 3. Edit region:AddTestSiteUrl, Add your sites url, 
    /// </summary>
    public class SharePointOnlineAuditReportScanner : IDisposable
    {

        #region private fields
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SharePointOnlineAuditReportScanner));
        protected ClientAuditReportDto mClientAuditReportDto;
        private CloudInsightsApiClient mCloudInsightsApiClient;
        private StorageSettingModel mStorageSettingModel;
        private TenantAdvanceSetting mAdvanceSettingModel;
        //private Dictionary<string, string> spDisplayNameMapping;
        private string mJobId = string.Empty;
        private string mReportDataFolder = string.Empty;
        private string mJobSummary = string.Empty;

        private ClientAuditReportFilterEngine mClientAuditReportFilterEngine;
        private bool mJobHasException = false;
        private bool mJobHasStopped = false;
        private DateTime mStartTime;
        private DateTime mEndTime;
        protected Dictionary<string, long> siteDetails = new Dictionary<string, long>();
        protected Dictionary<string, Dictionary<string, string>> mUrlDic = null;
        //Maybe we'll add an upper limit, and we can use ReportedCount to control.
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private ISPSettingTreeService mRMSPTreeService { get; set; }
        private ISPSettingTreeService RMSPTreeService
        {
            get
            {
                if (mRMSPTreeService == null)
                {
                    mRMSPTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
                }
                return mRMSPTreeService;
            }
        }

        private IRMRemoteNodeDao mRMRemoteNodeDao;
        private IRMRemoteNodeDao RMRemoteNodeDao
        {
            get
            {
                if (mRMRemoteNodeDao == null)
                {
                    mRMRemoteNodeDao = (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                }
                return mRMRemoteNodeDao;
            }
        }

        private IRMReportManager mReportManger;

        private IRMReportManager ReportManager
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

        private IGeneralSettingDao GeneralSettingDao { get; set; }

        private GeneralSettingModel GeneralSetting
        {
            get
            {
                if (GeneralSettingDao == null)
                {
                    GeneralSettingDao = (IGeneralSettingDao)PlatformWindsorManager.GetService(typeof(IGeneralSettingDao));
                }
                return GeneralSettingDao.GetCurrentGeneralSetting();
            }
        }
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        #endregion

        #region public fields
        #endregion

        #region ctor
        public SharePointOnlineAuditReportScanner(RMProfileDto profileDto, string jobId, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            ClientAuditReportDto auditDto = JsonUtil.JsonDeserialize<ClientAuditReportDto>(profileDto.Extension1);
            auditDto.TimeFilterMode = profileDto.RangeType;

            mClientAuditReportDto = auditDto;
            InitAsync(profileDto, jobType).Wait();
            ReportManager.StartUpdateJobProgress(60);
            #region CreateCloudInsightsClient
#if DEBUG
            //mCloudInsightsApiClient = AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient("https://graph.sharepointguild.com/cloudinsights", "386d5768-80b2-4758-959f-e83f3b2dd113");
            mCloudInsightsApiClient = AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient("https://graph.sharepointguild.com/cloudinsights", TenantLocalValue.LogonGroupId);
#else

            mCloudInsightsApiClient = AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(GCommonRoleConfiguration.PortalCloudInsightsApiURL, TenantLocalValue.LogonGroupId);
#endif
            #endregion
            mJobId = jobId;
            mReportDataFolder = SPAuditReportUtility.GetTempFolder(TenantLocalValue.LogonGroupId, mJobId);
            DateTime startTime = DateTime.MinValue, endTime = DateTime.MinValue;
            if (mClientAuditReportDto.TimeFilterMode == AvePoint.RA.Contract.JobMonitor.TimeRangeType.Custom)
            {
                var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.GeneralSetting.TimeZoneId.Replace("_", " "));
                startTime = ConverDateTimeToUTC(mClientAuditReportDto.StartDateTime, globalTimeZone);
                endTime = ConverDateTimeToUTC(mClientAuditReportDto.EndDateTime, globalTimeZone);
            }
            else
            {
                SPAuditReportUtility.GetRangeDate(ref startTime, ref endTime, mClientAuditReportDto.TimeFilterMode);
            }
            mStartTime = startTime;
            mEndTime = endTime;

            mClientAuditReportFilterEngine = new ClientAuditReportFilterEngine(mClientAuditReportDto, startTime, endTime);
        }
        #endregion

        #region public methods
        public void Scan()
        {
            try
            {
                InitCloudInsightsSettings();
                bool spAuditPacketStorage = mAdvanceSettingModel != null ? mAdvanceSettingModel.SaveSPAudit : false;
                Logger.Info($"spAudit packet storage value is {spAuditPacketStorage}");
                CloudInsightsDataReader mDataReader = new CloudInsightsDataReader(mStorageSettingModel, mJobId, mStartTime, mEndTime);
                var siteList = from node in siteDetails select node.Key;
                mDataReader.SetAuditPacketStore(spAuditPacketStorage, siteList.ToList());
                mDataReader.SetEvent(SetCalculatedCount, IncreaseProgress);
                mDataReader.Build();
                //spDisplayNameMapping = mDataReader.UserMappings;
                DateTime mMinTime = DateTime.MinValue;
                foreach (var data in mDataReader)
                {
                    using (new CheckJobStopScope())
                    {
                        var reports = ReadAndFilterData(data, ref mMinTime);
                    }
                }
            }
            catch (AvePoint.RA.Contract.Exceptions.JobStopException ex)
            {
                mJobHasStopped = true;
                Logger.Warn(ex.ToString());
            }
            catch (CloudInsightsCollectionNotEnableException cse)
            {
                mJobHasException = true;
                Logger.Error(I18NEntity.GetString(cse.Message));
                mJobSummary = cse.Message;
            }
            catch (Exception e)
            {
                mJobHasException = true;
                Logger.Error(e.ToString());
                throw;
            }
            finally
            {
                var finalStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
                if (mJobHasException)
                {
                    finalStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
                }
                if (mJobHasStopped)
                {
                    finalStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped;
                }
                SetJobDetails();
                ReportManager.SetJobFinished(finalStatus, mJobSummary);
            }

        }

        #endregion

        #region private methods

        protected virtual async Task InitAsync(RMProfileDto profileDto, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            List<string> selectedNodes = new List<string>();
            if (mClientAuditReportDto.TreeScope == TreeModeSettings.AllSites)
            {
                List<int> sourceTypeList = new List<int>() { (int)SourceFlag.SharePoint,(int)SourceFlag.OneDrive};
                var userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var allContainers = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x => sourceTypeList.Contains(x.Key));
                var containerIds = GetHasPermissionContainerIds(allContainers);
                var remoteNodes = RMRemoteNodeDao.GetAllRemoteSiteCollections();
                bool isSuperAdmin = await IsSuperAdminAsync() && await IsSOSuperAdminAsync();
                Logger.Info($"current user is superadmin?{isSuperAdmin},job type:{jobType.ToString()}");
                if (jobType == AvePoint.RA.Contract.JobMonitor.JobType.SPOActionAuditReport)
                {
                    if (isSuperAdmin)
                    {
                        var spoSites = from node in remoteNodes
                                       where node.NodeType == RemoveNodeType.SiteCollection || node.NodeType == RemoveNodeType.O365GroupSites || node.NodeType == RemoveNodeType.PrivateChannel
                                       select node.url;
                        selectedNodes.AddRange(spoSites);
                    }
                    else
                    {
                        //var spoSites = remoteNodes.Where(n => n.NodeType == RemoveNodeType.SiteCollection || n.NodeType == RemoveNodeType.O365GroupSites|| n.NodeType == RemoveNodeType.PrivateChannel).ToList();
                        var spoSites = from node in remoteNodes
                                       where (node.NodeType == RemoveNodeType.SiteCollection || node.NodeType == RemoveNodeType.O365GroupSites || node.NodeType == RemoveNodeType.PrivateChannel) && containerIds.Contains(new Guid(node.parentId))
                                       select node.url;
                        selectedNodes.AddRange(spoSites);
                    }
                    
                }
                else if (jobType == AvePoint.RA.Contract.JobMonitor.JobType.OneDriveActionAuditReport)
                {
                    if (isSuperAdmin)
                    {
                        var oneDriveSites = from node in remoteNodes where node.NodeType == RemoveNodeType.SkyDrivePro select node.url;
                        selectedNodes.AddRange(oneDriveSites);
                    }
                    else
                    {
                        var oneDriveSites = from node in remoteNodes where (node.NodeType == RemoveNodeType.SkyDrivePro) && containerIds.Contains(new Guid(node.parentId)) select node.url;
                        selectedNodes.AddRange(oneDriveSites);
                    }
                }
            }
            else
            {
                //process tree nodes
                var mTreeNodes = await AssembleRunableSitesAsync(profileDto);
                var sites = from node in mTreeNodes where node.Level == (int)NodeLevel.SiteCollection select node.FullPath;
                selectedNodes.AddRange(sites);
            }
            #region AddTestSiteUrl
#if DEBUG
            selectedNodes.Add("https://m365x89811045.sharepoint.com/sites/test1");
            selectedNodes.Add("https://m365x89811045.sharepoint.com");
#endif
            #endregion

            mUrlDic = SPAuditReportUtility.GetUrlDic(selectedNodes);
            foreach (var s in selectedNodes)
            {
                if (!siteDetails.ContainsKey(s))
                {
                    siteDetails.Add(s, 0);
                }
            }
        }
        private Task<bool> IsSuperAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsAdmin);
        }

        private Task<bool> IsSOSuperAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsAdmin);
        }
        protected List<Guid> GetHasPermissionContainerIds(IEnumerable<KeyValuePair<int, List<Guid>>> allContainers)
        {
            List<Guid> containerIds = new List<Guid>();
            foreach (KeyValuePair<int, List<Guid>> item in allContainers)
            {
                item.Value.ForEach(o =>
                {
                    if (!containerIds.Contains(o))
                    {
                        containerIds.Add(o);
                    }
                });
            }
            return containerIds;
        }
        private void InitCloudInsightsSettings()
        {
            mStorageSettingModel = PortalUtil.Execute(() => mCloudInsightsApiClient.StorageService.GetStorageSetting());
            mAdvanceSettingModel = PortalUtil.Execute(() => mCloudInsightsApiClient.SettingsService.GetAdvanceSetting());
            var settings = PortalUtil.Execute(() => mCloudInsightsApiClient.SettingsService.GetCollectionSetting());
            var result = settings != null && settings.SharePointActivityEnabled && settings.ActivityDataEnabled;
            Logger.Info("Get management activity api settings result is " + result);
            if (!result)
            {
                throw new CloudInsightsCollectionNotEnableException();
            }
        }

        private List<AuditDataInfo> ReadAndFilterData(AuditDownloadDataInfo auditDownloadDataInfo, ref DateTime minTime)
        {
            List<AuditDataInfo> infos = new List<AuditDataInfo>();
            var spData = new List<AuditDataInfo>();
            var itemCount = 0;
            var files = Directory.GetFiles(auditDownloadDataInfo.FileFolder);
            Logger.Info($"get files from auditDownloadDataInfo fileFolder,count:{files?.Length},fileFolder:{auditDownloadDataInfo.FileFolder}");
            var watchProcessedData = new Stopwatch();
            watchProcessedData.Start();
            ArgumentCheck.CheckNotNull(files);
            foreach (var spFile in files)
            {
                try
                {
                    using (var reader = new TsvReader(spFile))
                    {
                        while (reader.Read())
                        {
                            var siteUrl = string.Empty;
                            var aveSiteId = reader.GetString(8);
                            var aveWebId = reader.GetString(23);
                            if (string.IsNullOrEmpty(aveSiteId))
                            {
                                siteUrl = UrlUtility.GetSiteUrl(reader.GetString(5));
                                aveSiteId = SPAuditReportUtility.GetAveId(siteUrl);
                            }
                            else if (mUrlDic["SiteCollection"].ContainsKey(aveSiteId))
                            {
                                siteUrl = mUrlDic["SiteCollection"][aveSiteId];
                            }
                            if (string.IsNullOrEmpty(aveSiteId) || string.IsNullOrEmpty(siteUrl))
                            {
                                continue;
                            }
                            var currentTime = reader.GetDateTime(1);
                            if (currentTime.Ticks < minTime.Ticks)
                            {
                                minTime = currentTime;
                            }
                            var auditDataInfo = new ClientSPAuditReport();
                            auditDataInfo.SiteUrl = siteUrl;
                            auditDataInfo.ObjectLevel = SPAuditReportUtility.ConvertItemType(reader.GetString(7));
                            //if (spDisplayNameMapping.ContainsKey(reader.GetString(2)))
                            //{
                            //    auditDataInfo.User = spDisplayNameMapping[reader.GetString(2)];
                            //}
                            //else
                            //{
                            //    auditDataInfo.User = reader.GetString(2);
                            //}
                            auditDataInfo.User = GetUserName(reader.GetString(2));
                            auditDataInfo.Url = HttpUtility.UrlDecode(reader.GetString(5));
                            auditDataInfo.Occurred = reader.GetDateTime(1).Ticks;
                            auditDataInfo.Event = SPAuditReportUtility.ConvertEventType(reader.GetString(3));
                            auditDataInfo.EventTypeName = reader.GetString(3);
                            auditDataInfo.EventTypeI18NName = ManagementAPIReportConstants.I18nEvents.ContainsKey(reader.GetString(3)) ? ManagementAPIReportConstants.I18nEvents[reader.GetString(3)] : reader.GetString(3);
                            auditDataInfo.TitleOrName = "";
                            auditDataInfo.DisplayName = "";

                            if (reader.Count >= 27)
                            {
                                auditDataInfo.Browser = reader.GetString(26);
                            }
                            else
                            {
                                auditDataInfo.Browser = "";
                            }
                            if (mClientAuditReportFilterEngine.IsQualified(auditDataInfo))
                            {
                                //Maybe we'll add an upper limit, and we can use ReportedCount to control.
                                //ReportedCount++;
                                siteDetails[auditDataInfo.SiteUrl]++;
                                ReportManager.SendJobReport(auditDataInfo);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn($"Failed to process audit data, error {e}");
                    mJobHasException = true;
                }
                finally
                {
                    int i = 0;
                    while (i < 3)
                    {
                        try
                        {
                            File.Delete(spFile);
                            break;
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"Failed to delete audit file data, error {e}");
                        }
                        Thread.Sleep(1000);
                        i++;
                    }
                }
            }
            return infos;
        }

        private string GetUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return string.Empty;
            }

            if (userName.StartsWith("i:0i.t|00000003-0000-0ff1-ce00-000000000000|"))
            {
                return userName.Substring("i:0i.t|00000003-0000-0ff1-ce00-000000000000|".Length);
            }

            return userName;
        }

        private void SetCalculatedCount(object e, SetCalculatedCountEventArgs args)
        {
            if (args != null)
                ReportManager.IncreaseBase(args.Count);
        }

        private void IncreaseProgress(object e, IncreaseProgressEventArgs args)
        {
            if (args != null && args.HasError)
            {
                mJobHasException = true;
            }
            ReportManager.Increase();
        }

        private void SetJobDetails()
        {
            foreach (var kv in siteDetails)
            {
                JMClientAuditReportJobDetails detail = new JMClientAuditReportJobDetails();
                detail.Status = JobDetailsStatus.Successful;
                detail.ObjectPath = kv.Key;
                detail.Type = "Site Collection";
                detail.Count = kv.Value.ToString();
                mReportManger.SendJobDetail(detail);
            }
        }

        protected virtual async Task<List<RMSPTreeNode>> AssembleRunableSitesAsync(RMProfileDto dto, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline)
        {
            List<RMSPTreeNode> nodeList = new List<RMSPTreeNode>();
            if (!string.IsNullOrEmpty(dto.Extension2))
            {
                var farmNode = this.GetFarmSPTreeNode(dto.Extension2);
                //nodeList = this.AssembleAllTreeNode(farmNode, type);
                nodeList = await this.AssembleAllTreeNodeAsync(farmNode, type);
            }
            return nodeList;
        }

        public RMSPTreeNode GetFarmSPTreeNode(string ext2)
        {
            //var farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(ext2);
            var farmNode = AvePoint.GCommon.Utility.SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(ext2, true);
            return farmNode;
        }

        private async Task<List<RMSPTreeNode>> AssembleAllTreeNodeAsync(RMSPTreeNode farmNode, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline)
        {
            List<RMSPTreeNode> treeNodes = new List<RMSPTreeNode>();
            foreach (var group in farmNode.Children)
            {
                List<RMSPTreeNode> allSiteUnderGroup = await RMSPTreeService.BrowseAsync(group, true, type);
                if (group.CheckNumber == 1)
                {
                    allSiteUnderGroup.ForEach(a => a.CheckNumber = 1);
                    treeNodes.AddRange(allSiteUnderGroup.Select(o => GetCloneSite(o)));
                    Logger.Info("The current Container {0} is fully selected, and all Site Collections, including newly created ones, are browsed out", group.Name);
                }
                else if (group.CheckNumber == 2)
                {
                    if (group.Children != null)
                    {
                        foreach (var site in group.Children)
                        {
                            if (HasSelectNode(site) && SiteExists(site))
                            {
                                var NotSelectSite = allSiteUnderGroup.Where(o => o.Id == site.Id).First();
                                NotSelectSite.Children = site.Children;
                                NotSelectSite.ChildrenIds = site.ChildrenIds;
                                NotSelectSite.ChildrenCount = site.ChildrenCount;
                                Logger.Info("The current Container {0} is in semi-selected state. Special processing node {1} ,Keep the children below it", group.Name, site.Name);
                            }
                            else
                            {
                                allSiteUnderGroup.Remove(allSiteUnderGroup.Where(o => o.Id == site.Id).FirstOrDefault());
                                allSiteUnderGroup.ForEach(a => a.CheckNumber = 1);
                                Logger.Info("The current Container {0} is in semi-selected state. Removed Node is {1}", group.Name, site.Name);
                            }
                        }
                        treeNodes.AddRange(allSiteUnderGroup.Select(o => GetCloneSite(o, group)));
                    }
                }
                else
                {
                    Logger.Info("The current Container {0} is in normal selection state", group.Name);
                    if (group.Children != null)
                    {
                        foreach (var site in group.Children)
                        {
                            if (HasSelectNode(site) && SiteExists(site))
                            {
                                treeNodes.Add(GetCloneSite(site, group));
                            }
                            else
                            {
                                Logger.Debug("No select node in {0}", site.Name);
                            }
                        }
                    }
                }
            }
            return treeNodes;
        }

        private bool SiteExists(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.SiteCollection)
            {
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(node.SPObjectId);
                if (site == null)
                {
                    Logger.Warn("Site not exits, {0},{1}", node.Name, node.Id);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 原site中带了parent以及其children等其他大量无关的sites，所以需要清除，否则一旦在大数据环境下， tree会很大，导致插入数据库非常慢, 参考RECO-6200
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private RMSPTreeNode GetCloneSite(RMSPTreeNode site, RMSPTreeNode container = null)
        {
            var tmpSite = site.Clone();
            if (site.Parent != null)
            {
                tmpSite.Parent = site.Parent == null ? null : site.Parent.Clone();
                tmpSite.Parent.Children = new List<RMSPTreeNode> { tmpSite };
                tmpSite.Parent.ChildrenIds = new List<string> { tmpSite.Id };
                tmpSite.Parent.Parent = null;
                RemoveParentUnderSite(tmpSite);
            }
            else
            {
                tmpSite.Parent = container;
                tmpSite.Parent.Children = new List<RMSPTreeNode> { tmpSite };
                tmpSite.Parent.ChildrenIds = new List<string> { tmpSite.Id };
                tmpSite.Parent.Parent = null;
                RemoveParentUnderSite(tmpSite);
            }
            //在run job时，会用到site节点的parent中的属性，因此这里保留了site的parent，但是去除了parent中除了此site外其他的子节点         
            return tmpSite;
        }

        /// <summary>
        /// 对于site以下的节点，run job时，实际上用不到parent属性，因此给清空，防止出现tree过大的情况
        /// </summary>
        /// <param name="site"></param>
        private void RemoveParentUnderSite(RMSPTreeNode site)
        {
            if (site == null || site.Children == null) return;
            foreach (var child in site.Children)
            {
                child.Parent = null;
                RemoveParentUnderSite(child);
            }
        }


        private bool HasSelectNode(RMSPTreeNode current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                foreach (RMSPTreeNode child in current.Children)
                {
                    if (HasSelectNode(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        private DateTime ConverDateTimeToUTC(string dateTimeStr, TimeZoneInfo generalTimeZone)
        {
            var temp = DateTime.Parse(dateTimeStr);
            //temp = DateTime.SpecifyKind(temp, DateTimeKind.Utc);
            temp = TimeZoneInfo.ConvertTimeToUtc(temp, generalTimeZone);
            return temp;
        }

        #endregion

        #region Dispose methods
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}