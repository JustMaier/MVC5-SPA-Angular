angular.module('app')
.controller('LoginCtrl', ['$scope', 'security', function ($scope, Security) {
	Security.redirectAuthenticated('/');
	var LoginModel = function () {
		return {
			username: '',
			password: '',
			rememberMe: false
		}
	};

	$scope.user = new LoginModel();
	$scope.login = function () {
		if (!$scope.loginForm.$valid) return;
		$scope.message = "Processing Login...";
		Security.login(angular.copy($scope.user)).catch(function (data) {
			$scope.message = null;
		});
	}
	$scope.schema = [
		{ property: 'username', type: 'text', attr: { ngMinlength: 4, required: true } },
		{ property: 'password', type: 'password', attr: { required: true } },
		{ property: 'rememberMe', label: 'Keep me logged in', type: 'checkbox' }
	];
}]);