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
using AvePoint.RA.Contract.Label;
using AvePoint.RA.DB.Model;
using Google.Apis.DriveLabels.v2.Data;
using Newtonsoft.Json;

namespace RAGoogle.Extension
{
    public static class GoogleLabelExtension
    {
        public static RMLabel MapGoogleLabelToRMLabel(this GoogleAppsDriveLabelsV2Label labelsV2Label, string TenantId)
        {
            return new RMLabel
            {
                Name = labelsV2Label.Properties.Title,
                Description = labelsV2Label.Properties.Description,
                UniqueId = Guid.NewGuid(),
                Values = JsonConvert.SerializeObject(new LabelValue
                {
                    LabelId = labelsV2Label.Id,
                    Title = labelsV2Label.Properties.Title,
                    Name = labelsV2Label.Name,
                    RevisionId = labelsV2Label.RevisionId,
                    LabelType = ConvertLabelType(labelsV2Label.LabelType),
                    CreateTime = labelsV2Label.CreateTimeRaw != null ? DateTime.Parse(labelsV2Label.CreateTimeRaw).Ticks : null,
                    CustomerId = GetCustomerId(labelsV2Label.Customer),
                    RevisionCreateTime = labelsV2Label.CreateTimeRaw != null ? DateTime.Parse(labelsV2Label.CreateTimeRaw).Ticks : null,
                    State = ConvertState(labelsV2Label.Lifecycle.State),
                    HasUnpublishedChanges = labelsV2Label.Lifecycle.HasUnpublishedChanges
                }),
                TenantId = TenantId,
                IsDeleted = false
            };
        }
        public static RMLabel MapGoogleLabelToRMLabelForUpdate(this GoogleAppsDriveLabelsV2Label labelsV2Label, RMLabel mLabel)
        {
            if (mLabel == null || labelsV2Label == null)
            {
                return null;
            }
            mLabel.Name = labelsV2Label.Properties.Title;
            mLabel.Description = labelsV2Label.Properties.Description;
            mLabel.Values = JsonConvert.SerializeObject(new LabelValue
            {
                LabelId = labelsV2Label.Id,
                Title = labelsV2Label.Properties.Title,
                Name = labelsV2Label.Name,
                RevisionId = labelsV2Label.RevisionId,
                LabelType = ConvertLabelType(labelsV2Label.LabelType),
                CreateTime = labelsV2Label.CreateTimeRaw != null ? DateTime.Parse(labelsV2Label.CreateTimeRaw).Ticks : null,
                CustomerId = GetCustomerId(labelsV2Label.Customer),
                RevisionCreateTime = labelsV2Label.RevisionCreateTimeRaw != null ? DateTime.Parse(labelsV2Label.RevisionCreateTimeRaw).Ticks : null,
                State = ConvertState(labelsV2Label.Lifecycle.State),
                HasUnpublishedChanges = labelsV2Label.Lifecycle.HasUnpublishedChanges
            });
            mLabel.IsDeleted = false;
            return mLabel;
        }
        public static GoogleAppsDriveLabelsV2Label MapRMLabelToGoogleLabel(this RMLabel rmLabel)
        {
            return new GoogleAppsDriveLabelsV2Label
            {
                Properties = new GoogleAppsDriveLabelsV2LabelProperties
                {
                    Title = rmLabel.Name,
                    Description = rmLabel.Description,
                },
                LabelType = ConvertLabelTypeToLabelTypeGoogle(LabelType.Shared)
            };
        }
        public static GoogleAppsDriveLabelsV2Label MapRMTermToGoogleLabel(this RMTerm rmTerm)
        {
            return new GoogleAppsDriveLabelsV2Label
            {
                Properties = new GoogleAppsDriveLabelsV2LabelProperties
                {
                    Title = rmTerm.Name,
                    Description = rmTerm.Description
                },
                LabelType = ConvertLabelTypeToLabelTypeGoogle(LabelType.Shared)
            };
        }
        public static void MapGoogleLabelToRMGoogleLabelInfoForUpdate(this GoogleAppsDriveLabelsV2Label labelsV2Label, RMTerm rmTerm, RMGoogleLabelInfo rmGoogleLabelInfo)
        {
            rmGoogleLabelInfo.LabelId = labelsV2Label.Id;
            rmGoogleLabelInfo.TermId = rmTerm.Id;
            rmGoogleLabelInfo.LabelName = labelsV2Label.Properties.Title;
            rmGoogleLabelInfo.LabelType = ConvertLabelType(labelsV2Label.LabelType);
            rmGoogleLabelInfo.TenantId = GetCustomerId(labelsV2Label.Customer);
            rmGoogleLabelInfo.TermUniqueId = rmTerm.UniqueId;
            rmGoogleLabelInfo.Extension = JsonConvert.SerializeObject(labelsV2Label);
        }
        public static string ConvertLabelTypeToLabelTypeGoogle(LabelType labelType)
        {
            return labelType switch
            {
                LabelType.Admin => "ADMIN",
                LabelType.Shared => "SHARED",
                _ => "LABEL_TYPE_UNSPECIFIED",
            };
        }
        public static LabelType ConvertLabelType(string labelType)
        {
            return labelType switch
            {
                "ADMIN" => LabelType.Admin,
                "SHARED" => LabelType.Shared,
                _ => LabelType.None,
            };
        }
        public static State ConvertState(string state)
        {
            return state switch
            {
                "PUBLISHED" => State.Published,
                "DISABLED" => State.Disabled,
                "UNPUBLISHED_DRAFT" => State.UnpublishedDraft,
                "DELETED" => State.Deleted,
                _ => State.None,
            };
        }
        public static string GetCustomerId(string input)
        {
            string prefix = "customers/";
            if (input.StartsWith(prefix))
            {
                return input.Substring(prefix.Length);
            }
            return input;
        }
    }
}