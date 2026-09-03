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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IExtRecordDao
    {
        ///// <summary>
        ///// add data to ext managed
        ///// </summary>
        ///// <param name="extInfo"></param>
        //void AddDataToManagedExt(RMExtForManagedRecord extInfo);
        ///// <summary>
        ///// add data to ext destroyed
        ///// </summary>
        ///// <param name="extInfo"></param>
        //void AddDataToDestroyExt(RMExtForDestroyedRecord extInfo);

        ///// <summary>
        ///// delete record
        ///// </summary>
        ///// <param name="destroyed"></param>
        ///// <param name="scopeId"></param>
        ///// <param name="dirPath"></param>
        //void DeleteRecord(bool destroyed, Guid scopeId, string dirPath);

        ///// <summary>
        ///// get ext data object
        ///// </summary>
        ///// <param name="destroyed"></param>
        ///// <param name="scopeId"></param>
        ///// <param name="dirPath"></param>
        ///// <returns></returns>
        //RMBaseExtForRecord GetExtisionByKey(bool destroyed, Guid scopeId, string dirPath);
        ///// <summary>
        ///// only get fullPath and metainfo
        ///// </summary>
        ///// <param name="destroyed"></param>
        ///// <param name="scopeId"></param>
        ///// <param name="dirPath"></param>
        ///// <returns></returns>
        //RMBaseExtForRecord GetMetaInfoByKey(bool destroyed, Guid scopeId, string dirPath);
    }
}
