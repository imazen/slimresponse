## SlimResponse

### Effortless reponsive images powered by [ImageResizer](http://imageresizing.net) and [Slimmage.js](https://github.com/imazen/slimmage)

Dual-licensed by Imazen under the [MIT](http://www.opensource.org/licenses/mit-license.php) and [Apache](http://www.apache.org/licenses/LICENSE-2.0.html) licenses.


###What does it to?

SlimResponse looks through outgoing HTML for `<img>` tags with a `slimmage` class applied, such as 

```html
<img class="slimmage" src="image.jpg?width=150" />
```

It then adds the appropriate markup to allow [slimmage.js](https://github.com/imazen/slimmage) to turn them into responsive images.



### Requirements
* ASP.Net 4.0
* [ImageResizer](http://imageresizing.net)
* [slimmage.js](https://github.com/imazen/slimmage).



### Spin it up quickly for a test!
* Use WebMatrix and fire it up on the "www"-folder, that's all!


Thanks to [esn303](https://twitter.com/#!/esn303) for creating [ImageResizer.ResponsivePresets](https://github.com/mindrevolution/ImageResizer-ResponsivePresets) and being the inspiration behind this project.


This project is open to collaboration. **Fork. Push. Innovate.**
