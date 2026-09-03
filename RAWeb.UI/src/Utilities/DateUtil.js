export function formatLocalDay(date, time) {
  const day = new Date(date);
  const pad = (n) => n.toString().padStart(2, '0');

  return `${day.getFullYear()}-${pad(day.getMonth() + 1)}-${pad(day.getDate())} ${time}`;
}

export function isMoreThanCustomDaysOld(ticks, customDays) {
    const date = new Date((ticks - 621355968000000000) / 10000); // Convert to JS Date
    const now = new Date();
    const diffMs = now - date;
    const diffDays = diffMs / (1000 * 60 * 60 * 24);
    return diffDays > customDays;
};