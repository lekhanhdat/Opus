import RuleBaseInfo from "../../Common/RuleItem/RuleBaseInfo";
import RuleSourceAndRuleSetting from "../../Common/RuleItem/RuleItem";
import { RuleLevel, RuleModuleTypes, RuleSourceTabIndex } from "./Components/Constants";
import { CRComponentType } from "../../../Constants/Constants";
import { LicenseHelper } from "../../../Utilities/CommonUtil";

export default class CreatRule extends R.Component {
    idAttr = true;
    componentCreate() {
        this.bind(["onClose", "onClickNext", "onBack", "onCreateRule" ]);
        this.showInfoType ={ 
            baseInfo: "1", 
            sourceAndRuleSetting: "2"
        };

        this.baseInfoFootButtons = <>
            <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Cancel}
                onClick={this.onClose}
            />
            <R.Button
                slot="buttons"
                text={RMResx.RM_CP_Agent_Remind_Next}
                primary={true}
                classify="theme"
                onClick={this.onClickNext}
            />
        </>;
        this.sourceAndRuleSettingFootButtons = <>
            <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Cancel}
                onClick={this.onClose}
            />
            <R.Button
                slot="buttons"
                text={RMResx.RM_CP_Agent_Remind_Back}
                primary={false}
                classify="default"
                onClick={this.onBack}
            />
            <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Save}
                primary={true}
                classify="theme"
                onClick={this.onCreateRule}
            />
        </>;
        
        this.state = {
            ruleId: null,
            showCreateRulePanel: false,
            panelFootButtons: this.baseInfoFootButtons,
            showInfoTab: this.showInfoType.baseInfo,
            selectedRowRuleLevelId: "", //term management create rule 传入ruleLevelId
            componentKey: 0,
            panelTitle: RMResx.RM_RC_Audit_Action_CreateRule,
            workflowItems: [],
            ruleContainerOptions: [],
            hasRecenter: false,
            storagePolicyList: [],
            levelStubSettingList: [],
            isNestleCustomize: false,
            moduleType: RuleModuleTypes.None,
            indexDeviceId: "",
        };
    }

    componentInit(){
        !LicenseHelper.HasOpusSOLicenseOnly() && this.initWorkflowComboboxItems();
        this.checkHaveRecenter();
        this.loadStorageList();
        !LicenseHelper.HasOpusGoogleLicenseOnly() && this.loadLevelStubSettingList();
        this.checkIsNestleCustomize();
    }

    componentReceive(type, data, data2, moduleType) {
        this.ComponentType = type;
        this.ScopeContainerId = data;
        this.setRuleContainerOptions(()=>{
            if(type == CRComponentType.RM){
                this.initCreateRuleForm(data);
            }
            if(type == CRComponentType.TM){
                this.initCreateRuleForm("", data, RuleModuleTypes.Records);
            }
            if(type == CRComponentType.EXOSetting){
                this.initCreateRuleForm("", RuleLevel.Document, moduleType);
            }
            if(type == CRComponentType.OnedriveSetting){
                this.initCreateRuleForm("", data2, moduleType);
            }
            if(type == CRComponentType.SPSetting){
                this.initCreateRuleForm("", data2, moduleType);
            }
            if(type == CRComponentType.TeamsSetting){
                this.initCreateRuleForm("", data2, moduleType);
            }
            if(type == CRComponentType.LabelManagement){
                this.initCreateRuleForm("", data2, moduleType);
            }
        });
    }

    checkHaveRecenter(){
        $$.loading(true);
        let urlData = "/api/RuleApi/CheckHaveRecenter";
        let option = {
            url: urlData,
            method: "Get"
        };
        fetchUtility(option).then((res) => {
            this.setState({hasRecenter: res});
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        }); 
    }

    loadStorageList = async () => {
        $$.loading(true);
        let requestOptions = {
            url: "/api/StorageDevice/GetAllActiveStorage",
            method: "Post",
            data: {
                PageIndex: -1,
                PageSize: 10,
                SearchValue: "",
                TotalNumber: 0
            }
        };
        let res = await fetchUtility(requestOptions);
        $$.loading(false);
        let storageDeviceUIDtosList = res.StorageDeviceUIDtosList;
        this.setState({
            storagePolicyList: storageDeviceUIDtosList,
            indexDeviceId: res.IndexDeviceId,
        });
    };

    loadLevelStubSettingList = async () => {
        $$.loading(true);
        let requestOptions = {
            url: "/api/StubSetting/GetAllStubSettingsNotPaged",
        };
        let res = await fetchUtility(requestOptions);
        $$.loading(false);
        this.setState({
            levelStubSettingList: res
        });
    };


    checkIsNestleCustomize () {
        $$.loading(true);
        let option = {
            url: "/api/RuleApi/CheckIsNestleCustomize",
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            this.setState({ isNestleCustomize: res });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    initCreateRuleForm(ruleId, ruleLevelId, moduleType){
        this.setState({ 
            ruleId: ruleId || "",
            showCreateRulePanel: true,
            showInfoTab: this.showInfoType.baseInfo,
            panelFootButtons: this.baseInfoFootButtons,
            selectedRowRuleLevelId: ruleLevelId || "",
            panelTitle: ruleId ? RMResx.RM_RC_Audit_Action_EditRule : RMResx.RM_RC_Audit_Action_CreateRule,
            ruleContainerOptions: this.ruleContainerOptions,
            moduleType: moduleType
        });
    }

    onClickNext(){
        $$.verify("raCreateRulePanel");
        let isToSourceAndRuleSetting = false;
        this.dispatch("raRuleBaseInfo", (data) => { 
            isToSourceAndRuleSetting = data.isValid;
            this.baseInfo = data.baseInfo;
        });
        if(isToSourceAndRuleSetting){
            this.setState({ 
                panelFootButtons: this.sourceAndRuleSettingFootButtons,
                showInfoTab: this.showInfoType.sourceAndRuleSetting,
            });
        }
        return false;
    }

    onBack(){
        this.setState({ 
            showInfoTab: this.showInfoType.baseInfo,
            panelFootButtons: this.baseInfoFootButtons
        });
        return false;
    }

    onClose(){
        this.props.onClose && this.props.onClose();
        this.setState({ showCreateRulePanel: false });
    }

    onCreateRule(){
        this.props.onSave && this.props.onSave();
        this.dispatch("raRuleSourceAndRuleSetting", "CreateRule", this.baseInfo);
        return false;
    }

    onOperated = (data) =>{
        this.setState({ showCreateRulePanel: false });
        this.props.callback(data);
    }

    initWorkflowComboboxItems(){
        $$.loading(true);
        let urlData = "/api/RuleApi/GetAllWorkflows";
        let option = {
            url: urlData,
            method: "GET"
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let data = JSON.parse(res);
            this.setState({ workflowItems: data });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getLoadRuleContainerRequestUrl = (type) => {
        const mapping = {
            [CRComponentType.RM]: "/api/RuleApi/GetAllRuleContainers",
            [CRComponentType.TM]: `/api/RuleApi/GetRuleContainersByTermId?termId=${this.props.termId}`,
            [CRComponentType.EXOSetting]: `/api/RuleApi/GetRuleContainersByContainerId?containerId=${this.ScopeContainerId}&sourceFlag=3`,
            [CRComponentType.OnedriveSetting]: `/api/RuleApi/GetRuleContainersByContainerId?containerId=${this.ScopeContainerId}&sourceFlag=6`,
            [CRComponentType.SPSetting]: `/api/RuleApi/GetRuleContainersByContainerId?containerId=${this.ScopeContainerId}&sourceFlag=1`,
            [CRComponentType.TeamsSetting]: `/api/RuleApi/GetRuleContainersByContainerId?containerId=${this.ScopeContainerId}&sourceFlag=11`,
            [CRComponentType.LabelManagement]: "/api/RuleApi/GetRuleContainersForLabel"
        };
        return mapping[type] ?? "";
    }

    setRuleContainerOptions(callback){
        $$.loading(true);
        let url = this.getLoadRuleContainerRequestUrl(this.ComponentType);
        let option = {
            url: url,
            method: "Get",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.ruleContainerOptions = JSON.parse(res);
            callback();
        });
    }

    getCurrentRuleContainerOptions(){
        let currentRuleContainerOptions = RM.deepcopy(this.state.ruleContainerOptions);
        for(let item of currentRuleContainerOptions){
            item.Checked = item.ContainerId == this.props.containerId;
        }
        return currentRuleContainerOptions;
    }

    renderCreateProgressBar(){
        let isBaseInfoTab = this.state.showInfoTab == this.showInfoType.baseInfo;
        let sourceAndRuleTab = this.state.showInfoTab == this.showInfoType.sourceAndRuleSetting;
        let progressBarLeftClass = `progress-bar-background-left ${isBaseInfoTab ? "" : "progress-bar-bgc-blank"}`;
        let progressBarRightClass = `progress-bar-background-right ${sourceAndRuleTab ? "" : "progress-bar-bgc-blank"}`;
        return <React.Fragment>
            <div className="cr-progress-bar-background">
                <div className={progressBarLeftClass}></div>
                <div className={progressBarRightClass}></div>
            </div>
            <div className="cr-progress-text">
                <div className="progress-bar-text">{RMResx.RM_RDM_CR_BasicInfo.format("1")}</div>
                <div className="progress-bar-text">{RMResx.RM_RDM_CR_RuleSetting.format("2")}</div>
            </div>
        </React.Fragment>;
    }

    reRender = (moduleType, levelId) =>{
        this.setState({componentKey: Math.random()},()=>{
            this.dispatch("raRuleSourceAndRuleSetting","InitRuleSettingByLevel", { moduleType, levelId } );
        });
    }

    copyRule = () =>{
        this.setState({componentKey: Math.random()});
    }

    renderRuleBaseInfo(){
        let isShowBaseInfoClass = 
            this.state.showInfoTab == this.showInfoType.baseInfo ?  "block"  : "none";
        let ruleContainerOptions = this.getCurrentRuleContainerOptions();
        return <div className={isShowBaseInfoClass}>
            <RuleBaseInfo 
                id="raRuleBaseInfo" 
                ruleId={this.state.ruleId} 
                currentRowRuleLevelId={this.state.selectedRowRuleLevelId}
                ruleContainerOptions={ruleContainerOptions}
                reRender={this.reRender}
                copyRule={this.copyRule}
                termId={this.props.termId}
                currentModuleType={this.state.moduleType}
                componentType={this.ComponentType}
            />
        </div>;
    }

    renderRuleSourceAndRuleSetting(){
        let isShowSourceAndRuleSetting = 
            this.state.showInfoTab == this.showInfoType.sourceAndRuleSetting ?  "block"  : "none";
        return <div className={isShowSourceAndRuleSetting} key={this.state.componentKey}>
            <RuleSourceAndRuleSetting 
                id={"raRuleSourceAndRuleSetting"}
                hasRecenter={this.state.hasRecenter}
                isNestleCustomize={this.state.isNestleCustomize}
                onOperated={this.onOperated}   
                ruleId={this.state.ruleId} 
                currentRowRuleLevelId={this.state.selectedRowRuleLevelId}
                history={this.props.history}
                workflowItems={this.state.workflowItems}
                storagePolicyList={this.state.storagePolicyList}
                indexDeviceId={this.state.indexDeviceId}
                levelStubSettingList={this.state.levelStubSettingList}
                lastAccessTimeCollection={this.props.lastAccessTimeCollection}
                onRefetchStubSettingList={this.loadLevelStubSettingList}
            />         
        </div>;
    }
        
    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <R.Panel
                    header={this.state.panelTitle}
                    size={664}
                    id="raCreateRulePanel"
                    status={{ show: this.state.showCreateRulePanel }}
                    onHide={this.onClose.bind(this)}
                    destroy={true}
                >
                    <div>
                        {this.renderCreateProgressBar()}
                        {this.renderRuleBaseInfo()}
                        {this.renderRuleSourceAndRuleSetting()}
                    </div>
                    {this.state.panelFootButtons}
                </R.Panel>
            </R.Validation>
        </div>;
    }
}