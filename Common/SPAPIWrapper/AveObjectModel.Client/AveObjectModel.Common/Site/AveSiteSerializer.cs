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




namespace AvePoint.ObjectModel.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    #endregion

    internal class AveSiteSerializer : IAveSiteSerializer
    {
        private AveSite m_Site;
        private AvePoint.GCommon.AveLogger logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(AveSiteSerializer));
        public AveSiteSerializer(AveSite site)
        {
            m_Site = site;
        }

        public AveSiteInfo GetObjectData()
        {
            AveSiteInfo siteInfo = new AveSiteInfo();

            if (m_Site.WebApplication != null)
            {
                siteInfo.WebAppUrl = m_Site.WebApplication.GetResponseUri(AveUrlZone.Default).ToString();
            }
            else
            {
                if (!m_Site.ServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase))
                {
                    siteInfo.WebAppUrl = m_Site.Url.Split(m_Site.ServerRelativeUrl.ToCharArray())[0];
                }
                else
                {
                    siteInfo.WebAppUrl = m_Site.Url;
                }
            }
            siteInfo.IsHostheader = m_Site.HostHeaderIsSiteName;
            siteInfo.ServerRelativeUrl = m_Site.ServerRelativeUrl;
            siteInfo.Url = m_Site.RootWeb.Url;
            siteInfo.Title = m_Site.RootWeb.Title;
            siteInfo.Description = m_Site.RootWeb.Description;
            siteInfo.LCID = m_Site.RootWeb.Language;
            siteInfo.CompatibilityLevel = m_Site.CompatibilityLevel;  //Archiver Site Collection 时需要。
            if (m_Site.RootWeb.WebTemplate != null)
            {
                siteInfo.WebTemplate = m_Site.RootWeb.WebTemplate + "#" + m_Site.RootWeb.Configuration;
            }

            IAveUser siteOwner = m_Site.Owner;
            if (siteOwner != null)
            {
                siteInfo.OwnerLogin = siteOwner.LoginName;
                siteInfo.OwnerName = siteOwner.Name;
                siteInfo.OwnerEmail = siteOwner.Email;
            }

            IAveUser siteSecondaryContact = m_Site.SecondaryContact;
            if (siteSecondaryContact != null)
            {
                siteInfo.SecondaryContactLogin = siteSecondaryContact.LoginName;
                siteInfo.SecondaryContactName = siteSecondaryContact.Name;
                siteInfo.SecondaryContactEmail = siteSecondaryContact.Email;
            }
            try
            {
                siteInfo.StorageMaximumLevel = m_Site.Quota.StorageMaximumLevel;
                siteInfo.UserCodeMaximumLevel = m_Site.Quota.UserCodeMaximumLevel;
            }
            catch (Exception e)
            {
                logger.Warn("[SAAS-38695]An error occured when get site quota due to {0}", e);
            }
            siteInfo.UniqueId = m_Site.ID;

            return siteInfo;
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
