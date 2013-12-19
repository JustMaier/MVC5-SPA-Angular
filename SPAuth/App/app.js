angular.module('app', ['security', 'ngSanitize', 'ngRoute', 'ui.bootstrap', 'autoFields'])
.config(['$routeProvider', '$httpProvider', '$locationProvider', '$parseProvider', 'securityProvider', function ($routeProvider, $httpProvider, $locationProvider, $parseProvider, securityProvider) {
	$locationProvider.html5Mode(true);
	$httpProvider.defaults.headers.common["X-Requested-With"] = "XMLHttpRequest";
	$httpProvider.interceptors.push('myHttpInterceptor');
	securityProvider.urls.login = '/login';

	$routeProvider
	.when('/:controller?/:action?/:id?', {
		template: '<ng-include src="include"></ng-include>',
		controller: 'DynamicCtrl'
	})
	.otherwise({
		redirectTo: '/'
	});
}])
.run(['$rootScope', 'security', '$route', function ($rootScope, security, $route) {
	$rootScope.app = {
		params: null,
		loading: true
	};
	$rootScope.security = security;

	$rootScope.$on('$locationChangeStart', function (event) {
		$rootScope.app.loading = true;
	});
	$rootScope.$on('$locationChangeSuccess', function (event) {
		$rootScope.app.params = angular.copy($route.current.params);
		$rootScope.app.loading = false;
	});
}])
.controller('DynamicCtrl', ['$rootScope', '$scope', '$routeParams', function ($rootScope, $scope, $routeParams) {
	$rootScope.app.title = $routeParams.action;
	var route = [];
	if ($routeParams.controller != null && $routeParams.controller != '') route.push($routeParams.controller);
	if ($routeParams.action != null && $routeParams.action != '') route.push($routeParams.action);
	$scope.include = ('/' + route.join('/')).toLowerCase();
}]);