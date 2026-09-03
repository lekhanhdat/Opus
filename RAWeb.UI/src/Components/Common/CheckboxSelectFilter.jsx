import {bindEvents} from "../../Utilities/CommonUtil";

class CheckboxSelectFilter extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            searchTooltip: props.searchTooltip,
            allItems: this.cloneAllItems(props.items),  //当前popup展示的item
            searchedItems: null,
            isSelectedAll: true,
            isClearDisabled: true,
            isSearching: false,
            isOkBtnDisabled: false
        };
        bindEvents(this, "onSearch", "selectedAll", "selectClick", "clearClick", "okClick", "cancelClick");
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.items != this.props.items) {
            this.initData(nextProps.items);
        } else {
            this.initData(this.props.items);
        }
    }

    cloneAllItems(items) {
        return JSON.parse(JSON.stringify(items));
    }

    clearClick() {
        if (!this.state.isClearDisabled) {
            for (let item of this.props.items) {
                item.checked = true;
            }
            for (let item of this.state.allItems) {
                item.checked = true;
            }
            this.setState({
                isSelectedAll: true
            });
            if (this.props.onSave) {
                this.props.onSave(event, this.props.items);
            }
        }
    }

    //清空组件
    clearFilterRender() {
        return (
            <div
                className='ra-clear-filter'
                style={{color: (this.state.isClearDisabled) ? 'rgb(170, 170, 170)' : 'rgb(57, 57, 57)'}}
                onClick={this.clearClick}>
                <span className="fia-funnel-clear"></span>
                <span> {RMResx.RM_JS_Common_AUI_Datagrid_Filter_ClearFilter}</span>
            </div>
        );
    }

    onSearch(args) {
        let searchValue = (args || "").trim();

        if (searchValue === "") { 
            this.setState({
                isSearching: true,
                searchedItems: this.state.allItems
            });
        } else {
            let filteredData = this.state.allItems.filter((item) => {
                return item.name && item.name.toUpperCase().indexOf(searchValue.toUpperCase()) != -1;
            });
            this.setState({
                isSearching: true,
                searchedItems: filteredData
            });
        }
    }

    selectedAll(e) {
        let checked = e.target.checked;
        for (let item of this.state.allItems) {
            item.checked = checked;
        }
        this.setState({
            allItems: this.state.allItems,
            isSelectedAll: checked,
            isOkBtnDisabled: !checked,
        });

    }

    selectClick = (currentItem, e) => {
        currentItem.checked = e.target.checked;
        let selectAll = this.isSelectAll(this.state.allItems);
        let okBtnDisabled = this.okBtnDisabled(this.state.allItems);
        this.setState({
            allItems: this.state.allItems,
            isSelectedAll: selectAll,
            isOkBtnDisabled: okBtnDisabled
        });
    }

    initData(items) {
        this.setState({
            isSearching: false,
            isOkBtnDisabled: false,
            allItems: this.cloneAllItems(items),
            isClearDisabled: this.isSelectAll(items),
            isSelectedAll: this.isSelectAll(items)
        });
    }

    okBtnDisabled(items) {
        let isDisabled = true;
        for (let item of items) {
            if (item.checked) {
                isDisabled = false;
                break;
            }
        }
        return isDisabled;
    }

    isSelectAll(items) {
        let checkedCount = 0;
        let selectAll = true;
        for (let item of items) {
            if (item.checked) {
                checkedCount++;
            }
        }
        if (items.length == checkedCount) {
            selectAll = true;
        }
        // else if (0 < checkedCount && checkedCount < items.length) {
        //     selectAll = null;
        // }
        else {
            selectAll = false;
        }
        return selectAll;
    }

    saveCheckedStatus() {
        let checkedItemIDs = [];
        for (let item of this.state.allItems) {
            if (item.checked) {
                checkedItemIDs.push(item.id);
            }
        }
        for (let item of this.props.items) {
            item.checked = checkedItemIDs.indexOf(item.id) > -1;
        }
    }

    okClick(event) {
        this.stopPropagation(event);
        this.saveCheckedStatus();
        if (this.props.onSave) {
            this.props.onSave(event, this.state.allItems);
        }
    }

    cancelClick(event) {
        this.stopPropagation(event);
        if (this.props.onCancel) {
            this.props.onCancel(event);
        }
    }

    //阻止冒泡
    stopPropagation(e) {
        e.stopImmediatePropagation();
    }

    //筛选组件
    selectFilterRender() {
        let viewItems = this.state.isSearching
            ? this.state.searchedItems
            : this.state.allItems;

        return <div className='ra-CheckboxSelectFilter'>
            <R.Searchbox
                ref={r => this.searchBoxRef = r}
                placeholder={RMResx.RM_JS_JM_SearchKeyWord}
                disabled={false}
                onSearch={this.onSearch}
                width={227}
            />
            <div className='ra-search-popup-content'>
                <label className='ra-search-popup-row-all strong'>
                    <input type="checkbox" checked={this.state.isSelectedAll} onChange={this.selectedAll}/>
                    {RMResx.RM_JS_BCM_Explorer_Filter_All}
                    {/*<R.Checkbox*/}
                    {/*    name="checkboxAll"*/}
                    {/*    text={RMResx.RM_JS_BCM_Explorer_Filter_All}*/}
                    {/*    checked={this.state.isSelectedAll}*/}
                    {/*    isSeparate={true}*/}
                    {/*    onChange={this.selectedAll}*/}
                    {/*/>*/}
                </label>
                {
                    viewItems.map((item, index) => {
                        return <label className='ra-search-popup-row strong' key={item.id}>
                            <input type="checkbox" value={item.name} checked={item.checked}
                                onChange={this.selectClick.bind(this, item)}/>
                            <span className='ra-search-popup-checkbox-value' data-tooltip aria-label={item.name}>{item.name}</span>
                        </label>;
                    })
                }
            </div>
            <div id="rm_control_filter-btn">
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_OK}
                    disabled={this.state.isOkBtnDisabled}
                    onClick={this.okClick}/>
                <R.Button
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.cancelClick}/>
            </div>
        </div>;
    }

    clearSearchInput() {
        this.searchBoxRef.clear();
    }

    render() {
        return <React.Fragment>
            {this.clearFilterRender()}
            {this.selectFilterRender()}
        </React.Fragment>;
    }
}

export {CheckboxSelectFilter};