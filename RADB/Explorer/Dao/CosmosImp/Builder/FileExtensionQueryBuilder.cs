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
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using Newtonsoft.Json;
using SqlKata;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    /// <summary>
    /// will filter nodetypes or extension files
    /// </summary>
    public class FileExtensionQueryBuilder : IFilterBuilder, IAdvancedQueryBuilder
    {

        private const string SPDocumentKey = "RM_RDM_RecordDetails_DataType_SPDocument";

        private const string SPItemKey = "RM_RDM_RecordDetails_DataType_SPItem";

        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;

            var phyNodeTypes = new List<int>();
            var otherTypes = new List<string>();
            PreProcessExtensions(filterOption.FileExtensions, otherTypes, phyNodeTypes, out var hasFilteredSPDoucment);

            if (hasFilteredSPDoucment)
            {
                phyNodeTypes.Add(500);
                return query.Where(q =>q.WhereArrayContainV2(phyNodeTypes, CosmosConst.C_NodeType.FormatColumnName())
                .AndWhere(q1 =>q1.WhereNotStringEquals(CosmosConst.C_ExtensionForFile.FormatColumnName(), SPItemKey)
                .OrWhereNull(CosmosConst.C_ExtensionForFile.FormatColumnName()))
                .OrWhere(q2 =>q2.WhereArrayContainV3(otherTypes.Select(item => item.ToLower()), CosmosConst.C_ExtensionForFile.FormatColumnName(), "LOWER")));
            }

            return query.Where(q1 => q1.WhereArrayContainV2(phyNodeTypes, CosmosConst.C_NodeType.FormatColumnName())
                    .OrWhere(q2 => q2.WhereArrayContainV3(otherTypes.Select(item => item.ToLower()), CosmosConst.C_ExtensionForFile.FormatColumnName(), "LOWER"))
            );

        }

        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.FileExtensions != null && filterOption.FileExtensions.Count > 0;
        }

        private void PreProcessExtensions(List<string> fileExtensions, List<string> otherTypes, List<int> nodeTypes, out bool hasFilteredSPDoucment)
        {
            hasFilteredSPDoucment = false;
            foreach (var item in fileExtensions)
            {
                int.TryParse(item, out int phyType);
                if (phyType > 0)
                {
                    nodeTypes.Add(phyType);
                    continue;
                }

                if (item.Equals(SPDocumentKey, System.StringComparison.OrdinalIgnoreCase))
                {
                    hasFilteredSPDoucment = true;
                    continue;
                }

                otherTypes.Add(item);
            }
        }

        #region Advanced search
        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!string.Equals(GetColumnId(), column.Id, System.StringComparison.OrdinalIgnoreCase)) return query;

            var filterOption = Convert2SearchOptionV2(column, objJson, columnOperationLogic, keyOperationLogic);
            return Filter(query, filterOption);
        }

        private string GetColumnId()
        {
            return Contract.TemplateManagement.QueryCloumnIds.FileExtension;
        }

        private ExplorerFilterOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic)
        {
            return new ExplorerFilterOptionV2
            {
                FileExtensions = JsonConvert.DeserializeObject<List<string>>(objJson)
            };
        }

        #endregion

    }
}
