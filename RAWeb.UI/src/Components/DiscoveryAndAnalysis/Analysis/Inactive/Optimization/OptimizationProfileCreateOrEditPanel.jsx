// import { useState, useImperativeHandle, useRef } from "react";
// import {
//     DiscoveryActionType,
//     DiscoverySizeRangeQueryMode,
// } from "../../Constants";
// import WithoutModifiedDate from "../../Components/WithoutModifiedDate";
// import { forwardRef } from "react";
// import SizeRang from "../../Components/SizeRange";
// import FileExtension from "../../Components/FileExtension";
// import SortBy from "../../Components/SortBy";
// import useStableCallback from "../../../../Common/Hooks/useStableCallback";
// import ProfileRequester from "../../requests/ProfileRequester";
// import { showToast } from "../../../../../Utilities/CommonUtil";

// const panelTitleMap = new Map([
//     [DiscoveryActionType.Create, RMResx.RM_DA_Analysis_DiscoveryActionType_Create],
//     [DiscoveryActionType.Edit, RMResx.RM_DA_Analysis_DiscoveryActionType_Edit],
// ]);

// const defaultProfileInfo = {
//     name: "",
//     sizeRange: -1,
//     sizeRangeQueryMode: DiscoverySizeRangeQueryMode.GenerateThanEqual,
//     greaterThanEqualWithoutInDate: -1,
//     lessThanEqualWithoutInDate: 999,
//     fileExtensionIds: [],
//     ruleIds: [],
//     sortBy: "FileTotalSize",
// };

// const buildInSortByColumns = [
//     {
//         internalName: "FileTotalSize",
//         displayName: RMResx.RM_FA_ROTRule_TreeNode_SizeDataSize,
//     },
//     {
//         internalName: "FileSumCount",
//         displayName: RMResx.RM_DA_Profile_ProfileFileCount,
//     },
//     {
//         internalName: "InactiveFileTotalSize",
//         displayName: RMResx.RM_DA_Profile_ProfileInactiveDataSizeGB,
//     },
//     {
//         internalName: "InactiveFileSumCount",
//         displayName: RMResx.RM_DA_Profile_ProfileInactiveFileCount,
//     },
// ];

// const InactiveOptimizationProfileCreateOrEditPanel = (
//     { o365TenantId, reRenderProfilesFunc },
//     ref
// ) => {
//     const [showPanel, setShowPanel] = useState(false);

//     const [actionType, setActionType] = useState(DiscoveryActionType.Create);

//     const [profileInfo, setProfileInfo] = useState(
//         _.cloneDeep(defaultProfileInfo)
//     );

//     const [existsProfileInfoes, setExistsProfileInfoes] = useState([]);

//     const [validateInfoes, setValidateInfoes] = useState(new Map());

//     const [validateNameLength, setValidateNameLength] = useState(false);

//     useImperativeHandle(ref, () => ({
//         onAdd: (profileInfo, profileInfoes = []) => {
//             setShowPanel(true);
//             setActionType(DiscoveryActionType.Create);
//             setExistsProfileInfoes(profileInfoes);
//             if (profileInfo == null) {
//                 setProfileInfo(_.cloneDeep(defaultProfileInfo));
//                 return;
//             }

//             setProfileInfo(_.cloneDeep(profileInfo));
//         },
//         onEdit: (profileInfo, profileInfoes = []) => {
//             setActionType(DiscoveryActionType.Edit);
//             setShowPanel(true);
//             setProfileInfo(profileInfo);
//             setExistsProfileInfoes(profileInfoes);
//         },
//     }));

//     const onProfileNameChange = (value) => {
//         setValidateNameLength(false);
//         if(value.length > 255)
//         {
//             setValidateNameLength(true);
//         }
//         const clonedProfileInfo = _.cloneDeep(profileInfo);
//         clonedProfileInfo.name = value;
//         setProfileInfo(clonedProfileInfo);
//         if(validateInfoes.has("name")) {
//             const clonedValidateInfoes = _.cloneDeep(validateInfoes);
//             clonedValidateInfoes.delete("name");
//             setValidateInfoes(clonedValidateInfoes);
//         }
//     };

//     const onDateRangeChange = (queryParameter) => {
//         const clonedProfileInfo = _.cloneDeep(profileInfo);
//         clonedProfileInfo.greaterThanEqualWithoutInDate =
//             queryParameter.withoutDateQueryParameter.from;
//         clonedProfileInfo.lessThanEqualWithoutInDate =
//             queryParameter.withoutDateQueryParameter.to;
//         setProfileInfo(clonedProfileInfo);
//     };

//     const onSizeRangeChange = (queryParameter) => {
//         const clonedProfileInfo = _.cloneDeep(profileInfo);
//         clonedProfileInfo.sizeRange =
//             queryParameter.sizeRangeQueryParameter.sizeRange;
//         clonedProfileInfo.sizeRangeQueryMode =
//             queryParameter.sizeRangeQueryParameter.queryMode;
//         setProfileInfo(clonedProfileInfo);
//     };

//     const onFileExtensionChange = (queryParameter) => {
//         const clonedProfileInfo = _.cloneDeep(profileInfo);
//         clonedProfileInfo.fileExtensionIds =
//             queryParameter.fileExtensionQueryParameter.fileExtensions;
//         setProfileInfo(clonedProfileInfo);
//     };

//     const onSortByChange = (queryParameter) => {
//         const clonedProfileInfo = _.cloneDeep(profileInfo);
//         clonedProfileInfo.sortBy = queryParameter.nodeQueryParameter.sortBy;
//         setProfileInfo(clonedProfileInfo);
//     };

//     const onSave = useStableCallback(async () => {

//         const clonedValidateInfoes = _.cloneDeep(validateInfoes);

//         if(_.isEmpty(profileInfo.name)) {
//             clonedValidateInfoes.set("name", RMResx.RM_PRM_PRE_ColumnValid_RequireText);
//             setValidateInfoes(clonedValidateInfoes);
//             return false;
//         }
//         if (validateNameLength) {
//             clonedValidateInfoes.set("name", RMResx.RM_PF_profileNametooLong);
//             setValidateInfoes(clonedValidateInfoes);
//             return false;
//         }
//         if(existsProfileInfoes.some(item => item.name.toLowerCase().trim() === profileInfo.name.toLowerCase().trim() && item.id !== profileInfo.id)) {
//             clonedValidateInfoes.set("name", RMResx.RM_DA_Profile_ProfileName_Exists);
//             setValidateInfoes(clonedValidateInfoes);
//             return false;
//         }
//         const clonedProfileInfo = _.cloneDeep(profileInfo);
//         clonedProfileInfo.o365TenantId = o365TenantId;
//         if(actionType == DiscoveryActionType.Create) {
//             const res = await ProfileRequester.addInactiveProfileInfo(clonedProfileInfo);
//             if(res.MessageType == 1) {
//                 showToast.error(res.ErrorMessage);
//                 return false;
//             }
//             else {
//                 showToast.success(<$g.I18NProvider msg={RMResx.RM_DA_Profile_ProfileSave}>
//                     <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
//                 </$g.I18NProvider>);
//             }
//         }
//         else {
//             const res = await ProfileRequester.updateInactiveProfileInfo(clonedProfileInfo);
//             if(res.MessageType == 1) {
//                 showToast.error(res.ErrorMessage);
//                 return false;
//             }
//             else {
//                 showToast.success(<$g.I18NProvider msg={RMResx.RM_DA_Profile_ProfileUpdated}>
//                     <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
//                 </$g.I18NProvider>);
//             }
//         }

//         setShowPanel(false);

//         if(!_.isNil(reRenderProfilesFunc)) {
//             await reRenderProfilesFunc(actionType);
//         }

//         return true;
//     });

//     return (
//         <R.Panel
//             id="reco-inactive-profile-panel"
//             title={panelTitleMap.get(actionType)}
//             size={660}
//             status={{ show: showPanel }}
//             onHide={() => setShowPanel(false)}
//             destroy={false}
//         >
//             <div className="reco-inactive-profile-panel">
//                 <div className="reco-profile-field-item">
//                     <div className="reco-profile-field-title require">
//                         {RMResx.RM_DA_Profile_ProfileName}
//                     </div>
//                     <div className="reco-profile-field-input">
//                         <R.Input
//                             value={profileInfo.name}
//                             type="text"
//                             width={"100%"}
//                             onChange={onProfileNameChange}
//                             aria={{ ariaLabel: RMResx.RM_DA_Profile_ProfileName }}
//                         />
//                         <div
//                             className="reco-profile-field-validate-message"
//                             hidden={!validateInfoes.has("name")}
//                             tabIndex="0"
//                         >
//                             {validateInfoes.get("name")}
//                         </div>
//                     </div>
//                 </div>
//                 <div className="reco-profile-field-item">
//                     <div className="reco-profile-field-title require" tabIndex="0">
//                         {RMResx.RM_DA_Profile_ProfileModifiedTimeRange}
//                     </div>
//                     <div
//                         className="reco-profile-field-input"
//                         style={{ width: 374 }}
//                     >
//                         <WithoutModifiedDate
//                             queryParameter={{
//                                 withoutDateQueryParameter: {
//                                     from: profileInfo.greaterThanEqualWithoutInDate,
//                                     to: profileInfo.lessThanEqualWithoutInDate,
//                                 },
//                             }}
//                             onChange={onDateRangeChange}
//                         />
//                     </div>
//                 </div>
//                 <div className="reco-profile-field-item">
//                     <div id="ariaInactiveSizeRang" className="reco-profile-field-title require">
//                         {RMResx.RM_DA_Profile_ProfileFileSize}
//                     </div>
//                     <div className="reco-profile-field-input">
//                         <SizeRang
//                             queryParameter={{
//                                 sizeRangeQueryParameter: {
//                                     sizeRange: profileInfo.sizeRange,
//                                     queryMode: profileInfo.sizeRangeQueryMode,
//                                 },
//                             }}
//                             onChange={onSizeRangeChange}
//                             ariaId="ariaInactiveSizeRang"
//                         />
//                     </div>
//                 </div>
//                 <div className="reco-profile-field-item">
//                     <div id="ariaInactiveFileExtension" className="reco-profile-field-title require">
//                         {RMResx.RM_DA_Profile_ProfileFileType}
//                     </div>
//                     <div className="reco-profile-field-input">
//                         <FileExtension
//                             o365TenantId={o365TenantId}
//                             queryParameter={{
//                                 fileExtensionQueryParameter: {
//                                     fileExtensions:
//                                         profileInfo.fileExtensionIds,
//                                 },
//                             }}
//                             onChange={onFileExtensionChange}
//                             ariaId="ariaInactiveFileExtension"
//                         />
//                     </div>
//                 </div>
//                 <div className="reco-profile-field-item">
//                     <div id="ariaInactiveSortBy" className="reco-profile-field-title require">
//                         {RMResx.RM_DA_Profile_ProfileSortBy}
//                     </div>
//                     <div className="reco-profile-field-input">
//                         <SortBy
//                             sortByColumns={_.cloneDeep(buildInSortByColumns)}
//                             queryParameter={{
//                                 nodeQueryParameter: {
//                                     sortBy: profileInfo.sortBy,
//                                 },
//                             }}
//                             onChange={onSortByChange}
//                             ariaId="ariaInactiveSortBy"
//                         />
//                     </div>
//                 </div>
//             </div>
//             <>
//                 <R.Button
//                     slot="buttons"
//                     text={RMResx.RM_JS_Common_Cancel}
//                     onClick={() => setShowPanel(false)}
//                 />
//                 <R.Button
//                     slot="buttons"
//                     primary
//                     classify="theme"
//                     text={RMResx.RM_JS_Common_Save}
//                     onClick={onSave}
//                 />
//             </>
//         </R.Panel>
//     );
// };

// export default forwardRef(InactiveOptimizationProfileCreateOrEditPanel);
