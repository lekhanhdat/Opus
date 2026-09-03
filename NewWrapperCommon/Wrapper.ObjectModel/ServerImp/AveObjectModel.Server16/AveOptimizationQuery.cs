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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveOptimizationQuery : IOptimizationService, IDisposable
    {
        //private IAveCommonQueryService m_QueryService = null;
        private static AveOptimizationQuery m_instance = null;
        private static object m_Lock = new object();
        private AveOptimizationQuery()
        {

        }

        public static AveOptimizationQuery CreateInstance()
        {
            if (m_instance == null)
            {
                lock (m_Lock)
                {
                    if (m_instance == null)
                    {
                        m_instance = new AveOptimizationQuery();
                    }
                }
            }

            return m_instance;
        }

        #region Navigation
        public string GetNavigationNodeMetainfo(AveWeb web, int Eid)
        {
            return ((AveSite)web.Site).QueryService.GetNavigationNodeMetainfo(web, Eid);
        }

        #endregion

        #region Feature
        public AveFeatureInfoBox GetSiteFeatures(AveSite site)
        {
            return site.QueryService.GetFeatures(site.ID, new Guid("00000000-0000-0000-0000-000000000000"), AveFeatureScope.Site);
        }

        public AveFeatureInfoBox GetWebFeatures(AveWeb web)
        {
            return ((AveSite)web.Site).QueryService.GetFeatures(web.Site.ID, web.ID, AveFeatureScope.Web);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //if (m_QueryService != null)
            //{
            //    m_QueryService.Dispose();
            //    m_QueryService = null;
            //}
        }

        #endregion
    }
}
