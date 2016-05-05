app.directive('pmTitleBar', function () {
    return {
        restrict: 'E',
        templateUrl: 'app/titlebar.html',
        controller: [
            '$scope', '$http', '$location', 'hubManager', function ($scope, $http, $location, hubManager) {
                var detach;
                function getPortfolios() {
                    $http.get('api/portfolios').success(function(data) {
                        $scope.state.portfolios = data;
                        sessionStorage.setItem('portfolios', JSON.stringify(data));
                        if (detach) {
                            detach();
                        }
                    });
                }

                var sessionPortfolios = sessionStorage.getItem('portfolios');
                $scope.state = {
                    isFirmSummary: $location.path() === '/',
                    portfolioId: (function() {
                        var path = $location.path().toString();
                        return path.length > 11 && path.substring(0, 11) == '/portfolio/' ? path.substring(11) : null;
                    })(),
                    portfolios: sessionPortfolios ? JSON.parse(sessionPortfolios) : []
                };

                $scope.navigateToPortfolio = function (portfolioId) {
                    if ($scope.state.portfolioId !== portfolioId) {
                        $scope.state.isFirmSummary = false;
                        $scope.state.portfolioId = portfolioId;
                        $location.path('/portfolio/' + portfolioId);
                    }
                };

                $scope.navigateToFirmSummary = function () {
                    if (!$scope.state.isFirmSummary) {
                        $scope.state.isFirmSummary = true;
                        $scope.state.portfolioId = null;
                        $location.path('/');
                    }
                };

                if (sessionPortfolios) {
                    return;
                }
                if (hubManager.state == $.signalR.connectionState.connected) {
                    getPortfolios();
                } else {
                    detach = $scope.$on('stateChanged', function(event, value) {
                        if (value === $.signalR.connectionState.connected) {
                            getPortfolios();
                        }
                    });
                }
            }
        ]
    };
});