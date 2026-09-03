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
using AvePoint.RA.APIContract;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.API
{
    /// <summary>
    /// for other product call ,get terms , remove the original API integrated with OC, only leave the API in WebAPI Role with Identity Service authentication.
    /// </summary>
    //public class TermAPIController : PortalApiController
    //{
    //    protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
    //    private ITaxonomyService _TaxonomyService = null;
    //    private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);

    //    [HttpPost]
    //    public Contract.TaxonomyModel.RMColumnInfo GetColumnInfo([FromBody]string mailBox)
    //    {
    //        var columnInfo = new Contract.TaxonomyModel.RMColumnInfo()
    //        {
    //            Id = TermColumnInfo.WellKnowTermColumnId,
    //            UniqueId = TermColumnInfo.WellKnowTermColumnGuid,
    //            Name = TermColumnInfo.WellKnowTermColumnName
    //        };
    //        try
    //        {
    //            Logger.Info("access api get columInfo");
    //            var term = TaxonomyService.GetDefaultTermByMailBox(mailBox);
    //            if (term != null)
    //            {
    //                columnInfo.DefaultTermId = term.UniqueId;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            ApiMessageUtil.SetResponseErrorMsg(RestStateCode.GetTermColumnInfo, ex);
    //            Logger.Error("error occurred while get column info{0}", ex.ToString());
    //            return null;
    //        }
    //        return columnInfo;
    //    }
    //    [HttpPost]
    //    public async Task<List<TermInfo>> GetTerm([FromBody]TermTreePager tree)
    //    {
    //        List<TermInfo> reulst = new List<TermInfo>();
    //        try
    //        {
    //            Logger.Info("access api get term tree pager");
    //            int pIndex = tree.PageIndex = tree.PageIndex == 0 ? tree.PageIndex : tree.PageIndex - 1;

    //            Contract.TaxonomyModel.RMTermType type = (Contract.TaxonomyModel.RMTermType)tree.NodeType;

    //            List<Contract.TaxonomyModel.RMTermInfo> terms = await TaxonomyService.GetTaxonomyTreeDataAsync(type, tree.NodeId, pIndex, tree.PageSize);
    //            reulst = terms.ConvertAll(t => ModeConvertUtil.ToApiTermInfo(t));
    //        }
    //        catch (Exception ex)
    //        {
    //            ApiMessageUtil.SetResponseErrorMsg(RestStateCode.GetTermTreeByPager, ex);
    //            Logger.Error("get term error:{0}", ex.ToString());
    //            return null;
    //        }
    //        return reulst;
    //    }


    //    [HttpPost]
    //    public TermInfo GetTermTreeByMailBox([FromBody]string mailBox)
    //    {
    //        TermInfo termInfo = null;
    //        try
    //        {
    //            Logger.Info("access api get term tree by mailBox");
    //            List<TermInfo> terms = new List<TermInfo>();
    //            var treeNode = TaxonomyService.GetTermTreeByMailBox(mailBox);
                
    //            termInfo = ModeConvertUtil.ToApiTermInfo(treeNode);
                
    //        }
    //        catch (Exception ex)
    //        {
    //            ApiMessageUtil.SetResponseErrorMsg(RestStateCode.GetTermTreeByEmail, ex);
    //            Logger.Error("get term by mailbox error:{0}", ex.ToString());
    //            return null;
    //        }
    //        return termInfo;
    //    }

    //}
}
