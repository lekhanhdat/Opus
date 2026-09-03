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
using System.ServiceModel;
using Microsoft.Office.Project.Server.Schema;

namespace Microsoft.Office.Project.Server.Interfaces
{
	// Token: 0x0200000E RID: 14
	[ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/", Name = "PortfolioAnalyses")]
	[XmlSerializerFormat(Style = OperationFormatStyle.Document)]
	public interface IPortfolioAnalyses
	{
		// Token: 0x06000104 RID: 260
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadOptimizerSolutionList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadOptimizerSolutionListResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		OptimizerSolutionDataSet ReadOptimizerSolutionList(Guid analysisUid);

		// Token: 0x06000105 RID: 261
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadOptimizerSolution", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadOptimizerSolutionResponse")]
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		OptimizerSolutionDataSet ReadOptimizerSolution(Guid solutionUid);

		// Token: 0x06000106 RID: 262
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueDeleteOptimizerSolutions", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueDeleteOptimizerSolutionsResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		void QueueDeleteOptimizerSolutions(Guid[] solutionUids, Guid[] jobUids);

		// Token: 0x06000107 RID: 263
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueCreateOptimizerSolution", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueCreateOptimizerSolutionResponse")]
		void QueueCreateOptimizerSolution(Guid analysisUid, OptimizerSolutionDataSet solutionDS, Guid jobUid);

		// Token: 0x06000108 RID: 264
		[FaultContract(typeof(DefaultServerFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadAnalysis", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadAnalysisResponse")]
		[FaultContract(typeof(ServerExecutionFault))]
		AnalysisDataSet ReadAnalysis(Guid analysisUid);

		// Token: 0x06000109 RID: 265
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadAnalysisList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadAnalysisListResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		[FaultContract(typeof(ServerExecutionFault))]
		AnalysisDataSet ReadAnalysisList();

		// Token: 0x0600010A RID: 266
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/CreateDependency", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/CreateDependencyResponse")]
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		OptimizerDependencyDataSet CreateDependency(OptimizerDependencyDataSet dependencyDataSet);

		// Token: 0x0600010B RID: 267
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadDependency", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadDependencyResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		OptimizerDependencyDataSet ReadDependency(Guid dependencyUid);

		// Token: 0x0600010C RID: 268
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/DeleteDependencies", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/DeleteDependenciesResponse")]
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		void DeleteDependencies(Guid[] dependencyUids);

		// Token: 0x0600010D RID: 269
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/UpdateDependency", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/UpdateDependencyResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		OptimizerDependencyDataSet UpdateDependency(OptimizerDependencyDataSet dependencyDataSet);

		// Token: 0x0600010E RID: 270
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadDependencyList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadDependencyListResponse")]
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		OptimizerDependencyDataSet ReadDependencyList();

		// Token: 0x0600010F RID: 271
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadPlannerSolutionList", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadPlannerSolutionListResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		PlannerSolutionDataSet ReadPlannerSolutionList(Guid parentSolutionUid);

		// Token: 0x06000110 RID: 272
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadPlannerSolution", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/ReadPlannerSolutionResponse")]
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		PlannerSolutionDataSet ReadPlannerSolution(Guid solutionUid);

		// Token: 0x06000111 RID: 273
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueDeletePlannerSolutions", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueDeletePlannerSolutionsResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		void QueueDeletePlannerSolutions(Guid[] solutionUIDs, Guid[] jobUids);

		// Token: 0x06000112 RID: 274
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueCreatePlannerSolution", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueCreatePlannerSolutionResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		void QueueCreatePlannerSolution(PlannerSolutionDataSet plannerSolutionDataSet, Guid jobUid);

		// Token: 0x06000113 RID: 275
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueDeleteAnalyses", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueDeleteAnalysesResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		[FaultContract(typeof(ServerExecutionFault))]
		void QueueDeleteAnalyses(Guid[] analysesUIDs, Guid[] jobUids);

		// Token: 0x06000114 RID: 276
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueCreateAnalysis", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueCreateAnalysisResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		void QueueCreateAnalysis(AnalysisDataSet analysisDataSet, Guid jobUid);

		// Token: 0x06000115 RID: 277
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueUpdateAnalysis", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/QueueUpdateAnalysisResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		void QueueUpdateAnalysis(AnalysisDataSet analysisDataSet, bool forceRefreshPlannerData, Guid jobUid);

		// Token: 0x06000116 RID: 278
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/CommitOptimizerSolution", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/CommitOptimizerSolutionResponse")]
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		void CommitOptimizerSolution(Guid solutionUid);

		// Token: 0x06000117 RID: 279
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/CommitPlannerSolution", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PortfolioAnalyses/CommitPlannerSolutionResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		void CommitPlannerSolution(Guid solutionUid);
	}
}
