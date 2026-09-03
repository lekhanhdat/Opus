import { bindEvents } from '../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../Components/TreeActions';
import { NodeLevel, NodeIconClass } from '../../../../Constants/DAEnums';

export default class GoogleNodeContent extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            item: props.item
        };
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
                return `${NodeIconClass.GoogleSharedDrive}-new`;
            case NodeLevel.GoogleDriveContainer:
                return NodeIconClass.Folder;
            default:
                return NodeIconClass.WebApp;
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

    render() {
        let item = this.state.item;
        return <React.Fragment>
            <div className={"ra-tree-node-content"} aria-label={item.text} data-tooltip="true" data-tooltip-wrap="force">
                <$g.Icon className={this.getNodeIconClass(item)}></$g.Icon>
                <div className="ra-tree-node-text">
                    {item.text}
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
