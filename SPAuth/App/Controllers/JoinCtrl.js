angular.module('app')
.controller('JoinCtrl', ['$scope', 'security', '$modal', function ($scope, Security, $modal) {
	Security.redirectAuthenticated('/');
	var User = function () {
		return {
			username: '',
			password: '',
			confirmPassword: ''
		}
	}

	$scope.user = new User();
	$scope.join = function () {
		if (!$scope.joinForm.$valid) return;
		$scope.message = "Processing Registration...";
		Security.register(angular.copy($scope.user)).then(function () {
			//Success
		}, function (data) {
			$scope.message = null;
		})
	};
	$scope.schema = [
		{ label: 'Email Address', property: 'username', type: 'email', attr: { required: true } },
		{ property: 'password', type: 'password', attr: { required: true } },
		{ property: 'confirmPassword', label: 'Confirm Password', type: 'password', attr: { confirmPassword: 'user.password', required: true } }
	];
}]);