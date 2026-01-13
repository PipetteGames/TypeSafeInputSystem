using System;
using UnityEngine.InputSystem;
using PipetteGames.TypeSafeInputSystem.Interfaces;

namespace PipetteGames.TypeSafeInputSystem.Implements
{
    /// <summary>
    /// 入力イベントの購読を管理するクラス
    /// </summary>
    internal class InputSubscription<T> : IInputSubscription where T : Enum
    {
        private readonly ITypedInputSystem<T> _inputSystem;
        private readonly T _action;
        private readonly Action<InputAction.CallbackContext> _callback;
        private readonly CallbackType _callbackType;
        private bool _disposed;

        internal enum CallbackType
        {
            Started,
            Performed,
            Canceled
        }

        public InputSubscription(
            ITypedInputSystem<T> inputSystem,
            T action,
            Action<InputAction.CallbackContext> callback,
            CallbackType callbackType)
        {
            _inputSystem = inputSystem ?? throw new ArgumentNullException(nameof(inputSystem));
            _action = action;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _callbackType = callbackType;
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
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
            }

            _disposed = true;
        }
    }
}
