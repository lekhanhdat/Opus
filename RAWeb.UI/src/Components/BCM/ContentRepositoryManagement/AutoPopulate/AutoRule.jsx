import StringUtil from "../../../../Utilities/StringUtil";
import AutoFilterGroup from "./AutoFilterGroup";
import SelectTermTree from "../../../Common/Tree/Instances/TermTree/SelectTermTree";
import CRMCommonUtil from "../Common/CRMCommonUtil";
import TermStatusForInputText from "../DocumentTermSetting/StatusTermInputText";
export default class AutoRule extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.topFilterGroupContents = {};
        this.accordionValidationContent = [];
        this.accordionTermStatusContent = [];
        this.accordionChangeTermGroupContent = [];
        this.accordionIfContent = [];
        this.autoRules = [...this.props.data];
        let tempShowSelectTermPanel = {};
        this.autoRules.forEach((autoRule) => {
            autoRule.UniqueId = StringUtil.newGuid();
            autoRule.FilterGroups.forEach(filterGroup => {
                filterGroup.UniqueId = StringUtil.newGuid();
            });
            tempShowSelectTermPanel[autoRule.UniqueId] = { show: false };
        });
        
        this.state = {
            autoRules: this.autoRules,
            showSelectTermPanel: tempShowSelectTermPanel,
        };
    }
    componentInit() {
        if (this.autoRules.length == 0) {
            this.addDefaultRule();
            this.addRule();
        }
    }

    clearAutoRuleTerm() {
        this.autoRules.forEach(rule => {
            rule.TermName = "";
            rule.TermId = CRMCommonUtil.GuidEmpty;
        });
        this.setState({ autoRules: [...this.autoRules] });
    }

    getAutoRuleData(){
        this.autoRules.forEach(rule => {
            const filterGroupContent = this.topFilterGroupContents[rule.UniqueId];
            let topFilterGroups = [];
            for (const filterGroupUniqueId in filterGroupContent) {
                if (Object.hasOwnProperty.call(filterGroupContent, filterGroupUniqueId)) {
                    const content = filterGroupContent[filterGroupUniqueId];
                    if (content) {
                        let filterGroup = content.getFilterGroupData({ index: 1 });
                        topFilterGroups.push(filterGroup);
                    }
                }
            }
            rule.FilterGroups = topFilterGroups;
        });
        return this.autoRules;
    }

    autoRuleValidate() {
        let validateResult = true;
        this.autoRules.forEach(rule => {
            const filterGroupContent = this.topFilterGroupContents[rule.UniqueId];
            for (const filterGroupUniqueId in filterGroupContent) {
                if (Object.hasOwnProperty.call(filterGroupContent, filterGroupUniqueId)) {
                    const content = filterGroupContent[filterGroupUniqueId];
                    if (content) {
                        if (!content.filterGroupValidate()) {
                            validateResult = false;
                            return;
                        }
                    }
                }
            }
            //duplicate
            //if add faker validate 
            // if (CRMCommonUtil.guidIsEmpty(rule.TermId)) {
            //     validateResult = false;
            //     return;
            // }
        });
        return validateResult;
    }

    validChangeTerm = (showInputError, errorMessage) => {
        this.showInputError = showInputError;
        this.inputErrorMessage = errorMessage;

        for (const filterUniqueId in this.accordionChangeTermGroupContent) {
            if (Object.hasOwnProperty.call(this.accordionChangeTermGroupContent, filterUniqueId)) {
                const content = this.accordionChangeTermGroupContent[filterUniqueId];
                if (content) {
                    $$.verify(content.ref.current);
                }
            }
        }
    }

    setAccordionContent(accordionContent, autoRule, filterGroup) {
        if (!this.topFilterGroupContents[autoRule.UniqueId]) {
            this.topFilterGroupContents[autoRule.UniqueId] = {};
        }
        this.topFilterGroupContents[autoRule.UniqueId][filterGroup.UniqueId] = accordionContent;
    }

    showSelectedTermTree = (autoRuleUniqueId) => {
        this.selectedDefaultTermCache = null;
        this.state.showSelectTermPanel[autoRuleUniqueId] = { show: true };
        this.setState({ showSelectTermPanel: Object.assign({}, this.state.showSelectTermPanel) });
    }

    cancelSelectTerm = (autoRuleUniqueId) => {
        this.state.showSelectTermPanel[autoRuleUniqueId] = { show: false };
        this.setState({ showSelectTermPanel: Object.assign({}, this.state.showSelectTermPanel) });
    }

    onSelectTermChanged = (args) => {
        this.selectedDefaultTermCache = args[0];
    }

    saveAutoRuleSelectTerm = (autoRuleUniqueId) => {
        if (!$$.verify(this.refSelectedTermScopeValid.ref.current)) {
            return false;
        }
        this.state.autoRules.forEach(rule => {
            if (rule.UniqueId == autoRuleUniqueId) {
                rule.TermName = this.selectedDefaultTermCache.Name;
                rule.TermId = this.selectedDefaultTermCache.UniqueId;
                rule.TermIsRemoved = false;
                rule.TermIsDeprecated = false;
            }
        });
        this.state.showSelectTermPanel[autoRuleUniqueId] = { show: false };
        this.setState({ showSelectTermPanel: Object.assign({}, this.state.showSelectTermPanel), autoRules: [...this.state.autoRules] }, () => {
            this.accordionTermStatusContent[autoRuleUniqueId] && this.accordionTermStatusContent[autoRuleUniqueId].clearStatus();
        });
        $$.verify(this.accordionValidationContent[autoRuleUniqueId].ref.current);
    }

    customDefaultTermScopeValid = () => {
        var selectedDefaultTermTree = this.selectedDefaultTermCache == null ? true : false;
        if (selectedDefaultTermTree) {
            return RMResx.RM_SPS_CS_SelectDefaulterm;
        } else {
            return true;
        }
    }
    

    addDefaultRule =()=>{
        let ruleLevel = 64;
        if (this.props.itemId == "exo") {
            ruleLevel = 65536;
        }

        if (this.props.itemId == "fs") {
            ruleLevel = 1048576;
        }
        let defaultRule = {
            "RuleLevel": ruleLevel,
            "FilterGroups": [],
            "Order": -1,
            "TermId": "",
            "TermName": "",
            "IsDefaultRule": true,
            "NoDefaultTerm": true,
            UniqueId: StringUtil.newGuid()
        };
        this.autoRules.push(defaultRule);
        this.setState({ autoRules: [...this.autoRules] });
    }

    addRule = ()=> {
        let ruleLevel = 64;
        if (this.props.itemId == "exo") {
            ruleLevel = 65536;
        }

        if (this.props.itemId == "fs") {
            ruleLevel = 1048576;
        }
        let rule = {
            IsDefaultRule: false,
            NoDefaultTerm: true,
            TermIsRemoved: false,
            TermIsDeprecated: false,
            RuleLevel: ruleLevel,
            Category: 0,
            RuleOrder: 0,
            AndOrExpression: "1",
            UniqueId: StringUtil.newGuid(),
            FilterGroups: []
        };
        rule.FilterGroups.push({ UniqueId: StringUtil.newGuid(), FilterGroups: [], Filters: [] });
        this.autoRules.push(rule);
        this.updateGroupCountForRule();
        this.setState({ autoRules: [...this.autoRules] }, () => {
            let filteredIfContents = this.accordionIfContent.filter(r => r != null);
            let ifContent = filteredIfContents[filteredIfContents.length - 1];
            if (ifContent) {
                ifContent.focus();
            }
        });
    }

    deleteRule = (autoRule) => {
        let deleteIndex = 0;
        this.autoRules.forEach((f, index) => {
            if (autoRule.UniqueId == f.UniqueId) {
                deleteIndex = index;
            }
        });
        this.autoRules.splice(deleteIndex, 1);
        this.updateGroupCountForRule();
        this.setState({ autoRules: [...this.autoRules] }, this.focusFirstRule);
    }

    focusFirstRule = () => {
        let filteredIfContents = this.accordionIfContent.filter(r => r != null);
        if (filteredIfContents && filteredIfContents.length != 0) {
            let ifContent = filteredIfContents[0];
            if (ifContent) {
                ifContent.focus();
            }
        }
    }

    updateGroupCountForRule() {
        this.autoRules.forEach(rule => {
            const filterGroupContent = this.topFilterGroupContents[rule.UniqueId];
            for (const filterGroupUniqueId in filterGroupContent) {
                if (Object.hasOwnProperty.call(filterGroupContent, filterGroupUniqueId)) {
                    const groupContent = filterGroupContent[filterGroupUniqueId];
                    if (groupContent) {
                        groupContent.updateGroupCount(this.autoRules.length - 1);
                    }
                }
            }
        });
    }

    customValidAutoCriteriaTerm = (autoRuleUniqueId) => {
        let result = true;
        this.state.autoRules.forEach(rule => {
            if (rule.UniqueId == autoRuleUniqueId) {
                result = !CRMCommonUtil.guidIsEmpty(rule.TermId) && rule.TermName;
                if (rule.TermIsRemoved || rule.TermIsDeprecated) {
                    result = false;
                }
            }
        });
        return result ? true : RMResx.RM_JS_SPS_AutoClassification_NoTerm;
    }

    customValidAutoChangeTermGroupValidTerm = (autoRuleUniqueId) => {
        let result = true;
        this.state.autoRules.forEach(rule => {
            if (rule.UniqueId == autoRuleUniqueId) {
                result = !this.showInputError;
            }
        });
        return result ? true : this.inputErrorMessage;
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    render() {
        /*--------------------------------------------------------------------------------------*/
        /*  ClassificationRule                                                                  */
        /*    --FilterGroups(Top, only one, mapped in AutoRule.jsx)                             */
        /*      --FilterGroup                                                                   */
        /*        --Filters                                                                     */
        /*        --FilterGroups                                                                */
        /*      ...                                                                             */
        /*--------------------------------------------------------------------------------------*/
        this.topFilterGroupContents = {};
        this.accordionIfContent = [];
        // console.log(this.state.autoRules);
        return <div id={this.props.id}>
            {
                this.state.autoRules.map((autoRule) => {
                    //Although FilterGroups are mapped here, there is only ONE element in FilterGroups. Filters and others are in this group
                    return <div key={autoRule.UniqueId}>
                        {!autoRule.IsDefaultRule && <div className="margin-top-m margin-bottom-s font-m font-bold" tabIndex="0" ref={r => this.accordionIfContent.push(r)}>
                            {RMResx.RM_SPS_AutoClassification_If}</div>}
                        {autoRule.FilterGroups.map((filterGroup) => {
                            return <AutoFilterGroup
                                ref={r => { this.setAccordionContent(r, autoRule, filterGroup); }}
                                itemId={this.props.itemId}
                                key={filterGroup.UniqueId}
                                data={filterGroup}
                                deepCount={1}
                                delGroup={this.deleteRule.bind(this, autoRule)}
                                groupCount={this.state.autoRules.length - 1}//except default rule
                                focusFirstRule={this.focusFirstRule}
                                lastAccessTimeCollection={this.props.lastAccessTimeCollection}
                            ></AutoFilterGroup>;
                        })}
                        {!autoRule.IsDefaultRule && <div className="ra-crm-form-content">
                            <div className="margin-top-m margin-bottom-s font-m font-bold" tabIndex="0">{RMResx.RM_JS_SPS_AutoClassification_Then}</div>
                            <div className="auto-ruleTerm-body">
                                <div className="require ra-setting-panel-title">{RMResx.RM_SPS_AutoClassification_ApplyTerm}</div>
                                <div className="inline-block">
                                    <div className="class-selector" id={`auto-rule-term-div-${autoRule.UniqueId}`} tabIndex="0">
                                        <div className="class-selector-value" data-tooltip="diffneed" tabIndex="0" role="combobox" aria-label={RMResx.RM_SPS_AutoClassification_ApplyTerm}>
                                            <TermStatusForInputText
                                                ref={accordionContent => this.accordionTermStatusContent[autoRule.UniqueId] = accordionContent}
                                                termRemoved={autoRule.TermIsRemoved}
                                                termDeprecated={autoRule.TermIsDeprecated}></TermStatusForInputText>
                                            {autoRule.TermName}
                                        </div>
                                    </div>
                                    {!this.props.inputSelectTermDisable &&
                                        <div className="class-selector-icon" data-tooltip aria-label={RMResx.RM_JS_SPS_DocumentSettings_SelectTerm} onClick={this.showSelectedTermTree.bind(this, autoRule.UniqueId)} tabIndex="0" onKeyDown={this.onKeyDown}>
                                            <div className="fia-term" aria-hidden="true"></div>
                                        </div>}
                                    <R.ValidationFaker
                                        of={`#auto-rule-term-div-${autoRule.UniqueId}`}
                                        ref={accordionContent => this.accordionValidationContent[autoRule.UniqueId] = accordionContent}
                                        valid={this.customValidAutoCriteriaTerm.bind(this, autoRule.UniqueId)}
                                    />
                                </div>
                                <R.ValidationFaker
                                    of={`#auto-rule-term-div-${autoRule.UniqueId}`}
                                    ref={accordionContent => this.accordionChangeTermGroupContent[autoRule.UniqueId] = accordionContent}
                                    valid={this.customValidAutoChangeTermGroupValidTerm.bind(this, autoRule.UniqueId)}
                                />
                            </div>
                            <R.Panel
                                size={670}
                                header={RMResx["RM_SPS_CS_SelectDefault-checkbox"]}
                                status={this.state.showSelectTermPanel[autoRule.UniqueId]}
                                destroy={true}
                                actionType={'back'}
                            >
                                <div>
                                    <div className="margin-top-s margin-left-l">
                                        <R.ValidationFaker valid={this.customDefaultTermScopeValid} ref={r => this.refSelectedTermScopeValid = r} />
                                    </div>
                                    <SelectTermTree
                                        rootItem={this.props.selectedTermScope}
                                        onSelectedNodeChanged={this.onSelectTermChanged}
                                        sourceFlag={this.props.sourceFlag}
                                        containerId={this.props.containerId}
                                        uniqueId={autoRule.TermId}
                                    >
                                    </SelectTermTree>
                                </div>
                                <>
                                    <R.Button slot="buttons" id="raCrmAutoTermTreePanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelSelectTerm.bind(this, autoRule.UniqueId)} />
                                    <R.Button slot="buttons" id="raCrmAutoTermTreePanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveAutoRuleSelectTerm.bind(this, autoRule.UniqueId)} />
                                </>
                            </R.Panel>
                        </div>}
                    </div>;
                })
            }
            <div className="margin-top-m">
                <R.Button type="link" text={`+ ${RMResx.RM_SPS_AutoClassification_AddCondition}`} onClick={this.addRule} />
            </div>
        </div>;
    }
}
