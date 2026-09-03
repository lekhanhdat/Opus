import React, { useEffect, useRef, useState } from 'react';
import { DiscoveryActionType, DiscoveryJobStatus, DiscoveryNodeViewMode, DiscoveryRuleCategory } from '../../../Constants';
import { GoogleDriveProfileRequester, GoogleDriveProfileRotDataRequester } from '../../../requests/GoogleDrive';
import { CalculateUtil, JobUtil } from '../../../Utils';
import { KeyValuePairLabel } from '../../../Components/Label';
import GoogleDiscoveryDataView from '../../../Components/DiscoveryDataView/GoogleDrive';
import OptimizationProfilePanel from './OptimizationProfilePanel';
import { showToast } from '../../../../../../Utilities/CommonUtil';
import "./index.less";

const defaultQueryParameter = {
    withoutDateQueryParameter: {from: -1, to: 999},
    nodeQueryParameter: {
        viewMode: DiscoveryNodeViewMode.Container,
        joinedContainerId: 0,
        containerIds: [],
        siteIds: [],
        pageSize: 5,
    },
    rotRuleQueryParameter: {},
    fileExtensionQueryParameter: {},
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
        ],
    ],
    [
        DiscoveryNodeViewMode.Site,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_DriveCollection,
                internalName: "driveName",
                width: 350,
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
        ],
    ],
    [
        DiscoveryNodeViewMode.SiteInContainer,
        [
            {
                displayName: RMResx.RM_FA_TableColumn_DriveCollection,
                internalName: "driveName",
                width: 350,
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
        ],
    ],
]);

const buildInProfileInfo = [
    {
        id: "00000000-0000-0000-0000-000000000000",
        organizationId: "00000000-0000-0000-0000-000000000000",
        name: RMResx.RM_FA_Rot_Default_ProfileName,
        status: DiscoveryJobStatus.Waiting,
        modifiedTimeRangeLabel: RMResx.RM_DA_Profile_ProfileModifiedTimeRange,
        sizeRangeLabel: RMResx.RM_DA_ProfleInfoes_ALL,
        fileTypeLabel: RMResx.RM_DA_ProfleInfoes_ALL,
        ruleInfoLabel: RMResx.RM_DA_ProfleInfoes_ALL,
        customColumns: [],
        availableRuleCategories: [],
    },
];

const ROTOptimizationV3 = ({ organizationId, jobInfo }) => {
    const dataViewRef = useRef(null);
    const profilePanelRef = useRef(null);

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);
    const [profileInfoList, setProfileInfoList] = useState(buildInProfileInfo);
    const [selectedProfileInfo, setSelectedProfileInfo] = useState(buildInProfileInfo[0]);

    useEffect(() => {
        (async () => {
            const profileInfoList = await GoogleDriveProfileRequester.getRotProfileInfoList(organizationId);
    
            if(_.isNil(profileInfoList) || profileInfoList.length === 0) {
                return;
            }
    
            const defaultIndex = profileInfoList.findIndex((item) => item.isDefault);
            setProfileInfoList(profileInfoList);
            setSelectedProfileInfo(profileInfoList[defaultIndex]);
    
            const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
            clonedQueryParameter.nodeQueryParameter.sortBy = profileInfoList[defaultIndex].sortBy;
            clonedQueryParameter.nodeQueryParameter.isDesc = true;
            clonedQueryParameter.organizationId = organizationId;
            clonedQueryParameter.profileId = profileInfoList[defaultIndex].id;
    
            setQueryParameter(clonedQueryParameter);
        })();
    }, [organizationId]);

    useEffect(() => {
        (async () => {
            await dataViewRef.current.reRenderColumns();
    
            const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
            clonedQueryParameter.organizationId = organizationId;
            clonedQueryParameter.profileId = selectedProfileInfo.id;
            clonedQueryParameter.nodeQueryParameter.sortBy = selectedProfileInfo.sortBy;
            clonedQueryParameter.nodeQueryParameter.isDesc = true;
    
            setQueryParameter(clonedQueryParameter);
        })();
    }, [selectedProfileInfo.id, selectedProfileInfo.sortBy]);

    const getProfileOptions = (selectedProfileId) => {
        return profileInfoList.map((item) => ({
            name: item.name,
            value: item.id,
            isLoading: [DiscoveryJobStatus.Waiting, DiscoveryJobStatus.Running].includes(item.status),
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
                content: RMResx.RM_DA_Profile_CalculatProfile,
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
        const index = profileInfoList.findIndex((item) => item.id == id);
        setSelectedProfileInfo(profileInfoList[index]);

        const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
        clonedQueryParameter.organizationId = organizationId;
        clonedQueryParameter.profileId = profileInfoList[index].id;
        clonedQueryParameter.nodeQueryParameter.sortBy = profileInfoList[index].sortBy;
        clonedQueryParameter.nodeQueryParameter.isDesc = true;

        setQueryParameter(clonedQueryParameter);
    };

    const getTableColumns = async () => {
        let needAppendColumns = [];
        const ruleCategoriesColumnsCloneInfo = _.cloneDeep(ruleCategoriesColumns);
        const availableRuleCategories = selectedProfileInfo.availableRuleCategories;
        for (let availableRuleCategory of availableRuleCategories) {
            needAppendColumns = needAppendColumns.concat(
                ruleCategoriesColumnsCloneInfo.get(availableRuleCategory)
            );
        }

        const ruleColumns = selectedProfileInfo.customColumns;
        ruleColumns.forEach((a) => { a.displayName += " (GB)" });
        needAppendColumns = needAppendColumns.concat(ruleColumns);

        const clonedBuildInColumns = _.cloneDeep(buildInColumns);
        if (!_.isNil(ruleColumns)) {
            clonedBuildInColumns.forEach((buildInColumn) => {
                buildInColumn.forEach((column) => {
                    if (
                        !_.isNil(selectedProfileInfo.sortBy) &&
                        column.internalName.toLowerCase() === selectedProfileInfo.sortBy.toLowerCase()
                    ) {
                        column.sortable = true;
                        column.sortField = selectedProfileInfo.sortBy;
                    }
                });
                needAppendColumns.forEach((needAppendColumn) => {
                    needAppendColumn.width = 200;
                    needAppendColumn.isAggregateField = true;
                    if (
                        !_.isNil(selectedProfileInfo.sortBy) &&
                        needAppendColumn.internalName.toLowerCase() === selectedProfileInfo.sortBy.toLowerCase()
                    ) {
                        needAppendColumn.sortable = true;
                        needAppendColumn.sortField = selectedProfileInfo.sortBy;
                    }
                    buildInColumn.push(needAppendColumn);
                });
            });
        }

        return clonedBuildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await GoogleDriveProfileRotDataRequester.queryOptimizationNodesData(queryParameter);
        res.items = await CalculateUtil.CalculateGoogleRotOptimizationNodesDataV3(
            res.items,
            selectedProfileInfo.customColumns
        );
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res = await GoogleDriveProfileRotDataRequester.queryOptimizationNodeTotalAggregateInfo(queryParameter);
        return await CalculateUtil.CalculateGoogleRotOptimizationNodeTotalAggregateInfoV3(
            res,
            selectedProfileInfo.customColumns
        );
    };

    const onProfileCreate = () => {
        profilePanelRef.current.onAdd(null, profileInfoList);
    };

    const onProfileEdit = () => {
        profilePanelRef.current.onEdit(selectedProfileInfo, profileInfoList);
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
                    onClick: () => { $$.messagedialog(false) },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: async () => {
                        $$.messagedialog(false);
                        const res = await GoogleDriveProfileRequester.deleteRotProfileInfo(selectedProfileInfo);
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
        var profileInfoList = await GoogleDriveProfileRequester.getRotProfileInfoList(organizationId);
        if (actionType === DiscoveryActionType.Create) {
            setProfileInfoList(profileInfoList);
        } else {
            const defaultIndex = profileInfoList.findIndex((item) => item.isDefault);
            setProfileInfoList(profileInfoList);
            setSelectedProfileInfo(profileInfoList[defaultIndex]);
        }
    };

    return (
        <div className="reco-rot-optimization-container">
            <section className="reco-data">
                <div className="reco-profile-action-bar">
                    <div className="reco-profile-selector">
                        <R.Combobox
                            id="raInacitveProfileSelector"
                            items={getProfileOptions(selectedProfileInfo.id)}
                            disabled={JobUtil.isRunning(jobInfo) || !jobInfo.enableRot}
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
                                            <div className={item.checked ? "fia-check" : "fia-check reco-profile-custom-combobox-checked-hidden"}></div>
                                            <div className="reco-profile-custom-combobox-item-name">
                                                {item.name}
                                            </div>
                                        </div>
                                        {item.isLoading ? (
                                            <div className="fia-in-progress reco-profile-custom-combobox-item-loading"></div>
                                        ) : item.isDefault ? (
                                            <div className="reco-profile-custom-combobox-item-default">
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
                                id="raGoogleProfileSelectorBtn"
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
                        valueText={selectedProfileInfo.ruleInfoLabel}
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
                <GoogleDiscoveryDataView
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
            <OptimizationProfilePanel
                organizationId={organizationId}
                reRenderProfilesFunc={reRenderProfiles}
                ref={profilePanelRef}
            />
        </div>
    )
}

export default ROTOptimizationV3