angular.module('app')
.controller('RegisterExternalCtrl', ['$scope', 'security', function ($scope, Security) {
	if (!Security.externalUser) Security.authenticate();
	Security.redirectAuthenticated('/');
	$scope.registerExternalUser = function () {
		if (!$scope.registerForm.$valid) return;
		$scope.message = "Processing Login...";
		Security.registerExternal().catch(function (data) {
			$scope.message = null;
		});
	}
	$scope.schema = [
		{ property: 'userName', label: 'Username', type: 'text', attr: { ngMinlength: 4, required: true } }
	];
}]);