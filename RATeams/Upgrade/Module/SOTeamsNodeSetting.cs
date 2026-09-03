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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATeams.Upgrade.Module
{
    public class SOTeamsNodeSetting : RMArchiverSetting
    {
        public List<RMExchangeOnlineSettingRuleMapping> Rules { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is SOTeamsNodeSetting))
                return false;

            SOTeamsNodeSetting other = (SOTeamsNodeSetting)obj;

            return RuleContainerId == other.RuleContainerId &&
                isEnableSuperUserDecrypt == other.isEnableSuperUserDecrypt &&
                isEnableRemoveRetentionLabel == other.isEnableRemoveRetentionLabel &&
                isIncludeManagedMetadataService == other.isIncludeManagedMetadataService &&
                isIncludeWorkflowDefinition == other.isIncludeWorkflowDefinition &&
                EnableArchiverManagement == other.EnableArchiverManagement &&
                CleanRestoredOption == other.CleanRestoredOption;
        }

        public override int GetHashCode()
        {
            unchecked // 防止整数溢出
            {
                int hash = 17;

                hash = hash * 23 + (RuleContainerId?.GetHashCode() ?? 0);
                hash = hash * 23 + isEnableSuperUserDecrypt.GetHashCode();
                hash = hash * 23 + isEnableRemoveRetentionLabel.GetHashCode();
                hash = hash * 23 + isIncludeManagedMetadataService.GetHashCode();
                hash = hash * 23 + isIncludeWorkflowDefinition.GetHashCode();
                hash = hash * 23 + EnableArchiverManagement.GetHashCode();
                hash = hash * 23 + (CleanRestoredOption?.GetHashCode() ?? 0);

                return hash;
            }
        }

        public override string ToString()
        {
            return "isEnableSuperUserDecrypt: " + isEnableSuperUserDecrypt +
                ", isEnableRemoveRetentionLabel" + isEnableSuperUserDecrypt +
                ", isIncludeManagedMetadataService" + isIncludeManagedMetadataService +
                ", isIncludeWorkflowDefinition" + isIncludeWorkflowDefinition +
                ", EnableArchiverManagement" + EnableArchiverManagement +
                ", CleanRestoredOption" + CleanRestoredOption;
        }

    }
}
