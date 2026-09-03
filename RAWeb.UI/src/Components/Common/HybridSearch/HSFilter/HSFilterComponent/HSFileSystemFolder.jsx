import FSTree from "../../../Tree/Instances/FSTree/FSDestinationTree";
import { ToSearchComponentDispatchType } from '../../Constants';
import { TreeType } from "../../../../../Constants/Constants";
let idCount = 0;
export default class HSFileSystemFolder extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            selectedFSText: RMResx.RM_JS_Common_None,
            isShowFsTree: false,
            fsTreeValid: true
        };
        this.fsTreeData = null;
        this.fsTreeId = "fsTree" + idCount++;
        this.onDocumentMouseDown = this.onDocumentMouseDown.bind(this);
        window.addEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentDestroy() {
        window.removeEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentReceive(type, data) {
        switch (type) {
            case ToSearchComponentDispatchType.InitData:
                this.fsTreeData = data || [];
                this.setState({ fsTreeData: RM.deepcopy(this.fsTreeData) });
                this.setSelectedFSText();
                break;
            case ToSearchComponentDispatchType.Valid:
                this.showValidMsg();
                break;
        }
    }

    onApplyClick = () => {
        this.fsTreeData = this.refFsTree.getTreeData();
        let selectedFsTreeNodeId = this.getSelectedFsTreeNodeId();
        this.setSelectedFSText();
        this.props.onChange(selectedFsTreeNodeId, this.fsTreeData);
    }

    getSelectedFsTreeNodeId() {
        let selectedFsTreeNodeId = null;
        for (let item of this.fsTreeData) {
            if (item.CheckNumber == 1) {
                selectedFsTreeNodeId = item.Id;
                break;
            }
        }
        return selectedFsTreeNodeId;
    }

    setSelectedFSText() {
        let selectedFSText = RMResx.RM_JS_Common_None;
        let fsTreeValid = false;
        for (let item of this.fsTreeData) {
            if (item.CheckNumber == 1) {
                selectedFSText = item.Name;
                fsTreeValid = true;
                break;
            }
        }
        this.setState({
            selectedFSText: selectedFSText,
            fsTreeValid: fsTreeValid
        });
    }

    onDocumentMouseDown(e) {
        this.mouseDownTarget = e.target;
    }

    isTreeRefreshClick(target) {
        let $target = $(target);
        return $target.closest(".ra-tree-menu-expand").length > 0;
    }

    onWillHideFSFilterPopup = () => {
        let isTreeRefreshClick = this.isTreeRefreshClick(this.mouseDownTarget);
        this.mouseDownTarget = null;
        if (isTreeRefreshClick) {
            return false;
        }
    }

    onHideFSFilterPopup = () => {
        this.setState({
            isShowFsTree: false
        });
    }

    onShowFSFilterPopup = () => {
        this.setState({
            isShowFsTree: true,
            fsTreeData: RM.deepcopy(this.fsTreeData)
        });
    }

    showValidMsg() {
        this.setState({ fsTreeValid: false });
    }

    onCancel = () =>{

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
                    content={this.state.selectedFSText}
                    height={40}
                    popupHeight={[, 300]}
                    popupWidth={[,'100%']}
                    width={"100%"}
                    id={this.fsTreeId}
                    block={false}
                    triggerType="all"
                    status={{ show: this.state.isShowFsTree }}
                    willHide={this.onWillHideFSFilterPopup}
                    onHide={this.onHideFSFilterPopup}
                    onShow={this.onShowFSFilterPopup}
                >
                    <div id="hsFilterFileSystem" className="padding-m">
                        {
                            this.state.isShowFsTree && <FSTree
                                ref={r => this.refFsTree = r}
                                treeData={this.state.fsTreeData}
                                type={TreeType.Filter}
                            />
                        }
                    </div>
                    <>
                        <R.Button
                            slot="buttons"
                            name="cancel"
                            text={RMResx.RM_JS_Common_Cancel}
                            value="close"
                            onClick={this.onCancel}
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
                <R.ValidationFaker valid={this.state.fsTreeValid} of={`#${this.fsTreeId}`} message={RMResx.RM_HS_NoSearchColValValidMsg} />
            </div>
        </div>;
    }
}
