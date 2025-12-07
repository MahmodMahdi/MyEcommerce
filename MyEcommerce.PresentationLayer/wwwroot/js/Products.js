var dtable;
$(document).ready(function(){
    loaddata()
});

function loaddata(){
    dtable = $("#mytable").DataTable({
        "ajax":{
            "url":"/Admin/Product/GetData"
        },
        "columns":[
        {"data":"name"},
        {"data":"description"},
        { "data": "price" },
        { "data": "createdAt" },
            { "data": "category.name" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a href=/Admin/Product/Edit/${data} class="btn btn-success">Edit</a>
                      <a href=/Admin/Product/Delete/${data} class="btn btn-danger">Delete</a>
                    `
                }
            }
        ]
    });
}
