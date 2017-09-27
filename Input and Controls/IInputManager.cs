
public interface IInputManager
{
    bool isEnabled {get;set;}
    bool GetButton(int playeId, InputAction action);
    bool GetButtonDown(int playeId, InputAction action);
    bool GetButtonUp(int playeId, InputAction action);
    float GetAxis(int playeId, InputAction action);
}
