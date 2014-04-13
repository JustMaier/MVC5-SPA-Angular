angular.module('app')
.controller('ResetPasswordCtrl', ['$scope', 'security', '$modal', 'Alert', function ($scope, Security, $modal, Alert) {
	Security.redirectAuthenticated('/');
	var User = function () {
		return {
			email: '',
			password: '',
			confirmPassword: '',
			code: decodeURIComponent($scope.app.params.code)
		}
	}

	$scope.user = new User();
	$scope.reset = function () {
		if (!$scope.resetForm.$valid) return;
		$scope.message = "Processing Request...";
		$scope.user.code = $scope.app.params.code;
		Security.resetPassword(angular.copy($scope.user)).then(function (data) {
			//Success
			Alert.new('success', data.message);
			Security.authenticate();
		}, function (data) {
			$scope.message = null;
		})
	};
	$scope.schema = [
		{ label: 'Email Address', property: 'email', type: 'email', attr: { required: true } },
		{ property: 'password', type: 'password', attr: { required: true } },
		{ property: 'confirmPassword', label: 'Confirm Password', type: 'password', attr: { confirmPassword: 'user.password', required: true } }
	];
}]);