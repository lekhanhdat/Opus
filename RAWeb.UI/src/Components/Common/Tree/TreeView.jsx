import TreeList from './Components/TreeList';
import BaseContextTemplate from './BaseContextTemplate';

let treeNO = 0;
export class TreeView extends React.Component {
    constructor(props) {
        super(props);
        this.treeContext = this.props.treeContext;
        let tempContext = Object.assign(new BaseContextTemplate(), this.treeContext);
        Object.assign(this.treeContext, tempContext);
        this.treeContext.treeNO = ++treeNO;
        this.treeItems = this.transToTreeItems(props.items);
    }
    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.items != this.props.items) {
            this.treeItems = this.transToTreeItems(nextProps.items);
        }
    }
    transToTreeItems(items) {
        return items.map((item, index) => {
            return this.treeContext.transToTreeNodeObject(item);
        });
    }
    render() {
        return (
            <div id={this.props.id} className="ra-treeview">
                <div className={"ra-tree" + (this.props.classicMode ? " ra-tree-classic" : "") + " ra-tree-page" }>
                    <TreeList
                        treeContext={this.treeContext}
                        show={true}
                        items={this.treeItems} />
                </div>
            </div>
        );
    }
}