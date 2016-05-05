app.factory('hubManager', [
    '$rootScope', 'notifications',
    function ($rootScope, notifications) {
        function dispatch(action) {
            if (!$rootScope.$$phase) {
                $rootScope.$apply(action);
            } else if (action) {
                action();
            }
        }

        var connection = $.hubConnection('signalR');
        var currentView;
        var previousView;

        var proxy = connection.createHubProxy('data')
            .on('OnFirmSummaryChanged', function (changes) {
                if (currentView && !currentView.portfolioId) {
                    dispatch(function() {
                        currentView.onChanges(changes);
                    });
                }
            })
            .on('OnPortfolioChanged', function (portfolioId, changes) {
                if (currentView && currentView.portfolioId == portfolioId) {
                    dispatch(function() {
                        currentView.onChanges(changes);
                    });
                }
            });

        function connect() {
            if (connection.state === $.signalR.connectionState.connected) {
                return;
            }
            connection.start().fail(function() {
                notifications.error('Will attempt to reconnect in a minute.', 'Failed to connect to PoMo!');
                setTimeout(connect, 60000);
            });
        }

        connection.stateChanged(function(change) {
            if (currentView) {
                currentView.setIsLoading();
            }
            switch (change.newState) {
            case $.signalR.connectionState.connecting:
                notifications.info('Attempting to connect.', 'Connecting to PoMo...');
                break;
            case $.signalR.connectionState.reconnecting:
                notifications.warning('Attempting to reconnect.', 'Connection to PoMo dropped...');
                break;
            case $.signalR.connectionState.connected:
                notifications.success(null, 'Connection to PoMo established!');
                setTimeout(function () {
                    if (previousView) {
                        previousView.unsubscribe();
                        previousView = null;
                    }
                    if (currentView) {
                        currentView.subscribe();
                    }
                }, 150);
                break;
            case $.signalR.connectionState.disconnected:
                
                notifications.error('Will attempt to reconnect in a minute.', 'Connection to PoMo lost...');
                setTimeout(connect, 60000);
                break;
            }
            dispatch(function() {
                $rootScope.$broadcast('stateChanged', change.newState);
            });
        });

        connect();

        function setCurrentView(portfolioId) {
            previousView = currentView;

            var isLoading = true;
            var data;
            var keyColumnName;
            var totalPnl = 0;
            var columnNames;
            var pnlOrdinal = -1;
            var rowMap = null;

            function binarySearch(key) {
                var startIndex = 0;
                var endIndex = data.length - 1;
                while (endIndex >= startIndex) {
                    var midPoint = startIndex + Math.floor((endIndex - startIndex) / 2);
                    var itemKey = data[midPoint][keyColumnName];
                    switch (itemKey.localeCompare(key)) {
                        case 0:
                            return midPoint;
                        case -1:
                            startIndex = midPoint + 1;
                            break;
                        case 1:
                            endIndex = midPoint - 1;
                            break;
                    }

                }
                return ~startIndex;
            }

            function createDatum(rowData) {
                var datum = {};
                _.forEach(columnNames, function (columnName, ordinal) {
                    var value = rowData[ordinal];
                    if (ordinal == pnlOrdinal) {
                        totalPnl += value;
                    }
                    datum[columnName] = value;
                });
                return rowMap[datum[keyColumnName]] = datum;
            }

            currentView = {
                portfolioId: portfolioId,
                subscribe: function() {
                    isLoading = true;
                    if (connection.state !== $.signalR.connectionState.connected) {
                        return;
                    }
                    proxy.invoke.apply(proxy, portfolioId ? ['SubscribeToPortfolio', portfolioId] : ['SubscribeToFirmSummary'])
                        .done(function(dataTable) {
                            columnNames = _.map(dataTable.cols, function (item, index) {
                                if (item.name == 'Pnl') {
                                    pnlOrdinal = index;
                                }
                                return item.name;
                            });
                            totalPnl = 0;
                            keyColumnName = columnNames[dataTable.key[0]];
                            rowMap = {};
                            data = _.map(dataTable.rows, createDatum);
                            dispatch(function () {
                                isLoading = false;
                            });
                        });
                },
                unsubscribe: function () {
                    data = null;
                    rowMap = null;
                    if (connection.state !== $.signalR.connectionState.connected) {
                        return;
                    }
                    proxy.invoke.apply(proxy, portfolioId ? ['UnsubscribeFromPortfolio', portfolioId] : ['UnsubscribeFromFirmSummary']);
                },
                onChanges: function (changes) {
                    if (!rowMap || !data) {
                        return;
                    }
                    _.forEach(changes, function (change) {
                        var datum;
                        switch (change.type) {
                            case 'added':
                                var index = binarySearch(change.rowKey);
                                if (index < 0) {
                                    data.splice(~index, 0, createDatum(change.data));
                                }
                                break;
                            case 'columnsChanged':
                                if (!change.rowKey in rowMap) {
                                    return;
                                }
                                datum = rowMap[change.rowKey];
                                _.forEach(change.changes, function (columnChange) {
                                    if (columnChange.columnOrdinal != pnlOrdinal) {
                                        var columnName = columnNames[columnChange.columnOrdinal];
                                        datum[columnName] = columnChange.value;
                                    } else {
                                        var currentRowPnl = datum.Pnl;
                                        var newPnl = columnChange.value;
                                        totalPnl += newPnl - currentRowPnl;
                                        datum.Pnl = newPnl;
                                    }
                                });
                                break;
                            case 'removed':
                                if (!change.rowKey in rowMap) {
                                    return;
                                }
                                var removeIndex = binarySearch(change.rowKey);
                                if (removeIndex >= 0) {
                                    datum = data[removeIndex];
                                    totalPnl -= datum.Pnl;
                                    delete rowMap[change.rowKey];
                                    data.splice(removeIndex, 1);
                                }
                                break;
                        }
                    });
                },
                setIsLoading: function () {
                    isLoading = true;
                },
                state: {
                    isLoading: function() {
                        return isLoading;
                    },
                    data: function() {
                        return data;
                    },
                    totalPnl: function () {
                        return totalPnl;
                    }
                }
            };
        };

        $rootScope.$on('$locationChangeStart', function () {
            if (currentView) {
                currentView.unsubscribe();
                currentView = null;
            }
        });

        return {
            state: function () {
                return connection.state;
            },
            firmSummaryView: function () {
                setCurrentView(null);
                if (connection.state === $.signalR.connectionState.connected) {
                    currentView.subscribe();
                }
                return currentView.state;
            },
            portfolioView: function (portfolioId) {
                setCurrentView(portfolioId);
                if (connection.state === $.signalR.connectionState.connected) {
                    currentView.subscribe();
                }
                return currentView.state;
            }
        };
    }
]);