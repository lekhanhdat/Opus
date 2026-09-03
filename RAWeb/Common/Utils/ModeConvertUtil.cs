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
using AvePoint.RA.Contract.Security;
using AvePoint.RA.APIContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AvePoint.RA.Web.Common.Utils
{
    public class ModeConvertUtil
    {
        public static RMAccessToken ToAccessToken(AccessTokenModel mode)
        {
            return new RMAccessToken
            {
                Type = mode.Type,
                TenantGroupId = mode.TenantGroupId,
                Email = mode.Email,
                AccessToken = mode.AccessToken,
                ExpiredTime = mode.ExpiredTime,
                Signature = mode.Signature
            };
        }

        public static AccessTokenModel FromAccessToken(RMAccessToken token)
        {
            return new AccessTokenModel
            {
                Type = token.Type,
                Email = token.Email,
                TenantGroupId = token.TenantGroupId,
                AccessToken = token.AccessToken,
                ExpiredTime = token.ExpiredTime,
                Signature = token.Signature
            };
        }

        public static RMLoginInfo ToLoginInfo(RMLoginModel model)
        {
            return new RMLoginInfo
            {
                Type = model.Type,
                AppUrl = model.AppUrl,
                Email = model.Email,
                Signature = model.Signature
            };
        }

        public static TermInfo ToApiTermInfo(Contract.TaxonomyModel.RMTermInfo dto)
        {
            return new TermInfo()
            {
                Id = dto.Id,
                UniqueId = dto.UniqueId,
                Name = dto.Name,
                Type = (TermType)dto.Type
            };
        }

        public static TermInfo ToApiTermInfo(Contract.RMReport.TermTreeNode treeNode)
        {
            TermInfo termInfo = new TermInfo();
            termInfo.UniqueId = treeNode.ID;
            termInfo.Name = treeNode.Name;
            termInfo.Type = (TermType)treeNode.Type;
            termInfo.Children = treeNode.Children == null ? null : ToApiTermInfoList(treeNode.Children.Values.ToList());
            return termInfo;
        }

        private static List<TermInfo> ToApiTermInfoList(List<Contract.RMReport.TermTreeNode> treeNodeList)
        {
            List<TermInfo> terms = new List<TermInfo>();
            foreach (var item in treeNodeList)
            {
                terms.Add(ToApiTermInfo(item));
            }
            return terms;
        }
    }
}