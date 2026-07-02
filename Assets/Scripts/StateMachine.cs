using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State currentState;

    public void SetCurrentState(State state)
    {
        // Whenever a new state is set
        var message = "";
        if (currentState != null)
        {
            // Exit the old one if it exists
            // Note: The currentState will be null if we just started the state machine
            message += $"Exiting {currentState.GetType()} state... ";

            // Call exit state to clean up the state if necessary
            currentState.ExitState();
        }

        // Set the current state
        
        currentState = state;
        message += $"Entering {state.GetType()} state";
        Debug.Log(message);

        // Call enter state to setup the state if necessary
        currentState.EnterState();

    }

    public void ExecuteCurrentState()
    {
        // Execute the current state
        currentState.ExecuteState();
    }

    public virtual void CheckForTransitions()
    {
        // Nothing is defined here... on purpose
        // We want each class derived from StateMachine
        // to make their own decisions on how to transition
        // to other states
    }
}

public class Wander : State
{
    public float lastPositionSelectedTime;
    public Vector3 lastPositionSelected;
    public Wander(StateMachine stateMachine) : base(stateMachine)
    {
    
    }

    public override void EnterState()
    {
        // Check if our state machine is a ZombieStateMachine
        if (stateMachine.GetType() == typeof(ZombieStateMachine))
        {
            ZombieStateMachine zombie = (ZombieStateMachine)stateMachine;
           

            // Fetch the NavMeshAgent from the state machine
             zombie.navMeshAgent.speed = zombie.wanderSpeed;
             lastPositionSelectedTime = Time.time - zombie.wanderDuration - 1f;
            // Set the speed of the agent to the wandering speed
            // Set the last selected time to the past so we can select a random position immediately
        }
    }

    public override void ExecuteState()
    {
        // Check if our state machine is an enemy state machine
        if (stateMachine.GetType() == typeof(ZombieStateMachine))
        {
            ZombieStateMachine zombie = (ZombieStateMachine)stateMachine;
            if (Time.time - lastPositionSelectedTime > zombie.wanderDuration)
            {
                Vector3 randomDirection = new Vector3(Random.Range(-zombie.wanderDistance, zombie.wanderDistance), 0f, Random.Range(-zombie.wanderDistance, zombie.wanderDistance));
                lastPositionSelected = zombie.navMeshAgent.transform.position + randomDirection;
                lastPositionSelectedTime = Time.time;
            }
            float speedMagnitude = zombie.navMeshAgent.desiredVelocity.magnitude;
            zombie.animator.SetBool("isMoving", speedMagnitude > 0.25f);

            // Fetch the NavMeshAgent from the state machine
            // If the time since we last selected a position is greater than the wander duration
                // Select an new position as a random offset from our current position
            // Set the animator to be moving if we have a desired velocity greater than 0.25f;
        }
    }

    public override void ExitState()
    {
        
    }
}

public class Chase : State
{
    public Chase(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void EnterState()
    {
        // Check if our state machine is an enemy state machine
        if (stateMachine.GetType() == typeof(ZombieStateMachine))

        {
            ZombieStateMachine zombie = (ZombieStateMachine)stateMachine;
            zombie.navMeshAgent.speed = zombie.chaseSpeed;
            zombie.animator.SetBool("isChasing", true);
            // Fetch the NavMeshAgent from the state machine
            // Update the movement speed and the animator to chase
        }
    }

    public override void ExecuteState()
    {
        // Check if our state machine is an enemy state machine
        if (stateMachine.GetType() == typeof(ZombieStateMachine))
        {
            ZombieStateMachine zombie = (ZombieStateMachine)stateMachine;
            if(zombie.player != null)
            {
                zombie.navMeshAgent.SetDestination(zombie.player.transform.position);
            }
            float currentSpeed = zombie.navMeshAgent.desiredVelocity.magnitude;
            zombie.animator.SetBool("isMoving", currentSpeed > 0.25f);
            // Fetch the NavMeshAgent from the state machine
            // Set player as our destination
            // Update the animator if we're moving
        }
    }

    public override void ExitState()
    {
        // Check if our state machine is an enemy state machine
        if (stateMachine.GetType() == typeof(ZombieStateMachine))
        {
            ZombieStateMachine zombie = (ZombieStateMachine)stateMachine;
            if(zombie.player != null)
            {
                zombie.navMeshAgent.SetDestination(zombie.player.transform.position);
                zombie.animator.SetBool("isChasing", false);
            }
            // Fetch the NavMeshAgent from the state machine
        }
    }
}

public class Attack : State
{
    public Attack(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void EnterState()
    {
       
        // Check if our state machine is an enemy state machine
        if (stateMachine.GetType() == typeof(ZombieStateMachine))
        {
            ZombieStateMachine zombie = (ZombieStateMachine)stateMachine;
            zombie.navMeshAgent.SetDestination(zombie.transform.position);
            zombie.animator.SetTrigger("Attack");
            // Fetch the NavMeshAgent from the state machine
            // Set our position here so we don't move while attacking
        }
    }

    public override void ExecuteState()
    {
    }

    public override void ExitState()
    {
        // Check if our state machine is an enemy state machine
        if (stateMachine.GetType() == typeof(ZombieStateMachine))
        {
            ZombieStateMachine zombie = (ZombieStateMachine)stateMachine;
            float distanceToPlayer = Vector3.Distance(zombie.transform.position, zombie.player.transform.position);
if(distanceToPlayer < zombie.attackDistance && zombie.player.isAlive)
            {
                zombie.player.DamagePlayer(10);
            }
            // Fetch the NavMeshAgent from the state machine
            // If the player is within our attack distance when we exit the attack state, do damage
        }
    }
}

public class Death : State
{
    public Death(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void EnterState()
    {
        // Check if our state machine is an enemy state machine
        if (stateMachine.GetType() == typeof(ZombieStateMachine))
        {
            ZombieStateMachine zombie = (ZombieStateMachine)stateMachine;
            zombie.navMeshAgent.SetDestination(zombie.transform.position);
            zombie.navMeshAgent.speed = 0f;
            zombie.animator.SetTrigger("Dead");
            zombie.navMeshAgent.enabled = false;
            // Fetch the NavMeshAgent from the state machine
            // Set our position to where we're currently at so that we stop moving
            // Also set the speed to 0 just in case
            // Update the animator to play the death animation
        }
    }

    public override void ExecuteState()
    {
    }

    public override void ExitState()
    {
    }
}

public abstract class State
{
    protected StateMachine stateMachine;

    // Allow every state to hold a reference to the state machine
    // This is used for grabbing data i.e. NavMeshAgents or Rigidbodies
    // that are attached to the AI
    public State(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    // Abstract methods that all states must implement
    public virtual void EnterState()
    {
    }

    public virtual void ExecuteState()
    {
    }

    public virtual void ExitState()
    {
    }
}