$(() => {
    var $table =
        $("#people")
            .DataTable({
                "ajax": "/Table",
                "serverSide": true,
                "columns": [
                    { data: "FullName" },
                    { data: "EmailAddress", defaultContent: "" },
                    { data: "PhoneNumber", defaultContent: "" },
                    { data: "FaxNumber", defaultContent: "" }                    
                ]
            });
});