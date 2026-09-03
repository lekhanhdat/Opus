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
namespace ExchangeUtility.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class YammerGroupCsv
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string Description = "description";
        public const string IsPrivate = "private";
        public const string Moderated = "moderated";
        public const string ApiUrl = "api_url";
        public const string CreatedById = "created_by_id";
        public const string CreatedByType = "created_by_type";
        public const string CreatedAt = "created_at";
        public const string UpdatedAt = "updated_at";
        public const string Deleted = "deleted";
        public const string External = "external";
        public const string OfficeGroupId = "office_group_id";
    }
}