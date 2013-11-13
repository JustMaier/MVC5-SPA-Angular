'use strict';

angular.module('app')
.factory('Alert', ['$rootScope', '$timeout', function ($rootScope, $timeout) {
	var Alert = this;
	var alerts = $rootScope.alerts = [];

	Alert.close = function (alert, index) {
		if (alert.timer != null) $timeout.cancel(alert.timer);
		alerts.splice(index, 1);
	}

	Alert.new = function (type, message, time) {
		var alert = { type: type, message: message, close: Alert.close };
		if (time != null) {
			alert.timer = $timeout(function () {
				alerts.splice(alerts.indexOf(alert), 1);
			}, time);
		}
		alerts.push(alert);
	}

	return Alert;
}]);