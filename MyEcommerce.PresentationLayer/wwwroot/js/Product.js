var dtble;
$(document).ready(function () {
    loaddata();
});

function loaddata() {
    dtable = $("#table").DataTable({
        "ajax": {
            "url": "/Admin/Product/GetData"
        },
        "columns": [
            { "data": "name" },
            { "data": "description" },
            { "data": "price" },
            {
                "data": "stockQuantity",
                "render": function (data) {
                    // لو المخزن أقل من 5 يظهر باللون الأحمر وعريض، وإلا يظهر عادي
                    if (data < 5) {
                        return `<span class="text-danger fw-bold">${data}</span>`;
                    }
                    return data;
                },
                "width": "15%"
            },
            { "data": "category.name" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a href="/Admin/Product/Edit/${data}" class="btn btn-success">Edit</a>
                    <a onClick=DeleteItem("/Admin/Product/Delete/${data}") class="btn btn-danger">Delete</a>
                    `
                }
            }
        ]
    });
}

// from site SweetAlerts
function DeleteItem(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "Delete",
                success: function (data) {
                    if (data.success) {
                        dtable.ajax.reload();
                        toaster.success(data.message);
                    }
                    else {
                        toaster.error(data.message);
                    }
                }
            });
            Swal.fire({
                title: "Deleted!",
                text: "Your file has been deleted.",
                icon: "success"
            });
        }
    });
}
