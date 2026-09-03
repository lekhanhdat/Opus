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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.LocationManagement.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.LocationManagement
{
    [Audit]
    public class ContainerManagementService : RMServiceBase, IContainerManagementService
    {
        //private RALogger logger = RALogger.GetInstance(typeof(ContainerManagementService));
        private IContainerDao ContainerDao => PlatformWindsorManager.GetService<IContainerDao>();

        public string GetAllContainers()
        {
            string strResult = string.Empty;
            strResult = GetJsonStrByObj(ContainerDao.GetAllContainers());
            return strResult;
        }

        private string GetJsonStrByObj(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.ContainerManagement, Action = AuditAction.CreateContainer, AfterHandler = typeof(ContainerManagementAfterAuditHandler))]
        public string SaveContainerType(string typeName, float size, string description, bool isDefault)
        {
            try
            {
                if (size <= 0)
                {
                    throw new Exception(I18N.Core.I18NEntity.GetString("RM_CZ_SizeValueInvalid"));
                }
                return GetJsonStrByObj(ContainerDao.CreateContainer(typeName, size, description, isDefault));
            }
            catch(Exception e)
            {
                return GetJsonStrByObj(new { message = e.Message });
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.ContainerManagement, Action = AuditAction.EditContainer, BeforeHandler = typeof(ContainerManagementBeforeAuditHandler), AfterHandler = typeof(ContainerManagementAfterAuditHandler))]
        public async Task<string> UpdateContainerTypeAsync(int containerId, string typeName, float size, string description, bool isDefault = false)
        {
            try
            {
                if (size <= 0)
                {
                    throw new Exception(I18N.Core.I18NEntity.GetString("RM_CZ_SizeValueInvalid"));
                }
                return GetJsonStrByObj(await ContainerDao.UpdateContainerTypeAsync(containerId,typeName, size, description, isDefault));
            }
            catch (Exception e)
            {
                return GetJsonStrByObj(new { message = e.Message });
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.ContainerManagement, Action = AuditAction.EditContainerDefault, BeforeHandler = typeof(ContainerManagementBeforeAuditHandler), AfterHandler = typeof(ContainerManagementAfterAuditHandler))]
        public async Task<bool> UpdateContainerIsDefaultAsync(int containerId, bool isDefault = false)
        {
            try
            {
                return await ContainerDao.UpdateContainerIsDefaultAsync(containerId, isDefault);
            }
            catch
            {
                return false;
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.ContainerManagement, Action = AuditAction.DeleteContainer, BeforeHandler = typeof(ContainerManagementBeforeAuditHandler), AfterHandler = typeof(ContainerManagementAfterAuditHandler))]
        public async Task<bool> DeleteContainerTypeAsync(int containerId)
        {
            try
            {
                return await ContainerDao.DeleteContainerTypeAsync(containerId);
            }
            catch
            {
                return false;
            }
        }

    }
}
