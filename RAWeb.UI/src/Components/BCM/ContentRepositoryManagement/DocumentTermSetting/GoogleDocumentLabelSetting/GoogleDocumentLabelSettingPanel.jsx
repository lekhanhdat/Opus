import { showToast } from "../../../../../Utilities/CommonUtil";
import StringUtil from "../../../../../Utilities/StringUtil";
import SelectLabelTree from "../../../../Common/Tree/Instances/TermTree/SelectLabelTree";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import TermStatusForInputText from "../StatusTermInputText";
import RuleSettingComponent from "../../RuleSetting/RuleSettingComponent";
import { RuleModuleTypes } from "../../../../Common/RuleItem/Components/Constants";
import GoogleAutoRule from "../../AutoPopulate/GoogleAutoRule";
import { SelectProcessType } from "../../ManualApprovalSetting/ManualApprovalSettingPanel";


export const DeployLabelMethod = {
    UseManualclassification: 0,
    UseAutoClassification: 1,
    UseIntelligentClassification: 2,
};

export const ApplyExistType = {
    None: 0,
    OverWrite: 1,
    SkipAndKeep: 2
};

export const AutoJobOption = {
    None: 0,
    Skip: 1,
    Override: 2,
    Append: 3
};

export const SavedTermSettingType = {
    GroupTerm: 0,
    CustomTerm: 1
};

export const ArtificialIntelligenceTermUseType = {
    None: 0,
    ApplyTerm: 1,
    AutoDefault: 2
};

export const AutoDefaultRuleUseIntelligenceClassification = 3;

export default class GoogleDocumentLabelSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        let currNode = this.props.data;
        this.autoRuleData = this.props.data.AutoClassificationRules ? RM.deepcopy(this.props.data.AutoClassificationRules) : [];
        let termScope = !CRMCommonUtil.guidIsEmpty(currNode.TermId)
            ? { nodeId: currNode.TermId, nodeType: "Term", nodeName: currNode.TermName }
            : { nodeId: currNode.TermSetId, nodeType: "TermSet", nodeName: currNode.LabelSetName };
        let deployTermMethodInitItems = [
            { text: RMResx.RM_JS_SPS_AutoClassification_NoDefaultValue, value: DeployLabelMethod.UseManualclassification, checked: currNode.DeployLabelMethod == DeployLabelMethod.UseManualclassification },
        ];
        if (this.props.context.configurations.autoRuleDeploy) {
            deployTermMethodInitItems.push({ text: RMResx.RM_JS_SPS_AutoClassification_UseRule, value: DeployLabelMethod.UseAutoClassification, checked: currNode.DeployLabelMethod == DeployLabelMethod.UseAutoClassification });
        }
        this.defaultRule = currNode.AutoClassificationRules ? currNode.AutoClassificationRules.find(r => r.IsDefaultRule) : null;
        this.enableAITerm = this.props.context.configurations.enableAITerm;
        this.autoAITerm = this.enableAITerm && currNode.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault;
        let tempAutoNoDefaultTermMethodItems = [
            {
                text: RMResx.RM_JS_SPS_Label_AutoClassification_NoDefaultValue,
                value: true,
                checked: this.defaultRule
                    ? this.defaultRule.NoDefaultTerm
                    : true,
            },
            {
                text: RMResx.RM_JS_SPS_AutoClassification_UseDefaultLabel,
                value: false,
                checked: this.defaultRule
                    ? !this.defaultRule.NoDefaultTerm
                    : false,
            },
        ];
        if (this.enableAITerm) {
            tempAutoNoDefaultTermMethodItems.push({
                text: RMResx.RM_MachineLearning_DeployTermMethodIntelligence,
                value: DeployLabelMethod.UseIntelligentClassification,
                checked: this.autoAITerm,
            });
        }
        this.state = {
            autoNoDefaultTermMethodItems: tempAutoNoDefaultTermMethodItems,
            radioApplyTerm: [
                { text: RMResx.RM_SPS_ApplyOverwirteTerm, value: ApplyExistType.OverWrite, checked: (currNode.NeedCheckDefaultValue || currNode.ApplyTermIncludeFolder) ? currNode.ApplyExistType == ApplyExistType.OverWrite : false },
                { text: RMResx.RM_SPS_ApplySkipTerm, value: ApplyExistType.SkipAndKeep, checked: (currNode.NeedCheckDefaultValue || currNode.ApplyTermIncludeFolder) ? currNode.ApplyExistType == ApplyExistType.SkipAndKeep : false },
            ],
            termDataLoaded: false,
            savedTermTreeData: [],
            selectedTermScope: termScope,
            documentLevel: 1,
            isShowSelectTermPanel: { show: false },
            termDefaultName: currNode.DefaultTermName,
            applyToAll: currNode.NeedCheckDefaultValue,
            applyToDSetsAndFolders: currNode.ApplyTermIncludeFolder,
            applyExistType: currNode.ApplyExistType,
            includeDeclaredRecords: currNode.IncludeDeclaredRecords,
            deployLabelMethod: currNode.DeployLabelMethod,
            autoNoDefaultTermMethod: this.defaultRule ? this.defaultRule.NoDefaultTerm : true,
            autoJobOption: currNode.AutoJobOption == AutoJobOption.None ? AutoJobOption.Append : currNode.AutoJobOption,
            runAutoFullJob: currNode.RunAutoFullJob,
            autoDefaultTermName: this.defaultRule ? this.defaultRule.TermName : "",
            enableRelatedRecords: currNode.EnableRelatedRecords,
            inputSelectTermDisable: false,
            searchKey: "",
            enableTermSettingStatus: this.props.showTermSettings,
            enableTermSettingStatusChanged: false,
            intelligenceEnableApprovalItem: currNode.AIApprovalType ? parseInt(currNode.AIApprovalType.toString(), 10) : 0,
            workflowReferenceId: currNode.AIWorkflowReferenceId,
            workflowList: [],
            userList: [],
            mailToOwner: currNode.AISendEMail,
            aiTermUseType: this.enableAITerm ? currNode.AITermUseType : ArtificialIntelligenceTermUseType.None,
            isTermHasNoPermission: this.defaultRule ? this.defaultRule.TermHasNoPermission : false,
            aiThenIsDefaultTermMethod: currNode.AIThenIsDefaultTermMethod,
            aiThenDefaultTermName: currNode.AIThenDefaultTermName,
        };
        this.termSetId = currNode.TermSetId;
        this.labelSetName = currNode.LabelSetName;
        this.termId = currNode.TermId;
        this.ObjectId = currNode.ObjectId;
        this.termName = currNode.TermName;
        this.defaultTermId = currNode.DefaultTermId;

        this.treePageSize = 15;
        this.createColumnName = currNode.ColumnName;
        this.existColumnName = currNode.ExistColumnName;
        this.isDisplayTermPath = currNode.IsDisplyaTermPath;
        this.isDefaultTermRemoved = this.props.data.IsDefaultTermRemoved;
        this.isDefaultTermDeprecated = this.props.data.IsDefaultTermDeprecated;
        this.RuleSettingComponent = null;
        this.addedRules = currNode.Rules || [];
        this.addUserChanged = [];
        this.aiThenDefaultTermId = currNode.AIThenDefaultTermId;
    }

    componentInit() {
        if (this.enableAITerm) {
            this.initPeopleCombobox();
        }
    }

    componentReceive(type, args) {
        switch (type) {
            case "onShow":
                this.init(args);
                break;
            case "onSave":
                this.save(args);
                break;
        }
    }

    initPeopleCombobox() {
        let users = this.props.data.AIReviewers;
        if (users) {
            let newUsers = CRMCommonUtil.convertUsersToRichCombobox(users);
            this.addUserChanged = newUsers;
            this.setState({
                userList: newUsers,
            });
        }
    }

    changeAnotherTermGroup(result) {
        this.showInputError = true;
        this.inputErrorMessage = result.message;
        if (this.refChangeTermGroupValid) {
            $$.verify(this.refChangeTermGroupValid.ref.current);
        }
        if (this.refAutoRuleChangeTermGroupValid) {
            $$.verify(this.refAutoRuleChangeTermGroupValid.ref.current);
        }
        if (this.refAutoRule) {
            this.refAutoRule.validChangeTerm(this.showInputError, this.inputErrorMessage);
        }
    }

    save(callback) {
        if (!this.state.enableTermSettingStatus) {
            let { isValid, trList } = this.RuleSettingComponent.getTermRules();
            if(!isValid){
                showToast.error(RMResx.RM_JS_BCM_Msg_TermRulesNotAllHasRule);
                return false;
            }
            this.addedRules = trList;
            if (this.addedRules.length != 0) {
                this.props.context.saveTermSettings(this, callback);
            } else {
                this.showRemoveAllRulesMessageBox(() => { this.props.context.saveTermSettings(this, callback); });
            }
            return;
        }
        this.props.context.saveTermSettings(this, callback);
    }

    setAutoRuleData() {
        let autoRuleValidateResult = true;
        let currNode = this.props.data;
        if (this.state.deployLabelMethod == DeployLabelMethod.UseAutoClassification) {
            autoRuleValidateResult = this.refAutoRule.autoRuleValidate();
            if (autoRuleValidateResult) {
                let autoRules = this.refAutoRule.getAutoRuleData();
                let tempDefaultRule = autoRules.find(r => r.IsDefaultRule);
                if (this.defaultRule) {
                    tempDefaultRule.NoDefaultTerm = this.defaultRule.NoDefaultTerm;
                    tempDefaultRule.TermId = this.defaultRule.TermId;
                    tempDefaultRule.TermName = this.defaultRule.TermName;
                    tempDefaultRule.TermHasNoPermission = this.defaultRule.TermHasNoPermission;
                }
                currNode.AutoClassificationRules = autoRules;
            }
        } else {
            currNode.AutoClassificationRules = null;
        }
        return autoRuleValidateResult;
    }

    termFullPathChanged = (args) => {
        this.isDisplayTermPath = args;
    }

    autoNoDefaultTermMethodChanged = (args) => {
        if (args == DeployLabelMethod.UseIntelligentClassification) {
            this.setState({
                autoNoDefaultTermMethod: true, autoDefaultTermName: "",
                aiTermUseType: ArtificialIntelligenceTermUseType.AutoDefault
            });
        this.defaultRule = {};
            this.defaultRule.NoDefaultTerm = true;
        } else {
            this.setState({
                autoNoDefaultTermMethod: args, autoDefaultTermName: "",
                aiTermUseType: ArtificialIntelligenceTermUseType.None
            });
            this.defaultRule = {};
        this.defaultRule.NoDefaultTerm = args;
    }
    }

    deployLabelMethodChanged = (args) => {
        this.autoRuleData = [];
        this.state.radioApplyTerm.forEach(element => {
            element.checked = false;
        });
        this.defaultTermId = CRMCommonUtil.GuidEmpty;
        this.defaultRule = {};
        this.defaultRule.NoDefaultTerm = true;

        let tempAutoNoDefaultTermMethodItems = this.state.autoNoDefaultTermMethodItems;
        tempAutoNoDefaultTermMethodItems.forEach((element, index) => {
            element.checked = index == 0;
        });
        this.addUserChanged = [];
        this.setState({
            deployLabelMethod: args,
            termDefaultName: "",
            radioApplyTerm: this.state.radioApplyTerm,
            applyToAll: false,
            includeDeclaredRecords: false,
            applyToDSetsAndFolders: false,
            autoNoDefaultTermMethod: true,
            autoNoDefaultTermMethodItems: RM.deepcopy(tempAutoNoDefaultTermMethodItems),
            aiTermUseType: (this.enableAITerm && args == DeployLabelMethod.UseIntelligentClassification) ? ArtificialIntelligenceTermUseType.ApplyTerm : ArtificialIntelligenceTermUseType.None,
            alwaysScanAllExistDocuments: false,
        }, () => {
            this.defaultTermStatusText && this.defaultTermStatusText.clearStatus();
            this.autoRuleDefaultTermStatusText && this.autoRuleDefaultTermStatusText.clearStatus();
        });
        this.resetAiOptions();
    }

    resetAiOptions() {
        this.state.workflowList.forEach(item => {
            item.Checked = false;
        });
        this.aiThenDefaultTermId = CRMCommonUtil.GuidEmpty;
        this.setState({
            intelligenceEnableApprovalItem: SelectProcessType.SelectOwnerRecords,
            workflowList: RM.deepcopy(this.state.workflowList),
            mailToOwner: false,
            aiThenDefaultTermName: "",
            aiThenIsDefaultTermMethod: false,
            userList: [],
        });
    }

    showSelectedAutoDefaultRuleTermTree = () => {
        this.selectedDefaultTermCache = null;
        this.setState({ isShowSelectTermPanel: { show: true } });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    cancelSelectTerm = () => {
        this.setState({ isShowSelectTermPanel: { show: false } });
    }

    saveAutoDefaultRuleSelectTerm = () => {
        if (!$$.verify(this.refSelectAutoDefaultRuleTermValid.ref.current)) {
            return false;
        }
        this.setState({ isShowSelectTermPanel: { show: false } });
        this.setState({ autoDefaultTermName: this.selectedDefaultTermCache.Name });
        this.setState({ isTermHasNoPermission: false });
        this.defaultRule.TermId = this.selectedDefaultTermCache.UniqueId;
        this.defaultRule.TermName = this.selectedDefaultTermCache.Name;
        this.defaultRule.TermIsRemove = false;
        this.defaultRule.TermIsDeprecated = false;
        this.defaultRule.TermExistingTermGroup = true;
        this.autoRuleDefaultTermStatusText && this.autoRuleDefaultTermStatusText.clearStatus();
        $$.verify(this.refAutoDefaultRuleTermValid.ref.current);
    }

    onSelectDefaultTermChanged = (args) => {
        this.selectedDefaultTermCache = args[0];
    }

    onAutoJobOptionChanged = (args) => {
        this.setState({ autoJobOption: args });
    }

    onRunAutoFullJobChanged = (args) => {
        this.setState({ runAutoFullJob: args });
    }

  
    customDefaultTermScopeValid = () => {
        return true;
    }

    customDefaultTermValid = () => {
        return !CRMCommonUtil.guidIsEmpty(this.defaultTermId) && this.state.termDefaultName
            && !this.isDefaultTermRemoved && !this.isDefaultTermDeprecated
            ? true : RMResx.RM_SPS_CS_DefaultTermEmpty;
    }

    applyTermValid = () => {
        return this.state.radioApplyTerm.every(element => !element.checked) ? RMResx.RM_SPS_ApplyCheckErrorMsg : true;
    }

    changeTermGroupValid = () => {
        return !this.showInputError ? true : this.inputErrorMessage;
    }

    customAutoDefaultRuleTermValid = () => {
        let termStatusIsNormal = true;
        let errorMessageKey = "";
        if (this.defaultRule) {
            if (this.defaultRule.TermIsRemove || this.defaultRule.TermIsDeprecated) {
                termStatusIsNormal = false;
                errorMessageKey = RMResx.RM_JS_SPS_AutoClassification_NoLabel;
            } else if (!this.defaultRule.TermExistingTermGroup) {
                termStatusIsNormal = false;
                errorMessageKey = RMResx.RM_JS_SPS_CS_ChangeGroup;
            }
        }
        return !CRMCommonUtil.guidIsEmpty(this.defaultRule.TermId) && this.defaultRule.TermName && termStatusIsNormal ? true : errorMessageKey;
        // return true;
    }

    onSearch = (args) => {
        this.setState({ searchKey: args.value });
    }

    onStopSearch = () => {
        this.setState({ searchKey: "" });
    }

    willChangeEnableTermSetting = () => {
        this.setState({
            enableTermSettingStatusChanged: true,
            enableTermSettingStatus: !this.props.showTermSettings
        });
    }

    onCancelChangeEnableLabelSetting = () => {
        $$.messagedialog(false);
    }

    switchEnableTermSetting = (checkedStatus) => {
        if (this.props.context.configurations.isSwitchTermSettingEffectChildren && (checkedStatus !== this.props.showTermSettings)) {
            $$.messagedialog(true, {
                classify: "warn",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: checkedStatus ? RMResx.RM_CRM_TermSetting_Google_SwitchToEnableSetTermTip : RMResx.RM_CRM_TermSetting_Google_SwitchDisableSetTermTip,
                buttons: [
                    { text: RMResx.RM_JS_Common_Cancel, onClick: this.onCancelChangeEnableLabelSetting },
                    { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.willChangeEnableTermSetting }
                ],
                willClose: this.onCancelChangeEnableLabelSetting
            });
            return false;
        } else {
            this.setState({ enableTermSettingStatus: checkedStatus, enableTermSettingStatusChanged: true });
        }
    }

    getApplyLabelDescription() {
        return RMResx.RM_JS_SPS_Label_ApplyDocumentCommonDescription;
    }

    onApprovalEnableChange = (args) => {
        if (args) {
            this.setState({ intelligenceEnableApprovalItem: SelectProcessType.SelectOwnerRecords });
        } else {
            this.setState({ intelligenceEnableApprovalItem: SelectProcessType.SelectNoneApprovalType });
        }
    }

    onAIReviewApprovalChanged = (args) => {
        this.setState({ intelligenceEnableApprovalItem: args });
    }

    onAIReviewSearchUser = (args) => {
        let searchValue = args.key;
        let urlData = `/api/BCMCommonSettingApi/SearchAADUsers?tenantId=&key=${searchValue}`;
        let option = {
            url: urlData,
            method: "get"
        };
        if (searchValue) {
            return fetchUtility(option).then((res) => {
                let users = RM.deepcopy(res.Users);
                return CRMCommonUtil.convertUsersToRichCombobox(users);
            }).catch((e) => {

            });
        }
    }

    onAIReviewUserChanged = (args) => {
        this.addUserChanged = RM.deepcopy(args.newValue);
    }

    onEMailToRecordOwnerChanged = (args) => {
        this.setState({ mailToOwner: args });
    }

    aiThenIsDefaultTermMethodChanged = (args) => {
        this.setState({ aiThenIsDefaultTermMethod: args, aiThenDefaultTermName: "" });
    }

    aiThenDefaultRuleTermValid = () => {
        return !CRMCommonUtil.guidIsEmpty(this.aiThenDefaultTermId) && this.state.aiThenDefaultTermName
            && !this.isAiThenDefaultTermRemoved && !this.isAiThenDefaultTermDeprecated
            ? true : RMResx.RM_SPS_CS_DefaultTermEmpty;
    }

    saveAiThenDefaultRuleSelectTerm = () => {
        if (!$$.verify(this.refSelectAiThenDefaultRuleTermValid.ref.current)) {
            return false;
        }
        this.setState({
            isShowSelectTermPanel: { show: false },
            aiThenDefaultTermName: this.selectedDefaultTermCache.Name,
        });
        this.aiThenRuleDefaultTermStatusText && this.aiThenRuleDefaultTermStatusText.clearStatus();
        this.aiThenSelectedTerm = this.selectedDefaultTermCache;
        this.aiThenDefaultTermId = this.aiThenSelectedTerm.UniqueId;
        this.aiThenDefaultTermName = this.aiThenSelectedTerm.Name;
        this.isAiThenDefaultTermRemoved = false;
        this.isAiThenDefaultTermDeprecated = false;
        if (this.refAiThenDefaultRuleTermValid) {
            $$.verify(this.refAiThenDefaultRuleTermValid.ref.current);
        }
    }

    showRemoveAllRulesMessageBox(onCliekOKFunc) {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div className="margin-bottom-m">{RMResx.RM_AR_SPS_IL_Options_Warning_RemoveAllRules}</div>
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmRecordsRemoveAllRulesDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: onCliekOKFunc
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    renderAutoRule() {
        const defaultTermID = this.autoRuleData.find(rule => rule.IsDefaultRule)?.TermId || null;
        return <div className="ra-auto-rule">
            <div className="require ra-setting-panel-title" tabIndex="0">{StringUtil.trimEndColon(RMResx.RM_JS_SPS_Label_AutoClassification_SetCondition)}</div>
            <GoogleAutoRule
                ref={r => this.refAutoRule = r}
                itemId={this.props.context.itemId}
                data={this.autoRuleData}
                selectedTermScope={this.state.selectedTermScope}
                inputSelectTermDisable={this.state.inputSelectTermDisable}
                sourceFlag={this.props.sourceFlag}
                containerId={this.props.context.getGroupNodeId(this.props.data)}
                lastAccessTimeCollection={this.props.lastAccessTimeCollection}
                nodeId={this.props.data.Id}
            ></GoogleAutoRule>
            <div className="ra-crm-form-content">
                <div className="ra-setting-panel-title margin-top-l">
                    <span id="ariaAutoDefault" tabIndex={0}>{RMResx.RM_SPS_AutoClassification_DefaultConditionTitle}</span>
                </div>
                <div className="margin-top-s">
                    <R.Radio.Group
                        block={true}
                        name="radiogroup-autoDeployTerm"
                        items={this.state.autoNoDefaultTermMethodItems}
                        onChange={this.autoNoDefaultTermMethodChanged}
                        aria="#ariaAutoDefault"
                    />
                </div>
            </div>


            {!this.state.autoNoDefaultTermMethod && <div>
                <div className="ra-crm-form-content">
                    <div className="inline-block">
                        <div className="class-selector" id="auto-default-term-div" tabIndex="0">
                            <div className="class-selector-value" data-tooltip="diffneed" aria-label={this.state.autoDefaultTermName}>
                                {this.defaultRule && <TermStatusForInputText
                                    ref={r => this.autoRuleDefaultTermStatusText = r}
                                    termRemoved={this.defaultRule.TermIsRemoved}
                                    termDeprecated={this.defaultRule.TermIsDeprecated}></TermStatusForInputText>}
                                {this.state.autoDefaultTermName}
                            </div>
                        </div>
                        {!this.state.inputSelectTermDisable &&
                            <div className="class-selector-icon" data-tooltip aria-label={RMResx.RM_JS_SPS_DocumentSettings_SelectTerm} onClick={this.showSelectedAutoDefaultRuleTermTree} tabIndex="0" onKeyDown={this.onKeyDown}>
                                <div className="fia-term" aria-hidden="true"></div>
                            </div>}
                        <R.ValidationFaker valid={this.customAutoDefaultRuleTermValid} of={"#auto-default-term-div"} ref={r => this.refAutoDefaultRuleTermValid = r} />
                    </div>
                    <R.ValidationFaker valid={this.changeTermGroupValid} of={"#auto-default-term-div"} ref={r => this.refAutoRuleChangeTermGroupValid = r} />
                    {this.state.isTermHasNoPermission && <span className="permission-error-message">{RMResx.RM_JS_SPS_AutoClassification_TermHasNoPermission}</span>}
                </div>
                <R.Panel
                    size={670}
                    header={RMResx.RM_JS_SPS_Select_Label}
                    status={this.state.isShowSelectTermPanel}
                    destroy={true}
                    hasClose={true}
                    actionType={'back'}
                    position={"right"}
                    backdropHide={true}
                   >
                    <div>
                        <div className="margin-top-s margin-left-l">
                            <R.ValidationFaker valid={this.customDefaultTermScopeValid} ref={r => this.refSelectAutoDefaultRuleTermValid = r} />
                        </div>
                        <SelectLabelTree
                            rootItem={this.state.selectedTermScope}
                            onSelectedNodeChanged={this.onSelectDefaultTermChanged}
                            sourceFlag={this.props.sourceFlag}
                            containerId={this.props.context.getGroupNodeId(this.props.data)}
                            nodeId={this.props.data.Id}
                            uniqueId={defaultTermID}
                        >
                        </SelectLabelTree>
                    </div>
                    <>
                        <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelSelectTerm} />
                        <R.Button slot="buttons" id="raCrmDocLvlAutoSetDefaultSelectTermSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveAutoDefaultRuleSelectTerm} />
                    </>
                </R.Panel>
            </div>}
            {this.state.aiTermUseType == ArtificialIntelligenceTermUseType.AutoDefault && this.renderAITerm(ArtificialIntelligenceTermUseType.AutoDefault)}
           
            {this.renderApplyExistOptions()}
        </div>;
    }

    renderApplyExistOptions() {
        return <div>
            <div className="ra-crm-form-content">
                <div className="require ra-setting-panel-title margin-top-s" tabIndex={0}>
                    {this.getApplyLabelDescription()}
                </div>
                <div role="radiogroup" aria-label={this.getApplyLabelDescription()}>
                    <div className="margin-top-s">
                        <R.Radio
                            name="radio-AutoJobOption"
                            text={RMResx.RM_JS_SPS_ApplyOverwirteLabel}
                            tooltip={RMResx.RM_JS_SPS_ApplyOverwirteLabel}
                            value={AutoJobOption.Override}
                            checked={this.state.autoJobOption == AutoJobOption.Override}
                            onChange={this.onAutoJobOptionChanged}
                        />
                    </div>
                    <div className="margin-top-s">
                        <R.Radio
                            name="radio-AutoJobOption"
                            text={RMResx.RM_JS_SPS_ApplySkipLabel}
                            tooltip={RMResx.RM_JS_SPS_ApplySkipLabel}
                            value={AutoJobOption.Skip}
                            checked={this.state.autoJobOption == AutoJobOption.Skip}
                            onChange={this.onAutoJobOptionChanged}
                        />
                    </div>
                    <div className="margin-top-s">
                        <R.Radio
                            name="radio-AutoJobOption"
                            text={RMResx.RM_JS_SPS_AppendLabel}
                            tooltip={RMResx.RM_JS_SPS_AppendLabel}
                            value={AutoJobOption.Append}
                            checked={this.state.autoJobOption == AutoJobOption.Append}
                            onChange={this.onAutoJobOptionChanged}
                        />
                    </div>
                </div>
            </div>

            <div className="ra-crm-form-content">
                <div className="ra-document-panel">
                    <R.Checkbox
                        id="raCrmDocLvlRunAutoFullJobChk"
                        text={RMResx.RM_SPS_Auto_RunFullJob}
                        tooltip={RMResx.RM_SPS_Auto_RunFullJob}
                        checked={this.state.runAutoFullJob}
                        onChange={this.onRunAutoFullJobChanged}
                    />
                </div>
            </div>
        </div>;
    }

    renderAITerm(AIType) {
        return (
            <div className="margin-bottom-l">
                <div className="ra-setting-panel-containerEnable">
                    <span className="ra-setting-panel-containerSwitch">
                        <R.Checkbox
                            id="raCrmAiMaEnableChk"
                            text={RMResx.RM_MachineLearning_IntelligenceMA}
                            tooltip={RMResx.RM_MachineLearning_IntelligenceMA}
                            checked={this.state.intelligenceEnableApprovalItem != SelectProcessType.SelectNoneApprovalType}
                            onChange={this.onApprovalEnableChange}
                        />
                    </span>
                </div>
                {this.state.intelligenceEnableApprovalItem != SelectProcessType.SelectNoneApprovalType && (
                    <div className="margin-left-l">
                        <div className="ra-crm-form-content">
                            <div className="margin-top-s">
                                <div tabIndex={0}>{RMResx.RM_MachineLearning_Title_AddUser}</div>
                                <div className="margin-top-s">
                                    <R.Validation
                                        element="RichCombobox"
                                        require={RMResx.RM_JS_CP_AM_Owner_Require} >
                                        <R.RichCombobox
                                            asyncSearch
                                            id={"userAiManualApproval"}
                                            width={557}
                                            height={80}
                                            value={this.state.userList}
                                            searchPlaceholder={RMResx.RM_Common_PeoplePicker_Watermark}
                                            disabled={false}
                                            textField="name"
                                            valueField="value"
                                            template="profile"
                                            itemTemplate="profile"
                                            checkedField="checked"
                                            tooltipField="tooltip"
                                            disabledField="disabled"
                                            readonlyField="readonly"
                                            invalidField="invalid"
                                            groupField={null}
                                            matchFields={{ 'name': false }}
                                            searchable={true}
                                            singleMode={false}
                                            silence={false}
                                            excludeChecked={true}
                                            doLoad={this.onAIReviewSearchUser}
                                            onChange={this.onAIReviewUserChanged}
                                        />
                                    </R.Validation>
                                </div>
                            </div>
                        </div>
                        <div className="ra-setting-panel-checkbox">
                            <R.Checkbox
                                id="raCrmAiMaSendMailToOwner"
                                text={RMResx.RM_MachineLearning_SendMailToOwner}
                                title={RMResx.RM_MachineLearning_SendMailToOwner}
                                checked={this.state.mailToOwner}
                                onChange={this.onEMailToRecordOwnerChanged}
                            />
                        </div>
                    </div>
                )}

                <div className="ra-crm-form-content">
                    <div className="ra-setting-panel-title margin-top-l">
                        <span id="ariaAiThenDefault">{RMResx.RM_MachineLearning_IntelligenceThen}</span>
                    </div>
                    <div role="radiogroup" aria-label={RMResx.RM_MachineLearning_IntelligenceThen}>
                        <div className="margin-top-s">
                            <R.Radio
                                name="radiogroup-aiThenDeployTerm"
                                text={RMResx.RM_JS_SPS_AutoClassification_NoDefaultValue}
                                tooltip={RMResx.RM_JS_SPS_AutoClassification_NoDefaultValue}
                                value={false}
                                checked={this.state.aiThenIsDefaultTermMethod === false}
                                onChange={this.aiThenIsDefaultTermMethodChanged}
                            />
                        </div>
                        <div className="margin-top-s">
                            <R.Radio
                                name="radiogroup-aiThenDeployTerm"
                                text={RMResx.RM_JS_SPS_AutoClassification_UseDefault}
                                tooltip={RMResx.RM_JS_SPS_AutoClassification_UseDefault}
                                value={true}
                                checked={this.state.aiThenIsDefaultTermMethod === true}
                                onChange={this.aiThenIsDefaultTermMethodChanged}
                            />
                        </div>
                        
                    </div>
                </div>
                {this.state.aiThenIsDefaultTermMethod && <div>
                    <div className="ra-crm-form-content">
                        <div className="inline-block">
                            <div className="class-selector" id="aiThen-default-term-div" tabIndex="0">
                                <div className="class-selector-value" data-tooltip="diffneed" aria-label={this.state.aiThenDefaultTermName}>
                                    {this.defaultRule && <TermStatusForInputText
                                        ref={r => this.aiThenRuleDefaultTermStatusText = r}
                                        termRemoved={this.defaultRule.TermIsRemoved}
                                        termDeprecated={this.defaultRule.TermIsDeprecated}></TermStatusForInputText>}
                                    {this.state.aiThenDefaultTermName}
                                </div>
                            </div>
                            {!this.state.inputSelectTermDisable &&
                                <div className="class-selector-icon" data-tooltip aria-label={RMResx.RM_JS_SPS_DocumentSettings_SelectTerm} onClick={this.showSelectedAutoDefaultRuleTermTree} tabIndex="0" onKeyDown={this.onKeyDown}>
                                    <div className="fia-term" aria-hidden="true"></div>
                                </div>}
                            <R.ValidationFaker valid={this.aiThenDefaultRuleTermValid} of={"#aiThen-default-term-div"} ref={r => this.refAiThenDefaultRuleTermValid = r} />
                        </div>
                        <R.ValidationFaker valid={this.changeTermGroupValid} of={"#aiThen-default-term-div"} ref={r => this.refAiThenRuleChangeTermGroupValid = r} />
                    </div>
                    <R.Panel
                        size={670}
                        header={RMResx["RM_SPS_CS_SelectDefault-checkbox"]}
                        status={this.state.isShowSelectTermPanel}
                        destroy={true}
                        actionType={'back'}
                    >
                        <div>
                            <div className="margin-top-s margin-left-l">
                                <R.ValidationFaker valid={this.customDefaultTermScopeValid} ref={r => this.refSelectAiThenDefaultRuleTermValid = r} />
                            </div>
                            <SelectLabelTree
                                rootItem={this.state.selectedTermScope}
                                onSelectedNodeChanged={this.onSelectDefaultTermChanged}
                                sourceFlag={this.props.sourceFlag}
                                containerId={this.props.context.getGroupNodeId(this.props.data)}
                                nodeId={this.props.data.Id}
                                uniqueId={this.aiThenDefaultTermId}
                            >
                            </SelectLabelTree>
                        </div>
                        <>
                            <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelSelectTerm} />
                            <R.Button slot="buttons" id="raCrmDocLvlAiThenSetDefaultSelectTermSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveAiThenDefaultRuleSelectTerm} />
                        </>
                    </R.Panel>
                </div>}
                {AIType == ArtificialIntelligenceTermUseType.ApplyTerm && this.renderApplyExistOptions(ArtificialIntelligenceTermUseType.ApplyTerm)}
            </div>
        );
    }

    render() {
        const isGroupNode = this.props.context.isGroupNode(this.props.data);
        const hasSupportSettingWithoutClassification = this.props.context.supportSettingWithoutClassification(this.props.data);
        return <div id={this.props.id}>
            {hasSupportSettingWithoutClassification && isGroupNode &&
                <div className="ra-setting-panel-switch-enableTermSetting ">
                    <div tabIndex="0">{RMResx.RM_JS_SPS_EnableApplyTermSettingsTitle}</div>
                    <R.Switch
                        id="raCrmExoEnableTermSettingSwitch"
                        checked={this.state.enableTermSettingStatus}
                        willChange={this.switchEnableTermSetting} 
                    />
                </div>
            }
            {(this.state.enableTermSettingStatus) && this.state.enableTermSettingStatusChanged && <div className="ra-general-panel">
                <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex="0">
                    <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                    <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_EnableTermSettings}</span>
                </div>
            </div>}
            {(!this.state.enableTermSettingStatus) && this.state.enableTermSettingStatusChanged && <div className="ra-general-panel">
                <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex="0">
                    <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                    <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_DisableTermSettings}</span>
                </div>
            </div>}
            {
                this.state.enableTermSettingStatus && 
                <div id="ra-crm-term-settings" className={hasSupportSettingWithoutClassification ? "margin-top-l" : ""}>
                    <R.Validation>
                        <div ref={r => this.allValidation = r}>
                            {this.props.context.configurations.columnName && <div className="ra-crm-form-content">
                                <div className="ra-setting-panel-title">{StringUtil.trimEndColon(RMResx.RM_JS_SPS_DocSettingTitle)}</div>
                                <div className="ra-setting-panel-column" role="region" aria-label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_DocSettingTitle)} tabIndex="0">
                                    <span>{this.props.data.IsUsingExistColumnName ? this.existColumnName : this.createColumnName}</span>
                                </div>
                            </div>}

                            <div className="ra-crm-form-content">
                                <div className="require ra-setting-panel-title" tabIndex={0}>
                                    <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_SPS_Label_AutoClassification_Method)} />
                                </div>
                                <div role="radiogroup" aria-label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_Label_AutoClassification_Method)}>
                                    <div>
                                        <div className={this.props.context.configurations.applyTermIsShowTips ? "" : "margin-top-s"}>
                                            <R.Radio
                                                name="radio-applyterm"
                                                text={RMResx.RM_JS_SPS_Label_AutoClassification_NoDefaultValue}
                                                value={DeployLabelMethod.UseManualclassification}
                                                checked={this.props.data.DeployLabelMethod == DeployLabelMethod.UseManualclassification}
                                                onChange={this.deployLabelMethodChanged}
                                            />
                                            {this.props.context.configurations.applyTermIsShowTips && <$g.Popover>{this.props.context.applyTermItemTips[0]}</$g.Popover>}
                                        </div>
                                        {this.props.context.configurations.autoRuleDeploy && <div className={this.props.context.configurations.applyTermIsShowTips ? "" : "margin-top-s"}>
                                            <R.Radio
                                                name="radio-applyterm"
                                                text={RMResx.RM_JS_SPS_Label_AutoClassification_UseRule}
                                                value={DeployLabelMethod.UseAutoClassification}
                                                checked={this.props.data.DeployLabelMethod == DeployLabelMethod.UseAutoClassification}
                                                onChange={this.deployLabelMethodChanged}
                                            />
                                            {this.props.context.configurations.applyTermIsShowTips && <$g.Popover>{this.props.context.applyTermItemTips[2]}</$g.Popover>}
                                        </div>}
                                        {this.enableAITerm && (
                                            <div className={"margin-top-s"}>
                                                <R.Radio
                                                    name="radio-applyterm"
                                                    text={RMResx.RM_MachineLearning_DeployTermMethodIntelligence}
                                                    value={DeployLabelMethod.UseIntelligentClassification}
                                                    checked={this.props.data.DeployLabelMethod == DeployLabelMethod.UseIntelligentClassification}
                                                    onChange={this.deployLabelMethodChanged}
                                                />
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>
                            {this.state.deployLabelMethod == DeployLabelMethod.UseAutoClassification && this.renderAutoRule()}
                            {this.state.aiTermUseType == ArtificialIntelligenceTermUseType.ApplyTerm
                                && this.renderAITerm(ArtificialIntelligenceTermUseType.ApplyTerm)}
                        </div>
                    </R.Validation>
                </div>
            }
            {
                !this.state.enableTermSettingStatus &&
                <RuleSettingComponent
                    ref={r => this.RuleSettingComponent = r}
                    context={this.props.context}
                    currentNode={this.props.data}
                    availableRules={this.props.availableRules}
                    refreshRules={this.props.refreshRules}
                    createRuleComponentType={this.props.context.configurations.createRuleComponentType}
                    moduleType={RuleModuleTypes.Records}
                />
            }

        </div>;
    }
}