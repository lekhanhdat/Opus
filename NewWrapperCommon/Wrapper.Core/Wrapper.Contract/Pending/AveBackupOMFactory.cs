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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Contract
{
    public abstract class AveBackupOMFactory
    {
        internal const string BackupAssemblyName = "AgentCommonWrapperBackup";
        internal const string BackupTypeName = "AvePoint.Wrapper.Backup.AveWrapperBackupOMFactory";

        public abstract IAveSPGlobalConfiguration GlobalConfiguration { get; }

        public static AveBackupOMFactory CreateBackupOMFactory()
        {
            return AveAssemblyUtility.CreateInstance(BackupAssemblyName, BackupTypeName) as AveBackupOMFactory;
        }
        public abstract IAveSPWebApp CreateAveSPWebApp(string url, IAveBackupStream sender);
        public abstract IAveSPSite CreateAveSPSite(string _url, AveContextKind contextKind, AveBPOSAccountInfo userAccountInfo, IAveBackupStream _stream);
        public abstract IAveSPSite CreateAveSPSite(IAveSite site, string databaseConnectionString, IAveBackupStream _stream, AveObjectModelFactory factory);
        public abstract IAveSPMySite CreateAveSPMySite(IAveSPSite site);
        public abstract IAveSPMySite CreateAveSPMySite(IAveWebApplication webApp, string loginName, AveContextKind contextKind);
        public abstract IAveSPWeb CreateAveSPWeb(IAveSPSite _AveSite, Guid _WebId, string _name, bool enableReloadForTimeout = true);
        public abstract IAveSPList CreateAveSPList(IAveSPWeb _AveWeb, Guid _id, string _title);
        public abstract IAveSPList CreateAveSPList(IAveSPWeb _AveWeb, Guid _id, string _title, bool getFullSchema);
        public abstract IAveSPFolder CreateAveSPFolder(IAveSPList aveList);
        public abstract IAveSPFolder CreateAveSPFolder(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version);
        public abstract IAveSPFolder CreateAveSPFolder(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version, DateTime currentVersionModified);
        public abstract IAveSPDoc CreateAveSPDoc(IAveSPFolder aveFolder, Guid id, int rowId, int version, string serverRelativeUrl = null, int level = 0);
        public abstract IAveSPDoc CreateAveSPDoc(IAveSPFolder aveFolder, Guid id, int rowId, int version, string serverRelativeUrl, int level, DateTime currentVersionModified);
        public abstract IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version);
        public abstract IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version, string serverRelativeUrl);
        public abstract IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version, string serverRelativeUrl, DateTime currentVersionModified);
        public abstract IAveSPAttachment CreateAveSPAttachment(IAveSPFolder aveFolder, Guid id, string name, string serverRelativeUrl = null);
        public abstract IAveSPAttachment CreateAveSPAttachment(IAveSPFolder aveFolder, Guid id, string name, string serverRelativeUrl, IAveSPItem dependItem);
   
        public abstract IAveSPContentTypeHub CreateAveSPContentTypeHub(AveObjectModelFactory fac, IAveMetadataServiceApplication application);
        public abstract IAveSPContentTypeHub CreateAveSPContentTypeHub(AveObjectModelFactory fac, Guid applicationId);
        public abstract IAveSPContentTypeHub CreateAveSPContentTypeHub(AveObjectModelFactory fac, Guid applicationId, Guid partitionId);

        public abstract IAveSPTermStore CreateAveSPTermStore(AveObjectModelFactory modelFactory);
        public abstract IAveSPTermStore CreateAveSPTermStore(AveObjectModelFactory modelFactory, Guid applicationId);
        public abstract IAveSPTaxonomyGroup CreateAveSPTaxonomyGroup(IAveSPTermStore aveSPTermStore);
        public abstract IAveSPTermSet CreateAveSPTermSet(IAveSPTaxonomyGroup taxonomyGroup);
        public abstract IAveSPTerm CreateAveSPTerm(IAveSPTermSet aveTermSet);
        public abstract IAveSPTerm CreateAveSPTerm(IAveSPTerm aveTerm);
        public abstract IAveSPAppManager CreateAveSPApp(IAveSPWeb web, Guid productId);
    }
}
