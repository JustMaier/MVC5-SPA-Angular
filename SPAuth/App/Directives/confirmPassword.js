angular.module('app')
.directive('confirmPassword', [function () {
	return {
		restrict: 'A',
		require: 'ngModel',
		link: function (scope, element, attrs, ngModel) {
			ngModel.$parsers.unshift(function (viewValue, $scope) {
				var password = scope.$eval(attrs.confirmPassword);
				var noMatch = viewValue != password;
				ngModel.$setValidity('noMatch', !noMatch);
				return viewValue;
			});
		}
	}
}]);