import { bindEvents } from '../../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../../Components/TreeActions';
import { NodeLevel, NodeIconClass } from '../../../../../Constants/DAEnums';

export default class BoxNodeContent extends React.Component {
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

    componentDidMount () {
    }
    componentDidUpdate (prevProps, prevState) {
    }
    UNSAFE_componentWillReceiveProps (nextProps) {
        if (nextProps.item != this.props.item) {
            this.setState({ item: nextProps.item });
        }
    }

    getNodeIconClass (item) {
        return "ra-tree-icon " + this.getBaseNodeIconClass(item);
    }

    getBaseNodeIconClass(item) {
        switch (item.nodeType) {
            case NodeLevel.RMSelectAll:
                return NodeIconClass.SelectAll;
            case NodeLevel.Root:
                return NodeIconClass.BoxRoot;
            case NodeLevel.BoxConnectionGroup:
                return NodeIconClass.FSConnectionGroup;
            case NodeLevel.BoxConnection:
                return NodeIconClass.FSConnection;
            case NodeLevel.BoxUser:
                return NodeIconClass.User;
            default: return NodeIconClass.FSFolder;
        }
    }

    onNodeKeyDown (e) {
        if (e.keyCode === 13) {
            e.target.click();
        }
    }

    onNodeClick (e) {
        e.stopPropagation();
        this.props.onClick(e);
    }

    onRefreshActionClick (e) {
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

    render () {
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
