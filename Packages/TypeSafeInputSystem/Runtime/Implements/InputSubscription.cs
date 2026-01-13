using System;
using UnityEngine.InputSystem;
using PipetteGames.TypeSafeInputSystem.Interfaces;

namespace PipetteGames.TypeSafeInputSystem.Implements
{
    /// <summary>
    /// 入力購読の実装クラス
    /// </summary>
    internal class InputSubscription<T> : IInputSubscription where T : Enum
    {
        private readonly TypedInputSystem<T> _inputSystem;
        private readonly T _action;
        private readonly Action<InputAction.CallbackContext> _callback;
        private readonly CallbackType _callbackType;
        private bool _isDisposed;

        internal enum CallbackType
        {
            Started,
            Performed,
            Canceled
        }

        internal InputSubscription(
            TypedInputSystem<T> inputSystem,
            T action,
            Action<InputAction.CallbackContext> callback,
            CallbackType callbackType)
        {
            _inputSystem = inputSystem;
            _action = action;
            _callback = callback;
            _callbackType = callbackType;
            _isDisposed = false;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            switch (_callbackType)
            {
                case CallbackType.Started:
                    _inputSystem.UnregisterStarted(_action, _callback);
                    break;
                case CallbackType.Performed:
                    _inputSystem.UnregisterPerformed(_action, _callback);
                    break;
                case CallbackType.Canceled:
                    _inputSystem.UnregisterCanceled(_action, _callback);
                    break;
                default:
                    UnityEngine.Debug.LogError($"Unknown CallbackType: {_callbackType}");
                    break;
            }

            _isDisposed = true;
        }
    }
}
