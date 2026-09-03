import { bindEvents } from '../../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../../Components/TreeActions';
import { NodeLevel, NodeType, NodeIconClass } from '../../../../../Constants/DAEnums';
import {SourceFlags} from "../../../../../Constants/Constants";

export default class GoogleNodeContent extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            showAtions: true,
            item: props.item
        };
        this.treeContext = this.props.treeContext;
        bindEvents(this, "onRefreshActionClick");
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.item != this.props.item) {
            this.setState({ item: nextProps.item });
        }
    }

    getIconStatus(node) {
        switch (node.iconStatus) {
            case 1:
                return "-inherit-b";
            case 2:
                return "-unique-c";
            default:
                return "";
        }
    }

    getNodeIconClass(item) {
        return `ra-tree-icon ${this.getBaseNodeIconClass(item)}${this.getIconStatus(item)}`;
    }
    getBaseNodeIconClass(item) {
        let node = item.origin;
        const isReportTree = item?.isReportTree;
        switch (node.Level) {
            case NodeLevel.Root:
                return NodeIconClass.GoogleDriveRoot;
            case NodeLevel.GoogleUserDriveContainer:
                return NodeIconClass.GoogleUserDriveContainer;
            case NodeLevel.GoogleSharedDriveContainer:
                return NodeIconClass.GoogleShareDriveContainer;
            case NodeLevel.GoogleUserDrive:
                    return NodeIconClass.GoogleUserDrive;
            case NodeLevel.GoogleSharedDrive:
                if (node.IconStatus && !isReportTree) {
                    return NodeIconClass.GoogleSharedDrive;
                }
                return `${NodeIconClass.GoogleSharedDrive}-new`;
            default:
                return NodeIconClass.WebApp;
        }
    }

    onRefreshActionClick(e) {
        this.props.item.loaded = false;
        this.props.itemComponent.loadNodes(0, this.props.item.pagerSize,(success) => {
            this.treeContext.onNodeRefresh();
        });
        e.stopPropagation();
    }

    renderNodeText(text) {
        let searchKey = this.treeContext.searchKey;
        if (!searchKey) {
            return text;
        } 
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

    render() {
        let item = this.state.item;
        return <React.Fragment>
            <div className={"ra-tree-node-content"} data-tooltip data-tooltip-wrap="force">
                <$g.Icon className={this.getNodeIconClass(item)}></$g.Icon>
                <div className="ra-tree-node-text">
                    <span ref={r => this.NodeText = r}>
                        {this.renderNodeText(item.text)}
                    </span>
                </div>
                <TreeActionsPopup
                    itemComponent={this.props.itemComponent}
                    treeContext={this.props.treeContext}
                    show={this.state.showAtions}> 
                    <TreeActionItem
                        text={RMResx.RM_DAM_Refesh}
                        iconClass="fia-refresh"
                        onActionClick={this.onRefreshActionClick}
                        onBlur={this.onActionBlur} />
                </TreeActionsPopup>
            </div>
        </React.Fragment>;
    }
}
