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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Backup
{
    public class AveNintexFormOnline : AveNintexForm
    {
        private const string nintexFormString = "remoteAppUrl=https://formso365.nintex.com";
        public AveNintexFormOnline(AveSPList list) : base(list)
        {
        }
        protected override List<AveNintexFormInfo> BackupFormXml(string contentTypeId, NfClientContext nfContext = null)
        {
            var stream = mAveSPList.SPList.ExportNintexForm(contentTypeId);
            if (stream != null && stream.Length > 0)
            {
                using (var reader = new StreamReader(stream))
                {
                    return new List<AveNintexFormInfo>() { new AveNintexFormInfo { FormXml = reader.ReadToEnd() } };
                }
            }
            return new List<AveNintexFormInfo>();
        }
        protected override bool IncludeNintexForm(string contentTypeId, List<string> xmlDocuments)
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
        public override void Dispose()
        {
            return;
        }
    }
}
