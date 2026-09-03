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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using RATeams;

namespace AvePoint.RA.Service.Services.TermManagement.AuditHandler
{

    public class TermManagementBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(TermManagementBeforeAuditHandler));
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {

            string termFullPath = string.Empty;
            var info = new RMAuditInfo();
            string termNameOld = string.Empty;
            string termDescriptionOld = string.Empty;
            string termGroupNameOld = string.Empty;
            string termSetNameOld = string.Empty;
            string termId = string.Empty;
            string termAdvancedSettingsOld = string.Empty;
            TermAuditInfo termInfo = new TermAuditInfo();
            string strResource_RetirementSetting = "RM_TM_ExpirDate";
            string strResource_NoExpirDate = "RM_TM_NoExpirDate";
            string strResource_BreakInheritance = "RM_JS_TM_inher";
            string strResource_TermPermanent = "RM_TM_TermMarkAsPermanent";
            string strResource_STime = "RM_TM_STime";
            string strResource_ETime = "RM_TM_ETime";
            string strResource_FTime = "RM_TM_FTime";
            string strResource_ToTime = "RM_TM_ToTime";
            string strResource_Rule = "RM_TM_TermRuleLabel";
            string strResource_Enforce = "RM_TM_EnforceRetention";
            string strResource_TermDescription = "RM_TM_TermDescription";
            string strResource_SharePoint = "RM_TM_Retension_SharePoint";
            string strResource_Exchange = "RM_TM_Retension_Exchange";
            string i18n_Exchange_Label = "RM_TM_Retension_Exchange_Label";
            string i18n_SP_Label = "RM_TM_Retension_SP_Label";
            string strResource_OneDrive = "RM_TM_Retension_OneDrive";
            string i18n_OneDrive_Label = "RM_TM_Retension_OneDrive_Label";
            string term_Advanced_Settings = "RM_TM_AdvanceSetting";
            string strResource_Teams = "RM_TM_Retention_Teams";
            string i18n_Teams_Label = "RM_TM_Retention_Teams_Label";

            bool hasUpgradeTeams = TeamsPermissionHelper.HasUpgradeTeamsFeature();

            try
            {
                switch (action)
                {
                    case (int)AuditAction.RenameTerm:
                        int renametermId = Convert.ToInt32(args[0]);
                        termNameOld = TaxonomyService.GetTermNameByTermId(renametermId);
                        termFullPath = TaxonomyService.GetTermNamesPathByTermId(renametermId);
                        break;
                    case (int)AuditAction.ConfigureTermGeneralSetting:
                        termId = args[0] as TermSettingsInfo == null ? args[0].ToString() : ((TermSettingsInfo)args[0]).tId.ToString();
                        termDescriptionOld = TaxonomyService.GetTermDescriptionByTermId(Convert.ToInt32(termId));
                        termAdvancedSettingsOld = TaxonomyService.GetTermAdvancedSettingsByTermId(Convert.ToInt32(termId));
                        break;
                    case (int)AuditAction.RenameTermGroup:
                        int termGroupId = Convert.ToInt32(args[0]);
                        termGroupNameOld = TaxonomyService.GetTermGroupNameById(termGroupId);
                        break;
                    case (int)AuditAction.RenameTermSet:
                        int termSetId = Convert.ToInt32(args[0]);
                        termSetNameOld = TaxonomyService.GetTermSetNameById(termSetId);
                        break;
                }

                if (info.ModifyContent == null) { info.ModifyContent = new List<AuditItem>(); }

                if (action == (int)AuditAction.RenameTerm)
                {
                    AuditItem auditItem = new AuditItem();
                    auditItem.OldValue = !string.IsNullOrEmpty(termNameOld) ? termNameOld : string.Empty;
                    info.ModifyContent.Add(auditItem);
                }
                else if (action == (int)AuditAction.RenameTermGroup)
                {
                    AuditItem auditItem = new AuditItem();
                    auditItem.OldValue = !string.IsNullOrEmpty(termGroupNameOld) ? termGroupNameOld : string.Empty;
                    info.ModifyContent.Add(auditItem);
                }
                else if (action == (int)AuditAction.RenameTermSet)
                {
                    AuditItem auditItem = new AuditItem();
                    auditItem.OldValue = !string.IsNullOrEmpty(termSetNameOld) ? termSetNameOld : string.Empty;
                    info.ModifyContent.Add(auditItem);
                }
                else if (action == (int)AuditAction.ConfigureTermSetSetting)
                {
                    var desc = TaxonomyService.GetTermSetDescByTermSetId(args[0].ToString());
                    AuditItem auditItem = new AuditItem();
                    auditItem.TargetSetting = "RM_TM_TermGroup_Desciption";
                    auditItem.OldValue = desc;
                    info.ModifyContent.Add(auditItem);
                }
                else if (action == (int)AuditAction.ConfigureTermGroupSetting)
                {
                    int termGroupId = Convert.ToInt32(args[0]);
                    TermGroupAuditInfo termGroupAuditInfo = TaxonomyService.GetTermGroupInfoById(termGroupId);

                    AuditItem auditItem = new AuditItem();
                    auditItem.TargetSetting = "RM_TM_TermGroup_Desciption";
                    auditItem.OldValue = termGroupAuditInfo.Description;
                    info.ModifyContent.Add(auditItem);


                    AuditItem auditItem1 = new AuditItem();
                    auditItem1.TargetSetting = "RM_TM_TermGroup_MMSChoose";
                    auditItem1.OldValue = termGroupAuditInfo.M365TermSyncOption switch
                    {
                        "Specified" => string.Format("{0}", termGroupAuditInfo.UsingpecificMMSSMessage),
                        "All" => string.Format("{0}", termGroupAuditInfo.UsingAllMMSSMessage),
                        _ => string.Format("{0}", termGroupAuditInfo.UsingNoneMMSSMessage)
                    };
                    info.ModifyContent.Add(auditItem1);
                    if (LicenseHelperService.HasOpusGoogleLicense)
                    {
                        AuditItem auditItem2 = new AuditItem();
                        auditItem2.TargetSetting = "RM_TM_TermGroup_GoogleChoose";
                        auditItem2.OldValue = termGroupAuditInfo.GoogleTermSyncOption switch
                        {
                            "Specified" => $"{termGroupAuditInfo.UsingSpecificGoogleMessage}",
                            "All" => $"{termGroupAuditInfo.UsingAllGoogleMessage}",
                            _ => $"{termGroupAuditInfo.UsingNoneGoogleMessage}"
                        };
                        info.ModifyContent.Add(auditItem2);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(termId))
                    {
                        termInfo = await TaxonomyService.GetTermRuleInfosByTermIdAsync(Convert.ToInt32(termId));
                        string strBeginTime = termInfo.BeginTime;
                        string strEndTime = termInfo.EndTime;

                        string breakString = false.ToString();
                        if (!termInfo.IsRootTerm)
                        {
                            breakString = termInfo.IsBreakInheritance.ToString();
                        }
                        
                        AuditItem auditItemBrekInher = new AuditItem();
                        auditItemBrekInher.TargetSetting = strResource_BreakInheritance;
                        auditItemBrekInher.OldValue = breakString;
                        info.ModifyContent.Add(auditItemBrekInher);

                        //AuditItem auditItemPermanent = new AuditItem();
                        //auditItemPermanent.TargetSetting = strResource_TermPermanent;
                        //auditItemPermanent.OldValue = termInfo.Permanent.ToString();
                        //info.ModifyContent.Add(auditItemPermanent);

                        AuditItem auditItemDisposalRule = new AuditItem();
                        auditItemDisposalRule.TargetSetting = strResource_Rule;
                        auditItemDisposalRule.OldValue = termInfo.RuleNames;
                        info.ModifyContent.Add(auditItemDisposalRule);


                        AuditItem auditItemEnforceRetention = new AuditItem();
                        auditItemEnforceRetention.TargetSetting = strResource_Enforce;
                        auditItemEnforceRetention.OldValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention);
                        info.ModifyContent.Add(auditItemEnforceRetention);

                        #region 增加Retension:SharePoint,Exchange,Label
                        if(hasUpgradeTeams)
                        {
                            AuditItem retentionTeams = new AuditItem()
                            {
                                TargetSetting = strResource_Teams,
                                OldValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention, Level.Teams)
                            };
                            info.ModifyContent.Add(retentionTeams);
                        }
                        AuditItem retentionSharePoint = new AuditItem()
                        {
                            TargetSetting = strResource_SharePoint,
                            OldValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention,Level.SharePoint)
                        };
                        info.ModifyContent.Add(retentionSharePoint);
                        AuditItem retensionExchange = new AuditItem()
                        {
                            TargetSetting = strResource_Exchange,
                            OldValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention,Level.Exchange)
                        };
                        info.ModifyContent.Add(retensionExchange);
                        AuditItem retensionOneDrive = new AuditItem()
                        {
                            TargetSetting = strResource_OneDrive,
                            OldValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention, Level.OneDrive)
                        };
                        info.ModifyContent.Add(retensionOneDrive);
                        if(hasUpgradeTeams)
                        {
                            AuditItem retensionTeamsLabel = new AuditItem()
                            {
                                TargetSetting = i18n_Teams_Label,
                                OldValue = termInfo.TeamsLabel
                            };
                            info.ModifyContent.Add(retensionTeamsLabel);
                        }
                        AuditItem retensionLabel = new AuditItem()
                        {
                            TargetSetting = i18n_Exchange_Label,
                            OldValue = termInfo.ExchangeLabel
                        };
                        info.ModifyContent.Add(retensionLabel);
                        AuditItem retensionspLabel = new AuditItem()
                        {
                            TargetSetting = i18n_SP_Label,
                            OldValue = termInfo.SPLabel
                        };
                        info.ModifyContent.Add(retensionspLabel);

                        AuditItem retensionOneDriveLabel = new AuditItem()
                        {
                            TargetSetting = i18n_OneDrive_Label,
                            OldValue = termInfo.OneDriveLabel
                        };
                        info.ModifyContent.Add(retensionOneDriveLabel);
                        #endregion

                        AuditItem auditItemExpriDate = new AuditItem();
                        auditItemExpriDate.TargetSetting = strResource_RetirementSetting;

                        if (!string.IsNullOrEmpty(strBeginTime) && !string.IsNullOrEmpty(strEndTime))
                        {
                            auditItemExpriDate.OldValue = string.Format("{0} {2} {1} {3}", strResource_FTime, strResource_ToTime, strBeginTime, strEndTime);
                            info.ModifyContent.Add(auditItemExpriDate);
                        }
                        if (string.IsNullOrEmpty(strBeginTime) && string.IsNullOrEmpty(strEndTime))
                        {
                            auditItemExpriDate.OldValue = string.Format("{0}", strResource_NoExpirDate);
                            info.ModifyContent.Add(auditItemExpriDate);
                        }
                        if (string.IsNullOrEmpty(strBeginTime) && !string.IsNullOrEmpty(strEndTime))
                        {
                            auditItemExpriDate.OldValue = string.Format("{0} {1}", strResource_ETime, strEndTime);
                            info.ModifyContent.Add(auditItemExpriDate);
                        }
                        if (!string.IsNullOrEmpty(strBeginTime) && string.IsNullOrEmpty(strEndTime))
                        {
                            auditItemExpriDate.OldValue = string.Format("{0} {1}", strResource_STime, strBeginTime);
                            info.ModifyContent.Add(auditItemExpriDate);
                        }
                        AuditItem termDescription = new AuditItem();
                        termDescription.TargetSetting = strResource_TermDescription;
                        termDescription.OldValue = termDescriptionOld;
                        info.ModifyContent.Add(termDescription);

                        AuditItem termAdvancedSettings = new AuditItem();
                        termAdvancedSettings.TargetSetting = term_Advanced_Settings;
                        termAdvancedSettings.OldValue = termAdvancedSettingsOld;
                        info.ModifyContent.Add(termAdvancedSettings);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            return info;
        }
    }
}
