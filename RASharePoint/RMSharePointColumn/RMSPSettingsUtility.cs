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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using ServerFilterPolicy = AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.JobMonitor;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using System.Xml;
using System.IO;
using AvePoint.Wrapper.Common.Office;
using AvePoint.RA.CommonUtil;
using System.Threading;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Common.Throttle;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.I18N.Core;
using System.Net;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.SPObjDiscover;
using System.Reflection;
using AvePoint.RA.SharePoint.Extension;
using Newtonsoft.Json;
using RMContract.SharePoint;
using System.Globalization;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.SharePoint.Object;
using Microsoft.SharePoint.Client.RecordsRepository;
using AutoMapper;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.JobMessage;
using Aspose.Pdf.Operators;
using Microsoft.Extensions.Logging.Abstractions;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class SPSettingsUtility
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(SPSettingsUtility));
        private static readonly string BCSColumnInternalName = "RevIMBCS";
        private static readonly string BCSPropertyName = "RevIM";
        private static readonly Guid BCSColumnID = new Guid("20f84bba906045b4af568ee102a52dcb");

        private static readonly Guid RelatedColumnId = new Guid("b40273fb-26d2-40e8-9a34-dd20bc9ca1d7");
        private static readonly Guid RevIMUniqueIDColumnID = new Guid("40f84bba906045b4af568ee102a52dcb");
        private static readonly string RelatedColumnInternalName = "RecordsRelated";
        private static readonly string FileArchiveStatusInternalName = "_FileArchiveStatus";
        private const string DATETIME_ISO_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private static readonly string CSDClassName_EN = "CSD Class";
        private static readonly string CSDClassName_DE = "KSU Klasse";
        private static readonly string CSDClassName_ES = "Clase CSD";
        private static readonly string CSDClassName_HU = "CSD Osztály";
        private static readonly string CSDClassName_PT = "Classe CSD";
        private static readonly string CSDClassName_CS = "Třída KSU";
        private static readonly Dictionary<string, string> CSDClassNameAndCultureMapping = new Dictionary<string, string>()
        {
            { "en-US", CSDClassName_EN },
            { "de-DE", CSDClassName_DE },
            { "es-ES", CSDClassName_ES },
            { "hu-HU", CSDClassName_HU },
            { "pt-PT", CSDClassName_PT },
            { "cs-CZ", CSDClassName_CS },
        };
        private static ITermSetDao TermSetDao = new TermSetDao();
        private static ITermDao TermDao = new TermDao();
        private static IContainerDao ContainerDao = new ContainerDao();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static IRMMLTrainingModelDao TrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        public static AveObjectModelFactory factoryForAuto;
        public static IJobMonitorService JobService { get; set; }
        public static bool HasFailedReport = false;
        private static CallLimiter _callLimiter;

        public static RMBrowseTreeNodeSourceType sourceType = RMBrowseTreeNodeSourceType.SharepointOnline;
        public static Guid teamsId = Guid.Empty;
        public static NodeLevel currentSiteCollectionLevel = NodeLevel.O365GroupSites;
        //public static AveDiscoveryOMFactory discoverFactoryForAuto;
        //protected static IReportService ReportService;//Add for config physical ,need rebuild next version.
        protected static IRMReportManager ReportManager
        {
            get
            {
                return ReportMangerFactory.Instance.ReportManager;
            }
        }
        private static int itemsPerTask = 200;
        private static int smartAutoCacheitemsPerTask = 50;
        static SPSettingsUtility()
        {
            //if (JobContext.Current.ReportManager != null)
            //{
            //    ReportService = JobContext.Current.ReportManager.Create();
            //}
            JobService = PlatformWindsorManager.GetService(typeof(IJobMonitorService)) as IJobMonitorService;
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_APPLY_SETTINGS_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out itemsPerTask);
            }
            var callLimitPerSecond = RMGlobalConfiguration.AppConfig.GetNumberValue(RMAppSettingKey.SPO_CALL_LIMIT_PER_SECOND, 1000);
            _callLimiter = CallLimiterFactory.CreateInstance("SPOCalllimiter", callLimitPerSecond);
            logger.Info($"SPOApplySettingsItemsPerTask : {itemsPerTask}, SPOCallLimitPerSecond : {callLimitPerSecond}");
        }

        public SPSettingsUtility()
        {

        }
        private static string RelatedColumnDisplayName
        {
            get
            {
                return I18NEntity.GetString("RM_SS_RelatedRecords");
            }
        }


        #region config bcs classification column
        public static SettingResult ConfigBCSColumn(IAveSite site, RMSharePointSetting setting, ref IAveTaxonomyField taxField, ConfigSiteSetting configSiteSetting = null)
        {
            using (var scope = new PerformanceScope("RMSPSettingUtility.ConfigBCSColumn4SiteCollection", $"RMSPSettingUtility.ConfigBCSColumn4SiteCollection{site.Url}", true))
            {
                logger.Info($"FullPath:[{setting.FullPath}] IsUsingExistColumn:[{setting.IsUsingExistColumnName}] ExistingCoumnName:[{setting.ExistColumnName}] Configure term settings in Records:[{setting.SetDocLevelTermForExistColumn}]");
                SettingResult result = SettingResult.None;
                if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    if (!CheckClassificationSetting(setting, site))
                    {
                        throw new Exception("Term Is Unavailable");
                    }
                    //Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
                    IAveTaxonomyField siteField = null;
                    FieldConflict conflict = VerifyFieldConflict(site, site.RootWeb.Fields, setting, ref siteField);
                    logger.Debug($"config site column, conflict:{conflict}");
                    result = HandleSiteFieldConflict(conflict, site, setting, ref siteField, true, configSiteSetting);
                    taxField = siteField;
                }
                else
                {
                    //RECO-2574
                    //if (setting.IsUsingExistColumnName)
                    //{
                    //    return SettingResult.UseExistSkip;
                    //}
                    try
                    {
                        var siteField = site.RootWeb.Fields.GetFieldById(BCSColumnID, false);
                        if (siteField != null)
                        {
                            taxField = siteField as IAveTaxonomyField;
                            var siteTextField = site.RootWeb.Fields.GetFieldById(taxField.TextField, false);
                            if (siteTextField != null)
                            {
                                try
                                {
                                    if (siteTextField.Hidden)
                                    {
                                        siteTextField.Hidden = false;
                                        siteTextField.Update();
                                    }
                                    siteTextField.Delete();
                                    siteTextField.Update();
                                }
                                catch (Exception)
                                {
                                    if (!siteTextField.Hidden)
                                    {
                                        siteTextField.Hidden = true;
                                        siteTextField.Update();
                                        logger.Info("Reset siteTextField hidden to true.");
                                    }
                                    throw;
                                }
                            }
                            else
                            {
                                try
                                {
                                    logger.Warn("can't get taxonomy field's note field, will get note field by internal name [i0f84bba906045b4af568ee102a52dcb], url:{0}", site.Url);
                                    var siteTextFieldByName = site.RootWeb.Fields.GetFieldByInternalName("i0f84bba906045b4af568ee102a52dcb");
                                    siteTextFieldByName.Delete();
                                    siteTextFieldByName.Update();
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("get note field by internal name [i0f84bba906045b4af568ee102a52dcb], error {0}", e.ToString());
                                }
                            }

                            siteField.Delete();
                            siteField.Update();
                            result = SettingResult.Delete;
                        }
                        else
                        {
                            result = SettingResult.SkipDelete;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("remove column, url:{0}; error: {1}", site.Url, e.ToString());
                        result = SettingResult.Delete;
                    }
                }
                return result;
            }
        }

        private static bool NeedUpdateBCSColumn(IAveTaxonomyField taxField, RMSharePointSetting setting, Guid termStoreId, bool isSiteLevel, out bool needUpdateView, bool withoutCheckDefaultValue = false)
        {
            bool result = false;
            needUpdateView = false;
            string columnName = setting.IsUsingExistColumnName ? setting.ExistColumnName : setting.ColumnName;
            if (!setting.IsUsingExistColumnName && !taxField.Title.Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                result = true;
            }
            if (taxField.SspId != termStoreId)
            {
                result = true;
            }
            if (taxField.EnforceUniqueValues == true)
            {
                result = true;
            }
            if (taxField.AllowMultipleValues == true)
            {
                result = true;
            }
            if (taxField.TermSetId != setting.TermSetId)
            {
                result = true;
            }
            if (taxField.Indexed == false)
            {
                result = true;
            }
            if (!setting.IsUsingExistColumnName)
            {
                bool requiredInDB = setting.ColumnRequired == null ? true : (bool)setting.ColumnRequired;
                if (taxField.Required != requiredInDB)
                {
                    result = true;
                }
                bool hiddenInDB = setting.ColumnHidden == null ? false : (bool)setting.ColumnHidden;
                if (taxField.Hidden != hiddenInDB && !isSiteLevel)
                {
                    needUpdateView = true;
                    result = true;
                }
                if (!string.IsNullOrEmpty(taxField.Description) || !string.IsNullOrEmpty(setting.Description))
                {
                    if (!taxField.Description.Equals(setting.Description, StringComparison.OrdinalIgnoreCase))
                    {
                        result = true;
                    }
                }
            }
            if (taxField.IsPathRendered != setting.IsDisplyaTermPath)
            {
                result = true;
            }
            if (taxField.AnchorId != setting.TermId)
            {
                result = true;
            }
            if (!withoutCheckDefaultValue)
            {
                switch ((DeployTermMethod)setting.DeployTermMethod)
                {
                    case DeployTermMethod.UseDefaultTerm:
                        if (setting.DefaultTermId != Guid.Empty &&
                            (string.IsNullOrEmpty(taxField.DefaultValue)
                            || !taxField.DefaultValue.Contains(setting.DefaultTermId.ToString())
                            || (!isSiteLevel && taxField.DefaultValue.StartsWith("-1"))))
                        {
                            result = true;
                        }
                        break;
                    case DeployTermMethod.UseAutoClassification:
                        if (!JobContext.IsCSDTenant)
                        {
                            if (!string.IsNullOrEmpty(taxField.DefaultValue))
                            {
                                result = true;
                            }
                        }
                        break;
                    case DeployTermMethod.NoDefaultTerm:
                        if (!string.IsNullOrEmpty(taxField.DefaultValue))
                        {
                            result = true;
                        }
                        break;
                    case DeployTermMethod.UseIntelligenceClassification:
                        if (!string.IsNullOrEmpty(taxField.DefaultValue))
                        {
                            result = true;
                        }
                        break;
                    default:
                        break;
                }
            }
            //if (!taxField.DefaultValue.Contains(setting.DefaultTermId.ToString()))
            //{
            //    result = true;
            //}
            if (JobContext.IsCSDTenant && !setting.IsUsingExistColumnName)
            {
                if (!taxField.ReadOnlyField)
                {
                    result = true;
                }
                if (!taxField.ShowInVersionHistory)
                {
                    result = true;
                }
            }
            return result;
        }
        protected static bool CheckClassificationSetting(RMSharePointSetting setting, IAveSite site)
        {
            if (!setting.IsUsingExistColumnName || (setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn))
            {
                if (!ValidateTermIds(site, setting.TermSetId, setting.TermId, setting.DefaultTermId))
                {
                    return false;
                }
            }
            return true;
        }
        protected static bool ValidateTermIds(IAveSite site, Guid termSetId, Guid termId, Guid defaultTermId)
        {
            using (var scope = new PerformanceScope("RMSPSettingUtility.ValidateTermIds", $"RMSPSettingUtility.ValidateTermIds:{termSetId}-{termId}-{defaultTermId}", true))
            {
                bool result = true;
                try
                {
                    IAveTermStore termStore = site.AveSPTaxonomySession.TermStores[0];
                    var termSet = termStore.GetTermSet(termSetId);
                    if (termSet == null)
                    {
                        result = false;
                    }
                    if (termId != null && termId != Guid.Empty)
                    {
                        var rmTerm = TermDao.GetRMTermByGuId(termId);
                        if (rmTerm == null || rmTerm.IsDeprecated || rmTerm.IsRemoved)
                        {
                            result = false;
                        }
                        AvePoint.GCommon.Utility.ArgumentCheck.NotNull(rmTerm, nameof(rmTerm));
                        if (rmTerm.TermExpirationFrom != 0 || rmTerm.TermExpirationTo != 0)
                        {
                            if (DateTime.UtcNow.Ticks < rmTerm.TermExpirationFrom || (rmTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > rmTerm.TermExpirationTo))
                            {
                                return false;
                            }
                        }
                        var term = termStore.GetTerm(termId);
                        if (term == null || term.IsDeprecated)
                        {
                            result = false;
                        }
                    }
                    if (defaultTermId != null && defaultTermId != Guid.Empty)
                    {
                        var defaultRmTerm = TermDao.GetRMTermByGuId(defaultTermId);
                        if (defaultRmTerm == null || defaultRmTerm.IsDeprecated || defaultRmTerm.IsRemoved)
                        {
                            result = false;
                        }
                        AvePoint.GCommon.Utility.ArgumentCheck.NotNull(defaultRmTerm, nameof(defaultRmTerm));
                        if (defaultRmTerm.TermExpirationFrom != 0 || defaultRmTerm.TermExpirationTo != 0)
                        {
                            if (DateTime.UtcNow.Ticks < defaultRmTerm.TermExpirationFrom || (defaultRmTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > defaultRmTerm.TermExpirationTo))
                            {
                                return false;
                            }
                        }
                        var defaultTerm = termStore.GetTerm(defaultTermId);
                        if (defaultTerm == null || defaultTerm.IsDeprecated)
                        {
                            result = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Validate term failed {0}", e.ToString());
                    result = false;
                }
                return result;
            }
        }
        public static SettingResult ConfigBCSColumn(IAveSite site, IAveList list, RMSharePointSetting setting, ref IAveTaxonomyField taxField, ConfigSiteSetting configSiteSetting = null)
        {
            using (var scope = new PerformanceScope("RMSPSettingUtility.ConfigBCSColumn4List", $"RMSPSettingUtility.ConfigBCSColumn4List{list.Title}", true))
            {
                logger.Info($"FullPath:[{setting.FullPath}] IsUsingExistColumn:[{setting.IsUsingExistColumnName}] ExistingCoumnName:[{setting.ExistColumnName}] Configure term settings in Records:[{setting.SetDocLevelTermForExistColumn}]");
                SettingResult result = SettingResult.SKip;
                if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    if (!CheckClassificationSetting(setting, site))
                    {
                        throw new Exception("Term Is Unavailable");
                    }
                    IAveTaxonomyField siteField = null;
                    Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
                    FieldConflict listConflict = VerifyFieldConflict(site, list.Fields, setting, ref taxField);
                    result = HandleListFieldConflict(listConflict, site, list, setting, siteField, ref taxField, configSiteSetting);
                    if (taxField != null)
                    {
                        EnsureFieldAdded2AllContentTypes(taxField, list);
                        if ((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.UseDefaultTerm &&
                       setting.DefaultTermId != null && setting.DefaultTermId != Guid.Empty && (!setting.IsUsingExistColumnName || (setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn)))
                        {
                            var existSPDefaultValue = !(taxField.DefaultValue == null || taxField.DefaultValue.StartsWith("-1"));//DefaultValue如果是空字符串，说明是在SP手动修改的,这种情况认为是有值的，值是空字符串
                            var isKeepSPDefaultValue = IsKeepSPDefaultValue(setting);
                            logger.Info($"ConfigBCSColumn: exist default value in sharepoint is: [{existSPDefaultValue}], isKeepSPDefaultValue:[{isKeepSPDefaultValue}]");
                            if (isKeepSPDefaultValue && existSPDefaultValue)
                            {
                                if (result == SettingResult.Add)
                                {
                                    logger.Info($"Add list column for the first time, use the site column default value, title: {list.Title} ");
                                }
                                else
                                {
                                    result = SettingResult.SKip;
                                    logger.Info($"skip update list column default value, title: {list.Title} ");
                                }
                            }
                            else
                            {
                                UpdateBCSColumnDefaultValue(list, setting, taxField);
                                
                                try
                                {
                                    string wssId = GetTermWssId(site, setting.DefaultTermName, setting.DefaultTermId);
                                    if (wssId != "-1")
                                    {
                                        string rootFolderDefaultValue = wssId + ";#" + setting.DefaultTermName + "|" + setting.DefaultTermId;
                                      
                                        IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(site, taxField.InternalName);
                                        string existRootDefault = string.Empty;
                                        try
                                        {
                                            existRootDefault = mDefaults.GetFieldDefault(
                                                list.ParentWeb.ServerRelativeUrl,
                                                list.Title,
                                                list.ID,
                                                list.RootFolder.ServerRelativeUrl);
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Warn("Get root folder default error: {0}", ex.ToString());
                                        }

                                        if (rootFolderDefaultValue != existRootDefault)
                                        {
                                            mDefaults.SetFieldDefault(
                                                list.ParentWeb.ServerRelativeUrl,
                                                list.Title,
                                                list.ID,
                                                list.RootFolder.ServerRelativeUrl,
                                                rootFolderDefaultValue);
                                            logger.Info("Set root folder default value:{0}", rootFolderDefaultValue);
                                        }

                                        taxField.DefaultValue = string.Empty;
                                        taxField.Update();
                                        logger.Info("Cleared list column default, root folder default = {0}", rootFolderDefaultValue);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn("Set root folder default failed, list:{0}, error:{1}", list.Title, ex.ToString());
                                }
                            }
                        }
                        else if ((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.NoDefaultTerm
         && !IsKeepSPDefaultValue(setting))
                        {
                            try
                            {
                                IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(site, taxField.InternalName);
                                string existRootDefault = string.Empty;
                                try
                                {
                                    existRootDefault = mDefaults.GetFieldDefault(
                                        list.ParentWeb.ServerRelativeUrl,
                                        list.Title,
                                        list.ID,
                                        list.RootFolder.ServerRelativeUrl);
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn("Get root folder default error: {0}", ex.ToString());
                                }

                                if (!string.IsNullOrEmpty(existRootDefault))
                                {
                                    mDefaults.RemoveFieldDefault(
                                        list.ParentWeb.ServerRelativeUrl,
                                        list.Title,
                                        list.ID,
                                        list.RootFolder.ServerRelativeUrl);
                                    logger.Info("Removed inherited root folder default value for manually-choose-term library: {0}", list.Title);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Warn("Remove root folder default failed, list:{0}, error:{1}", list.Title, ex.ToString());
                            }
                        }
                    }
                }
                else
                {
                    //RECO-2574
                    //if (setting.IsUsingExistColumnName)
                    //{
                    //    return SettingResult.UseExistSkip;
                    //}
                    try
                    {
                        //var listField = list.Fields.GetFieldById(BCSColumnID, false);
                        IAveField listField;
                        if (setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn)
                        {
                            listField = list.Fields.GetRecordTaxonomyField(setting.ExistColumnName);
                        }
                        else
                        {
                            listField = list.Fields.GetFieldById(BCSColumnID, false);
                        }
                        if (listField != null)
                        {
                            IAveTaxonomyField removeTaxField = listField as IAveTaxonomyField;
                            var listTextField = list.Fields.GetFieldById(removeTaxField.TextField, false);
                            if (listTextField != null)
                            {
                                if (listTextField.Hidden)
                                {
                                    listTextField.Hidden = false;
                                    listTextField.Update();
                                }
                                listTextField.Delete();
                                listTextField.Update();
                            }
                            else
                            {
                                try
                                {
                                    logger.Warn("can't get taxonomy field's note field, will get note field by internal name [i0f84bba906045b4af568ee102a52dcb], url:{0}", list.RootFolder.Url);
                                    var listTextFieldByName = list.Fields.GetFieldByInternalName("i0f84bba906045b4af568ee102a52dcb");
                                    listTextFieldByName.Delete();
                                    listTextFieldByName.Update();
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("get note field by internal name [i0f84bba906045b4af568ee102a52dcb], error {0}", e.ToString());
                                }
                            }
                            listField.Delete();
                            listField.Update();

                            try
                            {
                                Queue<IAveFolder> queue = new Queue<IAveFolder>();
                                foreach (var folder in list.RootFolder.Folders)
                                {
                                    queue.Enqueue(folder);
                                }
                                while (queue.Count > 0)
                                {
                                    var folder = queue.Dequeue();
                                    logger.Debug("remove folder property: {0}", folder.Url);
                                    foreach (var sunFolder in folder.SubFolders)
                                    {
                                        queue.Enqueue(sunFolder);
                                    }

                                    try
                                    {
                                        if (folder.Properties.ContainsKey(listField.InternalName))
                                        {
                                            folder.Properties[listField.InternalName] = null;
                                            folder.Properties.Remove(listField.InternalName);
                                        }
                                        AvePoint.GCommon.Utility.ArgumentCheck.NotNull(listTextField, nameof(listTextField));
                                        if (folder.Properties.ContainsKey(listTextField.InternalName))
                                        {
                                            folder.Properties[listTextField.InternalName] = null;
                                            folder.Properties.Remove(listTextField.InternalName);
                                        }
                                        folder.Update();
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Warn("remove folder property, url:{0}; error: {1}", folder.Url, e.ToString());
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn("remove list all folder property, url:{0}; error: {1}", list.RootFolder.Url, e.ToString());
                            }
                        }
                        else
                        {
                            return SettingResult.SkipDelete;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("remove column, url:{0}; error: {1}", list.RootFolder.Url, e.ToString());
                        throw;
                    }

                    result = SettingResult.Delete;
                }
                return result;
            }
        }

        private static void EnsureFieldAdded2AllContentTypes(IAveTaxonomyField taxField, IAveList list)
        {
            try
            {
                list.Reload();
                var fieldLink = factoryForAuto.CreateFieldLink(taxField);
                bool hasContentTypeUpdate = false;
                foreach (var contentType in list.ContentTypes)
                {
                    try
                    {
                        if (IsSupportContentType(contentType) && !contentType.Fields.ContainsFieldWithInternalName(taxField.InternalName))
                        {
                            if (contentType.Sealed)
                            {
                                logger.Info($"This cotent type is sealed, won't add field. Id:{contentType.ID}");
                                //var tempContentType = list.ParentWeb.ContentTypes.Where(c => contentType.ID.IsChildOf(c.ID)).FirstOrDefault();
                                //if (tempContentType != null && !mUpdateSiteContentTypeIds.Contains(tempContentType.ID))
                                //{
                                //    tempContentType.FieldLinks.Add(fieldLink);
                                //    tempContentType.Update(false);
                                //    hasContentTypeUpdate = true;
                                //}
                            }
                            else
                            {
                                contentType.FieldLinks.Add(fieldLink);
                                contentType.Update(false);
                                hasContentTypeUpdate = true;
                            }
                            //contentType.Fields.AddFieldAsXml(taxField.SchemaXml, true, AveAddFieldOptions.DefaultValue);
                        }
                        else if (contentType.ID.IsChildOf(new AvePoint.ObjectModel.Common.AveContentTypeId(AveBuiltInContentTypeId.DocumentSet)) && contentType.Fields.ContainsFieldWithInternalName(taxField.InternalName))
                        {
                            try
                            {
                                var fieldL = contentType.FieldLinks[taxField.InternalName];
                                logger.Info($"The document set [{contentType.Name}], fieldL.Required:[{fieldL.Required}],taxField.Required:[{taxField.Required}],fieldL.Hidden:[{fieldL.Hidden}],taxField.Hidden:[{taxField.Hidden}], NeedUpdateRequired:[{fieldL.Required != taxField.Required}], NeedUpdateHidden:[{fieldL.Hidden != taxField.Hidden}]");
                                if (fieldL.Required != taxField.Required)
                                {
                                    fieldL.Required = taxField.Required;
                                    hasContentTypeUpdate = true;
                                    logger.Info($"Update document set [{contentType.Name}] field link [{taxField.InternalName}] Required to [{taxField.Required}]");
                                }
                                if (fieldL.Hidden != taxField.Hidden)
                                {
                                    fieldL.Hidden = taxField.Hidden;
                                    hasContentTypeUpdate = true;
                                    logger.Info($"Update document set [{contentType.Name}] field link [{taxField.InternalName}] Hidden to [{taxField.Hidden}]");
                                }
                                if (hasContentTypeUpdate)
                                {
                                    contentType.Update();
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error($"Failed Update document set [{contentType.Name}] field link. error:{e}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("An error occurred while adding bcs field to content type. Content Type Id:{0} Error:{1}", contentType?.ID, ex.ToString());
                    }
                }
                if (hasContentTypeUpdate)
                {
                    list.ContentTypes.Update();
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while adding bcs field to content types. Error:{0}", e.ToString());
            }
        }

        private static bool IsSupportContentType(IAveContentType contentType)
        {
            if (contentType.ID.IsChildOf(new AvePoint.ObjectModel.Common.AveContentTypeId(AveBuiltInContentTypeId.Folder))
                      || contentType.ID.IsChildOf(new AvePoint.ObjectModel.Common.AveContentTypeId(AveBuiltInContentTypeId.Document))
                      || contentType.ID.IsChildOf(new AvePoint.ObjectModel.Common.AveContentTypeId(AveBuiltInContentTypeId.DocumentSet)))
            {
                return true;
            }
            return false;
        }
        private static void UpdateBCSColumnDefaultValue(IAveList list, RMSharePointSetting node, IAveTaxonomyField listTaxField)
        {
            using (var scope = new PerformanceScope("RMSPSettingUtility.UpdateBCSColumnDefaultValue", $"RMSPSettingUtility.UpdateBCSColumnDefaultValue{list.Title}", true))
            {
                string wssId = GetTermWssId(list.ParentWeb.Site, node.DefaultTermName, node.DefaultTermId);
                if (wssId == "-1")
                {
                    try
                    {
                        var term = list.ParentWeb.Site.AveSPTaxonomySession.GetTerm(node.DefaultTermId);
                        
                        IAveTaxonomyFieldValue taxValue = listTaxField.TaxonomyFieldValue;
                        taxValue.TermGuid = term.ID.ToString();
                        taxValue.Label = term.Name;

                        AveItemCreationInformation info = new AveItemCreationInformation()
                        {
                            UnderlyingObjectType = AveFileSystemObjectType.Folder,
                            FolderUrl = list.RootFolder.ServerRelativeUrl,
                            LeafName = string.Concat("Temporary_Folder_For_WssId_Creation_", DateTime.Now.ToFileTime().ToString())
                        };

                        #region 
                        var item = list.AddItem(info);
                        try
                        {
                            item.SystemUpdate();
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("UpdateBCSColumnDefaultValue failed {0}:{1} error {2}", list.Title, term.Name, ex.ToString());

                            item.SystemUpdate();
                        }

                        logger.Info("temp taxonomy value: {0}", taxValue.ToString());
                        listTaxField.SetFieldValue(item, taxValue);

                        item.Delete();
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("add item for get wssid error:{0}", ex.ToString());
                    }
                    wssId = GetTermWssId(list.ParentWeb.Site, node.DefaultTermName, node.DefaultTermId);
                    if (wssId == "-1")
                    {
                        throw new Exception(string.Format("term not found in the term store,termStoreId:{0},TermSet:{1},TermName:{2},DefaultTermName:{3}", node.TermStoreId, node.TermSetName, node.TermName, node.DefaultTermName));
                    }
                    listTaxField.DefaultValue = string.Format("{0};#{1}|{2}", wssId, node.DefaultTermName, node.DefaultTermId);
                    logger.Info("Update column default value {0}", listTaxField.DefaultValue);
                    listTaxField.Update();
                }
                else
                {
                    listTaxField.DefaultValue = wssId + ";#" + node.DefaultTermName + "|" + node.DefaultTermId;
                    logger.Info("Update column default value {0}", listTaxField.DefaultValue);
                    listTaxField.Update();
                }
            }
        }
        private static string GetTermWssId(IAveSite site, string term, Guid termId)
        {
            using (var scope = new PerformanceScope("RMSPSettingUtility.GetTermWssId", $"RMSPSettingUtility.GetTermWssId{term}", true))
            {
                try
                {
                    string result = "-1";
                    IAveList taxonomyList = site.RootWeb.Lists.GetByTitle("TaxonomyHiddenList");
                    if(taxonomyList == null)
                    {
                        logger.Warn("TaxonomyHiddenList not found in site {0}", site.Url);
                        return result;
                    }
                    //修改了Term Name，或者其它情况， 需要1个小时才会同步到HiddenList， 如果用TermName查询， 有可能会得不到可用Term
                    AveCamlQuery camlQueryForTerm = new AveCamlQuery();
                    camlQueryForTerm.ViewXml = @"<View>
          <Query>
          <Where>
            <Eq>
            <FieldRef Name='IdForTerm'/>
            <Value Type='Text'>" + termId + @"</Value>
            </Eq>
          </Where>
          </Query>
        </View>";
                    camlQueryForTerm.FolderServerRelativeUrl = taxonomyList.RootFolder.ServerRelativeUrl;
                    IAveListItemCollection termItems = taxonomyList.GetItems(camlQueryForTerm);
                    logger.Info($"{termItems.Count} terms is found in TaxonomyHiddenList, {term}|{termId}");
                    foreach (var termItem in termItems)
                    {
                        if (termItem["Title"] == null)
                        {
                            logger.Warn("Term Title in TaxonomyHiddenList is null.TermGuid:[{0}] TermSetId:[{1}]"
                                , termItem["IdForTerm"].ToString(), termItem["IdForTermSet"]);
                            continue;
                        }
                        //string taxId = termItem["IdForTerm"].ToString();
                        //if (taxId.Equals(termId.ToString(), StringComparison.OrdinalIgnoreCase))
                        //{
                        string temp = termItem["ID"].ToString();
                        logger.Debug("Get temp Term ID: {0}, guid:{1}, name:{2}", temp, termId, termItem["Title"]);
                        //todo 使用Id较小且不是-1的
                        if (temp != "-1" && IsS1LessThanS2(temp, result))
                        {
                            logger.Debug("New temp term ID:{0} is less than previous one {1}", temp, result);
                            result = temp;
                        }
                        //return termItem["ID"].ToString();
                        //}
                    }
                    return result;
                }
                catch (Exception e1)
                {
                    logger.Debug($"Error while getting term wssid. {e1}");
                    return "-1";
                }
            }
        }

        private static bool IsS1LessThanS2(string s1, string s2)
        {
            try
            {
                if (s2 == "-1")
                {
                    return true;
                }
                int int1 = int.Parse(s1);
                int int2 = int.Parse(s1);
                return int1 < int2;
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                return true;
            }
        }

        private static FieldConflict VerifyFieldConflict(IAveSite site, IAveFieldCollection collection, RMSharePointSetting setting, ref IAveTaxonomyField taxField)
        {
            FieldConflict conflict = FieldConflict.None;
            if (!setting.IsUsingExistColumnName)
            {
                var bcsColumn = collection.GetFieldById(BCSColumnID, false);
                if (bcsColumn == null)
                {
                    var tempField = collection.Where(f => f.Title == setting.ColumnName).FirstOrDefault();
                    if (tempField != null)
                    {
                        taxField = tempField as IAveTaxonomyField;
                        conflict = taxField != null ? FieldConflict.NameConflict : FieldConflict.ColumnNotFound;
                    }
                    else
                    {
                        conflict = FieldConflict.ColumnNotFound;
                    }
                }
                else
                {
                    taxField = bcsColumn as IAveTaxonomyField;
                    conflict = FieldConflict.ColumnExisting;
                }
            }
            else
            {
                var tempField = collection.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault();
                tempField ??= collection.Where(f => f.InternalName == setting.ExistColumnName).FirstOrDefault();
                if (tempField == null)
                {
                    string staticName = SPCommonUtility.GetSiteLevelExistColumnStaticName(site, setting.ExistColumnName);
                    tempField ??= collection.Where(f => f.StaticName == staticName).FirstOrDefault();
                }
                if (tempField == null)
                {
                    logger.Warn($"[VerifyFieldConflict] Can not get column by name.");
                    conflict = FieldConflict.ColumnNotFound;
                }
                else
                {
                    logger.Info($"[VerifyFieldConflict] Configuration ColumnName:{setting.ExistColumnName}, Title:{tempField.Title}, InternalName: {tempField.InternalName}, StaticName: {tempField.StaticName}");
                    taxField = tempField as IAveTaxonomyField;
                    if (setting.SetDocLevelTermForExistColumn)
                    {
                        conflict = FieldConflict.ColumnExisting;
                    }
                    else
                    {
                        conflict = FieldConflict.SkipCheckColumn;
                    }
                }
            }
            return conflict;
        }

        private static SettingResult HandleSiteFieldConflict(FieldConflict conflict, IAveSite site, RMSharePointSetting setting, ref IAveTaxonomyField siteField, bool needUpdate = false, ConfigSiteSetting configSiteSetting = null)
        {
            SettingResult result = SettingResult.None;
            Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
            switch (conflict)
            {
                case FieldConflict.ColumnNotFound:
                    if (!setting.IsUsingExistColumnName)
                    {
                        IAveField tempField = JobContext.IsCSDTenant ?
                            site.RootWeb.Fields.AddFieldAsXml("<Field Type='" + "TaxonomyFieldType" + "'   Name='" + XmlUtil.TransferSpecialCharactor(BCSColumnInternalName) + "' ID='" + BCSColumnID + "' DisplayName='" + XmlUtil.TransferSpecialCharactor(setting.ColumnName) + "'  ShowField='Term" + site.RootWeb.GetWorkingLanguage() + "' StaticName='RevIMBCS' ReadOnly='TRUE' ShowInVersionHistory='TRUE' />", true, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes)
                            : site.RootWeb.Fields.AddFieldAsXml("<Field Type='" + "TaxonomyFieldType" + "'   Name='" + XmlUtil.TransferSpecialCharactor(BCSColumnInternalName) + "' ID='" + BCSColumnID + "' DisplayName='" + XmlUtil.TransferSpecialCharactor(setting.ColumnName) + "'  ShowField='Term1033' StaticName='RevIMBCS' />", true, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes);
                        siteField = tempField as IAveTaxonomyField;
                        if (JobContext.IsCSDTenant)
                        {
                            InitTaxnomyField(siteField, setting, termStoreId, true, spObj: site, settingResult: ref result,  lcid: site.RootWeb.GetWorkingLanguage());
                        }
                        else
                        {
                            InitTaxnomyField(siteField, setting, termStoreId, true, spObj: site, settingResult: ref result);
                        }
                        result = SettingResult.Add;
                    }
                    else
                    {
                        throw new Exception(I18NEntity.GetString("RM_SPS_CanNotFindExistingColumn"));
                    }

                    break;
                case FieldConflict.ColumnExisting:
                    if (needUpdate && NeedUpdateBCSColumn(siteField, setting, termStoreId, true, out _))
                    {
                        result = SettingResult.Update;
                        if (JobContext.IsCSDTenant)
                        {
                            InitTaxnomyField(siteField, setting, termStoreId, true, spObj: site, settingResult: ref result, lcid: site.RootWeb.GetWorkingLanguage());
                        }
                        else
                        {
                            InitTaxnomyField(siteField, setting, termStoreId, true, spObj: site, settingResult: ref result);
                        }
                    }
                    else
                    {
                        result = SettingResult.SKip;
                    }
                    break;
                case FieldConflict.NameConflict:
                    throw new Exception(I18NEntity.GetString("RM_SS_SCAddiOrNameRepeat"));
                case FieldConflict.SkipCheckColumn:
                    result = SettingResult.SKip;
                    break;
                default:
                    break;
            }
            return result;
        }


        private static SettingResult HandleListFieldConflict(FieldConflict conflict, IAveSite site, IAveList list, RMSharePointSetting setting, IAveTaxonomyField siteField, ref IAveTaxonomyField listField, ConfigSiteSetting configSiteSetting = null)
        {
            logger.Debug($"config list:{list.RootFolder?.ServerRelativeUrl} bcs column, list conflict:{conflict}");
            SettingResult result = SettingResult.None;
            Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
            switch (conflict)
            {
                case FieldConflict.ColumnNotFound:
                    var gSetting = SettingsHelpers.LoadSharePointSetting(setting.SiteGroupId, Guid.Empty);
                    if (gSetting.IsUsingExistColumnName && !gSetting.SetDocLevelTermForExistColumn)
                    {
                        logger.Warn($"use existing column, skip to set doclevel setting,{setting.FullPath}.");
                        result = SettingResult.SKip;
                        return result;
                    }
                    FieldConflict siteConflict = VerifyFieldConflict(site, site.RootWeb.Fields, setting, ref siteField);
                    logger.Debug($"config list:{list.RootFolder?.ServerRelativeUrl} bcs column, site conflict:{siteConflict}");

                    HandleSiteFieldConflict(siteConflict, site, gSetting, ref siteField, configSiteSetting: configSiteSetting);
                    if (siteField == null)
                    {
                        logger.Warn("siteField info is null");
                    }
                    IAveField tempListField;
                    if (JobContext.IsCSDTenant)
                    {
                        AvePoint.GCommon.Utility.ArgumentCheck.NotNull(siteField, nameof(siteField));
                        var listColumnSchemaXml = siteField.SchemaXml;
                        if (!setting.IsUsingExistColumnName)
                        {
                            try
                            {
                                if (list.ParentWeb.GetWorkingLanguage() != site.RootWeb.GetWorkingLanguage())
                                {
                                    XmlDocument xml = new XmlDocument();
                                    xml.LoadXml(listColumnSchemaXml);
                                    var fieldNode = xml.SelectSingleNode("Field");

                                    var showFieldNode = fieldNode.Attributes.GetNamedItem("ShowField");
                                    showFieldNode.Value = "Term" + list.ParentWeb.GetWorkingLanguage();

                                    var displayNameNode = fieldNode.Attributes.GetNamedItem("DisplayName");
                                    var currentWebCultureName = new CultureInfo(list.ParentWeb.GetWorkingLanguage()).Name;
                                    var cultureClassName = string.Empty;
                                    if (CSDClassNameAndCultureMapping.TryGetValue(currentWebCultureName, out cultureClassName))
                                    {
                                        displayNameNode.Value = cultureClassName;
                                    }
                                    listColumnSchemaXml = xml.OuterXml;
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn("parse SchemaXml error: {0}", e.ToString());
                            }
                        }
                        var addToDefaultView = gSetting.ColumnHidden == false || gSetting.ColumnHidden == null;
                        var addFieldOptions = addToDefaultView ? AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes : AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToAllContentTypes;
                        logger.Debug($"Add new filed: addToDefaultView:{addToDefaultView}");
                        tempListField = list.Fields.AddFieldAsXml(listColumnSchemaXml, addToDefaultView, addFieldOptions);
                    }
                    else
                    {
                        AvePoint.GCommon.Utility.ArgumentCheck.NotNull(siteField, nameof(siteField));
                        var addToDefaultView = gSetting.ColumnHidden == false || gSetting.ColumnHidden == null;
                        var addFieldOptions = addToDefaultView ? AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes : AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToAllContentTypes;
                        logger.Debug($"Add new filed: addToDefaultView:{addToDefaultView}");
                        tempListField = list.Fields.AddFieldAsXml(siteField.SchemaXml, addToDefaultView, addFieldOptions);
                    }
                    listField = tempListField as IAveTaxonomyField;
                    //不使用folder setting 更新column
                    if (setting.ScopeId != setting.FolderId)
                    {
                        if (JobContext.IsCSDTenant)
                        {
                            InitTaxnomyField(listField, setting, termStoreId, false, lcid: list.ParentWeb.GetWorkingLanguage(), settingResult: ref result);
                            if (list.ParentWeb.GetWorkingLanguage() != site.RootWeb.GetWorkingLanguage() && !setting.IsUsingExistColumnName)
                            {
                                RepeatUpdateTitle(list.ParentWeb.GetWorkingLanguage(), listField);
                            }
                            if (setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn)
                            {
                                RepeatUpdateTitle(list.ParentWeb.GetWorkingLanguage(), siteField, listField);
                            }
                            var view = list.Views.Where(v => v.DefaultView).First();
                            var moveToIndex = 0;
                            for (int i = 0; i < view.ViewFields.Count; i++)
                            {
                                if (view.ViewFields.ElementAt(i).Equals(CSDFieldName.DeletionDate))
                                {
                                    moveToIndex = i;
                                }
                            }
                            view.ViewFields.MoveFieldTo(BCSColumnInternalName, moveToIndex);
                            view.Update();
                        }
                        else
                        {
                            InitTaxnomyField(listField, setting, termStoreId, false, settingResult: ref result);
                        }
                    }
                    //暂时认为重复，可能引发SaveConflict 去掉
                    //listField.DefaultValue = siteField.DefaultValue;
                    //listField.Update();
                    result = SettingResult.Add;
                    break;
                case FieldConflict.ColumnExisting:
                    if (!JobContext.IsCSDTenant)
                    {
                        ResetCantoggleHiddenValue(listField);
                    }

                    if (NeedUpdateBCSColumn(listField, setting, termStoreId, false,out var needUpdateView, setting.ScopeId == setting.FolderId))
                    {
                        result = SettingResult.Update;
                        if (JobContext.IsCSDTenant)
                        {
                            InitTaxnomyField(listField, setting, termStoreId, false, lcid: list.ParentWeb.GetWorkingLanguage(), settingResult: ref result);
                            if (!setting.IsUsingExistColumnName)
                            {
                                RepeatUpdateTitle(list.ParentWeb.GetWorkingLanguage(), listField);
                            }
                        }
                        else
                        {
                            InitTaxnomyField(listField, setting, termStoreId, false, settingResult: ref result);
                        }
                        if (needUpdateView)
                        {
                            logger.Info("Update default view fields for list:{0}", list.Title);
                            try
                            {
                                var view = list.Views.Where(v => v.DefaultView).FirstOrDefault();
                                if (view != null)
                                {
                                    if (listField.Hidden)
                                    {
                                        logger.Info($"Remove {BCSColumnInternalName} column for view:{view.Title}");
                                        if (view.ViewFields.Exists(listField.InternalName))
                                        {
                                            view.ViewFields.Remove(listField.InternalName);
                                            view.Update();
                                            logger.Info($"{listField.InternalName} column has been removed from view:{view.Title}");
                                        }
                                    }
                                    else
                                    {
                                        logger.Info($"Add {listField.InternalName} column for view:{view.Title}");
                                        if (!view.ViewFields.Exists(listField.InternalName))
                                        {
                                            view.ViewFields.Add(listField.InternalName);
                                            view.Update();
                                            logger.Info($"{listField.InternalName} column has been added from view:{view.Title}");
                                        }
                                    }
                                }
                            }
                            catch (Exception e2)
                            {
                                logger.Info($"Failed to get list default view for:{list.Title}, ex:{e2}");
                            }
                        }
                    }
                    else
                    {
                        result = SettingResult.SKip;
                    }
                    break;
                case FieldConflict.NameConflict:
                    throw new Exception(I18NEntity.GetString("RM_SS_SCAddiOrNameRepeat"));

                case FieldConflict.SkipCheckColumn:
                    result = SettingResult.SKip;
                    break;
                default:
                    break;
            }

            return result;
        }

        private static void ResetCantoggleHiddenValue(IAveField field)
        {
            try
            {
                var fieldSchema = field.SchemaXml;
                if (!string.IsNullOrEmpty(fieldSchema))
                {
                    var tempValue = field.GetAttributeFromSchemaXml("CanToggleHidden");
                    if (tempValue != null)
                    {
                        var fieldAttrValue = field.GetAttributeFromSchemaXml("Hidden");
                        bool hidden = Convert.ToBoolean(fieldAttrValue);
                        bool canToggleHidden = Convert.ToBoolean(tempValue);
                        logger.Info($"The Hidden value is [{hidden}], CanToggleHidden value is [{canToggleHidden}] in the sharepoint");
                        if (!canToggleHidden)
                        {
                            var doc = new XmlDocument();
                            doc.LoadXml(fieldSchema);
                            var fieldNode = doc.DocumentElement;
                            fieldNode.SetAttribute("CanToggleHidden", "TRUE");
                            fieldSchema = doc.DocumentElement.OuterXml;
                            field.SchemaXml = fieldSchema;
                            field.Update();
                            logger.Info("Reset canToggleHidden to true.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An exception was encountered when ResetCantoggleHiddenValue, message: {ex}");
            }
        }
        
        private static void RepeatUpdateTitle(int lcid, IAveTaxonomyField taxField)
        {
            taxField.Title = CSDClassName_EN;
            foreach (var culName in CSDClassNameAndCultureMapping.Keys)
            {
                var culInfo = new CultureInfo(culName);
                if (culInfo.LCID == lcid)
                {
                    taxField.Title = CSDClassNameAndCultureMapping[culName];
                }
                taxField.TitleResource.SetValueForUICulture(culName, CSDClassNameAndCultureMapping[culName]);
            }
            taxField.TitleResource.Update();
            taxField.Update();
        }

        private static void RepeatUpdateTitle(int lcid, IAveTaxonomyField siteField, IAveTaxonomyField listField)
        {
            listField.Title = CSDClassName_EN;
            foreach (var culName in CSDClassNameAndCultureMapping.Keys)
            {
                var culInfo = new CultureInfo(culName);
                if (culInfo.LCID == lcid)
                {
                    var titleResource = string.Empty;
                    try
                    {
                        titleResource = siteField.TitleResource.GetValueForUICulture(culName);
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"get title resource error, culture:{culName}, error:{e}");
                        continue;
                    }
                    listField.Title = titleResource;
                }
                listField.TitleResource.SetValueForUICulture(culName, CSDClassNameAndCultureMapping[culName]);
            }
            listField.TitleResource.Update();
            listField.Update();
        }

        private static void InitTaxnomyField(IAveTaxonomyField taxField, RMSharePointSetting setting, Guid termStoreId, bool isSiteLevel, ref SettingResult settingResult , object spObj = null, int lcid = -1, bool updateIndex = true)//TO DO Replace Setting from control...
        {
            using (var scope = new PerformanceScope("RMSPSettingUtility.InitTaxnomyField", $"RMSPSettingUtility.InitTaxnomyField{taxField.ID}", true))
            {
                var taxFieldId = taxField.ID;
                var skipUpdateTaxField = false;
                var isKeepSPDefaultValue = IsKeepSPDefaultValue(setting);
                logger.Info("Init taxonomy field:{0}", taxField.ID);
                try
                {
                    if (!setting.IsUsingExistColumnName)
                    {
                        if (JobContext.IsCSDTenant && lcid > 0)
                        {
                            taxField.Title = CSDClassName_EN;
                            foreach (var culName in CSDClassNameAndCultureMapping.Keys)
                            {
                                var culInfo = new CultureInfo(culName);
                                if (culInfo.LCID == lcid)
                                {
                                    taxField.Title = CSDClassNameAndCultureMapping[culName];
                                }
                                taxField.TitleResource.SetValueForUICulture(culName, CSDClassNameAndCultureMapping[culName]);
                            }
                            taxField.TitleResource.Update();
                        }
                        else
                        {
                            taxField.Title = setting.IsUsingExistColumnName ? taxField.Title : setting.ColumnName;
                        }
                    }
                    if (JobContext.IsCSDTenant && !setting.IsUsingExistColumnName)
                    {
                        taxField.ReadOnlyField = true;
                        taxField.ShowInVersionHistory = true;
                    }
                    taxField.SspId = termStoreId;
                    taxField.EnforceUniqueValues = false;
                    taxField.AllowMultipleValues = false;
                    taxField.TermSetId = setting.TermSetId;
                    if (updateIndex)
                    {
                        taxField.Indexed = true;
                    }
                    if (!setting.IsUsingExistColumnName)
                    {
                        taxField.Required = setting.ColumnRequired == null ? true : (bool)setting.ColumnRequired;
                        var hiddenInDB = setting.ColumnHidden == null ? false : (bool)setting.ColumnHidden;
                        if (!isSiteLevel && taxField.Hidden != hiddenInDB)
                        {
                            logger.Info($"The Hidden of column settings is {hiddenInDB}, taxField.Hidden is {taxField.Hidden}.");
                            taxField.Hidden = hiddenInDB;
                        }
                        taxField.Description = string.IsNullOrEmpty(setting.Description) ? "" : setting.Description;
                    }
                    taxField.IsPathRendered = setting.IsDisplyaTermPath;
                    if (setting.TermId != null && setting.TermId != Guid.Empty)
                    {
                        taxField.AnchorId = setting.TermId;
                    }
                    else
                    {
                        taxField.AnchorId = Guid.Empty;
                    }
                    var existSPDefaultValue = !(taxField.DefaultValue == null || taxField.DefaultValue.StartsWith("-1"));
                    //SP Default Value是空字符串的时候，需要看Setting设置是否更新
                    var useRecordDefaultTermSetting = setting.SetTermForEmptyDefaultValue && taxField.DefaultValue == "";
                    var isRemoveDefaultValue = isSiteLevel ? isKeepSPDefaultValue : (isKeepSPDefaultValue && existSPDefaultValue);
                    switch ((DeployTermMethod)setting.DeployTermMethod)
                    {
                        case DeployTermMethod.UseDefaultTerm:
                            if (setting.DefaultTermId != null && setting.DefaultTermId != Guid.Empty)
                            {
                                logger.Info($"InitTaxnomyField: exist default value in sharepoint is: [{existSPDefaultValue}], isSiteLevel:[{isSiteLevel}], isKeepSPDefaultValue:[{isKeepSPDefaultValue}], SetTermForEmptyDefaultValue:[{setting.SetTermForEmptyDefaultValue}]");
                                if (isKeepSPDefaultValue && existSPDefaultValue && !useRecordDefaultTermSetting)
                                {
                                    logger.Info("Preserving existing SP default value.");
                                }
                                else
                                {
                                    taxField.DefaultValue = "-1" + ";#" + setting.DefaultTermName + "|" + setting.DefaultTermId;
                                    logger.Info("Update default column value {0}", taxField.DefaultValue);
                                }
                            }
                            break;
                        case DeployTermMethod.UseAutoClassification:
                            if (!JobContext.IsCSDTenant)
                            {
                                if (!isRemoveDefaultValue)
                                {
                                    taxField.DefaultValue = string.Empty;
                                }
                            }
                            break;
                        case DeployTermMethod.NoDefaultTerm:
                            if (!isRemoveDefaultValue)
                            {
                                taxField.DefaultValue = string.Empty;
                            }
                            break;
                        case DeployTermMethod.UseIntelligenceClassification:
                            if (!isRemoveDefaultValue)
                            {
                                taxField.DefaultValue = string.Empty;
                            }
                            break;
                        default:
                            break;
                    }

                    if (skipUpdateTaxField)
                    {
                        logger.Info($"skipped to update default column value, deploy term method:[{(DeployTermMethod)setting.DeployTermMethod}]");
                    }
                    taxField.Update();
                }
                catch (AveRPCException e)
                {
                    if (e.Message.Contains("Save Conflict") && spObj != null)
                    {
                        var listObj = spObj as IAveList;
                        var siteObj = spObj as IAveSite;
                        IAveTaxonomyField reloadField = null;
                        if (listObj != null)
                        {
                            logger.Info("Retry update list column logic {0}:{1}", listObj.Title, e.ToString());
                            if (!setting.IsUsingExistColumnName)
                            {
                                reloadField = listObj.Fields.GetById(taxFieldId) as IAveTaxonomyField;
                            }
                            else
                            {
                                reloadField = listObj.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault() as IAveTaxonomyField;
                            }
                            InitTaxnomyField(reloadField, setting, termStoreId, false, lcid: lcid, settingResult: ref settingResult);
                        }
                        else if (siteObj != null)
                        {
                            logger.Info("Retry update site column logic {0}:{1}", siteObj.Url, e.ToString());
                            if (!setting.IsUsingExistColumnName)
                            {
                                reloadField = siteObj.RootWeb.Fields.GetById(taxFieldId) as IAveTaxonomyField;
                            }
                            else
                            {
                                reloadField = siteObj.RootWeb.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault() as IAveTaxonomyField;
                            }
                            InitTaxnomyField(reloadField, setting, termStoreId, false, lcid: lcid, settingResult: ref settingResult);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    //JobContext.IsCSDTenant &&
                    if (updateIndex)
                    {
                        logger.Warn($"Job failed to update column, we try to update column again with no index. Error:{e.ToString()}");
                        InitTaxnomyField(taxField, setting, termStoreId, isSiteLevel, lcid: lcid, updateIndex: false, settingResult: ref settingResult);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        //private string GetListColumnDefaultValue();

        public static bool ApplyExistItems(Guid remoteSiteId, IAveList list, IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointSetting setting, IAveORecords records, SPOLabelUtility labelUtility, ConfigSiteSetting configSiteSetting = null, bool setTermForFolderSelf = false)
        {
            bool hasError = false;
            using (new PerformanceScope("RMSPSettingUtility.ApplyExistItems", $"RMSPSettingUtility.ApplyExistItems{folder.Url}", true))
            {
                var defaultTermId = Guid.Empty;
                if (!JobContext.IsCSDTenant && setting.IsKeepSharePointDefaultValue)
                {
                    if (list.RootFolder.UniqueId != folder.UniqueId)
                    {
                        defaultTermId = GetFolderDefaultTermInSP(list, folder, aveTaxField, setting);
                    }
                    if (defaultTermId.Equals(Guid.Empty))
                    {
                        defaultTermId = GetDefaultTermIdInSP(aveTaxField);
                        logger.Info("use library default value.");
                    }
                }
                else
                {                  
                    if (list.RootFolder.UniqueId != folder.UniqueId)
                    {
                        defaultTermId = GetFolderDefaultTermInSP(list, folder, aveTaxField, setting);
                    }
                  
                    if (defaultTermId.Equals(Guid.Empty))
                    {
                        defaultTermId = setting.DefaultTermId;
                    }
                }
                var aveTerm = list.ParentWeb.Site.AveSPTaxonomySession.GetTerm(defaultTermId);
                if (aveTerm == null)
                {
                    throw new Exception("RM_SS_ConfigureColumnFailed");
                }

                List<string> excludePath = SettingsHelpers.GetExcludePath(remoteSiteId, list);
                excludePath = excludePath.Where(p => p.StartsWith(folder.ServerRelativeUrl.TrimEnd('/') + "/")).ToList();

                if (setTermForFolderSelf)
                {
                    logger.Info($"Set term for folder self. Folder Url:[{folder.Url}]");
                    IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;
                    taxValue.TermGuid = aveTerm.ID.ToString();
                    taxValue.Label = aveTerm.Name;
                    hasError = SetOneItemValue(folder.Item, taxValue, aveTaxField, aveTerm, records, setting, labelUtility, excludePath, !NeedIncluedeFolder(setting), configSiteSetting);
                }

                var rowLimit = int.Parse(RMKeyValueDao.GetValueByKey("SPQueryRowLimit")?.Value ?? "2000");
                AveCamlQuery query = null;
                int startIdx = 0;
                int maxItemId = 0;
                using (new PerformanceScope("RMSPSettingUtility.InitQuery", $"RMSPSettingUtility.InitQuery{folder.Url}", true))
                {
                    //using (new PerformanceScope("RMSPSettingUtility.GetThrottled"))
                    //{
                    //    rowLimit = GetMaxItemsPerThrottledOperation(list.ParentWeb.Site);
                    //}
                    maxItemId = GetLastItemId(list, list.RootFolder);   //取List下的最大Id， 否则SubFolder Items超过5000, 一样会Exceed Threshold
                    logger.Info("max item id in list {0}", maxItemId);
                    query = GetApplyExistingQuery(setting,aveTaxField.InternalName, folder, list, startIdx, rowLimit);
                }
                logger.Info($"rowLimit:{rowLimit}");
                IAveListItemCollection items = null;
                bool isOverWrite = setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite;

                bool needQueryNext = false;
                do
                {
                    using (new PerformanceScope("RMSPSettingUtility.GetItemsForRecords", $"RMSPSettingUtility.GetItemsForRecords{folder.Url}", true))
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            items = list.GetItemsForRecords(query);
                        }
                    }
                    ReportManager.IncreaseBase(items.Count);
                    logger.Info($"Existing job process list url {list.RootFolder.Url} item count:[{items.Count}]");
                    bool hasFailedItem = SetValue(items, aveTaxField, aveTerm, records, setting, labelUtility, excludePath, configSiteSetting: configSiteSetting, needChedkFileSystemObjectType: !NeedIncluedeFolder(setting));
                    if (!hasError && hasFailedItem)
                    {
                        hasError = true;
                    }

                    needQueryNext = isOverWrite ? items.ListItemCollectionPosition != null : startIdx < maxItemId;
                    if (startIdx + rowLimit < maxItemId)
                    {
                        needQueryNext = true;
                        startIdx = startIdx + rowLimit;
                    }
                    else
                    {
                        needQueryNext = false;
                    }
                    if (needQueryNext)
                    {
                        //if (isOverWrite)
                        //{
                        //    query.ListItemCollectionPosition.PagingInfo = items.ListItemCollectionPosition.PagingInfo;
                        //    logger.Info($"PagerInfo:{items.ListItemCollectionPosition.PagingInfo}");
                        //}
                        //else
                        //{
                        logger.Info($"Query for skip. StartIndex:[{startIdx}] EndIndex:[{startIdx + rowLimit}]");
                        query.ViewXml = GetApplyExistingQueryXml(isOverWrite, list.BaseTemplate == AveListTemplateType.DiscussionBoard, aveTaxField.InternalName, rowLimit, setting ,startIdx, startIdx + rowLimit);
                        //}
                    }
                }
                while (needQueryNext);
            }
            return hasError;
        }

        private static bool IsKeepSPDefaultValue(RMSharePointSetting setting)
        {
            return JobContext.IsCSDTenant ? false : setting.IsKeepSharePointDefaultValue;
        }

        private static Guid GetDefaultTermIdInSP(IAveTaxonomyField aveTaxField)
        {
            var termId = Guid.Empty;
            try
            {
                termId = new Guid(aveTaxField.DefaultValue.Substring(aveTaxField.DefaultValue.IndexOf('|') + 1));
                logger.Info($"default term id in sp, it is [{termId}]");
            }
            catch (Exception ex)
            {
                logger.Warn($"An error while get default term id in sp, message: {ex}");
            }
            return termId;
        }

        private static Guid GetFolderDefaultTermInSP(IAveList list, IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointSetting setting)
        {
            var termId = Guid.Empty;
            try
            {
                IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(list.ParentWeb.Site, aveTaxField.InternalName);
                var existFolderDefaultValue = mDefaults.GetFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl);
                if (!string.IsNullOrEmpty(existFolderDefaultValue) && !existFolderDefaultValue.StartsWith("-1"))
                {
                    termId = new Guid(existFolderDefaultValue.Substring(existFolderDefaultValue.IndexOf('|') + 1));
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error while get folder default term id in sp, path: {setting.FullPath}, message:{ex}");
            }
            return termId;
        }
        
        /// <summary>
        /// 注意：这个方法有时获取出来的是folder的最大ID
        /// </summary>
        /// <returns></returns>
        public static string GetLastItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit Paged='True'>1</RowLimit>
                </View>";
            logger.Info($"GetLastItemQueryXml:{result}");
            return result;
        }

        public static string GetLastFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit Paged='True'>1</RowLimit>
                </View>";
            logger.Info($"GetLastFileQueryXml:{result}");
            return result;
        }

        public static int InnerGetLastItemId(IAveList list, IAveFolder folder, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ViewXml = queryXml;
            var itemCollection = list.GetItemsForRecords(query);
            var item = itemCollection.FirstOrDefault();
            return item != null ? item.ID : -1;
        }
        public static int GetLastItemId(IAveList list, IAveFolder folder)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetLastItemId(list, folder, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetLastItemId(list, folder, fileQueryXml);
            return Math.Max(lastItemId, maxFileId);
        }

        private static string GetApplyExistingQueryXml(bool isOverwrite, bool isDiscussionList, string columnInternalName, int rowLimit, RMSharePointSetting setting,
            int startIdx = 0, int endIdx = 0)
        {
            string queryXml = string.Empty;
            string scope = GetQueryScopeType(setting, isDiscussionList).ToString();
            if (isOverwrite)
            {
                //queryXml= $"<View Scope=\"RecursiveAll\"><RowLimit>{rowLimit}</RowLimit></View>";
                queryXml = $@"
                    <View Scope='{scope}'>
                        <Query>
                            <Where>
                                             <And>
                                                    <Gt><FieldRef Name='ID'/><Value Type='Integer'>{startIdx}</Value></Gt>
                                                    <Leq><FieldRef Name='ID'/><Value Type='Integer'>{endIdx}</Value></Leq>
                                             </And>
                                      </Where>
                        </Query>
                        <RowLimit>{rowLimit}</RowLimit>
                    </View>";
            }
            else
            {
                queryXml = $@"
                <View Scope='{scope}'>
                    <Query>
                        <Where>
                            <And>
                                <IsNull><FieldRef Name='{columnInternalName}'/></IsNull>
                                <And>
                                    <Gt><FieldRef Name='ID'/><Value Type='Integer'>{startIdx}</Value></Gt>
                                    <Leq><FieldRef Name='ID'/><Value Type='Integer'>{endIdx}</Value></Leq>
                                </And>
                            </And>
                        </Where>
                    </Query>
                    <RowLimit>{rowLimit}</RowLimit>
                </View>";
            }
            logger.Info($"ApplyExisting query xml: {queryXml}");
            return queryXml;
        }

        //对于大众Tenant默认处理folder，不受页面选项影响，对于非大众tenant受页面选项影响
        private static bool NeedIncluedeFolder(RMSharePointSetting setting)
        {
            if (JobContext.IsCSDTenant)
            {
                return true;
            }
            else
            {
                return setting.IsApplyTermIncludeFolder();
            }
        }

        private static Types.ScopeTypes GetQueryScopeType(RMSharePointSetting setting, bool isDiscussionList = false)
        {
            var result = Types.ScopeTypes.RecursiveAll;
            if (JobContext.IsCSDTenant)
            {
                return result;
            }
            if (setting.IsApplyDocuments())
            {
                result = Types.ScopeTypes.Recursive;
            }
            if (setting.IsApplyTermIncludeFolder() || isDiscussionList)
            {
                result = Types.ScopeTypes.RecursiveAll;
            }
            return result;
        }

        private static AveCamlQuery GetApplyExistingQuery(RMSharePointSetting setting, string columnInternalName, IAveFolder folder, IAveList list,
            int startIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            try
            {
                query.LoadAllItems = false;
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                query.DatesInUtc = true;
                bool isOverwrite = setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite;
                bool isDiscussionList = list.BaseTemplate == AveListTemplateType.DiscussionBoard;
                query.ViewXml = GetApplyExistingQueryXml(isOverwrite, isDiscussionList, columnInternalName, rowLimit, setting, startIndex, startIndex + rowLimit);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while GetQueryXml,ERROR:{0}", ex.ToString());
            }
            return query;
        }






        //public static void ApplyExistItems(IAveList list, IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointSetting setting, IAveORecords records)//TO DO Debug
        //{
        //    using (new PerformanceScope($"RMSPSettingUtility.ApplyExistItems.Folder.{folder.ServerRelativeUrl}"))
        //    {

        //        var aveTerm = list.ParentWeb.Site.AveSPTaxonomySession.GetTerm(setting.DefaultTermId);

        //        if (aveTerm == null)
        //        {
        //            throw new Exception("RM_SS_ConfigureColumnFailed");
        //        }
        //        int rowLimit = GetMaxItemsPerThrottledOperation(list.ParentWeb.Site);
        //        List<IAveListItem> items = new List<IAveListItem>();
        //        ReportManager.IncreaseBase(items.Count);

        //        if (list.BaseType == AveBaseType.DocumentLibrary)
        //        {
        //            items = SPDicoverCache.Instance.ListCache.GetItemsUnderFolder(list, folder);
        //            //ony docment library set folder default value

        //            var folders = SPDicoverCache.Instance.ListCache.GetSubFolders(list, folder);
        //            foreach (var subFolder in folders)
        //            {
        //                ReportManager.IncreaseBase(folders.Count);
        //                var item = subFolder.Item;
        //                if (subFolder.Item != null)
        //                {
        //                    IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;
        //                    var folderSetting = SettingDao.GetSettingInfoByScope(setting.SiteGroupId, setting.SiteId, subFolder.UniqueId);
        //                    if (folderSetting != null)
        //                    {
        //                        var folderTerm = list.ParentWeb.Site.AveSPTaxonomySession.GetTerm(folderSetting.DefaultTermId);
        //                        if (folderTerm != null)
        //                        {
        //                            taxValue.TermGuid = aveTerm.ID.ToString();
        //                            taxValue.Label = aveTerm.Name;
        //                            SetOneItemValue(item, taxValue, aveTaxField, aveTerm, records, folderSetting);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        taxValue.TermGuid = aveTerm.ID.ToString();
        //                        taxValue.Label = aveTerm.Name;
        //                        SetOneItemValue(item, taxValue, aveTaxField, aveTerm, records, setting);
        //                    }
        //                }
        //                else
        //                {
        //                    logger.Warn($"invalid folder set default value:{folder.ServerRelativeUrl}");
        //                }

        //            }
        //        }
        //        else
        //        {
        //            items = SPDicoverCache.Instance.ListCache.GetAllItems(list);
        //        }
        //        SetValue(items, aveTaxField, aveTerm, records, setting);

        //    }
        //}

        private static bool SetValue(IAveListItemCollection items, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, IAveORecords records, RMSharePointSetting setting, SPOLabelUtility labelUtility, List<string> excludePath = null, bool needChedkFileSystemObjectType = false, ConfigSiteSetting configSiteSetting = null)
        {
            using (new PerformanceScope("RMSPSettingUtility.SetValue", $"RMSPSettingUtility.SetValue.{items.Count}", true))
            {
                bool hasError = false;
                if (items != null)
                {
                    IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;
                    taxValue.TermGuid = aveTerm.ID.ToString();
                    taxValue.Label = aveTerm.Name;

                    if (items.Count > itemsPerTask)
                    {
                        logger.Info("Use multi thread.");
                        var cts = new CancellationTokenSource();
                        hasError = RunMultiThreadsSetValue(items, itemsPerTask, cts, taxValue, aveTaxField, aveTerm, records, setting, labelUtility, excludePath, needChedkFileSystemObjectType, configSiteSetting);
                        return hasError;
                    }

                    foreach (var item in items)
                    {
                        bool isFailed = SetOneItemValue(item, taxValue, aveTaxField, aveTerm, records, setting, labelUtility, excludePath, needChedkFileSystemObjectType, configSiteSetting);
                        if (!hasError && isFailed)
                        {
                            hasError = true;
                        }
                    }
                }
                return hasError;
            }
        }

        private static bool RunMultiThreadsSetValue(IAveListItemCollection items, int itemsPerTask, CancellationTokenSource cts, IAveTaxonomyFieldValue taxValue, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, IAveORecords records, RMSharePointSetting setting, SPOLabelUtility labelUtility, List<string> excludePath = null, bool needChedkFileSystemObjectType = false, ConfigSiteSetting configSiteSetting = null)
        {
            bool hasError = false;
            AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
            {
                bool isFailed = SetOneItemValue(item, taxValue, aveTaxField, aveTerm, records, setting, labelUtility, excludePath, needChedkFileSystemObjectType, configSiteSetting);
                if (!hasError && isFailed)
                {
                    hasError = true;
                }
            });
            return hasError;
        }

        private static bool NeedSkip(IAveListItem item, List<string> excludePaths)
        {
            if (excludePaths != null && excludePaths.Count > 0)
            {
                string itemPath = item["FileRef"].ToString();
                foreach (var excludePath in excludePaths)
                {
                    var normalizedExcludePath = excludePath.TrimEnd('/') + "/";
                    if (JobContext.IsCSDTenant)
                    {
                        if (itemPath == excludePath || itemPath.StartsWith(normalizedExcludePath))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (itemPath.StartsWith(normalizedExcludePath))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static bool ShouldSkipArchivedItem(IAveListItem item)
        {
            if (item?.FieldValues == null)
            {
                return false;
            }

            if (!item.FieldValues.ContainsKey(FileArchiveStatusInternalName))
            {
                return false;
            }

            var archiveStatusValue = item.FieldValues[FileArchiveStatusInternalName];
            if (archiveStatusValue == null)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(archiveStatusValue.ToString());
        }

        private static DateTime CalculateDeletionDate(DateTime curTime, RetentionSetting rs)
        {
            switch (rs.Unit)
            {
                case PeriodUnit.Days:
                    return curTime.AddDays(rs.Value);
                case PeriodUnit.Months:
                    return curTime.AddMonths(rs.Value);
                case PeriodUnit.Years:
                    return curTime.AddYears(rs.Value);
                default:
                    throw new Exception("The unit in RetentionSetting is wrong.");
            }
        }

        private static string SerializeExtendsData(ExtendsData data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private static bool IsFile(IAveListItem item)
        {
            return item.FileSystemObjectType == AveFileSystemObjectType.File;
        }

        private static bool IsNormalFile(IAveListItem item, ConfigSiteSetting configSiteSetting)
        {
            return IsFile(item) || (IsOneNoteFolder(item) && !HandleOneNoteFolderWithModifiedClass(configSiteSetting));
        }

        private static bool IsCSDWhiteTerm(Guid termId, ConfigSiteSetting configSiteSetting)
        {
            return termId.Equals(configSiteSetting.ExcludedFileTypeDefaultTerm.ID);
        }

        private static bool IsCSDModifiedBasedClass(Guid termId, ConfigSiteSetting configSiteSetting)
        {
            return configSiteSetting.ModifiedBasedTermIds.Contains(termId);
        }

        private static bool IsFolder(IAveListItem item)
        {
            return item.FileSystemObjectType == AveFileSystemObjectType.Folder;
        }

        private static bool IsCSDRuleConfigured(Guid termId, ConfigSiteSetting configSiteSetting, out string tipMsg)
        {
            tipMsg = string.Empty;
            if (!configSiteSetting.CSDRules.ContainsKey(termId))
            {
                logger.Info($"There is no csd rule related to the term. TermId:[{termId.ToString()}]");
                tipMsg = "RM_JS_JMD_Comment_NoRule_Skip";
                return false;
            }
            else
            {
                var isModifiedTerm = IsCSDModifiedBasedClass(termId, configSiteSetting);
                if (!isModifiedTerm)
                {
                    if (configSiteSetting.CSDRules[termId].CreationRetentionSetting != null &&
                        configSiteSetting.CSDRules[termId].CreationRetentionSetting.Unit != PeriodUnit.None &&
                        configSiteSetting.CSDRules[termId].CreationRetentionSetting.Value > 0)
                    {
                        return true;
                    }
                }
                else
                {
                    if (configSiteSetting.CSDRules[termId].ModifiedBasedRetentionSetting != null &&
                        configSiteSetting.CSDRules[termId].ModifiedBasedRetentionSetting.Unit != PeriodUnit.None &&
                        configSiteSetting.CSDRules[termId].ModifiedBasedRetentionSetting.Value > 0)
                    {
                        return true;
                    }
                }
                tipMsg = "RM_JS_SS_CannotGetModifiedBasedRule";
                logger.Info($"There is no available CSD rule related to the term. TermId:[{termId.ToString()}] IsModifiedTerm:[{isModifiedTerm}]");
                return false;
            }
        }

        private static void ClearCSDVal4ModifiedClass(IAveListItem item)
        {
            item[CSDFieldName.DeletionDate] = null;
            if (item.Fields.ContainsField(CSDFieldName.EventDate))
            {
                item[CSDFieldName.EventDate] = null;
            }
            if (item.Fields.ContainsField(CSDFieldName.Comments))
            {
                item[CSDFieldName.Comments] = null;
            }
            if (item.Fields.ContainsField(CSDFieldName.ReclassDateOfModified2Creation))
            {
                item[CSDFieldName.ReclassDateOfModified2Creation] = null;
            }
        }

        private static void SetCSDDeletionDate(IAveListItem item, DateTime baseTime, RetentionSetting rs)
        {
            //计算DeletionDate
            var deletionDate = CalculateDeletionDate(baseTime, rs);
            item[CSDFieldName.DeletionDate] = deletionDate;
            //Clear EventDate、EventComment
            if (item.Fields.ContainsField(CSDFieldName.EventDate))
            {
                //For OneDrive
                item[CSDFieldName.EventDate] = null;
            }
            if (item.Fields.ContainsField(CSDFieldName.Comments))
            {
                //For OneDrive
                item[CSDFieldName.Comments] = null;
            }
        }

        private static void SetCSDExtendsColumn(IAveListItem item, Guid termId)
        {
            ExtendsData extendsData;
            if (item[CSDFieldName.Extends] != null && !string.IsNullOrEmpty(item[CSDFieldName.Extends].ToString()))
            {
                try
                {
                    extendsData = JsonConvert.DeserializeObject<ExtendsData>(item[CSDFieldName.Extends].ToString());
                }
                catch (Exception e)
                {
                    logger.Warn($"The value of {CSDFieldName.Extends} is illegal and job will create a new value. ItemID:[{item.ID}]. Exception: {e}");
                    extendsData = new ExtendsData();
                }
            }
            else
            {
                extendsData = new ExtendsData();
            }
            extendsData.KSUClass = termId.ToString();
            extendsData.Reclassified = DateTime.UtcNow.ToString(DATETIME_ISO_FORMAT);
            extendsData.ReclassifiedBy = "Records";
            item[CSDFieldName.Extends] = SerializeExtendsData(extendsData);
        }

        private static void SetCSDLabel(IAveListItem item, string labelName)
        {
            if (!DataCenterUtil.Is21V())
            {
                //添加Label (目前不用校验label是否存在)
                //item.SetComplianceTag(labelName, false, false, false, false);
                item.SetComplianceTagOnBulkItems(labelName);
            }
        }

        private static bool IsOldClassModifiedClass(IAveListItem item, ConfigSiteSetting configSiteSetting)
        {
            if (item[BCSColumnInternalName] != null)
            {
                var termStrVal = item[BCSColumnInternalName].ToString();
                var oldClass = new Guid(termStrVal.Substring(termStrVal.LastIndexOf('|') + 1));
                return IsCSDModifiedBasedClass(oldClass, configSiteSetting);
            }
            return false;
        }

        /// <summary>
        /// 兼容逻辑，list没运行脚本，没有必要的column时，走旧逻辑
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool HasDateOfM2CColumn(IAveListItem item)
        {
            //return item.Properties.Contains(CSDFieldName.ReclassDateOfModified2Creation) && item.Fields.ContainsField(CSDFieldName.ReclassDateOfModified2Creation);
            return item.Fields.ContainsField(CSDFieldName.ReclassDateOfModified2Creation);
        }

        private static void InitReclassDateFromModified2Creation(IAveListItem item)
        {
            item[CSDFieldName.ReclassDateOfModified2Creation] = DateTime.UtcNow;
        }

        private static bool HasReclassDateFromModified2Creation(IAveListItem item)
        {
            return !string.IsNullOrEmpty(item[CSDFieldName.ReclassDateOfModified2Creation]?.ToString());
        }

        private static void SetCSDVal4NormalFile(IAveListItem item, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, ConfigSiteSetting configSiteSetting)
        {
            DateTime baseTime;
            //var tagInfo = new AveItemComplianceTagInfo() { TagPolicyHold = false, TagPolicyRecord = false, EventBasedTag = false };
            string labelName;
            bool initReclassDateOfM2C = false;
            if (HasDateOfM2CColumn(item) && IsOldClassModifiedClass(item, configSiteSetting))
            {
                InitReclassDateFromModified2Creation(item);
                initReclassDateOfM2C = true;
            }
            if (HasDateOfM2CColumn(item) && (HasReclassDateFromModified2Creation(item) || initReclassDateOfM2C))
            {
                var label4M2C = configSiteSetting.CSDRules[aveTerm.ID].RetentionLabel4ReclassModified2Creation;
                if (!DataCenterUtil.Is21V() && string.IsNullOrEmpty(label4M2C))
                {
                    throw new Exception($"RM_JS_JMD_NoRetentionLabel4ReclassModified2Creation");
                }
                labelName = label4M2C;
                baseTime = initReclassDateOfM2C 
                    ? DateTime.UtcNow
                    : new DateTime(Convert.ToDateTime(item[CSDFieldName.ReclassDateOfModified2Creation]).Ticks, DateTimeKind.Utc);
            }
            else
            {
                labelName = configSiteSetting.CSDRules[aveTerm.ID].CreationRetentionSetting.RetentionLabel;
                baseTime = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.Created]).Ticks, DateTimeKind.Utc);
            }
            SetBCSValue(item, aveTaxField, aveTerm);
            SetCSDDeletionDate(item, baseTime, configSiteSetting.CSDRules[aveTerm.ID].CreationRetentionSetting);
            SetCSDExtendsColumn(item, aveTerm.ID);
            item.SystemUpdateForRecords();

            //item.SetComplianceTag(tagInfo);
            SetCSDLabel(item, labelName);
        }

        private static void SetCSDVal4NormalFolder(IAveListItem item, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, ConfigSiteSetting configSiteSetting)
        {
            SetBCSValue(item, aveTaxField, aveTerm);
            SetCSDExtendsColumn(item, aveTerm.ID);
            item.SystemUpdateForRecords();
        }

        private static void SetCSDVal4CreationClass(IAveListItem item, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, ConfigSiteSetting configSiteSetting)
        {
            if (IsNormalFile(item, configSiteSetting))
            {
                SetCSDVal4NormalFile(item, aveTaxField, aveTerm, configSiteSetting);
            }
            else
            {
                SetCSDVal4NormalFolder(item, aveTaxField, aveTerm, configSiteSetting);
            }
        }

        private static void SetCSDColumnValues(IAveListItem item, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, ConfigSiteSetting configSiteSetting)
        {
            var isNewClassModifiedClass = IsCSDModifiedBasedClass(aveTerm.ID, configSiteSetting);
            if (isNewClassModifiedClass)
            {
                SetCSDValWithModifiedClass(item, aveTaxField, aveTerm, configSiteSetting);
            }
            else
            {
                SetCSDVal4CreationClass(item, aveTaxField, aveTerm, configSiteSetting);
            }
        }

        private static bool SetOneItemValue4NormalTenant(IAveListItem item, IAveTaxonomyFieldValue taxValue, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, IAveORecords records, RMSharePointSetting setting, SPOLabelUtility labelUtility, List<string> excludePath = null, bool needChedkFileSystemObjectType = false, ConfigSiteSetting configSiteSetting = null)
        {
            using (new PerformanceScope("RMSPSettingUtility.SetOneItem", $"RMSPSettingUtility.SetOneItem.{item.ID}", true))
            {
                if (JobContext.IsCSDTenant)
                {
                    logger.Error("This method is only for normal tenant.");
                    return true;
                }
                bool hasError = false;
                logger.Info("Set Item default value {0}", item.ID);
                string itemFullUrl = item.ParentList.ParentWeb.Url + "/" + item.Url;
                try
                {
                    ReportManager.Increase();
                    if (setting.ApplyExistType == (int)ApplyExistingTermType.SkipAndKeep)
                    {
                        if (item.FieldValues.ContainsKey(aveTaxField.InternalName) && item.FieldValues[aveTaxField.InternalName] != null)
                        {
                            logger.Info($"skip to set default value:{item?.ID}");
                            return hasError;
                        }
                    }

                    if (NeedSkip(item, excludePath))
                    {
                        logger.Info($"Skip current item, for it's ancestor folder has own setting. ItemPath:[{item["FileRef"]}]");
                        return hasError;
                    }

                    if (ShouldSkipArchivedItem(item))
                    {
                        return hasError;
                    }
                    var isUpdateDeclared = false;
                    if (IsBlockEditAndDeleteRecord(item))
                    {
                        logger.Info("Item is Block Edit and delete {0}", item.Name);
                        //*****ReportService.Commit(new SPSettingJobReportEntry(item.Name, item.Url, "",
                        //string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Skipped, "RM_SS_ItemBlockEditAndDelete"));
                        if (setting.IncludeDeclaredRecords)
                        {
                            isUpdateDeclared = true;
                        }
                        else
                        {
                            if (JobContext.IsCSDTenant)
                            {
                                SendSPSettingReport(item.Name, item.Url, "RM_SS_ApplyExist", JobDetailsStatus.Skipped, "RM_SS_ItemBlockEditAndDelete");
                            }
                            return hasError;
                        }

                    }

                    //判断当前是否是Discussion item
                    var contentTypeId = item["ContentTypeId"] as string;
                    var isDiscussion = contentTypeId != null && contentTypeId.StartsWith("0x012002");
                    if (needChedkFileSystemObjectType && item.FileSystemObjectType == AveFileSystemObjectType.Folder && !isDiscussion)
                    {
                        logger.Info("skip item is Folder, itemId: {0}", item.ID);
                        return hasError;
                    }

                    if (setting.IsApplyTermIncludeFolder() && !setting.IsApplyDocuments() && item.FileSystemObjectType != AveFileSystemObjectType.Folder)
                    {
                        logger.Info("skip item because only apply folder is checked, itemId: {0}", item.ID);
                        return hasError;
                    }

                    if (isUpdateDeclared)
                    {
                        try
                        {
                            WaitExecuteAction(() =>
                            {
                                records.UndeclareItemAsRecord(item);
                            });
                        }
                        catch (Exception e)
                        {
                            logger.Error("undeclare item failed [{0}]:{1}", item?.Url, e.ToString());
                            throw;
                        }
                    }
                    string oldValue = item[aveTaxField.InternalName] == null ? null : ((string)item[aveTaxField.InternalName]).ToLowerInvariant();
                    Guid oldTermId = Guid.Empty;
                    try
                    {
                        if (oldValue != null && !string.IsNullOrEmpty(oldValue.ToString()))
                        {
                            var valueString = oldValue.ToString().Split('|');
                            if (valueString.Length > 1)
                            {
                                oldTermId = new Guid(valueString[1]);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Convert to guid failed. Error : {e}");
                    }

                    bool labelNotExist = false;

                    if (string.IsNullOrEmpty(oldValue) || !oldValue.Contains(taxValue.TermGuid.ToLowerInvariant()))
                    {
                        try
                        {
                            item[aveTaxField.ID] = taxValue;
                            item[aveTaxField.TextField] = taxValue.ToString();
                            WaitExecuteAction(() =>
                            {
                                item.SystemUpdateForRecords();
                            });
                            using (new PerformanceScope("RMSPSettingUtility.UpdateLabelTotal", addToStatistics: true))
                            {
                                var recId = IDGenerator.GetRecordId(item.ParentList.ParentWeb.Site.ID, item.UniqueId);
                                if (aveTerm != null)
                                {
                                    labelNotExist = labelUtility.UpdateLabel(item, aveTerm.ID, recId, oldTermId);
                                }
                            }
                            logger.Info($"apply term to item success:{item?.ID}");
                        }
                        catch (Exception e)
                        {
                            logger.Error("update item failed [{0}]:{1}", itemFullUrl, e.ToString());
                            throw;

                        }
                        try
                        {
                            if (isUpdateDeclared)
                            {
                                using (PerformanceScope scope = new PerformanceScope("SetValue.DeclareItemAsRecord", "", true))
                                {

                                    WaitExecuteAction(() =>
                                    {
                                        var dItem = item.ParentList.GetItemById(item.ID);
                                        WaitExecuteAction(() =>
                                        {
                                            records.DeclareItemAsRecord(dItem);
                                        });
                                    });
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("declare item failed: [{0}]:{1}", itemFullUrl, e.ToString());
                            throw;
                        }
                    }

                    if (!JobContext.IsCSDTenant && labelNotExist)
                    {
                        JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
                        detail.ObjectName = item?.Name;
                        detail.SourceURL = itemFullUrl;
                        detail.Action = "RM_SS_ApplyExist";
                        detail.Status = JobDetailsStatus.Failed;
                        detail.Comment = "RM_SPO_ApplySetting_LabelNotExist";
                        ReportManager.SendJobDetail(detail);
                        HasFailedReport = true;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Set Item default value failed [{0}]:{1}", itemFullUrl, e.ToString());
                    JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
                    detail.ObjectName = item?.Name;
                    detail.SourceURL = itemFullUrl;
                    detail.Action = "RM_SS_ApplyExist";
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = GetExceptionMessage(e);
                    ReportManager.SendJobDetail(detail);
                    hasError = true;
                }
                return hasError;
            }
        }

        private static bool SetOneItemValue4CSDTenant(IAveListItem item, IAveTaxonomyFieldValue taxValue, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, IAveORecords records, RMSharePointSetting setting, SPOLabelUtility labelUtility, List<string> excludePath = null, bool needChedkFileSystemObjectType = false, ConfigSiteSetting configSiteSetting = null)
        {
            using (new PerformanceScope("RMSPSettingUtility.SetOneItem", $"RMSPSettingUtility.SetOneItem.{item.ID}", true))
            {
                if (!JobContext.IsCSDTenant)
                {
                    logger.Error("This method is only for CSD tenant.");
                    return true;
                }
                bool hasError = false;
                logger.Info("Set Item default value {0}", item.ID);
                string itemFullUrl = item.ParentList.ParentWeb.Url + "/" + item.Url;
                try
                {
                    ReportManager.Increase();
                    if (setting.ApplyExistType == (int)ApplyExistingTermType.SkipAndKeep)
                    {
                        if (item.FieldValues.ContainsKey(aveTaxField.InternalName) && item.FieldValues[aveTaxField.InternalName] != null)
                        {
                            logger.Info($"skip to set default value:{item?.ID}");
                            return hasError;
                        }
                    }

                    if (NeedSkip(item, excludePath))
                    {
                        logger.Info($"Skip current item, for it's ancestor folder has own setting. ItemPath:[{item["FileRef"]}]");
                        return hasError;
                    }

                    if (ShouldSkipArchivedItem(item))
                    {
                        return hasError;
                    }
                    var isUpdateDeclared = false;
                    if (IsBlockEditAndDeleteRecord(item))
                    {
                        logger.Info("Item is Block Edit and delete {0}", item.Name);
                        //*****ReportService.Commit(new SPSettingJobReportEntry(item.Name, item.Url, "",
                        //string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Skipped, "RM_SS_ItemBlockEditAndDelete"));
                        if (setting.IncludeDeclaredRecords)
                        {
                            isUpdateDeclared = true;
                        }
                        else
                        {
                            if (JobContext.IsCSDTenant)
                            {
                                SendSPSettingReport(item.Name, item.Url, "RM_SS_ApplyExist", JobDetailsStatus.Skipped, "RM_SS_ItemBlockEditAndDelete");
                            }
                            return hasError;
                        }

                    }

                    //判断当前是否是Discussion item
                    var contentTypeId = item["ContentTypeId"] as string;
                    var isDiscussion = contentTypeId != null && contentTypeId.StartsWith("0x012002");
                    if (needChedkFileSystemObjectType && item.FileSystemObjectType == AveFileSystemObjectType.Folder && !isDiscussion)
                    {
                        logger.Info("skip item is Folder, itemId: {0}", item.ID);
                        return hasError;
                    }

                    if (setting.IsApplyTermIncludeFolder() && !setting.IsApplyDocuments() && item.FileSystemObjectType != AveFileSystemObjectType.Folder)
                    {
                        logger.Info("skip item because only apply folder is checked, itemId: {0}", item.ID);
                        return hasError;
                    }

                    if (JobContext.IsCSDTenant)
                    {
                        if (!isUpdateDeclared && IsCheckOut(item))
                        {
                            throw new Exception("RM_JS_SPS_FileIsCheckOut");
                        }
                        if (ProcessCSDWhiteFile(item, aveTaxField, configSiteSetting, "RM_SS_ApplyExist"))
                        {
                            return hasError;
                        }
                        if (ProcessCSDModifiedBasedFile(item, aveTaxField, configSiteSetting, "RM_SS_ApplyExist"))
                        {
                            return hasError;
                        }
                        if (IsCSDWhiteTerm(aveTerm.ID, configSiteSetting))
                        {
                            SendSPSettingReport(item.Name, item.Url, "RM_SS_ApplyExist",
                                JobDetailsStatus.Skipped, "RM_JS_JMD_CannotUseSpecialClassReclassify");
                            return hasError;
                        }
                        if (!IsCSDRuleConfigured(aveTerm.ID, configSiteSetting, out string tipMsg))
                        {
                            SendSPSettingReport(item.Name, item.Url, "RM_SS_ApplyExist", JobDetailsStatus.Skipped, tipMsg);
                            return hasError;
                        }
                        if (isUpdateDeclared && !CheckCSDDeletionDataForLockFile(item, aveTerm.ID, "RM_SS_ApplyExist", configSiteSetting))
                        {
                            return hasError;
                        }
                    }

                    SetCSDColumnValues(item, aveTaxField, aveTerm, configSiteSetting);
                    SendSPSettingReport(item.Name, itemFullUrl, "RM_SS_ApplyExist", JobDetailsStatus.Successful);
                    logger.Info($"apply term to item success:{item?.ID}");
                }
                catch (Exception e)
                {
                    logger.Error("Set Item default value failed [{0}]:{1}", itemFullUrl, e.ToString());
                    JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
                    detail.ObjectName = item?.Name;
                    detail.SourceURL = itemFullUrl;
                    detail.Action = "RM_SS_ApplyExist";
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = GetExceptionMessage(e);
                    ReportManager.SendJobDetail(detail);
                    hasError = true;
                }
                return hasError;
            }
        }

        private static bool SetOneItemValue(IAveListItem item, IAveTaxonomyFieldValue taxValue, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, IAveORecords records, RMSharePointSetting setting, SPOLabelUtility labelUtility, List<string> excludePath = null, bool needChedkFileSystemObjectType = false, ConfigSiteSetting configSiteSetting = null)
        { 
            if (JobContext.IsCSDTenant)
            {
                return SetOneItemValue4CSDTenant(item, taxValue, aveTaxField, aveTerm, records, setting, labelUtility, excludePath, needChedkFileSystemObjectType, configSiteSetting);
            }
            else
            {
                return SetOneItemValue4NormalTenant(item, taxValue, aveTaxField, aveTerm, records, setting, labelUtility, excludePath, needChedkFileSystemObjectType, configSiteSetting);
            }
        }
        

        private static ApplySettingPredictResult GetPredictResult(IAveListItem item, RMSharePointSetting setting)
        {
            ApplySettingPredictResult result = new();
            result.Source = SettingsHelpers.GetSourceFlag();
            var predictTerm = RMMachineLearningUtility.GetFilePredictTerm(item.UniqueId);
            if (predictTerm != null)
            {
                if (setting.AIApprovalType == DB.Model.ApprovalType.None)
                {
                    //需要同步sp数据到cosmosdb, 并标记是ai打的term
                    result.TermId = predictTerm.Id;
                    result.TermName = predictTerm.Name;
                    result.TermScore = predictTerm.PredictTermScore;
                    result.UnderReviewMethod = RMMLUnderReview.DirectAssign;
                    result.IsUpdateSharePoint = true;
                    result.IsSyncCosmosDB = true;
                    logger.Info($"direct set item value use predictTerm, itemId: [{item.ID}], itemUniqueId: [{item.UniqueId}], predictTermId: [{predictTerm.Id}]");
                }
                else if (setting.AIApprovalType == DB.Model.ApprovalType.RecordOwners)
                {
                    logger.Info($"use ai manual of current item, itemId: [{item.ID}], itemUniqueId:[{item.UniqueId}], predictTermId: [{predictTerm.Id}]");
                    if (predictTerm.AutoApply)
                    {
                        //需要同步sp数据到cosmosdb，当前数据使用的训练Term开启了AutoApply, 不走Manual流程
                        result.TermId = predictTerm.Id;
                        result.TermName = predictTerm.Name;
                        result.TermScore = predictTerm.PredictTermScore;
                        result.UnderReviewMethod = RMMLUnderReview.DirectAssign;
                        result.IsUpdateSharePoint = true;
                        result.IsSyncCosmosDB = true;
                        logger.Info($"set item value use predictTerm, because the predictTerm autoApply is [{predictTerm.AutoApply}], itemId: [{item.UniqueId}], predictTermId: [{predictTerm.Id}]");
                    }
                    else
                    {
                        //需要同步sp数据到cosmosdb，并标记是Manual状态的数据
                        result.TermId = predictTerm.Id;
                        result.TermName = predictTerm.Name;
                        result.TermScore = predictTerm.PredictTermScore;
                        result.UnderReviewMethod = RMMLUnderReview.IsManual;
                        result.IsSyncCosmosDB = true;
                    }
                }
            }
            else
            {
                //AI预测Term没有结果时处理逻辑
                logger.Info($"there are no prediction results, itemId: [{item.ID}], itemUniqueId: [{item.UniqueId}]");
                if (setting.AIThenIsDefaultTermMethod)
                {
                    result.TermId = setting.AIThenDefaultTermId;
                    result.TermName = setting.AIThenDefaultTermName;
                    result.IsUpdateSharePoint = true;
                    result.IsApplyDefaultTerm = true;
                    logger.Info($"when there are no prediction results, use then default term, itemUniqueId: [{item.UniqueId}], termId: [{setting.AIThenDefaultTermId}]");
                }
                else
                {
                    logger.Info($"when there are no prediction results, use manual choose term, itemUniqueId: [{item.UniqueId}]");
                }
            }
            return result;
        }

        private static bool IsFileExtentionInExculdeList(List<string> excludeFileExtention, IAveListItem item)
        {
            if (IsFolder(item))
            {
                return false;
            }
            var extention = item.Name.Substring(item.Name.LastIndexOf('.') + 1);
            if (excludeFileExtention.Contains(extention.ToLowerInvariant()))
            {
                return true;
            }
            return false;
        }

       /* private static bool CheckItemExistEventDate(IAveListItem item)
        {
            var eventDate = DateTime.MinValue;
            object objVal;
            if (item.FieldValues.TryGetValue(CSDFieldName.EventDate, out objVal))
            {
                var dt = objVal as DateTime?;
                if (dt != null)
                {
                    eventDate = dt.Value;
                    return true;
                }
            }
            return false;
        }*/

        private static void SetExculdeListFileDefaultValue(IAveListItem item, IAveTaxonomyField aveTaxField, ConfigSiteSetting configSiteSetting)
        {
            var excludedFileTypeDefaultTerm = configSiteSetting.ExcludedFileTypeDefaultTerm;
            if (excludedFileTypeDefaultTerm != null)
            {
                logger.Info($"File extention is in ExcludeFileExtention, so set Item Value in Configuration settings. Name:[{item.Url}], Term:[{excludedFileTypeDefaultTerm.Name}]");
                IAveTaxonomyFieldValue whiteListItemTaxValue = aveTaxField.TaxonomyFieldValue;
                whiteListItemTaxValue.TermGuid = excludedFileTypeDefaultTerm.ID.ToString();
                whiteListItemTaxValue.Label = excludedFileTypeDefaultTerm.Name;
                item[aveTaxField.ID] = whiteListItemTaxValue;
                item[aveTaxField.TextField] = whiteListItemTaxValue.ToString();

                item[CSDFieldName.DeletionDate] = null;
                SetCSDExtendsColumn(item, excludedFileTypeDefaultTerm.ID);
                WaitExecuteAction(() =>
                {
                    item.SystemUpdateForRecords();
                });
            }
        }

        private static void WaitExecuteAction(Action action)
        {
            _callLimiter.WaitCallLimitPerSecond();
            action();
        }

        #region config bcs  column for folder
        public static void RemoveFolderDefaultValue(IAveList list, string folderUrl, string columnInternalName)
        {
            try
            {
                IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(list.ParentWeb.Site, columnInternalName);
                mDefaults.RemoveFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, WebUtil.MakeServerRelativeUrl(folderUrl));
                logger.Info("remove folder default value {0}", folderUrl);
            }
            catch (Exception e)
            {
                logger.Warn($"Remove Folder Default Value From '/forms/client_LocationBasedDefaults.html'. FolderPath:[{folderUrl}] Error:{e.ToString()}");
            }
        }
        public static void RemoveFolderDefalutValue(IAveFolder folder, IAveList list, RMSharePointSetting setting)
        {
            using (var scope = new PerformanceScope("RMSPSettingUtility.RemoveFolderDefaultValue", $"RMSPSettingUtility.RemoveFolderDefaultValue.{folder.Name}", true))
            {
                try
                {
                    var defaultValues = GetXmlWithFolderDefaultValue(folder.ParentList);
                    if (!string.IsNullOrEmpty(defaultValues))
                    {
                        logger.Info("'/forms/client_LocationBasedDefaults.html' is  exist.");
                        var columnInternalName = BCSColumnInternalName;
                        if (setting.IsUsingExistColumnName)
                        {
                            columnInternalName = list.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault()?.InternalName;
                            columnInternalName ??= list.Fields.Where(f => f.InternalName == setting.ExistColumnName).FirstOrDefault()?.InternalName;
                        }
                        IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(list.ParentWeb.Site, columnInternalName);
                        var existFolderDefaultValue = string.Empty;
                        try
                        {
                            existFolderDefaultValue = mDefaults.GetFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl);
                            logger.Info("Get Field Default Value is empty, folder url:{0}", folder.ServerRelativeUrl);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Get Field Default Value error, folder server relative url: {0}, ERROR:{1}", folder.ServerRelativeUrl, ex.ToString());
                        }

                        if (!string.IsNullOrEmpty(existFolderDefaultValue))
                        {
                            mDefaults.RemoveFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl);
                            logger.Info("remove folder default value {0}", folder.ServerRelativeUrl);
                        }
                        else
                        {
                            logger.Info("No Need remove folder default value {0}", folder.ServerRelativeUrl);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Remove Folder Default Value From '/forms/client_LocationBasedDefaults.html' error:{0}", e.ToString());
                }
            }
        }

        public static RMSharePointSetting LoadParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMSharePointSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.Farm)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = SettingsHelpers.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadParentSeting(node.Parent, siteId);
            }

            return SPSetting;
        }

        public static SettingResult ConfigBCSColumn(IAveSite site, IAveList list, IAveFolder folder, RMSharePointSetting setting, ref IAveTaxonomyField taxField, ConfigSiteSetting configSiteSetting = null)
        {
            using (var scope = new PerformanceScope("RMSPSettingUtility.ConfigBCSColumn4Folder", $"RMSPSettingUtility.ConfigBCSColumn4Folder.{folder.Name}", true))
            {
                logger.Info($"FullPath:[{setting.FullPath}] IsUsingExistColumn:[{setting.IsUsingExistColumnName}] ExistingCoumnName:[{setting.ExistColumnName}] Configure term settings in Records:[{setting.SetDocLevelTermForExistColumn}]");
                SettingResult result = SettingResult.SKip;
                if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    if (!CheckClassificationSetting(setting, site))
                    {
                        throw new Exception("Term Is Unavailable");
                    }

                    IAveTaxonomyField siteField = null;
                    Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
                    FieldConflict listConflict = VerifyFieldConflict(site, list.Fields, setting, ref taxField);
                    result = HandleListFieldConflict(listConflict, site, list, setting, siteField, ref taxField, configSiteSetting);
                    if (taxField == null)
                    {
                        return result;
                    }
                    else
                    {
                        EnsureFieldAdded2AllContentTypes(taxField, list);
                    }
                    //var columnSetting = GetParentColumnSetting(setting);
                    RMSPTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                    var columnSetting = LoadParentSeting(dbNodeInfo, setting.SiteId);
                    if (result == SettingResult.Add)   //ATSB FullJob重复Init List Column, 出现问题, 只在Add时更新一次
                    {
                        if (JobContext.IsCSDTenant)
                        {
                            InitTaxnomyField(taxField, columnSetting, termStoreId, false, lcid: list.ParentWeb.GetWorkingLanguage(), settingResult: ref result);
                        }
                        else
                        {
                            InitTaxnomyField(taxField, columnSetting, termStoreId, false, settingResult: ref result);
                        }
                    }
                    if ((DeployTermMethod)columnSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm &&
                                columnSetting.DefaultTermId != null && columnSetting.DefaultTermId != Guid.Empty 
                                && (!columnSetting.IsUsingExistColumnName || (columnSetting.IsUsingExistColumnName && columnSetting.SetDocLevelTermForExistColumn)))
                    {
                        if (taxField.DefaultValue == null || taxField.DefaultValue.StartsWith("-1"))
                        {
                            logger.Info("Folder need to update parent list column default value {0}", taxField.DefaultValue);
                            UpdateBCSColumnDefaultValue(list, columnSetting, taxField);
                        }
                        else
                        {
                            logger.Info("Parent list default value:{0}", taxField.DefaultValue);
                        }
                    }

                    //}

                    //folder
                    var isKeepSPDefaultValue = IsKeepSPDefaultValue(setting);

                    if (setting.DefaultTermId == Guid.Empty)
                    {
                        try
                        {
                            var defaultValues = GetXmlWithFolderDefaultValue(list);
                            if (!isKeepSPDefaultValue && !string.IsNullOrEmpty(defaultValues))
                            {
                                logger.Info("'/forms/client_LocationBasedDefaults.html' is  exist.");
                                IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(site, taxField.InternalName);
                                mDefaults.RemoveFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl);
                                result = SettingResult.Update;
                            }
                            else 
                            {
                                result = SettingResult.SKip;
                                logger.Info("Skip remove folder default value ,folder path:{0}", folder.ServerRelativeUrl);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Remove Folder Default Value From '/forms/client_LocationBasedDefaults.html' error:{0}", e.ToString());
                        }
                    }
                    else
                    {
                        logger.Info("Start to set default term value for classification column");
                        var existFolderDefaultValue = string.Empty;
                        try
                        {
                            IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(site, taxField.InternalName);
                            existFolderDefaultValue = mDefaults.GetFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Get Field Default Value error, do add logic,path: {0}", setting.FullPath);
                        }

                        string wssId = GetTermWssId(site, setting.DefaultTermName, setting.DefaultTermId);
                        if (wssId == "-1")
                        {
                            logger.Info("Term id {0}, name {1}, never used in this site", setting.DefaultTermId, setting.DefaultTermName);
                            try
                            {
                                AveItemCreationInformation info = new AveItemCreationInformation()
                                {
                                    UnderlyingObjectType = AveFileSystemObjectType.Folder,
                                    FolderUrl = string.Concat("Temporary_Folder_For_WssId_Creation_", DateTime.Now.ToFileTime().ToString()),
                                    LeafName = string.Concat("Temporary_Folder_For_WssId_Creation_", DateTime.Now.ToFileTime().ToString())     //使用item.SystemUpate必须赋值， 否则FileRef Not Found
                                };
                                var item = list.AddItem(info);
                                var term = list.ParentWeb.Site.AveSPTaxonomySession.GetTerm(setting.DefaultTermId);

                                #region  try to use term
                                IAveTaxonomyFieldValue taxValue = taxField.TaxonomyFieldValue;
                                taxValue.TermGuid = term.ID.ToString();
                                taxValue.Label = term.Name;
                                try
                                {
                                    logger.Info("temp taxonomy value: {0}", taxValue.ToString());
                                    item[taxField.ID] = taxValue;
                                    item[taxField.TextField] = taxValue.ToString();
                                    item.SystemUpdate();
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn("UpdateBCSColumnDefaultValue failed {0}:{1} error {2}", list.Title, term.Name, ex.ToString());
                                }
                                item.Delete();
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                logger.Warn("Add item for get wssid error:{0}", ex.ToString());
                            }
                        }
                        wssId = GetTermWssId(list.ParentWeb.Site, setting.DefaultTermName, setting.DefaultTermId);
                        if (wssId == "-1")
                        {
                            throw new Exception(string.Format("Term not found in the term store,termStoreId:{0},TermSet:{1},TermName:{2},DefaultTermName:{3}", setting.TermStoreId, setting.TermSetName, setting.TermName, setting.DefaultTermName));
                        }
                        string folderDefaultValue = wssId + ";#" + setting.DefaultTermName + "|" + setting.DefaultTermId;

                        if (folderDefaultValue == existFolderDefaultValue)
                        {
                            logger.Info("Folder default value does no change, {0}", folderDefaultValue);
                            result = SettingResult.SKip;
                            //this.AddDetailToList(setting.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_SkipFolderColumn"), JobDetailsStatus.Skipped, null);
                        }
                        else
                        {
                            var useRecordDefaultTermSetting = setting.SetTermForEmptyDefaultValue && string.IsNullOrEmpty(existFolderDefaultValue);
                            if (isKeepSPDefaultValue && !useRecordDefaultTermSetting)
                            {
                                result = SettingResult.SKip;
                                logger.Info("Skip update folder column ,folder path:{0}", folder.ServerRelativeUrl);
                            }
                            else 
                            {
                                IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(site, taxField.InternalName);
                                mDefaults.SetFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl, folderDefaultValue);

                                result = SettingResult.Add;
                                if (string.IsNullOrEmpty(existFolderDefaultValue))
                                {
                                    logger.Info("Add folder column success,folder path:{0}", folder.ServerRelativeUrl);
                                    //this.AddDetailToList(folder.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddFolderColumn"), JobDetailsStatus.Successful, null);
                                }
                                else
                                {
                                    logger.Info("update folder column success,folder path:{0}", folder.ServerRelativeUrl);
                                    //this.AddDetailToList(folder.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateFolderColumn"), JobDetailsStatus.Successful, null);
                                }
                            }
                        }
                        //if ((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.UseDefaultTerm &&
                        //setting.DefaultTermId != null && setting.DefaultTermId != Guid.Empty)
                        //{
                        //    ApplyExistItems(list, folder, taxField, setting);
                        //}
                    }
                }
                else
                {
                    result = SettingResult.Delete;
                }
                return result;
            }
        }

        public static IAveTaxonomyField GetTaxonomyField(IAveList list, RMSharePointSetting setting)
        {
            IAveField listField;
            if (setting.IsUsingExistColumnName)
            {
                listField = list.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault();
                listField ??= list.Fields.Where(f => f.InternalName == setting.ExistColumnName).FirstOrDefault();
            }
            else
            {
                listField = list.Fields.GetFieldById(BCSColumnID, false);
            }
            var taxField = listField as IAveTaxonomyField;
            return taxField;
        }


        #endregion
        #endregion

        #region Auto-Classification
        private static AveCamlQuery GetAutoClassificationQuery(IAveList list, IAveFolder folder, RMSharePointSetting setting, DateTime startTime, DateTime endTime, string columnInternalName, int startIndex, int endIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            try
            {
                query.LoadAllItems = false;
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                string queryStr = string.Empty;

                CAMLManager cm = new CAMLManager(GetQueryScopeType(setting));
                var group = new QueryGroup();

                switch ((AutoJobOption)setting.AutoJobOption)
                {
                    case AutoJobOption.None:
                    case AutoJobOption.SkipAndKeep:
                        if (setting.RunAutoFullJob || startTime.Equals(DateTime.MinValue))
                        {
                            #region full job query string

                            AddMMSValueNotNullCondition(group, columnInternalName);
                            AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);
                            #endregion

                        }
                        else
                        {
                            #region incremental job query string 
                            AddMMSValueNotNullCondition(group, columnInternalName);
                            AddTimeContidion(group, startTime, endTime);
                            AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);
                            #endregion
                        }
                        break;
                    case AutoJobOption.Override:
                        if (setting.RunAutoFullJob || startTime.Equals(DateTime.MinValue))
                        {

                            AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);
                        }
                        else
                        {
                            AddTimeContidion(group, startTime, endTime);
                            AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);

                        }
                        break;
                    default:
                        break;
                }
                cm.QueryGroup.AddGroup(group);
                string queryXml = cm.GetFullCAML(true);
                query.ViewXml = queryXml;
                query.DatesInUtc = true;
                logger.Info($"Process Folder {folder.ServerRelativeUrl}, startTime:{startTime}, endTime:{endTime} query xml {queryXml}");
                logger.Info("Query XML:{0}", query.ViewXml);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting items with caml query, ERROR:{0}", ex.ToString());
            }
            return query;
        }

        protected static void AddMMSValueNotNullCondition(QueryGroup group, string columnInternalName)
        {
            group.Conditions.Add(new QueryCondition(
              Types.JoinTypes.And,
              Types.FieldRefTypes.Name,
              columnInternalName,
              Types.FieldTypes.MMSData,
              Types.QueryTypes.IsNull,
              "",
                          false));
        }
        protected static void AddTimeContidion(QueryGroup group, DateTime startTime, DateTime endTime)
        {
            group.Conditions.Add(new QueryCondition(
               Types.JoinTypes.And,
               Types.FieldRefTypes.Name,
               SPBuiltInFieldName.ModifiedTime,
               Types.FieldTypes.DateTime,
               Types.QueryTypes.FromTo,
               CreateISO8601DateTimeFromSystemDateTime(startTime),
                CreateISO8601DateTimeFromSystemDateTime(endTime),
                           true));
        }
        protected static void AddRowLimitQueryCondition(CAMLManager cm, QueryGroup group, int startIndex, int endIndex, int QueryConditionMaxCount)
        {
            //cm.ScopeType = Types.ScopeTypes.Default;
            cm.RowLimit = QueryConditionMaxCount;
            group.Conditions.Add(new QueryCondition(
                              Types.JoinTypes.And,
                              Types.FieldRefTypes.Name,
                               "ID",
                             Types.FieldTypes.Number,
                             Types.QueryTypes.Leq,
                              endIndex.ToString(), false));
            group.Conditions.Add(new QueryCondition(
                                 Types.JoinTypes.And,
                                 Types.FieldRefTypes.Name,
                                 "ID",
                                 Types.FieldTypes.Number,
                                  Types.QueryTypes.Gt,
                                 startIndex.ToString(), false));
        }
        public static void Autoclassification(Guid remoteSiteId, IAveList list, IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointSetting setting, DateTime startTime, DateTime endTime, IAveORecords records, ref bool hasError, SPOLabelUtility labelUtility, ConfigSiteSetting configSiteSetting = null, bool setTermForFolderSelf = false)
        {
            using (var autoScope = new PerformanceScope("AutoClassification", $"Auto Classification {list.ID}{folder?.ServerRelativeUrl}", true))
            {
                List<string> excludePath = SettingsHelpers.GetExcludePath(remoteSiteId, list);
                excludePath = excludePath.Where(p => p.StartsWith(folder.ServerRelativeUrl.TrimEnd('/') + "/")).ToList();

                List<ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules);
                Dictionary<Guid, IAveTerm> aveTerms = GetAveTerms(list, autoRules);
                Dictionary<string, Guid> ruleTermIdMapping = new Dictionary<string, Guid>();
                RuleCollection ruleCollection = GetRuleCollection(autoRules, ref ruleTermIdMapping);
                RuleManagement ruleManagement = new RuleManagement(ruleCollection);

                if (setTermForFolderSelf)
                {
                    logger.Info($"Set auto for folder self. Folder Url:[{folder.Url}]");
                    AutoSetOneItem(folder.Item, list, excludePath, aveTaxField, records, setting, ruleManagement, ruleTermIdMapping, aveTerms, configSiteSetting, labelUtility, remoteSiteId);
                }

                bool needQueryNext = false;
                int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
                int maxItemId = GetLastItemId(list, list.RootFolder);
                logger.Info($"The max ID of item in library is: {maxItemId}");

                int startIndex = 0;
                IAveListItemCollection items = null;
                do
                {
                    using (var queryAuto = new PerformanceScope("QueryAutoData", $"{folder.ServerRelativeUrl} start{startIndex}", true))
                    {
                        AveCamlQuery query = GetAutoClassificationQuery(list, folder, setting, startTime, endTime, aveTaxField.InternalName, startIndex, startIndex + rowLimit, rowLimit);
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            items = list.GetItemsForRecords(query);
                        }
                        ReportManager.IncreaseBase(items.Count);
                        logger.Info($"AutoJob process folder url {folder.ServerRelativeUrl} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");
                    }
                    using (var queryAuto = new PerformanceScope("SetAutoData", $"{folder.ServerRelativeUrl} count {items.Count}", true))
                    {
                        hasError = AutoSetValues(items, list, excludePath, aveTaxField, records, setting, ruleManagement, ruleTermIdMapping, aveTerms, configSiteSetting, labelUtility, remoteSiteId);
                    }
                    if (startIndex + rowLimit < maxItemId)
                    {
                        needQueryNext = true;
                        startIndex += rowLimit;
                        logger.Info($"PagingInfo:{startIndex}");
                    }
                    else
                    {
                        needQueryNext = false;
                    }
                }
                while (needQueryNext);
            }
            //logger.Info($"Finish to process auto classification. Path:[{folder?.ServerRelativeUrl}]");
        }

        /// <summary>
        /// if there is any error, return true, or else return false
        /// </summary>
        /// <param name="items"></param>
        /// <param name="list"></param>
        /// <param name="excludePath"></param>
        /// <param name="aveTaxField"></param>
        /// <param name="records"></param>
        /// <param name="setting"></param>
        /// <param name="ruleManagement"></param>
        /// <param name="ruleTermIdMapping"></param>
        /// <param name="aveTerms"></param>
        /// <param name="configSiteSetting"></param>
        /// <returns></returns>
        private static bool AutoSetValues(IAveListItemCollection items, IAveList list, List<string> excludePath, IAveTaxonomyField aveTaxField, IAveORecords records, RMSharePointSetting setting, RuleManagement ruleManagement, Dictionary<string, Guid> ruleTermIdMapping, Dictionary<Guid, IAveTerm> aveTerms, ConfigSiteSetting configSiteSetting, SPOLabelUtility labelUtility, Guid remoteSiteId)
        {
            RMMLAutoSmartItemsCache.Instance.Init(ProcessSmartAutoCacheItemsAction);
            var hasError = false;
            if (items.Count > itemsPerTask)
            {
                logger.Info("Use multi thread to run auto classification.");
                AveTenantTasks.RunParallel(items, itemsPerTask, new CancellationTokenSource(), item =>
                {
                    if (AutoSetOneItem(item, list, excludePath, aveTaxField, records, setting, ruleManagement, ruleTermIdMapping, aveTerms, configSiteSetting, labelUtility, remoteSiteId))
                    {
                        hasError = true;
                    }
                });
            }
            else
            {
                foreach (var item in items)
                {
                    if (AutoSetOneItem(item, list, excludePath, aveTaxField, records, setting, ruleManagement, ruleTermIdMapping, aveTerms, configSiteSetting, labelUtility, remoteSiteId))
                    {
                        hasError = true;
                    }
                }
            }

            if (RMMLAutoSmartItemsCache.Instance.NeedProcessCache)
            {
                RMMLAutoSmartItemsCache.Instance.SetFinished();
                if (RMMLAutoSmartItemsCache.Instance.HasError)
                {
                    hasError = true;
                }
                RMMLAutoSmartItemsCache.Instance.Dispose();
            }
            
            return hasError;
        }

        public static void ProcessSmartAutoCacheItemsAction(List<AutoSmartCacheItemInfo> cacheItems)
        {
            var hasError = false;
            var totalCount = cacheItems?.Count;
            if (totalCount > 0)
            {
                try
                {
                    var spCacheItems = cacheItems.ConvertAll(o => (SPAutoSmartCacheItemInfo)o);
                    var aveTaxField = spCacheItems.Select(o => o.AveTaxField).FirstOrDefault();
                    var aveItems = spCacheItems.Select(o => o.AveItem).ToList();
                    BatchPredictTerm(aveItems, aveTaxField);
                    if (spCacheItems.Count > smartAutoCacheitemsPerTask)
                    {
                        logger.Info("Use multi thread to run auto classification for smart cache items.");
                        AveTenantTasks.RunParallel(spCacheItems, smartAutoCacheitemsPerTask, new CancellationTokenSource(), item =>
                        {
                            AutoSetOneSmartCacheItemTermAsync(item.AveItem, item.AveList, item.AveTaxField, item.Records, item.LabelUtility, item.Setting, item.RemoteSiteId, item.ConfigSiteSetting).Wait();
                        });
                    }
                    else
                    {
                        foreach (var item in spCacheItems)
                        {
                            AutoSetOneSmartCacheItemTermAsync(item.AveItem, item.AveList, item.AveTaxField, item.Records,item.LabelUtility, item.Setting, item.RemoteSiteId, item.ConfigSiteSetting).Wait();
                        }
                    }
                    RMMLAutoSmartItemsCache.Instance.HasError = hasError;
                }
                catch (Exception ex)
                {
                    logger.Error($"An error while process smart auto cache items action, message:{ex}");
                    RMMLAutoSmartItemsCache.Instance.HasError = true;
                }
            }
        }

        public static async System.Threading.Tasks.Task<bool> AutoSetOneSmartCacheItemTermAsync(IAveListItem item, IAveList list, IAveTaxonomyField aveTaxField, IAveORecords records, SPOLabelUtility labelUtility, RMSharePointSetting setting, Guid remoteSiteId, ConfigSiteSetting configSiteSetting = null)
        {
            using (new PerformanceScope("RMSPSettingUtility.AutoSetOneSmartCacheItemTerm", addToStatistics: true))
            {
                if (JobContext.IsCSDTenant)
                {
                    logger.Error("CSD tenant does not support smart AutoSetOneSmartCacheItemTermAsync");
                    return true;
                }

                bool hasError = false;
                ReportManager.Increase();
                var isUpdateDeclared = false;
                if (ShouldSkipArchivedItem(item))
                {
                    return hasError;
                }
                if (IsBlockEditAndDeleteRecord(item))
                {
                    logger.Info("Item is Block Edit and delete {0}", item.Name);
                    if (setting.IncludeDeclaredRecords)
                    {
                        isUpdateDeclared = true;
                    }
                    else
                    {
                        if (JobContext.IsCSDTenant)
                        {
                            SendSPSettingReport(item.Name, item.Url, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Skipped, "RM_SS_ItemBlockEditAndDelete");
                        }
                        return hasError;
                    }
                }
                string itemFullUrl = list.ParentWeb.Url + "/" + item.Url;
                try
                {
                    IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;
                    ApplySettingPredictResult predictResult = GetPredictResult(item, setting); ;
                    var termId = predictResult.TermId;
                    if (!termId.Equals(Guid.Empty))
                    {
                        string oldValue = item[aveTaxField.InternalName] == null ? null : ((string)item[aveTaxField.InternalName]).ToLowerInvariant();
                        Guid oldTermId = Guid.Empty;
                        try
                        {
                            if (oldValue != null && !string.IsNullOrEmpty(oldValue.ToString()))
                            {
                                var valueString = oldValue.ToString().Split('|');
                                if (valueString.Length > 1)
                                {
                                    oldTermId = new Guid(valueString[1]);
                                }
                            }
                        }
                        catch (Exception ex) 
                        {
                            logger.Warn($@"have exception when get oldTerm ID,ex:{ex}");
                        }
                        //if (string.IsNullOrEmpty(oldValue) || !oldValue.Contains(termId.ToString().ToLowerInvariant()))
                        //{
                            taxValue.TermGuid = termId.ToString();
                            taxValue.Label = predictResult?.TermName;
                            logger.Info("Auto set smart cache item classification value {0}", item.ID);
                            if (isUpdateDeclared)
                            {
                                try
                                {
                                    WaitExecuteAction(() =>
                                    {
                                        records.UndeclareItemAsRecord(item);
                                    });
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("undeclare item failed {0}:{1}", item.Url, e.ToString());
                                }
                            }
                            bool labelNotExist = false;
                            try
                            {
                                var isUpdateSharePoint = predictResult?.IsUpdateSharePoint ?? true;
                                if (isUpdateSharePoint)
                                {
                                    item[aveTaxField.ID] = taxValue;
                                    item[aveTaxField.TextField] = taxValue.ToString();
                                    //item.SystemUpdate();
                                    WaitExecuteAction(() =>
                                    {
                                        item.SystemUpdateForRecords();//*********
                                    });
                                    using (new PerformanceScope("RMSPSettingUtility.UpdateLabelTotal", addToStatistics: true))
                                    {
                                        var recId = IDGenerator.GetRecordId(list.ParentWeb.Site.ID, item.UniqueId);
                                        labelNotExist = labelUtility.UpdateLabel(item, termId, recId, oldTermId);
                                    }
                                    if(!labelNotExist)
                                    {
                                        if (predictResult?.IsApplyDefaultTerm ?? false)
                                            SendSPSettingReport(item.Name, itemFullUrl, "RM_SS_ApplyExist", JobDetailsStatus.Successful, setting.ColumnName, taxValue.Label);
                                        else
                                            SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAIClassification", JobDetailsStatus.Successful, setting.ColumnName, taxValue.Label);
                                    }
                                }
                                else
                                {
                                    
                                    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SkipAIManualApproval", JobDetailsStatus.Successful, setting.ColumnName, taxValue.Label);
                                    logger.Info($"skip update the current item's term value, url:{item.Url}, because isUpdateSharePoint is false");
                                }

                                var isSyncCosmosDB = predictResult?.IsSyncCosmosDB ?? false;
                                if (isSyncCosmosDB)
                                {
                                    await RMMachineLearningDataSyncManager.SyncItemToDBAsync(item, remoteSiteId, setting, predictResult);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn("update item failed {0}:{1}", item.Url, e.ToString());
                                hasError = true;
                                var expMsg = GetExceptionMessage(e);
                                //if (JobContext.IsCSDTenant)
                                {
                                    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAIClassification", JobDetailsStatus.Failed, setting.ColumnName, string.Empty, expMsg);
                                }
                            }
                            if (isUpdateDeclared)
                            {
                                using (PerformanceScope scope = new PerformanceScope("AutoSetTerm.DeclareItemAsRecord", "", true))
                                {
                                    WaitExecuteAction(() =>
                                    {
                                        var dItem = list.GetItemById(item.ID);
                                        records.DeclareItemAsRecord(dItem);
                                    });
                                }
                            }

                            if (!JobContext.IsCSDTenant && labelNotExist)
                            {
                                HasFailedReport = true;
                                SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAIClassification", JobDetailsStatus.Failed, setting.ColumnName, taxValue.Label, "RM_SPO_ApplySetting_LabelNotExist");
                            }
                        }
                        else
                        {
                            //if (JobContext.IsCSDTenant)
                            //{
                            //    SetCSDSettingForAuto(item, list, records, termId, configSiteSetting, isUpdateDeclared);
                            //    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Skipped);
                            //}
                        }
                    //}
                    var predictResultFail = RMMLPredictHelper.GetPredictRequestFailCache(item.UniqueId);
                    if (predictResultFail != null)
                    {
                        ///SendSPSettingReport(item.Name, itemFullUrl, "RM_JM_Details_Failed_ExtractFileContentFaile", JobDetailsStatus.Failed);
                        hasError = true;
                    }
                }
                catch (Exception e)
                {
                    hasError = true;
                    logger.Error("Auto set smart cache item classification value failed {0}:{1}", itemFullUrl, e.ToString());
                    var expMsg = GetExceptionMessage(e);
                    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAIClassification", JobDetailsStatus.Failed, setting.ColumnName, string.Empty, expMsg);
                }

                return hasError;
            }
        }

        private static bool IsCheckOut(IAveListItem item)
        {
            bool isCheckOut = false;
            try
            {
                if (item != null)
                {
                    var values = item.FieldValues;
                    string checkoutUser = values.ContainsKey("CheckoutUser") ? values["CheckoutUser"]?.ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(checkoutUser))
                    {
                        string separator = ";#";
                        int index = checkoutUser.IndexOf(separator);
                        if (index > 0)
                        {
                            var checkoutUserName = checkoutUser.Substring(index + 2);
                            if (!string.IsNullOrEmpty(checkoutUser))
                            {
                                isCheckOut = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Debug(" Can not get Check Out User. Reason : {0}", e.ToString());
            }
            return isCheckOut;
        }

        /// <summary>
        /// if set succussfully, return false which means no error happens, or else return true
        /// </summary>
        /// <param name="item"></param>
        /// <param name="list"></param>
        /// <param name="excludePath"></param>
        /// <param name="aveTaxField"></param>
        /// <param name="records"></param>
        /// <param name="setting"></param>
        /// <param name="ruleManagement"></param>
        /// <param name="ruleTermIdMapping"></param>
        /// <param name="aveTerms"></param>
        /// <param name="configSiteSetting"></param>
        /// <returns></returns>
        private static bool AutoSetOneItem(IAveListItem item, IAveList list, List<string> excludePath, IAveTaxonomyField aveTaxField, IAveORecords records, RMSharePointSetting setting, RuleManagement ruleManagement, Dictionary<string, Guid> ruleTermIdMapping, Dictionary<Guid, IAveTerm> aveTerms, ConfigSiteSetting configSiteSetting, SPOLabelUtility labelUtility, Guid remoteSiteId)
        {
            var hasError = false;
            ReportManager.Increase();

            if (!NeedIncluedeFolder(setting) && item.FileSystemObjectType == AveFileSystemObjectType.Folder)
            {
                logger.Info("Current item:{0} is folder and setting is not include folder, skip set classification.", item.Url);
                return hasError;
            }

            if (ShouldSkipArchivedItem(item))
            {
                return hasError;
            }

            if (!NeedSkip(item, excludePath))
            {
                AutoSetOneItemTerm(item, list, aveTaxField, records, setting.IncludeDeclaredRecords, ruleManagement, ruleTermIdMapping, aveTerms, ref hasError, labelUtility, setting, remoteSiteId, configSiteSetting);
            }

            return hasError;
        }

        public static void SetWhiteFileTerm(IAveListItem item, IAveList list, IAveORecords records, IAveTaxonomyField aveTaxField, bool isBlockEditAndDelete, ConfigSiteSetting configSiteSetting)
        {
            if (isBlockEditAndDelete)
            {
                try
                {
                    records.UndeclareItemAsRecord(item);
                }
                catch (Exception e)
                {
                    logger.Warn("undeclare item failed {0}:{1}", item.Url, e.ToString());
                }
            }
            SetExculdeListFileDefaultValue(item, aveTaxField, configSiteSetting);
            if (isBlockEditAndDelete)
            {
                using (PerformanceScope scope = new PerformanceScope("AutoSetTerm.DeclareItemAsRecord", "", true))
                {
                    var dItem = list.GetItemById(item.ID);
                    records.DeclareItemAsRecord(dItem);
                }
            }
        }

        //public static void SetCSDSettingForAuto(IAveListItem item, IAveList list, IAveORecords records, Guid termId, ConfigSiteSetting configSiteSetting, bool isUpdateLockedItem)
        //{
        //    if (JobContext.IsCSDTenant)
        //    {
        //        if (isUpdateLockedItem)
        //        {
        //            try
        //            {
        //                records.UndeclareItemAsRecord(item);
        //            }
        //            catch (Exception e)
        //            {
        //                logger.Warn("undeclare item failed {0}:{1}", item.Url, e.ToString());
        //            }
        //        }
        //        SetCSDSettings(item, termId, configSiteSetting.CSDRules, isUpdateLockedItem);
        //        if (isUpdateLockedItem)
        //        {
        //            using (PerformanceScope scope = new PerformanceScope("AutoSetTerm.DeclareItemAsRecord", "", true))
        //            {
        //                var dItem = list.GetItemById(item.ID);
        //                records.DeclareItemAsRecord(dItem);
        //            }
        //        }
        //    }
        //}
        private static void SendSPSettingReport(string name, string url, string action, JobDetailsStatus status, string comment = "")
        {
            JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
            detail.ObjectName = name;
            detail.SourceURL = url;
            detail.Action = action;
            detail.Status = status;
            detail.Comment = comment;
            ReportManager.SendJobDetail(detail);
        }

        private static void SendSPSettingReport(string name, string url, string action, JobDetailsStatus status, string columnName, string classification, string comment = "")
        {
            JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
            detail.ObjectName = name;
            detail.SourceURL = url;
            detail.Action = action;
            detail.Status = status;
            detail.Comment = comment;
            detail.Classification = classification;
            detail.ColumnName = columnName;
            ReportManager.SendJobDetail(detail);
        }

        private static bool HandleOneNoteFolderWithModifiedClass(ConfigSiteSetting configSiteSetting)
        {
            return (configSiteSetting.ModifiedBasedFileTypeMapping.ContainsKey(OneNoteFileType.One)
                    || configSiteSetting.ModifiedBasedFileTypeMapping.ContainsKey(OneNoteFileType.Onetoc2));
        }

        private static bool IsOneNoteFolder(IAveListItem item)
        {
            return item.FileSystemObjectType == AveFileSystemObjectType.Folder
                && item["ProgId"].ToString().Equals("OneNote.Notebook", StringComparison.OrdinalIgnoreCase);
        }

       /* private static bool IsOneNoteFile(IAveListItem item)
        {
            if (item.FileSystemObjectType == AveFileSystemObjectType.File)
            {
                var extention = item.Name.Substring(item.Name.LastIndexOf('.') + 1);
                if (extention.ToLowerInvariant().Equals(OneNoteFileType.One)
                    || extention.ToLowerInvariant().Equals(OneNoteFileType.Onetoc2))
                {
                    return true;
                }
            }
            return false;
        }*/

        //private static bool NeedProcessOneNoteFile(IAveListItem item, ConfigSiteSetting configSiteSetting)
        //{
        //    if (IsOneNoteFile(item))
        //    {
        //        var extention = item.Name.Substring(item.Name.LastIndexOf('.') + 1);
        //        if (configSiteSetting.ModifiedBasedFileExtentions.Contains(extention.ToLowerInvariant()))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        private static bool IsModifiedBasedFile(IAveListItem item, ConfigSiteSetting configSiteSetting)
        {
            if (item.FileSystemObjectType == AveFileSystemObjectType.File)
            {
                var extention = item.Name.Substring(item.Name.LastIndexOf('.') + 1);
                if (configSiteSetting.ModifiedBasedFileTypeMapping.ContainsKey(extention.ToLowerInvariant()))
                {
                    return true;
                }
            }
            else if (IsOneNoteFolder(item) && HandleOneNoteFolderWithModifiedClass(configSiteSetting))
            {
                return true;
            }
            return false;
        }

        private static IAveTerm GetDefaultModifiedBasedTerm(IAveListItem item, Dictionary<string, IAveTerm> modifiedBasedFileTypeMapping)
        {
            if (IsOneNoteFolder(item))
            {
                if (modifiedBasedFileTypeMapping.ContainsKey(OneNoteFileType.One) && modifiedBasedFileTypeMapping[OneNoteFileType.One] != null)
                {
                    return modifiedBasedFileTypeMapping[OneNoteFileType.One];
                }
                else if (modifiedBasedFileTypeMapping.ContainsKey(OneNoteFileType.Onetoc2) && modifiedBasedFileTypeMapping[OneNoteFileType.Onetoc2] != null)
                {
                    return modifiedBasedFileTypeMapping[OneNoteFileType.Onetoc2];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                string fileExtention = item.Name.Substring(item.Name.LastIndexOf('.') + 1);
                if (modifiedBasedFileTypeMapping.ContainsKey(fileExtention))
                {
                    return modifiedBasedFileTypeMapping[fileExtention];
                }
                else
                {
                    return null;
                }
            }
        }

        private static void SetCSDVal4ModifiedBasedFile(IAveListItem item, IAveTaxonomyField aveTaxField, ConfigSiteSetting configSiteSetting, out bool needSkip, out string skipMsg)
        {
            needSkip = false;
            skipMsg = string.Empty;

            var modifeidBasedTerm = GetDefaultModifiedBasedTerm(item, configSiteSetting.ModifiedBasedFileTypeMapping);
            if (modifeidBasedTerm == null)
            {
                needSkip = true;
                skipMsg = "RM_JS_SS_CannotGetModifiedBasedTerm";
                logger.Warn("Can not find the default modified based class, please check the \"Modified Date based Retention File Type\" list.");
                return;
            }
            var csdRules = configSiteSetting.CSDRules;
            if (!csdRules.ContainsKey(modifeidBasedTerm.ID) || !csdRules[modifeidBasedTerm.ID].IsModifiedBasedRule)
            {
                needSkip = true;
                skipMsg = "RM_JS_SS_CannotGetModifiedBasedRule";
                logger.Warn($"Can not find the modified based csd rule, or the csd rule is not modified based rule. Term Id:[{modifeidBasedTerm.ID}] Term Name:[{modifeidBasedTerm.Name}]");
                return;
            }
            logger.Info($"Reclassify modified based file. Term Id:[{modifeidBasedTerm.ID}] Term Name:[{modifeidBasedTerm.Name}]");
            SetCSDValWithModifiedClass(item, aveTaxField, modifeidBasedTerm, configSiteSetting);
        }

        private static void SetCSDValWithModifiedClass(IAveListItem item, IAveTaxonomyField aveTaxField, IAveTerm modifeidBasedTerm, ConfigSiteSetting configSiteSetting)
        {
            var csdRules = configSiteSetting.CSDRules;
            SetBCSValue(item, aveTaxField, modifeidBasedTerm);
            SetCSDExtendsColumn(item, modifeidBasedTerm.ID);
            ClearCSDVal4ModifiedClass(item);
            item.SystemUpdateForRecords();

            if (IsNormalFile(item, configSiteSetting))
            {
                RetentionSetting rs = csdRules[modifeidBasedTerm.ID].ModifiedBasedRetentionSetting;
                SetCSDLabel(item, rs.RetentionLabel);
            }
        }

        private static void SetBCSValue(IAveListItem item, IAveTaxonomyField aveTaxField, IAveTerm term)
        {
            IAveTaxonomyFieldValue taxFieldVal = aveTaxField.TaxonomyFieldValue;
            taxFieldVal.TermGuid = term.ID.ToString();
            taxFieldVal.Label = term.Name;

            item[aveTaxField.ID] = taxFieldVal;
            item[aveTaxField.TextField] = taxFieldVal.ToString();
        }

        private static bool ProcessCSDModifiedBasedFile(IAveListItem item, IAveTaxonomyField aveTaxField, ConfigSiteSetting configSiteSetting, string action)
        {
            bool hasProcessed = false;
            if (IsModifiedBasedFile(item, configSiteSetting))
            {
                var itemFullUrl = item.ParentList.ParentWeb.Url + "/" + item.Url;
                if (item[aveTaxField.ID] == null || string.IsNullOrEmpty(item[aveTaxField.ID].ToString()))
                {
                    logger.Info($"Process modified based file. Item Url:[{itemFullUrl}]");
                    bool needSkip;
                    string skipMsg;
                    SetCSDVal4ModifiedBasedFile(item, aveTaxField, configSiteSetting, out needSkip, out skipMsg);
                    if (needSkip)
                    {
                        SendSPSettingReport(item.Name, itemFullUrl, action, JobDetailsStatus.Skipped, skipMsg);
                    }
                    else
                    {
                        SendSPSettingReport(item.Name, itemFullUrl, action, JobDetailsStatus.Successful, "RM_JS_SS_ReclassifyWithModifiedBasedClass");
                    }
                }
                else
                {
                    logger.Info($"Skip modified based file for the item already has a term. Item Url:[{itemFullUrl}]");
                    SendSPSettingReport(item.Name, itemFullUrl, action, JobDetailsStatus.Skipped, "RM_JS_SS_SkipModifiedFile");
                }
                hasProcessed = true;
            }
            return hasProcessed;
        }

        private static bool ProcessCSDWhiteFile(IAveListItem item, IAveTaxonomyField aveTaxField, ConfigSiteSetting configSiteSetting, string action)
        {
            bool hasProcessed = false;
            if (IsFileExtentionInExculdeList(configSiteSetting.ExcludeFileExtentions, item))
            {
                var itemFullUrl = item.ParentList.ParentWeb.Url + "/" + item.Url;
                if (item[aveTaxField.ID] == null || string.IsNullOrEmpty(item[aveTaxField.ID].ToString()))
                {
                    SetExculdeListFileDefaultValue(item, aveTaxField, configSiteSetting);
                    SendSPSettingReport(item.Name, itemFullUrl, action, JobDetailsStatus.Successful, "RM_JS_JMD_Comment_WhiteList_SetValue");
                }
                else
                {
                    SendSPSettingReport(item.Name, itemFullUrl, action, JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_WhiteList_Skip_ExistValue");
                }
                hasProcessed = true;
            }
            return hasProcessed;
        }

        private static bool CheckCSDDeletionDataForLockFile(IAveListItem item, Guid termId, string action, ConfigSiteSetting configSiteSetting = null)
        {
            bool checkPassed = true;
            //计算DeletionDate，
            var createdTime = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.Created]).Ticks, DateTimeKind.Utc);
            var calculatedDeletionDate = CalculateDeletionDate(createdTime, configSiteSetting.CSDRules[termId].CreationRetentionSetting);

            var currentDeletionDate = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.DeletionDate]).Ticks, DateTimeKind.Utc);
            if (DateTime.Compare(calculatedDeletionDate, currentDeletionDate) <= 0)
            {
                logger.Info($"CalculatedDeletionDate is small than CurrentDeletionDate. CalculatedDeletionDate:[{calculatedDeletionDate}] CurrentDeletionDate:[{currentDeletionDate}]");
                SendSPSettingReport(item.Name, item.ParentList.ParentWeb.Url + "/" + item.Url, action, JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_DeletionDateIsEarly");
                checkPassed = false;
            }
            return checkPassed;
        }

        public static void AutoSetOneItemTerm4NormalTenant(IAveListItem item, IAveList list, IAveTaxonomyField aveTaxField, IAveORecords records,
            bool includeDeclaredRecords, RuleManagement ruleManagement, Dictionary<string, Guid> ruleTermIdMapping,
            Dictionary<Guid, IAveTerm> aveTerms, ref bool hasError, SPOLabelUtility labelUtility, RMSharePointSetting setting, Guid remoteSiteId, ConfigSiteSetting configSiteSetting = null)
        {
            var isUpdateDeclared = false;
            string itemFullUrl = list.ParentWeb.Url + "/" + item.Url;
            using (new PerformanceScope("RMSPSettingUtility.AutoSetOneItemTerm", addToStatistics: true))
            {
                try
                {
                    //这个判断会导致效率问题。。。。改为在CamlQuery中使用Recursive 不是RecursiveAll
                    //if (item.File == null)
                    //{
                    //    //只处理Document，其他skip
                    //    continue;
                    //}

                    if (JobContext.IsCSDTenant)
                    {
                        logger.Error("This method is only for normal tenant.");
                        hasError = true;
                        return;
                    }

                    if (IsBlockEditAndDeleteRecord(item))
                    {
                        logger.Info("Item is Block Edit and delete {0}", item.ID);
                        if (includeDeclaredRecords)
                        {
                            isUpdateDeclared = true;
                        }
                        else
                        {
                            if (JobContext.IsCSDTenant)
                            {
                                SendSPSettingReport(item.Name, item.Url, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Skipped, "RM_SS_ItemBlockEditAndDelete");
                            }
                            return;
                        }
                    }

                    Rule soRule = null;
                    try
                    {
                        soRule = ruleManagement.CheckItemCriteria(item.UniqueId, item);
                    }
                    catch (Exception ex)
                    {
                        if (!JobContext.IsCSDTenant && item.FileSystemObjectType == AveFileSystemObjectType.Folder)
                        {
                            logger.Error("Auto set Item classification value failed {0}:{1}", itemFullUrl, ex.ToString());
                            throw new FailedCheckRuleException("RM_JS_JMD_Folder_FailedCheckRuleMessage");
                        }
                        else
                        {
                            throw;
                        }
                    }
                    Guid termId = soRule == null ? ruleTermIdMapping[Guid.Empty.ToString()] : ruleTermIdMapping[soRule.Id];
                    IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;

                    if (termId.Equals(Guid.Empty) && setting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                    {
                        RMMLAutoSmartItemsCache.Instance.ProcessItem(new SPAutoSmartCacheItemInfo
                        {
                            AveItem = item,
                            AveList = list,
                            AveTaxField = aveTaxField,
                            Records = records,
                            Setting = setting,
                            ConfigSiteSetting = configSiteSetting,
                            LabelUtility = labelUtility,
                            RemoteSiteId = remoteSiteId
                        });
                        return;
                    }

                    if (!termId.Equals(Guid.Empty))
                    {
                        string oldValue = item[aveTaxField.InternalName] == null ? null : ((string)item[aveTaxField.InternalName]).ToLowerInvariant();
                        Guid oldTermId = Guid.Empty;
                        try
                        {
                            if (oldValue != null && !string.IsNullOrEmpty(oldValue.ToString()))
                            {
                                var valueString = oldValue.ToString().Split('|');
                                if (valueString.Length > 1)
                                {
                                    oldTermId = new Guid(valueString[1]);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"error occured when AutoSetOneItemTerm,error:{e}");
                        }
                        if (string.IsNullOrEmpty(oldValue) || !oldValue.Contains(termId.ToString().ToLowerInvariant()))
                        {
                            IAveTerm aveTerm = aveTerms[termId];
                            taxValue.TermGuid = aveTerm.ID.ToString();
                            taxValue.Label = aveTerm.Name;
                            logger.Info("Auto set Item classification value for autoclassification {0}", item.ID);
                            if (isUpdateDeclared)
                            {
                                try
                                {
                                    WaitExecuteAction(() =>
                                    {
                                        records.UndeclareItemAsRecord(item);
                                    });
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("undeclare item failed {0}:{1}", item.Url, e.ToString());
                                }
                            }
                            bool labelNotExist = false;
                            try
                            {
                                item[aveTaxField.ID] = taxValue;
                                item[aveTaxField.TextField] = taxValue.ToString();
                                //item.SystemUpdate();
                                WaitExecuteAction(() =>
                                {
                                    item.SystemUpdateForRecords();//*********
                                });
                                using (new PerformanceScope("RMSPSettingUtility.UpdateLabelTotal", addToStatistics: true))
                                {
                                    var recId = IDGenerator.GetRecordId(list.ParentWeb.Site.ID, item.UniqueId);
                                    labelNotExist = labelUtility.UpdateLabel(item, termId, recId, oldTermId);
                                }
                                if (!labelNotExist)
                                {
                                    if (soRule == null)
                                    {
                                        SendSPSettingReport(item.Name, itemFullUrl, "RM_SS_ApplyExist", JobDetailsStatus.Successful, setting.ColumnName, taxValue.Label);
                                    }
                                    else
                                    {
                                        SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Successful, setting.ColumnName, taxValue.Label);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn("update item failed {0}:{1}", item.Url, e.ToString());
                                hasError = true;
                                var expMsg = GetExceptionMessage(e);
                                SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Failed, setting.ColumnName, taxValue.Label, expMsg);
                            }
                            if (isUpdateDeclared)
                            {
                                using (PerformanceScope scope = new PerformanceScope("AutoSetTerm.DeclareItemAsRecord", "", true))
                                {
                                    WaitExecuteAction(() =>
                                    {
                                        var dItem = list.GetItemById(item.ID);
                                        records.DeclareItemAsRecord(dItem);
                                    });
                                }
                            }

                            if (!JobContext.IsCSDTenant && labelNotExist)
                            {
                                HasFailedReport = true;
                                SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Failed, setting.ColumnName, taxValue.Label, "RM_SPO_ApplySetting_LabelNotExist");
                            }
                        }
                        else
                        {
                            //if (JobContext.IsCSDTenant)
                            //{
                            //    SetCSDSettingForAuto(item, list, records, termId, configSiteSetting, isUpdateDeclared);
                            //    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Skipped);
                            //}
                        }
                    }
                }
                catch (FailedCheckRuleException e)
                {
                    hasError = true;
                    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Failed, setting.ColumnName, string.Empty, e.Message);
                }
                catch (Exception e)
                {
                    hasError = true;
                    logger.Error("Auto set Item classification value failed {0}:{1}", itemFullUrl, e.ToString());
                    var expMsg = GetExceptionMessage(e);
                    //if (JobContext.IsCSDTenant)
                    {
                        SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Failed, setting.ColumnName, string.Empty, expMsg);
                    }
                }
            }
        }

        public static void AutoSetOneItemTerm4CSDTenant(IAveListItem item, IAveList list, IAveTaxonomyField aveTaxField, IAveORecords records,
            bool includeDeclaredRecords, RuleManagement ruleManagement, Dictionary<string, Guid> ruleTermIdMapping,
            Dictionary<Guid, IAveTerm> aveTerms, ref bool hasError, SPOLabelUtility labelUtility, RMSharePointSetting setting, Guid remoteSiteId, ConfigSiteSetting configSiteSetting = null)
        {
            var isUpdateDeclared = false;
            string itemFullUrl = list.ParentWeb.Url + "/" + item.Url;
            using (new PerformanceScope("RMSPSettingUtility.AutoSetOneItemTerm", addToStatistics: true))
            {
                try
                {
                    //这个判断会导致效率问题。。。。改为在CamlQuery中使用Recursive 不是RecursiveAll
                    //if (item.File == null)
                    //{
                    //    //只处理Document，其他skip
                    //    continue;
                    //}

                    if (!JobContext.IsCSDTenant)
                    {
                        logger.Error("This method is only for CSD tenant.");
                        hasError = true;
                        return;
                    }

                    if (IsBlockEditAndDeleteRecord(item))
                    {
                        logger.Info("Item is Block Edit and delete {0}", item.ID);
                        if (includeDeclaredRecords)
                        {
                            isUpdateDeclared = true;
                        }
                        else
                        {
                            if (JobContext.IsCSDTenant)
                            {
                                SendSPSettingReport(item.Name, item.Url, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Skipped, "RM_SS_ItemBlockEditAndDelete");
                            }
                            return;
                        }
                    }

                    if (JobContext.IsCSDTenant)
                    {
                        if (!isUpdateDeclared && IsCheckOut(item))
                        {
                            throw new Exception("RM_JS_SPS_FileIsCheckOut");
                        }
                        if (ProcessCSDWhiteFile(item, aveTaxField, configSiteSetting, "RM_JS_JMD_Action_SetAutoClassification"))
                        {
                            return;
                        }
                        if (ProcessCSDModifiedBasedFile(item, aveTaxField, configSiteSetting, "RM_JS_JMD_Action_SetAutoClassification"))
                        {
                            return;
                        }
                    }

                    Rule soRule = null;
                    try
                    {
                        soRule = ruleManagement.CheckItemCriteria(item.UniqueId, item);
                    }
                    catch (Exception ex)
                    {
                        if (!JobContext.IsCSDTenant && item.FileSystemObjectType == AveFileSystemObjectType.Folder)
                        {
                            logger.Error("Auto set Item classification value failed {0}:{1}", itemFullUrl, ex.ToString());
                            throw new FailedCheckRuleException("RM_JS_JMD_Folder_FailedCheckRuleMessage");
                        }
                        else
                        {
                            throw;
                        }
                    }
                    Guid termId = soRule == null ? ruleTermIdMapping[Guid.Empty.ToString()] : ruleTermIdMapping[soRule.Id];
                    IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;

                    if (!termId.Equals(Guid.Empty))
                    {
                        if (IsCSDWhiteTerm(termId, configSiteSetting))
                        {
                            SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification",
                                JobDetailsStatus.Skipped, "RM_JS_JMD_CannotUseSpecialClassReclassify");
                            return;
                        }
                        else if (!IsCSDRuleConfigured(termId, configSiteSetting, out string tipMsg))
                        {
                            SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification",
                                JobDetailsStatus.Skipped, tipMsg);
                            return;
                        }
                        if (isUpdateDeclared && !CheckCSDDeletionDataForLockFile(item, termId, "RM_JS_JMD_Action_SetAutoClassification", configSiteSetting))
                        {
                            return;
                        }

                        SetCSDColumnValues(item, aveTaxField, aveTerms[termId], configSiteSetting);
                        SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Successful);
                    }
                }
                catch (FailedCheckRuleException e)
                {
                    hasError = true;
                    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Failed, setting.ColumnName, string.Empty, e.Message);
                }
                catch (Exception e)
                {
                    hasError = true;
                    logger.Error("Auto set Item classification value failed {0}:{1}", itemFullUrl, e.ToString());
                    var expMsg = GetExceptionMessage(e);
                    //if (JobContext.IsCSDTenant)
                    {
                        SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Failed, setting.ColumnName, string.Empty, expMsg);
                    }
                }
            }
        }

        public static void AutoSetOneItemTerm(IAveListItem item, IAveList list, IAveTaxonomyField aveTaxField, IAveORecords records,
            bool includeDeclaredRecords, RuleManagement ruleManagement, Dictionary<string, Guid> ruleTermIdMapping,
            Dictionary<Guid, IAveTerm> aveTerms, ref bool hasError, SPOLabelUtility labelUtility, RMSharePointSetting setting, Guid remoteSiteId, ConfigSiteSetting configSiteSetting = null)
        { 
            if (JobContext.IsCSDTenant)
            {
                AutoSetOneItemTerm4CSDTenant(item, list, aveTaxField, records, includeDeclaredRecords, ruleManagement, ruleTermIdMapping, aveTerms, ref hasError, labelUtility, setting, remoteSiteId, configSiteSetting);
            }
            else
            {
                AutoSetOneItemTerm4NormalTenant(item, list, aveTaxField, records, includeDeclaredRecords, ruleManagement, ruleTermIdMapping, aveTerms, ref hasError, labelUtility, setting, remoteSiteId, configSiteSetting);
            }
        }
        

        private static string GetExceptionMessage(Exception e)
        {
            bool getLATError = e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) && e.InnerException.Message.StartsWith("The site do not meet the conditions.", StringComparison.OrdinalIgnoreCase);
            // "RM_SPS_LastAccessTimeQueryException" : e.Message;
            bool updateFolderError = e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) && e.InnerException.Message.StartsWith("To update this folder, go to the channel in Microsoft Teams", StringComparison.OrdinalIgnoreCase);
            string comment = string.Empty;
            if (getLATError)
            {
                comment = "RM_SPS_LastAccessTimeQueryException";
            }
            else if (updateFolderError)
            {
                switch(currentSiteCollectionLevel)
                {
                    case NodeLevel.PrivateChannel:
                        comment = "RM_SPS_UpdatePrivateChannelFolerError";
                        break;
                    case NodeLevel.SharedChannel:
                        comment = "RM_SPS_UpdateShareChannelFolerError";
                        break;
                    case NodeLevel.Office365GroupEntire:
                    default:
                        comment = "RM_SPS_UpdateChannelFolerError";
                        break;
                }
            }
            else if (e.InnerException != null && e.InnerException.Message.Contains("The label that's applied to this item prevents it from being edited or deleted"))
            {
                comment = "RM_SPS_UpdateLabelDocumentError";
            }
            else
            {
                comment = e.Message;
                if (e is System.Reflection.TargetInvocationException)
                {
                    System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                    if (te.InnerException != null)
                    {
                        comment = te.InnerException.Message;
                    }
                }
            }
            return comment;
        }
        #region old logic
        //public static void Autoclassification(IAveList list, IAveTaxonomyField aveTaxField, RMSharePointSetting setting, DateTime startTime, DateTime endTime, IAveORecords records, ref bool hasError)
        //{
        //    #region old logic
        //    //QueryAutoClassificationItems(list, setting, startTime, endTime, aveTaxField, ref hasError);
        //    #endregion
        //    IAveListItemCollection items = null;
        //    using (new RA.Common.PerformanceScope(string.Format("Autoclassification Query Items. List Url:[{0}]", list.RootFolder.Url)))
        //    {
        //        items = GetItemsWithCamlQuery(list, setting, startTime, endTime, aveTaxField.InternalName);
        //        logger.Info("Autoclassification Query Items Count: {0}", items == null ? 0 : items.Count);
        //    }

        //    using (new RA.Common.PerformanceScope(string.Format("Autoclassification Set Value. List Url:[{0}]", list.RootFolder.Url)))
        //    {
        //        AutoSetTerm(items, list, aveTaxField, setting.AutoClassificationRules, records, setting.IncludeDeclaredRecords, ref hasError);
        //    }
        //}
        //private static IAveListItemCollection GetItemsWithCamlQuery(IAveList list, RMSharePointSetting setting, DateTime startTime, DateTime endTime, string columnInternalName)
        //{
        //    IAveListItemCollection items = null;
        //    try
        //    {
        //        string queryStr = string.Empty;
        //        AveCamlQuery query = AveCamlQuery.CreateAllItemsQuery();
        //        switch ((AutoJobOption)setting.AutoJobOption)
        //        {
        //            case AutoJobOption.None:
        //            case AutoJobOption.SkipAndKeep:
        //                if (setting.RunAutoFullJob || startTime.Equals(DateTime.MinValue))
        //                {
        //                    #region full job query string
        //                    queryStr = @"
        //                    <View Scope='FilesOnly'>
        //                        <Query>
        //                           <Where>
        //                                <IsNull>
        //                                    <FieldRef Name='{0}' />
        //                                </IsNull>
        //                           </Where>
        //                        </Query>
        //                    </View>";
        //                    #endregion
        //                    query.ViewXml = string.Format(queryStr, columnInternalName);
        //                }
        //                else
        //                {
        //                    #region incremental job query string
        //                    queryStr = @"
        //                    <View Scope='FilesOnly'>
        //                        <Query>
        //                           <Where>
        //                              <And>
        //                                 <IsNull>
        //                                    <FieldRef Name='{2}' />
        //                                 </IsNull>
        //                                 <Or>
        //                                    <And>
        //                                       <Gt>
        //                                          <FieldRef Name='Created' />
        //                                          <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{0}</Value>
        //                                       </Gt>
        //                                       <Leq>
        //                                          <FieldRef Name='Created' />
        //                                          <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{1}</Value>
        //                                       </Leq>
        //                                    </And>
        //                                    <And>
        //                                       <Gt>
        //                                          <FieldRef Name='Modified' />
        //                                          <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{0}</Value>
        //                                       </Gt>
        //                                       <Leq>
        //                                          <FieldRef Name='Modified' />
        //                                          <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{1}</Value>
        //                                       </Leq>
        //                                    </And>
        //                                 </Or>
        //                              </And>
        //                           </Where>
        //                        </Query>
        //                    </View>";
        //                    #endregion
        //                    string startTimeStr = CreateISO8601DateTimeFromSystemDateTime(startTime);
        //                    string endTimeStr = CreateISO8601DateTimeFromSystemDateTime(endTime);
        //                    query.ViewXml = string.Format(queryStr, startTimeStr, endTimeStr, columnInternalName);
        //                }
        //                break;
        //            case AutoJobOption.Override:
        //                if (setting.RunAutoFullJob || startTime.Equals(DateTime.MinValue))
        //                {
        //                    queryStr = @"<View Scope='FilesOnly'><Query></Query></View>";
        //                    query.ViewXml = queryStr;
        //                }
        //                else
        //                {
        //                    #region Inc job query string
        //                    queryStr = @"
        //                    <View Scope='FilesOnly'>
        //                        <Query>
        //                           <Where>
        //                                <Or>
        //                                    <And>
        //                                        <Gt>
        //                                            <FieldRef Name='Created' />
        //                                            <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{0}</Value>
        //                                        </Gt>
        //                                        <Leq>
        //                                            <FieldRef Name='Created' />
        //                                            <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{1}</Value>
        //                                        </Leq>
        //                                    </And>
        //                                    <And>
        //                                        <Gt>
        //                                            <FieldRef Name='Modified' />
        //                                            <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{0}</Value>
        //                                        </Gt>
        //                                        <Leq>
        //                                            <FieldRef Name='Modified' />
        //                                            <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{1}</Value>
        //                                        </Leq>
        //                                    </And>
        //                                </Or>
        //                           </Where>
        //                        </Query>
        //                    </View>";
        //                    #endregion
        //                    string startTimeStr = CreateISO8601DateTimeFromSystemDateTime(startTime);
        //                    string endTimeStr = CreateISO8601DateTimeFromSystemDateTime(endTime);
        //                    query.ViewXml = string.Format(queryStr, startTimeStr, endTimeStr);
        //                }
        //                break;
        //            default:
        //                break;
        //        }
        //        logger.Info("Query XML:{0}", query.ViewXml);
        //        items = list.GetItems(query);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("An error occurred while getting items with caml query, ERROR:{0}", ex.ToString());
        //    }
        //    return items;
        //}

        /// <summary>
        /// 已废弃
        /// </summary>
        //[Obsolete]
        //public static void AutoSetTerm(IAveListItemCollection items, IAveList list, IAveTaxonomyField aveTaxField, string ruleStr, IAveORecords records, bool includeDeclaredRecords, ref bool hasError)
        //{
        //    if (items == null || items.Count <= 0)
        //    {
        //        return;
        //    }
        //    List<ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(ruleStr);
        //    Dictionary<Guid, IAveTerm> aveTerms = GetAveTerms(list, autoRules);
        //    Dictionary<string, Guid> ruleTermIdMapping = new Dictionary<string, Guid>();
        //    RuleCollection ruleCollection = GetRuleCollection(autoRules, ref ruleTermIdMapping);
        //    RuleManagement ruleManagement = new RuleManagement(ruleCollection);
        //    foreach (var item in items)
        //    {
        //        AutoSetOneItem(item, list, aveTaxField, records, includeDeclaredRecords, ruleManagement, ruleTermIdMapping, aveTerms, ref hasError);
        //    }
        //}
        #endregion
        public static Dictionary<Guid, IAveTerm> GetAveTerms(IAveList list, List<ClassificationRule> autoRules)
        {
            Dictionary<Guid, IAveTerm> aveTerms = new Dictionary<Guid, IAveTerm>();
            var taxonomySession = list.ParentWeb.Site.AveSPTaxonomySession;
            foreach (var autoRule in autoRules)
            {
                if (string.IsNullOrEmpty(autoRule.TermId))
                {
                    continue;
                }
                Guid termId = new Guid(autoRule.TermId);
                if (!termId.Equals(Guid.Empty))
                {
                    if (!aveTerms.ContainsKey(termId))
                    {
                        var term = taxonomySession.GetTerm(termId);
                        if (term == null)
                        {
                            throw new Exception("RM_SS_ConfigureColumnFailed");
                        }
                        aveTerms.Add(termId, term);
                    }
                }
            }
            return aveTerms;
        }
        public static RuleCollection GetRuleCollection(List<ClassificationRule> autoRules, ref Dictionary<string, Guid> termRuleMapping)
        {
            List<Rule> rules = new List<Rule>();
            List<SOFilterPolicy> soFilters;
            foreach (var autoRule in autoRules)
            {
                if (autoRule.IsDefaultRule)
                {
                    if (autoRule.NoDefaultTerm)
                    {
                        termRuleMapping.Add(Guid.Empty.ToString(), Guid.Empty);
                    }
                    else
                    {
                        termRuleMapping.Add(Guid.Empty.ToString(), new Guid(autoRule.TermId));
                    }
                }
                else
                {
                    soFilters = new List<SOFilterPolicy>();
                    int sequenceNo = 0;
                    ConvertToSOFilters(autoRule.FilterGroups, ref sequenceNo, ref soFilters);
                    List<FilterPolicy> filerPolicies = ConvertSOFiletrPolicyToFilterPolicy(soFilters);
                    string andOrExpressionStr = GetGroupsAndOrExpression(autoRule.FilterGroups, ArchiverFilterCombineMode.And);
                    logger.Info("AndOr Expression:{0}", andOrExpressionStr);
                    Rule soRule = ConvertToSORule(autoRule, soFilters, filerPolicies, andOrExpressionStr);
                    rules.Add(soRule);

                    termRuleMapping.Add(soRule.Id, new Guid(autoRule.TermId));
                }
            }

            RuleCollection ruleCol = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
            for (int i = 0; i < rules.Count; i++)
            {
                ruleCol.Rules.Add(i, rules[i]);
            }
            return ruleCol;
        }
        public static string GetGroupAndOrExpression(FilterGroup filterGroup)
        {
            string groupAndOrExpression = string.Empty;

            string filtersExpression = GetFiltersAndOrExpression(filterGroup.Filters);
            groupAndOrExpression = filtersExpression;

            if (filterGroup.FilterGroups != null && filterGroup.FilterGroups.Count > 0)
            {
                string groupsResult = GetGroupsAndOrExpression(filterGroup.FilterGroups, filterGroup.CombineMode);
                groupAndOrExpression += " " + filterGroup.CombineMode.ToString() + " " + groupsResult;
            }

            if (filterGroup.Filters.Count == 1 && filterGroup.FilterGroups.Count == 0)
            {
                //do nothing
            }
            else
            {
                groupAndOrExpression = "(" + groupAndOrExpression + ")";
            }
            return groupAndOrExpression;
        }
        public static string GetGroupsAndOrExpression(List<FilterGroup> filterGroups, ArchiverFilterCombineMode combineMode)
        {
            string result = string.Empty;
            for (int i = 0; i < filterGroups.Count; i++)
            {
                string groupResult = GetGroupAndOrExpression(filterGroups[i]);
                if (i == 0)
                {
                    result = groupResult;
                }
                else
                {
                    result += " " + combineMode.ToString() + " " + groupResult;
                }
            }
            return result;
        }
        public static List<FilterPolicy> ConvertSOFiletrPolicyToFilterPolicy(List<SOFilterPolicy> soFilters)
        {
            List<FilterPolicy> filerPolicies = new List<FilterPolicy>();
            foreach (var filter in soFilters)
            {
                FilterPolicy filterPolicy = new FilterPolicy();
                if (filter.Condition == PolicyCondition.Exactly || filter.Condition == PolicyCondition.Equals)
                {
                    filterPolicy.Condition = PolicyCondition.Equals;
                }
                else
                {
                    filterPolicy.Condition = filter.Condition;
                }
                filterPolicy.Level = filter.Level;
                filterPolicy.Rule = filter.Rule;
                filterPolicy.RuleType = filter.RuleType;
                filterPolicy.SequenceNo = filter.SequenceNo;
                filterPolicy.Value = filter.Value;

                filerPolicies.Add(filterPolicy);
            }
            return filerPolicies;
        }
        public static Rule ConvertToSORule(ClassificationRule autoRule, List<SOFilterPolicy> soFilters, List<FilterPolicy> filerPolicies, string andOrStr)
        {
            Rule rule = new Rule();
            rule.Id = Guid.NewGuid().ToString();
            rule.SOFilters = soFilters;
            rule.Filters = filerPolicies;
            rule.PolicyLevel = (PolicyLevel)autoRule.RuleLevel;
            rule.Order = autoRule.RuleOrder;
            rule.ProfileType = ServerFilterPolicy.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            //rule.AndOrExpression = GetAndOrExpression(soFilters, autoRule.RuleLevel);
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>() { { autoRule.RuleLevel, andOrStr } };
            return rule;
        }
        public static string GetFiltersAndOrExpression(List<RuleFilter> filters)
        {
            //string AndOrExpression = "(";
            string AndOrExpression = string.Empty;
            for (int i = 0; i < filters.Count; i++)
            {
                RuleFilter filter = filters[i];
                if (i == filters.Count - 1)
                {
                    AndOrExpression += string.Format("{0}", filter.SequenceNo);
                }
                else
                {
                    AndOrExpression += string.Format("{0} {1} ", filter.SequenceNo, filter.CombineMode == ArchiverFilterCombineMode.And ? "And" : "Or");
                }
            }
            //AndOrExpression += ")";
            return AndOrExpression;
        }
        public static void ConvertToSOFilters(List<FilterGroup> filterGroups, ref int sequenceNo, ref List<SOFilterPolicy> soFilters)
        {
            foreach (var filterGroup in filterGroups)
            {
                foreach (var raFilter in filterGroup.Filters)
                {
                    sequenceNo++;
                    SOFilterPolicy soFilter = BuildSOFilter(raFilter, sequenceNo);
                    soFilters.Add(soFilter);
                }
                ConvertToSOFilters(filterGroup.FilterGroups, ref sequenceNo, ref soFilters);
            }
        }
        public static SOFilterPolicy BuildSOFilter(RuleFilter filter, int sequenceNo)
        {
            ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
            arFilter.CombineMode = filter.CombineMode;
            //arFilter.SequenceNo = filter.SequenceNo;
            arFilter.SequenceNo = sequenceNo;
            arFilter.Level = filter.Level;
            arFilter.Condition = filter.Condition;
            arFilter.RuleType = filter.RuleType;
            if (!string.IsNullOrEmpty(filter.filterName))
            {
                arFilter.RuleName = filter.filterName;
            }
            //arFilter.Dto.Rule = arFilter.RuleBase;
            if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime || arFilter.RuleType == ArchiverFilterRuleType.CreatedTime
         || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime
         || arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn || arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty)
            {
                string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                {

                    DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                    if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                    {
                        //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                        throw new Exception("");
                    }
                    arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.Before)
                {
                    // ValidateValueCount(value, 3);
                    DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                {
                    //ValidateValueCount(value, 1);
                    //SetValueForOlderThan(value[0]);
                    arFilter.Value1 = filter.Value1;
                    arFilter.Value1Unit = filter.Value1Unit;
                }
            }
            else
            {
                arFilter.Value1 = filter.Value1;
                if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                    || filter.RuleType == ArchiverFilterRuleType.Size)
                {
                    arFilter.Value1Unit = filter.Value1Unit;
                    arFilter.Value2Unit = filter.Value2Unit;
                }
                arFilter.Value2 = filter.Value2;
            }
            return arFilter.Dto;
        }



        private static string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(dtValue.Year.ToString("0000"));
            stringBuilder.Append("-");
            stringBuilder.Append(dtValue.Month.ToString("00"));
            stringBuilder.Append("-");
            stringBuilder.Append(dtValue.Day.ToString("00"));
            stringBuilder.Append("T");
            stringBuilder.Append(dtValue.Hour.ToString("00"));
            stringBuilder.Append(":");
            stringBuilder.Append(dtValue.Minute.ToString("00"));
            stringBuilder.Append(":");
            stringBuilder.Append(dtValue.Second.ToString("00"));
            stringBuilder.Append("Z");
            return stringBuilder.ToString();
        }


        #region Auto for folder
        //public static void Autoclassification(IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointSetting setting, DateTime startTime, DateTime endTime, IAveORecords records, ref bool hasError)
        //{
        //    IAveListItemCollection items = null;
        //    using (new RA.Common.PerformanceScope(string.Format("Autoclassification Query Items. Folder Url:[{0}]", folder.Url)))
        //    {
        //        items = GetItemsWithCamlQuery(folder, setting, startTime, endTime, aveTaxField.InternalName);
        //        logger.Info("Autoclassification Query  Items Count: {0}", items == null ? 0 : items.Count);
        //    }

        //    using (new RA.Common.PerformanceScope(string.Format("Autoclassification Set Value. Folder Url:[{0}]", folder.Url)))
        //    {
        //        AutoSetTerm(items, folder.ParentList, aveTaxField, setting.AutoClassificationRules, records, setting.IncludeDeclaredRecords, ref hasError);
        //    }
        //}
        //private static IAveListItemCollection GetItemsWithCamlQuery(IAveFolder folder, RMSharePointSetting setting, DateTime startTime, DateTime endTime, string columnInternalName)
        //{
        //    IAveListItemCollection items = null;
        //    try
        //    {
        //        string queryStr = string.Empty;
        //        AveCamlQuery query = AveCamlQuery.CreateAllItemsQuery();
        //        var list = folder.ParentList;
        //        switch ((AutoJobOption)setting.AutoJobOption)
        //        {
        //            case AutoJobOption.None:
        //            case AutoJobOption.SkipAndKeep:
        //                if (setting.RunAutoFullJob || startTime.Equals(DateTime.MinValue))
        //                {
        //                    #region full job query string
        //                    queryStr = @"
        //                    <View Scope='FilesOnly'>
        //                        <Query>
        //                           <Where>
        //                                <IsNull>
        //                                    <FieldRef Name='{0}' />
        //                                </IsNull>
        //                           </Where>
        //                        </Query>
        //                    </View>";
        //                    #endregion
        //                    query.ViewXml = string.Format(queryStr, columnInternalName);
        //                }
        //                else
        //                {
        //                    #region incremental job query string
        //                    queryStr = @"
        //                    <View Scope='FilesOnly'>
        //                        <Query>
        //                           <Where>
        //                              <And>
        //                                 <IsNull>
        //                                    <FieldRef Name='{2}' />
        //                                 </IsNull>
        //                                 <Or>
        //                                    <And>
        //                                       <Gt>
        //                                          <FieldRef Name='Created' />
        //                                          <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{0}</Value>
        //                                       </Gt>
        //                                       <Leq>
        //                                          <FieldRef Name='Created' />
        //                                          <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{1}</Value>
        //                                       </Leq>
        //                                    </And>
        //                                    <And>
        //                                       <Gt>
        //                                          <FieldRef Name='Modified' />
        //                                          <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{0}</Value>
        //                                       </Gt>
        //                                       <Leq>
        //                                          <FieldRef Name='Modified' />
        //                                          <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{1}</Value>
        //                                       </Leq>
        //                                    </And>
        //                                 </Or>
        //                              </And>
        //                           </Where>
        //                        </Query>
        //                    </View>";
        //                    #endregion
        //                    string startTimeStr = CreateISO8601DateTimeFromSystemDateTime(startTime);
        //                    string endTimeStr = CreateISO8601DateTimeFromSystemDateTime(endTime);
        //                    query.ViewXml = string.Format(queryStr, startTimeStr, endTimeStr, columnInternalName);
        //                }
        //                break;
        //            case AutoJobOption.Override:
        //                if (setting.RunAutoFullJob || startTime.Equals(DateTime.MinValue))
        //                {
        //                    queryStr = @"<View Scope='FilesOnly'><Query></Query></View>";
        //                    query.ViewXml = queryStr;
        //                }
        //                else
        //                {
        //                    #region Inc job query string
        //                    queryStr = @"
        //                    <View Scope='FilesOnly'>
        //                        <Query>
        //                           <Where>
        //                                <Or>
        //                                    <And>
        //                                        <Gt>
        //                                            <FieldRef Name='Created' />
        //                                            <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{0}</Value>
        //                                        </Gt>
        //                                        <Leq>
        //                                            <FieldRef Name='Created' />
        //                                            <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{1}</Value>
        //                                        </Leq>
        //                                    </And>
        //                                    <And>
        //                                        <Gt>
        //                                            <FieldRef Name='Modified' />
        //                                            <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{0}</Value>
        //                                        </Gt>
        //                                        <Leq>
        //                                            <FieldRef Name='Modified' />
        //                                            <Value IncludeTimeValue='TRUE' Type='DateTime' StorageTZ='TRUE'>{1}</Value>
        //                                        </Leq>
        //                                    </And>
        //                                </Or>
        //                           </Where>
        //                        </Query>
        //                    </View>";
        //                    #endregion
        //                    string startTimeStr = CreateISO8601DateTimeFromSystemDateTime(startTime);
        //                    string endTimeStr = CreateISO8601DateTimeFromSystemDateTime(endTime);
        //                    query.ViewXml = string.Format(queryStr, startTimeStr, endTimeStr);
        //                }
        //                break;
        //            default:
        //                break;
        //        }
        //        query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
        //        logger.Info("Query XML:{0}", query.ViewXml);
        //        items = list.GetItems(query);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("An error occurred while getting items with caml query, ERROR:{0}", ex.ToString());
        //    }
        //    return items;
        //}
        #endregion
        #endregion

        #region config bcs classification property
        public static void ConfigBCSProperty(IAveSiteProperties siteProperties, string siteUrl, IAveWeb web, Guid termId)
        {
            if (!web.AllProperties.ContainsKey(BCSPropertyName) && termId == Guid.Empty)
            {
                //sp web don't need reset property to empty AND web node disable container level term classification;
                //None Remove && None Add;
                return;
            }
            SPCommonUtility.DisableDenyAddAndCustomizePages(siteProperties, siteUrl);
            if (web.AllProperties.ContainsKey(BCSPropertyName))
            {
                web.AllProperties[BCSPropertyName] = termId.ToString();
            }
            else
            {
                web.AllProperties.Add(BCSPropertyName, termId.ToString());
            }
            web.Update();

            web.ReloadWeb();
            if (!web.AllProperties.ContainsKey(BCSPropertyName))
            {
                throw new Exception("Add web prop RevIM is error, please check site DenyAddAndCustomizePages is disabled.");
            }
            else
            {
                var webPropRevIM = web.AllProperties[BCSPropertyName];
                if (null == webPropRevIM || !string.Equals(webPropRevIM.ToString(), termId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Update web prop RevIM is error, please check site DenyAddAndCustomizePages is disabled.");
                }
            }
        }

        public static bool RemoveBCSProperty(IAveWeb web)
        {
            if (web.AllProperties.ContainsKey(BCSPropertyName))
            {
                web.AllProperties[BCSPropertyName] = Guid.Empty.ToString();//RECO-2481
                web.AllProperties[BCSPropertyName] = null;
                //web.AllProperties.Remove(BCSPropertyName);
                web.Update();
                return true;
            }
            return false;
        }

        public static void ConfigBCSProperty(IAveList list, Guid termId)
        {
            if (!list.RootFolder.Properties.ContainsKey(BCSPropertyName) && termId == Guid.Empty)
            {
                //sp list don't need reset property to empty AND list node disable container level term classification;
                //None Remove && None Add;
                return;
            }

            if (list.RootFolder.Properties.ContainsKey(BCSPropertyName))
            {
                list.RootFolder.Properties[BCSPropertyName] = termId.ToString();
            }
            else
            {
                list.RootFolder.Properties.Add(BCSPropertyName, termId.ToString());
            }
            
            list.RootFolder.Update();
            // Trigger a SharePoint change log entry.
            // For GenericList, create and delete a temporary item; otherwise use a temporary folder.
            try
            {
                if (list.BaseType == AveBaseType.GenericList)
                {
                    // Create a transient generic list item using 'File' underlying type (represents a list item in wrapper)
                    var tempItem = list.AddItem(list.RootFolder.ServerRelativeUrl, AveFileSystemObjectType.File);
                    var tempTitle = "_RevIMTempTrigger_" + Guid.NewGuid().ToString("N");
                    try { tempItem["Title"] = tempTitle; } catch { }
                    tempItem.Update();
                    tempItem.Delete();
                    logger.Info("Created and deleted temp item '{0}' to trigger change log for BCS property update.", tempTitle);
                }
                else
                {
                    var triggerFolderName = "_RevIMTempTrigger_" + Guid.NewGuid().ToString("N");
                    var tempFolder = list.RootFolder.Folders.Add(triggerFolderName);
                    tempFolder.Update();
                    tempFolder.Delete();
                    logger.Info("Created and deleted temp folder '{0}' to trigger change log for BCS property update.", triggerFolderName);
                }
            }
            catch (Exception ex)
            {
                // Non-critical: failure should not block property configuration.
                logger.Warn("Failed to trigger change log via temp artifact. Error: {0}", ex);
            }
            
        }
        public static bool RemoveBCSProperty(IAveList list)
        {
            if (list.RootFolder.Properties.ContainsKey(BCSPropertyName))
            {
                //list.RootFolder.Properties.Remove(BCSPropertyName);
                list.RootFolder.Properties[BCSPropertyName] = Guid.Empty.ToString();//RECO-2481
                list.RootFolder.Properties[BCSPropertyName] = null;
                list.RootFolder.Update();
                return true;
            }
            return false;
        }
        public static void ConfigBCSProperty(IAveFolder folder, Guid termId)
        {
            if (!folder.Properties.ContainsKey(BCSPropertyName) && termId == Guid.Empty)
            {
                //sp folder don't need reset property to empty AND folder node disable container level term classification;
                //None Remove && None Add;
                return;
            }


            if (folder.Properties.ContainsKey(BCSPropertyName))
            {
                folder.Properties[BCSPropertyName] = termId.ToString();
            }
            else
            {
                folder.Properties.Add(BCSPropertyName, termId.ToString());
            }
            folder.Update();
        }
        public static bool RemoveBCSProperty(IAveFolder folder)
        {
            if (folder.Properties.ContainsKey(BCSPropertyName))
            {
                //folder.Properties.Remove(BCSPropertyName);
                folder.Properties[BCSPropertyName] = Guid.Empty.ToString();//RECO-2481
                folder.Properties[BCSPropertyName] = null;//RECO-2481
                folder.Update();
                return true;
            }
            return false;
        }
        public static bool NeedUpdateContainer(object obj, Guid termId)
        {
            bool result = false;
            if (obj is IAveWeb)
            {
                var web = obj as IAveWeb;
                if (web.AllProperties.Contains(BCSPropertyName))
                {
                    if (web.AllProperties[BCSPropertyName].ToString() != termId.ToString())
                    {
                        result = true;
                    }
                }
                else
                {
                    result = true;
                }
            }
            else if (obj is IAveList)
            {
                var list = obj as IAveList;
                if (list.RootFolder.Properties.ContainsKey(BCSPropertyName))
                {
                    if (list.RootFolder.Properties[BCSPropertyName].ToString() != termId.ToString())
                    {
                        result = true;
                    }
                }
                else
                {
                    result = true;
                }
            }
            else if (obj is IAveFolder)
            {
                var folder = obj as IAveFolder;
                if (folder.Properties.ContainsKey(BCSPropertyName))
                {
                    if (folder.Properties[BCSPropertyName].ToString() != termId.ToString())
                    {
                        result = true;
                    }
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
        #endregion

        #region config related records column
        public static bool AddSiteCollectionRelatedColumn(IAveSite site)
        {
            var siteField = site.RootWeb.Fields.GetFieldById(RelatedColumnId, false);
            if (siteField != null)
            {
                logger.Info("site collection config app column {0}", site.Url);
                return false;
            }
            else
            {
                //string columnSchema = "<Field Type=\"Note\" ReadOnly=\"TRUE\" DisplayName='" + RelatedColumnDisplayName + "' RichText=\"TRUE\" RichTextMode=\"FullHtml\" Group=\"Custom Columns\"  ID=\"{b40273fb-26d2-40e8-9a34-dd20bc9ca1d7}\"   Name='" + RelatedColumnInternalName + "' ShowInDisplayForm='TRUE' ShowInEditForm='FALSE' ShowInNewForm='FALSE' ShowInFileDlg='TRUE' ShowInListSettings='TRUE' ShowInVersionHistory='TRUE' ShowInViewForms='TRUE' UnlimitedLengthInDocumentLibrary=\"TRUE\"  />";
                logger.Info("create new sitecollection app column {0}", site.Url);
                //string columnSchema = "<Field Type=\"Note\" DisplayName='" + RelatedColumnDisplayName + "' RichText=\"TRUE\" RichTextMode=\"FullHtml\" Group=\"Custom Columns\"  ID=\"{b40273fb-26d2-40e8-9a34-dd20bc9ca1d7}\"   Name='" + RelatedColumnInternalName + "' ShowInDisplayForm='TRUE' ShowInEditForm='FALSE' ShowInNewForm='FALSE' ShowInFileDlg='FALSE' ShowInListSettings='FALSE' ShowInVersionHistory='TRUE' ShowInViewForms='TRUE' UnlimitedLengthInDocumentLibrary=\"TRUE\"  />";

                string columnSchema = "<Field Type=\"Note\" ReadOnly=\"TRUE\" DisplayName='" + RelatedColumnDisplayName + "' RichText=\"TRUE\" RichTextMode=\"FullHtml\" Group=\"Custom Columns\"  ID=\"{b40273fb-26d2-40e8-9a34-dd20bc9ca1d7}\"   Name='" + RelatedColumnInternalName + "' ShowInDisplayForm='TRUE' ShowInEditForm='TRUE' ShowInNewForm='TRUE' ShowInFileDlg='TRUE' ShowInListSettings='TRUE' ShowInVersionHistory='TRUE' ShowInViewForms='TRUE' UnlimitedLengthInDocumentLibrary=\"TRUE\"  />";

                site.RootWeb.Fields.AddFieldAsXml(columnSchema, true, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes);
                site.RootWeb.Update();
                return true;
            }
        }
        public static bool AddListRelatedColumn(IAveSite site, IAveList list)
        {
            var siteRelatedColumn = site.RootWeb.Fields.GetFieldById(RelatedColumnId, false);
            var listRelateColumn = list.Fields.GetFieldById(RelatedColumnId, false);
            if (siteRelatedColumn == null)
            {
                AddSiteCollectionRelatedColumn(site);
                siteRelatedColumn = site.RootWeb.Fields.GetFieldById(RelatedColumnId, false);
            }
            if (siteRelatedColumn != null && listRelateColumn == null)
            {
                list.Fields.AddFieldAsXml(siteRelatedColumn.SchemaXml, true, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes);
                list.Update();
                //var test = list.Fields.GetById(new Guid("b40273fb-26d2-40e8-9a34-dd20bc9ca1d7"));
                //var ro = test.ReadOnlyField;
                return true;
            }
            else if (siteRelatedColumn == null)
            {
                throw new Exception("Site not config related column");
            }
            return false;
        }
        public static bool VerifyExistsSiteRelatedColumn(IAveSite site)
        {
            var siteRelatedColumn = site.RootWeb.Fields.GetFieldById(RelatedColumnId, false);
            return !(null == siteRelatedColumn);
        }
        public static void DeleteSiteCollectionRelatedColumn(IAveSite site)
        {

            var siteField = site.RootWeb.Fields.GetFieldById(RelatedColumnId, false);
            if (siteField != null)
            {
                siteField.ReadOnlyField = false;
                siteField.Update();

                siteField.Delete();
                siteField.Update();
                logger.Info("remove site collection config app column {0}", site.Url);
            }
        }
        public static bool DeleteListRelatedColumn(IAveSite site, IAveList list)
        {
            var siteField = site.RootWeb.Fields.GetFieldById(RelatedColumnId, false);
            if (siteField != null)
            {
                siteField.ReadOnlyField = false;
                siteField.Update();

                siteField.Delete();
                site.RootWeb.Update();
                logger.Info("remove site collection config app column {0}", list.RootFolder.Url);
            }

            var listField = list.Fields.GetFieldById(RelatedColumnId, false);
            if (listField != null)
            {
                listField.ReadOnlyField = false;
                listField.Update();

                listField.Delete();
                listField.Update();
                logger.Info("remove list config app column {0}", list.RootFolder.Url);
                return true;
            }
            return false;
        }

        public static bool UninstallApp(IAveWeb aveWeb, Guid appId)
        {
            try
            {
                var apps = aveWeb.GetAppInstancesByProductId(appId);
                if (apps.Count > 0)
                {
                    var app = apps.First();
                    if (app.Status == AveAppInstanceStatus.Uninstalling)
                    {
                        logger.Info("remove app {0}, status is uninstalling.", aveWeb.Url);
                    }
                    else if (app.Status == AveAppInstanceStatus.Installing)
                    {
                        //app.Cancel
                        logger.Info("remove app {0}, status is installing.", aveWeb.Url);
                    }
                    else if (app.Status == AveAppInstanceStatus.Installed)
                    {
                        logger.Info("remove app {0}.", aveWeb.Url);
                        app.Uninstall();
                    }
                    else
                    {
                        logger.Info("remove app {0}, status is {1}.", aveWeb.Url, app.Status.ToString());
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Info("Uninstall app failed,web: {0}, error: {1}", aveWeb.Url, ex.ToString());
                throw;
            }
        }

        protected static int GetMaxItemsPerThrottledOperation(IAveSite aveSite)
        {
            int maxItemsPer = 2000; //5000;  //SPO默认值为5000 并且不能修改， 某些Library 5000分页查询依然会超出Throttle， 限制到2000   from CI
            try
            {
                var dataCacheType = aveSite.GetType().GetProperty("DataCache");
                var dataCacheObj = dataCacheType.GetValue(aveSite);
                BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache", InstanceBindFlags);
                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
                var propertiesDic = (propertiesCacheObj as AveDictionary<string, object>);
                object maxItemsPerObj;
                if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
                {
                    maxItemsPer = Convert.ToInt32(maxItemsPerObj);
                    logger.Info($"GetMaxItemsPerThrottledOperation succeed. Count:[{maxItemsPer}]");
                    if (maxItemsPer > 2000)
                    {
                        logger.Info("MaxItemsPerThrottledOperation is > 2000, limit it to 2000");
                        maxItemsPer = 2000;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
            }
            return maxItemsPer;
        }

        #endregion

        #region config physical setting
        public static void ConfigPhysicalSetting(IAveSite site)
        {
            string libPhyName = Common.Util.GetAppSettingValue("RevIMHoldPhysicalLibraryName");
            string colPhyName = Common.Util.GetAppSettingValue("RevIMHomeLocationName");
            string contentTypeNames = Common.Util.GetAppSettingValue("RevIMWorkflowContentTypes");
            string requestListName = Common.Util.GetAppSettingValue("RevIMRequestListName");
            List<string> needRemoveBCSContentTypes = new List<string>();
            if (!string.IsNullOrEmpty(contentTypeNames))
            {
                needRemoveBCSContentTypes = contentTypeNames.Split(';').ToList();
            }
            if (string.IsNullOrEmpty(libPhyName) || string.IsNullOrEmpty(colPhyName))
            {
                //TO DO job detali
                logger.Warn("Physical Config file error");
                //TO DO Update SP Setting....
                throw new Exception("Physical Config file error");
            }
            try
            {
                var physicalLibrary = site.RootWeb.GetListByName(libPhyName, true);
                var requestList = site.RootWeb.GetListByName(requestListName, true);
                RemoveBCSColumn(physicalLibrary, needRemoveBCSContentTypes);
                ConfigPhysicalLibraryColumn(physicalLibrary);
                ConfigPhysicalLibraryColumn(requestList);
                ConfigBoxTypeColumn(physicalLibrary);
                ConfigBoxTypeColumn(requestList);
            }
            catch (Exception e)
            {
                logger.Warn("Set physical node error {0}", e.ToString());
            }
        }
        /// <summary>
        /// remove bcs column in physical list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="contentTypeNames"></param>
        private static void RemoveBCSColumn(IAveList list, List<string> contentTypeNames)
        {
            foreach (var contentType in list.ContentTypes)
            {
                try
                {
                    if (contentTypeNames.Contains(contentType.Name))
                    {
                        var fieldLink = contentType.FieldLinks[BCSColumnID];
                        fieldLink.Delete();
                        contentType.Update(true);
                    }
                }
                catch (Exception e)
                {
                    logger.Info("Remove Field link error {0}", e.ToString());
                }
            }
            list.Update();
        }
        private static void ConfigPhysicalLibraryColumn(IAveList list)
        {
            try
            {
                var phyColName = Common.Util.GetAppSettingValue("RevIMHomeLocationName");
                var locationTermSetId = TermSetDao.GetRMTermSet((int)TermSetType.Physical).UniqueId;
                var physicalColumn = list.Fields.GetField(phyColName);
                if (physicalColumn == null)
                {
                    //TO  DO Detail & update sp setting
                    throw new Exception("RevIMHomeLocationName is null");
                }
                IAveTaxonomyField physicalLocationCol = physicalColumn as IAveTaxonomyField;
                var termStoreId = list.ParentWeb.Site.AveSPTaxonomySession.TermStores[0].ID;
                if (termStoreId.Equals(physicalLocationCol.SspId) && locationTermSetId.Equals(physicalLocationCol.TermSetId))
                {
                    //TO DO detail && add skip logic
                    //******                ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                    //string.Empty, "RM_SS_ConfigPhysicalAction", JobReportDetailStatus.Skipped, string.Empty));
                    logger.Info("Skip update the physical column");
                    return;
                }
                physicalLocationCol.SspId = termStoreId;
                physicalLocationCol.TermSetId = locationTermSetId;
                physicalLocationCol.EnforceUniqueValues = false;
                physicalLocationCol.AllowMultipleValues = false;
                physicalLocationCol.DefaultValue = string.Empty;
                physicalLocationCol.Title = phyColName;
                physicalLocationCol.Indexed = true;
                physicalLocationCol.Required = true;
                physicalLocationCol.Description = string.Empty;
                physicalLocationCol.Update();
                //******    ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                //string.Empty, "RM_SS_ConfigPhysicalAction", JobReportDetailStatus.Success, string.Empty));
                //TO DO detail && update setting status
            }
            catch (Exception e)
            {
                logger.Warn("Config physical setting error {0}", e.ToString());
                // ******           ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                //string.Empty, "RM_SS_ConfigPhysicalAction", JobReportDetailStatus.Failed, e.Message));
                throw new Exception("Config Physical setting error");
            }
        }
        private static void ConfigBoxTypeColumn(IAveList list)
        {
            try
            {
                var boxTypeColumnName = Common.Util.GetAppSettingValue("RevIMBoxTypeName");
                var boxTypeField = list.Fields.GetField(boxTypeColumnName);
                if (boxTypeField == null)
                {
                    logger.Warn("Get Physical Box type field error {0}", list.RootFolder.Url);
                    //*****                ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                    //string.Empty, "RM_SS_ConfigBoxTypeColumnAction", JobReportDetailStatus.Failed, "RM_SS_NotFoundBoxTypeFiled"));
                    //TO DO Detail
                    //Setting Status
                    throw new Exception("RevIMBoxTypeName is null");
                }
                IAveFieldChoice boxTypeChoiceField = boxTypeField as IAveFieldChoice;
                List<RMContainer> allContainers = ContainerDao.GetAllContainers();
                string defaultValue = string.Empty;
                //List<string> containerNames = new List<string>();
                StringCollection containerNames = new StringCollection();
                foreach (var container in allContainers)
                {
                    if (container.IsDefault)
                    {
                        defaultValue = container.TypeName;
                    }
                    if (!container.IsRemoved)
                    {
                        containerNames.Add(container.TypeName);
                    }
                }
                if ((boxTypeChoiceField.DefaultValue.Equals(defaultValue) && boxTypeChoiceField.Choices.Equals(containerNames)) || allContainers.Count == 0)
                {
                    logger.Info("skip config box type column");
                    //********                   ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                    //string.Empty, "RM_SS_ConfigBoxTypeColumnAction", JobReportDetailStatus.Skipped, string.Empty));
                    //this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigBoxTypeAction, JobDetailsStatus.Skipped, null);
                    return;
                }

                boxTypeChoiceField.DefaultValue = defaultValue;
                boxTypeChoiceField.Choices.Clear();
                foreach (var containerName in containerNames)
                {
                    boxTypeChoiceField.Choices.Add(containerName);
                }
                boxTypeChoiceField.Update();
                // ******               ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                //string.Empty, "RM_SS_ConfigBoxTypeColumnAction", JobReportDetailStatus.Success, string.Empty));
                //TO DO add detail
            }
            catch (Exception e)
            {
                // ******               ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                //string.Empty, "RM_SS_ConfigBoxTypeColumnAction", JobReportDetailStatus.Failed, e.Message));
                logger.Warn("Config box type column error {0}", e.ToString());
                throw new Exception("Config box type column error");
                //TO DO detail && set setting status.
            }
        }
        #endregion

        #region hold && records
        public static bool IsBlockEditAndDeleteRecord(IAveListItem item)
        {
            return IsBlockEditAndDeleteRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }
        private static int GetHoldAndRecordStatus(IAveListItem item)
        {
            int result = 0;
            try
            {
                if ((GetBoolIprPropertyCore(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
                {
                    try
                    {
                        if (item.Fields.Contains(HoldRecordStatus))
                        {
                            object obj2 = item[HoldRecordStatus];
                            if ((obj2 != null) && !int.TryParse(obj2.ToString(), out result))
                            {
                                result = 0;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("An error occur in get hold and declare status, reason : {0}.", ex.ToString()));
            }
            return result;
        }
        internal static Guid HoldRecordStatus
        {
            get
            {
                return new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E");
            }
        }
        private static bool GetBoolIprPropertyCore(IAveList list, string propName)
        {
            bool? nullable = null;
            if (list != null && list.RootFolder != null && list.RootFolder.Properties != null)
            {
                object obj = list.RootFolder.Properties[propName];
                if (obj != null) nullable = new bool?(obj.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase));
            }
            return (nullable == true);
        }
        private static bool IsHoldOrRecordsEnabled(IAveList list)
        {
            if (list == null || list.Fields == null)
            {
                throw new ArgumentNullException("list");
            }
            if (list.Fields.Contains(new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")))
            {
                return (list.Fields[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")] != null);
            }
            else
            {
                return false;
            }
        }
        internal enum HoldAndRecordStatusMask
        {
            EditBlockedMask = 1, //只要不允许编辑, 这位值就为1, 包括Hold 和 Block edit and delete
            RecordMask = 0x10, //Record 文件，这位值 就是1 ， 包含Block edit and delete， block delete
            DeleteBlockedMask = 0x100,//只要不允许删除，这位值就为1, 包括 Hold， block edit and delete， block delete
            HoldMask = 0x1000, //Hold 文件，这位值就是 1， 
        }
        #endregion

        #region Get/Remove Field Default Value in Folder
        public static List<string> GetFoldersWithDefaultValue(IAveList list, string filedInernalName, string parentFolderPath = "")
        {
            logger.Info($"Get folder default values for list:{list.Title}");
            var result = new List<string>();
            string foldersXml = GetXmlWithFolderDefaultValue(list);
            if (!string.IsNullOrEmpty(foldersXml))
            {
                logger.Info($"Folder Default Values:{foldersXml}");
                var defaultsXmlDoc = new XmlDocument();
                try
                {
                    defaultsXmlDoc.LoadXml(foldersXml);
                }
                catch (Exception e)
                {
                    logger.Warn("xml have special character {0}", e.ToString());
                    var fci = new FileCreationInformation();
                    if (foldersXml.Contains("&"))
                    {
                        var replaceXml = foldersXml.Replace("&", "%26");
                        defaultsXmlDoc.LoadXml(replaceXml);
                        logger.Info("Replace and reload xml");
                    }
                    else
                    {
                        throw e;
                    }
                }
                XmlNodeList nodes = defaultsXmlDoc.DocumentElement.SelectNodes(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "/MetadataDefaults/a/DefaultValue[@FieldName='{0}']", filedInernalName));
                foreach (XmlNode node in nodes)
                {
                    var currentFolder = WebUtility.UrlDecode(((XmlElement)node.ParentNode).GetAttribute("href"));
                    logger.Info($"Current Folder:{currentFolder}");
                    if (string.IsNullOrEmpty(parentFolderPath))
                    {
                        result.Add(currentFolder);
                    }
                    else
                    {
                        var normalizedParentFolderPath = parentFolderPath.TrimEnd('/') + "/";
                        if (currentFolder == parentFolderPath || currentFolder.StartsWith(normalizedParentFolderPath))
                        {
                            result.Add(currentFolder);
                        }
                        else
                        {
                            logger.Info($"{currentFolder} is not a sub folder of {parentFolderPath}");
                        }
                    }
                }
            }
            return result;
        }

        private static string GetXmlWithFolderDefaultValue(IAveList list)
        {
            IAveFolder formsFolder = list.GetFolder(list.RootFolder.ServerRelativeUrl + "/forms");

            var clientLocationBasedDefaultsFile =
                formsFolder.Files.FirstOrDefault(
                    f => f.Name.ToLowerInvariant() == "client_LocationBasedDefaults.html".ToLowerInvariant());

            if (clientLocationBasedDefaultsFile != null)
            {
                return ReadFileContent(clientLocationBasedDefaultsFile);
            }

            return null;
        }
        private static string ReadFileContent(IAveFile file)
        {
            Stream stream = file.OpenBinaryStream();
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        #endregion

        #region Remove Unique
        public static bool DeleteUniqueIdColumn(IAveSite siteCollection)
        {
            //try
            //{
            IAveField field = null;
            field = siteCollection.RootWeb.Fields.GetFieldById(RevIMUniqueIDColumnID, false);
            if (field != null)
            {
                field.ReadOnlyField = false;
                field.Update();

                field.Delete();
                field.Update();
                return true;
            }
            return false;

            //}
            //catch (Exception e)
            //{
            //    logger.Warn("Config site collection unique id column failed {0}", e.ToString());
            //}
        }
        public static bool DeleteUniqueIdColumn(IAveList list)
        {
            //try
            //{
            IAveField field = list.Fields.GetFieldById(RevIMUniqueIDColumnID, false);
            if (field != null)
            {
                field.ReadOnlyField = false;
                field.Update();

                field.Delete();
                field.Update();
                return true;
            }
            return false;
            //}
            //catch (Exception e)
            //{
            //    logger.Warn("Config list unique id column failed {0}", e.ToString());
            //}
        }



        #endregion

        #region AI

        public static void BatchPredictTerm(List<IAveListItem> items, IAveTaxonomyField taxField)
        {
            if (items != null &&  items.Count > 0)
            {
                logger.Info($"start batch predict, items count: {items.Count}");
                var aveSite = items[0].ParentList.ParentWeb.Site;
                var isZeroShotFeature = RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                RMMachineLearningUtility.SetMinTermScore(isZeroShotFeature ? RMMLPredictHelper.MinTermScore4ZeroShot : RMMLPredictHelper.MinTermScore);
                RMMachineLearningUtility.StartPredictTerm(items, taxField, aveSite);
                logger.Info($"end batch predict, items count: {items.Count}");
            }
        }

        public static async Task<bool> SetIntelligenceClassificationAsync(Guid remoteSiteId, IAveList list, IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointSetting setting, DateTime startTime, DateTime endTime, IAveORecords records, SPOLabelUtility labelUtility, ConfigSiteSetting configSiteSetting = null)
        {
            bool hasError = false;
            using (new PerformanceScope("RMSPSettingUtility.SetIntelligenceClassification", $"RMSPSettingUtility.SetIntelligenceClassification{folder.Url}", true))
            {
                List<string> excludePath = SettingsHelpers.GetExcludePath(remoteSiteId, list);
                excludePath = excludePath.Where(p => p.StartsWith(folder.ServerRelativeUrl.TrimEnd('/') + "/")).ToList();

                bool needQueryNext = false;
                int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
                int maxItemId = GetLastItemId(list, list.RootFolder);
                logger.Info($"The max ID of item in library is: {maxItemId}");

                int startIndex = 0;
                IAveListItemCollection items = null;
                do
                {
                    using (var queryAuto = new PerformanceScope("QueryIntelligenceProcessData", $"IntelligenceSetValues {folder.ServerRelativeUrl} start{startIndex}", true))
                    {
                        AveCamlQuery query = GetAutoClassificationQuery(list, folder, setting, startTime, endTime, aveTaxField.InternalName, startIndex, startIndex + rowLimit, rowLimit);
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            items = list.GetItemsForRecords(query);
                        }
                        ReportManager.IncreaseBase(items.Count);
                        logger.Info($"ai process folder url {folder.ServerRelativeUrl} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");
                    }
                    using (var queryAuto = new PerformanceScope("IntelligenceSetValues", $"IntelligenceSetValues {folder.ServerRelativeUrl} count {items.Count}", true))
                    {
                        hasError = await IntelligenceSetValuesAsync(items, list, excludePath, aveTaxField, records, setting, configSiteSetting: configSiteSetting, labelUtility, remoteSiteId);
                    }
                    if (startIndex + rowLimit < maxItemId)
                    {
                        needQueryNext = true;
                        startIndex += rowLimit;
                        logger.Info($"PagingInfo:{startIndex}");
                    }
                    else
                    {
                        needQueryNext = false;
                    }
                }
                while (needQueryNext);
            }
            return hasError;
        }

        private static async Task<bool> IntelligenceSetValuesAsync(IAveListItemCollection items, IAveList list, List<string> excludePath, IAveTaxonomyField aveTaxField, IAveORecords records, RMSharePointSetting setting, ConfigSiteSetting configSiteSetting, SPOLabelUtility labelUtility, Guid remoteSiteId)
        {
            var hasError = false;
            BatchPredictTerm(items.ToList(), aveTaxField);

            if (items.Count > itemsPerTask)
            {
                logger.Info("Use multi thread to run intelligence classification.");
                AveTenantTasks.RunParallel(items, itemsPerTask, new CancellationTokenSource(), item =>
                {
                    if (IntelligenceSetOneItemAsync(item, list, excludePath, aveTaxField, records, setting, configSiteSetting, labelUtility, remoteSiteId).Result)
                    {
                        hasError = true;
                    }
                });
            }
            else
            {
                foreach (var item in items)
                {
                    if (await IntelligenceSetOneItemAsync(item, list, excludePath, aveTaxField, records, setting , configSiteSetting, labelUtility, remoteSiteId))
                    {
                        hasError = true;
                    }
                }
            }

            return hasError;
        }

        private static async Task<bool> IntelligenceSetOneItemAsync(IAveListItem item, IAveList list, List<string> excludePath, IAveTaxonomyField aveTaxField, IAveORecords records, RMSharePointSetting setting, ConfigSiteSetting configSiteSetting, SPOLabelUtility labelUtility, Guid remoteSiteId)
        {
            var hasError = false;
            ReportManager.Increase();

            if (!NeedIncluedeFolder(setting) && item.FileSystemObjectType == AveFileSystemObjectType.Folder)
            {
                logger.Info("Current item:{0} is folder and setting is not include folder, skip set classification.", item.Url);
                return hasError;
            }
            if (!NeedSkip(item, excludePath))
            {
                hasError = await IntelligenceSetOneItemTermAsync(item, list, aveTaxField, records, setting.IncludeDeclaredRecords, labelUtility, setting, remoteSiteId, configSiteSetting);
            }

            return hasError;
        }

        public static async System.Threading.Tasks.Task<bool> IntelligenceSetOneItemTermAsync(IAveListItem item, IAveList list, IAveTaxonomyField aveTaxField, IAveORecords records,
            bool includeDeclaredRecords, SPOLabelUtility labelUtility, RMSharePointSetting setting, Guid remoteSiteId,ConfigSiteSetting configSiteSetting = null)
        {
            bool hasError = false;
            var isUpdateDeclared = false;
            string itemFullUrl = list.ParentWeb.Url + "/" + item.Url;
            using (new PerformanceScope("RMSPSettingUtility.IntelligenceSetOneItemTerm", addToStatistics: true))
            {
                if (JobContext.IsCSDTenant)
                {
                    logger.Error("CSD tenant does not support intelligence classification.");
                    return true;
                }

                try
                {
                    if (IsBlockEditAndDeleteRecord(item))
                    {
                        logger.Info("Item is Block Edit and delete {0}", item.ID);
                        if (includeDeclaredRecords)
                        {
                            isUpdateDeclared = true;
                        }
                        else
                        {
                            return hasError;
                        }
                    }

                    IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;
                    ApplySettingPredictResult predictResult = GetPredictResult(item, setting);
                    Guid termId = predictResult.TermId;

                    string actionI18NKey = predictResult.IsApplyDefaultTerm ? "RM_SS_ApplyExist" : "RM_JS_JMD_Action_SetAIClassification";


                    if (!termId.Equals(Guid.Empty))
                    {
                        string oldValue = item[aveTaxField.InternalName] == null ? null : ((string)item[aveTaxField.InternalName]).ToLowerInvariant();
                        Guid oldTermId = Guid.Empty;
                        try
                        {
                            if (oldValue != null && !string.IsNullOrEmpty(oldValue.ToString()))
                            {
                                var valueString = oldValue.ToString().Split('|');
                                if (valueString.Length > 1)
                                {
                                    oldTermId = new Guid(valueString[1]);
                                }
                            }
                        }
                        catch
                        {
                            logger.Error("Convert to guid failed.");
                        }
                        //if (string.IsNullOrEmpty(oldValue) || !oldValue.Contains(termId.ToString().ToLowerInvariant()))
                        //{
                            taxValue.TermGuid = termId.ToString();
                            taxValue.Label = predictResult?.TermName;
                            logger.Info("set Item classification value for Intelligence {0}", item.ID);
                            if (isUpdateDeclared)
                            {
                                try
                                {
                                    WaitExecuteAction(() =>
                                    {
                                        records.UndeclareItemAsRecord(item);
                                    });
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("undeclare item failed {0}:{1}", item.Url, e.ToString());
                                }
                            }
                            bool labelNotExist = false;
                            try
                            {
                                if (predictResult.IsUpdateSharePoint)
                                {
                                    item[aveTaxField.ID] = taxValue;
                                    item[aveTaxField.TextField] = taxValue.ToString();
                                    //item.SystemUpdate();
                                    WaitExecuteAction(() =>
                                    {
                                        item.SystemUpdateForRecords();//*********
                                    });
                                    using (new PerformanceScope("RMSPSettingUtility.UpdateLabelTotal", addToStatistics: true))
                                    {
                                        var recId = IDGenerator.GetRecordId(list.ParentWeb.Site.ID, item.UniqueId);
                                        labelNotExist = labelUtility.UpdateLabel(item, termId, recId, oldTermId);
                                    }
                                    if(!labelNotExist) 
                                    { 
                                        SendSPSettingReport(item.Name, itemFullUrl, actionI18NKey, JobDetailsStatus.Successful, setting.ColumnName, taxValue.Label);
                                    }
                                }
                                else
                                {
                                    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SkipAIManualApproval", JobDetailsStatus.Successful, setting.ColumnName, taxValue.Label);
                                    logger.Info($"skip update the current item's term value, url:{item.Url}, because IsUpdateSharePoint is false");
                                }
                                
                                if (predictResult.IsSyncCosmosDB)
                                {
                                await RMMachineLearningDataSyncManager.SyncItemToDBAsync(item, remoteSiteId, setting, predictResult);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn("update item failed {0}:{1}", item.Url, e.ToString());
                                throw;
                            }
                            if (isUpdateDeclared)
                            {
                                using (PerformanceScope scope = new PerformanceScope("IntelligenceSetOneItemTerm.DeclareItemAsRecord", "", true))
                                {
                                    WaitExecuteAction(() =>
                                    {
                                        var dItem = list.GetItemById(item.ID);
                                        records.DeclareItemAsRecord(dItem);
                                    });
                                }
                            }

                            if (!JobContext.IsCSDTenant && labelNotExist)
                            {
                                HasFailedReport = true;
                                SendSPSettingReport(item.Name, itemFullUrl, actionI18NKey, JobDetailsStatus.Failed, setting.ColumnName, taxValue.Label, "RM_SPO_ApplySetting_LabelNotExist");
                            }
                        //}
                    }
                    // write predict result faile
                    var predictResultFail = RMMLPredictHelper.GetPredictRequestFailCache(item.UniqueId);
                    if (predictResultFail != null)
                    {
                        //SendSPSettingReport(item.Name, itemFullUrl, "RM_JM_Details_Failed_ExtractFileContentFaile", JobDetailsStatus.Failed);
                        hasError = true;
                    }
                }
                catch (Exception e)
                {
                    hasError = true;
                    logger.Error("Intelligence set classification value failed {0}:{1}", itemFullUrl, e.ToString());
                    var expMsg = GetExceptionMessage(e);
                    //if (JobContext.IsCSDTenant)
                    {
                        SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAIClassification", JobDetailsStatus.Failed, setting.ColumnName, string.Empty, expMsg);
                    }
                }
                return hasError;
            }
        }

        #endregion
        /*   private static AveCamlQuery GetItemCamlQuery(int rowlimit)
           {
               CAMLManager cm = new CAMLManager();
               cm.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, "ID", Types.FieldTypes.Number, Types.QueryTypes.IsNotNull, ""));
               AveCamlQuery query = new AveCamlQuery();
               cm.ScopeType = Types.ScopeTypes.Default;
               cm.RowLimit = rowlimit;
               string queryXml = cm.GetFullCAML();
               query.ViewXml = queryXml;
               return query;
           }*/

        public static RMSharePointSetting ConvertTeamSettingToSharePointSetting(RMTeamsSetting teamsSetting)
        {
            RMSharePointSetting sharePointSetting = new RMSharePointSetting();
            if (teamsSetting != null)
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.LicenseKey = ReadEmbeddedLicense(); 
                    cfg.CreateMap<RMTeamsSetting, RMSharePointSetting>(MemberList.Destination)
                        .ForMember(dto => dto.SiteGroupId, conf => conf.MapFrom(ol => ol.TeamsGroupId));
                }, NullLoggerFactory.Instance);
                var mapper = config.CreateMapper();
                sharePointSetting = mapper.Map<RMSharePointSetting>(teamsSetting);
            }
            return sharePointSetting;
        }

        private static string ReadEmbeddedLicense()
        {
            var assembly = typeof(SPSettingsUtility).Assembly;
            using var stream = assembly.GetManifestResourceStream("AvePoint.RA.SharePoint.RMSharePointColumn.automapper.lic");
            if (stream == null)
                throw new InvalidOperationException("Embedded resource 'AvePoint.RA.SharePoint.RMSharePointColumn.automapper.lic' not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        static class SettingsHelpers
        {
            private static ISharePointSettingDao SettingDao = new SharePointSettingDao();
            private static ITeamsSettingDao TeamsSettingDao = new TeamsSettingDao();
            public static List<string> GetExcludePath(Guid remoteSiteId, IAveList list)
            {
                return sourceType == RMBrowseTreeNodeSourceType.Teams ?
                                    TeamsSettingDao.GetFolderSettingUnderList(list.ID, remoteSiteId, teamsId).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList() :
                                    SettingDao.GetFolderSettingUnderList(list.ID, remoteSiteId).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
            }
            public static RMSharePointSetting LoadSharePointSetting(Guid id, Guid siteId, bool includeOnlySetPhysicalNode = false)
            {
                return sourceType == RMBrowseTreeNodeSourceType.Teams ?
                                    ConvertTeamSettingToSharePointSetting(TeamsSettingDao.LoadTeamsSetting(id, teamsId, siteId, includeOnlySetPhysicalNode)) :
                                    SettingDao.LoadSharePointSetting(id, siteId, includeOnlySetPhysicalNode);
            }

            public static SourceFlag GetSourceFlag()
            {
                SourceFlag source;
                if (sourceType == RMBrowseTreeNodeSourceType.Teams)
                {
                    source = SourceFlag.Teams;
                }
                else
                {
                    source = SourceFlag.SharePoint;
                }

                return source;
            }
        }
    }
    public enum SettingResult
    {
        None = -1,
        Add = 0,
        Update = 1,
        SKip = 2,
        Failed = 3,
        UseExistSkip = 4,
        Delete = 5,
        SkipDelete = 6,
    }

    public enum FieldConflict
    {
        None = -1,
        ColumnExisting = 1,
        NameConflict = 2,
        ColumnNotFound = 3,
        SkipCheckColumn = 4
    }
}
