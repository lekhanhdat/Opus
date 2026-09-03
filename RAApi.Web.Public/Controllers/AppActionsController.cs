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
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [SpfxScopeFilter("opus_user_impersonation")]
    public class AppActionsController: ControllerBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(AppActionsController));
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();


        [HttpPost]
        public Task<string> Test()
        {
            return Task.FromResult("ok");
        }

        [HttpPost]
        public Task<string> GetRelatedRecordDetail([FromBody] RelatedReordsParm itemInfo)
        {
            List<RelatedReordsParm> args = new List<RelatedReordsParm>() { itemInfo };
            return GetDetailByTypeAsync(args);
        }

        [HttpPost]
        public RAReturnMessage SubmitRelatedItems([FromBody] RelatedItemSubmit saveInfo)
        {
            return ExplorerService.SubmitRelatedItems(saveInfo);
        }

        private async Task<string> GetDetailByTypeAsync(List<RelatedReordsParm> args)
        {
            var itemInfo = args[0];
            //_ = Guid.TryParse(itemInfo.UniqueId, out Guid recordId);
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            var recordId = Guid.Empty;
            string result;
            switch (itemInfo.SourceFlag)
            {
                case (int)SourceFlag.SharePoint:
                    recordId = IDGenerator.GetRecordId(itemInfo.SiteId, itemInfo.UniqueId);
                    var spItem = await ExplorerService.LoadDetailByKeyAsync(0, recordId, ExplorerDetailTab.All);
                    if (spItem?.Summary == null)
                    {
                        var item4GetDetails = new RelatedItemSubmitInfo()
                        {
                            SiteId = itemInfo.SiteId,
                            ListId = itemInfo.ListId,
                            ListItemId = itemInfo.ListItemId,
                            NeedDelete = false,
                            SiteUrl = itemInfo.SiteUrl,
                            UniqueId = itemInfo.UniqueId,
                            WebId = itemInfo.WebId,
                        };
                        var itemDetail = ExplorerService.GetRelatedItemDetailsInfo(item4GetDetails);
                        spItem = itemDetail;
                    }
                    if (int.TryParse(spItem.Summary.DisposalAction, out int disposalAction))
                    {
                        spItem.Summary.DisposalAction = RuleHelper.ConvertDisposalActionToString(disposalAction);
                    }
                    result = JsonConvert.SerializeObject(spItem);
                    break;
                case (int)SourceFlag.Physical:
                    recordId = itemInfo.UniqueId;
                    PhysicalObjectDto phyItem = null;
                    if (recordId != Guid.Empty)
                    {
                        phyItem = await ExplorerService.GetPhysicalObjectByIdAsync(recordId);
                        phyItem.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(recordId);
                        phyItem.TermFullPath = TaxonomyService.GetTermPathByTermId(phyItem.TermId);
                        if (phyItem.MetaInfo == null)
                        {
                            phyItem.MetaInfo = new Dictionary<string, string>();
                        }
                        if (phyItem.Id != Guid.Empty)
                        {
                            phyItem.Template = await TemplateManagementService.LoadTemplateDtoAsync(phyItem.TemplateId);

                        }
                        await ExplorerService.ConvertDateTimeColumnValueTimeZoneAsync(phyItem);
                    }

                    else
                    {
                        logger.Error($"Load physical object info, current id seems is not in correct format, id value: [{recordId}].");
                    }

                    RecordDetailDto phyDetails = new RecordDetailDto();

                    phyDetails.Summary = new RecordSummary()
                    {
                        DeclareAsRecord = false,
                        DeclaredBy = "",
                        DisposalAction = RuleHelper.ConvertDisposalActionToString(phyItem.RuleAction),
                        DisposalDate = phyItem.DisposalDueDate,
                        FullPath = $"{phyItem.HomeLocationFullPath}/{phyItem.Name}",
                        HoldBy = phyItem.HoldBy,
                        HoldId = phyItem.HoldBy,
                        HoldReleaseTime = phyItem.HoldStatus == HoldStatus.None ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, phyItem.HoldReleaseTime, true).SimplifyFormatTime,
                        HoldSetting = new HoldSetting() { Name = phyItem.HoldProfileTitle, Description = phyItem.HoldProfileComment },
                        HoldStatus = phyItem.HoldStatus != HoldStatus.None,
                        LeafName = phyItem.Name,
                        RecordId = phyItem.UniqueId,
                        RuleId = phyItem.RuleId,
                        RuleName = phyItem.RuleName,
                        SourceFlag = SourceFlag.Physical,
                        Term = phyItem.TermFullPath,
                    };


                    //result = SerializerHelper.SerializeByJsonSerializer(phyItem);
                    //result = JsonConvert.SerializeObject(phyItem);
                    result = JsonConvert.SerializeObject(phyDetails);
                    break;
                default:
                    throw new NotSupportedException("invalid type");
            }

            return result;

        }

        [DataContract]
        public class RelatedReordsParm
        {
            [DataMember]
            public Guid ListId { get; set; }

            [DataMember]
            public Guid WebId { get; set; }

            [DataMember]
            public Guid UniqueId { get; set; }

            [DataMember]
            public int ListItemId { get; set; }

            [DataMember]
            public string SiteUrl { get; set; }

            [DataMember]
            public Guid SiteId { get; set; }

            [DataMember]
            public int SourceFlag { get; set; }
        }
    }
}
