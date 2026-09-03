import { bindEvents } from '../../../../../Utilities/CommonUtil';
import { TreeActionsPopup, TreeActionItem } from '../../Components/TreeActions';

//ref: Report Term Tree and Location Tree
export default class TermNodeContent extends React.Component {
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

    onRefreshActionClick(e) {
        this.props.item.loaded = false;
        if(this.treeContext.treeType == "CRM"){
            //refresh 给主页面传值的情况
            this.props.itemComponent.loadNodes(0, this.props.item.pagerSize,(success) => {
                this.treeContext.onNodeRefresh();
            });
        }else{
            this.props.itemComponent.reload(0);
        }
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

    render() {
        let item = this.state.item;
        return <React.Fragment>
            <div className={"ra-tree-node-content"} aria-label={item.text} data-tooltip="true">
                <$g.Icon className={item.iconClass}/>
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