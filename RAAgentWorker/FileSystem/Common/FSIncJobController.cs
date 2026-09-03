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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.FileSystem.Collect
{
    public class FSJobController
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DateTime JobStartTime { get; set; }
        public DateTime IncrementalStartTime { get; set; }
        private Guid ScopeId { get; set; }
        private string Path { get; set; }
        public FSSettingDto Setting { get; set; }
        public FSJobType JobType { get; set; }
        public TermConflictOption TermConflictOption { get; set; }
        public bool NeedToChangeScopeProfile { get; set; }
        public FSJobController()
        {
            JobStartTime = DateTime.UtcNow;
            NeedToChangeScopeProfile = false;
        }
        //internal void InitINCStartTime()
        //{
        //    DateTime startTime = QueryIncJobStartTime();
        //    if (startTime != System.Data.SqlTypes.SqlDateTime.MinValue.Value)
        //    {
        //        List<RMTerm> possibleTermsInTheJob = QueryJobTerms();
        //        logger.Info("There are {0} positive terms in the job.", possibleTermsInTheJob.Count);
        //        List<Guid> changedTerms = QueryChangedTerms(startTime);
        //        if (changedTerms.Any(t => possibleTermsInTheJob.Any(p => p.UniqueId == t)))
        //        {
        //            logger.Info("The criterias of the rule or the term-rule association was changed since last sync job.So this job will also match the rule for the scanned files again.");
        //            JobType = JobType.RematchRuleFullJob;
        //        }
        //        else
        //        {
        //            logger.Info("There is a record for this scope,and there is no term/rule changed. The job will be incremental job.");
        //            JobType = JobType.IncrementalJob;
        //        }
        //    }
        //    else
        //    {
        //        logger.Info("There is no record for this scope[{0}]. The job will be full job.");
        //        JobType = JobType.UserFullJob;
        //    }
        //    IncrementalStartTime = startTime;
        //}


        //private List<Guid> QueryChangedTerms(DateTime startTime)
        //{
        //    var changes = mAPIUtility.GetChangedTermIds(startTime.Ticks);
        //    logger.Info("There were {0} terms changed since last job{1}.", changes.Count, startTime);
        //    return changes;
        //}

        //private List<RMTerm> QueryJobTerms()
        //{
        //    List<RMTerm> terms = new List<RMTerm>();
        //    if (Setting.TermId == Guid.Empty)
        //    {
        //        //get all terms under the termset
        //        //ITermDao dao = new TermDao();
        //        //ITermSetDao termsetDao = new TermSetDao();
        //        var termSet = mAPIUtility.GetRMTermSetByGuid(Setting.TermSetId);
        //        if (termSet != null)
        //        {
        //            //terms = dao.GetTermFromTermSet(termSet.Id);
        //            terms = mAPIUtility.GetAllTermsUnderTermSet(termSet.Id);
        //        }
        //    }
        //    else
        //    {
        //        //get all terms under the term
        //        //ITermDao dao = new TermDao();
        //        var term = mAPIUtility.GetRMTermByGuId(Setting.TermId);
        //        if (term != null)
        //        {
        //            terms = mAPIUtility.GetAllSubLocationTerm(term.Id);
        //        }
        //    }
        //    return terms;
        //}

        //private DateTime QueryIncJobStartTime()
        //{
        //    var time = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
        //    try
        //    {
        //        //IFileSystemJobTimeReferenceDao dao = new FileSystemJobTimeReferenceDao();
        //        //RMFileSystemJobTimeReference jobEntry = dao.GetJobEntry(ScopeId);               
        //        DateTime lastJobTime = mAPIUtility.GetLastJobTime(ScopeId);
        //        if (lastJobTime != DateTime.MinValue)
        //        {
        //            time = lastJobTime;
        //        }
        //        else
        //        {
        //            logger.Info("There is no job entry in the database. So this job will process full scan.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Failed to get the job start time from Explorer database. Exception:{0}", ex.ToString());
        //    }
        //    logger.Info("Incremental start time from Job Reference table:{0}", time);
        //    return time;
        //}

        internal void InitJob(FSSettingDto setting, Guid scopeId, string fullPath, FSJobMessage message,string dirName)
        {
            Path = fullPath;
            ScopeId = scopeId;
            Setting = setting;
            JobType = message.FSJobType;
            IncrementalStartTime = message.IBStartTime;
            NeedToChangeScopeProfile = message.NeedChangeProfile;
            TermConflictOption = message.TermConflictOption;
            logger.Info(@"
Path:{0} 
ScopeId:{1} 
JobType:{2} 
IncrementalStartTime:{3} 
NeedToChangeScopeProfile:{4} 
TermConflictOption:{5}
            ", dirName.LogBase64(), ScopeId, JobType.ToString(), IncrementalStartTime.ToString(), NeedToChangeScopeProfile, TermConflictOption);
            //if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
            //{
            //    TermConflictOption = setting.AutoJobOption == (int)AutoJobOption.Override ? TermConflictOption.Overwrite : TermConflictOption.Skip;
            //    if (setting.RunAutoFullJob)
            //    {
            //        logger.Info("The job is user started full job.");
            //        JobType = JobType.UserFullJob;
            //        NeedToChangeScopeProfile = true;
            //        IncrementalStartTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            //    }
            //    else
            //    {
            //        JobType = JobType.IncrementalJob;
            //        InitINCStartTime();
            //    }
            //}
            //else
            //{
            //    TermConflictOption = TermConflictOption.Skip;
            //    if (setting.NeedCheckDefaultValue)
            //    {
            //        logger.Info("The job is user started full job.");
            //        JobType = JobType.UserFullJob;
            //        IncrementalStartTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            //        if (setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
            //        {
            //            TermConflictOption = TermConflictOption.Overwrite;
            //        }
            //        NeedToChangeScopeProfile = true;
            //    }
            //    else
            //    {
            //        JobType = JobType.IncrementalJob;
            //        InitINCStartTime();
            //    }
            //}
        }

        internal void UpdateScopeSettingProfile()
        {
            try
            {
                if (NeedToChangeScopeProfile)
                {
                    var updated = JobContext.Current.ApiClient.ResetApplyExistingOption(Setting.ScopeId);
                    logger.Info($"Update scope setting finished. Scope Id:{Setting.ScopeId} Status: {updated}");
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while ResetApplyExistingOption. Error:" + e.ToString());
            }
        }

        //TODO when to save? need to check the job status? only job successfull ....
        public void StoreJobTime()
        {
            try
            {
                var entry = new RMFileSystemJobTimeReferenceDto() { LastJobTime = JobStartTime, Path = Path, ScopeId = ScopeId };
                var updated = JobContext.Current.ApiClient.UpdateJobTime(entry);
                logger.Info($"Update job time finished. Scope Id: {ScopeId} Status: {updated}");
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while StoreJobTime. Error:" + e.ToString());
            }
        }
    }
}
