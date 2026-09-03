import { NodeLevel } from "../../../../Constants/DAEnums";
import { SourceFlags } from "../../../../Constants/Constants";
import "../../../../Less/BCM/ContentRepositoryManagement/generalManagementSetting.less";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import { checkPermission } from "../../../../Utilities/permissionManager";
import { EnableRecordManagementSetting } from "../CRMForSPO/ContentRepositoryManagementForSPO";

export default class GeneralManagementPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        const enableLifecycleManagementForSharePointLists = this.props.data.EnableLifecycleManagementForSharePointLists ?? true;
        this.state = {
            radioClassification: [
                { text: RMResx.RM_JS_Common_Yes, value: EnableRecordManagementSetting.Enable, checked: this.props.data.EnableRecordManagement == EnableRecordManagementSetting.Enable ? true : false },
                { text: RMResx.RM_JS_Common_No, value: EnableRecordManagementSetting.Disable, checked: this.props.data.EnableRecordManagement == EnableRecordManagementSetting.Enable ? false : true },
            ],
            radioDataSync: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.props.data.IsSyncData },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !this.props.data.IsSyncData },
            ],
            radioDisplayUniqueId: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.props.data.IsShowUniqueId },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !this.props.data.IsShowUniqueId },
            ],
            radioUnlockSite: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.props.data.SupportLockedSite },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !this.props.data.SupportLockedSite },
            ],
            radioShowEnableLifecycle: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: enableLifecycleManagementForSharePointLists },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !enableLifecycleManagementForSharePointLists },
            ],
            enableClassification: this.props.data.EnableRecordManagement,
            enableDataSync: this.props.data.IsSyncData,
            enableClassificationChanged: false,
            enableDataSyncChanged: false,
            displayUniqueId: this.props.data.IsShowUniqueId,
            displayUniqueIdChanged: false,
            uniqueIdData: {},
            isSupportLockedSite: this.props.data.SupportLockedSite,
            isSupportLockedSiteChanged: false,
            radioDownloadRCCReport: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.props.data.IsAllowUserDownloadRCCReport },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !this.props.data.IsAllowUserDownloadRCCReport },
            ],
            isSupportDownloadRCCReport: this.props.data.IsAllowUserDownloadRCCReport,
            isSupportDownloadRCCReportChanged: false,
            isShowEnableLifecycle: enableLifecycleManagementForSharePointLists,
            isShowEnableLifecycleChanged:false
        };
        this.supportingLockedSCOption = [NodeLevel.WebApplication, NodeLevel.SiteCollection, NodeLevel.Office365GroupEntire];
    }

    componentInit() {
        this.loadUniqueIdSetting();
    }

    componentReceive(type, args) {
        switch (type) {
            case "reloadUniqueId":
                this.loadUniqueIdSetting(args);
                break;
        }
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    loadUniqueIdSetting() {
        let option = {
            url: "/API/BCMAdminSettingApi/LoadingUniqueIdSetting",
            method: "Post",
            data: {
                SourceFlag: this.props.sourceFlag,
            }
        };
        fetchUtility(option).then((res) => {
            this.setState({
                uniqueIdData: res
            });
        }).catch((e) => {
        });
    }

    onSave(callback) {
        let generalSettingData = this.props.data;
        generalSettingData.EnableRecordManagement = this.state.enableClassification;
        generalSettingData.IsSyncData = this.state.enableDataSync;
        generalSettingData.IsShowUniqueId = this.state.displayUniqueId;
        generalSettingData.SupportLockedSite = this.state.isSupportLockedSite;
        generalSettingData.IsAllowUserDownloadRCCReport = this.state.isSupportDownloadRCCReport;
        generalSettingData.EnableLifecycleManagementForSharePointLists = this.state.isShowEnableLifecycle;
        if(LicenseHelper.EnableJPMCFileSystemFeature() && this.props.sourceFlag === SourceFlags.FS){
            generalSettingData.IsActive = true;
        }
        let option = {
            url: this.props.context.saveDataUrl,
            method: "Post",
            data: generalSettingData
        };
        return fetchUtility(option).then(function (res) {
            return { data: JSON.parse(res) };
        }).then(result => {
            callback(result, this.state.enableClassification == EnableRecordManagementSetting.Disable && this.state.enableClassificationChanged);
        });
    }

    onClassificationChanged = (args) => {
        this.setState({ enableClassification: args, enableClassificationChanged: true });

    }

    onDataSyncChanged = (args) => {
        this.setState({ enableDataSync: args, enableDataSyncChanged: true });
    }

    onDisplayUniqueIdChanged = (args) => {
        this.setState({ displayUniqueId: args, displayUniqueIdChanged: true });
    }

    onSupportLockedSiteChanged = (args) => {
        this.setState({ isSupportLockedSite: args, isSupportLockedSiteChanged: true });
    }

    onSupportDownloadRCCReportChanged = (args) => {
        this.setState({ isSupportDownloadRCCReport: args, isSupportDownloadRCCReportChanged: true });
    }

    onShowEnableLifecycleChanged = (args) => {
        this.setState({ isShowEnableLifecycle: args, isShowEnableLifecycleChanged: true });
    }

    showUniqueIdPanel = (generalData) => {
        if (checkPermission("BCM_ContentRepositoryManagement_UniqueId", RM.UserResources)) {
            this.dispatch("uniqueId", 'showUniqueIdPanel');
        } else {
            let args = {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_SPS_UniqueIdDisplay_DelegateWarning,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                            $$.messagedialog(false);
                        }
                    }
                ]
            };
            $$.messagedialog(true, args);
        }
    }

    renderCheckUniqueId() {
        if (this.state.enableDataSyncChanged) {
            if (this.state.enableDataSync) {
                if (this.props.context.showUniqueIdWarn && !this.state.uniqueIdData.IsActived && this.state.uniqueIdData.Id == 0) {
                    return <div className="ra-general-panel">
                        <div className="ra-general-panel-content" role="alert" aria-live="assertive">
                            <span className="ra-general-panel-font">
                                <$g.I18NProvider msg={RMResx.RM_JS_SPS_UniqueIdDisplay_Warning}>
                                    <span className="ra-general-panel-uniqueid" onClick={this.showUniqueIdPanel} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_JS_SP_UniqueIdSetting_Btn}</span>
                                </$g.I18NProvider>
                            </span>
                        </div>
                    </div>;
                } else {
                    return <div className="ra-general-panel">
                        <div className="ra-general-panel-content" role="alert" aria-live="assertive">
                            <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                            <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_EnableDataSync}</span>
                        </div>
                    </div>;
                }
            } else {
                return <div className="ra-general-panel">
                    <div className="ra-general-panel-content" role="alert" aria-live="assertive">
                        <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                        <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_DisableDataSync}</span>
                    </div>
                </div>;
            }
        } else if (this.state.displayUniqueIdChanged) {
            if (this.state.displayUniqueId) {
                if (this.props.context.showUniqueIdWarn && !this.state.uniqueIdData.IsActived && this.state.uniqueIdData.Id == 0) {
                    return <div className="ra-general-panel">
                        <div className="ra-general-panel-content" role="alert" aria-live="assertive">
                            <span className="ra-general-panel-font">
                                <$g.I18NProvider msg={RMResx.RM_JS_SPS_UniqueIdDisplay_Warning}>
                                    <span className="ra-general-panel-uniqueid" onClick={this.showUniqueIdPanel} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_JS_SP_UniqueIdSetting_Btn}</span>
                                </$g.I18NProvider>
                            </span>
                        </div>
                    </div>;
                } else {
                    return <div className="ra-general-panel" role="alert" aria-live="assertive">
                        <div className="ra-general-panel-content">
                            <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                            <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_EnableDisplayUniqueId}</span>
                        </div>
                    </div>;
                }
            } else {
                return <div className="ra-general-panel">
                    <div className="ra-general-panel-content" role="alert" aria-live="assertive">
                        <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                        <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_DisableDisplayUniqueId}</span>
                    </div>
                </div>;
            }
        }
    }

    render() {
        let generalSetting = this.props.data;
        const isShowEnableLifecycle = (this.props.sourceFlag === SourceFlags.Teams || this.props.sourceFlag === SourceFlags.SP) && LicenseHelper.EnableRecordsArchiver() && this.supportingLockedSCOption.includes(generalSetting.Level)
        return (
            <div id={this.props.id}>
                <div className="ra-crm-form-content">
                    <div
                        id="ariaEnableClassification"
                        className="ra-setting-panel-title"
                    >
                        {RMResx.RM_JS_SPS_EnableClassicationTitle}
                    </div>
                    <R.Radio.Group
                        aria={{ "aria-labelledby": "ariaEnableClassification" }}
                        name="enableClassification"
                        items={this.state.radioClassification}
                        onChange={this.onClassificationChanged}
                    />
                    {this.state.enableClassification ==
                        EnableRecordManagementSetting.Enable &&
                        this.state.enableClassificationChanged && (
                            <div className="ra-general-panel">
                                <div
                                    className="ra-general-panel-content"
                                    role="alert"
                                    aria-live="assertive"
                                >
                                    <span className="ra-general-panel-warn">
                                        {RMResx.RM_JS_SPS_Warning}
                                    </span>
                                    <span className="ra-general-panel-font">
                                        {" "}
                                        {
                                            RMResx.RM_JS_SPS_Warning_EnableRecordsManagement
                                        }
                                    </span>
                                </div>
                            </div>
                        )}
                    {this.state.enableClassification ==
                        EnableRecordManagementSetting.Disable &&
                        this.state.enableClassificationChanged && (
                            <div className="ra-general-panel">
                                <div
                                    className="ra-general-panel-content"
                                    role="alert"
                                    aria-live="assertive"
                                >
                                    <span className="ra-general-panel-warn">
                                        {RMResx.RM_JS_SPS_Warning}
                                    </span>
                                    <span className="ra-general-panel-font">
                                        {" "}
                                        {
                                            RMResx.RM_JS_SPS_Warning_DisableRecordsManagement
                                        }
                                    </span>
                                </div>
                            </div>
                        )}
                </div>
                {generalSetting &&
                    this.state.enableClassification ==
                        EnableRecordManagementSetting.Enable &&
                    this.props.context.supportSync(generalSetting) && (
                        <div className="ra-crm-form-content">
                            <div
                                id="ariaEnableDataSync"
                                className="ra-setting-panel-title"
                            >
                                {RMResx.RM_SPS_IsSync}
                            </div>
                            <R.Radio.Group
                                aria={{
                                    "aria-labelledby": "ariaEnableDataSync",
                                }}
                                name="enableDataSync"
                                items={this.state.radioDataSync}
                                onChange={this.onDataSyncChanged}
                            />
                            {this.renderCheckUniqueId()}
                        </div>
                    )}

                {generalSetting &&
                    this.state.enableClassification ==
                        EnableRecordManagementSetting.Enable &&
                    this.props.context.supperDisplayUniqueId(
                        generalSetting,
                    ) && (
                        <div className="ra-crm-form-content">
                            <div
                                id="ariaUniqueId"
                                className="ra-setting-panel-title"
                            >
                                {RMResx.RM_JS_SPS_OneDriveUniqueId}
                            </div>
                            <R.Radio.Group
                                aria={{ "aria-labelledby": "ariaUniqueId" }}
                                name="displayUniqueId"
                                items={this.state.radioDisplayUniqueId}
                                onChange={this.onDisplayUniqueIdChanged}
                            />
                            {this.renderCheckUniqueId()}
                        </div>
                    )}
                {generalSetting &&
                    this.state.enableClassification ==
                        EnableRecordManagementSetting.Enable &&
                    this.supportingLockedSCOption.includes(generalSetting.Level) &&
                    this.props.context.supportUnlockSite && (
                        <div className="ra-crm-form-content">
                            <div
                                id="unlockSite"
                                className="ra-setting-panel-title"
                            >
                                {RMResx.RM_JS_SPS_UnLockedSiteCollection}
                            </div>
                            <R.Radio.Group
                                aria={{ "aria-labelledby": "unlockSite" }}
                                name="isSupportLockedSite"
                                items={this.state.radioUnlockSite}
                                onChange={this.onSupportLockedSiteChanged}
                            />
                            {this.state.isSupportLockedSite &&
                                this.state.isSupportLockedSiteChanged && (
                                    <div className="ra-general-panel">
                                        <div
                                            className="ra-general-panel-content"
                                            role="alert"
                                            aria-live="assertive"
                                        >
                                            <span className="ra-general-panel-warn">
                                                {RMResx.RM_JS_SPS_Warning}
                                            </span>
                                            <span className="ra-general-panel-font">
                                                {" "}
                                                {
                                                    RMResx.RM_AR_SPS_Options_LockedSiteCollection
                                                }
                                            </span>
                                        </div>
                                    </div>
                                )}
                        </div>
                    )}
                {generalSetting &&
                    this.state.enableClassification ==
                        EnableRecordManagementSetting.Enable &&
                    this.props.context.supportDownloadRCCReport && (
                        <div className="ra-crm-form-content">
                            <div
                                id="downloadRCCReport"
                                className="ra-setting-panel-title"
                            >
                                {RMResx.RM_JS_FS_Radio_DownloadRCCReport}
                            </div>
                            <R.Radio.Group
                                aria={{ "aria-labelledby": "downloadRCCReport" }}
                                name="isSupportDownloadRCCReport"
                                items={this.state.radioDownloadRCCReport}
                                onChange={this.onSupportDownloadRCCReportChanged}
                            />
                            {this.state.isSupportDownloadRCCReport &&
                                this.state.isSupportDownloadRCCReportChanged && (
                                    <div className="ra-general-panel">
                                        <div
                                            className="ra-general-panel-content"
                                            role="alert"
                                            aria-live="assertive"
                                        >
                                            <span className="ra-general-panel-warn">
                                                {RMResx.RM_JS_SPS_Warning}
                                            </span>
                                            <span className="ra-general-panel-font">
                                                {" "}
                                                {
                                                    RMResx.RM_JS_FS_Warning_EnableDownloadRCCReport
                                                }
                                            </span>
                                        </div>
                                    </div>
                                )
                            }
                            {!this.state.isSupportDownloadRCCReport &&
                                this.state.isSupportDownloadRCCReportChanged && (
                                    <div className="ra-general-panel">
                                        <div
                                            className="ra-general-panel-content"
                                            role="alert"
                                            aria-live="assertive"
                                        >
                                            <span className="ra-general-panel-warn">
                                                {RMResx.RM_JS_SPS_Warning}
                                            </span>
                                            <span className="ra-general-panel-font">
                                                {" "}
                                                {
                                                    RMResx.RM_JS_FS_Warning_DisableDownloadRCCReport
                                                }
                                            </span>
                                        </div>
                                    </div>
                                )
                            }
                        </div>
                    )
                }
                {generalSetting && isShowEnableLifecycle && this.state.enableClassification ==
                        EnableRecordManagementSetting.Enable && (
                        <div className="ra-crm-form-content">
                            <div
                                id="enableLifecycleManagement"
                                className="ra-setting-panel-title"
                            >
                                {RMResx.RM_JS_FS_Radio_ApplyILSetting}
                            </div>
                            <R.Radio.Group
                                aria={{ "aria-labelledby": "enableLifecycleManagement" }}
                                name="isShowEnableLifecycle"
                                items={this.state.radioShowEnableLifecycle}
                                onChange={this.onShowEnableLifecycleChanged}
                            />
                            {this.state.isShowEnableLifecycle &&
                                this.state.isShowEnableLifecycleChanged && (
                                    <div className="ra-general-panel">
                                        <div
                                            className="ra-general-panel-content"
                                            role="alert"
                                            aria-live="assertive"
                                        >
                                            <span className="ra-general-panel-warn">
                                                {RMResx.RM_JS_SPS_Warning}
                                            </span>
                                            <span className="ra-general-panel-font">
                                                {" "}
                                                {
                                                    RMResx.RM_JS_FS_Warning_EnableApplyILSetting
                                                }
                                            </span>
                                        </div>
                                    </div>
                                )
                            }
                            {!this.state.isShowEnableLifecycle &&
                                this.state.isShowEnableLifecycleChanged && (
                                    <div className="ra-general-panel">
                                        <div
                                            className="ra-general-panel-content"
                                            role="alert"
                                            aria-live="assertive"
                                        >
                                            <span className="ra-general-panel-warn">
                                                {RMResx.RM_JS_SPS_Warning}
                                            </span>
                                            <span className="ra-general-panel-font">
                                                {" "}
                                                 {
                                                    RMResx.RM_JS_FS_Warning_DisableApplyILSetting
                                                 }
                                            </span>
                                        </div>
                                    </div>
                                )
                            }
                        </div>
                    )
                }
            </div>
        );
    }
}