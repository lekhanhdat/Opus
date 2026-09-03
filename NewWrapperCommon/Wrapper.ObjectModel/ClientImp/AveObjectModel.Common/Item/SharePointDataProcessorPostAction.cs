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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    public class SharePointDataProcessorPostAction : ISharePointDataProcessor
    {
        private readonly IAveSite site;
        private readonly AveSiteMappingManager mapping;
        private readonly AveSiteInfo sourceSiteInfo;
        private readonly IAveFile file;
        private readonly Func<string, string> getuserAction;

        public SharePointDataProcessorPostAction(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo SourceSiteInfo, Func<string, string> GetUserFromMapping)
        {
            this.site = site;
            this.mapping = mapping;
            this.sourceSiteInfo = SourceSiteInfo;
            this.getuserAction = GetUserFromMapping;
        }

        public void PostActionImpl()
        {
            (this.site as AveSite).Request.PostRestoreModernWebpart(this.site, mapping, sourceSiteInfo, this.getuserAction);
        }

        public bool ProcessUserData(Dictionary<string, object> userData)
        {
            throw new NotImplementedException();
        }

        public void RecordPostActions()
        {
            throw new NotImplementedException();
        }
    }
}
