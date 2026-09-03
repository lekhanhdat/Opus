import { SourceFlags } from "../../../../Constants/Constants";
import { IntRuleLevel, NodeLevel } from "../../../../Constants/DAEnums";
import Enviroments from "../../../../Constants/Enviroments";
import { showToast } from "../../../../Utilities/CommonUtil";
import { RuleModuleTypes } from "../../../Common/RuleItem/Components/Constants";
import { RuleLevel } from "../AutoPopulate/Constants/Constants";
import RuleSettingComponent from "../RuleSetting/RuleSettingComponent";

export default class ArchiveSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        let currNode = this.props.data;
        this.state = {
            isWorkflowChecked: currNode.IsWorkflowDefinition,
            isIncludeManagedChecked: currNode.IsManagedMetadataService,
            isSuperUserChecked: currNode.IsEnableSuperUserDecrypt,
            isRemoveRetentionLabelChecked: currNode.IsEnableRemoveRetentionLabel,
            isSupportLockedSite: currNode.SupportLockedSite,
            isIncludeArchivedTeamsChecked: currNode.SupportArchivedTeams,
            showOrphanedNote: false,
        };
        this.RuleSettingComponent = null;
        this.addedRules = currNode.Rules || [];
        this.supportingLockedSCOption = [NodeLevel.WebApplication, NodeLevel.SiteCollection, NodeLevel.Office365GroupEntire];
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.save(args);
                break;
        }
    }

    save(callback) {
        let { isValid, trList } = this.RuleSettingComponent.getTermRules();
        this.addedRules = trList;
        if (!$$.verify(this.refSelectRuleValid.ref.current)) {
            return false;
        }

        //SiteCollection:100, Site:200, List:300, Folder:400
        let ruleLevelMapping = {
            100: [IntRuleLevel.Teams],
            200: [IntRuleLevel.Teams, IntRuleLevel.SiteCollection],
            300: [IntRuleLevel.Teams, IntRuleLevel.SiteCollection, IntRuleLevel.Site],
            400: [IntRuleLevel.Teams, IntRuleLevel.SiteCollection, IntRuleLevel.Site, IntRuleLevel.List],
        };
        if (this.addedRules.find(r => ruleLevelMapping[this.props.data.Level] && ruleLevelMapping[this.props.data.Level].indexOf(r.IntRuleLevel) > -1)) {
            showToast.error(RMResx.RM_AR_SPS_ArchiverSetting_SaveError);
            return false;
        } else {
            if (this.addedRules.length != 0) {
                this.props.context.saveArchiveSetting(this, callback);
            } else {
                this.showRemoveAllRulesMessageBox(() => { this.props.context.saveArchiveSetting(this, callback); });
            }
        }
    }

    showRemoveAllRulesMessageBox(onCliekOKFunc) {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div className="margin-bottom-m">{RMResx.RM_AR_SPS_Options_Warning_RemoveAllRules}</div>
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmArchivingRemoveAllRulesDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: onCliekOKFunc
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    validMissingRule = () => {
        this.addedRules = this.RuleSettingComponent.getTermRules();
        if (!$$.verify(this.refSelectRuleValid.ref.current)) {
            return false;
        }
    }

    onWorkflowChanged = (args) => {
        this.setState({ isWorkflowChecked: args });
    }

    onIncludeManagedChanged = (args) => {
        this.setState({ isIncludeManagedChecked: args });
    }

    onSuperUserChanged = (args) => {
        this.setState({ isSuperUserChecked: args });
    }

    onRemoveRetentionLabelChanged = (args) => {
        this.setState({ isRemoveRetentionLabelChecked: args });
    }

    onSupportLockedSiteChanged = (args) => {
        this.setState({ isSupportLockedSite: args });
    }

    onIncludeArchivedTeamsChanged = (args) => {
        this.setState({ isIncludeArchivedTeamsChecked: args });
    }

    selectRuleValid = () => {
        let checkLevelHasRule = this.RuleSettingComponent.termRulesAllHasRule();
        return checkLevelHasRule ? true : RMResx.RM_AR_SPS_SelRulesError;
    }

    onCheckOrphanedMessagebar = (termRulesGroup) => {
        let isShow = this.props.sourceFlag === SourceFlags.OneDrive && Object.keys(termRulesGroup).length > 0 && termRulesGroup[RuleLevel.SiteCollection].length > 0;
        this.setState({ showOrphanedNote: isShow });
    }

    render() {
        return (
            <div id={this.props.id}>
                <R.Validation>
                    <div ref={(r) => (this.allValidation = r)}>
                        <div className="ra-archive-content">
                            {this.props.context.configurations
                                .showConfigureWarn && (
                                <div className="ra-archive-messagebar">
                                    <R.Messagebar
                                        message={
                                            RMResx.RM_AR_SPS_Rule_ConfigureWarn
                                        }
                                        classify="warn"
                                        hasClose={false}
                                        status={{ show: true }}
                                    />
                                </div>
                            )}
                            <div className="ra-archive-messagebar">
                                <R.Messagebar
                                    message={
                                        RMResx.RM_AR_SPS_Rule_OrphanOneDriveNote
                                    }
                                    classify="info"
                                    hasClose={false}
                                    status={{
                                        show: this.state.showOrphanedNote,
                                    }}
                                />
                            </div>
                            <RuleSettingComponent
                                ref={(r) => (this.RuleSettingComponent = r)}
                                context={this.props.context}
                                currentNode={this.props.data}
                                availableRules={this.props.availableRules}
                                refreshRules={this.props.refreshRules}
                                createRuleComponentType={
                                    this.props.context.configurations
                                        .createRuleComponentType
                                }
                                nodeLevel={this.props.nodeLevel}
                                validRule={this.validMissingRule}
                                moduleType={RuleModuleTypes.SOArchiver}
                                sourceFlag={this.props.sourceFlag}
                                checkOrphanedMessagebar={
                                    this.onCheckOrphanedMessagebar
                                }
                            />
                            <div className="margin-top-s">
                                <R.ValidationFaker
                                    valid={this.selectRuleValid}
                                    ref={(r) => (this.refSelectRuleValid = r)}
                                />
                            </div>
                        </div>
                    </div>
                </R.Validation>
                <div>
                    <div className="ra-archive-title">
                        {RMResx.RM_AR_SPS_Title_Options}
                    </div>
                    {/* temporarily hide workflow option */}
                    {/* <div className="ra-archive-checkbox">
                    <R.Checkbox
                        id="raWorkflowChk"
                        text={RMResx.RM_AR_SPS_Options_Workflow}
                        checked={this.state.isWorkflowChecked}
                        onChange={this.onWorkflowChanged}
                    />
                </div> */}
                    <div>
                        <R.Checkbox
                            id="raIncludeManagedChk"
                            text={RMResx.RM_AR_SPS_Options_Managed}
                            checked={this.state.isIncludeManagedChecked}
                            onChange={this.onIncludeManagedChanged}
                        />
                        <$g.Popover>
                            {RMResx.RM_AR_SPS_Options_TermStoreDes}
                        </$g.Popover>
                    </div>
                    <div>
                        <R.Checkbox
                            id="raSuperUserChk"
                            text={RMResx.RM_AR_SPS_Options_SuperUser}
                            checked={this.state.isSuperUserChecked}
                            onChange={this.onSuperUserChanged}
                        />
                        <$g.Popover>
                            {RMResx.RM_AR_SPS_Options_SuperUserDes}
                        </$g.Popover>
                    </div>
                    {RM.gData.enviromentName != Enviroments.ChinaNorth && (
                        <div>
                            <R.Checkbox
                                id="raSuperUserChk"
                                text={
                                    RMResx.RM_AR_SPS_Options_Remove_RetentionLabel
                                }
                                checked={
                                    this.state.isRemoveRetentionLabelChecked
                                }
                                onChange={this.onRemoveRetentionLabelChanged}
                            />
                            <$g.Popover>
                                {RMResx.RM_AR_SPS_Options_RemoveRetentionLabel}
                            </$g.Popover>
                        </div>
                    )}
                    {this.supportingLockedSCOption.includes(this.props.data.Level) && this.props.context.supportUnlockSite && (
                        <div>
                            <R.Checkbox
                                id="raSuperUserChk"
                                text={
                                    RMResx.RM_AR_SPS_Options_SupportLockedSite
                                }
                                checked={this.state.isSupportLockedSite}
                                onChange={this.onSupportLockedSiteChanged}
                            />
                            <$g.Popover>
                                {
                                    RMResx.RM_AR_SPS_Options_IncludeLockedSiteCollection
                                }
                            </$g.Popover>
                        </div>
                    )}
                    {this.props.sourceFlag === SourceFlags.Teams && (
                        <div>
                            <R.Checkbox
                                id="raIncludeArchivedTeamsChk"
                                text={RMResx.RM_AR_SPS_Options_SupportArchivedTeams}
                                checked={this.state.isIncludeArchivedTeamsChecked}
                                onChange={this.onIncludeArchivedTeamsChanged}
                            />
                            <$g.Popover>
                                {RMResx.RM_AR_SPS_Options_SupportArchivedTeamsDesc}
                            </$g.Popover>
                        </div>
                    )}
                </div>
            </div>
        );
    }
}