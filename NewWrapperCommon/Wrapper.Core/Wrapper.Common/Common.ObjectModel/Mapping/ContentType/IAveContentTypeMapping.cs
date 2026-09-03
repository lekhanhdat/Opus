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

    public interface IAveContentTypeMapping: IAveCustomContentTypeMapping, IAveRestoredContentTypeMapping 
    {
        void SetCustomMapping(IAveCustomContentTypeMapping customContentTypeMapping);
    }

    public interface IAveRestoredContentTypeMapping : IDisposable
    {
        

        #region contentTypeNameMapping
        void AddContentTypeNameMapping(string srcCTName, string desCTName);

        string GetMappingRestoredContentTypeName(string srcName);

        IEnumerable<KeyValuePair<string, string>> EnumContentTypeNameMapping();

        #endregion
        #region contenttypeIdMapping

        void SetContentTypeIdMapping(Dictionary<string, string> idMapping);

        void AddContentTypeIdMapping(string srcCTId, string desCTId);

        string GetMappingRestoredContentTypeId(string srcId);

        IEnumerable<KeyValuePair<string, string>> EnumContentTypeIdMapping();
        #endregion
        #region contentTypeNameMappingById
        void SetContentTypeNameMappingById(Dictionary<string, NameMapping> mapping);
        void AddContentTypeNameMappingById(string srcId, string srcName, string desName);

        string GetMappingRestoredContentTypeNameById(string srcId, string srcName);
        NameMapping GetMappingRestoredContentTypeNameMappingById(string srcId);
        #endregion
    }
}
