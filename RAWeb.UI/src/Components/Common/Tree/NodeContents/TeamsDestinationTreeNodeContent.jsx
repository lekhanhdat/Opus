import { bindEvents } from "../../../../Utilities/CommonUtil";
import { TreeActionsPopup, TreeActionItem } from "../Components/TreeActions";
import { NodeLevel, NodeIconClass, NodeType } from "../../../../Constants/DAEnums";

export default class TeamsDestinationTreeNodeContent extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            showAtions: false,
            item: props.item,
        };
        this.treeContext = this.props.treeContext;
        bindEvents(
            this,
            "onNodeKeyDown",
            "onNodeClick",
            "onActionBlur",
            "onRefreshActionClick"
        );
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.item != this.props.item) {
            this.setState({ item: nextProps.item });
        }
    }

    getNodeIconClass(item) {
        return "ra-tree-icon " + this.getBaseNodeIconClass(item);
    }

    getBaseNodeIconClass(item) {
        let node = item.origin;
        switch (node.Level) {
            case NodeLevel.RMIncludeNew:
                return NodeIconClass.IncludeNew;
            case NodeLevel.RMSelectAll:
                return NodeIconClass.SelectAll;
            case NodeLevel.Farm:
                return NodeIconClass.TeamsFarm;
            case NodeLevel.WebApplication:
                return NodeIconClass.TeamsContainer;
            case NodeLevel.Office365GroupEntire:
                if (node.NodeType == NodeType.O365TeamSites) {
                    return NodeIconClass.TeamsGroup;
                }
                return NodeIconClass.SiteCollection;
            case NodeLevel.RootFolder:
            case NodeLevel.Folder:
                return NodeIconClass.Folder;
            case NodeLevel.Folders:
                return NodeIconClass.Folders;
            default:
                return NodeIconClass.SiteCollection;
        }
    }

    onNodeKeyDown(e) {
        if (e.keyCode === 13) {
            e.target.click();
        }
    }

    onNodeClick(e) {
        e.stopPropagation();
        this.props.onClick(e);
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
            return (
                <$g.I18NProvider msg={text.replace(searchKey, "{0}")}>
                    <span className="ra-tree-node-text-red">{searchKey}</span>
                </$g.I18NProvider>
            );
        }
    }

    render() {
        let item = this.state.item;
        return (
            <React.Fragment>
                <div
                    className={"ra-tree-node-content"}
                    aria-label={item.text}
                    data-tooltip="true"
                >
                    <$g.Icon className={this.getNodeIconClass(item)}></$g.Icon>
                    <div className="ra-tree-node-text">
                        {this.renderNodeText(item.text)}
                    </div>
                </div>

                <TreeActionsPopup
                    itemComponent={this.props.itemComponent}
                    treeContext={this.props.treeContext}
                    disabled={item.isLeafNode}
                >
                    <TreeActionItem
                        text={RMResx.RM_DAM_Refesh}
                        iconClass="fia-refresh"
                        onActionClick={this.onRefreshActionClick}
                        onBlur={this.onActionBlur}
                    />
                </TreeActionsPopup>
            </React.Fragment>
        );
    }
}
