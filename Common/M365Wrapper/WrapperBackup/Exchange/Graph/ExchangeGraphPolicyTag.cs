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
using AvePoint.RA.CommonUtil;
using ExchangeUtility.Graph;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Models.Security;
using Microsoft365.Graph.Service;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ExchangeBackupUtility.Graph
{
    public class ExchangeGraphPolicyTag : IExchangePolicyTag
    {
        private readonly MailboxItem item;
        public GraphService Service { get; }
        public ExchangeGraphPolicyTag(MailboxItem item)
        {
            this.item = item;
        }

        public ExchangeGraphPolicyTag(GraphService service, MailboxItem? item)
        {
            Service = service;
            this.item = item;
        }

        public Guid? RetentionId => GetRetentionIdFromExtendedProperties();

        public string? RetentionName => item?.SingleValueExtendedProperties?.Find(p => p.Id == "String {403fc56b-cd30-47c5-86f8-ede9e35a022b} Name ComplianceTag")?.Value;

        public DateTimeOffset? RetentionDate => GetRetentionDateFromExtendedProperties();

        public int? RetentionPeriod => GetRetentionPeriodFromExtendedProperties();

        public int? RetentionFlags => GetRetentionFlagsFromExtendedProperties();

        public byte[]? RetentionStartDateEtc => GetRetentionStartDateEtcFromExtendedProperties();

        private Guid? GetRetentionIdFromExtendedProperties()
        {
            SingleValueLegacyExtendedProperty? retentionIdProperty = this.item?.SingleValueExtendedProperties?.Find(p => p.Id == "Binary 0x3019");
            if (retentionIdProperty != null)
            {
                return retentionIdProperty.Value!.ConvertFromBase64ToGuidId();
            }
            return null;
        }

        private DateTimeOffset? GetRetentionDateFromExtendedProperties()
        {
            SingleValueLegacyExtendedProperty? retentionDateProperty = this.item?.SingleValueExtendedProperties?.Find(p => p.Id == "SystemTime 0x301c");
            if (retentionDateProperty != null)
            {
                return DateTimeOffset.Parse(retentionDateProperty.Value!);
            }
            return null;
        }

        private int? GetRetentionPeriodFromExtendedProperties()
        {
            SingleValueLegacyExtendedProperty? retentionPeriodProperty = this.item?.SingleValueExtendedProperties?.Find(p => p.Id == "Integer 0x301a");
            if (retentionPeriodProperty != null && int.TryParse(retentionPeriodProperty.Value, out int retentionPeriod))
            {
                return retentionPeriod;
            }
            return null;
        }

        private int? GetRetentionFlagsFromExtendedProperties()
        {
            SingleValueLegacyExtendedProperty? retentionFlagsProperty = this.item?.SingleValueExtendedProperties?.Find(p => p.Id == "Integer 0x301d");
            if (retentionFlagsProperty != null && int.TryParse(retentionFlagsProperty.Value, out int retentionFlags))
            {
                return retentionFlags;
            }
            return null;
        }

        private byte[]? GetRetentionStartDateEtcFromExtendedProperties()
        {
            SingleValueLegacyExtendedProperty? retentionStartDateEtcProperty = this.item?.SingleValueExtendedProperties?.Find(p => p.Id == "Binary 0x301b");
            if (retentionStartDateEtcProperty != null)
            {
                return Convert.FromBase64String(retentionStartDateEtcProperty.Value!);
            }
            return null;
        }

        public async Task<List<RetentionLabel>> GetRetentionLabelsAsync()
        {
            var response = await Service.Security.GetRetentionLabelsAsync();
            return response.Value;
        }
    }
}
