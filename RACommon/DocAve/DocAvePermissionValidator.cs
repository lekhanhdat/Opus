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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.AccountManager.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.DocAve
{
    public class DocAvePermissionValidator
    {
        private static RALogger logger = RALogger.GetInstance(typeof(DocAvePermissionValidator));

        public static bool HasThisPermisson(List<PermissionDto> permissionDtos, string moduleName)
        {
            logger.Info("begin to Validate module permission:{0}", moduleName);
            if (null == permissionDtos)
            {
                return false;
            }
            List<PermissionDto> accountPerm = permissionDtos;
            foreach (PermissionDto item in accountPerm)
            {
                var result = item.PermissionLevels.Where(param => param.Name == SystemRoleConstants.PERMISSION_FULL_CONTROL);
                if (result != null && result.ToList().Count > 0)
                {
                    return true;
                }
            }
            return accountPerm.SelectMany(permissionDto => permissionDto.PermissionLevels).Any(permissionLevel => HasThisModule(permissionLevel.PermGroups, moduleName));
        }


        private static bool HasThisModule(List<PermissionGroup> group, string moduleName)
        {
            foreach (var g in group)
            {
                if (g.SubGroups != null && g.SubGroups.Count > 0)
                {
                    if (HasThisModule(g.SubGroups, moduleName))
                    {
                        return true;
                    }
                }
                if (g.Name.Equals(moduleName))
                {
                    if (g.Checked)
                    {
                        return true;
                    }
                    else if (g.SubGroups != null && g.SubGroups.Count > 0)
                    {
                        return ValidatePermission(g.SubGroups, moduleName);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 递归判断哪个模块被选中,选中返回true，否则返回false
        /// </summary>
        /// <param name="group"> 为当前登录用户的Permission权限</param>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        private static bool ValidatePermission(List<PermissionGroup> Groups, string moduleName)
        {
            foreach (PermissionGroup item in Groups)
            {
                if (item.Checked)
                {
                    return true;
                }
                if (item.SubGroups != null && item.SubGroups.Count > 0)
                {
                    if (ValidatePermission(item.SubGroups, moduleName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


    
    }
}
