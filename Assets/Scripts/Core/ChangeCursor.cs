using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeCursor : MonoBehaviour
{
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D combatCursor;

    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    [SerializeField] private string[] combatScenes = { " " };

    private bool isInCombat = false;
    private bool isCombatScene = false;

    private void Start()
    {
        CheckCurrentScene();

        if (isCombatScene)
        {
            SetCombatCursor();
        }
        else
        {
            SetNormalCursor();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();

        if (isCombatScene)
        {
            SetCombatCursor();
        }
        else
        {
            SetNormalCursor();
        }
    }

    private void CheckCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isCombatScene = System.Array.Exists(combatScenes, sceneName => sceneName == currentScene);
    }

    public void SetNormalCursor()
    {
        Cursor.SetCursor(normalCursor, cursorHotspot, CursorMode.Auto);
        isInCombat = false;
    }

    public void SetCombatCursor()
    {
        if (isCombatScene)
        {
            Cursor.SetCursor(combatCursor, cursorHotspot, CursorMode.Auto);
            isInCombat = true;
        }
    }

    public void ToggleCombatMode()
    {
        if (isCombatScene)
        {
            if (isInCombat)
            {
                SetNormalCursor();
            }
            else
            {
                SetCombatCursor();
            }
        }
    }
}
