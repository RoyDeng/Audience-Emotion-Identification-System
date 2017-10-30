Bob.binders.registerBinder("buildImage", function (node,
         onchange, onadd, onremove) {

    return {
        updateProperty: function (value) {

            console.log(value, node);

            var ctx = node.getContext("2d");

            node.width = 640;
            node.height = 480;

            var image = new Image();
            image.onload = function () {
                ctx.drawImage(image, 0, 0);
                ctx.globalAlpha = 0.2;
                value.emotionResults.forEach(function (result) {
                    ctx.fillStyle = result.color;
                    ctx.fillRect(result.FaceRectangle.Left, result.FaceRectangle.Top, result.FaceRectangle.Width, result.FaceRectangle.Height);
                });
            }
            image.src = value.url;





        }
    };
});

Bob.binders.registerBinder("background-color", function (node,
    onchange, onadd, onremove) {

    return {
        updateProperty: function (value) {

            node.style.backgroundColor = value;


        }
    };
});

Bob.binders.registerBinder("percent", function (node,
   onchange, onadd, onremove) {

    return {
        updateProperty: function (value) {
            node.textContent = (parseFloat(value) * 100).toFixed(2) + "%";

        }
    };
});
