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
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.DisposalStubDao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.DeleteArchivedData.Cache;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using Cloud.Sdk.Data.AosModern;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{
    public class RMDeleteArchivedDataStubManager
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataStubManager));

        private readonly RestoredSitesInfo _restoredSiteInfo;

        private readonly RMDeleteArchivedDataSettingManager _settingManager;

        private readonly RMDeleteArchivedDataSiteCacheManager _siteCacheManager;

        private readonly RMDeleteArchivedDataWebCacheManager _webCacheManager;

        private readonly bool _hasSiteInfo;

        private readonly AppProfileInfo _profileInfo;

        private readonly TenantConnectionInfo _tenantConnectionInfo;

        private TokenResult _tokenResult;
        private IRMStubFileRecordDao StubFileRecordDao => PlatformWindsorManager.GetService<IRMStubFileRecordDao>();

        public RMDeleteArchivedDataStubManager(
            RestoredSitesInfo restoredSiteInfo, 
            RMDeleteArchivedDataSettingManager settingManager,
            RMDeleteArchivedDataSiteCacheManager siteCacheManager,
            RMDeleteArchivedDataWebCacheManager webCacheManager)
        {
            _restoredSiteInfo = restoredSiteInfo;
            _settingManager = settingManager;
            _siteCacheManager = siteCacheManager;
            _webCacheManager = webCacheManager;

            var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(restoredSiteInfo.SiteUrl);
            _hasSiteInfo = remoteSiteCollection != null;
            if (!_hasSiteInfo)
            {
                _logger.Error($"The site [{restoredSiteInfo.SiteUrl}] not found in opus db.");
                return;
            }

            var o365TenantId = remoteSiteCollection.TenantId;
            _profileInfo = PoolUserUtil.GetBPOSInfoAsync(o365TenantId).GetAwaiter().GetResult();
            if (_profileInfo == null)
            {
                _logger.Error($"The site [{restoredSiteInfo.SiteUrl}] no app profile found.");
                return;
            }

            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            _tenantConnectionInfo = client.TenantManagementService.GetByTenantIdAsync(o365TenantId).GetAwaiter().GetResult();
            if (_tenantConnectionInfo == null)
            {
                _logger.Error($"The site [{restoredSiteInfo.SiteUrl}] no tenant info found in AOS.");
                return;
            }
        }

        public async Task<bool> DeleteStubsAsync(ArchiverBasicIndex item)
        {
            try
            {
                if (!_settingManager.IsEnableDeleteAllStub() || item.Name.Contains(":"))
                {
                    return true;
                }

                var (has, stubId, stubType) = TryGetStubInfo(item.stubInfo);
                if (!has)
                {
                    return true;
                }

                if (!_hasSiteInfo || _profileInfo == null || _tenantConnectionInfo == null)
                {
                    return false;
                }

                var res = true;


                if (stubId != null)
                {
                    var stubDic = await SearchStubsAsync(stubId);
                    var originalStubUrl = item.Url.TrimEnd('/') + GetStubExtension(stubType);
                    foreach (var stubEntry in stubDic)
                    {
                        _logger.Info($"Start delete item [{item.Id}] site [{stubEntry.Key}] stubs [{stubEntry.Value.Count}].");
                        res &= DeleteStubs(stubEntry.Key, stubEntry.Value, item.NodeGuid);
                        _logger.Info($"End delete item [{item.Id}] site [{stubEntry.Key}] stubs [{stubEntry.Value.Count}].");
                    }
                    if (!stubDic.Values.SelectMany(item => item.Select(item => item.stubUrl)).ToHashSet().Contains(originalStubUrl))
                    {
                        _logger.Info("[Delete Stub] The stub file maybe moved, delete the original stub file by path.");
                        var webRelativeUrl = _webCacheManager.GetWebRelativeUrl(item.ParentPathMD5);
                        res &= DeleteOriginalStub(item, stubType, webRelativeUrl);
                    }
                }
                else
                {
                    _logger.Info("[Delete Stub] The stubId is null, so delete the original stub file by path.");
                    var webRelativeUrl = _webCacheManager.GetWebRelativeUrl(item.ParentPathMD5);
                    res &= DeleteOriginalStub(item, stubType, webRelativeUrl);
                }

                return res;
            }
            catch (Exception e)
            {
                if (item != null && item.stubInfo != null)
                {
                    _logger.Info($"ItemInfo:{item.stubInfo}");
                }
                _logger.Error($"An error occurred while delete item [{item.Id}] stubs. Error: {e}");
                return false;
            }
        }

        private async Task<Dictionary<string, List<(string stubUrl, Guid webId)>>> SearchStubsAsync(string stubId)
        {
            var res = new Dictionary<string, List<(string stubUrl, Guid webId)>>();

            using var searchContext = new ClientContext(_tenantConnectionInfo.AdminUrl);
            var token = await GetTokenAsync();
            searchContext.ExecutingWebRequest += (sender, e) => e.WebRequestExecutor.WebRequest.Headers["Authorization"] = "Bearer " + token;
            var keywordQuery = new KeywordQuery(searchContext);
            keywordQuery.SelectProperties.Add("SiteName");
            keywordQuery.SelectProperties.Add("Path");
            keywordQuery.SelectProperties.Add("WebID");
            keywordQuery.TrimDuplicates = false;
            keywordQuery.RowLimit = 10;
            keywordQuery.StartRow = 0;
            keywordQuery.EnableSorting = true;
            keywordQuery.Culture = 1033;
            keywordQuery.QueryText = stubId;
            var searchExecutor = new SearchExecutor(searchContext);
            var results = searchExecutor.ExecuteQuery(keywordQuery);
            await searchContext.ExecuteQueryAsync();
            var result = results.Value[0].ResultRows.ToList();
            foreach (var row in result)
            {
                var siteName = row["SiteName"].ToString();
                var path = row["Path"].ToString();
                var webId = new Guid(row["WebID"].ToString());
                if (res.TryGetValue(siteName, out var paths))
                {
                    paths.Add((path, webId));
                }
                else
                {
                    res[siteName] = new List<(string stubUrl, Guid webId)> { (path, webId) };
                }
            }

            return res;
        }

        private (bool has, string stubId, string stubType) TryGetStubInfo(string stubInfo)
        {
            if (string.IsNullOrWhiteSpace(stubInfo) || stubInfo.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return (false, null, null);
            }

            var doc = new XmlDocument();
            doc.LoadXml(stubInfo);
            //var elements = doc.GetElementsByTagName("StubInfo");
            var element = doc.GetElementsByTagName("StubInfo").Cast<XmlElement>().FirstOrDefault();
            if (element == null)
            {
                return (false, null, null);
            }
            var id = element.HasAttribute("StubId") ? element.GetAttribute("StubId") : null;
            var type = element.HasAttribute("StubType") ? element.GetAttribute("StubType") : null;
            _logger.Info("TryGetStubInfo Successful.");
            return (!string.IsNullOrWhiteSpace(id) || !string.IsNullOrWhiteSpace(type), id, type);
        }

        private bool DeleteStubs(string siteUrl, List<(string stubUrl, Guid webId)> stubs, string nodeGuid)
        {
            try
            {
                if (!_siteCacheManager.TryGetSite(siteUrl, out var siteRecordPair))
                {
                    _logger.Error($"The site [{siteUrl}] not found in opus.");
                    return false;
                }

                var (site, record) = siteRecordPair;
                foreach (var stub in stubs)
                {
                    var fileInfo = site.OpenWeb(stub.webId).GetFile(stub.stubUrl);
                    if (!fileInfo.Exists)
                    {
                        continue;
                    }

                    try
                    {
                        fileInfo.Delete();
                    }
                    catch (Exception e)
                    {
                        _logger.Info($"delete file exception: {e.Message}. retry action.");
                        record.UndeclareItemAsRecord(fileInfo.Item);
                        fileInfo.Delete();
                    }
                }

                StubFileRecordDao.DeleteStubFileRecordEntitiesInBatch(TenantLocalValue.LogonGroupId, site.ID.ToString(), nodeGuid);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete stubs. Error: {e}");
                return false;
            }
        }

        private bool DeleteOriginalStub(ArchiverBasicIndex item, string stubType, string webRelativeUrl)
        {
            try
            {
                if (!_siteCacheManager.TryGetSite(item.SitePath, out var siteRecordPair))
                {
                    _logger.Error($"The site [{item.SitePath}] not found in opus.");
                    return false;
                }

                var (site, record) = siteRecordPair;
                var fileInfo = site.OpenWeb(webRelativeUrl).GetFile(item.Url.TrimEnd('/') + GetStubExtension(stubType));
                if (!fileInfo.Exists)
                {
                    StubFileRecordDao.DeleteStubFileRecordEntitiesInBatch(TenantLocalValue.LogonGroupId, site.ID.ToString(), item.NodeGuid);
                    return true;
                }

                if (fileInfo.Item != null
                      && fileInfo.Item.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                      && fileInfo.Item.FieldValues[LinkFileCommon.LinkFileFieldName] != null
                      && fileInfo.Item.FieldValues[LinkFileCommon.LinkFileFieldName].ToString().Length > 0)
                {
                    try
                    {

                        fileInfo.Delete();
                    }
                    catch (Exception e)
                    {
                        _logger.Info($"delete original stub exception: {e.Message}. retry action.");
                        record.UndeclareItemAsRecord(fileInfo.Item);
                        fileInfo.Delete();
                    }
                }

                StubFileRecordDao.DeleteStubFileRecordEntitiesInBatch(TenantLocalValue.LogonGroupId, site.ID.ToString(), item.NodeGuid);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete original stub. Error: {e}");
                return false;
            }
        }

        private static string GetStubExtension(string stubType)
        {
            if (stubType.Equals("Aspx", StringComparison.OrdinalIgnoreCase))
            {
                return ".aspx";
            }
            else if (stubType.Equals("Html", StringComparison.OrdinalIgnoreCase))
            {
                return ".html";
            }
            else if (stubType.Equals("Link", StringComparison.OrdinalIgnoreCase))
            {
                return ".url";
            }

            return ".txt";
        }

        private async Task<string> GetTokenAsync()
        {
            if (_tokenResult != null && _tokenResult.ExpiresOn > DateTime.UtcNow.AddMinutes(10))
            {
                return _tokenResult.AccessToken;
            }

            var client = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId);
            _tokenResult = await client.ModernTokenService.GetTokenByAppProfileAsync(
                _profileInfo.Type,
                TokenResourceType.SharePoint,
                _profileInfo.TenantId,
                _profileInfo.Id,
                new Uri(_tenantConnectionInfo.AdminUrl).GetLeftPart(UriPartial.Authority),
                TokenType.ApplicationToken
            );
            return _tokenResult.AccessToken;
        }
    }
}
