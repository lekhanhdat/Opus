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


namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOBackupDataFilterDto
    {
        /// <summary>
        /// 注释：根据PlanId过滤记录，通常不要和FarmAndPlanIds属性同时使用。
        /// </summary>
        [DataMember]
        public List<String> PlanIds { get; set; }

        [DataMember]
        public List<EOBackupLevel> BackupLevels { get; set; }

        [DataMember]
        public List<EOBackupType> BackupTypes { get; set; }

        [DataMember]
        public bool IncludePartialData { get; set; }

        [DataMember]
        public List<long> TimeRange { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        #region == 收集CEIP信息 ==
        [DataMember]
        public bool UsePlanFilter { get; set; }

        [DataMember]
        public bool UseJobFilter { get; set; }

        [DataMember]
        public bool UseTimeRange { get; set; }
        #endregion ==
    }
}
