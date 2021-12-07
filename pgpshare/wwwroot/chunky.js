'use strict';

var chunky = (function () {
			
	function uploadFileInChunks(file, options) {
		
		let encoder    = (options?.encoder)   ? options.encoder   : function(block) { return block; };
		let chunkSize  = (options?.chunkSize) ? options.chunkSize : 4 * 1024 * 1024;		
		let fileSize   = file.size;
		let offset     = 0;
		let part       = 0;
        let reader = new FileReader();

		reader.onload = async function (evt) {
			
			if (evt.target.error == null) {
				
				const block = new Uint8Array(evt.target.result);
				
				offset += block.length;
				
                const xhr = new XMLHttpRequest();
                xhr.open('POST', '/api/files/' + file.name + '/' + part++);
				xhr.setRequestHeader('Content-Type', 'application/octet-stream');
				xhr.onload = (oEvent) => {
					if (offset < fileSize) {
						reader.readAsArrayBuffer(file.slice(offset, chunkSize + offset));
					}
				};
											
				const encoded = await encoder(block);
				
				xhr.send(encoded);

			} else {
				console.log("Read error: " + evt.target.error);
				return;
			}
			
			if (offset >= fileSize) {
				console.log("Done reading file");
				return;
			}
		}

		// start reading the first block from the file
		reader.readAsArrayBuffer(file.slice(0, chunkSize));
	};
	
	function downloadFileInChunks(fileName, options) {
		
		const decoder    = (options?.decoder)   ? options.decoder   : function(block) { return block; };
		
		const fileStream = streamSaver.createWriteStream(fileName, {
			//size: 22, // (optional) Will show progress
			writableStrategy: undefined, // (optional)
			readableStrategy: undefined  // (optional)
		});
		
		const writer = fileStream.getWriter();
		
		const writeBlock = async function (block) {
			if (block.length == 0) {
				writer.close();
			} else {			
				let decrypted = await decoder(block);			
				writer.write(decrypted);
            }
		};
		
		const downloadBlock = async function (name, part) {
			const xhr = new XMLHttpRequest();
			xhr.open('GET', '/api/files/' + name + '/' + part);
			xhr.responseType = 'arraybuffer';
			xhr.onload = async (oEvent) => {
				let buffer = oEvent.target.response;
				if (buffer) {
					let block = new Uint8Array(buffer);
					await writeBlock(block);
					if (block.length > 0) {
						downloadBlock(name, ++part);
					} else {
						console.log("download finished");
					}
					return;
				}
			}
			xhr.send();
		};
		
		// start downloading the first block from the server
		downloadBlock(fileName, 0);
	}
	
	function uploadKeyString(userId, key, part) {
		return new Promise(function(resolve, reject) {
			const xhr = new XMLHttpRequest();
			xhr.open('POST', '/api/keys/' + userId + '/' + part);
			xhr.setRequestHeader('Content-Type', 'application/octet-stream');
			xhr.onerror = () => reject(Error("network error"));
			xhr.onload = async (oEvent) => {
				if (oEvent.target.status != 200) {
					reject(Error("http code: " + oEvent.target.status));
				} else {
					resolve(key);
				} 
			}			
			xhr.send(new TextEncoder().encode(key));
		});
	}
	
	function downloadKeyString(userId, part) {
		return new Promise(function(resolve, reject) {
			const xhr = new XMLHttpRequest();
			xhr.open('GET', '/api/keys/' + userId + '/' + part);
			xhr.responseType = 'arraybuffer';
			xhr.onerror = () => reject(Error("network error"));
			xhr.onload = async (oEvent) => {
				if (oEvent.target.status != 200) {
					reject(Error("http code: " + oEvent.target.status));
				} else {
					let buffer = oEvent.target.response;
					if (buffer) {
						let block = new Uint8Array(buffer);
						let key = new TextDecoder().decode(block);
						resolve(key);
					} else {
						reject(Error("response undefined"));
					}
				}
			}
			xhr.send();
		});
	}
	
	// public Interface
    return {
        upload: function (file, options) {
            return uploadFileInChunks(file, options);
        },		
		download: function (fileName, options) {
			return downloadFileInChunks(fileName, options);
		},
		uploadKey: function (userId, key, part) {
            return uploadKeyString(userId, key, part);
        },
		downloadKey: function (userId, part) {
            return downloadKeyString(userId, part);
        }
    };
})();