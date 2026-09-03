import {Component} from "react";
import PropTypes from "prop-types";
import {CheckboxSelectFilter} from "../../CheckboxSelectFilter";

let maxIdIndex = 0;
let shownHeaderId = 0;

class ColumnHeader extends Component {
    constructor(props) {
        super(props);
        this.headerId = ++maxIdIndex;
        this.state = {
            isSortBoxShow: false,   //判断显示隐藏
            isAscBtnShow: false,
            sortIconClassName: "fia-datagrid-button ra-grid-sort-icon"  //初始化样式名
        };
        this.initBinding();
    }

    initBinding() {
        const eventsArr = ["isSortBoxShowFun", "columnClick", "sortBtnClick", "filterFormClick", "onFilter"];
        eventsArr.forEach((ev) => {
            this[ev] = this[ev].bind(this);
        });
    }

    componentDidMount() {
        document.addEventListener("click", this.isSortBoxShowFun);
    }

    // 组件销毁监听
    componentWillUnmount() {
        this.isUnmounted = true;
        document.removeEventListener("click", this.isSortBoxShowFun, false);
    }

    // 点击筛选按钮
    sortBtnClick(event) {
        this.isClickSortBtn = true;
        if (this.state.isSortBoxShow) {
            shownHeaderId = null;
        } else {
            shownHeaderId = this.headerId;
        }
    
        // event.stopPropagation();
    }

    filterFormClick(event) {
        this.stopPropagation(event);
    }

    //点击column切换排序
    columnClick() {
        if(!this.isClickSortBtn){
            shownHeaderId = null;
            this.setState({
                isAscBtnShow: !this.state.isAscBtnShow
            }, () => {
                this.onSort(this.state.isAscBtnShow);
            });
        }
    }

    //排序方法
    onSort(isAsc) {
        this.setState({
            isSortBoxShow: false,
        });
        if (isAsc) {
            this.setState({
                isAscBtnShow: true,
                sortIconClassName: "fia-datagrid-sort-up ra-grid-sort-icon"
            });
        } else {
            this.setState({
                isAscBtnShow: false,
                sortIconClassName: "fia-datagrid-sort-down ra-grid-sort-icon"
            });
        }
        shownHeaderId = null;
        this.clearSearchInput();
        this.props.onSort(isAsc);
    }

    //隐藏
    isSortBoxShowFun() {
        if(this.isUnmounted) {
            return;
        }
        if (this.headerId != shownHeaderId || shownHeaderId == null) {
            this.setState({
                isSortBoxShow: false
            });
            this.clearSearchInput();
        } else {
            this.setState({
                isSortBoxShow: true
            });
            shownHeaderId = null;
        }
        this.isClickSortBtn = false;
    }

    clearSearchInput() {
        if(this.selectFilterRef) {
            this.selectFilterRef.clearSearchInput();
        }
    }

    onFilter(event, items) {
        if (this.props.onFilter) {
            this.props.onFilter(items);
        }
        shownHeaderId = null;
        this.isSortBoxShowFun(event);
    }

    //阻止冒泡
    stopPropagation(e) {
        e.nativeEvent.stopImmediatePropagation();
    }

    //清空数据
    selectFilterRender() {
        return <CheckboxSelectFilter
            ref={r => this.selectFilterRef = r}
            items={this.props.filterData}
            onSave={this.onFilter}
            onCancel={this.isSortBoxShowFun}
        >
        </CheckboxSelectFilter>;
    }

    sortRender() {
        return <div className='ra-sort-box'>
            <div role="option" className='ra-sort-row' onClick={this.onSort.bind(this, true)}>
                <span className="fia-datagrid-az"></span>
                <span>{RMResx.RM_JS_Common_AUI_Datagrid_Filter_AtoZ}</span>
            </div>
            <div role="option" className='ra-sort-row' onClick={this.onSort.bind(this, false)}>
                <span className="fia-datagrid-za"></span>
                <span>{RMResx.RM_JS_Common_AUI_Datagrid_Filter_ZtoA}</span>
            </div>
        </div>;
    }

    //点击列
    render() {
        return <div>
            <div onClick={this.columnClick}>
                <span className='ra-grid-columnTitle'>{this.props.title}</span>
                <span 
                    tabIndex="0" role="button"
                    aria-label="sort &amp; filter"
                    className={this.state.sortIconClassName}
                    onClick={this.sortBtnClick}></span>
            </div>
            <div 
                className='ra-grid-filter'
                style={{display: (this.state.isSortBoxShow) ? "block" : "none"}}>
                {this.sortRender()}
                {this.props.filterData &&
                    <div onClick={this.filterFormClick}>
                        {this.selectFilterRender()}
                    </div>
                }
            </div>
        </div>;
    }
}

ColumnHeader.propTypes = {
    title: PropTypes.string
};
ColumnHeader.defaultProps = {
    title: ""
};

export {ColumnHeader};
