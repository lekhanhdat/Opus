// import { useEffect, useState } from "react";
// import TotalSummary from "../../Components/TotalSumamry";
// import "./index.less";
// import _ from "lodash";
// import WithoutModifiedDate from "../../Components/WithoutModifiedDate";
// import DiscoveryDataView from "../../Components/DiscoveryDataView";
// import SizeRangeChart from "../../Components/SizeRangeChart";
// import FileTypeChart from "../../Components/FileTypeChart";
// import {
//     DiscoveryNodeViewMode,
//     DiscoveryQueryDataType,
//     DiscoveryTotalDataType,
// } from "../../Constants";
// import TotalData from "../Components/Office365/TotalData";
// import { BasicDataRequester, InactiveDataRequester } from "../../requests";
// import { CalculateUtil } from "../../Utils";

// const ActionTab = {
//     Summary: 0,
//     Optimization: 1,
// };

// const buildInColumns = new Map([
//     [
//         DiscoveryNodeViewMode.Container,
//         [
//             {
//                 displayName: RMResx.RM_FA_TableColumn_Container,
//                 internalName: "name",
//                 isLink: true,
//                 width: 350,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_SiteCollection,
//                 internalName: "siteCount",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_TotalSize,
//                 internalName: "fileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
//                 internalName: "fileSumCount",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_InactiveTotalSize,
//                 internalName: "inactiveFileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_InactiveFileCount,
//                 internalName: "inactiveFileSumCount",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_Rate,
//                 internalName: "rate",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_Saving,
//                 internalName: "saving",
//                 isAggregateField: true,
//                 width: 200,
//             },
//         ],
//     ],
//     [
//         DiscoveryNodeViewMode.Site,
//         [
//             {
//                 displayName: RMResx.RM_FA_TableColumn_SiteCollection,
//                 internalName: "url",
//                 width: 350,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_TotalSize,
//                 internalName: "fileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
//                 internalName: "fileSumCount",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_InactiveTotalSize,
//                 internalName: "inactiveFileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_InactiveFileCount,
//                 internalName: "inactiveFileSumCount",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_Rate,
//                 internalName: "rate",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_Saving,
//                 internalName: "saving",
//                 isAggregateField: true,
//                 width: 200,
//             },
//         ],
//     ],
//     [
//         DiscoveryNodeViewMode.SiteInContainer,
//         [
//             {
//                 displayName: RMResx.RM_FA_TableColumn_SiteCollection,
//                 internalName: "url",
//                 width: 350,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_TotalSize,
//                 internalName: "fileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
//                 internalName: "fileSumCount",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_InactiveTotalSize,
//                 internalName: "inactiveFileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_InactiveFileCount,
//                 internalName: "inactiveFileSumCount",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_Rate,
//                 internalName: "rate",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_TableColumn_Saving,
//                 internalName: "saving",
//                 isAggregateField: true,
//                 width: 200,
//             },
//         ],
//     ]
// ]);

// const defaultQueryParameter = {
//     dataType: DiscoveryQueryDataType.Inactive,
//     withoutDateQueryParameter: {
//         from: -1,
//         to: 999,
//     },
//     sizeRangeQueryParameter: {},
//     nodeQueryParameter: {
//         viewMode: DiscoveryNodeViewMode.Container,
//         joinedContainerId: 0,
//         containerIds: [],
//         siteIds: [],
//         pageSize: 5
//     },
//     rotRuleQueryParameter:{},
//     fileExtensionQueryParameter: {},
//     needCalculateTotalDataTypes: [
//         DiscoveryTotalDataType.SizeAndCount,
//         DiscoveryTotalDataType.Sites,
//     ],
//     nodeChangeNeedRerender: true,
// };

// const InactiveSummaryHistoryVersion = ({ o365TenantId }) => {
//     const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

//     useEffect(() => {
//         if(_.isNil(o365TenantId)) {
//             return;
//         }
//         const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
//         clonedQueryParameter.o365TenantId = o365TenantId;
//         setQueryParameter(clonedQueryParameter);
//     }, [o365TenantId]);

//     const getTableColumns = async () => {
//         const ruleColumns = await BasicDataRequester.getInactiveTableColumns();
//         ruleColumns.forEach((a) => {
//             a.displayName += " (GB)"; 
//         });
//         const clonedBuildInColumns = _.cloneDeep(buildInColumns);
//         if (!_.isNil(ruleColumns)) {
//             clonedBuildInColumns.forEach((i) =>
//                 ruleColumns.forEach((j) => {
//                     j.width = 200;
//                     j.isAggregateField = true;
//                     i.push(j);
//                 })
//             );
//         }
//         return clonedBuildInColumns;
//     };

//     const queryNodeDataInfo = async (queryParameter) => {
//         const res = await InactiveDataRequester.querySummaryNodesData(queryParameter);
//         res.items = await CalculateUtil.CalculateInactivesNodesData(res.items);
//         return res;
//     };

//     const queryNodeTotalAggregateInfo = async (queryParameter) => {
//         const res = await InactiveDataRequester.querySummaryNodeTotalAggregateInfo(queryParameter);
//         return await CalculateUtil.CalculateInactivesNodeTotalAggregateInfo(res);
//     };

//     return (
//         <div className="reco-inactive-summary-container">
//             <div>
//                 <TotalSummary
//                     queryParameter={queryParameter}
//                 />
//             </div>
//             <div className="reco-data">
//                 <section className="reco-title">
//                     <span tabIndex="0">{RMResx.RM_FA_Inactive_SummaryTab_InactiveDataTitle}</span>
//                 </section>
//                 <div className="reco-discovery-split-line"></div>
//                 <section className="reco-basic-data">
//                     <div className="reco-modified-date">
//                         <WithoutModifiedDate
//                             title={RMResx.RM_FA_Inactive_ModifiedTitle}
//                             queryParameter={queryParameter}
//                             onChange={setQueryParameter}
//                         />
//                     </div>
//                     <TotalData
//                         tab={ActionTab.Summary}
//                         queryParameter={queryParameter}
//                     />
//                 </section>
//                 <div className="reco-discovery-split-line"></div>
//                 <section className="reco-node-data">
//                     <DiscoveryDataView
//                         getColumns={getTableColumns}
//                         queryParameter={queryParameter}
//                         onChange={setQueryParameter}
//                         queryNodeDataInfo={queryNodeDataInfo}
//                         queryNodeTotalAggregateInfo={queryNodeTotalAggregateInfo}
//                         hasSearchbox
//                     />
//                 </section>
//                 <div className="reco-discovery-split-line"></div>
//                 <section className="reco-chart-data">
//                     <div>
//                         <div className="reco-chart-title">
//                             {RMResx.RM_FA_Inactive_SummaryTab_DocumentSizeTitle}
//                         </div>
//                         <div className="reco-column-chart">
//                             <SizeRangeChart
//                                 id={"inactive_summary_size_range"}
//                                 height={300}
//                                 queryParameter={queryParameter}
//                                 onChange={setQueryParameter}
//                             />
//                         </div>
//                     </div>
//                     <div>
//                         <div className="reco-chart-title">
//                             {RMResx.RM_FA_Inactive_SummaryTab_FileTypeTitle}
//                         </div>
//                         <div className="reco-treemap-chart">
//                             <FileTypeChart
//                                 id={"inactive_summary_file_type"}
//                                 height={300}
//                                 queryParameter={queryParameter}
//                                 onChange={setQueryParameter}
//                                 queryData={InactiveDataRequester.queryFileExtensions}
//                             />
//                         </div>
//                     </div>
//                 </section>
//             </div>
//         </div>
//     );
// };

// export default InactiveSummaryHistoryVersion;
