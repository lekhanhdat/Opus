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
using AvePoint.RA.DB.Model;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ExportDataEncryptionSettingDao : BaseDao<RMExportDataEncryptionSetting>, IExportDataEncryptionSettingDao
    {
        public RMExportDataEncryptionSetting GetExportDataEncryptionSetting()
        {
            using (var context = this.GetNewContext())
            {
                var setting = context.RMExportDataEncryptionSetting.Where(s => s.IsCurrent).FirstOrDefault();
                return setting;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]    //加线程同步, 避免产生两条记录
        public bool Save(RMExportDataEncryptionSetting setting)
        {
            using (var context = this.GetNewContext())
            {
                var oldSettings = context.RMExportDataEncryptionSetting.Where(s => s.IsCurrent).ToList();
                if (oldSettings != null && oldSettings.Count > 0)
                {
                    oldSettings.ForEach(s =>
                    {
                        s.IsCurrent = false;
                    });
                    this.BatchUpdate(oldSettings);
                }
                context.RMExportDataEncryptionSetting.Add(setting);
                return context.SaveChanges() > 0;
            }
        }
    }
}
