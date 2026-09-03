import React from "react";
import { getSelectedCountClassName } from "../utils";
import PropTypes from "prop-types";
import "./index.less";

export const Step = ({ item, isSelected, isCompleted, stepText, onChange }) => {
	const renderStepperCountContent = () => {
		if (isCompleted) {
			return <div style={{ cursor: "pointer" }} className="fia-check"></div>;
		}

		return <div className="stepper-text">{stepText}</div>;
	}

	const handleKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

	return (
		<div className="opus-wizard-process-step">
			<div
				className={`stepper-count ${getSelectedCountClassName(
					isCompleted,
					isSelected
				)}-count`}
				onKeyDown={handleKeyDown}
				onClick={isCompleted ? onChange : null}
			>
				{renderStepperCountContent()}
			</div>
			<div
				className={`stepper-content ${getSelectedCountClassName(
					isCompleted,
					isSelected
				)}-text`}
			>
				{item.text ?? item.onRender?.()}
			</div>
		</div>
	);
};

Step.propTypes = {
  item: PropTypes.shape({
    text: PropTypes.string.isRequired,
    onRender: PropTypes.func,
  }).isRequired,
  stepText: PropTypes.oneOfType([
    PropTypes.string,
    PropTypes.number,
  ]).isRequired,
  isSelected: PropTypes.bool.isRequired,
  isCompleted: PropTypes.bool.isRequired,
  onChange: PropTypes.func,
};
