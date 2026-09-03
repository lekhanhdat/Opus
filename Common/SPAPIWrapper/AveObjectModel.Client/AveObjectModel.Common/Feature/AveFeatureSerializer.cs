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
    internal class AveFeatureSerializer : IAveFeatureSerializer
    {
        private AveSite m_Site;
        private AveWeb m_Web;
        private AveFeatureScope m_Scope;
        private AveFeatureImport m_FeatureImportManager = null;
        private IAveRequest m_Request;
        private AveFeatureCollection m_FeatureCollection;

        public AveFeatureSerializer(AveSite site, IAveRequest request)
        {
            m_Site = site;
            m_Scope = AveFeatureScope.Site;
            m_Request = request;
            m_FeatureCollection = site.Features as AveFeatureCollection;
            m_FeatureImportManager = new AveFeatureImport(site, request);
        }

        public AveFeatureSerializer(AveWeb web, IAveRequest request)
        {
            m_Web = web;
            m_Scope = AveFeatureScope.Web;
            m_Request = request;
            m_FeatureCollection = web.Features as AveFeatureCollection;
            m_FeatureImportManager = new AveFeatureImport(web, request);
        }

        public AveFeatureInfoBox GetObjectData()
        {
            AveFeatureInfoBox featureBox = new AveFeatureInfoBox();
            foreach (AveFeature feature in m_FeatureCollection)
            {
                AveFeatureInfo info = new AveFeatureInfo();
                info.Id = feature.DefinitionId;
                info.Scope = m_Scope;
                featureBox.FeatureList.Add(info);
            }
            featureBox.FeatureList.Sort();
            return featureBox;
        }

        public object SetObjectData(List<AveFeatureInfo> featureInfoList)
        {
            if (featureInfoList != null)
            {
                m_FeatureImportManager.Run(featureInfoList);
            }
            return null;
        }
    }
}
