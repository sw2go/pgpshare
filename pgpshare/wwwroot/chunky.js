'use strict';

var chunky = (function () {
			
	function uploadFileInChunks(file, options) {
		return new Promise(function(resolve, reject) {			
			try {
				
				let encoder    = (options && options.encoder) ? options.encoder : function(block) { return block; };
				let chunkSize  = (options && options.chunkSize) ? options.chunkSize : 4 * 1024 * 1024;		
				let fileSize   = file.size;
				let offset     = 0;
				let part       = 0;
				let reader = new FileReader();

				reader.onload = async function (evt) {
					
					if (evt.target.error == null) {
						try {

							const block = new Uint8Array(evt.target.result);
						
							offset += block.length;
							
							const xhr = new XMLHttpRequest();
							xhr.open('POST', '/api/files/' + file.name + '/' + part++);
							xhr.setRequestHeader('Content-Type', 'application/octet-stream');
							xhr.onerror = () => reject(Error("network error"));
							xhr.onload = (oEvent) => {
								if (oEvent.target.status != 200) {
									reject(Error(file.name + "/" + part + " http " + oEvent.target.status));								
								} else {
									if (offset < fileSize) {
										reader.readAsArrayBuffer(file.slice(offset, chunkSize + offset));
									} else {
										console.log("Done reading file");
										resolve(true);
									}
								}
							};
			
							const encoded = await encoder(block);
							
							xhr.send(encoded);
							
						} catch(error) {
							reject(error);
						}
						
					} else {					
						console.log("Read error: " + evt.target.error);
						reject(evt.target.error);
					}
				};

				// start reading the first block from the file
				reader.readAsArrayBuffer(file.slice(0, chunkSize));

			} catch(error) {
				reject(error);
			}		
		});		
	};
	
	function downloadFileInChunks(fileName, options) {
		return new Promise(function(resolve, reject) {			
			try {
				
				const decoder = (options && options.decoder) ? options.decoder : function(block) { return block; };
				
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
					xhr.onerror = () => reject(Error("network error"));
					xhr.onload = async (oEvent) => {
						if (oEvent.target.status != 200) {
							reject(Error(name + "/" + part + " http " + oEvent.target.status));					
						} else {
							let buffer = oEvent.target.response;
							if (buffer) {
								try {
									let block = new Uint8Array(buffer);
									await writeBlock(block);
									if (block.length > 0) {
										downloadBlock(name, ++part);
									} else {
										resolve(true);
									}
								} catch (error) {
									reject(error);
								}
							} else {
								reject(Error("response undefined"));
							}
						}
					}
					xhr.send();
				};
				
				// start downloading the first block from the server
				downloadBlock(fileName, 0);				
				
			} catch (error) {
				reject(error);
			}			
		});
	}
	
	function uploadString(folder, file, text) {
		return new Promise(function(resolve, reject) {
			const xhr = new XMLHttpRequest();
			xhr.open('POST', '/api/keys/' + folder + '/' + file);
			xhr.setRequestHeader('Content-Type', 'application/octet-stream');
			xhr.onerror = () => reject(Error("network error"));
			xhr.onload = async (oEvent) => {
				if (oEvent.target.status != 200) {
					reject(Error(folder + "/" + file + " http " + oEvent.target.status));
				} else {
					resolve(text);
				} 
			}			
			xhr.send(new TextEncoder().encode(text));
		});
	}
	
	function downloadString(folder, file) {
		return new Promise(function(resolve, reject) {
			const xhr = new XMLHttpRequest();
			xhr.open('GET', '/api/keys/' + folder + '/' + file);
			xhr.responseType = 'arraybuffer';
			xhr.onerror = () => reject(Error("network error"));
			xhr.onload = async (oEvent) => {
				if (oEvent.target.status != 200) {
					reject(Error(folder + "/" + file + " http " + oEvent.target.status));
				} else {
					let buffer = oEvent.target.response;
					if (buffer) {
						let block = new Uint8Array(buffer);
						let text = new TextDecoder().decode(block);
						resolve(text);
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
        uploadFile: function (file, options) {
            return uploadFileInChunks(file, options);
        },		
		downloadFile: function (fileName, options) {
			return downloadFileInChunks(fileName, options);
		},
		uploadString: function (folder, file, text) {
            return uploadString(folder, file, text);
        },
		downloadString: function (folder, file) {
            return downloadString(folder, file);
        }
    };
})();