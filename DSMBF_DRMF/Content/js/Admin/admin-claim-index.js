$(document).ready(function () {
    $(".pagination").pagination({
        items: $('.pagination').data('totalitems'),
        itemsOnPage: 10,
        currentPage: $('.pagination').data('pageindex'),
        hrefTextPrefix: "",
        prevText: "&laquo;",
        nextText: "&raquo;",
        cssStyle: "",
        onPageClick: function (pageNumber, event) {
            event.preventDefault();
            bindPageItems({
                PageNumber: pageNumber,
                AccountNo: $("#AccountCode").val() || '',
                Company: $("#Company").val() || '',
                Company_SC: $("#Company_SC").val() || '',
                Category: $("#Category").val() || '',
                Invoice: $("#Invoice").val() || '',
                InvoiceType: $("#InvoiceType").val() || '',
                InvoiceDate: $("#InvoiceDate").val() || '',
                Quarter: $("#Quarter").val() || '',
                SortBy: $("#distributorlist").data("sortby") || '',
                SortDirection: $("#distributorlist").data("sortdirection") || ''
            });
            $('.pagination').attr('data-pageindex', pageNumber);
        }
    });

    jQuery(function () {
        $("#InvoiceDate").datepicker({
            showOn: "button",
            buttonText: 'Show Calendar',
            buttonImage: apppath + "/Content/images/calendar.png",
            buttonImageOnly: true,
            dateFormat: 'mm-dd-yy'
            //maxDate:0,
            //minDate: new Date()
        });

    });


    $("form").validate({
        errorPlacement: function (error, element) {
            $("span[data-valmsg-for='" + $(element).attr("name") + "']").html($(error));
        }
    });

    $("#Search").click(function () {
        var Acode = $("#AccountCode").val();
        var Category = $("#Category").val();
        var InvoiceDate = $("#InvoiceDate").val();
        var Company = $("#Company").val();
        var Company_SC = $("#Company_SC").val();
        var Invoice = $("#Invoice").val();
        var InvoiceType = $("#InvoiceType").val();
        var Quarter = $("#Quarter").val();
        if (
            (Acode || '' !== '') ||
            (Category || '' !== '') ||
            (InvoiceDate || '' !== '') ||
            (Company || '' !== '') ||
            (Company_SC || '' !== '') ||
            (Invoice || '' !== '') ||
            (InvoiceType || '' !== '') ||
            (Quarter || '' !== '')
        )
        {
            $("form").submit();
        }
        else {
            $("#errortext").html("please input data");

        }
    });


});
$(".sort-by").each(function () {

    $(this).click(function () {

        var sortBy = $("#distributorlist").data("sortby"),
            sortDirection = $("#distributorlist").data("sortdirection"),
            curSortBy = $(this).data("sortby");

        if (sortBy == curSortBy) {
            if (sortDirection == 'asc') {
                sortDirection = 'desc';
            }
            else {
                sortDirection = 'asc';
            }
        }
        else {
            sortDirection = 'asc';
        }

        $("#distributorlist").data("sortdirection", sortDirection);
        $("#distributorlist").data("sortby", curSortBy);


        $('.pagination').attr('data-pageindex', 1);

        $('.pagination').pagination('drawPage', 1);

        bindPageItems({
            PageNumber: 1,
            AccountNo: $("#AccountCode").val() || '',
            Company: $("#Company").val() || '',
            Company_SC: $("#Company_SC").val() || '',
            Category: $("#Category").val() || '',
            Invoice: $("#Invoice").val() || '',
            InvoiceType: $("#InvoiceType").val() || '',
            InvoiceDate: $("#InvoiceDate").val() || '',
            Quarter: $("#Quarter").val() || '',
            SortBy: $("#distributorlist").data("sortby") || '',
            SortDirection: $("#distributorlist").data("sortdirection") || ''
        });
    });
});


function bindPageItems(e) {
    $.ajax({
        url: apppath + "/Admin/Claim/PartialIndex/" + e.PageNumber,
        dataType: 'html',
        data: {
            AccountNo: e.AccountNo,
            Company: e.Company,
            Company_SC: e.Company_SC,
            Category: e.Category,
            Invoice: e.Invoice,
            InvoiceType: e.InvoiceType,
            InvoiceDate: e.InvoiceDate,
            Quarter: e.Quarter,
            SortBy: e.SortBy,
            SortDirection: e.SortDirection
        },
        success: function (response) {
            if (response === undefined && response == '') {
                response = '<tr><td colspan="6" class="center">No Results Found</td></tr>';
            }
            $("#distributorlist tbody").html(response);
            var pageIndex = parseInt($('.pagination').attr('data-pageindex'));
            console.log(pageIndex);
            var startItem = (pageIndex == 1 ? 0 : (pageIndex - 1) * 10) + 1;
            var lastItem = startItem + ($("#distributorlist tbody tr").length - 1);
            $(".pageof").html("Displaying items " + startItem + " - " + lastItem + " of " + $('.pagination').data('totalitems'));

        },
        failure: function (response) {
            console.log(response);
            $("#distributorlist tbody").html('<tr><td colspan="6" class="center error">Error occured</td></tr>');
        }
    });
}

