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
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    /// <summary>
    /// 主要是给多线程使用，为了避免问题和SPWebImportWrapper分开了。
    /// </summary>
    class SPWebImportWrapperMT : ISPWebImport
    {
        private readonly SPSiteImport siteImport;
        private readonly SPWebImport webImport;

        public SPWebImportWrapperMT(SPSiteImport siteImport, string url, ISPRestoreAPI restoreAPI)
        {
            if(siteImport == null)
            {
                throw new ArgumentNullException("siteImport");
            }

            if(string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            this.siteImport = siteImport.Clone();
            this.webImport = (SPWebImport)restoreAPI.CreateSPWebImport(siteImport, url);
        }

        public SPFileRestoreReport Restore(Common.IAveRestoreStream restoreStream, SPWebRestoreOption spWebRestoreOption)
        {
            return webImport.Restore(restoreStream, spWebRestoreOption);
        }

        public void Restore(Common.IAveRestoreStream restoreStream, SPWebRestoreOption spWebRestoreOption, ISPWebImportProfiler profiler)
        {
            webImport.Restore(restoreStream, spWebRestoreOption, profiler);
        }

        public void Dispose()
        {
            webImport.Dispose();
            siteImport.Close();//不掉用post action是等所有都还原结束之后，主函数调用即可。
        }

        public Common.IAveWeb SPWeb
        {
            get { return webImport.SPWeb; }
        }

        public Wrapper.Core.SPRestore.Mapping.IFieldMapping FieldMapping { get; set; }


        public Wrapper.Core.SPRestore.Mapping.IContentTypeMapping ContentTypeMapping { get; set; }
      
    }
}
