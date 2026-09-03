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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AvePoint.RA.RADataBroker.Extension
{
    //public static class DAOAPIClientExtension
    //{
    //    private static IRALogger logger = RALogger.GetInstance(typeof(DAOAPIClientExtension));
    //    public static RemoteSiteCollection GetSiteNode(this DAOAPIClientV1 mDocAveClient, string fullPath)
    //    {
    //        ThrowUtil.ThrowIfNull(fullPath, "SiteCollection Url");

    //        Func<RemoteSiteCollection> getObj = () =>
    //        {
    //            var node = mDocAveClient.GetRemoteSiteCollectionByUrl(fullPath);
    //            return node;
    //        };
    //        return CacheService.Get(CacheNamespace.O365Site, fullPath, getObj, TimeSpan.FromHours(12));
    //    }

    //    public static RemoteSiteCollection GetSiteNode(this DAOAPIClientV1 mDocAveClient, Guid aveId)
    //    {
    //        ThrowUtil.ThrowIfNull(aveId, "SiteCollection Id");
    //        Func<RemoteSiteCollection> getObj = () =>
    //        {
    //            List<string> aveIds = new List<string>();
    //            aveIds.Add(aveId.ToString());
    //            var node = mDocAveClient.GetRemoteSiteCollectionsByIdList(aveIds).FirstOrDefault();
    //            return node;
    //        };
    //        return CacheService.Get(CacheNamespace.O365Site, aveId.ToString(), getObj, TimeSpan.FromHours(12));
    //    }

    //    public static RemoteSiteCollection GetRemoteSiteCollectionByListUrl(this DAOAPIClientV1 client, string listUrl)
    //    {
    //        RemoteSiteCollection matchSite = null;
    //        try
    //        {
    //            Stopwatch watch = new Stopwatch();
    //            watch.Start();
    //            listUrl = HttpUtility.UrlDecode(listUrl);
    //            var sites = client.GetAuthorisedRemoteSiteCollectionsByUser();
    //            if (sites != null && sites.Count > 0)
    //            {
    //                matchSite = sites.OrderByDescending(a => a.url.Length).Where(s => listUrl.StartsWith(s.url)).FirstOrDefault();
    //            }
    //            watch.Stop();
    //            logger.Info("Get RemoteSiteCollection by list url, Take Milliseconds: {0} ms .", watch.ElapsedMilliseconds);
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Warn("Error Get RemoteSiteCollection By List Url :{1}, message:{0}", ex.Message, listUrl);
    //        }
    //        return matchSite;
    //    }

    //}
}
