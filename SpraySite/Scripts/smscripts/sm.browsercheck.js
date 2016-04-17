$(document).ready(function () {

    Modernizr.addTest('xhr2', 'FormData' in window);
    if (!Modernizr.xhr2) {
        $("#badBrowserWarning").removeClass("hide");
    }

});