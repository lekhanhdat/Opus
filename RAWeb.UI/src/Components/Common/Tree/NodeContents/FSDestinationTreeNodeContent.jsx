import { bindEvents } from '../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../Components/TreeActions';
import { NodeLevel, NodeIconClass } from '../../../../Constants/DAEnums';

export default class FSDestinationTreeNodeContent extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            showAtions: false,
            item: props.item
        };
        this.radioRef = React.createRef();
        this.nodeClickTimer = null;
        this.treeContext = this.props.treeContext;
        bindEvents(this, "onNodeKeyDown", "onNodeClick", "onNodeBlur",
            "onActionBlur", "onRadioChange", "onRefreshActionClick");
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
        let iconName = "";
        switch(node.Level){
            case NodeLevel.Farm:
                iconName = NodeIconClass.FSRoot;
                break;
            case NodeLevel.WebApplication:
                iconName = NodeIconClass.FSConnectionGroup;
                break;
            case NodeLevel.SiteCollection:
                iconName = NodeIconClass.FSConnection;
                break;
            case NodeLevel.FSFolder:
                iconName = NodeIconClass.FSFolder;
                break;
        }
        return iconName;
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
            return <$g.I18NProvider msg={text.replace(searchKey, "{0}")}>
                <span className="ra-tree-node-text-red">{searchKey}</span>
            </$g.I18NProvider>;
        }
    }

    render() {
        let item = this.state.item;
        return <React.Fragment>
            <div className={"ra-tree-node-content"}  aria-label={item.text} data-tooltip="true">
                <$g.Icon className={this.getNodeIconClass(item)}></$g.Icon>
                <div className="ra-tree-node-text">
                    {this.renderNodeText(item.text)}
                </div>
            </div>

            <TreeActionsPopup
                itemComponent={this.props.itemComponent}
                treeContext={this.props.treeContext}
                disabled={item.isLeafNode}>
                <TreeActionItem
                    text={RMResx.RM_DAM_Refesh}
                    iconClass="fia-refresh"
                    onActionClick={this.onRefreshActionClick}
                    onBlur={this.onActionBlur} />
            </TreeActionsPopup>
        </React.Fragment>;
    }
}
