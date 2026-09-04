import { NodeType } from "../../../../Constants/DAEnums";
import PhysicalRuleMoveTree from "../../../Common/Tree/Instances/Physical/PhyDestinationTree";

export default class PhyObjectMove extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            searchKey: "",
            selConflictType: "1",
            noSelectNode: false,
            isExceedLimitSearch: false,
        };
        this.bind(["onMoveTreeSelectedNodeChanged", "onConflictChange"]);
        this.moveData = this.props.data;
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.moveData.ConflictOption = this.state.selConflictType;
                if (this.moveData.Target) {
                    args(this.moveData);
                } else {
                    this.setState({ noSelectNode: true });
                }
                break;
        }
    }
    initData(args) {
        this.moveData = args;
    }

    onSearch = (value) => {
        this.setState({ searchKey: value });
    }

    onMoveTreeSelectedNodeChanged(nodeItem) {
        this.moveData.Target = nodeItem;
        this.setState({
            noSelectNode: false,
        });
    }

    getConflictOptions() {
        let options = [
            {
                text: RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip,
                value: "1",
            },
            {
                text: RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite,
                value: "2",
            },
            {
                text: RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename,
                value: "3",
            },
        ];
        return options.map((op) => {
            op.title = op.text;
            op.checked = this.state.selConflictType == op.value;
            return op;
        });
    }

    onConflictChange(value) {
        this.setState({
            selConflictType: value,
        });
    }

    getSearchPlaceholder(leafNodeType) {
        if (leafNodeType == NodeType.PhyFile) {
            return RMResx.RM_JS_BCM_Explorer_SearchFilePlaceHolder;
        }
        return RMResx.RM_JS_BCM_Explorer_SearchPlaceHolder;
    }

    shouldShowSearchBox() {
        const sources = this.props.data?.Source;
        if (sources?.some(item => item.NodeType === NodeType.PhyBox)) {
            return false;
        }

        return true;
    }

    render() {
        const leafNodeType = this.props.smallNodeType;
        const showSearchBox = this.shouldShowSearchBox();
        const searchKey = showSearchBox ? this.state.searchKey : "";
        const searchPlaceHolder = this.getSearchPlaceholder(leafNodeType);

        return (
            <div id={this.props.id}>
                {showSearchBox && (
                    <React.Fragment>
                        <R.Searchbox
                            width={380}
                            height={34}
                            placeholder={searchPlaceHolder}
                            disabled={false}
                            onSearch={this.onSearch}
                        />
                        {this.state.isExceedLimitSearch && (
                            <div tabIndex={0} style={{ color: "red" }} className="margin-top-s">
                                {RMResx.RM_JS_BCM_Explorer_SearchErrorContent}
                            </div>
                        )}
                    </React.Fragment>
                )}
                <PhysicalRuleMoveTree
                    searchKey={searchKey}
                    onSelectedNodeChanged={this.onMoveTreeSelectedNodeChanged}
                    leafNodeType={leafNodeType}
                    onSetIsExceedLimitSearch={(isExceedLimitSearch) => { this.setState({ isExceedLimitSearch }); }}
                    data={this.props.data}
                ></PhysicalRuleMoveTree>
                <div className="margin-left-l">
                    <$g.ValidationMsg show={this.state.noSelectNode}>
                        {
                            RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode
                        }
                    </$g.ValidationMsg>
                </div>
            </div>
        );
    }
}
