import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import "./index.less";

import { LocationSummary, EndUserLocationSummary, TermSummary } from "./LocationSummary";
import { TopLocations, TopTerms } from "./Tops";
import Request from "./Request";

const RequestOption = {
    url: "/api/Dashboard/IsPhysicalEndUser",
};

const PhysicalReport = ({ isTermView }) => {

    const [isEndUser, setIsEndUser] = useState(true);

    const [isInit, setIsInit] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            const responseDatas = await fetchUtility(RequestOption);
            setIsEndUser(responseDatas);
            setIsInit(true);
        };

        fetchData();
    }, []);

    return (
        isInit ?
            <div className="reco-phy-report-wrapper">
                {isEndUser ? <EndUserLocationSummary /> : (isTermView ? <TermSummary/> : <LocationSummary />)}
                {!isEndUser && (isTermView ? <TopTerms /> : <TopLocations />)}
                <Request />
            </div> :
            <div></div>
    );
};

PhysicalReport.propTypes = {
    isTermView: PropTypes.bool,
};

export default PhysicalReport;