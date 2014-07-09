$(document).ready(function () {
    $('.language').click(function (event) {
        event.preventDefault();
        //$.cookie('language', $(event.target).data('lang'));
        //var cookieValue = $.cookie('i18n.langtag');
        // setting new cookie
        $.cookie('i18n.langtag', $(event.target).data('lang'), { path: '/', expires: 365 });
        //var newCookieValue = $.cookie('i18n.langtag');
        window.location.reload();
    })
});