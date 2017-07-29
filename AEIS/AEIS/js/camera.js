
navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || navigator.msGetUserMedia;
attachMediaStream = function (element, stream) {
    if (typeof element.srcObject !== 'undefined') {
        element.srcObject = stream;
    } else if (typeof element.mozSrcObject !== 'undefined') {
        element.mozSrcObject = stream;
    } else if (typeof element.src !== 'undefined') {
        element.src = URL.createObjectURL(stream);
    } else {
        console.log('Error attaching stream to element.');
    }
};
var Camera = (function () {

    var ctor = function (srm, w, h, canvas) {
        var self = this;
        this.canvas = canvas;
        this.canvas.width = w || 320;
        this.canvas.height = h || 180;
        this.video = document.createElement("video");
        this.video.autoplay = true;
        this.video.width = w || 320;
        this.video.height = h || 180;
        attachMediaStream(this.video, srm);
        this.context = this.canvas.getContext('2d');
        this.interval = -1;

        window.setInterval(function () {
            self.context.drawImage(self.video, 0, 0, self.canvas.width, self.canvas.height);
        }, 1000 / 25);
        this.rectangles = [];

    };

    ctor.prototype.takePicture = function (fn, ms) {
        if (!ms) {
            this.context.drawImage(this.video, 0, 0, this.canvas.width, this.canvas.height);
            this.canvas.toBlob(function (blob) {
                fn(blob, URL.createObjectURL(blob));
            });
        } else {
            var that = this;
            this.interval = window.setInterval(function () {
                that.context.drawImage(that.video, 0, 0, that.canvas.width, that.canvas.height);
                that.canvas.toBlob(function (blob) {
                    fn(blob, URL.createObjectURL(blob));
                });
            }, ms);
        }

    };

    return ctor;

})();