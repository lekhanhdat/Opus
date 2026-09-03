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
using System.Collections.Generic;

namespace AvePoint.RA.DB.Dao
{
    public interface ICSDApiKeyDao : IBaseDao<CSDApiKey>
    {
        CSDApiKey GetApiKey(int id);

        List<CSDApiKey> GetApiKeys(IEnumerable<int> ids);

        List<CSDApiKey> GetApiKeys(int pageIndex, int pageSize, out int totalCount);

        bool AddApiKey(string name, string encryptedValue, string valuePrefix, long expiredTime, string operatorLoginName);

        bool EditApiKey(int id, string name, long expiredTime, string operatorLoginName);

        bool RemoveApiKeys(IEnumerable<int> ids);

        bool ExistsKeyName(int id, string name);
    }
}
