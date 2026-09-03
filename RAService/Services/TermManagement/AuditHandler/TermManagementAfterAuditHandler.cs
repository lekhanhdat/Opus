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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.RMRuleManageMent;
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
    public class TermManagementAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(TermManagementAfterAuditHandler));
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();


        public async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            try
            {
                RMAuditInfo auditInfo = new RMAuditInfo();
                string termFullPath = string.Empty;
                string newTermName = string.Empty;
                string newTermGroupName = string.Empty;
                string newTermSetName = string.Empty;
                string newTermDescription = string.Empty;
                string termId = string.Empty;
                bool pathWithGroupName = true;
                string newTermAdvancedSettings = string.Empty;
                TermAuditInfo termInfo = new TermAuditInfo();
                string strResource_RetirementSetting = "RM_TM_ExpirDate";
                string strResource_TermPermanent = "RM_TM_TermMarkAsPermanent";
                string strResource_NoExpirDate = "RM_TM_NoExpirDate";
                string strResource_BreakInheritance = "RM_JS_TM_inher";
                string strResource_STime = "RM_TM_STime";
                string strResource_ETime = "RM_TM_ETime";
                string strResource_FTime = "RM_TM_FTime";
                string strResource_ToTime = "RM_TM_ToTime";
                string strResource_Rule = "RM_TM_TermRuleLabel";
                string strResource_Enforce = "RM_TM_EnforceRetention";
                string strResource_SharePoint = "RM_TM_Retension_SharePoint";
                string strResource_Exchange = "RM_TM_Retension_Exchange";
                string i18n_Exchange_Label = "RM_TM_Retension_Exchange_Label";
                string i18n_Sp_Label = "RM_TM_Retension_SP_Label";
                string strResource_OneDrive = "RM_TM_Retension_OneDrive";
                string i18n_OneDrive_Label = "RM_TM_Retension_OneDrive_Label";
                string term_Advanced_Settings = "RM_TM_AdvanceSetting";
                string strResource_Teams= "RM_TM_Retention_Teams";
                string i18n_Teams_Label = "RM_TM_Retention_Teams_Label";

                bool hasUpgradeTeams = TeamsPermissionHelper.HasUpgradeTeamsFeature();

                TermSettingsInfo tInfo = null;
                switch (action)
                {
                    case (int)AuditAction.CreateTerm:
                        if (!string.IsNullOrEmpty(returnValue.ToString()))
                        {
                            var termDto = args[0] as TermInfo;
                            var parentPath = "";
                            if (termDto.ParentTermId == 0)
                            {
                                parentPath = TaxonomyService.GetGermSetNamesPathByTermSetId(termDto.TermSetId);
                            }
                            else { 
                                parentPath = TaxonomyService.GetTermNamesPathByTermId(termDto.ParentTermId);
                            }
                            termFullPath = $"{parentPath}/{termDto.TermName}";
                        }
                        break;

                    case (int)AuditAction.DeleteTerm:
                        int deltermId = Convert.ToInt32(args[0]);
                        termFullPath = TaxonomyService.GetTermNamesPathByTermId(deltermId);
                        break;
                   
                    case (int)AuditAction.DeprecateTerm:
                        int depretermId = Convert.ToInt32(args[0]);
                        termFullPath = TaxonomyService.GetTermNamesPathByTermId(depretermId);
                        break;
                    case (int)AuditAction.EnableTerm:
                        int enabletermId = Convert.ToInt32(args[0]);
                        termFullPath = TaxonomyService.GetTermNamesPathByTermId(enabletermId);
                        break;
                    case (int)AuditAction.RenameTerm:
                        int renameTermId = Convert.ToInt32(args[0]);
                        newTermName = args[1].ToString();
                        pathWithGroupName = (action == (int)AuditAction.RenameTerm ? true : false);
                        termFullPath = TaxonomyService.GetTermNamesPathByTermId(renameTermId);
                        break;
                    case (int)AuditAction.RenameTermGroup:
                        newTermGroupName = args[1].ToString();
                        int termGroupIdInt;
                        if (int.TryParse(args[0].ToString(), out termGroupIdInt))
                        {
                            termFullPath = TaxonomyService.GetTermGroupNameById(termGroupIdInt);
                        }
                        break;
                    case (int)AuditAction.RenameTermSet:
                    case (int)AuditAction.RenameLocationTermSet:
                        newTermSetName = args[1].ToString();
                        int termSetIdInt;
                        if (int.TryParse(args[0].ToString(), out termSetIdInt))
                        {
                            termFullPath = TaxonomyService.GetGermSetNamesPathByTermSetId(termSetIdInt);
                        }
                        break;
                    case (int)AuditAction.ConfigureTermSetSetting:
                        termId = args[0].ToString();
                        termFullPath = TaxonomyService.GetGermSetNamesPathByTermSetId(Convert.ToInt32(termId));
                        break;
                    case (int)AuditAction.ConfigureTermGeneralSetting:
                        tInfo = args[0] as TermSettingsInfo == null ? (TermSettingsInfo)args[1] : (TermSettingsInfo)args[0];
                        termId = tInfo.tId.ToString();
                        newTermDescription = tInfo.des != null ? tInfo.des.ToString() : string.Empty;
                        newTermAdvancedSettings = tInfo.advanceSettings != null ? tInfo.advanceSettings.ToString() : string.Empty;
                        termFullPath = TaxonomyService.GetTermNamesPathByTermId(Convert.ToInt32(termId));
                        break;
                    case (int)AuditAction.ConfigureTermGroupSetting:
                        termFullPath = args[1].ToString();
                        break;
                    case (int)AuditAction.DeleteTermGroup:
                        Guid termGroupIdGuid = Guid.Empty;
                        if (Guid.TryParse(args[0].ToString(), out termGroupIdGuid))
                        {
                            termFullPath = TaxonomyService.GetTermGroupNameById(termGroupIdGuid);
                        }
                        break;
                    case (int)AuditAction.CreateTermGroup:
                        if(args[0] is TermInfo dto)
                        {
                            termFullPath = dto.TermGroupName;
                        }else
                        {
                            termFullPath = args[0].ToString();
                        }
                        break;
                    case (int)AuditAction.ImportTerm:
                        termFullPath = returnValue as string;
                        break;
                    case (int)AuditAction.ImportGoogleTerm:
                        termFullPath = returnValue as string;
                        break;
                    case (int)AuditAction.PhysicalItemImportReport:
                        termFullPath = returnValue as string;
                        break;
                    case (int)AuditAction.DeleteRootTerms:
                        int delTermSetId = Convert.ToInt32(args[0]);
                        termFullPath = TaxonomyService.GetGermSetNamesPathByTermSetId(delTermSetId);
                        break;
                    case (int)AuditAction.CreateTermSet:
                        var termSetName = args[0].ToString();
                        if (Guid.TryParse(args[1].ToString(), out termGroupIdGuid))
                        {
                            var termGroupPath = TaxonomyService.GetTermGroupNameById(termGroupIdGuid);
                            termFullPath = $"{termGroupPath}/{termSetName}";
                        }
                        break;
                }
                auditInfo.Object = termFullPath;
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                if (action == (int)AuditAction.ConfigureTermGeneralSetting)
                {
                    bool flag = true;
                    if (string.IsNullOrEmpty(returnValue.ToString()) || returnValue.ToString().Length == 1)
                    {
                        flag = false;
                    }
                    auditInfo.Status = !flag ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
                }
                else if (action == (int)AuditAction.ExportTerm)
                {
                    string filePath = args[0].ToString();
                    auditInfo.Status = !string.IsNullOrEmpty(filePath) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }
                else if (action == (int)AuditAction.ConfigureTermGroupSetting)
                {
                    var returnResult = (RAReturnMessage)returnValue;
                    auditInfo.Status = returnResult.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }
                else if(action == (int)AuditAction.DownloadTemplate)
                {
                    auditInfo.Object = "File Plan Import Template.xlsx";
                }
                else if (action == (int)AuditAction.ImportGoogleTerm)
                {
                    var termGroupNameAndGoogleTenants =
                        await TaxonomyService.GetTermGroupNameAndGoogleTenantsAsync(Guid.Parse(args[2].ToString()!));
                    AuditItem auditItem = new AuditItem
                    {
                        TargetSetting = "RM_JS_TM_SelectTermGroup",
                        NewValue = termGroupNameAndGoogleTenants.First().Key,
                    };
                    auditInfo.ModifyContent = [auditItem];
                    AuditItem auditItem1 = new AuditItem
                    {
                        TargetSetting = "RM_TM_TermGroup_GoogleChoose",
                        NewValue = string.Join(";\n ", termGroupNameAndGoogleTenants.First().Value),
                    };
                    auditInfo.ModifyContent.Add(auditItem1);
                    auditInfo.Status = string.IsNullOrEmpty(returnValue.ToString()) ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
                }
                else if (action == (int)AuditAction.AIRecommendation)
                {
                    var aiRecommendation  = args[0] as AIRecomentdation;
                    List<AuditItem> auditItem = new List<AuditItem>
                    {
                        new AuditItem
                        {
                            TargetSetting = "RM_RC_Audit_AI_Industry",
                            NewValue = aiRecommendation.Industry,
                        },
                        new AuditItem
                        {
                            TargetSetting = "RM_RC_Audit_AI_Country",
                            NewValue = aiRecommendation.Country,
                        },
                        new AuditItem
                        {
                            TargetSetting = "RM_RC_Audit_AI_Requirement",
                            NewValue = aiRecommendation.Requirement,
                        },
                        new AuditItem
                        {
                            TargetSetting = "RM_RC_Audit_AI_FileName",
                            NewValue = aiRecommendation.FileName,
                        },
                    };
                    auditInfo.ModifyContent = auditItem;
                    var returnResult = (RAReturnMessage)returnValue;
                    auditInfo.Status = returnResult.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }
                else
                {
                    auditInfo.Status = string.IsNullOrEmpty(returnValue.ToString()) ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
                }

                if (info != null && info.ModifyContent != null && info.ModifyContent.Count != 0)
                {
                    if (action == (int)AuditAction.RenameTerm || action == (int)AuditAction.RenameLocationTerm)
                    {
                        AuditItem renameTermItem = info.ModifyContent.FirstOrDefault();
                        if (renameTermItem != null)
                        {
                            renameTermItem.NewValue = newTermName;
                        }
                    }
                    else if (action == (int)AuditAction.RenameTermGroup)
                    {
                        AuditItem renameTermGroupItem = info.ModifyContent.FirstOrDefault();
                        if (renameTermGroupItem != null)
                        {
                            renameTermGroupItem.NewValue = newTermGroupName;
                        }
                    }
                    else if (action == (int)AuditAction.RenameTermSet || action == (int)AuditAction.RenameLocationTermSet)
                    {
                        AuditItem renameTermSetItem = info.ModifyContent.FirstOrDefault();
                        if (renameTermSetItem != null)
                        {
                            renameTermSetItem.NewValue = newTermSetName;
                        }
                    }
                    else if (action == (int)AuditAction.ConfigureTermGroupSetting)
                    {

                        AuditItem auditItem = new AuditItem();
                        auditItem.TargetSetting = "RM_TM_TermGroup_Desciption";
                        auditItem.NewValue = args[2].ToString();
                        info.ModifyContent.Add(auditItem);


                        AuditItem auditItem1 = new AuditItem();
                        auditItem1.TargetSetting = "RM_TM_TermGroup_MMSChoose";
                        if (int.TryParse(args[5].ToString(), out var m365TermSyncoption))
                        {
                            switch (m365TermSyncoption)
                            {
                                case 1:
                                {
                                    List<RMSiteInfo> siteInfos = (List<RMSiteInfo>)args[3];
                                    auditItem1.NewValue = string.Join("\n", siteInfos.Where(a => !a.Action.Equals(SiteAction.Delete) && a.SiteType != SiteType.Google).Select(a => a.SiteUrl));
                                    break;
                                }
                                case 2:
                                    auditItem1.NewValue = $"{"RM_TM_AllMMS"}";
                                    break;
                                default:
                                    auditItem1.NewValue = $"{"RM_JS_Common_None"}";
                                    break;
                            }
                        }
                        info.ModifyContent.Add(auditItem1);
                        if (LicenseHelperService.HasOpusGoogleLicense)
                        {
                            AuditItem auditItem2 = new AuditItem();
                            auditItem2.TargetSetting = "RM_TM_TermGroup_GoogleChoose";
                            if (int.TryParse(args[6].ToString(), out var googleTermSyncOption))
                            {
                                switch (googleTermSyncOption)
                                {
                                    case 1:
                                    {
                                        List<RMSiteInfo> siteInfos = (List<RMSiteInfo>)args[3];
                                        auditItem2.NewValue = string.Join("\n", siteInfos.Where(a => !a.Action.Equals(SiteAction.Delete) && a.SiteType == SiteType.Google).Select(a => a.DisplayName));
                                        break;
                                    }
                                    case 2:
                                        auditItem2.NewValue = $"{"RM_TM_AllMMS"}";
                                        break;
                                    default:
                                        auditItem2.NewValue = $"{"RM_JS_Common_None"}";
                                        break;
                                }
                                info.ModifyContent.Add(auditItem2);
                            }
                        }
                    }
                    else if (action == (int)AuditAction.ConfigLocationTermSettings)
                    {
                        AuditItem auditItem = new AuditItem();
                        auditItem.TargetSetting = "RM_LM_LocationSettingTotalSpace".TrimEnd(':');
                        double space = 0;
                        double.TryParse(args[1].ToString(), out space);
                        if (Convert.ToInt32(space) != 0)
                        {
                            space = Math.Round(space, 2);
                        }
                        auditItem.NewValue = space.ToString();
                        info.ModifyContent.Add(auditItem);
                    }
                    else if (action == (int)AuditAction.ConfigureTermSetSetting)
                    {
                        AuditItem auditItem = new AuditItem();
                        auditItem.TargetSetting = "RM_TM_TermGroup_Desciption";
                        auditItem.NewValue = args[2].ToString();
                        info.ModifyContent.Add(auditItem);
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
                            AuditItem BrekInher = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_BreakInheritance)).FirstOrDefault();
                            if (BrekInher != null) { BrekInher.NewValue = breakString; }

                            //AuditItem Permanent = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_TermPermanent)).FirstOrDefault();
                            //if (Permanent != null) { Permanent.NewValue = termInfo.Permanent.ToString(); }


                            AuditItem DisposalRule = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_Rule)).FirstOrDefault();
                            if (DisposalRule != null) { DisposalRule.NewValue = termInfo.RuleNames; }

                            #region retention Setting
                            AuditItem retention = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_Enforce)).FirstOrDefault();
                            if (retention != null) { retention.NewValue =AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention); }

                            if(hasUpgradeTeams)
                            {
                                AuditItem retensionTeams = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_Teams)).FirstOrDefault();
                                if (retensionTeams != null)
                                {
                                    retensionTeams.NewValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention, Level.Teams);
                                }
                            }
                            AuditItem retensionSharePoint = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_SharePoint)).FirstOrDefault();
                            if(retensionSharePoint!=null)
                            {
                                retensionSharePoint.NewValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention, Level.SharePoint);
                            }
                            AuditItem retensionExchange = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_Exchange)).FirstOrDefault();
                            if (retensionExchange != null)
                            {
                                retensionExchange.NewValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention, Level.Exchange);
                            }
                            AuditItem retensionOneDrive = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_OneDrive)).FirstOrDefault();
                            if (retensionOneDrive != null)
                            {
                                retensionOneDrive.NewValue = AuditHandleUtil.GetEnforceRetention(termInfo.EnfoceRentention, Level.OneDrive);
                            }

                            if(hasUpgradeTeams)
                            {
                                AuditItem retensionTeamsLabel = info.ModifyContent.Where(a => a.TargetSetting.Equals(i18n_Teams_Label)).FirstOrDefault();
                                if (retensionTeamsLabel != null)
                                {
                                    retensionTeamsLabel.NewValue =
                                        ((termInfo.EnfoceRentention & (int)EnforceRetentionType.Teams) == (int)EnforceRetentionType.Teams) ? termInfo.TeamsLabel : "";
                                }
                            }

                            AuditItem retensionExchangeLabel = info.ModifyContent.Where(a => a.TargetSetting.Equals(i18n_Exchange_Label)).FirstOrDefault();
                            if (retensionExchangeLabel!=null)
                            {
                                retensionExchangeLabel.NewValue =
                                    ((termInfo.EnfoceRentention & (int)EnforceRetentionType.Exchange) == (int)EnforceRetentionType.Exchange) ? termInfo.ExchangeLabel : "";
                            }

                            AuditItem retensionSpLabel = info.ModifyContent.Where(a => a.TargetSetting.Equals(i18n_Sp_Label)).FirstOrDefault();
                            if (retensionSpLabel != null)
                            {
                                retensionSpLabel.NewValue =
                                    ((termInfo.EnfoceRentention & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint) ? termInfo.SPLabel : "";
                            }

                            AuditItem retensionOneDriveLabel = info.ModifyContent.Where(a => a.TargetSetting.Equals(i18n_OneDrive_Label)).FirstOrDefault();
                            if (retensionOneDriveLabel != null)
                            {
                                retensionOneDriveLabel.NewValue =
                                    ((termInfo.EnfoceRentention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive) ? termInfo.OneDriveLabel : "";
                            }
                            #endregion

                            string strResource_TermDescription = "RM_TM_TermDescription";
                            AuditItem descriptionItem = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_TermDescription)).FirstOrDefault();
                            if (DisposalRule != null) { descriptionItem.NewValue = newTermDescription; }

                            AuditItem advanceSettingsItem = info.ModifyContent.Where(a => a.TargetSetting.Equals(term_Advanced_Settings)).FirstOrDefault();
                            if(DisposalRule != null)
                            {
                                advanceSettingsItem.NewValue = newTermAdvancedSettings;
                            }


                            AuditItem ExpriDate = new AuditItem();
                            ExpriDate = info.ModifyContent.Where(a => a.TargetSetting.Equals(strResource_RetirementSetting)).FirstOrDefault();

                            if (!string.IsNullOrEmpty(strBeginTime) && !string.IsNullOrEmpty(strEndTime))
                            {
                                if (ExpriDate != null) { ExpriDate.NewValue = string.Format("{0} {2} {1} {3}", strResource_FTime, strResource_ToTime, strBeginTime, strEndTime); }
                            }
                            if (string.IsNullOrEmpty(strBeginTime) && string.IsNullOrEmpty(strEndTime))
                            {
                                if (ExpriDate != null) { ExpriDate.NewValue = string.Format("{0}", strResource_NoExpirDate); }
                            }
                            if (string.IsNullOrEmpty(strBeginTime) && !string.IsNullOrEmpty(strEndTime))
                            {
                                if (ExpriDate != null) { ExpriDate.NewValue = string.Format("{0} {1}", strResource_ETime, strEndTime); }
                            }
                            if (!string.IsNullOrEmpty(strBeginTime) && string.IsNullOrEmpty(strEndTime))
                            {
                                if (ExpriDate != null) { ExpriDate.NewValue = string.Format("{0} {1}", strResource_STime, strBeginTime); }
                            }
                        }
                    }
                }
                auditInfo.ModifyContent = info != null && info.ModifyContent != null ? info.ModifyContent : auditInfo.ModifyContent;
                return auditInfo;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                return null;
            }
        }
    }
}
