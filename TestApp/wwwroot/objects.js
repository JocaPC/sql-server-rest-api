//QUnit.cases.combinatorial([
//    { a: 1, b: 1, expectedSum: 2 }
//])
//    .test("Sum test", function (params, assert) {
//        var actualSum = params.a + params.b;
//        assert.equal(actualSum, params.expectedSum);
//    });


QUnit.cases
    .combinatorial([
        { orderby: "object_id " },
        { orderby: "(-object_id mod 4) " },
        { orderby: "((object_id sub -3) mod (4.1 add object_id)) " },
        { orderby: "tolower(name) " }
    ])
    .combinatorial([
        { dir: "asc" },
        { dir: "desc" }
    ])
    .combinatorial([
        { filter1: "object_id mod 20 eq 7" },
        { filter1: "(type eq 'IT' and type is not null)" }
    ])
    .combinatorial([
        { filter2: " and length(name) gt 10" },
        { filter2: " or (hour(create_date) lt 10.5)" },
        { filter2: "" }
    ])
    .combinatorial([
        { param: "$skip=5&$top=10" },
        { param: "$select=object_id,name,type&$skip=10" },
        { param: "" }
    ])
    .test("query test", function(params, assert) {
        var finishTest = assert.async();
        var data = null;
        $.ajax("/objects?$orderby=" + params.orderby + params.dir +
            "&$filter=" + params.filter1 + params.filter2 +
            "&" + params.param, { dataType: "json" })
        .done(result => {
            assert.ok(result.value !== null, "Response is retrieved");
            for (i = 0; i < result.value.length; i++) {
                assert.notEqual(result.value[i].object_id, null, "object_id should not be null");
                assert.notEqual(result.value[i].name, null, "name should not be null");
                if (params.filter1 === "(type eq 'IT' and $type is not null)"
                    && !params.filter2.trim().startsWith("or")) {
                    assert.equal(result.value[i].type, 'IT', "type should be 'IT'");
                }
                if (params.filter1 === "object_id mod 20 eq 7"
                    && !params.filter2.trim().startsWith("or")) {
                    assert.equal(result.value[i].object_id % 20, 7, "object_id mod 20 should be 7. object_id: " + result.value[i].object_id);
                }
            }
        })
        .fail(result => assert.notOk(true, result))
        .always(() => finishTest());
    });