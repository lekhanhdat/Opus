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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
{
    public class ProjectLevel : IndividualBase
    {
        public ProjectLevel(AveObjectModelFactory objectModel, string siteUrl)
            : base(objectModel, string.Empty, siteUrl)
        {
            mSiteUrl = siteUrl;
        }

        public List<SPTreeNodeDto> GetProjects(int siteLockStatus, int startIndex, uint perPage, ref int childrenCount)
        {
            List<SPTreeNodeDto> projs = new List<SPTreeNodeDto>();
            List<AveProjectBrowserInfo> projsInfo = Query?.GetBrowserProjects(startIndex, perPage, ref childrenCount);
            projsInfo?.ForEach(p => projs?.Add(ConvertToDto(p, siteLockStatus)));
            projs?.Sort(new SPTreeNodeDtoComparer());
            childrenCount++;
            return projs;
        }

        protected SPTreeNodeDto ConvertToDto(AveProjectBrowserInfo proj, int siteLockStatus)
        {
            SPTreeNodeDto projDto = new SPTreeNodeDto();
            projDto.HasSubFolder = true;
            projDto.Name = proj.Name;
            projDto.DisplayName = proj.Name;
            projDto.SPObjectId = proj.ID.ToString();

            if (proj.IsEnterpriseProject)
            {
                projDto.Type = NodeType.EnterpriseProject;
            }
            else
            {
                projDto.Type = NodeType.SharePointTaskProject;
            }

            projDto.FullPath = proj.Url;
            projDto.Url = proj.Url;
            projDto.Level = NodeLevel.ProjectOnline;
            projDto.FarmID = FarmId;
            projDto.SiteLockStatus = siteLockStatus;
            if (projDto.NodeExtension == null)
            {
                projDto.NodeExtension = new NodeExtensionDto();
                projDto.NodeExtension.IsEnterpriseProject = proj.IsEnterpriseProject;
                projDto.NodeExtension.IsCheckedOut = proj.IsCheckedOut;
                projDto.NodeExtension.EnterpriseProjectTypeId = proj.EnterpriseProjectTypeId;
            }
            return projDto;
        }
    }
}
