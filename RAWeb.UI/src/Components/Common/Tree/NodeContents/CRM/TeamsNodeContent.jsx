import { NodeIconClass, NodeLevel, NodeType } from "../../../../../Constants/DAEnums";
import { TreeActionItem, TreeActionsPopup } from "../../Components/TreeActions";

class TeamsNodeContent extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            showAtions: true,
            item: props.item,
        };
        this.treeContext = this.props.treeContext;
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

    onRefreshActionClick = (e) => {
        this.props.item.loaded = false;
        this.props.itemComponent.loadNodes(0, this.props.item.pagerSize, () => {
            this.treeContext.onNodeRefresh();
        });
        e.stopPropagation();
    };

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
        let groupTooltip = `${item?.origin?.ParentName} / ${item?.origin?.FullPath}`;
        let isShowGroupTooltip =
            this.treeContext.needShowSpecialTooltip &&
            item.nodeType === NodeLevel.Office365GroupEntire;
        let tooltipProps = isShowGroupTooltip
            ? {
                  "data-tooltip": "diff",
                  "aria-label": groupTooltip,
                  "data-tooltip-wrap": "force",
              }
            : {};
        let textTooltipProps = isShowGroupTooltip
            ? {}
            : {
                  "data-tooltip": "ifneed",
                  "aria-label": item.text,
                  "data-tooltip-wrap": "force",
              };
        return (
            <React.Fragment>
                <div className={"ra-tree-node-content"} {...tooltipProps}>
                    <$g.Icon className={this.getNodeIconClass(item)}></$g.Icon>
                    <div className="ra-tree-node-text" {...textTooltipProps}>
                        <span ref={(r) => (this.NodeText = r)}>
                            {this.renderNodeText(item.text)}
                        </span>
                    </div>
                    <TreeActionsPopup
                        itemComponent={this.props.itemComponent}
                        treeContext={this.props.treeContext}
                        show={this.state.showAtions}
                    >
                        <TreeActionItem
                            text={RMResx.RM_DAM_Refesh}
                            iconClass="fia-refresh"
                            onActionClick={this.onRefreshActionClick}
                            onBlur={this.onActionBlur}
                        />
                    </TreeActionsPopup>
                </div>
            </React.Fragment>
        );
    }
}

export default TeamsNodeContent;
