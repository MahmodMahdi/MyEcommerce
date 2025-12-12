var dtable;
$(function(){
    loaddata();
});

function loaddata(){
    dtable = $("#mytable").DataTable({
        "ajax":{
            "url":"/Admin/Order/GetData"
        },
        "columns":[
        {"data":"id"},
        {"data":"name"},
        { "data": "phoneNumber" },
        { "data": "applicationUser.email" },
            { "data": "orderStatus" },
            {"data":"totalPrice"},
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a href="/Admin/Order/Details?OrderId=${data}" class="btn btn-success">Details</a>
                  
                    `
                }
            }
        ]
    });
}
