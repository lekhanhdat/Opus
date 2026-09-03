import "../../../Less/RDM/createRule.less";
import { levels, RuleSourceTabIndex, RuleLevel, RuleModuleTypes, RuleLevelOptions, SourcesByRuleLevel, RecordsRuleSources, ArchiveLevelOptionsHaveTeams, TeamsArchiveLevelOption } from "./Components/Constants";
import { EnvironmentHelper, LicenseHelper } from "../../../Utilities/CommonUtil";
import { CRComponentType } from "../../../Constants/Constants";
import { checkPermission } from "../../../Utilities/permissionManager";

const CopyOrNewRuleType = {
    Copy: "1",
    New: "2",
};

const allSourceOptions = [
    { name: RMResx.RM_JS_SPS_TabLabel_SP, value: RuleSourceTabIndex.SP, checked: false, isCheckApiKey: "IsSpSource" },
    { name: RMResx.RM_JS_SPS_TabLabel_OneDrive, value: RuleSourceTabIndex.OneDrive, checked: false, isCheckApiKey: "IsOneDriveSource" },
    { name: RMResx.RM_JS_SPS_TabLabel_EXO, value: RuleSourceTabIndex.Exchange, checked: false, isCheckApiKey: "IsExoSource" },
    { name: RMResx.RM_JS_SPS_TabLabel_Physical, value: RuleSourceTabIndex.Physical, checked: false, isCheckApiKey: "IsPhySource" },
    { name: RMResx.RM_JS_SPS_TabLabel_FS, value: RuleSourceTabIndex.FS, checked: false, isCheckApiKey: "IsFSSource" },
    { name: RMResx.RM_JS_SPS_TabLabel_SPLocal, value: RuleSourceTabIndex.SPLocal, checked: false, isCheckApiKey: "IsSPLocalSource" },
    { name: RMResx.RM_JS_Common_ReportType_AzureFile, value: RuleSourceTabIndex.AzureFile, checked: false, isCheckApiKey: "IsAzureFileSource" },
    { name: RMResx.RM_JS_SPS_TabLabel_Box, value: RuleSourceTabIndex.Box, checked: false, isCheckApiKey: "IsBoxSource" },
    { name: RMResx.RM_CP_Connector, value: RuleSourceTabIndex.Connector, checked: false, isCheckApiKey: "IsConnectorSource" },
];

const allSourceOptionsHaveTeams = [
    ...allSourceOptions,
    { name: RMResx.RM_JS_SPS_TabLabel_Teams, value: RuleSourceTabIndex.Teams, checked: false, isCheckApiKey: "IsTeamsSource" },
];

const defaultRuleModulesOptions = [
    { name: RMResx.RM_AR_SPS_TabControl_Information, value: RuleModuleTypes.Records, checked: true, show: LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusGoogleLicense() || !LicenseHelper.EnableRecordsArchiver() },
    { name: RMResx.RM_AR_SPS_TabControl_Storage, value: RuleModuleTypes.SOArchiver, checked: false, show: LicenseHelper.HasOpusSOLicense() && LicenseHelper.EnableRecordsArchiver() }
];

const defaultRuleLevels = levels.map((li, idx) => { return Object.assign(li, { Checked: idx == 0 }); });

export default class RuleBaseInfo extends R.Component {
    componentCreate() {
        const hasGoogleOption = allSourceOptions.some(option => option.value === RuleSourceTabIndex.GoogleDrive);
        const hasGooglePermission = checkPermission("Source_Google", RM.UserResources);
        if (hasGooglePermission && !hasGoogleOption) {
            allSourceOptions.push({
                name: RMResx.RM_JS_SPS_TabLabel_GoogleDrive, value: RuleSourceTabIndex.GoogleDrive, checked: false, isCheckApiKey: "IsGoogleDriveSource"
            })
        }
        const isGoogleLabel = !!(this.props.componentType == CRComponentType.LabelManagement);

        // Combine filtering and updating the checked value of GoogleDrive
        const updatedSourceOptions = RM.deepcopy(allSourceOptions)?.map(option => {
                if (option.value === RuleSourceTabIndex.GoogleDrive && isGoogleLabel) {
                    return { ...option, checked: isGoogleLabel }; // Update the checked property for GoogleDrive
                }
                return option;
        });
        this.levelId = RuleLevel.Document;
        this.levels = RM.deepcopy(defaultRuleLevels);
        this.ruleNameMaxLength = 255;
        this.descriptionAndDisposalClassMaxLength = 5000;
        this.selectedSourcesIndexs = [];
        this.selectedCopyRuleItem = {};
        this.selectedRuleContainerOption = RM.deepcopy(this.props.ruleContainerOptions).find((item) => { return item.Checked; }) || {};
        this.isCopyChecked = false;
        this.originalContainerId = "";
        this.sourcesByModuleType = RecordsRuleSources;
        this.selectedRuleModuleType = this.props.currentModuleType || RM.deepcopy(defaultRuleModulesOptions).filter((item)=> item.show)[0].value;
        this.isGoogleLabel = isGoogleLabel;
        this.state = {
            createRuleName: "",
            equalCopyName: false,
            description: "",
            disposalClass: "",
            termId: this.props.termId,
            ruleNameIsTooLong: false,
            desIsToLong: false,
            disposalClassToLong: "",
            disposalClassIsNoneValue: false,
            ruleModules: this.getRuleModules(),
            isShowCopyRule: false,
            createRuleType: CopyOrNewRuleType.New,
            rulesList: [],
            noRuleSelect: false,
            levels: this.levels,
            ruleContainerOptions: RM.deepcopy(this.props.ruleContainerOptions),
            elementDisabled: false,
            isEditRule: !!this.props.ruleId,
            currentLevelSourceOptions: this.getLevelSourceOptions(this.levelId).filter((item) => { return item.value != RuleSourceTabIndex.Physical; }),
            currentLevelSourceOptionsGoogle: updatedSourceOptions,
        };
    }

    componentReceive(callback) {
        this.onCreateRule(callback);
    }

    componentInit() {
        if(this.props.ruleId){
            this.echoRuleItem(this.props.ruleId);
        }else{
            this.initRuleItem(
                this.selectedRuleModuleType, 
                this.props.currentRowRuleLevelId || RuleLevel.Document, 
                true
            );
        }
    }

    getLevelOptions() {
        this.levels = RM.deepcopy(RuleLevelOptions)[this.selectedRuleModuleType];
        if (this.selectedRuleModuleType == RuleModuleTypes.SOArchiver && LicenseHelper.HasUpgradeTeams()) {
            this.levels.push(TeamsArchiveLevelOption);
        }
    }

    getLevelSourceOptions(levelId) {
        const hasTeamsOptions = LicenseHelper.HasUpgradeTeams() && LicenseHelper.EnableRecordsArchiver();
        const newAllSourceOptions = hasTeamsOptions && levelId === RuleLevel.Teams ? RM.deepcopy(allSourceOptionsHaveTeams) : RM.deepcopy(allSourceOptions);
        return newAllSourceOptions;
    }

    getRuleModules(){
        let ruleModules = RM.deepcopy(defaultRuleModulesOptions).filter((item)=> item.show);
        if(this.props.currentModuleType){
            return ruleModules.filter(item => item.value === this.props.currentModuleType);
        }
        return ruleModules;
    }

    validRuleBaseInfo() {
        let isValid = true;
        let ruleNameWithoutBlank = $.trim(this.state.createRuleName);
        let ruleNameIsTooLong = $.trim(this.state.createRuleName).length > this.ruleNameMaxLength;
        let desIsToLong = $.trim(this.state.description).length > this.descriptionAndDisposalClassMaxLength;
        let disposalClassToLong = $.trim(this.state.disposalClass).length > this.descriptionAndDisposalClassMaxLength;
        const disposalClassIsNoneValue = ['None', 'Aucun', '空', '없음', 'なし'].includes($.trim(this.state.disposalClass));
        let equalCopyName = this.isCopyChecked && (this.selectedCopyRuleItem.RuleName == this.state.createRuleName);
        let noSelectCopyRuleName = this.isCopyChecked && !(this.selectedCopyRuleItem?.RuleName);
        if (!ruleNameWithoutBlank) {
            isValid = false;
        }
        if (!this.selectedRuleContainerOption.ContainerId) {
            isValid = false;
        }
        if (ruleNameIsTooLong || desIsToLong || disposalClassToLong || disposalClassIsNoneValue) {
            isValid = false;
        }
        if (this.selectedSourcesIndexs.length == 0  && !this.isGoogleLabel) {
            isValid = false;
        }
        if (equalCopyName) {
            isValid = false;
        }
        if(!this.selectedRuleModuleType){
            isValid = false;
        }
        if(noSelectCopyRuleName){
            isValid = false;
        }
        this.setState({
            ruleNameIsTooLong: ruleNameIsTooLong,
            desIsToLong: desIsToLong,
            disposalClassToLong: disposalClassToLong,
            disposalClassIsNoneValue,
            equalCopyName: equalCopyName
        });
        return isValid;
    }

    onCreateRule(callback) {
        callback({
            isValid: this.validRuleBaseInfo(),
            baseInfo: this.getRuleBaseInfoParam()
        });
    }

    getRuleBaseInfoParam() {
        if (!this.isCopyChecked) { this.selectedCopyRuleItem = {}; }
        let baseInfoParam = {
            RuleName: this.state.createRuleName,
            RuleLevel: this.levelId,
            Description: this.state.description,
            DisposalClass: this.state.disposalClass,
            ContainerId: this.selectedRuleContainerOption.ContainerId,
            RuleModuleType: this.selectedRuleModuleType,
            selectedSourcesIndexs: this.selectedSourcesIndexs,
            isCopyChecked: this.isCopyChecked,
            selectedCopyRuleId: this.selectedCopyRuleItem.RuleId,
            originalContainerId: this.originalContainerId
        };
        return baseInfoParam;
    }

    onChangeRuleName = (value) => {
        this.setState({ createRuleName: value }, () => { this.validRuleBaseInfo(); });
        this.hasChanged = true;
    }

    onChangeDescription = (value) => {
        this.setState({ description: value }, () => { this.validRuleBaseInfo(); });
        this.hasChanged = true;
    }

    disposalClassChange = (value) => {
        this.setState({ disposalClass: value }, () => { this.validRuleBaseInfo(); });
        this.hasChanged = true;
    }

    clearBaseInfo() {
        this.setState({
            createRuleName: "",
            equalCopyName: false,
            description: "",
            disposalClass: "",
            ruleModules: this.getRuleModules(),
        });
    }

    onCopyOrCreateRuleRadioChange = (value) => {
        this.isCopyChecked = value == CopyOrNewRuleType.Copy;
        if (this.isCopyChecked) {
            this.setRulesList(value);
        } else {
            this.initRuleItem(
                this.props.currentModuleType || RM.deepcopy(defaultRuleModulesOptions).filter((item)=> item.show)[0].value, 
                this.props.currentRowRuleLevelId || RuleLevel.Document, 
                true
            );
            this.clearBaseInfo();
        }
        this.setState({
            createRuleType: value,
            elementDisabled: this.isCopyChecked
        });
    }

    setRulesList() {
        $$.loading(true);
        let urlData = `/api/RuleApi/GetCanCopyRulesByTermId?termId=${this.props.termId || 0}&moduleType=${this.props.currentModuleType || -1}`;     //-1 rule management copy
        if (this.isGoogleLabel) {
            urlData = `/api/RuleApi/GetCanCopyRulesForDisableClassification`; // google label
        }
        let option = {
            url: urlData,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            this.setState({ rulesList: JSON.parse(res) });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onRuleSelectChange = (args) => {
        this.selectedCopyRuleItem = args.newValue;
        this.setState({
            // createRuleName: "",
            elementDisabled: false
        });
        this.props.copyRule();
        this.echoRuleItem(this.selectedCopyRuleItem.RuleId);
    }

    echoRuleItem(ruleItemId) {
        $$.loading(true);
        let url = "/api/RuleApi/GetRuleByID";
        let option = {
            url: url,
            method: "POST",
            data: ruleItemId
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.selectedSourcesIndexs = [];
            let currentRuleItemInfo = JSON.parse(res.Extension);
            this.selectedRuleModuleType = currentRuleItemInfo.ModelType; 
            this.getLevelOptions();
            this.sourcesByModuleType = RM.deepcopy(SourcesByRuleLevel)[this.selectedRuleModuleType];
            let currentLevelSourceOptions = this.getSourceOptionsByLevel(currentRuleItemInfo.RuleLevel);
            let ruleContainerOptions = this.state.ruleContainerOptions;
            this.originalContainerId = currentRuleItemInfo.ContainerId;
            for (let item of this.levels) {
                item.Checked = item.id == currentRuleItemInfo.RuleLevel;
            }
            for (let item of currentLevelSourceOptions) {
                item.checked = currentRuleItemInfo[item.isCheckApiKey];
                if (item.checked) {
                    this.selectedSourcesIndexs.push(item.value);
                }
            }
            for (let item of ruleContainerOptions) {
                item.Checked = item.ContainerId == currentRuleItemInfo.ContainerId;
                if (item.ContainerId == currentRuleItemInfo.ContainerId) {
                    this.selectedRuleContainerOption = item;
                }
            }
            for (let item of this.state.ruleModules) {
                item.checked = item.value == this.selectedRuleModuleType;
            }

            if (!this.isCopyChecked) {
                this.setState({
                    createRuleName: currentRuleItemInfo.RuleName,
                });
            }

            this.setState({
                // createRuleName: this.isCopyChecked ? "" : currentRuleItemInfo.RuleName,
                description: currentRuleItemInfo.Description,
                disposalClass: currentRuleItemInfo.DisposalClass,
                levels: this.levels,
                currentLevelSourceOptions: currentLevelSourceOptions,
                ruleContainerOptions: RM.deepcopy(ruleContainerOptions),
                ruleModules: RM.deepcopy(this.state.ruleModules)
            });
            currentRuleItemInfo.selectedSourcesIndexs = this.selectedSourcesIndexs;
            this.dispatch("raRuleSourceAndRuleSetting", "EchoRuleSettingData", currentRuleItemInfo);
        });
    }

    onChangeRuleContainer = (args) => {
        this.selectedRuleContainerOption = args.newValue;
    }

    onChangeRuleModules = (args) => {
        this.initRuleItem(args.newValue.value, this.levels[0].id, true);
    }

    initRuleItem = (selectedRuleModuleType, selectLevelId, isRender) => {
        this.selectedRuleModuleType = selectedRuleModuleType;
        // this.levels = RuleLevelOptions[this.selectedRuleModuleType];
        this.getLevelOptions();
        this.sourcesByModuleType = SourcesByRuleLevel[this.selectedRuleModuleType];
        this.onChangeLevelSelected(selectLevelId, isRender);
    }

    onChangeLevelSelected = (levelId, isRender) => {
        let ruleLevels = RM.deepcopy(this.levels);
        this.selectedSourcesIndexs = [];
        let currentLevelSourceOptions = this.getSourceOptionsByLevel(levelId);
        for (let item of ruleLevels) { item.Checked = item.id == levelId; }
        this.setState({
            currentLevelSourceOptions: currentLevelSourceOptions,
            levels: RM.deepcopy(ruleLevels)
        });
        if (isRender) {
            this.props.reRender(this.selectedRuleModuleType, levelId);
        }
    };

    getSourceOptionsByLevel(levelId) {
        const sourceOptions = this.getLevelSourceOptions(levelId);
        let currentLevelSourceOptions = sourceOptions.filter((item) => {
            return this.sourcesByModuleType[levelId].includes(item.value);
        });
        // hide File System and SP On-Premises option on GCP env
        if (EnvironmentHelper.IsGCPEnvironment) {
            currentLevelSourceOptions = currentLevelSourceOptions.filter(item => item.value != RuleSourceTabIndex.FS && item.value != RuleSourceTabIndex.SPLocal);
        }
        return currentLevelSourceOptions;
    }

    onChangeSources = (args) => {
        this.selectedSourcesIndexs = args.newValue.map((item) => { return item.value; }).sort((a, b) => Number(a) - Number(b));
        this.dispatch("raRuleSourceAndRuleSetting", "InitRuleSettingSources", this.selectedSourcesIndexs);
    }

    renderCreateRuleName() {
        let ruleNameTooLongValidMsg = RMResx.RM_JS_RDM_CreateRule_Validation_RuleNameTooLong.format(this.ruleNameMaxLength);
        return <$g.FormRow label={RMResx.RM_RDM_CreateRule_Title_RuleName.replace(":", "")} require>
            <R.Validation element="Input" require={RMResx.RM_JS_RDM_CreateRule_Validation_noRuleName}>
                <R.Input
                    id="raCrRuleName"
                    disabled={this.state.isEditRule || this.state.elementDisabled}
                    onChange={this.onChangeRuleName}
                    value={this.state.createRuleName}
                    aria={{ ariaLabel: RMResx.RM_RDM_CreateRule_Title_RuleName.replace(":", "") }}
                />
            </R.Validation>
            <R.ValidationFaker valid={!this.state.ruleNameIsTooLong} of="#raCrRuleName" message={ruleNameTooLongValidMsg} />
            <R.ValidationFaker valid={!this.state.equalCopyName} message={RMResx.RM_JS_RDM_CreateRule_Validation_EqualCopyName} />
        </$g.FormRow>;
    }

    renderDescription() {
        return <$g.FormRow label={RMResx.RM_RDM_CreateRule_Title_Description.replace(":", "")}>
            <R.Validation>
                <R.Input
                    id="raCrDescription"
                    type="textarea"
                    value={this.state.description}
                    disabled={this.state.elementDisabled}
                    onChange={this.onChangeDescription}
                    aria={{ ariaLabel: RMResx.RM_RDM_CreateRule_Title_Description.replace(":", "") }}
                />
                <R.ValidationFaker valid={!this.state.desIsToLong} of="#raCrDescription" 
                message={RMResx.RM_TM_CustomProperties_NameLengthLimit} />
            </R.Validation>
        </$g.FormRow>;
    }

    renderCreateAndCopyRule() {
        return <$g.FormRow>
            <$g.RadioGroup
                name="cr-create-rule-type"
                value={this.state.createRuleType}
                onChange={this.onCopyOrCreateRuleRadioChange}>
                <$g.RadioOption value={CopyOrNewRuleType.New} text={RMResx.RM_JS_RDM_CreateRule_Options_CreateNew} />
                <$g.RadioOption value={CopyOrNewRuleType.Copy} text={RMResx.RM_JS_RDM_CreateRule_Options_CopyRule}>
                    {
                        this.state.createRuleType == CopyOrNewRuleType.Copy && <div className="ra-rule-copy-combobox">
                            <R.Validation element="Combobox" require={RMResx.RM_JS_RDM_CreateRule_Validation_noRuleSelect}>
                                <R.Combobox
                                    id="raCrCreateAndCopy"
                                    width={"100%"}
                                    textField='RuleName'
                                    valueField='RuleId'
                                    checkedField='Checked'
                                    items={this.state.rulesList}
                                    onChange={this.onRuleSelectChange}
                                    searchPlaceholder={RMResx.RM_JS_RC_RUR_SelectRuleDefault}
                                />
                            </R.Validation>
                        </div>
                    }
                </$g.RadioOption>
            </$g.RadioGroup>
        </$g.FormRow>;
    }

    renderRuleContainer() {
        return <$g.FormRow label={RMResx.RM_JS_Rule_Detail_Container} require id="ariaRuleContainer">
            <R.Validation element="Combobox" require={RMResx.RM_JS_Rule_Detail_ContainerNotSelectedTip}>
                <R.Combobox
                    id="raCrContainer"
                    width={"100%"}
                    textField='Name'
                    valueField='ContainerId'
                    checkedField='Checked'
                    disabled={this.state.elementDisabled}
                    items={this.state.ruleContainerOptions}
                    onChange={this.onChangeRuleContainer}
                    aria="#ariaRuleContainer"
                />
            </R.Validation>
        </$g.FormRow>;
    }

    renderModules() {
        return <$g.FormRow label={RMResx.RM_RDM_CreateRule_Title_Module} require id="ariaRuleModule">
            <R.Validation element="Combobox" require={RMResx.RM_RDM_CreateRule_Validation_NoSelectModule}>
                <R.Combobox
                    id="raCrModules"
                    width={"100%"}
                    textField='name'
                    valueField='value'
                    checkedField='checked'
                    disabled={this.state.isEditRule || this.state.elementDisabled}
                    items={this.state.ruleModules}
                    onChange={this.onChangeRuleModules}
                    aria="#ariaRuleModule"
                />
            </R.Validation>
        </$g.FormRow>;
    }

    renderRuleLevelSelection() {
        return <$g.FormRow label={RMResx.RM_RDM_CreateRule_Title_ObjectLevel} require id="ariaRuleLevel">
            <R.Combobox
                id="raCrRuleLevel"
                width={"100%"}
                textField='Name'
                valueField='id'
                checkedField='Checked'
                disabled={this.state.elementDisabled}
                items={this.state.levels}
                onChange={(args) => { this.onChangeLevelSelected(args.newValue.id, true); }}
                aria={{
                    ariaLabelledby: "ariaRuleLevel",
                    ariaRequired: true
                }}
            />
        </$g.FormRow>;
    }

    renderDisposalClass() {
        return <div className="ra-disposal-form-row">
            <$g.FormRow
                label={RMResx.RM_RDM_CreateRule_Title_DisposalClass}
                tipMsg={RMResx.RM_JS_Rule_DisposalClass_Description}
            >
                <R.Validation>
                    <R.Input
                        id="raCrDisposalClass"
                        type="textarea"
                        value={this.state.disposalClass}
                        disabled={this.state.elementDisabled}
                        onChange={this.disposalClassChange}
                        aria={{ ariaLabel: RMResx.RM_RDM_CreateRule_Title_DisposalClass }}
                    />
                    <R.ValidationFaker valid={!this.state.disposalClassToLong} of="#raCrDisposalClass"
                     message={RMResx.RM_TM_CustomProperties_NameLengthLimit}/>
                     <R.ValidationFaker valid={!this.state.disposalClassIsNoneValue} of="#raCrDisposalClass" message={RMResx.RM_MA_ApprovalComment_NotAllowNoneInput} />
                </R.Validation>
            </$g.FormRow>
        </div>;
    }

    renderSources() {
        return <$g.FormRow label={RMResx.RM_RDM_CR_Source} require>
            <R.Validation element="Multicombobox" require={RMResx.RM_RDM_CR_Source_NotSelectedMsg}>
                <R.Multicombobox
                    id="raCrSource"
                    width={"100%"}
                    disabled={this.state.elementDisabled}
                    textField="name"
                    items={this.state.currentLevelSourceOptions}
                    disabledField="disabled"
                    hasSelectAll={true}
                    searchable={false}
                    onChange={this.onChangeSources}
                    clearable={true}
                    aria={{ ariaLabel: RMResx.RM_RDM_CR_Source }}
                />
            </R.Validation>
        </$g.FormRow>;
    }

    render() {
        return <div id="raRuleBaseInfo">
            {this.renderCreateRuleName()}
            {this.renderDescription()}
            {!this.state.isEditRule && this.renderCreateAndCopyRule()}
            {this.renderRuleContainer()}
            {this.state.ruleModules.length > 1 && this.renderModules()}
            {this.renderRuleLevelSelection()}
            {this.renderDisposalClass()}
            {this.renderSources()}
        </div>;
    }
}