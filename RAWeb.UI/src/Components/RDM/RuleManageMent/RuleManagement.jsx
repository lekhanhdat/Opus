import SiteMapLinks from "../../../Constants/SiteMapLinks";
import "../../../Less/RDM/ruleManagement.less";
import CreatRule from "../../Common/RuleItem/CreateRule";
import RuleExpanderDetail from "./RuleExpanderDetail";
import { showToast, LicenseHelper, isShowActionByDC } from "../../../Utilities/CommonUtil";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import { TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import RuleContainerTree from "../../Common/Tree/Instances/Rule/RuleContainerTree";
import { RuleModuleTypes } from "../../Common/RuleItem/Components/Constants";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import { createRef } from "react";

const isMultiGeoMainDC = isShowActionByDC();
export default class RuleManagement extends R.Component {
    constructor(props) {
        super(props);
        this.data = {
            searchValue: ""
        };
        this.state = {
            messageTipInfo: { showTip: false, type: "success", content: "" },
            containerName: "",
            showRightSearchAndCreate: false,
            rulesAllData: [],
            rulesCount: 0,
            currentPageItems: [],
            rulesPagerIndex: 0,
            rulesPagerSize: 10,
            criteriaTabsIndex: {},
            termDto: {},
            containerId: null,
            noRuleItem: false,
            lastAccessTimeCollection: "",
            isSelectAllOnCurrentPage: false,
            selectedRuleItems: [],
        };
        this.ruleLevel = {
            2: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection,
            4: RMResx.RM_JS_Rule_ObjectLevel_Site,
            8: RMResx.RM_JS_Rule_ObjectLevel_List,
            16: RMResx.RM_JS_Rule_ObjectLevel_Folder,
            32: RMResx.RM_JS_Rule_ObjectLevel_Item,
            64: RMResx.RM_JS_Rule_ObjectLevel_Document
        };
        this.createRuleComponentId = "raCreateRuleItem";
        this.refRightTopButtons = createRef();
    }

    componentInit() {
        !LicenseHelper.HasOpusSOLicenseOnly() && this.validateDAConSetting();
        this.loadLastAccessTimeCollectionData()
    }

    loadSearchData = (isReset, noSearchRule) => {
        if(isReset){
            this.data.searchValue = "";
        }
        this.data.ContainerId = this.ruleContainerTreeNode.ContainerId;
        $$.loading(true);
        let urlData = "/api/RuleApi/GetSearchRuleDatas";
        let option = {
            url: urlData,
            data: this.data
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (data.length != 0) {
                for (let key of data) {
                    key.Level = this.ruleLevel[key.RuleLevel];
                }
                this.setState({ noRuleItem: false });
            } else {
                if (noSearchRule) {
                    this.setState({ noRuleItem: true });
                } else {
                    this.setState({ noRuleItem: false });
                }
            }
            this.setState({
                rulesAllData: data,
                rulesCount: data.length,
                showRightSearchAndCreate: true,
                isSelectAllOnCurrentPage: false,
            },()=>{
                 this.onPageChange(0, this.state.rulesPagerSize); 
            }); 
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    validateDAConSetting() {
        $$.loading(true);
        let urlData = "/API/TermUsageReportApi/ValidateDAConnectionSetting";
        let option = {
            url: urlData,
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (!res.Success) {
                this.setState({ messageTipInfo: { type: "error", showTip: true, content: res.Message } });
            }
        }).catch((e) => {
            $$.loading(false);
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

    hideMessageTip() {
        this.setState({
            messageTipInfo: { showTip: false }
        });
    }

    getShowRuleButtons = () => {
        let buttons = [];
        const selectedRuleItems = this.state.selectedRuleItems;

        if (!selectedRuleItems.length) {
            buttons = [
                {
                    isStatic: true,
                    id: "raRdmRmCreateBtn",
                    name: RMResx.RM_JS_Common_Create,
                    onClick: this.handleCreatRule,
                }
            ];
        } else if (selectedRuleItems.length === 1) {
            buttons = [
                {
                    id: "raRdmRmEditBtn",
                    name: RMResx.RM_JS_Common_Edit,
                    icon: "fia-edit",
                    onClick: () => this.handleEditRule(selectedRuleItems[0]),
                },
                {
                    id: "raRdmRmDeleteBtn",
                    name: RMResx.RM_JS_Common_Delete,
                    icon: "fia-delete",
                    onClick: () => this.handleDeleteRule(selectedRuleItems[0]),
                },
            ];
        } else {
            buttons = [
                {
                    id: "raRdmRmDeleteBtn",
                    name: RMResx.RM_JS_Common_Delete,
                    icon: "fia-delete",
                    onClick: () => this.handleDeleteRule(selectedRuleItems[0], true),
                },
            ];
        }

        return isMultiGeoMainDC ? buttons : [];
    }

    handleCreatRule = () => {
        this.dispatch(this.createRuleComponentId, 1); //1 RuleManagement, 2 TermManagement
    }

    handleEditRule(ruleItems) {
        this.dispatch(this.createRuleComponentId, 1, ruleItems.RuleId);  //1 RuleManagement, 2 TermManagement
    }

    //删除提示框
    handleDeleteRule(item, isDeleteMultiple) {
        if(LicenseHelper.HasOpusILLicense())
        {
            let checkData = [];
            if (isDeleteMultiple && this.state.selectedRuleItems?.length) {
                checkData = this.state.selectedRuleItems.map((item) => ({
                    RuleName: item.RuleName,
                    RuleId: item.RuleId,
                    TermNames: item.TermNames
                }));
            } else {
                const filterObject = { RuleName: item.RuleName, RuleId: item.RuleId, TermNames: item.TermNames };
                checkData.push(filterObject);
            }
            let urlData = "/api/RuleApi/GetAssociateTerms";
            let option = {
                url: urlData,
                method: "POST",
                data: checkData
            };
            fetchUtility(option).then((res) => {
                let termDto = JSON.parse(res);
                this.setState({
                    termDto: termDto
                }, () => {
                    this.deleteMessageBox(item, isDeleteMultiple);
                });
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }
        else
        {
            this.deleteMessageBox(item, isDeleteMultiple);
        }
    }

    deleteMessageBox(item, isDeleteMultiple) {
        let currentRuleRelateTerms = this.state.termDto?.Terms?.length > 0 && this.state.termDto?.HasTerms;
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div>{item.ModelType == RuleModuleTypes.SOArchiver ? RMResx.RM_RDM_Rule_DeleteSORule : RMResx.RM_RDM_Rule_DeleteRule}</div>
                {
                    currentRuleRelateTerms && <div>
                        <div className="margin-top-m strong">{RMResx.RM_RDM_Rule_AssociatedTerms}</div>
                        <div className="ra-rule-relate-term">
                            {
                                this.state.termDto.Terms.map((item, key) => {
                                    return <React.Fragment key={key}>
                                        {item.TermNames && (
                                            <div className="margin-top-s">
                                                <$g.I18NProvider msg={RMResx.RM_Common_ComboboxText_Format}>
                                                    <span>{item.RuleName}</span>
                                                    <span>{item.TermNames}</span>
                                                </$g.I18NProvider>
                                            </div>
                                        )}
                                    </React.Fragment>;
                                })
                            }

                        </div>
                    </div>
                }
            </div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.onDeleteCancelClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onDeleteSureClick.bind(this, item, isDeleteMultiple) },   
            ]
        };
        $$.messagedialog(true, args);
    }

    onDeleteSureClick(item, isDeleteMultiple) {
        $$.messagedialog(false);
        $$.loading(true);
        let dataArr = [];
        
        if (isDeleteMultiple && this.state.selectedRuleItems?.length) {
            dataArr = this.state.selectedRuleItems.map((item) => item.RuleId);
        } else {
            dataArr.push(item.RuleId);
        }

        let urlData = "/api/RuleApi/DeleteRules";
        let option = {
            url: urlData,
            method: "POST",
            data: dataArr,
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res == "") {
                showToast.success(RMResx.RM_JS_RDM_DeleteRule_MessageInfo_Success);
                this.loadSearchData();
            } else {
                showToast.error(res);
                // showToast.error(RMResx.RM_JS_RDM_DeleteRule_MessageInfo_Faild);
            }
            addTelemetryRecord(TelemetryModule.RuleManagement, TelemetryEventType.RuleDeleted);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onDeleteCancelClick() {
        $$.messagedialog(false);
    }

    updatedSelectedAllRuleItems = (list, checked) => {
        return list.map((item) => ({
            ...item,
            isChecked: checked,
        }));
    }

    onIsSelectAllOnCurrentPageChange = (checked) => {
        this.setState((prev) => {
            return {
                isSelectAllOnCurrentPage: checked,
                // rulesAllData: this.updatedSelectedAllRuleItems(prev.rulesAllData, checked),
                currentPageItems: this.updatedSelectedAllRuleItems(prev.currentPageItems, checked),
                selectedRuleItems: this.updatedSelectedAllRuleItems(prev.currentPageItems, checked).filter((rule) => rule.isChecked),
            }
        }, () => {
            const buttons = this.getShowRuleButtons();
            this.refRightTopButtons.current.updateButtons(buttons);
        });
    }

    updatedCheckedRuleItems = (list, item, checked) => {
        return list.map((currItem) => {
            if (currItem.RuleId === item.RuleId) {
                return { ...currItem, isChecked: checked };
            }
            return currItem;
        })
    }

    onRuleItemCheckedChange = (checked, item) => {
        this.setState((prev) => {
            const updatedCurrentPageItems = this.updatedCheckedRuleItems(prev.currentPageItems, item, checked);

            let isSelectAllOnCurrentPage = prev.isSelectAllOnCurrentPage;
            const checkedRules = updatedCurrentPageItems.filter((rule) => rule.isChecked);
            const checkedRulesCount = checkedRules.length;
            const total = updatedCurrentPageItems.length;

            if (checkedRulesCount === 0 || checkedRulesCount === total) {
                isSelectAllOnCurrentPage = checkedRulesCount === total;
            } else {
                isSelectAllOnCurrentPage = 'mixed';
            }

            return {
                currentPageItems: updatedCurrentPageItems,
                isSelectAllOnCurrentPage,
                selectedRuleItems: checkedRules,
            }
        }, () => {
            const buttons = this.getShowRuleButtons();
            this.refRightTopButtons.current.updateButtons(buttons)
        });
    }

    onPageChange = (index, size, callback) => {
        let currentPageItems = JSON.parse(JSON.stringify(this.state.rulesAllData.slice(index * size, (index + 1) * size)));
        this.setState({
            // currentPageItems: currentPageItems,
            currentPageItems: this.updatedSelectedAllRuleItems(currentPageItems, false),
            rulesPagerIndex: index,
            rulesPagerSize: size,
            isSelectAllOnCurrentPage: false,
            selectedRuleItems: [],
        }, () => {
            const buttons = this.getShowRuleButtons();
            this.refRightTopButtons.current.updateButtons(buttons);
        });
        if (callback) {
            callback(true);
        }
    }

    onSearchRuleName = (args) => {
        this.data.searchValue = args;
        this.loadSearchData(false, true);
    }

    onSearchRuleNameStop = (args) => {
        this.loadSearchData(true, false);
    }

    onSearchContainer = (args) => {
        this.refContainerTree.startSearch(args);
        this.setState({
            showRightSearchAndCreate: false,
            rulesAllData: [],
            rulesCount: 0,
            currentPageItems: [],
        });
    }

    onStopSearchContainer = (args) => {
        this.refContainerTree.stopSearch();
        this.setState({
            showRightSearchAndCreate: false,
            rulesAllData: [],
            rulesCount: 0,
            currentPageItems: [],
        });
    }

    onDeleteRule = (args) => {
        this.setState({
            showRightSearchAndCreate: false,
            rulesAllData: [],
            rulesCount: 0,
            currentPageItems: [],
            containerName: "",
        });
    }

    onContainerName = (args) => {
        this.setState({
            containerName: args,
        });
    }

    renderMessageBar() {
        return <div className="margin-bottom-l">
            <R.Messagebar
                message={this.state.messageTipInfo.content}
                classify={this.state.messageTipInfo.type}
                status={{ show: this.state.messageTipInfo.showTip }}
                onClose={this.hideMessageTip}
            />
        </div>;
    }

    onTreeChanged = (item) => {
        this.ruleContainerTreeNode = item;
        this.setState({
            containerId: this.ruleContainerTreeNode.ContainerId,
            containerName: this.ruleContainerTreeNode.Name,
            showRightSearchAndCreate: true
        });
        this.refSearchRuleName?.clear();
        this.loadSearchData(true);
    }

    renderFooter() {
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.rulesCount}
                pagerIndex={this.state.rulesPagerIndex}
                pagerSize={this.state.rulesPagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15, 50, 100]}
                onChange={this.onPageChange} />
        </div>;
    }

    renderRuleItemsHeader() {
        if (this.state.showRightSearchAndCreate) {
            return (
                <div className="ra-splitter-buttons flex justify-between align-center padding-left-l padding-top-m padding-right-l padding-bottom-m">
                    <TopButtonsComponent
                        ref={(r) => this.refRightTopButtons.current = r}
                        data={{ menuBtnItems: this.getShowRuleButtons() }}
                        showCount={3}
                    ></TopButtonsComponent>
                    <div tabIndex={0}>
                        {RMResx.RM_JS_RM_SelectedRuleCount.format(this.state.selectedRuleItems.length, this.state.rulesAllData.length)}
                    </div>
                </div>
            );
        }
        return null;
    }

    renderRuleItems() {
        if (this.state.containerId !== null) {
            if (this.state.rulesAllData.length === 0 || this.state.noRuleItem) {
                return (
                    <div className="ra-noitems">{RMResx.RM_JS_RD_NoItems}</div>
                );
            }

            if (this.state.rulesAllData.length > 0) {
                return (
                    <div>
                        <div className="ra-list-item-title flex justify-between align-center">
                            <div style={{ flex: 1 }}>
                                <R.Checkbox
                                    id="raRdmRmSelectAll"
                                    text={RMResx.RM_JS_RM_RuleNameColumn}
                                    checked={this.state.isSelectAllOnCurrentPage}
                                    onChange={this.onIsSelectAllOnCurrentPageChange}
                                />
                            </div>
                            {isMultiGeoMainDC && <div tabIndex={0}>{RMResx.RM_JS_RM_ActionsColumn}</div>}
                        </div>
                        {this.state.currentPageItems.map((item) => {
                            return (
                                <div key={item.RuleId}>
                                    <R.Expander
                                        status={false}
                                        triggerType="action"
                                        groupName="title">
                                        <div className="ra-rule-expander">
                                            <div className="ra-expander-fontStyle text-overflow" data-tooltip="ifneed">
                                                <R.Checkbox
                                                    id={`raRdmRmRuleItemCkb-${item.RuleId}`}
                                                    text={item.RuleName}
                                                    tooltip={item.RuleName}
                                                    checked={item.isChecked}
                                                    onChange={(checked) => this.onRuleItemCheckedChange(checked, item)}
                                                />
                                            </div>
                                            {isMultiGeoMainDC && <div className="ra-expander-action">
                                                <R.Scope>
                                                    <R.Button
                                                        id="raRdmRmEditBtn"
                                                        type="bald"
                                                        icon="fia-edit"
                                                        tooltip={RMResx.RM_JS_Common_Edit}
                                                        onClick={this.handleEditRule.bind(this, item)} />
                                                    <R.Button
                                                        id="raRdmRmDeleteBtn"
                                                        type="bald"
                                                        icon="fia-delete"
                                                        tooltip={RMResx.RM_JS_Common_Delete}
                                                        onClick={this.handleDeleteRule.bind(this, item, false)} />
                                                </R.Scope>
                                            </div>}
                                        </div>
                                        <div>
                                            <RuleExpanderDetail ruleItem={item}></RuleExpanderDetail>
                                        </div>
                                    </R.Expander>
                                </div>
                            );
                        })}
                    </div>
                );
            }
        }

        return null;
    }

    render() {
        return <div id='raRuleManagement'>
            <section>
                <$g.SiteMap data={[SiteMapLinks.RDM_RuleManagement]}></$g.SiteMap>
                {this.renderMessageBar()}
            </section>
            <section className="rule-content">
                <div className="rule-splitter-container">
                    <R.Splitter minAsize="25%" minBsize="58%" defaultAsize="40%">
                        <div className="rule-splitter-left">
                            <div className="ra-splitter-headerLeft ra-splitter-headerborder">
                                <div className="ra-splitter-headerTitle" tabIndex="0">{RMResx.RM_PRM_PRE_ContainerTitle}</div>
                                <div className="ra-splitter-searchboxLeft">
                                    <R.Searchbox
                                        width='100%'
                                        height={34}
                                        placeholder={RMResx.RM_JS_RM_SearchContainerTxt}
                                        disabled={false}
                                        onSearch={(args) => (args || "").trim() === "" ? this.onStopSearchContainer(args) : this.onSearchContainer(args)}
                                    />
                                </div>
                            </div>
                            <div className="ra-splitter-tree">
                                <RuleContainerTree
                                    ref={r => this.refContainerTree = r}
                                    onTreeChanged={this.onTreeChanged}
                                    onDeleteRule={this.onDeleteRule}
                                    containerName={this.onContainerName}
                                >
                                </RuleContainerTree>
                            </div>
                        </div>
                        <div className="rule-splitter-right">
                            <div className={this.state.rulesAllData.length == 0 ? "ra-splitter-headerRight ra-splitter-headerborder" : "ra-splitter-headerRight"}>
                                <div style={{ width: "calc(100% - 343px)" }}>
                                    <div className="ra-splitter-headerTitle" tabIndex="0">{RMResx.RM_PRM_PRE_RuleTitle}</div>
                                    <div className="ra-splitter-headerName" data-tooltip="diffneed" aria-label={this.state.containerName}>
                                        {this.state.containerName != "" && <span className="fia-folder ra-splitter-folder"></span>}
                                        <span tabIndex="0">{this.state.containerName}</span>
                                    </div>
                                </div>
                                {this.state.showRightSearchAndCreate && (
                                    <div className="ra-header-right">
                                        <div className="ra-splitter-searchboxRight">
                                            <R.Searchbox
                                                ref={r => this.refSearchRuleName = r}
                                                width='100%'
                                                height={34}
                                                placeholder={RMResx.RM_JS_RM_SearchRuleNameTxt}
                                                tooltip={RMResx.RM_JS_RM_SearchRuleNameTxt}
                                                disabled={false}
                                                onSearch={(args) => (args || "").trim() === "" ? this.onSearchRuleNameStop(args) : this.onSearchRuleName(args)}
                                            />
                                        </div>
                                    </div>
                                )}
                            </div>
                            {this.renderRuleItemsHeader()}
                            {this.renderRuleItems()}
                            {this.state.rulesAllData.length != 0 && this.renderFooter()}
                        </div>
                    </R.Splitter>
                </div>
            </section>
            <CreatRule id="raCreateRuleItem" callback={this.loadSearchData} history={this.props.history} containerId={this.state.containerId} lastAccessTimeCollection={this.state.lastAccessTimeCollection} />
        </div>;
    }
}