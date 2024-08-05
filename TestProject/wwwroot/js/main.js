document.addEventListener("DOMContentLoaded", () => {
  window.addEventListener("popstate", fetchFilesForCurrentDir);
  fetchFilesForCurrentDir();
});

function fetchFilesForCurrentDir() {
  const path = window.location.pathname;//.replace(/\/$/, "");
  fetchFiles(path);
}

function fetchFiles(currentDir) {
  console.log("fetching files...")
  console.log("  dir=" + currentDir);
  const path = "/api/dir";
  // const path = `/api/dir?dir=${encodeURIComponent(currentDir)}`;

  // Could not for the life of me get the GET + query params to work, so just switched to POST

  fetch(path, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      dirname: currentDir,
    })
  })
    .then(response => response.status === 200 ? response.json() : Promise.reject('error retrieving files'))
    .then(result => {
      const fileList = document.getElementById("fileList");
      fileList.innerHTML = "";

      if (currentDir == null || currentDir === "" || currentDir === "/" || currentDir === "undefined") {
        href = "";
        topLevel = true;
        let dirCount = document.getElementById("current-dir")
        if (dirCount != null) {
          dirCount.innerText = "/";
        }
      } else {
        href = currentDir;
        topLevel = false;
        let dirCount = document.getElementById("current-dir")
        if (dirCount != null) {
          dirCount.innerText = currentDir;
        }

        const li = document.createElement("li");
        li.className = "file-item dir-item";
        // TODO: trailing slashes here cause issues.
        // Not including them makes this jump up one extra dir level; need to use href="." for this case
        // Including them, this works beautifully
        li.innerHTML = `
            <span class="file-name"><a href="..">..</a></span>
        `;
        fileList.appendChild(li);
      }

      // update file and dir count
      let fileCount = document.getElementById("file-count")
      if (fileCount != null) {
        fileCount.innerText = "Files: " + result.files.length;
      }
      let dirCount = document.getElementById("dir-count")
      if (dirCount != null) {
        dirCount.innerText = "Directories: " + result.dirs.length;
      }

      result.files.forEach(file => {
        const li = document.createElement("li");
        li.className = "file-item";
        uploadDate = file.uploadDate.substring(0, 10);
        filePath = topLevel ? file.fileName : result.dirName + "/" + file.fileName;
        console.log("file path: " + filePath)
        li.innerHTML = `
          <span class="file-name">${file.fileName}</span>
          <span class="file-size">${file.length} (Bytes)</span>
          <span class="file-date">uploaded ${uploadDate}</span>
          <button class="button button-delete" onclick="deleteFile(\'${file.fileName}\')">Delete</button>
          <button class="button button-download" onclick="downloadFile(\'${file.fileName}\')">Download</button>
          <button class="button button-download" onclick="moveFile(\'${filePath}\')">Move</button>
          <button class="button button-download" onclick="copyFile(\'${filePath}\')">Copy</button>
        `;
        fileList.appendChild(li);
      });

      result.dirs.forEach(dir => {
        const li = document.createElement("li");
        li.className = "file-item dir-item";
        li.innerHTML = `
            <span class="file-name"><a href="${href}/${dir.dirName}">Directory: ${dir.dirName}</a></span>
        `;
        fileList.appendChild(li);
      })
    });
}


function uploadFile() {
  const fileInput = document.getElementById("fileInput");
  const file = fileInput.files[0]; // only one file at a time right now
  if (!file) {
    return
  };

  // Create a new form so we don't have to in the HTML
  const formData = new FormData();
  formData.append("dir", window.location.pathname);
  formData.append("file", file);

  fetch("/api/dir/upload", {
    method: "POST",
    body: formData,
  })
    .then(response => response.status === 200 ? response.json() : Promise.reject('error uploading file'))
    .then(response => {
      console.log(response)
      fetchFiles();
      alert(`File ${response.fileName} uploaded successfully!`);
    })
    .catch(error => console.error("Error uploading file: ", file, error));
}


function downloadFile(fileName) {
  console.log('download()');

  fetch("/api/dir/download", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      "filepath": fileName,
    })
  })
    .then(response => response.status === 200 ? response.blob() : Promise.reject('error downloading file'))
    .then(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.style.display = 'none';
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      alert(`File ${fileName} downloaded successfully!`);
    })
    .catch(error => console.error("Error downloading file: ", fileName, error));
}


function createDir() {
  console.log("createDir()");
  let dirName = prompt("Directory name?");
  fetch("/api/dir/create", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      "dirname": dirName,
    })
  })
    .then(response => response.status === 200 ? response.json() : Promise.reject('error creating directory'))
    .then(response => {
      fetchFiles();
      alert("Successfully created directory: " + dirName);
    })
    .catch(error => console.error("Error creating directory: " + dirName));
}


function moveFile(filePath) {
  console.log("moveFile()");
  let newPath = prompt("New file path?");

  fetch("/api/dir/move", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      "filePath": filePath,
      "newPath": newPath,
    })
  })
    .then(response => response.status === 200 ? response.json() : Promise.reject('error moving file'))
    .then(response => {
      fetchFiles();
      alert("Successfully moved file: " + newPath);
    })
    .catch(error => console.error("Error moving file: " + newPath));
}


function copyFile(filePath) {
  console.log("copyFile()");
  let newPath = prompt("New file path?");

  fetch("/api/dir/copy", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      "filePath": filePath,
      "newPath": newPath,
    })
  })
    .then(response => response.status === 200 ? response.json() : Promise.reject('error copying file'))
    .then(response => {
      fetchFiles();
      alert("Successfully copied file: " + newPath);
    })
    .catch(error => console.error("Error copying file: " + newPath));
}


function deleteFile(fileName) {
  console.log("delete()");

  fetch("/api/dir/delete", {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      "filepath": fileName,
    })
  })
    .then(response => {
      if (response.ok && !response.error) {
        fetchFiles();
        alert(`File ${fileName} deleted successfully!`);
      } else {
        alert("Error deleting file: " + response.error);
      }
    })
    .catch(error => console.error("Error deleting file: ", fileName, error));
}

