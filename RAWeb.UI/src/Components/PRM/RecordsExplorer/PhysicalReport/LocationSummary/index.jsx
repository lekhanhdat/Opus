import React, { useEffect, useState } from "react";
import { useHistory } from 'react-router-dom';
import RouterUrls from "../../../../../Constants/RouterUrls";
import { PickListForLoanStatusType, PickListForDestroyStatusType } from "../../../../../Constants/Constants";
import PropTypes from "prop-types";
import "./index.less";

const LocationSummaryCounter = ({ children, icon, title }) => {
    return (
        <div className="reco-location-summary-counter-wrapper">
            <div className={`reco-location-summary-counter-icon ${icon}`}></div>
            <div className="reco-location-summary-counter">
                <div className="reco-location-summary-counter-title"
                    tabIndex="0"
                    data-tooltip="ifneed"
                    aria-label={title}>
                    {title}
                </div>
                {children}
            </div>
        </div>
    );
};

LocationSummaryCounter.propTypes = {
    children: PropTypes.oneOfType([PropTypes.element, PropTypes.arrayOf(PropTypes.element)]),
    icon: PropTypes.string,
    title: PropTypes.string
};

const DashboardPhysicalLoan = {
    LoanTotal: "LoanTotal",
    LoanExpiredTotal: "LoanExpiredTotal",
};

const TotalLocationRequest = {
    url: "/api/Dashboard/GetPhysicalLocationTotal"
};

const PhysicalLoanExpriedAndTotalRequset = {
    url: "/api/Dashboard/GetPhysicalLoanExpriedAndTotal"
};

const PhysicalPendingLoanRequset = {
    url: "/api/Dashboard/GetPhysicalLoanPenddingTotal"
};

const PhysicalPendingDestroyRequset = {
    url: "/api/Dashboard/GetPhysicalDestructionPenddingTotal"
};

const isHoldManagerRole = () => Number(RM && RM.RoleType) === 5;

const getDisabledLinkProps = (isDisabled) => {
    if (!isDisabled) {
        return {};
    }

    return {
        "aria-disabled": true,
        onClick: (event) => event.preventDefault(),
        style: {
            color: "#9aa0a6",
            cursor: "not-allowed",
            pointerEvents: "none",
            textDecoration: "none"
        }
    };
};

const getHrefLinkProps = (url, isDisabled) => {
    if (isDisabled) {
        return getDisabledLinkProps(true);
    }

    return { href: url };
};

const getActionLinkProps = (action, isDisabled) => {
    if (isDisabled) {
        return getDisabledLinkProps(true);
    }

    return { onClick: action };
};

const LocationSummary = () => {

    const [totalLocation, setTotalLocation] = useState(0);

    const [expiredLoanCount, setExpiredLoanCount] = useState(".");

    const [loanTotal, setLoanTotal] = useState(".");

    const [pendingToLoanCount, setPendingToLoanCount] = useState("0");

    const [pendingToDestroyCount, setPendingToDestroyCount] = useState("0");

    const disableNavigation = isHoldManagerRole();

    let history = useHistory();

    useEffect(() => {
        requestTotalLocation();
        requestLoanExpiredAndTotal();
        requestPendingLoanTotal();
        requestPendingDestroyTotal();
    }, []);

    const routeTo = (url, param) =>{
        history.push({pathname: url, query: param}) ;
    };

    const requestTotalLocation = async () => {
        const responseData = await fetchUtility(TotalLocationRequest);
        setTotalLocation(responseData);
    };

    const requestLoanExpiredAndTotal = async () => {
        const responseData = await fetchUtility(PhysicalLoanExpriedAndTotalRequset);
        setExpiredLoanCount(responseData[DashboardPhysicalLoan.LoanExpiredTotal]);
        setLoanTotal(responseData[DashboardPhysicalLoan.LoanTotal]);
    };

    const requestPendingLoanTotal = async () => {
        const responseData = await fetchUtility(PhysicalPendingLoanRequset);
        setPendingToLoanCount(responseData);
    };

    const requestPendingDestroyTotal = async () => {
        const responseData = await fetchUtility(PhysicalPendingDestroyRequset);
        setPendingToDestroyCount(responseData);
    };

    return (
        <div className="reco-phy-location-summary-wrapper">
            <section className="reco-phy-report-section-title" tabIndex="0">
                {RMResx.RM_Phy_DSB_Title_Summary}
            </section>
            <section className="reco-phy-location-summary-counters">
                <LocationSummaryCounter icon="fia-location" title={RMResx.RM_Phy_DSB_Total_Locations}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getHrefLinkProps("/Root/PRM/LocationManagement", disableNavigation)}>
                            {totalLocation}
                        </a>
                    </div>
                </LocationSummaryCounter>
                <LocationSummaryCounter icon="fia-loan" title={RMResx.RM_Phy_DSB_Loan}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getHrefLinkProps("/Root/BCM/HybridSearch?source=-1", disableNavigation)}>
                            {expiredLoanCount}
                        </a>/<span tabIndex="0">
                            {loanTotal}
                        </span>
                    </div>
                </LocationSummaryCounter>
                <LocationSummaryCounter icon="fia-pending-to-loan" title={RMResx.RM_MT_PickList_Status_PendingLoan}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getActionLinkProps(() => {
                            routeTo(RouterUrls.MT_PickListForLoanRequests, { Status: PickListForLoanStatusType.Pendding });
                        }, disableNavigation)}>
                            {pendingToLoanCount}
                        </a>
                    </div>
                </LocationSummaryCounter>
                <LocationSummaryCounter icon="fia-pending-to-destroy" title={RMResx.RM_MT_PickList_Status_PendingDestroy}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getActionLinkProps(() => {
                            routeTo(RouterUrls.MT_PickListForDestruction, { Status: PickListForDestroyStatusType.Pendding });
                        }, disableNavigation)}>
                            {pendingToDestroyCount}
                        </a>
                    </div>
                </LocationSummaryCounter>
            </section>
        </div>
    );
};

const TotalTermRequest = {
    url: "/api/Dashboard/GetPhysicalTermTotal"
};

const TermSummary = () => {

    const [totalTerm, setTotalTerm] = useState(0);

    const [expiredLoanCount, setExpiredLoanCount] = useState(".");

    const [loanTotal, setLoanTotal] = useState(".");

    const [pendingToLoanCount, setPendingToLoanCount] = useState("0");

    const [pendingToDestroyCount, setPendingToDestroyCount] = useState("0");

    const disableNavigation = isHoldManagerRole();

    let history = useHistory();

    useEffect(() => {
        requestTotalLocation();
        requestLoanExpiredAndTotal();
        requestPendingLoanTotal();
        requestPendingDestroyTotal();
    }, []);

    const routeTo = (url, param) =>{
        history.push({pathname: url, query: param}) ;
    };

    const requestTotalLocation = async () => {
        const responseData = await fetchUtility(TotalTermRequest);
        setTotalTerm(responseData);
    };

    const requestLoanExpiredAndTotal = async () => {
        const responseData = await fetchUtility(PhysicalLoanExpriedAndTotalRequset);
        setExpiredLoanCount(responseData[DashboardPhysicalLoan.LoanExpiredTotal]);
        setLoanTotal(responseData[DashboardPhysicalLoan.LoanTotal]);
    };

    const requestPendingLoanTotal = async () => {
        const responseData = await fetchUtility(PhysicalPendingLoanRequset);
        setPendingToLoanCount(responseData);
    };

    const requestPendingDestroyTotal = async () => {
        const responseData = await fetchUtility(PhysicalPendingDestroyRequset);
        setPendingToDestroyCount(responseData);
    };

    return (
        <div className="reco-phy-location-summary-wrapper">
            <section className="reco-phy-report-section-title" tabIndex="0">
                {RMResx.RM_Phy_DSB_Title_Summary}
            </section>
            <section className="reco-phy-location-summary-counters">
                <LocationSummaryCounter icon="fia-term-set" title={RMResx.RM_Phy_DSB_Total_Terms}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getHrefLinkProps("/Root/BCM/TermManagement", disableNavigation)}>
                            {totalTerm}
                        </a>
                    </div>
                </LocationSummaryCounter>
                <LocationSummaryCounter icon="fia-loan" title={RMResx.RM_Phy_DSB_Loan}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getHrefLinkProps("/Root/BCM/HybridSearch?source=-1", disableNavigation)}>{expiredLoanCount}</a>/<span tabIndex="0">{loanTotal}</span>
                    </div>
                </LocationSummaryCounter>
                <LocationSummaryCounter icon="fia-pending-to-loan" title={RMResx.RM_MT_PickList_Status_PendingLoan}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getActionLinkProps(() => {
                            routeTo(RouterUrls.MT_PickListForLoanRequests, { Status: PickListForLoanStatusType.Pendding });
                        }, disableNavigation)}>
                            {pendingToLoanCount}
                        </a>
                    </div>
                </LocationSummaryCounter>
                <LocationSummaryCounter icon="fia-pending-to-destroy" title={RMResx.RM_MT_PickList_Status_PendingDestroy}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getActionLinkProps(() => {
                            routeTo(RouterUrls.MT_PickListForDestruction, { Status: PickListForDestroyStatusType.Pendding });
                        }, disableNavigation)}>
                            {pendingToDestroyCount}
                        </a>
                    </div>
                </LocationSummaryCounter>
            </section>
        </div>
    );
};

const EndUserLocationSummary = () => {

    const [expiredLoanCount, setExpiredLoanCount] = useState(".");

    const [loanTotal, setLoanTotal] = useState(".");

    const disableNavigation = isHoldManagerRole();

    useEffect(() => {
        requestLoanExpiredAndTotal();
    }, []);

    const requestLoanExpiredAndTotal = async () => {
        const responseData = await fetchUtility(PhysicalLoanExpriedAndTotalRequset);
        setExpiredLoanCount(responseData[DashboardPhysicalLoan.LoanExpiredTotal]);
        setLoanTotal(responseData[DashboardPhysicalLoan.LoanTotal]);
    };

    return (
        <div className="reco-phy-location-summary-wrapper">
            <section className="reco-phy-report-section-title" tabIndex="0">
                {RMResx.RM_Phy_DSB_MyLoan_Title}
            </section>
            <section className="reco-phy-location-summary-counters">
                <LocationSummaryCounter icon="fia-total-loan" title={RMResx.RM_Phy_DSB_Total_Loan}>
                    <div className="reco-phy-location-summary-counter-count">
                        <span tabIndex="0">
                            {loanTotal}
                        </span>
                    </div>
                </LocationSummaryCounter>
                <LocationSummaryCounter icon="fia-loan" title={RMResx.RM_Phy_DSB_Expired_Loan}>
                    <div className="reco-phy-location-summary-counter-count">
                        <a className="highlight" tabIndex="0" {...getHrefLinkProps("/Root/BCM/HybridSearch?source=-1", disableNavigation)}>{expiredLoanCount}</a>
                    </div>
                </LocationSummaryCounter>
            </section>
        </div>
    );
};

export { LocationSummary, TermSummary, EndUserLocationSummary };