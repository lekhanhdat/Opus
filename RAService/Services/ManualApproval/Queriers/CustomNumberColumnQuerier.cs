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
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class CustomNumberColumnQuerier : ICustomFilter, ICustomSorter
    {
        public ManualApprovalOrderOptions OrderOption => ManualApprovalOrderOptions.CustomNumber;

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.CustomNumber;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            throw new NotImplementedException();
        }

        public Expression<Func<ManualApprovalRecord, dynamic>> GetCosmosDBOrderExpression()
        {
            throw new NotImplementedException();
        }

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string columnId, string value)
        {
            return (root) => Enumerable.Contains(root.CustomColumnDic[columnId].Value_Array, value);
        }

        public Expression<Func<ManualApprovalRecord, dynamic>> GetCosmosDBOrderExpression(string columnId)
        {
            return (root) => root.CustomColumnDic[columnId].Number;
        }
    }
}
