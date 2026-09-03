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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveTaxonomyGroup : AvePoint.Wrapper.Backup.IAveSPTaxonomyGroup
    {
        //private IAveTaxonomyGroup taxonomyGroup;
        private AveTermStore aveTermStore;

        internal IAveMetadataServiceApplication ServiceApplication
        {
            get
            {
                return this.aveTermStore.ServiceApplication;
            }
        }

        public Guid TaxonomyGroup
        {
            get;
            set;
        }

        public AveTaxonomyGroup(AveTermStore aveTermStore)
        {
            this.aveTermStore = aveTermStore;
        }

        public void Export(IAveBackupStream output, Guid groupId)
        {
            this.TaxonomyGroup = groupId;
            output.WriteMetadata(AveMetadataType.MetadataGroup, this.ServiceApplication.GetGroup(groupId));
        }
        public void Export(IAveBackupStream output, Guid groupId,Guid partitionId)
        {
            this.TaxonomyGroup = groupId;
            output.WriteMetadata(AveMetadataType.MetadataGroup, this.ServiceApplication.GetGroup(groupId, partitionId));
        }
    }
}