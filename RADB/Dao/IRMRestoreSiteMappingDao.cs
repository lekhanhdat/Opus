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
using AvePoint.RA.Contract.RestoreCenter;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMRestoreSiteMappingDao : IBaseDao<RMRestoreSiteMapping>
    {
        void SaveMapping(List<RMRestoreSiteMapping> record);
        Task CreateByBulkCopyAsync(IEnumerable<RMRestoreSiteMapping> items);
        void DeleteAllMapping();
        void DeleteAllMappingByPage();
        List<RMRestoreSiteMapping> GetAllMappings();
        List<RMRestoreSiteMapping> GetRecordsByIds(IEnumerable<String> ids);
        List<RMRestoreSiteMapping> GetMappingsById(IEnumerable<String> ids);
        List<RMRestoreSiteMapping> GetMappings(int pageIndex, int pageSize, out int totalRecord);
        List<RMRestoreSiteMapping> GetSiteMappingsByTargetSCUrl(string targetSCUrl);
        List<String> GetSourceSCUrlsByTargetSCUrl(string targetSCUrl);
        Task<List<string>> GetSourceSCUrlsByTargetSCUrlAsync(string targetSCUrl);
        void DeleteMappingBySourceUrl(string sourceUrl);
        void BatchDeleteMapping(params string[] ids);
        void DeleteMapping(string sourceUrl, string targetSiteUrl);
        RMRestoreSiteMapping GetMappingBySourceSiteUrl(string sourceSiteUrl);
        Task<RMRestoreSiteMapping> GetMappingBySourceSiteUrlAsync(string sourceSiteUrl);
        bool ExistMappingInSourcesSiteUrls(IEnumerable<string> sourceSiteUrl);
        int GetLastMappingIntId();

        #region whitelist

        List<RMRestoreSiteMapping> GetAllWhitelist();
        List<RMRestoreSiteMapping> GetWhitelistByPage(int pageIndex, int pageSize, out int totalRecord);
        void BatchDeleteWhitelist(params string[] ids);
        bool ExistWhitelistInSiteUrls(IEnumerable<string> siteUrl);

        int GetWhiteListCount();
        int GetLastWhitelistIntId();
        int GetWhitelistCount();
        void DeleteWhitelist();
        void ConvertFullTextIndexListType(RestoreSettingFlag source, RestoreSettingFlag target);
        void DeleteMappingsByFlag(RestoreSettingFlag flag);
        void SaveWhitelist(RMRestoreSiteMapping siteInfo);
        List<RMRestoreSiteMapping> GetAllBlacklist();
        List<RMRestoreSiteMapping> GetBlacklistByPage(int pageIndex, int pageSize, out int totalRecord);
        void BatchDeleteBlacklist(params string[] ids);
        bool ExistBlacklistInSiteUrls(IEnumerable<string> siteUrl);
        int GetBlacklistCount();
        int GetLastBlacklistIntId();
        void DeleteBlacklist();
        #endregion
    }
}
