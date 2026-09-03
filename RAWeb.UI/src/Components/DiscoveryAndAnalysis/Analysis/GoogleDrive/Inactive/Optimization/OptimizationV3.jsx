import { useEffect, useState, useRef } from "react";
import "./index.less";
import TotalMutableData from "../Components/TotalMutableData";
import PieChartWithProgress from "../../../Components/PieChartWithProgress";
import { KeyValuePairLabel } from "../../../Components/Label";
import GoogleDiscoveryDataView from "../../../Components/DiscoveryDataView/GoogleDrive";
import { DiscoveryActionType, DiscoveryJobStatus, DiscoveryNodeViewMode } from "../../../Constants";
import { CalculateUtil, JobUtil, NumberUtil } from "../../../Utils";
import { showToast } from "../../../../../../Utilities/CommonUtil";
import OptimizationProfilePanel from "./OptimizationProfilePanel";
import { GoogleDriveBasicDataRequester, GoogleDriveProfileInactiveDataRequester, GoogleDriveProfileRequester } from "../../../requests/GoogleDrive";

const defaultQueryParameter = {
    withoutDateQueryParameter: {from: -1, to: 999},
    sizeRangeQueryParameter: {},
    nodeQueryParameter: {
        viewMode: DiscoveryNodeViewMode.Container,
        joinedContainerId: 0,
        containerIds: [],
        siteIds: [],
        pageSize: 5,
    },
    fileExtensionQueryParameter: {},
};

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
                displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
                internalName: "inactiveFileSumCount",
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
                displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
                internalName: "inactiveFileSumCount",
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
                displayName: RMResx.RM_FA_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_OptimizableTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_Inactive_TableColumn_OptimizableFileCount,
                internalName: "inactiveFileSumCount",
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
        name: RMResx.RM_FA_Inactive_Default_ProfileName,
        status: DiscoveryJobStatus.Waiting,
        modifiedTimeRangeLabel: RMResx.RM_DA_Profile_ProfileModifiedTimeRange,
        sizeRangeLabel: RMResx.RM_DA_ProfleInfoes_ALL,
        fileTypeLabel: RMResx.RM_DA_ProfleInfoes_ALL,
    },
];

const InactiveOptimizationV3 = ({ organizationId, jobInfo }) => {
    const dataViewRef = useRef(null);
    const profilePanelRef = useRef(null);

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);
    const [profileInfoList, setProfileInfoList] = useState(buildInProfileInfo);
    const [selectedProfileInfo, setSelectedProfileInfo] = useState(buildInProfileInfo[0]);
    const [totalDataInfo, setTotalDataInfo] = useState({
        fileTotalSize: 0,
        optimizableFileTotalSize: 0,
        optimizableFileSumCount: 0,
    });


    useEffect(() => {
        (async () => {
            const profileInfoList = await GoogleDriveProfileRequester.getInactiveProfileInfoList(organizationId);
            if(_.isNil(profileInfoList) || profileInfoList.length === 0) {
                return;
            }
            const defaultIndex = profileInfoList.findIndex((item) => item.isDefault);
            setProfileInfoList(profileInfoList);
            setSelectedProfileInfo(profileInfoList[defaultIndex]);
            const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
            clonedQueryParameter.nodeQueryParameter.sortBy =
                profileInfoList[defaultIndex].sortBy;
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

    useEffect(() => {
        (async () => {
            var res = await GoogleDriveProfileInactiveDataRequester.queryAggregateInfo(
                queryParameter
            );
            setTotalDataInfo(res);
        })();
    }, [JSON.stringify(queryParameter)]);

    const getProfileOptions = (selectedProfileId) => {
        return profileInfoList.map((item) => ({
            name: item.name,
            value: item.id,
            isLoading: item.status === DiscoveryJobStatus.Waiting || item.status === DiscoveryJobStatus.Running,
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
        const ruleColumns = await GoogleDriveBasicDataRequester.getInactiveTableColumns();
        ruleColumns.forEach((ruleColumn) => { ruleColumn.displayName += " (GB)" });
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
                ruleColumns.forEach((ruleColumn) => {
                    ruleColumn.width = 200;
                    ruleColumn.isAggregateField = true;
                    buildInColumn.push(ruleColumn);
                });
            });
        }

        return clonedBuildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await GoogleDriveProfileInactiveDataRequester.queryOptimizationNodesData(queryParameter);
        res.items = await CalculateUtil.CalculateGoogleInactivesNodesData(res.items);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res = await GoogleDriveProfileInactiveDataRequester.queryOptimizationNodeTotalAggregateInfo(queryParameter);
        return await CalculateUtil.CalculateGoogleInactivesNodeTotalAggregateInfo(res);
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
                        const res =
                            await GoogleDriveProfileRequester.deleteInactiveProfileInfo(
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
        var profileInfoList = await GoogleDriveProfileRequester.getInactiveProfileInfoList(organizationId);
        if (actionType === DiscoveryActionType.Create) {
            setProfileInfoList(profileInfoList);
        } else {
            const defaultIndex = profileInfoList.findIndex((item) => item.isDefault);
            setProfileInfoList(profileInfoList);
            setSelectedProfileInfo(profileInfoList[defaultIndex]);
        }
    };

    return (
        <div className="reco-inactive-optimization-container">
            <section className="reco-data">
                <div className="reco-profile-action-bar">
                    <div className="reco-profile-selector">
                        <R.Combobox
                            id="raInacitveProfileSelector"
                            items={getProfileOptions(selectedProfileInfo.id)}
                            disabled={JobUtil.isRunning(jobInfo)}
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
                                            <div className={ item.checked ? "fia-check" : "fia-check reco-profile-custom-combobox-checked-hidden"}></div>
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
                        {!JobUtil.isRunning(jobInfo) && (
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
                        keyText={RMResx.RM_DA_Profile_ProfileFileSize}
                        valueText={selectedProfileInfo.sizeRangeLabel}
                    />
                    <KeyValuePairLabel
                        maxWidth={360}
                        height={24}
                        keyText={RMResx.RM_DA_Profile_ProfileFileType}
                        valueText={selectedProfileInfo.fileTypeLabel}
                    />
                </div>
            </section>
            <section className="reco-data reco-profile-total-data">
                <div className="reco-mutable-data">
                    <TotalMutableData
                        data={[
                            {
                                text: RMResx.RM_DA_Optimization_OptimizationV3_OptimizableFileTotalSize,
                                value: totalDataInfo.optimizableFileTotalSize,
                            },
                            {
                                text: RMResx.RM_DA_Optimization_OptimizationV3_OptimizableFileSumCount,
                                value: totalDataInfo.optimizableFileSumCount,
                            },
                        ]}
                    />
                </div>
                <div className="reco-percentage-chart">
                    <PieChartWithProgress
                        total={NumberUtil.internaltionalCounting(totalDataInfo.fileTotalSize)}
                        active={
                            totalDataInfo.fileTotalSize === 0
                                ? totalDataInfo.fileTotalSize
                                : Number.parseInt(
                                      (
                                          totalDataInfo.optimizableFileTotalSize /
                                          totalDataInfo.fileTotalSize /
                                          1.0
                                      )
                                          .toFixed(2)
                                          .replace(".", "")
                                  ) === 0 && totalDataInfo.optimizableFileTotalSize !== 0
                                ? 1
                                : Number.parseInt(
                                      (
                                          totalDataInfo.optimizableFileTotalSize /
                                          totalDataInfo.fileTotalSize /
                                          1.0
                                      )
                                          .toFixed(2)
                                          .replace(".", "")
                                  )
                        }
                        name={RMResx.RM_FA_Inactive_SummaryTab_PieChartTitle}
                        unit={RMResx.RM_JS_RDM_CreateRule_Unit_GB}
                    />
                </div>
            </section>
            <section className="reco-data">
                <GoogleDiscoveryDataView
                    title={
                        RMResx.RM_FA_Inactive_OptimizationTab_InactiveOptimizationTitle
                    }
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
    );
};

export default InactiveOptimizationV3;
