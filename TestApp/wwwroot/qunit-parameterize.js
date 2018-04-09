/*
 * Parameterize v 0.4
 * A QUnit Addon For Running Parameterized Tests
 * https://github.com/AStepaniuk/qunit-parameterize
 * Released under the MIT license.
 */
QUnit.extend(QUnit, {
    cases: (function () {
        'use strict';
        var currentCases = null,
            clone = function (testCase) {
                var result = {},
                    p = null;

                for (p in testCase) {
                    if (testCase.hasOwnProperty(p)) {
                        result[p] = testCase[p];
                    }
                }

                return result;
            },

            createTest = function (methodName, title, callback, parameters) {

                QUnit[methodName](title, function (assert) {
                    return callback.call(this, parameters, assert);
                });
            },

            iterateTestCases = function (methodName, title, callback) {
                var i = 0,
                    parameters = null,
                    testCaseTitle = null;

                if (!currentCases || currentCases.length === 0) {
                    // setup test which will always fail
                    QUnit.test(title, function (assert) {
                        assert.ok(false, "No test cases are provided");
                    });
                    return;
                }

                for (i = 0; i < currentCases.length; i += 1) {
                    parameters = currentCases[i];

                    testCaseTitle = title;
                    if (parameters.title) {
                        testCaseTitle += "[" + parameters.title + "]";
                    }

                    if (parameters._skip === true) {
                        methodName = 'skip';
                    }

                    createTest(methodName, testCaseTitle, callback, parameters);
                }
            },

            getLength = function (arr) {
                return arr ? arr.length : 0;
            },

            getItem = function (arr, idx) {
                return arr ? arr[idx] : undefined;
            },

            mix = function (testCase, mixData) {
                var result = null,
                    p = null;

                if (testCase && mixData) {
                    result = clone(testCase);

                    for (p in mixData) {
                        if (mixData.hasOwnProperty(p)) {
                            if (p !== "title") {
                                if (!(result.hasOwnProperty(p))) {
                                    result[p] = mixData[p];
                                }
                            } else {
                                result[p] = [result[p], mixData[p]].join("");
                            }
                        }
                    }

                } else if (testCase) {
                    result = testCase;
                } else if (mixData) {
                    result = mixData;
                } else {
                    // return null or undefined whatever testCase is
                    result = testCase;
                }

                return result;
            };

        return {

            init: function (testCasesList) {
                currentCases = testCasesList;
                return this;
            },

            sequential: function (addData) {
                var casesLength = getLength(currentCases),
                    addDataLength = getLength(addData),
                    length = casesLength > addDataLength ? casesLength : addDataLength,
                    newCases = [],
                    i = 0,
                    currentCaseI = null,
                    dataI = null,
                    newCase = null;

                for (i = 0; i < length; i += 1) {
                    currentCaseI = getItem(currentCases, i);
                    dataI = getItem(addData, i);
                    newCase = mix(currentCaseI, dataI);

                    if (newCase) {
                        newCases.push(newCase);
                    }
                }

                currentCases = newCases;

                return this;
            },

            combinatorial: function (mixData) {
                var current = (currentCases && currentCases.length > 0) ? currentCases : [null],
                    currentLength = current.length,
                    mixDataLength = 0,
                    newCases = [],
                    i = 0,
                    j = 0,
                    currentCaseI = null,
                    dataJ = null,
                    newCase = null;

                mixData = (mixData && mixData.length > 0) ? mixData : [null];
                mixDataLength = mixData.length;

                for (i = 0; i < currentLength; i += 1) {
                    for (j = 0; j < mixDataLength; j += 1) {
                        currentCaseI = current[i];
                        dataJ = mixData[j];
                        newCase = mix(currentCaseI, dataJ);

                        if (newCase) {
                            newCases.push(newCase);
                        }
                    }
                }

                currentCases = newCases;

                return this;
            },

            test: function (title, callback) {
                iterateTestCases("test", title, callback);
                return this;
            },

            getCurrentTestCases: function () {
                return currentCases;
            }
        };
    }())
});