var AjaxActions = {

    prepareBootstrapValidation: function (container) {
        // any validation summary items should be encapsulated by a class alert and alert-danger
        $('.validation-summary-errors').each(function () {
            $(this).addClass('alert');
            $(this).addClass('alert-danger');
        });

        $('span.field-validation-valid, span.field-validation-error').each(function () {
            $(this).addClass('help-block');
            $(this).addClass('has-error');
        });

        //// update validation fields on submission of form
        //$('form').submit(function () {
        //    if ($(this).valid()) {
        //        $(this).find('div.control-group').each(function () {
        //            if ($(this).find('span.field-validation-error').length == 0) {
        //                $(this).removeClass('has-error');
        //                $(this).addClass('has-success');
        //            }
        //        });
        //    }
        //    else {
        //        $(this).find('div.control-group').each(function () {
        //            if ($(this).find('span.field-validation-error').length > 0) {
        //                $(this).removeClass('has-success');
        //                $(this).addClass('has-error');
        //            }
        //        });
        //        $('.validation-summary-errors').each(function () {
        //            if ($(this).hasClass('alert-danger') == false) {
        //                $(this).addClass('alert');
        //                $(this).addClass('alert-danger');
        //            }
        //        });
        //    }
        //});

        //// check each form-group for errors on ready
        //$('form').each(function () {
        //    $(this).find('div.form-group').each(function () {
        //        if ($(this).find('span.field-validation-error').length > 0) {
        //            $(this).addClass('has-error');
        //        }
        //    });
        //});
    },


    prepareAjaxForms: function (container, container_params) {
        $.validator.unobtrusive.parse(container);
        this.prepareBootstrapValidation(container);

        container.find('button[type=submit]').click(function (event) {
            event.preventDefault();
            var form = $(this).closest("form");
            var tempElement = $("<input type='hidden'/>");
            // clone the important parts of the button used to submit the form.
            tempElement
                .attr("name", this.name)
                .val($(this).val())
                .appendTo(form);
            // submiting
            form.submit();
            // remove tempElement
            tempElement.remove();
        });

        container.find('form').submit(function (event) {
            event.preventDefault();
            if ($(this).valid()) {
                $.ajax({
                    url: this.action,
                    type: this.method,  // post
                    data: $(this).serialize(),
                    success: function (result) {
                        if (result.redirectTo) {
                            // The operation was a success on the server as it returned
                            // a JSON objet with an url property pointing to the location
                            // you would like to redirect to => now use the window.location.href
                            // property to redirect the client to this location
                            if (result.redirectTo == 'reload')
                                location.reload()
                            else
                                window.location.href = result.redirectTo;
                        } else {
                            // The server returned a partial view => let's refresh
                            // the corresponding section of our DOM with it
                            container.html(result);
                            AjaxActions.prepareClickActions(container, container_params);
                            AjaxActions.prepareTooltips(container);
                            AjaxActions.prepareAjaxForms(container, container_params);
                        }
                        //alert('Success!');
                    },
                    error: function () {
                        alert('prepareAjaxForms - Ajax Get - Error!');
                    }
                });
            };
        });
    },


    prepareTooltips: function (container) {
        container.find('.has-tooltip').each(function () {
            $(this).tooltip({
                html: true
            });
        });
    },


    prepareClickActions: function (container, container_params) {
        container.find('.action-redirect').click(function (event) {
            if (event.target.tagName.toLowerCase() == 'a') return;    // so that inner 'a href' would fire event but not outer div
            event.preventDefault();
            event.stopPropagation();    // so that inner div would fire the event but not outer

            var action = $(this).data('action'); // the url to the controller
            var id = $(this).data('id'); // the id that's given to each button in the list
            var params = $(this).data('params'); // params

            //alert('redirrecing to: "' + action + '/' + id + '"');
            //window.location.href = action + '/' + id;
            AjaxActions.redirectToAction(action, id, params);
        });

        container.find('.action-ajax-container').click(function (event) {
            if (event.target.tagName.toLowerCase() == 'a') return;    // so that inner 'a href' would fire event but not outer div
            event.preventDefault();
            event.stopPropagation();    // so that inner div would fire the event but not outer

            var action = $(this).data('action'); // the url to the controller
            var id = $(this).data('id'); // the id that's given to each button in the list
            var params = $(this).data('params'); // params
            var container_selector = $(this).data('container'); // container

            AjaxActions.fireAjaxAction(action, id, params, container_selector);
        });

        container.find('.action-ajax-modal').click(function (event) {
            if (event.target.tagName.toLowerCase() == 'a') return;    // so that inner 'a href' would fire event but not outer div
            event.preventDefault();
            event.stopPropagation();    // so that inner div would fire the event but not outer

            var action = $(this).data('action'); // the url to the controller
            var id = $(this).data('id'); // the id that's given to each button in the list
            var params = $(this).data('params'); // params
            var container_selector = '#modal-ajax-container'; // modal container

            AjaxActions.fireAjaxAction(action, id, params, container_selector);
        });

        container.find('.action-ajax-popover').click(function (event) {
            if (event.target.tagName.toLowerCase() == 'a') return;    // so that inner 'a href' would fire event but not outer div
            event.preventDefault();
            event.stopPropagation();    // so that inner div would fire the event but not outer

            var action = $(this).data('action'); // the url to the controller
            var id = $(this).data('id'); // the id that's given to each button in the list
            var params = $(this).data('params'); // params

            var container = $(this);    // will be container for itself

            // already has the popover?
            if (container.data("bs.popover")) {
                // hide it or show
                container.popover('toggle')
            }
            else {
                // initialize and show
                AjaxActions.fireAjaxAction(action, id, params, container, { popover: true });
            }
        });

        container.find('a[data-toggle="tab"]').click(function (e) {
            e.preventDefault()
            $(this).tab('show')
        });

        container.find('a[data-toggle="popover"]').click(function (e) {
            e.preventDefault()
            if ($(this).hasClass("action-ajax-popover"))
                return; // it is the source itself (the button that is defined as a popover), and thus it has it's own onClick

            // where is the popover source?
            var popover_source;
            if (container_params && container_params.popover_source)   // not null, not empty, not 0 and not undefined
                popover_source = container_params.popover_source;
            else
                alert("Error - Cannot find popover_source!");
            // already has the popover?
            if (popover_source.data("bs.popover")) {
                // hide it or show
                popover_source.popover('toggle')
            }
        });
    },

    redirectToAction: function (action, id, params) {
        var url;
        if (!id && id != 0)        // null, empty or undefined
            url = action;
        else
            url = action + '/' + id;

        if (!params)    // null, empty, 0 or undefined
            window.location.href = action + '/' + id;
        else
            window.location.href = action + '/' + id + '?' + $.param(params);
    },

    fireAjaxAction: function (action, id, params, container_selector, options) {
        var url;
        if (!id && id != 0)        // null, empty, 0 or undefined
            url = action;
        else
            url = action + '/' + id;

        $.get(url, params, function (data) {
            if (data.redirectTo) {
                // The operation was a success on the server as it returned
                // a JSON objet with an url property pointing to the location
                // you would like to redirect to => now use the window.location.href
                // property to redirect the client to this location
                if (data.redirectTo == 'reload')
                    location.reload()
                else
                    window.location.href = data.redirectTo;
            }

            var container;
            if (typeof container_selector == 'string')
                container = $(container_selector);  //jquery selector
            else
                container = container_selector; // already jquery object

            if (options && options.popover) {
                // initializing popover
                container.popover({
                    html: true,
                    trigger: 'manual',
                    content: data
                })
                .on('shown.bs.popover', function () {
                    var popover = container.data("bs.popover").tip();

                    var container_params = { popover_source: container };

                    AjaxActions.prepareClickActions(popover, container_params);
                    AjaxActions.prepareTooltips(popover);

                    if (popover.find('form').length)
                        AjaxActions.prepareAjaxForms(popover, container_params);    // to apply the same transformations to the form's contents when they will refresh
                })
                .popover('show');
            }
            else {
                container.html(data);

                AjaxActions.prepareClickActions(container);
                AjaxActions.prepareTooltips(container);

                if (container.find('form').length)
                    AjaxActions.prepareAjaxForms(container);

                container.filter('#modal-ajax-container').closest('.modal').modal('show');
            }
        });
    }
};



var page = function () {
    //Update the validator
    $.validator.setDefaults({
        highlight: function (element) {
            $(element).closest(".form-group").addClass("has-error");
            $(element).closest(".form-group").removeClass("has-success");
        },
        unhighlight: function (element) {
            $(element).closest(".form-group").removeClass("has-error");
            $(element).closest(".form-group").addClass("has-success");
        }
    });
}();


$(document).ready(function () {
    AjaxActions.prepareClickActions($('body'));
    AjaxActions.prepareTooltips($('body'));

    $('.ajax-container').each(function (e) {
        AjaxActions.prepareAjaxForms($(this));
    });
});
