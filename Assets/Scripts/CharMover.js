 
private var jumpSpeed:float = 8.0;
private var gravity:float = 20.0;
private var runSpeed:float = 5.0;
private var walkSpeed:float = 1.0;
private var rotateSpeed:float = 150.0;
 
private var grounded:boolean = false;
private var moveDirection:Vector3 = Vector3.zero;
private var isWalking:boolean = false;
private var moveStatus:String = "idle";
 
function Update ()
{
    // Only allow movement and jumps while grounded
    if(grounded) {
        moveDirection = new Vector3((Input.GetMouseButton(1) ? Input.GetAxis("Horizontal") : 0),0,Input.GetAxis("Vertical"));
       
        // if moving forward and to the side at the same time, compensate for distance
        // TODO: may be better way to do this?
        if(Input.GetMouseButton(1) &&  Input.GetAxis("Horizontal") && Input.GetAxis("Vertical")) {
            moveDirection *= .7;
        }
       
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= isWalking ? walkSpeed : runSpeed;
       
        moveStatus = "idle";
        if(moveDirection != Vector3.zero)
            moveStatus = isWalking ? "walking" : "running";
       
        // Jump!
        if(Input.GetButton("Jump"))
            moveDirection.y = jumpSpeed;
    }
   
    // Allow turning at anytime. Keep the character facing in the same direction as the Camera if the right mouse button is down.
    if(Input.GetMouseButton(1)) {
        transform.rotation = Quaternion.Euler(0,Camera.main.transform.eulerAngles.y,0);
    } else {
        transform.Rotate(0,Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0);
    }
   
    if(Input.GetMouseButton(1) || Input.GetMouseButton(0))
        Screen.lockCursor = true;
    else
        Screen.lockCursor = false;
   
    // Toggle walking/running with the T key
    if(Input.GetKeyDown("t"))
        isWalking = !isWalking;
   
    //Apply gravity
    moveDirection.y -= gravity * Time.deltaTime;
   
    //Move controller
    var controller:CharacterController = GetComponent(CharacterController);
    var flags = controller.Move(moveDirection * Time.deltaTime);
    grounded = (flags && CollisionFlags.Below) != 0;
}
 
@script RequireComponent(CharacterController)
 