using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WALL_STATE
{
    NONE,
    CORNER,
    WALL
}
public class PlatformCharacterTest : CustomBehaviour
{
    
    protected override void Awake()
    {
        UpdateManager.register(this);
    }

    protected override void OnDestroy()
    {
        UpdateManager.unregister(this);
    }

    #region _STATES_

    //States
    int IDLE;
    int RUN;
    int JUMP;
    int FREE_FALL;
    int CLIMB_CORNER;
    int WALL_ADHERE;
    int WALL_JUMP;
    int SWING;
    int BALL_SHOOT_GROUND;
    int HIT_GROUND;
    int CHANGE_BALL_DIRECTION;
    #endregion

    #region _MEMBERS_

    //Members

    StateMachine m_stateMachine;
    Rigidbody2D m_rigidBody;
    BoxCollider2D m_boxCollider;


    public float runSpeed;
    public float jumpVelocity;

    public float groundCheckDistance;
    public float wallCheckDistance;
    public float ballGroundedThreshold;
    bool m_isGrounded;

    private DIRECTION m_spriteDirection;
    private DIRECTION m_movementDirection;
    private Vector3 m_defaultScale;
    private float m_defaultGravityScale;

    private Animator m_animator;
    private float m_stateTimeStamp;

    public float airMovementModifier;
    public float wallAdhereDuration;
    public float wallJumpDuration;
    //can't wall adhere again unless touched the ground or wall jumped
    private bool m_canWallAdhere;
    private bool m_canGoToBallMode = false;

    private GameObject m_swingObject = null;
    public List<GameObject> m_swingObjects;
    private DistanceJoint2D m_swingDistanceJoint;

    public Rope rope;
    public GameObject hand;
    public Ball ball;

    public Vector2 simpleDribbleForce;
    public Vector2 groundShootForce;
    public float additionalVerticalShootForce;
    public float ballProximityThreshold;

    public float swingAcceleration;
    public Vector3 ballRestDistance;

    public HitChecker hitChecker;
    private float m_currentOutputDamage = 0.0f;
    #endregion

    #region _INIT_
    //Init
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_boxCollider = GetComponent<BoxCollider2D>();
        m_stateMachine = new StateMachine();
        m_animator = GetComponent<Animator>();
        setupStateMachine();
        m_defaultGravityScale = m_rigidBody.gravityScale;
        rope.setHand(hand);
        m_swingDistanceJoint = GetComponent<DistanceJoint2D>();
        m_swingObjects = new List<GameObject>();
        setupHitChecker();
    }

    void setupStateMachine()
    {
        IDLE = m_stateMachine.addState(idle);
        RUN = m_stateMachine.addState(run);
        JUMP = m_stateMachine.addState(null);
        FREE_FALL = m_stateMachine.addState(freeFall,fixedFreeFall,null);
        CLIMB_CORNER = m_stateMachine.addState(null);
        WALL_ADHERE = m_stateMachine.addState(wallAdhere);
        WALL_JUMP = m_stateMachine.addState(wallJump);
        SWING = m_stateMachine.addState(swing);
        BALL_SHOOT_GROUND = m_stateMachine.addState(ballHitGround);
        HIT_GROUND = m_stateMachine.addState(hitGround);
        CHANGE_BALL_DIRECTION = m_stateMachine.addState(changeBallDirection, fixedChangeBallDirection, null);
        

        m_stateMachine.setState(IDLE);

        m_stateMachine.addTransition(FREE_FALL, CLIMB_CORNER, initClimbCorner);
        m_stateMachine.addTransition(CLIMB_CORNER, IDLE, stopHorizontal);
        m_stateMachine.addTransition(IDLE, RUN, initRun);
        m_stateMachine.addTransition(RUN, IDLE, stopHorizontal);
        m_stateMachine.addTransition(IDLE, JUMP, jump);
        m_stateMachine.addTransition(RUN, JUMP, jump);
        m_stateMachine.addTransition(IDLE, FREE_FALL, initFreeFall);
        m_stateMachine.addTransition(JUMP, FREE_FALL, initFreeFall);
        m_stateMachine.addTransition(FREE_FALL, IDLE, endFreeFall);
        m_stateMachine.addTransition(RUN, FREE_FALL, initFreeFall);
        m_stateMachine.addTransition(FREE_FALL, WALL_ADHERE, initWallAdhere);
        m_stateMachine.addTransition(WALL_ADHERE, FREE_FALL, initFreeFall);
        m_stateMachine.addTransition(WALL_ADHERE, WALL_JUMP, initWallJump);
        m_stateMachine.addTransition(WALL_JUMP, WALL_ADHERE, initWallAdhere);
        m_stateMachine.addTransition(WALL_JUMP, IDLE, stopHorizontal);
        m_stateMachine.addTransition(WALL_JUMP, CLIMB_CORNER, initClimbCorner);
        m_stateMachine.addTransition(WALL_JUMP, FREE_FALL, null);
        m_stateMachine.addTransition(WALL_JUMP, SWING, initSwing);
        m_stateMachine.addTransition(FREE_FALL, SWING, initSwing);
        m_stateMachine.addTransition(SWING, FREE_FALL, initFreeFall);
        m_stateMachine.addTransition(BALL_SHOOT_GROUND, IDLE, stopHorizontal);
        m_stateMachine.addTransition(BALL_SHOOT_GROUND, FREE_FALL, initFreeFall);
        m_stateMachine.addTransition(BALL_SHOOT_GROUND, RUN, initRun);
        m_stateMachine.addTransition(IDLE, BALL_SHOOT_GROUND, initBallHitGround);
        m_stateMachine.addTransition(RUN, BALL_SHOOT_GROUND, initBallHitGround);
        m_stateMachine.addTransition(IDLE, HIT_GROUND, initHitGround);
        m_stateMachine.addTransition(RUN, HIT_GROUND, initHitGround);
        m_stateMachine.addTransition(HIT_GROUND, IDLE, stopHorizontal);
        m_stateMachine.addTransition(HIT_GROUND, RUN, initRun);
        m_stateMachine.addTransition(HIT_GROUND, FREE_FALL, initFreeFall);
        m_stateMachine.addTransition(HIT_GROUND, JUMP, jump);
        m_stateMachine.addTransition(CHANGE_BALL_DIRECTION, RUN, initRun);
        m_stateMachine.addTransition(RUN, CHANGE_BALL_DIRECTION, initChangeBallDirection);
        m_stateMachine.addTransition(CHANGE_BALL_DIRECTION, IDLE, stopHorizontal);


        m_defaultScale = transform.localScale;

    }

    void setupHitChecker()
    {
        hitChecker.functions = new HitChecker.HitFunction[1];
        hitChecker.tags = new string[1];

        hitChecker.functions[0] = hitEnemy;
        hitChecker.tags[0] = "Enemy";
    }
    #endregion

    #region _EVENTS_

    void OnEnable()
    {
        Rope.OnHookPlaced += Rope_OnHookPlaced;
    }

    private void Rope_OnHookPlaced()
    {
        m_swingDistanceJoint.enabled = true;
    }

    void OnDisable()
    {
        Rope.OnHookPlaced -= Rope_OnHookPlaced;
    }
    #endregion

    #region _HIT_CHECKER_FUNCTIONS_
    void hitEnemy(Collider2D other)
    {
        other.GetComponent<Enemy>().takeDamage(m_currentOutputDamage);
    }

    #endregion

    #region _STATE_FUNCTIONS_


    //state functions:
    #region _BALL_RELATED_

    void initChangeBallDirection()
    {
        m_stateTimeStamp = Time.time;
        m_rigidBody.velocity = Vector2.zero;
        ball.stopBall();
        float direction = getCoefFromSpriteDirection();
        Vector2 ballPosition =  ballRestDistance;
        ballRestDistance.x *= direction;
        ballPosition = transform.position + ballRestDistance;
        ball.ignoreCollision("Player", true);
        ball.setPosition(ballPosition);
        m_animator.Play("Ball_Dir_Change");
    }

    void fixedChangeBallDirection()
    {
        float direction = getCoefFromSpriteDirection();
        ball.setPath(direction, 4.1f);
    }
    
    void changeBallDirection()
    {
        if (timeHasElapsed(m_stateTimeStamp, 0.25f))
        {
            ball.stopBall();
            ball.ignoreCollision("Player", false);
            if (Input.GetAxis("Horizontal") != 0.0f)
                m_stateMachine.setState(RUN);
            else m_stateMachine.setState(IDLE);
        }
    }

    void simpleDribble()
    {
        if (isInFront(ball.transform.position))
        {
            Vector2 force = simpleDribbleForce;
            force.x *= getCoefFromSpriteDirection();
            ball.controlledHit(force, 0.0f);
            ball.setDribblingMode(true);
        }
    }


    void initBallHitGround()
    {
        m_stateTimeStamp = Time.time;
        Vector2 force = groundShootForce;
        force.x *= getCoefFromSpriteDirection();
        force.y += Input.GetAxis("Vertical") * additionalVerticalShootForce;
        ball.hit(force, force.magnitude * 0.02f);
    }

    void ballHitGround()
    {
        if (checkForFreeFall()) return;
        if (timeHasElapsed(m_stateTimeStamp, 0.03f))
        {
            if (Input.GetAxis("Horizontal") == 0.0f) m_stateMachine.setState(IDLE);
            else m_stateMachine.setState(RUN);
        }
    }
    #endregion

    #region _JUMP_RELATED_
    void jump()
    {
        setVelocityY(jumpVelocity);
        setStateFromTransition(FREE_FALL);
    }

    void initWallJump()
    {
        m_animator.Play("Free_Fall");
        m_stateTimeStamp = Time.time;
        setVelocityX(-getCoefFromSpriteDirection() * runSpeed);
        setVelocityY(jumpVelocity);
        m_canWallAdhere = true;
    }

    void wallJump()
    {
        Debug.Log("wall jump");
        if (timeHasElapsed(m_stateTimeStamp, wallJumpDuration))
        {
            m_stateMachine.setState(FREE_FALL);
        }
        else
        {
            freeFall();
            updateAllDirections();
        }
    }

    void initClimbCorner()
    {
        m_animator.Play("Climb_Corner");
        m_rigidBody.isKinematic = true;
    }

    void initWallAdhere()
    {
        m_animator.Play("Wall_Adhere");
        m_canWallAdhere = false;
        stop();
        m_rigidBody.gravityScale = 0.0f;
        m_stateTimeStamp = Time.time;
    }

    void wallAdhere()
    {
        if (timeHasElapsed(m_stateTimeStamp, wallAdhereDuration) || Input.GetAxis("Vertical") == 0.0f)
        {
            m_rigidBody.gravityScale = m_defaultGravityScale;
            m_stateMachine.setState(FREE_FALL);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            m_rigidBody.gravityScale = m_defaultGravityScale;
            m_stateMachine.setState(WALL_JUMP);
        }
    }

    public void endClimbCorner()
    {
        Vector3 offset = new Vector3(m_spriteDirection == DIRECTION.RIGHT ? 0.5f : -0.5f, m_boxCollider.bounds.extents.y, 0.0f);
        transform.position += offset;

        m_rigidBody.isKinematic = false;
        m_stateMachine.setState(IDLE);
    }

    void initFreeFall()
    {
        m_animator.Play("Free_Fall");
    }

    void freeFall()
    {
        if (checkGround()) m_stateMachine.setState(IDLE);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canSwing())
            {
                m_stateMachine.setState(SWING);
                return;
            }
        }
        if (Input.GetAxis("Vertical") == 1.0f)
        {
            Vector3 newPos = transform.position;
            WALL_STATE wallState = checkWall(ref newPos);

            if (wallState == WALL_STATE.CORNER)
            {
                transform.position = newPos + new Vector3(m_spriteDirection == DIRECTION.RIGHT ?
                -m_boxCollider.bounds.extents.x * 0.8f : m_boxCollider.bounds.extents.x * 0.8f, -(m_boxCollider.bounds.extents.y - 0.1f), 0.0f);
                m_stateMachine.setState(CLIMB_CORNER);
            }
            else if (m_canWallAdhere && wallState == WALL_STATE.WALL)
            {
                m_stateMachine.setState(WALL_ADHERE);
            }
        }


    }

    void fixedFreeFall()
    {
        float axis = Input.GetAxis("Horizontal");
        float speed = m_rigidBody.velocity.x + axis * runSpeed * airMovementModifier;
        if (Mathf.Abs(speed) > runSpeed) speed = axis * runSpeed;

        setVelocityX(speed);
        if (axis > 0) m_spriteDirection = DIRECTION.RIGHT;
        else if (axis < 0) m_spriteDirection = DIRECTION.LEFT;

        updateMovementDirection();
        updateSpriteDirection();
    }

    void endFreeFall()
    {
        if (Input.GetAxis("Horizontal") == 0.0f) stopHorizontal();
        Debug.Log("is grounded");
    }
    bool checkForFreeFall()
    {
        if (!checkGround())
        {
            m_stateMachine.setState(FREE_FALL);
            return true;
        }
        return false;
    }
    #endregion

    #region _SWING_RELATED_
    bool canSwing()
    {
        
        if (m_swingObject == null) return false;
        if (!isInFront(m_swingObject.transform.position)) return false;
        if (transform.position.y > m_swingObject.transform.position.y) return false;
        return true;
    }

    void initSwing()
    {
        rope.shoot(m_swingObject);
        setVelocityX(getCoefFromSpriteDirection() * runSpeed * 1.5f);
    }

    void swing()
    {
        float accel = (swingAcceleration * Input.GetAxis("Horizontal") * Time.deltaTime);
        updateMovementDirection();
        if(Mathf.Sign(accel) == Mathf.Sign(m_rigidBody.velocity.x))
            setVelocityX(m_rigidBody.velocity.x + accel);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rope.pull();
            m_swingDistanceJoint.enabled = false;
            setVelocityY(jumpVelocity);
            setVelocityX(getCoefFromSpriteDirection() * runSpeed);
            m_stateMachine.setState(FREE_FALL);
        }
    }


    #endregion

    #region _BASIC_

    void setStateFromTransition(int state)
    {
        StartCoroutine(lateSetState(state));
    }
    IEnumerator lateSetState(int state)
    {
        yield return new WaitForSeconds(0.0f);
        m_stateMachine.setState(state);
    }

    void idle()
    {
        if (checkForFreeFall()) return;
        if (basicAttackCheck()) return;
        if (Input.GetKeyDown(KeyCode.C)) refreshBall();
        if (Input.GetKeyDown(KeyCode.Space)) m_stateMachine.setState(JUMP);
        else if (Input.GetAxis("Horizontal") != 0.0f) m_stateMachine.setState(RUN);
        
    }
    void initRun()
    {
        m_animator.Play("Run");
    }

    void run()
    {
        if (checkForFreeFall()) return;
        if (basicAttackCheck()) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_stateMachine.setState(JUMP);
            return;
        }
        float axis = Input.GetAxis("Horizontal");
        if (axis == 0.0f)
        {
            m_stateMachine.setState(IDLE);
        }
        else setVelocityX(axis * runSpeed);

        if (Input.GetKeyDown(KeyCode.C)) refreshBall();

        DIRECTION d = m_spriteDirection;
        updateAllDirections();
        if(m_canGoToBallMode)
        {
            if(d != m_spriteDirection)
            {
                m_stateMachine.setState(CHANGE_BALL_DIRECTION);
            }
        }
    }

    void runRaw()
    {
        float axis = Input.GetAxis("Horizontal");
        setVelocityX(axis * runSpeed);
    }

    

    void refreshBall()
    {
        ball.startForce.x = Mathf.Abs(ball.startForce.x) * getCoefFromSpriteDirection();
        ball.refresh(m_boxCollider.bounds.center + new Vector3(getCoefFromSpriteDirection(),0.0f,0.0f));
    }


    void stopHorizontal()
    {
        m_animator.Play("Idle");
        setVelocityX(0.0f);
    }

    void stopVertical()
    {
        m_animator.Play("Idle");
        setVelocityY(0.0f);
    }

    void stop()
    {
        setVelocity(Vector2.zero);
    }

    bool basicAttackCheck()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (m_canGoToBallMode) m_stateMachine.setState(BALL_SHOOT_GROUND);
            else m_stateMachine.setState(HIT_GROUND);
            return true;
        }
        else if (m_canGoToBallMode)
        {
            float lowerLimit = transform.position.y - ballGroundedThreshold;
            if (ball.transform.position.y < transform.position.y && ball.transform.position.y > lowerLimit && (
                ball.transform.position - transform.position).sqrMagnitude < ballProximityThreshold)
                simpleDribble();
        }
        return false;
    }

    #endregion

    #region _ATTACK_RELATED_

    void initHitGround()
    {
        m_animator.Play("Hit_1");
        m_stateTimeStamp = Time.time;
        m_currentOutputDamage = 10.0f;
    }

   
    void hitGround()
    {
        if (checkForFreeFall()) return;
        runRaw();
        updateAllDirections();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_stateMachine.setState(JUMP);
            return;
        }
        if (timeHasElapsed(m_stateTimeStamp, 0.3f))
        {
            if (Input.GetAxis("Horizontal") == 0.0f) m_stateMachine.setState(IDLE);
            else m_stateMachine.setState(RUN);
        }
    }
    #endregion

    #endregion

    #region _UPDATE_FUNCTIONS_
    //Update functions
    public override void update()
    {
        m_stateMachine.update();
    }

    public override void fixedUpdate()
    {
        m_stateMachine.fixedUpdate();
    }
    public override void lateUpdate()
    {
        m_stateMachine.lateUpdate();
    }
    #endregion

    #region _OTHER_FUNCTIONS
    // other functions

    float getCoefFromSpriteDirection()
    {
        return m_spriteDirection == DIRECTION.RIGHT ? 1.0f : -1.0f;
    }
    
    float getCoefFromMovementDirection()
    {
        return m_movementDirection == DIRECTION.RIGHT ? 1.0f : -1.0f;
    }

    //velocity
    void setVelocity(Vector2 velocity)
    {
        m_rigidBody.velocity = velocity;
    }

    void setVelocityX(float x)
    {
        Vector3 vel = m_rigidBody.velocity;
        vel.x = x;
        m_rigidBody.velocity = vel;
    }

    void setVelocityY(float y)
    {
        Vector3 vel = m_rigidBody.velocity;
        vel.y = y;
        m_rigidBody.velocity = vel;
    }

    bool timeHasElapsed(float startTime, float duration)
    {
        return Time.time - startTime >= duration;
    }


    #endregion

    #region _CHECK_ENVIRONMENT_

    bool checkGround()
    {
        Vector2 middle = m_boxCollider.bounds.center;
        Vector2 left = m_boxCollider.bounds.center;
        Vector2 right = m_boxCollider.bounds.center;

        left.x -= m_boxCollider.bounds.extents.x - groundCheckDistance - 0.0001f;
        left.y -= m_boxCollider.bounds.extents.y;
        right.x += m_boxCollider.bounds.extents.x - groundCheckDistance - 0.0001f; ;
        right.y -= m_boxCollider.bounds.extents.y;
        middle.y -= m_boxCollider.bounds.extents.y;

        m_isGrounded = false;
        if (raycast(middle, Vector2.down, groundCheckDistance, "Platform"))
        {
            m_isGrounded = true;
        }
        if (raycast(left, Vector2.down, groundCheckDistance, "Platform"))
        {
            m_isGrounded = true;
        }
        if (raycast(right, Vector2.down, groundCheckDistance, "Platform"))
        {
            m_isGrounded = true;
        }
        if (m_isGrounded) m_canWallAdhere = true;
        return m_isGrounded;
    }

    WALL_STATE checkWall(ref Vector3 cornerVertex)
    {
        Vector2 middle = m_boxCollider.bounds.center;
        Vector2 up = m_boxCollider.bounds.center;
        Vector2 down = m_boxCollider.bounds.center;

        float coefficient = m_spriteDirection == DIRECTION.RIGHT ? 1.0f : -1.0f;

        up.x += coefficient * m_boxCollider.bounds.extents.x;
        up.y += m_boxCollider.bounds.extents.y - wallCheckDistance + 0.2f;
        down.x += coefficient * m_boxCollider.bounds.extents.x; ;
        down.y -= m_boxCollider.bounds.extents.y - wallCheckDistance + 0.2f;
        middle.x += coefficient * m_boxCollider.bounds.extents.y;


        WALL_STATE wallState = WALL_STATE.NONE;
        Collider2D c = raycast(middle, Vector2.right * coefficient, wallCheckDistance, "Platform");
        bool wallConfirmed = false;

        if (c)
        {
            wallState = WALL_STATE.WALL;
        }
        
        c = raycast(down, Vector2.right * coefficient, wallCheckDistance, "Platform");
        if(c)
        {
            if(wallState == WALL_STATE.WALL)
                wallConfirmed = true;
        }


        if (!raycast(up, Vector2.right * coefficient, wallCheckDistance, "Platform"))
        {

            if (wallState == WALL_STATE.WALL)
            {
                wallState = WALL_STATE.CORNER;
                cornerVertex = getClosestVertexFromBounds(up, c.bounds);
                Debug.Log("up = " + up + " cornerVertex = " + cornerVertex + " center = " + c.bounds.center);

            }
        }
        else if (!wallConfirmed) wallState = WALL_STATE.NONE;

        return wallState;
    }

    Collider2D raycast(Vector2 origin, Vector2 direction, float distance, string tag_)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, distance);

        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].tag == tag_) return colliders[i];
            }
        }
        Debug.DrawLine(origin, origin + (direction * distance), Color.blue);
        return null;
    }

    Vector3 getClosestVertexFromBounds(Vector3 point, Bounds bounds)
    {
        Vector3 tl = bounds.center;
        Vector3 bl = bounds.center;
        Vector3 tr = bounds.center;
        Vector3 br = bounds.center;

        tl.x -= bounds.extents.x;
        tl.y += bounds.extents.y;
        br.x += bounds.extents.x;
        br.y -= bounds.extents.y;
        bl.x = tl.x;
        bl.y = br.y;
        tr.x = br.x;
        tr.y = tl.y;

        
        Vector3 tld = point - tl;
        Vector3 brd = point - br;
        Vector3 trd = point - tr;
        Vector3 bld = point - bl;



        Vector3 result = tl;
        float min = tld.sqrMagnitude;
        if (min > brd.sqrMagnitude) result = br;
        if (min > trd.sqrMagnitude) result = tr;
        if (min > bld.sqrMagnitude) result = bl;
        return result;
    }

    bool isInFront(Vector3 obj)
    {
        Vector3 relative = obj - transform.position;
        return Mathf.Sign(relative.x) == Mathf.Sign(getCoefFromSpriteDirection());
    }

    #endregion

    #region _DIRECTION_UPDATES_
    void updateMovementDirection()
    {
        m_movementDirection = m_rigidBody.velocity.x > 0 ? DIRECTION.RIGHT : m_movementDirection;
        m_movementDirection = m_rigidBody.velocity.x < 0 ? DIRECTION.LEFT : m_movementDirection;
    }

    void updateSpriteDirection()
    {
        Vector3 scale = transform.localScale;
        if(m_spriteDirection == DIRECTION.RIGHT)
        {
            scale.x = m_defaultScale.x;
            transform.localScale = scale;
        }
        if(m_spriteDirection == DIRECTION.LEFT)
        {
            scale.x = -m_defaultScale.x;
            transform.localScale = scale;
        }
        
    }

    void updateAllDirections()
    {
        updateMovementDirection();
        m_spriteDirection = m_movementDirection;
        updateSpriteDirection();
    }
    #endregion

    #region _COLLISION_EVENTS_

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "SwingObject")
        {
            m_swingObjects.Add(other.gameObject);
            if (m_swingObjects.Count == 1)
                m_swingObject = other.gameObject;
            else m_swingObject = getClosestSwingObject();
            Debug.Log("SWING");
        }

        if(other.tag == "Ball")
        {
            m_canGoToBallMode = true;  
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "SwingObject")
        {
            m_swingObjects.Remove(other.gameObject);

            if(m_swingObject == other.gameObject && m_stateMachine.currentState.id != SWING)
            {
                if (m_swingObjects.Count == 0) m_swingObject = null;
                else m_swingObject = getClosestSwingObject();
                Debug.Log("state id = " + m_stateMachine.currentState.id);
                Debug.Log("END_SWING");
            }
        }

        if(other.tag == "Ball")
        {
            m_canGoToBallMode = false;
        }
    }

    GameObject getClosestSwingObject()
    {
        int l = m_swingObjects.Count;
        float min = Mathf.Infinity;
        GameObject result = null;
        for (int i = 0; i < l; i++)
        {
            Vector3 dist = transform.position - m_swingObjects[i].transform.position;
            if (dist.sqrMagnitude < min) result = m_swingObjects[i];
        }
        return result;
    }
    #endregion
}
