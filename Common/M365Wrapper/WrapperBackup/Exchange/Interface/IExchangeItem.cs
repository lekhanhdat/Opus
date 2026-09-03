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
using System.IO;
using System.Threading.Tasks;
using ExchangeUtility.Graph;
using Microsoft.Exchange.WebServices.Data;

namespace ExchangeBackupUtility.Graph
{
    public interface IExchangeItem
    {
        string Category { get; }
        ChangeStatus ChangeStatus { get; }
        DateTime Created { get; }
        DateTime Received { get; }
        string DisplayTo { get; }
        string ExchangeId { get; }
        int FailedCount { get; set; }
        bool HasAttach { get; }
        bool IsDraft { get; }
        bool IsRead { get; }
        string ItemId { get; }
        string ItemInternalPath { get; set; }
        string ItemName { get; }
        string ItemPath { get; set; }
        long ItemSize { get; }
        string ItemType { get; }
        string MessageId { get; }
        DateTime Modified { get; }
        string ParentFolderId { get; }
        string ParentFolderDisplayName { get; }
        int ParentNameEnumerator { get; }
        DateTime SendDateUTC { get; }
        string SendDateUtcString { get; }
        string Sender { get; }
        string ModifiedBy { get; }
        string SenderDisplayName { get; }
        string SenderEmailAddress { get; }
        int AttachmentCount { get; }
        string RetentionLabel { get; }
        string SensitivityLabel { get; }
        bool IsNew { get; }
        bool IsUnmodified { get; }
        IExchangePolicyTag PolicyTag { get; }
        Dictionary<string, string> GetProperties();
        Dictionary<string, string?> GetExtendedProperties();
        List<string> AttachmentNames { get; }
        bool TryGetExtendProperty(ExtendProperty property, out string value);
        void SetExtendProperty(string definition, string value);

        Task<bool> DeleteAsync(bool isHardDelete = false);
        Task<bool> MoveAsync(string targetFolderId);
        Task<bool> SetExtendedPropertyAsync(ExtendedPropertyDefinition prop, object value);
        Task<Stream> GetMimeContentAsync();
        Task<bool> SetRetentionLabelAsync(Guid labelId);
        void RemovePolicyTag();

        void TagLabel(Guid labelId);
        void RemoveLabel();
        bool CanUpdateLabel(List<Guid> labelIds);
        bool IsLabelExist();
        Guid ApplyedLabelId();

        string DisplayCc { get; }
        int Importance { get; }
    }
    
    public enum ExtendProperty
    {
        Term,
        SensitiveLabel
    }
}