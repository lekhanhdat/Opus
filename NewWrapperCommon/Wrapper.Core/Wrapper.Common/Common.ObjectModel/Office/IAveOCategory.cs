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
using System.Collections;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOCategory
    {
        bool Contains(Guid propset);
        IEnumerable GetAllCrawledProperties();
        IEnumerable GetUnmappedCrawledProperties();
        void Update();

        bool AutoCreateNewManagedProperties { get; set; }
        int CrawledPropertyCount { get; }
        bool DiscoverNewProperties { get; set; }
        bool FullTextQueriable { get; set; }
        bool MapToContents { get; set; }
        bool MatchExistingManagedProperty { get; set; }
        string MatchIgnorePrefix { get; set; }
        string MatchIgnoreSuffix { get; set; }
        int MaxIndexedStringLength { get; set; }
        int MaxNonIndexedStringLength { get; set; }
        bool MultipleValues { get; set; }
        string Name { get; set; }
        bool Queryable { get; set; }
        bool Retrievable { get; set; }
        bool Scoped { get; set; }
    }
}
