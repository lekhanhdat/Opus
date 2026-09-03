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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Object
{
    [DataContract]
    public class StubDisposalSiteInfoDto
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string SiteCollectionUrl { get; set; }

        [DataMember]
        public long MinRetentionTime { get; set; }

        [DataMember]
        public DateTime StartDisposalTime { get; set; }
    }

    [DataContract]
    public class SiteStubSettingMappingDto
    {
        [DataMember]
        public string SiteCollectionUrl { get; set; }

        [DataMember]
        public Guid StubTemplateId { get; set; }

        [DataMember]
        public bool IsEnabledRetention { get; set; }

        [DataMember]
        public int RetentionValue { get; set; }

        [DataMember]
        public DateUnit RetentionUnit { get; set; }

        [DataMember]
        public long FirstStubCreatedTime { get; set; }
    }

    public class StubFileRecordDto
    {
        public Guid SiteCollectionID { get; set; } // PartitionKey
        public Guid ArchivedItemId { get; set; } // {RefTimeTicks:yyyyMMddHHmmss}_{ArchivedItemId:N} to build RowKey

        public string StubTemplateId { get; set; }

        public string ArchivedFileFullPath { get; set; }

        public LeaveStubType StubType { get; set; }

        public string StubId { get; set; }

        public Guid WebId { get; set; }

        public Guid ListId { get; set; }

        public DateTime RefDateTime { get; set; } // DateTime.UtcNow

        //public int RecordType { get; set; } // 0: main, 1: index

        public override string ToString()
        {
            return $"SiteCollectionID: {SiteCollectionID}, ArchivedItemId: {ArchivedItemId}, StubTemplateId: {StubTemplateId}, ArchivedFileFullPath: {ArchivedFileFullPath}, StubType: {StubType}, StubId: {StubId}, WebId: {WebId}, ListId: {ListId}, RefTimeTicks: {RefDateTime}";
        }
    }
}
