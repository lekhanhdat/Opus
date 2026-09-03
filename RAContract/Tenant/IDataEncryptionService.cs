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
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Security;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Tenant
{
    public interface IDataEncryptionService
    {
        void AddProfile(RADataEncryptionProfile info);
        void CreateSwitchProfileJobs();
        void DeleteProfile(string tenantId);
        int UpdateProfileInfo(RADataEncryptionProfile profile);
        int UpdateProfileJobStatus(int id, RMSwitchSecurityProfileJobStatus status);
        RADataEncryptionProfile GetProfile(string tenantId);
        int UpdateProfileStatus4TimeoutJobs();

        RMEncryptionDataInfo AddEncryptionDataItem(RMEncryptionDataInfo item);
        IEnumerable<RMEncryptionDataInfo> GetAll();
        int Update(RMEncryptionDataInfo data);

        string Encrypt(string plainText, string tenantId, string profileId = null);
        string Decrypt(string plainText, string tenantId, string profileId = null);
    }
}
