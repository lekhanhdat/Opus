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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveTermStore : IDisposable, AvePoint.Wrapper.Backup.IAveSPTermStore
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory objectModelFactory;

        //private IAveTermStore termStore;
        private IAveMetadataServiceApplication metadataServiceApplication;

        internal Guid ApplicationId { get; set; }

        internal Guid TermStoreId { get; set; }

        //public IAveTermStore TermStore
        //{
        //    get { return this.termStore; }
        //}
        internal IAveMetadataServiceApplication ServiceApplication
        {
            get { return metadataServiceApplication; }
        }

        public AveTermStore(AveObjectModelFactory modelFactory)
        {
            this.objectModelFactory = modelFactory;
        }

        public AveTermStore(AveObjectModelFactory modelFactory, Guid applicationId)
        {
            this.objectModelFactory = modelFactory;
        }

        public void Export(IAveBackupStream output, IAveSite site)
        {
            this.metadataServiceApplication = this.objectModelFactory.CreateMetadataServiceApplication(site);
            output.WriteMetadata(AveMetadataType.MetadataTermStore, this.metadataServiceApplication.GetTermStore());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="output"></param>
        /// <param name="applicationId">Managed Metadata Service application ID</param>
        public void Export(IAveBackupStream output, Guid applicationId)
        {
            this.ApplicationId = applicationId;
            this.metadataServiceApplication = this.objectModelFactory.CreateMetadataServiceApplication(this.ApplicationId);
            output.WriteMetadata(AveMetadataType.MetadataTermStore, this.metadataServiceApplication.GetTermStore());
        }

        public void Export(IAveBackupStream output, Guid applicationId, Guid partitionId, string siteUrl = null)
        {
            this.ApplicationId = applicationId;
            this.metadataServiceApplication = this.objectModelFactory.CreateMetadataServiceApplication(this.ApplicationId, partitionId);
            var termStoreInfo = this.metadataServiceApplication.GetTermStore(partitionId);
            termStoreInfo.SiteUrl = siteUrl;
            output.WriteMetadata(AveMetadataType.MetadataTermStore, termStoreInfo);
        }

        public void ExportByTermStore(IAveBackupStream output, Guid termStoreId)
        {
            using (IAveSite aveSite = this.objectModelFactory.CreateAdministrationWebApplication().Local.Sites[0])
            {
                IAveTaxonomySession taxonomySession = this.objectModelFactory.CreateTaxonomySession(aveSite);

                this.ApplicationId = GetApplication(taxonomySession.TermStores[termStoreId]);
            }

            this.metadataServiceApplication = this.objectModelFactory.CreateMetadataServiceApplication(this.ApplicationId);

            output.WriteMetadata(AveMetadataType.MetadataTermStore, this.metadataServiceApplication.GetTermStore());
        }

        private Guid GetApplication(IAveTermStore termStore)
        {
            foreach (IAveService service in this.objectModelFactory.CreateFarm().Local.Services)
            {
                foreach (IAveServiceApplication application in service.Applications)
                {
                    if (application.IsConnected(termStore.SharedServiceProxy))
                    {
                        return application.ID;
                    }
                }
            }
            return Guid.Empty;
        }

        //private AveTermStoreInfo GetTermStoreInfo(Guid termStoreId)
        //{
        //    IAveTaxonomySession taxonomySession = this.objectModelFactory.CreateTaxonomySession();
        //    this.termStore = taxonomySession.TermStores[termStoreId];
        //    return this.termStore.TermStoreSerializer.GetObjectData() as AveTermStoreInfo;
        //}

        public void Dispose()
        {
            if (this.metadataServiceApplication != null)
            {
                this.metadataServiceApplication.Dispose();
            }
        }
    }
}