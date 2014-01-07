angular.module('security', [])
.constant('security.urls', {
	site: '/',
	join: '/api/account/register',
	login: '/token',
	logout: '/api/account/logout',
	userInfo: '/api/account/userInfo',
	changePassword: '/api/account/changePassword',
	externalLogins: '/api/account/externalLogins',
	registerExternal: '/api/account/registerExternal'
})
.factory('security.api', ['$http', 'security.urls', function ($http, Urls) {
	//Parameterize - Necessary for funky login expectations...
	var formdataHeader = { 'Content-Type': 'application/x-www-form-urlencoded' };
	var parameterize = function (data) {
		var param = function (obj) {
			var query = '';
			var subValue, fullSubName, innerObj, i;
			angular.forEach(obj, function (value, name) {
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
					angular.forEach(value, function (subValue, subName) {
						fullSubName = name + '[' + subName + ']';
						innerObj = {};
						innerObj[fullSubName] = subValue;
						query += param(innerObj) + '&';
					});
				}
				else if (value !== undefined && value !== null) {
					query += encodeURIComponent(name) + '=' + encodeURIComponent(value) + '&';
				}
			});

			return query.length ? query.substr(0, query.length - 1) : query;
		};
		return angular.isObject(data) && String(data) !== '[object File]' ? param(data) : data;
	};

	var Api = {
		getUserInfo: function (accessToken) {
			return $http({ url: Urls.userInfo, method: 'GET', headers: { 'Authorization': 'Bearer ' + accessToken } });
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
		},
		getExternalLogins: function () {
			return $http({ method: 'GET', url: Urls.externalLogins + '?returnUrl=' + encodeURIComponent(Urls.site)+'&generateState=true', isArray: true });
		},
		registerExternal: function (accessToken, data) {
			return $http({ method: 'POST', url: Urls.registerExternal, data: data, headers: { 'Authorization': 'Bearer ' + accessToken } });
		}
	};

	return Api;
}])
.provider('security', ['security.urls', function (Urls) {
	var securityProvider = this;
	//Options
	securityProvider.registerThenLogin = true;
	securityProvider.usePopups = false;
	securityProvider.urls = {
		login: '/login',
		registerExternal: '/registerExternal',
		postLogout: '/login',
		home: '/'
	};
	securityProvider.apiUrls = Urls;
	securityProvider.events = {
		login: null,
		logout: null,
		register: null,
		reloadUser: null,
		closeOAuthWindow: null
	};

	securityProvider.$get = ['security.api', '$q', '$http', '$location', '$timeout', function (Api, $q, $http, $location, $timeout) {
		//Private Variables
		var externalLoginWindowTimer = null;

		//Private Methods
		var parseQueryString = function (queryString) {
			var data = {},
				pairs, pair, separatorIndex, escapedKey, escapedValue, key, value;

			if (queryString === null) {
				return data;
			}

			pairs = queryString.split("&");

			for (var i = 0; i < pairs.length; i++) {
				pair = pairs[i];
				separatorIndex = pair.indexOf("=");

				if (separatorIndex === -1) {
					escapedKey = pair;
					escapedValue = null;
				} else {
					escapedKey = pair.substr(0, separatorIndex);
					escapedValue = pair.substr(separatorIndex + 1);
				}

				key = decodeURIComponent(escapedKey);
				value = decodeURIComponent(escapedValue);

				data[key] = value;
			}

			return data;
		};
		var accessToken = function (accessToken, persist) {
			if (accessToken) {
				if (accessToken == 'clear') {
					localStorage.removeItem('accessToken');
					sessionStorage.removeItem('accessToken');
					delete $http.defaults.headers.common.Authorization;
				} else {
					if (persist) localStorage.accessToken = accessToken;
					else sessionStorage.accessToken = accessToken;
					$http.defaults.headers.common.Authorization = 'Bearer ' + accessToken;
				}
			}
			return sessionStorage.accessToken || localStorage.accessToken;
		};
		var redirectTarget = function (newTarget) {
			if (newTarget == 'clear') {
				delete localStorage.redirectTarget;
				return;
			}
			if (newTarget) localStorage.redirectTarget = newTarget;
			return localStorage.redirectTarget;
		};
		var handleExternalData = function (external_data, provider) {
			var deferred = $q.defer();

			//Return if there was an error
			if (external_data.error) {
				deferred.reject({ message: external_data.error });
			} else {
				//Get user info and login or show external register screen
				Api.getUserInfo(external_data.access_token).success(function (user) {
					if (user.hasRegistered) {
						accessToken(external_data.access_token);
						Security.user = user;
						Security.redirectAuthenticated(redirectTarget() || securityProvider.urls.home);
						if (securityProvider.events.login) securityProvider.events.login(Security, user); // Your Login events
						deferred.resolve(Security.user);
					} else {
						Security.externalUser = user;
						Security.externalUser.access_token = external_data.access_token;
						Security.externalUser.provider = provider;
						$location.path(securityProvider.urls.registerExternal);
						deferred.reject();
					}
				});
			}

			return deferred.promise;
		}
		var initialize = function () {
			//Check for external access token from 3rd party auth
			if ($location.path().indexOf('access_token') != -1) {
				var external_data = parseQueryString($location.path().substring(1));
				$location.path('/');
				if (window.opener) {
					window.opener.external_data = external_data;
					window.close();
				} else {
					var login = JSON.parse(localStorage.loginProvider);
					delete localStorage.loginProvider;
					handleExternalData(external_data, login);
				}
			}

			//Check for access token and get user info
			if (accessToken()) {
				accessToken(accessToken());
				Api.getUserInfo(accessToken()).success(function (user) {
					Security.user = user;
					if (securityProvider.events.reloadUser) securityProvider.events.reloadUser(Security, user); // Your Register events
				});
			}

			//Fetch list of external logins
			Api.getExternalLogins().success(function (logins) {
				Security.externalLogins = logins;
			});
		};

		//Public Variables
		var Security = this;
		Security.user = null;
		Security.externalUser = null;
		Security.externalLogins = [];

		//Public Methods
		Security.login = function (data) {
			var deferred = $q.defer();

			data.grant_type = 'password';
			Api.login(data).success(function (user) {
				accessToken(user.access_token, data.rememberMe);
				Security.user = user;
				Security.redirectAuthenticated(redirectTarget() || securityProvider.urls.home);
				if (securityProvider.events.login) securityProvider.events.login(Security, user); // Your Login events
				deferred.resolve(Security.user);
			}).error(function (errorData) {
				deferred.reject(errorData);
			});

			return deferred.promise;
		};

		Security.loginWithExternal = function (login) {
			var deferred = $q.defer();
			if (securityProvider.usePopups) {
				var loginWindow = window.open(login.url, 'frame', 'resizeable,height=510,width=380');

				//Watch for close
				$timeout.cancel(externalLoginWindowTimer);
				externalLoginWindowTimer = $timeout(function closeWatcher() {
					if (!loginWindow.closed) {
						externalLoginWindowTimer = $timeout(closeWatcher, 500);
						return;
					}
					//closeOAuthWindow handler - passes external_data if there is any
					if (securityProvider.events.closeOAuthWindow) securityProvider.events.closeOAuthWindow(Security, window.external_data);

					//Return if the window was closed and external data wasn't added
					if (typeof (window.external_data) === 'undefined') {
						deferred.reject();
						return;
					}

					//Move external_data from global to local
					var external_data = window.external_data;
					delete window.external_data;

					deferred.resolve(handleExternalData(external_data, login));
				}, 500);
			} else {
				localStorage.loginProvider = JSON.stringify(login);
				window.location.href = login.url;
			}

			return deferred.promise;
		};

		Security.logout = function () {
			var deferred = $q.defer();

			Api.logout().success(function () {
				Security.user = null;
				accessToken('clear');
				redirectTarget('clear');
				if (securityProvider.events.login) securityProvider.events.logout(Security); // Your Logout events
				$location.path(securityProvider.urls.postLogout);
				deferred.resolve();
			}).error(function (errorData) {
				deferred.reject(errorData);
			});

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
			});

			return deferred.promise;
		};

		Security.registerExternal = function () {
			var deferred = $q.defer();

			if (!Security.externalUser) {
				deferred.reject();
			} else {
				Api.registerExternal(Security.externalUser.access_token, Security.externalUser).success(function () {
					//Success
					deferred.resolve(Security.loginWithExternal(Security.externalUser.provider));
					Security.externalUser = null;
				}).error(function (errorData) {
					deferred.reject(errorData);
				});
			}

			return deferred.promise;
		};

		Security.changePassword = function (data) {
			var deferred = $q.defer();

			Api.changePassword(data).success(function () {
				deferred.resolve();
			}).error(function (errorData) {
				deferred.reject(errorData);
			});

			return deferred.promise;
		};

		Security.authenticate = function () {
			if (accessToken()) return;
			if(!redirectTarget())redirectTarget($location.path());
			$location.path(securityProvider.urls.login);
		};

		Security.redirectAuthenticated = function (url) {
			if (!accessToken()) return;
			if(redirectTarget())redirectTarget('clear');
			$location.path(url);
		};

		// Initialize
		initialize();

		return Security;
	}];
}]);