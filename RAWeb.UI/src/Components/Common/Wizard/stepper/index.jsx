import React from "react";
import PropTypes from "prop-types";
import { Step } from "../step";
import "./index.less";

export const Stepper = ({ items, activeStep, onChange }) => {
    return (
        <div className="opus-wizard-process-stepper">
            {items.map((item, index) => {
                return (
                    <div
                        className="stepper-item"
                    >
                        <Step
                            item={item}
                            isSelected={activeStep === index + 1}
                            isCompleted={activeStep > index + 1}
                            stepText={index + 1}
                            onChange={() => onChange(index)}
                        ></Step>
                    </div>
                );
            })}
        </div>
    );
};

Stepper.propTypes = {
    items: PropTypes.arrayOf(
        PropTypes.shape({
            text: PropTypes.string.isRequired,
            onRender: PropTypes.func,
        })
    ).isRequired,
    activeStep: PropTypes.number.isRequired,
	onChange: PropTypes.func
};
