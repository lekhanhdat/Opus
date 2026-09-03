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
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Hybrid.Browser.Contract;
using AvePoint.RA.Hybrid.Browser.Util;
using AvePoint.RA.SharePoint.RMExplorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.Browser
{
    public class SharePointOnPremRealtimeJob : IBrowser
    {
        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(SharePointOnPremRealtimeJob));
        public HybridBrowserType BrowserType => HybridBrowserType.SharePointOnPremRealtimeJob;

        //private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        //{
        //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //};
        public string Browse(string message)
        {
            var result = new SharePointOnPremRealtimeJobResult();
            try
            {
                Logger.Info("Realtime job message: ", message);
                var args = SerializerHelper.DeserializeByJsonConvert<SharePointOnPremRealtimeJobArgs>(message);
                var jobMessage = SerializerHelper.DeserializeByDataContractSerializer<AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage>(args.Message);
                Task.Factory.StartNew(() => TakeRealtimeAction(jobMessage));
            }
            catch (Exception e)
            {
                Logger.Error($"An error occur while browse sharepoint tree node. Error: {e}");
                result.Result = SharePointOnPremRealtimeJobResultEnum.Failed;
                result.Message = e.Message;
            }
            return SerializerHelper.SerializeByJsonSerializer(result);
        }

        private void TakeRealtimeAction(AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage)
        {
            try
            {
                var action = jobMessage.Action;
                RMExplorerUtility explorerUtility = new RMExplorerUtility();
                switch (action)
                {
                    case RA.Contract.Global.JobMessage.RealTimeAction.ChangeTerm:
                        explorerUtility.ChangeAllTerms(jobMessage.ChangeTermOption, jobMessage.JobId, false);
                        break;
                    case RA.Contract.Global.JobMessage.RealTimeAction.Declare:
                    case RA.Contract.Global.JobMessage.RealTimeAction.UnDeclare:
                        bool isDeclare = action == RA.Contract.Global.JobMessage.RealTimeAction.Declare;
                        explorerUtility.DeclaredRecords(jobMessage.DeclareIds, jobMessage.JobId, isDeclare, jobMessage.DeclaredBy);
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while taking realtime action. Jobid:{0} Error:{1}", jobMessage?.JobId, e.ToString());
            }
        }
    }
}
