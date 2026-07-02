    using UnityEngine;
    using UnityEngine.AI;

    public class ZombieStateMachine : StateMachine
    {
        [HideInInspector]
        public NavMeshAgent navMeshAgent;
        [HideInInspector]
        public Player player;
        [HideInInspector]
        public Animator animator;
        public float wanderDuration = 10f;
        public float wanderDistance = 5f;
        public float wanderSpeed = 0.1f;

        public float chaseDistance = 5f;
        public float chaseSpeed = 5f;

        public float attackDuration = 1f;
        public float attackDistance = 0.2f;
        private float lastAttackTime = 0f;

        private int health = 100;
        public bool isAlive { get => health > 0; }
        void Start()
        {
            // Get the player and all necessary components
            player = Transform.FindFirstObjectByType<Player>();
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();

            // Set the wander state as our beginning state
            SetCurrentState(new Wander(this));
        }

        public void FixedUpdate()
        {
            // Every fixed update execute the current state
            ExecuteCurrentState();
        }

        public void Update()
        {
            // Every update check if we need to transition to a new state
            CheckForTransitions();
        }

        public override void CheckForTransitions()
        {
            // All the transitions between states are defined here
            switch (currentState)
            {
                // Transitions from Wander to other states
                case Wander:
                    // If is not alive aka dead
                    if (!isAlive)
                    {
                        // Transition to the Death state
                        SetCurrentState(new Death(this));
                        break;
                    }
                    float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                    if(distanceToPlayer < chaseDistance && player.isAlive)
                {
                    SetCurrentState(new Chase(this));
                    lastAttackTime = Time.time;
                    break;
                }
                else if (distanceToPlayer < attackDistance && player.isAlive)
                {
                    SetCurrentState(new Attack(this));
                    
                }

                    // If the player is within the chase distance and is alive
                    if (Vector3.Distance(transform.position, player.transform.position) < chaseDistance && player.isAlive)
                    {
                        // Transition to the Chase state
                        SetCurrentState(new Chase(this));
                    }

                    // If the player is within the attack distance and is alive
                    if (Vector3.Distance(transform.position, player.transform.position) < attackDistance && player.isAlive)
                    {
                        // Transition to the Attack state
                        SetCurrentState(new Attack(this));
                        lastAttackTime = Time.time;
                    }
                    break;
                // Transitions from Chase to other states
                case Chase:
                    // If is not alive aka dead
                    if (health <= 0)
                    {
                        // Transition to the Death state
                        SetCurrentState(new Death(this));
                    }

                    // If the player is outside the chase distance or player is dead
                    if (Vector3.Distance(transform.position, player.transform.position) >= chaseDistance || !player.isAlive)
                    {
                        // Transition to the Wander state
                        SetCurrentState(new Wander(this));
                    }

                    // If the player is within the attack distance and player is alive
                    if (Vector3.Distance(transform.position, player.transform.position) < attackDistance && player.isAlive)
                    {
                        // Transition to the Attack state
                        SetCurrentState(new Attack(this));
                        lastAttackTime = Time.time;
                    }
                    break;
                // Transitions from Attack to other states
                case Attack:
                    // If is not alive aka dead
                    if (health <= 0)
                    {
                        // Transition to the Death state
                        SetCurrentState(new Death(this));
                    }
                    // If the our attack duration time has passed and the player is within the attack distance and player is alive
                    if (Time.time - lastAttackTime >= attackDuration && Vector3.Distance(transform.position, player.transform.position) < attackDistance && player.isAlive)
                    {
                        // Transition to the Attack state
                        SetCurrentState(new Attack(this));
                        lastAttackTime = Time.time;
                    }

                    // If we've exceeded the attack duration
                    if (Time.time - lastAttackTime >= attackDuration)
                    {
                        // Transition to the Wander state
                        SetCurrentState(new Wander(this));
                    }
                    break;
                // No transitions needed for Death because once the zombie is dead it doesn't have to transition to any other state!
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // If a projectile enters our trigger
            var projectile = other.GetComponent<Projectile>();
            if (projectile != null)
            {
                // Then die
                health = 0;
            }
        }
    }
