import ConfigScopeTable from "./Components/ConfigScopeTable";
export default class ConfigScope extends R.Component {
    idAttr = true;
    componentCreate() {
        this.containerItems = [];
        this.searchedItems = [];
        this.searchKey = "";
        this.configScopeColumns = [];
        this.scopeContainerId = "raScopeContainer";
        this.state = {
            selectedCount: 0,
            shownCount: 0,
            totalCount: 0,
            pagerIndex: 0,
            pagerSize: 10,
            showTip: false,
            showMessageTip: this.showMessageTip
        };
        this.bind('onCheckChanged', 'onScopeCellClick', 'onSearch', 'onPageChange');
    }

    componentInit() {
        
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "onSave":
                this.saveContainer(args[0]);
                break;
            case "init":
                this.initContainers(args[0]);
                break;
        }
    }

    initContainerColumns() {
        return [
            {
                header: this.sourceType === 4 ? RMResx.RM_CP_AM_Table_Column_Location : RMResx.RM_CP_AM_Table_Column_ContainerName,
                width: 400,
                align: "start"
            }];
    }

    initContainers(scopeData)
    {
        let scopeInfo = RM.deepcopy(scopeData);
        let scopeContainers = scopeInfo.Containers;
        this.containerItems = scopeContainers;
        this.sourceType = scopeInfo.SourceType;
        // this.setSelectedInfo();
        this.configScopeColumns = this.initContainerColumns();
        this.onPageChange(0, this.state.pagerSize);
    }

    onSearch = (args) => {
        let key = $.trim(args);
        if(key)
        {
            this.searchKey = key;
            let allItems = RM.deepcopy(this.containerItems);
            this.searchedItems = allItems.filter(o => o.Name.toLowerCase().indexOf(key.toLowerCase()) > -1);
            this.onPageChange(0, this.state.pagerSize);
        } else {
            this.searchKey = "";
            this.onPageChange(0, this.state.pagerSize);
        }
    }

    showMessageTip = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip = () => {
        this.setState({
            showTip: false
        });
    }

    saveContainer(callback) {
        callback({SourceType: this.sourceType, ContainerItems: this.containerItems});
    }
    
    onCheckChanged(items) {
        let currentPageItems = items.slice();
        this.resetContainerItemsStatus(currentPageItems);
        // this.setSelectedInfo();
    }

    resetContainerItemsStatus(items)
    {
        if(this.searchKey)
        {
            this.searchedItems.map(o => {
                let item = items.find(t => t.Id == o.Id);
                if(item !== undefined)
                {
                    o.isChecked = item.isChecked;
                }
            });
        }
        this.containerItems.map(o => {
            let item = items.find(t => t.Id == o.Id);
            if(item !== undefined)
            {
                o.isChecked = item.isChecked;
            }
        });
    }

    setSelectedInfo()
    {
        let items = this.containerItems;
        this.setState({
            selectedCount: this.getSelectedItems(items).length,
            totalCount: items.length
        });
    }

    onPageChange(pageIndex, pageSize, callback) {
        let items = this.searchKey? this.searchedItems : this.containerItems;
        let currentPageItems = JSON.parse(JSON.stringify(items.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        this.dispatch(this.scopeContainerId, "init", currentPageItems, this.configScopeColumns);
        this.setState({
            pagerIndex: pageIndex,
            pagerSize: pageSize,
            shownCount: currentPageItems.length,
            totalCount: items.length
        });
        if (callback) {
            callback(true);
        }
    }

    renderPager() {
        return <div className='ra-main-footer'>
            <$g.Pager
                itemsCount={this.state.totalCount}
                pagerIndex={this.state.pagerIndex}
                pagerSize={this.state.pagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onPageChange}/>
        </div>;
    }

    render() {
        return <div id={this.props.id}>
            <div className='ra-page-container'>
                {/* <div className='navbar-right'> */}
                {/* <div className="navbar-selected-count">{`${this.state.selectedCount}/${this.state.totalCount} Selected`}</div> */}
                <div className='ra-main-header'>
                    <R.Searchbox
                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                        disabled={false}
                        onSearch={this.onSearch}
                        width={380}
                    />
                </div>
                {/* </div>            */}
                <div className="ra-main-table">
                    <ConfigScopeTable
                        id={this.scopeContainerId}
                        columnInfo={this.configScopeColumns}
                        onCheckChanged={this.onCheckChanged}
                    />            
                </div>
                {this.renderPager()}
            </div>
        </div>;
    }
}