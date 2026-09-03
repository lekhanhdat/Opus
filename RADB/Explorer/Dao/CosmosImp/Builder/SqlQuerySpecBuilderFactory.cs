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
using System.Collections.Generic;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class SqlQuerySpecBuilderFactory
    {
        /// <summary>
        /// create a default builder
        /// </summary>
        /// <returns></returns>
        public static SqlQuerySpecBuilder Create()
        {
            return new SqlQuerySpecBuilder()
            {
                SearchBuilder = new List<ISearchBuilder> {
                    //new CreatedByQueryBuilder(),
                    //new ModifiedByQueryBuilder(),
                    new NameQueryBuilder(),
                    new UniqueIdQueryBuilder(),
                    new CustomColumnQueryBuilder(),
                },
                FilterBuilder = new List<IFilterBuilder> {
                    //new ContainerIdQueryBuilder(),
                    new CreatedByQueryBuilder(),
                    new CreatedTimeQueryBuilder(),
                    new CustomColumnQueryBuilder(),
                    new DeclareAsRecordQueryBuilder(),
                    new LockedByRecordLabelQueryBuilder(),
                    new DisposalDueDateQueryBuilder(),
                    new FileExtensionQueryBuilder(),
                    new HoldByQueryBuilder(),
                    new HoldStatusQueryBuilder(),
                    new ModifiedByQueryBuilder(),
                    new ModifiedTimeQueryBuilder(),
                    new NodeIdQueryBuilder(),
                    //new NodeTypeQueryBuilder(),
                    new PermissionIdQueryBuilder(),
                    new RecordOwnerQueryBuilder(),
                    new RecordStatusAndNodeTypeQueryBuilder(),
                    new RecordStatusQueryBuilder(),
                    new RuleIdQueryBuilder(),
                    new SourceFlagAndContainerIdQueryBuilder(),
                    //new SourceFlagQueryBuilder(),
                    //new TermIdQueryBuilder(),
                    //new WithoutTermsQueryBuilder(),
                    new TermQueryBuilder(),
                    new IdNotContainQueryBuilder(),
                    new ParentIdQueryBuilder(),
                    new PhysicalTemplateQueryBuilder(),
                    new SPTreeFilterQueryBuilder(),
                    new ListIdQueryBuilder(),
                    new ScopeIdQueryBuilder(),
                    new CollectionTimeQueryBuilder(),
                    new MailboxAddressQueryBuilder(),
                    new ContentArchivedQueryBuilder(),
                    new DirPathQueryBuilder(),
                    new RuleIdNotContainsQueryBuilder(),
                    new TrainingAddTypeQueryBuilder(),
                    new TrainingScopeQueryBuilder(),
                    new SCsIdNotContainsQueryBuilder(),
                    new NodeTypeQueryBuilder(),
                    new ScopeIdQueryBuilder(),
                    new WebIdArrayQueryBuilder(),
                    new TermIdArrayQueryBuilder(),
                    new PhysicalLocationQueryBuilder(),
                }
            };
        }

        /// <summary>
        /// create a builder for default andvanced search
        /// </summary>
        /// <returns></returns>
        public static SqlQuerySpecBuilder CreateDefaultAdvancedSearchBuilder()
        {
            return new SqlQuerySpecBuilder()
            {
                //To attach builtin filters
                FilterBuilder = new List<IFilterBuilder>
                {
                    new PermissionIdQueryBuilder(),
                    new RecordStatusAndNodeTypeQueryBuilder(),
                    new SourceFlagAndContainerIdQueryBuilder(),
                    new TermQueryBuilder(),
                    new SCsIdNotContainsQueryBuilder()
                },
                //advanced search filters
                AdvancedQueryBuilders = new List<IAdvancedQueryBuilder> {
                    //new NameOrUniqueIdQueryBuilder(),
                    new NameQueryBuilder(),
                    new UniqueIdQueryBuilder(),
                    new CustomColumnSingleTextQueryBuilder(),
                    new CustomColumnDatetimeQueryBuilder(),
                    new CustomColumnMultiChoiceQueryBuilder(),
                    new CustomColumnMultiTextQueryBuilder(),
                    new CustomColumnNumberQueryBuilder(),
                    new CustomColumnPeopleOrGroupQueryBuilder(),
                    new CustomColumnSingleChoiceQueryBuilder(),
                    new CustomColumnYesOrNoQueryBuilder(),
                    new CreatedByQueryBuilder(),
                    new CreatedTimeQueryBuilder(),
                    new DeclareAsRecordQueryBuilder(),
                    new LockedByRecordLabelQueryBuilder(),
                    new DisposalDueDateQueryBuilder(),
                    new FileExtensionQueryBuilder(),
                    new HoldByQueryBuilder(),
                    new HoldStatusQueryBuilder(),
                    new ModifiedByQueryBuilder(),
                    new ModifiedTimeQueryBuilder(),
                    new NodeIdQueryBuilder(),
                    new PhysicalTemplateQueryBuilder(),
                    new RecordOwnerQueryBuilder(),
                    new LoanDateQueryBuilder(),
                    //new RecordStatusQueryBuilder(),
                    new SourceFlagQueryBuilder(),
                    new TermQueryBuilder(),
                    new SPTreeFilterQueryBuilder(),
                    new ContentArchivedQueryBuilder(),
                    new RuleIdNotContainsFilterV3Builder(),
                    new TermIdNotContainFilterV3Builder(),
                    new RecordStatusEqualsQueryV3Builder(),
                    new RecordLoanPickStatusQueryBuilder(),
                    new RecordDestructionPickStatusQueryBuilder(),
                    new IdQueryBuilder(),
                    new TrainingScopeQueryBuilder(),
                    new TrainingTermIdQueryBuilder(),
                    new PredictTermIdQueryBuilder(),
                    new PredictTimeQueryBuilder(),
                    new MLApproveStatusQueryBuilder(),
                    new TeamsTreeFilterQueryBuilder(),
                    new NodeTypeQueryBuilder(),
                    new ContainerIdQueryBuilder(),
                    new ScopeIdQueryBuilder(),
                    new WebIdArrayQueryBuilder(),
                    new GoogleTreeFilterQueryBuilder(),
                    new TermIdArrayQueryBuilder(),
                    new PlaceHoldUsersQueryBuilder(),
                    new PhysicalLocationQueryBuilder(),
                    new DirPathListQueryBuilder(),
                },
            };
        }

        /// <summary>
        /// create a builder for searching only source flag, dir path and extension for file
        /// </summary>
        /// <returns></returns>
        public static SqlQuerySpecBuilder CreateDirPathSuggestionSearchBuilder()
        {
            return new SqlQuerySpecBuilder()
            {
                EnableOrderBy = false,
                FilterBuilder =
                [
                    new PermissionIdQueryBuilder(),
                    new SourceFlagAndContainerIdQueryBuilder(),
                    new NodeTypeQueryBuilder(),
                    new SCsIdNotContainsQueryBuilder(),
                    new RecordStatusQueryBuilder()
                ],
                AdvancedQueryBuilders =
                [
                    new SourceFlagQueryBuilder(),
                    new NodeTypeAdvancedQueryBuilder(),
                    new DirPathSuggestQueryBuilder(),
                ],
            };
        }

        /// <summary>
        /// create a builder for physical explorer
        /// </summary>
        /// <returns></returns>
        public static SqlQuerySpecBuilder CreatePhysicalExplorerBuilder()
        {
            return new SqlQuerySpecBuilder()
            {
                SearchBuilder = new List<ISearchBuilder> {
                    new NameQueryBuilder(),
                    new UniqueIdQueryBuilder(),
                },
                FilterBuilder = new List<IFilterBuilder> {
                    new CreatedByQueryBuilder(),
                    new IdNotContainQueryBuilder(),
                    new ModifiedByQueryBuilder(),
                    //new NodeTypeQueryBuilder(), // do not add NodeTypeQueryBuilder here because it is processed in PhysicalShallowQueryBuilder and PhysicalDeepQueryBuilder,
                    //new PermissionIdNotContainQueryBuilder(),
                    //new PermissionIdQueryBuilder(),
                    new PhysicalPermissionQueryBuilder(),
                    new PhysicalDeepQueryBuilder(),
                    new SourceFlagQueryBuilder(),
                    new PhysicalShallowQueryBuilder(),
                    //new PhysicalBoxQueryBuilder(),
                    //new PhysicalFileQueryBuilder(),
                    //new PhysicalLocationQueryBuilder(),
                    new RecordOwnerQueryBuilder(),
                    new RecordStatusQueryBuilder(),
                    new TermIdQueryBuilder(),
                }
            };
        }
    }
}
