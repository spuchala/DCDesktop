'use strict';
app.controller('aboutController', ['$scope',
    function ($scope, $translate) {
    }])
.controller('attendanceController', ['$scope', '$location', '$routeParams', 'backgroundService', 'dayCareService',
    function ($scope, $location, $routeParams, backgroundService, dayCareService) {
        $scope.kids = {};
        this.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
            dayCareService.GetAttendance($routeParams.dayCareId).then(function (data) {
                $scope.kids = data;
                if ($scope.kids == null)
                    $scope.warning = 'No Kids found in your day care. Add a kid from day care section.';
            },
               function (error) {
                   $scope.error = error;
               });
        };

        this.logAttendance = function () {
            $scope.showSpin = true;
            dayCareService.LogAttendance($scope.kids).then(function (data) {
                $scope.success = 'attendance updated successfully.';
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showSpin = false;
               });
        };
    }])
.controller('classesController', ['$scope', '$location', '$routeParams', 'classesService', 'dayCareService', '$timeout', 'backgroundService',
    function ($scope, $location, $routeParams, classesService, dayCareService, $timeout, backgroundService) {
        $scope.class = {};
        $scope.classes = [];
        $scope.showKidDetails = false;
        $scope.kids = [];
        $scope.showAddKid = false;
        $scope.selectedClassId = '';
        $scope.newKid = {};
        $scope.shortKid = false;

        this.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
            classesService.GetClasses($routeParams.dayCareId).then(function (data) {
                if (data != null && data != undefined)
                    $scope.classes = data;
                else
                    $scope.warning = 'No classes found for your day care. Click Create Class above to create one.';
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
        };

        this.addClass = function () {
            $scope.class.DayCareId = $routeParams.dayCareId;
            classesService.AddClass($scope.class).then(function (data) {
                $scope.class.ClassId = data.ClassId;
                $scope.class.NoOfKids = 0;
                if ($scope.classes != null && $scope.classes != undefined)
                    $scope.classes.push($scope.class);
                $scope.class = {};
                $scope.success = 'class added successfully.';
                $scope.warning = '';
                $timeout(function () {
                    $scope.success = null;
                }, 4000);
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
               });
        };
        this.getKidsInAClass = function (classId) {
            classesService.GetKidsInAClass(classId).then(function (data) {
                $scope.kids = data;
                if ($scope.kids == null) {
                    classesService.GetKidsWithNoClass($routeParams.dayCareId).then(function (data) {
                        $scope.noClassKids = data;
                    },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
                }
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
        };
        this.assignClassToKids = function (classId) {
            $scope.class.ClassId = classId;
            $scope.class.DayCareId = $routeParams.dayCareId;
            $scope.class.Kids = $scope.noClassKids;
            classesService.AssignKidsToClass($scope.class).then(function (data) {
                $scope.warning = '';
                $scope.success = "kids added to class";
                $scope.noClassKids = null;
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
        };
        this.getKidDetails = function (kidId, kidName) {
            $scope.kName = kidName;
            $scope.showKidDetails = true;
            classesService.GetKid(kidId, $routeParams.dayCareId).then(function (data) {
                $scope.kid = data;
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
        };
        this.showAddKidModal = function (classId) {
            $scope.showAddKid = true;
            $scope.selectedClassId = classId;
        };
        this.addKid = function () {
            $scope.showSpin = true;
            $scope.newKid.ClassId = $scope.selectedClassId;
            $scope.newKid.DayCareId = $routeParams.dayCareId;
            if ($scope.shortKid) {
                dayCareService.AddShortKid($scope.newKid).then(function (data) {
                    $scope.showAddKid = false;
                    $scope.newKid.KidId = data.KidId;
                    $scope.kids.push($scope.newKid);
                    $scope.success = 'kid added successfully.';
                    $scope.warning = '';
                },
                   function (error) {
                       $scope.error = error;
                   }).finally(function () {
                       $scope.showSpin = false;
                   });
            }
            else {
                dayCareService.AddKid($scope.newKid).then(function (data) {
                    $scope.showAddKid = false;
                    $scope.newKid.KidId = data.KidId;
                    $scope.kids.push($scope.newKid);
                    $scope.success = 'kid added successfully.';
                    $scope.warning = '';
                },
                   function (error) {
                       $scope.error = error;
                   }).finally(function () {
                       $scope.showSpin = false;
                   });
            }
        };
    }])
.controller('customReportController', ['$scope', '$location', '$routeParams', 'backgroundService', 'customReportService', '$timeout',
    function ($scope, $location, $routeParams, backgroundService, customReportService, $timeout) {
        $scope.questionTypes = customReportService.questionTypes;
        $scope.selectedQuestion = 'Select One';
        $scope.question = {};
        $scope.questions = [];
        $scope.customReport = {};
        $scope.count = 0;

        $scope.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
        };

        $scope.saveCustomReport = function () {
            $scope.showSpin = true;
            $scope.customReport.questions = $scope.questions;
            $scope.customReport.DayCareId = $routeParams.dayCareId;
            customReportService.CreateCustomReport($scope.customReport).then(function (data) {
                $scope.success = 'Custom Report added successfully. You will be redirected to Kids page';
                $timeout(function () {
                    $location.path('/dayCare/' + $routeParams.dayCareId)
                }, 4000);
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showSpin = false;
               });
        };
        $scope.questionSelected = function (item) {
            $scope.selectedQuestion = item.type;
            $scope.question = item;
        };
        $scope.addQuestion = function () {
            $scope.activeQuestion = { type: $scope.question.type, id: $scope.question.id, trackId: $scope.count + 1, value: '', values: ['', ''], options: [{ text: 'option', value: '' }, { text: 'option', value: '' }, { text: 'option', value: '' }], template: $scope.question.template };
            $scope.count = $scope.count + 1;
            $scope.questions.push($scope.activeQuestion);
            $scope.activeQuestion = null;
        };
        $scope.addOptionToQuestion = function (item) {
            var option = { text: 'option', value: '' };
            item.options.push(option);
        };
        $scope.removeOptionFromQuestion = function (item) {
            if (item.options.length > 4)
                item.options.pop();
        };
        $scope.removeQuestion = function (item) {
            for (var i = 0; i < $scope.questions.length; i++) {
                if ($scope.questions[i].trackId == item.trackId) {
                    $scope.questions.splice(i);
                    break;
                }
            }
        };
        $scope.fileUpload = function (ele) {
            var file = ele.files[0];
            $scope.showSpinUploading = true;
            if (file.name.lastIndexOf('.docx') > 0 || file.name.lastIndexOf('.doc') > 0) {
                if (file.size < 3145728) {
                    if (window.FormData !== undefined) {
                        var data = new FormData();
                        data.append("file", file);
                        customReportService.UploadCustomReport(data).then(function (data) {
                            $scope.questions = data;
                            $scope.showSpinUploading = false;
                        },
                       function (error) {
                           $scope.error = error;
                           $scope.showSpinUploading = false;
                       }).finally(function () {
                           $scope.showSpinUploading = false;
                       });
                    }
                }
                else {
                    $scope.error = "File size can't exceed 3MB!";
                    return;
                }
            }
            else {
                $scope.error = "Only word files are allowed!";
                return;
            }
        };
    }])
.controller('dayCareController', ['$scope', '$location', '$routeParams', 'backgroundService', 'dayCareService', 'classesService', 'customReportService', 'localStorageService', '$rootScope',
    function ($scope, $location, $routeParams, backgroundService, dayCareService, classesService, customReportService, localStorageService, $rootScope) {
        $scope.dayCareData = {};
        $scope.kid = {};
        $scope.kidLog = {};
        $scope.kids = [];
        $scope.foods = [{ WhatKidAte: '', WhenKidAte: '', HowKidAte: '', AnySnack: '' }];
        $scope.pottys = [{ DiaperCheckTime: '', PottyTime: '', DiaperPottyType: '' }];
        $scope.naps = [{ NapTime: '' }];
        $scope.activities = [{ ActivityTime: '', Mood: '', Activities: '' }];
        $scope.showKidLog = false;
        $scope.selectedKid = 0;
        $scope.selectedClass = {};
        $scope.customReport = {};
        $scope.hide = false;
        $scope.editKid = false;
        $scope.shortKid = false;
        this.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
            $rootScope.$broadcast('notification-read');
            $scope.dayCareData.DayCareId = $scope.kid.dayCareId = $routeParams.dayCareId;
            dayCareService.GetDayCareData($scope.dayCareData.DayCareId).then(function (data) {
                $scope.dayCareData = data;

                if (localStorageService.get('settings') == null)
                    localStorageService.set('settings', $scope.dayCareData.Settings);
                else if (localStorageService.get('settings') != null) {
                    localStorageService.remove('settings');
                    localStorageService.set('settings', $scope.dayCareData.Settings);
                }

                if (!$scope.dayCareData.Settings.SettingsVisited) {
                    $location.path('/settings/' + $scope.dayCareData.DayCareId);
                    return;
                }
                if ($scope.dayCareData.Kids != null)
                    $scope.kids = $scope.dayCareData.Kids;
                else
                    $scope.warning = 'No Kids found in your day care. Add a kid clicking Add Kid above';
                $scope.heading = $scope.dayCareData.DayCareName;
                dayCareService.CheckCustomReportExists($scope.dayCareData.DayCareId).then(function (data) {
                    $scope.customReportCreated = data;
                },
                function (error) {
                    $scope.error = error;
                })
            },
               function (error) {
                   $scope.error = error;
               });
        };

        this.createCustomReport = function () {
            $location.path('/customReport/' + $scope.dayCareData.DayCareId);
        };

        this.getAvatar = function () {
            $scope.avatars = dayCareService.GetAvatars($scope.kid.Sex);
        };

        this.addKid = function () {
            $scope.heading = 'Add Kid';
            //check if new class or not
            if (this.validateClass()) {
                $scope.showSpin = true;
                $scope.kid.DayCareName = $scope.dayCareData.DayCareName;
                if ($scope.shortKid) {
                    dayCareService.AddShortKid($scope.kid).then(function (data) {
                        $scope.hide = false;
                        $scope.kid.KidId = data.KidId;
                        $scope.kids.push($scope.kid);
                        $scope.success = 'kid added successfully.';
                        $scope.warning = '';
                        $scope.errorInModal = '';
                    },
                       function (error) {
                           $scope.error = error;
                       }).finally(function () {
                           $scope.showSpin = false;
                       });
                }
                else {
                    if ($scope.kid.Avatar == null || $scope.kid.Avatar == undefined) {
                        var avatars = dayCareService.GetAvatars($scope.kid.Sex);
                        $scope.kid.Avatar = avatars[Math.floor(Math.random() * avatars.length)]
                    }
                    dayCareService.AddKid($scope.kid).then(function (data) {
                        $scope.hide = false;
                        $scope.kid.KidId = data.KidId;
                        $scope.kids.push($scope.kid);
                        $scope.success = 'kid added successfully.';
                        $scope.warning = '';
                        $scope.errorInModal = '';
                    },
                       function (error) {
                           $scope.error = error;
                       }).finally(function () {
                           $scope.showSpin = false;
                       });
                }
            }
        };

        this.validateClass = function () {
            if ($scope.selectedClass.ClassOption == 'new') {
                if ($scope.selectedClass.NewClassName != '' && $scope.selectedClass.NewClassName != undefined) {
                    if ($scope.classes != null) {
                        for (var i = 0; i < $scope.classes.length; i++) {
                            if ($scope.classes[i].ClassName == $scope.selectedClass.NewClassName) {
                                $scope.errorInModal = "Class already exists";
                                return false;
                            }
                        }
                    }
                    $scope.kid.ClassName = $scope.selectedClass.NewClassName;
                }
                else {
                    $scope.errorInModal = "Enter new class name";
                    return false;
                }
            }
            else if ($scope.selectedClass.ClassOption == 'existing') {
                if ($scope.selectedClass.ExistingClassName == undefined) {
                    $scope.errorInModal = "Select an existing class";
                    return false;
                }
                else
                    $scope.kid.ClassId = $scope.selectedClass.ExistingClassName.ClassId;
            }
            return true;
        };

        this.addFood = function () {
            $scope.foods.push({ WhatKidAte: '', WhenKidAte: '', HowKidAte: '', AnySnack: '' });
        };

        this.addPotty = function () {
            $scope.pottys.push({ DiaperCheckTime: '', PottyTime: '', DiaperPottyType: '' });
        };

        this.addNap = function () {
            $scope.naps.push({ NapTime: '' });
        };

        this.addActivity = function () {
            $scope.activities.push({ ActivityTime: '', Mood: '', Activities: '' });
        };

        this.pullKidReport = function (kidId, kidName) {
            $scope.showSpinPullReport = true;
            $scope.selectedKid = kidId;
            if (!$scope.dayCareData.Settings.CustomReport) {
                dayCareService.PullReport(kidId, $scope.dayCareData.DayCareId).then(function (data) {
                    $scope.kidLog = data;
                    $scope.foods = $scope.kidLog.Foods;
                    $scope.pottys = $scope.kidLog.Pottys;
                    $scope.naps = $scope.kidLog.Naps;
                    $scope.activities = $scope.kidLog.Activities;
                    $scope.heading = "Today's Report for " + kidName;
                    $scope.tab = 'dvFood';
                    $scope.showKidLog = true;
                },
                   function (error) {
                       $scope.heading = 'Report for ' + kidName + ' for Today:';
                       $scope.tab = 'dvFood';
                       $scope.showKidLog = true;
                   }).finally(function () {
                       $scope.showSpinPullReport = false;
                   });
            }
            else {
                customReportService.GetCustomReport(kidId, $scope.dayCareData.DayCareId).then(function (data) {
                    $scope.customReport = data;
                    $scope.heading = "Today's Report for " + kidName;
                    $scope.showKidLog = true;
                },
                   function (error) {
                       $scope.heading = 'Report for ' + kidName + ' for Today:';
                   }).finally(function () {
                       $scope.showSpinPullReport = false;
                   });
            }
        };

        this.saveReport = function () {
            $scope.showSpinReport = true;
            $scope.kidLog.KidId = $scope.selectedKid;
            $scope.kidLog.Foods = $scope.foods;
            $scope.kidLog.Pottys = $scope.pottys;
            $scope.kidLog.Naps = $scope.naps;
            $scope.kidLog.Activities = $scope.activities;
            dayCareService.SaveReport($scope.kidLog).then(function (data) {
                $scope.showKidLog = false;
                $scope.heading = $scope.dayCareData.DayCareName;
                resetLog();
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showSpinReport = false;
               });
        };

        this.saveCustomReport = function () {
            $scope.showCustomSpinReport = true;
            $scope.customReport.KidId = $scope.selectedKid;
            $scope.customReport.DayCareId = $scope.dayCareData.DayCareId;
            customReportService.SaveCustomReport($scope.customReport).then(function (data) {
                $scope.showKidLog = false;
                $scope.heading = $scope.dayCareData.DayCareName;
                $scope.customReport = {};
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showCustomSpinReport = false;
               });
        };

        this.changeRadioOption = function (option, id) {
            for (var i = 0; i < $scope.customReport.questions.length; i++) {
                if ($scope.customReport.questions[i].CustomReportQuestionId == id) {
                    for (var j = 0; j < $scope.customReport.questions[i].options.length; j++)
                        $scope.customReport.questions[i].options[j].check = false;
                    break;
                }
            }
            option.check = true;
        };

        this.resetLog = function () {
            $scope.foods = [{ WhatKidAte: '', WhenKidAte: '', HowKidAte: '', AnySnack: '' }];
            $scope.pottys = [{ DiaperCheckTime: '', PottyTime: '', DiaperPottyType: '' }];
            $scope.naps = [{ NapTime: '' }];
            $scope.activities = [{ ActivityTime: '', Mood: '', Activities: '' }];
        };

        this.getClasses = function () {
            $scope.errorInModal = '';
            classesService.GetClasses($routeParams.dayCareId).then(function (data) {
                $scope.classes = data;
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
               });
        };

        this.addEditKidUI = function (hide, heading, kidId) {
            $scope.hide = hide;
            $scope.heading = heading;
            this.getClasses();
            $scope.selectedClass = {};
            if (kidId > 0) {
                if ($scope.kid.ClassName != 'N/A' && $scope.kid.ClassName != '') {
                    $scope.selectedClass.ClassOption = 'existing';
                    if ($scope.classes != null) {
                        for (var i = 0; i < $scope.classes.length; i++) {
                            if ($scope.classes[i].ClassName == $scope.kid.ClassName) {
                                $scope.selectedClass.ExistingClassName = $scope.classes[i];
                            }
                        }
                    }
                }
                else
                    $scope.selectedClass.ClassOption = 'none';
                $scope.editKid = true;
                for (var i = 0; i < $scope.kids.length; i++) {
                    if ($scope.kids[i].KidId == kidId) {
                        $scope.kid = $scope.kids[i];
                        break;
                    }
                }
            }
            else {
                $scope.editKid = false;
                $scope.selectedClass.ClassOption = 'none';
            }
        };

        this.editKid = function () {
            if (this.validateClass()) {
                $scope.showSpin = true;
                $scope.kid.DayCareName = $scope.dayCareData.DayCareName;
                dayCareService.EditKid($scope.kid).then(function (data) {
                    $scope.hide = false;
                    $scope.editKid = false;
                    for (var i = 0; i < $scope.kids.length; i++) {
                        if ($scope.kids[i].KidId == $scope.kid.KidId) {
                            $scope.kids[i] = $scope.kid;
                            break;
                        }
                    }
                    $scope.kid = {};
                    $scope.success = 'kid info updated successfully.';
                    $scope.errorInModal = '';
                },
                   function (error) {
                       $scope.error = error;
                   }).finally(function () {
                       $scope.showSpin = false;
                   });
            }
        };

        this.deleteKid = function (kidId, kidName, reason) {
            if (confirm('Are you sure you want to delete ' + kidName + '?')) {
                dayCareService.DeleteKid(kidId, $scope.dayCareData.DayCareId, reason).then(function (data) {
                    for (var i = 0; i < $scope.kids.length; i++) {
                        if ($scope.kids[i].KidId == kidId) {
                            $scope.kids.splice(i, 1);
                            break;
                        }
                    }
                    $scope.success = 'kid deleted successfully.';
                    $scope.errorInModal = '';
                },
                   function (error) {
                       $scope.error = error;
                   }).finally(function () {
                       $scope.showSpin = false;
                   });
            }
        };
    }])
.controller('documentController', ['$scope', '$routeParams', 'backgroundService', 'documentService', 'reportsService', 'localStorageService',
    function ($scope, $routeParams, backgroundService, documentService, reportsService, localStorageService) {
        $scope.uploadTitle = '';
        $scope.docs = [];
        $scope.kids = {};
        $scope.role = localStorageService.get('userRole');

        this.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
            documentService.GetDocuments($routeParams.dayCareId).then(function (data) {
                $scope.docs = data;
                if (!$scope.docs)
                    $scope.warning = "No documents exist.";
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
        };

        this.uploadDoc = function () {
            var file = document.getElementById('file').files[0];
            $scope.showSpin = true;
            if (file.name.lastIndexOf('.docx') > 0 || file.name.lastIndexOf('.doc') > 0
                || file.name.lastIndexOf('.txt') > 0 || file.name.lastIndexOf('.pdf') > 0
                || file.name.lastIndexOf('.xlsx') > 0 || file.name.lastIndexOf('.xls') > 0
                || file.name.lastIndexOf('.ppt') > 0 || file.name.lastIndexOf('.pptx') > 0
                || file.name.lastIndexOf('.gif') > 0 || file.name.lastIndexOf('.jpeg') > 0
                || file.name.lastIndexOf('.jpg') > 0) {
                if (window.FormData !== undefined) {
                    var data = new FormData();
                    data.append("file", file);
                    data.append("title", $scope.uploadTitle);
                    data.append("id", $routeParams.dayCareId);
                    documentService.UploadDocument(data).then(function (response) {
                        if ($scope.docs == null)
                            $scope.docs = [];
                        $scope.docs.push(response);
                        $scope.success = "Document uploaded successfully.";
                        $scope.showSpin = false;
                    },
                   function (error) {
                       $scope.error = error;
                       $scope.showSpin = false;
                   }).finally(function () {
                       $scope.showSpin = false;
                   });
                }
            }
            else {
                $scope.error = "Only certain files are allowed:Images/Pdfs/Word/Excel/Text/Powerpoint";
                return;
            }
        };

        this.remove = function (id) {
            documentService.RemoveDocument(id).then(function (data) {
                $scope.success = "Document removed successfully";
                for (var i = 0; i < $scope.docs.length; i++) {
                    if ($scope.docs[i].Id == id) {
                        $scope.docs.splice(i);
                        break;
                    }
                }
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
        };

        this.getKids = function () {
            reportsService.GetKids($routeParams.dayCareId, localStorageService.get('userRole')).then(function (data) {
                $scope.kids = data;
                if ($scope.kids == null)
                    $scope.warning = 'No Kids found in your day care. Go to kids section to add kids';
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
        };

        this.email = function (doc, kids, emailId) {
            if (emailId == "" && (kids == null || kids.length == 0))
                $scope.error = "Please select kids or enter email.";
            else {
                var emailList = [];
                if (kids != null && kids.length > 0)
                    for (var i = 0; i < kids.length; i++) {
                        emailList.push(kids[i].Email);
                    }
                if (emailId != "")
                    emailList.push(emailId);
                doc.EmailList = emailList;
                doc.DayCareName = localStorageService.get('userName');
                documentService.SendEmail(doc).then(function (data) {
                    $scope.success = "Emails sent successfully";
                },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
            }
        };

        this.download = function (id, name) {
            documentService.GetDocument(id, name).then(function (data) {
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
        };
        this.copyUrl = function () {
            var txtCopy = document.getElementById('txtCopy');
            txtCopy.focus();
            txtCopy.select();
            setTimeout(function () {
                document.execCommand('copy');
            }, 20);
            $scope.success = "Url copied to share.";
        };
    }])
.controller('featuresController', ['$scope', '$routeParams', '$anchorScroll', '$location',
    function ($scope, $routeParams, $anchorScroll, $location) {
        this.onLoad = function () {
            $location.hash($routeParams.id);
            $anchorScroll();
        }
    }])
.controller('homeController', ['backgroundService', '$scope', '$timeout',
    function (backgroundService, $scope, $timeout) {

        $scope.images = [
            { id: 0, image: 'images/Merged.png', title: 'One stop solution for Family Day Care Management', show: false, color: 'white', textOnBackGroundColor: false },
            { id: 1, image: 'images/Profile.png', title: 'Get a free public facing website to promote and share your daycare', show: false, color: 'black', knowMore: 'features/dvProfile', knowMoreText: 'Know More', textOnBackGroundColor: true },
            { id: 2, image: 'images/Alexa.png', title: 'Alexa + GigglesWare = Handsfree assistant to log kids activities with commands and ask Alexa for kids reports', show: false, color: 'black', knowMore: 'features/dvAlexa', knowMoreText: 'Know More', textOnBackGroundColor: true },            
            { id: 3, image: 'images/CustomReport.png', title: 'Create Custom Reports for Kids Now', show: false, color: 'black', knowMore: 'features/dvCustomReport', knowMoreText: 'Know More', textOnBackGroundColor: false },
            { id: 4, image: 'images/InstantLog.png', title: 'Now you can instantly log or report kids activities using Instant Log feature', show: false, color: 'black', knowMore: 'features/dvInstantLog', knowMoreText: 'Know More', textOnBackGroundColor: true }];
        $scope.bgImage = {};
        $scope.currentIndex = 0;

        this.getData = function () {
            backgroundService.setCurrentBg("null");
            sliderFunc();
        };

        var sliderFunc = function () {
            var timer;
            timer = $timeout(function () {
                $scope.next();
                $scope.bgImage = $scope.images[$scope.currentIndex];
                document.getElementById("foo").style.backgroundImage = "url(" + $scope.bgImage.image + ")";
                timer = $timeout(sliderFunc, 3000);
            }, 3000);
        };

        $scope.prev = function () {
            $scope.currentIndex > 0 ? $scope.currentIndex-- : $scope.currentIndex = $scope.images.length - 1;
        };

        $scope.next = function () {
            $scope.currentIndex < $scope.images.length - 1 ? $scope.currentIndex++ : $scope.currentIndex = 0;
        };

        $scope.$watch('currentIndex', function () {
            $scope.images.forEach(function (image) {
                image.show = false;
            });
            $scope.images[$scope.currentIndex].show = true;
            $scope.bgImage = $scope.images[$scope.currentIndex];
        });
    }])
.controller('instantLogController', ['$scope', '$routeParams', 'backgroundService', 'reportsService', 'instantLogService', 'localStorageService',
    function ($scope, $routeParams, backgroundService, reportsService, instantLogService, localStorageService) {
        $scope.kids = {};
        $scope.messages = [];
        $scope.message = {};
        $scope.logMessage = '';
        $scope.logId = '';
        $scope.selectedKidName = 'Select Kid';
        $scope.selectedKidId = '';
        $scope.uRole = localStorageService.get('userRole');
        this.onLoad = function () {
            $scope.showSpin = true;
            backgroundService.setCurrentBg("wrapper");
            reportsService.GetKids($routeParams.id, $scope.uRole).then(function (data) {
                $scope.kids = data;
                if ($scope.kids == null)
                    $scope.warning = 'No Kids found in your day care. Go to kids section to add kids';
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
        };
        this.showLogForKid = function (kid) {
            $scope.showSpin = true;
            $scope.selectedKidName = kid.FName + " " + kid.LName;
            $scope.selectedKidId = kid.KidId;
            $scope.logId = '';
            $scope.messages = [];
            $scope.clear();
            instantLogService.GetInstantLog(kid.KidId).then(function (data) {
                if (data != null) {
                    $scope.messages = data.Messages;
                    $scope.logId = data.LogId;
                }
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showSpin = false;
               });
        };
        this.logInstantMessage = function () {
            $scope.message = {};
            $scope.message.KidId = $scope.selectedKidId;
            $scope.message.LogId = $scope.logId;
            $scope.message.Time = this.getTime();
            $scope.message.Value = $scope.logMessage;
            instantLogService.SaveMessage($scope.message).then(function (data) {
                $scope.message.LogId = data.LogId;
                $scope.message.MessageId = data.MessageId;
                $scope.message.Type = data.Type;
                $scope.messages.push($scope.message);
                $scope.clear();
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showSpin = false;
               });
        };
        $scope.clear = function () {
            $scope.message = null;
            $scope.logMessage = '';
        };
        this.getAlertClass = function (type) {
            switch (type) {
                case "Food":
                    return "form-control alert alert-success alert-dismissable";
                    break;
                case "Nap":
                    return "form-control alert alert-info alert-dismissable";
                    break;
                case "Potty":
                    return "form-control alert alert-warning alert-dismissable";
                    break;
                case "Activity":
                    return "form-control alert alert-success alert-dismissable";
                    break;
                case "Mood":
                    return "form-control alert alert-info alert-dismissable";
                    break;
                case "Sick":
                    return "form-control alert alert-warning alert-dismissable";
                    break;
                case "Misc":
                    return "form-control alert alert-info alert-dismissable";
                    break;
            }
        };
        this.getTime = function () {
            var date = new Date();
            var hours = date.getHours();
            var minutes = date.getMinutes();
            var ampm = hours >= 12 ? 'pm' : 'am';
            hours = hours % 12;
            hours = hours ? hours : 12; // the hour '0' should be '12'
            minutes = minutes < 10 ? '0' + minutes : minutes;
            var strTime = hours + ':' + minutes + ' ' + ampm;
            return strTime;
        };
    }])
.controller('loginController', ['$scope', '$location', '$routeParams', 'localStorageService', 'backgroundService', 'authService',
    function ($scope, $location, $routeParams, localStorageService, backgroundService, authService) {
        $scope.user = {};
        $scope.error = null;

        this.setBodyCss = function () {
            backgroundService.setCurrentBg("wrapper");
            $scope.success = $routeParams.message;
        };

        this.setBodyCssForAlexa = function () {
            backgroundService.setCurrentBg("null");
        };

        this.login = function () {
            $scope.showSpin = true;
            authService.Login($scope.user).then(function (data) {
                $scope.user = data;
                var returnUrl = localStorageService.get('transferUrl');
                if (returnUrl != null && returnUrl != '')
                    $location.path(returnUrl);
                else {
                    if ($scope.user.userRole == 'dayCare')
                        $location.path('/dayCare/' + $scope.user.id);
                    else if ($scope.user.userRole == 'parent')
                        $location.path('/parent/' + $scope.user.id);
                }
            },
             function (error) {
                 $scope.error = error.error_description;
             }).finally(function () {
                 $scope.showSpin = false;
             });
        };

        this.aLogin = function () {
            $scope.showSpin = true;
            authService.Login($scope.user).then(function (data) {
                var token = { AccessToken: data.access_token, Id: data.id, Role: data.userRole };
                authService.InsertToken(token).then(function (data) {
                },
             function (error) {
                 $scope.error = error.Message;
             }).finally(function () {
             });
                var state = getQueryString('state');
                window.location.replace("https://pitangui.amazon.com/spa/skill/account-linking-status.html?vendorId=MMC7RP7MEAJQ3#state=" + state + "&access_token=" + data.access_token + "&token_type=Bearer");
            },
             function (error) {
                 $scope.error = error.error_description;
             }).finally(function () {
                 $scope.showSpin = false;
             });
        };

        var getQueryString = function (field, url) {
            var href = url ? url : window.location.href;
            var reg = new RegExp('[?&]' + field + '=([^&#]*)', 'i');
            var string = reg.exec(href);
            return string ? string[1] : null;
        };
    }])
.controller('mainController', ['$scope', 'backgroundService', 'localStorageService', 'scrollService', 'authService', 'notificationService', '$location',
     function ($scope, backgroundService, localStorageService, scrollService, authService, notificationService, $location) {

         $scope.bgService = backgroundService;
         $scope.authentication = authService.Authentication;

         if ($scope.authentication.userId == undefined) {
             if (localStorageService.get('userId') != null)
                 $scope.authentication.userId = localStorageService.get('userId');
             if (localStorageService.get('userRole') != null)
                 $scope.authentication.userRole = localStorageService.get('userRole');
             if (localStorageService.get('isAuth') != null)
                 $scope.authentication.isAuth = localStorageService.get('isAuth');
             if (localStorageService.get('userName') != null)
                 $scope.authentication.userName = localStorageService.get('userName');
         }

         //check notifications
         if ($scope.authentication.isAuth) {
             notificationService.GetCount($scope.authentication.userId).then(function (data) {
                 $scope.nots = data;
             },
             function (error) {
                 $scope.error = error;
             }).finally(function () {
             });
         }

         $scope.$on('notification-read', function () {
             notificationService.GetCount($scope.authentication.userId).then(function (data) {
                 $scope.nots = data;
             },
            function (error) {
                $scope.error = error;
            }).finally(function () {
            });
         });

         this.logOut = function () {
             authService.LogOut();
             $location.path('/home');
         };
     }])
.controller('notificationsController', ['$scope', '$routeParams', 'backgroundService', 'notificationService', '$rootScope',
    function ($scope, $routeParams, backgroundService, notificationService, $rootScope) {
        $scope.nots = {};

        this.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
            notificationService.GetNotifications($routeParams.id).then(function (data) {
                $scope.nots = data;
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
        };
        this.removeNotification = function (id) {
            notificationService.DeleteNotification($routeParams.id, id).then(function (data) {
                for (var i = 0; i < $scope.nots.length; i++) {
                    if ($scope.nots[i].Id == id) {
                        $scope.nots.splice(i, 1);
                        break;
                    }
                }
                $scope.success = 'Notification marked as Read';
                $rootScope.$broadcast('notification-read');
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                });
        };
    }])
.controller('parentController', ['$scope', '$location', '$routeParams', 'backgroundService', 'parentService',
    function ($scope, $location, $routeParams, backgroundService, parentService) {
        $scope.parent = {};
        this.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
            $scope.showSpin = true;
            parentService.GetParentData($routeParams.parentId).then(function (data) {
                $scope.parent = data;
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
        };
    }])
.controller('registerController', ['$scope', '$location', 'backgroundService', 'registerService',
    function ($scope, $location, backgroundService, registerService) {
        $scope.dayCare = {};
        $scope.parent = {};
        this.setBodyCss = function () {
            backgroundService.setCurrentBg("wrapper");
        };

        this.registerDayCare = function () {
            $scope.showSpin = true;
            registerService.SignUpDayCare($scope.dayCare).then(function (data) {
                //$scope.dayCare = data;
                //$location.path('/dayCare/' + $scope.dayCare.id);
                $location.path('/login/' + 'Account Created. Please Login.');
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
        };

        this.registerParent = function () {
            $scope.showSpin = true;
            registerService.SignUpParent($scope.parent).then(function (data) {
                //$scope.parent = data;
                //$location.path('/parent/' + $scope.parent.id);
                $location.path('/login/' + 'Account Created. Please Login.');
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
        };

        this.checkParent = function () {
            $scope.showParentSpin = true;
            registerService.CheckParent($scope.parent).then(function (data) {
                $scope.success = "Your day care has added your little Giggler to the Giggles!. Please register as a Parent here.";
                $scope.parent = data;
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showParentSpin = false;
                });
        };
    }])
.controller('reportsController', ['$scope', '$location', '$routeParams', 'backgroundService', 'reportsService', 'dayCareService', 'localStorageService', 'customReportService',
    function ($scope, $location, $routeParams, backgroundService, reportsService, dayCareService, localStorageService, customReportService) {
        $scope.kids = {};
        $scope.log = {};
        $scope.selectedKid = "";
        $scope.selectedKidName = "";
        $scope.kidLog = {};
        $scope.today = '';
        $scope.foods = [{ WhatKidAte: '', WhenKidAte: '', HowKidAte: '', AnySnack: '' }];
        $scope.pottys = [{ DiaperCheckTime: '', PottyTime: '', DiaperPottyType: '' }];
        $scope.naps = [{ NapTime: '' }];
        $scope.activities = [{ ActivityTime: '', Mood: '', Activities: '' }];
        $scope.tab = 'dvFood';
        $scope.role = localStorageService.get('userRole');
        $scope.settings = localStorageService.get('settings');
        $scope.emailOptions = false;
        this.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
            $scope.showSpin = true;
            $scope.showKids = true;
            $scope.today = $scope.day;
            reportsService.GetKids($routeParams.id, localStorageService.get('userRole')).then(function (data) {
                $scope.kids = data;
                if ($scope.kids == null)
                    $scope.warning = 'No Kids found in your day care. Go to kids section to add kids';
            },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
        };

        this.go = function () {
            $scope.showSpin = true;
            this.pullKidReport($scope.selectedKid, $scope.selectedKidName);
        };

        this.pullKidReport = function (kidId, kidName) {
            $scope.selectedKid = kidId;
            $scope.selectedKidName = kidName;
            if ($scope.settings != null && $scope.settings.CustomReport) {
                customReportService.GetCustomReportOnAGivenDay(kidId, $routeParams.id, $scope.day).then(function (data) {
                    $scope.customReport = data;
                    if ($scope.day == $scope.today)
                        $scope.heading = "Today's Report for " + kidName;
                    else
                        $scope.heading = "Report for " + kidName + " on" + $scope.day;
                    if ($scope.customReport == null) {
                        $scope.subWarning = 'A report was not created for the kid today';
                        $scope.noReport = true;
                    }
                    else {
                        $scope.subWarning = '';
                        $scope.noReport = false;
                    }
                    $scope.showKidLog = true;
                    $scope.showKids = false;
                },
                   function (error) {
                       $scope.showKidLog = true;
                   }).finally(function () {
                       $scope.showSpin = false;
                   });
            }
            else {
                reportsService.PullReport(kidId, $routeParams.id, $scope.day).then(function (data) {
                    $scope.log = data;
                    if ($scope.day == $scope.today)
                        $scope.heading = "Today's Report for " + kidName;
                    else
                        $scope.heading = "Report for " + kidName + " on" + $scope.day;
                    if ($scope.log == null) {
                        $scope.subWarning = 'A report was not created for the kid today';
                        $scope.noReport = true;
                    }
                    else {
                        $scope.subWarning = '';
                        $scope.noReport = false;
                    }
                    $scope.showKidLog = true;
                    $scope.showKids = false;
                },
                   function (error) {
                       $scope.showKidLog = true;
                   }).finally(function () {
                       $scope.showSpin = false;
                   });
            }
        };
        this.createReport = function () {
            $scope.error = null;
            $scope.showKids = false;
            $scope.showKidLog = false;
            $scope.showCreateKidLog = true;
            if ($scope.settings.CustomReport) {
                customReportService.GetCustomReport($scope.selectedKid, $routeParams.id).then(function (data) {
                    if (data != null)
                        $scope.customCreateReport = data;
                    else
                        $scope.warningCustReport = "No Custom Report was created. Please go to Kids section and create a custom report.";
                },
                   function (error) {
                       $scope.error = error;
                   }).finally(function () {

                   });
            }
        };
        this.saveReport = function () {
            $scope.showSpinReport = true;
            $scope.kidLog.KidId = $scope.selectedKid;
            $scope.kidLog.Foods = $scope.foods;
            $scope.kidLog.Pottys = $scope.pottys;
            $scope.kidLog.Naps = $scope.naps;
            $scope.kidLog.Activities = $scope.activities;
            dayCareService.SaveReport($scope.kidLog).then(function (data) {
                $scope.showKidLog = false;
                $scope.showKids = true;
                $scope.showCreateKidLog = false;
                for (var i = 0; i < $scope.kids.length; i++) {
                    if ($scope.kids[i].KidId == $scope.selectedKid) {
                        $scope.kids[i].HasReportToday = true;
                        break;
                    }
                }
                resetLog();
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showSpinReport = false;
               });
        };
        this.saveCustomReport = function () {
            $scope.showCustomSpinReport = true;
            $scope.customCreateReport.KidId = $scope.selectedKid;
            $scope.customCreateReport.DayCareId = $routeParams.id;
            customReportService.SaveCustomReport($scope.customCreateReport).then(function (data) {
                $scope.showKidLog = false;
                $scope.showKids = true;
                $scope.showCreateKidLog = false;
                $scope.customCreateReport = {};
                for (var i = 0; i < $scope.kids.length; i++) {
                    if ($scope.kids[i].KidId == $scope.selectedKid) {
                        $scope.kids[i].HasReportToday = true;
                        break;
                    }
                }
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showCustomSpinReport = false;
               });
        };
        this.selectAllForEmail = function (select) {
            for (var i = 0; i < $scope.kids.length; i++)
                $scope.kids[i].sendEmail = select;
        };
        this.sendEmail = function () {
            var emailList = [];
            var emailProof = true;
            $scope.error = '';
            for (var i = 0; i < $scope.kids.length; i++) {
                if ($scope.kids[i].sendEmail) {
                    if ($scope.kids[i].HasReportToday)
                        emailList.push($scope.kids[i].KidId);
                    else {
                        emailProof = false;
                        $scope.error = $scope.kids[i].FName + " " + $scope.kids[i].LName
                            + (i > 0 && i != $scope.kids.length - 1 ? ", " : "") + $scope.error;
                    }
                }
            }
            if (emailProof) {
                if (!$scope.settings.CustomReport) {
                    reportsService.SendEmail($routeParams.id, emailList).then(function (data) {
                        $scope.success = 'Reports for today emailed to parents';
                        $scope.emailOptions = false;
                        $timeout(function () {
                            $scope.success = null;
                        }, 2000);
                    },
                        function (error) {
                            $scope.error = error;
                        }).finally(function () {
                            $scope.showSpin = false;
                        });
                }
                else {
                    reportsService.SendCustomEmail($routeParams.id, emailList).then(function (data) {
                        $scope.success = 'Reports for today emailed to parents';
                        $scope.emailOptions = false;
                        $timeout(function () {
                            $scope.success = null;
                        }, 2000);
                    },
                        function (error) {
                            $scope.error = error;
                        }).finally(function () {
                            $scope.showSpin = false;
                        });
                }
            }
            else
                $scope.error = "Please start reports for " + $scope.error.substring(0, $scope.error.length - 2);
        };
        this.resetLog = function () {
            $scope.foods = [{ WhatKidAte: '', WhenKidAte: '', HowKidAte: '', AnySnack: '' }];
            $scope.pottys = [{ DiaperCheckTime: '', PottyTime: '', DiaperPottyType: '' }];
            $scope.naps = [{ NapTime: '' }];
            $scope.activities = [{ ActivityTime: '', Mood: '', Activities: '' }];
        };
        this.addFood = function () {
            $scope.foods.push({ WhatKidAte: '', WhenKidAte: '', HowKidAte: '', AnySnack: '' });
        };

        this.addPotty = function () {
            $scope.pottys.push({ DiaperCheckTime: '', PottyTime: '', DiaperPottyType: '' });
        };

        this.addNap = function () {
            $scope.naps.push({ NapTime: '' });
        };

        this.addActivity = function () {
            $scope.activities.push({ ActivityTime: '', Mood: '', Activities: '' });
        };
    }])
.controller('settingsController', ['$scope', '$location', '$routeParams', 'backgroundService', 'settingsService', 'localStorageService', 'textAngularManager',
    function ($scope, $location, $routeParams, backgroundService, settingsService, localStorageService, textAngularManager) {
        $scope.settings = {};
        $scope.about = {};
        $scope.tab = 'dvSettings';
        this.onLoad = function () {
            backgroundService.setCurrentBg("wrapper");
            settingsService.GetSettings($routeParams.dayCareId).then(function (data) {
                $scope.settings = data;
            },
            function (error) {
                $scope.error = error;
            }).finally(function () {
                $scope.showSpin = false;
            });
        };
        this.saveSettings = function () {
            $scope.showSpin = true;
            if ($scope.tab == 'dvSettings') {
                settingsService.SaveSettings($scope.settings).then(function (data) {
                    if (localStorageService.get('settings') == null)
                        localStorageService.set('settings', $scope.settings);
                    else if (localStorageService.get('settings') != null) {
                        localStorageService.remove('settings');
                        localStorageService.set('settings', $scope.settings);
                    }
                    $location.path('/dayCare/' + $routeParams.dayCareId)
                },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
            }
            else if ($scope.tab == 'dvAbout') {
                settingsService.ManageDayCareInfo($scope.about).then(function (data) {
                    $scope.success = "Info updated successfully";
                },
                function (error) {
                    $scope.error = error;
                }).finally(function () {
                    $scope.showSpin = false;
                });
            }
        };
        this.getAboutData = function () {
            settingsService.GetDayCareInfo($routeParams.dayCareId).then(function (data) {
                if (data != null)
                    $scope.about = data;
                $scope.about.DayCareId = $routeParams.dayCareId;
            },
               function (error) {
                   $scope.error = error;
               }).finally(function () {
                   $scope.showSpin = false;
               });
        };
        $scope.fileUpload = function (ele) {
            var file = ele.files[0];
            $scope.showSpinUploading = true;
            if (file.name.lastIndexOf('.jpg') > 0 || file.name.lastIndexOf('.jpeg') > 0 || file.name.lastIndexOf('.png') > 0
                || file.name.lastIndexOf('.gif') > 0 || file.name.lastIndexOf('.bmp') > 0
                || file.name.lastIndexOf('.JPG') > 0 || file.name.lastIndexOf('.JPEG') > 0 || file.name.lastIndexOf('.PNG') > 0
                || file.name.lastIndexOf('.GIF') > 0 || file.name.lastIndexOf('.BMP') > 0) {
                if (file.size < 3145728) {
                    if (window.FormData !== undefined) {
                        var data = new FormData();
                        data.append("file", file);
                        settingsService.UploadSnap(data).then(function (data) {
                            if (data.Error != null && data.Error != '')
                                $scope.snapError = data.Error;
                            else
                                $scope.about.Snaps.push(data);
                            $scope.showSpinUploading = false;
                        },
                       function (error) {
                           $scope.error = error;
                           $scope.showSpinUploading = false;
                       }).finally(function () {
                           $scope.showSpinUploading = false;
                       });
                    }
                }
                else {
                    $scope.snapError = "Image size can't exceed 3MB!";
                    $scope.showSpinUploading = false;
                    return;
                }
            }
            else {
                $scope.snapError = "Only image files are allowed!";
                $scope.showSpinUploading = false;
                return;
            }
        };
    }])
.controller('signupController', ['$location', 'backgroundService',
    function ($location, backgroundService) {
        this.setBodyCss = function () {
            backgroundService.setCurrentBg("wrapper");
        };

        this.dayCareSignUp = function () {
            $location.path('/registerDayCare');
        };

        this.parentSignUp = function () {
            $location.path('/registerParent');
        };
    }])
.controller('sliderController', function ($scope) {
    $scope.images = [
    {
        src: 'images/iPhone1.png',
        title: 'iPhone Pic 1'
    }, {
        src: 'images/iPhone2.png',
        title: 'iPhone Pic 2'
    }
    , {
        src: 'images/iPhone3.png',
        title: 'iPhone Pic 3'
    }
    , {
        src: 'images/iPhone4.png',
        title: 'iPhone Pic 4'
    }
    , {
        src: 'images/img1.png',
        title: 'Pic 1'
    }, {
        src: 'images/img2.png',
        title: 'Pic 2'
    }
    , {
        src: 'images/img3.png',
        title: 'Pic 3'
    }, {
        src: 'images/img4.png',
        title: 'Pic 4'
    }];
})
.controller('schedulesController', function ($scope, $routeParams, schedulesService, reportsService, localStorageService) {
    $scope.heading = "Schedules";
    $scope.schedules = [];
    $scope.slots = [];
    $scope.schedule = { From: '', To: '', Desc: '', FromPlaceHolder: 'From', ToPlaceHolder: 'To' };
    $scope.buffer = { MessageId: '', Time: '', Message: '', From: '', To: '' };
    $scope.scheduleName = '';
    $scope.times = schedulesService.times;
    $scope.kids = {};
    $scope.role = localStorageService.get('userRole');
    this.onLoad = function () {
        schedulesService.GetSchedules($routeParams.dayCareId).then(function (data) {
            if (data != null)
                $scope.schedules = data;
            if ($scope.schedules == null || $scope.schedules.length == 0)
                $scope.warning = "No Schedules Present. Create One.";
        },
        function (error) {
            $scope.error = error;
        });
    };
    this.getKids = function () {
        reportsService.GetKids($routeParams.dayCareId, $scope.role).then(function (data) {
            $scope.kids = data;
            if ($scope.kids == null)
                $scope.warning = 'No Kids found in your day care. Go to kids section to add kids';
        },
            function (error) {
                $scope.error = error;
            }).finally(function () {
                $scope.showSpin = false;
            });
    };
    this.email = function (schedule, kids, emailId) {
        if (emailId == "" && (kids == null || kids.length == 0))
            $scope.error = "Please select kids or enter email.";
        else {
            var emailList = [];
            if (kids != null && kids.length > 0)
                for (var i = 0; i < kids.length; i++) {
                    emailList.push(kids[i].Email);
                }
            if (emailId != "")
                emailList.push(emailId);
            schedule.EmailList = emailList;
            schedule.DayCareName = localStorageService.get('userName');
            schedulesService.SendEmail(schedule).then(function (data) {
                $scope.success = "Emails sent successfully";
            },
            function (error) {
                $scope.error = error;
            }).finally(function () {
            });
        }
    };
    this.copyUrl = function () {
        var txtCopy = document.getElementById('txtCopy');
        txtCopy.focus();
        txtCopy.select();
        setTimeout(function () {
            document.execCommand('copy');
        }, 20);
        $scope.success = "Url copied to share.";
    };
    this.addScheduleSlot = function () {
        if ($scope.scheduleName == '' || $scope.schedule.From == '' || $scope.schedule.To == '' || $scope.schedule.Desc == '') {
            $scope.error = 'All fields are required';
            return;
        }
        $scope.slots.push($scope.schedule);
        $scope.error = '';
        $scope.schedule = { From: '', To: '', Desc: '', FromPlaceHolder: 'From', ToPlaceHolder: 'To' };
    };
    this.removeScheduleSlot = function (id) {
        $scope.slots.splice(id);
    };
    $scope.removeSchedule = function (id) {
        schedulesService.RemoveSchedule(id).then(function (data) {
            $scope.success = "Schedule deleted successfully";
            for (var i = 0; i < $scope.schedules.length; i++) {
                if ($scope.schedules[i].Id == id) {
                    $scope.schedules.splice(i, 1);
                    break;
                }
            }
        },
        function (error) {
            $scope.error = error;
        }).finally(function () {
        });
    };
    this.removeScheduleMessage = function (schedule, messageId) {
        schedulesService.RemoveScheduleMessage($routeParams.dayCareId, schedule.Id, messageId).then(function (data) {
            $scope.success = "Schedule message deleted successfully";
            for (var i = 0; i < schedule.Messages.length; i++) {
                if (schedule.Messages[i].MessageId == messageId) {
                    schedule.Messages.splice(i, 1);
                    break;
                }
            }
            if (schedule.Messages.length == 0)
                $scope.removeSchedule(schedule.Id);
        },
        function (error) {
            $scope.error = error;
        }).finally(function () {
        });
    };
    this.editScheduleSlot = function (message) {
        $scope.buffer.From = message.From;
        $scope.buffer.To = message.To;
        $scope.buffer.MessageId = message.MessageId;
        $scope.buffer.Message = message.Message;
    };
    this.saveScheduleSlot = function (message) {
        $scope.showSpinSave = true;
        schedulesService.SaveScheduleMessage($scope.buffer).then(function (data) {
            $scope.success = 'Schedule Updated Successfully.';
            message.Time = $scope.buffer.From + '-' + $scope.buffer.To;
            message.Message = $scope.buffer.Message;
            message.From = $scope.buffer.From;
            message.To = $scope.buffer.To;
            $scope.buffer = { MessageId: '', Time: '', Message: '', From: '', To: '' };
            $scope.warning = '';
        },
        function (error) {
            $scope.error = error;
        }).finally(function () {
            $scope.showSpinSave = false;
        });
    };
    this.addSchedule = function () {
        $scope.showSpin = true;
        var schedule = { Name: $scope.scheduleName, DayCareId: $routeParams.dayCareId, Messages: [] };
        for (var i = 0; i < $scope.slots.length; i++) {
            var message = { Time: $scope.slots[i].From + '-' + $scope.slots[i].To, Message: $scope.slots[i].Desc };
            schedule.Messages.push(message);
        }
        schedulesService.AddSchedule(schedule).then(function (data) {
            $scope.success = 'Schedule Added Successfully.';
            $scope.warning = '';
            $scope.schedules.push(data);
        },
        function (error) {
            $scope.error = error;
        }).finally(function () {
            $scope.showSpin = false;
        });
    };
});