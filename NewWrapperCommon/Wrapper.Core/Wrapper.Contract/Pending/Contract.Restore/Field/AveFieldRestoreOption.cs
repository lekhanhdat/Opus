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

namespace AvePoint.Wrapper.Restore
{
    /// <summary>
    /// The restore option of field restore, 
    /// </summary>
    public class AveFieldRestoreOption
    {
        public bool CompareMd5 { get; set; }

        public bool OverwriteChoices { get; set; }

        /// <summary>
        /// Field冲突解决方案
        /// </summary>
        public FieldConflictOption ConflictOption { get; set; }

        /// <summary>
        /// 查找option，有使用Id匹配，也有Name匹配的，也有schema匹配的。
        /// </summary>
        public FieldFindOption[] FindOption { get; set; }

        /// <summary>
        /// 现在只有Replicator模块才会使用这个参数，主要是如果目的端用ID能找到对应的Field，则无需要判断Type冲突，直接更新既可以。
        /// 如果其他模块也需要这个功能，请外围直接设置这个参数。
        /// </summary>
        public bool KeepConsistengByID { get; set; }

        public bool ListFieldNewDisplayName { get; set; }
        public bool ListFieldSkip { get; set; }
        public bool WebField { get; set; }
        public bool WebFieldNewDisplayName { get; set; }

        //当源端和目的端的column的ID相同但是Type不同时change column id，create 一个新的column
        public bool CheckFieldTypeWhenSameId { get; set; }

        public bool OverwriteBuiltinField { get; set; }
        public AveFieldRestoreOption()
        {
            CompareMd5 = false;
            ConflictOption = FieldConflictOption.Overwrite;
            FindOption = new[]{
                                        FieldFindOption.CustomMapping, FieldFindOption.Schema,
                                        FieldFindOption.Id, FieldFindOption.InternalName,
                                        FieldFindOption.StaticName
                                    };
            KeepConsistengByID = false;
            ListFieldNewDisplayName = true;
            ListFieldSkip = true;
            WebField = true;
            WebFieldNewDisplayName = true;
            OverwriteChoices = false;
            CheckFieldTypeWhenSameId = false;
            OverwriteBuiltinField = false;
        }
    }

    public enum FieldFindOption
    {
        /// <summary>
        /// 使用我们生成的field mapping关系
        /// </summary>
        Schema,

        Id,
        InternalName,
        StaticName,
        DisplayName,

        /// <summary>
        /// 使用自定义mapping，需要客户提供mapping关系
        /// </summary>
        CustomMapping,
    }

    public enum FieldConflictOption
    {
        Skip,

        /// <summary>
        /// 把目的端rename，源端直接创建
        /// </summary>
        AppendSourceWin,

        /// <summary>
        /// 不破坏目的端，rename源端的title进行创建
        /// </summary>
        AppendDestinationWin,

        Overwrite,
    }

    public enum FieldRestoreStatus
    {
        None,
        NewCreated,
        Existed,
        Skipped,
        Exception,
    }

    public enum FieldType
    {
        Site,
        List
    }

    //used for sort field order
    public enum FieldOrderType
    {
        LookupPrimary,
        LookupSecondary,
        Other
    }
}