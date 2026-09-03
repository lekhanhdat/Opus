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
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Generic;
namespace AvePoint.RA.Service.Security
{
    [RACodeReview("Allen Yin")]
    public class RMSecurityUtil
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RMSecurityUtil));

        internal static string GetAuthenticationMode(RMAuthenticationTypes type)
        {
            string modeName = null;
            switch (type)
            {
                case RMAuthenticationTypes.LocalSystem:
                    modeName = I18NEntity.GetString("RM_LogOn_LocalSystem");
                    break;
                case RMAuthenticationTypes.ADIntegration:
                    modeName = I18NEntity.GetString("RM_LogOn_ADIntegration");
                    break;
                case RMAuthenticationTypes.WindowsIntegration:
                    //modeName = I18NEntity.GetString("RM_LogOn_WindowsIntegration");
                    break;
                default:
                    modeName = type.ToString();
                    break;
            }

            return modeName;
        }

        //internal static RMAccount ConvertToDBAccount(RMADAccountDto info)
        //{
        //    RMAccount account = new RMAccount();
        //    account.AccountType = info.Type;
        //    account.LoginName = info.LoginName;
        //    account.DisplayName = info.DisplayName;
        //    account.SID = info.AccountSID;
        //    account.DomainId = info.DomainId;

        //    return account;
        //}

        //internal static RMADAccountDto ConvertToAccountDto(RMAccount account)
        //{
        //    RMADAccountDto info = new RMADAccountDto();
        //    info.Id = account.Id;
        //    info.Type = account.AccountType;
        //    info.LoginName = account.LoginName;
        //    info.AccountSID = account.SID;
        //    info.DomainId = account.DomainId;
        //    if (account.AccountType == RMAccountType.Local)
        //    {
        //        info.DisplayName = I18NEntity.GetString("RM_JS_Common_LocalAdminName"); ;
        //    }
        //    else
        //    {
        //        info.DisplayName = account.DisplayName;
        //    }
        //    return info;
        //}

        internal static RMAudit ConvertToDBAudit(RMAuditInfo info)
        {
            RMAudit audit = new RMAudit();
            audit.Id = info.Id;
            audit.UserId = info.UserId;
            audit.Role = info.Role;
            audit.Module = (int)info.Module;
            audit.Category = (int)info.Category;
            audit.Action = (int)info.Action;
            audit.Content = SerializerHelper.SerializeToXmlString(info.ModifyContent);
            audit.Status = info.Status;
            audit.ExecuteOn = info.ExecuteOn.Ticks;
            audit.Method = info.Method;
            audit.Object = info.Object;
            audit.UserName = info.UserName;
            audit.ClientIP = info.ClientIP;
            return audit;
        }

        internal static RMAuditInfo ConvertToAuditDto(RMAudit auditDto)
        {
            RMAuditInfo audit = new RMAuditInfo();
            audit.Id = auditDto.Id;
            audit.UserId = auditDto.UserId;
            audit.Role = auditDto.Role;
            audit.Module = (AuditModule)auditDto.Module;
            audit.Category = (AuditCategory)auditDto.Category;
            audit.Action = (AuditAction)auditDto.Action;
            audit.ModifyContent = ConvertToAuditItems(auditDto.Content, (AuditAction)auditDto.Action);
            audit.Status = auditDto.Status;
            audit.ExecuteOn = new DateTime(auditDto.ExecuteOn);
            audit.Method = auditDto.Method;
            audit.Object = auditDto.Object != null ? I18NEntity.GetString(auditDto.Object) : "";
            //audit.Object= I18NEntity.ReplaceI18NKey(auditDto.Object, "RM_", new string[] { "/" });
            audit.UserName = I18NEntity.GetString(auditDto.UserName);
            audit.ClientIP = auditDto.ClientIP;
            return audit;
        }

        internal static List<AuditItem> ConvertToAuditItems(string content, AuditAction auditAction)
        {
            List<AuditItem> items = SerializerHelper.DeserializeFromXmlString<List<AuditItem>>(content);
            var resultItems = new List<AuditItem>();
            if (items != null && items.Count > 0)
            {
                items.ForEach((item) =>
                {
                    var auditItem = new AuditItem
                    {
                        Id = item.Id,
                        NewValue = I18NEntity.ReplaceI18NKey(item.NewValue, "RM_", new string[] { ";", ",", " " }),
                        OldValue = I18NEntity.ReplaceI18NKey(item.OldValue, "RM_", new string[] { ";", ",", " " }),
                        TargetSetting = I18NEntity.GetString(item.TargetSetting)
                    };
                    ModifyAuditContentByTargetSetting(auditItem, item.TargetSetting, auditAction);
                    

                    resultItems.Add(auditItem);
                });
            }
            return resultItems;
        }

        // use to modify OriginalValue/NewValue of some audit item with complex value
        internal static void ModifyAuditContentByTargetSetting(AuditItem auditItem, string targetSettingI18NKey, AuditAction auditAction)
        {
            if (auditItem == null || string.IsNullOrWhiteSpace(targetSettingI18NKey) || auditAction == AuditAction.Unknown)
            {
                return;
            }

            try
            {
                switch (auditAction)
                {
                    case AuditAction.StubSettingCreate:
                    case AuditAction.StubSettingUpdate:
                        if (RMConstants.STUBCONTENT.Equals(targetSettingI18NKey))
                        {
                            auditItem.OldValue = LinkFileCommon.ReplaceStubTags(auditItem.OldValue, false);
                            auditItem.NewValue = LinkFileCommon.ReplaceStubTags(auditItem.NewValue, false);
                        }
                        else if (RMConstants.STUBRETENTIONPERIOD.Equals(targetSettingI18NKey))
                        {
                            auditItem.OldValue = auditItem.OldValue?.Trim();
                            auditItem.NewValue = auditItem.NewValue?.Trim();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error($"ModifyAuditContentByTargetSetting failed for TargetSetting:{targetSettingI18NKey}, AuditAction:{auditAction}, keep the original auditItem. Exception:{e}");
            }
        }

    }
}
