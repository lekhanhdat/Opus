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
using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using System;
using System.Configuration;
using System.Xml;

namespace AvePoint.RA.FileSystem.Collect
{
    //todo  hyw for long term, need to watch the config file.  it can be changed when the job  is running
    public class FSJobCommonConfig
    {
        private readonly AveLogger logger = new AveLogger(typeof(FSJobCommonConfig));
        public int DiscoveryThreadCount { get; set; }
        public int AnalyzerThreadCount { get; set; }
        public int PersistThreadCount { get; set; }
        public int DiscoveryCacheThrottling { get; set; }
        public int AnalyzerCacheThrottling { get; set; }
        public int PersistCacheThrottling { get; set; }
        public FSJobCommonConfig(XmlNode section)
        {
            //<FSJobConfig>
            //  <DiscoveryThreadCount>1</DiscoveryThreadCount>
            //  <AnalyzerThreadCount>2</AnalyzerThreadCount>
            //  <PersistThreadCount>1</PersistThreadCount>
            //  <DiscoveryCacheThrottling>1</DiscoveryCacheThrottling>
            //  <AnalyzerCacheThrottling>1</AnalyzerCacheThrottling>
            //  <PersistCacheThrottling>1</PersistCacheThrottling>
            //</FSJobConfig>
            DiscoveryThreadCount = 1;
            AnalyzerThreadCount = 1;
            PersistThreadCount = 1;
            DiscoveryCacheThrottling = 100000;
            AnalyzerCacheThrottling = 5000000;
            PersistCacheThrottling = 300000;
            try
            {
                foreach (XmlNode node in section.ChildNodes)
                {
                    try
                    {
                        if (node.Name.Eq("DiscoveryThreadCount"))
                        {
                            DiscoveryThreadCount = int.Parse(node.InnerText);
                            continue;
                        }
                        if (node.Name.Eq("AnalyzerThreadCount"))
                        {
                            AnalyzerThreadCount = int.Parse(node.InnerText);
                            continue;
                        }
                        if (node.Name.Eq("PersistThreadCount"))
                        {
                            PersistThreadCount = int.Parse(node.InnerText);
                            continue;
                        }
                        if (node.Name.Eq("DiscoveryCacheThrottling"))
                        {
                            DiscoveryCacheThrottling = int.Parse(node.InnerText);
                            continue;
                        }
                        if (node.Name.Eq("AnalyzerCacheThrottling"))
                        {
                            AnalyzerCacheThrottling = int.Parse(node.InnerText);
                            continue;
                        }
                        if (node.Name.Eq("PersistCacheThrottling"))
                        {
                            PersistCacheThrottling = int.Parse(node.InnerText);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to parse the ndoe{0}. Exception:{1}", node.Name.LogBase64(), e.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get configurations from config file. Exception:{0}", ex.ToString());
            }
        }
    }

    public class FSCollectJobConfigHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            return new FSJobCommonConfig(section);
        }
    }
}
