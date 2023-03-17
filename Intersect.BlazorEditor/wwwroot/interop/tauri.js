window.TauriInterop = window.Interop.tauri = class TauriInterop extends window.Interop {
	constructor() {
		super();

		this._tauri = window.__TAURI__;
	}

	getRuntimeVersion() {
		const { app } = this._tauri;
		return app.getTauriVersion();
	}

	async sendNotification(message) {
		let permissionGranted = await notification.isPermissionGranted();

		if (!permissionGranted) {
			const permission = await notification.requestPermission();
			permissionGranted = permission === 'granted';
		}
		
		if (!permissionGranted) {
			return Promise.reject('No permission');
		}

		await notification.sendNotification(message);
	}
}
