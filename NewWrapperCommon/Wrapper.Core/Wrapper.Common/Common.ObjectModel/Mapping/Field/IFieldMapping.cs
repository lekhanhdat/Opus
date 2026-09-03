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



namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IAveFieldMapping : IAveCustomFieldMapping, IAveRestoredFieldMapping
    {
        void SetCustomMapping(IAveCustomFieldMapping customFieldMapping);
    }

    public interface IAveRestoredFieldMapping : IDisposable
    {
        void AddFieldMapping(string srcName, Guid destFieldId);

        /// <returns>r如果该Column没有还原过，返回Guid.Empty</returns>
        Guid GetMappingRestoredField(string srcName);

        #region FieldIdMapping
        void AddFieldIdMapping(Guid srcFieldId, Guid destFieldId);

        Guid GetMappingRestoredFieldId(Guid srcFieldId);

        IEnumerable<KeyValuePair<Guid, Guid>> EnumFieldIdMapping();
        #endregion

        #region FieldInternalNameMapping
        void AddFieldInternalNameMapping(string srcName, string destName);

        string GetMappingRestoredFieldInternalName(string srcName);

        IEnumerable<KeyValuePair<string, string>> EnumFieldInternalNameMapping();
        #endregion

        #region FieldDisplayNameMapping
        void AddFieldDisplayNameMapping(string srcName, string destName);

        string GetMappingRestoredFieldDisplayName(string srcName);

        IEnumerable<KeyValuePair<string, string>> EnumFieldDisplayNameMapping();
        #endregion

        #region FieldSchemaMappings
        void SetFieldIdSchemaMappings(Dictionary<Guid, Guid> schemaMappings);

        void AddFieldIdSchemaMapping(Guid srcFieldId, Guid destFieldId);

        Guid GetMappingSchemaFieldId(Guid srcFieldId);

        IEnumerable<KeyValuePair<Guid, Guid>> EnumFieldSchemaMapping();
        #endregion

        #region SkippedFields
        void AddSkippedFields(string internalName);

        IEnumerable<string> EnumSkippedFields();
        #endregion

        #region FailedFields
        void AddFailedFields(string internalName);

        IEnumerable<string> EnumFailedFields();
        #endregion
    }
}
