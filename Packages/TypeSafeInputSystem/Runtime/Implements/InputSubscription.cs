using System;
using PipetteGames.TypeSafeInputSystem.Interfaces;

namespace PipetteGames.TypeSafeInputSystem.Implements
{
    /// <summary>
    /// 入力イベントの購読を表す実装クラス
    /// </summary>
    internal class InputSubscription : IInputSubscription
    {
        private Action _unsubscribeAction;
        private bool _disposed;

        public InputSubscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction ?? throw new ArgumentNullException(nameof(unsubscribeAction));
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _unsubscribeAction?.Invoke();
            _unsubscribeAction = null;
            _disposed = true;
        }
    }
}
