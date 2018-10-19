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
        { orderby: "((OrderID add 3) mod (4 sub OrderID)) " },
        { orderby: "month(OrderDate) " }
    ])
    .combinatorial([
        { dir: "asc" },
        { dir: "desc" }
    ])
    .combinatorial([
        { filter1: "OrderID lt 12000" },
        { filter1: "(OrderID gt 300)" },
        { filter1: "month(OrderDate) gt 2" }
    ])
    .combinatorial([
        { filter2: " or month(OrderDate) le 10" },
        { filter2: " and (month(OrderDate) lt 4)" },
        { filter2: " or (month(OrderDate) gt 4)" },
        { filter2: " and (((3 sub OrderID) mod 4) eq 1)" },
        { filter2: " and OrderID gt 0" }
    ])
    .combinatorial([
        { limit: "$skip=5,$top=10" },
        { limit: "$skip=10,$top=50" },
        { limit: "$top=2" }
    ])
    .combinatorial([
        { selecto: "" },
        { selecto: ",$select=OrderID,OrderDate" }
    ])
    .combinatorial([
        { selecti: "" },
        { selecti: ",$select=InvoiceID,InvoiceDate" }
    ])
    .test("$expand test", function (params, assert) {
        var finishTest = assert.async();
        var data = null;
        $.ajax("/odata?$top=2&$expand=Invoices(" + params.limit + params.selecti +"),Orders($orderBy=" + params.orderby + params.dir +
            ",$filter=" + params.filter1 + params.filter2 +
            "," + params.limit + params.selecto + ")", { dataType: "json" })
            .done(result => {
                assert.ok(result.value !== null, "Response is retrieved");
                for (i = 0; i < result.value.length; i++) {
                    var orders = result.value[i].Orders;
                    if (orders != null && params.param === "$top=2") {
                        assert.ok(orders.length <= 2, "Incorrect number of orders");
                    }
                    if (orders != null && params.param === "$select=OrderID,OrderDate,$skip=10,$top=4") {
                        assert.ok(orders.length <= 4, "Incorrect number of orders");
                    }
                    for (var keyo in orders) {
                        var o = orders[keyo];
                        assert.notEqual(o.OrderID, null, "OrderID should not be null");
                        assert.notEqual(o.OrderDate, null, "OrderDate should not be null");
                        if (params.filter2 === " and (((3 sub OrderID) mod 4) eq 1)") {
                            assert.equal((3 - OrderID) % 4, 1, "OrderID " + OrderID + " should be ((3-OrderID) mod 4) eq 1)");
                        }
                        if (params.filter2 === " and (month(OrderDate) lt 4)") {
                            assert.equal(true, new Date(o.OrderDate).getMonth() < 4, "Month " + new Date(o.OrderDate).getMonth() + " should be less than 4");
                        }
                    }
                    var invoices = result.value[i].Invoices;
                    for (var keyi in invoices) {
                        var i = invoices[keyi];
                        assert.notEqual(i.InvoiceID, null, "InvoiceID should not be null");
                    }
                }
                finishTest();
            });
    });