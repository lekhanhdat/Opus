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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Common;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.Contract.Tenant;
using AngleSharp.Css;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Teams.RMTeamsColumn;
using Aspose.Pdf.Operators;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler
{
    public class SharePointSettingBeforeAuditHandler : IBeforeAuditHandler
    {
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private IEXOSettingDao EXOSettingDao => PlatformWindsorManager.GetService<IEXOSettingDao>();
        private IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IPhysicalRecordSettingDao PhysicalRecordSettingDao => PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao => PlatformWindsorManager.GetService<ISharePointOnPremiseSettingDao>();
        private IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IRMArchiverSettingsService ArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
        private IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();

        private IRMCustomIndexMetadataDao RMCustomIndexMetadataDao = PlatformWindsorManager.GetService<IRMCustomIndexMetadataDao>();

        private IRMCustomMetadataColumnDao RMCustomMetadataColumnDao = PlatformWindsorManager.GetService<IRMCustomMetadataColumnDao>();

        private RALogger logger = RALogger.GetInstance(typeof(SharePointSettingBeforeAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            try
            {
                info.ModifyContent = new List<AuditItem>();
                info.Action = (AuditAction)action;
                info.Category = (AuditCategory)category;
                info.Module = (AuditModule)model;
                List<string> scopeIds = new List<string>();
                //disposal Skip remove content and destroy action记录   RunPRDisposalJob 不在这里
                if (action == (int)AuditAction.RunDisposalJob || action == (int)AuditAction.RunOneDriveDisposalJob || action == (int)AuditAction.RunTeamsDisposalJob)
                {
                    var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(args[2].ToString());
                    string newResult = node.SkipRemoveContentAndDestroyAction ? "True" : "False";
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_Skip", NewValue = newResult });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_BCM_EnsureRun_DecryptIRM", NewValue = node.IsEnableSuperUserDecrypt ? "True" : "False" });
                }
                else if (action == (int)AuditAction.RunEXODisposalJob)
                {
                    var EXOnode = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(args[2].ToString());
                    string newResult = EXOnode.SkipRemoveContentAndDestroyAction ? "True" : "False";
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_Skip", NewValue = newResult });
                }


                if (action == (int)AuditAction.ConfigureGroupGlobalsetting)
                {
                    SaveTreePage tree = (SaveTreePage)args[0];
                    List<RMSPTreeNode> nodes = tree.allRMSPTreeNode; 
                    List<string> groupNames = new List<string>();
                    foreach (RMSPTreeNode node in nodes)
                    {
                        groupNames.Add(node.Name);
                        scopeIds.Add(node.SPObjectId);
                    }
                    info.Object = string.Join(";", groupNames);
                    List<RMSharePointSetting> dbSettings = SharePointSettingDao.GetColumnInfos(scopeIds.ToArray<string>());
                    Dictionary<string, RMSharePointSetting> scopeIdSettingMap = new Dictionary<string, RMSharePointSetting>();
                    foreach (RMSharePointSetting dbSetting in dbSettings)
                    {
                        scopeIdSettingMap.Add(dbSetting.ScopeId.ToString(), dbSetting);
                    }
                    foreach (RMSPTreeNode node in nodes)
                    {
                        string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                        string oldContainerPath = "RM_SPS_NoRecordOwner", newContainerPath = "RM_SPS_NoRecordOwner";
                        string oldContainerDes = string.Empty;
                        bool oldInheritParent = false;
                        //bool oldEMailToRecordOwner = false;
                        bool oldApplyExistDocument = false;
                        if (scopeIdSettingMap.ContainsKey(node.SPObjectId))
                        {
                            RMSharePointSetting settingValue = new RMSharePointSetting();
                            scopeIdSettingMap.TryGetValue(node.SPObjectId, out settingValue);
                            oldContainerDes = settingValue.DescriptionOfContainer;
                            oldInheritParent = settingValue.IsInheritParentTerm;
                            //oldEMailToRecordOwner = settingValue.EMailToRecordOwner;
                            oldApplyExistDocument = settingValue.NeedCheckDefaultValue;
                            List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(settingValue.Id).Select(a => a.ObjectId).ToList();
                            List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                            if (!node.IsUsingExistColumnName)
                            {
                                if (settingValue.IsUsingExistColumnName && !string.IsNullOrEmpty(settingValue.ExistColumnName))
                                {
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ColumnName", OldValue = settingValue.ExistColumnName, NewValue = node.ColumnName });
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DocumentDes", NewValue = node.Description });
                                    string newPath = string.Empty;
                                    if (node.DefaultTermId != Guid.Empty)
                                    {
                                        newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                                    }
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                                    info.ModifyContent.Add(new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                                        NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                                    });
                                    info.ModifyContent.Add(new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_GS_ApplyExistingDoc",
                                        NewValue = tree.NeedCheckDefaultVaule ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                                    });
                                }
                                else
                                {
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ColumnName", OldValue = settingValue.ColumnName, NewValue = node.ColumnName });
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DocumentDes", OldValue = settingValue.Description, NewValue = node.Description });
                                    string newPath = string.Empty;
                                    string oldPath = string.Empty;
                                    if (node.DefaultTermId != Guid.Empty)
                                    {
                                        newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                                    }
                                    if (settingValue.DefaultTermId != Guid.Empty)
                                    {
                                        oldPath = TermDao.GetTermNamesPathByTermId(settingValue.DefaultTermId);
                                    }
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath, NewValue = newPath });
                                    info.ModifyContent.Add(new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                                        OldValue = settingValue.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel",
                                        NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                                    });
                                    info.ModifyContent.Add(new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_GS_ApplyExistingDoc",
                                        NewValue = tree.NeedCheckDefaultVaule ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                                        OldValue = oldApplyExistDocument ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    });
                                }
                            }
                            else
                            {
                                if (settingValue.IsUsingExistColumnName && !string.IsNullOrEmpty(settingValue.ExistColumnName))
                                {
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ColumnName", OldValue = settingValue.ExistColumnName, NewValue = node.ExistColumnName });
                                }
                                else
                                {
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ColumnName", OldValue = settingValue.ColumnName, NewValue = node.ExistColumnName });
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DocumentDes", OldValue = settingValue.Description });
                                    string oldPath = string.Empty;
                                    if (settingValue.DefaultTermId != Guid.Empty)
                                    {
                                        oldPath = TermDao.GetTermNamesPathByTermId(settingValue.DefaultTermId);
                                    }
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                                    info.ModifyContent.Add(new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                                        OldValue = settingValue.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel",
                                    });
                                    info.ModifyContent.Add(new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_GS_ApplyExistingDoc",
                                        OldValue = oldApplyExistDocument ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    });
                                }
                            }
                            ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : "RM_SPS_NoRecordOwner";
                            if (settingValue.TermIdOfContainer != Guid.Empty)
                            {
                                oldContainerPath = TermDao.GetTermNamesPathByTermId(settingValue.TermIdOfContainer);
                            }

                            if (node.TermIdOfContainer != Guid.Empty)
                            {
                                newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                            }
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                OldValue = settingValue.EnableRelatedRecords ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                                NewValue = node.EnableRelatedRecords ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                            });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel", NewValue = newContainerPath, OldValue = oldContainerPath });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_Des", OldValue = oldContainerDes, NewValue = node.DescriptionOfContainer });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_IsInheritParentTerm",
                                OldValue = oldInheritParent ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                                NewValue = node.IsInheritParentTerm ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });

                            //ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.displayName)) : I18NEntity.GetString("RM_SPS_NoRecordOwner");
                            //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_RecordOwners"), NewValue = ownerNewValue, OldValue = ownerOldValue });
                            //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_SendEMail"), NewValue = node.EMailToRecordOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"), OldValue = oldEMailToRecordOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No") });
                        }
                        else
                        {
                            if (!node.IsUsingExistColumnName)
                            {
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ColumnName", NewValue = node.ColumnName });
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DocumentDes", NewValue = node.Description });
                                string newPath = string.Empty;
                                if (node.DefaultTermId != Guid.Empty)
                                {
                                    newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                                }
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                                    NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                                });
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_SPS_GS_ApplyExistingDoc",
                                    NewValue = tree.NeedCheckDefaultVaule ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                });
                            }
                            else
                            {
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ColumnName", NewValue = node.ExistColumnName });
                            }

                            if (node.TermIdOfContainer != Guid.Empty)
                            {
                                newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                            }
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                NewValue = node.EnableRelatedRecords ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                            });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel", NewValue = newContainerPath });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_Des", NewValue = node.DescriptionOfContainer });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_IsInheritParentTerm", NewValue = node.IsInheritParentTerm ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });

                            //ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.displayName)) : I18NEntity.GetString("RM_SPS_NoRecordOwner");
                            //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_RecordOwners"), NewValue = ownerNewValue });
                            //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_SendEMail"), NewValue = node.EMailToRecordOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No") });
                        }
                    }
                }
                else if (action == (int)AuditAction.ConfigureCustomSetting)
                {
                    List<RMSPTreeNode> nodes = (List<RMSPTreeNode>)args[0];
                    bool needCheckDefaultValue = (bool)args[3];
                    string nodeUrl = string.Empty;
                    List<string> nodeUrls = new List<string>();
                    foreach (RMSPTreeNode node in nodes)
                    {
                        nodeUrls.Add(this.GetFullUrl(node));
                        scopeIds.Add(node.SPObjectId);
                    }

                    List<RMSharePointSetting> dbSettings = SharePointSettingDao.GetColumnInfos(scopeIds.ToArray<string>());
                    Dictionary<string, RMSharePointSetting> scopeIdSettingMap = new Dictionary<string, RMSharePointSetting>();
                    foreach (RMSharePointSetting dbSetting in dbSettings)
                    {
                        scopeIdSettingMap.Add(dbSetting.ScopeId.ToString(), dbSetting);
                    }
                    foreach (RMSPTreeNode node in nodes)
                    {
                        string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                        string oldContainerPath = "RM_SPS_NoRecordOwner", newContainerPath = "RM_SPS_NoRecordOwner";
                        string oldContainerDes = string.Empty;
                        bool oldInheritParent = false;
                        //bool oldEMailToRecordOwner = false;
                        bool oldApplyExistDocument = false;
                        bool oldMarkPhysical = false;
                        if (scopeIdSettingMap.ContainsKey(node.SPObjectId))
                        {
                            RMSharePointSetting settingValue = new RMSharePointSetting();
                            scopeIdSettingMap.TryGetValue(node.SPObjectId, out settingValue);
                            oldContainerDes = settingValue.DescriptionOfContainer;
                            oldInheritParent = settingValue.IsInheritParentTerm;
                            //oldEMailToRecordOwner = settingValue.EMailToRecordOwner;
                            oldApplyExistDocument = settingValue.NeedCheckDefaultValue;
                            oldMarkPhysical = settingValue.IsEnableHoldPhyical;
                            string newSubsetPath = string.Empty;
                            string oldSubsetPath = string.Empty;
                            List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(settingValue.Id).Select(a => a.ObjectId).ToList();
                            List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                            if (node.TermId != Guid.Empty)
                            {
                                newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                            }
                            if (settingValue.TermId != Guid.Empty)
                            {
                                oldSubsetPath = TermDao.GetTermNamesPathByTermId(settingValue.TermId);
                            }
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DocumentDes", NewValue = node.Description, OldValue = settingValue.Description });
                            string newPath = string.Empty;
                            string oldPath = string.Empty;
                            if (node.DefaultTermId != Guid.Empty)
                            {
                                newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                            }
                            else
                            {
                                newPath = "RM_SS_NoDefaultValue";
                            }
                            if (settingValue.DefaultTermId != Guid.Empty)
                            {
                                oldPath = TermDao.GetTermNamesPathByTermId(settingValue.DefaultTermId);
                            }
                            else
                            {
                                oldPath = "RM_SS_NoDefaultValue";
                            }
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath, NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                                OldValue = settingValue.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel",
                                NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                            });
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SPS_GS_ApplyExistingDoc",
                                NewValue = needCheckDefaultValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                                OldValue = oldApplyExistDocument ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                            });
                            ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : "RM_SPS_NoRecordOwner";
                            if (node.Level != (int)NodeLevel.List && node.Level != (int)NodeLevel.Folder)
                            {
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_SP_SettingRelatedRecords",
                                    OldValue = settingValue.EnableRelatedRecords ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                                    NewValue = node.EnableRelatedRecords ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                });
                                //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_RecordOwners"), OldValue = owners });
                            }
                            if (node.Level != (int)NodeLevel.Folder)
                            {
                                if (settingValue.TermIdOfContainer != Guid.Empty)
                                {
                                    oldContainerPath = TermDao.GetTermNamesPathByTermId(settingValue.TermIdOfContainer);
                                }
                                if (node.TermIdOfContainer != Guid.Empty)
                                {
                                    newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                                }
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel", OldValue = oldContainerPath, NewValue = newContainerPath });
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_Des", OldValue = oldContainerDes, NewValue = node.DescriptionOfContainer });
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_IsInheritParentTerm",
                                    OldValue = oldInheritParent ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                                    NewValue = node.IsInheritParentTerm ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });
                            }
                            //ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.displayName)) : I18NEntity.GetString("RM_SPS_NoRecordOwner");
                            //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_RecordOwners"), NewValue = ownerNewValue, OldValue = ownerOldValue });
                            //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_SendEMail"), NewValue = node.EMailToRecordOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"), OldValue = oldEMailToRecordOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No") });

                        }
                        else
                        {
                            string newSubsetPath = string.Empty;
                            if (node.TermId != Guid.Empty)
                            {
                                newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                            }
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DocumentDes", NewValue = node.Description });
                            string newPath = string.Empty;
                            if (node.DefaultTermId != Guid.Empty)
                            {
                                newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                            }
                            else
                            {
                                newPath = "RM_SS_NoDefaultValue";
                            }
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                                NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                            });
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SPS_GS_ApplyExistingDoc",
                                NewValue = needCheckDefaultValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                            });
                            if (node.Level != (int)NodeLevel.List && node.Level != (int)NodeLevel.Folder)
                            {
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_SP_SettingRelatedRecords",
                                    NewValue = node.EnableRelatedRecords ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                });
                            }
                            if (node.Level != (int)NodeLevel.Folder)
                            {
                                if (node.TermIdOfContainer != Guid.Empty)
                                {
                                    newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                                }
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel", NewValue = newContainerPath });
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_Des", NewValue = node.DescriptionOfContainer });
                                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_IsInheritParentTerm", NewValue = node.IsInheritParentTerm ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });
                            }
                            //ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.displayName)) : I18NEntity.GetString("RM_SPS_NoRecordOwner");
                            //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_RecordOwners"), NewValue = ownerNewValue });
                            //info.ModifyContent.Add(new AuditItem() { TargetSetting = I18NEntity.GetString("RM_SPS_SendEMail"), NewValue = node.EMailToRecordOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No") });

                        }
                    }
                    info.Object = string.Join(";", nodeUrls);
                }


                #region SP Setting Auditor
                if (action == (int)AuditAction.EditColumnSetting)
                {
                    #region AuditAction.EditColumnSetting
                    var isCSDTenant = TenantService.IsCSDTenant();
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointSetting dbSetting = SharePointSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ColumnName",
                            OldValue = dbSetting.IsUsingExistColumnName ? GetExistingColumnWords(dbSetting.ExistColumnName, dbSetting.SetDocLevelTermForExistColumn) : dbSetting.ColumnName,
                            NewValue = node.IsUsingExistColumnName ? GetExistingColumnWords(node.ExistColumnName, node.SetDocLevelTermForExistColumn) : node.ColumnName,
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_DocumentDes",
                            OldValue = dbSetting.IsUsingExistColumnName ? "" : dbSetting.Description,
                            NewValue = node.IsUsingExistColumnName ? "" : node.Description
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_DisplayColumnRequired",
                            OldValue = dbSetting.IsUsingExistColumnName ? "" : YesOrNoString(dbSetting.ColumnRequired == null ? true : (bool)dbSetting.ColumnRequired),
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnRequired)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_HiddenColumn",
                            OldValue = dbSetting.IsUsingExistColumnName ? "" : YesOrNoString(dbSetting.ColumnHidden == null ? false : (bool)dbSetting.ColumnHidden),
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnHidden)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_EditKey_ShowUniqueID",
                            OldValue = YesOrNoString(dbSetting.IsShowUniqueId == null ? true : (bool)dbSetting.IsShowUniqueId),
                            NewValue = YesOrNoString(node.IsShowUniqueId)
                        });
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_JS_SPS_EditKey_KeepSPDefaultValue",
                                    OldValue = GetKeepDefaultValueYesOrNoString(dbSetting.IsKeepSharePointDefaultValue, dbSetting.SetTermForEmptyDefaultValue),
                                    NewValue = GetKeepDefaultValueYesOrNoString(node.IsKeepSharePointDefaultValue, node.SetTermForEmptyDefaultValue)
                                });
                            }

                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                OldValue = YesOrNoString(dbSetting.EnableRelatedRecords),
                                NewValue = YesOrNoString(node.EnableRelatedRecords)
                            });
                        }
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ColumnName",
                            NewValue = node.IsUsingExistColumnName ? node.ExistColumnName : node.ColumnName
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_DocumentDes",
                            NewValue = node.IsUsingExistColumnName ? "" : node.Description
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_DisplayColumnRequired",
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnRequired)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_HiddenColumn",//stodo
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnHidden)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_EditKey_ShowUniqueID",
                            NewValue = YesOrNoString(node.IsShowUniqueId)
                        });
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                NewValue = YesOrNoString(node.EnableRelatedRecords)
                            });
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditLocationOwnersSetting)
                {
                    #region AuditAction.EditLocationOwnersSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointSetting dbSetting = SharePointSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    var enableApprovalAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_EnableApproval" };
                    var processAudit = new AuditItem { TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName" };
                    var emailAudit = new AuditItem { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation" };
                    var ownerAudit = new AuditItem { TargetSetting = "RM_SPS_RecordOwners" };
                    var autoProcessAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_AutoApprove" };
                    string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                    ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;

                    enableApprovalAudit.NewValue = YesOrNoString(node.ApprovalType != (int)ApprovalType.None);
                    if (node.ApprovalType != (int)ApprovalType.None)
                    {
                        if (node.ApprovalType == (int)ApprovalType.ApprovalProcess)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(new Guid(node.WorkflowReferenceId));
                            if (!string.IsNullOrEmpty(workflow?.Name))
                            {
                                processAudit.NewValue = workflow?.Name;
                            }
                        }
                        else if (node.ApprovalType == (int)ApprovalType.AutoApproval)
                        {
                            autoProcessAudit.NewValue = YesOrNoString(true);
                        }
                        emailAudit.NewValue = YesOrNoString(node.EMailToRecordOwner);
                        ownerAudit.NewValue = ownerNewValue;
                    }

                    if (dbSetting != null)
                    {
                        enableApprovalAudit.OldValue = YesOrNoString(dbSetting.ApprovalType != ApprovalType.None);
                        List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id).Select(a => a.ObjectId).ToList();
                        List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                        ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                        if (dbSetting.ApprovalType != ApprovalType.None)
                        {
                            if (dbSetting.ApprovalType == ApprovalType.ApprovalProcess)
                            {
                                var workflow = ManualProcessManagementService.GetWorkflow(new Guid(dbSetting.WorkflowReferenceId));
                                if (!string.IsNullOrEmpty(workflow?.Name))
                                {
                                    processAudit.OldValue = workflow?.Name;
                                }
                            }
                            else if (dbSetting.ApprovalType == ApprovalType.AutoApproval)
                            {
                                autoProcessAudit.OldValue = YesOrNoString(true);
                            }

                            emailAudit.OldValue = YesOrNoString(dbSetting.EMailToRecordOwner);
                            ownerAudit.OldValue = ownerOldValue;
                        }
                    }
                    info.ModifyContent.Add(enableApprovalAudit);
                    info.ModifyContent.Add(processAudit);
                    info.ModifyContent.Add(ownerAudit);
                    info.ModifyContent.Add(emailAudit);
                    info.ModifyContent.Add(autoProcessAudit);
                    #endregion
                }
                else if (action == (int)AuditAction.EditDocLevelSetting)
                {
                    #region AuditAction.EditDocLevelSetting
                    var isCSDTenant = TenantService.IsCSDTenant();
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointSetting dbSetting = SharePointSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        bool oldApplyExistDocument = false;
                        string newSubsetPath = string.Empty;
                        string oldSubsetPath = string.Empty;
                        oldApplyExistDocument = dbSetting.NeedCheckDefaultValue;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        if (dbSetting.TermId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermId);
                        }
                        else if (dbSetting.TermSetId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dbSetting.TermSetId);
                        }

                        string newPath = string.Empty;
                        string oldPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        if (dbSetting.DefaultTermId != Guid.Empty)
                        {
                            oldPath = TermDao.GetTermNamesPathByTermId(dbSetting.DefaultTermId);
                        }
                        else
                        {
                            oldPath = "RM_SS_NoDefaultValue";
                        }
                        //if (node.Level != (int)NodeLevel.WebApplication)
                        //{
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });
                        //}
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                            OldValue = dbSetting.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel",
                            NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                        });

                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)dbSetting.DeployTermMethod),
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod)
                        });
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, dbSetting.ApplyExistType, dbSetting.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", OldValue = GetApplyExistString(dbSetting.IsApplyTermIncludeFolder(), dbSetting.ApplyExistType, false) });
                            }
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType, node.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", NewValue = GetApplyExistString(node.ApplyTermIncludeFolder, node.ApplyExistType, false) });
                            }
                        }
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(dbSetting.AutoClassificationRules);
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(dbSetting.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", OldValue = YesOrNoString(dbSetting.IsApplyTermIncludeFolder()) });
                            }
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                                TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                            });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", NewValue = YesOrNoString(node.ApplyTermIncludeFolder) });
                            }
                        }

                        if (IsEnableApplySettingAlwaysScanAll())
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_SPS_Auto_AlwaysRunFullJob",
                                OldValue = YesOrNoString(dbSetting.AlwaysScanAllExistDocuments),
                                NewValue = YesOrNoString(node.AlwaysScanAllExistDocuments)
                            });
                        }


                        if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || node.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                        {
                            var aiDefaultTermNewPath = string.Empty;
                            if (node.AIThenDefaultTermId != Guid.Empty)
                            {
                                aiDefaultTermNewPath = TermDao.GetTermNamesPathByTermId(node.AIThenDefaultTermId);
                            }
                            else
                            {
                                aiDefaultTermNewPath = "RM_SS_NoDefaultValue";
                            }
                            var aiReviewersNewValue = node.AIReviewers.Count > 0 ? string.Join(";", node.AIReviewers.Select(a => a.DisplayName)) : string.Empty;
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", NewValue = aiReviewersNewValue });
                            if (node.AIApprovalType != (int)ApprovalType.None)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", NewValue = YesOrNoString(node.AISendEMail) });
                            }
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", NewValue = aiDefaultTermNewPath });
                        }

                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || dbSetting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                        {
                            var aiDefaultTermOldPath = string.Empty;
                            if (dbSetting.AIThenDefaultTermId != Guid.Empty)
                            {
                                aiDefaultTermOldPath = TermDao.GetTermNamesPathByTermId(dbSetting.AIThenDefaultTermId);
                            }
                            else
                            {
                                aiDefaultTermOldPath = "RM_SS_NoDefaultValue";
                            }
                            List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id, RecordOwnerSettingType.AISharePointOnline).Select(a => a.ObjectId).ToList();
                            List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                            var aiReviewersOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", OldValue = aiReviewersOldValue });
                            if (dbSetting.AIApprovalType != ApprovalType.None)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", OldValue = YesOrNoString(dbSetting.AISendEMail) });
                            }
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", OldValue = aiDefaultTermOldPath });
                        }

                        if (node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                        }

                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(dbSetting.IncludeDeclaredRecords) });
                        }

                        if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                OldValue = YesOrNoString(dbSetting.EnableRelatedRecords),
                                NewValue = YesOrNoString(node.EnableRelatedRecords)
                            });
                        }
                    }
                    else
                    {

                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "", OldValue = "RM_JS_TM_inherBreak" });

                        string newSubsetPath = string.Empty;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        string newPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }

                        //if (node.Level != (int)NodeLevel.WebApplication)
                        //{
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });
                        //}
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                            NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                        });

                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod)
                        });
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType, node.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", NewValue = GetApplyExistString(node.ApplyTermIncludeFolder, node.ApplyExistType, false) });
                            }
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                                TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                            });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", NewValue = YesOrNoString(node.ApplyTermIncludeFolder) });
                            }
                        }

                        if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || node.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                        {
                            var aiDefaultTermNewPath = string.Empty;
                            if (node.AIThenDefaultTermId != Guid.Empty)
                            {
                                aiDefaultTermNewPath = TermDao.GetTermNamesPathByTermId(node.AIThenDefaultTermId);
                            }
                            else
                            {
                                aiDefaultTermNewPath = "RM_SS_NoDefaultValue";
                            }
                            var aiReviewersNewValue = node.AIReviewers.Count > 0 ? string.Join(";", node.AIReviewers.Select(a => a.DisplayName)) : string.Empty;
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", NewValue = aiReviewersNewValue });
                            if (node.AIApprovalType != (int)ApprovalType.None)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", NewValue = YesOrNoString(node.AISendEMail) });
                            }
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", NewValue = aiDefaultTermNewPath });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                        }

                        if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                NewValue = YesOrNoString(node.EnableRelatedRecords)
                            });
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditConLevelSetting)
                {
                    #region AuditAction.EditConLevelSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointSetting dbSetting = SharePointSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    string oldContainerPath = string.Empty, newContainerPath = string.Empty;
                    string oldContainerDes = string.Empty;
                    var oldEnableInheritParentTerm = false;
                    bool oldMarkPhysical = false;
                    if (dbSetting != null)
                    {
                        oldMarkPhysical = dbSetting.IsEnableHoldPhyical;
                        oldContainerDes = dbSetting.DescriptionOfContainer;
                        oldEnableInheritParentTerm = dbSetting.IsInheritParentTerm;
                        if (dbSetting.TermIdOfContainer != Guid.Empty)
                        {
                            oldContainerPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermIdOfContainer);
                        }
                        if (node.TermIdOfContainer != Guid.Empty)
                        {
                            newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                        }

                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_TermOfContainer", NewValue = newContainerPath, OldValue = oldContainerPath });
                        //i18n TODO
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_DescriptionOfContainer", NewValue = node.DescriptionOfContainer, OldValue = oldContainerDes });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_InheritParentTerm", NewValue = YesOrNoString(node.IsInheritParentTerm), OldValue = YesOrNoString(oldEnableInheritParentTerm) });
                    }
                    else
                    {
                        if (node.TermIdOfContainer != Guid.Empty)
                        {
                            newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                        }
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_TermOfContainer", NewValue = newContainerPath });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_DescriptionOfContainer", NewValue = node.DescriptionOfContainer });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_InheritParentTerm", NewValue = YesOrNoString(node.IsInheritParentTerm) });
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditInheritSetting)
                {
                    #region AuditAction.EditInheritSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.GeneralSetting4SPO)
                {
                    #region AuditAction.GeneralSetting4SPO
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointSetting dbSetting = SharePointSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        var oldEnableRecordManagement = dbSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        var newEnableRecordManagement = node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            OldValue = YesOrNoString(oldEnableRecordManagement),
                            NewValue = YesOrNoString(newEnableRecordManagement),
                        });
                        if (oldEnableRecordManagement && newEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                OldValue = YesOrNoString(dbSetting.IsSyncData),
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                        else if (oldEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                OldValue = YesOrNoString(dbSetting.IsSyncData)
                            });
                        }
                        else if (newEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                        RMSPTreeNode oldSPTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(dbSetting.NodeInfo);
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_SupportLockedSite",
                            OldValue = YesOrNoString(oldSPTreeNode.SupportLockedSite),
                            NewValue = YesOrNoString(node.SupportLockedSite),
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_EnableLifecycleManagementForSharePointLists",
                            OldValue = YesOrNoString(oldSPTreeNode.EnableLifecycleManagementForSharePointLists ?? true),
                            NewValue = YesOrNoString(node.EnableLifecycleManagementForSharePointLists ?? true),
                        });
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            NewValue = YesOrNoString(node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable),
                        });
                        if (node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_JS_SPS_SupportLockedSite",
                                NewValue = YesOrNoString(node.SupportLockedSite),
                            });
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_JS_SPS_EnableLifecycleManagementForSharePointLists",
                                NewValue = YesOrNoString(node.EnableLifecycleManagementForSharePointLists ?? true),
                            });
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.ArchiverGeneralSetting)
                {
                    #region AuditAction.ArchiverGeneralSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Type == ContentSourceType.OneDrive)
                    {
                        info.Action = AuditAction.ArchiverGeneralSetting4OneDrive;
                    }
                    else if (node.Type == ContentSourceType.Teams)
                    {
                        info.Action = AuditAction.ArchiverGeneralSetting4Teams;
                    }

                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    if (node != null)
                    {
                        var setting = ArchiverSettingDao.LoadCurrentNodeArchiverSettingByUrl(node.FullPath, node.Type);
                        if (setting != null)
                        {
                            //bool isFileLevelBackup = false;
                            //if (int.TryParse(_keyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                            //{
                            //    if (outputStreamLevel == (int)OutputStreamLevel.FileLevel)
                            //    {
                            //        isFileLevelBackup = true;
                            //    }
                            //}
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableArchiveManagement", OldValue = YesOrNoString(setting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Enable?true:false) });
                            if (!string.IsNullOrEmpty(setting.CleanRestoredOption) && (_keyValueDao.TryGetBoolValue(RMKeyValuesConstants.EnableDeleteRestoredDataFeature, out var enabled) && enabled))
                            {
                                var option = SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(setting.CleanRestoredOption);

                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableDelDataCheckbox", OldValue = YesOrNoString(option.EnableDelArchivedData) });
                                if (option.EnableDelArchivedData)
                                {
                                    info.ModifyContent.Add(new AuditItem()
                                    {
                                        Id = Guid.NewGuid(),
                                        TargetSetting = "RM_JS_JMD_Grid_Type",
                                        OldValue = option.CleanupAndDelRestoredType switch
                                        {
                                            CleanRestoreOption.None => "RM_RC_Audit_None",
                                            CleanRestoreOption.FileOrVersionOnly => "RM_AR_SPS_General_DelFileAndVersion",
                                            CleanRestoreOption.FileAndReletedVersions => "RM_AR_SPS_General_DelRelatedFileOrVersion",
                                            _ => "RM_RC_Audit_None",
                                        }
                                    });
                                }
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_CleanupRestoreDataDays", OldValue = option.DayNum.ToString() });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableCleanupAllStubsCheckbox", OldValue = YesOrNoString(option.EnableCleanStubs) });
                            }
                        }
                        }
                        #endregion
                    }
                else if (action == (int)AuditAction.EditArchiverSetting)
                {
                    #region AuditAction.EditArchiverSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    Guid groupId = new Guid(node.GetGroupNode().SPObjectId);
                    Guid siteId = Guid.Empty;
                    if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Office365GroupEntire)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        var siteNode = node.GetSiteCollectionNode();
                        siteId = siteNode != null ? new Guid(siteNode.SPObjectId) : Guid.Empty;
                        info.Object = GetFullUrl(node);
                    }

                    ScheduleType scheduleType = ScheduleType.SPArchiveJobSchedule;
                    if (node.Type == ContentSourceType.OneDrive)
                    {
                        info.Action = AuditAction.EditArchiverSetting4OneDrive;
                        scheduleType = ScheduleType.OneDriveArchiveJobSchedule;
                    }
                    else if (node.Type == ContentSourceType.Teams)
                    {
                        info.Action = AuditAction.EditArchiverSetting4Teams;
                        scheduleType = ScheduleType.TeamsArchiveJobSchedule;
                    }

                    var setting = ArchiverSettingsService.LoadSampleNodeSettings(RMDtoConverter.ConvertSPTree2RMSampleTree(RMDtoConverter.ConvertRMTree2SPTree(node)), scheduleType);
                    var oldRuleNames = String.Join("; ", setting?.Rules?.Select(o => o.RuleName) ?? []);
                    AuditHelper.SaveOldAuditItem(info, "RM_JS_SPS_RuleNames_Title", oldRuleNames);

                    var newRuleNames = String.Join("; ", node.Rules?.Select(o => o.RuleName));
                    AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_RuleNames_Title", newRuleNames);

                    AuditHelper.SaveOldAuditItem(info, "RM_AR_SPS_Options_Managed", YesOrNoString(setting.IsManagedMetadataService));
                    AuditHelper.SaveNewAuditItem(info, "RM_AR_SPS_Options_Managed", YesOrNoString(node.IsManagedMetadataService));

                    AuditHelper.SaveOldAuditItem(info, "RM_AR_SPS_Options_SuperUser", YesOrNoString(setting.IsEnableSuperUserDecrypt));
                    AuditHelper.SaveNewAuditItem(info, "RM_AR_SPS_Options_SuperUser", YesOrNoString(node.IsEnableSuperUserDecrypt));

                    AuditHelper.SaveOldAuditItem(info, "RM_AR_SPS_Options_Remove_RetentionLabel", YesOrNoString(setting.IsEnableRemoveRetentionLabel));
                    AuditHelper.SaveNewAuditItem(info, "RM_AR_SPS_Options_Remove_RetentionLabel", YesOrNoString(node.IsEnableRemoveRetentionLabel));

                    // only show locked site setting for web application, group and site collection level
                    if (node.Level == (int)NodeLevel.WebApplication
                        || node.Level == (int)NodeLevel.Office365GroupEntire
                        || node.Level == (int)NodeLevel.SiteCollection
                        )
                    {
                        AuditHelper.SaveOldAuditItem(info, "RM_AR_SPS_Options_SupportLockedSite", YesOrNoString(setting.SupportLockedSite));
                        AuditHelper.SaveNewAuditItem(info, "RM_AR_SPS_Options_SupportLockedSite", YesOrNoString(node.SupportLockedSite));
                    }

                    AuditHelper.SaveOldAuditItem(info, "RM_AR_SPS_Options_SupportArchivedTeams", YesOrNoString(setting.SupportArchivedTeams));
                    AuditHelper.SaveNewAuditItem(info, "RM_AR_SPS_Options_SupportArchivedTeams", YesOrNoString(node.SupportArchivedTeams));

                    #endregion
                }
                else if (action == (int)AuditAction.EditArchiverInheritSetting)
                {
                    #region AuditAction.EditArchiverInheritSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Type == ContentSourceType.OneDrive)
                    {
                        info.Action = AuditAction.ArchiverInheritSetting4OneDrive;
                    }
                    else if (node.Type == ContentSourceType.Teams)
                    {
                        info.Action = AuditAction.ArchiverInheritSetting4Teams;
                    }

                    if (node.Level == (int)NodeLevel.WebApplication || (node.Type == ContentSourceType.Teams && node.Level == (int)NodeLevel.Office365GroupEntire))
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.InheritSubNodeToCurrent) 
                {

                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Type == ContentSourceType.OneDrive)
                    {
                        info.Action = AuditAction.InheritSubNodeToCurrent4OneDrive;
                    }
                    else if (node.Type == ContentSourceType.Teams)
                    {
                        info.Action = AuditAction.InheritSubNodeToCurrent4Teams;
                    }

                    if (node.Level == (int)NodeLevel.WebApplication || (node.Type == ContentSourceType.Teams && node.Level == (int)NodeLevel.Office365GroupEntire))
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                }
                else if (action == (int)AuditAction.SaveCustomMetadataColumn)
                {
                    var newColumns = (List<CustomMetadataColumnInfo>)args[0];
                    var oldColumns = await RMCustomMetadataColumnDao.GetAllCustomMetadataColumnsAsync();
                    var columnAudit = new AuditItem { TargetSetting = "RM_SPS_CustomMetadata_ConfiguredColumns" };
                    var newColumnsString = new StringBuilder();
                    foreach(var newColumn in newColumns)
                    {
                        var auditStr = $"{I18NEntity.GetString("RM_JS_SP_ManageMetadata_ColumnName")}: {newColumn.ColumnName}," +
                            $"{I18NEntity.GetString("RM_JS_SP_ManageMetadata_ColumnType")}: {GetCustomColumnTypeString(newColumn.ColumnType)}," +
                            $"{I18NEntity.GetString("RM_JS_SP_ManageMetadata_SortColumn")}: {I18NEntity.GetString(YesOrNoString(newColumn.EnableSort))}";
                        newColumnsString.AppendLine(auditStr);
                    }

                    var oldColumnsString = new StringBuilder();
                    foreach (var oldColumn in oldColumns)
                    {
                        var auditStr = $"{I18NEntity.GetString("RM_JS_SP_ManageMetadata_ColumnName")}: {oldColumn.ColumnName}," +
                            $"{I18NEntity.GetString("RM_JS_SP_ManageMetadata_ColumnType")}: {GetCustomColumnTypeString(oldColumn.ColumnType)}," +
                            $"{I18NEntity.GetString("RM_JS_SP_ManageMetadata_SortColumn")}: {I18NEntity.GetString(YesOrNoString(oldColumn.EnableSort))}";
                        oldColumnsString.AppendLine(auditStr);
                    }
                    columnAudit.OldValue = oldColumnsString.ToString();
                    columnAudit.NewValue = newColumnsString.ToString();
                    info.ModifyContent.Add(columnAudit);
                    info.Object = string.Empty;
                }
                else if (action == (int)AuditAction.SaveCustomIndexMetadata)
                {
                    info.Object = string.Empty;
                    var newMetadataInfo = (CustomIndexMetadataInfo)args[0];
                    var sourceFlag = (SourceFlag)args[1];
                    var oldMeatadataInfo = await RMCustomIndexMetadataDao.GetCustomIndexMetadatasBySourceFlagAsync(sourceFlag);
                    _ = _keyValueDao.TryGetBoolValue(KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnable);
                    var enableAudit = new AuditItem { TargetSetting = "RM_SPS_CustomMetadata_Enable" };
                    enableAudit.NewValue = I18NEntity.GetString(YesOrNoString(newMetadataInfo.IsEnableCustomIndexMetadata));
                    enableAudit.OldValue = I18NEntity.GetString(YesOrNoString(isEnable));

                    var customMetadatasAudit = new AuditItem { TargetSetting = "RM_SPS_CustomMetadata_ConfiguredMetadatas" };
                    var newMetadataString = new StringBuilder();

                    var sourceColumnName = string.Empty;
                    if (sourceFlag == SourceFlag.SharePoint)
                    {
                        sourceColumnName = I18NEntity.GetString("RM_JS_SP_CustomMetadata_SharePointColumnName");
                    }
                    else if (sourceFlag == SourceFlag.Exchange)
                    {
                        sourceColumnName = I18NEntity.GetString("RM_JS_SP_CustomMetadata_ExchangeColumnName");
                    }

                    if (newMetadataInfo.IsEnableCustomIndexMetadata)
                    {
                        foreach (var metadata in newMetadataInfo.CustomIndexMetadataDtos)
                        {
                            var auditStr = $"{sourceColumnName}: {metadata.SourceColumnName}," +
                                $"{I18NEntity.GetString("RM_JS_SP_CustomMetadata_NameInSearchColumnName")}: {metadata.TargetColumnName}({GetCustomColumnTypeString(metadata.ColumnType)})";
                            newMetadataString.AppendLine(auditStr);
                        }
                    }
                    
                    var oldMetadataString = new StringBuilder();
                    if (isEnable)
                    {
                        var allColumns = await RMCustomMetadataColumnDao.GetAllCustomMetadataColumnsAsync();
                        foreach (var metadata in oldMeatadataInfo)
                        {
                            var column = allColumns.FirstOrDefault(column => column.UniqueId == metadata.TargetColumnId);
                            var columnStr = column == null ? string.Empty : column.ColumnName + $"{column.ColumnName}({GetCustomColumnTypeString(column.ColumnType)})";
                            var auditStr = $"{sourceColumnName}: {metadata.SourceColumnName}, " +
                                $"{I18NEntity.GetString("RM_JS_SP_CustomMetadata_NameInSearchColumnName")}: {columnStr}";
                            oldMetadataString.AppendLine(auditStr);
                        }
                    }
                    
                    customMetadatasAudit.OldValue = oldMetadataString.ToString();
                    customMetadatasAudit.NewValue = newMetadataString.ToString();
                    info.ModifyContent.Add(enableAudit);
                    info.ModifyContent.Add(customMetadatasAudit);
                    info.Object = string.Empty;
                }

                #endregion

                #region EXO Setting

                else if (action == (int)AuditAction.EditEXOLocationOwnersSetting)
                {
                    #region AuditAction.EditLocationOwnersSetting
                    RMEXOTreeNode node = (RMEXOTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        info.Object = ConvertEXODefaultContainerName(node.Name);
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMExchangeOnlineSetting dbSetting = EXOSettingDao.GetColumnInfos(new string[] { node.Id }).FirstOrDefault();
                    var enableApprovalAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_EnableApproval" };
                    var processAudit = new AuditItem { TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName" };
                    var emailAudit = new AuditItem { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation" };
                    var ownerAudit = new AuditItem { TargetSetting = "RM_SPS_RecordOwners" };
                    var autoProcessAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_AutoApprove" };
                    string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                    ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;

                    enableApprovalAudit.NewValue = YesOrNoString(node.ApprovalType != (int)ApprovalType.None);
                    if (node.ApprovalType != (int)ApprovalType.None)
                    {
                        if (node.ApprovalType == (int)ApprovalType.ApprovalProcess)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(new Guid(node.WorkflowReferenceId));
                            if (!string.IsNullOrEmpty(workflow?.Name))
                            {
                                processAudit.NewValue = workflow?.Name;
                            }
                        }
                        else if (node.ApprovalType == (int)ApprovalType.AutoApproval)
                        {
                            autoProcessAudit.NewValue = YesOrNoString(true);
                        }
                        emailAudit.NewValue = YesOrNoString(node.EMailToRecordOwner);
                        ownerAudit.NewValue = ownerNewValue;
                    }

                    if (dbSetting != null)
                    {
                        enableApprovalAudit.OldValue = YesOrNoString(dbSetting.ApprovalType != ApprovalType.None);
                        List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id).Select(a => a.ObjectId).ToList();
                        List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                        ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                        if (dbSetting.ApprovalType != ApprovalType.None)
                        {
                            if (dbSetting.ApprovalType == ApprovalType.ApprovalProcess)
                            {
                                var workflow = ManualProcessManagementService.GetWorkflow(new Guid(dbSetting.WorkflowReferenceId));
                                if (!string.IsNullOrEmpty(workflow?.Name))
                                {
                                    processAudit.OldValue = workflow?.Name;
                                }
                            }
                            else if (dbSetting.ApprovalType == ApprovalType.AutoApproval)
                            {
                                autoProcessAudit.OldValue = YesOrNoString(true);
                            }

                            emailAudit.OldValue = YesOrNoString(dbSetting.EMailToRecordOwner);
                            ownerAudit.OldValue = ownerOldValue;
                        }
                    }
                    info.ModifyContent.Add(enableApprovalAudit);
                    info.ModifyContent.Add(processAudit);
                    info.ModifyContent.Add(ownerAudit);
                    info.ModifyContent.Add(emailAudit);
                    info.ModifyContent.Add(autoProcessAudit);
                    #endregion
                }
                else if (action == (int)AuditAction.EditEXOTermSetting)
                {
                    #region AuditAction.EditDocLevelSetting
                    RMEXOTreeNode node = (RMEXOTreeNode)args[0];
                    var isGroupNode = false;
                    if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        isGroupNode = true;
                        info.Object = ConvertEXODefaultContainerName(node.Name);
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    var newEnableTermSettings = !node.IsNullClassificationSetting;
                    RMExchangeOnlineSetting dbSetting = EXOSettingDao.GetColumnInfos(new string[] { node.Id }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        bool oldApplyExistDocument = false;
                        string newSubsetPath = string.Empty;
                        string oldSubsetPath = string.Empty;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        if (dbSetting.TermId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermId);
                        }
                        else if (dbSetting.TermSetId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dbSetting.TermSetId);
                        }

                        string newPath = string.Empty;
                        string oldPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        if (dbSetting.DefaultTermId != Guid.Empty)
                        {
                            oldPath = TermDao.GetTermNamesPathByTermId(dbSetting.DefaultTermId);
                        }
                        else
                        {
                            oldPath = "RM_SS_NoDefaultValue";
                        }
                        var oldEnableTermSettings = !dbSetting.IsNullClassificationSetting;
                        AuditHelper.SaveAuditItem(info, "RM_JS_SPS_EnableApplyTermSettingsTitle", YesOrNoString(oldEnableTermSettings), YesOrNoString(newEnableTermSettings));

                        if (isGroupNode)
                        {
                            if (!oldEnableTermSettings)
                            {
                                var rules = EXOSettingRuleDao.GetMappingRules(new Guid(node.Id));
                                var oldRuleNames = String.Join("; ", rules?.Select(o => o.RuleName));
                                AuditHelper.SaveOldAuditItem(info, "RM_JS_SPS_RuleNames_Title", oldRuleNames);
                            }
                            if (!newEnableTermSettings)
                            {
                                var newRuleNames = String.Join("; ", node.Rules?.Select(o => o.RuleName));
                                AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_RuleNames_Title", newRuleNames);
                            }
                        }

                        if (oldEnableTermSettings)
                        {
                            AuditHelper.SaveOldAuditItem(info, "RM_SPS_SubsetTerm", oldSubsetPath);
                            AuditHelper.SaveOldAuditItem(info, "RM_SPS_AutoClassification_DeployTermMethod", ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)dbSetting.DeployTermMethod, true));
                            if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                            {
                                AuditHelper.SaveOldAuditItem(info, "RM_SPS_DefaultValue", oldPath);
                                AuditHelper.SaveOldAuditItem(info, "RM_SPS_GS_ApplyExistingDoc", GetApplyExistString(oldApplyExistDocument, dbSetting.ApplyExistType));
                            }
                            if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                            {
                                var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(dbSetting.AutoClassificationRules);
                                AuditHelper.SaveOldAuditItem(info, "RM_JS_SPS_AutoClassification_ApplyPolicy", ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules));
                                AuditHelper.SaveOldAuditItem(info, "RM_SPS_AutoClassification_SkipOverrideOption", ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption));
                                AuditHelper.SaveOldAuditItem(info, "RM_SPS_Auto_RunFullJob", YesOrNoString(dbSetting.RunAutoFullJob));
                            }
                        }

                        if (newEnableTermSettings)
                        {
                            AuditHelper.SaveNewAuditItem(info, "RM_SPS_SubsetTerm", newSubsetPath);
                            AuditHelper.SaveNewAuditItem(info, "RM_SPS_AutoClassification_DeployTermMethod", ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod, true));
                            if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                            {
                                AuditHelper.SaveNewAuditItem(info, "RM_SPS_DefaultValue", newPath);
                                AuditHelper.SaveNewAuditItem(info, "RM_SPS_GS_ApplyExistingDoc", GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType));
                            }
                            if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                            {
                                //AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_AutoClassification_ApplyPolicy", ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules));
                                AuditHelper.SaveNewAuditItem(info, "RM_SPS_AutoClassification_SkipOverrideOption", ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption));
                                AuditHelper.SaveNewAuditItem(info, "RM_SPS_Auto_RunFullJob", YesOrNoString(node.RunAutoFullJob));
                            }
                        }
                    }
                    else
                    {
                        string newSubsetPath = string.Empty;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        string newPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }

                        if (newEnableTermSettings)
                        {
                            AuditHelper.SaveNewAuditItem(info, "RM_SPS_SubsetTerm", newSubsetPath);
                            AuditHelper.SaveNewAuditItem(info, "RM_SPS_AutoClassification_DeployTermMethod", ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod, true));
                            if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                            {
                                AuditHelper.SaveNewAuditItem(info, "RM_SPS_DefaultValue", newPath);
                                AuditHelper.SaveNewAuditItem(info, "RM_SPS_GS_ApplyExistingDoc", GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType));
                            }
                            if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                            {
                                AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_AutoClassification_ApplyPolicy", ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules));
                                AuditHelper.SaveNewAuditItem(info, "RM_SPS_AutoClassification_SkipOverrideOption", ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption));
                                AuditHelper.SaveNewAuditItem(info, "RM_SPS_Auto_RunFullJob", YesOrNoString(node.RunAutoFullJob));
                            }
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditEXOInheritSetting)
                {
                    #region AuditAction.EditInheritSetting
                    RMEXOTreeNode node = (RMEXOTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        info.Object = ConvertEXODefaultContainerName(node.Name);
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.GeneralSetting4EXO)
                {
                    #region AuditAction.GeneralSetting4EXO
                    RMEXOTreeNode node = (RMEXOTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        info.Object = ConvertEXODefaultContainerName(node.Name);
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMExchangeOnlineSetting dbSetting = EXOSettingDao.GetColumnInfos(new string[] { node.Id }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        var oldEnableRecordManagement = dbSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        var newEnableRecordManagement = node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            OldValue = YesOrNoString(oldEnableRecordManagement),
                            NewValue = YesOrNoString(newEnableRecordManagement),
                        });
                        if (oldEnableRecordManagement && newEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                OldValue = YesOrNoString(dbSetting.IsSyncData),
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                        else if (oldEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                OldValue = YesOrNoString(dbSetting.IsSyncData)
                            });
                        }
                        else if (newEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            NewValue = YesOrNoString(node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable),
                        });
                        if (node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                    }

                    #endregion
                }
                #endregion

                #region PR Setting

                else if (action == (int)AuditAction.EditPRLocationOwnersSetting)
                {
                    #region AuditAction.EditPRLocationOwnersSetting
                    RMPRSaveRecordOwnerDto node = (RMPRSaveRecordOwnerDto)args[0];
                    info.Object = GetLocationPath(node.UniqueId);
                    RMPhysicalRecordSetting dbSetting = PhysicalRecordSettingDao.Find(s => s.LocationUniqueId == node.UniqueId);
                    var enableApprovalAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_EnableApproval" };
                    var processAudit = new AuditItem { TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName" };
                    var emailAudit = new AuditItem { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation" };
                    var ownerAudit = new AuditItem { TargetSetting = "RM_SPS_RecordOwners" };
                    var autoProcessAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_AutoApprove" };
                    string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                    ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;

                    enableApprovalAudit.NewValue = YesOrNoString(node.ApprovalType != (int)ApprovalType.None);
                    if (node.ApprovalType != (int)ApprovalType.None)
                    {
                        if (node.ApprovalType == (int)ApprovalType.ApprovalProcess)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(new Guid(node.WorkflowReferenceId));
                            if (!string.IsNullOrEmpty(workflow?.Name))
                            {
                                processAudit.NewValue = workflow?.Name;
                            }
                        }
                        else if (node.ApprovalType == (int)ApprovalType.AutoApproval)
                        {
                            autoProcessAudit.NewValue = YesOrNoString(true);
                        }
                        emailAudit.NewValue = YesOrNoString(node.EMailToRecordOwner);
                        ownerAudit.NewValue = ownerNewValue;
                    }

                    if (dbSetting != null)
                    {
                        enableApprovalAudit.OldValue = YesOrNoString(dbSetting.ApprovalType != ApprovalType.None);
                        List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id).Select(a => a.ObjectId).ToList();
                        List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                        ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                        if (dbSetting.ApprovalType != ApprovalType.None)
                        {
                            if (dbSetting.ApprovalType == ApprovalType.ApprovalProcess)
                            {
                                var workflow = ManualProcessManagementService.GetWorkflow(new Guid(dbSetting.WorkflowReferenceId));
                                if (!string.IsNullOrEmpty(workflow?.Name))
                                {
                                    processAudit.OldValue = workflow?.Name;
                                }
                            }
                            else if (dbSetting.ApprovalType == ApprovalType.AutoApproval)
                            {
                                autoProcessAudit.OldValue = YesOrNoString(true);
                            }

                            emailAudit.OldValue = YesOrNoString(dbSetting.EMailToRecordOwner);
                            ownerAudit.OldValue = ownerOldValue;
                        }
                    }
                    info.ModifyContent.Add(enableApprovalAudit);
                    info.ModifyContent.Add(processAudit);
                    info.ModifyContent.Add(ownerAudit);
                    info.ModifyContent.Add(emailAudit);
                    info.ModifyContent.Add(autoProcessAudit);
                    #endregion
                }
                else if (action == (int)AuditAction.EditPRTermSetting)
                {
                    #region AuditAction.EditDocLevelSetting
                    RMPRSaveTermDto node = (RMPRSaveTermDto)args[0];
                    info.Object = GetLocationPath(node.UniqueId);
                    RMPhysicalRecordSetting dbSetting = PhysicalRecordSettingDao.Find(s => s.LocationUniqueId == node.UniqueId);
                    if (dbSetting != null)
                    {
                        string newSubsetPath = string.Empty;
                        string oldSubsetPath = string.Empty;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        if (dbSetting.TermId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermId);
                        }
                        else if (dbSetting.TermSetId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dbSetting.TermSetId);
                        }

                        string newPath = string.Empty;
                        string oldPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        if (dbSetting.DefaultTermId != Guid.Empty)
                        {
                            oldPath = TermDao.GetTermNamesPathByTermId(dbSetting.DefaultTermId);
                        }
                        else
                        {
                            oldPath = "RM_SS_NoDefaultValue";
                        }
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });

                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)dbSetting.DeployTermMethod, true),
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod, true)
                        });
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                        }
                    }
                    else
                    {
                        string newSubsetPath = string.Empty;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        string newPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }

                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod, true)
                        });
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditPRInheritSetting)
                {
                    #region AuditAction.EditInheritSetting
                    Guid locationUniqueId = (Guid)args[0];
                    var treeNode = new RMPRTreeNode
                    {
                        UniqueId = locationUniqueId
                    };
                    info.Object = GetLocationPath(locationUniqueId);
                    var dbLocation = LocationDao.GetLocationByUniqueId(locationUniqueId, false);
                    bool isTopLevelLocation;
                    Guid topLevelLocationUniqueId;
                    List<string> locationDirPathIds;
                    CheckIsTopLevelSetting(dbLocation.DirPath, out isTopLevelLocation, out topLevelLocationUniqueId, out locationDirPathIds);
                    treeNode.IsTopLevelSetting = isTopLevelLocation;
                    var currentNode = PhysicalRecordSettingDao.GetPhysicalRecordSetting(locationUniqueId);
                    var parentNode = PhysicalRecordSettingDao.GetAncestryPhysicalRecordSetting(locationDirPathIds);


                    string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                    if (parentNode != null && currentNode != null)
                    {

                        #region term set
                        string newSubsetPath = string.Empty;
                        string oldSubsetPath = string.Empty;
                        if (parentNode.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(parentNode.TermId);
                        }
                        else if (parentNode.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(parentNode.TermSetId);
                        }

                        if (currentNode.TermId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermNamesPathByTermId(currentNode.TermId);
                        }
                        else if (currentNode.TermSetId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(currentNode.TermSetId);
                        }

                        string newPath = string.Empty;
                        string oldPath = string.Empty;
                        if (parentNode.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(parentNode.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        if (currentNode.DefaultTermId != Guid.Empty)
                        {
                            oldPath = TermDao.GetTermNamesPathByTermId(currentNode.DefaultTermId);
                        }
                        else
                        {
                            oldPath = "RM_SS_NoDefaultValue";
                        }
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });

                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)currentNode.DeployTermMethod, true),
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)parentNode.DeployTermMethod, true)
                        });
                        if ((DeployTermMethod)currentNode.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                        }
                        if (parentNode.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                        }
                        #endregion 

                    }

                    #endregion
                }
                #endregion

                #region SP-OnPrem Setting Auditor
                if (action == (int)AuditAction.EditSPOnPremColumnSetting)
                {
                    #region AuditAction.EditSPOnPremColumnSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointOnPremiseSetting dbSetting = SharePointOnPremiseSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ColumnName",
                            OldValue = dbSetting.IsUsingExistColumnName ? GetExistingColumnWords(dbSetting.ExistColumnName, dbSetting.SetDocLevelTermForExistColumn) : dbSetting.ColumnName,
                            NewValue = node.IsUsingExistColumnName ? GetExistingColumnWords(node.ExistColumnName, node.SetDocLevelTermForExistColumn) : node.ColumnName,
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_DocumentDes",
                            OldValue = dbSetting.IsUsingExistColumnName ? "" : dbSetting.Description,
                            NewValue = node.IsUsingExistColumnName ? "" : node.Description
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_DisplayColumnRequired",
                            OldValue = dbSetting.IsUsingExistColumnName ? "" : YesOrNoString(dbSetting.ColumnRequired),
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnRequired)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_EditKey_ShowUniqueID",
                            OldValue = YesOrNoString(dbSetting.IsShowUniqueId),
                            NewValue = YesOrNoString(node.IsShowUniqueId)
                        });
                        //Enable App
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                           info.ModifyContent.Add(new AuditItem()
                           {
                               TargetSetting = "RM_SP_SettingRelatedRecords",
                               OldValue = YesOrNoString(dbSetting.EnableRelatedRecords),
                               NewValue = YesOrNoString(node.EnableRelatedRecords)
                           });
                        }
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ColumnName",
                            NewValue = node.IsUsingExistColumnName ? node.ExistColumnName : node.ColumnName
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_DocumentDes",
                            NewValue = node.IsUsingExistColumnName ? "" : node.Description
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_DisplayColumnRequired",
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnRequired)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_EditKey_ShowUniqueID",
                            NewValue = YesOrNoString(node.IsShowUniqueId)
                        });
                        //Enable App
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                           info.ModifyContent.Add(new AuditItem()
                           {
                               TargetSetting = "RM_SP_SettingRelatedRecords",
                               NewValue = YesOrNoString(node.EnableRelatedRecords)
                           });
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditSPOnPremLocationOwnersSetting)
                {
                    #region AuditAction.EditSPOnPremLocationOwnersSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointOnPremiseSetting dbSetting = SharePointOnPremiseSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    var enableApprovalAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_EnableApproval" };
                    var processAudit = new AuditItem { TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName" };
                    var emailAudit = new AuditItem { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation" };
                    var ownerAudit = new AuditItem { TargetSetting = "RM_SPS_RecordOwners" };
                    var autoProcessAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_AutoApprove"};
                    string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                    ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;

                    enableApprovalAudit.NewValue = YesOrNoString(node.ApprovalType != (int)ApprovalType.None);
                    if (node.ApprovalType != (int)ApprovalType.None)
                    {
                        if (node.ApprovalType == (int)ApprovalType.ApprovalProcess)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(new Guid(node.WorkflowReferenceId));
                            if (!string.IsNullOrEmpty(workflow?.Name))
                            {
                                processAudit.NewValue = workflow?.Name;
                            }
                        }else if(node.ApprovalType == (int)ApprovalType.AutoApproval)
                        {
                            autoProcessAudit.NewValue = YesOrNoString(true);
                        }
                        emailAudit.NewValue = YesOrNoString(node.EMailToRecordOwner);
                        ownerAudit.NewValue = ownerNewValue;
                    }

                    if (dbSetting != null)
                    {
                        enableApprovalAudit.OldValue = YesOrNoString(dbSetting.ApprovalType != ApprovalType.None);
                        List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id).Select(a => a.ObjectId).ToList();
                        List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                        ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                        if (dbSetting.ApprovalType != ApprovalType.None)
                        {
                            if (dbSetting.ApprovalType == ApprovalType.ApprovalProcess)
                            {
                                var workflow = ManualProcessManagementService.GetWorkflow(new Guid(dbSetting.WorkflowReferenceId));
                                if (!string.IsNullOrEmpty(workflow?.Name))
                                {
                                    processAudit.OldValue = workflow?.Name;
                                }
                            }else if(dbSetting.ApprovalType == ApprovalType.AutoApproval)
                            {
                                autoProcessAudit.OldValue = YesOrNoString(true);
                            }

                            emailAudit.OldValue = YesOrNoString(dbSetting.EMailToRecordOwner);
                            ownerAudit.OldValue = ownerOldValue;
                        }
                    }
                    info.ModifyContent.Add(enableApprovalAudit);
                    info.ModifyContent.Add(processAudit);
                    info.ModifyContent.Add(ownerAudit);
                    info.ModifyContent.Add(emailAudit);
                    info.ModifyContent.Add(autoProcessAudit);
                    #endregion
                }
                else if (action == (int)AuditAction.EditSPOnPremDocLevelSetting)
                {
                    #region AuditAction.EditSPOnPremDocLevelSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointOnPremiseSetting dbSetting = SharePointOnPremiseSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        bool oldApplyExistDocument = false;
                        string newSubsetPath = string.Empty;
                        string oldSubsetPath = string.Empty;
                        oldApplyExistDocument = dbSetting.NeedCheckDefaultValue;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }
                        if (dbSetting.TermId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermId);
                        }
                        else if (dbSetting.TermSetId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dbSetting.TermSetId);
                        }
                        string newPath = string.Empty;
                        string oldPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        if (dbSetting.DefaultTermId != Guid.Empty)
                        {
                            oldPath = TermDao.GetTermNamesPathByTermId(dbSetting.DefaultTermId);
                        }
                        else
                        {
                            oldPath = "RM_SS_NoDefaultValue";
                        }
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                            OldValue = dbSetting.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel",
                            NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)dbSetting.DeployTermMethod),
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod)
                        });
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, dbSetting.ApplyExistType, dbSetting.IncludeDeclaredRecords) });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType, node.IncludeDeclaredRecords) });
                        }
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(dbSetting.AutoClassificationRules);
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(dbSetting.IncludeDeclaredRecords) });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.Empty,
                                TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                            });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                        }
                        //Enable App
                        if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                        {
                           info.ModifyContent.Add(new AuditItem()
                           {
                               Id = Guid.NewGuid(),
                               TargetSetting = "RM_SP_SettingRelatedRecords",
                               OldValue = YesOrNoString(dbSetting.EnableRelatedRecords),
                               NewValue = YesOrNoString(node.EnableRelatedRecords)
                           });
                        }
                    }
                    else
                    {
                        string newSubsetPath = string.Empty;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        string newPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                            NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                        });

                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod)
                        });
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType, node.IncludeDeclaredRecords) });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.Empty,
                                TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                            });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                        }
                        //Enable App
                        if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                        {
                           info.ModifyContent.Add(new AuditItem()
                           {
                               Id = Guid.NewGuid(),
                               TargetSetting = "RM_SP_SettingRelatedRecords",
                               NewValue = YesOrNoString(node.EnableRelatedRecords)
                           });
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditSPOnPremConLevelSetting)
                {
                    #region AuditAction.EditSPOnPremConLevelSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointOnPremiseSetting dbSetting = SharePointOnPremiseSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    string oldContainerPath = string.Empty, newContainerPath = string.Empty;
                    string oldContainerDes = string.Empty;
                    if (dbSetting != null)
                    {
                        oldContainerDes = dbSetting.DescriptionOfContainer;
                        if (dbSetting.TermIdOfContainer != Guid.Empty)
                        {
                            oldContainerPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermIdOfContainer);
                        }
                        if (node.TermIdOfContainer != Guid.Empty)
                        {
                            newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                        }
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_TermOfContainer", NewValue = newContainerPath, OldValue = oldContainerPath });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_DescriptionOfContainer", NewValue = node.DescriptionOfContainer, OldValue = oldContainerDes });
                    }
                    else
                    {
                        if (node.TermIdOfContainer != Guid.Empty)
                        {
                            newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                        }
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_TermOfContainer", NewValue = newContainerPath });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_DescriptionOfContainer", NewValue = node.DescriptionOfContainer });
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditSPOnPremInheritSetting)
                {
                    #region AuditAction.EditSPOnPremInheritSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.GeneralSetting4SPOnPrem)
                {
                    #region AuditAction.GeneralSetting4SPOnPrem
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMSharePointOnPremiseSetting dbSetting = SharePointOnPremiseSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        var oldEnableRecordManagement = dbSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        var newEnableRecordManagement = node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            OldValue = YesOrNoString(oldEnableRecordManagement),
                            NewValue = YesOrNoString(newEnableRecordManagement),
                        });
                        if (oldEnableRecordManagement && newEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                OldValue = YesOrNoString(dbSetting.IsSyncData),
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                        else if (oldEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                OldValue = YesOrNoString(dbSetting.IsSyncData)
                            });
                        }
                        else if (newEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            NewValue = YesOrNoString(node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable),
                        });
                        if (node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                    }
                    #endregion
                }
                #endregion

                #region One Drive Setting Auditor
                else if (action == (int)AuditAction.EditOneDriveLocationOwnersSetting)
                {
                    #region AuditAction.EditOneDriveLocationOwnersSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMOneDriveSetting dbSetting = OneDriveSettingDao.GetSettingsByIds(new string[] { node.SPObjectId }).FirstOrDefault();
                    var enableApprovalAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_EnableApproval" };
                    var processAudit = new AuditItem { TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName" };
                    var emailAudit = new AuditItem { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation" };
                    var ownerAudit = new AuditItem { TargetSetting = "RM_SPS_RecordOwners" };
                    var autoProcessAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_AutoApprove"};
                    string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                    ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;

                    enableApprovalAudit.NewValue = YesOrNoString(node.ApprovalType != (int)ApprovalType.None);
                    if (node.ApprovalType != (int)ApprovalType.None)
                    {
                        if (node.ApprovalType == (int)ApprovalType.ApprovalProcess)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(new Guid(node.WorkflowReferenceId));
                            if (!string.IsNullOrEmpty(workflow?.Name))
                            {
                                processAudit.NewValue = workflow?.Name;
                            }
                        }else if(node.ApprovalType == (int)ApprovalType.AutoApproval)
                        {
                            autoProcessAudit.NewValue = YesOrNoString(true);
                        }
                        emailAudit.NewValue = YesOrNoString(node.EMailToRecordOwner);
                        ownerAudit.NewValue = ownerNewValue;
                    }

                    if (dbSetting != null)
                    {
                        enableApprovalAudit.OldValue = YesOrNoString(dbSetting.ApprovalType != ApprovalType.None);
                        List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id).Select(a => a.ObjectId).ToList();
                        List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                        ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                        if (dbSetting.ApprovalType != ApprovalType.None)
                        {
                            if (dbSetting.ApprovalType == ApprovalType.ApprovalProcess)
                            {
                                var workflow = ManualProcessManagementService.GetWorkflow(new Guid(dbSetting.WorkflowReferenceId));
                                if (!string.IsNullOrEmpty(workflow?.Name))
                                {
                                    processAudit.OldValue = workflow?.Name;
                                }
                            }else if(dbSetting.ApprovalType == ApprovalType.AutoApproval)
                            {
                                autoProcessAudit.OldValue = YesOrNoString(true);
                            }               

                            emailAudit.OldValue = YesOrNoString(dbSetting.EMailToRecordOwner);
                            ownerAudit.OldValue = ownerOldValue;
                        }
                    }
                    info.ModifyContent.Add(enableApprovalAudit);
                    info.ModifyContent.Add(processAudit);
                    info.ModifyContent.Add(ownerAudit);
                    info.ModifyContent.Add(emailAudit);
                    info.ModifyContent.Add(autoProcessAudit);
                    #endregion
                }
                else if (action == (int)AuditAction.EditOneDriveTermSetting)
                {
                    #region AuditAction.EditOneDriveTermSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    Guid groupId = new Guid(node.GetGroupNode().SPObjectId);
                    Guid siteId = Guid.Empty;
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        siteId = new Guid(node.GetSiteCollectionNode().SPObjectId);
                        info.Object = GetFullUrl(node);
                    }
                    RMOneDriveSetting dbSetting = OneDriveSettingDao.GetSettingsByIds(new string[] { node.SPObjectId }).FirstOrDefault();
                    var newEnableTermSettings = !node.IsNullClassificationSetting;
                    if (dbSetting != null)
                    {
                        bool oldApplyExistDocument = false;
                        string newSubsetPath = string.Empty;
                        string oldSubsetPath = string.Empty;
                        oldApplyExistDocument = dbSetting.NeedCheckDefaultValue;
                        var oldEnableTermSettings = !dbSetting.IsNullClassificationSetting;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }
                        if (dbSetting.TermId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermId);
                        }
                        else if (dbSetting.TermSetId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dbSetting.TermSetId);
                        }
                        string newPath = string.Empty;
                        string oldPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        if (dbSetting.DefaultTermId != Guid.Empty)
                        {
                            oldPath = TermDao.GetTermNamesPathByTermId(dbSetting.DefaultTermId);
                        }
                        else
                        {
                            oldPath = "RM_SS_NoDefaultValue";
                        }

                        AuditHelper.SaveAuditItem(info, "RM_JS_SPS_EnableApplyTermSettingsTitle", YesOrNoString(oldEnableTermSettings), YesOrNoString(newEnableTermSettings));
                        if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.SiteCollection)
                        {
                            if (!oldEnableTermSettings)
                            {
                                var rules = EXOSettingRuleDao.GetOneDriveMappingRules(groupId, siteId);
                                var oldRuleNames = String.Join("; ", rules?.Select(o => o.RuleName));
                                AuditHelper.SaveOldAuditItem(info, "RM_JS_SPS_RuleNames_Title", oldRuleNames);
                            }
                            if (!newEnableTermSettings)
                            {
                                var newRuleNames = String.Join("; ", node.Rules?.Select(o => o.RuleName));
                                AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_RuleNames_Title", newRuleNames);
                            }

                        }
                        if (oldEnableTermSettings)
                        {
                            AuditHelper.SaveOldAuditItem(info, "RM_SPS_SubsetTerm", oldSubsetPath);
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                                OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)dbSetting.DeployTermMethod),
                            });
                            if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, dbSetting.ApplyExistType, dbSetting.IncludeDeclaredRecords) });
                            }
                            if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                            {
                                var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(dbSetting.AutoClassificationRules);
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(dbSetting.IncludeDeclaredRecords) });
                            }
                            if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || dbSetting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                            {
                                var aiDefaultTermOldPath = string.Empty;
                                if (dbSetting.AIThenDefaultTermId != Guid.Empty)
                                {
                                    aiDefaultTermOldPath = TermDao.GetTermNamesPathByTermId(dbSetting.AIThenDefaultTermId);
                                }
                                else
                                {
                                    aiDefaultTermOldPath = "RM_SS_NoDefaultValue";
                                }
                                List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id, RecordOwnerSettingType.AIOneDrive).Select(a => a.ObjectId).ToList();
                                List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                                var aiReviewersOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", OldValue = aiReviewersOldValue });
                                if (dbSetting.AIApprovalType != ApprovalType.None)
                                {
                                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", OldValue = YesOrNoString(dbSetting.AISendEMail) });
                                }
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", OldValue = aiDefaultTermOldPath });
                            }
                            if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(dbSetting.IncludeDeclaredRecords) });
                            }
                        }

                        if (newEnableTermSettings)
                        {
                            AuditHelper.SaveNewAuditItem(info, "RM_SPS_SubsetTerm", newSubsetPath);
                            AuditHelper.SaveNewAuditItem(info, "RM_SPS_AutoClassification_DeployTermMethod", ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod));
                            if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType, node.IncludeDeclaredRecords) });
                            }
                            if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                            {
                                AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_AutoClassification_ApplyPolicy", ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules));
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                            }
                            if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || node.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                            {
                                var aiDefaultTermNewPath = string.Empty;
                                if (node.AIThenDefaultTermId != Guid.Empty)
                                {
                                    aiDefaultTermNewPath = TermDao.GetTermNamesPathByTermId(node.AIThenDefaultTermId);
                                }
                                else
                                {
                                    aiDefaultTermNewPath = "RM_SS_NoDefaultValue";
                                }
                                var aiReviewersNewValue = node.AIReviewers.Count > 0 ? string.Join(";", node.AIReviewers.Select(a => a.DisplayName)) : string.Empty;
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", NewValue = aiReviewersNewValue });
                                if (node.AIApprovalType != (int)ApprovalType.None)
                                {
                                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", NewValue = YesOrNoString(node.AISendEMail) });
                                }
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", NewValue = aiDefaultTermNewPath });
                            }
                            if (node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                            }
                        }
                    }
                    else
                    {
                        string newSubsetPath = string.Empty;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        string newPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        AuditHelper.SaveAuditItem(info, "RM_JS_SPS_EnableApplyTermSettingsTitle", YesOrNoString(node.Level == (int)NodeLevel.WebApplication ? false : true), YesOrNoString(newEnableTermSettings));
                        if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.SiteCollection)
                        {
                            if (!newEnableTermSettings)
                            {
                                var newRuleNames = String.Join("; ", node.Rules?.Select(o => o.RuleName));
                                AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_RuleNames_Title", newRuleNames);
                            }
                        }
                        if (newEnableTermSettings)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });
                            //info.ModifyContent.Add(new AuditItem()
                            //{
                            //Id = Guid.NewGuid(),
                            //TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                            //NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                            //});
                            AuditHelper.SaveNewAuditItem(info, "RM_SPS_AutoClassification_DeployTermMethod", ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod));
                            if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType, node.IncludeDeclaredRecords) });
                            }
                            if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                            {
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    Id = Guid.Empty,
                                    TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                    NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                                });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                            }
                            if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || node.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                            {
                                var aiDefaultTermNewPath = string.Empty;
                                if (node.AIThenDefaultTermId != Guid.Empty)
                                {
                                    aiDefaultTermNewPath = TermDao.GetTermNamesPathByTermId(node.AIThenDefaultTermId);
                                }
                                else
                                {
                                    aiDefaultTermNewPath = "RM_SS_NoDefaultValue";
                                }
                                var aiReviewersNewValue = node.AIReviewers.Count > 0 ? string.Join(";", node.AIReviewers.Select(a => a.DisplayName)) : string.Empty;
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", NewValue = aiReviewersNewValue });
                                if (node.AIApprovalType != (int)ApprovalType.None)
                                {
                                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", NewValue = YesOrNoString(node.AISendEMail) });
                                }
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", NewValue = aiDefaultTermNewPath });
                            }
                            if (node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                            }
                        }
                        //Enable App
                        //if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                        //{
                        //    info.ModifyContent.Add(new AuditItem()
                        //    {
                        //        Id = Guid.NewGuid(),
                        //        TargetSetting = "RM_SP_SettingRelatedRecords",
                        //        NewValue = YesOrNoString(node.EnableRelatedRecords)
                        //    });
                        //}
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditOneDriveInheritSetting)
                {
                    #region AuditAction.EditOneDriveInheritSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.GeneralSetting4OneDrive)
                {
                    #region AuditAction.GeneralSetting4OneDrive
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMOneDriveSetting dbSetting = OneDriveSettingDao.GetSettingsByIds(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        var oldEnableRecordManagement = dbSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        var newEnableRecordManagement = node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            OldValue = YesOrNoString(oldEnableRecordManagement),
                            NewValue = YesOrNoString(newEnableRecordManagement),
                        });
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                            if (oldEnableRecordManagement && newEnableRecordManagement)
                            {
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_JS_SPS_EditKey_ShowUniqueID",
                                    OldValue = YesOrNoString(dbSetting.IsShowUniqueId == null ? false : (bool)dbSetting.IsShowUniqueId),
                                    NewValue = YesOrNoString(node.IsShowUniqueId)
                                });
                            }
                            else if (oldEnableRecordManagement)
                            {
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_JS_SPS_EditKey_ShowUniqueID",
                                    OldValue = YesOrNoString(dbSetting.IsShowUniqueId == null ? false : (bool)dbSetting.IsShowUniqueId)
                                });
                            }
                            else if (newEnableRecordManagement)
                            {
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_JS_SPS_EditKey_ShowUniqueID",
                                    NewValue = YesOrNoString(node.IsShowUniqueId)
                                });
                            }
                        }
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            NewValue = YesOrNoString(node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable),
                        });
                        if ((node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable) && (node.Level == (int)NodeLevel.WebApplication))
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_JS_SPS_EditKey_ShowUniqueID",
                                NewValue = YesOrNoString(node.IsShowUniqueId)
                            });
                        }
                    }
                    #endregion
                }
                #endregion

                #region TeamsSetting

                else if (action == (int)AuditAction.EditTeamsColumnSetting)
                {
                    #region AuditAction.EditColumnSetting4Teams
                    var isCSDTenant = TenantService.IsCSDTenant();
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMTeamsSetting dbSetting = TeamsSettingDao.GetColumnInfos([node.SPObjectId]).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ColumnName",
                            OldValue = dbSetting.IsUsingExistColumnName ? GetExistingColumnWords4Teams(dbSetting.ExistColumnName, dbSetting.SetDocLevelTermForExistColumn) : dbSetting.ColumnName,
                            NewValue = node.IsUsingExistColumnName ? GetExistingColumnWords4Teams(node.ExistColumnName, node.SetDocLevelTermForExistColumn) : node.ColumnName,
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_DocumentDes",
                            OldValue = dbSetting.IsUsingExistColumnName ? "" : dbSetting.Description,
                            NewValue = node.IsUsingExistColumnName ? "" : node.Description
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_DisplayColumnRequired",
                            OldValue = dbSetting.IsUsingExistColumnName ? "" : YesOrNoString(dbSetting.ColumnRequired == null ? true : (bool)dbSetting.ColumnRequired),
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnRequired)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_Teams_HiddenColumn",
                            OldValue = dbSetting.IsUsingExistColumnName ? "" : YesOrNoString(dbSetting.ColumnHidden == null ? false : (bool)dbSetting.ColumnHidden),
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnHidden)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_Teams_EditKey_ShowUniqueID",
                            OldValue = YesOrNoString(dbSetting.IsShowUniqueId == null ? true : (bool)dbSetting.IsShowUniqueId),
                            NewValue = YesOrNoString(node.IsShowUniqueId)
                        });
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem()
                                {
                                    TargetSetting = "RM_JS_SPS_Teams_EditKey_KeepSPDefaultValue",
                                    OldValue = GetKeepDefaultValueYesOrNoString4Teams(dbSetting.IsKeepSharePointDefaultValue, dbSetting.SetTermForEmptyDefaultValue),
                                    NewValue = GetKeepDefaultValueYesOrNoString4Teams(node.IsKeepSharePointDefaultValue, node.SetTermForEmptyDefaultValue)
                                });
                            }

                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                OldValue = YesOrNoString(dbSetting.EnableRelatedRecords),
                                NewValue = YesOrNoString(node.EnableRelatedRecords)
                            });
                        }
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ColumnName",
                            NewValue = node.IsUsingExistColumnName ? node.ExistColumnName : node.ColumnName
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_DocumentDes",
                            NewValue = node.IsUsingExistColumnName ? "" : node.Description
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_DisplayColumnRequired",
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnRequired)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_Teams_HiddenColumn",//stodo
                            NewValue = node.IsUsingExistColumnName ? "" : YesOrNoString(node.ColumnHidden)
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_Teams_EditKey_ShowUniqueID",
                            NewValue = YesOrNoString(node.IsShowUniqueId)
                        });
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                NewValue = YesOrNoString(node.EnableRelatedRecords)
                            });
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditTeamsLocationOwnersSetting)
                {
                    #region AuditAction.EditTeamsLocationOwnersSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Office365GroupEntire)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMTeamsSetting dbSetting = TeamsSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    var enableApprovalAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_EnableApproval" };
                    var processAudit = new AuditItem { TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName" };
                    var emailAudit = new AuditItem { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation" };
                    var ownerAudit = new AuditItem { TargetSetting = "RM_SPS_RecordOwners" };
                    var autoProcessAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_AutoApprove" };
                    string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                    ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;

                    enableApprovalAudit.NewValue = YesOrNoString(node.ApprovalType != (int)ApprovalType.None);
                    if (node.ApprovalType != (int)ApprovalType.None)
                    {
                        if (node.ApprovalType == (int)ApprovalType.ApprovalProcess)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(new Guid(node.WorkflowReferenceId));
                            if (!string.IsNullOrEmpty(workflow?.Name))
                            {
                                processAudit.NewValue = workflow?.Name;
                            }
                        }
                        else if (node.ApprovalType == (int)ApprovalType.AutoApproval)
                        {
                            autoProcessAudit.NewValue = YesOrNoString(true);
                        }
                        emailAudit.NewValue = YesOrNoString(node.EMailToRecordOwner);
                        ownerAudit.NewValue = ownerNewValue;
                    }

                    if (dbSetting != null)
                    {
                        enableApprovalAudit.OldValue = YesOrNoString(dbSetting.ApprovalType != ApprovalType.None);
                        List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id).Select(a => a.ObjectId).ToList();
                        List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                        ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                        if (dbSetting.ApprovalType != ApprovalType.None)
                        {
                            if (dbSetting.ApprovalType == ApprovalType.ApprovalProcess)
                            {
                                var workflow = ManualProcessManagementService.GetWorkflow(new Guid(dbSetting.WorkflowReferenceId));
                                if (!string.IsNullOrEmpty(workflow?.Name))
                                {
                                    processAudit.OldValue = workflow?.Name;
                                }
                            }
                            else if (dbSetting.ApprovalType == ApprovalType.AutoApproval)
                            {
                                autoProcessAudit.OldValue = YesOrNoString(true);
                            }

                            emailAudit.OldValue = YesOrNoString(dbSetting.EMailToRecordOwner);
                            ownerAudit.OldValue = ownerOldValue;
                        }
                    }
                    info.ModifyContent.Add(enableApprovalAudit);
                    info.ModifyContent.Add(processAudit);
                    info.ModifyContent.Add(ownerAudit);
                    info.ModifyContent.Add(emailAudit);
                    info.ModifyContent.Add(autoProcessAudit);
                    #endregion
                }
                else if (action == (int)AuditAction.EditTeamsDocLevelSetting)
                {
                    #region AuditAction.EditTeamsDocLevelSetting
                    var isCSDTenant = TenantService.IsCSDTenant();
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Office365GroupEntire)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMTeamsSetting dbSetting = TeamsSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        bool oldApplyExistDocument = false;
                        string newSubsetPath = string.Empty;
                        string oldSubsetPath = string.Empty;
                        oldApplyExistDocument = dbSetting.NeedCheckDefaultValue;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        if (dbSetting.TermId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermId);
                        }
                        else if (dbSetting.TermSetId != Guid.Empty)
                        {
                            oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dbSetting.TermSetId);
                        }

                        string newPath = string.Empty;
                        string oldPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }
                        if (dbSetting.DefaultTermId != Guid.Empty)
                        {
                            oldPath = TermDao.GetTermNamesPathByTermId(dbSetting.DefaultTermId);
                        }
                        else
                        {
                            oldPath = "RM_SS_NoDefaultValue";
                        }
                        //if (node.Level != (int)NodeLevel.WebApplication)
                        //{
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });
                        //}
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                            OldValue = dbSetting.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel",
                            NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                        });

                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)dbSetting.DeployTermMethod),
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod)
                        });
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, dbSetting.ApplyExistType, dbSetting.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() 
                                { 
                                    Id = Guid.NewGuid(), 
                                    TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder",
                                    OldValue = GetApplyExistString(dbSetting.IsApplyTermIncludeFolder(), dbSetting.ApplyExistType, false)
                                });
                            }
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType, node.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", NewValue = GetApplyExistString(node.ApplyTermIncludeFolder, node.ApplyExistType, false) });
                            }
                        }
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(dbSetting.AutoClassificationRules);
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(dbSetting.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() 
                                { 
                                    Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder",
                                    OldValue = YesOrNoString(dbSetting.IsApplyTermIncludeFolder()) 
                                });
                            }
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                                TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                            });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", NewValue = YesOrNoString(node.ApplyTermIncludeFolder) });
                            }
                        }

                        if (IsEnableApplySettingAlwaysScanAll())
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_SPS_Auto_AlwaysRunFullJob",
                                OldValue = YesOrNoString(dbSetting.AlwaysScanAllExistDocuments),
                                NewValue = YesOrNoString(node.AlwaysScanAllExistDocuments)
                            });
                        }


                        if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || node.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                        {
                            var aiDefaultTermNewPath = string.Empty;
                            if (node.AIThenDefaultTermId != Guid.Empty)
                            {
                                aiDefaultTermNewPath = TermDao.GetTermNamesPathByTermId(node.AIThenDefaultTermId);
                            }
                            else
                            {
                                aiDefaultTermNewPath = "RM_SS_NoDefaultValue";
                            }
                            var aiReviewersNewValue = node.AIReviewers.Count > 0 ? string.Join(";", node.AIReviewers.Select(a => a.DisplayName)) : string.Empty;
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", NewValue = aiReviewersNewValue });
                            if (node.AIApprovalType != (int)ApprovalType.None)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", NewValue = YesOrNoString(node.AISendEMail) });
                            }
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", NewValue = aiDefaultTermNewPath });
                        }

                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || dbSetting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                        {
                            var aiDefaultTermOldPath = string.Empty;
                            if (dbSetting.AIThenDefaultTermId != Guid.Empty)
                            {
                                aiDefaultTermOldPath = TermDao.GetTermNamesPathByTermId(dbSetting.AIThenDefaultTermId);
                            }
                            else
                            {
                                aiDefaultTermOldPath = "RM_SS_NoDefaultValue";
                            }
                            List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id, RecordOwnerSettingType.AISharePointOnline).Select(a => a.ObjectId).ToList();
                            List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                            var aiReviewersOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", OldValue = aiReviewersOldValue });
                            if (dbSetting.AIApprovalType != ApprovalType.None)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", OldValue = YesOrNoString(dbSetting.AISendEMail) });
                            }
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", OldValue = aiDefaultTermOldPath });
                        }

                        if (node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                        }

                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(dbSetting.IncludeDeclaredRecords) });
                        }

                        if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                OldValue = YesOrNoString(dbSetting.EnableRelatedRecords),
                                NewValue = YesOrNoString(node.EnableRelatedRecords)
                            });
                        }
                    }
                    else
                    {

                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "", OldValue = "RM_JS_TM_inherBreak" });

                        string newSubsetPath = string.Empty;
                        if (node.TermId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                        }
                        else if (node.TermSetId != Guid.Empty)
                        {
                            newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                        }

                        string newPath = string.Empty;
                        if (node.DefaultTermId != Guid.Empty)
                        {
                            newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                        }
                        else
                        {
                            newPath = "RM_SS_NoDefaultValue";
                        }

                        //if (node.Level != (int)NodeLevel.WebApplication)
                        //{
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });
                        //}
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                            NewValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                        });

                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod)
                        });
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType, node.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", NewValue = GetApplyExistString(node.ApplyTermIncludeFolder, node.ApplyExistType, false) });
                            }
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                                TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                            });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                            if (!isCSDTenant)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDSetAndFolder", NewValue = YesOrNoString(node.ApplyTermIncludeFolder) });
                            }
                        }

                        if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification || node.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                        {
                            var aiDefaultTermNewPath = string.Empty;
                            if (node.AIThenDefaultTermId != Guid.Empty)
                            {
                                aiDefaultTermNewPath = TermDao.GetTermNamesPathByTermId(node.AIThenDefaultTermId);
                            }
                            else
                            {
                                aiDefaultTermNewPath = "RM_SS_NoDefaultValue";
                            }
                            var aiReviewersNewValue = node.AIReviewers.Count > 0 ? string.Join(";", node.AIReviewers.Select(a => a.DisplayName)) : string.Empty;
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", NewValue = aiReviewersNewValue });
                            if (node.AIApprovalType != (int)ApprovalType.None)
                            {
                                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", NewValue = YesOrNoString(node.AISendEMail) });
                            }
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceDefaultTerm", NewValue = aiDefaultTermNewPath });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification)
                        {
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(node.IncludeDeclaredRecords) });
                        }

                        if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_SP_SettingRelatedRecords",
                                NewValue = YesOrNoString(node.EnableRelatedRecords)
                            });
                        }
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.GeneralSetting4Teams)
                {
                    #region AuditAction.GeneralSetting4Teams
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Office365GroupEntire)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMTeamsSetting dbSetting = TeamsSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    if (dbSetting != null)
                    {
                        var oldEnableRecordManagement = dbSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        var newEnableRecordManagement = node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            OldValue = YesOrNoString(oldEnableRecordManagement),
                            NewValue = YesOrNoString(newEnableRecordManagement),
                        });
                        if (oldEnableRecordManagement && newEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                OldValue = YesOrNoString(dbSetting.IsSyncData),
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                        else if (oldEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                OldValue = YesOrNoString(dbSetting.IsSyncData)
                            });
                        }
                        else if (newEnableRecordManagement)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                        }
                        RMSPTreeNode oldSPTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(dbSetting.NodeInfo);
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_SupportLockedSite",
                            OldValue = YesOrNoString(oldSPTreeNode.SupportLockedSite),
                            NewValue = YesOrNoString(node.SupportLockedSite),
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_EnableLifecycleManagementForSharePointLists",
                            OldValue = YesOrNoString(oldSPTreeNode.EnableLifecycleManagementForSharePointLists ?? true),
                            NewValue = YesOrNoString(node.EnableLifecycleManagementForSharePointLists ?? true),
                        });
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_GS_ManagedScope",
                            NewValue = YesOrNoString(node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable),
                        });
                        if (node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                                NewValue = YesOrNoString(node.IsSyncData)
                            });
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_JS_SPS_SupportLockedSite",
                                NewValue = YesOrNoString(node.SupportLockedSite),
                            });
                        }
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_JS_SPS_EnableLifecycleManagementForSharePointLists",
                            NewValue = YesOrNoString(node.EnableLifecycleManagementForSharePointLists ?? true),
                        });
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditTeamsConLevelSetting)
                {
                    #region AuditAction.EditTeamsConLevelSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Office365GroupEntire)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    RMTeamsSetting dbSetting = TeamsSettingDao.GetColumnInfos(new string[] { node.SPObjectId }).FirstOrDefault();
                    string oldContainerPath = string.Empty, newContainerPath = string.Empty;
                    string oldContainerDes = string.Empty;
                    bool oldInheritParent = false;
                    bool oldMarkPhysical = false;
                    if (dbSetting != null)
                    {
                        oldMarkPhysical = dbSetting.IsEnableHoldPhyical;
                        oldContainerDes = dbSetting.DescriptionOfContainer;
                        oldInheritParent = dbSetting.IsInheritParentTerm;
                        if (dbSetting.TermIdOfContainer != Guid.Empty)
                        {
                            oldContainerPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermIdOfContainer);
                        }
                        if (node.TermIdOfContainer != Guid.Empty)
                        {
                            newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                        }
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_TermOfContainer", NewValue = newContainerPath, OldValue = oldContainerPath });
                        //i18n TODO
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_DescriptionOfContainer", NewValue = node.DescriptionOfContainer, OldValue = oldContainerDes });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_InheritParentTerm",
                            NewValue = node.IsInheritParentTerm ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"),
                            OldValue = oldInheritParent ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No")});
                    }
                    else
                    {
                        if (node.TermIdOfContainer != Guid.Empty)
                        {
                            newContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                        }
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_TermOfContainer", NewValue = newContainerPath });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_DescriptionOfContainer", NewValue = node.DescriptionOfContainer });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_InheritParentTerm", NewValue = node.IsInheritParentTerm ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No") });
                    }
                    #endregion
                }
                else if (action == (int)AuditAction.EditTeamsInheritSetting)
                {
                    #region AuditAction.EditTeamsInheritSetting
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Office365GroupEntire)
                    {
                        info.Object = node.Name;
                    }
                    else
                    {
                        info.Object = GetFullUrl(node);
                    }
                    #endregion
                }

                #endregion
            }
            catch (Exception e)
            {
                logger.Warn("SharePoint setting before Audit handler,message detail {0}", e.ToString());
            }

            return info;
        }

        private string ConvertEXODefaultContainerName(string containerName)
        {
            if (containerName == "Default_ Mailbox_ Group")
            {
                containerName = I18NEntity.GetString("RM_EXO_Default_Container");
            }
            return containerName;
        }

        private bool IsEnableApplySettingAlwaysScanAll()
        {
            var key = _keyValueDao.GetValueByKey(RMKeyValuesConstants.EnableApplySettingAlwaysScanAll);
            _ = bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private void CheckIsTopLevelSetting(string locationDirPath, out bool isTopLevelLocation, out Guid topLevelLocationUniqueId, out List<string> locationIds)
        {
            isTopLevelLocation = false;
            topLevelLocationUniqueId = default(Guid);
            locationIds = new List<string>();
            if (!string.IsNullOrEmpty(locationDirPath))
            {
                locationIds = locationDirPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (locationIds.Count > 0)
                {
                    if (locationIds.Count == 1)
                    {
                        isTopLevelLocation = true;
                    }
                    else
                    {
                        isTopLevelLocation = false;
                        //DirPath --> "1/2/3/"
                        //1 is root
                        //2 is topLevelSetting
                        var topLevelLocation = LocationDao.GetLocationById(Convert.ToInt32(locationIds[1]));
                        topLevelLocationUniqueId = topLevelLocation.UniqueId;
                    }
                }
            }
        }

        private string GetLocationPath(Guid uniqueId)
        {
            var locationPath = string.Empty;
            var tempLocation = LocationDao.GetLocationByUniqueId(uniqueId);
            if (tempLocation != null)
            {
                locationPath = string.Format($"{tempLocation.PathForDisplay}/{tempLocation.Name}");
            }

            return locationPath;
        }

        private string GetFullUrl(RMSPTreeNode node)
        {
            string fullUrl = node.FullPath;
            if (fullUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return fullUrl;
            }
            string siteUrl = string.Empty;
            string returnUrl = string.Empty;
            RMSPTreeNode treeNode = new RMSPTreeNode() { Level = node.Level, Parent = node.Parent };
            while (treeNode.Level != (int)NodeLevel.SiteCollection && treeNode.Level != (int)NodeLevel.Site)
            {
                treeNode = treeNode.Parent;
                if (treeNode == null)
                {
                    break;
                }
                siteUrl = treeNode.FullPath;
            }
            if (string.IsNullOrEmpty(siteUrl))
            {
                returnUrl = fullUrl;
            }
            else
            {
                //returnUrl = siteUrl + @"/" + node.Name;
                //error when list or folder
                returnUrl = WebUtil.MakeFullUrl(siteUrl, node.FullPath);
            }
            return returnUrl;
        }

        private string GetFullUrl(RMEXOTreeNode node)
        {
            return node.Name;
            //return node.FullPath;
        }

        private string GetExistingColumnWords(string columnName, bool setDocLevelTermForExistColumn)
        {
            string result = columnName + " " + "RM_JS_SPS_ExistingColumn";
            if (setDocLevelTermForExistColumn)
            {
                result = string.Format(result, "RM_JS_SPS_UseTermSettingsDefinedInRecords");
            }
            else
            {
                result = string.Format(I18NEntity.GetString("RM_JS_SPS_ExistingColumn"), columnName);
            }
            return result;
        }

        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }

        private string GetKeepDefaultValueYesOrNoString(bool isKeepSharePointDefaultValue, bool setTermForEmptyDefaultValueOfSP)
        {
            if (isKeepSharePointDefaultValue && setTermForEmptyDefaultValueOfSP)
            {
                return $"RM_JS_Common_Yes; RM_SPS_NoSetTermForEmptyDefaultValue_Title ";
            }
            return isKeepSharePointDefaultValue? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }

        private string GetApplyExistString(bool applyExistDocument, int applyExistType, bool includeIncludeDeclaredRecords)
        {
            if (applyExistDocument)
            {
                var includeString = "; " + "RM_JS_SPS_IncludeDeclaredRecords ";
                if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.OverWrite)
                {
                    return "RM_JS_Common_Yes" + "; " + "RM_JS_SPS_AutoClassification_ApplyOverwirteTerm " + (includeIncludeDeclaredRecords ? includeString : "");
                }
                else if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.SkipAndKeep)
                {
                    return "RM_JS_Common_Yes" + "; " + "RM_JS_SPS_AutoClassification_ApplySkipTerm " + (includeIncludeDeclaredRecords ? includeString : "");
                }
                else
                {
                    return "RM_JS_Common_Yes";
                }
            }
            else
            {
                return "RM_JS_Common_No";
            }
        }

        private string GetApplyExistString(bool applyExistDocument, int applyExistType)
        {
            if (applyExistDocument)
            {
                if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.OverWrite)
                {
                    return "RM_JS_Common_Yes" + "; " + "RM_JS_SPS_AutoClassification_ApplyOverwirteTerm ";
                }
                else if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.SkipAndKeep)
                {
                    return "RM_JS_Common_Yes" + "; " + "RM_JS_SPS_AutoClassification_ApplySkipTerm ";
                }
                else
                {
                    return "RM_JS_Common_Yes";
                }
            }
            else
            {
                return "RM_JS_Common_No";
            }
        }

        private string GetExistingColumnWords4Teams(string columnName, bool setDocLevelTermForExistColumn)
        {
            string result = columnName + " " + I18NEntity.GetString("RM_JS_SPS_ExistingColumn");
            if (setDocLevelTermForExistColumn)
            {
                result = string.Format(result, "RM_JS_SPS_UseTermSettingsDefinedInRecords ");
            }
            else
            {
                result = string.Format(result, "RM_JS_SPS_Teams_UseTermSettingsDefinedInTeams ");
            }
            return result;
        }

        private string GetKeepDefaultValueYesOrNoString4Teams(bool isKeepSharePointDefaultValue, bool setTermForEmptyDefaultValueOfSP)
        {
            if (isKeepSharePointDefaultValue && setTermForEmptyDefaultValueOfSP)
            {
                return $"RM_JS_Common_Yes; RM_SPS_Teams_NoSetTermForEmptyDefaultValue_Title ";
            }
            return isKeepSharePointDefaultValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }

        private string GetCustomColumnTypeString(CustomColumnType type)
        {
            return type switch
            {
                CustomColumnType.SingleText => I18NEntity.GetString("RM_JS_SP_ManageMetadata_ColumnType_Text"),
                CustomColumnType.YesOrNo => I18NEntity.GetString("RM_JS_SP_ManageMetadata_ColumnType_YesOrNo"),
                CustomColumnType.DateTime => I18NEntity.GetString("RM_JS_SP_ManageMetadata_ColumnType_DateTime"),
                CustomColumnType.Number => I18NEntity.GetString("RM_JS_SP_ManageMetadata_ColumnType_Number"),
                _ => string.Empty,
            };
        }
    }
}
