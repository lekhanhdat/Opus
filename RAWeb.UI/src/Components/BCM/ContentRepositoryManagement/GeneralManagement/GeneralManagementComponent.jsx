import GeneralManagementPanel from "./GeneralManagementPanel";
import "../../../../Less/BCM/ContentRepositoryManagement/generalManagementSetting.less";
import { EnableRecordManagementSetting } from "../CRMForSPO/ContentRepositoryManagementForSPO";
import { LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../Common/CRMCommonUtil";
import { NodeLevel } from "../../../../Constants/DAEnums";
import { SourceFlags } from "../../../../Constants/Constants";

export default class GeneralManagementComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            generalSettingInfo: {},
            isShowGeneralSettingsPanel: { show: false },
        };
        this.generalManagementSetting = "generalManagementSettingPanel";
        this.supportingLockedSCOption = [NodeLevel.WebApplication, NodeLevel.SiteCollection, NodeLevel.Office365GroupEntire];
    }

    initData(args) {
        this.setState({ generalSettingInfo: args });
    }

    showGeneralSettingsClick = (e) => {
        if (!(this.props.checkMissingConfig && this.props.checkMissingConfig())) {
            this.setState({ isShowGeneralSettingsPanel: { show: true } });
        }
    }

    saveGeneralSettings = (e) => {
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
    }

    cancelGeneralSettings = () => {
        this.setState({ isShowGeneralSettingsPanel: { show: false } });
    }

    render() {
        let generalSetting = this.state.generalSettingInfo;
        const isShowEnableLifecycle = (this.props.sourceFlag === SourceFlags.Teams || this.props.sourceFlag === SourceFlags.SP) && LicenseHelper.EnableRecordsArchiver() && this.supportingLockedSCOption.includes(generalSetting.Level);
        return (
            <div id={this.props.id}>
                <R.Expander status={false} groupName="title">
                    <div className="ra-crm-expander">
                        <div className="ra-expander-fontStyle">
                            {RMResx.RM_JS_SPS_EditTitle_GeneralManagement}
                        </div>
                        {!this.props.disabled &&
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
                                        this.props.context.supportSync(
                                            generalSetting,
                                        ) && (
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
                                        this.props.context.supperDisplayUniqueId(
                                            generalSetting,
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
                                    {generalSetting.EnableRecordManagement ==
                                        EnableRecordManagementSetting.Enable &&
                                        this.supportingLockedSCOption.includes(generalSetting.Level) &&
                                        this.props.context
                                            .supportUnlockSite && (
                                            <$g.DetailRow>
                                                <$g.DetailCell
                                                    label={
                                                        RMResx.RM_JS_SPS_SupportLockedSite
                                                    }
                                                >
                                                    <span tabIndex="0">
                                                        {generalSetting.SupportLockedSite
                                                            ? RMResx.RM_JS_Common_Yes
                                                            : RMResx.RM_JS_Common_No}
                                                    </span>
                                                </$g.DetailCell>
                                            </$g.DetailRow>
                                        )}
                                    {generalSetting.EnableRecordManagement == EnableRecordManagementSetting.Enable 
                                        && this.props.context.supportDownloadRCCReport &&(
                                            <$g.DetailRow>
                                                <$g.DetailCell
                                                    label={
                                                        RMResx.RM_JS_FS_DownloadRCCReport
                                                    }
                                                >
                                                    <span tabIndex="0">
                                                        {generalSetting.IsAllowUserDownloadRCCReport
                                                            ? RMResx.RM_JS_Common_Yes
                                                            : RMResx.RM_JS_Common_No}
                                                    </span>
                                                </$g.DetailCell>
                                            </$g.DetailRow>
                                        )
                                    }
                                    {generalSetting.EnableRecordManagement ==
                                        EnableRecordManagementSetting.Enable && isShowEnableLifecycle && (
                                        <$g.DetailRow>
                                                <$g.DetailCell
                                                    label={
                                                        RMResx.RM_JS_SPS_EnableLifecycleManagementForSharePointLists
                                                    }
                                                >
                                                    <span tabIndex="0">
                                                        {(generalSetting.EnableLifecycleManagementForSharePointLists ?? true)
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

                <R.Panel
                    header={RMResx.RM_JS_SPS_EditSetting}
                    size={670}
                    status={this.state.isShowGeneralSettingsPanel}
                    destroy={true}
                >
                    <div className="br" slot="header">
                        <span className="ra-setting-panel-header">
                            {RMResx.RM_JS_SPS_EditTitle_GeneralManagement}
                        </span>
                    </div>
                    <GeneralManagementPanel
                        context={this.props.context}
                        id={this.generalManagementSetting}
                        ref={(r) => (this.refGeneralManagementPanel = r)}
                        data={this.state.generalSettingInfo}
                        sourceFlag={this.props.sourceFlag}
                    ></GeneralManagementPanel>
                    <>
                        <R.Button
                            slot="buttons"
                            text={RMResx.RM_JS_Common_Cancel}
                            onClick={this.cancelGeneralSettings}
                        />
                        <R.Button
                            slot="buttons"
                            primary
                            classify="theme"
                            text={RMResx.RM_JS_Common_Save}
                            onClick={this.saveGeneralSettings}
                        />
                    </>
                </R.Panel>
            </div>
        );
    }
}