import { bindEvents } from '../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../Components/TreeActions';

export default class PhysicalExplorerTermViewNodeContent extends React.Component {
    constructor(props) {
        super(props);
        bindEvents(this, "onRefreshActionClick");
    }

    componentDidUpdate(prevProps, prevState) {
        $(".ra-tree-node-margin-left-for-none-status").removeClass("ra-tree-node-margin-left-for-none-status");
        if($(".ra-tree-node-status").length > 0){
            $(".ra-tree-node-status").closest(".ra-tree-node-content").addClass("ra-tree-node-margin-left-for-none-status");
        }   
    }

    getNodeIconClass(item) {
        if(item.iconClass){
            return item.iconClass;
        } else {
            return "fia-term-set";
        }
    }
    
    onRefreshActionClick(e) {
        this.props.item.loaded = false;
        this.props.itemComponent.reload(0);
        e.stopPropagation();
    }

    renderNodeText(text) {
        let searchKey = this.props.treeContext.searchKey;
        if (!searchKey) {
            return text;
        } else {
            return <$g.I18NProvider msg={text.replace(searchKey, "{0}")}>
                <span className="ra-tree-node-text-red">{searchKey}</span>
            </$g.I18NProvider>;
        }
    }

    render() {
        var item = this.props.item;
        let nodeStatusInfo = item.nodeStatusInfo;
        let nodeClassName = "ra-tree-node-text";
        if (item.clickNodeExpand) {
            nodeClassName = nodeClassName+ " ra-tree-node-text-click";
        }
        return <React.Fragment>
            {item.showStatus && <div className={"ra-tree-node-status"} style={{...nodeStatusInfo.iconScaleStyle}}>
                <div className={nodeStatusInfo.iconClass} style={{color: nodeStatusInfo.color}} data-tooltip aria-label={nodeStatusInfo.name}></div>
                {/* <div className="margin-left-s">{item.name}</div> */}
            </div>}
            <div
                className={"ra-tree-node-content"} aria-label={item.text} data-tooltip="true">
                <div className={`${this.getNodeIconClass(item)} ra-tree-node-icon`}></div>
                <div className={nodeClassName}>
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
