## SlimResponse

### Effortless reponsive images powered by [ImageResizer](http://imageresizing.net) and [Slimmage.js](https://github.com/imazen/slimmage)

Dual-licensed by Imazen under the [MIT](http://www.opensource.org/licenses/mit-license.php) and [Apache](http://www.apache.org/licenses/LICENSE-2.0.html) licenses.

`Install-Package Imazen.SlimResponse`

(Well, actually, you may also need to run `Install-Package HtmlAgilityPack`) The current release forgot to specify that dependency.

###What does it to?

SlimResponse looks through outgoing HTML for `<img>` tags with a `slimmage` class applied, such as 

```html
<img class="slimmage" src="image.jpg?width=150" />

<img class="thisclass slimmage thatclass" src="image.jpg?width=150" />
```

or for "slimmage=true" in any image URL

```html
<img src="image.jpg?width=150&slimmage=true" />
```

**Slimmage requires "width=[number]" be present in the URL. This value specifies the image size when javascript is disabled, but is modified by slimmage.js under normal circumstances. If you prefer to use ImageResizer presets, you can omit the width parameter if your specified preset provides a width.**


It then adds the appropriate markup to allow [slimmage.js](https://github.com/imazen/slimmage) to turn them into responsive images.



### Requirements
* ASP.Net 4.0
* [ImageResizer](http://imageresizing.net) must be [installed](http://imageresizing.net/docs/install)
* [slimmage.js](https://github.com/imazen/slimmage) must be included in your page's javascript. 

### Spin it up quickly for a test!
* Use WebMatrix or VisualStudio (Open Web Site) and fire it up on the "www"-folder, that's all!

### Notes & Caveats 

* This parses all outgoing HTML. This may increase CPU usage slightly. 
* Outgoing HTML is only modified if a responsive image is present.
* SlimResponse may mess up invalid HTML even further during parsing/serialization.
* If you're only using responsive images in limited situations, an HTML helper that spits out [slimmage markup](https://github.com/imazen/slimmage) may be more appropriate.
* SlimResponse does not include the [slimmage.js](https://github.com/imazen/slimmage) javascript file, that's your responsibility.

### Your web.config file must have both ImageResizer and SlimResponse installed

```xml
<?xml version="1.0"?>
<configuration>
  <system.web>
    <httpModules>
      <!-- This is for IIS5, IIS6, and IIS7 Classic, and Cassini/VS Web Server-->
      <add name="ImageResizingModule" type="ImageResizer.InterceptModule"/>
      <add name="SlimResponseModule" type="Imazen.SlimResponse.SlimResponseModule, Imazen.SlimResponse"/>
    </httpModules>
  </system.web>
  <system.webServer>
    <modules runAllManagedModulesForAllRequests="true">
      <!-- IIS7+ Integrated mode -->
      <add name="ImageResizingModule" type="ImageResizer.InterceptModule"/>
      <add name="SlimResponseModule" type="Imazen.SlimResponse.SlimResponseModule, Imazen.SlimResponse"/>
    </modules>
  </system.webServer>
</configuration>
```

Thanks to [esn303](https://twitter.com/#!/esn303) for creating [ImageResizer.ResponsivePresets](https://github.com/mindrevolution/ImageResizer-ResponsivePresets) and being the inspiration behind this project.

This project is open to collaboration. **Fork. Push. Innovate.**



