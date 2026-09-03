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

namespace AvePoint.Wrapper.QueryService
{
    extern alias QueryService19;
    using AvePoint.Wrapper.Common;
    using System;

    internal class AveQueryService: QueryService19.AvePoint.Wrapper.QueryService.AveQueryService
    {
        protected override AveRBSStubInfo GenerateStubinfo(byte[] tem_blobId, byte[] tem_poolId, long dataLen)
        {
            return new AveRBSStubInfo(tem_blobId, tem_poolId, AveRBSCommon.RBS_PROVIDER_NAME_SPSE, dataLen);
        }

        protected override void AddProviderNameParam()
        {
            mQueryWorker.AddParameter("@ProviderName", AveRBSCommon.RBS_PROVIDER_NAME_SPSE);
        }

        public bool CheckItemIdAvailable(Guid siteId, Guid listId, int itemId)
        {
            var dto = new RestoringDto();
            dto.NameMapping = $"{itemId}_.000";
            CheckConflictInfoForListItem(siteId, listId, dto);
            return dto.ConflictType == ConflictType.None;
        }

    }
    public class AveQueryServiceProvider
    {
        public static T Instance<T>(object arg) where T : IAveQueryService
        {
            return (T)CreateQueryService(arg);
        }
        internal static IAveQueryService CreateQueryService(object arg)
        {
            var queryService = new AveQueryService();
            queryService.InitQuerySession(arg);
            return queryService;
        }
    }
}
