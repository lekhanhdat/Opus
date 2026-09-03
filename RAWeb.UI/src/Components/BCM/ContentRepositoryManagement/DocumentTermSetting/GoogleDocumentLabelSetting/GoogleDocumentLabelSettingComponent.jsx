import GoogleDocumentLabelSettingPanel,{ ApplyExistType, ArtificialIntelligenceTermUseType, AutoJobOption, DeployLabelMethod } from "./GoogleDocumentLabelSettingPanel";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import StringUtil from "../../../../../Utilities/StringUtil";
import "../../../../../Less/BCM/ContentRepositoryManagement/documentTermSetting.less";
import "../../../../../Less/BCM/autoCriteria.less";
import { ClassificationSettingType } from "../../ClassificationSetting/ClassificationSettingPanel";

import Enviroments from "../../../../../Constants/Enviroments";
import { SelectProcessType } from "../../ManualApprovalSetting/ManualApprovalSettingPanel";

export default class GoogleDocumentLabelSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            isShowDocumentTermSettingsPanel: { show: false },
            nodeSettingInfo: {}
        };
        this.documentTermSetting = "documentTermSettingPanel";
    }

    componentReceive(type, args) {
        switch (type) {
            case "init":
                this.node = args;
                this.setState({ nodeSettingInfo: args });
                break;
        }
    }

    showDocumentTermSettingsClick = (e) => {
        if (!(this.props.checkMissingConfig && this.props.checkMissingConfig())) {
            this.setState({ isShowDocumentTermSettingsPanel: { show: true } });
        }
    };

    isDefaultTermConfigured() {
        return this.state.nodeSettingInfo.DeployLabelMethod == DeployLabelMethod.UseManualclassification;
    }

    isAutoClassificationConfigured() {
        return this.state.nodeSettingInfo.DeployLabelMethod == DeployLabelMethod.UseAutoClassification;
    }

    isAIClassificationConfigured() {
        return this.state.nodeSettingInfo.DeployLabelMethod == DeployLabelMethod.UseIntelligentClassification;
    }
    
    isAIClassificationConfiguredOrAiInAuto() {
        return this.isAIClassificationConfigured() || this.state.nodeSettingInfo.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault;
    }

    isAIApprovalTypeSelectOwner() {
        return this.state.nodeSettingInfo.AIApprovalType == SelectProcessType.SelectOwnerRecords;
    }

    hasConfigDocumentLevel() {
        return !CRMCommonUtil.guidIsEmpty(this.state.nodeSettingInfo.TermId) || !CRMCommonUtil.guidIsEmpty(this.state.nodeSettingInfo.TermSetId);
    }

    displayDeployLabelMethod() {
        var result = "";
        if (this.state.nodeSettingInfo.IconStatus !== 0) { // Not unique
            switch (this.state.nodeSettingInfo.DeployLabelMethod) {
                case DeployLabelMethod.UseManualclassification:
                    result = RMResx.RM_JS_SPS_Label_AutoClassification_NoDefaultValue;
                    break;
                case DeployLabelMethod.UseAutoClassification:
                    result = RMResx.RM_JS_SPS_Label_AutoClassification_UseRule;
                    break;
                case DeployLabelMethod.UseIntelligentClassification:
                    result = RMResx.RM_MachineLearning_DeployTermMethodIntelligence;
                    break;
                default:
                    break;
            }
        }
        return result;
    }

    applyActionString() {
        if (this.state.nodeSettingInfo.NeedCheckDefaultValue) {
            var includeString = this.state.nodeSettingInfo.IncludeDeclaredRecords ? "; " + RMResx.RM_JS_SPS_IncludeDeclaredRecords : "";
            if (this.state.nodeSettingInfo.ApplyExistType == ApplyExistType.OverWrite) {
                return RMResx.RM_JS_Common_Yes + "; " + RMResx.RM_JS_SPS_AutoClassification_ApplyOverwirteTerm + includeString;
            } else if (this.state.nodeSettingInfo.ApplyExistType == ApplyExistType.SkipAndKeep) {
                return RMResx.RM_JS_Common_Yes + "; " + RMResx.RM_JS_SPS_AutoClassification_ApplySkipTerm + includeString;
            }
        } else {
            return RMResx.RM_JS_Common_No;
        }
    }

    applyDocumentSetsAndFoldersStringForDefault() {
        if (this.state.nodeSettingInfo.ApplyTermIncludeFolder) {
            if (this.state.nodeSettingInfo.ApplyExistType == ApplyExistType.OverWrite) {
                return RMResx.RM_JS_Common_Yes + "; " + RMResx.RM_JS_SPS_AutoClassification_ApplySetsOverwirteTerm;
            } else if (this.state.nodeSettingInfo.ApplyExistType == ApplyExistType.SkipAndKeep) {
                return RMResx.RM_JS_Common_Yes + "; " + RMResx.RM_JS_SPS_AutoClassification_ApplySetsSkipTerm;
            }
        } else {
            return RMResx.RM_JS_Common_No;
        }
    } 

    applyDocumentSetsAndFoldersStringForAuto() {
        return this.state.nodeSettingInfo.ApplyTermIncludeFolder ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No;
    } 

    getSkipOverrideStr() {
        var result = "";
        switch (this.state.nodeSettingInfo.AutoJobOption) {
            case AutoJobOption.Skip:
                result = RMResx.RM_JS_SPS_AutoClassification_SkipOverrideOption_SkipLabel;
                break;
            case AutoJobOption.Override:
                result = RMResx.RM_JS_SPS_AutoClassification_SkipOverrideOption_OverrideLabel;
                break;
            case AutoJobOption.Append:
                result = RMResx.RM_JS_SPS_AutoClassification_AppendLabel;
                break;
            default:
                break;
        }
        return result;
    }

    saveDocumentTermSettings = () => {
        this.dispatch(this.documentTermSetting, 'onSave', (success, data) => {
            this.props.refreshNodeSettings();
            this.setState({ isShowDocumentTermSettingsPanel: { show: false } });
        });
        return false;
    }

    cancelDocumentTermSettings = () => {
        this.setState({ isShowDocumentTermSettingsPanel: { show: false } });
    }

    renderConditionsCriteria() {
        let defaultRule = null;
        return <div>
            {this.state.nodeSettingInfo.AutoClassificationRules.map(rule => {
                if (rule.IsDefaultRule) {
                    if (!rule.NoDefaultTerm) {
                        defaultRule = rule;
                    }
                } else {
                    let filters = this.getAllFilters(rule);
                    let displayAndOrStr = rule.AndOrExpression;
                    if (filters.length == 1) {
                        displayAndOrStr = "(" + displayAndOrStr + ")";
                    }
                    return <div key={StringUtil.newGuid()}>
                        {filters.map(f => {
                            return <div key={StringUtil.newGuid()} className="margin-top-xs">
                                {f.FilterCretia}
                            </div>;
                        })}
                        <div>
                            {displayAndOrStr}
                        </div>
                        <div className="margin-bottom-l">
                            <span>{`${RMResx.RM_JS_SPS_AutoClassification_DisplayPolicyApplyLabel} `}</span>
                            {(rule.TermIsRemoved || rule.TermIsDeprecated) && <span className="fia-status-error info-error-tab"></span>}
                            <span>{rule.TermName}</span>
                            {rule.TermIsRemoved &&
                                <span className="info-error-font">{RMResx.RM_JS_SPS_TermDelete}</span>}
                            {!rule.TermIsRemoved && rule.TermIsDeprecated &&
                                <span className="info-error-font">{RMResx.RM_JS_SPS_IsTermRetired}</span>}
                        </div>
                    </div>;
                }
            })}
            {defaultRule && <div key={StringUtil.newGuid()} className="margin-top-l">
                <span>{`${RMResx.RM_JS_SPS_AutoClassification_DisplayPolicyDefaultLabel} `}</span>
                {(defaultRule.TermIsRemoved || defaultRule.TermIsDeprecated) && <span className="fia-status-error info-error-tab"></span>}
                <span>{defaultRule.TermName}</span>
                {defaultRule.TermIsRemoved &&
                    <span className="info-error-font">{RMResx.RM_JS_SPS_TermDelete}</span>}
                {!defaultRule.TermIsRemoved && defaultRule.TermIsDeprecated &&
                    <span className="info-error-font">{RMResx.RM_JS_SPS_IsTermRetired}</span>}
            </div>}
        </div>;
    }

    renderUserSetting() {
        let aiOwner = this.state.nodeSettingInfo.AIReviewers;
        let newAIOwner = [];
        if (aiOwner) {
            aiOwner.forEach(user => {
                newAIOwner.push({
                    tooltip: user.UserPrincipalName,
                    name: user.DisplayName,
                    id: user.UserId
                });
            });
        }
        return newAIOwner;
    }

    getAllFilters(rule) {
        let resultFilters = [];
        this.getFiltersInGroup(resultFilters, rule.FilterGroups);
        return resultFilters;
    }

    getFiltersInGroup(resultFilters, filterGroups) {
        filterGroups.forEach(filterGroup => {
            resultFilters.push(...filterGroup.Filters);
            this.getFiltersInGroup(resultFilters, filterGroup.FilterGroups);
        });
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

    getClassificationData = () => {
        if (this.props.context.configurations.existClassificationSetting) {
            return this.props.getClassificationData();
        } else {
            return ClassificationSettingType.None;
        }
    }

    render() {
        const showTermSettings = this.props.showTermSettings ?? true;
        let applyDocumentSetsAndFoldersString =
            this.isAutoClassificationConfigured()
                ? this.applyDocumentSetsAndFoldersStringForAuto()
                : this.applyDocumentSetsAndFoldersStringForDefault();

        return <div id={this.props.id}>
            <R.Expander
                status={false}
                groupName="title">
                <div className="ra-crm-expander">
                    <div className="ra-expander-fontStyle">{this.props.context.termSettingsTitle}</div>
                    {!this.props.disabled && <R.Scope>
                        <R.Button
                            id="raCrmDocumentLevelEditBtn"
                            type="bald"
                            icon="fia-edit"
                            title={RMResx.RM_SPS_DocumentLevel}
                            tooltip={RMResx.RM_JS_SPS_Settings_EditSettings}
                            onClick={this.showDocumentTermSettingsClick} />
                    </R.Scope>}
                </div>
                <div>
                    {this.state.nodeSettingInfo && !showTermSettings && <$g.DetailList>
                        <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_JS_SPS_EnableApplyTermSettingsTitle}>
                                <span tabIndex="0">{RMResx.RM_JS_Common_No}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_JS_SPS_RuleNames_Title}>
                                <span tabIndex="0">{this.getSettingRuleNames()}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    </$g.DetailList>}

                    {this.state.nodeSettingInfo && showTermSettings && <$g.DetailList>
                        {this.props.context.supportSettingWithoutClassification(this.state.nodeSettingInfo) && 
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_SPS_EnableApplyTermSettingsTitle}>
                                    <span tabIndex="0">{RMResx.RM_JS_Common_Yes}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        }
                        {this.props.context.configurations.termDisplayFormat && !CRMCommonUtil.isFolder(this.state.nodeSettingInfo) && <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_TermDisplayForm}>
                                {this.hasConfigDocumentLevel() &&
                                    <span tabIndex="0">{this.state.nodeSettingInfo.IsDisplyaTermPath ? RMResx.RM_SPS_DisplayTerm_EntirePath : RMResx.RM_SPS_DisplayTerm_TermLabel}</span>}
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        <$g.DetailRow>
                            <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_AutoClassification_DeployLabelMethod)}>
                                <span tabIndex="0">{this.displayDeployLabelMethod()}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        {/* {this.isDefaultTermConfigured() && <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_DefaultValue}>
                                <span tabIndex="0">
                                    {(this.state.nodeSettingInfo.IsDefaultTermDeprecated || this.state.nodeSettingInfo.IsDefaultTermRemoved) && <div className="info-error">
                                        <div className="info-error-icon"><span className="fia-status-error info-error-tab"></span></div>
                                    </div>}
                                    <div className="ra-setting-termPath">
                                        <span>{this.state.nodeSettingInfo.DefaultTermFullPath}</span>
                                    </div>
                                    {this.state.nodeSettingInfo.IsDefaultTermRemoved &&
                                        <span className="info-error-font">{RMResx.RM_JS_SPS_TermDelete}</span>}
                                    {!this.state.nodeSettingInfo.IsDefaultTermRemoved && this.state.nodeSettingInfo.IsDefaultTermDeprecated &&
                                        <span className="info-error-font">{RMResx.RM_JS_SPS_IsTermRetired}</span>}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>} */}
                        {/* {this.props.context.configurations.defaultTermApplyExist && this.isDefaultTermConfigured() && <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_Action}>
                                {this.hasConfigDocumentLevel() && <span tabIndex="0">{this.applyActionString()}</span>}
                            </$g.DetailCell>
                        </$g.DetailRow>} */}
                        {this.isAutoClassificationConfigured() && <$g.DetailRow>
                            <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_AutoClassification_ApplyPolicy)}>
                                <div tabIndex="0">{this.renderConditionsCriteria()}</div>
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        {this.isAIClassificationConfiguredOrAiInAuto() && this.isAIApprovalTypeSelectOwner() && <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_MachineLearning_IntelligenceReviewers}>
                                <div tabIndex="0">{this.renderUserSetting().map((item) => {
                                    return <span key={item.id} className="ra-setting-profile margin-left-s" data-tooltip aria-label={item.tooltip} tabIndex="0">
                                        <R.Profile
                                            tooltip={item.tooltip}
                                            name={item.name}
                                            invalid="false">
                                        </R.Profile>
                                    </span>;
                                })}</div>
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        {this.isAIClassificationConfiguredOrAiInAuto() && <$g.DetailRow>
                            <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_MA_IsSendEmail)}>
                                <div tabIndex="0">{this.state.nodeSettingInfo.AISendEMail ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</div>
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        {this.isAIClassificationConfiguredOrAiInAuto() && this.state.nodeSettingInfo.AIThenIsDefaultTermMethod && <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_MachineLearning_IntelligenceDefaultTerm}>
                                <div tabIndex="0">{this.state.nodeSettingInfo.AIThenDefaultTermName}</div>
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        {(this.isAutoClassificationConfigured() || this.isAIClassificationConfigured()) && <$g.DetailRow>
                            <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_SPS_AutoClassification_SkipOverrideOption)}>
                                <span tabIndex="0">{this.getSkipOverrideStr()}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        {(this.isAutoClassificationConfigured() || this.isAIClassificationConfigured()) && <$g.DetailRow>
                            <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_SPS_AutoClassification_FullJobDescription)}>
                                <span tabIndex="0">{this.state.nodeSettingInfo.RunAutoFullJob ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        {!this.props.isCSDTenant && this.props.context.configurations.applyDocumentSetsAndFolders && 
                            (this.isDefaultTermConfigured() || this.isAutoClassificationConfigured()) && <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_JS_SPS_Expander_IncludeDSetAndFolder}>
                                {(this.state.nodeSettingInfo.TermScopeFullPath != "") && (this.state.nodeSettingInfo.TermScopeFullPath != null) &&
                                    <span tabIndex="0">{applyDocumentSetsAndFoldersString}</span>
                                }
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        {this.props.context.configurations.includeDeclared && this.isAutoClassificationConfigured() && <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_IncludeDeclaredRecords}>
                                <span tabIndex="0">{this.state.nodeSettingInfo.IncludeDeclaredRecords ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>}
                        {/* {RM.gData.enviromentName !== Enviroments.ChinaNorth && this.props.context.configurations.enableRelatedRecords && (CRMCommonUtil.isSiteCollection(this.state.nodeSettingInfo) || CRMCommonUtil.isSite(this.state.nodeSettingInfo)) && <$g.DetailRow>
                            <$g.DetailCell label={RMResx.RM_SP_SettingRelatedRecords}>
                                {(this.state.nodeSettingInfo.TermScopeFullPath != "") && (this.state.nodeSettingInfo.TermScopeFullPath != null) &&
                                    <span tabIndex="0">{this.state.nodeSettingInfo.EnableRelatedRecords ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                }
                            </$g.DetailCell>
                        </$g.DetailRow>} */}
                    </$g.DetailList>}
                </div>
            </R.Expander>

            <R.Panel
                header={RMResx.RM_JS_SPS_EditSetting}
                size={670}
                status={this.state.isShowDocumentTermSettingsPanel}
                destroy={true}
                hasClose={true}
                position={"right"}
            >
                 <div className="br" slot="header">
                    <span className="ra-setting-panel-header">{this.props.context.termSettingsTitle}</span>
                </div>
                <GoogleDocumentLabelSettingPanel
                    context={this.props.context}
                    id={this.documentTermSetting}
                    data={this.node ? RM.deepcopy(this.node) : this.node}
                    sourceFlag={this.props.sourceFlag}
                    showTermSettings={showTermSettings}
                    availableRules={this.props.availableRules}
                    refreshRules={this.props.refreshRules}
                    isCSDTenant={this.props.isCSDTenant}
                    getClassificationData={this.getClassificationData}
                    lastAccessTimeCollection={this.props.lastAccessTimeCollection}
                />
                  <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelDocumentTermSettings} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveDocumentTermSettings} />
                </>
            </R.Panel>
        </div>;
    }
}