$(document).ready(function () {

    $(document).on("click", "#addmore", attachmore);

    if ($('#EncryptedId').val() != "") {
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


    //$(document).on('focusout', '#InvoiceAmount', function (e) {
    //    var invoiceamt = $(this).val();
    //    var invoicetax = $("#TaxPercentage").val();
    //    var taxAmount = 0;
    //    var taxWithAmount = 0;
    //    if ($.trim(invoicetax) !== '' && invoicetax !== null && $.trim(invoiceamt) !== '') {
    //        taxAmount = (invoiceamt * invoicetax) / 100
    //        taxWithAmount = parseFloat(invoiceamt) + parseFloat(taxAmount);
    //        $("#TaxAmount").val(taxAmount.toFixed(2));
    //        $("#InvoiceAmountwithtax").val(taxWithAmount.toFixed(2));
    //    }
    //    else {
    //        $("#TaxAmount").val('');
    //        $("#InvoiceAmountwithtax").val('');
    //    }
    //});


    //$(document).on('focusout', '#TaxPercentage', function (e) {
    //    var invoicetax = $(this).val();
    //    var invoiceamt = $("#InvoiceAmount").val();
    //    var taxAmount = 0;
    //    var taxWithAmount = 0;
    //    if ($.trim(invoiceamt) !== '' && invoiceamt !== null && $.trim(invoicetax) !== '') {
    //        taxAmount = (invoiceamt * invoicetax) / 100
    //        taxWithAmount = parseFloat(invoiceamt) + parseFloat(taxAmount);
    //        $("#TaxAmount").val(taxAmount.toFixed(2));
    //        $("#InvoiceAmountwithtax").val(taxWithAmount.toFixed(2));
    //    }
    //    else {
    //        $("#TaxAmount").val('');
    //        $("#InvoiceAmountwithtax").val('');
    //    }
    //});

    jQuery(function () {
        $("#InvoiceDate").datepicker({
            showOn: "button",
            buttonText: 'Show Calendar',
            buttonImage: apppath + "/Content/images/calendar.png",
            buttonImageOnly: true,
            dateFormat: 'mm-dd-yy',
            //maxDate:0,
            minDate: new Date("2019", "0", "1")
        });

    });
    $.validator.addMethod('filesize', function (value, element, param) {
        return this.optional(element) || (element.files[0].size <= param)
    }, 'File size must be less than 2MB');

    $("form").validate({
        errorPlacement: function (error, element) {
            $("span[data-valmsg-for='" + $(element).attr("name") + "']").html($(error));
        }
    });

    if ($(".ReAttach").length <= 0) {
        $("input[type='file']").each(function (i, e) {
            $(e).rules("add", {
                required: true,
                filesize: 2097152,/*2MB*/
                extension: 'jpg|jpeg|png|pdf'
            });
        });
    }

    //$("input[type='checkbox']").each(function (i, e) {
    //if ($("input[type='file'][data-type='" + $(e).attr('data-type') + "']")) {
    //addRule({
    //Ele: e,
    //Rule: { required: true }
    //});
    //}
    //});


    $(".ReAttach").prop("checked", true);

    $(".ReAttach").on("change", function () {
        if ($(this).is(':checked')) {
            $("input[type='file']").each(function (i, e) {
                removeRule({ Ele: e, Rule: "required" });
            });
        }
        else {
            $("input[type='file']").each(function (i, e) {
                $(e).rules("add", {
                    required: true,
                    filesize: 2097152,/*2MB*/
                    extension: 'jpg|jpeg|png|pdf'
                });
            });
        }
    });
    $("#submitBtn").click(function () {
        if ($("form").valid()) {
            $('#ProgramType_Id').attr("disabled", false);
            $("form").submit();
        }
    });
});

var upload_number = 2;
function attachmore() {
    if (upload_number < 6) {
        //add more file
        var moreUploadTag = '';
        moreUploadTag += '<td><input type="file" id="files' + upload_number + '" name="files"/>';
        moreUploadTag += '<a href="javascript:del(' + upload_number + ')" style="cursor:pointer;">Delete</a></td>';
        $('.requiredDocs tr.docs').prev().after($('<tr id="delete_file' + upload_number + '"><td></td>' + moreUploadTag + '</tr>').fadeIn('slow').appendTo('.requiredDocs'));
        upload_number++;
    }
    if (upload_number === 6) {
        $('#addmore').hide();
    }
}

function del(eleId) {
    var ele = document.getElementById("delete_file" + eleId);
    ele.parentNode.removeChild(ele);
    upload_number--;
}

function edit(eleId) {
    var ele = document.getElementById("Edit_" + eleId);
    ele.parentNode.removeChild(ele);
}

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
    })

    //$("#ProgramType_Id option[value=" + track + "]").attr('selected', 'selected');
}

function addRule(e) {
    console.log("#" + $(e.Ele).attr("id"));
    $("#" + $(e.Ele).attr("id")).rules("add", e.Rule);
}

function removeRule(e) {

    $("#" + $(e.Ele).attr("id")).rules("remove", e.Rule);
}