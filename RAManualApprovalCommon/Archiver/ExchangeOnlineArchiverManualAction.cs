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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using RAManualApprovalCommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApprovalCommon.Archiver
{
    public class ExchangeOnlineArchiverManualAction : ArchiverManualAction
    {
        protected override SourceFlag ContentSource => SourceFlag.Exchange;

        private static readonly IEXOSettingDao ExoSettingDao = PlatformWindsorManager.GetService<IEXOSettingDao>();

        private readonly List<RMExchangeOnlineSetting> SettingCache;

        private readonly Dictionary<int, List<AvePoint.GCommon.Contract.StorageOptimization.Object.UserInfo>> SettingOwnersCache = new();

        private readonly string _mailBoxTreeNodeId;

        public ExchangeOnlineArchiverManualAction(string jobId, Guid containerId) : base(jobId, containerId)
        {
            _mailBoxTreeNodeId = string.Empty;
            SettingCache = ExoSettingDao.FindAll().Where(setting => !setting.IsRemoved).ToList();
        }
        public ExchangeOnlineArchiverManualAction(string jobId, Guid containerId, string mailBoxTreeNodeId) : base(jobId, containerId)
        {
            _mailBoxTreeNodeId = mailBoxTreeNodeId;
            SettingCache = ExoSettingDao.FindAll().Where(setting => !setting.IsRemoved).ToList();
        }

        protected override ManualApprovalSettingModel GetSettingInfo(Record record)
        {
            RMExchangeOnlineSetting? setting;
            Guid mailBoxTreeNodeId = Guid.Empty;
            if (!string.IsNullOrEmpty(this._mailBoxTreeNodeId))
            {
                mailBoxTreeNodeId = new Guid(this._mailBoxTreeNodeId);
            }
            setting = SettingCache.FirstOrDefault(item => item.ScopeId == mailBoxTreeNodeId);

            if (this._containerId != Guid.Empty)
            {
                setting ??= SettingCache.First(item => item.ScopeId == this._containerId);
            }
            else
            {
                setting ??= SettingCache.First(item => item.ScopeId == new Guid(record.ContainerId));
            }

            var res = new ManualApprovalSettingModel
            {
                IsSendEmialToOwner = setting.EMailToRecordOwner,
                ManualApprovalType = setting.ApprovalType,
                SettingId = setting.Id,
            };

            if (setting.ApprovalType == ApprovalType.ApprovalProcess)
            {
                res.WorkflowId = setting.WorkflowReferenceId;
            }
            else if (setting.ApprovalType == ApprovalType.RecordOwners)
            {
                if (!SettingOwnersCache.ContainsKey(setting.Id))
                {
                    var owners = ExoSettingDao.GetReocrdOwnersBySettingId(setting.Id);
                    SettingOwnersCache[setting.Id] = owners;
                }

                res.Owners = SettingOwnersCache[setting.Id];
            }

            return res;
        }

        protected override Task ProcessWorkflowOwnerAsync(ManualApprovalRuleModel ruleInfo, Record record)
        {
            var message = $"RM_MA_NoSupport_SiteOwner{I18NEntity.Separator}{"RM_JS_SPS_TabLabel_EXO"}";
            throw new NotImplementedException(message);
        }
    }
}
