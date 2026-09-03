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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;

namespace AvePoint.RA.Service.Services.PhysicalObject.AuditHandler
{
    class PhysicalObjectBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(PhysicalObjectBeforeAuditHandler));
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }

        public IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        public ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private const string BARCODE_STANDARD_KEY = "Barcode_Standard";

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            PhysicalObjectDto param = args[0] as PhysicalObjectDto;
            var info = new RMAuditInfo();
            info.Action = AuditCommon.GetAuditAction((AuditAction)action, param);
            switch (info.Action)
            {
                case AuditAction.UpdatePhysicalBox:
                case AuditAction.UpdatePhysicalFile:
                case AuditAction.UpdatePhysicalRecord:
                    await CollectUpdatePhyObjectAsync(info, model, category, action, args, target);
                    break;
                case AuditAction.SaveBarcodeStandard:
                    await CollectUpdateBarcodeStandardAsync(info, model, category, action, args, target);
                    break;
                default:
                    break;
            }
            return info;
        }

        private async System.Threading.Tasks.Task CollectUpdateBarcodeStandardAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target)
        {
            info.ModifyContent = new List<AuditItem>();
            var auditItem = new AuditItem();
            var oldValue = KeyValueDao.GetValueByKey(BARCODE_STANDARD_KEY);
            var newValue = args[0].ToString();
            auditItem.TargetSetting = I18NEntity.GetString("RM_PRM_PRE_BarcodeStandard");
            auditItem.OldValue = oldValue.Value == "0" ? I18NEntity.GetString("RM_PRM_PRE_BarcodeStandard_Code128") : I18NEntity.GetString("RM_PRM_PRE_BarcodeStandard_Code39");
            auditItem.NewValue = newValue == "0" ? I18NEntity.GetString("RM_PRM_PRE_BarcodeStandard_Code128") : I18NEntity.GetString("RM_PRM_PRE_BarcodeStandard_Code39");
            info.ModifyContent.Add(auditItem);
        }


        private async Task CollectUpdatePhyObjectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target)
        {
            PhysicalObjectDto newObject = args[0] as PhysicalObjectDto;
            PhysicalObjectDto oldObject = await ExplorerService.GetPhysicalObjectByIdAsync(newObject.Id);
            if (oldObject.Id == newObject.Id)
            {
                var template = await TemplateManagementService.LoadTemplateDtoAsync(oldObject.TemplateId, oldObject);
                var allColumns = GetColumnsInfo(template);
                info.ModifyContent = new List<AuditItem>();
                await AddPhyColumnsAuditAsync(info, allColumns, oldObject, newObject);
                AddLoanAudit(info, oldObject, newObject);
            }
        }

        private List<TemplateColumnDto> GetColumnsInfo(TemplateDto template)
        {
            var allColumns = new List<TemplateColumnDto>();
            template.categories.ForEach(c => allColumns.AddRange(c.columns));
            return allColumns;
        }

        private string GetSelectedChoiceColumnNames(Dictionary<int, string> options, List<string> optionValues)
        {
            var selectedOptionNames = options.Where(s => optionValues.Contains(s.Key.ToString())).Select(s => s.Value).ToList();
            if (selectedOptionNames.Count > 0)
            {
                return string.Join(",", selectedOptionNames).TrimEnd(',');
            }
            return string.Empty;
        }

        private async System.Threading.Tasks.Task AddPhyColumnsAuditAsync(RMAuditInfo info, List<TemplateColumnDto> allColumns, PhysicalObjectDto oldObject, PhysicalObjectDto newObject)
        {
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            string dtFormat = GeneralSettingService.GetDateTimeFormat(gls);

            foreach (var key in newObject.MetaInfo.Keys)
            {
                if (!oldObject.MetaInfo.ContainsKey(key))
                {
                    _ = oldObject.MetaInfo.TryAdd(key, string.Empty);
                }
            }

            foreach (KeyValuePair<string, string> oldMetaItem in oldObject.MetaInfo)
            {
                if (allColumns.Any(c => c.uniqueId.ToString() == oldMetaItem.Key))
                {
                    var columnInfo = allColumns.Where(c => c.uniqueId.ToString() == oldMetaItem.Key).First();
                    if (newObject.MetaInfo.ContainsKey(oldMetaItem.Key))
                    {
                        var oldValue = oldMetaItem.Value;
                        var newValue = newObject.MetaInfo[oldMetaItem.Key];
                        if (newValue != oldValue)
                        {
                            try
                            {
                                AuditItem auditItem = new AuditItem();
                                auditItem.TargetSetting = I18NEntity.GetString(columnInfo.columnName);
                                switch ((Contract.TemplateManagement.ColumnType)columnInfo.typeId)
                                {
                                    case Contract.TemplateManagement.ColumnType.SingleText:
                                    case Contract.TemplateManagement.ColumnType.MultipleText:
                                    case Contract.TemplateManagement.ColumnType.Number:
                                        auditItem.OldValue = oldValue;
                                        auditItem.NewValue = newValue;
                                        break;
                                    case Contract.TemplateManagement.ColumnType.DateTime:
                                        auditItem.OldValue = string.IsNullOrEmpty(oldValue) ?
                                            string.Empty :
                                            GeneralSettingService.ConvertTiksToDateTime(gls, JsonConvert.DeserializeObject<DateTimeColumnValue>(oldValue).GetUtcDate().Ticks, true).SimplifyFormatTime;
                                        auditItem.NewValue = string.IsNullOrEmpty(newValue) ?
                                            string.Empty :
                                            GeneralSettingService.ConvertTiksToDateTime(gls, JsonConvert.DeserializeObject<DateTimeColumnValue>(newValue).GetUtcDate().Ticks, true).SimplifyFormatTime;
                                        break;
                                    case Contract.TemplateManagement.ColumnType.SingleChoice:
                                        Dictionary<int, string> options = JsonConvert.DeserializeObject<Dictionary<int, string>>(columnInfo.optionsJSON);
                                        var oldSelectedOption = JsonConvert.DeserializeObject<ChoiceColumnValue>(oldValue);
                                        auditItem.OldValue = GetSelectedChoiceColumnNames(options, new List<string> { oldSelectedOption.Value });

                                        var newSelectedOption = JsonConvert.DeserializeObject<ChoiceColumnValue>(newValue);
                                        auditItem.NewValue = GetSelectedChoiceColumnNames(options, new List<string> { newSelectedOption.Value });
                                        break;
                                    case Contract.TemplateManagement.ColumnType.PeopleOrGroup:
                                        //Fix bug by jlnan in Explorer search dev branch
                                        List<UIPeopleColumnValue> oldP = JsonConvert.DeserializeObject<List<UIPeopleColumnValue>>(oldValue);
                                        if(oldP != null && oldP.Count > 0)
                                        {
                                            auditItem.OldValue = string.Join(",", oldP.Select(a => a.DisplayName).ToArray());
                                        }
                                        List<UIPeopleColumnValue> newP = JsonConvert.DeserializeObject<List<UIPeopleColumnValue>>(newValue);
                                        if (newP != null && newP.Count > 0)
                                        {
                                            auditItem.NewValue = string.Join(",", newP.Select(a => a.DisplayName).ToArray());
                                        } 
                                        break;
                                    case Contract.TemplateManagement.ColumnType.MultipleChoice:
                                        Dictionary<int, string> mulOptions = JsonConvert.DeserializeObject<Dictionary<int, string>>(columnInfo.optionsJSON);
                                        var oldCheckedOptions = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(oldValue);
                                        var oldOptionValues = oldCheckedOptions.Select(s => s.Value).ToList();
                                        auditItem.OldValue = GetSelectedChoiceColumnNames(mulOptions, oldOptionValues);

                                        var newCheckedOptions = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(newValue);
                                        var newOptionValues = newCheckedOptions.Select(s => s.Value).ToList();
                                        auditItem.NewValue = GetSelectedChoiceColumnNames(mulOptions, newOptionValues);
                                        break;
                                    case Contract.TemplateManagement.ColumnType.Taxonomy:
                                        auditItem.OldValue = JsonConvert.DeserializeObject<TaxonomyColumnValue>(oldValue).Name;
                                        auditItem.NewValue = JsonConvert.DeserializeObject<TaxonomyColumnValue>(newValue).Name;
                                        break;
                                }
                                info.ModifyContent.Add(auditItem);
                            }
                            catch (Exception ex)
                            {
                                logger.Info($"Physical Object Audit failed {ex.ToString()}");
                            }
                        }
                    }

                }
            }
        }

        private void AddLoanAudit(RMAuditInfo info, PhysicalObjectDto oldObject, PhysicalObjectDto newObject)
        {
            if (oldObject.NodeType == Contract.RMWeb.Tree.Base.RMNodeType.PhyFile)
            {
                if (oldObject.PersonHoldBy != newObject.PersonHoldBy)
                {
                    AuditItem onLoanItem = new AuditItem();
                    onLoanItem.TargetSetting = I18NEntity.GetString("RM_PRM_PRE_Column_PersonHoldStatus");
                    onLoanItem.OldValue = oldObject.PersonHoldBy;
                    onLoanItem.NewValue = newObject.PersonHoldBy;
                    info.ModifyContent.Add(onLoanItem);

                    AuditItem loanedByItem = new AuditItem();
                    loanedByItem.TargetSetting = I18NEntity.GetString("RM_PRM_PRE_Column_LoanBy");
                    loanedByItem.OldValue = oldObject.PersonHoldBy;
                    loanedByItem.NewValue = newObject.PersonHoldBy;
                    info.ModifyContent.Add(loanedByItem);
                }
            }
        }

    }
}
