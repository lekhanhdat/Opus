import { bindEvents } from '../../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../../Components/TreeActions';
import { NodeLevel, NodeIconClass, NodeType } from '../../../../../Constants/DAEnums';

export default class SPNodeContent extends React.Component {
    constructor(props) {
        super(props);
        bindEvents(this, "onRefreshActionClick");
        this.treeContext = this.props.treeContext;
        this.state = {
            item: props.item
        };
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
        switch (item.nodeType) {
            case NodeLevel.RMIncludeNew:
                return NodeIconClass.IncludeNew;
            case NodeLevel.RMSelectAll:
                return NodeIconClass.SelectAll;
            case NodeLevel.Farm:
                return NodeIconClass.TeamsFarm;
            case NodeLevel.WebApplication:
                return NodeIconClass.TeamsContainer;
            case NodeLevel.Office365GroupEntire:
                if (item.origin.NodeType == NodeType.O365TeamSites) {
                    return NodeIconClass.TeamsGroup;
                }
            default: return NodeIconClass.SiteCollection;
        }
    }
    
    onRefreshActionClick(e) {
        this.props.item.loaded = false;
        if(this.treeContext.onNodeRefreshAction){
            this.treeContext.onNodeRefreshAction();
        }
        this.props.itemComponent.reload(0);
        e.stopPropagation();
    }

    renderNodeText(text) {
        let searchKey = this.treeContext.searchKey;
        if (!searchKey) {
            return text;
        } else {
            const escapingSearchKey = searchKey.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
            const re = new RegExp(escapingSearchKey, "gi");
            if (re.test(text)) {
                const matchResult = [];
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
        return <React.Fragment>
            <div className={"ra-tree-node-content"} aria-label={item.text} data-tooltip="true">
                <$g.Icon className={this.getNodeIconClass(item)}></$g.Icon>
                <div className="ra-tree-node-text">
                    {this.renderNodeText(item.text)}
                </div>
            </div>

            <TreeActionsPopup
                itemComponent={this.props.itemComponent}
                treeContext={this.props.treeContext}>
                <TreeActionItem
                    text={RMResx.RM_DAM_Refesh}
                    iconClass="fia-refresh"
                    onActionClick={this.onRefreshActionClick} />
            </TreeActionsPopup>
        </React.Fragment>;
    }
}