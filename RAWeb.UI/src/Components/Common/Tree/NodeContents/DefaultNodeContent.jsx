import { bindEvents } from '../../../../Utilities/CommonUtil';

export default class DefaultNodeContent extends React.Component {
    constructor(props) {
        super(props);
        bindEvents(this);
    }

    getNodeIconClass(item) {
        if(item.iconClass){
            return item.iconClass;
        } else {
            return "fia-term-set";
        }
    }

    renderNodeText(text) {
        let searchKey = this.props.treeContext.searchKey;
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
        var item = this.props.item;
        return (
            <div className={"ra-tree-node-content"}>
                <$g.Icon className={"ra-tree-node-icon " + this.getNodeIconClass(item)}></$g.Icon>
                <div className={"ra-tree-node-text"} aria-label={item.text} data-tooltip="ifneed">
                    {this.renderNodeText(item.text)}
                </div>
            </div>
        );
    }
}
