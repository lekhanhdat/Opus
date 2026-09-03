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
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectSerializer : IAveProjectSerializer
    {

        private IAveRequest mRequest;
        private AveSite mSite;

        public AveProjectSerializer(IAveRequest request, AveSite site)
        {
            this.mRequest = request;
            this.mSite = site;
        }

        public AveRestoreResult SetObjectData(AveProjectInfo projInfo, AveProjectReader projectDetails, AveProjectConfig projectConfig, AveRestoreMode restoreMode)
        {
            var project = this.mSite.Projects.GetByName(projInfo.Name);
            if (project == null)
            {
                project = this.mSite.Projects.GetById(projInfo.OriginalId);
            }
            if (project != null)
            {
                projInfo.NewId = project.Id;
                projInfo.NewSummaryTaskId = project.SummaryTaskId;
                if (project.IsCheckedOut &&
                    (project.CheckedOutBy == null ||
                    !string.Equals(project.CheckedOutBy.LoginName, this.mSite.RootWeb.CurrentUser.LoginName)
                    ))
                {
                    throw new Exception(WrapperRestoreResource.Wrapper_SkippedByCheckout);
                }
                else if (project.EnterpriseProjectTypeId != projInfo.EnterpriseProjectTypeId)
                {
                    throw new Exception(WrapperRestoreResource.Wrapper_SkippedByEnterpriseType);
                }
                else if (!string.Equals(projInfo.ProjectSiteUrl, project.ProjectSiteUrl))
                {
                    throw new Exception(WrapperRestoreResource.Wrapper_SkippedByProjectSite);
                }
            }
            
            Dictionary<string, object> result = mRequest.RestoreProject(projInfo, projectDetails, projectConfig, restoreMode);
            return new AveRestoreResult();
        }
    }
}
