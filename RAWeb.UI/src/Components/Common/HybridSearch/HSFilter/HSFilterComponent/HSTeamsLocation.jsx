import { ToSearchComponentDispatchType } from '../../Constants';
import { TreeType } from "../../../../../Constants/Constants";
import { NodeLevel } from '../../../../../Constants/DAEnums';
import LocationTeamsTree from '../../../Tree/Instances/TeamsTree/LocationTeamsTree';

let idCount = 0;

export default class HSTeamsLocation extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            selectedTeamsText: RMResx.RM_JS_Common_None,
            isShowTeamsTree: false,
            teamsTreeValid: true
        };
        this.teamsTreeData = null;
        this.teamsTreeId = "teamsTree" + idCount++;
        this.onDocumentMouseDown = this.onDocumentMouseDown.bind(this);
        window.addEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentDestroy() {
        window.removeEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentReceive(type, data) {
        switch (type) {
            case ToSearchComponentDispatchType.InitData:
                this.teamsTreeData = data || [];
                this.setState({ teamsTreeData: RM.deepcopy(this.teamsTreeData) });
                this.setSelectedTeamsText();
                break;
            case ToSearchComponentDispatchType.Valid:
                this.showValidMsg();
                break;
        }
    }

    onApplyClick = () => {
        this.teamsTreeData = this.refTeamsTree.getTreeData().items;
        let selectedRootNode = false;
        let selectedCount = 0;
        let rootNode = null;
        for (let item of this.teamsTreeData) {
            if (item.CheckNumber == 1) {
                selectedCount++;
                if (item.Level == NodeLevel.Farm) {
                    selectedRootNode = true;
                    rootNode = item;
                }
            }
        }
        if (selectedRootNode && selectedCount == this.teamsTreeData.length && selectedCount > 1) {
            rootNode.CheckNumber = 0;
        }
        let teamsNodes = this.getSelectedTreeNode();
        this.setSelectedTeamsText();
        this.props.onChange(teamsNodes, this.teamsTreeData);
    }

    getSelectedTreeNode() {
        const teamsNodes = [];
        for (let item of this.teamsTreeData) {
            if (item.CheckNumber == 1) {
                teamsNodes.push({ Id: item.Id, Level: item.Level });
            }
        }
        return teamsNodes;
    }

    setSelectedTeamsText() {
        let selectedTeamsText = RMResx.RM_JS_Common_None;
        let teamsTreeValid = false;
        let selectedRootNode = false;
        let selectedCount = 0;
        let itemName = "";
        for (let item of this.teamsTreeData) {
            if (item.CheckNumber == 1 && item.Level != NodeLevel.SiteCollections) {
                selectedCount++;
                itemName = item.Name;
                teamsTreeValid = true;
                if (item.Level == NodeLevel.Farm) {
                    selectedRootNode = true;
                }
            }
        }
        if (selectedCount > 0) {
            if (selectedCount == 1) {
                selectedTeamsText = itemName;
                if (selectedRootNode) {
                    selectedTeamsText = RMResx.RM_JS_BCM_Explorer_Filter_All;
                }
            } else {
                selectedTeamsText = RMResx.RM_Common_Combobox_SelectedXItems.format(selectedCount);
            }
        }
        this.setState({
            selectedTeamsText: selectedTeamsText,
            teamsTreeValid: teamsTreeValid
        });
    }

    onDocumentMouseDown(e) {
        this.mouseDownTarget = e.target;
    }

    isTreeRefreshClick(target) {
        let $target = $(target);
        return $target.closest(".ra-tree-menu-expand").length > 0;
    }

    onWillHideTeamsFilterPopup = () => {
        let isTreeRefreshClick = this.isTreeRefreshClick(this.mouseDownTarget);
        this.mouseDownTarget = null;
        if (isTreeRefreshClick) {
            return false;
        }
    }

    onShowTeamsFilterPopup = () => {
        this.setState({
            isShowTeamsTree: true,
            teamsTreeData: RM.deepcopy(this.teamsTreeData)
        });
    }

    onHideTeamsFilterPopup = () => {
        this.setState({
            isShowTeamsTree: false
        });
    }

    showValidMsg() {
        this.setState({ teamsTreeValid: false });
    }

    render() {
        return <div className="flex">
            <div className="flex-1">
                <R.Input
                    type="text"
                    value={RMResx.RM_HS_Contains}
                    width={"100%"}
                    height={40}
                    readonly={true}
                />
            </div>
            <div className="flex-1 margin-left-m width-0">
                <R.ComboboxShell
                    dynamicSize
                    content={this.state.selectedTeamsText}
                    height={40}
                    popupHeight={[, 300]}
                    popupWidth={[,'100%']}
                    width={"100%"}
                    id={this.teamsTreeId}
                    block={false}
                    triggerType="all"
                    status={{ show: this.state.isShowTeamsTree }}
                    willHide={this.onWillHideTeamsFilterPopup}
                    onHide={this.onHideTeamsFilterPopup}
                    onShow={this.onShowTeamsFilterPopup}
                >
                    <div id="hsFilterTeams" className="padding-m">
                        <LocationTeamsTree
                            ref={r => this.refTeamsTree = r}
                            data={this.state.teamsTreeData}
                            treeType={TreeType.Filter}
                        />
                    </div>
                    <>
                        <R.Button
                            slot="buttons"
                            name="cancel"
                            text={RMResx.RM_JS_Common_Cancel}
                            value="close"
                        />
                        <R.Button
                            slot="buttons"
                            name="save"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_JS_Common_Save}
                            value="close"
                            onClick={this.onApplyClick}
                        />
                    </>
                </R.ComboboxShell>
                <R.ValidationFaker valid={this.state.teamsTreeValid} of={`#${this.teamsTreeId}`} message={RMResx.RM_HS_NoSearchColValValidMsg} />
            </div>
        </div>;
    }
}
