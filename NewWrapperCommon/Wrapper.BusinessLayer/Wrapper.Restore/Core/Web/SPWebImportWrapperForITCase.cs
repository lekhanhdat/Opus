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

using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    class SPWebImportWrapperForITCase : ISPWebImport
    {
        private ISPSiteImport restoreSite;
        private ISPWebImport restoreWeb;

        private readonly IAveSite destSite;
        private readonly string url;
        private readonly ISPRestoreAPI restoreAPI;

        public SPWebImportWrapperForITCase(IAveSite destSite, string url, ISPRestoreAPI restoreAPI)
        {
            if (destSite == null)
            {
                throw new ArgumentNullException("destSite");
            }

            this.destSite = destSite;
            this.url = url;
            this.restoreAPI = restoreAPI;

            Initialize();
        }

        private void Initialize()
        {
            restoreSite = restoreAPI.CreateSPSiteImport(destSite);
            restoreWeb = restoreAPI.CreateSPWebImport(restoreSite, url);
        }

        public void Dispose()
        {
            // web的post action以后会放到web的restore对象的dispose中。这里暂时先放在这里，以后会删掉。
            if (restoreWeb is IAveSPWeb)
            {
                //new AveSPWebPostAction(restoreWeb).Excute();
            }
            restoreWeb.Dispose();
            restoreSite.Dispose();

            restoreWeb = null;
            restoreSite = null;
        }

        private void EnsureRestoreWeb()
        {
            if (restoreWeb == null)
            {
                throw new ArgumentNullException("restoreWeb");
            }
        }

        public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPWebRestoreOption spWebRestoreOption)
        {
            EnsureRestoreWeb();
            restoreSite.Restore(restoreStream, new SPSiteRestoreOption()
                {
                    RestoreAction = SPContainerRestoreAction.Skip,
                    SecurityRestoreOption = new SPSecurityRestoreOption()
                    {
                        UserGroupRestoreOption = new SPUserGroupRestoreOption() { OverWrite = true }
                    }
                });
            restoreStream.Reset();
            var report = restoreWeb.Restore(restoreStream, spWebRestoreOption);
            return report;
        }


        public void Restore(IAveRestoreStream restoreStream, SPWebRestoreOption spWebRestoreOption, ISPWebImportProfiler profiler)
        {
            EnsureRestoreWeb();
            restoreSite.Restore(restoreStream, new SPSiteRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.Skip,
            });
            restoreStream.Reset();
            restoreWeb.Restore(restoreStream, spWebRestoreOption, profiler);
        }

        public IAveWeb SPWeb
        {
            get { return restoreWeb.SPWeb; }
        }

        public Wrapper.Core.SPRestore.Mapping.IFieldMapping FieldMapping { get; set; }


        public Wrapper.Core.SPRestore.Mapping.IContentTypeMapping ContentTypeMapping { get; set; }
       
    }
}
