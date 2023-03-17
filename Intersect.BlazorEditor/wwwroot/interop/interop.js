function rejectNotYetImplemented() {
	return Promise.reject('Not yet implemented!');
}

class Modal {
	/**
	 * @param {string} id
	 * @param {string} [returnValue]
	 */
	close(id, returnValue) {
		/** @type {HTMLDialogElement} */
		const dialog = document.querySelector(`dialog#${id}`);
		dialog?.close(returnValue);
	}

	/**
	 * @param {string} id
	 */
	show(id) {
		/** @type {HTMLDialogElement} */
		const dialog = document.querySelector(`dialog#${id}`);
		dialog?.showModal();
	}
}

window.Interop = class Interop {
	constructor() {
		this.modal = new Modal();
	}
	
	close() {
		window.close();
		return Promise.resolve();
	}
	
	open(url) {
		window.open(url, '_blank', 'noopener');
	}

	getRuntimeVersion() {
		return rejectNotYetImplemented();
	}
	
	sendNotification(message) {
		return rejectNotYetImplemented();
	}
}

function initializeInterop(interopName) {
	const url = new URL(location.href);
	const interop = url.searchParams.get('interop');
	const interopClass = interop ? Interop[interop] : window[interopName];
	window.interop = new interopClass();
}
