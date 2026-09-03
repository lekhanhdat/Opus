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

namespace AvePoint.ObjectModel.Common
{
    class AveHelpContextManager : AveClientObject, IAveHelpContextManager
    {
        private IAveRequest mRequest;
        private Dictionary<string, string> m_AvailableCollections;
        private StringBuilder m_CheckedHelpInfo = new StringBuilder();
        private StringBuilder m_DisableHelpInfo = new StringBuilder();

        public AveHelpContextManager() { }

        public string[] GetSiteDisabledHelpCollections(IAveSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            if (m_AvailableCollections == null)
            {
                GetSiteHelpCollection(site);
            }
            return m_DisableHelpInfo.ToString().TrimEnd('#').Split('#');
            //string[] strArray = null;
            //AveWeb rootWeb = site.RootWeb as AveWeb;
            //if (rootWeb.Properties != null && rootWeb.Properties.ContainsKey("DisabledHelpCollections"))
            //{
            //    string str = rootWeb.Properties["DisabledHelpCollections"];
            //    string[] separator = new string[] { ";#" };
            //    strArray = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            //}
            //return strArray;
        }

        public string[] GetSiteEnabledHelpCollections(IAveSite site)
        {
            if (m_AvailableCollections == null)
            {
                GetSiteHelpCollection(site);
            }
            return m_CheckedHelpInfo.ToString().TrimEnd('#').Split('#');
        }

        private void GetSiteHelpCollection(IAveSite site)
        {
            m_AvailableCollections = new Dictionary<string, string>();
            if (mRequest == null)
            {
                mRequest = (site as AveSite).Request;
            }
            List<string> helpCollection = mRequest.GetSiteEnabledHelpCollections();
            foreach (string Info in helpCollection)
            {
                string[] helpInfo = Info.Split('#');
                if (helpInfo.Length > 2)
                {
                    m_CheckedHelpInfo.Append(helpInfo[1] + "#");
                }
                else
                {
                    m_DisableHelpInfo.Append(helpInfo[1] + "#");
                }
                m_AvailableCollections.Add(helpInfo[1], helpInfo[0]);
            }
        }

        public string ContextWebHelpUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ContextWebHelpUrl");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ProductHelpLibraryUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ProductHelpLibraryUrl");
            }
        }

        public bool IsValidHelpLibraryUrl(Uri helpLibraryUrl)
        {
            throw new NotImplementedException();
        }

        public void SetSiteDisabledHelpCollections(IAveSite site, string[] disabledHelpCollections)
        {

        }

        public void SetSiteEnabledHelpCollections(IAveSite site, string[] enabledHelpCollections)
        {
            if (mRequest == null)
            {
                mRequest = (site as AveSite).Request;
            }
            mRequest.SetSiteEnabledHelpCollections(enabledHelpCollections);
        }

        public Dictionary<string, string> AvailableCollections
        {
            get
            {
                return m_AvailableCollections;
            }
        }
    }
}
