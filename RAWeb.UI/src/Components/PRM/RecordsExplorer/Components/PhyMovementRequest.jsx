import { NodeType } from "../../../../Constants/DAEnums";
import PhysicalRuleMoveTree from "../../../Common/Tree/Instances/Physical/PhyDestinationTree";

export default class PhyMovementRequest extends R.Component {
    idAttr = true;
    
    componentCreate() {
        this.state = {
            searchKey: "",
            holdOption: "1",
            comment: "",
            noSelectNode: false,
            isExceedLimitSearch: false
        };
        this.bind(["onSearch", "onNodeChanged", "onHoldOptionChange", "onCommentChange"]);
        this.targetNode = null;
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                if (this.targetNode) {
                    let resultData = {
                        Source: this.props.data.Source,
                        Target: this.targetNode,
                        HoldConflictOption: parseInt(this.state.holdOption, 10),
                        Comment: this.state.comment
                    };
                    args(resultData);
                } else {
                    this.setState({ noSelectNode: true });
                }
                break;
            default:
                break;
        }
    }

    onSearch(value) {
        this.setState({ searchKey: value });
    }

    onNodeChanged(nodeItem) {
        this.targetNode = nodeItem;
        this.setState({ noSelectNode: false });
    }

    onHoldOptionChange(value) {
        this.setState({ holdOption: value });
    }

    onCommentChange(value) {
        this.setState({ comment: value });
    }

    getSearchPlaceholder() {
        const source = this.props.data && this.props.data.Source && this.props.data.Source[0];

        switch (source && source.NodeType) {
            case NodeType.PhyRecord:
                return RMResx.RM_JS_BCM_Explorer_SearchRecordPlaceHolder;
            case NodeType.PhyFile:
                return RMResx.RM_JS_BCM_Explorer_SearchFolderPlaceHolder;
            case NodeType.PhyBox:
                return RMResx.RM_JS_BCM_Explorer_SearchBoxPlaceHolder;
            default:
                return "Search";
        }
    }

    render() {
        const leafNodeType = this.props.smallNodeType;
        const searchPlaceholder = this.getSearchPlaceholder();

        let isSourceFolder = false;
        if (this.props.data && this.props.data.Source && this.props.data.Source.length > 0) {
            isSourceFolder = this.props.data.Source.every(item => item.NodeType === NodeType.PhyFile);
        }
        
        return (
            <div id={this.props.id}>
                <div className="margin-bottom-m">
                    <span className="require tm-tree-right-form-label-font">{RMResx.RM_Rule_Movement_Tree_Header}</span>
                </div>

                <R.Searchbox
                    width={380}
                    height={34}
                    placeholder={searchPlaceholder}
                    disabled={false}
                    onSearch={this.onSearch}
                />

                {this.state.isExceedLimitSearch && (
                    <div tabIndex={0} style={{ color: "red" }} className="margin-top-s">
                        {RMResx.RM_JS_BCM_Explorer_SearchErrorContent}
                    </div>
                )}

                <PhysicalRuleMoveTree
                    searchKey={this.state.searchKey}
                    onSelectedNodeChanged={this.onNodeChanged}
                    leafNodeType={leafNodeType}
                    onSetIsExceedLimitSearch={(isExceedLimitSearch) => { this.setState({ isExceedLimitSearch }); }}
                    data={this.props.data}
                />

                <div className="margin-left-l">
                    <$g.ValidationMsg show={this.state.noSelectNode}>
                        {
                            RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode
                        }
                    </$g.ValidationMsg>
                </div>

                <div className="margin-top-l">
                    {isSourceFolder && (
                        <React.Fragment>
                            <div className="margin-bottom-s">
                                <span className="tm-tree-right-form-label-font" tabIndex="0">
                                    {RMResx.RM_Rule_Movement_Conflicted_OptionHeader}
                                </span>
                            </div>
                            <div className="flex flex-column gap-s margin-bottom-m">
                                <R.Radio
                                    name="radioHoldConflict"
                                    text="Use the destination hold"
                                    value="1"
                                    checked={this.state.holdOption === "1"}
                                    onChange={this.onHoldOptionChange}
                                />
                                <R.Radio
                                    name="radioHoldConflict"
                                    text="Use the hold with longer duration"
                                    value="2"
                                    checked={this.state.holdOption === "2"}
                                    onChange={this.onHoldOptionChange}
                                />
                            </div>
                        </React.Fragment>
                    )}
                    <div className="margin-bottom-s">
                        <span className="tm-tree-right-form-label-font" tabIndex="0">
                            {RMResx.RM_Rule_Movement_Comment_Header}
                        </span>
                    </div>
                    <div>
                        <R.Input
                            type="textarea"
                            value={this.state.comment}
                            height={60}
                            onChange={this.onCommentChange}
                        />
                    </div>
                </div>
            </div>
        );
    }
}
