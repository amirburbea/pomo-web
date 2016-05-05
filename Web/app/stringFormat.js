var module = angular.module('stringFormat', []).filter('stringFormat', [
	'$filter', function($filter) {
		var filter = $filter('number');

		function formatNumber(number, format) {
			var parens = format[0] == '(' && format[format.length - 1] == ')';
			if (parens) {
				number = Math.abs(number);
				format = format.substring(1, format.length - 1);
			}
			var pct = format[format.length - 1] == '%';
			if (pct) {
				var escape = [format.length - 2] == '\\';
				if (!escape) {
					number *= 100;
				}
				format = format.substring(0, format.length - (escape ? 2 : 1));
			}
			var text;
			switch (format) {
				case '#,##0':
					text = filter(Math.round(number), 0);
					break;
				case '#,##0.0':
					text = filter(number, 1);
					break;
				case '#,##0.00':
					text = filter(number, 2);
					break;
                case '#,##0.0000':
					text = filter(number, 4);
                    break;
				default:
					text = number.toString();
					break;
			}
			if (pct) {
				text += '%';
			}
			return parens ? '(' + text + ')' : text;
		}

		return function(value, format) {
			if (format && (value === 0 || value === '0' || value)) {
				var isNumber = typeof value === "number";
				if (isNumber || typeof value === "string" && isNumeric(value)) {
					var number = isNumber ? value : parseFloat(value);
					var indexOfSemicolon = format.indexOf(';');
					if (indexOfSemicolon == -1) {
						return formatNumber(
							number,
							format
						);
					}
					var positiveFormat = format.substring(0, indexOfSemicolon);
					if (number > 0) {
						return formatNumber(
							number,
							positiveFormat
						);
					}
					var indexOfSecondSemicolon = format.indexOf(';', indexOfSemicolon + 1);
					if (indexOfSecondSemicolon == -1) {
						return formatNumber(
							number,
							number === 0 ? positiveFormat : format.substring(indexOfSemicolon + 1)
						);
					}
					return formatNumber(
						number,
						number === 0 ? format.substring(indexOfSecondSemicolon + 1) : format.substring(indexOfSemicolon + 1, indexOfSecondSemicolon - indexOfSemicolon - 1)
					);
				}
			}
			return value;
		};
	}
]);