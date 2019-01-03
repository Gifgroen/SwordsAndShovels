using UnityEngine;
using UnityEngine.Events;

public class MouseManager : MonoBehaviour
{
    public LayerMask clickableLayer;

    public Texture2D pointer;
    public Texture2D target;
    public Texture2D doorway;
    public Texture2D sword;

    public EventVector3 onClickEnvironment;
    public EventVector3 onRightClickEnvironment;
    public EventGameObject onClickAttackable;

    private bool useDefaultCursor;

    private void Awake()
    {
        var gameManager = GameManager.Instance;
        if(gameManager != null) {
            gameManager.onGameStateChanged.AddListener(HandleGameStateChanged);
        }
    }

    private void HandleGameStateChanged(GameManager.GameState currentState, GameManager.GameState previousState)
    {
        useDefaultCursor = (currentState != GameManager.GameState.Running);
    }

    private void Update()
    {
        Cursor.SetCursor(pointer, Vector2.zero, CursorMode.Auto);
        if (useDefaultCursor)
        {
            return;
        }

        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50, clickableLayer.value))
        {
            return;
        }
        
        // Override cursor
        Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);

        bool door = false;
        if (hit.collider.gameObject.CompareTag("Doorway"))
        {
            Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
            door = true;
        }

        bool chest = false;
        if (hit.collider.gameObject.CompareTag("Chest"))
        {
            Cursor.SetCursor(pointer, new Vector2(16, 16), CursorMode.Auto);
            chest = true;
        }

        bool attackable = hit.collider.GetComponent(typeof(IAttackable)) != null;
        if(attackable)
        {
            Cursor.SetCursor(sword, new Vector2(16, 16), CursorMode.Auto);
        }
        // If environment surface is clicked, invoke callbacks.
        if (Input.GetMouseButtonDown(0))
        {
            if (door)
            {
                Transform hitDoorway = hit.collider.gameObject.transform;
                onClickEnvironment.Invoke(hitDoorway.position + hitDoorway.forward * 10);
            }
            else if(attackable)
            {
                onClickAttackable.Invoke(hit.collider.gameObject);
            }
            else if (!chest)
            {
                onClickEnvironment.Invoke(hit.point);
            }
        }
        else if(Input.GetMouseButtonDown(1))
        {
            if(!door && !chest)
            {
                onRightClickEnvironment.Invoke(hit.point);
            }
        }
    }
}

[System.Serializable]
public class EventVector3 : UnityEvent<Vector3> { }

[System.Serializable]
public class EventGameObject : UnityEvent<GameObject> {}
