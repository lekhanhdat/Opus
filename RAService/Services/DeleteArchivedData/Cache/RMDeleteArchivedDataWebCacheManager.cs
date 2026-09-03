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
using AvePoint.RA.Service.Services.DeleteArchivedData;
using AvePoint.RA.Service.Services.DeleteArchivedData.Archived;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.Cache
{
    public class RMDeleteArchivedDataWebCacheManager
    {
        private readonly RMArchivedIndexDBOperator _indexDBOperator;

        private readonly Dictionary<string, string> _webMapping = [];

        public RMDeleteArchivedDataWebCacheManager(RMArchivedIndexDBOperator indexDBOperator)
        {
            _indexDBOperator = indexDBOperator;
        }

        public string GetWebRelativeUrl(string containerPathMd5)
        {
            if (_webMapping.ContainsKey(containerPathMd5))
            {
                return _webMapping[containerPathMd5];
            }

            var containerPathMd5s = new List<string>() { containerPathMd5 };
            string webRelativeUrl;
            while (true)
            {
                var containerItem = _indexDBOperator.GetContainerItem(containerPathMd5);
                if (containerItem.Type == "W")
                {
                    webRelativeUrl = containerItem.Name == "." ? "" : containerItem.Name;
                    break;
                }
                if (_webMapping.ContainsKey(containerPathMd5))
                {
                    webRelativeUrl = _webMapping[containerPathMd5];
                    break;
                }

                containerPathMd5s.Add(containerItem.ParentPathMD5);
                containerPathMd5 = containerItem.ParentPathMD5;
            }

            foreach (var md5 in containerPathMd5s)
            {
                _webMapping[md5] = webRelativeUrl;
            }

            return _webMapping[containerPathMd5];
        }
    }
}
