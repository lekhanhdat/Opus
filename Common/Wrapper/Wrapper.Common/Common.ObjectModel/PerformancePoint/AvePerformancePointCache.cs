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
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public class AvePerformancePointCache
    {
        private static AvePerformancePointCache mInstance;
        private readonly static object mLock = new object();

        public Dictionary<Guid, Dictionary<Guid, List<int>>> ScoreCardInfoMapping = new Dictionary<Guid, Dictionary<Guid, List<int>>>();

        public Dictionary<Guid, Dictionary<Guid, List<int>>> KPIInfoMapping = new Dictionary<Guid, Dictionary<Guid, List<int>>>();

        public Dictionary<Guid, Dictionary<Guid, List<int>>> IndicatorInfoMapping = new Dictionary<Guid, Dictionary<Guid, List<int>>>();

        public Dictionary<Guid, Dictionary<Guid, List<int>>> DashBoardInfoMapping = new Dictionary<Guid, Dictionary<Guid, List<int>>>();

        public Dictionary<Guid, Dictionary<Guid, List<int>>> FilterInfoMapping = new Dictionary<Guid, Dictionary<Guid, List<int>>>();

        public Dictionary<Guid, Dictionary<Guid, List<int>>> ReportInfoMapping = new Dictionary<Guid, Dictionary<Guid, List<int>>>();

        public Dictionary<string, XmlElement> DataSourceInfoMapping = new Dictionary<string, XmlElement>();

        public static void AddToProcessInPostAction(AveBaseItemInfo info)
        {
            if (info.AveItem.ListItem.ContentType != null)
            {
                string type = info.AveItem.ListItem.ContentType.Name;
                switch (type)
                {
                    case "PerformancePoint KPI":
                        AddToMapping(info, WrapperRuntime.WrapperCache.PerformancePointCache.KPIInfoMapping);
                        break;
                    case "PerformancePoint Indicator":
                        AddToMapping(info, WrapperRuntime.WrapperCache.PerformancePointCache.IndicatorInfoMapping);
                        break;
                    case "PerformancePoint Report":
                        AddToMapping(info, WrapperRuntime.WrapperCache.PerformancePointCache.ReportInfoMapping);
                        return;
                    case "PerformancePoint Scorecard":
                        AddToMapping(info, WrapperRuntime.WrapperCache.PerformancePointCache.ScoreCardInfoMapping);
                        break;
                    case "PerformancePoint Filter":
                        AddToMapping(info, WrapperRuntime.WrapperCache.PerformancePointCache.FilterInfoMapping);
                        break;
                    case "PerformancePoint Dashboard":
                        AddToMapping(info, WrapperRuntime.WrapperCache.PerformancePointCache.DashBoardInfoMapping);
                        break;
                    default:
                        return;
                }
            }

        }

        private static void AddToMapping(AveBaseItemInfo info, IDictionary<Guid, Dictionary<Guid, List<int>>> infoMapping)
        {
            if (!infoMapping.ContainsKey(info.AveItem.Web.ID))
            {
                infoMapping.Add(info.AveItem.Web.ID, new Dictionary<Guid, List<int>>() { { info.GUID, new List<int>() { info.Version } } });
            }
            else
            {
                if (infoMapping[info.AveItem.Web.ID].ContainsKey(info.GUID))
                {
                    infoMapping[info.AveItem.Web.ID][info.GUID].Add(info.Version);
                }
                else
                {
                    infoMapping[info.AveItem.Web.ID].Add(info.GUID, new List<int>() { info.Version });
                }
            }
        }

        public static AvePerformancePointCache GetInstance()
        {
            if (mInstance == null)
            {
                lock (mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new AvePerformancePointCache();
                    }
                }
            }
            return mInstance;
        }

        public void ClearInfoMapping()
        {
            ScoreCardInfoMapping.Clear();
            KPIInfoMapping.Clear();
            IndicatorInfoMapping.Clear();
            DashBoardInfoMapping.Clear();
            FilterInfoMapping.Clear();
            ReportInfoMapping.Clear();
            DataSourceInfoMapping.Clear();
        }
    }

}
