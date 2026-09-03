import { useEffect, useState } from 'react';
import UnderReview from "./Main/UnderReview";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import "./Index.less";

const MachineLearningReview = () =>{

    const [filterAvailableOptions, setFilterAvailableOptions] = useState(new Map());

    useEffect(()=>{
        initFilterAvailableOptions();
    },[]);

    const initFilterAvailableOptions = async () => {
        const options = await fetchUtility({ url: "/api/ManualApproval/GetFilterDefaultOptions" });
        const map = new Map();
        for (const option of options) {
            map.set(option.defaultOption, option.value);
        }
        setFilterAvailableOptions(map);
    };

    const renderSiteMap = () => {
        return <$g.SiteMap data={[SiteMapLinks.MT_MachineLearningReview]}></$g.SiteMap>;
    };

    return <div id="raMachineLearningReview">
        {renderSiteMap()}
        {<UnderReview
            filterAvailableOptions={filterAvailableOptions}
        />}
    </div>;
};

export default MachineLearningReview; 