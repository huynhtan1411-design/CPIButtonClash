
using TemplateSystems.Keys;
using UnityEngine.Events;

namespace TemplateSystems
{
    public class InputSignals : MonoSingleton<InputSignals>
    {
        public UnityAction onFirstTimeTouchTaken = delegate { };
        public UnityAction onInputTaken = delegate { };
        public UnityAction onInputReleased = delegate { };
        public UnityAction<IdleInputParams> onJoystickDragged = delegate { };
        public UnityAction<IdleInputParams> onInputDragged = delegate { };
    }
}