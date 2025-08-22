// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function deleteTodo(id) {
    if (confirm("Bu görevi silmek istediğinize emin misiniz?")) {
        $.ajax({
            url: '/Home/Delete',
            type: 'POST',
            data: { id: id },
            success: function (res) {
                if (res.ok) {
                    window.location.reload();
                } else {
                    alert(res.error || "Silme işlemi başarısız.");
                }
            },
            error: function () {
                alert("Sunucu hatası oluştu.");
            }
        });
    }
}
function populateForm(i){
    $.ajax({
        url: 'Home/PopulateForm',
        type: 'GET',
        data: { id : id },
        success: function(response){
        $("#form-task").val(response.task);
        $("#form-button").val("Update Todo");
        $("#form-action").attr("action", "/Home/Edit");
    }
    });
};

   


