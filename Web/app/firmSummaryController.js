app.controller('firmSummaryController', [
    '$scope', 'hubManager', function ($scope, hubManager) {
        $scope.gridOptions = (function() {
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
                        displayName: 'Id',
                        field: 'PortfolioId',
                        cellClass: 'semiBold'
                    },
                    {
                        displayName: 'Name',
                        field: 'PortfolioName'
                    },
                    {
                        displayName: 'Pnl',
                        field: 'Pnl',
                        cellClass: 'semiBold',
                        cellTemplate: createFilterCellTemplate('stringFormat:\'#,##0;(#,##0)\'', true)
                    }
                ],
                multiSelect: false,
                data: 'state.data()',
                plugins: [new ngGridFlexibleHeightPlugin()]
            };
        })();

        $scope.state = hubManager.firmSummaryView();

        
    }
]);