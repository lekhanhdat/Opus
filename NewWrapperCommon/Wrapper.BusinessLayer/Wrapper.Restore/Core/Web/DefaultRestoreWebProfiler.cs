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
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    class DefaultRestoreWebProfiler : DefaultRestoreProfiler, ISPWebImportProfiler
    {
        public override void OnStatusChanged(SPImportEventArgs eventArgs)
        {
            if (eventArgs != null)
            {
                if (eventArgs.Status == WrapperRestoreStatus.Failed || eventArgs.Status == WrapperRestoreStatus.Skipped)
                {
                    logger.Warn("Url:{0}, Message:{1}, Status:{2}, Type:{3}, Title:{4}, Level:{5}", eventArgs.Url, eventArgs.Message, eventArgs.Status, eventArgs.Type, eventArgs.Title, eventArgs.Level);
                }

                switch (eventArgs.Type)
                {
                    case SPObjectType.Self:
                        UpdateWebStatus(eventArgs);
                        break;
                    case SPObjectType.Setting:
                        UpdateWebSettings(eventArgs);
                        break;
                    case SPObjectType.User:
                    case SPObjectType.UserSetting:
                        UpdateWebSecurity(eventArgs, AveMetadataType.Users, eventArgs.Type);
                        break;
                    case SPObjectType.Group:
                    case SPObjectType.GroupDistributionSetting:
                    case SPObjectType.GroupMembers:
                    case SPObjectType.GroupSettings:
                        UpdateWebSecurity(eventArgs, AveMetadataType.Groups, eventArgs.Type);
                        break;
                    case SPObjectType.Feature:
                        //TODO Web Feature Report
                        break;
                    case SPObjectType.TermGroup:
                    case SPObjectType.TermSet:
                    case SPObjectType.Term:
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Unsupported type:{0}", eventArgs.Type));
                }
            }
        }

        private void UpdateWebSecurity(SPImportEventArgs eventArgs, AveMetadataType metadataType, SPObjectType objectType)
        {
            var metadata = EnsureMetadataResult(metadataType);
            lock (metadata)
            {
                if (metadata.Details == null)
                {
                    metadata.Details = new MetadataRestoreDetails();
                }

                metadata.Details.AddDto(new SPObjectRestoreDto() { Message = eventArgs.Message, Name = eventArgs.Title, Status = eventArgs.Status, Type = objectType });
            }
        }

        private void UpdateWebSettings(SPImportEventArgs eventArgs)
        {
            var metadata = EnsureMetadataResult(AveMetadataType.WebProperty);

            metadata.Details = new MetadataRestoreDetails(eventArgs.Status, eventArgs.Message);
        }

        private void UpdateWebStatus(SPImportEventArgs eventArgs)
        {
            var metadata = EnsureMetadataResult(AveMetadataType.WebBasicInfo);

            metadata.Details = new MetadataRestoreDetails(eventArgs.Status, eventArgs.Message);
        }

        public override void OnProgressUpdated(SPImportEventArgs eventArgs)
        {
            if (eventArgs != null)
            {
                logger.Debug("Url:{0}, Message:{1}, Status:{2}, Type:{3}", eventArgs.Url, eventArgs.Message, eventArgs.Status, eventArgs.Type);
            }
        }
    }
}
