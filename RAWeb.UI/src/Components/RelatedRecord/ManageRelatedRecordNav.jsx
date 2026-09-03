const RelatedRecordsDataSource = {
    "500": RMResx.RM_RDM_RelatedRecordType_SharePoint,
    "9400": RMResx.RM_RDM_RelatedRecordType_PhysicalFolder,
    "9500": RMResx.RM_RDM_RelatedRecordType_PhysicalFile
};
export default class ManageRelatedRecordNav extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            navItems: [],
            pagerIndex: 0,
            pagerSize: 10,
            shownCount: 0,
            pagerHasNext: false,
            currentPagerNavItems: []
        };
    }

    componentInit() {
        this.setNavItems();
    }

    setNavItems(){
        $$.loading(true);
        let option = {
            url: "/api/RelatedRecordsApi/GetRelatedRecords",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res){
                let relatedRecords = JSON.parse(res);
                let currentPagerNavItems = relatedRecords.slice(0, this.state.pagerSize);
                relatedRecords[0].isSelected = true;
                this.setState({
                    navItems: relatedRecords,
                    shownCount: currentPagerNavItems.length,
                    pagerHasNext: relatedRecords.length > this.state.pagerSize,
                    currentPagerNavItems: currentPagerNavItems
                });  
                this.dispatch("raManageRelatedRecordDetail", relatedRecords[0]);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onClickNav = (item) =>{
        for(let navItem of this.state.currentPagerNavItems){
            navItem.isSelected = navItem.id === item.id;
        }
        this.setState({currentPagerNavItems: RM.deepcopy(this.state.currentPagerNavItems)});
        this.dispatch("raManageRelatedRecordDetail", item);
    }

    pagerChange = (pagerIndex, pagerSize) =>{
        let currentPagerNavItems = this.state.navItems.slice(pagerIndex * pagerSize, (pagerIndex + 1)* pagerSize);
        this.setState({
            pagerIndex: pagerIndex,
            pagerSize: pagerSize,
            pagerHasNext: this.state.navItems.length  >  (pagerIndex + 1) * pagerSize,
            shownCount: currentPagerNavItems.length,
            currentPagerNavItems: currentPagerNavItems
        });
    }

    render() {
        return <div id="raRelatedRecordNav">
            { 
                this.state.currentPagerNavItems.map((item, index)=>{
                    let navItemClass = item.isSelected ? "rd-manage-nav-item rd-manage-nav-select" : "rd-manage-nav-item";
                    return <div key={index} className={navItemClass} onClick={this.onClickNav.bind(this, item)}>
                        <div className="nav-item-title text-overflow" data-tooltip="ifneed">{item.name}</div>
                        <div className="nav-item-type">{RelatedRecordsDataSource[item.NodeType]}</div>
                    </div>;
                })   
            }   
            <div className="rd-nav-pager">
                {
                    this.state.currentPagerNavItems.length > 0 && 
                    <$g.SimplePager
                        pagerIndex={this.state.pagerIndex}
                        pagerSize={this.state.pagerSize}
                        shownCount={this.state.shownCount}
                        hasNext={this.state.pagerHasNext}
                        onChange={this.pagerChange}
                    ></$g.SimplePager>
                }
            </div>
        </div>; 
    }
}