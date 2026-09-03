import '../../Less/PRM/EditTemplate.less';

export class ColumnTable extends R.Component {
    idAttr = true;
    constructor(props){
        super(props);
        this.bind(["onChangeOrder", "getOrderItems", "onColumnDelete", "onColumnEdit"]);
        this.state = {
            tableId: this.props.columnTableId,
            items: this.props.RowItems,
            rootData: {
                items: this.props.RowItems,
            }
        };
        
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        let items = nextProps.RowItems.slice();
        this.setState({
            items: items,
            rootData:{items: items},
        });
    }

    onColumnEdit(rowIndex) {
        this.props.showEditColumnWindow(this.props.categoryId, rowIndex);
    }

    onColumnDelete(rowIndex){
        let columnItems = this.state.items.slice();
        let newColItems = columnItems.filter(item => item.index != rowIndex);
        let newItemIndex = 1;
        newColItems.forEach(item => {
            item.index = newItemIndex;
            newItemIndex++;
        });
        this.props.UpdateRowDataSource(this.props.categoryId, newColItems);
    }

    onChangeOrder(args, itemInfo) {
        let sourceOrder = args.oldValue.order;
        let targetOrder = args.newValue.order;
        let items = this.state.items.slice();
        const sourceItem = items.find((item) => item.index == sourceOrder);
        const currentIndex = sourceItem.index;

        if (targetOrder == currentIndex) {
            this.setState({
                items,
                rowData: { items },
            });
        }

        const updatedItems = items.map((item) => {
            // Update index for sourceItem
            if (item.uniqueId == itemInfo.uniqueId) {
                return { ...item, index: targetOrder };
            }

            // Move item up, the others will increase the index
            if (targetOrder < currentIndex && item.index >= targetOrder && item.index < currentIndex) {
                return { ...item, index: item.index + 1 };
            }

            // Move item down, the others will decrease the index
            if (targetOrder > currentIndex && item.index <= targetOrder && item.index > currentIndex) {
                return { ...item, index: item.index - 1 };
            }

            return item;
        })

        let newColItems = updatedItems.sort(function(a,b){
            return a.index - b.index;
        });
        this.setState({
            items: newColItems,
            rowData:{items: newColItems},
        });
        this.props.UpdateRowDataSource(this.props.categoryId, newColItems);
    }

    getOrderItems(rowIndex){
        let rowItems = this.state.items.slice();
        let orderItems = [];
        rowItems.forEach((r, idx)=>{
            let orderNumber = idx + 1;
            let orderItem = {
                order: orderNumber,
                checked: rowIndex == orderNumber? true :false,
            };
            orderItems.push(orderItem);
        });
        return orderItems;
    }   

    render(){
        return <React.Fragment>
            <div className="custom-table-main" id={this.props.id}>
            {
                this.state.items.map((item, index) => {
                    let orderItems = this.getOrderItems(item.index);
                    let columnName = RMResx[item.columnName] || item.columnName;
                    return <div key={index} className="item-row">
                    <div className="item-cell" style={{width:"20%"}}>
                        <R.Combobox
                            id={"raPrmTplColListOrder" + index}
                            mini={true}
                            height={32}
                            searchable={false}
                            width="60"
                            popupWidth="auto"
                            textField='order'
                            valueField='order'
                            checkedField='checked'
                            excludeChecked
                            items={orderItems}
                            onChange={(args) => this.onChangeOrder(args, item)}
                        />
                    </div>
                    <div className="item-cell normal-text" style={{width:"35%"}} tabIndex='0' data-tooltip aria-label={columnName}>{columnName}</div>
                    <div className="item-cell normal-text" style={{width:"20%"}}>
                        <div tabIndex='0'>
                            {item.required && RMResx.RM_EditTemplate_ColumnRequired}
                            {!item.required && RMResx.RM_EditTemplate_ColumnNotRequired}
                        </div>
                    </div>
                    <div className="item-cell btn-group" style={{width:"25%"}}>
                        {
                            item.allowEdit && <R.Button
                                                id="raPrmTplColEditBtn"
                                                type="bald"
                                                icon="fia-edit icon-option-item"
                                                onClick={(e) => this.onColumnEdit(item.index)}
                                                tooltip={RMResx.RM_JS_Common_Edit}
                                                />
                        }
                        
                        {
                            item.allowEdit &&<R.Button
                                                type="bald"
                                                icon="fia-delete"
                                                onClick={(e) => this.onColumnDelete(item.index)}
                                                tooltip={RMResx.RM_JS_Common_Delete}
                                                />
                        }
                    </div>
                </div>;
                })
            }
            </div>
        </React.Fragment>;
    }
}



