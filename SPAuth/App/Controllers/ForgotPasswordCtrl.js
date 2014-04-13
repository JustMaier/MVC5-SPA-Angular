angular.module('app')
.controller('ForgotPasswordCtrl', ['$scope', 'security', '$modal', 'Alert', function ($scope, Security, $modal, Alert) {
	Security.redirectAuthenticated('/');
	var User = function () {
		return {
			email: '',
		}
	}

	$scope.user = new User();
	$scope.requestReset = function () {
		if (!$scope.forgotPasswordForm.$valid) return;
		$scope.message = "Processing Request...";
		Security.forgotPassword(angular.copy($scope.user)).then(function (data) {
			//Success
			Alert.new('success', data.message);
			Security.authenticate();
		}, function (data) {
			$scope.message = null;
		})
	};
	$scope.schema = [
		{ label: 'Email Address', property: 'email', type: 'email', attr: { required: true } }
	];
}]);