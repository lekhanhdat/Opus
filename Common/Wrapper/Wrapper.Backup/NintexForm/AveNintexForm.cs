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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace AvePoint.Wrapper.Backup
{
    public class AveNintexForm
    {
        private const string nintexFormString = "remoteAppUrl=https://formso365.nintex.com";
        protected IAveList list;

        public AveNintexForm(IAveList list)
        {
            this.list = list;
        }

        protected string BackupFormXml(string contentTypeId)
        {
            var stream = list.ExportNintexForm(contentTypeId);
            if (stream != null && stream.Length > 0)
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 判断该ContentType是否有Nintex Form
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="xmlDocuments"></param>
        /// <returns></returns>
        protected bool IncludeNintexForm(string contentTypeId, List<string> xmlDocuments)
        {
            if (xmlDocuments == null)
            {
                return false;
            }
            foreach (var xmlDocument in xmlDocuments)
            {
                if (xmlDocument.IndexOf(nintexFormString, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public string ExportNintexForm(string contentTypeId, List<string> xmlDocuments)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("AveNintexForm.BackupFormXml"))
            {
                if (IncludeNintexForm(contentTypeId, xmlDocuments))
                {
                    return BackupFormXml(contentTypeId);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
