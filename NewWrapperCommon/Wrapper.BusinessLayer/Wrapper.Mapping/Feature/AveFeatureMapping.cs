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


namespace AvePoint.Wrapper.Mapping
{
    using System;
    using AvePoint.Wrapper.Common;
    using System.Collections.Generic;
    using System.Xml;
    using System.Linq;

    public class AveFeatureMapping : IAveFeatureMapping
    {
        private Dictionary<Guid, AveFeatureInfo> featureMapping;
        public AveFeatureMapping(Dictionary<Guid, AveFeatureInfo> mapping)
        {
            this.featureMapping = mapping;
        }

        public AveFeatureMapping(XmlElement config)
        {
            if (config == null)
            {
                throw new InvalidOperationException("The xml is invalid, config is empty or not a XmlElement.");
            }
            try
            {
                this.featureMapping = config.Cast<XmlElement>().ToDictionary(child => new Guid(child.GetAttribute("SourceGuid")), child => ConvertToFeatureInfo(child));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The xml is invalid", ex);
            }
        }

        private AveFeatureInfo ConvertToFeatureInfo(XmlElement ele)
        {
            return new AveFeatureInfo()
            {
                Id = new Guid(ele.GetAttribute("DestGuid")),
                Dependencies = ele.Cast<XmlElement>().Select(child => new Guid(child.InnerText)).ToList(),
            };
        }

        public AveFeatureInfo GetMappedFeatureInfo(Guid featureId)
        {
            if (featureMapping != null && featureMapping.ContainsKey(featureId))
            {
                return featureMapping[featureId];
            }
            return null;
        }
    }
}
