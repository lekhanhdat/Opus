import { useCallback, useEffect, useState } from "react";

import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import SwitchForTeamsTable from "./SwitchForTeamsTable";
import { showToast } from "../../../../Utilities/CommonUtil";
import { NodeIconClass } from "../../../../Constants/DAEnums";
import { StatusCode, ModuleType } from "./Constants";
import RouterUrls from "../../../../Constants/RouterUrls";
import { isMoreThanCustomDaysOld } from "../../../../Utilities/DateUtil";
import { RAMessageType } from "../Common/CRMCommonUtil";

import "../../../../Less/BCM/ContentRepositoryManagement/common.less";
import "../../../../Less/BCM/ContentRepositoryManagement/crmForTeams.less";

function SwitchForTeamsComponent({ history }) {
    const [isScanned, setIsScanned] = useState(false);
    const [isScanning, setIsScanning] = useState(false);
    const [isScanFailed, setIsScanFailed] = useState(false);
    const [progressValue, setProgressValue] = useState(0);
    const [activeTab, setActiveTab] = useState(ModuleType.Lifecycle);
    const [isIgnoreConfiguration, setIsIgnoreConfiguration] = useState(false);

    useEffect(() => {
        getTeamsChannelConflictCheckJobInfo();
    }, []);

    const handleSetScanStatus = (isScanFailed, isScanning, isScanned) => {
        setTimeout(() => {
            setIsScanFailed(isScanFailed);
            setIsScanning(isScanning);
            setIsScanned(isScanned);
        }, 300);
    }

    const getTeamsChannelConflictCheckJobInfo = () => {
        const option = {
            url: "/api/TeamsSettingApi/GetTeamsChannelConflictCheckJobInfo",
            method: "GET",
        };
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                if (res.Progress === 100) {
                    setProgressValue(99);
                } else {
                    setProgressValue(res.Progress);
                }

                if (res.Status === StatusCode.Failed) {
                    handleSetScanStatus(true, false, false);
                    return;
                }

                if (res.StartTime !== 0 && isMoreThanCustomDaysOld(res.StartTime, 2)) { // more than 2 days old
                    handleSetScanStatus(false, false, false);
                    return;
                }

                setIsScanning(
                    res.Status === StatusCode.Waiting ||
                        res.Status === StatusCode.InProgress
                );
                setIsScanned(res.Status === StatusCode.Finished);
            })
            .finally(() => $$.loading(false));
    };

    const handleScan = () => {
        setIsScanFailed(false);
        setIsScanning(true);
        const option = {
            url: "/api/TeamsSettingApi/RunTeamsChannelSettingConflictCheckJob",
            method: "POST",
        };
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                if (res) {
                    getTeamsChannelConflictCheckJobInfo();
                } else {
                    showToast.error(
                        RMResx.RM_AR_Teams_SwitchPage_ScanJobFailed
                    );
                }
            })
            .finally(() => {
                $$.loading(false);
                setIsScanning(false);
            });
    };

    // Export
    const onExport = () => {
        const option = {
            url: "/api/TeamsSettingApi/RunConflictSettingDetailExportJob",
            method: "POST",
        };
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                if (res) {
                    const content = (
                        <$g.I18NProvider
                            msg={
                                RMResx.RM_AR_Teams_SwitchPage_ExportJobSuccessfully
                            }
                        >
                            <a className="ra-link-a" href="/Root/JM/Index">
                                {RMResx.RM_JS_JM_Title}
                            </a>
                            <a className="ra-link-a" href="/Root/DC/Download">
                                {RMResx.RM_JS_DC_Title}
                            </a>
                        </$g.I18NProvider>
                    );
                    showToast.success(content);
                } else {
                    showToast.error(
                        RMResx.RM_AR_Teams_SwitchPage_ExportJobFailed
                    );
                }
            })
            .finally(() => {
                $$.loading(false);
                setIsScanning(false);
            });
    };

    const handleExport = () => {
        const args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_Teams_SwitchPage_ExportConfirmMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: onExport,
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    // Switch functions
    const handleCancel = async () => {
        const option = {
            url: "/api/TeamsSettingApi/CancelUpgradeTeamsNodeSetting",
            method: "POST",
        };
        $$.loading(true);
        await fetchUtility(option);
        $$.loading(false);
        history.push({
            pathname:
                RouterUrls.BCM_ContentRepositoryManagement_Teams,
        });
    };

    const onStart = () => {
        const option = {
            url: "/api/TeamsSettingApi/UpgradeTeams",
            method: "POST",
            data: true
        };
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                if (res.MessageType == RAMessageType.Successful) {
                    const content = (
                        <$g.I18NProvider
                            msg={RMResx.RM_AR_Teams_MigratePage_RunJobSuccess}
                        >
                            <a className="ra-link-a" href="/Root/JM/Index">
                                {RMResx.RM_JS_JM_Title}
                            </a>
                        </$g.I18NProvider>
                    );
                    showToast.success(content);
                } else if (res.MessageType == RAMessageType.Failed) {
                    showToast.error(res.ErrorMessage);
                }
            })
            .finally(() => $$.loading(false));
    };

    const handleStart = () => {
        const args = {
            width: "550px",
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_Teams_SwitchPage_ConfirmMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_No,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    id: "crmTeamsRunMigrate",
                    text: RMResx.RM_JS_Common_Yes,
                    primary: true,
                    classify: "theme",
                    onClick: onStart,
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    // Render
    const renderScanState = useCallback(() => {
        if (isScanFailed) {
            return (
                <div style={{ gap: 40 }} className="flex flex-column crm-progress">
                    <div className="crm-progress-wrapper">
                        <R.Progressbar
                            id="raScanFailedProgressbar"
                            value={progressValue}
                            classify="error"
                            template={false}
                            animated
                        />
                        <div tabIndex={0} className="crm-failed">{RMResx.RM_AR_Teams_SwitchPage_FailedStatus}</div>
                        <div tabIndex={0} style={{ fontStyle: "italic" }} className="text-red text-start margin-top-s">{RMResx.RM_AR_Teams_SwitchPage_FailedMsg}</div>
                    </div>
                    <div className="text-center">
                        <R.Button
                            id="raRescanBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_AR_Teams_SwitchPage_RescanBtn}
                            onClick={handleScan}
                        />
                    </div>
                </div>
            );
        }

        if (isScanning) {
            return (
                <div style={{ gap: 40 }} className="flex flex-column crm-progress">
                    <div>
                        <R.Progressbar
                            id="raScanProgressbar"
                            value={progressValue}
                            classify="info"
                            template="percent"
                            animated
                        />
                    </div>
                    <div className="text-center">
                        <R.Button
                            id="raRefreshBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_AR_Teams_SwitchPage_RefreshBtn}
                            onClick={() => {
                                getTeamsChannelConflictCheckJobInfo();
                            }}
                        />
                    </div>
                </div>
            );
        }

        return (
            <R.Button
                id="raScanBtn"
                primary={true}
                classify="theme"
                text={RMResx.RM_AR_Teams_SwitchPage_ScanBtn}
                onClick={handleScan}
            />
        );
    }, [isScanFailed, isScanning]);

    const renderUIScan = () => {
        if (isScanned) {
            return (
                <>
                    <div tabIndex={0}>
                        {RMResx.RM_AR_Teams_SwitchPage_Desc03}
                    </div>
                    <div>
                        <R.Button
                            id="raExportBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_AR_Teams_SwitchPage_ExportBtn}
                            onClick={handleExport}
                        />
                        <div className="margin-top-m teams-table-wrapper">
                            <div
                                style={{ paddingBottom: 0 }}
                                className="padding-l"
                            >
                                <R.Tabcontrol
                                    flex
                                    active={activeTab}
                                    onChange={(value) => setActiveTab(value)}
                                >
                                    {[
                                        RMResx.RM_AR_SPS_TabControl_Information,
                                        RMResx.RM_AR_SPS_TabControl_Storage,
                                    ].map((item, index) => (
                                        <R.TabPanel key={index} tab={item}>
                                            <div>
                                                <SwitchForTeamsTable
                                                    id={`raSwitchForTeamsTable-${index}`}
                                                    moduleType={activeTab}
                                                />
                                            </div>
                                        </R.TabPanel>
                                    ))}
                                </R.Tabcontrol>
                            </div>
                        </div>
                    </div>
                </>
            );
        }

        return (
            <>
                <div tabIndex={0}>{RMResx.RM_AR_Teams_SwitchPage_Desc01}</div>
                <div tabIndex={0}>{RMResx.RM_AR_Teams_SwitchPage_Desc02}</div>
                <div className="text-center">
                    {renderScanState()}
                </div>
            </>
        );
    };

    return (
        <>
            <section className="crm-header">
                <$g.SiteMap
                    data={[
                        SiteMapLinks.BCM_ContentRepositoryManagement_Teams,
                        SiteMapLinks.BCM_ContentRepositoryManagement_Teams_Switch,
                    ]}
                />
            </section>
            <section className="crm-content">
                <div id="crmForTeamsSwitch">
                    <div
                        style={{
                            padding: isScanned ? "40px 24px" : 40,
                            borderRadius: isScanned ? "8px 8px 0 0" : 8,
                        }}
                        className="border flex flex-column align-center gap-l bg-white"
                    >
                        <div className="flex justify-center align-center gap-l teams-icon-wrapper">
                            <div
                                className={`teams-icon ra-tree-icon ${NodeIconClass.TeamsFarm}`}
                                tabIndex={0}
                                aria-label={RMResx.RM_AR_Teams_MigratePage_Icon}
                            ></div>
                        </div>
                        <div
                            style={{ width: "100%" }}
                            className="flex flex-column gap-l"
                        >
                            {renderUIScan()}
                        </div>
                    </div>
                    {isScanned && (
                        <div className="footer border flex align-center bg-white">
                            <R.Checkbox
                                name="checkbox-1"
                                text={
                                    RMResx.RM_AR_Teams_SwitchPage_IgnoreConfigurationCb
                                }
                                checked={isIgnoreConfiguration}
                                onChange={(value) =>
                                    setIsIgnoreConfiguration(value)
                                }
                            />
                            <div className="flex align-center gap-s">
                                <R.Button
                                    id="raCancelBtn"
                                    classify="blank"
                                    text={RMResx.RM_JS_Common_Cancel}
                                    onClick={handleCancel}
                                />
                                <R.Button
                                    id="raStartBtn"
                                    primary={true}
                                    classify="theme"
                                    text={
                                        RMResx.RM_AR_Teams_MigratePage_StartBtn
                                    }
                                    disabled={!isIgnoreConfiguration}
                                    onClick={handleStart}
                                />
                            </div>
                        </div>
                    )}
                </div>
            </section>
        </>
    );
}

export default SwitchForTeamsComponent;
