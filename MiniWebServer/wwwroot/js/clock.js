function clock(elementId) {
    setInterval(function () {
        document.getElementById(elementId).innerHTML = new Date().toString();
    }, 1000);
}