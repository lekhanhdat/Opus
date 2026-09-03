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


using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Media.Storage.CAStor
{
    class CAStorConstants
    {
        public const string CONTENT_LENGTH = "Content-Length";
        public const string LOCATION = "Location";
        public const string ALIAS = "?alias=yes";
        public const string CASTOR_AUTH_HEAD = "Castor-Authorization";
        public const string AUTH = "auth";
        public const string CACHE_TIME_OUT = "realmCacheStaleTimeout";
        public const string LAST_MODIFY_TIME = "last-modified";
        public const string CONTENT_TYPE = "Content-type";
        public const string AVAILABEL_SPACE = "Castor-System-TotalGBAvailable";
        public const string TOTAL_SPACE = "Castor-System-TotalGBCapacity";
        public const string OBJCET_UUID = "Content-UUID";

        public const string CONTENT_DISPOSITION = "Content-Disposition";
        public const string CREATOR = "x-Dell-creator-meta";
        public const string CREATOR_VERSION = "x-Dell-creator-version-meta";

        public const string CARINGO_CREATOR = "x-Caringo-creator-meta";
        public const string CARINGO_CREATOR_VERSION = "x-Caringo-creator-version-meta";

        public const string ORIGINATOR = "x-Dell-originator";
        public const string ORIGINATOR_VERSION = "x-Dell-originator-version-meta";

        public const string PLAN_ID_HEADER = "Castor-plan-id";
        public const string JOB_ID_HEADER = "Castor-job-id";
        public const string CYCLE_ID_HEADER = "Castor-cycle-id";
        public const string FARM_HEADER = "Castor-farm-name";
        public const string WEBAPP_HEADER = "Castor-webapp";
        public const string POOL_ID_HEADER = "Castor-pool-id";
        public const string STIE_URL_HEADER = "Castor-site-url";
        public const string META_ID_HEADER = "Castor-meta-id";
        public const string INDEX_HEADER = "Castor-index";
        public const string CONTENT_DISCRIPUTION = "Content-Disposition";
        public const string LIFTPOINT = "Lifepoint";

        public static readonly int NO_COMPRESS = 0;
        public static readonly int FAST_COMPRESS = 2;
        public static readonly int BEST_COMPRESS = 1;

        //public int FILE_SIZE = 1;
        //public int LAST_MODIFIED_TIME = 2;
    }
}
