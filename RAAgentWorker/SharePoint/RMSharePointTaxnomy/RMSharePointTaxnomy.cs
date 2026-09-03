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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BposInfo = AvePoint.RA.Contract.Global.Object.BposInfo;

namespace AvePoint.RA.SharePoint.RMSharePointTaxnomy
{
    public class RMSharePointTaxnomy
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AveObjectModelFactory CurrentFactory { get; set; }
        public IAveTermStoreCollection TermStoreCollection { get; set; }
        public List<GRMTermSet> AllTermSet { get; set; }
        public GRMTermGroup CurTermGroup { get; set; }
        public List<Guid> SpecifiedTermStoreIds = new List<Guid>(); //当前同步的TermGroup指定关联的TermStoreId集合
        public List<Guid> SyncedTermStoreGuids = new List<Guid>();
        public Dictionary<Guid, List<Guid>> TermGroupIdMappingStoreIds = new Dictionary<Guid, List<Guid>>();
        public string CurrentSiteUrl = string.Empty;
        public bool JobHasError;
        public int FinsihCount;

        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private int mLcid;
        private string mTermStoreName = string.Empty;
        
        public RMSharePointTaxnomy()
        {
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            //Use server object model
            InitObjectModel();
        }

        public void InitTermGroupRelationInfo(GRMTermGroup rmTermGroup)
        {
            this.AllTermSet = rmTermGroup.subTerms;
            this.CurTermGroup = rmTermGroup;
            this.SyncedTermStoreGuids = new List<Guid>();
            this.SpecifiedTermStoreIds = rmTermGroup.UsingMMSSpecified ? TermGroupIdMappingStoreIds[rmTermGroup.UniqueId] : null;
        }
        private void InitObjectModel()
        {
            try
            {
                CurrentFactory = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.ServerObjectModel);
                logger.Debug($"Success to init server object model.");
                CurrentSiteUrl = CurrentFactory.CreateFarm()?.Local?.Name;
                InitTermStoreCollection();
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to init server object model, {ex}");
                throw;
            }
        }

        private void InitTermStoreCollection()
        {
            try
            {
                int retryTimes = 0;
                var sleepTime = 10000;
                while (retryTimes < 3)
                {
                    logger.Info($"get term store collection, retry times: {retryTimes}");
                    TermStoreCollection = CurrentFactory.CreateTaxonomySession().TermStores;
                    var termStoreCount = TermStoreCollection.Count;
                    logger.Info($"term store collection count: {termStoreCount}");
                    if (termStoreCount > 0)
                    {
                        break;
                    }
                    logger.Info($"sleep: {sleepTime} ms");
                    System.Threading.Thread.Sleep(sleepTime);
                    retryTimes++;
                }
                logger.Debug($"finish to get term store colletion.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get term store colletion, {ex}");
                throw;
            }
        }

        public void SyncTermToSharePoint()
        {
            if (TermStoreCollection.Count == 0)
            {
                AddJobDetail("RM_JS_Common_Pending", "RM_JS_Common_Pending", JobDetailsStatus.Failed, "RM_TS_NoMMS");
                return;
            }
            foreach (IAveTermStore termStore in TermStoreCollection)
            {
                var termStoreId = termStore.ID;
                try
                {
                    if (CheckTermStoreNeedSkip(termStore))
                    {
                        continue;
                    }
                    LoadBasicInfoForTermStore(termStore);
                    SyncTermGroup(termStore);
                }
                finally
                {
                    if (!SyncedTermStoreGuids.Contains(termStoreId))
                    {
                        SyncedTermStoreGuids.Add(termStoreId);
                    }
                }
            }
        }
        
        private void SyncTermGroup(IAveTermStore termStore)
        {
            try
            {
                logger.Info($"Start sync term group to termstore, name:{CurTermGroup.Name.LogBase64()}, id: {CurTermGroup.UniqueId}, termStoreName:{termStore.Name.LogBase64()}, termStoreId:{termStore.ID} ");
                var termGroup = termStore.GetGroup(CurTermGroup.UniqueId);
                if (termGroup != null)
                {
                    try
                    {
                        UpdateTermGroup(termGroup);
                    }
                    catch (Exception e)
                    {
                        ProgressService.Increase();
                        logger.Warn($"Failed to update term group, name:{CurTermGroup.Name.LogBase64()}, Error:{e}");
                        AddJobDetail(CurTermGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, GetExceptionMessage(e));
                        return;
                    }

                    IAveTermSetCollection termSetCollection = termGroup.TermSets;
                    foreach (var rmTermSet in AllTermSet)
                    {
                        ProgressService.IncreaseBase(AllTermSet.Count);
                        SyncTermSet(rmTermSet, termGroup, termSetCollection);
                    }
                    ProgressService.Increase();
                    logger.Info($"Finshed to sync term group to termstore, name:{CurTermGroup.Name.LogBase64()}, id: {CurTermGroup.UniqueId}, termStoreName:{termStore.Name.LogBase64()}, termStoreId:{termStore.ID} ");
                }
                else
                {
                    logger.Info("Need create termgroup {0}", CurTermGroup.Name.LogBase64());
                    CreateTermGroup(termStore, CurTermGroup);
                }
            }
            catch (Exception e)
            {
                //此处报异常说明这个term group在term store中不存在，所以需要新创建
                logger.Info("Need create termgroup {0}, warning message {1}", CurTermGroup.Name.LogBase64(), e.ToString());
                CreateTermGroup(termStore, CurTermGroup);
            }
        }

        private void SyncTermSet(GRMTermSet rmTermSet, IAveTaxonomyGroup defaultGroup, IAveTermSetCollection termSetCollection)
        {
            logger.Info("Begin process termset,{0}", rmTermSet.Name.LogBase64());
            try
            {
                var editTermSet = GetTermSetIfExist(termSetCollection, rmTermSet);
                if (editTermSet != null)
                {
                    if (rmTermSet.IsRemoved)
                    {
                        //if termset is deleted in records, delete the corresponding termset in SP
                        RemoveTermSetInSP(editTermSet, rmTermSet);
                        return;
                    }
                    if (CheckIsChanged(editTermSet.Name, rmTermSet.Name, editTermSet.Description, rmTermSet.Description))
                    {
                        UpdateTermSet(editTermSet, rmTermSet);
                    }
                    else
                    {
                        AddJobDetail(rmTermSet.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_NoChangeTermSet");
                    }
                    if (rmTermSet.RMTerms != null && rmTermSet.RMTerms.Count != 0)
                    {
                        SyncTerms(editTermSet.Terms, rmTermSet.RMTerms, editTermSet, null);
                    }
                }
                else
                {
                    if (rmTermSet.IsRemoved)
                    {
                        logger.Info($"Termset:{rmTermSet.Name.LogBase64()} was deleted in records, no need to create new termset in SharePoint.");
                    }
                    else
                    {
                        logger.Info("SharePoint do not contains termset, need create {0}", rmTermSet.Name.LogBase64());
                        CreateTermSet(defaultGroup, rmTermSet);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Some problems encountered in the process of dealing edit rmTermSet {0},detail message {1}", rmTermSet.Name.LogBase64(), e.ToString());
                AddJobDetailForFailedTerm(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, GetExceptionMessage(e), rmTermSet.subTerms);
            }
        }

        private IAveTermSet GetTermSetIfExist(IAveTermSetCollection termSetCollection, GRMTermSet rmTermSet)
        {
            IAveTermSet mEditTermSet;
            try
            {
                mEditTermSet = termSetCollection[rmTermSet.UniqueId];
            }
            catch (Exception e)
            {
                //此处报异常说明这个term set在term group中不存在，所以需要新创建
                logger.Info($"Termset not exists, name:{ rmTermSet.Name.LogBase64()} error:{e}");
                mEditTermSet = null;
            }
            return mEditTermSet;
        }

        private IAveTerm GetTermIfExist(GRMTerm rmTerm, IAveTermCollection editTermCollection)
        {
            IAveTerm mTerm;
            try
            {
                mTerm = editTermCollection[rmTerm.UniqueId];
            }
            catch (Exception e)
            {
                logger.Info($"Term not exists. Term name:{rmTerm.Name.LogBase64()} Error:{e}");
                mTerm = null;
            }
            return mTerm;
        }

        private void RemoveTermSetInSP(IAveTermSet termSet, GRMTermSet rmTermSet)
        {
            logger.Info("Delete TermSet,{0}", rmTermSet.Name.LogBase64());
            try
            {
                termSet.Delete();
                termSet.TermStore.CommitAll();
                AddJobDetail(rmTermSet.Name, "RM_TS_Action_Delete", JobDetailsStatus.Successful, null);
            }
            catch (Exception ex)
            {
                logger.Warn("Delete term error, term name {0},message detail {1}", rmTermSet.Name.LogBase64(), ex.ToString());
                AddJobDetail(rmTermSet.Name, "RM_TS_Action_Delete", JobDetailsStatus.Failed, "RM_TS_TermSetDeny");
            }
        }

        private void UpdateTermSet(IAveTermSet editTermSet, GRMTermSet rmTermSet)
        {
            try
            {
                editTermSet.Name = rmTermSet.Name;
                editTermSet.Description = rmTermSet.Description;
                AddJobDetail(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful, null);
                ProgressService.Increase();
            }
            catch (Exception e)
            {
                logger.Warn("Sync current rm term set has some error, set name {0} , error detail, {1}", rmTermSet.Name.LogBase64(), e.ToString());
                AddJobDetail(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSetDeny");
                ProgressService.Increase();
            }
        }

        private void UpdateTermGroup(IAveTaxonomyGroup termGroup)
        {
            if (CheckIsChanged(termGroup.Name, CurTermGroup.Name, termGroup.Description, CurTermGroup.Description))
            {
                try
                {
                    logger.Info("current term group name or description is changed.");
                    termGroup.Name = CurTermGroup.Name;
                    termGroup.Description = CurTermGroup.Description;
                    AddJobDetail(CurTermGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful, null);
                }
                catch (Exception e)
                {
                    AddJobDetail(CurTermGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_RepeatOrDeny");
                    throw;
                }
            }
            else
            {
                AddJobDetail(CurTermGroup.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_NoChangeTermGroup");
            }
        }

        private void LoadBasicInfoForTermStore(IAveTermStore termStore)
        {
            mTermStoreName = termStore.Name;
            mLcid = termStore.DefaultLanguage;
            logger.Info("Current TermStore, name:{0}, lcid:{1}", mTermStoreName.LogBase64(), mLcid);
        }

        private bool CheckTermStoreNeedSkip(IAveTermStore termStore)
        {
            bool needSkip = false;
            if (SpecifiedTermStoreIds != null && SpecifiedTermStoreIds.Count > 0 && !SpecifiedTermStoreIds.Contains(termStore.ID))
            {
                //如果TermGroup指定了关联TermStore, 其它TermStore不需要同步
                needSkip = true;
            }
            if (SyncedTermStoreGuids.Contains(termStore.ID))
            {
                //AddJobDetail(mTermStoreName, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_SkipToSyncMMS");
                logger.Info("This termStore has been synchronized. Termstore name,{0}", mTermStoreName.LogBase64());
                needSkip = true;
            }
            return needSkip;
        }

        private string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            {
                System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                if (te.InnerException != null)
                {
                    comment = te.InnerException.Message;
                }
            }
            return comment;
        }

        public void CreateTermGroup(IAveTermStore termStore, GRMTermGroup rmTermGroup)
        {
            try
            {
                IAveTaxonomyGroup newTermGroup = termStore.CreateGroup(rmTermGroup.Name, rmTermGroup.UniqueId);
                newTermGroup.Description = rmTermGroup.Description;
                termStore.CommitAll();
                logger.Info($"Success to create term group, name:{rmTermGroup.Name.LogBase64()}");
                AddJobDetail(rmTermGroup.Name, "RM_TS_Action_New", JobDetailsStatus.Successful, "");
                //创建TermGroup下的TermSet
                if (AllTermSet != null && AllTermSet.Count > 0)
                {
                    foreach (var rmTermSet in AllTermSet)
                    {
                        if (!rmTermSet.IsRemoved)
                        {
                            CreateTermSet(newTermGroup, rmTermSet);
                        }
                        else
                        {
                            logger.Info($"Termset:{rmTermSet.Name.LogBase64()} was deleted in records, no need to create termset in SharePoint.");
                        }
                    }
                }
                ProgressService.Increase();
            }
            catch (Exception e)
            {
                logger.Error($"Failed to create term group, name:{rmTermGroup.Name}, Error: {e}");
                //在SharePoint中已有同名且id不同的term group，或者当前用户没有权限操作这个metadata management service
                AddJobDetailForFailedTermSet(rmTermGroup.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, "RM_TS_RepeatOrDeny", AllTermSet);
            }
        }
        public void CreateTermSet(IAveTaxonomyGroup defaultGroup, GRMTermSet rmTermSet)
        {
            IAveTermSet newTermSet = null;
            try
            {
                logger.Info("Begin create TermSet name, {0} ; lcid, {1} ", rmTermSet.Name.LogBase64(), mLcid);
                newTermSet = defaultGroup.TermStore.GetTermSet(rmTermSet.UniqueId);
                if (newTermSet == null)
                {
                    newTermSet = defaultGroup.CreateTermSet(rmTermSet.Name, rmTermSet.UniqueId);
                    newTermSet.Description = rmTermSet.Description;
                    defaultGroup.TermStore.CommitAll();
                    logger.Info($"Success to create TermSet, name:{rmTermSet.Name.LogBase64()}");
                    AddJobDetail(rmTermSet.Name, "RM_TS_Action_New", JobDetailsStatus.Successful, null);
                    ProgressService.Increase();
                }
            }
            catch (Exception e)
            {
                //在SharePoint中已有同名且id不同的term set
                logger.Error($"Failed to create TermSet, name:{rmTermSet.Name.LogBase64()}, Error:{e}");
                AddJobDetailForFailedTerm(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSetRepeatOrDeny", rmTermSet.subTerms);
            }
            //创建TermSet下的Term
            if (rmTermSet.RMTerms != null && rmTermSet.RMTerms.Count != 0)
            {
                ProgressService.IncreaseBase(rmTermSet.RMTerms.Count);
                foreach (var rmTerm in rmTermSet.RMTerms)
                {
                    try
                    {
                        if (CheckTermNeedSkip(rmTerm))
                        {
                            continue;
                        }
                        CreateTermUnderTermSet(newTermSet, rmTerm);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Failed to create term, name:{rmTerm.Name.LogBase64()}, Error:{e}");
                        AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, GetExceptionMessage(e), rmTerm.subTerms);
                    }
                }
            }
        }

        private bool CheckTermNeedSkip(GRMTerm subRMTerm)
        {
            bool needSkip = false;
            //过滤掉已经在term management删除的term
            if (subRMTerm.IsRemoved)
            {
                logger.Info($"Term is skipped, because it has been removed in records. {subRMTerm.Name.LogBase64()}");
                needSkip = true;
            }
            //判断这个term是否在生效时间内，如果不在生效时间内，则不会创建这个term
            else if (!IsInTime(subRMTerm.TermExpirationFrom, subRMTerm.TermExpirationTo, subRMTerm.TimeZoneId))
            {
                logger.Info($"Term is skipped, because it is not within the valid time span. {subRMTerm.Name.LogBase64()}");
                AddJobDetail(subRMTerm.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_TermOutTime");
                needSkip = true;
            }
            return needSkip;
        }

        private void CreateTermUnderTermSet(IAveTermSet newTermSet, GRMTerm rmTerm)
        {
            IAveTerm subTerm = null;
            try
            {
                subTerm = newTermSet.CreateTerm(rmTerm.Name, mLcid, rmTerm.UniqueId);
                subTerm.Deprecate(rmTerm.IsDeprecated);
                subTerm.SetDescription(rmTerm.Description, mLcid);
                newTermSet.TermStore.CommitAll();
                AddJobDetail(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Successful, null);
                ProgressService.Increase();
            }
            catch (Exception e)
            {
                //在SharePoint中已有同名且id不同的term
                logger.Error($"Failed to create term, name: {rmTerm.Name.LogBase64()}, Error: {e}");
                AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, "RM_TS_TermRepeatOrDeny", rmTerm.subTerms);
                ProgressService.Increase();
                return;
            }
            if (rmTerm.subTerms != null && rmTerm.subTerms.Count != 0)
            {
                foreach (var subRMTerm in rmTerm.subTerms)
                {
                    try
                    {
                        if (CheckTermNeedSkip(subRMTerm))
                        {
                            continue;
                        }
                        CreateTermUnderTerm(subTerm, subRMTerm, true);
                    }
                    finally
                    {
                        ProgressService.Increase();
                    }
                }
            }
        }

        private void CreateTermUnderTerm(IAveTerm newTerm, GRMTerm rmTerm, bool isCreateChildTerm = false)
        {
            IAveTerm subTerm = null;
            try
            {
                subTerm = newTerm.CreateTerm(rmTerm.Name, mLcid, rmTerm.UniqueId);
                subTerm.Deprecate(rmTerm.IsDeprecated);
                subTerm.SetDescription(rmTerm.Description, mLcid);
                subTerm.TermStore.CommitAll();
                AddJobDetail(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Successful, null);
            }
            catch (Exception e)
            {
                logger.Warn("Some problems encountered in the process of dealing create term {0},detail message {1}", rmTerm.Name.LogBase64(), e.ToString());
                //isCreateChildTerm这个term不是termset下的第一级term
                AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, isCreateChildTerm ? "RM_TS_TermRepeatOrDeny" : "RM_TS_TermRepeat", rmTerm.subTerms);
                return;
            }
            if (rmTerm.subTerms != null && rmTerm.subTerms.Count != 0)
            {
                ProgressService.IncreaseBase(rmTerm.subTerms.Count);
                foreach (var subRMTerm in rmTerm.subTerms)
                {
                    try
                    {
                        if (CheckTermNeedSkip(subRMTerm))
                        {
                            continue;
                        }
                        CreateTermUnderTerm(subTerm, subRMTerm, true);
                    }
                    finally
                    {
                        ProgressService.Increase();
                    }
                }
            }
        }

        public void SyncTerms(IAveTermCollection editTermCollection, List<GRMTerm> rmTermList, IAveTermSet termSet, IAveTerm ParentTerm)
        {
            foreach (var rmTerm in rmTermList)
            {
                logger.Info("Process RMTerm,{0}", rmTerm.Name.LogBase64());
                try
                {
                    var term = GetTermIfExist(rmTerm, editTermCollection);
                    if (term != null)
                    {
                        SyncTerm(term, rmTerm);
                    }
                    else
                    {
                        CreateTerm(term, rmTerm, termSet, ParentTerm);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Some problems encountered in the process of dealing with this term,{0}, detail message {1}", rmTerm.Name.LogBase64(), e.ToString());
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, GetExceptionMessage(e), rmTerm.subTerms);
                }
                finally
                {
                    ProgressService.Increase();
                }
            }
        }

        private void SyncTerm(IAveTerm term, GRMTerm rmTerm)
        {
            if (rmTerm.IsRemoved)
            {
                //如果这个term在SharePoint中存在，并且在term management中已经被删除了，需要在SharePoint上也删除
                RemoveTermInSP(term, rmTerm);
                return;
            }
            logger.Info("Edit Term,{0}", rmTerm.Name.LogBase64());
            bool originalDeprecatedStatusInSP = term.IsDeprecated;
            bool currentDeprecatedStatusInSP = false;
            try
            {
                //修改这个term的Deprecated属性，即是否禁用，如果这个term已不在生效时间内，则禁用
                currentDeprecatedStatusInSP = DeprecateTermInSP(term, rmTerm);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while deprecating term, term name:{rmTerm.Name.LogBase64()} Error:{e}");
                return;
            }

            try
            {
                //check if need to update term
                UpdateTerm(term, rmTerm, originalDeprecatedStatusInSP != currentDeprecatedStatusInSP);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while updating term. Term name:{rmTerm.Name.LogBase64()} Error:{e}");
                return;
            }
            if (rmTerm.subTerms != null && rmTerm.subTerms.Count != 0)
            {
                SyncTerms(term.Terms, rmTerm.subTerms, null, term);
            }
        }

        private void RemoveTermInSP(IAveTerm term, GRMTerm rmTerm)
        {
            logger.Info("Delete Term,{0}", rmTerm.Name.LogBase64());
            try
            {
                term.Delete();
                term.TermStore.CommitAll();
                AddJobDetail(rmTerm.Name, "RM_TS_Action_Delete", JobDetailsStatus.Successful, null);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to delete term, name:{rmTerm.Name.LogBase64()}, error: {ex}");
                AddJobDetail(rmTerm.Name, "RM_TS_Action_Delete", JobDetailsStatus.Failed, "RM_TS_TermSyncDeny");
            }
        }
        private void CreateTerm(IAveTerm term, GRMTerm rmTerm, IAveTermSet termSet, IAveTerm ParentTerm)
        {
            if (CheckTermNeedSkip(rmTerm))
            {
                return;
            }

            if (termSet == null)
            {
                logger.Info($"Create term:{rmTerm.Name.LogBase64()}.");
                CreateTermUnderTerm(ParentTerm, rmTerm);
            }
            else
            {
                logger.Info($"Create term:{rmTerm.Name.LogBase64()} under termset.");
                CreateTermUnderTermSet(termSet, rmTerm);
            }
        }
        private void UpdateTerm(IAveTerm term, GRMTerm rmTerm, bool needChange)
        {
            var termDes = term.GetDescription(mLcid);
            var termName = term.Name;
            //判断这个term和之前同步的相比是否有变化
            if (CheckIsChanged(termName, rmTerm.Name, termDes, rmTerm.Description) || needChange)
            {
                try
                {
                    term.Name = rmTerm.Name;
                    term.SetDescription(rmTerm.Description, mLcid);
                    term.TermStore.CommitAll();
                    logger.Info($"Success to update term, name:{rmTerm.Name.LogBase64()}");
                    AddJobDetail(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful, null);
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to update term, name:{rmTerm.Name.LogBase64()}, Error: {e}");
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSyncDeny", rmTerm.subTerms);
                }
            }
            else
            {
                AddJobDetail(rmTerm.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_NoChangeTerm");
            }
        }
        private bool DeprecateTermInSP(IAveTerm term, GRMTerm rmTerm)
        {
            bool dbTermDeprecate = false;
            if (!IsInTime(rmTerm.TermExpirationFrom, rmTerm.TermExpirationTo, rmTerm.TimeZoneId))
            {
                try
                {
                    if (!term.IsDeprecated)
                    {
                        term.Deprecate(true);
                        term.TermStore.CommitAll();
                    }
                    dbTermDeprecate = true;
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to deprecated term, name: {rmTerm.Name.LogBase64()}, Error: {e}");
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSyncDeny", rmTerm.subTerms);
                    throw;
                }
            }
            else
            {
                try
                {
                    term.Deprecate(rmTerm.IsDeprecated);
                    term.TermStore.CommitAll();
                    dbTermDeprecate = rmTerm.IsDeprecated;
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to deprecated term, name: {rmTerm.Name.LogBase64()}, Error: {e}");
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSyncDeny", rmTerm.subTerms);
                    throw;
                }
            }
            return dbTermDeprecate;
        }

        private bool IsInTime(long TermExpirationFrom, long TermExpirationTo, string timeZoneId)
        {
            if (TermExpirationFrom == 0 && TermExpirationTo == 0)
            {
                return true;
            }
            else if (TermExpirationFrom == 0 && TermExpirationTo > DateTime.UtcNow.Ticks)
            {
                return true;
            }
            else if (TermExpirationFrom <= DateTime.UtcNow.Ticks && TermExpirationTo == 0)
            {
                return true;
            }
            else if (TermExpirationFrom <= DateTime.UtcNow.Ticks && DateTime.UtcNow.Ticks <= TermExpirationTo)
            {
                return true;
            }
            return false;
        }

        private bool CheckIsChanged(string name, string newName, string description, string newDescription)
        {
            if ((string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(newDescription)) || (!string.IsNullOrEmpty(description) && string.IsNullOrEmpty(newDescription)))
            {
                return true;
            }

            var legalName = GetLegalName(newName);
            if (!name.Equals(legalName) || (!string.IsNullOrEmpty(description) && !description.Equals(newDescription)))
            {
                return true;
            }
            return false;
        }

        private string GetLegalName(string newName)
        {
            string legalName = string.Empty;
            //移除多余空格
            if (!string.IsNullOrWhiteSpace(newName))
            {
                newName = newName.Trim();
                string[] strArray = newName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                legalName = string.Join(" ", strArray);
            }

            //半角替换全角
            return ReplaceStr(legalName);
        }

        private string ReplaceStr(string sourceStr)
        {
            string resultStr = "";
            if (!string.IsNullOrEmpty(sourceStr))
            {
                Regex reg = new Regex(@"[;<>|]+");
                sourceStr = reg.Replace(sourceStr.Trim(), "");
                if (!string.IsNullOrEmpty(sourceStr) && (sourceStr.Contains("&") || sourceStr.Contains("\"")))
                {
                    //替换成全角的
                    resultStr = sourceStr.Replace('&', '＆').Replace('"', '＂');
                }
                else
                {
                    resultStr = sourceStr;
                }
            }
            return resultStr;
        }

        private void AddJobDetail(string termName, string action, JobDetailsStatus status, string message)
        {
            if (status == JobDetailsStatus.Failed)
            {
                JobHasError = true;
            }
            else if (status == JobDetailsStatus.Successful)
            {
                FinsihCount++;
            }
            var detail = new JMTermSyncJobDetails
            {
                MMSApplication = mTermStoreName,
                Term = termName,
                Action = action,
                SiteCollectionURL = CurrentSiteUrl,
                AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                Status = status,
                Comment = message
            };
            JobDetailService.Commit(detail);
        }

        /// <summary>
        /// //如果一个父亲级别的term同步失败了，则需要把它下面的所有层级的子term的detail打出来
        /// </summary>
        /// <param name="termName"></param>
        /// <param name="action"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <param name="rmTerms"></param>
        private void AddJobDetailForFailedTerm(string termName, string action, JobDetailsStatus status, string message, List<GRMTerm> rmTerms)
        {
            try
            {
                if (!string.IsNullOrEmpty(termName))
                {
                    if (status == JobDetailsStatus.Failed)
                    {
                        JobHasError = true;
                    }
                    else if (status == JobDetailsStatus.Successful)
                    {
                        FinsihCount++;
                    }
                    JMTermSyncJobDetails detail = new JMTermSyncJobDetails
                    {
                        MMSApplication = mTermStoreName,
                        Term = termName,
                        Action = action,
                        AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                        Status = status,
                        Comment = message
                    };
                    JobDetailService.Commit(detail);
                }

                if (rmTerms != null && rmTerms.Count != 0)
                {
                    foreach (var rmTerm in rmTerms)
                    {
                        if (rmTerm.IsRemoved)
                        {
                            continue;
                        }
                        JMTermSyncJobDetails childDetail = new JMTermSyncJobDetails
                        {
                            MMSApplication = mTermStoreName,
                            Term = rmTerm.Name,
                            Action = "RM_TS_Action_Skip",
                            AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                            Status = JobDetailsStatus.Skipped,
                            Comment = "RM_TS_ParentSyncFail"
                        };
                        JobDetailService.Commit(childDetail);
                        AddJobDetailForFailedTerm(null, null, JobDetailsStatus.None, null, rmTerm.subTerms);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while AddJobDetailForFailedTerm, Error:{0}", e.ToString());
            }
        }
        /// <summary>
        /// //如果一个term set同步失败了，则需要把它下面的所有层级的子term的detail打出来
        /// </summary>
        /// <param name="termName"></param>
        /// <param name="action"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <param name="rmTermSets"></param>
        private void AddJobDetailForFailedTermSet(string termName, string action, JobDetailsStatus status, string message, List<GRMTermSet> rmTermSets)
        {
            try
            {
                if (!string.IsNullOrEmpty(termName))
                {
                    if (status == JobDetailsStatus.Failed)
                    {
                        JobHasError = true;
                    }
                    else if (status == JobDetailsStatus.Successful)
                    {
                        FinsihCount++;
                    }
                    var detail = new JMTermSyncJobDetails
                    {
                        MMSApplication = mTermStoreName,
                        Term = termName,
                        Action = action,
                        AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                        Status = status,
                        Comment = message
                    };
                    JobDetailService.Commit(detail);
                }
                if (rmTermSets != null && rmTermSets.Count != 0)
                {
                    foreach (var rmTermSet in rmTermSets)
                    {
                        JMTermSyncJobDetails detail = new JMTermSyncJobDetails
                        {
                            MMSApplication = mTermStoreName,
                            Term = rmTermSet.Name,
                            Action = "RM_TS_Action_Skip",
                            AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                            Status = JobDetailsStatus.Skipped,
                            Comment = "RM_TS_ParentSyncFail"
                        };
                        JobDetailService.Commit(detail);
                        if (rmTermSet.RMTerms != null && rmTermSet.RMTerms.Count != 0)
                        {
                            AddJobDetailForFailedTerm(null, null, JobDetailsStatus.None, null, rmTermSet.RMTerms);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while AddJobDetailForFailedTermSet, Error{0}", e.ToString());
            }
        }

    }
}
