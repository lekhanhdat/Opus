import { NodeLevel } from "../../../../Constants/DAEnums";
import RouterUrls from "../../../../Constants/RouterUrls";
import "../../../../Less/BCM/ContentRepositoryManagement/generalManagementSetting.less";
import { showToast } from "../../../../Utilities/CommonUtil";
import { checkPermission } from "../../../../Utilities/permissionManager";
import { EnableRecordManagementSetting } from "../CRMForSPO/ArchiveCRMForSPO";
import GeneralSettingPanel from "./GeneralSettingPanel";

export default class GeneralSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            generalSettingInfo: {},
            isShowGeneralSettingPanel: { show: false },
        };
        this.generalSetting = "archiveGeneralSettingPanel";
    }

    componentReceive(type, args) {
        switch (type) {
            case "initGeneralSetting":
                this.node = args;
                this.setState({ generalSettingInfo: args });
                break;
        }
    }

    showGeneralSettingClick = (e) => {
        this.setState({ isShowGeneralSettingPanel: { show: true } });
    }

    saveGeneralSetting = (e) => {
        let callback = (result, reload) => {
            $$.loading(false);

            showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            this.props.refreshNodeSettings(reload);
            if (result) {
                return result;
            }
        };
        let back = this.refGeneralSettingPanel.onSave(callback);
        if (back) {
            this.setState({ isShowGeneralSettingPanel: { show: false } });
        }
        return false;
    }

    cancelGeneralSetting = () => {
        this.setState({ isShowGeneralSettingPanel: { show: false } });
    }

    renderGeneralSettingPanel() {
        return <R.Panel
            header={RMResx.RM_JS_SPS_EditSetting}
            size={670}
            status={this.state.isShowGeneralSettingPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="ra-setting-panel-header">{RMResx.RM_JS_SPS_EditTitle_GeneralManagement}</span>
            </div>
            <GeneralSettingPanel
                context={this.props.context}
                id={this.generalSetting}
                ref={r => this.refGeneralSettingPanel = r}
                data={this.node}
            ></GeneralSettingPanel>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelGeneralSetting} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveGeneralSetting} />
            </>
        </R.Panel>;
    }

    render() {
        let generalSetting = this.state.generalSettingInfo;
        let supportDelArchivedData = (RM.gData.enableDeleteRestoredDataFeature && checkPermission(RouterUrls.CP_Index, RM.UserResources) && (generalSetting.Level === NodeLevel.WebApplication || generalSetting.Level === NodeLevel.SiteCollection || generalSetting.Level === NodeLevel.Office365GroupEntire));
        return <div id={this.props.id}>
            <R.Expander
                status={false}
                groupName="title">
                <div className="ra-crm-expander">
                    <div className="ra-expander-fontStyle">{RMResx.RM_JS_SPS_EditTitle_GeneralManagement}</div>
                    <R.Scope>
                        <R.Button
                            id="raCrmGeneralEditBtn"
                            type="bald"
                            icon="fia-edit"
                            title={RMResx.RM_JS_SPS_EditTitle_GeneralManagement}
                            tooltip={RMResx.RM_JS_SPS_Settings_EditSettings}
                            onClick={this.showGeneralSettingClick} />
                    </R.Scope>
                </div>
                <div>
                    {generalSetting && <div>
                        <$g.DetailList>
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_AR_SPS_General_EnableArchiveManagement}>
                                    <span tabIndex="0">{generalSetting.EnableArchiverManagement == EnableRecordManagementSetting.Enable ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            {supportDelArchivedData && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_AR_SPS_General_EnableRestoreManagement}>
                                    <span tabIndex="0">{generalSetting.EnableDelArchivedData ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>}
                        </$g.DetailList>
                    </div>}
                </div>
            </R.Expander>
            {this.renderGeneralSettingPanel()}
        </div>;
    }
}