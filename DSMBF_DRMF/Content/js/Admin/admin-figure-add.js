$(document).ready(function () {

    if ($('#EncryptedId').val() !== "") {
        $('#ProgramType_Id').attr("disabled", true);
    }

    $("#removeall").hide()
    var data1 = null;

    $("#AccountCode").on("blur", function () { CheckProgramType($(this).val()); });
    $("#AccountCode").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: apppath + "/Admin/Claim/AccountCode",
                type: "POST",
                dataType: "json",
                data: { Prefix: request.term },
                success: function (data) {
                    //console.log(data);
                    response($.map(data, function (item) {
                        return {
                            label: item.AccountCode, value: item.AccountCode//,track: item.Id
                        };
                    }));
                }
            })
        },
        select: function (event, ui) {
            CheckProgramType(ui.item.value);
        },
        messages: {
            noResults: "",
            results: function (count) {
                return count + (count == 0 ? ' result' : ' results');
            }
        }


    });


    $(".numonly").numeric({ allowMinus: false, allowDecSep: true, maxDecimalPlaces: 4 });

    $("form").validate({
        errorPlacement: function (error, element) {
            $("span[data-valmsg-for='" + $(element).attr("name") + "']").html($(error));
        }
    });

    $("#submitBtn").click(function () {
        if ($("form").valid()) {
            $('#ProgramType_Id').attr("disabled", false);
            $("form").submit();
        }
    });
});


function CheckProgramType(track) {

    $.ajax({
        url: apppath + "/Admin/Claim/GetProgramTypeList",
        type: "GET",
        dataType: "json",
        data: { AccountCode: track },
        success: function (data) {
            $("select[name='ProgramType.Id']").html('').append($("<option>").text("Please Select").attr({ value: "" }));
            $.each(data, function (i, obj) {
                $("select[name='ProgramType.Id']").append($("<option>").text(obj.Name).attr({ value: obj.Id }));
            });
        }
    });
}

