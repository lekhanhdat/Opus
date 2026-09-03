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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.DB.Model;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    [RACodeReview("Allen Yin")]
    public class GlobalStorageSettingDao : BaseDao<RMCPGlobalStorageSetting>, IGlobalStorageSettingDao
    {
        AsyncLock locker= new AsyncLock();
        public RMCPGlobalStorageSetting GetGlobalSettingInfoFromRA()
        {
            using (var context = this.GetNewContext())
            {
                return context.GlobalStorageSettingInfos.FirstOrDefault();
            }
                
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]    //加线程同步, 避免产生两条记录
        public async Task SaveOrUpdateAsync(RMCPGlobalStorageSetting newGssData)
        {
            using (await locker.LockAsync())
            {
                using (var context = this.GetNewContext())
                {
                    RMCPGlobalStorageSetting oldData = context.GlobalStorageSettingInfos.FirstOrDefault();
                    if (oldData == null)
                    {
                        context.GlobalStorageSettingInfos.Add(newGssData);
                        context.SaveChanges();
                    }
                    else
                    {
                        oldData.StoragePolicyId = newGssData.StoragePolicyId;
                        oldData.StoragePolicyName = newGssData.StoragePolicyName;
                        oldData.ExportLocationId = newGssData.ExportLocationId;
                        oldData.CompressionMethod = newGssData.CompressionMethod;
                        oldData.CompressionSpeed = newGssData.CompressionSpeed;
                        oldData.ExportLocationName = newGssData.ExportLocationName;
                        oldData.ExportLocationId = newGssData.ExportLocationId;
                        oldData.SecurityProfileId = newGssData.SecurityProfileId;
                        oldData.SecurityProfileName = newGssData.SecurityProfileName;
                        oldData.UseCompression = newGssData.UseCompression;
                        oldData.UseEncryption = newGssData.UseEncryption;
                        oldData.EncryptionMethod = newGssData.EncryptionMethod;
                        oldData.Extentions = newGssData.Extentions;
                        await this.UpdateAsync(oldData);
                    }
                }
            }
        }
    }


}
