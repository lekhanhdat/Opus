import { bindEvents } from '../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../Components/TreeActions';
import { NodeLevel, NodeType, NodeIconClass } from '../../../../Constants/DAEnums';

export default class PhysicalExplorerNodeContent extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            item: props.item
        };
        this.isLeafNode = this.props.item.NodeType == NodeLevel.POLocation;
        this.radioRef = React.createRef();
        this.nodeClickTimer = null;
        this.treeContext = this.props.treeContext;
        bindEvents(this, "onRefreshActionClick");
    }

    componentDidMount() {
    }
    componentDidUpdate(prevProps, prevState) {
        $(".ra-tree-node-margin-left-for-none-status").removeClass("ra-tree-node-margin-left-for-none-status");
        if($(".ra-tree-node-status").length > 0){
            $(".ra-tree-node-status").closest(".ra-tree-node-content").addClass("ra-tree-node-margin-left-for-none-status");
        }
    }
    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.item != this.props.item) {
            this.setState({ item: nextProps.item });
        }
    }

    getNodeIconClass(item) {
        switch (item.nodeType) {
            case NodeType.PhysicalRootLocation:
                return 'ra-tree-node-icon fia-physical-record';
            case NodeType.PhysicalNormalLocation:
                return 'ra-tree-node-icon fia-location';
            case NodeType.PhysicalBottomLocation:
                return 'ra-tree-node-icon fia-room';
            case NodeType.PhyBox:
                return 'ra-tree-node-icon fia-box-suite';
            case NodeType.PhyFile:
                return 'ra-tree-node-icon fia-folder';
            case NodeType.PhyRecord: 
                return 'ra-tree-node-icon fia-records-template';
            case NodeType.PhyCustom: 
                return 'ra-tree-node-icon fia-container';
            default:
                return '';
        }
    }

    onRefreshActionClick(e) {
        this.props.item.loaded = false;
        this.props.itemComponent.reload(0);
        e.stopPropagation();
    }

    renderNodeText(text) {
        let searchKey = this.treeContext.searchKey;
        if (!searchKey) {
            return text;
        } else {
            var escapingSearchKey = searchKey.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
            var re = new RegExp(escapingSearchKey, "gi");
            if (re.test(text)) {
                var matchResult = [];
                let index = 0;
                return <$g.I18NProvider msg={text.replace(re, function (match) { matchResult.push(match); return `{${index++}}`; })}>
                    {matchResult.map((matchStr, idx) => {
                        return <span key={idx} className="ra-tree-node-text-red">{matchStr}</span>;
                    })}
                </$g.I18NProvider>;
            }
            return text;
        }
    }

    render() {
        let item = this.state.item;
        let nodeStatusInfo = item.nodeStatusInfo;
        return <React.Fragment>
            {item.showStatus && nodeStatusInfo && <div className={"ra-tree-node-status"} style={{...nodeStatusInfo.iconScaleStyle}}>
                <div className={nodeStatusInfo.iconClass} style={{color: nodeStatusInfo.color}} data-tooltip aria-label={nodeStatusInfo.name}></div>
            </div>}
            <div
                className={"ra-tree-node-content"}  aria-label={item.text} data-tooltip="true">
                <div className={this.getNodeIconClass(item)}></div>
                <div className="ra-tree-node-text">
                    {this.renderNodeText(item.text)}
                </div>
            </div>
            <TreeActionsPopup
                itemComponent={this.props.itemComponent}
                treeContext={this.props.treeContext}
                disabled={false}>
                <TreeActionItem
                    text={RMResx.RM_DAM_Refesh}
                    iconClass="fia-refresh"
                    onActionClick={this.onRefreshActionClick}
                    onBlur={this.onActionBlur} />
            </TreeActionsPopup>
        </React.Fragment>;
    }
}
