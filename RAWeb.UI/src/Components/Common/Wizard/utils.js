export const getSelectedClassName = (itemIndex, activeStep) => {
	if (itemIndex === activeStep - 1) {
		return "selected";
	}

	if (itemIndex < activeStep - 1) {
		return "completed";
	}

	return "";
};

export const getSelectedCountClassName = (
	isComplete,
	isSelected
) => {
	if (isSelected) {
		return "selected";
	}
	
	if (isComplete) {
		return "completed";
	}

	return "";
};
