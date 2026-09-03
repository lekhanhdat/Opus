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

    public class AveRestoredFieldMapping : IAveRestoredFieldMapping
    {
        Dictionary<string, Guid> fieldMapping = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        Dictionary<Guid, Guid> fieldIdMapping = new Dictionary<Guid, Guid>();
        Dictionary<string, string> fieldInternalNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> fieldDisplayNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<Guid, Guid> fieldSchemaMappings = new Dictionary<Guid,Guid>();
        public void AddFieldMapping(string srcName, Guid destFieldId)
        {
            fieldMapping.AddWithLock(srcName, destFieldId);
        }

        public Guid GetMappingRestoredField(string srcName)
        {
            return fieldMapping.GetValueWithLock(srcName);
        }
        #region FieldIdMapping
        public void AddFieldIdMapping(Guid srcFieldId, Guid destFieldId)
        {
            fieldIdMapping.AddWithLock(srcFieldId, destFieldId);
        }

        public Guid GetMappingRestoredFieldId(Guid srcFieldId)
        {
            return fieldIdMapping.GetValueWithLock(srcFieldId);
        }

        public IEnumerable<KeyValuePair<Guid, Guid>> EnumFieldIdMapping()
        {
            lock (fieldIdMapping)
            {
                foreach (var value in fieldIdMapping)
                {
                    yield return value;
                }
            }
        }
        #endregion

        #region FieldInternalNameMapping
        public void AddFieldInternalNameMapping(string srcName, string destName)
        {
            fieldInternalNameMapping.AddWithLock(srcName, destName);
        }

        public string GetMappingRestoredFieldInternalName(string srcName)
        {
            return fieldInternalNameMapping.GetValueWithLock(srcName);
        }

        public IEnumerable<KeyValuePair<string, string>> EnumFieldInternalNameMapping()
        {
            lock (fieldInternalNameMapping)
            {
                foreach (var value in fieldInternalNameMapping)
                {
                    yield return value;
                }
            }
        }
        #endregion

        #region FieldDisplayNameMapping
        public void AddFieldDisplayNameMapping(string srcName, string destName)
        {
            fieldDisplayNameMapping.AddWithLock(srcName, destName);
        }

        public string GetMappingRestoredFieldDisplayName(string srcName)
        {
            return fieldDisplayNameMapping.GetValueWithLock(srcName);
        }
        public IEnumerable<KeyValuePair<string, string>> EnumFieldDisplayNameMapping()
        {
            lock (fieldDisplayNameMapping)
            {
                foreach (var value in fieldDisplayNameMapping)
                {
                    yield return value;
                }
            }
        }
        #endregion

        #region FieldSchemaMappings
        public void SetFieldIdSchemaMappings(Dictionary<Guid, Guid> schemaMappings)
        {
            fieldSchemaMappings = schemaMappings;
        }

        public void AddFieldIdSchemaMapping(Guid srcFieldId, Guid destFieldId)
        {
            fieldSchemaMappings.AddWithLock(srcFieldId, destFieldId);
        }

        public Guid GetMappingSchemaFieldId(Guid srcFieldId)
        {
            return fieldSchemaMappings.GetValueWithLock(srcFieldId);
        }
        public void EnumContentTypeNameMapping(Action<Guid, Guid> action)
        {
            fieldSchemaMappings.ForeachElementWithLock(action);
        }
        public IEnumerable<KeyValuePair<Guid, Guid>> EnumFieldSchemaMapping()
        {
            lock (fieldSchemaMappings)
            {
                foreach (var value in fieldSchemaMappings)
                {
                    yield return value;
                }
            }
        }
        #endregion

        public virtual void Dispose()
        {
            if (fieldMapping != null)
            {
                fieldMapping = null;
            }
            if (fieldIdMapping != null)
            {
                fieldIdMapping = null;
            }
            if (fieldInternalNameMapping != null)
            {
                fieldInternalNameMapping = null;
            }
            if (fieldDisplayNameMapping != null)
            {
                fieldDisplayNameMapping = null;
            }
            if (fieldSchemaMappings != null)
            {
                fieldSchemaMappings = null;
            }
        }
    }
}
