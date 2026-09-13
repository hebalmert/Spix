//Rastreo de actividad para IdleLogout.
//Se avisa a .NET como maximo una vez cada 5 segundos: antes cada movimiento del mouse era
//una llamada a .NET y una escritura en LocalStorage (cientos por segundo).
(function () {
    const events = ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'];
    const throttleMs = 5000;
    let helper = null;
    let lastSent = 0;

    function onActivity() {
        const now = Date.now();
        if (!helper || now - lastSent < throttleMs) return;

        lastSent = now;
        helper.invokeMethodAsync('ResetIdleTimer').catch(() => { });
    }

    //Idempotente: si el layout se vuelve a montar no se duplican los listeners
    window.startIdleTracking = function (dotnetHelper) {
        helper = dotnetHelper;
        lastSent = 0;

        events.forEach(evt => {
            document.removeEventListener(evt, onActivity);
            document.addEventListener(evt, onActivity, { passive: true });
        });
    };

    window.stopIdleTracking = function () {
        helper = null;
        events.forEach(evt => document.removeEventListener(evt, onActivity));
    };
})();
