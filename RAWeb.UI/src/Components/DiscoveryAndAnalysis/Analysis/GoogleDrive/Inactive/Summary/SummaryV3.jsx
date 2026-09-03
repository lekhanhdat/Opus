import { useEffect, useRef, useState } from "react";
import _ from "lodash";

import { DiscoveryNodeViewMode, DiscoveryQueryDataType, DiscoverySizeRangeQueryMode, DiscoveryTotalDataType } from "../../../Constants";
import { GoogleDriveBasicDataRequester, GoogleDriveInactiveDataRequester, GoogleDriveProfileRequester } from "../../../requests/GoogleDrive";
import { GoogleDriveTotalSummary, GoogleDriveWithoutModifiedDate, GoogleDriveTotalData } from '../Components'
import DiscoveryDataView from "../../../Components/DiscoveryDataView";
import SizeRangeChart from "../../../Components/SizeRangeChart";
import FileTypeChart from "../../../Components/FileTypeChart";

import './index.less';
import { CalculateUtil, JobUtil } from "../../../Utils";
import OptimizationProfilePanel from "../Optimization/OptimizationProfilePanel";

const buildInColumns = new Map([
    [
        DiscoveryNodeViewMode.Container,
        [
            {
                displayName: RMResx.RM_FA_GoogleDrive_TableColumn_Container,
                internalName: "name",
                isLink: false,
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_ROT_SummaryTab_DriveCollections,
                internalName: "driveCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_InactiveTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_InactiveFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_TableColumn_Rate,
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
                displayName: RMResx.RM_FA_GoogleDrive_ROT_SummaryTab_DriveCollections,
                internalName: "url",
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_InactiveTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_InactiveFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_TableColumn_Rate,
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
                displayName: RMResx.RM_FA_GoogleDrive_ROT_SummaryTab_DriveCollections,
                internalName: "url",
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_TableColumn_TotalSize,
                internalName: "fileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_FileCount,
                internalName: "fileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_InactiveTotalSize,
                internalName: "inactiveFileTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:
                    RMResx.RM_FA_GoogleDrive_Inactive_TableColumn_InactiveFileCount,
                internalName: "inactiveFileSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_GoogleDrive_TableColumn_Rate,
                internalName: "rate",
                isAggregateField: true,
                width: 200,
            },
        ],
    ],
]);

const defaultQueryParameter = {
    dataType: DiscoveryQueryDataType.Inactive,
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
    rotRuleQueryParameter: {},
    fileExtensionQueryParameter: {},
    needCalculateTotalDataTypes: [
        DiscoveryTotalDataType.SizeAndCount,
        DiscoveryTotalDataType.Sites,
    ],
    nodeChangeNeedRerender: true,
};

function InactiveSummaryV3({ organizationId, jobInfo }) {
    const profilePanelRef = useRef(null);

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);

    useEffect(() => {
        if (_.isNil(organizationId)) {
            return;
        }
        const clonedQueryParameter = _.cloneDeep(defaultQueryParameter);
        clonedQueryParameter.organizationId = organizationId;
        setQueryParameter(clonedQueryParameter);
    }, [organizationId]);

    const getTableColumns = async () => {
        const ruleColumns = await GoogleDriveBasicDataRequester.getInactiveTableColumns();
        ruleColumns.forEach((a) => {
            a.displayName += " (GB)";
        });
        const clonedBuildInColumns = _.cloneDeep(buildInColumns);
        if (!_.isNil(ruleColumns)) {
            clonedBuildInColumns.forEach((i) =>
                ruleColumns.forEach((j) => {
                    j.width = 200;
                    j.isAggregateField = true;
                    i.push(j);
                })
            );
        }
        return clonedBuildInColumns;
    };

    const queryNodeDataInfo = async (queryParameter) => {
        const res = await GoogleDriveInactiveDataRequester.querySummaryNodesData(queryParameter);
        res.items = await CalculateUtil.CalculateGoogleInactivesNodesData(res.items);
        return res;
    };

    const queryNodeTotalAggregateInfo = async (queryParameter) => {
        const res = await GoogleDriveInactiveDataRequester.querySummaryNodeTotalAggregateInfo(queryParameter);
        return await CalculateUtil.CalculateGoogleInactivesNodeTotalAggregateInfo(
            res
        );
    };

    const onProfileCreate = async () => {
        const profileInfoList = await GoogleDriveProfileRequester.getInactiveProfileInfoList(organizationId);
        const sizeRangeInfoes = await GoogleDriveBasicDataRequester.getSizeRangeList();
        const lessThanIds = sizeRangeInfoes[0].id;
        profilePanelRef.current.onAdd(
            {
                organizationId: organizationId,
                sizeRange: _.isNil(queryParameter.sizeRangeQueryParameter.sizeRange)
                    ? -1
                    : queryParameter.sizeRangeQueryParameter.sizeRange,
                sizeRangeQueryMode: queryParameter.sizeRangeQueryParameter.sizeRange !== lessThanIds
                    ? DiscoverySizeRangeQueryMode.GenerateThanEqual
                    : DiscoverySizeRangeQueryMode.LessThanEqual,
                greaterThanEqualWithoutInDate: queryParameter.withoutDateQueryParameter.from,
                lessThanEqualWithoutInDate: queryParameter.withoutDateQueryParameter.to,
                fileExtensionIds: _.isNil(queryParameter.fileExtensionQueryParameter.fileExtensions)
                    ? []
                    : queryParameter.fileExtensionQueryParameter.fileExtensions,
                sortBy: "FileTotalSize",
            },
            profileInfoList
        );
    };

    return (
        <>
            <div className="reco-inactive-summary-container">
                {/* Summary */}
                <div>
                    <GoogleDriveTotalSummary queryParameter={queryParameter} />
                </div>

                {/* Inactive data */}
                <div className="reco-data">
                    <section className="reco-title">
                        <span tabIndex="0">
                            {RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_InactiveDataTitle}
                        </span>
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-basic-data">
                        <div className="reco-modified-date">
                            <GoogleDriveWithoutModifiedDate
                                title={RMResx.RM_FA_GoogleDrive_Inactive_ModifiedTitle}
                                queryParameter={queryParameter}
                                onChange={setQueryParameter}
                            />
                        </div>
                        <GoogleDriveTotalData queryParameter={queryParameter} />
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-node-data">
                        <DiscoveryDataView
                            getColumns={getTableColumns}
                            queryParameter={queryParameter}
                            onChange={setQueryParameter}
                            queryNodeDataInfo={queryNodeDataInfo}
                            queryNodeTotalAggregateInfo={
                                queryNodeTotalAggregateInfo
                            }
                            disabledViewSwitcher={true}
                            hasSearchbox
                        />
                    </section>
                    <div className="reco-discovery-split-line"></div>
                    <section className="reco-chart-data">
                        <div>
                            <div className="reco-chart-title">
                                {RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_DocumentSizeTitle}
                            </div>
                            <div className="reco-column-chart">
                                <SizeRangeChart
                                    id={"google_drive_inactive_summary_size_range"}
                                    height={300}
                                    queryParameter={queryParameter}
                                    onChange={setQueryParameter}
                                    queryData={GoogleDriveInactiveDataRequester.querySizeRanges}
                                />
                            </div>
                        </div>
                        <div>
                            <div className="reco-chart-title">
                                {RMResx.RM_FA_GoogleDrive_Inactive_SummaryTab_FileTypeTitle}
                            </div>
                            <div className="reco-treemap-chart">
                                <FileTypeChart
                                    id={"google_drive_inactive_summary_file_type"}
                                    height={300}
                                    queryParameter={queryParameter}
                                    onChange={setQueryParameter}
                                    queryData={GoogleDriveInactiveDataRequester.queryFileExtensions}
                                />
                            </div>
                        </div>
                    </section>
                </div>
            </div>
            {!JobUtil.isRunning(jobInfo) && !JobUtil.isFailed(jobInfo) && (
                <div className="reco-discovery-summary-profile-bar">
                    <div className="reco-discovery-split-line"></div>
                    <div className="reco-discovery-summary-profile-btn">
                        <R.Button
                            id="raSaveAsProfileBtn"
                            text={RMResx.RM_DA_Summary_SaveProfile}
                            primary={true}
                            classify="theme"
                            onClick={onProfileCreate}
                        />
                    </div>
                    <OptimizationProfilePanel
                        organizationId={organizationId}
                        ref={profilePanelRef}
                    />
                </div>
            )}
        </>
    );
}

export default InactiveSummaryV3;
