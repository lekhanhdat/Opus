import React from "react";
import PropTypes from "prop-types";
import { StepperHeader } from "./stepperHeader";
import { Stepper } from "./stepper";
import "./index.less";

const Wizard = ({ headerName, activeStep, items, children, onChange }) => {
    return (
        <div className="opus-common-wizard">
            <div className="wizard-left">
                <div className="wizard-stepper-header">
                    <StepperHeader
                        headerName={headerName}
                        blockCount={items.length}
                        activeStep={activeStep}
                    />
                </div>
                <Stepper activeStep={activeStep} items={items} onChange={onChange}></Stepper>
            </div>
            <div className="wizard-right">{children}</div>
        </div>
    );
};

Wizard.propTypes = {
    headerName: PropTypes.string,
    activeStep: PropTypes.number.isRequired,
    items: PropTypes.arrayOf(
        PropTypes.shape({
            text: PropTypes.string.isRequired,
            onRender: PropTypes.func,
        })
    ).isRequired,
    children: PropTypes.node,
    onChange: PropTypes.func
};

export default Wizard;
