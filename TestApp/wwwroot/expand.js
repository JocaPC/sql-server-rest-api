//QUnit.cases.combinatorial([
//    { a: 1, b: 1, expectedSum: 2 }
//])
//    .test("Sum test", function (params, assert) {
//        var actualSum = params.a + params.b;
//        assert.equal(actualSum, params.expectedSum);
//    });


QUnit.cases
    .combinatorial([
        { orderby: "OrderDate " },
        { orderby: "(-OrderID mod 4) " },
        { orderby: "month(OrderDate) " }
    ])
    .combinatorial([
        { dir: "asc" },
        { dir: "desc" }
    ])
    .combinatorial([
        { filter1: "OrderID lt 5000" },
        { filter1: "(OrderID gt 300)" },
        { filter1: "month(OrderDate) gt 2" }
    ])
    .combinatorial([
        { filter2: " or month(OrderDate) lt 10" },
        { filter2: " or (month(OrderDate) gt 4)" },
        { filter2: " and OrderID gt 0" }
    ])
    .combinatorial([
        { param: "$skip=5,$top=10" },
        { param: "$select=OrderID,OrderDate,$skip=10,$top=4" },
        { param: "$top=2" }
    ])
    .test("$expand test", function (params, assert) {
        var finishTest = assert.async();
        var data = null;
        $.ajax("/odata?$top=2&$expand=Invoices($top=3),Orders($orderBy=" + params.orderby + params.dir +
            ",$filter=" + params.filter1 + params.filter2 +
            "," + params.param + ")", { dataType: "json" })
            .done(result => {
                assert.ok(result.value !== null, "Response is retrieved");
                for (i = 0; i < result.value.length; i++) {
                    assert.notEqual(result.value[i].Orders[0].OrderID, null, "OrderID should not be null");
                    assert.notEqual(result.value[i].Orders[0].OrderDate, null, "OrderDate should not be null");
                }
                finishTest();
            });
    });