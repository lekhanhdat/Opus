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
    public class AveNintexFormLocal : AveNintexForm
    {
        NfClientContext nfContext;
        public AveNintexFormLocal(AveSPList list) : base(list)
        {
        }
        protected override List<AveNintexFormInfo> BackupFormXml(string contentTypeId, NfClientContext nfContext = null)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("AveNintexForm.BackupFormXml"))
            {
                var formXmls = new List<AveNintexFormInfo>();
                var fileServerRelativeUrl = mAveSPList.ParentWeb.ContentTypesInfoIncludeNintexForm[mAveSPList.Id][contentTypeId];
                var file = mAveSPList.ParentWeb.SPWeb.GetFile(fileServerRelativeUrl);
                foreach (var fileVersion in file.Versions)
                {
                    //ADO-199030 only backup publish version.
                    if (IsPublishVersion(fileVersion.ID))
                    {
                        //nintex use alluserdata table tp_Modified
                        var versionModified = mAveSPList.ParentWeb.ParentSite.QueryService.GetVersionModified(mAveSPList.ParentSite.SPSite.ID, file.ParentFolder.UniqueId, file.Item.ID, fileVersion.ID);
                        formXmls.Add(ConvertToNintexFormInfo(versionModified, fileVersion.OpenBinaryStream()));
                    }
                }
                //ADO-199030 only backup publish version.
                if (IsPublishVersion(file.UIVersion))
                {
                    formXmls.Add(ConvertToNintexFormInfo(mAveSPList.ParentWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)file.Item["Modified"]), file.OpenBinaryStream()));
                }
                return formXmls;
            }
        }
        private bool IsPublishVersion(int version)
        {
            return version % 512 == 0;
        }
        private AveNintexFormInfo ConvertToNintexFormInfo(DateTime ModifiedTime, Stream stream)
        {
            return new AveNintexFormInfo
            {
                FormXml = GetFormXmlFileString(stream),
                VersionCreateTime = ModifiedTime,
            };
        }
        protected override bool IncludeNintexForm(string contentTypeId, List<string> xmlDocuments)
        {
            if (mAveSPList.ParentWeb.ContentTypesInfoIncludeNintexForm.ContainsKey(mAveSPList.Id)
                  && mAveSPList.ParentWeb.ContentTypesInfoIncludeNintexForm[mAveSPList.Id].ContainsKey(contentTypeId))
            {
                return true;
            }
            else
            {
                Dictionary<string, string> contentTypeIds = new Dictionary<string, string>();
                if (!mAveSPList.ParentWeb.ContentTypesInfoIncludeNintexForm.TryGetValue(Guid.Empty, out contentTypeIds)) //Guid.Empty表示site level的content type
                {
                    return false;
                }
                else
                {
                    foreach (string id in contentTypeIds.Keys)
                    {
                        if (contentTypeId.StartsWith(id, StringComparison.OrdinalIgnoreCase)) //当前content type的子content type
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }
        private NfClientContext GetNintexFormContext()
        {
            return new NfClientContext(mAveSPList.ParentWeb.SPWeb.Url, null, mAveSPList.ParentSite.ObjectModelFactory.ContextKind);
        }
        public override void Dispose()
        {
            if (nfContext != null)
            {
                nfContext.Dispose();
                nfContext = null;
            }
        }
    }
}
