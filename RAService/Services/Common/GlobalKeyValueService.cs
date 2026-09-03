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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Common
{
    public class GlobalKeyValueService : RMServiceBase, IGlobalKeyValueService
    {
        private RALogger logger = RALogger.GetInstance(typeof(KeyValueService));
        public IRMGlobalKeyValueDao KeyValueDao { get; set; }

        public RMGlobalNameValueDto Get(string key)
        {
            try
            {
                var entity = KeyValueDao.GetValueByKey(key);
                return entity != null ? entity.Conver2Dto() : null;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get Key value, key: {key}, error : {e.ToString()}");
            }

            return null;
        }

        public bool Save(RMGlobalNameValueDto dto)
        {
            try
            {
                var entity = dto.Conver2Entity();
                KeyValueDao.Save(entity);
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while save key and value, key : {dto.Name}{RMGlobalNameValueDto.Seprator}{dto.Type}. error: {e.ToString()}");
            }

            return false;
        }
    }
}
