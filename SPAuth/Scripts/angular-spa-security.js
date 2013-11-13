angular.module('security', [])
.constant('security.urls', {
	join: '/api/account/register',
	login: '/token',
	logout: '/api/account/logout',
	userInfo: '/api/account/userInfo',
	changePassword: '/api/account/changePassword'
})
.factory('security.api', ['$http','$q', 'security.urls', function ($http, $q, Urls) {
	var Api = this;

	//Parameterize - Necessary for funky login expectations...
	var formdataHeader = { 'Content-Type': 'application/x-www-form-urlencoded' };
	var parameterize = function (data) {
		var param = function (obj) {
			var query = '';
			var name, value, fullSubName, subName, subValue, innerObj, i;

			for (name in obj) {
				value = obj[name];

				if (value instanceof Array) {
					for (i = 0; i < value.length; ++i) {
						subValue = value[i];
						fullSubName = name + '[' + i + ']';
						innerObj = {};
						innerObj[fullSubName] = subValue;
						query += param(innerObj) + '&';
					}
				}
				else if (value instanceof Object) {
					for (subName in value) {
						subValue = value[subName];
						fullSubName = name + '[' + subName + ']';
						innerObj = {};
						innerObj[fullSubName] = subValue;
						query += param(innerObj) + '&';
					}
				}
				else if (value !== undefined && value !== null) {
					query += encodeURIComponent(name) + '=' + encodeURIComponent(value) + '&';
				}
			}

			return query.length ? query.substr(0, query.length - 1) : query;
		};
		return angular.isObject(data) && String(data) !== '[object File]' ? param(data) : data;
	}

	var Api = {
		getUserInfo: function (accessToken) {
			return $http({ url: Urls.userInfo, method: 'GET' });
		},
		login: function (data) {
			return $http({ method: 'POST', url: Urls.login, data: parameterize(data), headers: formdataHeader });
		},
		logout: function () {
			return $http({ method: 'POST', url: Urls.logout });
		},
		register: function (data) {
			return $http({ method: 'POST', url: Urls.join, data: data });
		},
		changePassword: function (data) {
			return $http({ method: 'POST', url: Urls.changePassword, data: data });
		}
	};

	return Api;
}])
.provider('security', ['security.urls', function (Urls) {
	var securityProvider = this;
	//Options
	securityProvider.registerThenLogin = true;
	securityProvider.urls = {
		login: '/login',
		postLogout: '/login',
		home: '/'
	};
	securityProvider.apiUrls = Urls;
	securityProvider.events = {
		login: null,
		logout: null,
		register: null,
		reloadUser: null
	};

	securityProvider.$get = ['security.api', '$q', '$http', '$location', function (Api, $q, $http, $location) {
		//Private Variables
		var redirectTarget = null;

		//Private Methods
		var accessToken = function (accessToken, persist) {
			if (accessToken != null) {
				if (accessToken == 'clear') {
					localStorage.removeItem("accessToken");
					sessionStorage.removeItem("accessToken");
					delete $http.defaults.headers.common['Authorization'];
				} else {
					if (persist) localStorage["accessToken"] = accessToken;
					else sessionStorage["accessToken"] = accessToken;
					$http.defaults.headers.common['Authorization'] = "Bearer " + accessToken;
				}
			}
			return sessionStorage["accessToken"] || localStorage["accessToken"];
		}

		//Public Variables
		var Security = this;
		Security.user = null;

		//Public Methods
		Security.login = function (data) {
			var deferred = $q.defer();

			data.grant_type = 'password';
			Api.login(data).success(function (user) {
				accessToken(user.access_token, data.rememberMe);
				Security.user = user;
				Security.redirectAuthenticated(redirectTarget || securityProvider.urls.home);
				if (securityProvider.events.login) securityProvider.events.login(Security, user); // Your Login events
				deferred.resolve(Security.user);
			}).error(function (errorData) {
				deferred.reject(errorData);
			});

			return deferred.promise;
		};

		Security.logout = function () {
			var deferred = $q.defer();

			Api.logout().success(function () {
				Security.user = null;
				accessToken('clear');
				redirectTarget = null;
				if (securityProvider.events.login) securityProvider.events.logout(Security); // Your Logout events
				$location.path(securityProvider.urls.postLogout);
				deferred.resolve();
			}).error(function (errorData) {
				deferred.reject(errorData);
			})

			return deferred.promise;
		};

		Security.register = function (data) {
			var deferred = $q.defer();

			Api.register(data).success(function () {
				if (securityProvider.events.register) securityProvider.events.register(Security); // Your Register events
				if (securityProvider.registerThenLogin) {
					Security.login(data).then(function (user) {
						deferred.resolve(user);
					}, function (errorData) {
						deferred.reject(errorData);
					});
				} else {
					deferred.resolve();
				}
			}).error(function (errorData) {
				deferred.reject(errorData);
			})

			return deferred.promise;
		}

		Security.changePassword = function (data) {
			var deferred = $q.defer();

			Api.changePassword(data).success(function () {
				deferred.resolve();
			}).error(function (errorData) {
				deferred.reject(errorData);
			})

			return deferred.promise;
		}

		Security.authenticate = function () {
			if (accessToken() != null) return;
			target = $location.path();
			$location.path(securityProvider.urls.login);
		}

		Security.redirectAuthenticated = function (url) {
			if (accessToken() == null) return;
			$location.path(url);
		}

		// Initialize
		if (accessToken() != null) {
			accessToken(accessToken());
			Api.getUserInfo(accessToken()).success(function (user) {
				Security.user = user;
				if (securityProvider.events.reloadUser) securityProvider.events.reloadUser(Security, user); // Your Register events
			})
		}

		return Security;
	}];
}]);