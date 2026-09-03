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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    public class DeletedRecordDto
    {
        public int Id { get; set; }

        public string RecordsId { get; set; }

        public Guid ScopeId { get; set; }
        public Guid NodeId { get; set; }

        public string DirPath { get; set; }// to do next

        public int NodeType { get; set; }

        public string LeafName { get; set; }

        public Guid TermId { get; set; }

        public string TermName { get; set; }

        public Guid RuleId { get; set; }

        public string RuleName { get; set; }

        public Guid WebId { get; set; }

        public Guid ListId { get; set; }

        public Guid FolderId { get; set; }
        public Guid ItemId { get; set; }

        // <summary>
        /// -1: None
        /// 0: All
        /// 1: SharePoint
        /// 2: FileSystem
        /// </summary>
        public int SourceFlag { get; set; }

        public string CreatedBy { get; set; }

        public DateTime TimeCreated { get; set; }

        public long TimeLastModified { get; set; }

        public long CollectionTime { get; set; }
    }
}
