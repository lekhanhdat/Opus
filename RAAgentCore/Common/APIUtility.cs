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
//using AvePoint.GCommon.Contract.Tree.Object;
//using AvePoint.GCommon.Utility;
//using AvePoint.RA.Contract.Explorer;
//using AvePoint.RA.Contract.FileSystem;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;




//namespace AvePoint.RA.FileSystem.Core
//{
//    public class APIUtility
//    {
//        private HttpHelper helper;
//        private string webApiUrl;
//        private MemoryListCacheService<FileSystemObjectDto> cachedRecords = new MemoryListCacheService<FileSystemObjectDto>();
//        private static APIUtility mAPIUtility;
//        private static object locker = new object();
//        private IReportService reportServcie;
//        private APIUtility(string hostUrl)
//        {
//            reportServcie = JobContext.Current.ReportManager.Create();
//            webApiUrl = hostUrl;
//            helper = new HttpHelper();
//        }

//        public static APIUtility GetInstance(string url)
//        {
//            if (mAPIUtility == null)
//            {
//                lock (locker)
//                {
//                    if (mAPIUtility == null)
//                    {
//                        mAPIUtility = new APIUtility(url);
//                    }
//                }
//            }
//            return mAPIUtility;
//        }

//        public FSJobMessage GetJobMessage(string subJobId)
//        {
//            var url = $"{webApiUrl}/{WebRequestStr.GetJobMessage}?subJobId={subJobId}";
//            var result = helper.HTTPJsonGet(url);
//            return SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(result);
//        }

//        public List<FileSystemObjectDto> QueryExistingRecords(List<Guid> ids)
//        {
//            var url = $"{webApiUrl}/{WebRequestStr.GetRecords}";
//            var data = JsonConvert.SerializeObject(ids);
//            var result = helper.HTTPJsonPost(url, data);
//            return JsonConvert.DeserializeObject<List<FileSystemObjectDto>>(result);
//        }

//        public void AddOrUpdateRecord(FileSystemObjectDto record)
//        {
//            cachedRecords.Add(record);
//            if (cachedRecords.Count > 30)
//            {
//                var tempRecords = cachedRecords.Take(30).ToList();
//                RealAddOrUpdateRecord(tempRecords);
//            }
//        }

//        private void RealAddOrUpdateRecord(List<FileSystemObjectDto> records)
//        {
//            try
//            {
//                var url = $"{webApiUrl}/{WebRequestStr.AddOrUpdateRecord}";
//                var data = JsonConvert.SerializeObject(records);
//                var result = helper.HTTPJsonPost(url, data);
//                List<Guid> failedGuids = JsonConvert.DeserializeObject<List<Guid>>(result);
//                var failedItems = records.Where(r => failedGuids.Contains(r.NodeId)).ToList();
//                failedItems.ForEach(item =>
//                {
//                    reportServcie.Commit(new FSCollectJobReportEntry(item)
//                    {
//                        Status = (int)AvePoint.GCommon.Contract.Server.Job.Object.JobReportDetailStatus.Failed,
//                        Comment = ".Failed to sync records to explorer db."
//                    });
//                });

//                var successItems = records.Except(failedItems).ToList();
//                successItems.ForEach(item =>
//                {
//                    if (item.NodeType != (int)NodeLevel.FSConnectionGroups && item.NodeType != (int)NodeLevel.FSConnectionGroup)
//                    {
//                        reportServcie.Commit(new FSCollectJobReportEntry(item));
//                    }
//                });

//            }
//            catch (Exception e)
//            {
//                throw;
//            }
//        }

//        public void FinalUpdateRecord()
//        {
//            if (cachedRecords.Count > 0)
//            {
//                RealAddOrUpdateRecord(cachedRecords.TakeAll().ToList());
//            }
//        }

//        public void ResetApplyExistingOption(Guid scopeId)
//        {
//            var url = $"{webApiUrl}/{WebRequestStr.ResetApplyExistingOption}?scopeId={scopeId}";
//            var result = helper.HTTPJsonPost(url, null);
//        }

//        public void UpdateJobTime(RMFileSystemJobTimeReferenceDto mFileSystemJobTimeReference)
//        {
//            var url = $"{webApiUrl}/{WebRequestStr.GetAllSubLocationTerm}";
//            var data = JsonConvert.SerializeObject(mFileSystemJobTimeReference);
//            var result = helper.HTTPJsonPost(url, data);
//        }

//        #region useless
//        //public DateTime GetLastJobTime(Guid scopeId)
//        //{
//        //    var url = $"{webApiUrl}/{WebRequestStr.GetLastJobTime}";
//        //    var data = JsonConvert.SerializeObject(scopeId);
//        //    var result = helper.HTTPJsonPost(url, data);
//        //    return JsonConvert.DeserializeObject<DateTime>(result);
//        //}

//        //public List<Guid> GetChangedTermIds(long ticks)
//        //{
//        //    var url = $"{webApiUrl}/{WebRequestStr.GetChangedTermIds}";
//        //    var result = helper.HTTPJsonPost(url, ticks.ToString());
//        //    return JsonConvert.DeserializeObject<List<Guid>>(result);
//        //}

//        //public RMTermSet GetRMTermSetByGuid(Guid termSetId)
//        //{
//        //    var url = $"{webApiUrl}/{WebRequestStr.GetRMTermSetByGuid}";
//        //    var result = helper.HTTPJsonPost(url, termSetId.ToString());
//        //    return JsonConvert.DeserializeObject<RMTermSet>(result);
//        //}

//        //public List<RMTerm> GetAllTermsUnderTermSet(int termSetId)
//        //{
//        //    var url = $"{webApiUrl}/{WebRequestStr.GetAllTermsUnderTermSet}";
//        //    var result = helper.HTTPJsonPost(url, termSetId.ToString());
//        //    return JsonConvert.DeserializeObject<List<RMTerm>>(result);
//        //}

//        //public RMTerm GetRMTermByGuId(Guid termSetId)
//        //{
//        //    var url = $"{webApiUrl}/{WebRequestStr.GetRMTermByGuid}";
//        //    var result = helper.HTTPJsonPost(url, termSetId.ToString());
//        //    return JsonConvert.DeserializeObject<RMTerm>(result);
//        //}

//        //public List<RMTerm> GetAllSubLocationTerm(int termId)
//        //{
//        //    var url = $"{webApiUrl}/{WebRequestStr.GetAllSubLocationTerm}";
//        //    var result = helper.HTTPJsonPost(url, termId.ToString());
//        //    return JsonConvert.DeserializeObject<List<RMTerm>>(result);
//        //}
//        #endregion


//    }
//}
