// these define the flock's behaviorvar minVelocity : float 	= 5;var maxVelocity : float 	= 20;
var randomness : float 		= 1; var flockSize : int			= 20;
var prefab : GameObject; 
var chasee : GameObject;

var flockCenter : Vector3;
var flockVelocity : Vector3;

private var boids;
function Start() {
	boids = new Array(flockSize);
	for (var i=0; i<flockSize; i++) {		var position = Vector3(						Random.value*collider.bounds.size.x,						Random.value*collider.bounds.size.y,						Random.value*collider.bounds.size.z)-collider.bounds.extents;		var boid = Instantiate(prefab, transform.position, transform.rotation);		boid.transform.parent = transform;		boid.transform.localPosition = position;
		boid.GetComponent("Boid Flocking").setController(gameObject);
		boids[i] = boid;
	}}function Update () {   
   	var theCenter = Vector3.zero;   	var theVelocity = Vector3.zero;
   	for (var boid : GameObject in boids) {
		theCenter       = theCenter + boid.transform.localPosition;		theVelocity     = theVelocity + boid.rigidbody.velocity;
   	}
	flockCenter = theCenter/(flockSize);	
	flockVelocity = theVelocity/(flockSize);
}
