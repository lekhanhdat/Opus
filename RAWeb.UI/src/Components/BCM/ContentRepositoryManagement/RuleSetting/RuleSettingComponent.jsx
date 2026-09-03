import { setCheckedStatus } from "../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../Common/CRMCommonUtil";
import { CRComponentType, SourceFlags } from "../../../../Constants/Constants";
import CreateRule from "../../../Common/RuleItem/CreateRule";
import RuleDetail from "../../../Common/RuleDetail/Index";
import "../../../../Less/BCM/tm.less";
import { NodeLevel } from "../../../../Constants/DAEnums";
import { RuleLevel } from "../../../Common/RuleItem/Components/Constants";
import { ObjectLevel, ObjectLevel4Teams, RuleLevelType } from "./context/RuleSettingConstants";
import { object } from "prop-types";
import { forEach } from "lodash";

export default class RuleSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            selectedRuleLevel: {},
            rulesGroupByLevel: {},
            termRulesGroupByLevel: {},
            createRuleUrl: "",
            ruleDetailId: null,
            currentRowRuleLevelId: '',
            showCreateRuleDialog: false,
            termSettingDisabled: false,
            itemSettingChanged: false,
            currentNode: props.currentNode,
            ruleLevelItems: this.getRuleLevelItems(),
            showRuleLevelOptions: false,
            lastAccessTimeCollection: ""
        };
        this.allRules = {};
        this.creatingTermRule = null;
        this.newTermRuleNum = -1;
        this.ruleLevels = this.getRuleLevelSource();
        this.createRuleComponentId = "raCreateRuleItem";
        window.RM.TM = { hideCreateRulePopup: this.onCreateNewRuleDialogClose };
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if(nextProps.availableRules.length != this.props.availableRules.length)
        {
            this.setAvailableRulesCache(nextProps.availableRules);
        }
    }

    componentDidMount() {
        this.setAvailableRulesCache();
        this.initSavedRules();
        this.loadLastAccessTimeCollectionData();
    }

    componentInit() {
        document.addEventListener('click', this.hideRuleLevelPopUp);
    }

    componentDestroy() {
        document.removeEventListener("click", this.hideRuleLevelPopUp, false);
    }

    initSavedRules = () => {
        const { Rules } = this.props?.currentNode;
        this.setState({ termRulesGroupByLevel: this.getTermRulesGroupsByLevel(Rules || []) }, () => {
            this.resetTermRulesOrder();
        });
    }

    loadLastAccessTimeCollectionData = () => {
        $$.loading(true);
        let url = "/api/RuleApi/GetLATEnableTime";
        let option = {
            url,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            this.setState({
                lastAccessTimeCollection: res
            })
        }).finally(() => $$.loading(false));
    }

    getObjectLevel = () => {
        if (this.props.sourceFlag === SourceFlags.Teams) {
            return ObjectLevel4Teams;
        } else {
            return ObjectLevel;
        }
    };

    getTermRulesGroupsByLevel(termRules) {
        let termRulesGroupByLevel = {};
        let objectLevels = this.getObjectLevel();
        for (let key in objectLevels) {
            termRulesGroupByLevel[objectLevels[key].value] = [];
        }
        for (let termRule of termRules) {
            termRule.Id = this.newTermRuleNum--;
            let ruleItem = this.allRules[termRule.RuleId];
            if (ruleItem) {
                termRulesGroupByLevel[ruleItem.RuleLevel].push(termRule);
            }
        }
        this.props.checkOrphanedMessagebar && this.props.checkOrphanedMessagebar(termRulesGroupByLevel);
        return termRulesGroupByLevel;
    }

    onRuleOperated = (data) => {
        this.props?.refreshRules(true);
        this.createdRule = data;
    }

    setAvailableRulesCache = (rules) => {
        const availableRules = rules ?? this.props.availableRules;
        this.allRules = {};
        let rulesGroup = {};
        let createdRule = this.createdRule ? RM.deepcopy(this.createdRule) : {};
        let objectLevels = this.getObjectLevel();
        for (let key in objectLevels) {
            rulesGroup[objectLevels[key].value] = [];
        }
        for (let rule of availableRules) {
            this.allRules[rule.RuleId] = rule;
            if (rule.RuleId === createdRule.RuleId && this.state.currentRowRuleLevelId == createdRule.RuleLevel) {
                this.creatingTermRule.RuleName = createdRule.RuleName;
                this.creatingTermRule.RuleId = createdRule.RuleId;
            }
        }
        for (let rule of availableRules) {
            rulesGroup[rule.RuleLevel].push(rule);
        }
        this.setState({ rulesGroupByLevel: rulesGroup, showCreateRuleDialog: false });
    }

    getRuleLevelSource() {
        var levelSource = [];
        let objectLevels = this.getObjectLevel();
        for (var key in objectLevels) {
            if (objectLevels[key]) {
                levelSource.push({ "name": objectLevels[key].name, "value": key });
            }
        }
        return levelSource;
    }

    getRuleLevelItems() {
        var levelSource = [];
        let siteCollectionIndex = 1;
        let teamsGroupIndex = 1;
        let siteIndex = 2;
        let listIndex = 3;
        let objectLevels = this.getObjectLevel();
        this.levelLists = [];
        for (var i in objectLevels) {
            let levelObj = Object.assign({ levelKey: i }, objectLevels[i]);
            this.levelLists.push(levelObj);
        }

        if (this.props.sourceFlag == SourceFlags.SP) {
            if (this.props.nodeLevel == NodeLevel.Site) {
                this.levelLists.splice(0, siteCollectionIndex);
            } else if (this.props.nodeLevel == NodeLevel.List) {
                this.levelLists.splice(0, siteIndex);
            } else if (this.props.nodeLevel == NodeLevel.Folder) {
                this.levelLists.splice(0, listIndex);
            }
        } else if (this.props.sourceFlag == SourceFlags.OneDrive) {
            // archiver onedrive hide sitecollection and site level
            if (this.props.nodeLevel == NodeLevel.WebApplication || this.props.nodeLevel == NodeLevel.SiteCollection) {
                this.levelLists.splice(1, siteCollectionIndex);
            } else if (this.props.nodeLevel == NodeLevel.Site || this.props.nodeLevel == NodeLevel.List) {
                this.levelLists.splice(0, siteIndex);
            } else if (this.props.nodeLevel == NodeLevel.Folder) {
                this.levelLists.splice(0, listIndex);
            }
        } else if (this.props.sourceFlag == SourceFlags.Teams) {
            if (this.props.nodeLevel == NodeLevel.SiteCollection) {
                this.levelLists.splice(0, teamsGroupIndex);
            } else if (this.props.nodeLevel == NodeLevel.Site) {
                this.levelLists.splice(0, siteIndex);
            } else if (this.props.nodeLevel == NodeLevel.List) {
                this.levelLists.splice(0, siteIndex + 1);
            } else if (this.props.nodeLevel == NodeLevel.Folder) {
                this.levelLists.splice(0, listIndex + 1);
            }
        }
        for (var key in this.levelLists) {
            if (this.levelLists[key]) {
                var ruleName = this.levelLists[key].name;
                levelSource.push(
                    {
                        checked: false,
                        name: ruleName,
                        value: this.levelLists[key].levelKey,
                        disabled: false,
                        tooltip: ruleName,
                        data: this.levelLists[key]
                    }
                );
            }
        }
        return levelSource;
    }

    handleRuleLevelChange = (args) => {
        let ruleLevel = args.newValue.value;
        let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
        let iLevel = args.newValue.data.value;
        let termRules = allTermRules[iLevel];
        termRules.push({ Id: this.newTermRuleNum--, RuleLevel: ruleLevel });
        this.setState({
            itemSettingChanged: true,
            termRulesGroupByLevel: allTermRules,
            showRuleLevelOptions: !this.state.showRuleLevelOptions
        }, () => {
            this.resetTermRulesOrder();
            this.props.checkOrphanedMessagebar && this.props.checkOrphanedMessagebar(allTermRules);
        });
    }

    handleShowRuleLevelList(disabledStatus, e) {
        if (disabledStatus) {
            e.preventDefault();
        } else {
            this.setState({ showRuleLevelOptions: !this.state.showRuleLevelOptions });
        }
        e.nativeEvent.stopImmediatePropagation();
    }

    hideRuleLevelPopUp = () => {
        this.setState({ showRuleLevelOptions: false });
    }

    existsTermRules() {
        let result = false;
        let allTermRules = this.state.termRulesGroupByLevel;
        for (let level in allTermRules) {
            let termRulesByLevel = allTermRules[level];
            if(termRulesByLevel && termRulesByLevel.length > 0) {
                result = true;
                break;
            }
        }
        return result;
    }

    resetTermRulesOrder = () => {
        let order = 1;
        let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);

        if(allTermRules.hasOwnProperty(RuleLevelType.Teams)){
            for (let rule of allTermRules[RuleLevelType.Teams]) {
                rule.RuleOrder = order++;
            }
        }

        for (let key in allTermRules) {
            if (key == RuleLevelType.Teams) { continue; }
            if (allTermRules[key]) {
                for (let rule of allTermRules[key]) {
                    rule.RuleOrder = order++;
                }
            }
        }
        this.setState({ termRulesGroupByLevel: allTermRules }, ()=> {
            $(".tbContent div[role='combobox']:eq(1)").focus();
        });
    }

    handleAddEmptyRuleClick = () => {
        let objectLevels = this.getObjectLevel();
        const ruleLevel = "Document";
        const allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
        const iLevel = objectLevels[ruleLevel].value;
        let termRules = allTermRules[iLevel];
        termRules.push({ Id: this.newTermRuleNum--, RuleLevel: ruleLevel });
        this.setState({
            itemSettingChanged: true,
            termRulesGroupByLevel: allTermRules,
        }, () => {
            this.resetTermRulesOrder();
        });
    }

    getTermRuleLevelValue(termRule) {
        let objectLevels = this.getObjectLevel();
        if (termRule.RuleId) {
            return this.allRules[termRule.RuleId].RuleLevel;
        } else {
            return objectLevels[termRule.RuleLevel].value;
        }
    }

    getTermRuleLevelName(ruleLevel) {
        switch (ruleLevel) {
            case 1:
                return RMResx.RM_JS_Rule_ObjectLevel_WebApplication;
            case 2:
                return RMResx.RM_JS_Rule_ObjectLevel_SiteCollection;
            case 4:
                return RMResx.RM_JS_Rule_ObjectLevel_Site;
            case 8:
                return RMResx.RM_JS_Rule_ObjectLevel_List;
            case 16:
                return RMResx.RM_JS_Rule_ObjectLevel_Folder;
            case 32:
                return RMResx.RM_JS_Rule_ObjectLevel_Item;
            case 64:
                return RMResx.RM_JS_Rule_ObjectLevel_Document;
            case 128:
                return RMResx.RM_JS_Rule_ObjectLevel_Attachment;
            case 256:
                return RMResx.RM_JS_Rule_ObjectLevel_DocumentVersion;
            case 512:
                return RMResx.RM_JS_Rule_ObjectLevel_ItemVersion;
            case 33554432:
                return RMResx.RM_JS_Rule_ObjectLevel_Teams;
            case 0:
            default:
                return RMResx.RM_JS_Rule_ObjectLevel_None;
        }
    }

    handleTermRuleOrderChanged = (args, termRule) => {
        let newItem = args.newValue;
        if (termRule.RuleOrder != newItem.RuleOrder) {
            let oldOrder = args.oldValue.RuleOrder;
            let newOrder = args.newValue.RuleOrder;
            let iLevel = this.getTermRuleLevelValue(termRule);
            let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
            let termRules = allTermRules[iLevel];
            for (let tr of termRules) {
                if (tr.Id == termRule.Id) {
                    tr.RuleOrder = newOrder;
                } else {
                    if (newOrder > oldOrder) {
                        if (newOrder >= tr.RuleOrder && tr.RuleOrder > oldOrder) {
                            tr.RuleOrder -= 1;
                        }
                    } else {
                        if (newOrder <= tr.RuleOrder && tr.RuleOrder < oldOrder) {
                            tr.RuleOrder += 1;
                        }
                    }
                }
            }
            termRules.sort((a, b) => a.RuleOrder > b.RuleOrder ? 1 : -1);
            this.setState({ itemSettingChanged: true, termRulesGroupByLevel: allTermRules });
            setTimeout(()=>{ this.forceUpdate();},100);
        }
    }

    handleTermRuleNameChanged = (args, termRule) =>{
        let newItem = args.newValue;
        let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
        let termRules = allTermRules[newItem.RuleLevel];
        let ruleItem = termRules.find(o => o.RuleId == termRule.RuleId);

        if (ruleItem.RuleId != newItem.RuleId) {
            ruleItem.RuleId = newItem.RuleId;
            ruleItem.RuleName = newItem.RuleName;
            this.setState({ itemSettingChanged: true, termRulesGroupByLevel: allTermRules }, () => {
                this.props.validRule && this.props.validRule();
            });
        }

    }

    onCreateNewRuleClick = (termRule, ruleLevel) =>{
        this.creatingTermRule = termRule;
        this.setState({
            currentRowRuleLevelId: ruleLevel
        });
        this.dispatch(this.createRuleComponentId, this.props.createRuleComponentType, this.getTreeNodeIdForRuleContainers(this.props.currentNode), ruleLevel, this.props.moduleType);
    }

    getTreeNodeIdForRuleContainers(currentNode) {
        switch (this.props.createRuleComponentType) {
            case CRComponentType.EXOSetting:
                return currentNode.Id;
            case CRComponentType.OnedriveSetting:
                return CRMCommonUtil.getGroupNode(currentNode).Id;
            case CRComponentType.SPSetting:
                return CRMCommonUtil.getGroupNode(currentNode).Id;
            case CRComponentType.TeamsSetting:
                return CRMCommonUtil.getGroupNode(currentNode).Id;
            case CRComponentType.LabelManagement:
                return CRMCommonUtil.getGoogleDriveContainerNode(currentNode).Id;
        }
    }

    onCreateNewRuleDialogClose = (e) => {
        this.setState({
            showCreateRuleDialog: false,
            createRuleUrl: ""
        });
        if (e == 1) {
            // this.getAvailableRuleList();
        }
    }

    onTermRuleViewClick = (termRule) => {
        this.ruleDetail.load({ ruleId: termRule.RuleId });
    }

    onTermRuleDelClick = (termRule) => {
        let iLevel = this.getTermRuleLevelValue(termRule);
        let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
        let termRules = allTermRules[iLevel];
        let trIndex = -1;
        for (let i = 0, len = termRules.length; i < len; i++) {
            let tr = termRules[i];
            if (tr.Id == termRule.Id) {
                trIndex = i;
            }
        }
        if (trIndex > -1) {
            termRules.splice(trIndex, 1);
            this.setState({ itemSettingChanged: true, termRulesGroupByLevel: allTermRules }, () => {
                this.resetTermRulesOrder();
            });
        }
        this.props.checkOrphanedMessagebar && this.props.checkOrphanedMessagebar(allTermRules);
        this.props.validRule && this.props.validRule();
    }

    termRulesAllHasRule() {
        let allHasRule = true;
        for (let level in this.state.termRulesGroupByLevel) {
            if (this.state.termRulesGroupByLevel[level]) {
                let termRules = this.state.termRulesGroupByLevel[level];
                if (termRules.length > 0) {
                    for (let tr of termRules) {
                        if (!tr.RuleName) {
                            allHasRule = false;
                        }
                    }
                }
            }
        }
        return allHasRule;
    }

    getTermRules() {
        let trList = [];
        let isValid = true;
        for (let level in this.state.termRulesGroupByLevel) {
            if (this.state.termRulesGroupByLevel[level]) {
                let termRules = this.state.termRulesGroupByLevel[level];
                if (termRules.length > 0) {
                    for (let tr of termRules) {
                        if (tr.RuleName) {
                            const { RuleId, RuleName, RuleOrder, IntRuleLevel } = tr;
                            trList.push({ RuleId, RuleName, RuleOrder, IntRuleLevel });
                        }else{
                            isValid = false;
                        }
                    }
                }
            }
        }
        return {trList, isValid};
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    renderTermRules() {
        let rulesGroups = [];
        let ordersGroups = {};
        let ruleOptionsOfLevels = {};
        let resetLevelOrder = [RuleLevelType.Teams, RuleLevelType.SiteCollection, RuleLevelType.Site, RuleLevelType.List, RuleLevelType.Folder, RuleLevelType.Item, RuleLevelType.Document, RuleLevelType.Attachment, RuleLevelType.DocumentVersion, RuleLevelType.ItemVersion];
        resetLevelOrder.forEach(level => {
            if (this.state.termRulesGroupByLevel.hasOwnProperty(level) && this.state.termRulesGroupByLevel[level]) {
                let termRules = this.state.termRulesGroupByLevel[level];
                if (termRules.length > 0) {
                    ordersGroups[level] = termRules;
                    let ruleIds = termRules.map((rule) => rule.RuleId);
                    let allRulesOfLevel = this.state.rulesGroupByLevel[level];
                    ruleOptionsOfLevels[level] = allRulesOfLevel.filter((rule) => {
                        return ruleIds.indexOf(rule.RuleId) < 0;
                    });
                    rulesGroups.push(termRules);
                }
            }
        });
        return rulesGroups.map((termRules, i) => {
            let ruleLevel = this.getTermRuleLevelValue(termRules[0]);
            let ruleLevelName = this.getTermRuleLevelName(ruleLevel);
            let ruleOrders = ordersGroups[ruleLevel];
            let ruleOptions = ruleOptionsOfLevels[ruleLevel];
            return <tr key={"tr_" + ruleLevel} className="trTable" style={{ display: "table-row" }}>
                <td colSpan="4" valign="top">
                    <table className="tbContent" cellPadding="0" cellSpacing="0">
                        <tbody>
                            {termRules.map((termRule, k) => {
                                let isNew = !termRule.RuleName;
                                let tempRuleOptions = RM.deepcopy(ruleOptions);
                                if (!isNew) {
                                    tempRuleOptions.push(termRule);
                                }
                                setCheckedStatus("RuleId", "Checked", tempRuleOptions, termRule);
                                let tempRuleOrders = setCheckedStatus("RuleOrder", "Checked", RM.deepcopy(ruleOrders), termRule);
                                return <tr key={k} className={"tm-rules-tr"}>
                                    <td className="cbOrder" style={{ minWidth: "50px", width: "15%" }}>
                                        <R.Combobox
                                            id="raTmRuleItemOrder"
                                            searchable={false}
                                            width="100%"
                                            height={32}
                                            // popupWidth="100%"
                                            disabled={this.state.termSettingDisabled || ruleOrders.length == 1 || isNew}
                                            textField='RuleOrder'
                                            valueField='RuleOrder'
                                            checkedField='Checked'
                                            excludeChecked
                                            items={tempRuleOrders}
                                            onChange={(args) => this.handleTermRuleOrderChanged(args, termRule)} />
                                    </td>
                                    <td style={{ width: "30%" }}>
                                        <div className="sp-level" tabIndex="0" data-tooltip aria-label={ruleLevelName}>{ruleLevelName}</div>
                                    </td>
                                    <td className="cbRule" style={{ width: "25%" }}>
                                        <R.Combobox
                                            id="raTmRuleName"
                                            width={"100%"}
                                            height={32}
                                            disabled={this.state.termSettingDisabled}
                                            textField='RuleName'
                                            valueField='RuleId'
                                            checkedField='Checked'
                                            excludeChecked
                                            // createNewText={RMResx.RM_JS_TM_CreateNewRule}
                                            items={tempRuleOptions}
                                            noneText={RMResx.RM_JS_TM_NoSelectRuleTip}
                                            // doCreateNew={() => this.onCreateNewRuleClick(termRule, ruleLevel)}
                                            onChange={(args) => this.handleTermRuleNameChanged(args, termRule)} />
                                    </td>
                                    <td className="tm-rule-actions" style={{ width: "30%" }}>
                                        <R.Button
                                            id="raTmRuleItemAddBtn"
                                            type="bald"
                                            icon="fia-plus icon-option-item"
                                            onClick={(e) => this.onCreateNewRuleClick(termRule, ruleLevel)}
                                            tooltip={RMResx.RM_JS_TM_CreateNewRule}
                                            className="margin-right-xs"
                                            disabled={this.state.termSettingDisabled}
                                        />
                                        <R.Button
                                            type="bald"
                                            icon="fia-eye icon-option-item"
                                            onClick={(e) => this.onTermRuleViewClick(termRule, ruleLevel)}
                                            disabled={isNew}
                                            tooltip={RMResx.RM_JS_TM_ViewRuleLabel}
                                            className="margin-right-xs"
                                        />
                                        <R.Button
                                            type="bald"
                                            icon="fia-delete icon-option-item"
                                            onClick={(e) => this.onTermRuleDelClick(termRule)}
                                            tooltip={RMResx.RM_JS_TM_RemoveRuleLabel}
                                            disabled={this.state.termSettingDisabled}
                                        />
                                    </td>
                                </tr>;
                            })}
                        </tbody>
                    </table>
                </td>
            </tr>;
        });
    }

    renderTermSettings() {
        this.ruleLevels = this.getRuleLevelSource();
        let isDisableOperation = this.state.termSettingDisabled;
        let isRequire = this.props.context.configurations.isRequire;
        let showRuleLevel = this.props.context.configurations.showRuleLevel;
        setCheckedStatus("value", "checked", this.ruleLevels, this.state.selectedRuleLevel);
        return  <div id="tmTermManagement">
            <div id="termSettings">
                <div id="divRule" className={isRequire ? "div-rule-top" : ""}>
                    <div>
                        <span
                            className={"tm-tree-right-form-label-font " + (isRequire ? "require" : "")}
                            id="ruleLabel"
                            tabIndex="0">
                            {this.props.context.setRuleTitle}
                        </span>
                    </div>
                </div>
                <div className="div-table-rule" style={{ display: "" }}>
                    <table id="tbMain" cellPadding="0" cellSpacing="0" tabIndex={this.existsTermRules()? 0 : -1}>
                        <tbody>
                            {this.renderTermRules()}
                        </tbody>
                    </table>
                </div>
                <div id="tm-rule-add-container">
                    <div id="tm_rule_add_content" className={isDisableOperation? "record-table-disabled": ""} 
                        role="combobox" 
                        aria-haspopup="listbox"
                        aria-expanded="false"
                        aria-disabled={isDisableOperation? "true" : "false"} 
                        onClick={showRuleLevel ? this.handleShowRuleLevelList.bind(this, isDisableOperation) : this.handleAddEmptyRuleClick} 
                        onKeyDown={this.onKeyDown} 
                        tabIndex="0" >
                        <div id="rule_add_icon" aria-hidden="true">
                            <div className="fia-plus"></div>
                        </div>
                        <span id="rule_add_text">{RMResx.RM_TM_Title_NewRule}</span>
                        {showRuleLevel && <span className="fia-triangle-down"></span>}
                    </div>
                    {showRuleLevel && this.state.showRuleLevelOptions && <div id="tm_rule_options">
                        <R.Selection
                            id="raTmRuleOptions"
                            items={this.state.ruleLevelItems}
                            disabled={false}
                            type="single"
                            textField="name"
                            valueField="value"
                            checkedField="checked"
                            tooltipField="tooltip"
                            disabledField="disabled"
                            searchable={false}
                            excludeChecked={false}
                            linkMode={false}
                            onChange={this.handleRuleLevelChange} />
                    </div>}
                </div>
            </div>
        </div>;
    }

    renderCreateRulePanel(){
        return <CreateRule id={this.createRuleComponentId} callback={this.onRuleOperated} history={this.props.history} lastAccessTimeCollection={this.state.lastAccessTimeCollection} />;
    }

    render() {
        return <>
            {this.renderTermSettings()}
            {this.renderCreateRulePanel()}
            <RuleDetail
                ref={r => this.ruleDetail = r}
            >
            </RuleDetail>
        </>;
    }
}