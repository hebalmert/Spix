window.getTimeZone = () => {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
};