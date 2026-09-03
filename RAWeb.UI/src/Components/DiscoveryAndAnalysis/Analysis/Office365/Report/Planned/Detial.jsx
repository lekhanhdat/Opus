import "./index.less";
import { forwardRef } from "react";
import {
    ArchiveDataType,
    MS365DataType,
    ArchiveOrRemoveFileType,
    ArchiveOrRemoveVersionType,
    TierTypes,
} from "../../../Constants/DataOptimizeType";
import ProgressRequester from "../../../requests/ProgressRequester";
import { useState } from "react";
import { useImperativeHandle } from "react";
import { StorageTypeIndex } from "../../../../../../Constants/Constants";

const DetailItem = ({ name, value }) => {
    return (
        <div className="reco-optimization-detail-item">
            <div className="name" tabIndex="0">
                {name}
            </div>
            <div
                className="value"
                tabIndex="0"
                data-tooltip="ifneed"
                aria-label={value}
            >
                {value}
            </div>
        </div>
    );
};

const Detail = ({}, ref) => {

    const defaultDeviceId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

    const [showPanel, setShowPanel] = useState(false);

    const [settingDetail, setSettingDetail] = useState(null);

    useImperativeHandle(ref, () => ({
        onShow: async (o365TenantId, settingId) => {
            setShowPanel(true);
            const res = await ProgressRequester.getOptimizationSettingDetail(
                o365TenantId,
                settingId
            );
            setSettingDetail(res);
        },
    }));

    const onHide = () => {
        setSettingDetail(null);
        setShowPanel(false);
    };

    const getStoreDataValue = () => {
        switch (settingDetail.moveToAnotherTierType) {
            case TierTypes.DefaultTier:
                return RMResx.RM_JS_Rule_DetailValue_DefaultTier;
            case TierTypes.ArchiveTier:
                return RMResx.RM_JS_Rule_DetailValue_ArchiveTier;
            case TierTypes.ColdTier:
                return RMResx.RM_JS_Rule_DetailValue_ColdTier;
            default:
                return "";
        }
    };

    return (
        <R.Panel
            id="reco-manual-review-filter-panel"
            header={RMResx.RM_FA_Progress_PlannedTab_Details}
            size={660}
            status={{ show: showPanel }}
            onHide={onHide}
            destroy={false}
        >
            {settingDetail && (
                <div className="reco-optimization-detail">
                    <div className="reco-optimization-detail-section">
                        <div className="detail-section-title" tabIndex="0">
                            {RMResx.RM_FA_DataOptimize_FileScopeExpander}
                        </div>
                        <div className="detail-section-content">
                            <DetailItem
                                name={RMResx.RM_FA_DataOptimize_DSOMS365DataFilterTypeTitle}
                                value={
                                    settingDetail.dataScopeInfo.ms365DataType ===
                                        MS365DataType.Phl
                                        ? RMResx.RM_FA_DataOptimize_PreservationHoldLibraryTitle
                                        : RMResx.RM_FA_DataOptimize_SharepointOrOneDriveTitle
                                }
                            />
                            <div className="reco-optimization-detail-item">
                                <div className="name" tabIndex="0">
                                    {RMResx.RM_FA_DataOptimize_ScopeTitle}
                                </div>
                                <div className="list-value">
                                    {settingDetail.dataScopeInfo.sites.map(
                                        (site) => (
                                            <div
                                                className="list-value-item"
                                                data-tooltip="ifneed"
                                                data-tooltip-wrap="force"
                                                aria-label={site}
                                                tabIndex="0"
                                            >
                                                {site}
                                            </div>
                                        )
                                    )}
                                </div>
                            </div>
                            <DetailItem
                                name={RMResx.RM_FA_Inactive_ModifiedTitle}
                                value={settingDetail.dataScopeInfo.timeRange}
                            />
                            <DetailItem
                                name={RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeTitle}
                                value={settingDetail.dataScopeInfo.sizeRange}
                            />
                            <DetailItem
                                name={RMResx.RM_FA_Inactive_OptimizationTab_FileCategoryTitle}
                                value={settingDetail.dataScopeInfo.fileType}
                            />
                        </div>
                    </div>
                    <div className="reco-optimization-detail-section">
                        <div className="detail-section-title" tabIndex="0">
                            {
                                RMResx.RM_FA_DataOptimize_ObjectRuleExpander
                            }
                        </div>
                        <div className="detail-section-content">
                            <DetailItem
                                name={RMResx.RM_FA_DataOptimize_ArchiveTitle}
                                value={
                                    settingDetail.objectScopeInfo.dataType ===
                                    ArchiveDataType.All
                                        ? RMResx.RM_FA_DataOptimize_Archive_All
                                        : RMResx.RM_FA_DataOptimize_Archive_Special
                                }
                            />
                            <DetailItem
                                name={RMResx.RM_FA_DataOptimize_Archive_InactiveVersionSwitch
                                }
                                value={
                                    settingDetail.objectScopeInfo.dataType ===
                                        ArchiveDataType.Special &&
                                    settingDetail.objectScopeInfo.inactiveEnable
                                        ? settingDetail.objectScopeInfo.inactiveRules.join(
                                              "; "
                                          )
                                        : RMResx.RM_JS_Common_None
                                }
                            />
                            <DetailItem
                                name={RMResx.RM_FA_DataOptimize_Archive_ROTRuleSwitch}
                                value={
                                    settingDetail.objectScopeInfo.dataType ===
                                        ArchiveDataType.Special &&
                                    settingDetail.objectScopeInfo.rotEnable
                                        ? settingDetail.objectScopeInfo.rotRules.join(
                                              "; "
                                          )
                                        : RMResx.RM_JS_Common_None
                                }
                            />
                        </div>
                    </div>
                    <div className="reco-optimization-detail-section">
                        <div className="detail-section-title" tabIndex="0">
                            {RMResx.RM_FA_DataOptimize_ProcessActionExpander}
                        </div>
                        <div className="detail-section-content">
                            {(settingDetail.objectScopeInfo.dataType ===
                                ArchiveDataType.All ||
                                settingDetail.objectScopeInfo.rotEnable) && (
                                <>
                                    <DetailItem
                                        name={
                                            RMResx.RM_FA_DataOptimize_FileTitle
                                        }
                                        value={
                                            settingDetail.actionInfo
                                                .fileAction ===
                                            ArchiveOrRemoveFileType.ArchiveAndRemove
                                                ? RMResx.RM_FA_DataOptimize_File_ArchiveAndRemove
                                                : settingDetail.actionInfo.fileAction === ArchiveOrRemoveFileType.Archive ? RMResx.RM_JS_RDM_CreateRule_Options_Backup : RMResx.RM_FA_DataOptimize_File_RemoveFile
                                        }
                                    />
                                    <DetailItem
                                        name={RMResx.RM_FA_DataOptimize_File_LeaveStub}
                                        value={
                                            settingDetail.actionInfo
                                                .isEnableLeaveStub
                                                ? RMResx.RM_JS_Common_Yes
                                                : RMResx.RM_JS_SPS_UniqueIsShow_OptionNo
                                        }
                                    />
                                    <DetailItem
                                        name={RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile}
                                        value={
                                            settingDetail.actionInfo
                                                .deleteRecords
                                                ? RMResx.RM_JS_Common_Yes
                                                : RMResx.RM_JS_SPS_UniqueIsShow_OptionNo
                                        }
                                    />
                                    <DetailItem
                                        name={RMResx.RM_JS_Rule_ArchiveVersionAndDestroyFile}
                                        value={settingDetail.actionInfo.archiveVersionValue || ""}
                                    />
                                    <DetailItem
                                        name={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                                        value={settingDetail.actionInfo.deleteRecordToRecycleBin ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}
                                    />
                                </>
                            )}
                            <DetailItem
                                name={
                                    RMResx.RM_FA_DataOptimize_VersionTitle
                                }
                                value={
                                    settingDetail.actionInfo.versionAction === ArchiveOrRemoveVersionType.None ? 
                                        RMResx.RM_JS_Common_None : settingDetail.actionInfo.versionAction === ArchiveOrRemoveVersionType.ArchiveAndRemove ? 
                                            RMResx.RM_FA_DataOptimize_Version_ArchiveAndRemove : RMResx.RM_FA_DataOptimize_Version_RemoveVersion
                                }
                            />
                            <DetailItem
                                name={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                                value={settingDetail.actionInfo.deleteVersionToRecycleBin ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}
                            />
                        </div>
                    </div>
                    <div className="reco-optimization-detail-section">
                        <div className="detail-section-title" tabIndex="0">
                            {RMResx.RM_FA_Progress_PlannedTab_OthersTitle}
                        </div>
                        <div className="detail-section-content">
                            <DetailItem
                                name={RMResx.RM_FA_DataOptimize_StorageTitle}
                                value={settingDetail.storageName}
                            />
                            {(settingDetail.storageDeviceUIDto && settingDetail.storageDeviceUIDto.Type === StorageTypeIndex.AzureBlob &&
                                settingDetail.storageDeviceUIDto.Id.toLowerCase() != defaultDeviceId && !settingDetail.storageDeviceUIDto.IsSystemStorage) && (
                                    <>
                                        <DetailItem
                                            name={RMResx.RM_JS_Rule_Detail_StoreData}
                                            value={getStoreDataValue()}
                                        />
                                    </>
                                )}
                            <DetailItem
                                name={RMResx.RM_FA_DataOptimize_ScheduleTitle}
                                value={settingDetail.scheduleTime}
                            />
                        </div>
                    </div>
                </div>
            )}
        </R.Panel>
    );
};

export default forwardRef(Detail);
