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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AvePoint.RA.Contract.RMWeb.ReportCenter
{

    public class RMTermDto {

        public int Id { get; set; }
        public string Name { get; set; }
        public string CreateTime { get; set; }
        public string LastModifiedTime { get; set; }
        public string TermSetId { get; set; }
        public string UniqueId { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int subTermCount { get; set; }
        public string ParentId { get; set; }
        public bool IsChecked { get; set; }
        public int pageIndex { get; set; }
        public bool expand { get; set; }
        public bool IsLeafNode { get; set; }
        public bool IsDeprecated { get; set; }
        public List<RMTermDto> subTerms { get; set; }
        public int PageSize { get; set; }
    }

    public class RMTermIdentity
    {
        //public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public string Name { get; set; }
        //public string TermSetId { get; set; }
        public string FullPath { get; set; }

        public RMTermStatus Status { get; set; }
    }

    public enum RMTermStatus
    {
        Avaliable = 0,
        Retired,
        Invalid,
        Removed
    }
    public class LocationTermExt
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TermSetId { get; set; }
        public string UniqueId { get; set; }
    }
}
