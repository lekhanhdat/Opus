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





namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;

    public class AveRestoredContentTypeMapping : IAveRestoredContentTypeMapping
    {
        private Dictionary<string, string> contentTypeNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> contentTypeIdMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, NameMapping> contentTypeNameMappingById = new Dictionary<string, NameMapping>();

        #region contentTypeNameMapping
        public void AddContentTypeNameMapping(string srcCTName,string desCTName)
        {
            contentTypeNameMapping.AddWithLock(srcCTName, desCTName);
        }

        public string GetMappingRestoredContentTypeName(string srcName)
        {
            return contentTypeNameMapping.GetValueWithLock(srcName);
        }

        public IEnumerable<KeyValuePair<string, string>> EnumContentTypeNameMapping()
        {
            lock (contentTypeNameMapping)
            {
                foreach (var value in contentTypeNameMapping)
                {
                    yield return value;
                }
            }
        }
        public void EnumContentTypeNameMapping(Action<string, string> action)
        {
            contentTypeNameMapping.ForeachElementWithLock(action);
        }
        #endregion
        #region contenttypeIdMapping

        public void SetContentTypeIdMapping(Dictionary<string, string> idMapping)
        {
            contentTypeIdMapping = idMapping;
        }

        public void AddContentTypeIdMapping(string srcCTId, string desCTId)
        {
            contentTypeIdMapping.AddWithLock(srcCTId, desCTId);
        }

        public string GetMappingRestoredContentTypeId(string srcId)
        {
            return contentTypeIdMapping.GetValueWithLock(srcId);
        }

        public IEnumerable<KeyValuePair<string, string>> EnumContentTypeIdMapping()
        {
            lock (contentTypeIdMapping)
            {
                foreach (var value in contentTypeIdMapping)
                {
                    yield return value;
                }
            }
        }
        #endregion
        #region contentTypeNameMappingById
        public void SetContentTypeNameMappingById(Dictionary<string, NameMapping> mapping)
        {
            contentTypeNameMappingById = mapping;
        }
        public  void AddContentTypeNameMappingById(string srcId, string srcName, string desName)
        {
            contentTypeNameMappingById.AddWithLock(srcId, new NameMapping { SourceName = srcName, DestName = desName });
        }

        public string GetMappingRestoredContentTypeNameById(string srcId, string srcName)
        {
            NameMapping mapping = contentTypeNameMappingById.GetValueWithLock(srcId);
            if (mapping != null)
            {
                if (mapping.SourceName.Equals(srcName, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.DestName;
                }
            }
            return srcName;
        }
        public NameMapping GetMappingRestoredContentTypeNameMappingById(string srcId)
        {
            return contentTypeNameMappingById.GetValueWithLock(srcId);
        }
        #endregion
        public virtual void Dispose()
        {
            if (contentTypeNameMapping != null)
            {
                contentTypeNameMapping = null;
            }
            if (contentTypeIdMapping != null)
            {
                contentTypeIdMapping = null;
            }
            if (contentTypeNameMappingById != null)
            {
                contentTypeNameMappingById = null;
            }
        }
    }
}
