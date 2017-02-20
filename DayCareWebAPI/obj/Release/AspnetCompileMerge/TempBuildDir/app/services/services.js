'use strict';
app.factory('accountService', ['$http', '$location', '$q', 'localStorageService', 'dayCareSettings',
    function ($http, $location, $q, localStorageService, dayCareSettings) {
        var serviceBase = dayCareSettings.apiBaseUri;
        var accountServiceFactory = {};

        var getServiceRequest = function (processId, currentUrl) {
            var deferred = $q.defer();
            var userId = localStorageService.get('userId');
            if (userId == '' || userId == null) {
                localStorageService.set('transferUrl', currentUrl);
                $location.path('/login');
            } else {
                $http.get(serviceBase + '/api/account/' + processId)
                .success(function (data) {
                    deferred.resolve(data);
                }).error(function (error) {
                    deferred.reject(error);
                });
            }
            return deferred.promise;
        };

        var getAllRequests = function () {
            var deferred = $q.defer();
            var userId = localStorageService.get('userId');
            if (userId == '' || userId == null) {
                $location.path('/login');
            } else {
                $http.get(serviceBase + '/api/account/GetAllRequests/' + userId)
                .success(function (data) {
                    deferred.resolve(data);
                }).error(function (error) {
                    deferred.reject(error);
                });
            }
            return deferred.promise;
        };

        var sendQuote = function (reply) {
            var deferred = $q.defer();
            $http.post(serviceBase + '/api/account/', reply)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
            return deferred.promise;
        };

        accountServiceFactory.GetServiceRequest = getServiceRequest;
        accountServiceFactory.GetAllRequests = getAllRequests;
        accountServiceFactory.SendQuote = sendQuote;
        return accountServiceFactory;
    }])
.factory('authInterceptorService', ['$q', '$injector', '$location', 'localStorageService', 'dayCareSettings', '$window',
    function ($q, $injector, $location, localStorageService, dayCareSettings, $window) {

        var authInterceptorServiceFactory = {};

        var _request = function (config) {

            config.headers = config.headers || {};

            var authData = localStorageService.get('authorizationData');
            if (authData) {
                config.headers.Authorization = 'Bearer ' + authData.token;
            }
            //custom header to handle code version
            config.headers.CodeVersion = dayCareSettings.apiBaseCodeVersion;
            return config;
        }

        var _responseError = function (rejection) {
            if (rejection.status === 401 || rejection.status === 417) {
                var authService = $injector.get('authService');
                var authData = localStorageService.get('authorizationData');
                authService.LogOut();
                var msg = '';
                if (rejection.status === 401)
                    msg = 'Sorry! Server was reset. Please login again.';
                else if (rejection.status === 417)
                    msg = 'Sorry for the inconvinience! We have updates rolled out. Please login again.';
                else
                    msg = 'Sorry! UnKnown Error';
                //$location.path('/login/' + msg);
                alert(msg);
                $location.path('/home');
                $window.location.reload(true);
            }
            return $q.reject(rejection);
        }

        authInterceptorServiceFactory.request = _request;
        authInterceptorServiceFactory.responseError = _responseError;

        return authInterceptorServiceFactory;
    }]).factory('authService', ['$http', '$q', 'localStorageService', 'dayCareSettings', function ($http, $q, localStorageService, dayCareSettings) {
        var serviceBase = dayCareSettings.apiBaseUri;
        var authServiceFactory = {};
        var authentication = {
            isAuth: false,
            userName: "",
            userToken: false,
            userId: "",
            userRole: ""
        };

        var login = function (user) {
            var deferred = $q.defer();
            var input = "grant_type=password&username=" + user.Email + ';' + user.password + "&password=" + user.password;
            //$http.post(serviceBase + '/api/login/LoginUser', user).success(function (data) {
            $http.post(serviceBase + '/token', input, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (data) {
                //$http.defaults.headers.common['Authorization'] = 'Bearer ' + data.userToken;
                localStorageService.set('authorizationData', { token: data.access_token, userName: data.userName });
                localStorageService.set('userId', data.id);
                localStorageService.set('userName', data.userName);
                localStorageService.set('userEmail', data.userEmail);
                localStorageService.set('userPhone', data.userPhone);
                localStorageService.set('userRole', data.userRole);
                localStorageService.set('isAuth', true);

                authentication.isAuth = true;
                authentication.userName = data.userName;
                authentication.userId = data.id;
                authentication.userRole = data.userRole;

                authServiceFactory.isAuth = true;
                authServiceFactory.userName = data.userName;

                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
            return deferred.promise;
        };

        var logOut = function () {
            localStorageService.remove('authorizationData');
            localStorageService.remove('userId');
            localStorageService.remove('userName');
            localStorageService.remove('userEmail');
            localStorageService.remove('userPhone');
            localStorageService.remove('userRole');
            localStorageService.remove('isAuth');
            localStorageService.remove('transferUrl');
            localStorageService.remove('settings');
            authentication.isAuth = false;
            authentication.userName = "";
            authentication.userId = "";
            authentication.userRole = "";
            authentication.userToken = false;
            $http.defaults.headers.common['Authorization'] = '';
        };

        var fillAuthData = function () {
            var authData = localStorageService.get('authorizationData');
            if (authData) {
                authentication.isAuth = true;
                authentication.userName = authData.userName;
                authentication.userId = authData.userId;
                authentication.userRole = authData.userRole;
            }
        };

        var insertToken = function (token) {
            var deferred = $q.defer();
            $http.post(serviceBase + "/api/alexa/LogToken", token)
                .success(function (data) {
                    deferred.resolve(data);
                }).error(function (error) {
                    deferred.reject(error);
                });
            return deferred.promise;
        };

        authServiceFactory.Login = login;
        authServiceFactory.InsertToken = insertToken;
        authServiceFactory.LogOut = logOut;
        authServiceFactory.Authentication = authentication;
        authServiceFactory.FillAuthData = fillAuthData;
        return authServiceFactory;
    }])
.factory('backgroundService', [function () {
    var currentBackgroundClass = 'home-bg';
    return {
        setCurrentBg: function (bgclass) {
            currentBackgroundClass = bgclass;
        },
        getCurrentBg: function () {
            return currentBackgroundClass;
        }
    };
}])
.factory('classesService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var classesServiceFactory = {};

    var _getClasses = function (dayCareId) {
        var deferred = $q.defer();
        $http.get(serviceBase + '/api/class/' + dayCareId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };
    var _addClass = function (item) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/class/', item)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _getKidsWithNoClass = function (dayCareId) {
        var deferred = $q.defer();
        $http.get(serviceBase + '/api/kid/GetKidsWithNoClass/' + dayCareId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _getKidsInAClass = function (classId) {
        var deferred = $q.defer();
        $http.get(serviceBase + '/api/kid/GetKidsInClass/' + classId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _assignKidsToClass = function (item) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/class/AssignClassToKid/', item)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _getKid = function (kidId, dayCareId) {
        var deferred = $q.defer();
        $http.get(serviceBase + "/api/kid/GetKidInDayCare/" + kidId + "/" + dayCareId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    classesServiceFactory.GetClasses = _getClasses;
    classesServiceFactory.GetKid = _getKid;
    classesServiceFactory.GetKidsWithNoClass = _getKidsWithNoClass;
    classesServiceFactory.GetKidsInAClass = _getKidsInAClass;
    classesServiceFactory.AddClass = _addClass;
    classesServiceFactory.AssignKidsToClass = _assignKidsToClass;
    return classesServiceFactory;
}])
.factory('customReportService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var customReportServiceFactory = {};

    customReportServiceFactory.questionTypes =
    [{ type: 'Heading', id: 1, trackId: 0, value: '', values: [''], template: '<button ng-click="removeQuestion()"><span class="fa fa-trash-o fa-1"></span></button>&nbsp;<input placeholder="type heading here" class="textwithoutBorder" ng-model="myDirectiveVar[0]"></input><br/><br/>' },
    { type: 'Question & Answer', id: 2, trackId: 0, value: '', values: [''], answers: [''], template: '<div class="form-inline"><button ng-click="removeQuestion()"><span class="fa fa-trash-o fa-1"></span></button>&nbsp;<input placeholder="type question here" class="input-small" ng-model="myDirectiveVar[0]"></input>&nbsp;<b>:</b>&nbsp;<input type="text" class="input-small" data-ng-disabled="true" placeholder="Answer"></input></div></div><br/>' },
    { type: 'Paired Question & Answer', id: 3, trackId: 0, value: '', values: ['', ''], answers: ['', ''], template: '<div class="form-inline"><button ng-click="removeQuestion()"><span class="fa fa-trash-o fa-1"></span></button>&nbsp;<input placeholder="question 1" class="input-small" ng-model="myDirectiveVar[0]"></input>&nbsp;<b>:</b>&nbsp;<input type="text" class="input-small" data-ng-disabled="true" placeholder="Answer"/>&nbsp;<input placeholder="question 2" class="input-small" ng-model="myDirectiveVar[1]"></input>&nbsp;<b>:</b>&nbsp;<input type="text" class="input-small" data-ng-disabled="true" placeholder="Answer"/></div><br/>' },
    { type: 'Question & Options', id: 4, trackId: 0, value: '', values: [''], options: [{ text: 'option', value: '' }, { text: 'option', value: '' }, { text: 'option', value: '' }], template: '<div class="form-inline"><button ng-click="removeQuestion()"><span class="fa fa-trash-o fa-1"></span></button>&nbsp;<input placeholder="question" class="input-small" ng-model="myDirectiveVar[0]"></input><span ng-repeat="option in customQuesOptions"><input placeholder="{{option.text}}" ng-model="option.value" class="input-small"></input>&nbsp;</span><button ng-click="addOption()"><span class="fa fa-plus-circle fa-1"></span></button><button ng-click="removeOption()"><span class="fa fa-minus-circle fa-1"></span></button></div><br/>' },
    { type: 'Question & Answer followed by Options', id: 5, trackId: 0, value: '', values: [''], options: [{ text: 'option', value: '' }, { text: 'option', value: '' }, { text: 'option', value: '' }], template: '<div class="form-inline"><button ng-click="removeQuestion()"><span class="fa fa-trash-o fa-1"></span></button>&nbsp;<input placeholder="question" class="input-small" ng-model="myDirectiveVar[0]"></input>&nbsp;<b>:</b>&nbsp;<input type="text" class="input-small" data-ng-disabled="true" placeholder="Answer"/>&nbsp;<span ng-repeat="option in customQuesOptions"><input placeholder="{{option.text}}" data-ng-disabled="option.disabled" ng-model="option.value" class="input-small"></input>&nbsp;</span><button ng-click="addOption()"><span class="fa fa-plus-circle fa-1"></span></button><button ng-click="removeOption()"><span class="fa fa-minus-circle fa-1"></span></button></div><br/>' },
    ];

    var _getCustomReport = function (kidId, dayCareId) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/customReport/GetCustomReportToday/' + kidId + "/" + dayCareId;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _getCustomReportOnAGivenDay = function (kidId, id, day) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/customReport/GetCustomReportOnADay/' + kidId + "/" + id + "/" + day;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _saveCustomReport = function (customReport) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/customReport/SaveCustomReport', customReport)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _createCustomReport = function (customReport) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/customReport/', customReport)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _uploadCustomReport = function (customReport) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/customReport/UploadCustomReport', customReport, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        }).success(function (data) {
            deferred.resolve(data);
        }).error(function (error) {
            deferred.reject(error);
        });
        return deferred.promise;
    };

    customReportServiceFactory.GetCustomReport = _getCustomReport;
    customReportServiceFactory.GetCustomReportOnAGivenDay = _getCustomReportOnAGivenDay;
    customReportServiceFactory.SaveCustomReport = _saveCustomReport;
    customReportServiceFactory.CreateCustomReport = _createCustomReport;
    customReportServiceFactory.UploadCustomReport = _uploadCustomReport;
    return customReportServiceFactory;
}])
.factory('dataService', function () {

    var dataServiceFactory = {};
    dataServiceFactory.days = [{ name: "Monday", checked: false },
        { name: "Tuesday", checked: false },
        { name: "Wednesday", checked: false },
        { name: "Thursday", checked: false },
        { name: "Friday", checked: false },
        { name: "Saturday", checked: false },
        { name: "Sunday", checked: false }];

    return dataServiceFactory;
})
.factory('dayCareService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var dayCareServiceFactory = {};

    var _getDayCareData = function (dayCareId) {
        var deferred = $q.defer();
        $http.get(serviceBase + '/api/dayCare/' + dayCareId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };
    var _addKid = function (kid) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/kid/', kid)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };
    var _editKid = function (kid) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/kid/EditKid', kid)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };
    var _deleteKid = function (kidId, dayCareId, reason) {
        var deferred = $q.defer();
        $http.put(serviceBase + "/api/kid/RemoveKid/" + kidId + "/" + dayCareId + "/" + reason)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };
    var _saveReport = function (kidLog) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/log/', kidLog)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };
    var _pullReport = function (kidId, dayCareId) {
        var deferred = $q.defer();
        $http.get(serviceBase + "/api/log/GetKidLogToday/" + kidId + "/" + dayCareId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _getAttendance = function (dayCareId) {
        var deferred = $q.defer();
        $http.get(serviceBase + "/api/kid/GetKidsAttendance/" + dayCareId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _logAttendance = function (kids) {
        var deferred = $q.defer();
        $http.post(serviceBase + "/api/kid/LogKidAttendance", kids)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _checkCustomReportExists = function (dayCareId) {
        var deferred = $q.defer();
        $http.get(serviceBase + "/api/settings/CheckCustomReportExists/" + dayCareId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _addShortKid = function (kid) {
        var deferred = $q.defer();
        $http.post(serviceBase + "/api/kid/InsertKidShort", kid)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _getAvatars = function (sex) {
        var avatars = [];
        if (sex == 'm' || sex == 'male' || sex == 'Male' || sex == 'M' || sex == 'boy' || sex == 'Boy') {
            for (var i = 0; i <= 22; i++)
                avatars.push('images/avatars/boy-' + i + '.png');
        }
        else if (sex == 'f' || sex == 'female' || sex == 'Female' || sex == 'F' || sex == 'girl' || sex == 'Girl') {
            for (var i = 0; i <= 26; i++)
                avatars.push('images/avatars/girl-' + i + '.png');
        }
        else {
            for (var i = 0; i <= 22; i++) {
                avatars.push('images/avatars/boy-' + i + '.png');
                avatars.push('images/avatars/girl-' + i + '.png');
            }
            for (var i = 22; i <= 26; i++) {
                avatars.push('images/avatars/girl-' + i + '.png');
            }
        }
        return avatars;
    };

    dayCareServiceFactory.GetDayCareData = _getDayCareData;
    dayCareServiceFactory.GetAttendance = _getAttendance;
    dayCareServiceFactory.LogAttendance = _logAttendance;
    dayCareServiceFactory.AddKid = _addKid;
    dayCareServiceFactory.AddShortKid = _addShortKid;
    dayCareServiceFactory.EditKid = _editKid;
    dayCareServiceFactory.DeleteKid = _deleteKid;
    dayCareServiceFactory.SaveReport = _saveReport;
    dayCareServiceFactory.PullReport = _pullReport;
    dayCareServiceFactory.CheckCustomReportExists = _checkCustomReportExists;
    dayCareServiceFactory.GetAvatars = _getAvatars;
    return dayCareServiceFactory;
}])
.factory('documentService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings, localStorageService) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var documentServiceFactory = {};

    var _getDocuments = function (id) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/document/' + id;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };

    var _getDocument = function (id, name) {
        var deferred = $q.defer();
        $http.get(serviceBase + "/api/document/DownloadDoc/" + id, { responseType: 'arraybuffer' })
            .success(function (data, status, headers) {
                var octetStreamMime = 'application/octet-stream';
                var success = false;

                // Get the headers
                headers = headers();

                // Get the filename from the x-filename header or default to "download.bin"
                var filename = name;

                // Determine the content type from the header or default to "application/octet-stream"
                var contentType = headers['content-type'] || octetStreamMime;

                try {
                    // Try using msSaveBlob if supported
                    console.log("Trying saveBlob method ...");
                    var blob = new Blob([data], { type: contentType });
                    if (navigator.msSaveBlob)
                        navigator.msSaveBlob(blob, filename);
                    else {
                        // Try using other saveBlob implementations, if available
                        var saveBlob = navigator.webkitSaveBlob || navigator.mozSaveBlob || navigator.saveBlob;
                        if (saveBlob === undefined) throw "Not supported";
                        saveBlob(blob, filename);
                    }
                    console.log("saveBlob succeeded");
                    success = true;
                } catch (ex) {
                    console.log("saveBlob method failed with the following exception:");
                    console.log(ex);
                }

                if (!success) {
                    // Get the blob url creator
                    var urlCreator = window.URL || window.webkitURL || window.mozURL || window.msURL;
                    if (urlCreator) {
                        // Try to use a download link
                        var link = document.createElement('a');
                        if ('download' in link) {
                            // Try to simulate a click
                            try {
                                // Prepare a blob URL
                                console.log("Trying download link method with simulated click ...");
                                var blob = new Blob([data], { type: contentType });
                                var url = urlCreator.createObjectURL(blob);
                                link.setAttribute('href', url);

                                // Set the download attribute (Supported in Chrome 14+ / Firefox 20+)
                                link.setAttribute("download", filename);

                                // Simulate clicking the download link
                                var event = document.createEvent('MouseEvents');
                                event.initMouseEvent('click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
                                link.dispatchEvent(event);
                                console.log("Download link method with simulated click succeeded");
                                success = true;

                            } catch (ex) {
                                console.log("Download link method with simulated click failed with the following exception:");
                                console.log(ex);
                            }
                        }

                        if (!success) {
                            // Fallback to window.location method
                            try {
                                // Prepare a blob URL
                                // Use application/octet-stream when using window.location to force download
                                console.log("Trying download link method with window.location ...");
                                var blob = new Blob([data], { type: octetStreamMime });
                                var url = urlCreator.createObjectURL(blob);
                                window.location = url;
                                console.log("Download link method with window.location succeeded");
                                success = true;
                            } catch (ex) {
                                console.log("Download link method with window.location failed with the following exception:");
                                console.log(ex);
                            }
                        }

                    }
                }

                if (!success) {
                    // Fallback to window.open method
                    console.log("No methods worked for saving the arraybuffer, using last resort window.open");
                    window.open(httpPath, '_blank', '');
                }
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };

    var _uploadDocument = function (data) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/document/UploadDocument', data, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        }).success(function (data) {
            deferred.resolve(data);
        }).error(function (error) {
            deferred.reject(error);
        });
        return deferred.promise;
    };

    var _removeDocument = function (id) {
        var deferred = $q.defer();
        $http.put(serviceBase + "/api/document/" + id)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };

    var _sendEmail = function (data) {
        var deferred = $q.defer();
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/document/SendEmail', data)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };

    documentServiceFactory.GetDocuments = _getDocuments;
    documentServiceFactory.GetDocument = _getDocument;
    documentServiceFactory.UploadDocument = _uploadDocument;
    documentServiceFactory.RemoveDocument = _removeDocument;
    documentServiceFactory.SendEmail = _sendEmail;
    return documentServiceFactory;
}])
.factory('instantLogService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var instantLogServiceFactory = {};

    var _getInstantLog = function (id, day) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/instantLog/GetInstantLog/' + id + "/" + day;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _saveMessage = function (message) {
        var deferred = $q.defer();
        $http.post(serviceBase + "/api/instantLog/LogInstantMessage", message)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };

    instantLogServiceFactory.GetInstantLog = _getInstantLog;
    instantLogServiceFactory.SaveMessage = _saveMessage;
    return instantLogServiceFactory;
}])
.factory('notificationService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings, localStorageService) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var notificationServiceFactory = {};

    var _getCount = function (id) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/notification/GetCount/' + id;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };

    var _deleteNotification = function (dayCareId, id) {
        var deferred = $q.defer();
        $http.put(serviceBase + "/api/notification/RemoveNotification/" + dayCareId + "/" + id)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };

    var _getNotifications = function (id) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/notification/' + id;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };

    notificationServiceFactory.GetCount = _getCount;
    notificationServiceFactory.GetNotifications = _getNotifications;
    notificationServiceFactory.DeleteNotification = _deleteNotification;
    return notificationServiceFactory;
}])
.factory('parentService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var parentServiceFactory = {};

    var _getParentData = function (parentId) {
        var deferred = $q.defer();
        $http.get(serviceBase + '/api/parent/' + parentId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });

        return deferred.promise;
    };
    parentServiceFactory.GetParentData = _getParentData;
    return parentServiceFactory;
}])
.factory('registerService', ['$http', '$q', 'localStorageService', 'authService', 'dayCareSettings',
    function ($http, $q, localStorageService, authService, dayCareSettings) {
        var serviceBase = dayCareSettings.apiBaseUri;
        var registerServiceFactory = {};

        var _signUpDayCare = function (dayCare) {
            var deferred = $q.defer();
            $http.post(serviceBase + '/api/dayCare/', dayCare)
                .success(function (data) {
                    deferred.resolve(data);
                }).error(function (error) {
                    deferred.reject(error);
                });

            return deferred.promise;
        };
        var _signUpParent = function (parent) {
            var deferred = $q.defer();
            $http.post(serviceBase + '/api/parent/', parent)
                .success(function (data) {
                    deferred.resolve(data);
                }).error(function (error) {
                    deferred.reject(error);
                });
            return deferred.promise;
        };
        var _fillAuthData = function (data) {
            $http.defaults.headers.common['Authorization'] = 'Bearer ' + data.userToken;
            localStorageService.set('authorizationData', { token: data.userToken, userName: data.userName });
            localStorageService.set('userId', data.id);
            localStorageService.set('userName', data.userName);
            localStorageService.set('userEmail', data.userEmail);
            localStorageService.set('userPhone', data.userPhone);
            localStorageService.set('userRole', data.userRole);
            localStorageService.set('isAuth', true);

            authService.Authentication.isAuth = true;
            authService.Authentication.userName = data.userName;
            authService.Authentication.userId = data.id;
        };
        var _checkParent = function (parent) {
            var deferred = $q.defer();
            $http.post(serviceBase + '/api/parent/CheckParentByEmail', parent)
                .success(function (data) {
                    deferred.resolve(data);
                }).error(function (error) {
                    deferred.reject(error);
                });

            return deferred.promise;
        };
        registerServiceFactory.SignUpDayCare = _signUpDayCare;
        registerServiceFactory.SignUpParent = _signUpParent;
        registerServiceFactory.CheckParent = _checkParent;
        return registerServiceFactory;
    }])
.factory('reportsService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var reportsServiceFactory = {};

    var _getKids = function (id, role) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/kid/';
        if (role === "parent")
            uri += "GetKidsFromParent/" + id;
        else if (role === "dayCare")
            uri += "GetKidsFromDayCare/" + id;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };

    var _pullReport = function (kidId, id, day) {
        var deferred = $q.defer();
        $http.get(serviceBase + "/api/log/GetKidLogOnADay/" + kidId + "/" + id + "/" + day)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _sendEmail = function (dayCareId, emailList) {
        var deferred = $q.defer();
        var Indata = { 'From': dayCareId, 'To': emailList };
        $http.post(serviceBase + "/api/dayCare/SendEmail/", Indata)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _sendCustomEmail = function (dayCareId, emailList) {
        var deferred = $q.defer();
        var Indata = { 'From': dayCareId, 'To': emailList };
        $http.post(serviceBase + "/api/dayCare/SendCustomEmail/", Indata)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };

    reportsServiceFactory.GetKids = _getKids;
    reportsServiceFactory.PullReport = _pullReport;
    reportsServiceFactory.SendEmail = _sendEmail;
    reportsServiceFactory.SendCustomEmail = _sendCustomEmail;
    return reportsServiceFactory;
}])
.service('scrollService', function () {
    this.scrollTo = function (eID) {
        // This scrolling function 
        // is from http://www.itnewb.com/tutorial/Creating-the-Smooth-Scroll-Effect-with-JavaScript

        var startY = currentYPosition();
        var stopY = elmYPosition(eID);
        var distance = stopY > startY ? stopY - startY : startY - stopY;
        if (distance < 100) {
            scrollTo(0, stopY); return;
        }
        var speed = Math.round(distance / 100);
        if (speed >= 20) speed = 20;
        var step = Math.round(distance / 25);
        var leapY = stopY > startY ? startY + step : startY - step;
        var timer = 0;
        if (stopY > startY) {
            for (var i = startY; i < stopY; i += step) {
                setTimeout("window.scrollTo(0, " + leapY + ")", timer * speed);
                leapY += step; if (leapY > stopY) leapY = stopY; timer++;
            } return;
        }
        for (var i = startY; i > stopY; i -= step) {
            setTimeout("window.scrollTo(0, " + leapY + ")", timer * speed);
            leapY -= step; if (leapY < stopY) leapY = stopY; timer++;
        }

        function currentYPosition() {
            // Firefox, Chrome, Opera, Safari
            if (self.pageYOffset) return self.pageYOffset;
            // Internet Explorer 6 - standards mode
            if (document.documentElement && document.documentElement.scrollTop)
                return document.documentElement.scrollTop;
            // Internet Explorer 6, 7 and 8
            if (document.body.scrollTop) return document.body.scrollTop;
            return 0;
        }

        function elmYPosition(eId) {
            var elm = document.getElementById(eId);
            var y = elm.offsetTop;
            var node = elm;
            while (node.offsetParent && node.offsetParent != document.body) {
                node = node.offsetParent;
                y += node.offsetTop;
            } return y;
        }
    };
})
.factory('settingsService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var settingsServiceFactory = {};

    var _getSettings = function (dayCareId) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/settings/' + dayCareId;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _saveSettings = function (settings) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/settings/', settings)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _getDayCareInfo = function (dayCareId) {
        var deferred = $q.defer();
        var uri = serviceBase + '/api/dayCare/GetDayCareInfo/' + dayCareId;
        $http.get(uri)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _manageDayCareInfo = function (data) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/dayCare/ManageDayCareInfo', data)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var _uploadSnap = function (data) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/dayCare/UploadSnap', data, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        }).success(function (data) {
            deferred.resolve(data);
        }).error(function (error) {
            deferred.reject(error);
        });
        return deferred.promise;
    };

    settingsServiceFactory.SaveSettings = _saveSettings;
    settingsServiceFactory.GetSettings = _getSettings;
    settingsServiceFactory.GetDayCareInfo = _getDayCareInfo;
    settingsServiceFactory.ManageDayCareInfo = _manageDayCareInfo;
    settingsServiceFactory.UploadSnap = _uploadSnap;
    return settingsServiceFactory;
}])
.factory('signupService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var consServiceFactory = {};

    var register = function (user) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/consumer/', user)
            .success(function (data) {
                $http.defaults.headers.common['Authorization'] = 'Bearer ' + data.userToken;
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    consServiceFactory.Register = register;
    return consServiceFactory;
}])
.factory('schedulesService', ['$http', '$q', 'dayCareSettings', function ($http, $q, dayCareSettings) {
    var serviceBase = dayCareSettings.apiBaseUri;
    var scheServiceFactory = {};
    scheServiceFactory.times = [{ time: '6:00am', value: 0 }, { time: '6:30am', value: 1 }, { time: '7:00am', value: 2 }, { time: '7:30am', value: 3 }, { time: '8:00am', value: 4 }, { time: '8:30am', value: 5 }
    , { time: '9:00am', value: 6 }, { time: '9:30am', value: 7 }, { time: '10:00am', value: 8 }, { time: '10:30am', value: 9 }, { time: '11:00am', value: 10 }, { time: '11:30am', value: 11 }, { time: '12:00pm', value: 12 }
    , { time: '12:30pm', value: 13 }, { time: '1:00pm', value: 14 }, { time: '1:30pm', value: 15 }, { time: '2:00pm', value: 16 }, { time: '2:30pm', value: 17 }, { time: '3:00pm', value: 18 }, { time: '3:30pm', value: 19 }
    , { time: '4:00pm', value: 20 }, { time: '4:30pm', value: 21 }, { time: '5:00pm', value: 22 }, { time: '5:30pm', value: 23 }, { time: '6:00pm', value: 24 }, { time: '6:30pm', value: 25 }, { time: '7:00pm', value: 26 }];

    var addSchedule = function (schedule) {
        var deferred = $q.defer();
        $http.post(serviceBase + "/api/schedule/InsertSchedule", schedule)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var getSchedules = function (dayCareId) {
        var deferred = $q.defer();
        $http.get(serviceBase + '/api/schedule/GetSchedules/' + dayCareId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var saveScheduleMessage = function (message) {
        var deferred = $q.defer();
        $http.post(serviceBase + "/api/schedule/SaveScheduleMessage", message)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var sendEmail = function (data) {
        var deferred = $q.defer();
        $http.post(serviceBase + '/api/schedule/SendEmail', data)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var removeSchedule = function (id) {
        var deferred = $q.defer();
        $http.put(serviceBase + "/api/schedule/" + id)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    var removeScheduleMessage = function (dayCareId, scheduleId, messageId) {
        var deferred = $q.defer();
        $http.put(serviceBase + "/api/schedule/RemoveScheduleMessage/" + dayCareId + "/" + scheduleId + "/" + messageId)
            .success(function (data) {
                deferred.resolve(data);
            }).error(function (error) {
                deferred.reject(error);
            });
        return deferred.promise;
    };
    scheServiceFactory.SendEmail = sendEmail;
    scheServiceFactory.GetSchedules = getSchedules;
    scheServiceFactory.AddSchedule = addSchedule;
    scheServiceFactory.SaveScheduleMessage = saveScheduleMessage;
    scheServiceFactory.RemoveSchedule = removeSchedule;
    scheServiceFactory.RemoveScheduleMessage = removeScheduleMessage;
    return scheServiceFactory;
}])
.directive('cancelClick', ['$window', function ($window) {
    return {
        restrict: 'A',
        link: function (scope, elem, attrs) {
            elem.bind('click', function () {
                $window.history.back();
            });
        }
    };
}])
.directive("clickAndDisable", ["$parse", function ($parse) {
    "use strict";
    return {
        restrict: "A",
        controller: ['$scope', function ($scope) {
            // indicates if function invoked by the click is currently running
            this.running = false;
        }],
        compile: function ($element, attr) {
            var fn = $parse(attr.clickAndDisable);
            return function (scope, element, attr, controller) {
                element.on("click", function (event) {
                    element.prop('disabled', true);
                    // if function is currently running don't execute it again
                    if (!controller.running) {
                        scope.$apply(function () {
                            // if invoked function returns a promise wait for it's completion
                            var result = fn(scope, { $event: event });
                            if (result.finally !== undefined) {
                                controller.running = true;
                                result.finally(function () {
                                    controller.running = false;
                                    element.prop('disabled', false);
                                });
                            }
                        });
                    }
                });
            };
        }
    };
}])
.directive('scheduleTime', function ($compile) {
    var getTemplate = function (scheduleMode, scheduleTimes, scheduleTime, placeHolder) {
        var template = '<div class="input-dropdown">' +
            '<input type="text" maxlength="8" ng-model="scheduleTime" placeholder="{{placeHolder}}" ng-blur="inputBlur($event)" ng-focus="inputFocus()" ng-change="inputChange()"/>' +
            '<ul ng-show="dropdownVisible">' +
            '<li ng-repeat="item in scheduleTimes"' +
            'ng-click="selectItem(item)" ng-class="{\'active\': activeItemIndex === $index}" ng-mouseenter="setActive($index)" ng-mousedown="dropdownPressed()">' +
            '<span ng-if="item.time">{{item.time}}</span>' +
            '</li>' +
            '</ul></div>';
        return template;
    };

    var linker = function (scope, element, attrs) {
        element.html(getTemplate(scope.scheduleMode, scope.scheduleTimes, scope.scheduleTime, scope.placeHolder));
        $compile(element.contents())(scope.$new());
        scope.dropdownVisible = false;
        scope.activeItemIndex = 0;
        var pressedDropdown = false;
        scope.inputFocus = function () {
            scope.setActive(0);
            showDropdown();
        };
        scope.setActive = function (itemIndex) {
            scope.activeItemIndex = itemIndex;
        };
        scope.selectItem = function (item) {
            scope.scheduleTime = scope.scheduleTime = item.time;
            hideDropdown();
        };
        scope.dropdownPressed = function () {
            pressedDropdown = true;
        };
        scope.inputBlur = function (event) {
            if (pressedDropdown) {
                pressedDropdown = false;
                return;
            }
            hideDropdown();
        };
        scope.inputChange = function () {
            scope.selectedItem = null;
            showDropdown();

            if (!scope.scheduleTime) {
                scope.dropdownItems = scope.defaultDropdownItems || [];
                return;
            }

            if (scope.filterListMethod) {
                var promise = scope.filterListMethod({ userInput: scope.scheduleTime });
                if (promise) {
                    promise.then(function (dropdownItems) {
                        scope.dropdownItems = dropdownItems;
                    });
                }
            }
        };
        var showDropdown = function () {
            scope.dropdownVisible = true;
        };
        var hideDropdown = function () {
            scope.dropdownVisible = false;
        };
    };
    return {
        restrict: 'E',
        link: linker,
        terminal: true,
        scope: {
            content: '=',
            scheduleMode: '=',
            scheduleTimes: '=',
            scheduleTime: '=',
            placeHolder: '='
        }
    };
})
.directive('customQuestion', function ($compile) {

    var linker = function (scope, element, attrs) {
        element.html(scope.content.template);
        $compile(element.contents())(scope.$new());
    };

    return {
        restrict: 'E',
        link: linker,
        terminal: true,
        scope: {
            content: '=',
            myDirectiveVar: '=',
            customQuesOptions: '=',
            customQuesIndex: '=',
            addOption: '&',
            removeOption: '&',
            removeQuestion: '&'
        }
    };
})
.directive('customReport', function ($compile) {

    var getTemplate = function (questionId, customQuesValues, customQuesAnswers, customQuesOptions) {
        var template = '';
        switch (questionId) {
            case 1:
                template = '<span class=".hero-heading-black-xs"><strong>{{customQuesValues[0]}}</strong></span><br/><br/>';
                break;
            case 2:
                template = '<form class="form-inline" role="form"><div class="form-group"><label>{{customQuesValues[0]}}:</label><input type="text" class="form-control" ng-model="customQuesAnswers[0]"></div></form>';
                break;
            case 3:
                template = '<form class="form-inline" role="form"><label>{{customQuesValues[0]}}:</label><input type="text" class="input-small" ng-model="customQuesAnswers[0]">&nbsp;<label>{{customQuesValues[1]}}:</label><input type="text" class="input-small" ng-model="customQuesAnswers[1]"></form>';
                break;
            case 4:
                template = '<label>{{customQuesValues[0]}}</label><br/><form role="form"><div ng-repeat="option in customQuesOptions"><label class="radio-inline"><input type="radio" name="optradio" ng-model="option.check" ng-value="true" ng-click="changeOptionFn(option,customQuesIndex)"/>{{option.value}}</label></div></form>';
                break;
            case 5:
                template = '<label>{{customQuesValues[0]}}</label>: <input type="text" class="form-control" ng-model="customQuesAnswers[0]"></input><form role="form"><div ng-repeat="option in customQuesOptions"><label class="radio-inline"><input type="radio" ng-model="option.check" ng-value="true" name="optradio" ng-model="option.check"/>{{option.value}}</label></div></form>';
                break;
        }
        return template;
    };

    var linker = function (scope, element, attrs) {
        scope.changeOptionFn = function (item, id) {
            scope.changeOption()(item, id);
        }
        element.html(getTemplate(scope.content.id, scope.customQuesValues, scope.customQuesAnswers, scope.customQuesOptions));
        $compile(element.contents())(scope.$new());
    };

    return {
        restrict: 'E',
        link: linker,
        terminal: true,
        scope: {
            content: '=',
            customQuesOptions: '=',
            customQuesValues: '=',
            customQuesAnswers: '=',
            customQuesIndex: '=',
            changeOption: "&"
        }
    };
})
.directive('customReportReadOnly', function ($compile) {
    var getTemplate = function (questionId, customQuesValues, customQuesAnswers, customQuesOptions) {
        var template = '';
        switch (questionId) {
            case 1:
                template = '<span class="hero-heading-black-xs"><strong>{{customQuesValues[0]}}</strong></span><br/><br/>';
                break;
            case 2:
                template = '<div class="form-group"><span class="hero-heading-black-xs"><b>{{customQuesValues[0]}}: </b></span><span class="hero-heading-black-xs">{{customQuesAnswers[0]}}</span></div>';
                break;
            case 3:
                template = '<div class="form-group"><span class="hero-heading-black-xs"><b>{{customQuesValues[0]}}: </b></span><span class="hero-heading-black-xs">{{customQuesAnswers[0]}}</span>&nbsp;<span class="hero-heading-black-xs"><b>{{customQuesValues[1]}}: </b></span><span class="hero-heading-black-xs">{{customQuesAnswers[1]}}</span></div>';
                break;
            case 4:
                template = '<div class="form-group"><span class="hero-heading-black-xs"><b>{{customQuesValues[0]}}: </b></span><br/><div ng-repeat="option in customQuesOptions"><span ng-class="{\'hero-heading-black-xs\': !option.check,\'hero-heading-black-xs-bold\': option.check}">{{option.value}}&nbsp;</span></div></div>';
                break;
            case 5:
                template = '<div class="form-group"><span class="hero-heading-black-xs"><b>{{customQuesValues[0]}}: </b></span><span class="hero-heading-black-xs">{{customQuesAnswers[0]}}</span><br/><div ng-repeat="option in customQuesOptions"><span ng-class="{\'hero-heading-black-xs\': !option.check,\'hero-heading-black-xs-bold\': option.check}">{{option.value}}&nbsp;</span></div></div>';
                break;
        }
        return template;
    };

    var linker = function (scope, element, attrs) {
        scope.changeOptionFn = function (item, id) {
            scope.changeOption()(item, id);
        }
        element.html(getTemplate(scope.content.id, scope.customQuesValues, scope.customQuesAnswers, scope.customQuesOptions));
        $compile(element.contents())(scope.$new());
    };

    return {
        restrict: 'E',
        link: linker,
        terminal: true,
        scope: {
            content: '=',
            customQuesOptions: '=',
            customQuesValues: '=',
            customQuesAnswers: '='
        }
    };
})
.directive('slider', function ($timeout) {
    return {
        restrict: 'AE',
        replace: true,
        scope: {
            images: '='
        },
        link: function (scope, elem, attrs) {
            scope.currentIndex = 0;
            scope.next = function () {
                scope.currentIndex < scope.images.length - 1 ? scope.currentIndex++ : scope.currentIndex = 0;
            };
            scope.prev = function () {
                scope.currentIndex > 0 ? scope.currentIndex-- : scope.currentIndex = scope.images.length - 1;
            };
            scope.$watch('currentIndex', function () {
                scope.images.forEach(function (image) {
                    image.visible = false;
                });
                scope.images[scope.currentIndex].visible = true;
            });
            /* Start: For Automatic slideshow*/

            var timer;
            var sliderFunc = function () {
                timer = $timeout(function () {
                    scope.next();
                    timer = $timeout(sliderFunc, 3000);
                }, 3000);
            };

            sliderFunc();
            scope.$on('$destroy', function () {
                $timeout.cancel(timer);
            });

            /* End : For Automatic slideshow*/
        },
        templateUrl: 'partials/slider.html'
    }
});