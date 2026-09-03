import { bindEvents } from '../../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../../Components/TreeActions';
import { NodeLevel, NodeIconClass } from '../../../../../Constants/DAEnums';

export default class FSNodeContent extends React.Component {
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
            "onActionBlur", "onRadioChange", "onRefreshActionClick", "onDeclareActionClick");
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.item != this.props.item) {
            this.setState({ item: nextProps.item });
        }
    }

    getNodeIconClass(item) {
        let iconStatusClass = this.getIconStatus(item);
        return `ra-tree-icon ${this.getBaseNodeIconClass(item)}${iconStatusClass} `;
    }

    getIconStatus(item) {
        let origin = item.origin;
        if (origin.IsCustomSetting && !origin.IsActive && (origin.Level == NodeLevel.FSFolder)) {
            return "-deactive-b";
        }else{
            switch (item.iconStatus) {
                case 1:
                    return "-inherit-b";
                case 2:
                    return "-unique-c";
                default:
                    return "";
            }
        }
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
        this.props.itemComponent.loadNodes(0, this.props.item.pagerSize,(success) => {
            this.treeContext.onNodeRefresh();
        });
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

    onActiveClick (isActive){
        this.props.itemComponent.onSingleCheckedChange();
        this.treeContext.onActiveClick(isActive, this.props.item);
    }

    render() {
        let item = this.state.item;
        let isActive = item.origin.IsActive;
        let isShowDeactiveBtn = !isActive && item.origin.Level == NodeLevel.FSFolder && item.origin.IsCustomSetting;
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
            >
                {!isShowDeactiveBtn && <TreeActionItem
                    text={RMResx.RM_DAM_Refesh}
                    iconClass="fia-refresh"
                    onActionClick={this.onRefreshActionClick}
                />}
                {
                    item.origin.Level == NodeLevel.FSFolder && item.origin.IconStatus != 0 &&<React.Fragment>
                        {
                            !isActive && <TreeActionItem
                                text={RMResx.RM_JS_SPS_FS_ActiveSettings}
                                iconClass="fia-activate"
                                onActionClick={this.onActiveClick.bind(this, true)}
                            />
                        }
                        {
                            isActive && <TreeActionItem
                                text={RMResx.RM_JS_SPS_FS_DeactiveSettings}
                                iconClass="fia-deactivate"
                                onActionClick={this.onActiveClick.bind(this, false)}
                            />
                        }
                    </React.Fragment>
                }
            </TreeActionsPopup>
        </React.Fragment>;
    }
}
