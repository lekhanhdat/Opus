import TreeItem from './TreeItem';
import {NodeIconClass} from '../../../../Constants/DAEnums'

export default class TreeList extends React.Component {
    constructor(props) {
        super(props);

        this.treeContext = props.treeContext;
        if (!this.props.parentItemComponent) {
            this.treeLevel = 1;
        } else {
            this.treeLevel = this.props.parentItemComponent.props.treeLevel + 1;
        }
    }
    shouldComponentUpdate(newProps, newState) {
        //if (newProps.items == this.props.items
        //    && typeof this.props.show == "boolean" && this.props.show == newProps.show) {
        //    return false;
        //}
        return true;
    }
    getCommonItems(item) {
        var commonItems = [];
        if (item.origin && item.enableIncludeNew) {
            commonItems.push({
                nodeKey: item.nodeKey + "i",
                nodeType: this.treeContext.commonNodeTypes.includeNew,
                text: RMResx.RM_JS_RC_Report_IncludeNew,
                iconClass: "ra-tree-icon " + NodeIconClass.IncludeNew,
                isLeafNode: true,
                enableIncludeNew: false,
                checked: item.includeNew,
            });
        }
        if (item.origin && (item.enableIncludeNew || item.onlySupportSelectAll)) {
            if (item.hasChildren && this.props.items.length > 0) {
                commonItems.push({
                    nodeKey: item.nodeKey + "s",
                    nodeType: this.treeContext.commonNodeTypes.selectAll,
                    text: RMResx.RM_JS_RC_Report_SelectAll,
                    iconClass: "ra-tree-icon " + NodeIconClass.SelectAll,
                    isLeafNode: true,
                    enableIncludeNew: false,
                    checked: item.selectAll,
                });
            }
        }
        return commonItems;
    }
    renderCommonItems() {
        let pitem = this.props.parentItem;
        if (!pitem) {
            return null;
        }
        let items = this.getCommonItems(pitem);
        if (!items || items.length == 0) {
            return null;
        }
        return items.map((item, index) => {
            item.treeLevel = this.treeLevel;
            return <TreeItem
                treeContext={this.props.treeContext}
                treeLevel={this.treeLevel}
                parentItemComponent={this.props.parentItemComponent}
                parentItem={this.props.parentItem}
                item={item}
                key={`tree-commonitem-${index}-${item.nodeKey}`}
            />;
        });
    }
    renderItems() {
        let items = this.props.items;
        if (!items) {
            return null;
        }
        return items.map((item, index) => {
            item.treeLevel = this.treeLevel;
            return <TreeItem
                treeContext={this.props.treeContext}
                treeLevel={this.treeLevel}
                parentItemComponent={this.props.parentItemComponent}
                parentItem={this.props.parentItem}
                item={item}
                key={`tree-item-${index}-${item.nodeKey}`}
            />;
        });
    }
    render() {
        let ariaRole = this.treeLevel == 1 ? {} : {role: "group"};
        return <React.Fragment>
            <ul className={`ra-tree-list ra-tree-lv-${this.treeLevel} ${(this.props.show ? "block" : "none")}`} {...ariaRole}>
                {this.renderCommonItems()}
                {this.renderItems()}
            </ul>
        </React.Fragment>;
    }
}