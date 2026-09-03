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
namespace ExchangeUtility.Graph
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using AvePoint.RA.CommonUtil;
    #endregion
    public abstract class YammerGroupServiceBase : IDisposable
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(YammerGroupServiceBase));
        protected YammerExportAPIService yeAPIService;
        protected YammerRestAPIService yrAPIService;
        protected IAuthObject authObj;

        #region Backup

        public ExportResult GetNetworkInfoByExport()
        {
            return yeAPIService.GetNetworkInfo($"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")}+00:00");
        }

        public YammerNetwork GetNetWorkInfoByRest()
        {
            return this.yrAPIService.GetNetwork();
        }

        public YammerUser GetYammerUserByRest(string userId)
        {
            return yrAPIService.GetYammerUser(userId);
        }

        public ExportResult GetGroupByExport(string since, string until)
        {
            return yeAPIService.GetGroup(since, until);
        }

        public void GetGroupMembership(string groupId)
        {

        }

        public ExportResult GetMessageByExport(string since, string until)
        {
            return yeAPIService.GetMessage(since, until);
        }

        public ExportResult GetMessageAndGroupByExport(string since, string until)
        {
            return yeAPIService.GetMessageAndGroup(since, until);
        }

        #endregion

        #region Restore

        public bool CreateGroup()
        {
            return false;
        }

        public bool UpdateGroup()
        {
            return false;
        }

        public bool UpdateGroupMembership()
        {
            return false;
        }

        public bool CreateThread()
        {
            return false;
        }

        public bool CreateReply()
        {
            return false;
        }

        public bool UpdateMessage()
        {
            return false;
        }


        #endregion

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }

    }

    public class YammerGroupSericeWithYammerApp : YammerGroupServiceBase
    {
        public YammerGroupSericeWithYammerApp(IAuthObject authObj, string exportLocation)
        {
            var yammerAuthObj = authObj as YammerAppTokenAuthObject;
            yeAPIService = new YammerExportAPIService(yammerAuthObj.GetAccessToken, exportLocation)
            {
                RetryController = new YammerAPIRetry()
            };
            yrAPIService = new YammerRestAPIService(yammerAuthObj.GetAccessToken)
            {
                RetryController = new YammerAPIRetry()
            };
            base.authObj = authObj;
            logger.Info($"Create yammer api service finished. Used refreshToken. ExportAPIService: [{yeAPIService != null}]. RestAPIService: [{yrAPIService != null}]. ");
        }
    }
}