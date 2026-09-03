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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FSIndexSubInfoDao : BaseDao<FSIndexSubInfo>, IFSIndexSubInfoDao
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(FSIndexSubInfoDao));

        public List<ArchiverIndexSubInfoContract> GetSubInfoesBySubJobId(string subJobId)
        {
            List<ArchiverIndexSubInfoContract> result = new List<ArchiverIndexSubInfoContract>();
            List<FSIndexSubInfo> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.FSIndexSubInfos.Where(s => s.SubJobId == subJobId).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                foreach (FSIndexSubInfo domain in domains)
                {
                    result.Add(this.ConvertToDto(domain));
                }
            }
            return result;
        }
        public async Task UpdateArchiverIndexSubInfoMediaSizeAsync(string jobId, long size)
        {
            FSIndexSubInfo domain = null;
            using (var context = GetNewContext())
            {
                domain = context.FSIndexSubInfos.Where(s => s.SubSubJobId.Equals(jobId)).FirstOrDefault();

                if (domain != null)
                {
                    logger.Info($"Decrease media data size for {jobId}, original size: {domain.MediaDataSize}, decrease size: {size}");
                    domain.MediaDataSize -= size;
                    if (domain.MediaDataSize < 0)
                    {
                        domain.MediaDataSize = 0;
                    }
                    await this.UpdateAsync(domain);
                }
                else
                {
                    logger.Warn("UpdateArchiverIndexSubInfoMediaSizeAsync:cannot find job Id: {0} ", jobId);
                }
            }
        }
        public List<FSIndexSubInfo> GetAllFSArchiverIndexSubInfoByStorageId(string storageId)
        {
            logger.Info("GetAllFSArchiverIndexSubInfoByStorageId: storageId: {0}", storageId);
            List<string> subJobIds = new List<string>();
            List<FSIndexSubInfo> domains = new List<FSIndexSubInfo>();
            long now = DateTime.UtcNow.Ticks;
            using (var context = GetNewContext())
            {
                int pageIndex = 0;
                int pageSize = 500;
                while (true)
                {
                    var temp = context.FSIndexSubInfos.Where(s => s.StorageId.Equals(storageId, StringComparison.OrdinalIgnoreCase) && s.RetentionTime < now).OrderBy(s => s.RetentionTime).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                    if (temp == null || temp.Count != pageSize)
                    {
                        logger.Info($"this page is the last page,temp count:{temp?.Count},pageindex:{pageIndex}");
                        if (temp != null)
                        {
                            domains.AddRange(temp);
                        }
                        break;
                    }
                    else
                    {
                        pageIndex++;
                        domains.AddRange(temp);
                    }
                }
            }
            logger.Info("GetAllFSArchiverIndexSubInfoByStorageId: storageId: {0}, subIndexs count: {1}.", storageId, domains.Count);
            return domains;
        }
        private ArchiverIndexSubInfoContract ConvertToDto(FSIndexSubInfo domain)
        {
            if (domain == null)
            {
                return null;
            }
            ArchiverIndexSubInfoContract info = new ArchiverIndexSubInfoContract();
            info.Id = domain.Id;
            info.JobId = domain.SubSubJobId;
            info.RetentionTime = domain.RetentionTime;
            info.RetentionTimeSpanSeconds = domain.KeepTime;
            info.StorageInfo = domain.StorageId;
            info.CurrentStorageId = domain.CurrentStorageId;
            info.MediaDataSize = domain.MediaDataSize;
            info.AgentDataSize = domain.AgentDataSize;
            info.RetentionCount = domain.RetentionCount;
            if (domain.Extension != null && domain.Extension != string.Empty)
            {
                info.ArchiverSubInfoExtension = SerializerHelper.DeserializeByDataContractSerializer<ArchiverSubInfoExtension>(domain.Extension);
                if (info.ArchiverSubInfoExtension != null)
                {
                    info.DataEncryptionInfo = info.ArchiverSubInfoExtension.DataEncryptionInfo;
                }
            }
            return info;
        }
        public async Task UpdateArchiverIndexSubInfoMergeIndexStatusAsync(string jobId, int status)
        {
            FSIndexSubInfo domain = null;
            using (var context = GetNewContext())
            {
                domain = context.FSIndexSubInfos.Where(s => s.SubSubJobId.Equals(jobId)).FirstOrDefault();

                if (domain != null)
                {
                    domain.MergeIndexState = status;
                    await this.UpdateAsync(domain);
                }
                else
                {
                    logger.Warn("UpdateArchiverIndexSubInfoMergeIndexStatus:cannot find job Id: {0} ", jobId);
                }
            }
        }

        public List<ArchiverIndexSubInfoContract> GetIndexByJobId(string jobId)
        {
            throw new NotImplementedException();
        }

        public ArchiverIndexSubInfoContract GetIndexBySubJobId(string subjobId)
        {
            return ConvertToDto(base.Find(s => s.SubJobId == subjobId));
        }

        public ArchiverIndexSubInfoContract GetIndexBySubSubJobId(string subsubjobId)
        {
            return ConvertToDto(base.Find(s => s.SubSubJobId == subsubjobId));
        }
    }
}
