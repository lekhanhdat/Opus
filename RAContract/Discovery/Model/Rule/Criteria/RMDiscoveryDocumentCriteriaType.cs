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

namespace AvePoint.RA.Contract.Discovery.Model.Rule.Criteria
{
    public enum RMDiscoveryDocumentCriteriaType
    {
        None = 0,
        Name = 1,
        ParentFolder = 2,
        CreatedTime = 3,
        ModifiedTime = 4,
        DocumentType = 5,
        DocumentSize = 6,
        ParentLibraryText = 8,
        ParentLibraryNumber = 9,
        ParentLibraryBoolean = 10,
        ParentLibraryDateTime = 11,
        ParentSiteCollectionText = 12,
        ParentSiteCollectionNumber = 13,
        ParentSiteCollectionBoolean = 14,
        ParentSiteCollectionDateTime = 15,
        PropertyBagText = 16,
        PropertyBagNumber = 17,
        PropertyBagBoolean = 18,
        PropertyBagDateTime = 19,
        CreateBy = 20,
        ModifiedBy = 21,
    }
}
