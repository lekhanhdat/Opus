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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.Search;
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common.Office;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSchemaDatabase : IAveOSchemaDatabase
    {
        private readonly string mSchemaDatabase_Type = "Microsoft.Office.Server.Search.Administration.SchemaDatabase";
        private object mSchemaDatabase;

        public AveOSchemaDatabase(object schemaDatabase)
        {
            mSchemaDatabase = schemaDatabase;
        }

        public AveOSchemaDatabase(IAveOSearchServiceApplication searchServiceApplication)
        {
            SPServiceApplication spServiceApplication = null;
            if (searchServiceApplication != null)
            {
                spServiceApplication = (searchServiceApplication as AveServiceApplication).ServiceApplication;
            }
            mSchemaDatabase = AveAssemblyUtility.CreateInstance(mSchemaDatabase_Type, new Type[] {typeof (SearchServiceApplication)}, new object[] {spServiceApplication});
        }
    }
}
