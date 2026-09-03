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

namespace AvePoint.RA.Browser.IndividualLevel
{
    public class WebLevel : IndividualBase
    {
        public WebLevel(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
            : base(objectModel, sqlConnString, siteUrl)
        {

        }

        public List<SPTreeNodeDto> GetWebs(Guid parentWebId, int siteLockStatus, int startIndex, uint perPage, ref int childrenCount)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> webs = new List<SPTreeNodeDto>();
            List<AveWebBrowserInfo> websInfo = Query.GetBrowserWebs(parentWebId, startIndex, perPage, ref childrenCount);//parentWeb.GetWebs();
            foreach (AveWebBrowserInfo webInfo in websInfo)
            {
                webs.Add(ConvertToDto(webInfo, siteLockStatus));
            }
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower Webs Elasped Time: {0}, WebCount: {1}, ParentWebId: {2}", sw.Elapsed.ToString(), webs.Count, parentWebId);
#endif
            return webs;
        }

        protected SPTreeNodeDto ConvertToDto(AveWebBrowserInfo web, int siteLockStatus)
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
            webDto.SiteLockStatus = siteLockStatus;
            webDto.NodeExtension = FillNodeExtension(webDto.NodeExtension, web);
            return webDto;
        }

        private SPTreeNodeDto ConvertToDto(IAveWeb web)
        {
            SPTreeNodeDto webDto = new SPTreeNodeDto();
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
            webDto.NodeExtension.TemplateName = web.WebTemplate + "#" + web.Configuration.ToString();
            //try// bpos-s does not support this
            //{
            //    webDto.NodeExtension.TemplateTitle = web.Site.GetWebTemplates(web.Language)[webDto.NodeExtension.TemplateName].Title;
            //}
            //catch (Exception e)
            //{
            //    Logger.Debug("BPOS-S does not support TemplateTitle, Error Message: {0}", e.ToString());
            //}
            webDto.NodeExtension.LCID = web.Language;
            webDto.Title = web.Title;
            webDto.InheritingPermissions = !web.HasUniqueRoleAssignments;
            webDto.Level = NodeLevel.Site;
            webDto.FarmID = FarmId;
            webDto.NodeExtension = FillNodeExtension(webDto.NodeExtension, web);
            return webDto;
        }

        public SPTreeNodeDto GetBrowserRootWeb(Guid siteId, int siteLockStatus)
        {
            AveWebBrowserInfo rootWeb = Query.GetBrowserRootWeb();
            return ConvertToDto(rootWeb, siteLockStatus);
        }

        public SPTreeNodeDto ConvertToWebDto(IAveWeb web)
        {
            return ConvertToDto(web);
        }
    }
}
