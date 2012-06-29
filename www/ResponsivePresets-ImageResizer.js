(function(win){
  var responsivePresets = win.responsivePresets || {},

    doc = window.document,
    docHead = doc.getElementsByTagName("head")[0],
    screenWidth = win.screen.availWidth,
    cookieName = "responsive-width" /* if you change this, you need to change server-side the code too! */,

	setCookie = function()
    {
        // - cookie for subsequent requests (featuring some dirty string concatenation...)
	    expdate = new Date();
        // - cookie is only temporary, for 60 minutes
        expdate.setTime(expdate.getTime()+(60*1000*60));
        doc.cookie = cookieName + "=" + screenWidth + ";expires=" + expdate.toUTCString() +"; path=/";
    },

    getCookieValue = function()
    {
        var cookies = doc.cookie.split(";");
        for (var x=0; x<cookies.length; x++)
        {
            if (cookies[x].substr(0,cookies[x].indexOf("=")).replace(/^\s+|\s+$/g,"") == cookieName)
            {
                return cookies[x].substr(cookies[x].indexOf("=")+1);
            }
        }
    },

    // - replace preset names
	replaceImages = function()
    {
        for (var x=0, imgs=doc.getElementsByTagName("img"), len = imgs.length; x < len; x++){
		    var img = imgs[x];
            var src = img.getAttribute("src");
            var hasNonCookiePreset = (src.toLowerCase().indexOf("preset=..") != -1);
            var canCookie = (getCookieValue()!=undefined && getCookieValue()!="");

            // - only if "can cookie"
			if(hasNonCookiePreset && canCookie)
            {
				img.src = img.src.replace(/preset\=\.\./gi, "preset=.");
			}
		}
	},

	loadCallback = function()
    {
        // - set cookie
        setCookie();

        // - replace images
        replaceImages();
	};
  
	// - standard W3C events 
	if (doc.addEventListener)
    {
		win.addEventListener("load", loadCallback, false);
	}
	// - IE event model
	else if (doc.attachEvent)
    {
		win.attachEvent("onload", loadCallback);
	}
})(this);