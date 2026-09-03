import { useEffect, useState } from "react";
import "./index.less";
import { KeyValuePairLabel } from "../../Components/Label";
import { BasicDataRequester, ProfileRotDataRequester } from "../../requests";
import DiscoveryDataView from "../../Components/DiscoveryDataView";
import {
    DiscoveryActionType,
    DiscoveryJobStatus,
    DiscoveryNodeViewMode,
    DiscoveryQueryDataType,
    DiscoveryRuleCategory,
    DiscoveryTotalDataType,
} from "../../Constants";
import { useRef } from "react";
import OptimizationProfileCreateOrEditPanel from "./OptimizationProfileCreateOrEditPanel";
import ProfileRequester from "../../requests/ProfileRequester";
import { CalculateUtil, JobUtil } from "../../Utils";
import { LicenseHelper, showToast } from "../../../../../Utilities/CommonUtil";
import { checkPermission } from "../../../../../Utilities/permissionManager";
import JobManagerRequester from "../../requests/JobMangerRequester";
import DataOptimizePanel from "../../Components/DataOptimizePanel";

const defaultQueryParameter = {
    dataType: DiscoveryQueryDataType.Rot,
    withoutDateQueryParameter: {
        from: -1,
        to: 999,
    },
    sizeRangeQueryParameter: {},
    nodeQueryParameter: {
        viewMode: DiscoveryNodeViewMode.Container,
        joinedContainerId: 0,
        containerIds: [],
        siteIds: [],
        pageSize: 5,
    },
    rotRuleQueryParameter: {
        ruleCategories: [
            {
                ruleCategory: 2,
                ruleIds: [],
                checked: false,
            },
            {
                ruleCategory: 3,
                ruleIds: [],
                checked: false,
            },
            {
                ruleCategory: 4,
                ruleIds: [],
                checked: false,
            },
        ],
        ruleIds: [],
    },
    fileExtensionQueryParameter: {},
    needCalculateTotalDataTypes: [
        DiscoveryTotalDataType.SizeAndCount,
        DiscoveryTotalDataType.Sites,
    ],
    nodeChangeNeedRerender: true,
};

const ruleCategoriesColumns = new Map([
    [
        DiscoveryRuleCategory.Redundant,
        [
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Redundant,
                internalName: "rCategoryFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rSaving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryRuleCategory.Obsolete,
        [
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Obsolete,
                internalName: "oCategoryFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "oSaving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryRuleCategory.Trivial,
        [
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_Trivial,
                internalName: "tCategoryFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "tSaving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
]);

const buildInColumns = new Map([
    [
        DiscoveryNodeViewMode.Container,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_Container,
                internalName: "name",
                isLink: true,
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_InScope,
                internalName: "inScope",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rotSaving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryNodeViewMode.Site,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_SiteCollection,
                internalName: "url",
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_InScope,
                internalName: "inScope",
                isPlaceholder: true,
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rotSaving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
    [
        DiscoveryNodeViewMode.SiteInContainer,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_SiteCollection,
                internalName: "url",
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_InScope,
                internalName: "inScope",
                isPlaceholder: true,
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
                internalName: "rotFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_TableColumn_Saving,
                internalName: "rotSaving",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
]);

const buildInProfileInfoes = [
    {
        id: "00000000-0000-0000-0000-000000000000",
        o365TenantId: "00000000-0000-0000-0000-000000000000",
        name: RMResx.RM_FA_Rot_Default_ProfileName,
        status: DiscoveryJobStatus.Waiting,
        modifiedTimeRangeLabel: RMResx.RM_DA_Profile_ProfileModifiedTimeRange,
        sizeRangeLabel: RMResx.RM_DA_ProfleInfoes_ALL,
        fileTypeLabel: RMResx.RM_DA_ProfleInfoes_ALL,
        ruleInfoesLabel: RMResx.RM_DA_ProfleInfoes_ALL,
        customColumns: [],
        avaliableRuleCategories: [],
    },
];

const isShowButton = checkPermission(
    "Archiver_Discovery_Optimization_RunJob",
    RM.UserResources
);

const ROTOptimizationV3 = ({ o365TenantId, jobInfo }) => {
    const optimizePanelRef = useRef(null);

    const dataViewRef = useRef(null);

    const profilePanelRef = useRef(null);

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

    const [profileInfoes, setProfileInfoes] = useState(buildInProfileInfoes);

    const [selectedProfileInfo, setSelectedProfileInfo] = useState(
        buildInProfileInfoes[0]
    );

    useEffect(() => {
        const fetchProfileInfoes = async () => {
            const profileInfoes = await ProfileRequester.getRotProfileInfoes(o365TenantId);
            if (_.isNil(profileInfoes) || profileInfoes.length === 0) {
                return;
            }
            const defaultIndex = profileInfoes.findIndex((item) => item.isDefault);
            setProfileInfoes(profileInfoes);
            setSelectedProfileInfo(profileInfoes[defaultIndex]);
            const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
            clonedQueryParameter.nodeQueryParameter.sortBy = profileInfoes[defaultIndex].sortBy;
            clonedQueryParameter.nodeQueryParameter.isDesc = true;
            clonedQueryParameter.o365TenantId = o365TenantId;
            clonedQueryParameter.profileId = profileInfoes[defaultIndex].id;
            setQueryParameter(clonedQueryParameter);
        };

        fetchProfileInfoes();
    }, [o365TenantId]);

    useEffect(() => {
        const reRenderColumns = async () => {
            await dataViewRef.current.reRenderColumns();

            const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
            clonedQueryParameter.o365TenantId = o365TenantId;
            clonedQueryParameter.profileId = selectedProfileInfo.id;
            clonedQueryParameter.nodeQueryParameter.sortBy = selectedProfileInfo.sortBy;
            clonedQueryParameter.nodeQueryParameter.isDesc = true;
            setQueryParameter(clonedQueryParameter);
        };

        reRenderColumns();
    }, [selectedProfileInfo]);

    const getProfileOptions = (selectedProfileId) => {
        return profileInfoes.map((item) => ({
            name: item.name,
            value: item.id,
            isLoading:
                item.status === DiscoveryJobStatus.Waiting ||
                item.status === DiscoveryJobStatus.Running,
            checked: item.id === selectedProfileId,
            isDefault: item.isDefault,
        }));
    };

    const onWillSelectProfileChange = (args) => {
        if (args.newValue.isLoading) {
            $$.messagedialog(true, {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content:
                        RMResx.RM_DA_Profile_CalculatProfile,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => true,
                    },
                ],
            });
            return false;
        }
    };

    const onSelectedProfileChange = (args) => {
        const id = args.newValue.value;
        const index = profileInfoes.findIndex((item) => item.id == id);
        setSelectedProfileInfo(profileInfoes[index]);

        const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
        clonedQueryParameter.o365TenantId = o365TenantId;
        clonedQueryParameter.profileId = profileInfoes[index].id;
        clonedQueryParameter.nodeQueryParameter.sortBy =
            profileInfoes[index].sortBy;
        clonedQueryParameter.nodeQueryParameter.isDesc = true;

        setQueryParameter(clonedQueryParameter);
    };

    const getTableColumns = async () => {
        let needAppendColumns = [];
        const ruleCategoriesColumnsCloneInfo = _.cloneDeep(ruleCategoriesColumns);
        const avaliableRuleCategories =
            selectedProfileInfo.avaliableRuleCategories;
        for (let avaliableRuleCategory of avaliableRuleCategories) {
            needAppendColumns = needAppendColumns.concat(
                ruleCategoriesColumnsCloneInfo.get(avaliableRuleCategory)
            );
        }

        const ruleColumns = selectedProfileInfo.customColumns;
        ruleColumns.forEach((a) => {
            a.displayName += " (GB)";
        });
        needAppendColumns = needAppendColumns.concat(ruleColumns);

        const clonedBuildInColumns = _.cloneDeep(buildInColumns);
        if (!_.isNil(ruleColumns)) {
            clonedBuildInColumns.forEach((i) => {
                i.forEach((c) => {
                    if (
                        !_.isNil(selectedProfileInfo.sortBy) &&
                        c.internalName.toLowerCase() ===
                            selectedProfileInfo.sortBy.toLowerCase()
                    ) {
                        c.sortable = true;
                        c.sortField = selectedProfileInfo.sortBy;
                    }
                });
                needAppendColumns.forEach((j) => {
                    j.width = 200;
                    j.isAggregateField = true;
                    if (
                        !_.isNil(selectedProfileInfo.sortBy) &&
                        j.internalName.toLowerCase() ===
                            selectedProfileInfo.sortBy.toLowerCase()
                    ) {
                        j.sortable = true;
                        j.sortField = selectedProfileInfo.sortBy;
                    }
                    i.push(j);
                });
            });
        }

        return clonedBuildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await ProfileRotDataRequester.queryOptimizationNodesData(
            queryParameter
        );
        res.items = await CalculateUtil.CalculateRotOptimizationNodesDataV3(
            res.items,
            selectedProfileInfo.customColumns
        );
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res =
            await ProfileRotDataRequester.queryOptimizationNodeTotalAggregateInfo(
                queryParameter
            );
        return await CalculateUtil.CalculateRotOptimizationNodeTotalAggregateInfoV3(
            res,
            selectedProfileInfo.customColumns
        );
    };

    const onProfileCreate = () => {
        profilePanelRef.current.onAdd(null, profileInfoes);
    };

    const onProfileEdit = () => {
        profilePanelRef.current.onEdit(selectedProfileInfo, profileInfoes);
    };

    const onProfileDelete = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_DA_Profile_WillDeleteProfile,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: async () => {
                        $$.messagedialog(false);
                        const res = await ProfileRequester.deleteRotProfileInfo(
                            selectedProfileInfo
                        );
                        if (res.MessageType == 1) {
                            showToast.error(res.ErrorMessage);
                            return false;
                        } else {
                            showToast.success(RMResx.RM_DA_Profile_DeleteProfile);
                            await reRenderProfiles(DiscoveryActionType.Delete);
                            return true;
                        }
                    },
                },
            ],
        });
    };

    const reRenderProfiles = async (actionType) => {
        var profileInfoes = await ProfileRequester.getRotProfileInfoes(
            o365TenantId
        );
        if (actionType === DiscoveryActionType.Create) {
            setProfileInfoes(profileInfoes);
        } else {
            const defaultIndex = profileInfoes.findIndex(
                (item) => item.isDefault
            );
            setProfileInfoes(profileInfoes);
            setSelectedProfileInfo(profileInfoes[defaultIndex]);
        }
    };

    const onDataOptimizeClick = async () => {
        const jobStatusInfo = await JobManagerRequester.getLatest();
        const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
        clonedQueryParameter.nodeQueryParameter =
            queryParameter.nodeQueryParameter;
        clonedQueryParameter.o365TenantId = o365TenantId;
        if (
            !_.isNil(selectedProfileInfo.fileExtensionIds) &&
            selectedProfileInfo.fileExtensionIds.length > 0
        ) {
            clonedQueryParameter.fileExtensionQueryParameter.fileExtensions =
                selectedProfileInfo.fileExtensionIds;
        }

        if (selectedProfileInfo.sizeRange > -1) {
            clonedQueryParameter.sizeRangeQueryParameter.sizeRange =
                selectedProfileInfo.sizeRange;
            clonedQueryParameter.sizeRangeQueryParameter.queryMode =
                selectedProfileInfo.queryMode;
        }

        if (selectedProfileInfo.ruleIds.length > 0) {
            const selectedRuleInfoes = (
                await BasicDataRequester.getRotRuleInfoes()
            ).filter((item) =>
                selectedProfileInfo.ruleIds.some((i) => i === item.id)
            );
            if (
                selectedProfileInfo.avaliableRuleCategories.some(
                    (item) => item === DiscoveryRuleCategory.Redundant
                )
            ) {
                const rRuleIds = selectedRuleInfoes
                    .filter(
                        (item) =>
                            item.category === DiscoveryRuleCategory.Redundant
                    )
                    .map((item) => item.id);
                clonedQueryParameter.rotRuleQueryParameter.ruleCategories[0].ruleIds =
                    rRuleIds;
                clonedQueryParameter.rotRuleQueryParameter.ruleCategories[0].checked = true;
            }

            if (
                selectedProfileInfo.avaliableRuleCategories.some(
                    (item) => item === DiscoveryRuleCategory.Obsolete
                )
            ) {
                const rRuleIds = selectedRuleInfoes
                    .filter(
                        (item) =>
                            item.category === DiscoveryRuleCategory.Obsolete
                    )
                    .map((item) => item.id);
                clonedQueryParameter.rotRuleQueryParameter.ruleCategories[1].ruleIds =
                    rRuleIds;
                clonedQueryParameter.rotRuleQueryParameter.ruleCategories[1].checked = true;
            }

            if (
                selectedProfileInfo.avaliableRuleCategories.some(
                    (item) => item === DiscoveryRuleCategory.Trivial
                )
            ) {
                const rRuleIds = selectedRuleInfoes
                    .filter(
                        (item) =>
                            item.category === DiscoveryRuleCategory.Trivial
                    )
                    .map((item) => item.id);
                clonedQueryParameter.rotRuleQueryParameter.ruleCategories[2].ruleIds =
                    rRuleIds;
                clonedQueryParameter.rotRuleQueryParameter.ruleCategories[2].checked = true;
            }
        }
        
        if(selectedProfileInfo.isBuildIn) {
            clonedQueryParameter.rotRuleQueryParameter.ruleCategories[0].checked = true;
            clonedQueryParameter.rotRuleQueryParameter.ruleCategories[1].checked = true;
            clonedQueryParameter.rotRuleQueryParameter.ruleCategories[2].checked = true;
        }

        clonedQueryParameter.withoutDateQueryParameter.from =
            selectedProfileInfo.greaterThanEqualWithoutInDate;
        clonedQueryParameter.withoutDateQueryParameter.to =
            selectedProfileInfo.lessThanEqualWithoutInDate;

        optimizePanelRef.current.onShow(
            clonedQueryParameter,
            o365TenantId,
            jobStatusInfo
        );
    };

    return (
        <div className="reco-rot-optimization-container">
            {LicenseHelper.EnableRecordsArchiver() &&
                isShowButton &&
                (queryParameter.nodeQueryParameter.containerIds.length > 0 ||
                    queryParameter.nodeQueryParameter.siteIds.length > 0) && (
                    <div>
                        <R.Button
                            id="raDataOptimizeBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_FA_DataOptimize_OptimizePanelBtn}
                            onClick={onDataOptimizeClick}
                        />
                    </div>
                )}
            <section className="reco-data">
                <div className="reco-profile-action-bar">
                    <div className="reco-profile-selector">
                        <R.Combobox
                            id="raInacitveProfileSelector"
                            items={getProfileOptions(selectedProfileInfo.id)}
                            disabled={
                                JobUtil.isRunning(jobInfo) || !jobInfo.enableRot
                            }
                            textField="name"
                            valueField="value"
                            customTrigger={true}
                            onChange={onSelectedProfileChange}
                            willChange={onWillSelectProfileChange}
                            searchable={false}
                            template={(item) => {
                                return (
                                    <div className="reco-profile-custom-combobox-item">
                                        <div>
                                            <div
                                                className={
                                                    item.checked
                                                        ? "fia-check"
                                                        : "fia-check reco-profile-custom-combobox-checked-hidden"
                                                }
                                            ></div>
                                            <div className="reco-profile-custom-combobox-item-name">
                                                {item.name}
                                            </div>
                                        </div>
                                        {item.isLoading ? (
                                            <div className="fia-in-progress reco-profile-custom-combobox-item-loading"></div>
                                        ) : item.isDefault ? (
                                            <div className="reco-profile-custom-bombobox-item-default">
                                                {RMResx.RM_FA_ROTRule_Optimization_Default}
                                            </div>
                                        ) : (
                                            <div></div>
                                        )}
                                    </div>
                                );
                            }}
                        >
                            <R.Button
                                id="raInacitveProfileSelectorBtn"
                                icon="fia-triangle-down"
                                className="hs-manage-column-btn reverse"
                                text={selectedProfileInfo.name}
                                tooltip={selectedProfileInfo.name}
                            />
                        </R.Combobox>
                        <span className="fia-select-all"></span>
                    </div>
                    <div className="reco-profile-action">
                        {!JobUtil.isRunning(jobInfo) && jobInfo.enableRot && (
                            <R.Button
                                id="raNewProfileBtn"
                                text={RMResx.RM_DA_Profile_CreateProfile}
                                primary={false}
                                classify="default"
                                icon="fia-plus"
                                onClick={onProfileCreate}
                            />
                        )}
                        {!JobUtil.isRunning(jobInfo) &&
                            jobInfo.enableRot &&
                            !selectedProfileInfo.isBuildIn && (
                                <R.ButtonGroup
                                    id="raRcManagementMoreBtn"
                                    type="action"
                                    classify="default"
                                    tooltip={RMResx.RM_PRM_PRE_More}
                                >
                                    <R.Button
                                        id="raProfileEditBtn"
                                        text={RMResx.RM_JS_Common_Edit}
                                        icon="fia-edit"
                                        onClick={onProfileEdit}
                                    />
                                    <R.Button
                                        id="raProfileDeleteBtn"
                                        text={RMResx.RM_JS_Common_Delete}
                                        icon="fia-delete"
                                        onClick={onProfileDelete}
                                    />
                                </R.ButtonGroup>
                            )}
                    </div>
                </div>
                <div className="reco-profile-view-bar">
                    <KeyValuePairLabel
                        maxWidth={360}
                        height={24}
                        keyText={RMResx.RM_DA_Profile_ProfileModifiedTimeRange}
                        valueText={selectedProfileInfo.modifiedTimeRangeLabel}
                    />
                    <KeyValuePairLabel
                        maxWidth={360}
                        height={24}
                        keyText={RMResx.RM_FA_ROTRule_Optimization_ROTrule}
                        valueText={selectedProfileInfo.ruleInfoesLabel}
                    />
                    <KeyValuePairLabel
                        maxWidth={360}
                        height={24}
                        keyText={RMResx.RM_DA_Profile_ProfileFileType}
                        valueText={selectedProfileInfo.fileTypeLabel}
                    /> 
                </div>
            </section>
            <section className="reco-data">
                <DiscoveryDataView
                    title={RMResx.RM_FA_ROT_OptimizationTab_YearlySavingTitle}
                    getColumns={getTableColumns}
                    queryNodeDataInfo={queryNodeDataInfo}
                    queryParameter={queryParameter}
                    onChange={setQueryParameter}
                    queryNodeTotalAggregateInfo={queryNodeTotalAggregateInfo}
                    ref={dataViewRef}
                    hasSearchbox
                />
            </section>
            <OptimizationProfileCreateOrEditPanel
                o365TenantId={o365TenantId}
                reRenderProfilesFunc={reRenderProfiles}
                ref={profilePanelRef}
            />
            <DataOptimizePanel ref={optimizePanelRef} viewMode={queryParameter.nodeQueryParameter.viewMode} />
        </div>
    );
};

export default ROTOptimizationV3;
