import { ToSearchComponentDispatchType } from '../../Constants';
import { TreeType } from "../../../../../Constants/Constants";
import { NodeLevel } from '../../../../../Constants/DAEnums';
import LocationGoogleTree from '../../../Tree/Instances/GoogleTree/LocationGoogleTree';

let idCount = 0;

export default class HSGoogleLocation extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            selectedGoogleText: RMResx.RM_JS_Common_None,
            isShowGoogleTree: false,
            googleTreeValid: true
        };
        this.googleTreeData = null;
        this.googleTreeId = "googleTree" + idCount++;
        this.onDocumentMouseDown = this.onDocumentMouseDown.bind(this);
        window.addEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentDestroy() {
        window.removeEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentReceive(type, data) {
        switch (type) {
            case ToSearchComponentDispatchType.InitData:
                this.googleTreeData = data || [];
                this.setState({ googleTreeData: RM.deepcopy(this.googleTreeData) });
                this.setSelectedGoogleText();
                break;
            case ToSearchComponentDispatchType.Valid:
                this.showValidMsg();
                break;
        }
    }

    onApplyClick = () => {
        this.googleTreeData = this.refGoogleTree.getTreeData().items;
        let selectedCount = 0;
        for (let item of this.googleTreeData) {
            if (item.CheckNumber == 1) {
                selectedCount++;
            }
        }
        let googleNodes = this.getSelectedTreeNode();
        this.setSelectedGoogleText();
        this.props.onChange(googleNodes, this.googleTreeData);
    }

    getSelectedTreeNode() {
        const googleNodes = [];
        for (let item of this.googleTreeData) {
            if (item.CheckNumber == 1) {
                if(item.Level == NodeLevel.GoogleUserDriveContainer || item.Level == NodeLevel.GoogleSharedDriveContainer) {
                    googleNodes.push({ Id: item.Id, Level: item.Level });
                } else {
                    googleNodes.push({ Id: item.ObjectId, Level: item.Level });
                }
            }
        }
        return googleNodes;
    }

    setSelectedGoogleText() {
        let selectedGoogleText = RMResx.RM_JS_Common_None;
        let googleTreeValid = false;
        let selectedCount = 0;
        let itemName = "";
        for (let item of this.googleTreeData) {
            if (item.CheckNumber == 1) {
                selectedCount++;
                itemName = item.DisplayName;
                googleTreeValid = true;
            }
        }
        if (selectedCount > 0) {
            if (selectedCount == 1) {
                selectedGoogleText = itemName;
            } else {
                selectedGoogleText = RMResx.RM_Common_Combobox_SelectedXItems.format(selectedCount);
            }
        }
        this.setState({
            selectedGoogleText: selectedGoogleText,
            googleTreeValid: googleTreeValid
        });
    }

    onDocumentMouseDown(e) {
        this.mouseDownTarget = e.target;
    }

    isTreeRefreshClick(target) {
        let $target = $(target);
        return $target.closest(".ra-tree-menu-expand").length > 0;
    }

    onWillHideGoogleFilterPopup = () => {
        let isTreeRefreshClick = this.isTreeRefreshClick(this.mouseDownTarget);
        this.mouseDownTarget = null;
        if (isTreeRefreshClick) {
            return false;
        }
    }

    onShowGoogleFilterPopup = () => {
        this.setState({
            isShowGoogleTree: true,
            googleTreeData: RM.deepcopy(this.googleTreeData)
        });
    }

    onCloseGoogleFilterPopup = () => {
        this.setState({
            isShowGoogleTree: false
        });
    }

    showValidMsg() {
        this.setState({ googleTreeValid: false });
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
                    content={this.state.selectedGoogleText}
                    height={40}
                    popupHeight={[, 300]}
                    popupWidth={[,'100%']}
                    width={"100%"}
                    id={this.googleTreeId}
                    block={false}
                    compact={false}
                    triggerType="all"
                    status={{ show: this.state.isShowGoogleTree }}
                    willHide={this.onWillHideGoogleFilterPopup}
                    onHide={this.onCloseGoogleFilterPopup}
                    onShow={this.onShowGoogleFilterPopup}
                >
                    <div id="hsFilterGoogle" className="padding-m">
                        <LocationGoogleTree
                            ref={r => this.refGoogleTree = r}
                            data={this.state.googleTreeData}
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
                <R.ValidationFaker valid={this.state.googleTreeValid} of={`#${this.googleTreeId}`} message={RMResx.RM_HS_NoSearchColValValidMsg} />
            </div>
        </div>;
    }
}
