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
namespace AvePoint.Wrapper.Common
{
    using System.Collections.Generic;
    using System.Linq;

    public class RecordingDrive
    {
        public const string PropertyName = "RecordingDrive";

        public RecordingDrive()
        {
        }

        public Dictionary<string, AveWrapperI18NException> Report { get; private set; }
        public IList<string> Urls { get; } = new List<string>();

        /// <summary>
        /// If any exception occurred when retrieve recording drive of the site/onedrive
        /// </summary>
        /// <param name="keywords">server relatie url of the site or the email of the onedrive</param>
        /// <param name="report">Server relative url of the parent folder of the recording folder and the exception</param>
        /// <returns></returns>
        public bool HasException(string keywords, out Dictionary<string, AveWrapperI18NException> report)
        {
            report = null;
            if (Report != null)
            {
                report = Report.Where(kv => kv.Key.IndexOf(keywords) >= 0)
                    .ToDictionary(kv => kv.Key, kv=>kv.Value);
                return report.Any();
            }
            return false;
        }

        public void AddReport(string keywords, AveWrapperI18NException exception)
        {
            if (exception != null && !string.IsNullOrEmpty(keywords))
            {
                if (Report == null)
                {
                    Report = new Dictionary<string, AveWrapperI18NException>();
                }

                Report[keywords] = exception;
            }
        }
    }
}