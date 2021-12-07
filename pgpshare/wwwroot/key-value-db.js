var KeyValueDB = (function () {
	
	const dbName    = 'KeyValueDB';
	const dbVersion = 1;
	const storeName = 'KeyValueStore';
		
	const dbconnect = window.indexedDB.open(dbName, dbVersion);
	dbconnect.onupgradeneeded = ev => {
	  const db = ev.target.result;
	  const store = db.createObjectStore(storeName, { keyPath: 'key', autoIncrement: false });
	  //store.createIndex('name', 'email', { unique: false });
	}
	
	dbconnect.onsuccess = ev => {
	  console.log(dbName + ' ready');
	}
	
	dbconnect.onerror = ev => {
	  console.log(dbName + ' failed');
	}
		
	function insertOrUpdateItem(key, value) {
		return new Promise(function(resolve, reject) {
			const dbconnect = window.indexedDB.open(dbName, dbVersion);
			dbconnect.onerror = reject;
			dbconnect.onsuccess = ev => {				
				const db = ev.target.result;
				const transaction = db.transaction(storeName, 'readwrite');
				const store = transaction.objectStore(storeName);
				const request = store.delete(key);
				request.onerror = reject;
				request.onsuccess = ev => {
					store.add({key: key, value: value});
					transaction.onerror = reject;
					transaction.oncomplete = ev => { resolve(value); };					
				};
			}
		});
	}
	
	function readItem(key) {
		return new Promise(function(resolve, reject) {
			const dbconnect = window.indexedDB.open(dbName, dbVersion);
			dbconnect.onerror = reject;
			dbconnect.onsuccess = ev => {
				const db = ev.target.result;
				const transaction = db.transaction(storeName, 'readonly');
				const store = transaction.objectStore(storeName);
				const request = store.get(key);
				request.onerror = reject;
				request.onsuccess = ev => resolve(request.result?.value);
			}
		});
	}
	
	function readAllItems() {
		return new Promise(function(resolve, reject) {
			const dbconnect = window.indexedDB.open(dbName, dbVersion);
			dbconnect.onerror = reject;
			dbconnect.onsuccess = ev => {
				const db = ev.target.result;
				const transaction = db.transaction(storeName, 'readonly');
				const store = transaction.objectStore(storeName);
				const query = store.openCursor();
				query.onerror = reject;
				let data = [];
				query.onsuccess = ev => {
					const cursor = ev.target.result;
					if (cursor) {
						data.push(cursor.value);
						cursor.continue();
					} else {
						resolve(data);
					}
				};
			}
		});
	}
	
	function deleteItem(key) {
		return new Promise(function(resolve, reject) {
			const dbconnect = window.indexedDB.open(dbName, dbVersion);
			dbconnect.onerror = reject;
			dbconnect.onsuccess = ev => {
				const db = ev.target.result;
				const transaction = db.transaction(storeName, 'readwrite');
				const store = transaction.objectStore(storeName);
				const request = store.delete(key);
				request.onerror = reject;
				request.onsuccess = ev => resolve(true);
			}
		});
	}
	
	// public interface
	return {
		set: function(key, value) {
			return insertOrUpdateItem(key, value);
		},
		get: function(key) {
			return readItem(key);
		},
		remove: function(key) {
			return deleteItem(key);
		},
		all: function() {
			return readAllItems();
		}

	}
})();