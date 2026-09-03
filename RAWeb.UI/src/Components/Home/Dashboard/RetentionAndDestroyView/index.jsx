import DeletedData from "./DeletedData";
import Retentionlist from "./List";

import "./index.less";

function RetentionAndDestroyView({ archivedRetentionData }) {
    return (
        <div className="reco-dashboard-soadmin-view-wrapper">
            <div className="reco-dashboard-so-layout-wrapper">
                <section className="reco-dashboard-cards">
                    <DeletedData
                        archivedRetentionData={archivedRetentionData}
                    />
                </section>
                <section className="reco-dashboard-cards">
                    <Retentionlist />
                </section>
            </div>
        </div>
    );
}

export default RetentionAndDestroyView;
