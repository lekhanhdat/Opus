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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Model;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;

namespace RATeams.Discover.Base
{
    public class ClientContextHelper
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(ClientContextHelper));
        public ClientContext Context { get; set; }
        public RegionalSettings RegionalSetting { get; set; }
        public IAveTimeZone SPWebTimeZone { get; set; }
        public void InitClientContext(NodeItem site, AveBPOSAccountInfo bposInfo)
        {
            CommonClientContext commonContext = new CommonClientContext();
            Context = commonContext.InitClientContext(new RMSPTreeNode()
            {
                BposInfo = site.BposInfo,
                FullPath = site.FullPath,
                Level = (int)NodeLevel.SiteCollection
            }, site.BposInfo == null ? bposInfo : null);
        }
        public RegionalSettings GetRegionalSetting(string webServerRelativeUrl)
        {
            Web web = Context.Site.OpenWeb(webServerRelativeUrl);
            Context.Load(web);
            RegionalSettings regionalSettings = web.RegionalSettings;
            Context.ExecuteQuery();
            return regionalSettings;
        }
        public DateTime GetDateTimeValue(DateTime dt)
        {
            try
            {
                if (RegionalSetting != null)
                {
                    var utcTime = RegionalSetting.TimeZone.UTCToLocalTime(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified));
                    Context.ExecuteQuery();
                    return utcTime.Value;
                }
                else
                {
                    //dt = DateTime.Parse(item[fieldName].ToString());
                    return SPWebTimeZone.UTCToLocalTime(dt);
                }
            }

            catch (Exception ex)
            {
                _logger.Warn("Get datetime field value failed", ex.ToString());
                try
                {
                    return SPWebTimeZone.UTCToLocalTime(dt);
                }
                catch (Exception e1)
                {
                    _logger.Warn("Get datetime field value failed", e1.ToString());
                }
            }
            return new DateTime();
        }
    }
}
