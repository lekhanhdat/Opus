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
using AveClientRequest.Common;
using AvePoint.ObjectModel.WebService;
using AvePoint.ObjectModel.WebService.Authentication;
using AvePoint.Office365.Api;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.CompliancePolicy;
using Microsoft.SharePoint.Client.Taxonomy;
using Microsoft.SharePoint.Client.UserProfiles;
using Microsoft.SharePoint.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClientFile = Microsoft.SharePoint.Client.File;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request
    {

        [ReplaceByAPI]
        public override int GetOneDriveCount(List<string> usernames)
        {
            using (AveClientContext context = CreateContext())
            {
                int oneDriveCount = 0;
                try
                {
                    PeopleManager pm = new PeopleManager(context);
                    Dictionary<string, PersonProperties> props = new Dictionary<string, PersonProperties>();
                    int batchSize = 250;
                    foreach (string username in usernames)
                    {
                        PersonProperties prop = pm.GetPropertiesFor(string.Format("i:0#.f|membership|{0}", username));
                        context.Load(prop, p => p.PersonalUrl);
                        props.Add(username, prop);
                        if (props.Count >= batchSize && props.Count % batchSize == 0)
                        {
                            context.ExecuteQuery();
                        }
                    }
                    if (context.HasPendingRequest)
                    {
                        context.ExecuteQuery();
                    }

                    foreach (KeyValuePair<string, PersonProperties> prop in props)
                    {
                        Dictionary<string, object> oneDriveInfo = AssembleSkyDriveProProperties(prop.Value, prop.Key);
                        if (!string.IsNullOrEmpty(oneDriveInfo["PersonalSpace"].ToString()))
                        {
                            oneDriveCount++;
                        }
                    }
                    return oneDriveCount;
                }
                catch (Exception e)
                {
                    mLogger.Error("Get OneDrive count failed, error message : {0}", e);
                    throw;
                }
            }
        }

        [ReplaceByAPI]
        public override int GetSiteCollectionsCount(string tenantAdminSiteUrl)
        {
            SPOSitePropertiesEnumerableFilter speFilter = new SPOSitePropertiesEnumerableFilter();
            var collection = GetSiteCollectionsList(tenantAdminSiteUrl, speFilter);
            return collection.Count;
        }

        [ReplaceByAPI]
        public override void UpdateSiteBasicPropertiesByUrl(string siteUrl, Dictionary<string, object> siteProp)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                var newSiteProps = tenant.GetSitePropertiesFromSharePointByFilter(string.Format("Url -eq '{0}'", siteUrl), "0", true);
                context.Load(newSiteProps);
                context.ExecuteQuery();
                var newSiteProp = newSiteProps.FirstOrDefault();
                if (newSiteProp != null)
                {
                    AveObjectCopy.UpdateObjectBasicProperties(siteProp, newSiteProp);
                    newSiteProp.Update();
                    context.ExecuteQuery();
                }
            };
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetSitePropertiesByUrl(string siteUrl)
        {
            SPOSitePropertiesEnumerableFilter filter = new SPOSitePropertiesEnumerableFilter()
            {
                Filter = string.Format("Url -eq '{0}", siteUrl.TrimEnd('/')),
            };
            return GetSiteCollectionsList(siteUrl, filter).FirstOrDefault();
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> CreatePersonalSiteEnqueueBulk(string[] emailIDs, string loginName)
        {
            Dictionary<string, object> newPersonalSiteProperty = new Dictionary<string, object>();
            DateTime endTime = DateTime.Now.AddMinutes(30);  //设置时间为30分钟，如果超出时间则停止等待。
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    ProfileLoader profileLoader = ProfileLoader.GetProfileLoader(context);
                    PeopleManager peopleManager = new PeopleManager(context);
                    ClientResult<string> result = null;
                    profileLoader.CreatePersonalSiteEnqueueBulk(emailIDs);
                    context.ExecuteQuery();
                    if (!string.IsNullOrEmpty(loginName))
                    {
                        do
                        {
                            System.Threading.Thread.Sleep(10000);
                            if (DateTime.Now > endTime)
                            {
                                throw new Exception("Create Site Collection timeout.");
                            }
                            result = peopleManager.GetUserProfilePropertyFor(loginName, "SPS-PersonalSiteInstantiationState");
                            context.ExecuteQuery();
                        } while (!result.Value.Equals(((int)PersonalSiteInstantiationState.Created).ToString()));
                    }
                    var userProfileProperties = peopleManager.GetPropertiesFor(loginName);
                    context.Load(userProfileProperties, property => property.PersonalUrl);
                    context.ExecuteQuery();
                    newPersonalSiteProperty["PersonalUrl"] = userProfileProperties.PersonalUrl;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to create Personal Site,  error message : {0}", e.ToString());
                newPersonalSiteProperty["ErrorMessage"] = e is ServerException ? "ServerException" + e.Message : e.Message; ;
            }
            return newPersonalSiteProperty;
        }


        [ReplaceByAPI]
        public override AveProvisionedMigrationQueueInfo ProvisionMigrationQueue()
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.ProvisionMigrationQueue();
                context.ExecuteQuery();
                var info = (ProvisionedMigrationQueueInfo)result.Value;
                return new AveProvisionedMigrationQueueInfo()
                {
                    JobQueueUri = info.JobQueueUri,
                    TypeId = info.TypeId
                };
            }
        }

        [ReplaceByAPI]
        public override AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers()
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.ProvisionMigrationContainers();
                context.ExecuteQuery();
                var info = (ProvisionedMigrationContainersInfo)result.Value;
                return new AveProvisionedMigrationContainersInfo()
                {
                    DataContainerUri = info.DataContainerUri,
                    EncryptionKey = info.EncryptionKey,
                    MetadataContainerUri = info.MetadataContainerUri,
                    TypeId = info.TypeId
                };
            }
        }

        [ReplaceByAPI]
        public override void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota)
        {
            using (ClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                var newSiteProps = tenant.GetSitePropertiesFromSharePointByFilter(string.Format("Url -eq '{0}'", siteUrl), "0", true);
                context.Load(newSiteProps);
                context.ExecuteQuery();
                var siteProperties = newSiteProps.FirstOrDefault();
                if (siteProperties != null)
                {
                    double rate = 0;
                    if (!string.Equals(siteProperties.Template, "SPSMSITEHOST#0")) //for my site
                    {
                        rate = siteProperties.StorageWarningLevel * 1.0 / siteProperties.StorageMaximumLevel * 1.0;
                        siteProperties.StorageWarningLevel = Convert.ToInt64(storageQuota * Math.Round(rate, 2));
                    }
                    siteProperties.StorageMaximumLevel = storageQuota;
                    if (!string.Equals(siteProperties.Template, "SPSMSITEHOST#0"))
                    {
                        rate = siteProperties.UserCodeMaximumLevel.Equals(0) ? 0.0 : siteProperties.UserCodeWarningLevel * 1.0 / siteProperties.UserCodeMaximumLevel * 1.0;
                        siteProperties.UserCodeWarningLevel = Convert.ToInt64(serverResourceQuota * Math.Round(rate, 2));
                    }
                    siteProperties.UserCodeMaximumLevel = serverResourceQuota;
                    siteProperties.Update();
                    context.ExecuteQuery();
                }
            }
        }


        [NoAPI("OperateSolution")]
        public override void ApplyCustomWebTemplateInSolution(string webServerRelativeUrl, string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                Web web = site.RootWeb;
                context.Load(site, item => item.Url);
                context.ExecuteQuery();

                #region 上传solution
                string fileUrl = webServerRelativeUrl.TrimEnd('/') + "/_catalogs/solutions/" + solutionName;
                using (FileStream fileStream = new FileStream(solutionPath, FileMode.Open, FileAccess.Read))
                {
                    ClientFile.SaveBinaryDirect(context, fileUrl, fileStream, true);
                }
                var path = ResourcePath.FromDecodedUrl(fileUrl);
                ClientFile file = web.GetFileByServerRelativePath(path);
                context.Load(file.ListItemAllFields, item => item.Id);
                context.ExecuteQuery();
                #endregion

                #region 查找solution  激活solution
                using (AveWebServiceRequest aveWebServiceRequest = new AveWebServiceRequest(site.Url, mUserAccountInfo, mObj, "15"))
                {
                    aveWebServiceRequest.OperateSolution("ACT", mWebUrl, AveUrlUtility.GetServerRelativeUrl(mWebUrl), file.ListItemAllFields.Id);
                }
                var filepath = ResourcePath.FromDecodedUrl(fileUrl);
                file = web.GetFileByServerRelativePath(filepath);
                context.Load(file, f => f.ListItemAllFields);
                context.Load(site.Features, fs => fs.Include(f => f.DefinitionId));
                context.ExecuteQuery();
                Dictionary<string, object> solutionPropiesDir = file.ListItemAllFields.FieldValues;
                #endregion

                #region 激活solution  同时要激活对应的feature
                object status;
                if (solutionPropiesDir.TryGetValue("Status", out status) && status is FieldLookupValue && int.Parse((solutionPropiesDir["Status"] as FieldLookupValue).LookupValue) == 1)
                {
                    bool activeFeature = false;
                    Guid newActiveSolutionId = solutionPropiesDir.ContainsKey("SolutionId") ? (Guid)solutionPropiesDir["SolutionId"] : new Guid();
                    foreach (AveSolutionFeature feature in packageFeatures)
                    {
                        if (packageSolutionId == newActiveSolutionId && feature.Scope == AveFeatureScope.Site)
                        {
                            if (site.Features.Select(f => f.DefinitionId == feature.FeatureId) == null)
                            {
                                site.Features.Add(feature.FeatureId, false, FeatureDefinitionScope.Site);
                                activeFeature = true;
                            }
                        }
                    }
                    if (activeFeature)
                    {
                        context.ExecuteQuery();
                    }
                }
                #endregion;

                #region 应用激活solution生成的WebTemplate
                web.ApplyWebTemplate(webTemplateName);
                context.ExecuteQuery();
                #endregion
            }
        }

        [KeepOriginalWithAPI]
        public override List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames)
        {
            return base.LoadPersonalSiteInfosForUsers(usernames);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetSiteBasicProperties()
        {
            return base.GetSiteBasicProperties();
        }

        [KeepOriginalWithAPI]
        public override int GetSiteOwnerId()
        {
            return base.GetSiteOwnerId();
        }

        [KeepOriginalWithAPI]
        public override DateTime GetLocalToUTCTime(string webServerRelativeUrl, DateTime time)
        {
            return base.GetLocalToUTCTime(webServerRelativeUrl, time);
        }

        [KeepOriginalWithAPI]
        public override DateTime GetUTCToLocalTime(string webServerRelativeUrl, DateTime time)
        {
            return base.GetUTCToLocalTime(webServerRelativeUrl, time);
        }

        [ReplaceByAPI]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".onmicrosoft.com should be ignored")]
        public override Dictionary<string, object> GetSiteStorageInfo()
        {
            Dictionary<string, object> storageProperties;
            try
            {
                string adminSiteUrl = mUserAccountInfo.UserName.Contains(".onmicrosoft.com") ? AveUrlUtility.GetTenantAdminSiteUrl(mWebUrl) : null;
                storageProperties = GetSiteStorageInfo(adminSiteUrl, mWebUrl);
            }
            catch (Exception e)
            {
                storageProperties = new Dictionary<string, object>();
                mLogger.Warn("An error ocurred while getting storage info.Account info:{0},WebUrl:{1},Error:{2}", mUserAccountInfo, mWebUrl, e);
            }
            return storageProperties;
        }


        public Dictionary<string, object> GetSiteStorageInfo(string adminSiteUrl, string siteUrl)
        {
            Dictionary<string, object> storageProperties = new Dictionary<string, object>();
            try
            {
                if (!string.IsNullOrEmpty(adminSiteUrl))
                {
                    using (AveClientContext context = InitClientObject(adminSiteUrl))
                    {
                        Tenant tenant = new Tenant(context);
                        var newSiteProps = tenant.GetSitePropertiesFromSharePointByFilter(string.Format("Url -eq '{0}'", siteUrl), "0", true);
                        context.Load(newSiteProps);
                        context.ExecuteQuery();
                        var properties = newSiteProps.FirstOrDefault();
                        if (properties != null)
                        {
                            AveObjectCopy.GetObjectBasicProperties(storageProperties, properties);
                            ConvertUnit(storageProperties);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while getting storage info.AdminSiteUrl:{0},SiteUrl:{1}, Error:{2}", adminSiteUrl, siteUrl, e);
            }
            return storageProperties;
        }


        public void ConvertUnit(Dictionary<string, object> storageProperties)
        {
            if (!storageProperties["StorageMaximumLevel"].ToString().Equals("0"))
            {
                storageProperties["StorageMaximumLevel"] = (long)storageProperties["StorageMaximumLevel"] * 1024 * 1024;
            }
            if (!storageProperties["StorageWarningLevel"].ToString().Equals("0"))
            {
                storageProperties["StorageWarningLevel"] = (long)storageProperties["StorageWarningLevel"] * 1024 * 1024;
            }
            if (!storageProperties["UserCodeMaximumLevel"].ToString().Equals("0"))
            {
                storageProperties["UserCodeMaximumLevel"] = (double)storageProperties["UserCodeMaximumLevel"] * 1024 * 1024;
            }
            if (!storageProperties["UserCodeWarningLevel"].ToString().Equals("0"))
            {
                storageProperties["UserCodeWarningLevel"] = (double)storageProperties["UserCodeWarningLevel"] * 1024 * 1024;
            }
        }

        [KeepOriginalWithAPI]
        public override void ApplyTheme(string webServerRelativeUrl, string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated)
        {
            base.ApplyTheme(webServerRelativeUrl, colorPaletteUrl, fontSchemeUrl, backgroundImageUrl, shareGenerated);
        }

        [KeepOriginalWithAPI]
        public override void SetRootWebAndMySiteWebMasterPageInfo(string mySiteWebServerRelativeUrl, AveWebMasterPageInfo pageInfo)
        {
            base.SetRootWebAndMySiteWebMasterPageInfo(mySiteWebServerRelativeUrl, pageInfo);
        }

        [KeepOriginalWithAPI]
        public override AveWebMasterPageInfo GetRootWebMasterPageInfo()
        {
            return base.GetRootWebMasterPageInfo();
        }

        [ReplaceByAPI]
        public override bool GetSiteExists(string url)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = Site.Exists(context, url);
                context.ExecuteQuery();
                return result.Value;
            }
        }

        [ReplaceByAPI]
        public override Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.CreateMigrationJobEncrypted(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri, new EncryptionOption() { AES256CBCKey = options.AES256CBCKey });
                context.ExecuteQuery();
                return result.Value;
            }
        }

        [ReplaceByAPI]
        public override Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.CreateMigrationJob(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri);
                context.ExecuteQuery();
                return result.Value;
            }
        }

        [NoAPI]
        public override void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            base.UpdateSiteRssSetting(syndicationEnabled);
        }

        [KeepOriginalWithAPI]
        public override void UpdateSpecialProperty(Dictionary<string, object> specialProp)
        {
            base.UpdateSpecialProperty(specialProp);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetSiteEventReceiverDefinitions(string siteServerRelativeUrl, string eventReceiverDefSource)
        {
            return base.GetSiteEventReceiverDefinitions(siteServerRelativeUrl, eventReceiverDefSource);
        }

        [NoAPI]
        public override Dictionary<string, object> GetPublishingWeb(string webServerRelativeUrl)
        {
            return GetPublishingWeb(webServerRelativeUrl);
        }

        [ReplaceByAPI]
        public override Dictionary<string, string> GetWebUserResource(string webServerRelativeUrl, string resourceName, List<string> cultureNames)
        {
            using (AveClientContext context = CreateContext())
            {
                UserResource resource;
                Dictionary<string, ClientResult<string>> values = new Dictionary<string, ClientResult<string>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        resource = web.TitleResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        resource = web.DescriptionResource;
                        break;
                    default:
                        throw new Exception(string.Format("resource name is invalid.{0}", resourceName));
                }
                foreach (string cultureName in cultureNames)
                {
                    values.Add(cultureName, resource.GetValueForUICulture(cultureName));
                }
                context.ExecuteQuery();
                return values.ToDictionary(k => k.Key, v => v.Value.Value);
            }
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> AddFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            return base.AddFeature(webServerRelativeUrl, featureId, force, scope, featuresSource);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> AddWeb(string parentWebRelativeUrl, string webUrl, string description, uint language, string title, bool useSamePermissionsAsParentSite, string webTemplate, bool bConvertIfThere)
        {
            return base.AddWeb(parentWebRelativeUrl, webUrl, description, language, title, useSamePermissionsAsParentSite, webTemplate, bConvertIfThere);
        }

        [ReplaceByAPI]
        public override void AddPersonalSite(string accountName, int lcid)
        {
            using (var context = CreateContext(AveUrlUtility.GetTenantAdminSiteUrl(mWebUrl)))
            {
                var loader = ProfileLoader.GetProfileLoader(context);
                loader.CreatePersonalSiteEnqueueBulk(new string[] { accountName });
                loader.Context.ExecuteQuery();
            }
        }

        [NoAPI]
        public override void AddViewToAllNodes(string webServerRelativeUrl, Guid listId, Guid viewId)
        {
            throw new NotImplementedException();
        }

        [NoAPI]
        public override Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            return mRequestCommon.AddKeyWord(term, startDate, localId, calendarType);
        }

        [NoAPI]
        public override string AddSynonm(string term, string synTerm, string terms)
        {
            return mRequestCommon.AddSynonm(term, synTerm, terms);
        }

        [NoAPI]
        public override Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            return mRequestCommon.AddBestBet(term, bestBetUrlList, bestBetProp, action);
        }

        [NoAPI]
        public override void AddSitePolicy(string policySchema, string siteUrl)
        {
            mRequestCommon.AddSitePolicy(policySchema, siteUrl);
        }

        [ReplaceByAPI]
        public override bool AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl = "")
        {
            try
            {
                if(string.IsNullOrEmpty(username))
                {
                    mLogger.Debug("Current authentication method maybe is app profile. Do not need add admin.");
                    return true;
                }
                string adminSiteUrl = string.IsNullOrEmpty(tenantAdminSiteUrl) ? AveUrlUtility.GetTenantAdminSiteUrl(siteCollectionUrl) : tenantAdminSiteUrl;
                using (AveClientContext context = InitClientObject(adminSiteUrl))     //mObj should be the cookieContainer we get from tenant admin site
                {
                    Tenant tenant = new Tenant(context);
                    tenant.SetSiteAdmin(siteCollectionUrl, username, true);
                    context.ExecuteQuery();
                    return true;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to add user to site collection administrators, site collection url : {0}, username : {1}, error message : {2}", siteCollectionUrl, username, e.ToString());
                return false;
            }
        }

        [NoAPI]
        public override void AddTag(string url, Guid termId, string title, bool? isPrivate)
        {
            mWebServiceRequest.AddTag(url, termId, title, isPrivate);
        }

        [NoAPI]
        public override void AddComment(string url, string comment, bool? isHighPriority, string title)
        {
            mWebServiceRequest.AddComment(url, comment, isHighPriority, title);
        }

        [ReplaceByAPI]
        public override string AddSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    //ADO-185210。 Office 365,需要用BLANKINTERNETCONTAINER#0创建Publishing Portal站点。
                    if (string.Equals(template, "BLANKINTERNET#0", StringComparison.OrdinalIgnoreCase))
                    {
                        mLogger.Debug("Change web template from {0} to {1}.", template, "BLANKINTERNETCONTAINER#0");
                        template = "BLANKINTERNETCONTAINER#0";
                    }
                    Tenant tenant = new Tenant(context);
                    SpoOperation ope = tenant.CreateSite(
                        new SiteCreationProperties()
                        {
                            CompatibilityLevel = compatibilityLevel,
                            Lcid = lcid,
                            Owner = owner,
                            Template = template,
                            TimeZoneId = timeZoneId,
                            Title = title,
                            Url = url,
                            StorageMaximumLevel = storageQuota,
                            UserCodeMaximumLevel = resourceQuota,
                            UserCodeWarningLevel = Math.Floor(resourceQuota * 0.85),
                            StorageWarningLevel = (long)Math.Floor(storageQuota * 0.85)
                        });
                    context.Load(ope);
                    context.ExecuteQuery();
                    if (!ope.IsComplete)
                    {
                        SiteProperties siteProperties = null;
                        bool errorOccurred = false;
                        do
                        {
                            errorOccurred = false;
                            try
                            {
                                System.Threading.Thread.Sleep(10000);
                                var newSiteProps = tenant.GetSitePropertiesFromSharePointByFilter(string.Format("Url -eq '{0}'",url), "0", false);
                                context.Load(newSiteProps);
                                context.ExecuteQuery();
                                siteProperties = newSiteProps.FirstOrDefault();
                                mLogger.Debug("Site Collection Status:{0}", siteProperties?.Status);
                            }
                            catch (Exception e)
                            {
                                string message = e.Message;
                                mLogger.Warn("An error occurred while getting site properties. Error:{0}", e);
                                errorOccurred = true;
                            }
                        }
                        while (errorOccurred || (siteProperties != null && string.Equals("Creating", siteProperties.Status, StringComparison.OrdinalIgnoreCase)));
                    }
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to create site collection, url : {0}, error message : {1}", url, e.ToString());
                return e is ServerException ? "ServerException: " + e.Message : e.Message;
            }
        }


        [KeepOriginalWithAPI]
        public override void DeleteSiteToRecylebin(string CAUrl, string url)
        {
            DeleteSiteCore(CAUrl, url, true);
        }

        [KeepOriginalWithAPI]
        public override void DeleteSite(string CAUrl, string url)
        {
            DeleteSiteCore(CAUrl, url, false);
        }

        private void DeleteSiteCore(string CAUrl, string url, bool deleteToRecybleBin)
        {
            string adminUrl = string.IsNullOrEmpty(CAUrl) ? AveUrlUtility.GetTenantAdminSiteUrl(url) : CAUrl;
            using (AveClientContext context = CreateContext(adminUrl))
            {
                Tenant tenant = new Tenant(context);
                tenant.RemoveSite(url);
                context.ExecuteQuery();
                //Delete Site from recycle bin.
                DeletedSiteProperties siteProperties = null;
                do
                {
                    System.Threading.Thread.Sleep(10000);
                    try
                    {
                        siteProperties = tenant.GetDeletedSitePropertiesByUrl(url);
                        context.Load(siteProperties);
                        context.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("The Site {0} is deleting.Message:{1}", url, e);
                    }
                }
                while (!siteProperties.IsPropertyAvailable("Status")
                    || (string.IsNullOrEmpty(siteProperties.Status) && siteProperties.Status.Equals("Recycling", StringComparison.OrdinalIgnoreCase)));
                if (!deleteToRecybleBin)
                {
                    tenant.RemoveDeletedSite(url);
                    context.ExecuteQuery();
                }
            }
        }

        [KeepOriginalWithAPI]
        public override void DeleteWeb(string webServerRelativeUrl)
        {
            base.DeleteWeb(webServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetAvailableWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage)
        {
            return base.GetAvailableWebTemplates(webServerRelativeUrl, lcid, doIncludeCrossLanguage);
        }
        [NoAPI]
        public override Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            return base.GetSitePortal(siteUrl);
        }
        [NoAPI]
        public override List<string> GetSiteEnabledHelpCollections()
        {
            return base.GetSiteEnabledHelpCollections();
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetAllWebs()
        {
            return base.GetAllWebs();
        }

        protected override void LoadWebAndSubwebs(ClientContext context, Web web, WebCollection subWebs)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(context.Site);
                    context.Load(web);
                    context.Load(web, tempWeb => tempWeb.CurrentUser,
                                                 tempWeb => tempWeb.RootFolder,
                                                 tempWeb => tempWeb.RequestAccessEmail,
                                                 tempWeb => tempWeb.MembersCanShare,
                                                 tempWeb => tempWeb.AccessRequestSiteDescription,
                                                 tempWeb => tempWeb.UseAccessRequestDefault,
                                                 //tempWeb => tempWeb.ListTemplates,
                                                 tempWeb => tempWeb.AllProperties,
                                                 tempWeb => tempWeb.Navigation.TopNavigationBar,
                                                 tempWeb => tempWeb.Navigation.QuickLaunch,
                                                 tempWeb => tempWeb.AllowDesignerForCurrentUser,
                                                 tempWeb => tempWeb.HasUniqueRoleAssignments,
                                                 tempWeb => tempWeb.AssociatedMemberGroup, tempWeb => tempWeb.AssociatedMemberGroup.Users, tempWeb => tempWeb.AssociatedMemberGroup.Owner.Id, tempWeb => tempWeb.AssociatedMemberGroup.Owner.PrincipalType,
                                                 tempWeb => tempWeb.AssociatedOwnerGroup, tempWeb => tempWeb.AssociatedOwnerGroup.Users, tempWeb => tempWeb.AssociatedOwnerGroup.Owner.Id, tempWeb => tempWeb.AssociatedOwnerGroup.Owner.PrincipalType
                                                 );
                    context.Load(subWebs, tempWebs => tempWebs.IncludeWithDefaultProperties(tempWeb => tempWeb.CurrentUser,
                                                                                  tempWeb => tempWeb.RootFolder,
                                                                                  tempWeb => tempWeb.RequestAccessEmail,
                                                                                  tempWeb => tempWeb.MembersCanShare,
                                                                                  tempWeb => tempWeb.AccessRequestSiteDescription,
                                                                                  tempWeb => tempWeb.UseAccessRequestDefault,
                                                                                  //tempWeb => tempWeb.ListTemplates,
                                                                                  tempWeb => tempWeb.AllProperties,
                                                                                  tempWeb => tempWeb.Navigation.TopNavigationBar,
                                                                                  tempWeb => tempWeb.Navigation.QuickLaunch,
                                                                                  tempWeb => tempWeb.AllowDesignerForCurrentUser,
                                                                                  tempWeb => tempWeb.HasUniqueRoleAssignments,
                                                                                  tempWeb => tempWeb.AssociatedMemberGroup, tempWeb => tempWeb.AssociatedMemberGroup.Users, tempWeb => tempWeb.AssociatedMemberGroup.Owner.Id, tempWeb => tempWeb.AssociatedMemberGroup.Owner.PrincipalType,
                                                                                  tempWeb => tempWeb.AssociatedOwnerGroup, tempWeb => tempWeb.AssociatedOwnerGroup.Users, tempWeb => tempWeb.AssociatedOwnerGroup.Owner.Id, tempWeb => tempWeb.AssociatedOwnerGroup.Owner.PrincipalType
                                                                                  ));
                }
                using (scope.StartCatch())
                {
                    context.Load(context.Site);
                    context.Load(web);
                    context.Load(web, temp => temp.CurrentUser,
                                                 temp => temp.RootFolder,
                                                 temp => temp.RequestAccessEmail,
                                                 temp => temp.MembersCanShare,
                                                 temp => temp.AccessRequestSiteDescription,
                                                 temp => temp.UseAccessRequestDefault,
                                                 //temp => temp.ListTemplates,
                                                 temp => temp.AllProperties,
                                                 temp => temp.Navigation.TopNavigationBar,
                                                 temp => temp.Navigation.QuickLaunch,
                                                 temp => temp.AllowDesignerForCurrentUser,
                                                 temp => temp.HasUniqueRoleAssignments,
                                                 temp => temp.AssociatedMemberGroup, temp => temp.AssociatedMemberGroup.Users, temp => temp.AssociatedMemberGroup.Owner.Id, temp => temp.AssociatedMemberGroup.Owner.PrincipalType
                                                 //w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType
                                                 );
                    context.Load(subWebs, tempWebs => tempWebs.IncludeWithDefaultProperties(temp => temp.CurrentUser,
                                                                                  temp => temp.RootFolder,
                                                                                  temp => temp.RequestAccessEmail,
                                                                                  temp => temp.MembersCanShare,
                                                                                  temp => temp.AccessRequestSiteDescription,
                                                                                  temp => temp.UseAccessRequestDefault,
                                                                                  //temp => temp.ListTemplates,
                                                                                  temp => temp.AllProperties,
                                                                                  temp => temp.Navigation.TopNavigationBar,
                                                                                  temp => temp.Navigation.QuickLaunch,
                                                                                  temp => temp.AllowDesignerForCurrentUser,
                                                                                  temp => temp.HasUniqueRoleAssignments,
                                                                                  temp => temp.AssociatedMemberGroup, temp => temp.AssociatedMemberGroup.Users, temp => temp.AssociatedMemberGroup.Owner.Id, temp => temp.AssociatedMemberGroup.Owner.PrincipalType
                                                                                  //w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType
                                                                                  ));
                }
            }
            context.ExecuteQuery();
        }

        protected override void LoadWebCollection(ClientContext context, WebCollection webCollection)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(webCollection, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                                                 w => w.RootFolder,
                                                                                                 w => w.RequestAccessEmail,
                                                                                                 w => w.MembersCanShare,
                                                                                                 w => w.AccessRequestSiteDescription,
                                                                                                 w => w.UseAccessRequestDefault,
                                                                                                 //w => w.ListTemplates,
                                                                                                 w => w.AllProperties,
                                                                                                 w => w.Navigation.TopNavigationBar,
                                                                                                 w => w.Navigation.QuickLaunch,
                                                                                                 w => w.AllowDesignerForCurrentUser,
                                                                                                 w => w.HasUniqueRoleAssignments,
                                                                                                 w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Users, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType,
                                                                                                 w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType));
                }
                using (scope.StartCatch())
                {
                    context.Load(webCollection, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                                                 w => w.RootFolder,
                                                                                                 w => w.RequestAccessEmail,
                                                                                                 w => w.MembersCanShare,
                                                                                                 w => w.AccessRequestSiteDescription,
                                                                                                 w => w.UseAccessRequestDefault,
                                                                                                 //w => w.ListTemplates,
                                                                                                 w => w.AllProperties,
                                                                                                 w => w.Navigation.TopNavigationBar,
                                                                                                 w => w.Navigation.QuickLaunch,
                                                                                                 w => w.AllowDesignerForCurrentUser,
                                                                                                 w => w.HasUniqueRoleAssignments,
                                                                                                 w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Users, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType));
                    //w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType
                }
            }
            context.ExecuteQuery();
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetRecycleBin(string webServerRelativeUrl = null)
        {
            return base.GetRecycleBin(webServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFeatures(string serverRelativeUrl, string featuresSource)
        {
            return base.GetFeatures(serverRelativeUrl, featuresSource);
        }


        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetWeb(string webServerRelativeUrl)
        {
            return base.GetWeb(webServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetWeb(Guid webId)
        {
            return base.GetWeb(webId);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetSite()
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                try
                {
                    LoadSite(context);
                    LoadWeb(context.Site.RootWeb, context);
                    context.ExecuteQuery();

                    this.mCompatibilityLevel = context.Site.CompatibilityLevel;

                    this.maxItemsPerThrottledOperation = context.Site.MaxItemsPerThrottledOperation;
                    CopyProperty(siteProperties, context.Site);
                    siteProperties["Usage"] = AssemblyUsageInfo(context.Site.Usage);
                    Dictionary<string, object> rootWebProperties = GetWebProperties(context, context.Site.RootWeb, mWebUrl, context.Site.ServerRelativeUrl, true);
                    siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                    if (context.Site.IsObjectPropertyInstantiated("Owner") && context.Site.Owner.IsPropertyAvailable("Id"))
                    {
                        siteProperties["Owner" + AveObjectModelConstant.ObjectPropertySuffix] = context.Site.Owner.Id;
                    }
                    //siteProperties.Add("SyndicationEnabled", context.Site.RootWeb.SyndicationEnabled);
                    siteProperties["IsMoss"] = false;
                    mSiteRelativeUrl = context.Site.ServerRelativeUrl;
                    //siteProperties.Add("IsPublish", false);
                }
                catch (Exception se)
                {
                    mLogger.Debug("An error occurred while getting site properties. Url:{0}. Error:{1}", context.Url, se.ToString());
                    throw new AveSiteNotFoundException("Can not find site.",se);
                }
                //catch (Exception e)
                //{
                //    mLogger.Debug(AveClientOMRequestResource.GetSiteError, context.Url, e.ToString());
                //    throw;
                //}
                return siteProperties;
            }
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetAdminCenterSite()
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                try
                {
                    context.Load(context.Site);
                    context.Load(context.Site.RootWeb);
                    context.ExecuteQuery();
                    CopyProperty(siteProperties, context.Site);

                    mCompatibilityLevel = context.Site.CompatibilityLevel;
                    Dictionary<string, object> rootWebProperties = new Dictionary<string, object>();
                    CopyProperty(rootWebProperties, context.Site.RootWeb);
                    rootWebProperties["IsRootWeb"] = true;
                    siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                    mSiteRelativeUrl = context.Site.ServerRelativeUrl;
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetSiteError, context.Url, e);
                    throw;
                }
                return siteProperties;
            }
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetSubWebs(string webServerRelativeUrl)
        {
            return base.GetSubWebs(webServerRelativeUrl);
        }

        protected override void LoadSubWebs(AveClientContext context, WebCollection webCollection)
        {
            context.Load(webCollection, webs => webs.IncludeWithDefaultProperties(w => w.CurrentUser,
                                                                    w => w.RootFolder,
                                                                    //w => w.ListTemplates,
                                                                    w => w.AllProperties,
                                                                    w => w.Navigation.TopNavigationBar,
                                                                    w => w.Navigation.QuickLaunch,
                                                                    w => w.AllowDesignerForCurrentUser,
                                                                    w => w.HasUniqueRoleAssignments,
                                                                    w => w.AppInstanceId,
                                                                    w => w.AssociatedMemberGroup,
                                                                    w => w.AssociatedMemberGroup.Users,
                                                                    w => w.AssociatedMemberGroup.Owner.Id,
                                                                    w => w.AssociatedMemberGroup.Owner.PrincipalType,
                                                                    w => w.RequestAccessEmail,
                                                                    w => w.UseAccessRequestDefault,
                                                                    w => w.MembersCanShare,
                                                                    w => w.AccessRequestSiteDescription
                                                                    ));
        }

        [NoAPI]
        public override string GetApplicationPath(string serverRelativeUrl)
        {
            throw new NotImplementedException();
        }


        [KeepOriginalWithAPI]
        public override Dictionary<string, object> OpenCurrentWeb()
        {
            return base.OpenCurrentWeb();
        }

        [NoAPI("Method: mRequestCommon.GetWebSearchAndOfflineAvailability   AveWebASPXPageIndexMode")]
        protected override Dictionary<string, object> GetWebProperties(ClientContext context, Web web, string contextUrl, string siteServerRelativeUrl, bool webLoaded)
        {
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            if (!webLoaded)
            {
                context.Load(context.Site.RootWeb);
                LoadWeb(web, context);
                context.ExecuteQuery();
            }
            CopyProperty(webProperties, web);

            bool isAppWeb = web.AppInstanceId != Guid.Empty;
            webProperties["IsAppWeb"] = isAppWeb;
            webProperties["Exists"] = true;
            webProperties["CurrentUser" + AveObjectModelConstant.ObjectPropertySuffix] = web.CurrentUser.LoginName;
            //webProperties.Add("IsPublish", false);
            webProperties[WebPropertyNames.MembersCanShare] = web.MembersCanShare;
            webProperties[WebPropertyNames.AccessRequestSiteDescription] = web.AccessRequestSiteDescription;
            webProperties[WebPropertyNames.RequestAccessEmail] = web.RequestAccessEmail;
            webProperties[WebPropertyNames.UseAccessRequestDefault] = web.UseAccessRequestDefault;
            bool IsRootWeb = true;
            string Name = string.Empty;
            string ParentWebServerRelativeUrl = string.Empty;
            if (!web.ServerRelativeUrl.Equals(siteServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                IsRootWeb = false;//isRootWeb
                int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                Name = web.ServerRelativeUrl.Substring(lastSlashIndex + 1);
                ParentWebServerRelativeUrl = web.ServerRelativeUrl.Substring(0, lastSlashIndex);
            }
            webProperties["IsRootWeb"] = IsRootWeb;
            // The value of HasUniqueRoleDefinitions in RootWeb is true.
            webProperties["HasUniqueRoleDefinitions"] = IsRootWeb;
            // Add RootWeb Id
            webProperties["FirstUniqueRoleDefinitionWeb" + AveObjectModelConstant.ObjectPropertySuffix] = context.Site.RootWeb.Id;
            webProperties["Name"] = Name;
            webProperties["ParentWeb" + AveObjectModelConstant.ObjectPropertySuffix] = ParentWebServerRelativeUrl;

            webProperties["WebTemplateId"] = GetWebtemplateId(web.Language, 15, web.WebTemplate + "#" + web.Configuration, context);

            webProperties["AllProperties" + AveObjectModelConstant.ObjectPropertySuffix] = web.AllProperties.FieldValues;

            Dictionary<string, object> AssociatedMemberGroupProperties = GetGroupProperties(base.mSiteTrimObj, context, web.AssociatedMemberGroup, false);
            Dictionary<string, object> AssociatedOwnerGroupProperties = GetGroupProperties(base.mSiteTrimObj, context, web.AssociatedOwnerGroup, false);
            Dictionary<string, object> AssociatedVisitorGroupProperties = GetGroupProperties(base.mSiteTrimObj, context, web.AssociatedVisitorGroup, false);

            webProperties["AssociatedMemberGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedMemberGroupProperties;
            webProperties["AssociatedOwnerGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedOwnerGroupProperties;
            webProperties["AssociatedVisitorGroup" + AveObjectModelConstant.ObjectPropertySuffix] = AssociatedVisitorGroupProperties;

            if (!isAppWeb)
            {
                mRequestCommon.GetWebSearchAndOfflineAvailability(web.ServerRelativeUrl, webProperties, mObj);
            }
            return webProperties;
        }

        #region WebTemplate

        [ReplaceByAPI]
        protected override void GetWebTemplate(AveWebBrowserInfo info, Web web, AveClientContext context)
        {
            info.TemplateName = web.WebTemplate + "#" + web.Configuration;

            info.TemplateTitle = GetWebTemplateTitle(web.Language, info.TemplateName, context);
        }

        [ReplaceByAPI]
        private string GetWebTemplateTitle(uint language, string templateName, AveClientContext context)
        {
            var webTemplates = context.Site.GetWebTemplates(language, 15);
            var webTemplate = webTemplates.GetByName(templateName);
            context.Load(webTemplate, tempalte => tempalte.Title);
            context.ExecuteQuery();

            return webTemplate.Title;
        }

        [ReplaceByAPI]
        private int GetWebtemplateId(uint language, int compatLevel, string templateName, ClientContext context)
        {
            var webTemplates = context.Site.GetWebTemplates(language, compatLevel);
            var webTemplate = webTemplates.GetByName(templateName);
            context.Load(webTemplate, tempalte => tempalte.Id);
            context.ExecuteQuery();
            return webTemplate.Id;
        }


        [ReplaceByAPI]
        public override string GetWebTemplateTitle(string siteUrl, uint language, string templateName)
        {
            using (AveClientContext context = CreateContext())
            {
                return GetWebTemplateTitle(language, templateName, context);
            }
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource)
        {
            return base.GetWebTemplates(webServerRelativeUrl, lcid, doIncludeCrossLanguage, webtemplateSource);
        }
        #endregion

        [ReplaceByAPI]
        public override void RemoveThemeFromWeb(string webServerRelativeUrl, bool deleteFiles)
        {
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                var cssFolderUrl = string.Empty;
                if (deleteFiles)
                {
                    context.Load(web);
                    context.ExecuteQuery();
                    cssFolderUrl = web.ThemedCssFolderUrl;
                }
                web.ThemedCssFolderUrl = null;
                web.Update();
                context.ExecuteQuery();
                Folder folder = null;
                if (!string.IsNullOrEmpty(cssFolderUrl))
                {
                    try
                    {
                        if (IsSharedTheme(cssFolderUrl, web))
                        {
                            context.Load(context.Site.RootWeb);
                            context.ExecuteQuery();
                            folder = context.Site.RootWeb.GetFolderByServerRelativeUrl(cssFolderUrl);
                        }
                        else
                        {
                            folder = web.GetFolderByServerRelativeUrl(cssFolderUrl);
                        }
                        context.ExecuteQuery();
                        if (folder != null && folder.Exists)
                        {
                            folder.DeleteObject();
                            context.ExecuteQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("An error occourred while deleting theme folders. Error:{0}", e);
                    }
                }
            }
        }

        [NoAPI]
        public override void DeleteTag(string url, Guid termId)
        {
            base.DeleteTag(url, termId);
        }

        [NoAPI("Portal url, portal name")]
        public override Dictionary<string, object> UpdateSite(Dictionary<string, object> siteProperties)
        {
            return base.UpdateSite(siteProperties);
        }

        [NoAPI("UpdateWebSearchAndOfflineAvailability")]
        public override Dictionary<string, object> UpdateWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            return base.UpdateWeb(webServerRelativeUrl, webProperties);
        }

        protected override bool UpdateWebAccessRequestSetting(ClientContext context, Web web, Dictionary<string, object> webProperties)
        {

            var change = false;
            var UseAccessRequestDefault = webProperties.SafeGetAndRemoveProperty<bool>(WebPropertyNames.UseAccessRequestDefault);
            var RequestAccessEmail = webProperties.SafeGetAndRemoveProperty(WebPropertyNames.RequestAccessEmail);
            var AccessRequestSiteDescription = webProperties.SafeGetAndRemoveProperty(WebPropertyNames.AccessRequestSiteDescription);
            var MembersCanShare = webProperties.SafeGetAndRemoveProperty<bool>(WebPropertyNames.MembersCanShare);
            if (UseAccessRequestDefault.HasValue)
            {
                ConditionalScope conditionScope = new ConditionalScope(context, () => web.HasUniqueRoleAssignments, true);
                using (conditionScope.StartScope())
                {
                    web.SetUseAccessRequestDefaultAndUpdate(UseAccessRequestDefault.Value);
                }
                change = true;
            }
            if (RequestAccessEmail != null)
            {
                ConditionalScope conditionScope = new ConditionalScope(context, () => web.HasUniqueRoleAssignments, true);
                using (conditionScope.StartScope())
                {
                    web.RequestAccessEmail = RequestAccessEmail;
                }
                change = true;
            }
            if (MembersCanShare.HasValue)
            {
                ConditionalScope conditionScope = new ConditionalScope(context, () => web.HasUniqueRoleAssignments, true);
                using (conditionScope.StartScope())
                {
                    web.MembersCanShare = MembersCanShare.Value;
                }
                change = true;
            }
            if (AccessRequestSiteDescription != null)
            {
                web.SetAccessRequestSiteDescriptionAndUpdate(AccessRequestSiteDescription);
                change = true;
            }
            return change;
        }

        [ReplaceByAPI]
        protected override void UpdateWebRegionalSetting(string webServerRelativeUrl, Dictionary<string, object> properties)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //context.Load(web.RegionalSettings, rg => rg.TimeZone, rg => rg.TimeZones);
                if (properties.ContainsKey("TimeZoneChangedProperties"))
                    web.RegionalSettings.TimeZone = web.RegionalSettings.TimeZones.GetById((Convert.ToInt32((properties["TimeZoneChangedProperties"] as Dictionary<string, object>)["ID"])));
                //if (RSProperties.ContainsKey("Local"))
                //    web.RegionalSettings.LocaleId = Convert.ToUInt32(RSProperties["Local"]);
                //saas-23724,在regional setting里无法对locale进行修改，原因为上面那段把locale属性的修改注释掉了，properties中的属性应为localeId，所以更改为如下写法。
                if (properties.ContainsKey("LocaleId") || properties.ContainsKey("Local"))
                    web.RegionalSettings.LocaleId = properties.ContainsKey("LocaleId") ? Convert.ToUInt32(properties["LocaleId"]) : Convert.ToUInt32(properties["Local"]);
                if (properties.ContainsKey("Collation"))
                    web.RegionalSettings.Collation = Convert.ToInt16(properties["Collation"]);
                if (properties.ContainsKey("CalendarType"))
                    web.RegionalSettings.CalendarType = Convert.ToInt16(properties["CalendarType"]);
                if (properties.ContainsKey("ShowWeeks"))
                    web.RegionalSettings.ShowWeeks = Convert.ToBoolean(properties["ShowWeeks"]);
                if (properties.ContainsKey("AlternateCalendarType"))
                    web.RegionalSettings.AlternateCalendarType = Convert.ToInt16(properties["AlternateCalendarType"]);
                if (properties.ContainsKey("WorkDays"))
                    web.RegionalSettings.WorkDays = Convert.ToInt16(properties["WorkDays"]);
                if (properties.ContainsKey("FirstDayOfWeek"))
                    web.RegionalSettings.FirstDayOfWeek = Convert.ToUInt32(properties["FirstDayOfWeek"]);
                if (properties.ContainsKey("FirstWeekOfYear"))
                    web.RegionalSettings.FirstWeekOfYear = Convert.ToInt16(properties["FirstWeekOfYear"]);
                if (properties.ContainsKey("WorkDayStartHour"))
                    web.RegionalSettings.WorkDayStartHour = Convert.ToInt16(properties["WorkDayStartHour"]);
                if (properties.ContainsKey("WorkDayEndHour"))
                    web.RegionalSettings.WorkDayEndHour = Convert.ToInt16(properties["WorkDayEndHour"]);
                if (properties.ContainsKey("Time24"))
                    web.RegionalSettings.Time24 = Convert.ToBoolean(properties["Time24"]);
                if (properties.ContainsKey("AdjustHijriDays"))
                    web.RegionalSettings.AdjustHijriDays = Convert.ToInt16(properties["AdjustHijriDays"]);

                web.RegionalSettings.Update();
                context.ExecuteQuery();
            }
        }
        [NoAPI]
        public override List<Dictionary<string, object>> GetDisplayGroupsForSite()
        {
            return base.GetDisplayGroupsForSite();
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl)
        {
            Dictionary<string, object> webLogoProp = new Dictionary<string, object>();
            using (var clientContext = CreateContext())
            {
                var web = clientContext.Site.OpenWeb(webServerRelativeUrl);
                clientContext.Load(web, w => w.SiteLogoUrl, w => w.SiteLogoDescription);
                clientContext.ExecuteQuery();
                webLogoProp["SiteLogoUrl"] = web.SiteLogoUrl;
                webLogoProp["SiteLogoDescription"] = web.SiteLogoDescription;
            }
            return webLogoProp;
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetCustomListTemplates(string webServerRelativeUrl)
        {
            return base.GetCustomListTemplates(webServerRelativeUrl);
        }

        [NoAPI("Local没有实现此方法，当前发现CA对该方法有调用。")]
        public override Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource)
        {
            return base.GetAllFeatureDefinitions(Url, featuresSource);
        }

        [KeepOriginalWithAPI]
        public override bool DoesUserHavePermissions(string webServerRelativeUrl, ulong permissionMask)
        {
            return base.DoesUserHavePermissions(webServerRelativeUrl, permissionMask);
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> GetWebRegionalSetting(string webServerRelativeUrl)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> regionalSettingProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web);
                RegionalSettings regionalSettings = web.RegionalSettings;
                context.Load(regionalSettings);
                context.Load(regionalSettings.TimeZone);
                context.Load(regionalSettings.InstalledLanguages);
                context.ExecuteQuery();
                CopyProperty(regionalSettingProperties, regionalSettings);
                regionalSettingProperties["InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix] = AssembleInstalledLanguages(regionalSettings.InstalledLanguages);
                Dictionary<string, object> timeZoneProperties = new Dictionary<string, object>();
                CopyProperty(timeZoneProperties, regionalSettings.TimeZone);
                timeZoneProperties["ID"] = Convert.ToUInt16(regionalSettings.TimeZone.Id);
                if (timeZoneProperties.ContainsKey("Id"))
                {
                    timeZoneProperties.Remove("Id");
                }
                regionalSettingProperties["TimeZone" + AveObjectModelConstant.ObjectPropertySuffix] = timeZoneProperties;
                return regionalSettingProperties;
            }
        }
        private Dictionary<string, object> AssembleInstalledLanguages(LanguageCollection languages)
        {
            Dictionary<string, object> container = new Dictionary<string, object>();
            var list = new List<Dictionary<string, object>>();
            container.Add(AveObjectModelConstant.ChildrenProperties, list);

            foreach (Language language in languages)
            {
                Dictionary<string, object> languageDict = new Dictionary<string, object>();
                languageDict["DisplayName"] = language.DisplayName;
                languageDict["LCID"] = language.Lcid;
                list.Add(languageDict);
            }

            return container;
        }
        [NoAPI]
        public override Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            return base.GetDefaultRegionalSetting(webServerRelativeUrl, lcid);
        }

        [ReplaceByAPI]
        public override bool GetDenyAddAndCustomizePagesStatus()
        {
            var tenantSiteUrl = string.Empty;
            try
            {
                tenantSiteUrl = GetSPOAdminUrl(mWebUrl);
                using (AveClientContext context = CreateContext(tenantSiteUrl))
                {
                    Tenant tenant = new Tenant(context);
                    var newSiteProps = tenant.GetSitePropertiesFromSharePointByFilter(string.Format("Url -eq '{0}'", mWebUrl), "0", true);
                    context.Load(newSiteProps);
                    context.ExecuteQuery();
                    var properties = newSiteProps.FirstOrDefault();
                    if (properties != null)
                    {
                        return properties.DenyAddAndCustomizePages == DenyAddAndCustomizePagesStatus.Enabled;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get site DenyAddAndCustomizePages info error . Url : {0}  Error : {1}", tenantSiteUrl, ex.ToString());
            }
            return false;
        }

        [ReplaceByAPI]
        public override void SetDenyAddAndCustomizePagesStatus(bool status)
        {
            var tenantSiteUrl = string.Empty;
            tenantSiteUrl = GetSPOAdminUrl(mWebUrl);
            using (AveClientContext context = CreateContext(tenantSiteUrl))
            {
                Tenant tenant = new Tenant(context);
                SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                context.Load(sp, p => p.DenyAddAndCustomizePages);
                context.ExecuteQuery();
                sp.DenyAddAndCustomizePages = status ? DenyAddAndCustomizePagesStatus.Enabled : DenyAddAndCustomizePagesStatus.Disabled;
                sp.Update();
                context.ExecuteQuery();
            }
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl, int compatibilityLevel)
        {
            using (var clientContext = CreateContext())
            {
                var web = clientContext.Site.OpenWeb(webServerRelativeUrl);
                clientContext.Load(web, w => w.ThemedCssFolderUrl);
                clientContext.ExecuteQuery();
                return new Dictionary<string, object> { { "ThemedCssFolderUrl", web.ThemedCssFolderUrl } };
            }
        }
        [NoAPI]
        public override Dictionary<string, object> GetThmxThemeInfo(string webServerRelativeUrl)
        {
            return base.GetThmxThemeInfo(webServerRelativeUrl);
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            using (var clientContext = CreateContext())
            {
                var web = clientContext.Site.OpenWeb(webServerRelativeUrl);
                clientContext.Load(web, w => w.AlternateCssUrl);
                clientContext.ExecuteQuery();
                return new Dictionary<string, object> { { "AlternateCssUrl", web.AlternateCssUrl } };
            }
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> OpenThmxTheme(string fileServerRelativeUrl)
        {
            return base.OpenThmxTheme(fileServerRelativeUrl);
        }
        [NoAPI("CSOM Site对象没有SyndicationEnabled属性，但Web上有。")]
        public override bool GetSiteRssSetting()
        {
            return base.GetSiteRssSetting();
        }
        [ReplaceByAPI]
        public override string GetWebTemplateConfiguration(string webRelativeUrl)
        {
            try
            {
                using (var clientContext = CreateContext())
                {
                    var web = clientContext.Site.OpenWeb(webRelativeUrl);
                    clientContext.Load(web, w => w.WebTemplate, w => w.Configuration);
                    clientContext.ExecuteQuery();
                    return string.Format("{0}#{1}", web.WebTemplate, web.Configuration.ToString());
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Get Web Template Configuration Error. Web:{0} Exception Message:{1}", webRelativeUrl, e);
                return string.Empty;
            }
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFirstUniqueNavigationWeb(string webServerRelativeUrl)
        {
            return base.GetFirstUniqueNavigationWeb(webServerRelativeUrl);
        }

        [NoAPI]
        public override Dictionary<string, object> GetManagedSitecollectionData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                context.Load(tenant);
                context.ExecuteQuery();

                AveObjectCopy.GetObjectBasicProperties(data, tenant);

                //SP Online 上available的值是storage减去所有站点已使用的值
                //在这里获取所有站点已使用的值
                int startIndex = 0;
                long storageUsage = 0;
                while (startIndex != -1)
                {
                    var sitesProperties = tenant.GetSitePropertiesFromSharePoint(startIndex.ToString(), true);
                    context.Load(sitesProperties, p => p.IncludeWithDefaultProperties(s => s.StorageUsage), p => p.NextStartIndex);
                    context.ExecuteQuery();
                    foreach (var siteProperties in sitesProperties)
                    {
                        storageUsage += siteProperties.StorageUsage;
                    }
                    startIndex = sitesProperties.NextStartIndex;
                }
                data["StorageUsage"] = storageUsage;
                if (mRequestCommon is AveHttpWebRequestCommonEmpty)
                {
                    GenerateManagedSiteCollectionData(data);
                }
                else
                {
                    GetManagedSiteCollectionData(data, mWebUrl, tenant);
                }
            }
            return data;
        }

        /// <summary>
        /// 由于APPToken不支持httprequest,当前这部分数据写死，如果以后有API支持了，再替换成API
        /// </summary>
        /// <param name="managedData"></param>
        private void GenerateManagedSiteCollectionData(Dictionary<string, object> managedData)
        {
            #region Add Lanauages

            var languageList = new List<Dictionary<string, object>> {
                new Dictionary<string, object> { { "DisplayName" , "Arabic" }, { "LCID", 1025 } },
                new Dictionary<string, object> { { "DisplayName", "Azerbaijani" }, { "LCID", 1068 }},
                new Dictionary<string, object> { { "DisplayName", "Basque" }, { "LCID", 1069 }},
                new Dictionary<string, object> { { "DisplayName", "Bosnian (Latin)" }, { "LCID", 5146 }},
                new Dictionary<string, object> { { "DisplayName", "Bulgarian" }, { "LCID", 1026 }},
                new Dictionary<string, object> { { "DisplayName", "Catalan" }, { "LCID", 1027 }},
                new Dictionary<string, object> { { "DisplayName", "Chinese (Simplified)" }, { "LCID", 2052 } },
                new Dictionary<string, object> { { "DisplayName", "Chinese (Traditional)" }, { "LCID", 1028 } },
                new Dictionary<string, object> { { "DisplayName", "Croatian" }, { "LCID", 1050 } },
                new Dictionary<string, object> { { "DisplayName", "Czech" }, { "LCID", 1029 } },
                new Dictionary<string, object> { { "DisplayName", "Danish" }, { "LCID", 1030 } },
                new Dictionary<string, object> { { "DisplayName", "Dari" }, { "LCID", 1164 } },
                new Dictionary<string, object> { { "DisplayName", "Dutch" }, { "LCID", 1043 } },
                new Dictionary<string, object> { { "DisplayName", "English" }, { "LCID", 1033 } },
                new Dictionary<string, object> { { "DisplayName", "Estonian" }, { "LCID", 1061 } },
                new Dictionary<string, object> { { "DisplayName", "Finnish" }, { "LCID", 1035 }  },
                new Dictionary<string, object> { { "DisplayName", "French" }, { "LCID", 1036 } },
                new Dictionary<string, object> { { "DisplayName", "Galician" }, { "LCID", 1110 }},
                new Dictionary<string, object> { { "DisplayName", "German" }, { "LCID", 1031 } },
                new Dictionary<string, object> { { "DisplayName", "Greek" }, { "LCID", 1032 } },
                new Dictionary<string, object> { { "DisplayName", "Hebrew" }, { "LCID", 1037 }},
                new Dictionary<string, object> { { "DisplayName", "Hindi" }, { "LCID", 1081 } },
                new Dictionary<string, object> { { "DisplayName", "Hungarian" }, { "LCID", 1038 } },
                new Dictionary<string, object> { { "DisplayName", "Indonesian" }, { "LCID", 1057 }  },
                new Dictionary<string, object> { { "DisplayName", "Irish" }, { "LCID", 2108 } },
                new Dictionary<string, object> { { "DisplayName", "Italian" }, { "LCID", 1040 } },
                new Dictionary<string, object> { { "DisplayName", "Japanese" }, { "LCID", 1041 }},
                new Dictionary<string, object> { { "DisplayName", "Kazakh" }, { "LCID", 1087 }},
                new Dictionary<string, object> { { "DisplayName", "Korean" }, { "LCID", 1042 }  },
                new Dictionary<string, object> { { "DisplayName", "Latvian" }, { "LCID", 1062 }  },
                new Dictionary<string, object> { { "DisplayName", "Lithuanian" }, { "LCID", 1063 }},
                new Dictionary<string, object> { { "DisplayName", "Macedonian" }, { "LCID", 1071 } },
                new Dictionary<string, object> { { "DisplayName", "Malay" }, { "LCID", 1086 } },
                new Dictionary<string, object> { { "DisplayName", "Norwegian (Bokmål)" }, { "LCID", 1044 } },
                new Dictionary<string, object> { { "DisplayName", "Polish" }, { "LCID", 1045 } },
                new Dictionary<string, object> { { "DisplayName", "Portuguese (Brazil)" }, { "LCID", 1046 }},
                new Dictionary<string, object> { { "DisplayName", "Portuguese (Portugal)" }, { "LCID", 2070 } },
                new Dictionary<string, object> { { "DisplayName", "Romanian" }, { "LCID", 1048 } },
                new Dictionary<string, object> { { "DisplayName", "Russian" }, { "LCID", 1049 } },
                new Dictionary<string, object> { { "DisplayName", "Serbian (Cyrillic, Serbia)" }, { "LCID", 10266 }},
                new Dictionary<string, object> { { "DisplayName", "Serbian (Latin, Serbia)" }, { "LCID", 9242 } },
                new Dictionary<string, object> { { "DisplayName", "Slovak" }, { "LCID", 1051 } },
                new Dictionary<string, object> { { "DisplayName", "Slovenian" }, { "LCID", 1060 }},
                new Dictionary<string, object> { { "DisplayName", "Spanish" }, { "LCID", 3082 }},
                new Dictionary<string, object> { { "DisplayName", "Swedish" }, { "LCID", 1053 }},
                new Dictionary<string, object> { { "DisplayName", "Thai" }, { "LCID", 1054 } },
                new Dictionary<string, object> { { "DisplayName", "Turkish" }, { "LCID", 1055 } },
                new Dictionary<string, object> { { "DisplayName", "Ukrainian" }, { "LCID", 1058 } },
                new Dictionary<string, object> { { "DisplayName", "Vietnamese" }, { "LCID", 1066 }},
                new Dictionary<string, object> { { "DisplayName", "Welsh" }, { "LCID", 1106 } },};

            Dictionary<string, object> languages = new Dictionary<string, object>();
            languages.Add(AveObjectModelConstant.ChildrenProperties, languageList);
            managedData["InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix] = languages;
            #endregion

            #region  Add Prefixes
            var prefixList = new List<Dictionary<string, object>> { new Dictionary<string, object> { { "Name", "/sites/" } }, new Dictionary<string, object> { { "Name", "/teams/" } } };
            Dictionary<string, object> prefixs = new Dictionary<string, object>();
            prefixs.Add(AveObjectModelConstant.ChildrenProperties, prefixList);
            managedData["Prefixes" + AveObjectModelConstant.ObjectPropertySuffix] = prefixs;
            #endregion
        }

        [NoAPI]
        private void GetManagedSiteCollectionData(Dictionary<string, object> managedData, string adminUrl, Tenant tenant)
        {
            if (mRequestCommon != null)
            {
                mRequestCommon.GetManagedSiteCollectionData(managedData, adminUrl, tenant.StorageQuota - tenant.StorageQuotaAllocated, tenant.ResourceQuota - tenant.ResourceQuotaAllocated);
            }
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetWebChangesByQuery(string webServerRelativeUrl, Dictionary<string, object> queryProps)
        {
            return base.GetWebChangesByQuery(webServerRelativeUrl, queryProps);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetSiteChangesByQuery(Dictionary<string, object> queryProps)
        {
            return base.GetSiteChangesByQuery(queryProps);
        }

        [NoAPI]
        public override void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            base.SetSiteEnabledHelpCollections(enabledHelpCollections);
        }
        [NoAPI]
        public override Dictionary<string, object> CreateScopeDisPlayGroup(string name, string description, Uri owningSiteUrl, bool displayInAdminUI)
        {
            return base.CreateScopeDisPlayGroup(name, description, owningSiteUrl, displayInAdminUI);
        }
        [NoAPI]
        public override Dictionary<string, object> CreateScope(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, string compilationType, string filter)
        {
            return base.CreateScope(name, description, owningSiteUrl, displayInAdminUI, alternateResultsPage, compilationType, filter);
        }
        [NoAPI("No API to active anad deactive solution.")]
        public override Dictionary<string, object> OperateSolution(string operation, string siteUrl, string webServerRelativeUrl, int id)
        {
            return base.OperateSolution(operation, siteUrl, webServerRelativeUrl, id);
        }
        [KeepOriginalWithAPI]
        public override void ApplyWebTemplate(string webUrl, string webTemplate)
        {
            base.ApplyWebTemplate(webUrl, webTemplate);
        }
        [NoAPI("No API to publish Infopath Form library.")]
        public override void PublishSharepointList(string webServerRelativeUrl, IAveFile templateFile, int lcid, string listId, string contentTypeId)
        {
            base.PublishSharepointList(webServerRelativeUrl, templateFile, lcid, listId, contentTypeId);
        }
        [KeepOriginalWithAPI]
        public override bool DeleteMigrationJob(Guid id)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.DeleteMigrationJob(id);
                context.ExecuteQuery();
                return result.Value;
            }
        }
        [KeepOriginalWithAPI]
        public override AveMigrationJobState GetMigrationJobStatus(Guid id)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.GetMigrationJobStatus(id);
                context.ExecuteQuery();
                return (AveMigrationJobState)result.Value;
            }
        }
        [NoAPI]
        [TODO("No API to reset Master page setting, Need consider update web properties to instead of API.")]
        public override void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            base.RestoreMasterPage(webServerRelativeUrl, siteServerRelativeUrl, pageInfo, alternateCssUrl);
        }
        [NoAPI("当源端是10站点时，需要使用web service还原，这部分不支持，影响migration。")]
        public override void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            base.RestoreTheme(webServerRelativeUrl, siteServerRelativeUrl, webSettingInfo, themedCssFolderUrl);
        }
        //[NoAPI]
        //public override List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList)
        //{
        //    return base.RestoreFeatures(webServerRelativeUrl, force, scope, featuresSource, featureInfoList);
        //}

        public override List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);

                switch (featuresSource)
                {
                    case "web.features":
                        return RestoreWebFeatures(context, webServerRelativeUrl, force, scope, featureInfoList);
                    case "site.features":
                        return RestoreSiteFeatures(context, webServerRelativeUrl, force, scope, featureInfoList);
                    default:
                        throw new NotImplementedException(string.Format("The scope:{0} is not supported", featuresSource));
                }
            }
        }

        public List<Dictionary<string, object>> RestoreSiteFeatures(ClientContext context, string webServerRelativeUrl, bool force, int scope, List<Dictionary<string, object>> featureInfoList)
        {
            FeatureCollection collection = context.Site.Features;
            FeatureDefinitionScope featureDefScope = FeatureDefinitionScope.Site;

            context.Load(collection, f => f.Include(a => a.DefinitionId));
            context.ExecuteQuery();

            HashSet<Guid> activedFeatures = new HashSet<Guid>();
            foreach (var featureDef in collection)
            {
                activedFeatures.Add(featureDef.DefinitionId);
            }

            List<Dictionary<string, object>> featuresProperties = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> featureInfo in featureInfoList)
            {
                try
                {
                    foreach (Guid id in featureInfo["Dependences"] as List<Guid>)
                    {
                        RestoreFeature(context, collection, id, force, scope, featureDefScope, activedFeatures);
                    }
                    Dictionary<string, object> featureProp = new Dictionary<string, object>();
                    Guid featureId = new Guid(featureInfo["ID"].ToString());
                    featureProp = RestoreFeature(context, collection, featureId, force, scope, featureDefScope, activedFeatures);
                    featuresProperties.Add(featureProp);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLogger.Error("Add Feature to {0}:{1} failed.Error Message:{2}", featureDefScope, webServerRelativeUrl, ex);
                }
            }
            return featuresProperties;
        }

        public List<Dictionary<string, object>> RestoreWebFeatures(ClientContext context, string webServerRelativeUrl, bool force, int scope, List<Dictionary<string, object>> featureInfoList)
        {
            FeatureCollection siteFeatures = context.Site.Features;
            FeatureCollection collection = context.Web.Features;
            FeatureDefinitionScope featureDefScope = FeatureDefinitionScope.Web;

            context.Load(siteFeatures, f => f.Include(a => a.DefinitionId));
            context.Load(collection, f => f.Include(a => a.DefinitionId));
            context.ExecuteQuery();

            HashSet<Guid> activedFeatures = new HashSet<Guid>();
            foreach (var featureDef in collection)
            {
                activedFeatures.Add(featureDef.DefinitionId);
            }

            HashSet<Guid> siteActivedFeatures = new HashSet<Guid>();
            foreach (var featureDef in siteFeatures)
            {
                siteActivedFeatures.Add(featureDef.DefinitionId);
            }

            List<Dictionary<string, object>> featuresProperties = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> featureInfo in featureInfoList)
            {
                try
                {
                    foreach (Guid id in featureInfo["Dependences"] as List<Guid>)
                    {
                        object featureSourceObj;
                        if (featureInfo.TryGetValue("FeatureSource", out featureSourceObj) && featureSourceObj != null)
                        {
                            if ("site.features".Equals(featureSourceObj.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                RestoreFeature(context, siteFeatures, id, force, scope, FeatureDefinitionScope.Site, siteActivedFeatures);
                            }
                            else
                            {
                                RestoreFeature(context, collection, id, force, scope, featureDefScope, activedFeatures);
                            }
                        }
                        else
                        {
                            RestoreFeature(context, collection, id, force, scope, featureDefScope, activedFeatures);
                        }
                    }
                    Dictionary<string, object> featureProp = new Dictionary<string, object>();
                    Guid featureId = new Guid(featureInfo["ID"].ToString());
                    featureProp = RestoreFeature(context, collection, featureId, force, scope, featureDefScope, activedFeatures);
                    featuresProperties.Add(featureProp);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLogger.Error("Add Feature to {0}:{1} failed.Error Message:{2}", featureDefScope, webServerRelativeUrl, ex);
                }
            }
            return featuresProperties;
        }

        private Dictionary<string, object> RestoreFeature(ClientContext context, FeatureCollection featureCollection, Guid featureId, bool force, int scope, FeatureDefinitionScope featureDefinitionScope, HashSet<Guid> activedFeatures)
        {
            Dictionary<string, object> featureProp = null;
            if (!activedFeatures.Contains(featureId))
            {
                int times = 2;
                ///测试过程中发现，对于sanbox solution，scope就是对应的scope，而SPO built 的feature都是farm的scope，但是备份的时候无法得知具体的scope，
                ///所以这里先使用farm进行尝试，然后再使用对应的scope。
                ///按照PNP的guide，可能需要等待一段时间来获取feature是否active上，比如publishing feature，这个先看看测试结果是否需要添加monitoring
                var defaultScope = FeatureDefinitionScope.Farm;
                featureProp = new Dictionary<string, object>();
                while (times > 0)
                {
                    times--;
                    try
                    {
                        mLogger.Info($"Start to restore feature: {featureId}, scope: {defaultScope}");
                        var feature = featureCollection.Add(featureId, force, defaultScope);

                        context.ExecuteQuery();
                        activedFeatures.Add(featureId);
                        featureProp["DefinitionId"] = featureId;
                        Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
                        featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
                        break;
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("failed to activate feature: {0} -> {1} due to: {2}", featureId, defaultScope, e);

                        defaultScope = featureDefinitionScope;
                    }
                }
            }

            return featureProp;
        }

        public override List<AveComplianceTagInfo> GetAvailableTagsForSite(string siteUrl)
        {
            using (ClientContext context = CreateContext(siteUrl))
            {
                List<AveComplianceTagInfo> AvailableTags = new List<AveComplianceTagInfo>();
                var availableComplianceTags = SPPolicyStoreProxy.GetAvailableTagsForSite(context, siteUrl);
                context.ExecuteQuery();
                foreach (var complianceTag in availableComplianceTags)
                {
                    var info = new AveComplianceTagInfo();
                    info.AcceptMessagesOnlyFromSendersOrMembers = complianceTag.AcceptMessagesOnlyFromSendersOrMembers;
                    info.AccessType = complianceTag.AccessType;
                    info.AllowAccessFromUnmanagedDevice = complianceTag.AllowAccessFromUnmanagedDevice;
                    info.AutoDelete = complianceTag.AutoDelete;
                    info.BlockDelete = complianceTag.BlockDelete;
                    info.BlockEdit = complianceTag.BlockEdit;
                    info.ContainsSiteLabel = complianceTag.ContainsSiteLabel;
                    info.DisplayName = complianceTag.DisplayName;
                    info.EncryptionRMSTemplateId = complianceTag.EncryptionRMSTemplateId;
                    info.HasRetentionAction = complianceTag.HasRetentionAction;
                    info.IsEventTag = complianceTag.IsEventTag;
                    info.Notes = complianceTag.Notes;
                    info.RequireSenderAuthenticationEnabled = complianceTag.RequireSenderAuthenticationEnabled;
                    info.ReviewerEmail = complianceTag.ReviewerEmail;
                    info.SharingCapabilities = complianceTag.SharingCapabilities;
                    info.SuperLock = complianceTag.SuperLock;
                    info.TagDuration = complianceTag.TagDuration;
                    info.TagId = complianceTag.TagId;
                    info.TagName = complianceTag.TagName;
                    info.TagRetentionBasedOn = complianceTag.TagRetentionBasedOn;
                    AvailableTags.Add(info);
                }
                return AvailableTags;
            }
        }
    }
}
