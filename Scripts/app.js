$(document).ready(function () {
    var order = 'asc';

    $('time').timeago();

    $(document).pjax('[data-pjax]', '.content-body', { timeout: 2000 });

    $(document).on('submit', 'form[data-pjax]', function (event) {
        $.pjax.submit(event, '.content-body')
    })

    $(document).on('pjax:send', function () {
        $('#loader').fadeIn();
    })
    $(document).on('pjax:complete', function () {
        $('#loader').delay(500).fadeOut();
        $('time').timeago();
        Prism.highlightAll();

        order = 'asc';
    })

    $(document).on('click', '[data-action="toggle-entity-modal"]', function (e) {
        e.preventDefault();

        var entity = $(this).data('entity'),
            target = $(this).data('target');

        $('#'+target).modal();
        $('#' + target + ' [data-action]').attr('data-entity', entity);

        if (target == 'rename-entity-modal') {
            $('#entity-name').focus();
        }
    });

    $(document).on('click', '[data-action="delete-entity"]', function () {
        var entity = $(this).data('entity');

        $.ajax({
            type: 'POST',
            url: '/entity/delete/'+entity,
            dataType: 'json',
            success: function (data) {
                $('#delete-entity-modal').modal('toggle');
                $('#entity-' + entity).remove();
            },
            error: function (ex) {
                console.log('Error');
            }
        });
    });

    $(document).on('focus', '#entity-name', function () {
        if ($(this).hasClass('is-invalid')) {
            $(this).removeClass('is-invalid');
            $('.invalid-feedback').remove();
        }
    });

    $(document).on('click', '[data-action="rename-entity"]', function () {
        var entity = $(this).data('entity'),
            name = $('#entity-name').val();

        name = name.substr(0, 1).toUpperCase() + name.substr(1);

        if (name.length < 1) {
            $('#entity-name').addClass('is-invalid');
            $('#rename-entity-modal .modal-body').append('<div class="invalid-feedback">Name cannot be blank.</div>');
            return;
        }

        $.ajax({
            type: 'POST',
            url: '/entity/rename/' + entity,
            dataType: 'json',
            data: {name: name},
            success: function (data) {
                $('#rename-entity-modal').modal('toggle');
                $('#entity-' + entity + ' .card-title a').html(name);
                $('#entity-' + entity + ' time').html('just now');
            },
            error: function (ex) {
                console.log('Error');
            }
        });
    });

    $(document).on('input propertychange', '#search-entities', function () {
        var input = $(this).val();

        $.ajax({
            type: 'POST',
            url: '/entity/search',
            data: { input: input },
            success: function (data) {
                $('#entity-group').html(data);
                $('time').timeago();
            },
            error: function (ex) {
                console.log('Error');
            }
        });
    });

    $(document).on('input propertychange', '#search-items', function () {
        var input = $(this).val(),
            entity = $(this).data('entity');

        if ($('.dropdown-item').hasClass('active')) {
            $('.dropdown-item').removeClass('active');
            $('.sort-icon').remove();
        }

        $.ajax({
            type: 'POST',
            url: '/entitydata/search/' + entity,
            data: { input: input },
            success: function (data) {
                $('#item-group tbody').html(data);
            },
            error: function (ex) {
                console.log('Error');
            }
        });
    });

    $(document).on('click', '[data-action="toggle-item-modal"]', function (e) {
        e.preventDefault();

        var entity = $(this).data('entity'),
            item = $(this).data('item');

        $('#delete-item-modal').modal();
        $('#delete-item-modal [data-action]').attr({
            'data-entity': entity,
            'data-item': item
        });
    });

    $(document).on('click', '[data-action="delete-item"]', function () {
        var entity = $(this).data('entity'),
            item = $(this).data('item');

        $.ajax({
            type: 'POST',
            url: '/entitydata/delete/' + entity + '/' + item,
            dataType: 'json',
            success: function (data) {
                $('#delete-item-modal').modal('toggle');
                $('#item-' + item).remove();
            },
            error: function (ex) {
                console.log('Error');
            }
        });
    });

    $(document).on('click', '[data-action="sort-items"]', function (e) {
        e.preventDefault();

        var entity = $(this).data('entity'),
            criteria = $(this).data('criteria');

        $.ajax({
            type: 'POST',
            url: '/entitydata/sort/' + entity,
            data: {
                criteria: criteria,
                order: order
            },
            success: function (data) {
                if ($('[data-action="sort-items"]').hasClass('active')) {
                    $('[data-action="sort-items"]').removeClass('active');
                }

                $('[data-criteria="' + criteria + '"').addClass('active');
                $('#item-group tbody').html(data);

                if (order == 'asc') {
                    order = 'desc';
                    $('.sort-icon').remove();
                    $('[data-criteria="' + criteria + '"]').append(
                        '<div class="sort-icon pull-right">' +
                        '<span class="fa fa-fw fa-sort-amount-desc" aria-hidden="true"></span>' +
                        '<span class="visually-hidden">Desc</span>' +
                        '</div>'
                        );
                } else {
                    order = 'asc';
                    $('.sort-icon').remove();
                    $('[data-criteria="' + criteria + '"').append(
                        '<div class="sort-icon pull-right">' +
                        '<span class="fa fa-fw fa-sort-amount-asc" aria-hidden="true"></span>' +
                        '<span class="visually-hidden">Asc</span>' +
                        '</div>'
                        );
                }           
            },
            error: function (ex) {
                console.log('Error');
            }
        });
    });
});