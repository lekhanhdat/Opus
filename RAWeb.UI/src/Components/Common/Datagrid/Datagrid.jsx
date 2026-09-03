import PropTypes from 'prop-types';
import GridRow from './GridRowTemplate';
import { Select } from "./Components/Select.jsx";
import { GridCellType } from "../../../Constants/Constants";

let datagridIndex = 1;

class Datagrid extends React.Component {
    constructor(props) {
        super(props);
        this.datagridId = props.id || ("rmDatagrid_" + datagridIndex++);
        this.state = {
            cellItems: [],                                  //cell数据
            selectedItems: [],                              //选中的数据
            isSelectAll: false,                             //全选按钮状态
            renderGrid: false,                              //控制render渲染
            horizontal: $$.datagrid('horizontal').auto,  //判断宽度百分比还是px
            columns: this.props.columns,                     //表头数据
            hasSelectionInfo: this.props.hasSelection,
            showClear: props.showClear
        };
        this.cacheSelectedItems = [];                        //缓存勾选的数据
        this.initBinding();
    }

    initBinding() {
        const eventsArr = ['handleIsSelectAllChanged', 'handleRowDataChanged', 'clearSelected', 'pageChange'];
        eventsArr.forEach((ev) => {
            this[ev] = this[ev].bind(this);
        });
    }

    UNSAFE_componentWillReceiveProps(props) {
        //更新数据(select 调用)
        if (props.columns.length > 0 && props.columns[0].headerType == GridCellType.SelectAll) {
            if (props.selectedItems != this.props.selectedItems) {
                this.cacheSelectedItems = props.selectedItems ? props.selectedItems : [];
                this.getSelectStatus(props);
            }
            if (props.items != this.props.items) {
                //获取勾选状态
                this.getSelectStatus(props);
            }
        } else {
            if (props.items != this.props.items) {
                $$.setState(this, this.datagridId, {
                    cellItems: props.items,
                });
            }
        }

    }

    componentDidMount() {
        this.setCellWidth();
        this.getGridHeight(this.props.pager.pagerSize);
    }

    componentDidUpdate() {
        if (this.state.height != "auto") {
            $('#' + this.datagridId)
                .css("min-height", this.gridHeight + "px")
                .css("height", "auto");
        }
    }

    //字符串格式化方法
    stringFormat() {
        if (arguments.length == 0)
            return null;
        var str = arguments[0];
        for (var i = 1; i < arguments.length; i++) {
            var re = new RegExp('\\{' + (i - 1) + '\\}', 'gm');
            str = str.replace(re, arguments[i]);
        }
        return str;
    }

    //去重方法
    unique(items, attribute) {
        const res = new Map();
        return items.filter((item) => !res.has(item[attribute]) && res.set(item[attribute], 1));
    }

    getGridHeight(pagerSize) {
        let gridHeight = 0;
        if (this.props.height == 'auto' && pagerSize) {
            gridHeight = 30 * pagerSize + 55; //当修改分页是计算grid总高度
        } else {
            gridHeight = this.props.height;
        }
        this.setState({
            height: gridHeight
        });
        this.gridHeight = gridHeight;
    }

    //勾选回显
    getSelectStatus(props) {
        let newItems = JSON.parse(JSON.stringify(props.items));
        this.cacheSelectedItems.forEach((item) => {
            for (let key of newItems) {
                if (item[this.props.rowId] == key[this.props.rowId]) {
                    key.isChecked = true;
                }
            }
        });
        $$.setState(this, this.datagridId, {
            cellItems: newItems,
        }, () => {
            //全选状态
            this.setSelectedAllState();
        });
    }

    //设置cell的宽度
    setCellWidth() {
        //获取datagrid的宽度
        let raGridWidth = $(this.raGrid).width() - 2,
            columnsData = this.props.columns;
        // 百分比
        if (this.props.horizontalFlag == 'persent') {
            let totalPercents = 0,
                otherColumns = [],
                selectAllWidth = 40,
                othersWidth = raGridWidth - 40;
            for (let column of columnsData) {
                if (column.headerType == GridCellType.SelectAll) {
                    column.width = selectAllWidth;
                } else {
                    totalPercents += column.width;
                    otherColumns.push(column);
                }
            }
            for (let column of otherColumns) {
                column.width = othersWidth * column.width / totalPercents;
            }
        }
        // px直接赋值
        $$.setState(this, this.datagridId, {
            columns: columnsData
        });
        // 加载数据后render
        this.setState({ renderGrid: true }, () => {
            $('#' + this.datagridId).width(raGridWidth);
        });
    }

    //自定方法调用
    handleRowDataChanged(args) {
        //选中的item
        switch (args.parameters.actionType) {
            case 'checked':
                this.props.selectChange(this.getSelectedItems(args));
                break;
            default:
                break;
        }
    }

    //点击全选按钮
    handleIsSelectAllChanged(checked) {
        let id = this.props.rowId;
        let isSelectAll = checked;
        for (let item of this.state.cellItems) {
            item.isChecked = isSelectAll;
        }
        $$.setState(this, this.datagridId, {
            isSelectAll: isSelectAll
        });
        let selectedItems = this.state.cellItems.filter(item => {
            return item.isChecked == true;
        });
        this.setState({
            selectedItems: selectedItems
        });
        //根据全选状态添加缓存
        if (isSelectAll) {
            this.cacheSelectedItems = this.cacheSelectedItems.concat(selectedItems);
            this.cacheSelectedItems = this.unique(this.cacheSelectedItems, this.props.rowId);
        } else {
            this.cacheSelectedItems = this.cacheSelectedItems.filter(item => {
                let itemIds = this.props.items.map(object => object[id]);
                return itemIds.indexOf(item[id]) == -1;
            });
        }
        this.props.selectChange(this.cacheSelectedItems);
    }

    //勾选选项框
    getSelectedItems(agu) {
        let rowData = agu.newValue.rowData;
        let id = this.props.rowId;    //主页传唯一标识
        let currentSelectedItems = this.state.cellItems.filter(item => {
            return item.isChecked == true;
        });
        this.cacheSelectedItems = this.cacheSelectedItems.concat(currentSelectedItems);
        this.cacheSelectedItems = this.unique(this.cacheSelectedItems, this.props.rowId);
        //取消勾选
        for (let key in this.cacheSelectedItems) {
            if (!rowData.isChecked && (rowData[id] == this.cacheSelectedItems[key][id])) {
                this.cacheSelectedItems.splice(key, 1);
            }
        }
        this.setState({
            selectedItems: this.cacheSelectedItems
        });
        this.setSelectedAllState();
        //判断是否显示Clear Selection按钮
        return this.cacheSelectedItems;
    }

    //判断是否全选勾选
    setSelectedAllState() {
        let currentSelectedItems = this.state.cellItems.filter(item => {
            return item.isChecked == true;
        });
        if ((currentSelectedItems.length == this.state.cellItems.length) && currentSelectedItems.length != 0) {
            $$.setState(this, this.datagridId, {
                isSelectAll: true
            });
        } else if (currentSelectedItems.length == 0) {
            $$.setState(this, this.datagridId, {
                isSelectAll: false
            });
        } else {
            $$.setState(this, this.datagridId, {
                isSelectAll: null
            });
        }
    }

    //获取Column
    getColumnHeaders() {
        for (let key of this.props.columns) {
            if (key.headerType == GridCellType.SelectAll) {
                key.headerTemplate = <Select onChange={this.handleIsSelectAllChanged}
                    isChecked={this.state.isSelectAll}></Select>;
            }
        }
        return this.props.columns;
    }

    //分页
    pageChange(pagerIndex, pagerSize, callback) {
        this.getGridHeight(pagerSize);
        this.props.pager.pageChange(pagerIndex, pagerSize, callback);
    }

    //清空选项框
    clearSelected() {
        this.cacheSelectedItems = [];
        let newItems = JSON.parse(JSON.stringify(this.props.items));
        for (let key of newItems) {
            key.isChecked = false;
        }
        $$.setState(this, this.datagridId, {
            cellItems: newItems,
            selectedItems: []
        }, () => {
            //全选状态
            this.props.selectChange(this.cacheSelectedItems);
            this.setSelectedAllState();
        });
    }

    render() {
        return <div className="ra-grid ra-grid-autoHeight" ref={r => this.raGrid = r}>
            {this.state.renderGrid && <React.Fragment>
                <R.Datagrid
                    id={this.datagridId}
                    columns={this.getColumnHeaders()}
                    horizontal={$$.datagrid('horizontal').auto}
                    items={this.state.cellItems}
                    width={this.props.width}
                    height={this.state.height}
                    rowTemplate={GridRow}
                    rowBackgroundProperty="color"
                    rowDataChanged={this.handleRowDataChanged}
                    rootData={this.props.cells}
                    noneMessage={this.props.noneMessage}
                />
                <div className={this.props.allowPager ? 'ra-grid-footer show' : 'ra-grid-footer hidden'}>
                    <div className={this.state.hasSelectionInfo ? 'ra-grid-count show' : 'ra-grid-count hidden'}>
                        <span tabIndex='0'>
                            {this.stringFormat(RMResx.RM_JS_RDM_Rule_SelectedPageCalc, this.cacheSelectedItems.length, this.props.pager.itemsCount)}
                        </span>
                        {this.state.showClear &&
                            <a className='ra-grid-clear' tabIndex='0' role='button' onClick={this.clearSelected}
                                style={{ display: (this.cacheSelectedItems.length != 0) ? 'inline' : 'none' }}>{RMResx.RM_JS_JM_ClearSelected}</a>
                        }
                    </div>
                    <div className={this.props.isShowPager ? 'show' : 'hidden'}>
                        <$g.Pager
                            className="ra-grid-pager"
                            itemsCount={this.props.pager.itemsCount}
                            pagerIndex={this.props.pager.pagerIndex}
                            pagerSize={this.props.pager.pagerSize}
                            showPagerSize={true}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={this.pageChange} />
                    </div>
                </div>
            </React.Fragment>}
        </div>;
    }
}

const propTypes = {
    pager: PropTypes.object,
    width: PropTypes.number,
    height: PropTypes.string,
    cells: PropTypes.array,
    noneMessage: PropTypes.string,
    isShowPager: PropTypes.bool
};

const defaultProps = {
    pager: {
        itemsCount: 0,
        pagerIndex: 0,
        pagerSize: 0
    },
    width: 0,
    height: '500',
    cells: [],
    noneMessage: '',
    hasSelection: true,
    showClear: true,
    isShowPager: true
};

Datagrid.propTypes = propTypes;
Datagrid.defaultProps = defaultProps;

export { Datagrid };