window.startIdleTracking = function (dotnetHelper) {
    const events = ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'];
    events.forEach(evt => {
        document.addEventListener(evt, () => {
            dotnetHelper.invokeMethodAsync('ResetIdleTimer');
        });
    });
};