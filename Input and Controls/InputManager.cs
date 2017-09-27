using UnityEngine;


/*Target input devices:
      -keyboard
      -joypad
      -steering wheel
   */
public abstract class InputManager : MonoBehaviour, IInputManager
{
    protected static InputManager instance;
    public static IInputManager Instance { get { return instance; } }
    private bool dontDestroyOnLoad = true;

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            if (dontDestroyOnLoad == true)
            {
                DontDestroyOnLoad(this.transform.root.gameObject);
            }         
        }
    }

    public virtual bool isEnabled
    {
        get
        {
            return dontDestroyOnLoad;
        }
        set
        {
            this.enabled = value;
        }
    }

    public abstract float GetAxis(int playeId, InputAction action);

    public abstract bool GetButton(int playeId, InputAction action);

    public abstract bool GetButtonDown(int playeId, InputAction action);

    public abstract bool GetButtonUp(int playeId, InputAction action);

  
}
