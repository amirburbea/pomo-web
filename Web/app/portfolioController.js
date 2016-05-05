app.controller('portfolioController', [
    '$scope', 'hubManager', 'portfolioId', function ($scope, hubManager, portfolioId) {
        $scope.portfolioId = portfolioId;

        $scope.gridOptions = (function () {
            function createFilterCellTemplate(filter, redNegatives) {
                var binding = '{{row.getProperty(col.field) | ' + filter + '}}';
                return '<div class="ngCellText" ng-class="col.colIndex()" style="text-align: right">' +
                    '<span ng-cell-text title="' + binding + '"' +
                    (!redNegatives ? '' : ' ng-style="{\'color\': row.getProperty(col.field) < 0 ? \'darkred\' : \'darkgreen\'}"') +
                    '>' + binding + '</span></div>';
            }

            return {
                columnDefs: [
                    {
                        displayName: 'Ticker',
                        field: 'Ticker',
                        cellClass: 'semiBold'
                    },
                    {
                        displayName: 'Description',
                        field: 'Description'
                    },
                    {
                        displayName: 'Direction',
                        field: '',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><span ng-cell-text>{{row.entity.quantity < 0 ? "Short" : "Long"}}</span></div>'
                    },
                    {
                        displayName: 'Quantity',
                        field: 'Quantity',
                        cellTemplate: createFilterCellTemplate('stringFormat:\'#,##0;(#,##0)\'', false)
                    },
                    {
                        displayName: 'Px Last',
                        field: 'LastPrice',
                        cellTemplate: createFilterCellTemplate('stringFormat:\'#,##0.0000\'', false)
                    },
                    {
                        displayName: 'MTM',
                        field: 'MarkToMarket',
                        cellTemplate: createFilterCellTemplate('stringFormat:\'#,##0.00;(#,##0.00)\'', true)
                    },
                    {
                        displayName: 'Cost Basis',
                        field: 'CostBasis',
                        cellTemplate: createFilterCellTemplate('stringFormat:\'#,##0;(#,##0)\'', false)
                    },
                    {
                        displayName: 'Cash',
                        field: 'Cash',
                        cellTemplate: createFilterCellTemplate('stringFormat:\'#,##0;(#,##0)\'', false)
                    },
                    {
                        displayName: 'Pnl',
                        field: 'Pnl',
                        cellClass: 'semiBold',
                        cellTemplate: createFilterCellTemplate('stringFormat:\'#,##0;(#,##0)\'', true)
                    },
                    {
                        displayName: ' ',
                        field: '',
                        width: '5px'
                    }
                ],
                multiSelect: false,
                data: 'state.data()'
            };
        })();

        $scope.state = hubManager.portfolioView(portfolioId);

        
    }
]);