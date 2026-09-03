import React from "react";
import DetailRow from "../../Common/DetailRow";
import { useState, useEffect } from "react";
import { ExportSPDataOption, GoogleLevelIds, phyLevelIds, RuleLevelIds, RuleModuleTypes, TierTypes } from "../../../RuleItem/Components/Constants";
import { LicenseHelper, formatBoolean } from "../../../../../Utilities/CommonUtil";
import { StorageTypeIndex } from "../../../../../Constants/Constants";
const ArchiverStorage = ({ ruleItem }) => {

    const defaultDeviceId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

    const [isShowStorageInfo, setIsShowStorageInfo] = useState(false);

    const [isShowRetentionInfo, setIsShowRetentionInfo] = useState(false);

    const [hiddenArchivedTier, setHiddenArchivedTier] = useState(false);

    useEffect(() => {
        let isPhy = ruleItem.RuleLevel === phyLevelIds.PhysicalBox || ruleItem.RuleLevel === phyLevelIds.PhysicalFile;
        let isFS = ruleItem.RuleLevel === RuleLevelIds.FS;
        let isGoogle = ruleItem.RuleLevel === GoogleLevelIds.GG;

        if (ruleItem.ModelType != RuleModuleTypes.SOArchiver) {
            let { RelatedRecordOption, RuleKeepDataOption } = ruleItem;
            let isExportOnly = ruleItem.ExportInfo && ruleItem.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive;
            let isDeleteRelatedRecordOption = RelatedRecordOption == 1;
            let isArchiveToAzureBlobStorage = RuleKeepDataOption == 1024 || RuleKeepDataOption == 2048;
            let isNotBackup = true;

            if (!isPhy && !isFS && !isGoogle) {
                if ((RuleKeepDataOption === 0 || RuleKeepDataOption === 128) && ruleItem.MoveDto == null && !isExportOnly) {
                    isNotBackup = (RuleKeepDataOption & 256) == 256;
                }
            }
            setIsShowStorageInfo(isArchiveToAzureBlobStorage || isDeleteRelatedRecordOption || !isNotBackup);
            setIsShowRetentionInfo(isArchiveToAzureBlobStorage);
        } else {
            let keepDataOption = ruleItem.RuleKeepDataOption;
            let isBackupAndRemove = (keepDataOption & 4096) == 4096 || (keepDataOption & 8192) == 8192;
            setIsShowStorageInfo(!!ruleItem.StoragePolicyName && isBackupAndRemove);
        }

        if (isPhy || ruleItem.StoragePolicyType != StorageTypeIndex.AzureBlob || ruleItem.StoragePolicyId.toLowerCase() === defaultDeviceId || ruleItem.IsSystemStorage) {
            setHiddenArchivedTier(true);
        } else {
            setHiddenArchivedTier(false);
        }

    }, [ruleItem]);

    const getStoreDataValue = (tierType) => {
        switch (tierType) {
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

    const storageInfoRows = [
        {
            label: RMResx.RM_JS_Rule_Detail_ArchiverStorage,
            value: ruleItem.StoragePolicyName,
            show: isShowStorageInfo
        },
        {
            label: RMResx.RM_JS_Rule_Detail_StoreData,
            value: getStoreDataValue(ruleItem.MoveToAnotherTierType),
            show: LicenseHelper.EnableRecordsArchiver() && isShowStorageInfo && !hiddenArchivedTier
        },
        {
            label: RMResx.RM_JS_Rule_Detail_Retention,
            value: formatBoolean(ruleItem.ModelType != RuleModuleTypes.SOArchiver ? ruleItem.IsEnableRetention : ruleItem.RetentionInfoList?.some(p => p.IsEnableRetention)),
            show: (LicenseHelper.EnableRecordsArchiver() ? isShowStorageInfo : isShowRetentionInfo) && ruleItem.RuleLevel != RuleLevelIds.FS
        },
    ];

    return <React.Fragment>
        {
            storageInfoRows.map((item, index) => {
                return (item.show && <DetailRow key={index} label={item.label}>
                    {item.value}
                </DetailRow>);
            })
        }
    </React.Fragment>;
};

export default ArchiverStorage;