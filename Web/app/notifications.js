app.factory('notifications', function () {
	toastr.options = {
		"showMethod": 'fadeIn',
		"hideMethod": 'fadeOut',
		"closeButton": true
	};
	return {
		success: function (message, title) {
			toastr.success(message, title);
		},
		error: function (message, title) {
			toastr.error(message, title);
		},
		info: function (message, title) {
			toastr.info(message, title);
		},
		warning: function (message, title) {
			toastr.warning(message, title);
		}
	};
});