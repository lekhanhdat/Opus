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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Backup
{
    public class AveWrapperBackupOMFactory : AvePoint.Wrapper.Contract.AveBackupOMFactory
    {
        static AveSPBackupGlobalConfiguration mGlobalConfiguration = new AveSPBackupGlobalConfiguration();

        public override IAveSPGlobalConfiguration GlobalConfiguration
        {
            get
            {
                return mGlobalConfiguration;
            }
        }

        public override IAveSPWebApp CreateAveSPWebApp(string url, IAveBackupStream sender)
        {
            return new AveSPWebApp(url, sender);
        }

        public override IAveSPSite CreateAveSPSite(string _url, AveContextKind contextKind, AveBPOSAccountInfo userAccountInfo, IAveBackupStream _stream)
        {
            return new AveSPSite(_url, contextKind, userAccountInfo, _stream);
        }

        public override IAveSPSite CreateAveSPSite(IAveSite site, string databaseConnectionString, IAveBackupStream _stream, AveObjectModelFactory factory)
        {
            return new AveSPSite(site, databaseConnectionString, _stream, factory);
        }

        public override IAveSPMySite CreateAveSPMySite(IAveSPSite site)
        {
            return new AveSPMySite(site as AveSPSite);
        }

        public override IAveSPMySite CreateAveSPMySite(IAveWebApplication webApp, string loginName, AveContextKind contextKind)
        {
            return new AveSPMySite(webApp, loginName, contextKind);
        }

        public override IAveSPWeb CreateAveSPWeb(IAveSPSite _AveSite, Guid _WebId, string _name, bool enableReloadForTimeout = true)
        {
            return new AveSPWeb(_AveSite as AveSPSite, _WebId, _name, enableReloadForTimeout);
        }

        public override IAveSPList CreateAveSPList(IAveSPWeb _AveWeb, Guid _id, string _title)
        {
            return new AveSPList(_AveWeb as AveSPWeb, _id, _title);
        }

        public override IAveSPList CreateAveSPList(IAveSPWeb _AveWeb, Guid _id, string _title, bool getFullSchema)
        {
            return new AveSPList(_AveWeb as AveSPWeb, _id, _title, getFullSchema);
        }

        public override IAveSPFolder CreateAveSPFolder(IAveSPList aveList)
        {
            return new AveSPFolder(aveList as AveSPList);
        }

        public override IAveSPFolder CreateAveSPFolder(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version)
        {
            return new AveSPFolder(aveFolder as AveSPFolder, name, id, rowId, version);
        }

        public override IAveSPFolder CreateAveSPFolder(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version, DateTime currentVersionModified)
        {
            return new AveSPFolder(aveFolder as AveSPFolder, name, id, rowId, version, currentVersionModified);
        }

        public override IAveSPDoc CreateAveSPDoc(IAveSPFolder aveFolder, Guid id, int rowId, int version, string serverRelativeUrl = null, int level = 0)
        {
            return new AveSPDoc(aveFolder as AveSPFolder, id, rowId, version, serverRelativeUrl, level);
        }

        public override IAveSPDoc CreateAveSPDoc(IAveSPFolder aveFolder, Guid id, int rowId, int version, string serverRelativeUrl, int level, DateTime currentVersionModified)
        {
            return new AveSPDoc(aveFolder as AveSPFolder, id, rowId, version, serverRelativeUrl, level, currentVersionModified);
        }

        public override IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version)
        {
            return new AveSPListItem(aveFolder as AveSPFolder, name, id, rowId, version);
        }

        public override IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version, string serverRelativeUrl)
        {
            return new AveSPListItem(aveFolder as AveSPFolder, name, id, rowId, version, serverRelativeUrl);
        }

        public override IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name, Guid id, int rowId, int version, string serverRelativeUrl, DateTime currentVersionModified)
        {
            return new AveSPListItem(aveFolder as AveSPFolder, name, id, rowId, version, serverRelativeUrl, currentVersionModified);
        }

        public override IAveSPAttachment CreateAveSPAttachment(IAveSPFolder aveFolder, Guid id, string name, string serverRelativeUrl = null)
        {
            return new AveSPAttachment(aveFolder as AveSPFolder, id, name, serverRelativeUrl);
        }

        public override IAveSPAttachment CreateAveSPAttachment(IAveSPFolder aveFolder, Guid id, string name, string serverRelativeUrl, IAveSPItem dependItem)
        {
            return new AveSPAttachment(aveFolder as AveSPFolder, id, name, serverRelativeUrl, dependItem as AveSPItem);
        }

        public override IAveSPTermStore CreateAveSPTermStore(AveObjectModelFactory modelFactory)
        {
            return new AveTermStore(modelFactory);
        }

        public override IAveSPTermStore CreateAveSPTermStore(AveObjectModelFactory modelFactory, Guid applicationId)
        {
            return new AveTermStore(modelFactory, applicationId);
        }

        public override IAveSPTaxonomyGroup CreateAveSPTaxonomyGroup(IAveSPTermStore aveSPTermStore)
        {
            return new AveTaxonomyGroup(aveSPTermStore as AveTermStore);
        }

        public override IAveSPTermSet CreateAveSPTermSet(IAveSPTaxonomyGroup taxonomyGroup)
        {
            return new AveTermSet(taxonomyGroup as AveTaxonomyGroup);
        }

        public override IAveSPTerm CreateAveSPTerm(IAveSPTermSet aveTermSet)
        {
            return new AveTerm(aveTermSet as AveTermSet);
        }

        public override IAveSPTerm CreateAveSPTerm(IAveSPTerm aveTerm)
        {
            return new AveTerm(aveTerm as AveTerm);
        }

        public override IAveSPContentTypeHub CreateAveSPContentTypeHub(AveObjectModelFactory fac, IAveMetadataServiceApplication application)
        {
            return new AveSPContentTypeHub(fac, application);
        }

        public override IAveSPContentTypeHub CreateAveSPContentTypeHub(AveObjectModelFactory fac, Guid applicationId)
        {
            return new AveSPContentTypeHub(fac, applicationId);
        }

        public override IAveSPContentTypeHub CreateAveSPContentTypeHub(AveObjectModelFactory fac, Guid applicationId, Guid partitionId)
        {
            return new AveSPContentTypeHub(fac, applicationId, partitionId);
        }

        public override IAveSPAppManager CreateAveSPApp(IAveSPWeb web, Guid productId)
        {
            return new AveSPAppManager(web as AveSPWeb, productId);
        }
    }
}
