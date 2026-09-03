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
using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class MoveSettingInfo
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(MoveSettingInfo));
        public MoveSettingInfo(MoveRecordSetting moveSetting)
        {
            switch (moveSetting.ContainerLevelConflictOption)
            {
                case ConflictOption.Skip:
                    ContainerConflictResolution = ContainerConflictResolution.Skip;
                    break;
                case ConflictOption.Merge:
                    ContainerConflictResolution = ContainerConflictResolution.Merge;
                    break;
                default:
                    ContainerConflictResolution = ContainerConflictResolution.Skip;
                    break;
            }
            switch (moveSetting.ItemLevelConflictOption)
            {
                case ConflictOption.AppendByName:
                    ContentConflictResolution = ContentConflictResolution.Append;
                    break;
                case ConflictOption.Skip:
                    ContentConflictResolution = ContentConflictResolution.Skip;
                    break;
                case ConflictOption.Overwrite:
                    ContentConflictResolution = ContentConflictResolution.Overwrite;
                    break;
                default:
                    ContentConflictResolution = ContentConflictResolution.Skip;
                    break;
            }
            FSPropertyMappings = moveSetting.FilePropertiesMapping;
            if (moveSetting.FileCommonMapping != null)
            {
                if (moveSetting.FileCommonMapping.LengthItem.IsCheckedMaxFileName)
                {
                    LimitedFileLength = moveSetting.FileCommonMapping.LengthItem.MaxFileNameLength;
                }
                if (moveSetting.FileCommonMapping.LengthItem.IsCheckedMaxForlderName)
                {
                    LimitedFolderLength = moveSetting.FileCommonMapping.LengthItem.MaxForlderNameLength;
                }
                foreach (IllegalCharReplaceMappingItem item in moveSetting.FileCommonMapping.IllegalCharReplaceMappings)
                {
                    try
                    {
                        IllegalCharMap.Add(item.IllegalChar[0], item.ReplaceChar[0]);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Add illegal char to the hash table failed. Detail: {0}", ex.ToString());
                    }
                }
            }
        }

        public ContainerConflictResolution ContainerConflictResolution { get; set; }

        public ContentConflictResolution ContentConflictResolution { get; set; }

        public ItemDependencyOption ItemDependencyOption { get; set; } = ItemDependencyOption.Append;

        public FilePropertiesMapping FSPropertyMappings { get; set; }

        public int LimitedFileLength { get; private set; } = 0;

        public int LimitedFolderLength { get; private set; } = 0;

        public Hashtable IllegalCharMap { get; private set; } = new Hashtable(StringComparer.OrdinalIgnoreCase);
    }
}
