app.directive('loadingIndicator', function () {
	return {
		restrict: 'A',
		scope: {
			loading: '=loadingIndicator'
		},
		transclude: false,
		link: function (scope, element) {
			scope.element = element[0];
		},
		controller: ['$scope', function ($scope) {
			var animation = null;

			$scope.element = null;
			$scope.$watch('loading', function(value) {
				if ($scope.element) {
					if (value) {
						if (!animation) {
							animation = new Spinner({ top: '400px' });
						}
						animation.spin();
						$scope.element.appendChild(animation.el);
					} else if (animation) {
						animation.stop();
					}
				}
			});
		}]
	};
});