import ManualApprovalSettingPanel, { SelectProcessType } from "./ManualApprovalSettingPanel";
import "../../../../Less/BCM/ContentRepositoryManagement/manualApprovalSetting.less";
import StringUtil from "../../../../Utilities/StringUtil";
import { showToast } from "../../../../Utilities/CommonUtil";

export default class ManualApprovalSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            isShowManualApprovalSettingsPanel: { show: false },
            users: null,
            manualApprovalSettingInfo: {},
        };
        this.manualApprovalSetting = "manualApprovalSettingPanel";
    }

    componentReceive(type, args) {
        switch (type) {
            case "manualApprovalData":
                this.node = args;
                this.setState({ manualApprovalSettingInfo: args });
                break;
        }
    }

    showManualApprovalSettingsClick = (e) => {
        if (!(this.props.checkMissingConfig && this.props.checkMissingConfig())) {
            this.setState({ isShowManualApprovalSettingsPanel: { show: true } });
        }
    };

    saveManualApprovalSettings = (e) => {
        this.dispatch(this.manualApprovalSetting, 'onSave', (success, data) => {
            this.props.refreshNodeSettings();
            showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            this.setState({ isShowManualApprovalSettingsPanel: { show: false } });
        });
        return false;
    }

    cancelManualApprovalSettings = () => {
        this.setState({ isShowManualApprovalSettingsPanel: { show: false } });
    }

    onPeopleChanged = (args) => {
        this.setState({ users: args });
    }

    renderUserSetting() {
        let recordOwner = this.state.manualApprovalSettingInfo.RecordOwner;
        let newRecordOwner = [];
        if (recordOwner) {
            recordOwner.forEach(user => {
                newRecordOwner.push({
                    tooltip: user.UserPrincipalName,
                    name: user.DisplayName,
                    id: user.UserId
                });
            });
        }
        return newRecordOwner;
    }

    render() {
        let manualApprovalSettingInfo = this.state.manualApprovalSettingInfo;
        let userSetting = this.renderUserSetting();
        return <div id={this.props.id}>
            <R.Expander
                status={false}
                groupName="title">
                <div className="ra-crm-expander">
                    <div data-tooltip="ifneed" className="ra-expander-fontStyle">{RMResx.RM_BCM_ManualApproval_Title_ManualApprovalSettings}</div>
                    {!this.props.disabled && (manualApprovalSettingInfo.IsTopLevelSetting || this.props.context.showManualApprovalPen) && <R.Scope>
                        <R.Button
                            id="raCrmManualApproveEditBtn"
                            type="bald"
                            icon="fia-edit"
                            title={RMResx.RM_BCM_ManualApproval_Title_ManualApprovalSettings}
                            tooltip={RMResx.RM_JS_SPS_Settings_EditSettings}
                            onClick={this.showManualApprovalSettingsClick} />
                    </R.Scope>}
                </div>

                <div>
                    {manualApprovalSettingInfo && <div>
                        <$g.DetailList>
                            {manualApprovalSettingInfo.ApprovalType == SelectProcessType.SelectNoneApprovalType && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_BCM_ManualApproval_Title_EnableApproval}>
                                    <span tabIndex="0">{RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>}
                            {(manualApprovalSettingInfo.ApprovalType == SelectProcessType.SelectApprovalProcess ||
                                manualApprovalSettingInfo.ApprovalType == SelectProcessType.SelectOwnerRecords) &&
                                <$g.DetailRow>
                                    <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_MA_IsSendEmail)}>
                                        <span tabIndex="0">{manualApprovalSettingInfo.EMailToRecordOwner ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                    </$g.DetailCell>
                                </$g.DetailRow>}
                            {manualApprovalSettingInfo.ApprovalType == SelectProcessType.SelectApprovalProcess && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_BCM_ManualApproval_Title_Process}>
                                    <span tabIndex="0">{manualApprovalSettingInfo.WorkflowReferenceName}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>}
                            {manualApprovalSettingInfo.ApprovalType == SelectProcessType.SelectOwnerRecords && <$g.DetailRow>
                                <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_SPS_RecordOwners)}>
                                    {userSetting.map((item) => {
                                        return <span key={item.id} className="ra-setting-profile" data-tooltip aria-label={item.tooltip} tabIndex="0">
                                            <R.Profile
                                                tooltip={item.tooltip}
                                                name={item.name}
                                                invalid="false">
                                            </R.Profile>
                                        </span>; 
                                    })}
                                </$g.DetailCell>
                            </$g.DetailRow>}
                            {manualApprovalSettingInfo.ApprovalType == SelectProcessType.SelectAutoApprove && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_BCM_ManualApproval_Detail_AutoApprove}>
                                    <span tabIndex="0">{RMResx.RM_JS_Common_Yes}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>}
                        </$g.DetailList>
                    </div>}
                </div>
            </R.Expander>

            <R.Panel
                header={RMResx.RM_JS_SPS_EditSetting}
                size={670}
                status={this.state.isShowManualApprovalSettingsPanel}
                destroy={true}
            >
                <div className="br" slot="header">
                    <span className="ra-setting-panel-header">{RMResx.RM_BCM_ManualApproval_Title_ManualApprovalSettings}</span>
                </div>
                <ManualApprovalSettingPanel
                    context={this.props.context}
                    id={this.manualApprovalSetting}
                    data={this.node}
                    selectionChanged={this.onPeopleChanged}
                ></ManualApprovalSettingPanel>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelManualApprovalSettings} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveManualApprovalSettings} />
                </>
            </R.Panel>
        </div>;
    }
}