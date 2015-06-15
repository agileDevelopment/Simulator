// these define the flock's behavior
var randomness : float 		= 1; 
var prefab : GameObject; 
var chasee : GameObject;

var flockCenter : Vector3;
var flockVelocity : Vector3;

private var boids;

	boids = new Array(flockSize);
	for (var i=0; i<flockSize; i++) {
		boid.GetComponent("Boid Flocking").setController(gameObject);
		boids[i] = boid;
	}
   	var theCenter = Vector3.zero;
   	for (var boid : GameObject in boids) {
		theCenter       = theCenter + boid.transform.localPosition;
   	}
	flockCenter = theCenter/(flockSize);	
	flockVelocity = theVelocity/(flockSize);
}