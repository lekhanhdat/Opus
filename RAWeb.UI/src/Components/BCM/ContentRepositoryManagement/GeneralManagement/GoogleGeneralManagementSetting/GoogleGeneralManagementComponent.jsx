import "../../../../../Less/BCM/ContentRepositoryManagement/generalManagementSetting.less";
import { showToast } from "../../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import GoogleGeneralManagementPanel, { EnableRecordManagementSetting } from "./GoogleGeneralManagementPanel";

export default class GoogleGeneralManagementComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            generalSettingInfo: {},
            isShowGeneralSettingsPanel: { show: false },
        };
        this.generalManagementSetting = "generalManagementSettingPanel";
    }

    initData(args) {
        this.setState({ generalSettingInfo: args });
    }

    showGeneralSettingsClick = () => {
        if (
            !(this.props.checkMissingConfig && this.props.checkMissingConfig())
        ) {
            this.setState({ isShowGeneralSettingsPanel: { show: true } });
        }
    };

    saveGeneralSettings = () => {
        let callback = (result, reload) => {
            $$.loading(false);
            if (this.props.context.showGeneralToast) {
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            }
            this.props.refreshNodeSettings(reload);
            if (result) {
                return result;
            }
        };
        let back = this.refGeneralManagementPanel.onSave(callback);
        if (back) {
            this.setState({ isShowGeneralSettingsPanel: { show: false } });
        }
        return false;
    };

    cancelGeneralSettings = () => {
        this.setState({ isShowGeneralSettingsPanel: { show: false } });
    };

    renderGeneralSettingExpander = () => {
        const generalSetting = this.state.generalSettingInfo;
        const { context, disabled } = this.props;

        return (
            <R.Expander status={false} groupName="title">
                <div className="ra-crm-expander">
                    <div className="ra-expander-fontStyle">
                        {RMResx.RM_JS_SPS_EditTitle_GeneralManagement}
                    </div>
                    {!disabled &&
                        generalSetting.EnableRecordManagement !=
                            EnableRecordManagementSetting.ParentDisable &&
                        !CRMCommonUtil.isFolder(generalSetting) && (
                            <R.Scope>
                                <R.Button
                                    id="raCrmGeneralEditBtn"
                                    type="bald"
                                    icon="fia-edit"
                                    title={
                                        RMResx.RM_JS_SPS_EditTitle_GeneralManagement
                                    }
                                    tooltip={
                                        RMResx.RM_JS_SPS_Settings_EditSettings
                                    }
                                    onClick={this.showGeneralSettingsClick}
                                />
                            </R.Scope>
                        )}
                </div>

                <div>
                    {generalSetting && (
                        <div>
                            <$g.DetailList>
                                <$g.DetailRow>
                                    <$g.DetailCell
                                        label={
                                            RMResx.RM_JS_SPS_EnableRecordsManagement
                                        }
                                    >
                                        <span tabIndex="0">
                                            {generalSetting.EnableRecordManagement ==
                                            EnableRecordManagementSetting.Enable
                                                ? RMResx.RM_JS_Common_Yes
                                                : RMResx.RM_JS_Common_No}
                                        </span>
                                    </$g.DetailCell>
                                </$g.DetailRow>
                                {generalSetting.EnableRecordManagement ==
                                    EnableRecordManagementSetting.Enable &&
                                    context.supportSync(generalSetting) && (
                                        <$g.DetailRow>
                                            <$g.DetailCell
                                                label={
                                                    RMResx.RM_JS_SPS_EnableDataSync
                                                }
                                            >
                                                <span tabIndex="0">
                                                    {generalSetting.IsSyncData
                                                        ? RMResx.RM_JS_Common_Yes
                                                        : RMResx.RM_JS_Common_No}
                                                </span>
                                            </$g.DetailCell>
                                        </$g.DetailRow>
                                    )}
                                {generalSetting.EnableRecordManagement ==
                                    EnableRecordManagementSetting.Enable &&
                                    context.supperDisplayUniqueId(
                                        generalSetting
                                    ) && (
                                        <$g.DetailRow>
                                            <$g.DetailCell
                                                label={
                                                    RMResx.RM_JS_SPS_OneDriveDisplayUniqueId
                                                }
                                            >
                                                <span tabIndex="0">
                                                    {generalSetting.IsShowUniqueId
                                                        ? RMResx.RM_JS_Common_Yes
                                                        : RMResx.RM_JS_Common_No}
                                                </span>
                                            </$g.DetailCell>
                                        </$g.DetailRow>
                                    )}
                            </$g.DetailList>
                        </div>
                    )}
                </div>
            </R.Expander>
        );
    };

    renderGeneralSettingPanel = () => {
        return (
            <R.Panel
                header={RMResx.RM_JS_SPS_EditSetting}
                size={670}
                status={this.state.isShowGeneralSettingsPanel}
                destroy={true}
                hasClose={true}
                position={"right"}
                backdropHide={true}
            >
                <div className="br" slot="header">
                    <span className="ra-setting-panel-header">{RMResx.RM_JS_SPS_EditTitle_GeneralManagement}</span>
                </div>
                <GoogleGeneralManagementPanel
                    context={this.props.context}
                    id={this.generalManagementSetting}
                    ref={(r) => (this.refGeneralManagementPanel = r)}
                    data={this.state.generalSettingInfo}
                />
                  <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelGeneralSettings} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveGeneralSettings} />
                </>
            </R.Panel>
        );
    };

    render() {
        return (
            <div id={this.props.id}>
                {this.renderGeneralSettingExpander()}
                {this.renderGeneralSettingPanel()}
            </div>
        );
    }
}
