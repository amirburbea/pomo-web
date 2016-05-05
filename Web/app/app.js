var app = angular.module('pomo', ['ngRoute', 'ngGrid', 'ui.bootstrap', 'stringFormat']).config([
    '$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {
        $routeProvider.when('/', {
            templateUrl: 'app/firmSummary.html',
            controller: 'firmSummaryController',
            caseInsensitiveMatch: true,
            reloadOnSearch: false
        }).when('/portfolio/:portfolioId', {
            templateUrl: 'app/portfolio.html',
            controller: 'portfolioController',
            caseInsensitiveMatch: true,
            reloadOnSearch: false,
            resolve: {
                portfolioId: [
                    '$route', function ($route) {
                        return $route.current.params.portfolioId;
                    }
                ]
            }
        });
        $locationProvider.html5Mode(true);
    }
]);