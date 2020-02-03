# Train Problem

## Design
My design uses 4 layers: Domain, Data, Service and UI. 

__Domain__: I abstracted the repositories and put the models into the domain so that the UI would only need to rely on the service and domain layers to keep coupling light.

__Data__: The data layer includes the concrete classes for Repositories. For purposes of this problem, I used these concretes types to hold the actual data instead of spinning up a database.

__Service__: The service layer hold the meat of the project and includes pathfinding algorithms and solves the presented problems (inputs/outputs). The assumptions of the service calls are either a success or an thrown exception that will be caught and handled in the UI layer. I did not incoporate a logger (which I would normally do in a real setting so we can track exceptions, warnings and other actions, etc.) I did use a third party library for some of the core pathfinding functionality (QuickGraph).

__UI__: This layer is server-side Blazor with links that take the user to the types of things they may want to know. In this instance, each page solves one or more of the problem outputs (there are link descriptors on the index page to direct you to which page you can expect to find your inputs/outputs)

## Assumptions
* I took the "use a directed graph to represent the train routes" as something I was doing behind the scenes, not necessarily something that had to be rendered with graphics on the screen. Screen rendered routes will appear as shown in Output 10 when they do need to be shown on screen.
* A trip consists of an origin and a destination as well as a single route.
* A route consists of one or more stops that takes a person from the trip origin to the trip destination.
