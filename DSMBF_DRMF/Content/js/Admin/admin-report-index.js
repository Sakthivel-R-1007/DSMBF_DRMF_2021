
$(document).ready(function () {
    $("form").validate({
        errorPlacement: function (error, element) {
            $("span[data-valmsg-for='" + $(element).attr("name") + "']").html($(error));
        },
        rules: {
            "ProgramType": {
                required: true
            },
            "Quarter": {
                required: function () {
                    return ($("#Reports").val() || '') === 'HTl9NQDvk6NCa_2f2G_2f4jjWQ_3d_3d' || (($("#Reports").val() || '') === 'mS_2fxqVFArIU6cLbn_2fC8OTw_3d_3d'&& ($("#InvoicePeriod").val() || '') === '');
                }
            },
            "InvoicePeriod": {
                required: function () {
                    return ($("#Reports").val() || '') === 'npn5_2f7SgySulWMHcKQguKg_3d_3d' || ($("#Reports").val() || '') === '_2fjxs_2b2Kncoqhw_2fWshi8Pow_3d_3d' || (($("#Reports").val() || '') === 'mS_2fxqVFArIU6cLbn_2fC8OTw_3d_3d' && ($("#Quarter").val() || '') === '');
                }
            },

        }
    });
    $("#Reports").on("change", function () {
        var rptVal = $(this).val() || '';
        $(".quarter_block").hide();
        $("#Quarter").val('');
        $(".invoice_block").hide();
        $("#InvoicePeriod").val('');
        loadInvoicePeriod(rptVal);
        if (rptVal === 'HTl9NQDvk6NCa_2f2G_2f4jjWQ_3d_3d' || rptVal === 'mS_2fxqVFArIU6cLbn_2fC8OTw_3d_3d') {
            $(".quarter_block").show();
        }
        if (rptVal === 'npn5_2f7SgySulWMHcKQguKg_3d_3d' || rptVal === '_2fjxs_2b2Kncoqhw_2fWshi8Pow_3d_3d' || rptVal === 'mS_2fxqVFArIU6cLbn_2fC8OTw_3d_3d') {
            $(".invoice_block").show();
        }
    });

    $("#InvoicePeriod").on("change", function () {
        $("#Quarter").val('');
        $("#Quarter").valid();
    });

    $("#Quarter").on("change", function () {
        $("#InvoicePeriod").val('');
        $("#InvoicePeriod").valid();
        loadInvoicePeriod(rptVal);
    });

    $("#Submit").click(function () {
        var Reports = $("#Reports").val();
        var ProgramType = $("#ProgramType").val();
        console.log(ProgramType);
        console.log(Reports);
        if ((Reports !== null && Reports !== '') || (ProgramType !== null && ProgramType !== '')) {
            $("form").submit();
        }
        else {
            $("#errortext").html("please Select data");
        }
    });
});

function loadInvoicePeriod(e) {
    $.ajax({
        url: apppath + "/Admin/Report/InvoicePeriod",
        type: "GET",
        dataType: "json",
        data: { EncDetail: e },
        success: function (data) {
            console.log(data);
            $("select[name='InvoicePeriod']").html('').append($("<option>").text("Please Select").attr({ value: "" }));
            $.each(data, function (i, obj) {
                $("select[name='InvoicePeriod']").append($("<option>").text(obj).attr({ value: obj }));
            });
        }
    })
}