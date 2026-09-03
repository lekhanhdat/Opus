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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class EXOArchiverIndexSubInfoDao : BaseDao<EXOArchiverIndexSubInfo>, IEXOArchiverIndexSubInfoDao
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(EXOArchiverIndexSubInfoDao));
        public void CreateEXOSubInfo(EXOArchiverIndexSubInfo info)
        {
            using (var context = GetNewContext())
            {
                context.EXOArchiverIndexSubInfos.Add(info);
                context.SaveChanges();
            }
        }
        public long GetArchiverStorageGBSize()
        {
            //防止计数不那么准确，先用MB为单位统计
            long sizeInMB = 0;
            using (var context = GetNewContext())
            {
                var mediaDataSizeList = context.EXOArchiverIndexSubInfos.Select(i => i.MediaDataSize).ToList();

                if (mediaDataSizeList != null)
                {
                    foreach (var size in mediaDataSizeList)
                    {
                        sizeInMB += size / (1024 * 1024);
                    }
                }
            }
            return sizeInMB / 1024;
        }

        public async Task<double> GetArchiverStorageGBSizeAsync(string storageId, CancellationToken cancellationToken = default)
        {
            long totalBytes = 0;
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 900;
                totalBytes = await context.EXOArchiverIndexSubInfos
                    .Where(i => i.CurrentStorageId == storageId)
                    .SumAsync(i => (long?)i.MediaDataSize, cancellationToken) ?? 0;
            }
            return totalBytes / 1024d / 1024 / 1024;
        }

        public EXOArchiverIndexSubInfo GetEXOArchiverSubInfoBySubSubJobId(string subSubJobId)
        {
            EXOArchiverIndexSubInfo result = null;
            using (var context = GetNewContext())
            {
                var tempResult = context.EXOArchiverIndexSubInfos.AsQueryable().Where(s => s.SubSubJobId == subSubJobId).ToList();
                result = tempResult?.FirstOrDefault();
            }
            return result;
        }

        public void UpdateEXOSubInfoMergeStatusBySubSubJobId(string subSubJobId, int status)
        {
            using (var context = GetNewContext())
            {
                var tempResult = context.EXOArchiverIndexSubInfos.AsQueryable().Where(s => s.SubSubJobId == subSubJobId).ToList();
                var result = tempResult?.FirstOrDefault();
                result.MergeIndexState = status;
                context.EXOArchiverIndexSubInfos.AddOrUpdate(result);
                context.SaveChanges();
            }
        }

        public void UpdateEXOSubInfoSizeBySubSubJobId(string subSubJobId,long size)
        {

            using (var context = GetNewContext())
            {
                var tempResult = context.EXOArchiverIndexSubInfos.AsQueryable().Where(s => s.SubSubJobId == subSubJobId).ToList();
                var result = tempResult?.FirstOrDefault();
                result.MediaDataSize = size;
                context.EXOArchiverIndexSubInfos.AddOrUpdate(result);
                context.SaveChanges();
            }
        }

        public Dictionary<string, double> GetAllEXOArchiverIndexSubInfoByMailboxAddresses(List<string> mailboxAddresses)
        {
            var result = new Dictionary<string, double>();
            using (var context = GetNewContext())
            {
                foreach (var address in mailboxAddresses)
                {
                    var infoes = context.EXOArchiverIndexSubInfos.AsQueryable().Where(s => s.MailBoxAddress == address && s.MergeIndexState == (int)MergeIndexState.Succeed);
                    if (!infoes.Any())
                    {
                        result[address] = 0;
                        continue;
                    }
                    var totalArchivedSize = infoes.Select(s => s.MediaDataSize).Sum();
                    var sizeInGB = (double)totalArchivedSize / ContractConstants.GBSizeInterval;
                    result[address] = sizeInGB;
                }
            }
            return result;
        }

        public Dictionary<string, double> GetAllEXOArchivedSizeMapping((long, long)? archivedTimeRange = null)
        {
            var result = new Dictionary<string, double>();
            using (var context = GetNewContext())
            {
                int pageIndex = 0;
                int pageSize = 5000;

                do
                {
                    IQueryable<EXOArchiverIndexSubInfo> query = context.EXOArchiverIndexSubInfos;
                    if (archivedTimeRange != null)
                    {
                        var (startTime, endTime) = archivedTimeRange.Value;
                        query = query.Where(i => i.ArchiverTime >= startTime && i.ArchiverTime <= endTime);
                    }

                    var temp = query.OrderBy(s => s.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();

                    foreach (var item in temp)
                    {
                        var sizeInGB = (double)item.MediaDataSize / ContractConstants.GBSizeInterval;
                        if (!result.TryGetValue(item.MailBoxAddress, out var sum))
                        {
                            sum = 0;
                        }
                        sum += sizeInGB;
                        result[item.MailBoxAddress] = sum;
                    }

                    if (temp.Count < pageSize)
                    {
                        break;
                    }
                    else
                    {
                        pageIndex++;
                    }

                } while (true);

            }
            return result;
        }
        public List<string> GetAllBackupOrMergeIndexFailedEXOSubJobIds()
        {
            mLogger.Info("start GetAllBackupOrMergeIndexFailedEXOSubJobIds");
            List<string> subJobIds = new List<string>();
            List<EXOArchiverIndexSubInfo> domains = new List<EXOArchiverIndexSubInfo>();
            using (var context = GetNewContext())
            {
                domains = context.EXOArchiverIndexSubInfos.Where(a => a.MergeIndexState != (int)MergeIndexState.Succeed && a.MergeIndexState != (int)MergeIndexState.DAOMigrated).ToList();
            }
            foreach (var temp in domains)
            {
                string subjobid = temp.SubSubJobId.Substring(0, temp.SubSubJobId.LastIndexOf('_'));
                if (!subJobIds.Contains(subjobid))
                {
                    subJobIds.Add(subjobid);
                }
            }
            mLogger.Info($"finish GetAllBackupOrMergeIndexFailedEXOSubJobIds,count:{subJobIds.Count}");
            return subJobIds;
        }
        public List<EXOArchiverIndexSubInfo> GetAllEXOArchiverIndexSubInfoByMainJobId(string mainJobId)
        {
            using (var context = GetNewContext())
            {
                return context.EXOArchiverIndexSubInfos.AsQueryable().Where(a => a.SubSubJobId.StartsWith(mainJobId)).ToList();
            }
        }
        public Dictionary<string, double> GetAllEXOArchiverIndexSubInfoByMailboxAddresses(List<string> mailboxAddresses, long startTime, long endTime)
        {
            var result = new Dictionary<string, double>();
            using (var context = GetNewContext())
            {
                foreach (var address in mailboxAddresses)
                {
                    var infoes = context.EXOArchiverIndexSubInfos.AsQueryable()
                        .Where(
                        s => s.MailBoxAddress == address 
                        && s.MergeIndexState == (int)MergeIndexState.Succeed 
                        && s.ArchiverTime <= endTime 
                        && s.ArchiverTime >= startTime);
                    if (!infoes.Any())
                    {
                        result[address] = 0;
                        continue;
                    }
                    var totalArchivedSize = infoes.Select(s => s.MediaDataSize).Sum();
                    var sizeGB = (double)totalArchivedSize / ContractConstants.GBSizeInterval;
                    result[address] = sizeGB;
                }
            }
            return result;
        }

        public async Task<EXOArchiverIndexSubInfo> GetSubInfoBySubsubJobIdAsync(string subsubjobId)
        {
            using (var context = GetNewContext())
            {
                return await context.EXOArchiverIndexSubInfos.FirstOrDefaultAsync(item => subsubjobId == item.SubSubJobId);
            }
        }

        public List<EXOArchiverIndexSubInfo> GetAllArchiverIndexSubInfo()
        {
            List<string> subJobIds = new List<string>();
            List<EXOArchiverIndexSubInfo> domains = new List<EXOArchiverIndexSubInfo>();
            long now = DateTime.UtcNow.Ticks;
            using (var context = GetNewContext())
            {
                int pageIndex = 0;
                int pageSize = 500;
                while (true)
                {
                    var temp = context.EXOArchiverIndexSubInfos.OrderBy(s => s.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                    if (temp == null || temp.Count != pageSize)
                    {
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
            return domains;
        }

        public List<EXOArchiverIndexSubInfo> GetAllArchiverIndexSubInfoByStorageId(string storageId)
        {
            List<string> subJobIds = new List<string>();
            List<EXOArchiverIndexSubInfo> domains = new List<EXOArchiverIndexSubInfo>();
            long now = DateTime.UtcNow.Ticks;
            using (var context = GetNewContext())
            {
                int pageIndex = 0;
                int pageSize = 500;
                while (true)
                {
                    var temp = context.EXOArchiverIndexSubInfos.Where(s => s.StorageId.Equals(storageId, StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                    if (temp == null || temp.Count != pageSize)
                    {
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
            return domains;
        }
    }
}
