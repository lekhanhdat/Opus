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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel
{
    public class WebLevelForSchedule : IndividualBase
    {
        public WebLevelForSchedule(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
            : base(objectModel, sqlConnString, siteUrl)
        {

        }

        public List<SPTreeNodeDto> GetWebs(Guid siteId, Guid parentWebId, uint siteLockStatus, int startIndex, uint perPage, ref int childrenCount)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> webs = new List<SPTreeNodeDto>();
            AveBrowserOption option = new AveBrowserOption
            {
                ParentSiteId = siteId,
                ParentWebId = parentWebId,
                NeedPaging = true,
                StartIndex = startIndex,
                PerPage = perPage,
                NeedFilter = true,
                FilterAppWeb = false,
                SiteUrl = siteUrl
            };
            List<AveWebBrowserInfo> websInfo = Query.GetBrowserWebs(option); //Query.GetBrowserWebs(siteId, parentWebId, startIndex, perPage, ref childrenCount, siteUrl);//parentWeb.GetWebs();
            websInfo.ForEach(w => webs.Add(ConvertToDto(w, siteLockStatus)));
            childrenCount = option.ChildrenTotalCount;
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower Webs For Schedule Elapsed Time: {0}, WebCount: {1}, SiteId: {2}, ParentWebId: {3}", sw.Elapsed.ToString(), webs.Count, siteId, parentWebId);
#endif
            return webs;
        }

        protected SPTreeNodeDto ConvertToDto(AveWebBrowserInfo web, uint siteLockStatus)
        {
            SPTreeNodeDto webDto = new SPTreeNodeDto();
            webDto.InheritingPermissions = !web.HasUniqueRoleAssignments;
            webDto.FullPath = web.Url;
            webDto.SPObjectId = web.ID.ToString();
            if (web.IsRootWeb)
            {
                webDto.Name = ".";
            }
            else
            {
                webDto.Name = web.Name;
            }
            webDto.DisplayName = webDto.Name;
            webDto.Url = web.Url;

            if (webDto.NodeExtension == null)
            {
                webDto.NodeExtension = new NodeExtensionDto();
            }
            webDto.NodeExtension.TemplateName = web.TemplateName;//web.WebTemplate + "#" + web.Configuration.ToString();
            try// bpos-s does not support this
            {
                webDto.NodeExtension.TemplateTitle = web.TemplateTitle;//web.Site.GetWebTemplates(web.Language)[webDto.NodeExtension.TemplateName].Title;
            }
            catch (Exception e)
            {
                Logger.Debug("BPOS-S does not support TemplateTitle, Error Message: {0}", e.ToString());
            }
            webDto.NodeExtension.LCID = web.Language;
            webDto.Title = web.Title;
            webDto.Level = NodeLevel.Site;
            webDto.FarmID = FarmId;
            webDto.SiteLockStatusValue = siteLockStatus;
            webDto.NodeExtension = FillNodeExtension(webDto.NodeExtension, web);
            return webDto;
        }
    }
}
