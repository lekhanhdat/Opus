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
using AvePoint.Media.Service.DomainModel;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;

namespace AvePoint.RA.SharePoint.ConvertStub
{
    public class StubListNode
    {
        public Guid ListId { get; set; }

        public string ListRootFolderPath { get; set; }

        public Guid WebId { get; set; }

        public Dictionary<Guid, StubFileNode> StubFileNodeCache { get; set; } = []; // old stub uniqueid , stub file node
    }

    public class StubSiteNode
    {
        public string SiteUrl { get; set; }

        public AveSPSite AveSPSite { get; set; }

        //public AveBPOSAccountInfo AveBPOSAccountInfo { get; set; }

        public AveObjectModelFactory AveObjectModelFactory { get; set; }

        public int CurrentFoundStubFileCount { get; set; }

        public Dictionary<Guid, StubListNode> StubListNodeCache { get; set; } = []; // list uniqueid , stub list node
    }

    public class StubFileNode
    {
        public Guid UniqueId { get; set; }

        public ArchiverBasicIndex FileIndex { get; set; }

        public bool IsSkipUpdateIndex { get; set; } = false;
    }
}
