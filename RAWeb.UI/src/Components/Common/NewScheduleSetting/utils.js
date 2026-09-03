export const getField = (data, field, backendField) =>
	data[field] || data[backendField];

export const getOptionValue = (options, backendValue, fallback) =>
	options.find(
		(option) => option.backendValue === String(backendValue),
	)?.value || fallback;

export const getBackendValue = (options, value, fallback) =>
	options.find((option) => option.value === value)?.backendValue || fallback;
