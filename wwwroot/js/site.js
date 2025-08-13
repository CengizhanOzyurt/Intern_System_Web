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
window.quickEdit = function (id, currentText) {
    const val = prompt("Görevi düzenle:", currentText);
    if (val === null) return;               
    const task = val.trim();
    if (!task) { alert("Görev boş olamaz."); return; }

    $.ajax({
        url: '/Home/Edit',
        type: 'POST',
        data: { id, task },
        success: function (res) {
            if (res.ok) location.reload();
            else alert(res.error || "Güncelleme başarısız.");
        },
        error: function () { alert("Sunucu hatası oluştu."); }
    });
};

   
























