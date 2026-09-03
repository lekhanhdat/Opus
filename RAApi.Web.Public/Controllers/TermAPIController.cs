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
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Web.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    /// <summary>
    /// for other product call ,get terms
    /// </summary>
    [Route("api/[controller]/[action]")]
    [RMAgentApiPerformanceLogger]
    public class TermAPIController : RAWebApiBase
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private ITaxonomyService _TaxonomyService;

        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);

        private IExplorerService _ExplorerService;

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);

        private IRMFileSystemSettingsService _RMFileSystemSettingsService;

        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService(ref _RMFileSystemSettingsService);

        [HttpPost]
        public RMColumnInfo GetColumnInfo([FromBody] string mailBox)
        {
            var columnInfo = new RMColumnInfo()
            {
                Id = TermColumnInfo.WellKnowTermColumnId,
                UniqueId = TermColumnInfo.WellKnowTermColumnGuid,
                Name = TermColumnInfo.WellKnowTermColumnName
            };
            try
            {
                Logger.Info("access api get columInfo");
                var term = TaxonomyService.GetDefaultTermByMailBox(mailBox);
                if (term != null)
                {
                    columnInfo.DefaultTermId = term.UniqueId;
                }
            }
            catch (Exception ex)
            {
                Response.SetResponseErrorMsg(RestStateCode.GetTermColumnInfo, ex);
                Logger.Error("error occurred while get column info{0}", ex.ToString());
                return null;
            }
            return columnInfo;
        }
        [HttpPost]
        public async Task<List<TermNodeInfo>> GetTerm([FromBody] TermTreePager tree)
        {
            List<TermNodeInfo> reulst = new List<TermNodeInfo>();
            try
            {
                Logger.Info("access api get term tree pager");
                int pIndex = tree.PageIndex = tree.PageIndex == 0 ? tree.PageIndex : tree.PageIndex - 1;

                RMTermType type = (RMTermType)tree.NodeType;

                List<RMTermInfo> terms = await TaxonomyService.GetTaxonomyTreeDataAsync(type, tree.NodeId, pIndex, tree.PageSize);
                reulst = terms.ConvertAll(t => ConvertUtil.ToApiTermInfo(t));
            }
            catch (Exception ex)
            {
                Response.SetResponseErrorMsg(RestStateCode.GetTermTreeByPager, ex);
                Logger.Error("get term error:{0}", ex.ToString());
                return null;
            }
            return reulst;
        }

        [HttpPost]
        public async Task<string> ChangeTermAsync([FromBody] ChangeTermDto termDto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.ChangeTermAsync(termDto));
        }

        [HttpPost]
        public async Task<string> ChangeLabelAsync([FromBody] ChangeTermDto termDto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.ChangeGoogleTermAsync(termDto));
        }

        [HttpGet]
        public List<RMTermInfo> GetAllTerms(string searchValue)
        {
            try
            {
                Logger.Info("access api get term by search value and limit");
                return TaxonomyService.SearchTermWithLimit(searchValue, 100);
            }
            catch (Exception ex)
            {
                Response.SetResponseErrorMsg(RestStateCode.GetTermTreeByPager, ex);
                Logger.Error("get term error:{0}", ex.ToString());
                return new();
            }
        }

        [HttpGet]
        public async Task<List<RMTermInfo>> GetAllLabels(string searchValue)
        {
            try
            {
                Logger.Info("access api get label by search value and limit");
                return await TaxonomyService.SearchLabelWithLimit(searchValue, 100);
            }
            catch (Exception ex)
            {
                Response.SetResponseErrorMsg(RestStateCode.GetTermTreeByPager, ex);
                Logger.Error("get term error:{0}", ex.ToString());
                return new();
            }
        }

        [HttpPost]
        public int GetFSClassificationLevel()
        {
            return RMFileSystemSettingsService.GetClassificationLevel();
        }

        [HttpGet]
        public string GetTermWithPath(Guid termId)
        {
            return TaxonomyService.GetTermWithPathByTermId(termId);
        }

        [HttpPost]
        public TermNodeInfo GetTermTreeByMailBox([FromBody] string mailBox)
        {
            TermNodeInfo termInfo = null;
            try
            {
                Logger.Info("access api get term tree by mailBox");
                List<TermInfo> terms = new List<TermInfo>();
                var treeNode = TaxonomyService.GetTermTreeByMailBox(mailBox);

                termInfo = ConvertUtil.ToApiTermInfo(treeNode);

            }
            catch (Exception ex)
            {
                Response.SetResponseErrorMsg(RestStateCode.GetTermTreeByEmail, ex);
                Logger.Error("get term by mailbox error:{0}", ex.ToString());
                return null;
            }
            return termInfo;
        }

    }
}
