let H5P = null;

export async function addh5pcontent(H5PID) {
    const el = document.getElementById('h5p-container');
    const options = {
        h5pJsonPath: '/_content/SharedComponents/H5PContent/public/' + H5PID,
        frameJs: '/_content/SharedComponents/js/h5p-standalone/frame.bundle.js',
        frameCss: '/_content/SharedComponents/js/h5p-standalone/styles/h5p.css',
        //optional:
        frame: true,
        copyright: true,
        export: true,
        downloadUrl: '/_content/SharedComponents/H5PContent/download/' + H5PID + '.h5p',
        fullScreen: true,

    }

    if (H5P === null) {
        // save H5P to variable, because main.bundle.js is loaded only once, and not again after navigating to another page and back
        H5P = H5PStandalone.H5P; //H5PStandalone.H5P taken from wwwroot/js/h5p-standalone/main.bundle.js loaded in index.html
    }
    await new H5P(el, options);
}