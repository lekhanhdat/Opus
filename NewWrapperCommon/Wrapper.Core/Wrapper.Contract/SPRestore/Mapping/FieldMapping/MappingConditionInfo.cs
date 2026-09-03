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

namespace AvePoint.Wrapper.Core.SPRestore.Mapping
{
    /// <summary>
    /// 用于比较Mapping Condition需要的原端数据
    /// </summary>
    public abstract class MappingConditionInfo
    {
        /// <summary>
        /// AveWebInfo.Url
        /// </summary>
        public string WebUrl { get; set; }

        /// <summary>
        /// SPField.Id,only column mapping use.
        /// </summary>
        public Guid FieldId { get; set; }
    }

    /// <summary>
    /// Web 的condition信息，表示对web上的column/CT做mapping
    /// </summary>
    public class WeMappingConditionInfo : MappingConditionInfo
    {
        /// <summary>
        /// Web上的Content Type集合，<CTName,List<FieldID>>。从AveContentTypeCollectionInfo中的Id和FieldsSchemaXml获取,only column mapping use.
        /// </summary>
        public Dictionary<string, List<Guid>> WebContentTypes { get; set; }
    }

    /// <summary>
    /// List 的condition信息，表示对list上的column/CT做mapping
    /// </summary>
    public class ListMappingConditionInfo : MappingConditionInfo
    {
        /// <summary>
        /// SPList.Title
        /// </summary>
        public string ListTitle { get; set; }
        /// <summary>
        /// Convert.ToString(AveListInfo.BaseTemplate)
        /// </summary>
        public string ListTemplateID { get; set; }//todo:oliver?string or int
        /// <summary>
        /// List上的Content Type集合，<CTName,List<FieldID>>。从AveContentTypeCollectionInfo中的Id和FieldsSchemaXml获取,only column mapping use.
        /// </summary>
        public Dictionary<string, List<Guid>> ListContentTypes { get; set; }
    }
}
