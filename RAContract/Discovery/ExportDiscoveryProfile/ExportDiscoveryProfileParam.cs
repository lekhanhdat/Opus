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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using System;

namespace AvePoint.RA.Contract.Discovery.ExportDiscoveryProfile
{
    public class ExportDiscoveryProfileParam
    {
        public string FileName { get; set; }

        public string FolderPath { get; set; }

        public string JobId { get; set; }

        public string ProfileId { get; set; }

        public Guid O365TenantId { get; set; }

        public string SortBy { get; set; }

        public bool IsDescending { get; set; }

        public string DiscoveryType { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
