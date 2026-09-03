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
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.RA.Contract.TemplateManagement;

namespace AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode
{
    [DataContract]
    public class PagedBarcodeTemplateSuiteResult
    {
        [DataMember]
        public int PageIndex { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int TotalCount { get; set; }

        [DataMember]
        public int TotalPages
        {
            get
            {
                if (PageSize <= 0) return 0;
                return (TotalCount + PageSize - 1) / PageSize;
            }
        }

        [DataMember]
        public bool HasNextPage => PageIndex < TotalPages - 1;

        [DataMember]
        public bool HasPreviousPage => PageIndex > 0;

        [DataMember]
        public List<BarcodeTemplateSuiteDto> Suites { get; set; } = new List<BarcodeTemplateSuiteDto>();

        [DataMember]
        public string SearchName { get; set; }
    }

    [DataContract]
    public class PagedBarcodeTemplateSuiteRequest
    {
        [DataMember]
        public int PageIndex { get; set; } = 0;

        [DataMember]
        public int PageSize { get; set; } = 20;

        [DataMember]
        public string SearchName { get; set; }
    }

    public class TemplateColumnInfo
    {
        public Dictionary<string, List<string>> BoxTemplateColumns { get; set; }

        public Dictionary<string, List<string>> FolderTemplateColumns { get; set; }
    }
}
