// import { useEffect, useState } from "react";
// import "./index.less";
// import TotalMutableData from "../Components/Office365/TotalMutableData";
// import PieChartWithProgress from "../../Components/PieChartWithProgress";
// import { KeyValuePairLabel } from "../../Components/Label";
// import {
//     BasicDataRequester,
//     ProfileInactiveDataRequester,
// } from "../../requests";
// import DiscoveryDataView from "../../Components/DiscoveryDataView";
// import {
//     DiscoveryActionType,
//     DiscoveryJobStatus,
//     DiscoveryNodeViewMode,
//     DiscoveryQueryDataType,
//     DiscoveryTotalDataType,
// } from "../../Constants";
// import { useRef } from "react";
// import OptimizationProfileCreateOrEditPanel from "./OptimizationProfileCreateOrEditPanel";
// import ProfileRequester from "../../requests/ProfileRequester";
// import { CalculateUtil, JobUtil } from "../../Utils";
// import { LicenseHelper, showToast } from "../../../../../Utilities/CommonUtil";
// import { checkPermission } from "../../../../../Utilities/permissionManager";
// import JobManagerRequester from "../../requests/JobMangerRequester";
// import DataOptimizePanel from "../../Components/DataOptimizePanel";

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
//         pageSize: 5,
//     },
//     rotRuleQueryParameter: {
//         ruleCategories: [
//             {
//                 ruleCategory: 2,
//                 ruleIds: [],
//                 checked: true,
//             },
//             {
//                 ruleCategory: 3,
//                 ruleIds: [],
//                 checked: true,
//             },
//             {
//                 ruleCategory: 4,
//                 ruleIds: [],
//                 checked: true,
//             },
//         ],
//     },
//     fileExtensionQueryParameter: {},
//     needCalculateTotalDataTypes: [
//         DiscoveryTotalDataType.SizeAndCount,
//         DiscoveryTotalDataType.Sites,
//     ],
//     nodeChangeNeedRerender: true,
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
//                 displayName: RMResx.RM_FA_TableColumn_InScope,
//                 internalName: "inScope",
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
//                 displayName:
//                     RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
//                 internalName: "inactiveFileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName:
//                     RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
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
//                 displayName: RMResx.RM_FA_TableColumn_InScope,
//                 internalName: "inScope",
//                 isAggregateField: true,
//                 isPlaceholder: true,
//                 width: 200,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_FileSize,
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
//                 displayName:
//                     RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
//                 internalName: "inactiveFileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName:
//                     RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
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
//                 displayName: RMResx.RM_FA_TableColumn_InScope,
//                 internalName: "inScope",
//                 isPlaceholder: true,
//                 isAggregateField: true,
//             },
//             {
//                 displayName: RMResx.RM_FA_Inactive_TableColumn_FileSize,
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
//                 displayName:
//                     RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
//                 internalName: "inactiveFileTotalSize",
//                 isAggregateField: true,
//                 width: 200,
//             },
//             {
//                 displayName:
//                     RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
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
// ]);

// const buildInProfileInfoes = [
//     {
//         id: "00000000-0000-0000-0000-000000000000",
//         o365TenantId: "00000000-0000-0000-0000-000000000000",
//         name: RMResx.RM_FA_Inactive_Default_ProfileName,
//         status: DiscoveryJobStatus.Waiting,
//         modifiedTimeRangeLabel: RMResx.RM_DA_Profile_ProfileModifiedTimeRange,
//         sizeRangeLabel: RMResx.RM_DA_ProfleInfoes_ALL,
//         fileTypeLabel: RMResx.RM_DA_ProfleInfoes_ALL,
//     },
// ];

// const isShowButton = checkPermission(
//     "Archiver_Discovery_Optimization_RunJob",
//     RM.UserResources
// );

// const InactiveOptimizationV3 = ({ o365TenantId, jobInfo }) => {
//     const optimizePanelRef = useRef(null);

//     const dataViewRef = useRef(null);

//     const profilePanelRef = useRef(null);

//     const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

//     const [totalDataInfo, setTotalDataInfo] = useState({
//         fileTotalSize: 0,
//         optimizableFileTotalSize: 0,
//         optimizableFileSumCount: 0,
//     });

//     const [profileInfoes, setProfileInfoes] = useState(buildInProfileInfoes);

//     const [selectedProfileInfo, setSelectedProfileInfo] = useState(
//         buildInProfileInfoes[0]
//     );

//     useEffect(() => {
//         const fetchProfileInfoes = async () => {
//             const profileInfoes = await ProfileRequester.getInactiveProfileInfoes(o365TenantId);
//             if (_.isNil(profileInfoes) || profileInfoes.length === 0) {
//                 return;
//             }
//             const defaultIndex = profileInfoes.findIndex((item) => item.isDefault);
//             setProfileInfoes(profileInfoes);
//             setSelectedProfileInfo(profileInfoes[defaultIndex]);
//             const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
//             clonedQueryParameter.nodeQueryParameter.sortBy = profileInfoes[defaultIndex].sortBy;
//             clonedQueryParameter.nodeQueryParameter.isDesc = true;
//             clonedQueryParameter.o365TenantId = o365TenantId;
//             clonedQueryParameter.profileId = profileInfoes[defaultIndex].id;
//             setQueryParameter(clonedQueryParameter);
//         };
//         fetchProfileInfoes();
//     }, [o365TenantId]);

//     useEffect(() => {
//         const reRenderColumns = async () => {
//             await dataViewRef.current.reRenderColumns();
//             const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
//             clonedQueryParameter.o365TenantId = o365TenantId;
//             clonedQueryParameter.profileId = selectedProfileInfo.id;
//             clonedQueryParameter.nodeQueryParameter.sortBy = selectedProfileInfo.sortBy;
//             clonedQueryParameter.nodeQueryParameter.isDesc = true;
//             setQueryParameter(clonedQueryParameter);
//         };
//         reRenderColumns();
//     }, [selectedProfileInfo, o365TenantId]);

//     useEffect(() => {
//         const fetchAggregateInfo = async () => {
//             const res = await ProfileInactiveDataRequester.queryAggregateInfo(queryParameter);
//             setTotalDataInfo(res);
//         };
//         fetchAggregateInfo();
//     }, [queryParameter]);

//     const getProfileOptions = (selectedProfileId) => {
//         return profileInfoes.map((item) => ({
//             name: item.name,
//             value: item.id,
//             isLoading:
//                 item.status === DiscoveryJobStatus.Waiting ||
//                 item.status === DiscoveryJobStatus.Running,
//             checked: item.id === selectedProfileId,
//             isDefault: item.isDefault,
//         }));
//     };

//     const onWillSelectProfileChange = (args) => {
//         if (args.newValue.isLoading) {
//             $$.messagedialog(true, {
//                 width: "550px",
//                 hideActions: false,
//                 title: RMResx.RM_JS_Common_Confirmation,
//                 content: RMResx.RM_DA_Profile_CalculatProfile,
//                 buttons: [
//                     {
//                         text: RMResx.RM_JS_Common_OK,
//                         primary: true,
//                         classify: "theme",
//                         onClick: () => true,
//                     },
//                 ],
//             });
//             return false;
//         }
//     };

//     const onSelectedProfileChange = (args) => {
//         const id = args.newValue.value;
//         const index = profileInfoes.findIndex((item) => item.id == id);
//         setSelectedProfileInfo(profileInfoes[index]);

//         const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
//         clonedQueryParameter.o365TenantId = o365TenantId;
//         clonedQueryParameter.profileId = profileInfoes[index].id;
//         clonedQueryParameter.nodeQueryParameter.sortBy =
//             profileInfoes[index].sortBy;
//         clonedQueryParameter.nodeQueryParameter.isDesc = true;

//         setQueryParameter(clonedQueryParameter);
//     };

//     const getTableColumns = async () => {
//         const ruleColumns = await BasicDataRequester.getInactiveTableColumns();
//         ruleColumns.forEach((a) => {
//             a.displayName += " (GB)";
//         });
//         const clonedBuildInColumns = _.cloneDeep(buildInColumns);
//         if (!_.isNil(ruleColumns)) {
//             clonedBuildInColumns.forEach((i) => {
//                 i.forEach((c) => {
//                     if (
//                         !_.isNil(selectedProfileInfo.sortBy) &&
//                         c.internalName.toLowerCase() ===
//                             selectedProfileInfo.sortBy.toLowerCase()
//                     ) {
//                         c.sortable = true;
//                         c.sortField = selectedProfileInfo.sortBy;
//                     }
//                 });
//                 ruleColumns.forEach((j) => {
//                     j.width = 200;
//                     j.isAggregateField = true;
//                     i.push(j);
//                 });
//             });
//         }

//         return clonedBuildInColumns;
//     };

//     const queryNodeDataInfo = async (queryParameter) => {
//         const res =
//             await ProfileInactiveDataRequester.queryOptimizationNodesData(
//                 queryParameter
//             );
//         res.items = await CalculateUtil.CalculateInactivesNodesData(res.items);
//         return res;
//     };

//     const queryNodeTotalAggregateInfo = async (queryParameter) => {
//         const res =
//             await ProfileInactiveDataRequester.queryOptimizationNodeTotalAggregateInfo(
//                 queryParameter
//             );
//         return await CalculateUtil.CalculateInactivesNodeTotalAggregateInfo(
//             res
//         );
//     };

//     const onProfileCreate = () => {
//         profilePanelRef.current.onAdd(null, profileInfoes);
//     };

//     const onProfileEdit = () => {
//         profilePanelRef.current.onEdit(selectedProfileInfo, profileInfoes);
//     };

//     const onProfileDelete = () => {
//         $$.messagedialog(true, {
//             width: "550px",
//             hideActions: false,
//             title: RMResx.RM_JS_Common_Confirmation,
//             content: RMResx.RM_DA_Profile_WillDeleteProfile,
//             buttons: [
//                 {
//                     text: RMResx.RM_JS_Common_Cancel,
//                     onClick: () => {
//                         $$.messagedialog(false);
//                     },
//                 },
//                 {
//                     text: RMResx.RM_JS_Common_OK,
//                     primary: true,
//                     classify: "theme",
//                     onClick: async () => {
//                         $$.messagedialog(false);
//                         const res =
//                             await ProfileRequester.deleteInactiveProfileInfo(
//                                 selectedProfileInfo
//                             );
//                         if (res.MessageType == 1) {
//                             showToast.error(res.ErrorMessage);
//                             return false;
//                         } else {
//                             showToast.success(RMResx.RM_DA_Profile_DeleteProfile);
//                             await reRenderProfiles(DiscoveryActionType.Delete);
//                             return true;
//                         }
//                     },
//                 },
//             ],
//         });
//     };

//     const reRenderProfiles = async (actionType) => {
//         var profileInfoes = await ProfileRequester.getInactiveProfileInfoes(
//             o365TenantId
//         );
//         if (actionType === DiscoveryActionType.Create) {
//             setProfileInfoes(profileInfoes);
//         } else {
//             const defaultIndex = profileInfoes.findIndex(
//                 (item) => item.isDefault
//             );
//             setProfileInfoes(profileInfoes);
//             setSelectedProfileInfo(profileInfoes[defaultIndex]);
//         }
//     };

//     const onDataOptimizeClick = async () => {
//         const jobStatusInfo = await JobManagerRequester.getLatest();
//         const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
//         clonedQueryParameter.nodeQueryParameter =
//             queryParameter.nodeQueryParameter;
//         clonedQueryParameter.o365TenantId = o365TenantId;
//         if (
//             !_.isNil(selectedProfileInfo.fileExtensionIds) &&
//             selectedProfileInfo.fileExtensionIds.length > 0
//         ) {
//             clonedQueryParameter.fileExtensionQueryParameter.fileExtensions =
//                 selectedProfileInfo.fileExtensionIds;
//         }

//         if (selectedProfileInfo.sizeRange > -1) {
//             clonedQueryParameter.sizeRangeQueryParameter.sizeRange =
//                 selectedProfileInfo.sizeRange;
//             clonedQueryParameter.sizeRangeQueryParameter.queryMode =
//                 selectedProfileInfo.sizeRangeQueryMode;
//         }

//         clonedQueryParameter.withoutDateQueryParameter.from =
//             selectedProfileInfo.greaterThanEqualWithoutInDate;
//         clonedQueryParameter.withoutDateQueryParameter.to =
//             selectedProfileInfo.lessThanEqualWithoutInDate;

//         optimizePanelRef.current.onShow(
//             clonedQueryParameter,
//             o365TenantId,
//             jobStatusInfo
//         );
//     };

//     return (
//         <div className="reco-inactive-optimization-container">
//             {LicenseHelper.EnableRecordsArchiver() &&
//                 isShowButton &&
//                 (queryParameter.nodeQueryParameter.containerIds.length > 0 ||
//                     queryParameter.nodeQueryParameter.siteIds.length > 0) && (
//                     <div>
//                         <R.Button
//                             id="raDataOptimizeBtn"
//                             primary={true}
//                             classify="theme"
//                             text={RMResx.RM_FA_DataOptimize_OptimizePanelBtn}
//                             onClick={onDataOptimizeClick}
//                         />
//                     </div>
//                 )}
//             <section className="reco-data">
//                 <div className="reco-profile-action-bar">
//                     <div className="reco-profile-selector">
//                         <R.Combobox
//                             id="raInacitveProfileSelector"
//                             items={getProfileOptions(selectedProfileInfo.id)}
//                             disabled={JobUtil.isRunning(jobInfo)}
//                             textField="name"
//                             valueField="value"
//                             width={"100%"}
//                             customTrigger={true}
//                             onChange={onSelectedProfileChange}
//                             willChange={onWillSelectProfileChange}
//                             searchable={false}
//                             template={(item) => {
//                                 return (
//                                     <div className="reco-profile-custom-combobox-item">
//                                         <div>
//                                             <div
//                                                 className={
//                                                     item.checked
//                                                         ? "fia-check"
//                                                         : "fia-check reco-profile-custom-combobox-checked-hidden"
//                                                 }
//                                             ></div>
//                                             <div className="reco-profile-custom-combobox-item-name">
//                                                 {item.name}
//                                             </div>
//                                         </div>
//                                         {item.isLoading ? (
//                                             <div className="fia-in-progress reco-profile-custom-combobox-item-loading"></div>
//                                         ) : item.isDefault ? (
//                                             <div className="reco-profile-custom-bombobox-item-default">
//                                                 {"(Default)"}
//                                             </div>
//                                         ) : (
//                                             <div></div>
//                                         )}
//                                     </div>
//                                 );
//                             }}
//                         >
//                             <R.Button
//                                 id="raInacitveProfileSelectorBtn"
//                                 icon="fia-triangle-down"
//                                 className="hs-manage-column-btn reverse"
//                                 text={selectedProfileInfo.name}
//                                 tooltip={selectedProfileInfo.name}
//                             />
//                         </R.Combobox>
//                         <span className="fia-select-all"></span>
//                     </div>
//                     <div className="reco-profile-action">
//                         {!JobUtil.isRunning(jobInfo) && (
//                             <R.Button
//                                 id="raNewProfileBtn"
//                                 text={RMResx.RM_DA_Profile_CreateProfile}
//                                 primary={false}
//                                 classify="default"
//                                 icon="fia-plus"
//                                 onClick={onProfileCreate}
//                             />
//                         )}
//                         {!JobUtil.isRunning(jobInfo) &&
//                             !selectedProfileInfo.isBuildIn && (
//                                 <R.ButtonGroup
//                                     id="raRcManagementMoreBtn"
//                                     type="action"
//                                     classify="default"
//                                     tooltip={RMResx.RM_PRM_PRE_More}
//                                 >
//                                     <R.Button
//                                         id="raProfileEditBtn"
//                                         text={RMResx.RM_JS_Common_Edit}
//                                         icon="fia-edit"
//                                         onClick={onProfileEdit}
//                                     />
//                                     <R.Button
//                                         id="raProfileDeleteBtn"
//                                         text={RMResx.RM_JS_Common_Delete}
//                                         icon="fia-delete"
//                                         onClick={onProfileDelete}
//                                     />
//                                 </R.ButtonGroup>
//                             )}
//                     </div>
//                 </div>
//                 <div className="reco-profile-view-bar">
//                     <KeyValuePairLabel
//                         maxWidth={360}
//                         height={24}
//                         keyText={RMResx.RM_DA_Profile_ProfileModifiedTimeRange}
//                         valueText={selectedProfileInfo.modifiedTimeRangeLabel}
//                     />
//                     <KeyValuePairLabel
//                         maxWidth={360}
//                         height={24}
//                         keyText={RMResx.RM_DA_Profile_ProfileFileSize}
//                         valueText={selectedProfileInfo.sizeRangeLabel}
//                     />
//                     <KeyValuePairLabel
//                         maxWidth={360}
//                         height={24}
//                         keyText={RMResx.RM_DA_Profile_ProfileFileType}
//                         valueText={selectedProfileInfo.fileTypeLabel}
//                     />
//                 </div>
//             </section>
//             <section className="reco-data reco-profile-total-data">
//                 <div className="reco-mutable-data">
//                     <TotalMutableData
//                         data={[
//                             {
//                                 text: RMResx.RM_DA_Optimization_OptimizationV3_OptimizableFileTotalSize,
//                                 value: totalDataInfo.optimizableFileTotalSize,
//                             },
//                             {
//                                 text: RMResx.RM_DA_Optimization_OptimizationV3_OptimizableFileSumCount,
//                                 value: totalDataInfo.optimizableFileSumCount,
//                             },
//                         ]}
//                     />
//                 </div>
//                 <div className="reco-percentage-chart">
//                     <PieChartWithProgress
//                         total={totalDataInfo.fileTotalSize}
//                         active={
//                             totalDataInfo.fileTotalSize === 0
//                                 ? totalDataInfo.fileTotalSize
//                                 : Number.parseInt(
//                                       (
//                                           totalDataInfo.optimizableFileTotalSize /
//                                           totalDataInfo.fileTotalSize /
//                                           1.0
//                                       )
//                                           .toFixed(2)
//                                           .replace(".", "")
//                                   ) === 0 && totalDataInfo.fileTotalSize !== 0
//                                 ? 0
//                                 : Number.parseInt(
//                                       (
//                                           totalDataInfo.optimizableFileTotalSize /
//                                           totalDataInfo.fileTotalSize /
//                                           1.0
//                                       )
//                                           .toFixed(2)
//                                           .replace(".", "")
//                                   )
//                         }
//                         name={RMResx.RM_FA_Inactive_SummaryTab_PieChartTitle}
//                         unit={RMResx.RM_JS_RDM_CreateRule_Unit_GB}
//                     />
//                 </div>
//             </section>
//             <section className="reco-data">
//                 <DiscoveryDataView
//                     title={
//                         RMResx.RM_FA_Inactive_OptimizationTab_InactiveOptimizationTitle
//                     }
//                     getColumns={getTableColumns}
//                     queryNodeDataInfo={queryNodeDataInfo}
//                     queryParameter={queryParameter}
//                     onChange={setQueryParameter}
//                     queryNodeTotalAggregateInfo={queryNodeTotalAggregateInfo}
//                     ref={dataViewRef}
//                     hasSearchbox
//                 />
//             </section>
//             <OptimizationProfileCreateOrEditPanel
//                 o365TenantId={o365TenantId}
//                 reRenderProfilesFunc={reRenderProfiles}
//                 ref={profilePanelRef}
//             />
//             <DataOptimizePanel ref={optimizePanelRef} />
//         </div>
//     );
// };

// export default InactiveOptimizationV3;
