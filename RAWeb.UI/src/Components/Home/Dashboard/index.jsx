import React, { useEffect, useState, useRef } from "react";
import "./index.less";

import SiteMapLinks from '../../../Constants/SiteMapLinks';
import AdminView from "./AdminView/index";
import EndUserView from "./EndUserView/index";
import { DashboardJobCreationStatus, DashboardEndUserPermission } from "./Common/Constants";
import { DashboardJobCreationStatusI18n } from "./Common/I18N";
import { LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import { TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import SOAdminView from "./SOAdminView/index";
import { StubFileType, StubFileTypeCol } from "../../CP/CPConstants";
import RetentionAndDestroyView from "./RetentionAndDestroyView";
import { ArchivedRetentionDataSizeRequestOption } from "./RetentionAndDestroyView/config";
import { checkPermission } from '../../../Utilities/permissionManager'
import RouterUrls from "../../../Constants/RouterUrls";
import { ValueAndSavingView } from "./ValueAndSavingView";

const GetEndUserPermissionRequestOption = {
    url: "/api/Dashboard/GetEndUserPermission"
};

const GetAllUsingObsoleteStubTypes = {
    url: "/api/StubSetting/GetAllUsingObsoleteStubTypes",
    method: "GET",
};

const IsAdminRequestOption = {
    url: "/api/Dashboard/IsAdmin"
};

const IsSOAdminRequestOption = {
    url: "/api/Dashboard/IsSOAdmin"
};

const lastCollectTimeRequestOption = {
    url: "/api/Dashboard/GetLastCollectTime"
};

const nextCollectTimeRequestOption = {
    url: "/api/Dashboard/GetNextCollectTime"
};

const checkJobStatusRequestOption = {
    url: "/api/Dashboard/CheckDashboardJobStatus"
};

const runJobRequestOption = {
    url: "/api/Dashboard/RunDashboardCollectJob"
};

const runSOJobRequestOption = {
    url: "/api/Dashboard/IsRunSODashboardJob"
};

const TabIndex = {
    Records: 0,
    Archive: 1,
    RetentionAndDestroy: 2,
    ValueAndSaving: 3
};

const TabIndexWithoutIL = {
    Archive: 0,
    RetentionAndDestroy: 1,
    ValueAndSaving: 2
};

const TabIndexOnlyGoogleAndFS = {
    Records: 0,
    RetentionAndDestroy: 1,
};

const initialUnsupportedStubValue = {
    show: false,
    content: "",
}

const Dashboard = () => {

    const [isWait, setIsWait] = useState(true);

    const [isAdmin, setIsAdmin] = useState(false);

    const [isSOAdmin, setIsSOAdmin] = useState(false);

    const [isRunSODashboardJob, setIsRunSODashboardJob] = useState(false);

    const realTimeIsAdminRef = useRef(false);

    const [endUserPermission, setEndUserPermission] = useState(0);

    const [collectTime, setCollectTime] = useState("");

    const [buttonIsDisable, setButtonIsDisable] = useState(true);

    const [buttonToolTip, setButtonToolTip] = useState("");

    const [showLifeCycleDashboard, setShowLifeCycleDashboard] = useState(LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusGoogleLicense());

    const [showRetentionAndDestroyDashboard, setShowRetentionAndDestroyDashboard] = useState(LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusGoogleLicense());

    const [showValueAndSavingDashboard, setShowValueAndSavingDashboard] = useState(LicenseHelper.HasOpusILLicense());

    const [tabIndex, setTabIndex] = useState(LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusGoogleLicense() ? TabIndex.Records : TabIndexWithoutIL.Archive);

    const [unsupportedStub, setUnsupportedStub] = useState(initialUnsupportedStubValue);

    const [archivedRetentionData, setArchivedRetentionData] = useState(null);

    const [retentionSyncTime, setRetentionSyncTime] = useState("");

    useEffect(() => {
        const fetchData = async () => {
            await checkCurrentUserPermission();
            getDashboardCollectTime();
            checkDashboardJobStatus();
            if (LicenseHelper.HasOpusSOLicenseOnly()) {
                checkSODashboardJobStatus();
            }
            addTelemetryRecord(TelemetryModule.Dashboard, TelemetryEventType.DashboardLoaded);
            if (LicenseHelper.HasOpusILLicense() || LicenseHelper.HasDiscoveryLicense() || LicenseHelper.HasOpusSOLicense()) {
                getAllObsoleteStubTypes();
            }
        };

        fetchData();
    }, []);

    useEffect(() => {
        if (checkPermission(RouterUrls.CP_Index, RM.UserResources)) {
            getArchivedRetentionDataSize();
        }
    }, [])

    const checkCurrentUserPermission = async () => {
        const endUserPermission = await fetchUtility(GetEndUserPermissionRequestOption);
        setEndUserPermission(endUserPermission);
        if (endUserPermission == DashboardEndUserPermission.None) {
            const isAdmin = await fetchUtility(IsAdminRequestOption);
            setIsAdmin(isAdmin);

            const isSOAdmin = await fetchUtility(IsSOAdminRequestOption);
            setIsSOAdmin(isSOAdmin);

            realTimeIsAdminRef.current = isAdmin;
        }
        setIsWait(false);
    };

    const getAllObsoleteStubTypes = async () => {
        const stubTypes = await fetchUtility(GetAllUsingObsoleteStubTypes);
        if (stubTypes.includes(StubFileType.Aspx)) {
            setUnsupportedStub({
                show: true,
                content: (
                    <$g.I18NProvider msg={RMResx.RM_DSB_Aspx_Warning}>
                        {StubFileTypeCol[StubFileType.Aspx].name}
                    </$g.I18NProvider>
                )
            });
        }
    }

    const getDashboardCollectTime = async () => {
        const lastPromise = fetchUtility(lastCollectTimeRequestOption);
        const nextPromise = fetchUtility(nextCollectTimeRequestOption);
        let values = await Promise.all([lastPromise, nextPromise]);

        let message = "";
        if (values[0] !== "") {
            message += (RMResx.RM_DSB_LastUpdateTime.format(values[0]) + " ");
        }
        else {
            if (realTimeIsAdminRef.current) {
                showTips();
            }
        }

        if (values[1] !== "") {
            message += RMResx.RM_DSB_NextUpdateTime.format(values[1]);
        }

        setCollectTime(message);
    };

    const checkDashboardJobStatus = async () => {
        const jobStatus = await fetchUtility(checkJobStatusRequestOption);
        if (jobStatus === DashboardJobCreationStatus.None) {
            setButtonIsDisable(false);
            return;
        }
        setButtonToolTip(DashboardJobCreationStatusI18n.get(jobStatus));
    };

    const checkSODashboardJobStatus = async () => {
        const isRunSOJob = await fetchUtility(runSOJobRequestOption);
        setIsRunSODashboardJob(isRunSOJob);
    };

    const runNowClick = async () => {
        setButtonIsDisable(true);
        const creationJobStatus = await fetchUtility(runJobRequestOption);
        let messageType = "success";

        if (creationJobStatus !== DashboardJobCreationStatus.Succeed) {
            messageType = "error";
        }

        const message = DashboardJobCreationStatusI18n.get(creationJobStatus);

        if (creationJobStatus === DashboardJobCreationStatus.Succeed) {
            let content = getDashboardJobCreationSucceedI18N();
            showToast._showMsg(messageType, content);
        }
        else {
            showToast._showMsg(messageType, message);
        }

        if (creationJobStatus === DashboardJobCreationStatus.Failed) {
            setButtonIsDisable(false);
        }

        if (creationJobStatus === DashboardJobCreationStatus.Succeed) {
            setButtonToolTip(DashboardJobCreationStatusI18n.get(DashboardJobCreationStatus.ExistsJobQueue));
        }
    };

    const getArchivedRetentionDataSize = async () => {
        const res = await fetchUtility(ArchivedRetentionDataSizeRequestOption);
        if (res) {
            setArchivedRetentionData(res);
            let message = "";
            if (res.LastRunJobDate) {
                message += `${RMResx.RM_DSB_LastUpdateTime.format(res.LastRunJobDate)} `;
            }
            if (res.NextRunJobDate) {
                message += RMResx.RM_DSB_NextUpdateTime.format(res.NextRunJobDate);
            }
            setRetentionSyncTime(message);
        }
    };

    const getDashboardJobCreationSucceedI18N = () => {
        return (
            <$g.I18NProvider msg={RMResx.RM_DSB_Succeed}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
            </$g.I18NProvider>
        );
    };

    const showTips = async () => {
        $$.alert({
            title: RMResx.RM_DSB_Message,
            content: RMResx.RM_DSB_Tips,
        });
    };

    const getUserView = () => {
        if (endUserPermission != DashboardEndUserPermission.None) {
            return <EndUserView endUserPermission={endUserPermission} />;
        }
        return <AdminView isAdmin={isAdmin} />;
    };

    const getSOView = () => {
        if (showRetentionAndDestroyDashboard) {
            return <RetentionAndDestroyView archivedRetentionData={archivedRetentionData} />;
        }
        if (showValueAndSavingDashboard) {
            return <ValueAndSavingView />;
        }
        return <SOAdminView isRunSODashboardJob={isRunSODashboardJob} />;
    };

    const handleSelectedTabChanged = (newIndex) => {
        if (newIndex === TabIndex.Archive) {
            checkSODashboardJobStatus();
        }
        setShowLifeCycleDashboard(newIndex === TabIndex.Records);
        setShowRetentionAndDestroyDashboard(newIndex === TabIndex.RetentionAndDestroy);
        setShowValueAndSavingDashboard(newIndex === TabIndex.ValueAndSaving);
        setTabIndex(newIndex);
    };

    const handleSelectedTabWithoutILChanged = (newIndex) => {
        if (newIndex === TabIndexWithoutIL.Archive) {
            checkSODashboardJobStatus();
        }
        setShowRetentionAndDestroyDashboard(newIndex === TabIndexWithoutIL.RetentionAndDestroy);
        setShowValueAndSavingDashboard(newIndex === TabIndexWithoutIL.ValueAndSaving);
        setTabIndex(newIndex);
    };

    const handleSelectedTabOnlyGoogleAndFSChanged = (newIndex) => {
        setShowLifeCycleDashboard(newIndex === TabIndexOnlyGoogleAndFS.Records);
        setShowRetentionAndDestroyDashboard(newIndex === TabIndexOnlyGoogleAndFS.RetentionAndDestroy);
        setTabIndex(newIndex);
    }

    const isPermissionDisplayILAndRetentionTabs = () => {
        const requiredPermissions = [RouterUrls.CP_Index, "Source_Google", "Source_FS"];
        return requiredPermissions.every(permission => checkPermission(permission, RM.UserResources));
    }

    const renderRetentionSyncTime = () => {
        const hasILOrGoogle = LicenseHelper.HasOpusILAndSOLicense() || LicenseHelper.HasOpusGoogleAndSOLicense();
        const timeText = (
            <div className="flex align-center ra-flex-1">
                <div className="reco-dashboard-time-text reco-dashboard-time-text-overflow" data-tooltip="ifneed" tabIndex={0}>
                    <span className="reco-dashboard-time-text-span">{retentionSyncTime}</span>
                </div>
                <$g.Popover>{RMResx.RM_DSB_RetentionAndInstruction_Tips}</$g.Popover>
            </div>
        );

        if (isSOAdmin) {
            if (hasILOrGoogle && tabIndex === TabIndex.RetentionAndDestroy) {
                return timeText;
            }

            if (!hasILOrGoogle && tabIndex === TabIndexWithoutIL.RetentionAndDestroy) {
                return timeText;
            }
        }

        return null;
    }

    const renderValueAndSavingInfo = () => {
        const information = (
            <div className="flex align-center ra-flex-1 justify-end">
                <$g.Popover>{RMResx.RM_DSB_ValueAndSaving_Tips}</$g.Popover>
            </div>
        );

        if (isSOAdmin) {
            if (LicenseHelper.HasOpusILLicense() && tabIndex === TabIndex.ValueAndSaving) {
                return information;
            }
            if (!LicenseHelper.HasOpusILLicense() && tabIndex === TabIndexWithoutIL.ValueAndSaving) {
                return information;
            }
        }
        return null;
    };

    return (
        <div className="reco-dashboard-wrapper">
            <div className="margin-bottom-l" hidden={!unsupportedStub.show}>
                <R.Messagebar
                    message={unsupportedStub.content}
                    classify="warn"
                    status={{ show: unsupportedStub.show }}
                    onClose={() => setUnsupportedStub(initialUnsupportedStubValue)}
                />
            </div>
            <section className="reco-dashboard-title">
                <$g.SiteMap data={[SiteMapLinks.Home]} />
                {
                    (isAdmin && tabIndex === TabIndex.Records) && <div className="reco-dashboard-button-runnow">
                        <R.Button
                            text={RMResx.RM_DSB_BtnRunNow}
                            primary={true}
                            block={true}
                            disabled={buttonIsDisable}
                            onClick={() => runNowClick()}
                            tooltip={buttonToolTip}
                        />
                    </div>
                }
            </section>
            <section className={"reco-dashboard-time"}>
                <div className="reco-dashboard-tabcontrol">
                    {isSOAdmin && (
                        (LicenseHelper.HasOpusILAndSOLicense() || LicenseHelper.HasOpusGoogleAndSOLicense()) ? (
                            <R.Tabcontrol flex maxWidth="none" active={tabIndex} onChange={handleSelectedTabChanged.bind(this)}>
                                <R.TabPanel key={0} tab={RMResx.RM_AR_SPS_TabControl_Information}></R.TabPanel>
                                <R.TabPanel key={1} tab={RMResx.RM_AR_SPS_TabControl_Storage}></R.TabPanel>
                                <R.TabPanel key={2} tab={RMResx.RM_AR_SPS_TabControl_RetentionAndDestroy}></R.TabPanel>
                                <R.TabPanel key={3} tab={RMResx.RM_AR_SPS_TabControl_ValueAndSaving}></R.TabPanel>
                            </R.Tabcontrol>
                        ) : (
                            <R.Tabcontrol flex maxWidth="none" active={tabIndex} onChange={handleSelectedTabWithoutILChanged.bind(this)}>
                                <R.TabPanel key={0} tab={RMResx.RM_AR_SPS_TabControl_Storage}></R.TabPanel>
                                <R.TabPanel key={1} tab={RMResx.RM_AR_SPS_TabControl_RetentionAndDestroy}></R.TabPanel>
                                <R.TabPanel key={2} tab={RMResx.RM_AR_SPS_TabControl_ValueAndSaving}></R.TabPanel>
                            </R.Tabcontrol>
                        )
                    )}
                    {!isSOAdmin && LicenseHelper.HasOpusGoogleLicense() && isPermissionDisplayILAndRetentionTabs() && (
                        <R.Tabcontrol flex maxWidth="none" active={tabIndex} onChange={handleSelectedTabOnlyGoogleAndFSChanged.bind(this)}>
                            <R.TabPanel key={0} tab={RMResx.RM_AR_SPS_TabControl_Information}></R.TabPanel>
                            <R.TabPanel key={1} tab={RMResx.RM_AR_SPS_TabControl_RetentionAndDestroy}></R.TabPanel>
                        </R.Tabcontrol>
                    )}
                </div>
                {(LicenseHelper.HasOpusILAndSOLicense() || LicenseHelper.HasOpusGoogleAndSOLicense()) && tabIndex === TabIndex.Records && (
                    <div className="reco-dashboard-time-text">
                        <div className="reco-dashboard-time-text-overflow" data-tooltip="ifneed" tabIndex={0}>
                            <span className="reco-dashboard-time-text-span">{collectTime}</span>
                        </div>
                        <$g.Popover>{isAdmin ? RMResx.RM_DSB_Tips : RMResx.RM_DSB_NoAdmin_Tips}</$g.Popover>
                    </div>
                )}
                {renderRetentionSyncTime()}
                {renderValueAndSavingInfo()}
            </section>
            {
                isWait ? <div className="reco-dashboard-wait-placeholder"></div> : showLifeCycleDashboard ? getUserView() : getSOView()
            }
        </div>
    );
};

export default Dashboard;