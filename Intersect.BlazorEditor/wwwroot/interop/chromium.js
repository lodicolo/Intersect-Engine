window.ChromiumInterop = window.Interop.chromium = class ChromiumInterop extends window.Interop {
	getRuntimeVersion() {
		return Promise.resolve(navigator.userAgent);
	}

	async sendNotification(message) {
		if (Notification.permission === 'denied') {
			return Promise.reject('No permission');
		}

		const permission = await Notification.requestPermission();
		if (permission === 'denied') {
			return Promise.reject('No permission');
		}

		if (permission === 'default') {
			return Promise.reject('Error requesting permission');
		}

		const notification = new Notification('Testing', {
			body: message,
		});
		notification.close();
	}
}
