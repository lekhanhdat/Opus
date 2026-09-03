import { getRequestVerificationToken, LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";
import StringUtil from "../../../../Utilities/StringUtil";
import CRMTeamTree from "../../../Common/Tree/Instances/TermTree/CRMTeamTree";
import SelectTermTree from "../../../Common/Tree/Instances/TermTree/SelectTermTree";
import AutoRule from "../AutoPopulate/AutoRule";
import CRMCommonUtil from "../Common/CRMCommonUtil";
import TermStatusForInputText from "./StatusTermInputText";
import RuleSettingComponent from "../RuleSetting/RuleSettingComponent";
import { ClassificationSettingType } from "../ClassificationSetting/ClassificationSettingPanel";
import { SourceFlags } from "../../../../Constants/Constants";
import { SelectProcessType } from "../ManualApprovalSetting/ManualApprovalSettingPanel";
import Enviroments from "../../../../Constants/Enviroments";
import { RuleModuleTypes } from "../../../Common/RuleItem/Components/Constants";


export const DeployTermMethod = {
    UseDefaultValue: 0,
    UseAutoClassification: 1,
    NoDefaultValue: 2,
    UseIntelligenceClassification: 3
};

export const ApplyExistType = {
    None: 0,
    OverWrite: 1,
    SkipAndKeep: 2
};

export const AutoJobOption = {
    None: 0,
    Skip: 1,
    Override: 2
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
const isEnableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();

export default class DocumentTermSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        let currNode = this.props.data;
        this.autoRuleData = this.props.data.AutoClassificationRules ? RM.deepcopy(this.props.data.AutoClassificationRules) : [];
        let termScope = !CRMCommonUtil.guidIsEmpty(currNode.TermId)
            ? { nodeId: currNode.TermId, nodeType: "Term", nodeName: currNode.TermName }
            : { nodeId: currNode.TermSetId, nodeType: "TermSet", nodeName: currNode.TermSetName };
        let deployTermMethodInitItems = [
            { text: RMResx.RM_JS_SPS_AutoClassification_NoDefaultValue, value: DeployTermMethod.NoDefaultValue, checked: currNode.DeployTermMethod == DeployTermMethod.NoDefaultValue },
            { text: RMResx.RM_JS_SPS_AutoClassification_UseDefault, value: DeployTermMethod.UseDefaultValue, checked: currNode.DeployTermMethod == DeployTermMethod.UseDefaultValue },
        ];
        if (this.props.context.configurations.autoRuleDeploy) {
            deployTermMethodInitItems.push({ text: RMResx.RM_JS_SPS_AutoClassification_UseRule, value: DeployTermMethod.UseAutoClassification, checked: currNode.DeployTermMethod == DeployTermMethod.UseAutoClassification });
        }
        this.defaultRule = currNode.AutoClassificationRules ? currNode.AutoClassificationRules.find(r => r.IsDefaultRule) : null;
        this.enableAITerm = this.props.context.configurations.enableAITerm;
        this.autoAITerm = this.enableAITerm && currNode.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault;
        let tempAutoNoDefaultTermMethodItems = [
            { text: RMResx.RM_JS_SPS_AutoClassification_NoDefaultValue, value: true, checked: (!this.autoAITerm) && (this.defaultRule ? this.defaultRule.NoDefaultTerm : true) },
            { text: RMResx.RM_JS_SPS_AutoClassification_UseDefault, value: false, checked: (!this.autoAITerm) && (this.defaultRule ? !this.defaultRule.NoDefaultTerm : false) },
        ];
        if (this.enableAITerm) {
            if (this.props.data.Level !== 300 || (this.props.data.Level === 300 && this.props.data.NodeType !== 0)) {
                tempAutoNoDefaultTermMethodItems.push({ text: RMResx.RM_MachineLearning_DeployTermMethodIntelligence, value: AutoDefaultRuleUseIntelligenceClassification, checked: this.autoAITerm });
            }
        }
        this.state = {
            termFullPathItems: [
                { text: RMResx.RM_SPS_DisplayTerm_TermLabel, value: false, checked: !currNode.IsDisplyaTermPath },
                { text: RMResx.RM_SPS_DisplayTerm_EntirePath, value: true, checked: currNode.IsDisplyaTermPath }
            ],
            deployTermMethodItems: deployTermMethodInitItems,
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
            deployTermMethod: currNode.DeployTermMethod,
            autoNoDefaultTermMethod: this.defaultRule ? this.defaultRule.NoDefaultTerm : true,
            autoJobOption: currNode.AutoJobOption == AutoJobOption.None ? AutoJobOption.Skip : currNode.AutoJobOption,
            runAutoFullJob: currNode.RunAutoFullJob,
            alwaysScanAllExistDocuments: currNode.AlwaysScanAllExistDocuments || false,
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
            aiThenIsDefaultTermMethod: currNode.AIThenIsDefaultTermMethod,
            aiThenDefaultTermName: currNode.AIThenDefaultTermName,
        };
        this.termSetId = currNode.TermSetId;
        this.termSetName = currNode.TermSetName;
        this.termId = currNode.TermId;
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
        this.isFileSystemJPMC = this.props.sourceFlag === SourceFlags.FS && isEnableJPMCFeature;
    }

    componentInit() {
        this.initTermTree();
        if (this.enableAITerm) {
            this.initWorkflowCombobox();
            this.initPeopleCombobox();
        }
        //for fs classification ci
        if (this.props.getClassificationData() == ClassificationSettingType.FolderLevel) {
            this.props.data.DeployTermMethod = DeployTermMethod.UseDefaultValue;
            this.setState({ deployTermMethod: DeployTermMethod.UseDefaultValue });
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

    init() {

    }

    initWorkflowCombobox() {
        let option = {
            url: '/api/RuleApi/GetAllWorkflows',
            method: "GET"
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            data.forEach(item => {
                if (item.ReferenceId == this.props.data.AIWorkflowReferenceId) {
                    item.Checked = true;
                } else {
                    item.Checked = false;
                }
            });

            this.setState({
                workflowList: data
            });
        }).catch((e) => {
        });
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

    initTermTree() {
        $$.elementLoading("termScopeId", true, {
            text: false,
        });
        let currNode = this.props.data;
        let validCallback = () => {
            if (this.refNoTermScopeValid) {
                $$.verify(this.refNoTermScopeValid.ref.current);
            }
        };
        if ((!CRMCommonUtil.guidIsEmpty(currNode.TermId)) || !CRMCommonUtil.guidIsEmpty(currNode.TermSetId)) {
            this.props.context.getSavedTerm(this).then((result) => {
                if (result) {
                    let isSuccess = result.isSuccess;
                    let isTermScopeError = result.isTermScopeError;
                    if (!isSuccess) {
                        if (result.termGroups) {
                            if (isTermScopeError) {
                                this.selectedTermScopeError = true;
                                this.selectedTermScopeErrorMsg = result.termNoPermissionMsg;
                                if (this.refSelectedTermScopePermissionValid) {
                                    $$.verify(this.refSelectedTermScopePermissionValid.ref.current);
                                }

                                if (result.isChangeAnotherTermGroup) {
                                    this.changeAnotherTermGroup(result);
                                }
                                if (this.refAutoRule) {
                                    this.refAutoRule.validChangeTerm(this.showInputError, this.inputErrorMessage);
                                }
                            } else {
                                this.changeAnotherTermGroup(result);
                            }
                        } else {
                            this.showTermScopeError = true;
                            this.termScopeErrorMessage = result.message;
                            if (this.refTermGroupPermissionValid) {
                                $$.verify(this.refTermGroupPermissionValid.ref.current);
                            }
                        }
                        this.setState({ inputSelectTermDisable: true }, validCallback);
                    }
                    let termGroups = result.termGroups;
                    if (termGroups) {
                        this.showNoTermError = false;
                        this.setState({ savedTermTreeData: termGroups, termDataLoaded: true, inputSelectTermDisable: (currNode.IsTermDeprecated || currNode.IsTermRemoved) }, validCallback);
                    }
                } else {
                    this.showNoTermError = true;
                    this.setState({ termDataLoaded: false, inputSelectTermDisable: true }, validCallback);
                }
                $$.loading(false);
                $$.elementLoading("termScopeId", false, {
                    text: false,
                });
            }).catch((e) => {
                $$.loading(false);
            });
        } else {
            if (this.props.context.isGroupNode(currNode)) {
                this.showNoTermError = false;
                this.setState({ termDataLoaded: true, inputSelectTermDisable: true }, validCallback);
            } else {
                this.showNoTermError = true;
                this.setState({ termDataLoaded: false, inputSelectTermDisable: true }, validCallback);
            }
            $$.elementLoading("termScopeId", false, {
                text: false,
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
        if (this.state.deployTermMethod == DeployTermMethod.UseAutoClassification) {
            autoRuleValidateResult = this.refAutoRule.autoRuleValidate();
            if (autoRuleValidateResult) {
                let autoRules = this.refAutoRule.getAutoRuleData();
                let tempDefaultRule = autoRules.find(r => r.IsDefaultRule);
                if (this.defaultRule) {
                    tempDefaultRule.NoDefaultTerm = this.defaultRule.NoDefaultTerm;
                    tempDefaultRule.TermId = this.defaultRule.TermId;
                    tempDefaultRule.TermName = this.defaultRule.TermName;
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
        this.resetAiOptions();
        if (args == AutoDefaultRuleUseIntelligenceClassification) {
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

    deployTermMethodChanged = (args) => {
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
            deployTermMethod: args,
            termDefaultName: "",
            radioApplyTerm: this.state.radioApplyTerm,
            applyToAll: false,
            includeDeclaredRecords: false,
            applyToDSetsAndFolders: false,
            autoNoDefaultTermMethod: true,
            autoNoDefaultTermMethodItems: RM.deepcopy(tempAutoNoDefaultTermMethodItems),
            aiTermUseType: (this.enableAITerm && args == DeployTermMethod.UseIntelligenceClassification) ? ArtificialIntelligenceTermUseType.ApplyTerm : ArtificialIntelligenceTermUseType.None,
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

    saveSelectDefaultTerm = () => {
        if (!$$.verify(this.refSelectDefaultTermValid.ref.current)) {
            return false;
        }
        this.setState({
            isShowSelectTermPanel: { show: false },
            termDefaultName: this.selectedDefaultTermCache.Name,
            // termRemoved: false,
            // termDeprecated: false,
        });
        this.defaultTermStatusText && this.defaultTermStatusText.clearStatus();
        this.selectedTerm = this.selectedDefaultTermCache;
        this.defaultTermId = this.selectedTerm.UniqueId;
        this.defaultTermName = this.selectedTerm.Name;
        this.isDefaultTermRemoved = false;
        this.isDefaultTermDeprecated = false;
        if (this.refDefaultTermValid) {
            $$.verify(this.refDefaultTermValid.ref.current);
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
        this.defaultRule.TermId = this.selectedDefaultTermCache.UniqueId;
        this.defaultRule.TermName = this.selectedDefaultTermCache.Name;
        this.defaultRule.TermIsRemove = false;
        this.defaultRule.TermIsDeprecated = false;
        this.autoRuleDefaultTermStatusText && this.autoRuleDefaultTermStatusText.clearStatus();
        $$.verify(this.refAutoDefaultRuleTermValid.ref.current);
    }

    onSelectDefaultTermChanged = (args) => {
        this.selectedDefaultTermCache = args[0];
    }

    onApplyToAllChange = (args) => {
        this.state.radioApplyTerm.forEach(element => {
            element.checked = false;
        });
        this.setState({ applyToAll: args, radioApplyTerm: this.state.radioApplyTerm, includeDeclaredRecords: false });
    }

    onApplyToDSetsAndFoldersChange = (args) => {
        this.state.radioApplyTerm.forEach(element => {
            element.checked = false;
        });
        this.setState({ applyToDSetsAndFolders: args, radioApplyTerm: this.state.radioApplyTerm });
    }

    onIncludeDeclaredChange = (args) => {
        this.setState({ includeDeclaredRecords: args });
    }

    onApplyExistTypeChanged = (args) => {
        this.setState({ applyExistType: args }, () => { $$.verify(this.refApplyTermValid.ref.current); });
    }

    onAutoJobOptionChanged = (args) => {
        this.setState({ autoJobOption: args });
    }

    onRunAutoFullJobChanged = (args) => {
        this.setState({ runAutoFullJob: args });
    }

    onAlwaysScanAllExistDocumentsChanged = (args) => {
        this.setState({ alwaysScanAllExistDocuments: args });
    }

    onTermScopeTreeChanged = (args) => {
        this.setState({
            selectedTermScope: { nodeId: args[0].Id, nodeType: args[0].Type },
            termDefaultName: "",
            autoDefaultTermName: "",
            aiThenDefaultTermName: "",
            inputSelectTermDisable: false,
        });
        this.defaultTermId = CRMCommonUtil.GuidEmpty;
        this.aiThenDefaultTermId = CRMCommonUtil.GuidEmpty;
        this.defaultTermStatusText && this.defaultTermStatusText.clearStatus();
        if (this.defaultRule) {
            this.defaultRule.TermId = CRMCommonUtil.guidIsEmpty;
            this.defaultRule.TermName = "";
        }
        if (args[0].Type == "Term") {
            this.termSetId = args[0].TermSetUniqueId;
            this.termSetName = args[0].TermSetName;
            this.termName = args[0].Name;
            this.termId = args[0].UniqueId;
        } else if (args[0].Type == "TermSet") {
            this.termSetName = args[0].Name;
            this.termSetId = args[0].UniqueId;
            this.termName = "";
            this.termId = CRMCommonUtil.guidIsEmpty;
        }
        if (this.refAutoRule) {
            this.refAutoRule.clearAutoRuleTerm();
        }
        this.showInputError = false;
        this.selectedTermScopeError = false;
        if (this.refChangeTermGroupValid) {
            $$.verify(this.refChangeTermGroupValid.ref.current);
        }
        if (this.refAutoRuleChangeTermGroupValid) {
            $$.verify(this.refAutoRuleChangeTermGroupValid.ref.current);
        }
        if (this.refAiThenRuleChangeTermGroupValid) {
            $$.verify(this.refAiThenRuleChangeTermGroupValid.ref.current);
        }
        if (this.refAutoRule) {
            this.refAutoRule.validChangeTerm(this.showInputError, this.inputErrorMessage);
        }
        $$.verify(this.refTermScopeValid.ref.current);
        $$.verify(this.refSelectedTermScopePermissionValid.ref.current);
    }

    customTermScopeValid = () => {
        if (this.selectedTermScopeError) {
            return true;
        }
        if (this.refTermScopeTree == null) {
            return true;
        }
        const errorMessage = this.isFileSystemJPMC ? RMResx.RM_JS_FS_SelectClassCodeScope : RMResx.RM_JS_BCM_Global_SelectTermScope;
        var selectedTree = this.refTermScopeTree.getSelectedTreeNode();
        if (selectedTree.node) {
            if (selectedTree.node.IsDeprecated || selectedTree.node.IsExpired) {
                return errorMessage;
            } else {
                return true;
            }
        } else {
            return errorMessage;
        }
    }

    customDefaultTermScopeValid = () => {
        var selectedDefaultTermTree = this.selectedDefaultTermCache == null ? true : false;
        if (selectedDefaultTermTree) {
            return RMResx.RM_SPS_CS_SelectDefaulterm;
        } else {
            return true;
        }
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

    termGroupPermissionValid = () => {
        return !this.showTermScopeError ? true : this.termScopeErrorMessage;
    }

    selectedTermScopePermissionValid = () => {
        return !this.selectedTermScopeError ? true : this.selectedTermScopeErrorMsg;
    }

    noTermScopeValid = () => {
        return !this.showNoTermError ? true : RMResx.RM_JS_SPS_ErrorMessage_NoTermError;
    }
    customAutoDefaultRuleTermValid = () => {
        let termStatusIsNormal = true;
        if (this.defaultRule) {
            if (this.defaultRule.TermIsRemove || this.defaultRule.TermIsDeprecated) {
                termStatusIsNormal = false;
            }
        }
        return !CRMCommonUtil.guidIsEmpty(this.defaultRule.TermId) && this.defaultRule.TermName && termStatusIsNormal ? true : RMResx.RM_JS_SPS_AutoClassification_NoTerm;
    }

    onRelatedRecordsChanged = (args) => {
        this.setState({ enableRelatedRecords: args });
    }

    onDownloadRelatedApp = (e) => {
        let downloadUniqueId = StringUtil.newGuid();
        var $downloadStatusKey = $("#documentDownloadFlag");
        $downloadStatusKey.val(downloadUniqueId);

        $("#crm-document-download")
            .attr("action", this.props.context.downloadRelatedAppUrl)
            .submit();
    }

    onSearch = (args) => {
        this.setState({ searchKey: args });
    }

    showRemoveAllRulesMessageBox(onSave) {
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
                    onClick: onSave
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    willChangeEnableTermSetting = () => {
        this.setState({
            enableTermSettingStatusChanged: true,
            enableTermSettingStatus: !this.props.showTermSettings
        }, () => {
            if (this.state.enableTermSettingStatus && !this.state.termDataLoaded) {
                this.initTermTree();
            }
        });
    }

    onCancelChangeEnableTermSetting = () => {
        $$.messagedialog(false);
    }

    switchEnableTermSetting = (checkedStatus) => {
        if (this.props.context.configurations.isSwitchTermSettingEffectChildren && (checkedStatus !== this.props.showTermSettings)) {
            $$.messagedialog(true, {
                classify: "warn",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: checkedStatus ? RMResx.RM_CRM_TermSetting_SwitchToEnableSetTermTip : RMResx.RM_CRM_TermSetting_SwitchDisableSetTermTip,
                buttons: [
                    { text: RMResx.RM_JS_Common_Cancel, onClick: this.onCancelChangeEnableTermSetting },
                    { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.willChangeEnableTermSetting }
                ],
                willClose: this.onCancelChangeEnableTermSetting
            });
            return false;
        } else {
            this.setState({ enableTermSettingStatus: checkedStatus, enableTermSettingStatusChanged: true });
        }
    }

    getApplyTermDescription() {
        return this.props.sourceFlag === SourceFlags.SP ? RMResx.RM_SPS_ApplyDocumentDescription : RMResx.RM_SPS_ApplyDocumentCommonDescription;
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

    onAIReviewUserChanged = (args) => {
        this.addUserChanged = RM.deepcopy(args.newValue);
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

    onEMailToRecordOwnerChanged = (args) => {
        this.setState({ mailToOwner: args });
    }

    onApprovalSelectionChanged = (args) => {
        let selectedWorkflowId = args.newValue.ReferenceId;
        this.setState({ workflowReferenceId: selectedWorkflowId });
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


    renderApplyToAll() {
        return <div>
            <div>
                {this.props.context.configurations.includeDeclared && <div className="margin-left-m margin-bottom-s">
                    <R.Checkbox
                        id="raCrmDocLvlIncludeDeclaredChk"
                        name="checkbox-include-declare"
                        text={RMResx.RM_JS_SPS_IncludeDeclaredRecords}
                        tooltip={RMResx.RM_JS_SPS_IncludeDeclaredRecords}
                        checked={this.state.includeDeclaredRecords}
                        onChange={this.onIncludeDeclaredChange}
                    />
                    {this.props.context.configurations.includeDeclaredTooltip && <$g.Popover>{this.props.context.configurations.includeDeclaredDesp}</$g.Popover>}
                </div>}
            </div>
        </div>;
    }

    renderApplyExistType() {
        if (
            (this.props.context.configurations.defaultTermApplyExist && this.state.applyToAll) ||
            (this.props.context.configurations.applyDocumentSetsAndFolders && this.state.applyToDSetsAndFolders && !this.props.isCSDTenant)
        ) {
            return <div>
                <div className="require ra-setting-panel-title margin-top-s">
                    <span id="ariaApplyDoc" className="margin-right-s">{this.getApplyTermDescription()}</span>
                </div>
                <R.Radio.Group
                    block={true}
                    name="radio-ApplyExistType"
                    items={this.state.radioApplyTerm}
                    onChange={this.onApplyExistTypeChanged}
                    aria="#ariaApplyDoc"
                />
                <R.ValidationFaker valid={this.applyTermValid} ref={r => this.refApplyTermValid = r} />
            </div>;
        }
    }

    renderDefaultTerm() {
        let currNode = this.props.data;
        return <div className="ra-crm-form-content">
            <div className="ra-crm-form-content">
                <div className="require ra-setting-panel-title">{RMResx.RM_SPS_GS_ChooseDefaultValue}</div>
                <div className="inline-block">
                    <div className="class-selector" id="default-term-div">
                        <div className="class-selector-value" data-tooltip="diffneed" tabIndex="0" role="combobox" aria-label={RMResx.RM_SPS_GS_ChooseDefaultValue}>
                            <TermStatusForInputText
                                ref={r => this.defaultTermStatusText = r}
                                termRemoved={currNode.IsDefaultTermRemoved}
                                termDeprecated={currNode.IsDefaultTermDeprecated}></TermStatusForInputText>
                            {this.state.termDefaultName}
                        </div>
                    </div>
                    {!this.state.inputSelectTermDisable && <div className="class-selector-icon" data-tooltip aria-label={RMResx.RM_JS_SPS_DocumentSettings_SelectTerm} onClick={this.showSelectedAutoDefaultRuleTermTree} tabIndex="0" onKeyDown={this.onKeyDown}>
                        <div className="fia-term" aria-hidden="true"></div>
                    </div>}
                    <R.ValidationFaker valid={this.customDefaultTermValid} of={"#default-term-div"} ref={r => this.refDefaultTermValid = r} />
                </div>
                <R.ValidationFaker valid={this.changeTermGroupValid} of={"#default-term-div"} ref={r => this.refChangeTermGroupValid = r} />
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
                        <R.ValidationFaker valid={this.customDefaultTermScopeValid} ref={r => this.refSelectDefaultTermValid = r} />
                    </div>
                    <SelectTermTree
                        rootItem={this.state.selectedTermScope}
                        onSelectedNodeChanged={this.onSelectDefaultTermChanged}
                        sourceFlag={this.props.sourceFlag}
                        containerId={this.props.context.getGroupNodeId(this.props.data)}
                        uniqueId={currNode.DefaultTermId}
                    >
                    </SelectTermTree>
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelSelectTerm} />
                    <R.Button slot="buttons" id="raCrmDocLvlSetDefaultSelectTermSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveSelectDefaultTerm} />
                </>
            </R.Panel>
            <div className="ra-crm-form-content">
                {
                    this.props.context.configurations.applyDocumentSetsTitle && !this.props.isCSDTenant &&
                    <div className="ra-setting-panel-title">{RMResx.RM_CRM_TermSetting_ApplyToDocumentSetting}</div>
                }
                {this.props.context.configurations.defaultTermApplyExist && <div>
                    <R.Checkbox
                        id="raCrmDocLvlApplyToAllChk"
                        className="ra-setting-panel-checkbox"
                        name="checkbox-applyToAll"
                        text={RMResx.RM_SPS_GS_ApplyToAll}
                        tooltip={RMResx.RM_SPS_GS_ApplyToAll}
                        checked={this.state.applyToAll}
                        onChange={this.onApplyToAllChange}
                    />
                    {this.state.applyToAll && this.renderApplyToAll()}
                </div>}
                {this.props.context.configurations.applyDocumentSetsAndFolders && !this.props.isCSDTenant && <div>
                    <R.Checkbox
                        className="ra-setting-panel-checkbox"
                        name="checkbox-defaultTermApply"
                        text={RMResx.RM_JS_SPS_IncludeDSetAndFolder}
                        tooltip={RMResx.RM_JS_SPS_IncludeDSetAndFolder}
                        checked={this.state.applyToDSetsAndFolders}
                        onChange={this.onApplyToDSetsAndFoldersChange}
                    />
                </div>}
                {this.props.context.configurations.applyAlwaysScanDocuments && this.state.deployTermMethod == DeployTermMethod.UseDefaultValue && (
                    <div style={{ marginTop: "-6px" }}>
                        <R.Checkbox
                            id="raCrmDocLvlAlwaysScanAllExistDocuments4DefaultValue"
                            text={RMResx.RM_SPS_Auto_AlwaysRunFullJob}
                            tooltip={RMResx.RM_SPS_Auto_AlwaysRunFullJob}
                            checked={this.state.alwaysScanAllExistDocuments}
                            onChange={this.onAlwaysScanAllExistDocumentsChanged}
                        />
                        <$g.Popover>{RMResx.RM_SPS_Auto_AlwaysRunFullJob_Desc}</$g.Popover>
                    </div>
                )}
            </div>
            {this.renderApplyExistType()}
        </div>;
    }

    renderAutoRule() {
        const defaultSelectedTermId = this.autoRuleData.find(rule => rule.IsDefaultRule)?.TermId || null;
        return <div className="ra-auto-rule">
            <div className="require ra-setting-panel-title" tabIndex="0">{StringUtil.trimEndColon(RMResx.RM_SPS_AutoClassification_SetCondition)}</div>
            <AutoRule
                ref={r => this.refAutoRule = r}
                itemId={this.props.context.itemId}
                data={this.autoRuleData}
                selectedTermScope={this.state.selectedTermScope}
                inputSelectTermDisable={this.state.inputSelectTermDisable}
                sourceFlag={this.props.sourceFlag}
                containerId={this.props.context.getGroupNodeId(this.props.data)}
                lastAccessTimeCollection={this.props.lastAccessTimeCollection}
            ></AutoRule>
            <div className="ra-crm-form-content">
                <div className="ra-setting-panel-title margin-top-l">
                    <span id="ariaAutoDefault">{RMResx.RM_SPS_AutoClassification_DefaultConditionTitle}</span>
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
                            <R.ValidationFaker valid={this.customDefaultTermScopeValid} ref={r => this.refSelectAutoDefaultRuleTermValid = r} />
                        </div>
                        <SelectTermTree
                            rootItem={this.state.selectedTermScope}
                            onSelectedNodeChanged={this.onSelectDefaultTermChanged}
                            sourceFlag={this.props.sourceFlag}
                            containerId={this.props.context.getGroupNodeId(this.props.data)}
                            uniqueId={defaultSelectedTermId}
                        >
                        </SelectTermTree>
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

    renderApplyExistOptions(useType) {
        return <div className={useType == ArtificialIntelligenceTermUseType.ApplyTerm ? "ra-auto-rule" : ""}>
            <div className="ra-crm-form-content">
                <div className="require ra-setting-panel-title margin-top-s">
                    {this.getApplyTermDescription()}
                </div>
                <div role="radiogroup" aria-label={this.getApplyTermDescription()}>
                    <div className="margin-top-s">
                        <R.Radio
                            name="radio-AutoJobOption"
                            text={RMResx.RM_SPS_ApplyOverwirteTerm}
                            tooltip={RMResx.RM_SPS_ApplyOverwirteTerm}
                            value={AutoJobOption.Override}
                            checked={this.state.autoJobOption == AutoJobOption.Override}
                            onChange={this.onAutoJobOptionChanged}
                        />
                    </div>
                    <div className="margin-top-s">
                        <R.Radio
                            name="radio-AutoJobOption"
                            text={RMResx.RM_SPS_ApplySkipTerm}
                            tooltip={RMResx.RM_SPS_ApplySkipTerm}
                            value={AutoJobOption.Skip}
                            checked={this.state.autoJobOption == AutoJobOption.Skip}
                            onChange={this.onAutoJobOptionChanged}
                        />
                    </div>
                </div>
            </div>

            <div className="ra-crm-form-content">
                <div className="ra-document-panel flex flex-column">
                    <R.Checkbox
                        id="raCrmDocLvlRunAutoFullJobChk"
                        text={RMResx.RM_SPS_Auto_RunFullJob}
                        tooltip={RMResx.RM_SPS_Auto_RunFullJob}
                        checked={this.state.runAutoFullJob}
                        onChange={this.onRunAutoFullJobChanged}
                    />
                    {this.props.context.configurations.applyAlwaysScanDocuments && this.state.runAutoFullJob && this.state.deployTermMethod == DeployTermMethod.UseAutoClassification && (
                        <div className="margin-left-xl">
                            <R.Checkbox
                                id="raCrmDocLvlAlwaysScanAllExistDocuments"
                                text={RMResx.RM_SPS_Auto_AlwaysRunFullJob}
                                tooltip={RMResx.RM_SPS_Auto_AlwaysRunFullJob}
                                checked={this.state.alwaysScanAllExistDocuments}
                                onChange={this.onAlwaysScanAllExistDocumentsChanged}
                            />
                            <$g.Popover>{RMResx.RM_SPS_Auto_AlwaysRunFullJob_Desc}</$g.Popover>
                        </div>
                    )}
                </div>
            </div>

            {this.props.context.configurations.applyDocumentSetsTitle && !this.props.isCSDTenant && useType != ArtificialIntelligenceTermUseType.ApplyTerm &&
                <div className="ra-crm-form-content">
                    <div className="ra-document-panel">
                        <R.Checkbox
                            text={RMResx.RM_JS_SPS_IncludeDSetAndFolder}
                            tooltip={RMResx.RM_JS_SPS_IncludeDSetAndFolder}
                            checked={this.state.applyToDSetsAndFolders}
                            onChange={this.onApplyToDSetsAndFoldersChange}
                        />
                    </div>
                </div>
            }

            {this.props.context.configurations.includeDeclared && <div className="ra-crm-form-content">
                <div className="ra-document-panel">
                    <R.Checkbox
                        id="raCrmDocLvlIncludeDeclaredRecordsChk"
                        text={RMResx.RM_JS_SPS_IncludeDeclaredRecords}
                        tooltip={RMResx.RM_JS_SPS_IncludeDeclaredRecords}
                        checked={this.state.includeDeclaredRecords}
                        onChange={this.onIncludeDeclaredChange}
                    />
                    {this.props.context.configurations.includeDeclaredTooltip && <$g.Popover>{this.props.context.configurations.includeDeclaredDesp}</$g.Popover>}
                </div>
            </div>}
        </div>;
    }

    renderAITerm(AIType) {
        return <div className="margin-bottom-l">
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
            {this.state.intelligenceEnableApprovalItem != SelectProcessType.SelectNoneApprovalType && <div className="margin-left-l">
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
            </div>}

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
                        <SelectTermTree
                            rootItem={this.state.selectedTermScope}
                            onSelectedNodeChanged={this.onSelectDefaultTermChanged}
                            sourceFlag={this.props.sourceFlag}
                            containerId={this.props.context.getGroupNodeId(this.props.data)}
                            uniqueId={this.aiThenDefaultTermId}
                        >
                        </SelectTermTree>
                    </div>
                    <>
                        <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelSelectTerm} />
                        <R.Button slot="buttons" id="raCrmDocLvlAiThenSetDefaultSelectTermSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveAiThenDefaultRuleSelectTerm} />
                    </>
                </R.Panel>
            </div>}
            {AIType == ArtificialIntelligenceTermUseType.ApplyTerm && this.renderApplyExistOptions(ArtificialIntelligenceTermUseType.ApplyTerm)}
        </div>;
    }

    renderEnableRelated() {
        let requestVerificationToken = getRequestVerificationToken();
        let currNode = this.props.data;
        return this.props.context.configurations.enableRelatedRecords && (CRMCommonUtil.isSiteCollection(currNode) || CRMCommonUtil.isSite(currNode) || CRMCommonUtil.isTeams(currNode)) && <div className="ra-setting-panel-checkbox">
            <div className="ra-setting-panel-enableRecords">
                <R.Checkbox
                    id="raCrmDocLvlEnableRecordsChk"
                    text={RMResx.RM_SP_SettingRelatedRecords}
                    title={RMResx.RM_SP_SettingRelatedRecords}
                    checked={this.state.enableRelatedRecords}
                    onChange={this.onRelatedRecordsChanged}
                />
                <$g.Popover>{RMResx.RM_SP_Download_APPSolution}</$g.Popover>
            </div>
            {this.state.enableRelatedRecords && <div>
                <div className="ra-setting-panel-enable">
                    <form id="crm-document-download" method="post" action="">
                        <input type="hidden" id="documentDownloadFlag" name="documentDownloadFlag" value="" />
                        <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                    </form>
                    <span onClick={this.onDownloadRelatedApp} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_SP_Download_APPPackage}</span>
                </div>
            </div>}
        </div>;
    }

    render() {
        let useDefaultTerm = this.props.context.configurations.useDefaultTerm ? RMResx.RM_JS_SPS_AutoClassification_UseDefault : RMResx.RM_JS_EXO_SetPresetTerm;
        const isGroupNode = this.props.context.isGroupNode(this.props.data);
        let isSupportNullClassificationSettingFun = this.props.context.isSupportNullClassificationSetting;
        let isSupportNullClassificationSetting = isSupportNullClassificationSettingFun ? isSupportNullClassificationSettingFun(this.props.data) : false;
        const isHiddenInGCPEnv = this.props.context.configurations.isHiddenInGCPEnv;
        return <div id={this.props.id}>
            {isSupportNullClassificationSetting && isGroupNode &&
            <div className="ra-setting-panel-switch-enableTermSetting ">
                <div tabIndex="0">
                    {RMResx.RM_JS_SPS_EnableApplyTermSettingsTitle}
                </div>
                <R.Switch
                    id="raCrmExoEnableTermSettingSwitch"
                    checked={this.state.enableTermSettingStatus}
                        willChange={this.switchEnableTermSetting} />
                </div>}
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
                <div id="ra-crm-term-settings" className={isSupportNullClassificationSetting ? "margin-top-l" : ""}>
                    <R.Validation>
                        <div ref={r => this.allValidation = r}>
                            {this.props.context.configurations.columnName && <div className="ra-crm-form-content">
                                <div className="ra-setting-panel-title">{StringUtil.trimEndColon(RMResx.RM_JS_SPS_DocSettingTitle)}</div>
                                <div className="ra-setting-panel-column" role="region" aria-label={StringUtil.trimEndColon(RMResx.RM_JS_SPS_DocSettingTitle)} tabIndex="0">
                                    <span>{this.props.data.IsUsingExistColumnName ? this.existColumnName : this.createColumnName}</span>
                                </div>
                            </div>}

                            {!CRMCommonUtil.isFolder(this.props.data) && <div className="ra-crm-form-content">
                                <div className="require ra-setting-panel-title">{this.isFileSystemJPMC ? RMResx.RM_FS_Validation_ClassCodeScope : RMResx.RM_SPS_TermScope}</div>
                                <div id="termScopeId" className="ra-setting-panel-termtree">
                                    {this.props.context.isGroupNode(this.props.data) && <div className="ra-selectterms-searchbox">
                                        <R.Searchbox
                                            width={570}
                                            height={34}
                                            placeholder={RMResx.RM_JS_TM_SearchTxt}
                                            disabled={false}
                                            onSearch={this.onSearch}
                                        />
                                    </div>}
                                    <div className="margin-top-s margin-left-l">
                                        <R.ValidationFaker valid={this.customTermScopeValid} ref={r => this.refTermScopeValid = r} />
                                        <R.ValidationFaker valid={this.selectedTermScopePermissionValid} ref={r => this.refSelectedTermScopePermissionValid = r} />
                                    </div>
                                    {this.state.termDataLoaded &&
                                <div className="ra-setting-panel-treepadding">
                                    <CRMTeamTree
                                        ref={r => this.refTermScopeTree = r}
                                        searchKey={this.state.searchKey}
                                        data={this.state.savedTermTreeData}
                                        onSelectedNodeChanged={this.onTermScopeTreeChanged}
                                        onNodeLevel={this.state.documentLevel}
                                        sourceFlag={this.props.sourceFlag}
                                        containerId={this.props.context.getGroupNodeId(this.props.data)}
                                    ></CRMTeamTree>
                                </div>
                                    }
                                    <div className="margin-top-s margin-left-l">
                                        <R.ValidationFaker valid={this.termGroupPermissionValid} ref={r => this.refTermGroupPermissionValid = r} />
                                        <R.ValidationFaker valid={this.noTermScopeValid} ref={r => this.refNoTermScopeValid = r} />
                                    </div>
                                </div>
                            </div>}
                            {!CRMCommonUtil.isFolder(this.props.data) && this.props.context.configurations.termDisplayFormat && <div className="ra-crm-form-content">
                                <div className="require ra-setting-panel-title">
                                    <span id="ariaTermDisplay">{RMResx.RM_JS_SPS_EditKey_TermDisplayForm}</span>
                                </div>
                                <R.Radio.Group
                                    block={true}
                                    name="radiogroup-setting01"
                                    items={this.state.termFullPathItems}
                                    onChange={this.termFullPathChanged}
                                    aria="#ariaTermDisplay"
                                />
                            </div>}

                            {!this.isFileSystemJPMC && (<div className="ra-crm-form-content">
                                <div className="require ra-setting-panel-title">
                                    <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_SPS_AutoClassification_Method)} />
                                </div>
                                <div role="radiogroup" aria-label={StringUtil.trimEndColon(RMResx.RM_SPS_AutoClassification_Method)}>
                                    {this.props.getClassificationData() == ClassificationSettingType.FolderLevel && !isHiddenInGCPEnv && <div>
                                        <div className={this.props.context.configurations.applyTermIsShowTips ? "" : "margin-top-s"}>
                                            <R.Radio
                                                name="radio-applyterm"
                                                text={useDefaultTerm}
                                                value={DeployTermMethod.UseDefaultValue}
                                                checked={this.props.data.DeployTermMethod == DeployTermMethod.UseDefaultValue}
                                                onChange={this.deployTermMethodChanged}
                                            />
                                            {this.props.context.configurations.applyTermIsShowTips && <$g.Popover>{this.props.context.applyTermItemTips[1]}</$g.Popover>}
                                        </div>
                                    </div>}

                                    {this.props.getClassificationData() != ClassificationSettingType.FolderLevel && <div>
                                        {!isHiddenInGCPEnv && <div className={this.props.context.configurations.applyTermIsShowTips ? "" : "margin-top-s"}>
                                            <R.Radio
                                                name="radio-applyterm"
                                                text={RMResx.RM_JS_SPS_AutoClassification_NoDefaultValue}
                                                value={DeployTermMethod.NoDefaultValue}
                                                checked={this.props.data.DeployTermMethod == DeployTermMethod.NoDefaultValue}
                                                onChange={this.deployTermMethodChanged}
                                            />
                                            {this.props.context.configurations.applyTermIsShowTips && <$g.Popover>{this.props.context.applyTermItemTips[0]}</$g.Popover>}
                                        </div>}
                                        {!isHiddenInGCPEnv && <div className={this.props.context.configurations.applyTermIsShowTips ? "" : "margin-top-s"}>
                                            <R.Radio
                                                name="radio-applyterm"
                                                text={useDefaultTerm}
                                                value={DeployTermMethod.UseDefaultValue}
                                                checked={this.props.data.DeployTermMethod == DeployTermMethod.UseDefaultValue}
                                                onChange={this.deployTermMethodChanged}
                                            />
                                            {this.props.context.configurations.applyTermIsShowTips && <$g.Popover>{this.props.context.applyTermItemTips[1]}</$g.Popover>}
                                        </div>}
                                        {this.props.context.configurations.autoRuleDeploy && <div className={this.props.context.configurations.applyTermIsShowTips ? "" : "margin-top-s"}>
                                            <R.Radio
                                                name="radio-applyterm"
                                                text={RMResx.RM_JS_SPS_AutoClassification_UseRule}
                                                value={DeployTermMethod.UseAutoClassification}
                                                checked={this.props.data.DeployTermMethod == DeployTermMethod.UseAutoClassification}
                                                onChange={this.deployTermMethodChanged}
                                            />
                                            {this.props.context.configurations.applyTermIsShowTips && <$g.Popover>{this.props.context.applyTermItemTips[2]}</$g.Popover>}
                                        </div>}

                                        {this.enableAITerm && (this.props.data.Level !== 300 || (this.props.data.Level === 300 && this.props.data.NodeType !== 0))
                                            && <div className={"margin-top-s"}>
                                                <R.Radio
                                                    name="radio-applyterm"
                                                    text={RMResx.RM_MachineLearning_DeployTermMethodIntelligence}
                                                    value={DeployTermMethod.UseIntelligenceClassification}
                                                    checked={this.props.data.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification}
                                                    onChange={this.deployTermMethodChanged}
                                                />
                                            </div>}
                                    </div>}
                                </div>
                            </div>)}
                            {this.state.deployTermMethod == DeployTermMethod.UseDefaultValue && !this.isFileSystemJPMC && this.renderDefaultTerm()}
                            {this.state.deployTermMethod == DeployTermMethod.UseAutoClassification && !this.isFileSystemJPMC && this.renderAutoRule()}
                            {this.state.aiTermUseType == ArtificialIntelligenceTermUseType.ApplyTerm && !this.isFileSystemJPMC
                                && (this.props.data.Level !== 300 || (this.props.data.Level === 300 && this.props.data.NodeType !== 0))
                                && this.renderAITerm(ArtificialIntelligenceTermUseType.ApplyTerm)}
                            {RM.gData.enviromentName !== Enviroments.ChinaNorth && this.renderEnableRelated()}
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