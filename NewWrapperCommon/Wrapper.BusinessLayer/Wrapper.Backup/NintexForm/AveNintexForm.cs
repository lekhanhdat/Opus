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
    public abstract class AveNintexForm : IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected AveSPList mAveSPList;

        public AveNintexForm(AveSPList list)
        {
            this.mAveSPList = list;
        }

        public static AveNintexForm CreateNintexForm(AveSPList list)
        {
            if (list.ParentSite.SPSite.IsOnlineSite)
            {
                return new AveNintexFormOnline(list);
            }
            else
            {
                return new AveNintexFormLocal(list);

            }
        }
        protected abstract List<AveNintexFormInfo> BackupFormXml(string contentTypeId, NfClientContext nfContext = null);
        protected abstract bool IncludeNintexForm(string contentTypeId, List<string> xmlDocuments);
        public abstract void Dispose();

        public List<AveNintexFormInfo> BackupFormXmlForUnitTest(string contentTypeId, NfClientContext context)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("AveNintexForm.BackupFormXml"))
            {
                if (IncludeNintexForm(contentTypeId,null))
                {
                    return BackupFormXml(contentTypeId, context);
                }
                else
                {
                    return new List<AveNintexFormInfo>();
                }
            }
        }
        public List<AveNintexFormInfo> ExportNintexForm(string contentTypeId,List<string> xmlDocuments)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("AveNintexForm.BackupFormXml"))
            {
                if (IncludeNintexForm(contentTypeId, xmlDocuments))
                {
                    return BackupFormXml(contentTypeId, null);
                }
                else
                {
                    return new List<AveNintexFormInfo>();
                }
            }
        }

        protected string GetFormXmlFileString(Stream stream)
        {
            using (stream)
            {
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}
