import StringUtil from "../../../../Utilities/StringUtil";
import { bindEvents } from "../../../../Utilities/CommonUtil";

export default class ManualReviewTable extends R.Component {

    componentCreate() {
        bindEvents(this, "onPageChange");
        this.state = {
            items : this.props.items,
            columns : this.props.columns,
            template : this.props.template,
            currentReview : this.props.items.reviewAudits.slice(0,10),
            manualReviewPager: {
                itemsCount: (this.props.items.reviewAudits || []).length,
                pagerIndex: 0,
                pagerSize: 10
            },
        };
    }

    onPageChange(index, size, callback) {
        let pagerInfo = {};
        let manualReviewInfo = RM.deepcopy(this.state.items);
        let reviewAudits = manualReviewInfo.reviewAudits || [];
        pagerInfo.pagerSize = size;
        pagerInfo.pagerIndex = index;
        reviewAudits = reviewAudits.slice(index * size, (index + 1) * size);
        pagerInfo.itemsCount = (this.state.items.reviewAudits || []).length;
        this.setState({
            currentReview : reviewAudits,
            manualReviewPager: pagerInfo
        });
        if (callback) {
            callback(true);
        }
    }

    render() {
        let pagerInfo = this.state.manualReviewPager;
        return (
            <div className="manual-detail-manual-reivew">
                <div className='manual-detail-manual-reivew-owner'>
                    <$g.DetailList className="" labelWidth={180}>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_RecordOwner)}
                                value={this.state.items.recordOwner} />
                        </$g.DetailRow>
                    </$g.DetailList>
                </div>
                <div className='manual-detail-manual-reivew-title' tabIndex="0">{StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_ReviewActivities)}</div>
                <R.Table 
                    id={"reco-manual-view-detail-table"}
                    rowTemplate={this.state.template}
                    items={this.state.currentReview}
                    columns={this.state.columns}
                >
                </R.Table>
                <div className="table-foot-right">
                    <$g.Pager
                        itemsCount={pagerInfo.itemsCount}
                        pagerIndex={pagerInfo.pagerIndex}
                        pagerSize={pagerInfo.pagerSize}
                        showPagerSize={true}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.onPageChange} />
                </div>
            </div>
        );
    }
}