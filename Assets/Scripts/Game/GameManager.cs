using System;
using System.Collections.Generic;
using System.Configuration;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The main game manager GameObject.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Is the game lost?
    /// </summary>
    private bool mGameLost = false;

    /// <summary>
    /// Did we start the game?
    /// </summary>
    private static bool sGameStarted = false;
    
    /// <summary>
    /// List of player which are currently alive.
    /// </summary>
    private List<GameObject> mLivingPlayers = new List<GameObject>();

    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    private static GameManager sInstance;
    
    /// <summary>
    /// Getter for the singleton GameManager object.
    /// </summary>
    public static GameManager Instance
    { get { return sInstance; } }

    /// <summary>
    /// Called when the script instance is first loaded.
    /// </summary>
    private void Awake()
    {
        // Initialize the singleton instance, if no other exists.
        if (sInstance != null && sInstance != this)
        { Destroy(gameObject); }
        else
        { sInstance = this; }
    }

    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        // Setup the game scene.
        SetupGame();
    }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    {
        // Remove any dead players from the list.
        for (int iii = mLivingPlayers.Count - 1; iii >= 0; --iii)
        {
            var playerGO = mLivingPlayers[iii];
            var player = playerGO.GetComponent<Player>();
            
            if (!player.IsAlive())
            {
                mLivingPlayers.RemoveAt(iii);
                if (!player.primaryPlayer)
                { // Only kill non-primary players.
                    // Update the UI.
                    var playerUI = Settings.Instance.PlayerUI(player.playerIndex);
                    playerUI?.GetComponent<HealthUI>()?.SetVisible(false);
                    
                    // Update the state.
                    Settings.Instance.RemovePlayer(playerGO);
                    Destroy(player.gameObject);
                }
            }
        }
        
        // Update the current list of players.
        UpdatePlayers();
    }

    /// <summary>
    /// Update the current list of players.
    /// </summary>
    public void UpdatePlayers()
    {
        // Initialize all players as living.
        mLivingPlayers = new List<GameObject>(Settings.Instance.players);
    }

    /// <summary>
    /// Setup the game scene.
    /// </summary>
    public void SetupGame()
    {
        // Set the state.
        UpdatePlayers();
        mGameLost = false;
    }

    /// <summary>
    /// Set the game to the "started" state.
    /// </summary>
    public void StartGame()
    {
        // Reload the scene as started.
        sGameStarted = true; 
        ResetGame();
    }
    
    /// <summary>
    /// Reset the game to the default state.
    /// </summary>
    public void ResetGame()
    {
        // Reload the active scene, triggering reset...
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Set the game to the "lost" state.
    /// </summary>
    public void LooseGame()
    {
        // Loose the game.
        mGameLost = true;
    }

    /// <summary>
    /// Display the current health.
    /// </summary>
    /// <param name="playerIdx">Index of the player to display health for.</param>
    /// <param name="current">Current health level.</param>
    /// <param name="min">Minimal health level.</param>
    /// <param name="max">Maximal health level.</param>
    public void DisplayHealth(int playerIdx, float current, float min, float max)
    {
        // Get UI for the specified player.
        var playerUI = Settings.Instance.PlayerUI(playerIdx);
        // Check if given player has a UI.
        if (playerUI == null)
        { return; }
        
        // Retrieve the Health UI and display the health.
        var healthUI = playerUI.GetComponent<HealthUI>();
        healthUI.SetVisible(true);
        healthUI.DisplayHealth(current, min, max);
    }

    /// <summary>
    /// Locate the nearest player to given position.
    /// </summary>
    [CanBeNull]
    public GameObject NearestPlayer(Vector3 position)
    {
        // Start with no player.
        GameObject closestPlayer = null;
        float closestDistance = float.MaxValue;
        
        foreach (var player in mLivingPlayers)
        { // Search for the nearest player.
            var playerPosition = player.transform.position;
            var playerDistance = (playerPosition - position).magnitude;
            
            if (playerDistance < closestDistance)
            { closestDistance = playerDistance; closestPlayer = player; }
        }

        // Return our findings.
        return closestPlayer;
    }

    /// <summary>
    /// Calculate and get a list of currently living players.
    /// </summary>
    public List<GameObject> LivingPlayers()
    { return mLivingPlayers; }

    /// <summary>
    /// Damage a living player by their index.
    /// </summary>
    public void DamagePlayer(int playerIdx, float damage)
    { mLivingPlayers[playerIdx].GetComponent<Player>().DamagePlayer(damage); }

    /// <summary>
    /// Toggle the development User Interface.
    /// </summary>
    public void ToggleDevUI()
    {
        var devUI = Settings.Instance.devUI;
        devUI.SetActive(!devUI.activeSelf);
    }
}
