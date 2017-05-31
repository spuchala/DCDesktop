var app = angular.module('dayCareApp', ['ngRoute', 'LocalStorageModule', 'angular-datepicker', 'ngAnimate', 'textAngular']);
app.config(function ($routeProvider, $locationProvider) {
    $routeProvider.when("/home", {
        controller: 'homeController',
        templateUrl: 'partials/home.html',
        title: 'Free Child Care Management Software, Free Daycare Management Software, Free Android, IPhone apps for Child Care Management, Free Android, IPhone apps for Daycare Management'
    }).
    when("/signup", {
        controller: 'signupController',
        templateUrl: 'partials/signup.html',
        title: 'Sign Up as a family day care/parent'
    }).
    when("/login", {
        controller: 'loginController',
        templateUrl: 'partials/login.html',
        title: 'Login'
    }).
    when("/login/:message", {
        controller: 'loginController',
        templateUrl: 'partials/login.html',
        title: 'Login'
    }).
    when("/aLogin", {
        controller: 'loginController',
        templateUrl: 'partials/aLogin.html',
        title: 'Alexa Login'
    }).
    when("/about", {
        controller: 'aboutController',
        templateUrl: 'partials/about.html',
        title: 'Free Child Care Management Software, Free Daycare Management Software, Free Android, IPhone apps for Child Care Management, Free Android, IPhone apps for Daycare Management'
    }).
    when("/features", {
        controller: 'featuresController',
        templateUrl: 'partials/features.html',
        title: 'Giggles ware Free Child Care Management features'
    }).
    when("/features/:id", {
        controller: 'featuresController',
        templateUrl: 'partials/features.html',
        title: 'Giggles ware Free Child Care Management features'
    }).
    when("/registerDayCare", {
        controller: 'registerController',
        templateUrl: 'partials/registerDayCare.html',
        title: 'Register Your Family Day Care with us to begin'
    }).
    when("/registerParent", {
        controller: 'registerController',
        templateUrl: 'partials/registerParent.html',
        title: 'Register as parent/guardian with us'
    }).
    when("/dayCare", {
        controller: 'dayCareController',
        templateUrl: 'partials/dayCare.html',
        title: 'Your Account for Home Day Care'
    }).
    when("/dayCare/:dayCareId", {
        controller: 'dayCareController',
        templateUrl: 'partials/dayCare.html',
        title: 'Your Account for Home Day Care'
    }).
    when("/settings", {
        controller: 'settingsController',
        templateUrl: 'partials/settings.html',
        title: 'Your Day Care Settings'
    }).
    when("/settings/:dayCareId", {
        controller: 'settingsController',
        templateUrl: 'partials/settings.html',
        title: 'Your Day Care Settings'
    }).
    when("/customReport/:dayCareId", {
        controller: 'customReportController',
        templateUrl: 'partials/customReport.html',
        title: 'Your Day Care Custom Report'
    }).
    when("/classes", {
        controller: 'classesController',
        templateUrl: 'partials/classes.html',
        title: 'Your Account for Home Day Care'
    }).
    when("/classes/:dayCareId", {
        controller: 'classesController',
        templateUrl: 'partials/classes.html',
        title: 'Your Account for Home Day Care'
    }).
    when("/documents/:dayCareId", {
        controller: 'documentController',
        templateUrl: 'partials/documents.html',
        title: 'Documents for Home Day Care'
    }).
    when("/parent", {
        controller: 'parentController',
        templateUrl: 'partials/parent.html',
        title: 'Your Account for Parent'
    }).
    when("/parent/:parentId", {
        controller: 'parentController',
        templateUrl: 'partials/parent.html',
        title: 'Your Account for Parent'
    }).
    when("/reports", {
        controller: 'reportsController',
        templateUrl: 'partials/reports.html',
        title: 'Reports for the kids'
    }).
    when("/reports/:id", {
        controller: 'reportsController',
        templateUrl: 'partials/reports.html',
        title: 'Reports for the kids'
    }).
    when("/instantLog", {
        controller: 'instantLogController',
        templateUrl: 'partials/instantLog.html',
        title: 'Instant Log for the kids'
    }).
    when("/instantLog/:id", {
        controller: 'instantLogController',
        templateUrl: 'partials/instantLog.html',
        title: 'Instant Log for the kids'
    }).
    when("/notifications", {
        controller: 'notificationsController',
        templateUrl: 'partials/notifications.html',
        title: 'Notifications for the kids'
    }).
    when("/notifications/:id", {
        controller: 'notificationsController',
        templateUrl: 'partials/notifications.html',
        title: 'Notifications for the kids'
    }).
    when("/schedules", {
        controller: 'schedulesController',
        templateUrl: 'partials/schedules.html',
        title: 'Your DayCare Schedules'
    }).
    when("/schedules/:dayCareId", {
        controller: 'schedulesController',
        templateUrl: 'partials/schedules.html',
        title: 'Your DayCare Schedules'
    }).
    when("/attendance/:dayCareId", {
        controller: 'attendanceController',
        templateUrl: 'partials/attendance.html',
        title: 'Attendance for the kids'
    }).
    when("/privacy", {
        templateUrl: 'partials/privacy.html',
        title: 'privacy statement'
    }).
    when('/', {
        controller: 'homeController',
        templateUrl: 'partials/home.html',
        title: 'Family Day Care Managment simplified'
    }).otherwise({
        redirectTo: '/'
    });
    $locationProvider.html5Mode({
        enabled: true
    });
});

var apiBase = 'http://localhost:51652';
//var apiBase = 'http://api.gigglesware.com';
//var apiBase = 'https://gigglesware.com';
var codeVersion = '7.1';
app.constant('dayCareSettings', {
    apiBaseUri: apiBase,
    apiBaseCodeVersion: codeVersion
});

app.run(['$location', '$rootScope', function ($location, $rootScope) {
    $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
        $rootScope.title = current.$$route.title;
    });
}]);

app.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
});

app.run(['authService', function (authService) {
    authService.FillAuthData();
}]);

app.config(function (localStorageServiceProvider) {
    localStorageServiceProvider
      .setStorageType('sessionStorage');
});