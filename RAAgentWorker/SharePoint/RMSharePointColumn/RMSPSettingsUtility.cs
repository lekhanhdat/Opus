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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Hybrid.AgentContract.Rule;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.OperationSystem;
using AvePoint.RA.Common.Global.Throttle;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.SharePoint.Common.Threads;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using ServerFilterPolicy = AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon;
using AgentUtil = AvePoint.RA.SharePoint.Common.Util;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class SPSettingsUtility
    {
        //private static readonly AveLogger logger = AveLogger.GetInstance(typeof(SPSettingsUtility));
        protected static readonly AveLogger logger = AveLogger.GetInstance(typeof(SPSettingsUtility));
        private static readonly string BCSColumnInternalName = "RevIMBCS";
        private static readonly string BCSPropertyName = "RevIM";
        private static readonly Guid BCSColumnID = new Guid("20f84bba906045b4af568ee102a52dcb");
        private static readonly Guid PhysicalColumnId = new Guid("30f84bba906045b4af568ee102a52dcb");
        private static readonly Guid RelatedColumnId = new Guid("b40273fb-26d2-40e8-9a34-dd20bc9ca1d7");
        private static readonly Guid RevIMUniqueIDColumnID = new Guid("40f84bba906045b4af568ee102a52dcb");
        private static readonly string RelatedColumnInternalName = "RecordsRelated";
        private const string DATETIME_ISO_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private static readonly string CSDClassName_EN = "CSD Class";
        private static readonly string CSDClassName_DE = "KSU Klasse";
        private static readonly string CSDClassName_ES = "Clase CSD";
        private static readonly string CSDClassName_HU = "CSD Osztály";
        private static readonly string CSDClassName_PT = "Classe CSD";
        private static readonly Dictionary<string, string> CSDClassNameAndCultureMapping = new Dictionary<string, string>()
        {
            { "en-US", CSDClassName_EN },
            { "de-DE", CSDClassName_DE },
            { "es-ES", CSDClassName_ES },
            { "hu-HU", CSDClassName_HU },
            { "pt-PT", CSDClassName_PT }
        };

        public static AveObjectModelFactory factoryForAuto;
        // public static IJobMonitorService JobService { get; set; }
        private static CallLimiter _callLimiter;
        private string jobId;
        //public static AveDiscoveryOMFactory discoverFactoryForAuto;
        //protected static IReportService ReportService;//Add for config physical ,need rebuild next version.
        private BaseJobDto jobDto;
        //protected static IRMReportManager ReportManager
        //{
        //    get
        //    {
        //        return ReportMangerFactory.Instance.ReportManager;
        //    }
        //}
        private static int itemsPerTask = 200;
        private static IReportService<JMJobDetails> JobDetailService { get; set; }
        static SPSettingsUtility()
        {
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            //if (JobContext.Current.ReportManager != null)
            //{
            //    ReportService = JobContext.Current.ReportManager.Create();
            //}
            //JobService = PlatformWindsorManager.GetService(typeof(IJobMonitorService)) as IJobMonitorService;
            var numSetting = System.Configuration.ConfigurationManager.AppSettings["SPOApplySettingsItemsPerTask"];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out itemsPerTask);
            }
            var callLimitPerSecond = 5;
            var spoCallLimitPerSecond = System.Configuration.ConfigurationManager.AppSettings["SPOCallLimitPerSecond"];
            if (!string.IsNullOrEmpty(spoCallLimitPerSecond))
            {
                int.TryParse(spoCallLimitPerSecond, out callLimitPerSecond);
            }
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
        public static SettingResult ConfigBCSColumn(IAveSite site, RMSharePointOnPremiseSetting setting)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.ConfigBCSColumn4SiteCollection", addToStatistics: true))
            {
                logger.Info($"FullPath:[{setting.ScopeId}] IsUsingExistColumn:[{setting.IsUsingExistColumnName}] ExistingCoumnName:[{setting.ExistColumnName.LogBase64()}] Configure term settings in Records:[{setting.SetDocLevelTermForExistColumn}]");
                SettingResult result = SettingResult.None;
                if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    if (!CheckClassificationSetting(setting, site))
                    {
                        throw new Exception("Term Is Unavailable");
                    }
                    Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
                    IAveTaxonomyField siteField = null;
                    FieldConflict conflict = VerifyFieldConflict(site.RootWeb.Fields, setting, ref siteField);
                    logger.Debug($"config site column, conflict:{conflict}");
                    result = HandleSiteFieldConflict(conflict, site, setting, ref siteField, true);

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
                            IAveTaxonomyField taxField = siteField as IAveTaxonomyField;
                            var siteTextField = site.RootWeb.Fields.GetFieldById(taxField.TextField, false);
                            if (siteTextField != null)
                            {
                                if (siteTextField.Hidden)
                                {
                                    siteTextField.Hidden = false;
                                    siteTextField.Update();
                                }
                                siteTextField.Delete();
                                siteTextField.Update();
                            }
                            else
                            {
                                try
                                {
                                    logger.Warn("can't get taxonomy field's note field, will get note field by internal name [i0f84bba906045b4af568ee102a52dcb], url:{0}", site.ID);
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
                        logger.Warn("remove column, url:{0}; error: {1}", site.Url.LogBase64(), e.ToString());
                        result = SettingResult.Delete;
                    }
                }
                return result;
            }
        }

        private static bool NeedUpdateBCSColumn(IAveTaxonomyField taxField, RMSharePointOnPremiseSetting setting, Guid termStoreId, bool isSiteLevel, bool withoutCheckDefaultValue = false)
        {
            bool result = false;
            string columnName = setting.IsUsingExistColumnName ? setting.ExistColumnName : setting.ColumnName;
            if (!taxField.Title.Equals(columnName, StringComparison.OrdinalIgnoreCase))
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
                        //if (!JobContext.IsCSDTenant)
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
                    default:
                        break;
                }
            }
            //if (!taxField.DefaultValue.Contains(setting.DefaultTermId.ToString()))
            //{
            //    result = true;
            //}

            return result;
        }
        protected static bool CheckClassificationSetting(RMSharePointOnPremiseSetting setting, IAveSite site)
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
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.ValidateTermIds", addToStatistics: true))
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
                        //var rmTerm = TermDao.GetRMTermByGuId(termId);
                        //if (rmTerm == null || rmTerm.IsDeprecated || rmTerm.IsRemoved)
                        //{
                        //    result = false;
                        //}
                        //if (rmTerm.TermExpirationFrom != 0 || rmTerm.TermExpirationTo != 0)
                        //{
                        //    if (DateTime.UtcNow.Ticks < rmTerm.TermExpirationFrom || (rmTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > rmTerm.TermExpirationTo))
                        //    {
                        //        return false;
                        //    }
                        //}
                        var term = termStore.GetTerm(termId);
                        if (term == null || term.IsDeprecated)
                        {
                            result = false;
                        }
                    }
                    if (defaultTermId != null && defaultTermId != Guid.Empty)
                    {
                        //var defaultRmTerm = TermDao.GetRMTermByGuId(defaultTermId);
                        //if (defaultRmTerm == null || defaultRmTerm.IsDeprecated || defaultRmTerm.IsRemoved)
                        //{
                        //    result = false;
                        //}
                        //if (defaultRmTerm.TermExpirationFrom != 0 || defaultRmTerm.TermExpirationTo != 0)
                        //{
                        //    if (DateTime.UtcNow.Ticks < defaultRmTerm.TermExpirationFrom || (defaultRmTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > defaultRmTerm.TermExpirationTo))
                        //    {
                        //        return false;
                        //    }
                        //}
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
        public static SettingResult ConfigBCSColumn(IAveSite site, IAveList list, RMSharePointOnPremiseSetting setting, ref IAveTaxonomyField taxField)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.ConfigBCSColumn4List", addToStatistics: true))
            {
                logger.Info($"FullPath:[{setting.ScopeId}] IsUsingExistColumn:[{setting.IsUsingExistColumnName}] ExistingCoumnName:[{setting.ExistColumnName.LogBase64()}] Configure term settings in Records:[{setting.SetDocLevelTermForExistColumn}]");
                SettingResult result = SettingResult.SKip;
                if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    if (!CheckClassificationSetting(setting, site))
                    {
                        throw new Exception("Term Is Unavailable");
                    }
                    IAveTaxonomyField siteField = null;
                    Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
                    FieldConflict listConflict = VerifyFieldConflict(list.Fields, setting, ref taxField);

                    result = HandleListFieldConflict(listConflict, site, list, setting, siteField, ref taxField);
                    if (taxField != null)
                    {
                        if ((DeployTermMethod)setting.DeployTermMethod == DeployTermMethod.UseDefaultTerm &&
                       setting.DefaultTermId != null && setting.DefaultTermId != Guid.Empty && (!setting.IsUsingExistColumnName || (setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn)))
                        {
                            UpdateBCSColumnDefaultValue(list, setting, taxField);
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
                            listField = list.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault();
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
                                    logger.Warn("can't get taxonomy field's note field, will get note field by internal name [i0f84bba906045b4af568ee102a52dcb], url:{0}", list.Title.LogBase64());
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
                                    logger.Debug("remove folder property: {0}", folder?.UniqueId);
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
                                        if (folder.Properties.ContainsKey(listTextField.InternalName))
                                        {
                                            folder.Properties[listTextField.InternalName] = null;
                                            folder.Properties.Remove(listTextField.InternalName);
                                        }
                                        folder.Update();
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Warn("remove folder property, url:{0}; error: {1}", folder.Url.LogBase64(), e.ToString());
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn("remove list all folder property, url:{0}; error: {1}", list.RootFolder.Url.LogBase64(), e.ToString());
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
        private static void UpdateBCSColumnDefaultValue(IAveList list, RMSharePointOnPremiseSetting node, IAveTaxonomyField listTaxField)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.UpdateBCSColumnDefaultValue", addToStatistics: true))
            {
                string wssId = GetTermWssId(list.ParentWeb.Site, node.DefaultTermName, node.DefaultTermId);
                if (wssId == "-1")
                {
                    try
                    {
                        AveItemCreationInformation info = new AveItemCreationInformation()
                        {
                            UnderlyingObjectType = AveFileSystemObjectType.Folder,
                            FolderUrl = string.Concat("Temporary_Folder_For_WssId_Creation_", DateTime.Now.ToFileTime().ToString()),
                            LeafName = string.Concat("Temporary_Folder_For_WssId_Creation_", DateTime.Now.ToFileTime().ToString())     //使用item.SystemUpate必须赋值， 否则FileRef Not Found
                        };
                        var item = list.AddItem(info);
                        var term = list.ParentWeb.Site.AveSPTaxonomySession.GetTerm(node.DefaultTermId);

                        //listTaxField.SetFieldValue(item, term);//not implemented
                        #region 
                        //TODO yangyang 
                        IAveTaxonomyFieldValue taxValue = listTaxField.TaxonomyFieldValue;
                        taxValue.TermGuid = term.ID.ToString();
                        taxValue.Label = term.Name;
                        try
                        {
                            logger.Info("temp taxonomy value: {0}", taxValue.ToString().LogBase64());
                            item[listTaxField.ID] = taxValue;
                            item[listTaxField.TextField] = taxValue.ToString();
                            item.SystemUpdate();
                            //item.SystemUpdateForRecords();    //出现item not exists error

                        }
                        catch (Exception ex)
                        {
                            logger.Warn("UpdateBCSColumnDefaultValue failed {0}:{1} error {2}", list.Title.LogBase64(), term.Name.LogBase64(), ex.ToString());
                            try
                            {
                                logger.Info("Try with update4Records.");
                                item = list.AddItem(info);
                                item[listTaxField.ID] = taxValue;
                                item[listTaxField.TextField] = taxValue.ToString();
                                item.SystemUpdateForRecords();
                            }
                            catch (Exception ex1)
                            {
                                logger.Warn("Retry UpdateBCSColumnDefaultValue failed {0}:{1} error {2}", list.Title.LogBase64(), term.Name.LogBase64(), ex1.ToString());
                            }
                        }
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
                    logger.Info("Update column default value {0}", listTaxField.DefaultValue.LogBase64());
                    listTaxField.Update();
                }
                else
                {
                    listTaxField.DefaultValue = wssId + ";#" + node.DefaultTermName + "|" + node.DefaultTermId;
                    logger.Info("Update column default value {0}", listTaxField.DefaultValue.LogBase64());
                    listTaxField.Update();
                }
            }
        }
        private static string GetTermWssId(IAveSite site, string term, Guid termId)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.GetTermWssId", addToStatistics: true))
            {
                try
                {
                    string result = "-1";
                    IAveList taxonomyList = site.RootWeb.Lists.GetByTitle("TaxonomyHiddenList");
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
                        logger.Debug("Get temp Term ID: {0}, guid:{1}, name:{2}", temp.LogBase64(), termId, termItem["Title"]);
                        //todo 使用Id较小且不是-1的
                        if (temp != "-1" && IsS1LessThanS2(temp, result))
                        {
                            logger.Debug("New temp term ID:{0} is less than previous one {1}", temp.LogBase64(), result.LogBase64());
                            result = temp;
                        }
                        //return termItem["ID"].ToString();
                        //}
                    }
                    return result;
                }
                catch (Exception e1)
                {
                    logger.Debug(e1.Message);
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

        private static FieldConflict VerifyFieldConflict(IAveFieldCollection collection, RMSharePointOnPremiseSetting setting, ref IAveTaxonomyField taxField)
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
                if (tempField == null)
                {
                    conflict = FieldConflict.ColumnNotFound;
                }
                else
                {
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

        private static SettingResult HandleSiteFieldConflict(FieldConflict conflict, IAveSite site, RMSharePointOnPremiseSetting setting, ref IAveTaxonomyField siteField, bool needUpdate = false)
        {
            SettingResult result = SettingResult.None;
            Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
            switch (conflict)
            {
                case FieldConflict.ColumnNotFound:
                    if (!setting.IsUsingExistColumnName)
                    {
                        IAveField tempField = site.RootWeb.Fields.AddFieldAsXml("<Field Type='" + "TaxonomyFieldType" + "'   Name='" + XmlUtil.TransferSpecialCharactor(BCSColumnInternalName) + "' ID='" + BCSColumnID + "' DisplayName='" + XmlUtil.TransferSpecialCharactor(setting.ColumnName) + "'  ShowField='Term1033' StaticName='RevIMBCS' />", true, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes);
                        siteField = tempField as IAveTaxonomyField;
                        //if (JobContext.IsCSDTenant)
                        //{
                        //    InitTaxnomyField(siteField, setting, termStoreId, true, site, lcid: site.RootWeb.GetWorkingLanguage());
                        //}
                        //else
                        {
                            InitTaxnomyField(siteField, setting, termStoreId, true, site);
                        }
                        result = SettingResult.Add;
                    }
                    else
                    {
                        throw new Exception(I18NEntity.GetString("RM_SPS_CanNotFindExistingColumn"));
                    }

                    break;
                case FieldConflict.ColumnExisting:
                    if (needUpdate && NeedUpdateBCSColumn(siteField, setting, termStoreId, true))
                    {
                        //if (JobContext.IsCSDTenant)
                        //{
                        //    InitTaxnomyField(siteField, setting, termStoreId, true, site, lcid: site.RootWeb.GetWorkingLanguage());
                        //}
                        //else
                        {
                            InitTaxnomyField(siteField, setting, termStoreId, true, site);
                        }
                        result = SettingResult.Update;
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


        private static SettingResult HandleListFieldConflict(FieldConflict conflict, IAveSite site, IAveList list, RMSharePointOnPremiseSetting setting, IAveTaxonomyField siteField, ref IAveTaxonomyField listField)
        {
            logger.Debug($"config list:{list.Title.LogBase64()} bcs column, list conflict:{conflict}");
            SettingResult result = SettingResult.None;
            Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
            switch (conflict)
            {
                case FieldConflict.ColumnNotFound:
                    var gSetting = RMSPSettingUtil.LoadSharePointSetting(setting.SiteGroupId, Guid.Empty);
                    if (gSetting.IsUsingExistColumnName && !gSetting.SetDocLevelTermForExistColumn)
                    {
                        logger.Warn($"use existing column, skip to set doclevel setting,{setting.ScopeId}.");
                        result = SettingResult.SKip;
                        return result;
                    }
                    FieldConflict siteConflict = VerifyFieldConflict(site.RootWeb.Fields, setting, ref siteField);
                    logger.Debug($"config list:{list.Title.LogBase64()} bcs column, site conflict:{siteConflict}");

                    HandleSiteFieldConflict(siteConflict, site, gSetting, ref siteField);
                    if (siteField == null)
                    {
                        logger.Warn("siteField info is null");
                    }
                    IAveField tempListField;
                    //if (JobContext.IsCSDTenant)
                    //{
                    //    var listColumnSchemaXml = siteField.SchemaXml;
                    //    try
                    //    {
                    //        if (list.ParentWeb.GetWorkingLanguage() != site.RootWeb.GetWorkingLanguage())
                    //        {
                    //            XmlDocument xml = new XmlDocument();
                    //            xml.LoadXml(listColumnSchemaXml);
                    //            var fieldNode = xml.SelectSingleNode("Field");

                    //            var showFieldNode = fieldNode.Attributes.GetNamedItem("ShowField");
                    //            showFieldNode.Value = "Term" + list.ParentWeb.GetWorkingLanguage();

                    //            var displayNameNode = fieldNode.Attributes.GetNamedItem("DisplayName");
                    //            var currentWebCultureName = new CultureInfo(list.ParentWeb.GetWorkingLanguage()).Name;
                    //            var cultureClassName = string.Empty;
                    //            if (CSDClassNameAndCultureMapping.TryGetValue(currentWebCultureName, out cultureClassName))
                    //            {
                    //                displayNameNode.Value = cultureClassName;
                    //            }
                    //            listColumnSchemaXml = xml.OuterXml;
                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        logger.Warn("parse SchemaXml error: {0}", e.ToString());
                    //    }
                    //    tempListField = list.Fields.AddFieldAsXml(listColumnSchemaXml, true, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes);
                    //}
                    //else
                    {
                        tempListField = list.Fields.AddFieldAsXml(siteField.SchemaXml, true, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddToAllContentTypes);
                    }
                    listField = tempListField as IAveTaxonomyField;
                    //不使用folder setting 更新column
                    if (setting.ScopeId != setting.FolderId)
                    {
                        //if (JobContext.IsCSDTenant)
                        //{
                        //    InitTaxnomyField(listField, setting, termStoreId, false, lcid: list.ParentWeb.GetWorkingLanguage());
                        //    if (list.ParentWeb.GetWorkingLanguage() != site.RootWeb.GetWorkingLanguage())
                        //    {
                        //        RepeatUpdateTitle(list.ParentWeb.GetWorkingLanguage(), listField);
                        //    }
                        //    var view = list.Views.Where(v => v.DefaultView).First();
                        //    var moveToIndex = 0;
                        //    for (int i = 0; i < view.ViewFields.Count; i++)
                        //    {
                        //        if (view.ViewFields.ElementAt(i).Equals(CSDFieldName.DeletionDate))
                        //        {
                        //            moveToIndex = i;
                        //        }
                        //    }
                        //    view.ViewFields.MoveFieldTo(BCSColumnInternalName, moveToIndex);
                        //    view.Update();
                        //}
                        //else
                        {
                            InitTaxnomyField(listField, setting, termStoreId, false);
                        }
                    }
                    //暂时认为重复，可能引发SaveConflict 去掉
                    //listField.DefaultValue = siteField.DefaultValue;
                    //listField.Update();
                    result = SettingResult.Add;
                    break;
                case FieldConflict.ColumnExisting:
                    if (NeedUpdateBCSColumn(listField, setting, termStoreId, false, setting.ScopeId == setting.FolderId))
                    {
                        //if (JobContext.IsCSDTenant)
                        //{
                        //    InitTaxnomyField(listField, setting, termStoreId, false, lcid: list.ParentWeb.GetWorkingLanguage());
                        //    RepeatUpdateTitle(list.ParentWeb.GetWorkingLanguage(), listField);
                        //}
                        //else
                        {
                            InitTaxnomyField(listField, setting, termStoreId, false);
                        }
                        result = SettingResult.Update;
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
                taxField.TitleResource.SetVauleForUICulture(culInfo, CSDClassNameAndCultureMapping[culName]);
            }
            taxField.TitleResource.Update();
            taxField.Update();
        }

        private static void InitTaxnomyField(IAveTaxonomyField taxField, RMSharePointOnPremiseSetting setting, Guid termStoreId, bool isSiteLevel, object spObj = null, int lcid = -1)//TO DO Replace Setting from control...
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.InitTaxnomyField", addToStatistics: true))
            {
                var taxFieldId = taxField.ID;
                logger.Info("Init taxonomy field:{0}", taxField.ID);
                try
                {
                    //if (JobContext.IsCSDTenant && lcid > 0)
                    //{
                    //    taxField.Title = CSDClassName_EN;
                    //    foreach (var culName in CSDClassNameAndCultureMapping.Keys)
                    //    {
                    //        var culInfo = new CultureInfo(culName);
                    //        if (culInfo.LCID == lcid)
                    //        {
                    //            taxField.Title = CSDClassNameAndCultureMapping[culName];
                    //        }
                    //        taxField.TitleResource.SetValueForUICulture(culName, CSDClassNameAndCultureMapping[culName]);
                    //    }
                    //    taxField.TitleResource.Update();
                    //}
                    //else
                    {
                        taxField.Title = setting.IsUsingExistColumnName ? setting.ExistColumnName : setting.ColumnName;
                    }
                    //if (JobContext.IsCSDTenant && !setting.IsUsingExistColumnName)
                    //{
                    //    taxField.ReadOnlyField = true;
                    //    taxField.ShowInVersionHistory = true;
                    //}
                    taxField.SspId = termStoreId;
                    taxField.EnforceUniqueValues = false;
                    taxField.AllowMultipleValues = false;
                    taxField.TermSetId = setting.TermSetId;
                    taxField.Indexed = true;
                    if (!setting.IsUsingExistColumnName)
                    {
                        taxField.Required = setting.ColumnRequired == null ? true : (bool)setting.ColumnRequired;
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
                    switch ((DeployTermMethod)setting.DeployTermMethod)
                    {
                        case DeployTermMethod.UseDefaultTerm:
                            if (setting.DefaultTermId != null && setting.DefaultTermId != Guid.Empty)
                            {
                                //if (isSiteLevel)
                                //{
                                taxField.DefaultValue = "-1" + ";#" + setting.DefaultTermName + "|" + setting.DefaultTermId;
                                logger.Info("Update default column value {0}", taxField.DefaultValue.LogBase64());
                                //} 
                            }
                            break;
                        case DeployTermMethod.UseAutoClassification:
                            // if (!JobContext.IsCSDTenant)
                            {
                                taxField.DefaultValue = string.Empty;
                            }
                            break;
                        case DeployTermMethod.NoDefaultTerm:
                            taxField.DefaultValue = string.Empty;
                            break;
                        default:
                            break;
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
                            logger.Info("Retry update list column logic {0}:{1}", listObj.Title.LogBase64(), e.ToString());
                            if (!setting.IsUsingExistColumnName)
                            {
                                reloadField = listObj.Fields.GetById(taxFieldId) as IAveTaxonomyField;
                            }
                            else
                            {
                                reloadField = listObj.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault() as IAveTaxonomyField;
                            }
                            InitTaxnomyField(reloadField, setting, termStoreId, false, lcid: lcid);
                        }
                        else if (siteObj != null)
                        {
                            logger.Info("Retry update site column logic {0}:{1}", siteObj.Url.LogBase64(), e.ToString());
                            if (!setting.IsUsingExistColumnName)
                            {
                                reloadField = siteObj.RootWeb.Fields.GetById(taxFieldId) as IAveTaxonomyField;
                            }
                            else
                            {
                                reloadField = siteObj.RootWeb.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault() as IAveTaxonomyField;
                            }
                            InitTaxnomyField(reloadField, setting, termStoreId, false, lcid: lcid);
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
            }
        }

        //private string GetListColumnDefaultValue();

        public static bool ApplyExistItems(Guid remoteSiteId, IAveList list, IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointOnPremiseSetting setting, IAveORecords records)
        {
            logger.Info($"ApplyExistItems Start. unique Id:[{folder?.UniqueId}]");
            bool hasError = false;
            var aveTerm = list.ParentWeb.Site.AveSPTaxonomySession.GetTerm(setting.DefaultTermId);
            if (aveTerm == null)
            {
                throw new Exception("RM_SS_ConfigureColumnFailed");
            }
            List<string> excludePath = RMSPSettingUtil.GetFolderSettingUnderList(list.ID, remoteSiteId).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
            excludePath = excludePath.Where(p => p.StartsWith(folder.ServerRelativeUrl) && p != folder.ServerRelativeUrl).ToList();

            int rowLimit = GetMaxItemsPerThrottledOperation(list.ParentWeb.Site);
            int startIdx = 0;
            int maxItemId = GetLastItemId(list, list.RootFolder);   //取List下的最大Id， 否则SubFolder Items超过5000, 一样会Exceed Threshold
            logger.Info("max item id in list {0}", maxItemId);
            AveCamlQuery query = GetApplyExistingQuery(setting, aveTaxField.InternalName, folder, startIdx, rowLimit);
            IAveListItemCollection items = null;
            bool isOverWrite = setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite;

            bool needQueryNext = false;
            do
            {
                items = list.GetItemsForRecords(query);
                //ReportManager.IncreaseBase(items.Count);
                logger.Info($"Existing job process list url {list.Title.LogBase64()} item count:[{items.Count}]");
                bool hasFailedItem = SetValue(items, aveTaxField, aveTerm, records, setting, excludePath);
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
                    query.ViewXml = GetApplyExistingQueryXml(isOverWrite, aveTaxField.InternalName, rowLimit, startIdx, startIdx + rowLimit);
                    //}
                }
            }
            while (needQueryNext);
            logger.Info($"ApplyExistItems Complete. unique Id:[{folder?.UniqueId}]");
            return hasError;
        }
        protected static int GetListItemMaxId(IAveFolder folder)
        {
            AveCamlQuery query = new AveCamlQuery();

            query.ViewXml = "<View Scope='RecursiveAll'><Query><OrderBy><FieldRef Ascending='FALSE' Name='ID' /></OrderBy></Query><RowLimit>1</RowLimit></View>";

            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            var items = folder.ParentList.GetItemsForRecords(query);
            if (items.Count <= 0) return 0;
            int maxId = items[0].ID;
            return maxId;
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
            logger.Info($"GetLastItemQueryXml:{result.LogBase64()}");
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
            logger.Info($"GetLastFileQueryXml:{result.LogBase64()}");
            return result;
        }

        public static int InnerGetLastItemId(IAveList list, IAveFolder folder, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            //query.LoadAllItems = false;
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

        private static string GetApplyExistingQueryXml(bool isOverwrite, string columnInternalName, int rowLimit,
            int startIdx = 0, int endIdx = 0)
        {
            string queryXml = string.Empty;
            if (isOverwrite)
            {
                //queryXml= $"<View Scope=\"RecursiveAll\"><RowLimit>{rowLimit}</RowLimit></View>";
                queryXml = $@"
                <View Scope='Recursive'>
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
                <View Scope='Recursive'>
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
            logger.Info($"ApplyExisting query xml: {queryXml.LogBase64()}");
            return queryXml;
        }
        private static AveCamlQuery GetApplyExistingQuery(RMSharePointOnPremiseSetting setting, string columnInternalName, IAveFolder folder,
            int startIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            try
            {
                //query.LoadAllItems = false;
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                query.DatesInUtc = true;
                bool isOverwrite = setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite;
                query.ViewXml = GetApplyExistingQueryXml(isOverwrite, columnInternalName, rowLimit, startIndex, startIndex + rowLimit);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while GetQueryXml,ERROR:{0}", ex.ToString());
            }
            return query;
        }

        private static IAveTaxonomyFieldValue GetTaxValue(IAveTaxonomyField aveTaxField, IAveTerm aveTerm)
        {
            IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;
            taxValue.TermGuid = aveTerm.ID.ToString();
            taxValue.Label = aveTerm.Name;
            return taxValue;
        }




        //public static void ApplyExistItems(IAveList list, IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointOnPremiseSetting setting, IAveORecords records)//TO DO Debug
        //{
        //    using (new AgentPerformanceScope($"RMSPSettingUtility.ApplyExistItems.Folder.{folder.ServerRelativeUrl}"))
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

        private static bool SetValue(IAveListItemCollection items, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, IAveORecords records, RMSharePointOnPremiseSetting setting, List<string> excludePath = null, bool needChedkFileSystemObjectType = false)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.SetValue", $"RMSPSettingUtility.SetValue.{items.Count}", true))
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
                        hasError = RunMultiThreadsSetValue(items, itemsPerTask, cts, taxValue, aveTaxField, aveTerm, records, setting, excludePath, needChedkFileSystemObjectType);
                        return hasError;
                    }
                    foreach (var item in items)
                    {
                        bool isFailed = SetOneItemValue(item, taxValue, aveTaxField, aveTerm, records, setting, excludePath, needChedkFileSystemObjectType);
                        if (!hasError && isFailed)
                        {
                            hasError = true;
                        }
                    }
                }
                return hasError;
            }
        }

        private static bool RunMultiThreadsSetValue(IAveListItemCollection items, int itemsPerTask, CancellationTokenSource cts, IAveTaxonomyFieldValue taxValue, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, IAveORecords records, RMSharePointOnPremiseSetting setting, List<string> excludePath = null, bool needChedkFileSystemObjectType = false)
        {
            bool hasError = false;
            AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
            {
                bool isFailed = SetOneItemValue(item, taxValue, aveTaxField, aveTerm, records, setting, excludePath, needChedkFileSystemObjectType);
                if (!hasError && isFailed)
                {
                    hasError = true;
                }
            });
            return hasError;
        }

        private static bool NeedSkip(IAveListItem item, List<string> excludePaths)
        {
            if (excludePaths != null)
            {
                string itemPath = item["FileRef"].ToString();
                foreach (var excludePath in excludePaths)
                {
                    if (itemPath.StartsWith(excludePath) && itemPath != excludePath)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //private static DateTime CalculateDeletionDate(DateTime curTime, RetentionSetting rs)
        //{
        //    switch (rs.Unit)
        //    {
        //        case PeriodUnit.Days:
        //            return curTime.AddDays(rs.Value);
        //        case PeriodUnit.Months:
        //            return curTime.AddMonths(rs.Value);
        //        case PeriodUnit.Years:
        //            return curTime.AddYears(rs.Value);
        //        default:
        //            throw new Exception("The unit in RetentionSetting is wrong.");
        //    }
        //}

        //private static string SerializeExtendsData(ExtendsData data)
        //{
        //    return JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        //}

        //private static void SetCSDSettings(IAveListItem item, Guid termId, Dictionary<Guid, CSDRuleObject> csdRules, bool isUpdateLockedItem)
        //{
        //    if (item.ContentType.ID.IsChildOf(new AvePoint.ObjectModel.Common.AveContentTypeId(AveBuiltInContentTypeId.Folder)))
        //    {
        //        logger.Info($"skip folder or document set, item Url: {item.Url}");
        //        return;
        //    }

        //    if (!csdRules.ContainsKey(termId))
        //    {
        //        logger.Info($"There is no csd rule related to the term. TermId:[{termId.ToString()}]");
        //        return;
        //    }
        //    RetentionSetting rs = csdRules[termId].CreationRetentionSetting;
        //    //计算DeletionDate，
        //    var createdTime = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.Created]).Ticks, DateTimeKind.Utc);
        //    var deletionDate = CalculateDeletionDate(createdTime, rs);
        //    item[CSDFieldName.DeletionDate] = deletionDate;
        //    //Clear EventDate、EventComment，Extention
        //    if (item.Fields.ContainsField(CSDFieldName.EventDate))
        //    {
        //        //For OneDrive
        //        item[CSDFieldName.EventDate] = null;
        //    }

        //    if (item.Fields.ContainsField(CSDFieldName.Comments))
        //    {
        //        //For OneDrive
        //        item[CSDFieldName.Comments] = null;
        //    }
        //    //item[CSDFieldName.Extends] = null;

        //    if (item[CSDFieldName.Extends] != null && !string.IsNullOrEmpty(item[CSDFieldName.Extends].ToString()))
        //    {
        //        var extendsData = JsonConvert.DeserializeObject<ExtendsData>(item[CSDFieldName.Extends].ToString());
        //        extendsData.KSUClass = termId.ToString();
        //        extendsData.Reclassified = DateTime.UtcNow.ToString(DATETIME_ISO_FORMAT);
        //        extendsData.ReclassifiedBy = "Records";
        //        item[CSDFieldName.Extends] = SerializeExtendsData(extendsData);
        //    }
        //    bool userRetentionLabelForLockedDoc = isUpdateLockedItem ? !string.IsNullOrEmpty(csdRules[termId].RetentionLabelForLockedDoc) : false;
        //    var calculatedLabel = userRetentionLabelForLockedDoc ? csdRules[termId].RetentionLabelForLockedDoc : rs.RetentionLabel;
        //    WaitExecuteAction(() =>
        //    {
        //        item.SystemUpdateForRecords();
        //        item.SetComplianceTag(calculatedLabel, userRetentionLabelForLockedDoc, false, false, false);
        //    });
        //}

        private static bool SetOneItemValue(IAveListItem item, IAveTaxonomyFieldValue taxValue, IAveTaxonomyField aveTaxField, IAveTerm aveTerm, IAveORecords records, RMSharePointOnPremiseSetting setting, List<string> excludePath = null, bool needChedkFileSystemObjectType = false)
        {
            using (var scope0 = new AgentPerformanceScope("RMSPSettingUtility.SetOneItem", $"RMSPSettingUtility.SetOneItem.{item.ID}", true))
            {
                bool hasError = false;
                logger.Info("Set Item default value {0}", item.ID);
                try
                {
                    //ReportManager.Increase();
                    string itemFullUrl = item.ParentList.ParentWeb.Url + "/" + item.Url;
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

                    if (item.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info("Current item:{0} is folder, skip set classification.", item.UniqueId);
                        return hasError;
                    }
                    var isUpdateDeclared = false;
                    if (IsBlockEditAndDeleteRecord(item))
                    {
                        logger.Info("Item is Block Edit and delete {0}", item.UniqueId);
                        //*****ReportService.Commit(new SPSettingJobReportEntry(item.Name, item.Url, "",
                        //string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Skipped, "RM_SS_ItemBlockEditAndDelete"));
                        if (setting.IncludeDeclaredRecords)
                        {
                            isUpdateDeclared = true;
                        }
                        else
                        {
                            return hasError;
                        }

                    }

                    if (needChedkFileSystemObjectType && item.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info("skip item is Folder, itemId: {0}", item.ID);
                        return hasError;
                    }
                    //if (JobContext.IsCSDTenant && isUpdateDeclared)
                    //{
                    //    //计算DeletionDate，
                    //    var createdTime = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.Created]).Ticks, DateTimeKind.Utc);
                    //    var calculatedDeletionDate = CalculateDeletionDate(createdTime, configSiteSetting.CSDRules[aveTerm.ID].CreationRetentionSetting);

                    //    var currentDeletionDate = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.DeletionDate]).Ticks, DateTimeKind.Utc);
                    //    if (DateTime.Compare(calculatedDeletionDate, currentDeletionDate) <= 0)
                    //    {
                    //        logger.Info($"CalculatedDeletionDate is small than CurrentDeletionDate. CalculatedDeletionDate:[{calculatedDeletionDate}] CurrentDeletionDate:[{currentDeletionDate}]");
                    //        SendSPSettingReport(item.Name, itemFullUrl, "RM_SS_ApplyExist",
                    //            JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_DeletionDateIsEarly");
                    //        return hasError;
                    //    }
                    //}
                    //if (JobContext.IsCSDTenant && IsFileExtentionInExculdeList(configSiteSetting.ExcludeFileExtentions, item))
                    //{
                    //    if (item[aveTaxField.ID] == null || string.IsNullOrEmpty(item[aveTaxField.ID].ToString()))
                    //    {
                    //        SetExculdeListFileDefaultValue(item, aveTaxField, configSiteSetting);
                    //        SendSPSettingReport(item.Name, itemFullUrl, "RM_SS_ApplyExist",
                    //            JobDetailsStatus.Successful, "RM_JS_JMD_Comment_WhiteList_SetValue");
                    //    }
                    //    else
                    //    {
                    //        SendSPSettingReport(item.Name, itemFullUrl, "RM_SS_ApplyExist",
                    //            JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_WhiteList_Skip_ExistValue");
                    //    }
                    //    return hasError;
                    //}
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
                            logger.Error("undeclare item failed [{0}]:{1}", item?.Url.LogBase64(), e.ToString());
                            throw;
                        }
                    }
                    try
                    {
                        item[aveTaxField.ID] = taxValue;
                        item[aveTaxField.TextField] = taxValue.ToString();

                        WaitExecuteAction(() =>
                        {
                            item.SystemUpdateForRecords();
                        });
                        //if (JobContext.IsCSDTenant)
                        //{
                        //    SetCSDSettings(item, aveTerm.ID, configSiteSetting.CSDRules, isUpdateDeclared);
                        //    SendSPSettingReport(item.Name, itemFullUrl, "RM_SS_ApplyExist", JobDetailsStatus.Successful);
                        //}
                        logger.Info($"apply term to item success:{item?.ID}");
                    }
                    catch (Exception e)
                    {
                        logger.Error("update item failed [{0}]:{1}", item?.Url.LogBase64(), e.ToString());
                        throw;

                    }
                    try
                    {
                        if (isUpdateDeclared)
                        {
                            using (var scope = new AgentPerformanceScope("SetValue.DeclareItemAsRecord", addToStatistics: true))
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
                        logger.Warn("declare item failed: [{0}]:{1}", item?.Url.LogBase64(), e.ToString());
                        throw;
                    }

                }
                catch (Exception e)
                {
                    logger.Error("Set Item default value failed [{0}]:{1}", item?.Url.LogBase64(), e.ToString());
                    JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
                    detail.ObjectName = item?.Name;
                    detail.SourceURL = item?.Url;
                    detail.Action = "RM_SS_ApplyExist";
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = GetExceptionMessage(e);
                    detail.AgentName = OSInformation.HostName;
                    JobDetailService.Commit(detail);
                    //ReportManager.SendJobDetail(detail);
                    hasError = true;
                }
                return hasError;
            }
        }

        private static bool IsFileExtentionInExculdeList(List<string> excludeFileExtention, IAveListItem item)
        {
            var extention = item.Name.Substring(item.Name.LastIndexOf('.') + 1);
            if (excludeFileExtention.Contains(extention.ToLowerInvariant()))
            {
                return true;
            }
            return false;
        }

        private static bool CheckItemExistEventDate(IAveListItem item)
        {
            //var eventDate = DateTime.MinValue;
            //object objVal;
            //if (item.FieldValues.TryGetValue(CSDFieldName.EventDate, out objVal))
            //{
            //    var dt = objVal as DateTime?;
            //    if (dt != null)
            //    {
            //        eventDate = dt.Value;
            //        return true;
            //    }
            //}
            return false;
        }

        //private static void SetExculdeListFileDefaultValue(IAveListItem item, IAveTaxonomyField aveTaxField, ConfigSiteSetting configSiteSetting)
        //{
        //    var excludedFileTypeDefaultTerm = configSiteSetting.ExcludedFileTypeDefaultTerm;
        //    if (excludedFileTypeDefaultTerm != null)
        //    {
        //        logger.Info($"File extention is in ExcludeFileExtention, so set Item Value in Configuration settings. Name:[{item.Url}], Term:[{excludedFileTypeDefaultTerm.Name}]");
        //        IAveTaxonomyFieldValue whiteListItemTaxValue = aveTaxField.TaxonomyFieldValue;
        //        whiteListItemTaxValue.TermGuid = excludedFileTypeDefaultTerm.ID.ToString();
        //        whiteListItemTaxValue.Label = excludedFileTypeDefaultTerm.Name;
        //        item[aveTaxField.ID] = whiteListItemTaxValue;
        //        item[aveTaxField.TextField] = whiteListItemTaxValue.ToString();

        //        item[CSDFieldName.DeletionDate] = null;
        //        WaitExecuteAction(() =>
        //        {
        //            item.SystemUpdateForRecords();
        //        });
        //    }
        //}

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
                logger.Info("remove folder default value {0}", folderUrl.LogBase64());
            }
            catch (Exception e)
            {
                logger.Warn($"Remove Folder Default Value From '/forms/client_LocationBasedDefaults.html'. FolderPath:[{folderUrl.LogBase64()}] Error:{e.ToString()}");
            }
        }
        public static void RemoveFolderDefalutValue(IAveFolder folder, IAveList list, RMSharePointOnPremiseSetting setting)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.RemoveFolderDefaultValue", $"RMSPSettingUtility.RemoveFolderDefaultValue.{folder.Name}", true))
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
                            columnInternalName = list.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault().InternalName;
                        }
                        IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(list.ParentWeb.Site, columnInternalName);
                        var existFolderDefaultValue = string.Empty;
                        try
                        {
                            existFolderDefaultValue = mDefaults.GetFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl);
                            logger.Info("Get Field Default Value is empty, folder url:{0}", folder.ServerRelativeUrl.LogBase64());
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Get Field Default Value error, folder server relative url: {0}, ERROR:{1}", folder.ServerRelativeUrl.LogBase64(), ex.ToString());
                        }

                        if (!string.IsNullOrEmpty(existFolderDefaultValue))
                        {
                            mDefaults.RemoveFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl);
                            logger.Info("remove folder default value {0}", folder.ServerRelativeUrl.LogBase64());
                        }
                        else
                        {
                            logger.Info("No Need remove folder default value {0}", folder.ServerRelativeUrl.LogBase64());
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Remove Folder Default Value From '/forms/client_LocationBasedDefaults.html' error:{0}", e.ToString());
                }
            }
        }

        public static RMSharePointOnPremiseSetting LoadParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMSharePointOnPremiseSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.Farm)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = RMSPSettingUtil.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadParentSeting(node.Parent, siteId);
            }

            return SPSetting;
        }

        public static SettingResult ConfigBCSColumn(IAveSite site, IAveList list, IAveFolder folder, RMSharePointOnPremiseSetting setting, ref IAveTaxonomyField taxField)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingUtility.ConfigBCSColumn4Folder", $"RMSPSettingUtility.ConfigBCSColumn4Folder.{folder.Name}", true))
            {
                logger.Info($"FullPath:[{setting.ScopeId}] IsUsingExistColumn:[{setting.IsUsingExistColumnName}] ExistingCoumnName:[{setting.ExistColumnName.LogBase64()}] Configure term settings in Records:[{setting.SetDocLevelTermForExistColumn}]");
                SettingResult result = SettingResult.SKip;
                if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    if (!CheckClassificationSetting(setting, site))
                    {
                        throw new Exception("Term Is Unavailable");
                    }

                    IAveTaxonomyField siteField = null;
                    Guid termStoreId = site.AveSPTaxonomySession.TermStores[0].ID;
                    FieldConflict listConflict = VerifyFieldConflict(list.Fields, setting, ref taxField);

                    result = HandleListFieldConflict(listConflict, site, list, setting, siteField, ref taxField);
                    if (taxField == null)
                    {
                        return result;
                    }
                    //var columnSetting = GetParentColumnSetting(setting);
                    RMSPTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                    var columnSetting = LoadParentSeting(dbNodeInfo, setting.SiteId);
                    if (result == SettingResult.Add)   //ATSB FullJob重复Init List Column, 出现问题, 只在Add时更新一次
                    {
                        //if (JobContext.IsCSDTenant)
                        //{
                        //    InitTaxnomyField(taxField, columnSetting, termStoreId, false, lcid: list.ParentWeb.GetWorkingLanguage());
                        //}
                        //else
                        {
                            InitTaxnomyField(taxField, columnSetting, termStoreId, false);
                        }
                    }
                    if ((DeployTermMethod)columnSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm &&
                                columnSetting.DefaultTermId != null && columnSetting.DefaultTermId != Guid.Empty
                                && (!columnSetting.IsUsingExistColumnName || (columnSetting.IsUsingExistColumnName && columnSetting.SetDocLevelTermForExistColumn)))
                    {
                        if (taxField.DefaultValue == null || taxField.DefaultValue.StartsWith("-1"))
                        {
                            logger.Info("Folder need to update parent list column default value {0}", taxField.DefaultValue.LogBase64());
                            UpdateBCSColumnDefaultValue(list, columnSetting, taxField);
                        }
                        else
                        {
                            logger.Info("Parent list default value:{0}", taxField.DefaultValue.LogBase64());
                        }
                    }

                    //}

                    //folder
                    if (setting.DefaultTermId == Guid.Empty)
                    {
                        try
                        {
                            var defaultValues = GetXmlWithFolderDefaultValue(list);
                            if (!string.IsNullOrEmpty(defaultValues))
                            {
                                logger.Info("'/forms/client_LocationBasedDefaults.html' is  exist.");
                                IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(site, taxField.InternalName);
                                mDefaults.RemoveFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl);

                            }
                            result = SettingResult.Update;
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
                            logger.Warn("Get Field Default Value error, do add logic,id: {0}", setting?.Id);
                        }

                        string wssId = GetTermWssId(site, setting.DefaultTermName, setting.DefaultTermId);
                        if (wssId == "-1")
                        {
                            logger.Info("Term id {0}, name {1}, never used in this site", setting.DefaultTermId, setting.DefaultTermName.LogBase64());
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
                                    logger.Info("temp taxonomy value: {0}", taxValue.ToString().LogBase64());
                                    item[taxField.ID] = taxValue;
                                    item[taxField.TextField] = taxValue.ToString();
                                    item.SystemUpdate();
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn("UpdateBCSColumnDefaultValue failed {0}:{1} error {2}", list.Title.LogBase64(), term.Name.LogBase64(), ex.ToString());
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
                            logger.Info("Folder default value does no change, {0}", folderDefaultValue.LogBase64());
                            result = SettingResult.SKip;
                            //this.AddDetailToList(setting.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_SkipFolderColumn"), JobDetailsStatus.Skipped, null);
                        }
                        else
                        {
                            IAveOMetadataDefaults mDefaults = factoryForAuto.CreateMetadataDefaults(site, taxField.InternalName);
                            mDefaults.SetFieldDefault(list.ParentWeb.ServerRelativeUrl, list.Title, list.ID, folder.ServerRelativeUrl, folderDefaultValue);

                            result = SettingResult.Add;
                            if (string.IsNullOrEmpty(existFolderDefaultValue))
                            {
                                logger.Info("Add folder column success,folder unique id:{0}", folder?.UniqueId);
                                //this.AddDetailToList(folder.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddFolderColumn"), JobDetailsStatus.Successful, null);
                            }
                            else
                            {
                                logger.Info("update folder column success,folder unique id:{0}", folder?.UniqueId);
                                //this.AddDetailToList(folder.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateFolderColumn"), JobDetailsStatus.Successful, null);
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

        private static RMSharePointOnPremiseSetting GetParentColumnSetting(RMSharePointOnPremiseSetting setting)
        {
            RMSharePointOnPremiseSetting result = null;
            if (setting.ScopeId != setting.FolderId)
            {
                result = setting;
            }
            else
            {
                result = RMSPSettingUtil.LoadSharePointSetting(setting.ListId, setting.SiteId);
                if (result == null)
                {
                    result = RMSPSettingUtil.LoadSharePointSetting(setting.SiteId, setting.SiteId);
                }
                if (result == null)
                {
                    result = RMSPSettingUtil.LoadSharePointSetting(setting.SiteGroupId, Guid.Empty);
                }

            }
            return result;
        }
        public static IAveTaxonomyField GetTaxonomyField(IAveList list, RMSharePointOnPremiseSetting setting)
        {
            IAveField listField;
            if (setting.IsUsingExistColumnName)
            {
                listField = list.Fields.Where(f => f.Title == setting.ExistColumnName).FirstOrDefault();
            }
            else
            {
                listField = list.Fields.GetFieldById(BCSColumnID, false);
            }
            var taxField = listField as IAveTaxonomyField;
            return taxField;
        }
        private static void UpdateDefaultFolderValues(IAveList list, string defaultValues)
        {
            IAveFolder formsFolder = list.GetFolder(list.RootFolder.ServerRelativeUrl + "/forms");
            var fci = new AveFileCreationInformation();
            fci.Content = Encoding.UTF8.GetBytes(defaultValues);
            fci.Url = "client_LocationBasedDefaults.html";
            fci.Overwrite = true;
            var metaDataFile = formsFolder.Files.Add(fci);
            formsFolder.Update();
            list.Update();
        }
        private static XmlNode SelectSingleFieldDefaultNode(XmlDocument defaultsXml, string folderPath, string fieldName)
        {
            return defaultsXml.DocumentElement.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "/MetadataDefaults/a[@href='{0}']/DefaultValue[@FieldName='{1}']", new object[]
            {
        Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false),
        fieldName
            }));
        }

        //private bool ParentDisableDocSetting(RMSPTreeNode node)
        //{
        //    bool parentDisable = false;
        //    if (node == null)
        //    {
        //        return parentDisable;
        //    }
        //    if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List)
        //    {
        //        var scopeId = new Guid(node.SPObjectId);
        //        if (SPColumnCacheSetting.Instance.DocClassificationSetting.ContainsKey(scopeId))
        //        {
        //            parentDisable = true;
        //        }

        //    }
        //    if (!parentDisable && node.Parent != null)
        //    {
        //        parentDisable = ParentDisableDocSetting(node.Parent);
        //    }
        //    return parentDisable;
        //}
        #endregion
        #endregion

        #region Auto-Classification
        private static AveCamlQuery GetAutoClassificationQuery(IAveList list, IAveFolder folder, RMSharePointOnPremiseSetting setting, DateTime startTime, DateTime endTime, string columnInternalName, int startIndex, int endIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            try
            {
                // query.LoadAllItems = false;
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                string queryStr = string.Empty;

                CAMLManager cm = new CAMLManager(Types.ScopeTypes.Recursive);
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
                logger.Info($"Process Folder {folder?.UniqueId}, startTime:{startTime}, endTime:{endTime} query xml {queryXml.LogBase64()}");
                logger.Info("Query XML:{0}", query.ViewXml.LogBase64());
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
        public static void Autoclassification(Guid remoteSiteId, IAveList list, IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointOnPremiseSetting setting, DateTime startTime, DateTime endTime, IAveORecords records, ref bool hasError)
        {
            logger.Info($"Start to process auto classification. unique Id:[{folder?.UniqueId}]");
            List<string> excludePath = RMSPSettingUtil.GetFolderSettingUnderList(list.ID, remoteSiteId).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
            excludePath = excludePath.Where(p => p.StartsWith(folder.ServerRelativeUrl) && p != folder.ServerRelativeUrl).ToList();

            List<AvePoint.RA.Contract.Global.Object.ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<AvePoint.RA.Contract.Global.Object.ClassificationRule>>(setting.AutoClassificationRules);
            Dictionary<Guid, IAveTerm> aveTerms = GetAveTerms(list, autoRules);
            Dictionary<string, Guid> ruleTermIdMapping = new Dictionary<string, Guid>();
            AvePoint.GCommon.Contract.StorageOptimization.Object.RuleCollection ruleCollection = GetRuleCollection(autoRules, ref ruleTermIdMapping);
            RuleManagement ruleManagement = new RuleManagement(ruleCollection);
            bool needQueryNext = false;
            int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            int maxItemId = GetListItemMaxId(list.RootFolder);

            int startIndex = 0;
            IAveListItemCollection items = null;
            do
            {
                AveCamlQuery query = GetAutoClassificationQuery(list, folder, setting, startTime, endTime, aveTaxField.InternalName, startIndex, startIndex + rowLimit, rowLimit);
                items = list.GetItemsForRecords(query);
                //ReportManager.IncreaseBase(items.Count);
                logger.Info($"AutoJob process folder unique Id {folder?.UniqueId} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");
                foreach (var item in items)
                {
                    if (item.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info("Current item:{0} is folder, skip set classification.", item.UniqueId);
                        continue;
                    }
                    //ReportManager.Increase();
                    if (!NeedSkip(item, excludePath))
                    {
                        AutoSetOneItemTerm(item, list, aveTaxField, records, setting.IncludeDeclaredRecords, ruleManagement, ruleTermIdMapping, aveTerms, ref hasError);
                    }
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
            logger.Info($"Finish to process auto classification. unique Id:[{folder?.UniqueId}]");
        }

        //public void AutoSetValues(IAveListItemCollection items, )
        //{
        //    if (items.Count > itemsPerTask)
        //    {
        //        logger.Info("Use multi thread.");
        //        var cts = new CancellationTokenSource();
        //        RunMultiThreadsSetValue(items, itemsPerTask, cts, taxValue, aveTaxField, aveTerm, records, setting, excludePath, needChedkFileSystemObjectType);
        //        return;
        //    }
        //    else
        //    {
        //        foreach (var item in items)
        //        {
        //            SetOneItemValue(item, taxValue, aveTaxField, aveTerm, records, setting, excludePath, needChedkFileSystemObjectType);
        //        }
        //    }

        //}

        //public static void SetWhiteFileTerm(IAveListItem item, IAveList list, IAveORecords records, IAveTaxonomyField aveTaxField, bool isBlockEditAndDelete, ConfigSiteSetting configSiteSetting)
        //{
        //if (isBlockEditAndDelete)
        //{
        //    try
        //    {
        //        records.UndeclareItemAsRecord(item);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn("undeclare item failed {0}:{1}", item.Url, e.ToString());
        //    }
        //}
        //SetExculdeListFileDefaultValue(item, aveTaxField, configSiteSetting);
        //if (isBlockEditAndDelete)
        //{
        //    using (AgentPerformanceScope scope = new AgentPerformanceScope("AutoSetTerm.DeclareItemAsRecord"))
        //    {
        //        var dItem = list.GetItemById(item.ID);
        //        records.DeclareItemAsRecord(dItem);
        //    }
        //}
        // }

        //public static void SetCSDSettingForAuto(IAveListItem item, IAveList list, IAveORecords records, Guid termId, ConfigSiteSetting configSiteSetting, bool isUpdateLockedItem)
        //{
        //if (JobContext.IsCSDTenant)
        //{
        //    if (isUpdateLockedItem)
        //    {
        //        try
        //        {
        //            records.UndeclareItemAsRecord(item);
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Warn("undeclare item failed {0}:{1}", item.Url, e.ToString());
        //        }
        //    }
        //    SetCSDSettings(item, termId, configSiteSetting.CSDRules, isUpdateLockedItem);
        //    if (isUpdateLockedItem)
        //    {
        //        using (AgentPerformanceScope scope = new AgentPerformanceScope("AutoSetTerm.DeclareItemAsRecord"))
        //        {
        //            var dItem = list.GetItemById(item.ID);
        //            records.DeclareItemAsRecord(dItem);
        //        }
        //    }
        //}
        //}
        private static void SendSPSettingReport(string name, string url, string action, JobDetailsStatus status, string comment = "")
        {
            JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
            detail.ObjectName = name;
            detail.SourceURL = url;
            detail.Action = action;
            detail.Status = status;
            detail.Comment = comment;
            detail.AgentName = OSInformation.HostName;
            //ReportManager.SendJobDetail(detail);
        }
        public static void AutoSetOneItemTerm(IAveListItem item, IAveList list, IAveTaxonomyField aveTaxField, IAveORecords records,
            bool includeDeclaredRecords, RuleManagement ruleManagement, Dictionary<string, Guid> ruleTermIdMapping,
            Dictionary<Guid, IAveTerm> aveTerms, ref bool hasError)
        {
            var isUpdateDeclared = false;
            string itemFullUrl = list.ParentWeb.Url + "/" + item.Url;
            try
            {
                //这个判断会导致效率问题。。。。改为在CamlQuery中使用Recursive 不是RecursiveAll
                //if (item.File == null)
                //{
                //    //只处理Document，其他skip
                //    continue;
                //}
                if (IsBlockEditAndDeleteRecord(item))
                {
                    logger.Info("Item is Block Edit and delete {0}", item.ID);
                    if (includeDeclaredRecords)
                    {
                        isUpdateDeclared = true;
                    }
                    else
                    {
                        return;
                    }
                }
                AvePoint.GCommon.Contract.StorageOptimization.Object.Rule soRule = ruleManagement.CheckItemCriteria(item.UniqueId, item);
                Guid termId = soRule == null ? ruleTermIdMapping[Guid.Empty.ToString()] : ruleTermIdMapping[soRule.Id];
                IAveTaxonomyFieldValue taxValue = aveTaxField.TaxonomyFieldValue;
                if (!termId.Equals(Guid.Empty))
                {
                    //if (JobContext.IsCSDTenant && isUpdateDeclared)
                    //{
                    //    //计算DeletionDate，
                    //    var createdTime = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.Created]).Ticks, DateTimeKind.Utc);
                    //    var calculatedDeletionDate = CalculateDeletionDate(createdTime, configSiteSetting.CSDRules[termId].CreationRetentionSetting);

                    //    var currentDeletionDate = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.DeletionDate]).Ticks, DateTimeKind.Utc);
                    //    if (DateTime.Compare(calculatedDeletionDate, currentDeletionDate) <= 0)
                    //    {
                    //        logger.Info($"CalculatedDeletionDate is small than CurrentDeletionDate. CalculatedDeletionDate:[{calculatedDeletionDate}] CurrentDeletionDate:[{currentDeletionDate}]");
                    //        SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification",
                    //            JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_DeletionDateIsEarly");
                    //        return;
                    //    }
                    //}
                    //if (JobContext.IsCSDTenant && IsFileExtentionInExculdeList(configSiteSetting.ExcludeFileExtentions, item))
                    //{
                    //    if (item[aveTaxField.ID] == null || string.IsNullOrEmpty(item[aveTaxField.ID].ToString()))
                    //    {
                    //        SetWhiteFileTerm(item, list, records, aveTaxField, isUpdateDeclared, configSiteSetting);
                    //        SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification",
                    //            JobDetailsStatus.Successful, "RM_JS_JMD_Comment_WhiteList_SetValue");
                    //    }
                    //    else
                    //    {
                    //        SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification",
                    //            JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_WhiteList_Skip_ExistValue");
                    //    }
                    //    return;
                    //}

                    string oldValue = item[aveTaxField.InternalName] == null ? null : ((string)item[aveTaxField.InternalName]).ToLowerInvariant();
                    if (string.IsNullOrEmpty(oldValue) || !oldValue.Contains(termId.ToString().ToLowerInvariant()))
                    {
                        IAveTerm aveTerm = aveTerms[termId];
                        taxValue.TermGuid = aveTerm.ID.ToString();
                        taxValue.Label = aveTerm.Name;
                        logger.Info("Set Item classification value for autoclassification {0}", item.ID);
                        if (isUpdateDeclared)
                        {
                            try
                            {
                                records.UndeclareItemAsRecord(item);
                            }
                            catch (Exception e)
                            {
                                logger.Warn("undeclare item failed {0}:{1}", item.Url.LogBase64(), e.ToString());
                            }
                        }
                        try
                        {
                            item[aveTaxField.ID] = taxValue;
                            item[aveTaxField.TextField] = taxValue.ToString();
                            //item.SystemUpdate();
                            item.SystemUpdateForRecords();//*********
                            //if (JobContext.IsCSDTenant)
                            //{
                            //    SetCSDSettings(item, termId, configSiteSetting.CSDRules, isUpdateDeclared);
                            //    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Successful);
                            //}
                        }
                        catch (Exception e)
                        {
                            logger.Warn("update item failed {0}:{1}", item.Url.LogBase64(), e.ToString());
                            hasError = true;
                            var expMsg = GetExceptionMessage(e);
                            //if (JobContext.IsCSDTenant)
                            //{
                            //    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Failed, expMsg);
                            //}
                        }
                        if (isUpdateDeclared)
                        {
                            using (var scope = new AgentPerformanceScope("AutoSetTerm.DeclareItemAsRecord", addToStatistics: true))
                            {
                                var dItem = list.GetItemById(item.ID);
                                records.DeclareItemAsRecord(dItem);
                            }
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
            catch (Exception e)
            {
                hasError = true;
                logger.Error("Set Item classification value failed {0}:{1}", item.Url.LogBase64(), e.ToString());
                var expMsg = GetExceptionMessage(e);
                //if (JobContext.IsCSDTenant)
                //{
                //    SendSPSettingReport(item.Name, itemFullUrl, "RM_JS_JMD_Action_SetAutoClassification", JobDetailsStatus.Failed, expMsg);
                //}
            }
        }

        private static string GetExceptionMessage(Exception e)
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
        #region old logic
        //public static void Autoclassification(IAveList list, IAveTaxonomyField aveTaxField, RMSharePointOnPremiseSetting setting, DateTime startTime, DateTime endTime, IAveORecords records, ref bool hasError)
        //{
        //    #region old logic
        //    //QueryAutoClassificationItems(list, setting, startTime, endTime, aveTaxField, ref hasError);
        //    #endregion
        //    IAveListItemCollection items = null;
        //    using (new RA.Common.AgentPerformanceScope(string.Format("Autoclassification Query Items. List Url:[{0}]", list.RootFolder.Url)))
        //    {
        //        items = GetItemsWithCamlQuery(list, setting, startTime, endTime, aveTaxField.InternalName);
        //        logger.Info("Autoclassification Query Items Count: {0}", items == null ? 0 : items.Count);
        //    }

        //    using (new RA.Common.AgentPerformanceScope(string.Format("Autoclassification Set Value. List Url:[{0}]", list.RootFolder.Url)))
        //    {
        //        AutoSetTerm(items, list, aveTaxField, setting.AutoClassificationRules, records, setting.IncludeDeclaredRecords, ref hasError);
        //    }
        //}
        //private static IAveListItemCollection GetItemsWithCamlQuery(IAveList list, RMSharePointOnPremiseSetting setting, DateTime startTime, DateTime endTime, string columnInternalName)
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
        public static Dictionary<Guid, IAveTerm> GetAveTerms(IAveList list, List<AvePoint.RA.Contract.Global.Object.ClassificationRule> autoRules)
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
            return aveTerms;
        }
        public static AvePoint.GCommon.Contract.StorageOptimization.Object.RuleCollection GetRuleCollection(List<AvePoint.RA.Contract.Global.Object.ClassificationRule> autoRules, ref Dictionary<string, Guid> termRuleMapping)
        {
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>();
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters;
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
                    soFilters = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy>();
                    int sequenceNo = 0;
                    ConvertToSOFilters(autoRule.FilterGroups, ref sequenceNo, ref soFilters);
                    List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> filerPolicies = ConvertSOFiletrPolicyToFilterPolicy(soFilters);
                    string andOrExpressionStr = GetGroupsAndOrExpression(autoRule.FilterGroups, ArchiverFilterCombineMode.And);
                    logger.Info("AndOr Expression:{0}", andOrExpressionStr.LogBase64());
                    AvePoint.GCommon.Contract.StorageOptimization.Object.Rule soRule = ConvertToSORule(autoRule, soFilters, filerPolicies, andOrExpressionStr);
                    rules.Add(soRule);

                    termRuleMapping.Add(soRule.Id, new Guid(autoRule.TermId));
                }
            }

            AvePoint.GCommon.Contract.StorageOptimization.Object.RuleCollection ruleCol = new AvePoint.GCommon.Contract.StorageOptimization.Object.RuleCollection() { Rules = new Dictionary<int, AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>() };
            for (int i = 0; i < rules.Count; i++)
            {
                ruleCol.Rules.Add(i, rules[i]);
            }
            return ruleCol;
        }
        public static string GetGroupAndOrExpression(AvePoint.RA.Contract.Global.Object.FilterGroup filterGroup)
        {
            string groupAndOrExpression = string.Empty;

            string filtersExpression = GetFiltersAndOrExpression(filterGroup.Filters);
            groupAndOrExpression = filtersExpression;

            if (filterGroup.FilterGroups != null && filterGroup.FilterGroups.Count > 0)
            {
                string groupsResult = GetGroupsAndOrExpression(filterGroup.FilterGroups, (ArchiverFilterCombineMode)filterGroup.CombineMode);
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
        public static string GetGroupsAndOrExpression(List<AvePoint.RA.Contract.Global.Object.FilterGroup> filterGroups, ArchiverFilterCombineMode combineMode)
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
        public static List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> ConvertSOFiletrPolicyToFilterPolicy(List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters)
        {
            List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> filerPolicies = new List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy>();
            foreach (var filter in soFilters)
            {
                AvePoint.GCommon.Contract.CommonFilter.FilterPolicy filterPolicy = new AvePoint.GCommon.Contract.CommonFilter.FilterPolicy();
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
        public static AvePoint.GCommon.Contract.StorageOptimization.Object.Rule ConvertToSORule(AvePoint.RA.Contract.Global.Object.ClassificationRule autoRule, List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters, List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> filerPolicies, string andOrStr)
        {
            AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule = new AvePoint.GCommon.Contract.StorageOptimization.Object.Rule();
            rule.Id = Guid.NewGuid().ToString();
            rule.SOFilters = soFilters;
            rule.Filters = filerPolicies;
            rule.PolicyLevel = GetPolicyLevel(autoRule.RuleLevel);
            rule.Order = autoRule.RuleOrder;
            rule.ProfileType = ServerFilterPolicy.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            //rule.AndOrExpression = GetAndOrExpression(soFilters, autoRule.RuleLevel);
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>() { { (PolicyLevel)autoRule.RuleLevel, andOrStr } };
            return rule;
        }
        private static PolicyLevel GetPolicyLevel(int level)
        {
            switch (level)
            {
                case 1048576:
                    return PolicyLevel.FileSysFile;
                case 2097152:
                    return PolicyLevel.FileSysFolder;
                case 32:
                    return PolicyLevel.Item;
                case 64:
                    return PolicyLevel.Document;
                case 128:
                    return PolicyLevel.Attachment;
                case 256:
                    return PolicyLevel.DocumentVersion;
                case 512:
                    return PolicyLevel.ItemVersion;
                default:
                    return PolicyLevel.None;
            }
        }

        public static string GetFiltersAndOrExpression(List<AvePoint.RA.Contract.Global.Object.RuleFilter> filters)
        {
            //string AndOrExpression = "(";
            string AndOrExpression = string.Empty;
            for (int i = 0; i < filters.Count; i++)
            {
                AvePoint.RA.Contract.Global.Object.RuleFilter filter = filters[i];
                if (i == filters.Count - 1)
                {
                    AndOrExpression += string.Format("{0}", filter.SequenceNo);
                }
                else
                {
                    AndOrExpression += string.Format("{0} {1} ", filter.SequenceNo, filter.CombineMode == (int)ArchiverFilterCombineMode.And ? "And" : "Or");
                }
            }
            //AndOrExpression += ")";
            return AndOrExpression;
        }
        public static void ConvertToSOFilters(List<AvePoint.RA.Contract.Global.Object.FilterGroup> filterGroups, ref int sequenceNo, ref List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters)
        {
            foreach (var filterGroup in filterGroups)
            {
                foreach (var raFilter in filterGroup.Filters)
                {
                    sequenceNo++;
                    AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy soFilter = BuildSOFilter(raFilter, sequenceNo);
                    soFilters.Add(soFilter);
                }
                ConvertToSOFilters(filterGroup.FilterGroups, ref sequenceNo, ref soFilters);
            }
        }
        public static AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy BuildSOFilter(AvePoint.RA.Contract.Global.Object.RuleFilter filter, int sequenceNo)
        {
            ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
            arFilter.CombineMode = (ArchiverFilterCombineMode)filter.CombineMode;
            //arFilter.SequenceNo = filter.SequenceNo;
            arFilter.SequenceNo = sequenceNo;
            arFilter.Level = (PolicyLevel)filter.Level;
            arFilter.Condition = (ArchiverFilterCondition)filter.Condition;
            arFilter.RuleType = (ArchiverFilterRuleType)filter.RuleType;
            if (!string.IsNullOrEmpty(filter.filterName))
            {
                arFilter.RuleName = filter.filterName;
            }
            //arFilter.Dto.Rule = arFilter.RuleBase;
            if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime || arFilter.RuleType == ArchiverFilterRuleType.CreatedTime
         || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn || arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty)
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
                    arFilter.Value1Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)filter.Value1Unit;
                }
            }
            else
            {
                arFilter.Value1 = filter.Value1;
                if (filter.RuleType == (int)ArchiverFilterRuleType.DocumentSize || filter.RuleType == (int)ArchiverFilterRuleType.SiteCollectionSizeTrigger
                    || filter.RuleType == (int)ArchiverFilterRuleType.Size)
                {
                    arFilter.Value1Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)filter.Value1Unit;
                    arFilter.Value2Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)filter.Value2Unit;
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
        //public static void Autoclassification(IAveFolder folder, IAveTaxonomyField aveTaxField, RMSharePointOnPremiseSetting setting, DateTime startTime, DateTime endTime, IAveORecords records, ref bool hasError)
        //{
        //    IAveListItemCollection items = null;
        //    using (new RA.Common.AgentPerformanceScope(string.Format("Autoclassification Query Items. Folder Url:[{0}]", folder.Url)))
        //    {
        //        items = GetItemsWithCamlQuery(folder, setting, startTime, endTime, aveTaxField.InternalName);
        //        logger.Info("Autoclassification Query  Items Count: {0}", items == null ? 0 : items.Count);
        //    }

        //    using (new RA.Common.AgentPerformanceScope(string.Format("Autoclassification Set Value. Folder Url:[{0}]", folder.Url)))
        //    {
        //        AutoSetTerm(items, folder.ParentList, aveTaxField, setting.AutoClassificationRules, records, setting.IncludeDeclaredRecords, ref hasError);
        //    }
        //}
        //private static IAveListItemCollection GetItemsWithCamlQuery(IAveFolder folder, RMSharePointOnPremiseSetting setting, DateTime startTime, DateTime endTime, string columnInternalName)
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
                logger.Info("site collection config app column {0}", site.ID);
                return false;
            }
            else
            {
                //string columnSchema = "<Field Type=\"Note\" ReadOnly=\"TRUE\" DisplayName='" + RelatedColumnDisplayName + "' RichText=\"TRUE\" RichTextMode=\"FullHtml\" Group=\"Custom Columns\"  ID=\"{b40273fb-26d2-40e8-9a34-dd20bc9ca1d7}\"   Name='" + RelatedColumnInternalName + "' ShowInDisplayForm='TRUE' ShowInEditForm='FALSE' ShowInNewForm='FALSE' ShowInFileDlg='TRUE' ShowInListSettings='TRUE' ShowInVersionHistory='TRUE' ShowInViewForms='TRUE' UnlimitedLengthInDocumentLibrary=\"TRUE\"  />";
                logger.Info("create new sitecollection app column {0}", site.ID);
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
                logger.Info("remove site collection config app column {0}", site.ID);
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
                logger.Info("remove site collection config app column {0}", list.Title.LogBase64());
            }

            var listField = list.Fields.GetFieldById(RelatedColumnId, false);
            if (listField != null)
            {
                listField.ReadOnlyField = false;
                listField.Update();

                listField.Delete();
                listField.Update();
                logger.Info("remove list config app column {0}", list.Title.LogBase64());
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
                    var app = apps.FirstOrDefault();
                    if (app.Status == AveAppInstanceStatus.Uninstalling)
                    {
                        logger.Info("remove app {0}, status is uninstalling.", aveWeb.Url.LogBase64());
                    }
                    else if (app.Status == AveAppInstanceStatus.Installing)
                    {
                        //app.Cancel
                        logger.Info("remove app {0}, status is installing.", aveWeb.Url.LogBase64());
                    }
                    else if (app.Status == AveAppInstanceStatus.Installed)
                    {
                        logger.Info("remove app {0}.", aveWeb.Url.LogBase64());
                        app.Uninstall();
                    }
                    else
                    {
                        logger.Info("remove app {0}, status is {1}.", aveWeb.Url.LogBase64(), app.Status.ToString().LogBase64());
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Info("Uninstall app failed,web: {0}, error: {1}", aveWeb.Url.LogBase64(), ex.ToString());
                return false;
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
                var propertiesDic = (propertiesCacheObj as Dictionary<string, object>);
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
            string libPhyName = AgentUtil.GetAppSettingValue("RevIMHoldPhysicalLibraryName");
            string colPhyName = AgentUtil.GetAppSettingValue("RevIMHomeLocationName");
            string contentTypeNames = AgentUtil.GetAppSettingValue("RevIMWorkflowContentTypes");
            string requestListName = AgentUtil.GetAppSettingValue("RevIMRequestListName");
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
                //var phyColName = Util.GetAppSettingValue("RevIMHomeLocationName");
                //var locationTermSetId = TermSetDao.GetRMTermSet((int)TermSetType.Physical).UniqueId;
                //var physicalColumn = list.Fields.GetField(phyColName);
                //if (physicalColumn == null)
                //{
                //    //TO  DO Detail & update sp setting
                //    throw new Exception("RevIMHomeLocationName is null");
                //}
                //IAveTaxonomyField physicalLocationCol = physicalColumn as IAveTaxonomyField;
                //var termStoreId = list.ParentWeb.Site.AveSPTaxonomySession.TermStores[0].ID;
                //if (termStoreId.Equals(physicalLocationCol.SspId) && locationTermSetId.Equals(physicalLocationCol.TermSetId))
                //{
                //    //TO DO detail && add skip logic
                //    //******                ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                //    //string.Empty, "RM_SS_ConfigPhysicalAction", JobReportDetailStatus.Skipped, string.Empty));
                //    logger.Info("Skip update the physical column");
                //    return;
                //}
                //physicalLocationCol.SspId = termStoreId;
                //physicalLocationCol.TermSetId = locationTermSetId;
                //physicalLocationCol.EnforceUniqueValues = false;
                //physicalLocationCol.AllowMultipleValues = false;
                //physicalLocationCol.DefaultValue = string.Empty;
                //physicalLocationCol.Title = phyColName;
                //physicalLocationCol.Indexed = true;
                //physicalLocationCol.Required = true;
                //physicalLocationCol.Description = string.Empty;
                //physicalLocationCol.Update();
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
                //var boxTypeColumnName = Util.GetAppSettingValue("RevIMBoxTypeName");
                //var boxTypeField = list.Fields.GetField(boxTypeColumnName);
                //if (boxTypeField == null)
                //{
                //    logger.Warn("Get Physical Box type field error {0}", list.RootFolder.Url);
                //    //*****                ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                //    //string.Empty, "RM_SS_ConfigBoxTypeColumnAction", JobReportDetailStatus.Failed, "RM_SS_NotFoundBoxTypeFiled"));
                //    //TO DO Detail
                //    //Setting Status
                //    throw new Exception("RevIMBoxTypeName is null");
                //}
                //IAveFieldChoice boxTypeChoiceField = boxTypeField as IAveFieldChoice;
                //List<RMContainer> allContainers = ContainerDao.GetAllContainers();
                //string defaultValue = string.Empty;
                ////List<string> containerNames = new List<string>();
                //StringCollection containerNames = new StringCollection();
                //foreach (var container in allContainers)
                //{
                //    if (container.IsDefault)
                //    {
                //        defaultValue = container.TypeName;
                //    }
                //    if (!container.IsRemoved)
                //    {
                //        containerNames.Add(container.TypeName);
                //    }
                //}
                //if ((boxTypeChoiceField.DefaultValue.Equals(defaultValue) && boxTypeChoiceField.Choices.Equals(containerNames)) || allContainers.Count == 0)
                //{
                //    logger.Info("skip config box type column");
                //    //********                   ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                //    //string.Empty, "RM_SS_ConfigBoxTypeColumnAction", JobReportDetailStatus.Skipped, string.Empty));
                //    //this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigBoxTypeAction, JobDetailsStatus.Skipped, null);
                //    return;
                //}

                //boxTypeChoiceField.DefaultValue = defaultValue;
                //boxTypeChoiceField.Choices.Clear();
                //foreach (var containerName in containerNames)
                //{
                //    boxTypeChoiceField.Choices.Add(containerName);
                //}
                //boxTypeChoiceField.Update();
                //// ******               ReportService.Commit(new SPSettingJobReportEntry(list.Title, list.ParentWeb.Url + "/" + list.RootFolder.Url, "",
                ////string.Empty, "RM_SS_ConfigBoxTypeColumnAction", JobReportDetailStatus.Success, string.Empty));
                ////TO DO add detail
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
            logger.Info($"Get folder default values for list:{list.Title.LogBase64()}");
            var result = new List<string>();
            string foldersXml = GetXmlWithFolderDefaultValue(list);
            if (!string.IsNullOrEmpty(foldersXml))
            {
                logger.Info($"Folder Default Values:{foldersXml.LogBase64()}");
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
                    logger.Info($"Current Folder:{currentFolder.LogBase64()}");
                    if (string.IsNullOrEmpty(parentFolderPath))
                    {
                        result.Add(currentFolder);
                    }
                    else
                    {
                        if (currentFolder.StartsWith(parentFolderPath))
                        {
                            result.Add(currentFolder);
                        }
                        else
                        {
                            logger.Info($"{currentFolder.LogBase64()} is not a sub folder of {parentFolderPath.LogBase64()}");
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

        private static AveCamlQuery GetItemCamlQuery(int rowlimit)
        {
            CAMLManager cm = new CAMLManager();
            cm.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, "ID", Types.FieldTypes.Number, Types.QueryTypes.IsNotNull, ""));
            AveCamlQuery query = new AveCamlQuery();
            cm.ScopeType = Types.ScopeTypes.Default;
            cm.RowLimit = rowlimit;
            string queryXml = cm.GetFullCAML();
            query.ViewXml = queryXml;
            return query;
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
