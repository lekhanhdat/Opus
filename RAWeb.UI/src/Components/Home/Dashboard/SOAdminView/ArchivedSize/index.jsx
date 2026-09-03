import "./index.less";
import "../../SOAdminView/index.less";
import _ from "lodash";
import React, { useEffect, useRef, useState } from "react";
import { ArchivedDataSizeRequestOption, ArchivedFileCountRequestOption, ArchivedVersionCountRequestOption, ArchiverDataUnit, ArchiverDataUnitName, GetConfigurationDataRequestOption, YearlySavingRequestOption } from "../config";
import { EnvironmentHelper, LicenseHelper, showToast } from "../../../../../Utilities/CommonUtil";
import EmptyContent from "../../Components/EmptyContent";

const DefaultConfigurationPriceInfo = {
    SharePointStoragePrice: 0.20,
    ArchivedStoragePrice: 0.00,
};

const DefaultTotalInfo = {
    TotalSize: "",
    ArchiverDataUnit: ArchiverDataUnit.Unknown
};

const isNewOpusAccount = LicenseHelper.EnableRecordsArchiver();
const is21VEnv = LicenseHelper.Is21VEnv();
const isGccEnv = EnvironmentHelper.IsGovAzureEnv;

const isSupportedStoreInM365 = isNewOpusAccount && !is21VEnv && !isGccEnv;

const ArchivedSize = () => {

    const allValidationRef = useRef();

    const [noData, setNoData] = useState(true);

    const [totalArchived, setTotalArchived] = useState(_.cloneDeep(DefaultTotalInfo));

    const [fileNumber, setFileNumber] = useState(_.cloneDeep(DefaultTotalInfo));

    const [versionNumber, setVersionNumber] = useState(_.cloneDeep(DefaultTotalInfo));

    const [yearlyNumber, setYearlyNumber] = useState(_.cloneDeep(DefaultTotalInfo));

    const [isShowDialog, setIsShowDialog] = useState(false);

    const [configurationPriceInfo, setConfigurationPriceInfo] = useState(_.cloneDeep(DefaultConfigurationPriceInfo));

    useEffect(() => {
        loadAllSize();
    }, []);

    const loadAllSize = async () => {
        const dataSize = fetchUtility(ArchivedDataSizeRequestOption);
        const fileCount = fetchUtility(ArchivedFileCountRequestOption);
        const versionCount = fetchUtility(ArchivedVersionCountRequestOption);
        const yearlySaving = fetchUtility(YearlySavingRequestOption);

        const [dataSizeResult, fileCountResult, versionCountResult, yearlySavingResult] = await Promise.all([dataSize, fileCount, versionCount, yearlySaving]);
        if (dataSizeResult && fileCountResult && versionCountResult && yearlySavingResult) {
            setTotalArchived(dataSizeResult);
            setFileNumber(fileCountResult);
            setVersionNumber(versionCountResult);
            setYearlyNumber(yearlySavingResult);
            setNoData(false);
        }
    };

    const onConfigurationClick = async () => {
        $$.loading(true);
        const getPriceData = await fetchUtility(GetConfigurationDataRequestOption);
        if (getPriceData) {
            setIsShowDialog(true);
            setConfigurationPriceInfo({
                SharePointStoragePrice: parseFloat(getPriceData.SharePointStoragePrice),
                ArchivedStoragePrice: parseFloat(getPriceData.ArchivedStoragePrice),
            });
        } else {
            setIsShowDialog(true);
            setConfigurationPriceInfo(_.cloneDeep(DefaultConfigurationPriceInfo));
        }
        $$.loading(false);
    };

    const onCancelDialog = () => {
        setIsShowDialog(false);
    };

    const onSaveDialog = async () => {
        if (!$$.verify(allValidationRef.current)) {
            return false;
        }

        $$.loading(true);
        const requestOption = {
            url: "/api/Dashboard/SaveSOPriceConfiguration",
            data: {
                SharePointStoragePrice: configurationPriceInfo.SharePointStoragePrice,
                ArchivedStoragePrice: configurationPriceInfo.ArchivedStoragePrice,
            }
        };
        const isSucceed = await fetchUtility(requestOption);
        if (isSucceed) {
            showToast.success(RMResx.RM_DSB_Config_SaveSuccessful);
            const yearlyCount = await fetchUtility(YearlySavingRequestOption);
            setYearlyNumber(yearlyCount);
        } else {
            showToast.error(RMResx.RM_DSB_Config_SaveFailed);
        }
        setIsShowDialog(false);
        $$.loading(false);
    };

    const onConfigurationChanged = (price, value) => {
        const clonedConfigurationPriceInfo = _.cloneDeep(configurationPriceInfo);
        clonedConfigurationPriceInfo[price] = value;
        setConfigurationPriceInfo(clonedConfigurationPriceInfo);
    };

    const configurationDialogContent = () => {
        return <div>
            <R.Validation>
                <div ref={allValidationRef}>
                    <div className="reco-dashboard-config">
                        <div className="reco-dashboard-config-title">{RMResx.RM_DSB_Config_SPPrice}</div>
                        <R.Validation
                            element="Input"
                            require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]}>
                            <R.Input
                                id="raSPPriceIpt"
                                type="number"
                                width="100px"
                                min={0}
                                float={4}
                                value={configurationPriceInfo.SharePointStoragePrice}
                                onChange={value => onConfigurationChanged("SharePointStoragePrice", value)}
                                aria={{ ariaLabel: RMResx.RM_DSB_Config_SPPrice }}
                            />
                            <div className="reco-dashboard-config-unit" tabIndex="0">{RMResx.RM_DSB_ConfigUnit}</div>
                        </R.Validation>
                    </div>
                    <div>
                        <div className="reco-dashboard-config-title">{RMResx.RM_DSB_Config_ArchivedPrice}</div>
                        <R.Validation
                            element="Input"
                            require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]}>
                            <R.Input
                                id="raStoragePriceIpt"
                                type="number"
                                width="100px"
                                min={0}
                                float={4}
                                value={configurationPriceInfo.ArchivedStoragePrice}
                                onChange={value => onConfigurationChanged("ArchivedStoragePrice", value)}
                                aria={{ ariaLabel: RMResx.RM_DSB_Config_ArchivedPrice }}
                            />
                            <div className="reco-dashboard-config-unit" tabIndex="0">{RMResx.RM_DSB_ConfigUnit}</div>
                        </R.Validation>
                    </div>
                </div>
            </R.Validation>
        </div>;
    };

    const renderConfigurationDialog = () => {
        return <R.Dialog
            id="configurationDialog"
            header={RMResx.RM_DSB_Title_Config}
            width={464}
            status={{ show: isShowDialog }}
            struct={{ foot: true }}
            onHide={onCancelDialog}
            destroy={true}
        >
            {configurationDialogContent()}
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onCancelDialog} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSaveDialog} />
            </>
        </R.Dialog>;
    };

    return <div>
        <div className="reco-dashboard-cards-title" tabIndex="0">
            {RMResx.RM_DSB_Title_Size}
        </div>
        <div className="reco-dashboard-size-cards">
            <div className="reco-dashboard-size-card">
                <div className="reco-dashboard-size-card-title" style={{ lineHeight: "34px" }} tabIndex="0">
                    {isSupportedStoreInM365 ? RMResx.RM_DSB_Title_External_Archived : RMResx.RM_DSB_Title_Archived}
                </div>
                <EmptyContent isEmpty={noData}>
                    <div className="reco-dashboard-size-card-data" tabIndex="0">
                        <span className="reco-dashboard-size-number">{totalArchived.TotalSize || "0"}</span>
                        <span className="reco-dashboard-size-unit"> {ArchiverDataUnitName[totalArchived.ArchiverDataUnit]}</span>
                    </div>
                </EmptyContent>
            </div>
            <div className="reco-dashboard-size-card">
                <div className="reco-dashboard-size-card-title" style={{ lineHeight: "34px" }} tabIndex="0">
                    {RMResx.RM_DSB_Title_FileNumber}
                </div>
                <EmptyContent isEmpty={noData}>
                    <div className="reco-dashboard-size-card-data" tabIndex="0">
                        <span className="reco-dashboard-size-number">{fileNumber.TotalSize || "0"}</span>
                        <span className="reco-dashboard-size-unit"> {ArchiverDataUnitName[fileNumber.ArchiverDataUnit]}</span>
                    </div>
                </EmptyContent>
            </div>
            <div className="reco-dashboard-size-card">
                <div className="reco-dashboard-size-card-title" style={{ lineHeight: "34px" }} tabIndex="0">
                    {RMResx.RM_DSB_Title_VersionNumber}
                </div>
                <EmptyContent isEmpty={noData}>
                    <div className="reco-dashboard-size-card-data" tabIndex="0">
                        <span className="reco-dashboard-size-number">{versionNumber.TotalSize || "0"}</span>
                        <span className="reco-dashboard-size-unit"> {ArchiverDataUnitName[versionNumber.ArchiverDataUnit]}</span>
                    </div>
                </EmptyContent>
            </div>
            <div className="reco-dashboard-size-card">
                <div className="reco-dashboard-size-card-title">
                    <span tabIndex="0">{RMResx.RM_DSB_Title_YearlySaving}</span>
                    <R.Button
                        id="raDSBConfig"
                        type="bald"
                        icon="fia-configure"
                        tooltip={RMResx.RM_DSB_BtnToolTip_Config}
                        onClick={onConfigurationClick}
                    />
                </div>
                <EmptyContent isEmpty={noData}>
                    <div className="reco-dashboard-size-card-data" tabIndex="0">
                        <span className="reco-dashboard-size-yearly-number">{yearlyNumber.TotalSize || "0"}</span>
                    </div>
                </EmptyContent>
            </div>
        </div>
        {renderConfigurationDialog()}
    </div>;
};

export default ArchivedSize;