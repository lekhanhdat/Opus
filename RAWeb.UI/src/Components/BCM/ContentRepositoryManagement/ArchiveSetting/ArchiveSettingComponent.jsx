import "../../../../Less/BCM/ContentRepositoryManagement/archiveSetting.less";
import ArchiveSettingPanel from "./ArchiveSettingPanel";
import { TabIndex } from '../CRMForSPO';
import { SourceFlags } from "../../../../Constants/Constants";
import { NodeLevel } from "../../../../Constants/DAEnums";

export default class ArchiveSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            nodeSettingInfo: {},
            isShowArchiveSettingPanel: { show: false },
        };
        this.archiveSetting = "archiveSettingPanel";
    }

    componentReceive(type, args) {
        switch (type) {
            case "init":
                this.node = args;
                this.setState({ nodeSettingInfo: args });
                break;
        }
    }

    getSettingRuleNames = () => {
        const associatedRules = this.state.nodeSettingInfo?.Rules?.filter(o => this.props.availableRules.some(p => p.RuleId == o.RuleId));
        if(associatedRules && associatedRules.length)
        {
            return (
                <div className="ra-setting-ruleNames-container">
                    {associatedRules?.map((o, index)=> <div key={index}>{`${index + 1}. ${o.RuleName}`}</div>)}
                </div>
            );
        }
    }

    getOptions = () => {
        let settings = this.state.nodeSettingInfo;
        const supportingLockedSCOption = [NodeLevel.WebApplication, NodeLevel.SiteCollection, NodeLevel.Office365GroupEntire];
        if (Object.keys(settings).length > 0) {
            let optionList = [];
            // if (this.state.nodeSettingInfo.IsWorkflowDefinition && this.props.mode != TabIndex.Archive) {
            //     optionList.push({ "name": RMResx.RM_AR_SPS_Options_Workflow });
            // }
            if (this.state.nodeSettingInfo.IsManagedMetadataService) {
                optionList.push({ "name": RMResx.RM_AR_SPS_Options_Managed });
            }
            if (this.state.nodeSettingInfo.IsEnableSuperUserDecrypt) {
                optionList.push({ "name": RMResx.RM_AR_SPS_Options_SuperUser });
            }
            if (this.state.nodeSettingInfo.IsEnableRemoveRetentionLabel) {
                optionList.push({ "name": RMResx.RM_AR_SPS_Options_Remove_RetentionLabel });
            }
            if (supportingLockedSCOption.includes(this.state.nodeSettingInfo.Level) && this.state.nodeSettingInfo.SupportLockedSite) {
                optionList.push({
                    name: RMResx.RM_AR_SPS_Options_SupportLockedSite,
                });
            }
            if (this.state.nodeSettingInfo.SupportArchivedTeams) {
                optionList.push({ "name": RMResx.RM_AR_SPS_Options_SupportArchivedTeams });
            }
            return optionList.map(op => op.name).join("; ");
        }
    }

    checkRunningJob = () => {
        let url = "/api/SPSettingApi/CheckRemoteNodeHaveRunningJob";
        if (this.props.sourceFlag == SourceFlags.Teams) {
            url = "/api/TeamsSettingApi/CheckRemoteNodeHaveRunningJob"
        }
        $$.loading(true);
        let option = {
            url,
            method: "Post",
            data: this.node,
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res) {
                this.showMessagebox();
            } else {
                this.saveArchiveSetting();
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    showMessagebox = () => {
        let args = {
            width: "550px",
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_SPS_Settings_RunningJobMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raRunningJob",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.saveArchiveSetting
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    showArchiveSettingClick = (e) => {
        this.setState({ isShowArchiveSettingPanel: { show: true } });
    }

    saveArchiveSetting = () => {
        $$.messagedialog(false);
        this.dispatch(this.archiveSetting, 'onSave', (success, data) => {
            this.props.refreshNodeSettings();
            this.setState({ isShowArchiveSettingPanel: { show: false } });
        });
        return false;
    }

    cancelArchiveSetting = () => {
        this.setState({ isShowArchiveSettingPanel: { show: false } });
    }

    renderArchiveSettingPanel() {
        return <R.Panel
            header={RMResx.RM_JS_SPS_EditSetting}
            size={670}
            status={this.state.isShowArchiveSettingPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="ra-setting-panel-header">{RMResx.RM_AR_SPS_EditTitle_ArchiveSetting}</span>
            </div>
            <ArchiveSettingPanel
                context={this.props.context}
                id={this.archiveSetting}
                data={this.node ? RM.deepcopy(this.node) : this.node}
                availableRules={this.props.availableRules}
                sourceFlag={this.props.sourceFlag}
                refreshRules={this.props.refreshRules}
                nodeLevel={this.props.nodeLevel}
            />
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelArchiveSetting} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.checkRunningJob} />
            </>
        </R.Panel>;
    }

    render() {
        let archiveSetting = this.state.nodeSettingInfo;
        return <div id={this.props.id}>
            <R.Expander
                status={false}
                groupName="title">
                <div className="ra-crm-expander">
                    <div className="ra-expander-fontStyle">{RMResx.RM_AR_SPS_EditTitle_ArchiveSetting}</div>
                    <R.Scope>
                        <R.Button
                            id="raCrmArchiveEditBtn"
                            type="bald"
                            icon="fia-edit"
                            title={RMResx.RM_AR_SPS_EditTitle_ArchiveSetting}
                            tooltip={RMResx.RM_JS_SPS_Settings_EditSettings}
                            onClick={this.showArchiveSettingClick} />
                    </R.Scope>
                </div>
                <div>
                    {archiveSetting && <div>
                        <$g.DetailList>
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_SPS_RuleNames_Title}>
                                    <span tabIndex="0">{this.getSettingRuleNames()}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_AR_SPS_Title_Options}>
                                    <span tabIndex="0">{this.getOptions()}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        </$g.DetailList>
                    </div>}
                </div>
            </R.Expander>
            {this.renderArchiveSettingPanel()}
        </div>;
    }
}