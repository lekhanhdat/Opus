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

using AvePoint.Wrapper.Core.Internal.Restore;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    class SPListImport : ISPListImport
    {
        private IListImport listImport;

        public IListImport ListImport
        {
            get { return listImport; }
        }

        public SPSiteImport ParentSite
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public SPFileRestoreReport Restore(Common.IAveRestoreStream restoreStream, SPListRestoreOption spListRestoreOption)
        {
            throw new NotImplementedException();
        }

        public Common.IAveList SPList
        {
            get { throw new NotImplementedException(); }
        }


        public void Restore(Common.IAveRestoreStream restoreStream, SPListRestoreOption spListRestoreOption, ISPListImportProfiler profiler)
        {
            throw new NotImplementedException();
        }

        public Wrapper.Core.SPRestore.Mapping.IFieldMapping FieldMapping { get; set; }

        public Wrapper.Core.SPRestore.Mapping.IContentTypeMapping ContentTypeMapping { get; set; }
    }
}
