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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveFieldInfo
    {
        public string Name;
        public string Type;
        public string SchemaXml;
        public bool AddToDefaultView = true;
    }

    public class AveFieldCollectionInfo
    {
        public List<AveFieldInfo> Fields = new List<AveFieldInfo>();        
        public string AveSchemaXml = null;
        public List<AveTermStoreInfo> RelatedMetadataInfo = new List<AveTermStoreInfo>();
    }

    public class AveLookupFieldInfo
    {
        public Guid Id;
        public Guid LookupList;
        public string LookupField;
        public Guid LookupWeb;
        /// <summary>
        /// 修改了此属性的属性名（LookupColumnName to LookupColumnRowNameForQurey），使其表意更明确。
        /// </summary>
        public string LookupColumnRowNameForQuery;
        /// <summary>
        /// 用于Lookup关联的column是computed类型时无法查出数据库中的数据时的API调用。
        /// </summary>
        public string LookupColumnDisplayName;
    }

    public class AveTaxFieldInfo
    {
        public string TextFieldInternalName;
        public Guid SspId;
        public Guid GroupId;
        public Guid TermSetId;
        //public Guid anchorId;//放到termIds中
        public bool IsKeywordsColumn;
        public List<Guid> TermIds = new List<Guid>();
        //public List<Guid> defaultTermIds = new List<Guid>(); //放到termIds中

        public AveTaxFieldInfo Clone()
        {
            return new AveTaxFieldInfo() 
            {
                TextFieldInternalName = this.TextFieldInternalName,
                SspId = this.SspId,
                GroupId = this.GroupId,
                TermSetId = this.TermSetId,
                IsKeywordsColumn = this.IsKeywordsColumn,
            };
        }
    }
}
