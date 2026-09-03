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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using RAReportCenter.ClientAuditReport.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    public abstract class CloudInsightsConnectorBase : IDisposable
    {
        #region private fields

        private static IRALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region internal fields
        internal string tmpFolder = string.Empty;
        internal Queue<AuditDownloadDataInfo> mAuditFileQueue = new Queue<AuditDownloadDataInfo>();
        #endregion

        #region events
        public EventHandler<SetCalculatedCountEventArgs> SetRportCount;
        public EventHandler<IncreaseProgressEventArgs> IncreaseProgress;
        #endregion

        #region public fields
        internal Queue<AuditDownloadDataInfo> AuditFileQueue
        {
            get
            {
                return mAuditFileQueue;
            }
        }
        #endregion

        #region ctor
        public CloudInsightsConnectorBase(string jobid)
        {
            InitTempFolder(jobid);
            //SPAuditReportUtility.GetTempFolder(TenantLocalValue.LogonGroupId, jobid);
        }
        #endregion


        #region public methods
        public abstract void Run();

        public abstract void SetAuditPacketStore(bool packetStore, List<string> siteUrls);

        public abstract Dictionary<string, string> GetUserMappings();

        public void Add2Queue(AuditDownloadDataInfo auditFileInfo)
        {
            lock (mAuditFileQueue)
            {
                while (mAuditFileQueue.Count > 1)
                {
                    Monitor.Wait(mAuditFileQueue);
                }

                mAuditFileQueue.Enqueue(auditFileInfo);
                Monitor.Pulse(mAuditFileQueue);
            }
        }

        public List<string> GetSCFirstTwoLetters(List<string> nodes)
        {
            var letters = new List<string>();

            if (nodes == null)
            {
                return null;
            }
            foreach (var node in nodes)
            {
                if (string.IsNullOrEmpty(node))
                {
                    continue;
                }
                var siteId = SPAuditReportUtility.GetAveId(node);
                var letterStr = GetSiteFolderId(siteId);
                if (!letters.Contains(letterStr))
                {
                    letters.Add(letterStr);
                }
            }
            return letters;
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(tmpFolder) && Directory.Exists(tmpFolder))
            {
                try
                {
                    Directory.Delete(tmpFolder, true);
                }
                catch (Exception e)
                {
                    mLog.Warn($"Failed to delete temp folder {tmpFolder} error {e.ToString()}");
                }
            }
        }
        #endregion

        #region internal methods

        /// <summary>
        /// 把时间转换成每日00:00:00
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal DateTime TruncateToMonthDay(DateTime dateTime)
        {
            return TruncateToMonthDay(dateTime.Ticks);
        }

        internal string GetNewFolderInTemp()
        {
            string newTemp = Path.Combine(tmpFolder, Guid.NewGuid().ToString());
            if (!Directory.Exists(newTemp))
            {
                Directory.CreateDirectory(newTemp);
            }
            return newTemp;
        }
        #endregion

        #region private methods

        #region new store data format for site collection
        private string GetSiteFolderId(string siteId)
        {
            var firstChar = 'A';
            var secondChar = 'A';

            var siteIdChars = siteId.ToCharArray();
            for (var i = 0; i < siteIdChars.Length; i++)
            {
                var c = siteIdChars[i];
                if (char.IsLetter(c))
                {
                    firstChar = c;
                    break;
                }
            }

            for (var i = siteIdChars.Length - 1; i > 0; i--)
            {
                var c = siteIdChars[i];
                if (char.IsLetter(c))
                {
                    secondChar = c;
                    break;
                }
            }

            return new string(new char[] { firstChar, secondChar }).ToUpper();
        }
        #endregion

        /// <summary>
        /// 把时间转换成每月1日
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /*private DateTime TruncateToMonth(long input)
        {
            var time = new DateTime(input);
            var output = new DateTime(time.Year, time.Month, 1, 0, 0, 0);
            return output;
        }
*/
        /// <summary>
        /// 把时间转换成每日00:00:00
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private DateTime TruncateToMonthDay(long input)
        {
            var time = new DateTime(input);
            var output = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            return output;
        }

        private void InitTempFolder(string key)
        {
            string path = Path.Combine(AveEnv.AgentTempFolder, @"AuditReport");
            path = Path.Combine(path, TenantLocalValue.LogonGroupId);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            tmpFolder = Path.Combine(path, key);
            if (!Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }
        }

        #endregion

    }

    public class AuditDownloadDataInfo
    {
        internal string FileFolder { get; set; }

    }
}