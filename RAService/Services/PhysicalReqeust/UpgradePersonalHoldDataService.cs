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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AvePoint.RA.Service.Services.PhysicalReqeust
{
    public class UpgradePersonalHoldDataService : RMServiceBase, IUpgradePersonalHoldDataService
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        public IRecordLoanAllianceDao RecordLoanAllianceDao => PlatformWindsorManager.GetService<IRecordLoanAllianceDao>();

        private IExplorerDao _explorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }

        public bool NeedUpgrade()
        {
            var dto = KeyValueService.Get(RMNameValueConstants.UpgradePersonalHold, RMNameValueType.UpgradePersonalHoldData);
            var v = dto?.Value;
            if (string.IsNullOrEmpty(v)) return true;

            return !bool.Parse(v);
        }

        public async System.Threading.Tasks.Task UpgradeAsync()
        {
            try
            {
                var groups = GetData();
                foreach (var group in groups)
                {
                    var holdBy = GetUser(group.Key);
                    foreach(var v in group)
                    {
                        UpgradeData(v.Item2, holdBy);
                    }
                }
                await MarkUpgradedAsync();
            }
            catch(Exception e)
            {
                logger.Error($"Error occurred while upgrading personal hold data in cosmos db, error : {e.ToString()}");
            }
        }

        private AOSUserDto GetUser(string displayName)
        {
            return new AOSUserDto { DisplayName = displayName };
        }

        private void UpgradeData(Guid id, AOSUserDto loanedBy)
        {
            var record = ExplorerDao.GetPhysicalRawDataById(id);
            if (record == null)
            {
                logger.Warn($"Can't find the record with id : {id}");
                return;
            }
            record.UpgradePersonalHoldData(loanedBy);
            ExplorerDao.Upsert(record);
        }
        /// <summary>
        /// get the data group by hold by
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IGrouping<string,Tuple<string,Guid>>> GetData()
        {
            var tuple = RecordLoanAllianceDao.GetAllRecordsIdAndHoldBy();
            return tuple.GroupBy(o => o.Item1); 
        }

        /// <summary>
        /// mark that the upgarde is done, will not upgrading again at next time
        /// </summary>
        private async System.Threading.Tasks.Task MarkUpgradedAsync()
        {
            var dto = new RMNameValueDto
            {
                Name = RMNameValueConstants.UpgradePersonalHold,
                Value = true.ToString(),
                Type = RMNameValueType.UpgradePersonalHoldData
            };
            await KeyValueService.SaveAsync(dto);
        }
    }
}
