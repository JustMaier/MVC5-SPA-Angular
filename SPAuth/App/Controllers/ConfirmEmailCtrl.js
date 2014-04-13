angular.module('app')
.controller('ConfirmEmailCtrl', ['$rootScope', 'security', 'Alert', function ($rootScope, Security, Alert) {
	Security.confirmEmail($rootScope.app.params).then(function (data) {
		Alert.new('success', data.message);
	});
	Security.authenticate('/');
}]);