import OverviewColumnValue from "./OverViewColumnValue";

const ManaulOverview = ({details}) => {

    return (
        <div className='reco-manual-detail-overview'>
            <div className='reco-manual-detail-overview-title' tabIndex="0">{RMResx.RM_PRM_PRE_MRR_Details_Section_OverView}</div>
            <$g.DetailList className="category-content" labelWidth={180}>
                {
                    details.map(detail => {
                        return(
                            <OverviewColumnValue key={detail.key} column={detail.column} value={detail.value} type={detail.type}/>
                        );
                    })
                }
            </$g.DetailList>
        </div>
    );
};

export default ManaulOverview;