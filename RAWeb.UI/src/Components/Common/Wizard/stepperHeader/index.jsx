import React from "react";
import PropTypes from "prop-types";
import { getSelectedClassName } from "../utils";
import "./index.less";

export const StepperHeader = ({ headerName, blockCount, activeStep }) => {
	const renderStepperHeaderBlock = (idx) => {
		return (
			<div
				key={idx}
				className={`wizard-process-header-block-item ${getSelectedClassName(
					idx,
					activeStep || 1
				)}`}
			></div>
		);
	};

	return (
		<div className="opus-wizard-process-header">
			<div className="wizard-process-header-text">{headerName}</div>
			<div className="wizard-process-header-block">
				{Array.from({ length: blockCount || 0 }).map((_, idx) =>
					renderStepperHeaderBlock(idx)
				)}
			</div>
		</div>
	);
};

StepperHeader.propTypes = {
	headerName: PropTypes.string,
	blockCount: PropTypes.number,
	activeStep: PropTypes.number,
};
