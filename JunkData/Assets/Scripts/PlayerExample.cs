using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;

    private Animator animator;
    private int food;

    /* OnStart, retrieve the animator, set player's reference to the food value, 
     * and call MovingObject's start, retrieving its init data.*/
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
        base.Start();
    }

    // When this GameObject becomes disabled or inactive...
    private void OnDisable()
    {
        // Update the GameManager's instance of food points.
        GameManager.instance.playerFoodPoints = food;
    }
    
    void Update()
    {
        // If it's not the player's turn, don't check for input.
        if (!GameManager.instance.playersTurn) return;

        // Otherwise, check for input.
        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        // To ensure no diagonal movement.
        if (horizontal != 0)
            vertical = 0;

        // Check whether the player is moving horizontally or vertically.
        if (horizontal != 0 || vertical != 0)
            AttemptMove<Wall>(horizontal, vertical);
    }

    // Called when checking whether an object can move to a given location.
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        // Attempting to move should cost food.
        food--;


        /* Checks: 
         *      - Whether you can't move
         *      - If you can't move, retrieves the component that's blocking
         *      - Calls OnCantMove using the blocking component as the parameter*/
        base.AttemptMove<T>(xDir, yDir);

        // For later implementation...
        RaycastHit2D hit;

        // Check if the game is over.
        CheckIfGameOver();

        // End the player turn so enemies can move.
        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
            other.gameObject.SetActive(false);
        }
    }

    /* Called when the player can't move. When the player moves into a wall, they should 
     * damage it. */
    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("playerChop");
    }
    
    /* Reopen the scene. */
    private void Restart()
    {
        // Reloads the scene at the given build index.
        SceneManager.LoadScene(0);
    }

    // Lose 'loss' amount of food and play the damage animation.
    public void LoseFood(int loss)
    {
        animator.SetTrigger("playerHit");
        food -= loss;
        CheckIfGameOver();
    }

    // Check enough points have been lost for the game to end.
    private void CheckIfGameOver()
    {
        if (food <= 0)
            GameManager.instance.GameOver();
    }
}
