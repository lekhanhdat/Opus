import { NodeLevel } from "../../../../Constants/DAEnums";
import CRMTeamTree from "../../../Common/Tree/Instances/TermTree/CRMTeamTree";
import "../../../../Less/BCM/ContentRepositoryManagement/containerTermSetting.less";
import StringUtil from "../../../../Utilities/StringUtil";
import CRMCommonUtil from "../Common/CRMCommonUtil";
import { showToast } from "../../../../Utilities/CommonUtil";
import { SourceFlags } from "../../../../Constants/Constants";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";


export default class ContainerTermSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.enableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
        this.state = {
            savedTermTreeData: [],
            containerLevel: 2,
            isEnableClassification: this.props.data.isEnableClassification,
            descriptionTextarea: this.props.data.DescriptionOfContainer,
            termId: this.props.data.TermIdOfContainer,
            termName: this.props.data.TermNameOfContainer,
            termDataLoaded: false,
            searchKey: "",
            inheritParentTerm: this.props.data.IsInheritParentTerm || false,
            cachedTermId: null,
        };
        this.treePageSize = 15;
    }

    componentInit() {
        this.initTermTree();
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    getGroupNode(node) {
        while (node.Level != NodeLevel.WebApplication) {
            node = node.Parent;
        }
        return node;
    }

    openElementLoading = (id, isOpen) =>{
        $$.elementLoading(id, isOpen, { text: false });
    }

    initTermTree() {
        this.openElementLoading("raCrmSettingPanelTermTree", true);
        let currNode = this.props.data;
        if (!CRMCommonUtil.guidIsEmpty(currNode.TermIdOfContainer)) {
            let groupNode = this.getGroupNode(currNode);
            let paramObj = {};
            paramObj.CurrentNodeId = currNode.TermIdOfContainer;
            paramObj.SettingType = 0;
            paramObj.spTreeNodes = [groupNode];
            paramObj.perPageCount = this.treePageSize;
            let option = {
                url: this.props.context.getSavedTermUrl,
                method: "Post",
                data: paramObj
            };
            fetchUtility(option).then((result) => {
                $$.loading(false);
                if (result) {
                    this.setState({ savedTermTreeData: JSON.parse(result), termDataLoaded: true });
                }
                this.openElementLoading("raCrmSettingPanelTermTree", false);
            }).catch((e) => {
                $$.loading(false);
                this.openElementLoading("raCrmSettingPanelTermTree", false);
            });
        } else {
            this.setState({ termDataLoaded: true });
            this.openElementLoading("raCrmSettingPanelTermTree",false);
        }
        this.setState({
            cachedTermId: this.props.data.TermIdOfContainer,
        });
    }

    save(containerSettingNode, callback) {
        $$.messagedialog(false);
        let option = {
            url: this.props.context.saveContainerSettingUrl,
            method: "Post",
            data: containerSettingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let res = JSON.parse(result);
            if (res.MessageType == 0) {
                this.setState({
                    cachedTermId: null,
                });
                callback(true, containerSettingNode);
                showToast.success(RMResx.RM_JS_BCM_SaveSettingsSuccess);
            } else {
                showToast.error(res.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        let containerSettingNode = this.props.data;
        let nodeLevel = containerSettingNode.Level;
        RM.deepcopy(this.props.data);
        if (nodeLevel == 2) {
            containerSettingNode.TermIdOfContainer = this.state.termId;
            containerSettingNode.TermNameOfContainer = this.state.termName;
            containerSettingNode.isEnableClassification = true;
            containerSettingNode.DescriptionOfContainer = this.state.descriptionTextarea;
            containerSettingNode.IsInheritParentTerm = this.enableRecordsArchiver && this.state.inheritParentTerm;
        } else {
            if (this.state.isEnableClassification == true) {
                containerSettingNode.TermIdOfContainer = this.state.termId;
                containerSettingNode.TermNameOfContainer = this.state.termName;
                containerSettingNode.isEnableClassification = true;
                containerSettingNode.DescriptionOfContainer = this.state.descriptionTextarea;
                containerSettingNode.IsInheritParentTerm = this.enableRecordsArchiver && this.state.inheritParentTerm;
            } else {
                containerSettingNode.TermIdOfContainer = CRMCommonUtil.GuidEmpty;
                containerSettingNode.TermNameOfContainer = "";
                containerSettingNode.isEnableClassification = false;
                containerSettingNode.DescriptionOfContainer = "";
                containerSettingNode.IsInheritParentTerm = false;
            }
        }
        this.save(containerSettingNode, callback);
    }

    cancel() {
        return true;
    }

    onContainerSwitchChange = (args) => {
        this.setState({ isEnableClassification: args });
    }

    onContainerDescriptionChange = (args) => {
        this.setState({ descriptionTextarea: args });
    }

    onInheritParentTermChanged = (args) => {
        this.setState({ inheritParentTerm: args });
    }

    onTermTreeChanged = (args) => {
        this.setState({
            termId: args[0].UniqueId,
            termName: args[0].Name,
        });
    }

    containerTermScopeValid = () => {
        var selectedTree = this.refTermScopeTree.getSelectedTreeNode();
        return selectedTree.node ? true : RMResx.RM_JS_BCM_Global_SelectTerm;
    }

    onSearch = (args) => {
        this.setState({ searchKey: args });
    }

    render() {
        let nodeLevel = this.props.data.Level;
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-containerpanel-tips" tabIndex={0}>
                        {this.props.context.configurations.containerDes}
                    </div>
                    {nodeLevel && nodeLevel != 2 && <div className="ra-crm-form-content ra-setting-panel-containerEnable">
                        <span className="ra-containerEnable-span" tabIndex="0">{RMResx.RM_SPS_CS_IsEnableClassification}</span>
                        <span className="ra-setting-panel-containerSwitch">
                            <R.Switch
                                checked={this.state.isEnableClassification}
                                onChange={this.onContainerSwitchChange} />
                        </span>
                    </div>}
                    {nodeLevel && (nodeLevel == 2 || this.state.isEnableClassification) && <div>
                        <div className="ra-crm-form-content">
                            <div className="ra-setting-panel-title"><$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_SPS_GS_Description)} /></div>
                            <R.Input
                                type="textarea"
                                height={100}
                                value={this.state.descriptionTextarea}
                                onChange={this.onContainerDescriptionChange}
                                aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_SPS_GS_Description) }}
                            />
                        </div>
                        <div className="ra-crm-form-content">
                            <div className="require ra-setting-panel-title" tabIndex="0">{StringUtil.trimEndFullStop(RMResx.RM_SPS_ChooseTermForClassificationTip)}</div>
                            <div id="raCrmSettingPanelTermTree" className="ra-setting-panel-termtree">
                                {this.state.termDataLoaded && <div>
                                    <div className="ra-selectterms-searchbox">
                                        <R.Searchbox
                                            width={570}
                                            height={34}
                                            placeholder={RMResx.RM_JS_TM_SearchTxt}
                                            disabled={false}
                                            onSearch={this.onSearch}
                                        />
                                    </div>
                                    <div className="margin-top-s margin-left-l" tabIndex="0">
                                        <R.ValidationFaker valid={this.containerTermScopeValid} ref={r => this.refTermScopeValid = r} />
                                    </div>
                                    <div className="ra-setting-panel-treepadding">
                                        <CRMTeamTree
                                            ref={r => this.refTermScopeTree = r}
                                            searchKey={this.state.searchKey}
                                            data={this.state.savedTermTreeData}
                                            onSelectedNodeChanged={this.onTermTreeChanged}
                                            onNodeLevel={this.state.containerLevel}
                                            sourceFlag={SourceFlags.SP}
                                            containerId={CRMCommonUtil.getGroupNode(this.props.data).Id}
                                        >
                                        </CRMTeamTree>
                                    </div>
                                </div>}
                            </div>
                        </div>
                        {this.enableRecordsArchiver && <div className="ra-crm-form-content">
                            <R.Checkbox
                                id="raCrmInheritParentTermCheckbox"
                                text={this.props.context.configurations.inheritParentTermText}
                                checked={this.state.inheritParentTerm}
                                onChange={this.onInheritParentTermChanged}
                            />
                        </div>}
                    </div>}
                </div>
            </R.Validation>
        </div>;
    }
}