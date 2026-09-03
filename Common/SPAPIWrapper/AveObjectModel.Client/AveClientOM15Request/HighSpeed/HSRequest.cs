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
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;

using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOM2013Request
    {
        public Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.CreateMigrationJob(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri);
                context.ExecuteQuery();
                return result.Value;
            }
        }

        public Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.CreateMigrationJobEncrypted(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri, new EncryptionOption() { AES256CBCKey = options.AES256CBCKey });
                context.ExecuteQuery();
                return result.Value;
            }
        }

        public AveMigrationJobState GetMigrationJobStatus(Guid id)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.GetMigrationJobStatus(id);
                context.ExecuteQuery();
                return (AveMigrationJobState)result.Value;
            }
        }

        public MigrationJobProgress GetMigrationJobProgress(Guid id, string nextToken = "0")
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.GetMigrationJobProgress(id, nextToken);
                context.ExecuteQuery();
                return result.Value;
            }
        }

        public Dictionary<Guid,AveMigrationJobState> GetMigrationStatus()
        {
            var returnValue = new Dictionary<Guid, AveMigrationJobState>();
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.GetMigrationStatus();
                context.Load(result);
                context.ExecuteQuery();
                foreach (var value in result)
                {
                    returnValue.Add(value.JobId, (AveMigrationJobState)value.JobState);
                }
                return returnValue;
            }
        }

        public AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers()
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.ProvisionMigrationContainers();
                context.ExecuteQuery();
                var info = (ProvisionedMigrationContainersInfo)result.Value;
                return new AveProvisionedMigrationContainersInfo()
                {
                    DataContainerUri = info.DataContainerUri,
                    EncryptionKey = info.EncryptionKey,
                    MetadataContainerUri = info.MetadataContainerUri,
                    TypeId = info.TypeId
                };
            }
        }

        public AveProvisionedMigrationQueueInfo ProvisionMigrationQueue()
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.ProvisionMigrationQueue();
                context.ExecuteQuery();
                var info = (ProvisionedMigrationQueueInfo)result.Value;
                return new AveProvisionedMigrationQueueInfo()
                {
                    JobQueueUri = info.JobQueueUri,
                    TypeId = info.TypeId
                };
            }
        }

        public bool DeleteMigrationJob(Guid id)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.DeleteMigrationJob(id);
                context.ExecuteQuery();
                return result.Value;
            }
        }
    }
}
