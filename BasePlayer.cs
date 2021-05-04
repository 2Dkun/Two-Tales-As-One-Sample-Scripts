using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayer : BaseEntity
{
	public PlayerConfig playerData;
	public BasePlayer other;

	private PlayerManager player;

	public bool InImmobileState => 
		curState == States.Hurt 
		|| curState == States.Swap
		|| curState == States.KO;
		
	/*
		curState == States.Attack
		|| curState == States.Special
		|| curState == States.Swap;
		*/
		
	// Spirit variables
	protected bool isSpirit, isMoving;

	// Link Variables
	private float curSoul;

	protected abstract void StartSpecial();
	//protected abstract void HandleSpecial();

	// Start is called before the first frame update
	protected new void Start()
	{
		base.Start();
		player = dungeon.player;
		isGrounded = true;
		
		//TODO: have actual hp system later
		curHP = 5;
	}

    // Update is called once per frame
	public override void UpdateEntity()
    {
	    //print(gameObject.name + " " + curState);
	    if (isSpirit) { MoveToPlayer(); return; }
	    
	    UpdateIFrames();
	    switch(curState){
		    case States.Idle:	    HandleIdle(); break;
		    case States.Attack:     HandleAttack(); break;
		    case States.Special:    HandleSpecial(); break;
		    case States.Hurt:		ApplyHitstun(); break;
		    default: 				print("fuck"); break;
	    }
	    if (!InImmobileState && PlayerInput.swap.isPressed) { player.AttemptSwap(); }
    }

	public override void Attacked(float damage, Vector3 knockback, bool wasTipper)
	{
		if (curState == States.KO || damage <= 0) return;
		if (curState == States.Hurt || iframes != 0) return;

		EndAttack();
	    TakeDamage(damage);
	    if (InImmobileState) { curState = prevState; }
	    if (curState != States.Hurt) { ChangeState(States.Hurt); }
	    velocity = constants.hurtKnockback; //knockback;
	    shader.FlashWhite();

	    if (curHP <= 0)
	    {
		    // animate player death
		    player.Die();
	    }

	    timer.ResetTimer();
	    // TODO: dungeon.ApplyHitlag();
    }
    
    #region Core Movement Functions

    // Handles the player's grounded options
	private void HandleGrounded() 
	{
		isMoving = true;
		jumpChance = Time.time;
		if (ContinueDash()) return;
		
		// Movement
		if (PlayerInput.isMoving)
		{
			var dir = (int) Mathf.Sign(PlayerInput.moveInput.x);
			if (!PlayerInput.keepDir.isHeld) transform.localScale = new Vector2(dir * flipScale, flipScale);
			Move(Vector2.right * (dir * playerData.walkSpeed * Time.deltaTime));
		}
		else isMoving = false;
		
		if (!CheckJump()) {
			// Decelerate the player
			if (Math.Abs(velocity.x) > Mathf.Epsilon) ApplyForce(new Vector2(100, 0));
			
			// TEMP
			if (PlayerInput.confirm.isPressed) DoAttack();
			else if (PlayerInput.cancel.isPressed) TrySpecial();
			//else if (PlayerInput.cancel.isPressed) StartSpecial();
		}
	}

	// Handles the player's air movement
	protected bool endFloat;
	protected float jumpChance, airSpeed;
	private void HandleAirborne() 
	{
		if (ContinueDash()) return;
		//if (PlayerInput.jump.isPressed) print(Time.time - jumpChance);
		if (Time.time - jumpChance < constants.jumpChanceTime) CheckJump();

		if (PlayerInput.isMoving) 
		{
			var dir = Mathf.Sign(PlayerInput.moveInput.x);
			//if (!PlayerInput.keepDir.isHeld) transform.localScale = new Vector2(dir * flipScale, flipScale);
			
			dir *= playerData.airSpeed;
			airSpeed = dir >= 0 ? 
				Mathf.Min(dir, airSpeed + playerData.airAcceleration * Mathf.Sign(dir)): 
				Mathf.Max(dir, airSpeed + playerData.airAcceleration * Mathf.Sign(dir));
			Move(Vector2.right * (airSpeed * Time.deltaTime));
		}
		//else 
		if(Math.Abs(velocity.x) > Mathf.Epsilon) { DecelerateX(100); }

		HandleJumpFloat();
		
		// TEMP
		if (PlayerInput.confirm.isPressed) { DoAttack(); }
		//else if (PlayerInput.cancel.isPressed) StartSpecial();
	}

	// Handles player's attack and movement during so
	protected virtual void HandleAttack() 
	{
		Attack(dungeon.GetObstacles());
	}

	#endregion

	#region Small Helper Functions
	
	private void HandleIdle()
	{
		if(isGrounded) HandleGrounded();
		else HandleAirborne();
		ApplyGravity(playerData.gravity, playerData.maxFallSpeed);
	}
	
	// TODO: IMPLEMENT KNOCKBACK AND ANIMATION
	// Handles the entity's state during hitstun
	protected void ApplyHitstun() 
	{
		// Return the previous state once out of hitstun
		if (timer.WaitForXFrames(constants.hurtTime)) {
			velocity = Vector2.zero;
			ChangeState(States.Idle);
			iframes = constants.hurtIframes;
		}
		ApplyForce(constants.hurtDeceleration);
	}

	// Checks if the player inputted jump and preform the jump if so
	protected bool CheckJump()
	{
		if (PlayerInput.jump.isPressed) 
		{ 
			Jump(playerData.jumpForce, playerData.airSpeed);
			endFloat = false;
			timer.ResetTimer();
			jumpChance = 0;

			// Maintain ground speed
			if (PlayerInput.isMoving) airSpeed = Mathf.Sign(PlayerInput.moveInput.x) * playerData.walkSpeed;
		}
		return PlayerInput.jump.isPressed;
	}

	// Determines how long the player can continue rising in their jump
	protected void HandleJumpFloat()
	{
		// Remove this line of code to allow rising jumps on release
		//if (PlayerInput.jump.isReleased && velocity.y > 0) velocity.y = 0;
		if (PlayerInput.jump.isReleased && velocity.y > 0) 
			velocity.y = Mathf.Min(playerData.jumpReleaseVel, velocity.y);
		
		if (endFloat) return;
		if (PlayerInput.jump.isHeld) velocity.y = Mathf.Abs(playerData.jumpForce) * 0.2f;
		endFloat = PlayerInput.jump.isReleased || timer.WaitForXFrames(playerData.jumpHold);
	}
	
	#endregion

	#region Spirit Functions
	
	// Smoothly move the ghost towards the player
	private void MoveToPlayer() 
	{
		var transPoint = new Vector3(constants.spiritXPos, constants.spiritYPos, 0);
		var newPos = other.transform.TransformPoint(transPoint);
		MoveToTarget(newPos);
	    
		// Make sure ghost is facing player
		var transform1 = transform;
		transform1.localScale = transform1.localPosition.x > other.transform.localPosition.x
			? new Vector2(-flipScale, flipScale)
			: new Vector2(flipScale, flipScale);
	}

	// Spirit will move towards target
	private void MoveToTarget(Vector3 target) 
	{
		transform.position = Vector2.SmoothDamp(transform.position, target, ref velocity, constants.spiritSmoothTime);
	}

	#endregion

	#region Public Functions

	// Allows other scripts to enable spirit mode
	public void ToggleSpirit(bool spiritOn) 
	{
		isSpirit = spiritOn;
		if(box == null) print(gameObject.name);
		box.enabled = !isSpirit;
	}

	// Handles swapping between characters
	public void SwapSpirit() 
	{ 
		EndAttack();
		isSpirit = !isSpirit;
		box.enabled = !isSpirit;
		curState = States.Idle;
		if (isSpirit) { shader.SpiritShader(); return; }
		
		// Init new player variables
		isGrounded = false;
		dungeon.UpdateCam(this);
		
		// Set position and shader
		shader.RevertMaterial();
		MoveObject(other.transform.localPosition);

		// Transfer over current data
		curHP = other.curHP;
		velocity = other.velocity;
		iframes = other.iframes;
		iFrameTimer = other.iFrameTimer;
		
		// Add swap force/dash
		var dir = (int) Mathf.Sign(PlayerInput.moveInput.x);
		if (!PlayerInput.isMoving) dir = (int) Mathf.Sign(transform.localScale.x);
		velocity = Vector2.right * (constants.swapForce * dir);
		_animator.SetTrigger(dir * Mathf.Sign(transform.localScale.x) > 0 ? "dashF" : "dashB");
		//BeginDash(dir);
	}
	
	// Activates link when player has gotten enough soul
	public void GiveSoul(float souls)
	{
		curSoul += souls;
		if (curSoul >= 100)
		{
			curSoul -= 100;
			//var curTransform = transform;
			//player.CreateLink(curTransform.localPosition, Mathf.Sign(curTransform.localScale.x));
		}
	}

	// Let's other scripts know if player is grounded
	public bool IsIdle() { return isGrounded && curState == States.Idle; }
	
	// Let's other scripts know if player is compeletely still
	public bool IsMoving() { return isMoving || !IsIdle(); }

	#endregion

	#region Dash Logic

	private int dashDir;
	protected void BeginDash(int direction = 0)
	{
		velocity = Vector3.zero;
		player.dashTime = Time.time;
		dashDir = direction;
		_animator.SetTrigger(dashDir * Mathf.Sign(transform.localScale.x) >= 0 ? "dashF" : "dashB");
	}

	protected bool CheckDash(bool canHold) 
	{
		// Check if a dash was inputted if it can be done
		if (dashDir == 0) {
			if (!PlayerInput.cancel.isPressed) return false;
			if (Time.time - player.dashTime < constants.dashCooldown) return false;
			
			// Prepare the dash
			dashDir = (int) Mathf.Sign(PlayerInput.moveInput.x);
			if (!PlayerInput.isMoving) dashDir = (int) -Mathf.Sign(transform.localScale.x);
			BeginDash(dashDir);
			return true;
		}

		return ContinueDash();
	}

	protected bool ContinueDash(bool canHold = false)
	{
		if (dashDir == 0) return false;
		
		// Move the player with directional dash speed in mind
		var speed = dashDir * Mathf.Sign(transform.localScale.x) >= 0 
			? playerData.dashSpeedF : playerData.dashSpeedB;
		Move(Vector2.right * (dashDir * speed * Time.deltaTime));

		// Check the dash duration
		var curTime = Time.time - player.dashTime;
		if (curTime <= constants.dashTime) return true;
		bool checkPlayerHold = canHold && PlayerInput.cancel.isHeld;
		if (checkPlayerHold && curTime <= constants.dashTime + (float)playerData.dashHold/60) return true;
		
		// End the dash
		EndDash();
		return false;
	}

	public void EndDash()
	{
		if (!isGrounded) velocity.x = dashDir * playerData.airSpeed;
		velocity.y = dashDir = 0;
		player.dashTime = Time.time;
		_animator.SetTrigger("endDash");
	}

	#endregion

	#region Temp Functions That May Need Polish

	// Determines which attack the player will do and preforms it
	protected void DoAttack()
	{
		Attack attack = playerData.basicAttack;
		if (!isGrounded)
		{
			if (PlayerInput.up.isHeld) attack = playerData.upAttack;
			else if(PlayerInput.down.isHeld) attack = playerData.downAttack;
		}
		else
		{
			if (PlayerInput.up.isHeld) attack = playerData.upAttack;
			else
			{
				playerData.basicAttack.movement.y = PlayerInput.down.isHeld ? -1 : 1;
			}
		}
		BeginAttack(attack);
	}

	protected void TrySpecial()
	{
		// TODO: CHANGE FOR DIFFERENT SPECIALS
		if (curMP >= player.heal.mpCost)
		{
			BeginAttack(player.heal);
			curState = States.Special;
		}
	}
	
	protected virtual void HandleSpecial()
	{
		Attack(dungeon.GetObstacles());
	}

	#endregion
}
