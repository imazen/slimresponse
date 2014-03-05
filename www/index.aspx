<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>SlimResponse - Effortless responsive images</title>
	<style type="text/css">
		body {
			font-size:200%;
			font-family:'Segoe UI', 'Frutiger LT 45', Arial, Helvetica, sans-serif;
			margin:5em;
			background-color:#fff;
		}

		h1
		{
			font-size:2em;
			font-weight:normal;
		}

        h2
		{
			font-size:1em;
			font-weight:normal;
		}

		h4
		{
			font-size:0.75em;
			font-weight:normal;
		}

		.autoscale-width
		{
			max-width:100%;
		}
		
		.logo
		{
			display:block;
			text-align:right;
			padding:2em;
			background-color:#ffd800;
		}
		
		.logo img
		{
			width:20%;
		}
		
		p
		{
			font-size:.5em;
			color:gray;
		}
	</style>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">		
    <script src="slimmage.js"></script>
</head>	
 
<body>
    <div>
		<h1>
            SlimResponse, ImageResizer & Slimmage for resource friendly responsive images
            <iframe src="http://ghbtns.com/github-btn.html?user=imazen&repo=SlimResponse&type=fork&count=true&size=large" allowtransparency="true" frameborder="0" scrolling="0" width="150" height="30"></iframe>
        </h1>

        <h2>Works with both, preset or width parameters which results in the same image rendered here by javascript enabled clients.</h2>

        <h4>Image with initial <em>width=100</em>, no preset</h4>
        <img src="sample.jpg?width=100&format=jpg&quality=90&slimmage=true" class="autoscale-width" alt="sample"/>

        <h4>Image with <em>preset=sample</em>, that has a default initial width of 800</h4> 
        <img src="sample.jpg?preset=sample&format=jpg&quality=90&slimmage=true" class="autoscale-width" alt="sample"/>

		<p>Stuttgart, Germany (<a href="http://www.flickr.com/photos/jan_ortmann/7147983853/">Photo by jojonks via Flickr</a>)</p>
    </div>    
</body>
 
</html>