'use strict';
angular.module('app')
.factory('ErrorHandler', ['$rootScope', 'Alert', function ($rootScope, Alert) {
	var ErrorHandler = this;

	ErrorHandler.handle = function (data, status, headers, config) {
		var message = [];
		if (data.message) {
			message.push("<strong>" + data.message + "</strong>");
		}
		if (data.modelState) {
			angular.forEach(data.modelState, function (errors, key) {
				message.push(errors);
			});
		}
		if (data.exceptionMessage) {
			message.push(data.exceptionMessage);
		}
		if (data.error_description) {
			message.push(data.error_description);
		}
		Alert.new('danger', message.join('<br/>'));
	}

	return ErrorHandler;
}])
.factory('myHttpInterceptor', ['ErrorHandler', '$q', function (ErrorHandler, $q) {
	return {
		response: function (response) {
			return response;
		},
		responseError: function (response) {
			ErrorHandler.handle(response.data, response.status, response.headers, response.config);

			// do something on error
			return $q.reject(response);
		}
	};
}]);