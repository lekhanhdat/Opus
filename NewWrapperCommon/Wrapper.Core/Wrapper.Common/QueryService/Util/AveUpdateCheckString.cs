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
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    internal class AveUpdateCheckString
    {
        public const string OneItemOrVersionSelectPatternForAllUserData = @"SELECT @_SiteId=tp_SiteId,@_ParentId=tp_ParentId, @_DocId=tp_DocId,@_IsCurrentVersion=tp_IsCurrentVersion,@_Level=tp_Level,@_CalculatedVersion=tp_CalculatedVersion FROM";

        public const string OneItemOrVersionForAllUserDataAllRows = @"SELECT COUNT(*) FROM AllUserData WHERE tp_SiteId=@_SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@_IsCurrentVersion AND tp_ParentId=@_ParentId AND tp_DocId=@_DocId AND tp_CalculatedVersion=@_CalculatedVersion AND tp_Level=@_Level";

        public const string AllPublishingVersionsSelectPatternForAllDocs = "SELECT @_DocId=Id FROM";

        public const string AllPublishingVersionsForAllDocs = "SELECT COUNT(*) FROM AllDocs WHERE Id=@_DocId";

        public const string AllPublishingVersionsSelectPatternForAllUserData = "SELECT @_SiteId=tp_SiteId,@_ParentId=tp_ParentId, @_DocId=tp_DocId FROM";

        public const string AllPublishingVersionsForAllUserDataOneRow = @"SELECT COUNT(*) FROM AllUserData WHERE tp_SiteId=@_SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 and tp_ParentId=@_ParentId AND tp_DocId=@_DocId AND tp_RowOrdinal=0";

        public const string AllPublishingVersionsForAllUserDataAllRows = @"SELECT COUNT(*) FROM AllUserData WHERE tp_SiteId=@_SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 and tp_ParentId=@_ParentId AND tp_DocId=@_DocId";

        public const string AllVersionsSelectPatternForAllUserDataAllRows = "SELECT @_SiteId=tp_SiteId,@_ParentId=tp_ParentId, @_DocId=tp_DocId FROM";

        public const string AllVersionsForAllUserDataOneRow = @"SELECT COUNT(*) FROM AllUserData WHERE tp_SiteId=@_SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) and tp_ParentId=@_ParentId AND tp_DocId=@_DocId AND tp_RowOrdinal=0";

        public const string AllVersionsForAllUserDataAllRows = @"SELECT COUNT(*) FROM AllUserData WHERE tp_SiteId=@_SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) and tp_ParentId=@_ParentId AND tp_DocId=@_DocId";

    }
}
